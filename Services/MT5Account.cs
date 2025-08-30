using Grpc.Core;
using Grpc.Net.Client;
using mt5_term_api;   
using System.Runtime.CompilerServices;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;



namespace MetaRPC.CSharpMT5;


/// <summary>
/// MT5 trading account connected through gRPC.
/// Provides access to trading and market data operations.
/// </summary>
public class MT5Account
{

    /// <summary>
    /// Unique login number of the MT5 account.
    /// </summary>
    public ulong User { get; }


    /// <summary>
    /// Password used to authenticate the MT5 account.
    /// </summary>
    public string Password { get; }


    /// <summary>
    /// Address of the MT5 trade server.
    /// </summary>
    public string Host { get; private set; } = string.Empty;


    /// <summary>
    /// Network port of the MT5 trade server (default 443).
    /// </summary>
    public int Port { get; private set; } = 443;


    /// <summary>
    /// Name of the MT5 server as defined by the broker.
    /// </summary>
    public string? ServerName { get; private set; }


    /// <summary>
    /// Default chart symbol used when no other is specified (e.g., EURUSD).
    /// </summary>
    public string BaseChartSymbol { get; private set; } = "EURUSD";


    /// <summary>
    /// Timeout in seconds for establishing a connection to the MT5 server.
    /// </summary>
    public int ConnectTimeoutSeconds { get; private set; } = 30;


    /// <summary>
    /// Full gRPC server endpoint used for MT5 connection.
    /// </summary>
    public string GrpcServer { get; private set; } = "https://mt5.mrpc.pro:443";


    /// <summary>
    /// Active gRPC channel for communicating with the MT5 server.
    /// </summary>
    public Grpc.Net.Client.GrpcChannel? GrpcChannel { get; private set; }


    /// <summary>
    /// gRPC client used for MT5 connection management.
    /// </summary>
    public Connection.ConnectionClient? ConnectionClient { get; private set; }


    /// <summary>
    /// gRPC client for managing MT5 data subscriptions.
    /// </summary>
    public SubscriptionService.SubscriptionServiceClient? SubscriptionClient { get; private set; }


    /// <summary>
    /// gRPC client for retrieving MT5 account information.
    /// </summary>
    public AccountHelper.AccountHelperClient? AccountClient { get; private set; }


    /// <summary>
    /// gRPC client for executing MT5 trading operations.
    /// </summary>
    public TradingHelper.TradingHelperClient? TradeClient { get; private set; }


    /// <summary>
    /// gRPC client for requesting MT5 market information.
    /// </summary>
    public MarketInfo.MarketInfoClient? MarketInfoClient { get; private set; }


    /// <summary>
    /// gRPC client providing access to MT5 trade functions.
    /// </summary>
    public TradeFunctions.TradeFunctionsClient? TradeFunctionsClient { get; private set; }


    /// <summary>
    /// gRPC client for accessing MT5 account details and status.
    /// </summary>
    public AccountInformation.AccountInformationClient? AccountInformationClient { get; private set; }


    /// <summary>
    /// Unique identifier of this MT5 account instance.
    /// </summary>
    public Guid Id { get; private set; } = Guid.Empty;

#pragma warning disable CS0169, CS0414
    private string? _selectedProfile;
#pragma warning restore CS0169, CS0414

private readonly ILogger<MT5Account>? _logger;



    /// <summary>
    /// Places a stop-limit order on the MT5 server.
    /// Validates parameters and maps them to MT5 enums.
    /// Supports TIF options (GTC, DAY, GTD, SPECIFIED).
    /// Ensures the trading symbol is visible before sending.
    /// Returns the ticket ID of the created order.
    /// </summary>
    public async Task<ulong> PlaceStopLimitOrderAsync(
        string symbol,
        string type,                 // "buystoplimit" | "sellstoplimit"
        double volume,
        double stop,                 // trigger price
        double limit,                // limit price
        double? sl,
        double? tp,
        string? tif,                 // "GTC" | "DAY" | "GTD" | null
        DateTimeOffset? expire,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("symbol must be provided", nameof(symbol));
        if (volume <= 0)
            throw new ArgumentOutOfRangeException(nameof(volume), "volume must be > 0");
        if (stop <= 0 || limit <= 0)
            throw new ArgumentOutOfRangeException(nameof(stop), "stop/limit must be > 0");

        // Map string type -> enum
        var op = type.Replace("-", "").Replace(".", "").Trim().ToLowerInvariant() switch
        {
            "buystoplimit" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyStopLimit,
            "sellstoplimit" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellStopLimit,
            _ => throw new NotSupportedException($"Unsupported stop-limit type: '{type}'")
        };

        // MT5 rule: Buy requires limit <= stop; Sell requires limit >= stop
        if (op == TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyStopLimit && !(limit <= stop))
            throw new ArgumentException("Buy Stop Limit requires limit <= stop");
        if (op == TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellStopLimit && !(limit >= stop))
            throw new ArgumentException("Sell Stop Limit requires limit >= stop");

        // Map TIF (Time in Force) -> enum
        var tifEnum = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeGtc;
        if (!string.IsNullOrWhiteSpace(tif))
        {
            switch (tif.Trim().ToUpperInvariant())
            {
                case "GTC": tifEnum = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeGtc; break;
                case "DAY": tifEnum = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeDay; break;
                case "GTD":
                case "SPECIFIED":
                    tifEnum = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecified; break;
                case "SPECIFIED_DAY":
                    tifEnum = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecifiedDay; break;
                default:
                    throw new ArgumentException($"Unsupported TIF '{tif}'. Use GTC|DAY|GTD.");
            }
        }
        if ((tifEnum == TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecified
          || tifEnum == TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecifiedDay) && !expire.HasValue)
            throw new ArgumentException("When TIF=GTD/SPECIFIED(_DAY), 'expire' must be provided.");

        // Ensure symbol is visible on the server (idempotent)
        await EnsureSymbolVisibleAsync(symbol, cancellationToken: ct);

        // Build request (stop -> StopLimitPrice, limit -> Price)
        var req = new OrderSendRequest
        {
            Symbol = symbol,
            Operation = op,
            Volume = volume,
            Price = limit,
            StopLimitPrice = stop,
            StopLoss = sl ?? 0,
            TakeProfit = tp ?? 0,
            Slippage = 0, // default for pending order
            ExpirationTimeType = tifEnum
        };

        // Add expiration if TIF requires it
        if ((tifEnum == TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecified
          || tifEnum == TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecifiedDay) && expire.HasValue)
        {
            req.ExpirationTime = Timestamp.FromDateTimeOffset(expire.Value.ToUniversalTime());
        }

        // Send order (wrapper handles error mapping)
        // Send order
        var reply = await TradeClient!.OrderSendAsync(req, cancellationToken: ct);

        if (reply.ResponseCase == OrderSendReply.ResponseOneofCase.Error)
            throw new InvalidOperationException($"OrderSend error: {reply.Error}");

        return reply.Data?.Order ?? 0UL;

    }


    // Export trading history (orders & deals) into a single string.
    // Summary:
    //   - Fetches mixed history via OrderHistoryAsync() for the last N days,
    //     optionally filters by symbol, flattens to a uniform row model,
    //     and serializes to CSV (default) or pretty-printed JSON.
    // Parameters:
    //   - days   : > 0; lookback window [UtcNow - days, UtcNow].
    //   - symbol : optional case-insensitive filter applied to both orders and deals.
    //   - format : "csv" | "json" (default "csv").
    //   - ct     : cancellation token; aborts the RPC and serialization if cancelled.
    // Output:
    //   - CSV: header "kind,ticket,symbol,side_or_state,volume,volume_initial,volume_current,price,price_open,profit,time,time_setup,time_done",
    //           numbers in InvariantCulture, timestamps ISO-8601 (O) UTC, proper RFC4180 quoting.
    //   - JSON: array of rows with nullable fields preserved, pretty-printed.
    // Exceptions:
    //   - ArgumentOutOfRangeException if days <= 0.
    //   - ArgumentException if format is not csv|json.
    //   - OperationCanceledException may bubble from ct.
    // Notes:
    //   - This allocates all rows in memory; for huge ranges consider streaming to a file.
    //   - The method includes both HistoryOrder and HistoryDeal entries with best-effort field mapping.
    public async Task<string> ExportHistoryAsync(int days, string? symbol, string format, CancellationToken ct)
    {
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days));
        format = (format ?? "csv").Trim().ToLowerInvariant();
        if (format != "csv" && format != "json")
            throw new ArgumentException("format must be csv|json", nameof(format));

        var from = DateTime.UtcNow.AddDays(-Math.Abs(days));
        var to = DateTime.UtcNow;

        var res = await OrderHistoryAsync(from, to, deadline: null, cancellationToken: ct);

        var items = res.HistoryData.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(symbol))
        {
            items = items.Where(h =>
                (h.HistoryOrder?.Symbol != null &&
                 h.HistoryOrder.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase)) ||
                (h.HistoryDeal?.Symbol != null &&
                 h.HistoryDeal.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase)));
        }

        var rows = new List<HistRow>(capacity: 256);

        foreach (var h in items)
        {
            if (h.HistoryOrder is not null)
            {
                var o = h.HistoryOrder;
                rows.Add(new HistRow
                {
                    Kind = "order",
                    Ticket = (ulong)o.Ticket,
                    Symbol = o.Symbol,
                    SideOrState = o.State.ToString(),
                    Volume = null,
                    VolumeInitial = o.VolumeInitial,
                    VolumeCurrent = o.VolumeCurrent,
                    Price = null,
                    PriceOpen = o.PriceOpen,
                    Profit = null,
                    Time = null,
                    TimeSetup = o.SetupTime?.ToDateTime(),
                    TimeDone = o.DoneTime?.ToDateTime()
                });
            }

            if (h.HistoryDeal is not null)
            {
                var d = h.HistoryDeal;
                rows.Add(new HistRow
                {
                    Kind = "deal",
                    Ticket = (ulong)d.Ticket,
                    Symbol = d.Symbol,
                    SideOrState = d.Type.ToString(),
                    Volume = d.Volume,
                    VolumeInitial = null,
                    VolumeCurrent = null,
                    Price = d.Price,
                    PriceOpen = null,
                    Profit = d.Profit,
                    Time = d.Time?.ToDateTime(),
                    TimeSetup = null,
                    TimeDone = null
                });
            }
        }

        if (format == "json")
        {
            return JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true });
        }

        // CSV
        var sb = new StringBuilder();
        sb.AppendLine("kind,ticket,symbol,side_or_state,volume,volume_initial,volume_current,price,price_open,profit,time,time_setup,time_done");

        string F(double? v) => v?.ToString(CultureInfo.InvariantCulture) ?? "";
        string T(DateTime? dt) => dt?.ToString("O", CultureInfo.InvariantCulture) ?? "";
        string Q(string? s) => s is null ? "" : "\"" + s.Replace("\"", "\"\"") + "\"";

        foreach (var r in rows)
        {
            sb.Append(Q(r.Kind)).Append(',')
              .Append(r.Ticket.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(Q(r.Symbol)).Append(',')
              .Append(Q(r.SideOrState)).Append(',')
              .Append(F(r.Volume)).Append(',')
              .Append(F(r.VolumeInitial)).Append(',')
              .Append(F(r.VolumeCurrent)).Append(',')
              .Append(F(r.Price)).Append(',')
              .Append(F(r.PriceOpen)).Append(',')
              .Append(F(r.Profit)).Append(',')
              .Append(Q(T(r.Time))).Append(',')
              .Append(Q(T(r.TimeSetup))).Append(',')
              .Append(Q(T(r.TimeDone)))
              .AppendLine();
        }

        return sb.ToString();
    }

    private sealed class HistRow
    {
        public string Kind { get; set; } = "";
        public ulong Ticket { get; set; }
        public string? Symbol { get; set; }
        public string? SideOrState { get; set; }
        public double? Volume { get; set; }
        public double? VolumeInitial { get; set; }
        public double? VolumeCurrent { get; set; }
        public double? Price { get; set; }
        public double? PriceOpen { get; set; }
        public double? Profit { get; set; }
        public DateTime? Time { get; set; }
        public DateTime? TimeSetup { get; set; }
        public DateTime? TimeDone { get; set; }
    }



    /// <summary>
    /// Estimates the value of 1 point per 1 lot in the quote currency.
    /// Uses simple heuristics: metals (XAU≈10, XAG≈5) and FX rules (XXXUSD / USDXXX).
    /// Falls back to 100000 * point for generic FX; point guessed if missing.
    /// Uses mid price for USDXXX pairs via TryGetMidAsync.
    /// </summary>
    public async Task<double> EstimatePointValuePerLotAsync(string symbol, CancellationToken ct)
    {
        // Point size (e.g., 0.0001 for EURUSD, 0.01 for XXXJPY)
        double point = GuessPointSizeInternal(symbol);
        if (point <= 0) point = 0.0001; // conservative fallback

        // Heuristics for common instruments
        var s = symbol.ToUpperInvariant();

        // Metals: many brokers use 100 oz/lot for XAU; 0.1 step → ~10 quote-currency per point
        if (s.Contains("XAU")) return 10.0;
        if (s.Contains("XAG")) return 5.0; // rough ballpark

        // FX:
        // XXXUSD → point value ≈ 100000 * point (per lot) → usually ~10
        if (s.EndsWith("USD", StringComparison.Ordinal))
            return 100000.0 * point;

        // USDXXX → point value ≈ (100000 * point) / mid(USDXXX)
        if (s.StartsWith("USD", StringComparison.Ordinal))
        {
            double price = await TryGetMidAsync(symbol, ct);
            if (price <= 0) price = 1.0; // guard against bad feed
            return (100000.0 * point) / price;
        }

        // Default FX fallback (keeps CLI responsive even without full symbol params)
        return 100000.0 * point;
    }


    // Best-effort mid price for a symbol; falls back to 1.0 on bad feed/errors.
    // Uses latest tick from server: (Bid+Ask)/2 if both > 0, else Bid or Ask.
    // Silent failure policy: swallow exceptions and return 1.0 to keep callers moving.
    private async Task<double> TryGetMidAsync(string symbol, CancellationToken ct)
    {
        try
        {
            var t = await SymbolInfoTickAsync(symbol, deadline: null, cancellationToken: ct);
            if (t.Bid > 0 && t.Ask > 0) return (t.Bid + t.Ask) / 2.0; // true mid
            if (t.Bid > 0) return t.Bid;                               // bid-only fallback
            if (t.Ask > 0) return t.Ask;                               // ask-only fallback
            return 1.0;                                                // no prices → neutral
        }
        catch
        {
            return 1.0;                                                // transport/other error → neutral
        }
    }


    // NOTE: This is a lightweight local-config check (host/server name present).
    // It does NOT confirm an active gRPC channel — use IsConnected for that.
    private bool Connected => Host is not null || ServerName is not null;


    /// <summary>
    /// True if the account has an active gRPC channel,
    /// a valid identifier, and all required clients initialized.
    /// Used to confirm full MT5 connection state.
    /// </summary>
    public bool IsConnected =>
        GrpcChannel is not null
        && Id != Guid.Empty
        && ConnectionClient is not null
        && SubscriptionClient is not null
        && AccountClient is not null
        && TradeClient is not null
        && MarketInfoClient is not null
        && TradeFunctionsClient is not null
        && AccountInformationClient is not null;


    /// <summary>
    /// Initializes gRPC clients for all MT5 services using the given address.
    /// Creates channel only if it is not already initialized.
    /// </summary>
    private void CreateGrpcClients(string address)
    {
        if (GrpcChannel is not null) return; // already created

        GrpcChannel = GrpcChannel.ForAddress(address);

        // Initialize gRPC clients for each MT5 service
        ConnectionClient = new Connection.ConnectionClient(GrpcChannel);
        SubscriptionClient = new SubscriptionService.SubscriptionServiceClient(GrpcChannel);
        AccountClient = new AccountHelper.AccountHelperClient(GrpcChannel);
        TradeClient = new TradingHelper.TradingHelperClient(GrpcChannel);
        MarketInfoClient = new MarketInfo.MarketInfoClient(GrpcChannel);
        TradeFunctionsClient = new TradeFunctions.TradeFunctionsClient(GrpcChannel);
        AccountInformationClient = new AccountInformation.AccountInformationClient(GrpcChannel);
    }


    /// <summary>
    /// Builds gRPC metadata headers for requests.
    /// Adds account <c>Id</c> if it is set, otherwise returns empty headers.
    /// </summary>
    private Metadata BuildHeaders()
    {
        return Id != Guid.Empty
            ? new Metadata { { "id", Id.ToString() } }
            : new Metadata();
    }


    /// <summary>
    /// Ensures the given value is not null; otherwise throws an exception.
    /// Used to enforce initialization before access.
    /// </summary>
    private static T Require<T>(T? value, string name) where T : class =>
        value ?? throw new InvalidOperationException($"{name} is not initialized. Call Connect* first.");


    /// <summary>
    /// Builds request headers with account <c>Id</c> if available.
    /// </summary>
    private Metadata GetHeaders() =>
        Id != Guid.Empty ? new Metadata { { "id", Id.ToString() } } : new Metadata();


    // Shorthand non-null accessors for gRPC clients
    private Connection.ConnectionClient Conn => Require(ConnectionClient, nameof(ConnectionClient));
    private SubscriptionService.SubscriptionServiceClient Subs => Require(SubscriptionClient, nameof(SubscriptionClient));
    private AccountHelper.AccountHelperClient Acc => Require(AccountClient, nameof(AccountClient));
    private TradingHelper.TradingHelperClient Trade => Require(TradeClient, nameof(TradeClient));
    private MarketInfo.MarketInfoClient Mkt => Require(MarketInfoClient, nameof(MarketInfoClient));
    private TradeFunctions.TradeFunctionsClient TF => Require(TradeFunctionsClient, nameof(TradeFunctionsClient));
    private AccountInformation.AccountInformationClient AccInfo => Require(AccountInformationClient, nameof(AccountInformationClient));
    


    /// <summary>
    /// Disconnects from the MT5 server and disposes the gRPC channel.
    /// Clears the account <c>Id</c> and all client instances.
    /// Safe to call multiple times; ignores dispose errors.
    /// Returns a completed task when cleanup is done.
    /// </summary>
    public Task DisconnectAsync()
    {
        try { (GrpcChannel as IDisposable)?.Dispose(); } // release channel if active
        catch { } // ignore dispose errors
        finally
        {
            // Reset account state
            Id = Guid.Empty;
            AccountInformationClient = null;
            TradeFunctionsClient = null;
            MarketInfoClient = null;
            TradeClient = null;
            AccountClient = null;
            SubscriptionClient = null;
            ConnectionClient = null;
            GrpcChannel = null;
        }
        return Task.CompletedTask;
    }


    /// <summary>
    /// Ensures the given symbol is visible and synchronized in Market Watch.
    /// Selects the symbol on the server and waits until it is ready.
    /// Allows optional timeout, polling interval, and deadline.
    /// Throws if the symbol is missing, cannot be selected, or sync times out.
    /// </summary>
    public async Task EnsureSymbolVisibleAsync(
        string symbol,
        TimeSpan? maxWait = null,
        TimeSpan? pollInterval = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol must be provided", nameof(symbol));

        // Step 1: Make the symbol visible in Market Watch
        var selectRes = await SymbolSelectAsync(symbol, true, deadline, cancellationToken);
        if (!selectRes.Success)
            throw new InvalidOperationException($"SymbolSelect failed for '{symbol}'.");

        // Step 2: Wait for symbol synchronization
        var timeout = maxWait ?? TimeSpan.FromSeconds(3);
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(150);
        var until = DateTime.UtcNow + timeout;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sync = await SymbolIsSynchronizedAsync(symbol, deadline, cancellationToken);
            if (sync.Synchronized)
                return; // symbol is ready

            if (DateTime.UtcNow >= until)
                throw new TimeoutException($"Symbol '{symbol}' is not synchronized within {timeout.TotalMilliseconds} ms.");

            await Task.Delay(interval, cancellationToken); // wait before retry
        }
    }


    /// <summary>
    /// Reads volume constraints for the given symbol from MT5.
    /// Returns minimum lot, step size, and maximum lot as a tuple.
    /// Useful for validating order volume before sending.
    /// Defaults step to 0.01 if server returns invalid value.
    /// Supports optional deadline and cancellation token.
    /// </summary>
    public async Task<(double Min, double Step, double Max)> GetVolumeConstraintsAsync(
        string symbol,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)

    {
        // SYMBOL_VOLUME_MIN / MAX / STEP
        var min = (await SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeMin, deadline, cancellationToken)).Value;
        var max = (await SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeMax, deadline, cancellationToken)).Value;
        var step = (await SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeStep, deadline, cancellationToken)).Value;
        if (step <= 0) step = 0.01;
        return (min, step, max);
    }


    /// <summary>
    /// Normalizes the given volume to fit within min, step, and max.
    /// Adjusts value to nearest valid step between limits.
    /// Rounds to 8 decimals to avoid floating errors.
    /// Throws if the result is zero or below minimum.
    /// Returns the normalized volume ready for order placement.
    /// </summary>
    public static double NormalizeVolume(double volume, double min, double step, double max)
    {
        if (volume < min) volume = min;
        if (volume > max) volume = max;
        // rounding to the nearest step
        var steps = Math.Round((volume - min) / step, MidpointRounding.AwayFromZero);
        var normalized = min + steps * step;
        // protection against accumulated double error
        normalized = Math.Round(normalized, 8);
        if (normalized <= 0) throw new ArgumentOutOfRangeException(nameof(volume), "Volume is below allowed minimum after normalization.");
        return normalized;
    }


    /// <summary>
    /// Sends a market order (Buy or Sell) with minimal setup.
    /// Ensures the symbol is visible before trading.
    /// Normalizes the requested volume to allowed min/step/max.
    /// Executes the order at current Bid/Ask with optional SL/TP.
    /// Returns the ticket number of the created order.
    /// </summary>
    public async Task<ulong> SendMarketOrderAsync(
            string symbol,
            bool isBuy,
            double volume,
            int deviation = 10,
            double? stopLoss = null,
            double? takeProfit = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        // 1) The symbol is ready
        await EnsureSymbolVisibleAsync(symbol, maxWait: TimeSpan.FromSeconds(3), deadline: deadline, cancellationToken: cancellationToken);

        // 2) Normalize the volume
        var (min, step, max) = await GetVolumeConstraintsAsync(symbol, deadline, cancellationToken);
        var vol = NormalizeVolume(volume, min, step, max);

        // 3) We take the price
        var tick = await SymbolInfoTickAsync(symbol, deadline, cancellationToken);
        var price = isBuy ? tick.Ask : tick.Bid;

        // 4) Sending the order
        var sendReq = new OrderSendRequest
        {
            Symbol = symbol,
            Operation = isBuy ? TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy : TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSell,
            Volume = vol,
            Price = price
        };

        var sendRes = await OrderSendAsync(sendReq, deadline, cancellationToken);
        var ticket = sendRes.Order;

        // 5) SL/TP (if specified) — a separate modification
        if (stopLoss.HasValue || takeProfit.HasValue)
        {
            var modReq = new OrderModifyRequest
            {
                Ticket = ticket,
                Price = price,
                StopLoss = stopLoss ?? 0,
                TakeProfit = takeProfit ?? 0
            };
            _ = await OrderModifyAsync(modReq, deadline, cancellationToken);
        }

        return ticket;
    }


    /// <summary>
    /// Returns a list of tickets for all pending orders.
    /// Can optionally filter by symbol.
    /// Uses AccountClient to query opened orders.
    /// </summary>
    public async Task<IReadOnlyList<ulong>> ListPendingTicketsAsync(string? symbol, CancellationToken ct)
    {
        if (AccountClient is null)
            throw new InvalidOperationException("AccountClient is not initialized.");

        // We take all open orders and filter the pending by type
        var reply = await AccountClient.OpenedOrdersAsync(new OpenedOrdersRequest(), cancellationToken: ct);

        // reply.Data.OpenedOrders: RepeatedField<OpenedOrderInfo>
        var ordersField = reply?.Data?.OpenedOrders;
        IEnumerable<OpenedOrderInfo> orders = ordersField != null
            ? ordersField            // RepeatedField<T> normally iterated asIEnumerable<T>
            : Enumerable.Empty<OpenedOrderInfo>();

        var pendingTypes = new HashSet<BMT5_ENUM_ORDER_TYPE>
{
    BMT5_ENUM_ORDER_TYPE.Bmt5OrderTypeBuyLimit,
    BMT5_ENUM_ORDER_TYPE.Bmt5OrderTypeSellLimit,
    BMT5_ENUM_ORDER_TYPE.Bmt5OrderTypeBuyStop,
    BMT5_ENUM_ORDER_TYPE.Bmt5OrderTypeSellStop,
    BMT5_ENUM_ORDER_TYPE.Bmt5OrderTypeBuyStopLimit,
    BMT5_ENUM_ORDER_TYPE.Bmt5OrderTypeSellStopLimit
};

        var list = orders
            .Where(o => pendingTypes.Contains(o.Type))
            .Where(o => string.IsNullOrWhiteSpace(symbol) ||
                        string.Equals(o.Symbol, symbol!, StringComparison.OrdinalIgnoreCase))
            .Select(o => (ulong)o.Ticket)
            .ToList();

        return list;
    }


    /// <summary>
    /// Builds a map (ticket → type string) for the given pending orders.
    /// Useful for filtering by order type ("buylimit", "sellstop", etc.).
    /// Returns an empty dictionary if no tickets provided.
    /// </summary>
    public async Task<Dictionary<ulong, string>> GetPendingKindsAsync(IEnumerable<ulong> tickets, CancellationToken ct)
    {
        var set = tickets?.ToHashSet() ?? new HashSet<ulong>();
        var result = new Dictionary<ulong, string>();
        if (set.Count == 0) return result;

        if (AccountClient is null)
            throw new InvalidOperationException("AccountClient is not initialized.");

        var reply = await AccountClient.OpenedOrdersAsync(new OpenedOrdersRequest(), cancellationToken: ct);
        var ordersField = reply?.Data?.OpenedOrders;
        IEnumerable<OpenedOrderInfo> orders = ordersField != null ? ordersField : Enumerable.Empty<OpenedOrderInfo>();

        foreach (var o in orders)
        {
            var t = (ulong)o.Ticket;
            if (!set.Contains(t)) continue;

            var kind = o.Type switch
            {
                BMT5_ENUM_ORDER_TYPE.Bmt5OrderTypeBuyLimit => "buylimit",
                BMT5_ENUM_ORDER_TYPE.Bmt5OrderTypeSellLimit => "selllimit",
                BMT5_ENUM_ORDER_TYPE.Bmt5OrderTypeBuyStop => "buystop",
                BMT5_ENUM_ORDER_TYPE.Bmt5OrderTypeSellStop => "sellstop",
                BMT5_ENUM_ORDER_TYPE.Bmt5OrderTypeBuyStopLimit => "buystoplimit",
                BMT5_ENUM_ORDER_TYPE.Bmt5OrderTypeSellStopLimit => "sellstoplimit",
                _ => "other"
            };
            result[t] = kind;
        }
        return result;
    }


    /// <summary>
    /// Cancels a pending order by its ticket ID.
    /// Sends OrderClose request through TradeClient.
    /// Throws if ticket is invalid or TradeClient not initialized.
    /// </summary>
    public async Task CancelPendingOrderAsync(ulong ticket, CancellationToken ct)
    {
        if (ticket == 0) throw new ArgumentOutOfRangeException(nameof(ticket));
        if (TradeClient is null)
            throw new InvalidOperationException("TradeClient is not initialized.");

        var req = new OrderCloseRequest
        {
            Ticket = ticket,
            Volume = 0,
            Slippage = 0
        };

        // If the server returns an error, gRPC will throw an exception — we will catch it at the top
        _ = await TradeClient.OrderCloseAsync(req, cancellationToken: ct);
    }


    /// <summary>
    /// Closes an existing order by its ticket ID.
    /// Ensures the symbol is visible and normalizes volume before closing.
    /// Builds and sends an OrderCloseRequest via gRPC.
    /// Throws if the connection is not active or the server returns an error.
    /// Supports optional deadline and cancellation token.
    /// </summary>
public async Task CloseOrderByTicketAsync(
    ulong ticket,
    string symbol,
    double volume,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
{
    if (!IsConnected)
        throw new ConnectExceptionMT5("Please call Connect method firstly");

    await EnsureSymbolVisibleAsync(symbol, maxWait: TimeSpan.FromSeconds(3), deadline: deadline, cancellationToken: cancellationToken);

    var (min, step, max) = await GetVolumeConstraintsAsync(symbol, deadline, cancellationToken);
    var vol = NormalizeVolume(volume, min, step, max);

    var closeReq = new OrderCloseRequest
    {
        Ticket = ticket,
        Volume = vol
    };

    var closeRes = await OrderCloseAsync(closeReq, deadline, cancellationToken);

    switch (closeRes.ReturnedCode)
    {
        case 0:
        case 10008:
        case 10009:
        case 10024:
            _logger?.LogInformation("Close OK: ticket={Ticket} {Symbol} (retcode {Code} {Desc})",
                ticket, symbol, closeRes.ReturnedCode, closeRes.ReturnedCodeDescription);
            return;

        default:
            throw new InvalidOperationException($"Close failed: {closeRes.ReturnedCode} {closeRes.ReturnedCodeDescription}");
    }
}



    /// <summary>
    /// Creates a new MT5Account instance with given login and password.
    /// Optionally accepts a custom gRPC server address (default is mt5.mrpc.pro:443).
    /// You may also provide a predefined Guid identifier for this account.
    /// Stores credentials and connection settings for later use.
    /// </summary>
    public MT5Account(ulong user, string password, string? grpcServer = null, Guid id = default)
{
    // init
    User = user;
    Password = password ?? throw new ArgumentNullException(nameof(password));
    GrpcServer = string.IsNullOrWhiteSpace(grpcServer)
        ? "https://mt5.mrpc.pro:443"
        : grpcServer;
    Id = id;
}

// New overload with the logger — it is called by Program(...)
public MT5Account(ulong user, string password, string? grpcServer, Guid id, ILogger<MT5Account>? logger)
    : this(user, password, grpcServer, id) 
{
    _logger = logger;
}


    /// <summary>
    /// Reconnects the account to the MT5 server.
    /// First disconnects the current gRPC channel and clients.
    /// Then connects again using either ServerName or Host/Port.
    /// Ensures base chart symbol is set and terminal is alive.
    /// Supports deadline and cancellation token for control.
    /// </summary>
    public async Task Reconnect(DateTime? deadline, CancellationToken cancellationToken)
    {
        // Quenching the current connection
        await DisconnectAsync();

        // Where to reconnect: by ServerName or Host/Port
        if (!string.IsNullOrWhiteSpace(ServerName))
        {
            await ConnectByServerNameAsync(
                serverName: ServerName!,
                baseChartSymbol: BaseChartSymbol,
                waitForTerminalIsAlive: true,
                timeoutSeconds: ConnectTimeoutSeconds,
                deadline: deadline,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            await ConnectByHostPortAsync(
                host: Host,
                port: Port,
                baseChartSymbol: BaseChartSymbol,
                waitForTerminalIsAlive: true,
                timeoutSeconds: ConnectTimeoutSeconds,
                deadline: deadline,
                cancellationToken: cancellationToken
            );
        }
    }


    /// <summary>
    /// Checks if the given gRPC status code is considered transient.
    /// Transient codes trigger reconnect/backoff logic.
    /// Can be extended with more codes if needed.
    /// </summary>
    private static bool IsTransient(StatusCode code) =>
        code == StatusCode.Unavailable
        || code == StatusCode.DeadlineExceeded
        || code == StatusCode.Internal
        || code == StatusCode.Cancelled
        || code == StatusCode.Unknown;


    /// <summary>
    /// Computes exponential backoff with jitter for reconnect attempts.
    /// Base delay doubles each retry up to 8000 ms (max 5 steps).
    /// Adds ±250 ms random jitter to avoid thundering herd.
    /// Returns the calculated delay as TimeSpan.
    /// </summary>
    private static TimeSpan ComputeBackoff(int attempt)
    {
        var pow = Math.Pow(2, Math.Min(attempt, 5));         // 2,4,8,16,32,32
        var baseMs = (int)(250 * pow);                       // 500..8000
        var jitter = Random.Shared.Next(-250, 251);          // ±250
        return TimeSpan.FromMilliseconds(Math.Clamp(baseMs + jitter, 250, 8000));
    }


    /// <summary>
    /// Updates SL and/or TP for an existing position by ticket.
    /// Requires at least one of SL/TP; uses GTC as expiration type.
    /// Sends OrderModify via TradeClient; throws on invalid state.
    /// Returns true when the request is sent successfully.
    /// </summary>
    public async Task<bool> ModifyPositionSlTpAsync(ulong ticket, double? sl, double? tp, CancellationToken ct)
    {
        if (ticket == 0) throw new ArgumentOutOfRangeException(nameof(ticket));
        if (sl is null && tp is null) throw new ArgumentException("Either SL or TP must be provided.");
        if (TradeClient is null) throw new InvalidOperationException("TradeClient is not initialized.");

        // Build modification request (positions: only SL/TP relevant; keep GTC)
        var req = new OrderModifyRequest
        {
            Ticket = ticket,
            StopLoss = sl ?? 0,
            TakeProfit = tp ?? 0,
            ExpirationTimeType = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeGtc
        };

        // Submit modify call; server will validate distances etc.
        _ = await TradeClient.OrderModifyAsync(req, cancellationToken: ct);
        return true;
    }


    /// <summary>
    /// Connects to the MT5 terminal using credentials provided in the constructor.
    /// </summary>
    /// <param name="host">The IP address or domain of the MT5 server.</param>
    /// <param name="port">The port on which the MT5 server listens (default is 443).</param>
    /// <param name="baseChartSymbol">The base chart symbol to use (e.g., "EURUSD").</param>
    /// <param name="waitForTerminalIsAlive">Whether to wait for terminal readiness before returning.</param>
    /// <param name="timeoutSeconds">How long to wait for terminal readiness before timing out.</param>
    /// <returns>A task representing the asynchronous connection operation.</returns>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error response.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC connection fails.</exception>
    public async Task ConnectByHostPortAsync(
    string host,
    int port = 443,
    string baseChartSymbol = "EURUSD",
    bool waitForTerminalIsAlive = true,
    int timeoutSeconds = 30,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        // 1) create a channel/clients at
        var address = GrpcServer ?? $"https://{host}:{port}";
        CreateGrpcClients(address);

        // 2) preparing a request
        var connectRequest = new ConnectRequest
        {
            User = User,
            Password = Password,
            Host = host,
            Port = port,
            BaseChartSymbol = baseChartSymbol,
            WaitForTerminalIsAlive = waitForTerminalIsAlive,
            TerminalReadinessWaitingTimeoutSeconds = timeoutSeconds
        };

        Metadata? headers = null;
        if (Id != default)
            headers = new Metadata { { "id", Id.ToString() } };

        // 3) calling the RPC
        var res = await ConnectionClient!.ConnectAsync(connectRequest, headers, deadline, cancellationToken);

        // 4) error handling/state preservation
        if (res.Error is not null)
            throw new ApiExceptionMT5(res.Error);

        Host = host;
        Port = port;
        BaseChartSymbol = baseChartSymbol;
        ConnectTimeoutSeconds = timeoutSeconds;

        Id = Guid.TryParse(res.Data?.TerminalInstanceGuid, out var gid) ? gid : Guid.Empty;
    }


    /// <summary>
    /// Establishes a synchronous connection to the MT5 terminal.
    /// Wraps the asynchronous ConnectByHostPortAsync and waits for completion.
    /// Typically used in apps without async entrypoints.
    /// Allows specifying host, port, base symbol, readiness check, and timeout.
    /// </summary>
    public void Connect(
        string host,
        int port = 443,
        string baseChartSymbol = "EURUSD",
        bool waitForTerminalIsAlive = true,
        int timeoutSeconds = 30)
    {
        ConnectByHostPortAsync(host, port, baseChartSymbol, waitForTerminalIsAlive, timeoutSeconds).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Connects to the MT5 terminal using credentials provided in the constructor.
    /// </summary>
    /// <param name="serverName">MT5 server name from MT5 Terminal.</param>
    /// <param name="baseChartSymbol">The base chart symbol to use (e.g., "EURUSD").</param>
    /// <param name="waitForTerminalIsAlive">Whether to wait for terminal readiness before returning.</param>
    /// <param name="timeoutSeconds">How long to wait for terminal readiness before timing out.</param>
    /// <returns>A task representing the asynchronous connection operation.</returns>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error response.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC connection fails.</exception>
    public async Task ConnectByServerNameAsync(
    string serverName,
    string baseChartSymbol = "EURUSD",
    bool waitForTerminalIsAlive = true,
    int timeoutSeconds = 30,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        // 1) we take the address for gRPC from GrpcServer (if specified), otherwise default
        var address = GrpcServer ?? "https://mt5.mrpc.pro:443";
        CreateGrpcClients(address);

        // 2) preparing a request
        var connectRequest = new ConnectExRequest
        {
            User = User,
            Password = Password,
            MtClusterName = serverName,
            BaseChartSymbol = baseChartSymbol,
            TerminalReadinessWaitingTimeoutSeconds = timeoutSeconds
        };

        Metadata? headers = null;
        if (Id != default)
            headers = new Metadata { { "id", Id.ToString() } };

        // 3) calling the RPC
        var res = await ConnectionClient!.ConnectExAsync(connectRequest, headers, deadline, cancellationToken);

        // 4) error handling/state preservation
        if (res.Error is not null)
            throw new ApiExceptionMT5(res.Error);

        ServerName = serverName;
        BaseChartSymbol = baseChartSymbol;
        ConnectTimeoutSeconds = timeoutSeconds;

        Id = Guid.TryParse(res.Data?.TerminalInstanceGuid, out var gid) ? gid : Guid.Empty;
    }


    /// <summary>
    /// Executes a gRPC call with automatic reconnect handling.
    /// Wraps the provided call, adding retries on transient errors or lost terminal instance.
    /// If RpcException(Unavailable) or specific error codes are returned, reconnects and retries.
    /// Throws ApiExceptionMT5 for other server-side errors.
    /// Throws OperationCanceledException if canceled by the caller.
    /// </summary>
    private async Task<T> ExecuteWithReconnect<T>(
     Func<Metadata, T> grpcCall,
     Func<T, Mt5TermApi.Error?> errorSelector,
     DateTime? deadline,
     CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var headers = BuildHeaders();
            T res;

            try
            {
                res = grpcCall(headers);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
            {
                await Task.Delay(500, cancellationToken);
                await Reconnect(deadline, cancellationToken);
                continue; // retry
            }

            var error = errorSelector(res);

            // codes that need to be reconnected (leave/expand if desired)
            if (error?.ErrorCode == "TERMINAL_INSTANCE_NOT_FOUND" ||
                error?.ErrorCode == "TERMINAL_REGISTRY_TERMINAL_NOT_FOUND")
            {
                await Task.Delay(500, cancellationToken);
                await Reconnect(deadline, cancellationToken);
                continue;
            }

            if (error != null)
                throw new ApiExceptionMT5(error);

            return res;
        }

        throw new OperationCanceledException("The operation was canceled by the caller.");
    }


    // Trailing-stop algorithm selector.
    // Classic  — fixed-step/offset behind price.
    // Chandelier — ATR-based trail (e.g., N * ATR).
    public enum TrailMode { Classic = 0, Chandelier = 1 }


    // Map of active trailing workers by position ticket.
    // Value = CTS to cancel/stop the running trail task.
    // Thread-safe: supports concurrent start/stop calls.
    // IMPORTANT: TryRemove + Dispose CTS to avoid leaks;
    // cancel on manual close/SL hit/reconnect.
    private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _activeTrails = new();


    /// <summary>
    /// Starts a background trailing-stop for the given ticket.
    /// Distance/step are in symbol points (not pips).
    /// Replaces an existing worker for this ticket if present.
    /// Linked to the provided cancellation token; cleans up on exit.
    /// Returns immediately after scheduling.
    /// </summary>
    public Task StartTrailingAsync(
        ulong ticket, string symbol, bool isLong, int distancePoints, int stepPoints, TrailMode mode, CancellationToken ct)
    {
        // Basic validation of inputs
        if (ticket == 0) throw new ArgumentOutOfRangeException(nameof(ticket));
        if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol required", nameof(symbol));
        if (distancePoints <= 0 || stepPoints <= 0) throw new ArgumentOutOfRangeException("distance/step must be > 0");

        // Restart semantics: stop previous trailing for this ticket if any
        StopTrailing(ticket);

        // Link worker CTS to the caller’s token so external cancel stops the loop too
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // Register the worker; if a race occurs and the key exists, fail and dispose CTS
        if (!_activeTrails.TryAdd(ticket, linkedCts))
        {
            linkedCts.Dispose();
            throw new InvalidOperationException("Failed to register trailing CTS.");
        }

        // Spawn background trailing loop (fire-and-forget).
        // NOTE: RunTrailingLoopAsync must handle its own exceptions/cancellation.
        _ = Task.Run(() => RunTrailingLoopAsync(ticket, symbol, isLong, distancePoints, stepPoints, mode, linkedCts.Token))
                // On completion (success/cancel/fault), remove registration and dispose CTS
                .ContinueWith(_ =>
                {
                    _activeTrails.TryRemove(ticket, out var oldCts);
                    oldCts?.Dispose();
                });

        // This starter is synchronous from the caller’s perspective.
        return Task.CompletedTask;
    }


    /// <summary>
    /// Stops the trailing worker for the specified ticket.
    /// Thread-safe and idempotent; safe to call multiple times.
    /// Cancels and disposes the worker CTS if present.
    /// No-op if no worker is registered for this ticket.
    /// </summary>
    public void StopTrailing(ulong ticket)
    {
        if (_activeTrails.TryRemove(ticket, out var cts))
        {
            try { cts.Cancel(); } catch { /* ignore if already canceled/disposed */ }
            cts.Dispose();
        }
    }


    /// <summary>
    /// Runs the trailing-stop loop for a single ticket.
    /// Classic: offset from price; Chandelier: from running extreme.
    /// distancePts/stepPts are in symbol points; throttles SL updates.
    /// Polls ticks until canceled; ignores transient errors.
    /// </summary>
    private async Task RunTrailingLoopAsync(
        ulong ticket, string symbol, bool isLong, int distancePts, int stepPts, TrailMode mode, CancellationToken ct)
    {
        // Resolve point size (fallbacks for common FX/JPY if SDK value is unavailable).
        double point = GuessPointSizeInternal(symbol);
        if (point <= 0)
            point = symbol.EndsWith("JPY", StringComparison.OrdinalIgnoreCase) ? 0.01 : 0.0001;

        // Convert point-based inputs to price units.
        double distance = distancePts * point;
        double step = stepPts * point;

        // Running extreme for Chandelier mode:
        //  - long: track max(high); short: track min(low).
        double? extreme = null;

        // Throttle SL modifications to avoid spam and server-side limits.
        var lastChangeAt = DateTime.UtcNow.AddSeconds(-10);
        var minChangeInterval = TimeSpan.FromMilliseconds(500);

        // Poll ticks with a light cadence. Cancellation ends the loop.
        await foreach (var q in PollTicksAsync(symbol, TimeSpan.FromMilliseconds(200), ct))
        {
            // For longs we reference Bid; for shorts — Ask (conservative side).
            var price = isLong ? q.Bid : q.Ask;

            // Maintain running extreme for Chandelier.
            if (mode == TrailMode.Chandelier)
            {
                if (extreme is null) extreme = price;
                extreme = isLong ? Math.Max(extreme.Value, price)
                                 : Math.Min(extreme.Value, price);
            }

            // Compute target SL depending on mode.
            double targetSl =
                mode == TrailMode.Classic
                    ? (isLong ? price - distance : price + distance)
                    : (isLong ? extreme!.Value - distance : extreme!.Value + distance);

            // Read current SL (best-effort; null if not available).
            double? currentSl = await TryGetPositionSlAsync(ticket, ct);

            // We only move SL if it improves protection (tightens towards price).
            bool improves = currentSl is null
                ? true
                : (isLong ? targetSl > currentSl.Value
                          : targetSl < currentSl.Value);

            // And only if the move exceeds step threshold from the current SL.
            bool stepOk = currentSl is null
                ? true
                : (isLong ? (targetSl - currentSl.Value) >= step
                          : (currentSl.Value - targetSl) >= step);

            // Also respect minimal interval between modifications.
            bool intervalOk = DateTime.UtcNow - lastChangeAt >= minChangeInterval;

            if (improves && stepOk && intervalOk)
            {
                try
                {
                    // Apply SL change (TP untouched). Server validates Stops/Freeze levels.
                    await ModifyPositionSlTpAsync(ticket, targetSl, null, ct);
                    lastChangeAt = DateTime.UtcNow;
                }
                catch (OperationCanceledException) { throw; } // propagate cancel
                catch
                {
                    // Swallow transient errors and continue trailing.
                    // (Reconnect logic lives elsewhere; loop will pick up next tick.)
                }
            }
        }
    }


    /// <summary>
    /// Produces a lightweight (Bid, Ask, Time) tick stream by polling.
    /// Fallback for when native quote streaming is unavailable.
    /// Yields one item per interval; honors cancellation.
    /// Uses UTC pull time for Time; swap to server tick time if needed.
    /// </summary>
    private async IAsyncEnumerable<(double Bid, double Ask, DateTime Time)> PollTicksAsync(
        string symbol,
        TimeSpan interval,
        [EnumeratorCancellation] CancellationToken ct)
    {
        // Loop until the caller cancels the enumerator (ct) or the method throws.
        while (!ct.IsCancellationRequested)
        {
            // Fetch the latest tick snapshot for the symbol.
            // NOTE: exceptions bubble to the caller; if you need resiliency, catch/log and continue.
            var t = await SymbolInfoTickAsync(symbol, deadline: null, cancellationToken: ct);

            // Timestamp: using UTC *pull time* here. If you prefer server tick time,
            // wire MrpcMqlTick.time/time_msc when switching to the native stream.
            var when = DateTime.UtcNow;

            // Yield a simple (Bid, Ask, Time) tuple for lightweight consumers.
            yield return (t.Bid, t.Ask, when);

            // Pace the polling loop; break cleanly on cancellation during the delay.
            try
            {
                await Task.Delay(interval, ct); // consider min interval ≥ 100–200 ms to avoid hammering
            }
            catch (OperationCanceledException)
            {
                yield break; // cooperative shutdown
            }
        }
    }


    /// <summary>
    /// Heuristic fallback for symbol point size when SDK value is unavailable.
    /// JPY pairs → 0.01; metals (XAU/XAG) → 0.1; otherwise → 0.0001.
    /// Not a guaranteed broker tick size; prefer SymbolInfoDouble(SymbolPoint).
    /// Intended for internal use when precise params are missing.
    /// </summary>
    private static double GuessPointSizeInternal(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol)) return 0.0001;
        var s = symbol.ToUpperInvariant();
        if (s.Contains("JPY")) return 0.01;                     // common JPY pairs
        if (s.Contains("XAU") || s.Contains("XAG")) return 0.1; // metals (rough)
        return 0.0001;                                          // default FX fallback
    }


    /// <summary>
    /// Best-effort read of a position’s Stop Loss by ticket.
    /// Fetches opened positions, finds the ticket, then reads SL via StopLoss/SL/Sl.
    /// Returns the SL value or null if not available (missing position/field or errors).
    /// Non-throwing helper; pulls a full snapshot per call (consider caching or a dedicated RPC).
    /// </summary>
    private async Task<double?> TryGetPositionSlAsync(ulong ticket, CancellationToken ct)
    {
        try
        {
            var opened = await OpenedOrdersAsync(deadline: null, cancellationToken: ct);
            var pos = opened?.PositionInfos?.FirstOrDefault(p => Convert.ToUInt64(p.Ticket) == ticket);
            if (pos is null) return null; // nothing to read

            // Property name variations across SDKs
            if (HasProperty(pos, "StopLoss")) return GetDouble(pos, "StopLoss");
            if (HasProperty(pos, "SL")) return GetDouble(pos, "SL");
            if (HasProperty(pos, "Sl")) return GetDouble(pos, "Sl");
            return null;
        }
        catch
        {
            return null; // swallow and let caller decide on fallback
        }
    }


    /// <summary>
    /// Checks if a public instance property with the specified name exists on the object.
    /// Case-sensitive lookup; returns false when the object is null.
    /// Uses reflection and does not consider fields.
    /// Prefer TryGetDoubleProperty for numeric reads.
    /// </summary>
    private static bool HasProperty(object? o, string name) =>
        o?.GetType().GetProperty(name) is not null;


    /// <summary>
    /// Gets a public instance property by name and converts its value to double.
    /// Case-sensitive; returns null if object, property, or value is missing.
    /// Uses invariant culture for conversion and may throw on non-convertible values.
    /// For safer, exception-free parsing use TryGetDoubleProperty.
    /// </summary>
    private static double? GetDouble(object? o, string name)
    {
        if (o is null) return null;
        var p = o.GetType().GetProperty(name);
        if (p is null) return null;
        var val = p.GetValue(o);
        if (val is null) return null;
        return Convert.ToDouble(val, System.Globalization.CultureInfo.InvariantCulture);
    }


    /// <summary>
    /// Modifies an existing pending order by ticket.
    /// Supports updating price, stop, limit, SL/TP, and TIF parameters.
    /// Validates StopLimit invariants (limit ≤ stop for Buy, limit ≥ stop for Sell).
    /// Throws if parameters are invalid or inconsistent with the order type.
    /// Returns true if modification request was successfully sent.
    /// </summary>
    public async Task<bool> ModifyPendingOrderAsync(
        ulong ticket,
        string? type,               // "buylimit"|"selllimit"|"buystop"|"sellstop"|"buystoplimit"|"sellstoplimit" | null
        double? price,              // for limit/stop
        double? stop,               // for stop/stop-limit (trigger)
        double? limit,              // for stop-limit (limit price)
        double? sl,
        double? tp,
        string? tif,                // "GTC"|"DAY"|"GTD"|null
        DateTimeOffset? expire,
        CancellationToken ct)
    {
        if (ticket == 0) throw new ArgumentOutOfRangeException(nameof(ticket));
        if (price.HasValue && price.Value <= 0) throw new ArgumentOutOfRangeException(nameof(price));
        if (stop.HasValue && stop.Value <= 0) throw new ArgumentOutOfRangeException(nameof(stop));
        if (limit.HasValue && limit.Value <= 0) throw new ArgumentOutOfRangeException(nameof(limit));

        // If the caller gave us type, do client-side validation for stop-limit invariants.
        if (!string.IsNullOrWhiteSpace(type))
        {
            var k = type.Replace("-", "").Replace(".", "").Trim().ToLowerInvariant();
            var isStopLimit = k is "buystoplimit" or "sellstoplimit";
            var isLimitOrStop = k is "buylimit" or "selllimit" or "buystop" or "sellstop";

            if (isStopLimit)
            {
                if (!stop.HasValue || !limit.HasValue)
                    throw new ArgumentException("Stop-limit modify requires both --stop and --limit.");

                if (k == "buystoplimit" && !(limit.Value <= stop.Value))
                    throw new ArgumentException("Buy Stop Limit modify requires --limit <= --stop.");
                if (k == "sellstoplimit" && !(limit.Value >= stop.Value))
                    throw new ArgumentException("Sell Stop Limit modify requires --limit >= --stop.");

                if (price.HasValue)
                    throw new ArgumentException("Do not pass --price for stop-limit modify. Use --stop and --limit.");
            }
            else if (isLimitOrStop)
            {
                if (!price.HasValue)
                    throw new ArgumentOutOfRangeException(nameof(price), "For limit/stop modify, --price must be provided.");
            }
        }
        // Map TIF
        var tifEnum = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeGtc;
        if (!string.IsNullOrWhiteSpace(tif))
        {
            switch (tif.Trim().ToUpperInvariant())
            {
                case "GTC": tifEnum = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeGtc; break;
                case "DAY": tifEnum = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeDay; break;
                case "GTD":
                case "SPECIFIED":
                    tifEnum = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecified; break;
                case "SPECIFIED_DAY":
                    tifEnum = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecifiedDay; break;
                default:
                    throw new ArgumentException($"Unsupported TIF '{tif}'. Use GTC|DAY|GTD.");
            }
        }

        if ((tifEnum == TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecified
          || tifEnum == TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecifiedDay) && !expire.HasValue)
            throw new ArgumentException("When --tif=GTD/SPECIFIED(_DAY), 'expire' must be provided.");

        var req = new OrderModifyRequest
        {
            Ticket = ticket,
            Price = price ?? 0,                  // limit/stop
            StopLimit = stop ?? 0,                // <- THE TRIGGER for stop / stop-limit
            StopLoss = sl ?? 0,
            TakeProfit = tp ?? 0,
            ExpirationTimeType = tifEnum
        };

        if ((tifEnum == TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecified
          || tifEnum == TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecifiedDay) && expire.HasValue)
        {
            req.ExpirationTime = Timestamp.FromDateTimeOffset(expire.Value.ToUniversalTime());
        }

        // if this is a stop-limit and the limit has been passed, we put the LIMIT in the Price
        if (limit.HasValue)
            req.Price = limit.Value;

        // Calling the RPC. In your code, the method is usually called OrderModifyAsync on TradeClient.
        if (TradeClient is null)
            throw new InvalidOperationException("TradeClient is not initialized.");

        var resp = await TradeClient.OrderModifyAsync(req, cancellationToken: ct);

        // If your server throws errors through an exception, we won't get here in case of an error.
        // If a response object arrives without exception, you can additionally check its fields (if any).
        return true;
    }


    /// <summary>
    /// Retrieves a compact account summary (balances, equity, margin, etc.) from MT5.
    /// Uses ExecuteWithReconnect to handle transient errors and lost terminal instance.
    /// Throws if the connection is not active or the server returns an error.
    /// Supports optional deadline and cancellation token.
    /// Returns the summary payload from the server.
    /// </summary>
    public async Task<AccountSummaryData> AccountSummaryAsync(
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var accountClient = Require(AccountClient, nameof(AccountClient));
        var request = new AccountSummaryRequest();

        var res = await ExecuteWithReconnect(
            headers => accountClient.AccountSummary(request, headers, deadline, cancellationToken),
            r => r.Error, // server-side error extractor
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Returns a snapshot map of open positions: ticket → volume (lots).
    /// Optionally filters by <paramref name="symbol"/> (case-insensitive).
    /// Uses OpenedOrdersAsync to fetch current positions; orders are ignored.
    /// Returns an empty dictionary if there are no positions.
    /// </summary>
    /// <param name="symbol">Optional symbol filter (e.g., "EURUSD"); null/empty = all.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dictionary of position ticket to volume.</returns>
    public async Task<Dictionary<ulong, double>> ListPositionVolumesAsync(string? symbol, CancellationToken ct)
    {
        var opened = await OpenedOrdersAsync(deadline: null, cancellationToken: ct);

        var map = new Dictionary<ulong, double>();
        if (opened?.PositionInfos == null)
            return map;

        foreach (var p in opened.PositionInfos)
        {
            if (!string.IsNullOrWhiteSpace(symbol) &&
                !string.Equals(p.Symbol, symbol, StringComparison.OrdinalIgnoreCase))
                continue;

            ulong ticket = Convert.ToUInt64(p.Ticket);
            map[ticket] = p.Volume;
        }

        return map;
    }


    /// <summary>
    /// Infers position direction from common MT5-like fields.
    /// Checks Type text for Buy/Long or Sell/Short (case-insensitive).
    /// Falls back to IsBuy/IsLong boolean if available.
    /// Returns true for long, false for short; defaults to true if unknown.
    /// Reflection-based helper intended for heterogeneous SDK models.
    /// </summary>
    private static bool IsLongPositionLocal(object pos)
    {
        var typeProp = pos.GetType().GetProperty("Type");
        var typeStr = typeProp?.GetValue(pos)?.ToString() ?? string.Empty;

        if (typeStr.IndexOf("Buy", StringComparison.OrdinalIgnoreCase) >= 0) return true;
        if (typeStr.IndexOf("Long", StringComparison.OrdinalIgnoreCase) >= 0) return true;
        if (typeStr.IndexOf("Sell", StringComparison.OrdinalIgnoreCase) >= 0) return false;
        if (typeStr.IndexOf("Short", StringComparison.OrdinalIgnoreCase) >= 0) return false;

        var dirProp = pos.GetType().GetProperty("IsBuy") ?? pos.GetType().GetProperty("IsLong");
        if (dirProp != null) return Convert.ToBoolean(dirProp.GetValue(pos));

        return true;
    }

    /// <summary>
    /// Sets a position’s SL/TP by distances in symbol points from entry price.
    /// Detects direction (long/short), converts points → price using point size.
    /// Uses GuessPointSizeInternal as fallback and calls ModifyPositionSlTpAsync.
    /// Throws if the position is missing or both SL/TP are null.
    /// Returns true once the modify request is submitted.
    /// </summary>
    public async Task<bool> SetPositionSlTpByPointsAsync(ulong ticket, int? slPts, int? tpPts, CancellationToken ct)
    {
        if (ticket == 0) throw new ArgumentOutOfRangeException(nameof(ticket));
        if (slPts is null && tpPts is null)
            throw new ArgumentException("Either SL or TP points must be provided.");

        var opened = await OpenedOrdersAsync(deadline: null, cancellationToken: ct);
        var pos = opened.PositionInfos.FirstOrDefault(p => Convert.ToUInt64(p.Ticket) == ticket)
                  ?? throw new InvalidOperationException($"Position #{ticket} not found.");

        var symbol = pos.Symbol;
        var entry = pos.PriceOpen;
        var isLong = IsLongPositionLocal(pos);

        var point = GuessPointSizeInternal(symbol);
        if (point <= 0)
            point = symbol.EndsWith("JPY", StringComparison.OrdinalIgnoreCase) ? 0.01 : 0.0001;

        double? newSl = null, newTp = null;
        if (slPts is not null)
            newSl = isLong ? entry - slPts.Value * point : entry + slPts.Value * point;
        if (tpPts is not null)
            newTp = isLong ? entry + tpPts.Value * point : entry - tpPts.Value * point;

        await ModifyPositionSlTpAsync(ticket, newSl, newTp, ct);
        return true;
    }


    public double PointGuess(string symbol) => GuessPointSizeInternal(symbol);

    public async Task MovePendingByPointsAsync(ulong ticket, string symbol, int byPoints, CancellationToken ct)
    {
        if (ticket == 0) throw new ArgumentOutOfRangeException(nameof(ticket));
        if (byPoints == 0) return;
        if (TradeClient is null)
            throw new InvalidOperationException("TradeClient is not initialized.");

        var opened = await OpenedOrdersAsync(deadline: null, cancellationToken: ct);

        var ord = opened.OpenedOrders.FirstOrDefault(o => Convert.ToUInt64(o.Ticket) == ticket)
                  ?? throw new InvalidOperationException($"Pending order #{ticket} not found.");

        double point = GuessPointSizeInternal(symbol);
        if (point <= 0) point = 0.0001;
        double delta = byPoints * point;

        var priceProp = ord.GetType().GetProperty("Price");
        if (priceProp is null)
            throw new InvalidOperationException("Order object doesn't have 'Price' property.");

        double curPrice = Convert.ToDouble(priceProp.GetValue(ord) ?? 0.0, CultureInfo.InvariantCulture);
        double newPrice = curPrice + delta;

        var trigProp =
            ord.GetType().GetProperty("PriceTriggerStopLimit") ??
            ord.GetType().GetProperty("StopLimitPrice") ??
            ord.GetType().GetProperty("StopLimit");

        double? curTrig = trigProp is null ? null
                         : Convert.ToDouble(trigProp.GetValue(ord) ?? 0.0, CultureInfo.InvariantCulture);
        double? newTrig = curTrig is null ? null : curTrig + delta;

        var req = new OrderModifyRequest
        {
            Ticket = ticket,
            Price = newPrice,
            ExpirationTimeType = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeGtc
        };

        if (newTrig is not null)
            req.StopLimit = newTrig.Value;

        _ = await TradeClient.OrderModifyAsync(req, cancellationToken: ct);
    }

    /// <summary>
    /// Gets the summary information for a trading account synchronously.
    /// </summary>
    /// <param name="deadline">Optional deadline after which the request will be canceled if not completed.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the request.</param>
    /// <returns>The server's response containing account summary data.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected before calling this method.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails due to communication or protocol errors.</exception>
    public AccountSummaryData AccountSummary(
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return AccountSummaryAsync(deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the currently opened orders and positions for the connected account asynchronously.
    /// </summary>
    /// <param name="sortMode">The sort mode for the opened orders (0 - open time, 1 - close time, 2 - ticket ID).</param>
    /// <param name="deadline">Optional deadline after which the request will be canceled if not completed.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the request.</param>
    /// <returns>A task representing the asynchronous operation. The result contains opened orders and positions.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected before calling this method.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails due to communication or protocol errors.</exception>
    public async Task<OpenedOrdersData> OpenedOrdersAsync(
        BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var accountClient = Require(AccountClient, nameof(AccountClient));

        var request = new OpenedOrdersRequest { InputSortMode = sortMode };

        var res = await ExecuteWithReconnect(
            headers => Acc.OpenedOrders(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Gets the currently opened orders and positions for the connected account synchronously.
    /// </summary>
    /// <param name="sortMode">The sort mode for the opened orders (0 - open time, 1 - close time, 2 - ticket ID).</param>
    /// <param name="deadline">Optional deadline after which the request will be canceled if not completed.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the request.</param>
    /// <returns>The server's response containing opened orders and positions.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected before calling this method.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails due to communication or protocol errors.</exception>
    public OpenedOrdersData OpenedOrders(
        BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return OpenedOrdersAsync(sortMode, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the historical orders for the connected trading account within the specified time range asynchronously.
    /// </summary>
    /// <param name="from">The start time for the history query (server time).</param>
    /// <param name="to">The end time for the history query (server time).</param>
    /// <param name="sortMode">The sort mode: 0 - by open time, 1 - by close time, 2 - by ticket ID.</param>
    /// <param name="pageNumber">The page number for paginated results (default 0).</param>
    /// <param name="itemsPerPage">The number of items per page (default 0 = all).</param>
    /// <param name="deadline">Optional deadline after which the request will be canceled if not completed.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the request.</param>
    /// <returns>A task representing the asynchronous operation. The result contains paged historical order data.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<OrdersHistoryData> OrderHistoryAsync(
        DateTime from,
        DateTime to,
        BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sortMode = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
        int pageNumber = 0,
        int itemsPerPage = 0,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new OrderHistoryRequest
        {
            InputFrom = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(from.ToUniversalTime()),
            InputTo = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(to.ToUniversalTime()),
            InputSortMode = sortMode,
            PageNumber = pageNumber,
            ItemsPerPage = itemsPerPage
        };

        var res = await ExecuteWithReconnect(
            headers => Acc.OrderHistory(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }


    /// <summary>
    /// Gets the historical orders for the connected trading account within the specified time range synchronously.
    /// </summary>
    /// <param name="from">The start time for the history query (server time).</param>
    /// <param name="to">The end time for the history query (server time).</param>
    /// <param name="sortMode">The sort mode: 0 - by open time, 1 - by close time, 2 - by ticket ID.</param>
    /// <param name="pageNumber">The page number for paginated results (default 0).</param>
    /// <param name="itemsPerPage">The number of items per page (default 0 = all).</param>
    /// <param name="deadline">Optional deadline after which the request will be canceled if not completed.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the request.</param>
    /// <returns>The server's response containing paged historical order data.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public OrdersHistoryData OrderHistory(
        DateTime from,
        DateTime to,
        BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sortMode = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
        int pageNumber = 0,
        int itemsPerPage = 0,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return OrderHistoryAsync(from, to, sortMode, pageNumber, itemsPerPage, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Gets ticket IDs of all currently opened orders and positions asynchronously.
    /// </summary>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task containing collection of opened order and position tickets.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<OpenedOrdersTicketsData> OpenedOrdersTicketsAsync(DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new OpenedOrdersTicketsRequest();

        var res = await ExecuteWithReconnect(
            headers => Acc.OpenedOrdersTickets(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }


    /// <summary>
    /// Gets ticket IDs of all currently opened orders and positions synchronously.
    /// </summary>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Collection of opened order and position tickets.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public OpenedOrdersTicketsData OpenedOrdersTickets(DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return OpenedOrdersTicketsAsync(deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Retrieves symbol parameters for multiple instruments asynchronously.
    /// </summary>
    /// <param name="request">The request containing filters and pagination.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task containing symbol parameter details.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<SymbolParamsManyData> SymbolParamsManyAsync(SymbolParamsManyRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var res = await ExecuteWithReconnect(
            headers => Acc.SymbolParamsMany(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }


    /// <summary>
    /// Retrieves symbol parameters for multiple instruments synchronously.
    /// </summary>
    /// <param name="request">The request containing filters and pagination.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Symbol parameter details.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public SymbolParamsManyData SymbolParamsMany(SymbolParamsManyRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return SymbolParamsManyAsync(request, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Gets tick value and tick size data for the given symbols asynchronously.
    /// </summary>
    /// <param name="symbols">List of symbol names.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task containing tick value and contract size info per symbol.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<TickValueWithSizeData> TickValueWithSizeAsync(IEnumerable<string> symbols, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new TickValueWithSizeRequest();
        request.SymbolNames.AddRange(symbols);

        var res = await ExecuteWithReconnect(
            headers => Acc.TickValueWithSize(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Gets tick value and tick size data for the given symbols synchronously.
    /// </summary>
    /// <param name="symbols">List of symbol names.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Tick value and contract size info per symbol.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public TickValueWithSizeData TickValueWithSize(IEnumerable<string> symbols, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return TickValueWithSizeAsync(symbols, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Retrieves historical positions based on filter and time range asynchronously.
    /// </summary>
    /// <param name="sortType">Sorting type for historical positions.</param>
    /// <param name="openFrom">Optional start of open time filter (UTC).</param>
    /// <param name="openTo">Optional end of open time filter (UTC).</param>
    /// <param name="page">Optional page number.</param>
    /// <param name="size">Optional items per page.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task containing historical position records.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<PositionsHistoryData> PositionsHistoryAsync(
        AH_ENUM_POSITIONS_HISTORY_SORT_TYPE sortType,
        DateTime? openFrom = null,
        DateTime? openTo = null,
        int page = 0,
        int size = 0,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new PositionsHistoryRequest
        {
            SortType = sortType,
            PageNumber = page,
            ItemsPerPage = size
        };

        if (openFrom.HasValue)
            request.PositionOpenTimeFrom = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(openFrom.Value.ToUniversalTime());

        if (openTo.HasValue)
            request.PositionOpenTimeTo = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(openTo.Value.ToUniversalTime());

        var res = await ExecuteWithReconnect(
            headers => Acc.PositionsHistory(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Retrieves historical positions based on filter and time range synchronously.
    /// </summary>
    /// <param name="sortType">Sorting type for historical positions.</param>
    /// <param name="openFrom">Optional start of open time filter (UTC).</param>
    /// <param name="openTo">Optional end of open time filter (UTC).</param>
    /// <param name="page">Optional page number.</param>
    /// <param name="size">Optional items per page.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Historical position records.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public PositionsHistoryData PositionsHistory(
        AH_ENUM_POSITIONS_HISTORY_SORT_TYPE sortType,
        DateTime? openFrom = null,
        DateTime? openTo = null,
        int page = 0,
        int size = 0,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return PositionsHistoryAsync(sortType, openFrom, openTo, page, size, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Sends a market or pending order to the trading server asynchronously.
    /// </summary>
    /// <param name="request">The order request to send.</param>
    /// <param name="deadline">Optional deadline for the operation.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task containing response with deal/order confirmation data.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<OrderSendData> OrderSendAsync(OrderSendRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var res = await ExecuteWithReconnect(
            headers => Trade.OrderSend(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }


    /// <summary>
    /// Sends a market or pending order to the trading server synchronously.
    /// </summary>
    /// <param name="request">The order request to send.</param>
    /// <param name="deadline">Optional deadline for the operation.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Response containing deal/order confirmation data.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public OrderSendData OrderSend(OrderSendRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return OrderSendAsync(request, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Modifies an existing order or position asynchronously.
    /// </summary>
    /// <param name="request">The modification request (SL, TP, price, expiration, etc.).</param>
    /// <param name="deadline">Optional deadline for the operation.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task containing updated order/deal info.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<OrderModifyData> OrderModifyAsync(OrderModifyRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var res = await ExecuteWithReconnect(
           headers => Trade.OrderModify(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Modifies an existing order or position synchronously.
    /// </summary>
    /// <param name="request">The modification request (SL, TP, price, expiration, etc.).</param>
    /// <param name="deadline">Optional deadline for the operation.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Response containing updated order/deal info.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public OrderModifyData OrderModify(OrderModifyRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return OrderModifyAsync(request, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Closes a market or pending order asynchronously.
    /// </summary>
    /// <param name="request">The close request including ticket, volume, and slippage.</param>
    /// <param name="deadline">Optional deadline for the operation.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task containing the close result and return codes.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<OrderCloseData> OrderCloseAsync(OrderCloseRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var res = await ExecuteWithReconnect(
            headers => Trade.OrderClose(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }


    /// <summary>
    /// Closes a market or pending order synchronously.
    /// </summary>
    /// <param name="request">The close request including ticket, volume, and slippage.</param>
    /// <param name="deadline">Optional deadline for the operation.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Response describing the close result and return codes.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public OrderCloseData OrderClose(OrderCloseRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return OrderCloseAsync(request, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Executes a gRPC server-streaming call with automatic reconnection logic on recoverable errors.
    /// </summary>
    /// <typeparam name="TRequest">The request type sent to the stream method.</typeparam>
    /// <typeparam name="TReply">The reply type received from the stream.</typeparam>
    /// <typeparam name="TData">The extracted data type yielded to the consumer.</typeparam>
    /// <param name="request">The request object to initiate the stream with.</param>
    /// <param name="streamInvoker">
    /// A delegate that opens the stream. It receives the request, metadata headers, and cancellation token, 
    /// and returns an <see cref="Grpc.Core.AsyncServerStreamingCall{TReply}"/>.
    /// </param>
    /// <param name="getErrorCode">
    /// A delegate that extracts the error code (if any) from a <typeparamref name="TReply"/> instance.
    /// Return <c>"TERMINAL_INSTANCE_NOT_FOUND"</c> to trigger reconnection logic, or any non-null code to throw <see cref="ApiExceptionMT5"/>.
    /// </param>
    /// <param name="getData">
    /// A delegate that extracts the data object from a <typeparamref name="TReply"/> instance.
    /// Return <c>null</c> to skip the current message.
    /// </param>
    /// <param name="headers">The gRPC metadata headers to include in the stream request.</param>
    /// <param name="cancellationToken">Optional cancellation token to stop streaming and reconnection attempts.</param>
    /// <returns>
    /// An <see cref="IAsyncEnumerable{T}"/> of extracted <typeparamref name="TData"/> items streamed from the server.
    /// </returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if reconnection logic fails due to missing account context.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown when the stream response contains a known API error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if a non-recoverable gRPC error occurs.</exception>
    private async IAsyncEnumerable<TData> ExecuteStreamWithReconnect<TRequest, TReply, TData>(
    TRequest request,
    Func<TRequest, Metadata, CancellationToken, AsyncServerStreamingCall<TReply>> streamInvoker,
    Func<TReply, Mt5TermApi.Error?> getError,
    Func<TReply, TData?> getData,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var reconnectRequired = false;

            AsyncServerStreamingCall<TReply>? stream = null;
            try
            {
                var headers = BuildHeaders();
                stream = streamInvoker(request, headers, cancellationToken);

                var responseStream = stream.ResponseStream;

                while (true)
                {
                    TReply reply;

                    try
                    {
                        if (!await responseStream.MoveNext(cancellationToken))
                            break; // Stream ended naturally

                        reply = responseStream.Current;
                    }
                    catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable) // || ex.StatusCode == StatusCode.Internal
                    {
                        reconnectRequired = true;
                        break; // Trigger reconnect
                    }

                    var error = getError(reply);
                    if (error?.ErrorCode == "TERMINAL_INSTANCE_NOT_FOUND")
                    {
                        reconnectRequired = true;
                        break; // Trigger reconnect
                    }
                    else if (error?.ErrorCode == "TERMINAL_REGISTRY_TERMINAL_NOT_FOUND")
                    {
                        reconnectRequired = true;
                        break; // Trigger reconnect
                    }

                    if (error != null)
                        throw new ApiExceptionMT5(error);

                    var data = getData(reply);
                    if (data != null)
                        yield return data; // Real-time yield outside try-catch
                }
            }
            finally
            {
                stream?.Dispose();
            }

            if (reconnectRequired)
            {
                await Task.Delay(500, cancellationToken);
                await Reconnect(null, cancellationToken);
            }
            else
            {
                break; // Exit loop normally
            }
        }
    }


    /// <summary>
    /// Subscribes to real-time tick data for specified symbols.
    /// </summary>
    /// <param name="symbols">The symbol names to subscribe to.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Async stream of tick data responses.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="Grpc.Core.RpcException">If the stream fails.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if an error is received from the stream.</exception>
    public async IAsyncEnumerable<OnSymbolTickData> OnSymbolTickAsync(
        IEnumerable<string> symbols,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new OnSymbolTickRequest();
        request.SymbolNames.AddRange(symbols);

        await foreach (var data in ExecuteStreamWithReconnect<OnSymbolTickRequest, OnSymbolTickReply, OnSymbolTickData>(
            request,
            (req, headers, ct) => Subs.OnSymbolTick(req, headers, cancellationToken: ct),
            reply => reply.Error,
            reply => reply.Data,
            cancellationToken))
        {
            yield return data;
        }
    }


    /// <summary>
    /// Subscribes to all trade-related events: orders, deals, positions.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Async stream of trade event data.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the stream fails.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the stream returns a known API error.</exception>
    public async IAsyncEnumerable<OnTradeData> OnTradeAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new OnTradeRequest();

        await foreach (var data in ExecuteStreamWithReconnect<OnTradeRequest, OnTradeReply, OnTradeData>(
            request,
            (req, headers, ct) => Subs.OnTrade(req, headers, cancellationToken: ct),
            reply => reply.Error, // assumes OnTradeReply has Error field of type Mt5TermApi.Error?
            reply => reply.Data,       // pass the full reply object as data
            cancellationToken))
        {
            yield return data;
        }
    }


    /// <summary>
    /// Subscribes to real-time profit updates for open positions.
    /// </summary>
    /// <param name="intervalMs">Interval in milliseconds to poll server.</param>
    /// <param name="ignoreEmpty">Skip frames with no change.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Async stream of profit updates.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the stream fails.</exception>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async IAsyncEnumerable<OnPositionProfitData> OnPositionProfitAsync(
    int intervalMs,
    bool ignoreEmpty = true,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new OnPositionProfitRequest
        {
            TimerPeriodMilliseconds = intervalMs,
            IgnoreEmptyData = ignoreEmpty
        };

        await foreach (var data in ExecuteStreamWithReconnect<OnPositionProfitRequest, OnPositionProfitReply, OnPositionProfitData>(
            request,
            (req, headers, ct) => Subs.OnPositionProfit(req, headers, cancellationToken: ct),
            reply => reply.Error,   // Assumes OnPositionProfitReply has an Error field
            reply => reply.Data,         // Yield the full reply object
            cancellationToken))
        {
            yield return data;
        }
    }


    /// <summary>
    /// Subscribes to updates of position and pending order ticket IDs.
    /// </summary>
    /// <param name="intervalMs">Polling interval in milliseconds.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Async stream of ticket ID snapshots.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the stream fails.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the stream returns a known API error.</exception>
    public async IAsyncEnumerable<OnPositionsAndPendingOrdersTicketsData> OnPositionsAndPendingOrdersTicketsAsync(
    int intervalMs,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new OnPositionsAndPendingOrdersTicketsRequest
        {
            TimerPeriodMilliseconds = intervalMs
        };

        await foreach (var data in ExecuteStreamWithReconnect<
            OnPositionsAndPendingOrdersTicketsRequest,
            OnPositionsAndPendingOrdersTicketsReply,
            OnPositionsAndPendingOrdersTicketsData>(
            request,
            (req, headers, ct) => Subs.OnPositionsAndPendingOrdersTickets(req, headers, cancellationToken: ct),
            reply => reply.Error,   // Assumes reply has an Error field of type Mt5TermApi.Error?
            reply => reply.Data,    // Return full reply
            cancellationToken))
        {
            yield return data;
        }
    }


    /// <summary>
    /// Subscribes to real-time trade transaction events such as order creation, update, or execution.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Async stream of trade transaction replies.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the stream fails.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the stream returns a known API error.</exception>
    public async IAsyncEnumerable<OnTradeTransactionData> OnTradeTransactionAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new OnTradeTransactionRequest();

        await foreach (var data in ExecuteStreamWithReconnect<
            OnTradeTransactionRequest,
            OnTradeTransactionReply,
            OnTradeTransactionData>(
            request,
            (req, headers, ct) => Subs.OnTradeTransaction(req, headers, cancellationToken: ct),
            reply => reply.Error,   // Assumes reply has an Error property of type Mt5TermApi.Error?
            reply => reply.Data,         // Pass the entire reply
            cancellationToken))
        {
            yield return data;
        }
    }


    /// <summary>
    /// Calculates the margin required for a planned trade operation.
    /// </summary>
    /// <param name="request">The request containing symbol, order type, volume, and price.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The required margin in account currency.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if gRPC fails to connect or respond.</exception>
    public async Task<OrderCalcMarginData> OrderCalcMarginAsync(
    OrderCalcMarginRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var res = await ExecuteWithReconnect(
            headers => TF.OrderCalcMargin(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Calculates the margin required for a planned trade operation.
    /// </summary>
    /// <param name="request">The request containing symbol, order type, volume, and price.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The required margin in account currency.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if gRPC fails to connect or respond.</exception>
    public OrderCalcMarginData OrderCalcMargin(OrderCalcMarginRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return OrderCalcMarginAsync(request, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Checks whether a trade request can be successfully executed under current market conditions.
    /// </summary>
    /// <param name="request">The trade request to validate.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Result of the trade request check, including margin and balance details.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<OrderCheckData> OrderCheckAsync(
    OrderCheckRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var res = await ExecuteWithReconnect(
            headers => TF.OrderCheck(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Checks whether a trade request can be successfully executed under current market conditions.
    /// </summary>
    /// <param name="request">The trade request to validate.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Result of the trade request check, including margin and balance details.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public OrderCheckData OrderCheck(OrderCheckRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return OrderCheckAsync(request, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Returns the total number of open positions on the current account.
    /// </summary>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The total number of open positions.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<PositionsTotalData> PositionsTotalAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var res = await ExecuteWithReconnect(
            headers => TF.PositionsTotal(new Google.Protobuf.WellKnownTypes.Empty(), headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Returns the total number of open positions on the current account.
    /// </summary>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The total number of open positions.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public PositionsTotalData PositionsTotal(DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return PositionsTotalAsync(deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Returns the total number of symbols available on the platform.
    /// </summary>
    /// <param name="selectedOnly">True to count only Market Watch symbols, false to count all.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Total symbol count data.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<SymbolsTotalData> SymbolsTotalAsync(bool selectedOnly, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolsTotalRequest { Mode = selectedOnly };

        var res = await ExecuteWithReconnect(
            headers => Mkt.SymbolsTotal(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    // <summary>
    /// Returns the total number of symbols available on the platform.
    /// </summary>
    /// <param name="selectedOnly">True to count only Market Watch symbols, false to count all.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Total symbol count data.</returns>
    /// <exception cref="ConnectException"/>
    /// <exception cref="ApiException"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public SymbolsTotalData SymbolsTotal(bool selectedOnly, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return SymbolsTotalAsync(selectedOnly, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Checks if a symbol with the specified name exists (standard or custom).
    /// </summary>
    /// <param name="symbol">The symbol name to check.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Information about symbol existence and type.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<SymbolExistData> SymbolExistAsync(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolExistRequest { Name = symbol };

        var res = await ExecuteWithReconnect(
            headers => Mkt.SymbolExist(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Checks if a symbol with the specified name exists (standard or custom).
    /// </summary>
    /// <param name="symbol">The symbol name to check.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Information about symbol existence and type.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public SymbolExistData SymbolExist(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return SymbolExistAsync(symbol, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Returns the name of a symbol by index.
    /// </summary>
    /// <param name="index">Symbol index (starting at 0).</param>
    /// <param name="selected">True to use only Market Watch symbols.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The symbol name at the specified index.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<SymbolNameData> SymbolNameAsync(int index, bool selected, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolNameRequest { Index = index, Selected = selected };

        var res = await ExecuteWithReconnect(
            headers => Mkt.SymbolName(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Returns the name of a symbol by index.
    /// </summary>
    /// <param name="index">Symbol index (starting at 0).</param>
    /// <param name="selected">True to use only Market Watch symbols.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The symbol name at the specified index.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public SymbolNameData SymbolName(int index, bool selected, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return SymbolNameAsync(index, selected, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Adds or removes a symbol from Market Watch.
    /// </summary>
    /// <param name="symbol">Symbol name.</param>
    /// <param name="select">True to add, false to remove.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Success status of the operation.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<SymbolSelectData> SymbolSelectAsync(string symbol, bool select, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolSelectRequest { Symbol = symbol, Select = select };

        var res = await ExecuteWithReconnect(
            headers => Mkt.SymbolSelect(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Adds or removes a symbol from Market Watch.
    /// </summary>
    /// <param name="symbol">Symbol name.</param>
    /// <param name="select">True to add, false to remove.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Success status of the operation.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public SymbolSelectData SymbolSelect(string symbol, bool select, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return SymbolSelectAsync(symbol, select, deadline, cancellationToken).GetAwaiter().GetResult();
    }



    /// <summary>
    /// Checks if the symbol's data is synchronized with the server.
    /// </summary>
    /// <param name="symbol">Symbol name to check.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if synchronized, false otherwise.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<SymbolIsSynchronizedData> SymbolIsSynchronizedAsync(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolIsSynchronizedRequest { Symbol = symbol };

        var res = await ExecuteWithReconnect(
            headers => Mkt.SymbolIsSynchronized(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }
    /// <summary>
    /// Checks if the symbol's data is synchronized with the server.
    /// </summary>
    /// <param name="symbol">Symbol name to check.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if synchronized, false otherwise.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public SymbolIsSynchronizedData SymbolIsSynchronized(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return SymbolIsSynchronizedAsync(symbol, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Retrieves a double-precision property value of a symbol.
    /// </summary>
    /// <param name="symbol">Symbol name.</param>
    /// <param name="property">The double-type property to retrieve.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The double property value.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<SymbolInfoDoubleData> SymbolInfoDoubleAsync(
    string symbol,
    SymbolInfoDoubleProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolInfoDoubleRequest { Symbol = symbol, Type = property };

        var res = await ExecuteWithReconnect(
            headers => Mkt.SymbolInfoDouble(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }
    /// <summary>
    /// Retrieves a double-precision property value of a symbol.
    /// </summary>
    /// <param name="symbol">Symbol name.</param>
    /// <param name="property">The double-type property to retrieve.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The double property value.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public SymbolInfoDoubleData SymbolInfoDouble(
        string symbol,
        SymbolInfoDoubleProperty property,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return SymbolInfoDoubleAsync(symbol, property, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Retrieves an integer-type property value of a symbol.
    /// </summary>
    /// <param name="symbol">Symbol name.</param>
    /// <param name="property">The integer property to query.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The integer property value.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<SymbolInfoIntegerData> SymbolInfoIntegerAsync(
    string symbol,
    SymbolInfoIntegerProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolInfoIntegerRequest { Symbol = symbol, Type = property };

        var res = await ExecuteWithReconnect(
            headers => Mkt.SymbolInfoInteger(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Retrieves an integer-type property value of a symbol.
    /// </summary>
    /// <param name="symbol">Symbol name.</param>
    /// <param name="property">The integer property to query.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The integer property value.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public SymbolInfoIntegerData SymbolInfoInteger(
        string symbol,
        SymbolInfoIntegerProperty property,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return SymbolInfoIntegerAsync(symbol, property, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Retrieves a string-type property value of a symbol.
    /// </summary>
    /// <param name="symbol">Symbol name.</param>
    /// <param name="property">The string property to retrieve.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The string property value.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<SymbolInfoStringData> SymbolInfoStringAsync(
    string symbol,
    SymbolInfoStringProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolInfoStringRequest { Symbol = symbol, Type = property };

        var res = await ExecuteWithReconnect(
            headers => Mkt.SymbolInfoString(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Retrieves a string-type property value of a symbol.
    /// </summary>
    /// <param name="symbol">Symbol name.</param>
    /// <param name="property">The string property to retrieve.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The string property value.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public SymbolInfoStringData SymbolInfoString(
        string symbol,
        SymbolInfoStringProperty property,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return SymbolInfoStringAsync(symbol, property, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Retrieves the margin rates for a given symbol and order type.
    /// </summary>
    /// <param name="symbol">Symbol name.</param>
    /// <param name="orderType">The order type (buy/sell/etc).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The initial and maintenance margin rates.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<SymbolInfoMarginRateData> SymbolInfoMarginRateAsync(
    string symbol,
    ENUM_ORDER_TYPE orderType,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolInfoMarginRateRequest { Symbol = symbol, OrderType = orderType };

        var res = await ExecuteWithReconnect(
            headers => Mkt.SymbolInfoMarginRate(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Retrieves the margin rates for a given symbol and order type.
    /// </summary>
    /// <param name="symbol">Symbol name.</param>
    /// <param name="orderType">The order type (buy/sell/etc).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The initial and maintenance margin rates.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public SymbolInfoMarginRateData SymbolInfoMarginRate(
        string symbol,
        ENUM_ORDER_TYPE orderType,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return SymbolInfoMarginRateAsync(symbol, orderType, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Retrieves the current tick data (bid, ask, last, volume) for a given symbol.
    /// </summary>
    /// <param name="symbol">Symbol name to fetch tick info for.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The latest tick information.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<MrpcMqlTick> SymbolInfoTickAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolInfoTickRequest { Symbol = symbol };

        var res = await ExecuteWithReconnect(
            headers => Mkt.SymbolInfoTick(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Retrieves the current tick data (bid, ask, last, volume) for a given symbol.
    /// </summary>
    /// <param name="symbol">Symbol name to fetch tick info for.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The latest tick information.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public MrpcMqlTick SymbolInfoTick(
        string symbol,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return SymbolInfoTickAsync(symbol, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Gets the quoting session start and end time for a symbol on a specific day and session index.
    /// </summary>
    /// <param name="symbol">The symbol name.</param>
    /// <param name="dayOfWeek">The day of the week.</param>
    /// <param name="sessionIndex">Index of the quoting session (starting at 0).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The session quote start and end time.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<SymbolInfoSessionQuoteData> SymbolInfoSessionQuoteAsync(
    string symbol,
    mt5_term_api.DayOfWeek dayOfWeek,
    uint sessionIndex,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolInfoSessionQuoteRequest
        {
            Symbol = symbol,
            DayOfWeek = dayOfWeek,
            SessionIndex = sessionIndex
        };

        var res = await ExecuteWithReconnect(
            headers => Mkt.SymbolInfoSessionQuote(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Gets the quoting session start and end time for a symbol on a specific day and session index.
    /// </summary>
    /// <param name="symbol">The symbol name.</param>
    /// <param name="dayOfWeek">The day of the week.</param>
    /// <param name="sessionIndex">Index of the quoting session (starting at 0).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The session quote start and end time.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public SymbolInfoSessionQuoteData SymbolInfoSessionQuote(
        string symbol,
        mt5_term_api.DayOfWeek dayOfWeek,
        uint sessionIndex,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return SymbolInfoSessionQuoteAsync(symbol, dayOfWeek, sessionIndex, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Gets the trading session start and end time for a symbol on a specific day and session index.
    /// </summary>
    /// <param name="symbol">The symbol name.</param>
    /// <param name="dayOfWeek">The day of the week.</param>
    /// <param name="sessionIndex">Index of the trading session (starting at 0).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The trading session start and end time.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<SymbolInfoSessionTradeData> SymbolInfoSessionTradeAsync(
    string symbol,
    mt5_term_api.DayOfWeek dayOfWeek,
    uint sessionIndex,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolInfoSessionTradeRequest
        {
            Symbol = symbol,
            DayOfWeek = dayOfWeek,
            SessionIndex = sessionIndex
        };

        var res = await ExecuteWithReconnect(
            headers => Mkt.SymbolInfoSessionTrade(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Gets the trading session start and end time for a symbol on a specific day and session index.
    /// </summary>
    /// <param name="symbol">The symbol name.</param>
    /// <param name="dayOfWeek">The day of the week.</param>
    /// <param name="sessionIndex">Index of the trading session (starting at 0).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The trading session start and end time.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public SymbolInfoSessionTradeData SymbolInfoSessionTrade(
        string symbol,
        mt5_term_api.DayOfWeek dayOfWeek,
        uint sessionIndex,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return SymbolInfoSessionTradeAsync(symbol, dayOfWeek, sessionIndex, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Opens the Depth of Market (DOM) for a symbol and subscribes to updates.
    /// </summary>
    /// <param name="symbol">Symbol name to subscribe.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if DOM subscription was successful.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<MarketBookAddData> MarketBookAddAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new MarketBookAddRequest { Symbol = symbol };

        var res = await ExecuteWithReconnect(
            headers => Mkt.MarketBookAdd(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Opens the Depth of Market (DOM) for a symbol and subscribes to updates.
    /// </summary>
    /// <param name="symbol">Symbol name to subscribe.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if DOM subscription was successful.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public MarketBookAddData MarketBookAdd(
        string symbol,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return MarketBookAddAsync(symbol, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Releases the Depth of Market (DOM) for a symbol and stops receiving updates.
    /// </summary>
    /// <param name="symbol">Symbol name to unsubscribe.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if DOM release was successful.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<MarketBookReleaseData> MarketBookReleaseAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new MarketBookReleaseRequest { Symbol = symbol };

        var res = await ExecuteWithReconnect(
            headers => Mkt.MarketBookRelease(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Releases the Depth of Market (DOM) for a symbol and stops receiving updates.
    /// </summary>
    /// <param name="symbol">Symbol name to unsubscribe.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if DOM release was successful.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public MarketBookReleaseData MarketBookRelease(
        string symbol,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return MarketBookReleaseAsync(symbol, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Gets the current Depth of Market (DOM) data for a symbol.
    /// </summary>
    /// <param name="symbol">Symbol name.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of book entries for the symbol's DOM.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public async Task<MarketBookGetData> MarketBookGetAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new MarketBookGetRequest { Symbol = symbol };

        var res = await ExecuteWithReconnect(
            headers => Mkt.MarketBookGet(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
    }


    /// <summary>
    /// Gets the current Depth of Market (DOM) data for a symbol.
    /// </summary>
    /// <param name="symbol">Symbol name.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of book entries for the symbol's DOM.</returns>
    /// <exception cref="ConnectExceptionMT5"/>
    /// <exception cref="ApiExceptionMT5"/>
    /// <exception cref="Grpc.Core.RpcException"/>
    public MarketBookGetData MarketBookGet(
        string symbol,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return MarketBookGetAsync(symbol, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    // -------------- Close position (single implementation + forwarder) --------------

    public async Task ClosePositionFullAsync(ulong ticket, double volume, int deviation, CancellationToken ct)
    {
        if (ticket == 0) throw new ArgumentOutOfRangeException(nameof(ticket));
        if (volume <= 0) throw new ArgumentOutOfRangeException(nameof(volume));
        if (TradeClient is null)
            throw new InvalidOperationException("TradeClient is not initialized.");

        var req = new OrderCloseRequest
        {
            Ticket = ticket,
            Volume = volume,
            Slippage = Math.Max(0, deviation)
        };

        _ = await TradeClient.OrderCloseAsync(req, cancellationToken: ct);
    }


    public Task ClosePositionPartialAsync(ulong ticket, double volume, int deviation, CancellationToken ct)
        => ClosePositionFullAsync(ticket, volume, deviation, ct);


    // Emulated Close By: two OrderClose calls with the same volume
    public async Task CloseByEmulatedAsync(ulong ticketA, ulong ticketB, double volume, int deviation, CancellationToken ct)
    {
        if (ticketA == 0 || ticketB == 0) throw new ArgumentOutOfRangeException(nameof(ticketA), "Tickets must be > 0.");
        if (volume <= 0) throw new ArgumentOutOfRangeException(nameof(volume));
        if (TradeClient is null)
            throw new InvalidOperationException("TradeClient is not initialized.");

        // Leg A
        var reqA = new OrderCloseRequest
        {
            Ticket = ticketA,
            Volume = volume,
            Slippage = Math.Max(0, deviation) // OrderCloseRequest.Slippage is int32
        };
        await TradeClient.OrderCloseAsync(reqA, cancellationToken: ct);

        // Leg B
        var reqB = new OrderCloseRequest
        {
            Ticket = ticketB,
            Volume = volume,
            Slippage = Math.Max(0, deviation)
        };
        await TradeClient.OrderCloseAsync(reqB, cancellationToken: ct);
    }

    // Place a pending limit/stop order (NOT stop-limit)
    public async Task<ulong> PlacePendingOrderAsync(
        string symbol,
        string type,             // "buylimit"|"selllimit"|"buystop"|"sellstop"
        double volume,
        double price,            // entry price for limit/stop
        double? sl,
        double? tp,
        string? tif,             // "GTC"|"DAY"|"GTD"|null
        DateTimeOffset? expire,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("symbol must be provided", nameof(symbol));
        if (volume <= 0) throw new ArgumentOutOfRangeException(nameof(volume));
        if (price <= 0) throw new ArgumentOutOfRangeException(nameof(price));

        var op = type.Replace("-", "").Replace(".", "").Trim().ToLowerInvariant() switch
        {
            "buylimit" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyLimit,
            "selllimit" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellLimit,
            "buystop" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyStop,
            "sellstop" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellStop,
            _ => throw new NotSupportedException($"Unsupported type for PlacePendingOrderAsync: '{type}'")
        };

        // TIF
        var tifEnum = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeGtc;
        if (!string.IsNullOrWhiteSpace(tif))
        {
            switch (tif.Trim().ToUpperInvariant())
            {
                case "GTC": tifEnum = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeGtc; break;
                case "DAY": tifEnum = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeDay; break;
                case "GTD":
                case "SPECIFIED": tifEnum = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecified; break;
                case "SPECIFIED_DAY": tifEnum = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecifiedDay; break;
                default: throw new ArgumentException($"Unsupported TIF '{tif}'. Use GTC|DAY|GTD.");
            }
        }
        if ((tifEnum == TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecified
          || tifEnum == TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecifiedDay) && !expire.HasValue)
            throw new ArgumentException("When TIF=GTD/SPECIFIED(_DAY), 'expire' must be provided.");

        // ensure symbol visible (server idempotent)
        await EnsureSymbolVisibleAsync(symbol, cancellationToken: ct);

        if (TradeClient is null)
            throw new InvalidOperationException("TradeClient is not initialized.");

        // Build OrderSendRequest
        var req = new OrderSendRequest
        {
            Symbol = symbol,
            Operation = op,
            Volume = volume,
            Price = price,                // limit/stop entry
            StopLimitPrice = 0,              // not used here
            StopLoss = sl ?? 0,
            TakeProfit = tp ?? 0,
            Slippage = 0UL,                // pending: slippage not used
            ExpirationTimeType = tifEnum
        };

        if ((tifEnum == TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecified
          || tifEnum == TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecifiedDay) && expire.HasValue)
        {
            req.ExpirationTime = Timestamp.FromDateTimeOffset(expire.Value.ToUniversalTime());
        }

        // Send order
        var reply = await TradeClient!.OrderSendAsync(req, cancellationToken: ct); 

        if (reply.ResponseCase == OrderSendReply.ResponseOneofCase.Error)
            throw new InvalidOperationException($"OrderSend error: {reply.Error}"); 

        return reply.Data?.Order ?? 0UL;

    }


    /// <summary>
    /// Retrieves a double-precision account property (e.g. balance, equity, margin).
    /// </summary>
    /// <param name="property">The account double property to retrieve.</param>
    /// <param name="deadline">Optional deadline after which the call will be cancelled.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>The double value of the requested account property.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<double> AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new AccountInfoDoubleRequest { PropertyId = property };

        var res = await ExecuteWithReconnect(
            headers => AccInfo.AccountInfoDouble(request, headers, deadline, cancellationToken),
            _ => null, // no error field in AccountInfoDoubleReply
            deadline,
            cancellationToken
        );

        return res.RequestedValue;
    }


    /// <summary>
    /// Retrieves a double-precision account property (e.g. balance, equity, margin).
    /// </summary>
    /// <param name="property">The account double property to retrieve.</param>
    /// <param name="deadline">Optional deadline after which the call will be cancelled.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>The double value of the requested account property.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public double AccountInfoDouble(
    AccountInfoDoublePropertyType property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        return AccountInfoDoubleAsync(property, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Retrieves an integer account property (e.g. login, leverage, trade mode).
    /// </summary>
    /// <param name="property">The account integer property to retrieve.</param>
    /// <param name="deadline">Optional deadline after which the call will be cancelled.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>The integer value of the requested account property.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<int> AccountInfoIntegerAsync(
    AccountInfoIntegerPropertyType property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new AccountInfoIntegerRequest { PropertyId = property };

        var res = await ExecuteWithReconnect(
            headers => AccInfo.AccountInfoInteger(request, headers, deadline, cancellationToken),
            _ => null, // No error field in AccountInfoIntegerReply
            deadline,
            cancellationToken
        );

        return res.RequestedValue;
    }


    /// <summary>
    /// Retrieves an integer account property (e.g. login, leverage, trade mode).
    /// </summary>
    /// <param name="property">The account integer property to retrieve.</param>
    /// <param name="deadline">Optional deadline after which the call will be cancelled.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>The integer value of the requested account property.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public int AccountInfoInteger(
    AccountInfoIntegerPropertyType property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        return AccountInfoIntegerAsync(property, deadline, cancellationToken).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Retrieves a string account property (e.g. account name, currency, server).
    /// </summary>
    /// <param name="property">The account string property to retrieve.</param>
    /// <param name="deadline">Optional deadline after which the call will be cancelled.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>The string value of the requested account property.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<string> AccountInfoStringAsync(
    AccountInfoStringPropertyType property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new AccountInfoStringRequest { PropertyId = property };

        var res = await ExecuteWithReconnect(
            headers => AccInfo.AccountInfoString(request, headers, deadline, cancellationToken),
            _ => null, // No error field in AccountInfoStringReply
            deadline,
            cancellationToken
        );

        return res.RequestedValue;
    }


    /// <summary>
    /// Retrieves a string account property (e.g. account name, currency, server).
    /// </summary>
    /// <param name="property">The account string property to retrieve.</param>
    /// <param name="deadline">Optional deadline after which the call will be cancelled.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>The string value of the requested account property.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public string AccountInfoString(
        AccountInfoStringPropertyType property,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return AccountInfoStringAsync(property, deadline, cancellationToken).GetAwaiter().GetResult();
    }
}
