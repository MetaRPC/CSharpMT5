using System.CommandLine;
using System.Text.Json;                
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mt5_term_api;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Grpc.Core;
using System.CommandLine.Invocation;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MetaRPC.CSharpMT5
{
    public sealed class MT5Options
    {
        public ulong AccountId { get; set; }
        public string? Password { get; set; }

        // MT5 trading server name (e.g., "MetaQuotes-Demo")
        public string? ServerName { get; set; }

        // For building gRPC URL if GrpcServer is not set
        public string? Host { get; set; }
        public int Port { get; set; } = 443;

        // Full gRPC endpoint to the MT5 bridge, e.g. "https://95.217.147.61:443"
        public string? GrpcServer { get; set; }

        public string DefaultSymbol { get; set; } = "EURUSD";
        public double DefaultVolume { get; set; } = 0.1;
}


    public class Program
    {
        
        private string? _currentGrpc;
        private readonly ILogger<Program> _logger;
        private readonly IConfiguration _configuration;
        private MT5Account _mt5Account;
        private static readonly TimeSpan _rpcTimeout = TimeSpan.FromSeconds(30);    
        private string _selectedProfile = "default";
        private readonly ILogger<MT5Account> _accLogger;


        // --- Helpers for market side ---
        // Parses textual side into a boolean flag: Buy = true, Sell = false.
        // Accepts case-insensitive aliases: "buy"/"b" and "sell"/"s".
        // Throws ArgumentException for any other input.
        private static bool ParseMarketSideIsBuy(string side)
        {
            // Normalize input: trim spaces and lowercase using invariant culture.
            return side.Trim().ToLowerInvariant() switch
            {
                // Buy → true
                "buy" or "b" => true,

                // Sell → false
                "sell" or "s" => false,

                // Unknown token → explicit error for the caller
                _ => throw new ArgumentException("Unknown side. Use: buy | sell")
            };
        }


        // Tries to read a numeric (double) property by any of the provided names.
        // - Uses reflection (public instance properties only), so this is O(#names) per call.
        // - Returns the *first* successfully parsed value (order of names = precedence).
        // - Accepts actual double, other numeric types (via IConvertible), and numeric strings.
        // - Returns null when property is missing, value is null, or cannot be parsed.
        // NOTE: current lookup is case-sensitive; use GetProperty(name, BindingFlags.Public|Instance|IgnoreCase)
        //       if you want case-insensitive matching. Consider caching PropertyInfo per Type for performance.
        private static double? TryGetDoubleProperty(object obj, params string[] names)
        {
            var t = obj.GetType(); // reflection target type

            foreach (var n in names)
            {
                // Only finds public instance properties with exact (case-sensitive) name.
                var p = t.GetProperty(n);
                if (p is null) continue;

                var v = p.GetValue(obj);
                if (v is null) continue;

                // Fast path: exact double
                if (v is double d) return d;

                // Other numeric types (int, long, float, decimal, etc.) and many built-ins implement IConvertible.
                // WARNING: bool also implements IConvertible (true → 1, false → 0). If that’s undesired, skip bool explicitly.
                if (v is IConvertible)
                {
                    try
                    {
                        // Culture-invariant conversion to avoid locale-specific decimal separators.
                        return Convert.ToDouble(v, CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        // Not convertible (e.g., DateTime) → try next candidate name.
                    }
                }

                // Strings like "1.2345" (InvariantCulture). Redundant for most cases because strings are IConvertible,
                // but kept as an explicit, cheap fallback.
                if (v is string s && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                    return parsed;
            }

            // No property matched or none were parsable.
            return null;
        }

        
        // Parse order kind (market/limit/stop/stop-limit) from CLI string.
        // 1) Parse order kind (market/limit/stop/stop-limit)
        static TMT5_ENUM_ORDER_TYPE ParseOrderType(string kind)
        {
            var k = kind.Replace(".", "").Replace("-", "").Trim().ToLowerInvariant();
            return k switch
            {
                "buy" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
                "sell" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSell,
                "buylimit" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyLimit,
                "selllimit" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellLimit,
                "buystop" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyStop,
                "sellstop" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellStop,
                "buystoplimit" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyStopLimit,
                "sellstoplimit" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellStopLimit,
                _ => throw new ArgumentException($"Unknown order kind '{kind}'.")
            };
        }


        // 2) Parse time-in-force (TIF)
        static TMT5_ENUM_ORDER_TYPE_TIME ParseTif(string? tif)
        {
            if (string.IsNullOrWhiteSpace(tif)) return TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeGtc;

            switch ((tif ?? "").Trim().ToUpperInvariant())
            {
                case "GTC": return TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeGtc;           // Good-Till-Cancel
                case "DAY": return TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeDay;           // Good-For-Day
                case "GTD": return TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecified;     // Good-Till-Date

                case "SPECIFIED": return TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecified;
                case "SPECIFIED_DAY": return TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecifiedDay;
                default: throw new ArgumentException($"Unsupported TIF '{tif}'. Use GTC|DAY|GTD.");
            }
        }


        // Detects whether a position is long (buy) using common MT5-like fields.
        // Prefers a direct boolean IsBuy when available.
        // Otherwise inspects Type/PositionType.ToString() for "buy/long" or "sell/short" (case-insensitive).
        // Returns true for long, false for short; throws if neither convention is present (e.g., localized enum names).
        // NOTE: This version does not read a boolean IsLong; add it if your SDK exposes it.
        private static bool IsLongPosition(object pos)
        {
            var t = pos.GetType();

            // 1) Direct boolean flag: IsBuy
            var isBuyProp = t.GetProperty("IsBuy");
            if (isBuyProp is not null && isBuyProp.PropertyType == typeof(bool))
                return (bool)(isBuyProp.GetValue(pos) ?? false);

            // 2) Enum-like property with a descriptive name (e.g., Buy/Sell or Long/Short)
            var typeProp = t.GetProperty("Type") ?? t.GetProperty("PositionType");
            if (typeProp is not null)
            {
                var val = typeProp.GetValue(pos);
                if (val is not null)
                {
                    var name = val.ToString()?.ToLowerInvariant() ?? string.Empty;
                    if (name.Contains("buy") || name.Contains("long")) return true;
                    if (name.Contains("sell") || name.Contains("short")) return false;
                }
            }

            // If neither convention is present, we cannot reliably infer direction.
            throw new NotSupportedException("Cannot determine position direction (buy/sell) from the SDK model.");
        }


        //-----=================================-----
        //-----===Simple DTO for quote output===-----
        //-----=================================-----
        private sealed class QuoteDto
        {
            public string Symbol { get; init; } = "";
            public double Bid { get; init; }
            public double Ask { get; init; }
            public DateTime? TimeUtc { get; init; }
        }


        // Returns the first available tick for the given symbol (snapshot).
        // Subscribes to the quote stream and exits on the first item.
        // Propagates cancellation/transport errors from the stream.
        // If the stream completes without any item, throws OperationCanceledException.
        // NOTE: TimeUtc comes from server Timestamp (UTC); consider ToDateTimeOffset() if you need offset/Kind preserved.
        private async Task<QuoteDto> FirstTickAsync(string symbol, CancellationToken ct)
        {
            // Return the first incoming tick and stop
            await foreach (var tick in _mt5Account.OnSymbolTickAsync(new[] { symbol }, ct))
            {
                return new QuoteDto
                {
                    Symbol = tick.SymbolTick.Symbol,
                    Bid = tick.SymbolTick.Bid,
                    Ask = tick.SymbolTick.Ask,
                    TimeUtc = tick.SymbolTick.Time?.ToDateTime()
                };
            }

            // Stream ended without any items (unusual) → treat as cancellation
            throw new OperationCanceledException("No tick received.");
        }


        // Maps textual side to MT5 order type enum.
        // Accepts case-insensitive aliases: "buy"/"b" → Buy, "sell"/"s" → Sell.
        // Trims input and lowercases using invariant culture.
        // Throws ArgumentException for any other value with a clear usage hint.
        private static TMT5_ENUM_ORDER_TYPE ParseMarketSideEnum(string side)
        {
            return side.Trim().ToLowerInvariant() switch
            {
                "buy" or "b" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
                "sell" or "s" => TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSell,
                _ => throw new ArgumentException("Unknown side. Use: buy | sell")
            };
        }


        //-----=================================-----
        //-----===-- Ticket search helpers --===-----
        //-----=================================-----

        
        // Searches an aggregate (e.g., OpenedOrders reply) for an item with a matching Ticket.
        // Scans all public properties that are IEnumerable, iterates items, and checks "Ticket"/"ticket" property.
        // Supports ulong/long ticket values; returns the found item and outputs the owning property name via bucketName.
        // Returns null if not found. Reflection-heavy; O(#props + total items). Case-sensitive prop search (Ticket/ticket only).
        private static object? TryFindByTicketInAggregate(object openedAggregate, ulong ticket, out string? bucketName)
        {
            bucketName = null;
            var t = openedAggregate.GetType();

            foreach (var p in t.GetProperties())
            {
                // Only sequences (Lists, arrays, etc.). Strings will be harmlessly skipped later.
                if (!typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType)) continue;

                // Null-safe fetch
                if (p.GetValue(openedAggregate) is not System.Collections.IEnumerable seq) continue;

                foreach (var item in seq)
                {
                    if (item is null) continue;

                    // Ticket / ticket (case-sensitive)
                    var it = item.GetType();
                    var tp = it.GetProperty("Ticket") ?? it.GetProperty("ticket");
                    if (tp is null) continue;

                    // Accept ulong or long tickets
                    var v = tp.GetValue(item);
                    bool match =
                        v is ulong ut && ut == ticket ||
                        v is long lt && unchecked((ulong)lt) == ticket;

                    if (match)
                    {
                        bucketName = p.Name; // which collection the item came from
                        return item;
                    }
                }
            }
            return null;
        }


        // Returns the first public *instance* property value (by the given names) that is assignable to T.
        // Case-sensitive lookup; tries names in order (earlier = higher precedence).
        // Returns default(T) when no match or incompatible type. Reflection-only (fields ignored).
        // NOTE: Nullable pitfall — typeof(double?).IsAssignableFrom(typeof(double)) == false.
        //       If you pass T=double? and the prop is double, this will skip it. Handle via Nullable.GetUnderlyingType if needed.
        private static T? Get<T>(object obj, params string[] names)
        {
            var it = obj.GetType();
            foreach (var n in names)
            {
                var p = it.GetProperty(n); // public instance, case-sensitive
                if (p != null && typeof(T).IsAssignableFrom(p.PropertyType))
                    return (T?)p.GetValue(obj);
            }
            return default;
        }


        //-------=======================================-------
        //-------==== Per-operation timeout plumbing ===-------
        //-------=======================================-------

        
        // Per-operation timeout override (set via UseOpTimeout). 
        // NOTE: instance-level mutable state → NOT thread-safe. 
        // Concurrent calls on the same MT5Account can stomp each other’s override.
        // For multi-threaded use consider AsyncLocal<TimeSpan?> instead.
        private TimeSpan? _opTimeoutOverride; // set per-command from --timeout-ms


        // Creates a CTS for a single RPC using the per-op override if set,
        // otherwise falls back to the default _rpcTimeout.
        // USAGE: using var cts = StartOpCts(); await rpc(..., cts.Token);
        // NOTE: not linked to an external CancellationToken — link explicitly if needed.
        private CancellationTokenSource StartOpCts() =>
            new CancellationTokenSource(_opTimeoutOverride ?? _rpcTimeout);


        // Temporarily sets a per-op timeout for the duration of a using-block.
        // Restores the previous value on dispose (supports nested scopes).
        // Clamp guard: 100ms..120s to avoid accidental zero/huge values.
        // USAGE: using (UseOpTimeout(5000)) { await CallAsync(...); }
        // WARNING: concurrent scopes on the SAME instance can interfere;
        // prefer per-call parameter or AsyncLocal for full safety.
        private IDisposable UseOpTimeout(int timeoutMs)
        {
            var prev = _opTimeoutOverride;
            var clamped = Math.Clamp(timeoutMs, 100, 120_000); // safety: 0.1s..120s
            _opTimeoutOverride = TimeSpan.FromMilliseconds(clamped);
            return new ResetTimeout(this, prev);
        }


        // Server-streaming runner with auto-reconnect until the overall duration elapses or the caller cancels.
        // Strategy:
        //  - Bound each streaming attempt with a linked CTS to the global deadline.
        //  - Ensure symbol visibility before (re)subscribing (non-fatal on failure).
        //  - On RpcException with a transient status → log, disconnect, reconnect, and retry.
        //  - Exponential backoff with jitter (±200 ms), capped at 10 s between attempts.
        // Normal exit paths: DoStreamingAsync completes (deadline hit or external cancel).
        // NOTE: DisconnectAsync is best-effort; ConnectAsync encapsulates its own retry logic.
        // TIP: If multiple symbols are used, consider batching visibility checks and/or caching successes.
        private async Task StreamWithReconnectAsync(string symbol, TimeSpan duration, CancellationToken ct)
        {
            var deadline = DateTime.UtcNow + duration;
            var delay = TimeSpan.FromMilliseconds(500); // backoff start

            while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
            {
                // Bound this attempt so it cannot run past the global deadline.
                using var remainingCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var remaining = deadline - DateTime.UtcNow;
                if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
                remainingCts.CancelAfter(remaining);

                try
                {
                    // Prepare symbol (idempotent server-side); non-fatal on errors.
                    try
                    {
                        using var visCts = StartOpCts();
                        await _mt5Account.EnsureSymbolVisibleAsync(
                            symbol, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
                        // continue; we'll still attempt the stream
                    }

                    // Run streaming; returns on cancellation (deadline/ct) or normal completion.
                    await DoStreamingAsync(remainingCts.Token);
                    return; // graceful exit
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested || DateTime.UtcNow >= deadline)
                {
                    // Caller canceled or duration elapsed.
                    return;
                }
                catch (Grpc.Core.RpcException ex) when (_transient.Contains(ex.StatusCode))
                {
                    // Transient transport/server hiccup; will back off and retry.
                    _logger.LogWarning("Stream transient gRPC error {Status}. Reconnect in {Delay}...", ex.Status.StatusCode, delay);
                }
                catch (Exception ex)
                {
                    // Unexpected error; log and retry with backoff.
                    _logger.LogWarning(ex, "Stream error. Reconnect in {Delay}...", delay);
                }

                // Try to tear down before reconnecting (ignore failures).
                try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }

                // Reconnect; ConnectAsync is responsible for its own retry policy.
                try
                {
                    await ConnectAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Reconnect failed, will retry in {Delay}...");
                }

                // Backoff with jitter; cap subsequent delays.
                var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(-200, 200));
                var wait = delay + jitter;
                await Task.Delay(wait, ct);
                delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, 10_000));
            }
        }


        // Attempts to modify SL/TP by ticket using whatever MT5Account exposes across SDK versions.
        // Tries known method shapes in priority order (A → B → C); awaits and returns on first success.
        // Reflection notes:
        //  - GetMethod is case-sensitive and returns a single (often first) overload; if multiple exist,
        //    consider using GetMethod(name, Type[]) to disambiguate.
        //  - Passing null placeholders assumes the target params are nullable (double?, Timestamp?, etc.).
        //    If an overload takes non-nullable double, null will throw at Invoke; adjust to proper overloads if needed.
        //  - Only public instance methods are discovered by default.
        private async Task ModifyStopsSmartAsync(ulong ticket, double? sl, double? tp, CancellationToken ct)
        {
            var acc = _mt5Account;
            var t = acc.GetType();

            // Variant A: ModifyStopsByTicketAsync(ticket, sl, tp, deadline, ct)
            // Most explicit API; ignores price/expiration/stopLimit.
            var m1 = t.GetMethod("ModifyStopsByTicketAsync");
            if (m1 is not null)
            {
                var task = (Task?)m1.Invoke(acc, new object?[] { ticket, sl, tp, /*deadline*/ null, ct });
                if (task != null) await task; // exceptions propagate naturally
                return;
            }

            // Variant B: OrderModifyAsync(ticket, price, sl, tp, expiration, stopLimit, deadline, ct)
            // Generic modify entrypoint; we only set SL/TP, leave others as null/defaults.
            var m2 = t.GetMethod("OrderModifyAsync");
            if (m2 is not null)
            {
                var task = (Task?)m2.Invoke(acc, new object?[]
                {
            ticket,
            /*price      */ null,    // must be nullable in the signature
            /*stopLoss   */ sl,
            /*takeProfit */ tp,
            /*expiration */ null,
            /*stopLimit  */ null,
            /*deadline   */ null,
            ct
                });
                if (task != null) await task;
                return;
            }

            // Variant C: ModifyOrderByTicketAsync(ticket, price, sl, tp, expiration, stopLimit, deadline, ct)
            // Less common alias; parameter order assumed same as Variant B.
            var m3 = t.GetMethod("ModifyOrderByTicketAsync");
            if (m3 is not null)
            {
                var task = (Task?)m3.Invoke(acc, new object?[]
                {
            ticket, /*price*/ null, sl, tp, /*expiration*/ null, /*stopLimit*/ null, /*deadline*/ null, ct
                });
                if (task != null) await task;
                return;
            }

            // No compatible API found on this MT5Account version.
            throw new NotSupportedException(
                "MT5Account does not expose a supported modify method. " +
                "Update MetaRPC.MT5 or add a wrapper.");
        }


        // Scope guard for per-operation timeout override.
        // Saves previous _opTimeoutOverride and restores it on Dispose (even on exceptions).
        // NOTE: Instance-level and not thread-safe across concurrent scopes on the same Program;
        // the last disposed scope wins (typical for simple using-scopes).
        // Usage: using (UseOpTimeout(5000)) { /* RPCs with override */ } → auto-restore afterward.
        private sealed class ResetTimeout : IDisposable
        {
            private readonly Program _p;
            private readonly TimeSpan? _prev;
            public ResetTimeout(Program p, TimeSpan? prev) { _p = p; _prev = prev; }
            public void Dispose() => _p._opTimeoutOverride = _prev;
        }


        private static string ToJson(object obj) =>
    System.Text.Json.JsonSerializer.Serialize(obj, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

        // English-only comments (as requested)
        internal static LogLevel MinLogLevel = LogLevel.Information;

        // ----------------------------------- Load MT5Options from appsettings.json ------------------------------------


        // Reads the "MT5Options" section and binds to MT5Options; falls back to a new instance if missing.
        // NOTE: No validation here — if you need required fields, validate after retrieval.
        private MT5Options GetOptions() =>
            _configuration.GetSection("MT5Options").Get<MT5Options>() ?? new MT5Options();


        // English-only comments (as requested)
        // True when logger is at Debug (or more verbose). Use to gate detailed output/diagnostics.
        private bool IsDetailed() => _logger.IsEnabled(LogLevel.Debug);


        // Output helpers
        // Case-insensitive check for "json" output mode (CLI flag, config, etc.).
        private static bool IsJson(string? output) =>
            string.Equals(output, "json", StringComparison.OrdinalIgnoreCase);


        // ------------------------------------  Timeout/Retry helpers  --------------------------------------
        

        // Per-call CTS is created elsewhere (StartOpCts / UseOpTimeout).
        // This section adds **retry-on-transient** wrappers with backoff+jitter.

        // gRPC status codes considered transient (safe to retry).
        // Tweak as needed; some setups also include Unknown, while Cancelled is typically *not* retried.
        // ResourceExhausted may indicate rate limiting; backoff helps avoid hot loops.
        private static readonly StatusCode[] _transient =
        {
    StatusCode.Unavailable,
    StatusCode.DeadlineExceeded,
    StatusCode.Internal,
    StatusCode.ResourceExhausted
};


        // Generic retry with exponential backoff + jitter.
        // - Propagates OperationCanceledException immediately (ct passed through).
        // - Non-transient RpcException also bubbles without retry.
        // - `maxAttempts` counts total tries (last failure rethrows; no silent swallow).
        // - Log line prints "Retry {attempt}/{maxAttempts-1}" which reads as "retry N of M" (M = retries, not attempts).
        private async Task<T> CallWithRetry<T>(
            Func<CancellationToken, Task<T>> call,
            CancellationToken ct,
            int maxAttempts = 4)
        {
            var delay = TimeSpan.FromMilliseconds(400);

            for (int attempt = 1; ; attempt++)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    return await call(ct);
                }
                catch (RpcException ex) when (_transient.Contains(ex.StatusCode) && attempt < maxAttempts)
                {
                    // Exponential backoff + jitter (±200 ms), capped to 10 s.
                    var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(-200, 200));
                    var wait = delay + jitter;

                    _logger.LogWarning("Transient gRPC error {Status}. Retry {Attempt}/{Max} in {Delay}...",
                        ex.Status.StatusCode, attempt, maxAttempts - 1, wait);

                    await Task.Delay(wait, ct);
                    delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, 10_000));
                }
                // Last attempt falls through and rethrows, preserving the original exception.
            }
        }


        // Void overload convenience; delegates to generic version and discards the dummy result.
        // Keep the same retry semantics and logging as the generic overload.
        private Task CallWithRetry(Func<CancellationToken, Task> call, CancellationToken ct, int maxAttempts = 4) =>
            CallWithRetry(async c => { await call(c); return true; }, ct, maxAttempts);


        // Closes a batch of positions by ticket with per-op timeout and retry.
        // For each item: StartOpCts() → CallWithRetry(...) → log → ok/fail counters.
        // Uses _mt5Account.CloseOrderByTicketAsync(ticket,symbol,volume,...).
        // NOTE: The method parameter `ct` is not used; only the per-op CTS drives cancellation.
        //       If you need cooperative cancel for the whole batch, link both tokens.
        private async Task<(int ok, int fail)> ClosePositionsAsync(
            IEnumerable<(ulong Ticket, string Symbol, double Volume)> batch,
            CancellationToken ct)
        {
            int ok = 0, fail = 0;
            foreach (var (ticket, symbol, volume) in batch)
            {
                try
                {
                    using var opCts = StartOpCts();
                    await CallWithRetry(
                        c => _mt5Account.CloseOrderByTicketAsync(
                                ticket: ticket,
                                symbol: symbol,
                                volume: volume,
                                deadline: null,
                                cancellationToken: c),
                        opCts.Token);

                    _logger.LogInformation("Closed #{Ticket} {Symbol} vol={Vol}", ticket, symbol, volume);
                    ok++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Close failed for #{Ticket} {Symbol}", ticket, symbol);
                    fail++;
                }
            }
            return (ok, fail);
        }


      public Program()
{
    // --- Configuration: files + env ---
    _configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile("profiles.json",   optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

    // --- Logger factory ---
    var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.ClearProviders();
        builder.AddSimpleConsole(o =>
        {
            o.IncludeScopes = true;
            o.SingleLine = false;
            o.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
        });
        builder.AddDebug();
        builder.SetMinimumLevel(Program.MinLogLevel);
    });

    // Create loggers
    _logger     = loggerFactory.CreateLogger<Program>();
    _accLogger  = loggerFactory.CreateLogger<MT5Account>(); // <-- important

    // --- Base initialization from appsettings.json (profile not selected yet) ---
    var options = GetOptions();

    // We DON'T require a password here; ConnectAsync() will validate it.
    string password = Environment.GetEnvironmentVariable("MT5_PASSWORD")
                      ?? options.Password
                      ?? string.Empty;

    // Prefer GrpcServer > Host:Port > SDK default
    var grpc =
        !string.IsNullOrWhiteSpace(options.GrpcServer) ? options.GrpcServer :
        !string.IsNullOrWhiteSpace(options.Host)       ? $"https://{options.Host}:{options.Port}" :
        null;

    _currentGrpc = grpc;

    // ✅ Instantiate account WITH logger here (NOT near fields)
    _mt5Account = new MT5Account(options.AccountId, password, grpc, id: default, logger: _accLogger);
}




        // ------------------------------------- Load profile from profiles.json -------------------------------------

        
        // NOTE: Despite the "(case-insensitive)" note, the lookup below is actually **case-sensitive**,
        //       because JsonSerializer will build a Dictionary<string, MT5Options> with the default comparer.
        //       `PropertyNameCaseInsensitive` only affects POCO property names, not dictionary KEYS.
        //
        // Suggestion (keep code unchanged if you like):
        //   var ci = new Dictionary<string, MT5Options>(dict, StringComparer.OrdinalIgnoreCase);
        //   if (ci.TryGetValue(name, out var opt) && opt is not null) return opt;
        //
        // Falls back to appsettings.json via GetOptions() on missing file/parse errors.
        private MT5Options LoadProfile(string name)
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "profiles.json");

                if (!File.Exists(path))
                    return GetOptions(); // fallback to appsettings.json

                var json = File.ReadAllText(path);
                var dict = JsonSerializer.Deserialize<Dictionary<string, MT5Options>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Case-sensitive lookup as written:
                if (dict is not null && dict.TryGetValue(name, out var opt) && opt is not null)
                    return opt;

                // Case-insensitive alternative (uncomment if desired):
                // if (dict is not null)
                // {
                //     var ci = new Dictionary<string, MT5Options>(dict, StringComparer.OrdinalIgnoreCase);
                //     if (ci.TryGetValue(name, out var opt2) && opt2 is not null) return opt2;
                // }
            }
            catch
            {
                // Swallow read/parse errors — use appsettings.json instead
            }

            return GetOptions();
        }


        // ------------------------------------- Read all profiles (used by `profiles list`) -------------------------------------


        // Returns an empty dictionary on any error. Keys will be **case-sensitive** by default.
        // If your CLI intends case-insensitive names, wrap the result:
        //   return new Dictionary<string, MT5Options>(loaded, StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, MT5Options> ReadProfiles()
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "profiles.json");

                if (!File.Exists(path)) return new();

                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<Dictionary<string, MT5Options>>(
                           json,
                           new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                       ) ?? new();
            }
            catch
            {
                return new();
            }
        }


        // Resolve effective MT5 options for a given profile.
        // Precedence:
        //   1) profiles.json → LoadProfile(profileName) (may fall back to appsettings.json)
        //   2) MT5_PASSWORD env var overrides the password (avoid storing secrets in files)
        // Returns the combined options; no validation of required fields is performed here.
        // NOTE: Profile name lookup case-sensitivity depends on LoadProfile’s implementation.
        private MT5Options GetEffectiveOptions(string profileName)
        {
            var opt = LoadProfile(profileName);
            var envPass = Environment.GetEnvironmentVariable("MT5_PASSWORD");
            if (!string.IsNullOrEmpty(envPass))
                opt.Password = envPass;
            return opt;
        }


      public static async Task Main(string[] args)
{
    CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
    using var appCts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) => { e.Cancel = true; appCts.Cancel(); };
    AppDomain.CurrentDomain.ProcessExit += (_, __) => { try { appCts.Cancel(); } catch { } };

    // Parse logging switches first
    bool trace   = args.Any(a => string.Equals(a, "--trace",   StringComparison.OrdinalIgnoreCase));
    bool verbose = !trace && args.Any(a => string.Equals(a, "--verbose", StringComparison.OrdinalIgnoreCase));

    // Set global min log level for Program() constructor
    if (trace) Program.MinLogLevel = LogLevel.Trace;
    else if (verbose) Program.MinLogLevel = LogLevel.Debug;
    else Program.MinLogLevel = LogLevel.Information;

    // Local entry logger (optional, just for outer try/catch)
    using var entryLoggerFactory = LoggerFactory.Create(lb =>
    {
        lb.AddSimpleConsole(o =>
        {
            o.IncludeScopes   = true;
            o.SingleLine      = false;
            o.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
        });
        lb.SetMinimumLevel(Program.MinLogLevel);
    });
    var entryLogger = entryLoggerFactory.CreateLogger("EntryPoint");

    // Create Program with parameterless ctor (it will read Program.MinLogLevel)
    var program = new Program();

    // Build CLI and add global logging options
    var root = program.BuildCli();
    root.AddGlobalOption(new Option<bool>("--verbose", "Enable verbose logging (Debug)."));
    root.AddGlobalOption(new Option<bool>("--trace",   "Enable trace logging (very noisy)."));

    if (args.Length == 0)
    {
        await root.InvokeAsync("--help");
        return;
    }

    using (entryLogger.BeginScope("SessionId:{SessionId}", Guid.NewGuid().ToString("N")))
    {
        try
        {
            entryLogger.LogInformation("Starting. Verbose={Verbose} Trace={Trace}", verbose, trace);
            await root.InvokeAsync(args);
            Environment.ExitCode = 0;
        }
        catch (Exception ex)
        {
            entryLogger.LogError("Unhandled error: {Message}", ex.Message);
            if (entryLogger.IsEnabled(LogLevel.Debug))
                entryLogger.LogError(ex, "Unhandled exception details.");
            Environment.ExitCode = 1;
        }
        finally
        {
            entryLogger.LogInformation("Stopped.");
        }
    }
}


        private RootCommand BuildCli()
{
    var root = new RootCommand("MT5 console CLI (info, buy, sell, close, stream, positions, orders, history, health)");
    // Global dry-run: do not send orders, just print what would be done
var dryRunOpt = new Option<bool>(new[] { "--dry-run" }, "Print actions without sending orders");
root.AddGlobalOption(dryRunOpt);


    // ---------- Common options ----------
            var daysOpt = new Option<int>(new[] { "--days", "-d" }, () => 7, "How many days back to fetch history");

    var profileOpt = new Option<string>(new[] { "--profile", "-p" }, () => "default", "The name of the profile from profiles.json");
    var outputOpt  = new Option<string>(new[] { "--output", "-o" }, () => "text", "Output format: text | json");

    var symbolOpt  = new Option<string?>(new[] { "--symbol", "-s" }, () => GetOptions().DefaultSymbol, "Ticker (for example, EURUSD)");
    var volumeOpt  = new Option<double>(new[] { "--volume", "-v" }, () => GetOptions().DefaultVolume, "Volume (lots)");
    var slOpt      = new Option<double?>("--sl", "Stop Loss (price), optional");
    var tpOpt      = new Option<double?>("--tp", "Take Profit (price), optional");
    var devOpt     = new Option<int>("--deviation", () => 10, "Max. slippage (points)");

    // Global per-operation timeout (ms)
    var timeoutOpt = new Option<int>(
        new[] { "--timeout-ms" },
        () => (int)_rpcTimeout.TotalMilliseconds,
        $"Per-RPC timeout in milliseconds (default {(int)_rpcTimeout.TotalMilliseconds}).");
    root.AddGlobalOption(timeoutOpt);


            //=====================================================
            //==------------------- profiles --------------------==
            //=====================================================

            // Group command for profile operations (alias: `pf`).
            var profilesCmd = new Command("profiles", "Working with profiles");
profilesCmd.AddAlias("pf");


            // profiles list
            // Prints profile names from profiles.json (or a friendly message if missing/empty).
            // Supports `--output text|json` via shared outputOpt; JSON uses ToJson(..) helper.
            // NOTE: dict.Keys iteration order is unspecified — sort if you want stable output.
            // NOTE: ReadProfiles() already swallows I/O/parse errors and returns {}.
            var listCmd = new Command("list", "Show profile names from profiles.json");
listCmd.AddAlias("ls");
listCmd.AddOption(outputOpt);
listCmd.SetHandler((string output) =>
{
    var dict = ReadProfiles();

    if (IsJson(output))
    {
        Console.WriteLine(ToJson(dict.Keys.ToArray())); // expect: ["default","demo",...]
    }
    else
    {
        if (dict.Count == 0)
        {
            Console.WriteLine("profiles.json not found or empty.");
            return;
        }

        Console.WriteLine("Profiles:");
        foreach (var name in dict.Keys) // consider: foreach (var name in dict.Keys.OrderBy(x => x))
            Console.WriteLine($"- {name}");
    }
}, outputOpt);

profilesCmd.AddCommand(listCmd);



            //======================================================
            //==------------------ profiles show -----------------==
            //======================================================


            // Prints effective profile settings (profiles.json merged with MT5_PASSWORD env).
            // Alias: `view`. Updates _selectedProfile for later commands.
            // NOTE: Profile name lookup case-sensitivity depends on LoadProfile(); password is masked on text output.
            // TIP: JSON path is ToJson(eff) → includes password value; avoid piping JSON to logs if secrets matter.
            var showCmd = new Command("show", "Show the parameters of the selected profile (taking into account MT5_PASSWORD)");
showCmd.AddAlias("view");
showCmd.AddOption(profileOpt);
showCmd.AddOption(outputOpt);
showCmd.SetHandler((string profile, string output) =>
{
    _selectedProfile = profile; // remember last chosen profile for subsequent ops

    var eff = GetEffectiveOptions(profile); // profiles.json + env override
    if (IsJson(output))
    {
        Console.WriteLine(ToJson(eff)); // sensitive: includes password in JSON
    }
    else
    {
        var envSet = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MT5_PASSWORD"));
        Console.WriteLine($"Profile: {profile}");
        Console.WriteLine($"  AccountId:     {eff.AccountId}");
        Console.WriteLine($"  ServerName:    {eff.ServerName}");
        Console.WriteLine($"  Host:          {eff.Host}");
        Console.WriteLine($"  Port:          {eff.Port}");
        Console.WriteLine($"  DefaultSymbol: {eff.DefaultSymbol}");
        Console.WriteLine($"  DefaultVolume: {eff.DefaultVolume}");
        Console.WriteLine($"  Password:      {(string.IsNullOrEmpty(eff.Password) ? "<empty>" : "*** (hidden)")}");
        Console.WriteLine($"  Password source: {(envSet ? "env MT5_PASSWORD" : "profiles/appsettings")}");
    }
}, profileOpt, outputOpt);

profilesCmd.AddCommand(showCmd);

            root.AddCommand(profilesCmd);


            //======================================================
            //==---------------------- info ----------------------==
            //======================================================


            // info command — prints account summary (JSON or text).
            // Flow:
            //  1) Validate/select profile (Validators.EnsureProfile) and store _selectedProfile.
            //  2) UseOpTimeout(timeoutMs) → per-RPC timeout override; BeginScope tags logs with cmd/profile.
            //  3) ConnectAsync(); then AccountSummaryAsync via CallWithRetry + per-op CTS (StartOpCts()).
            //  4) Output: ToJson(summary) when --output=json; otherwise logs a brief text (Balance shown).
            //  5) Errors: ErrorPrinter.Print + set Environment.ExitCode=1; always best-effort DisconnectAsync in finally.
            //
            // Notes:
            //  - timeoutMs applies to each RPC (per-op), not the whole handler duration.
            //  - Consider adding more fields to text output (Equity, Margin, FreeMargin, Currency) if needed.
            //  - Any RpcException with transient code is retried by CallWithRetry; others bubble to catch.
var info = new Command("info", "Show account summary");

// Alias + options
info.AddAlias("i");
info.AddOption(profileOpt);
info.AddOption(outputOpt);
info.AddOption(timeoutOpt);

// Single handler, single print point
info.SetHandler(async (string profile, string output, int timeoutMs) =>
{
    // 1) Validate + select profile
    Validators.EnsureProfile(profile);
    _selectedProfile = profile;

    // 2) Bound the whole operation with a timeout + logging scopes
    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:INFO Profile:{Profile}", profile))
    {
        try
        {
            // 3) Connect once
            await ConnectAsync();

            // 4) Summary with retry (no printing inside the retry!)
            using var opCts = StartOpCts();
            var summary = await CallWithRetry(
                ct => _mt5Account.AccountSummaryAsync(deadline: null, cancellationToken: ct),
                opCts.Token);

            // 5) Extra metrics via AccountInformation
            var ct = opCts.Token;
            double margin = 0, freeMargin = 0;

            try
            {
                var marginTask     = _mt5Account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountMargin,     null, ct);
                var freeMarginTask = _mt5Account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountMarginFree, null, ct);
                await Task.WhenAll(marginTask, freeMarginTask);
                margin     = await marginTask;
                freeMargin = await freeMarginTask;
            }
            catch
            {
                // keep zeros; we will apply a fallback below
            }

            // Fallback: if server returns zero FreeMargin, compute from Equity - Margin
            var freeMarginFixed = (freeMargin <= 0 && summary.AccountEquity > 0)
                ? summary.AccountEquity - margin
                : freeMargin;

            // 6) Single printing point
            if (IsJson(output))
            {
                var payload = new
                {
                    summary.AccountLogin,
                    summary.AccountBalance,
                    summary.AccountEquity,
                    summary.AccountUserName,
                    summary.AccountLeverage,
                    summary.AccountTradeMode,
                    summary.AccountCompanyName,
                    summary.AccountCurrency,
                    summary.ServerTime,
                    summary.UtcTimezoneServerTimeShiftMinutes,
                    summary.AccountCredit,
                    // extras:
                    Margin     = margin,
                    FreeMargin = freeMarginFixed
                };

                Console.WriteLine(JsonSerializer.Serialize(payload));
            }
            else
            {
                _logger.LogInformation("=== Account Info ===");

                void Print<T>(string label, T v) =>
                    _logger.LogInformation("{Label}: {Value}", label, v);

                // summary fields
                Print("Login",       summary.AccountLogin);
                Print("UserName",    summary.AccountUserName);
                Print("Currency",    summary.AccountCurrency);
                Print("Balance",     summary.AccountBalance);
                Print("Equity",      summary.AccountEquity);
                Print("Leverage",    summary.AccountLeverage);
                Print("TradeMode",   summary.AccountTradeMode);
                Print("Company",     summary.AccountCompanyName);

                // extras (with fallback applied)
                Print("Margin",      margin);
                Print("FreeMargin",  freeMarginFixed);

                // optional: server time
                try
                {
                    var dt = summary.ServerTime?.ToDateTime();
                    if (dt != null) Print("ServerTime (UTC)", dt.Value.ToUniversalTime().ToString("u"));
                    Print("UTC Shift (min)", summary.UtcTimezoneServerTimeShiftMinutes);
                }
                catch { /* ignore */ }
            }
        }
        catch (Exception ex)
        {
            // 7) Unified error path
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            // 8) Always disconnect
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
},
// Bind options to handler parameters (order matters)
profileOpt, outputOpt, timeoutOpt);

root.AddCommand(info);


            //=====================================================
            //==-------------------- quote ----------------------==
            //=====================================================


            // quote command — single-symbol snapshot (Bid/Ask/Time).
            // Flow: validate profile/symbol → scope logs → ConnectAsync → EnsureSymbolVisibleAsync (best-effort)
            // → FirstTickAsync (returns first streamed tick) with per-op timeout/retry → print as text/JSON.
            // Notes:
            // - Default symbol fallback uses GetOptions().DefaultSymbol (from appsettings), NOT the selected profile.
            //   If you want profile-aware defaults, use GetEffectiveOptions(profile).DefaultSymbol.
            // - FirstTickAsync depends on stream delivering at least one tick; StartOpCts() timeout prevents hanging.
            // - Visibility check failure is non-fatal by design; you still attempt to read a tick.
            // - Always disconnects in finally; errors go through ErrorPrinter and set ExitCode=1.
var quoteCmd = new Command("quote", "Get a snapshot price (Bid/Ask/Time)");
quoteCmd.AddAlias("q");

quoteCmd.AddOption(profileOpt);
quoteCmd.AddOption(symbolOpt);
quoteCmd.AddOption(outputOpt);
quoteCmd.AddOption(timeoutOpt);

quoteCmd.SetHandler(async (string profile, string? symbol, string output, int timeoutMs) =>
{
    // Validate + select profile
    Validators.EnsureProfile(profile);
    var s = Validators.EnsureSymbol(symbol ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:QUOTE Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol}", s))
    {
        try
        {
            await ConnectAsync();

            // Best-effort: ensure symbol visible
            try
            {
                using var visCts = StartOpCts();
                await _mt5Account.EnsureSymbolVisibleAsync(
                    s, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
            }

            // Fetch first tick with retry
            using var opCts = StartOpCts();
            var snap = await CallWithRetry(ct => FirstTickAsync(s, ct), opCts.Token);

// Derivatives & staleness (handle nullable TimeUtc)
var mid     = (snap.Bid + snap.Ask) / 2.0;
var spread  = snap.Ask - snap.Bid;
double ageMs = snap.TimeUtc.HasValue
    ? Math.Abs((DateTime.UtcNow - snap.TimeUtc.Value).TotalMilliseconds)
    : double.NaN;
var timeStr = snap.TimeUtc.HasValue ? snap.TimeUtc.Value.ToString("O") : "null";

if (IsJson(output))
{
    // Single print point (JSON)
    var payload = new
    {
        snap.Symbol,
        snap.Bid,
        snap.Ask,
        snap.TimeUtc,    // nullable DateTime?
        Mid    = mid,
        Spread = spread,
        AgeMs  = ageMs
    };
    Console.WriteLine(JsonSerializer.Serialize(payload));
}
else
{
    // Single print point (text)
    var stale = (!double.IsNaN(ageMs) && ageMs > 5000) ? "  [STALE >5s]" : "";
    var ageStr = double.IsNaN(ageMs) ? "NA" : ((int)ageMs).ToString();
    Console.WriteLine($"{snap.Symbol}  bid={snap.Bid}  ask={snap.Ask}  mid={mid}  spr={spread}  ageMs={ageStr}  time={timeStr}{stale}");
}

        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
}, profileOpt, symbolOpt, outputOpt, timeoutOpt);

root.AddCommand(quoteCmd);



            //=====================================================
            //==--------------------- buy -----------------------==
            //=====================================================


            // buy command — market BUY with optional SL/TP and slippage.
            // Flow:
            //  1) Validate inputs (profile/volume/deviation); resolve symbol (falls back to GetOptions().DefaultSymbol).
            //  2) Scopes for structured logs (profile, symbol, params).
            //  3) If --dry-run: print intended action and exit.
            //  4) Connect → EnsureSymbolVisibleAsync (best-effort) → SendMarketOrderAsync with per-op CTS via StartOpCts()
            //     wrapped in CallWithRetry (transient gRPC errors only).
            //  5) Log ticket, then best-effort disconnect in finally.
            //
            // Notes/Pitfalls:
            //  - Default symbol is taken from appsettings via GetOptions(), NOT from the selected profile;
            //    use GetEffectiveOptions(profile).DefaultSymbol if you want profile-aware defaults.
            //  - Per-op timeout uses StartOpCts(); the handler’s timeoutMs is set via UseOpTimeout and
            //    affects all StartOpCts() within the scope.
            //  - SL/TP are absolute prices; server will enforce Stops/Freeze levels and distances.
            //  - Visibility check failures are logged but do not stop the order (idempotent on server).
            //  - Errors go through ErrorPrinter and set ExitCode=1.

        var buy = new Command("buy", "Market buy");

            buy.AddAlias("b");
            buy.AddOption(profileOpt);
            buy.AddOption(symbolOpt);
            buy.AddOption(volumeOpt);
            buy.AddOption(slOpt);
            buy.AddOption(tpOpt);
            buy.AddOption(devOpt);
            buy.AddOption(outputOpt);
            buy.AddOption(timeoutOpt);
            buy.AddOption(dryRunOpt);


buy.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var symbolV   = ctx.ParseResult.GetValueForOption(symbolOpt);
    var volume    = ctx.ParseResult.GetValueForOption(volumeOpt);
    var sl        = ctx.ParseResult.GetValueForOption(slOpt);
    var tp        = ctx.ParseResult.GetValueForOption(tpOpt);
    var deviation = ctx.ParseResult.GetValueForOption(devOpt);
    var output    = ctx.ParseResult.GetValueForOption(outputOpt) ?? "text";
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureVolume(volume);
    Validators.EnsureDeviation(deviation);

    var s = Validators.EnsureSymbol(symbolV ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:BUY Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol}", s))
    using (_logger.BeginScope("OrderParams Vol:{Vol} Dev:{Dev} SL:{SL} TP:{TP}", volume, deviation, sl, tp))
    {
        if (dryRun)
        {
            if (string.Equals(output, "json", StringComparison.OrdinalIgnoreCase))
            {
                var payload = new { DryRun = true, Side = "BUY", Symbol = s, Volume = volume, Deviation = deviation, SL = sl, TP = tp };
                Console.WriteLine(JsonSerializer.Serialize(payload));
            }
            else
            {
                Console.WriteLine($"[DRY-RUN] BUY {s} vol={volume} dev={deviation} SL={sl} TP={tp}");
            }
            return;
        }

        try
        {
            await ConnectAsync();

            // Ensure symbol visible (best-effort)
            try
            {
                using var visCts = StartOpCts();
                await _mt5Account.EnsureSymbolVisibleAsync(s, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
            }

            using var opCts = StartOpCts();
            // --- preflight for BUY ---
            var q = await CallWithRetry(ct => FirstTickAsync(s, ct), opCts.Token);
            var bid = q.Bid; var ask = q.Ask;

            int? digits = null;             // TODO: fetch via MarketInfo if 
            double? stopLevelPoints = null; 
            double? point = null;           

            PreflightStops(isBuy: true, bid: bid, ask: ask, sl: ref sl, tp: ref tp,
                           digits: digits, stopLevelPoints: stopLevelPoints, point: point);

            // Send order with retry
            var ticket = await CallWithRetry(
                ct => _mt5Account.SendMarketOrderAsync(
                    symbol: s, isBuy: true, volume: volume, deviation: deviation,
                    stopLoss: sl, takeProfit: tp, deadline: null, cancellationToken: ct),
                opCts.Token);

            if (string.Equals(output, "json", StringComparison.OrdinalIgnoreCase))
            {
                var payload = new { Side = "BUY", Symbol = s, Volume = volume, Deviation = deviation, SL = sl, TP = tp, Ticket = ticket };
                Console.WriteLine(JsonSerializer.Serialize(payload));
            }
            else
            {
                _logger.LogInformation("BUY done: ticket={Ticket}", ticket);
            }
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
});

root.AddCommand(buy);


            //======================================
            //===--------- trail.start ----------===
            //======================================


            // trail.start — launch local trailing for a position.
            // ✅ Validates ticket/distance/step; parses --mode case-insensitively.
            //
            // ⚠ ENUM TYPE: `Enum.TryParse<MT5Account.TrailMode>` will only compile if `TrailMode` is
            //    nested inside MT5Account. If it’s a top-level enum, change to `TryParse<TrailMode>`.
            var trTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket") { IsRequired = true };
var trDistOpt   = new Option<int>(new[] { "--distance" }, () => 150, "Distance in POINTS from price to SL");
var trStepOpt   = new Option<int>(new[] { "--step" }, () => 20, "Minimal move in POINTS to update SL");
var trModeOpt   = new Option<string>(new[] { "--mode" }, () => "classic", "classic|chandelier");

var trailStart = new Command("trail.start", "Start local trailing stop for a position");
trailStart.AddOption(profileOpt);
trailStart.AddOption(trTicketOpt);
trailStart.AddOption(trDistOpt);
trailStart.AddOption(trStepOpt);
trailStart.AddOption(trModeOpt);
trailStart.AddOption(timeoutOpt);
trailStart.AddOption(dryRunOpt);

            trailStart.SetHandler(async (InvocationContext ctx) =>
{
    var profile  = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket   = ctx.ParseResult.GetValueForOption(trTicketOpt);
    var distance = ctx.ParseResult.GetValueForOption(trDistOpt);
    var step     = ctx.ParseResult.GetValueForOption(trStepOpt);
    var modestr  = ctx.ParseResult.GetValueForOption(trModeOpt);
    var timeoutMs= ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun   = ctx.ParseResult.GetValueForOption(dryRunOpt);

Validators.EnsureProfile(profile);
Validators.EnsureTicket(ticket);
if (distance <= 0) throw new ArgumentOutOfRangeException(nameof(distance));
if (step <= 0)     throw new ArgumentOutOfRangeException(nameof(step));

var modeText = (modestr ?? "classic").Trim();
if (!System.Enum.TryParse<MT5Account.TrailMode>(modeText, ignoreCase: true, out var mode))
    throw new ArgumentException("Invalid --mode. Use classic|chandelier.");

using (UseOpTimeout(timeoutMs))
using (_logger.BeginScope("Cmd:TRAIL.START Profile:{Profile}", profile))
using (_logger.BeginScope("Ticket:{Ticket} Dist:{Dist} Step:{Step} Mode:{Mode}", ticket, distance, step, mode))
{
    try
    {
        await ConnectAsync();

        using var opCts = StartOpCts();

            var opened = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                opCts.Token);

            var pos = opened.PositionInfos.FirstOrDefault(p => Convert.ToUInt64(p.Ticket) == ticket);
            if (pos is null)
            {
                Console.WriteLine($"Position #{ticket} not found.");
                Environment.ExitCode = 2;
                return;
            }

            var symbol = pos.Symbol;
            var isLong = IsLongPosition(pos);

            if (dryRun)
            {
                Console.WriteLine($"[DRY-RUN] TRAIL.START #{ticket} {symbol} mode={mode} dist={distance} step={step}");
                return;
            }

            await _mt5Account.StartTrailingAsync(ticket, symbol, isLong, distance, step, mode, opCts.Token);
            Console.WriteLine("✔ trail.start scheduled");
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
});
root.AddCommand(trailStart);


            //=======================================
            //===---------- close.percent --------===
            //=======================================


            // close.percent — partially close a position by ticket.
            // Core: validates pct∈(0;100], finds position, computes toClose, rounds to 0.01 lot, then calls ClosePositionPartialAsync.
            // Honors --dry-run; per-op timeout via UseOpTimeout/StartOpCts; retries on transient gRPC errors.
            // Exit codes: 0 OK, 1 error, 2 not found / nothing to close.
            //
            // Usage examples:
            //   mt5cli close.percent -p default -t 123456 --pct 25
            //   mt5cli close.percent -t 123456 --pct 50 --deviation 15
            //   mt5cli close.percent -t 123456 --pct 10 --timeout-ms 8000 --dry-run
            var cpTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket") { IsRequired = true };
var cpPctOpt    = new Option<double>(new[] { "--pct" }, () => 50.0, "Percent to close (0 < pct ≤ 100)");
var cpDevOpt    = devOpt;

var closePercent = new Command("close.percent", "Close a percentage of a position by ticket");
closePercent.AddAlias("cpp");

closePercent.AddOption(profileOpt);
closePercent.AddOption(cpTicketOpt);
closePercent.AddOption(cpPctOpt);
closePercent.AddOption(cpDevOpt);
closePercent.AddOption(timeoutOpt);
closePercent.AddOption(dryRunOpt);

            closePercent.SetHandler(async (InvocationContext ctx) =>
            {
                var profile = ctx.ParseResult.GetValueForOption(profileOpt)!;
                var ticket = ctx.ParseResult.GetValueForOption(cpTicketOpt);
                var pct = ctx.ParseResult.GetValueForOption(cpPctOpt);
                var deviation = ctx.ParseResult.GetValueForOption(cpDevOpt);
                var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
                var dryRun = ctx.ParseResult.GetValueForOption(dryRunOpt);

                Validators.EnsureProfile(profile);
                Validators.EnsureTicket(ticket);
                if (pct <= 0 || pct > 100) throw new ArgumentOutOfRangeException(nameof(pct), "Percent must be in (0;100].");

                using (UseOpTimeout(timeoutMs))
                using (_logger.BeginScope("Cmd:CLOSE.PERCENT Profile:{Profile}", profile))
                using (_logger.BeginScope("Ticket:{Ticket} Pct:{Pct} Dev:{Dev}", ticket, pct, deviation))
                {
                    try
                    {
                        await ConnectAsync();
                        using var opCts = StartOpCts();

                        var opened = await CallWithRetry(
                            ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                            opCts.Token);

                        var pos = opened.PositionInfos.FirstOrDefault(p => Convert.ToUInt64(p.Ticket) == ticket);
                        if (pos is null)
                        {
                            Console.WriteLine($"Position #{ticket} not found.");
                            Environment.ExitCode = 2;
                            return;
                        }

                        var symbol = pos.Symbol;
                        var posVol = pos.Volume;                  // double
                        var toClose = posVol * (pct / 100.0);

                        toClose = Math.Round(toClose, 2, MidpointRounding.AwayFromZero);
                        if (toClose <= 0) { Console.WriteLine("Nothing to close after rounding."); Environment.ExitCode = 2; return; }
                        if (toClose > posVol) toClose = posVol;

                        if (dryRun)
                        {
                            Console.WriteLine($"[DRY-RUN] CLOSE.PERCENT #{ticket} {symbol}: close {toClose} of {posVol} (dev={deviation})");
                            return;
                        }

                        await CallWithRetry(
                            ct => _mt5Account.ClosePositionPartialAsync(ticket, toClose, deviation, ct),
                            opCts.Token);

                        Console.WriteLine($"✔ close.percent done: #{ticket} vol={toClose}");
                    }
                    catch (Exception ex)
                    {
                        ErrorPrinter.Print(_logger, ex, IsDetailed());
                        Environment.ExitCode = 1;
                    }
                    finally
                    {
                        try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
                    }
                }
            });


            //===================================
            //==---------- close.half ---------==
            //===================================


            // close.half — alias for partial close at 50%
            // Calls `close.percent --pct 50` under the hood; mirrors options: --profile, --ticket, --deviation, --timeout-ms.
            // Prints a short alias note, then forwards args; exit code comes from the delegated command.
            // TIP: Forward --dry-run if set (uncomment code below).
            // Usage:
            //   mt5cli close.half -p default -t 123456
            //   mt5cli close.half -t 123456 --deviation 15 --timeout-ms 8000
            var chTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket") { IsRequired = true };

var closeHalf = new Command("close.half", "Close half of a position by ticket");
closeHalf.AddAlias("ch");

closeHalf.AddOption(profileOpt);
closeHalf.AddOption(chTicketOpt);
closeHalf.AddOption(cpDevOpt);
closeHalf.AddOption(timeoutOpt);
closeHalf.AddOption(dryRunOpt);

closeHalf.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(chTicketOpt);
    var deviation = ctx.ParseResult.GetValueForOption(cpDevOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    ctx.Console.WriteLine("(alias) close.half -> close.percent --pct 50");

    await closePercent.InvokeAsync(new[]
    {
        "--profile",    profile,
        "--ticket",     ticket.ToString(System.Globalization.CultureInfo.InvariantCulture),
        "--pct",        "50",
        "--deviation",  deviation.ToString(System.Globalization.CultureInfo.InvariantCulture),
        "--timeout-ms", timeoutMs.ToString(System.Globalization.CultureInfo.InvariantCulture),
        // if desired, we throw dry-run:
        // dryRun ? "--dry-run" : null
    }
        // if you add conditional elements, finish .Where(s => s != null)!.ToArray()
    );
});

root.AddCommand(closePercent);
root.AddCommand(closeHalf);


            //====================================
            //==---------- trail.stop ----------==
            //====================================


            // trail.stop — stop local trailing for a position.
            // Validates profile/ticket, calls _mt5Account.StopTrailing(ticket).
            // Idempotent: safe to call if no worker is running.
            // Usage:
            //   mt5cli trail.stop -p default -t 123456
            //   mt5cli trail.stop --ticket 123456
            var trailStop = new Command("trail.stop", "Stop local trailing stop");
trailStop.AddOption(profileOpt);
trailStop.AddOption(trTicketOpt);

trailStop.SetHandler((string profile, ulong ticket) =>
{
    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    _mt5Account.StopTrailing(ticket);
    Console.WriteLine($"✔ trailing stopped for #{ticket}");
}, profileOpt, trTicketOpt);

root.AddCommand(trailStop);


            //====================================
            //==------------ sell --------------==
            //====================================


            // sell command — market SELL with optional SL/TP and slippage.
            // Flow: validate → resolve symbol (defaults from appsettings, not profile) → optional DRY-RUN → Connect
            // → best-effort EnsureSymbolVisible → SendMarketOrderAsync(isBuy:false) with per-op timeout + retry → log ticket → Disconnect.
            // Notes:
            // - SL/TP are absolute prices; broker enforces Stops/Freeze distances.
            // - UseOpTimeout + StartOpCts control per-RPC timeout; CallWithRetry handles transient gRPC errors.
            // - If you want profile-aware defaults, use GetEffectiveOptions(profile).DefaultSymbol.
            var sell = new Command("sell", "Market sell");

            sell.AddAlias("s");

            sell.AddOption(profileOpt);
            sell.AddOption(symbolOpt);
            sell.AddOption(volumeOpt);
            sell.AddOption(slOpt);
            sell.AddOption(tpOpt);
            sell.AddOption(devOpt);
            sell.AddOption(outputOpt);
            sell.AddOption(timeoutOpt);
            sell.AddOption(dryRunOpt);


sell.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var symbolV   = ctx.ParseResult.GetValueForOption(symbolOpt);
    var volume    = ctx.ParseResult.GetValueForOption(volumeOpt);
    var sl        = ctx.ParseResult.GetValueForOption(slOpt);
    var tp        = ctx.ParseResult.GetValueForOption(tpOpt);
    var deviation = ctx.ParseResult.GetValueForOption(devOpt);
    var output    = ctx.ParseResult.GetValueForOption(outputOpt) ?? "text";
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureVolume(volume);
    Validators.EnsureDeviation(deviation);

    var s = Validators.EnsureSymbol(symbolV ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:SELL Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol}", s))
    using (_logger.BeginScope("OrderParams Vol:{Vol} Dev:{Dev} SL:{SL} TP:{TP}", volume, deviation, sl, tp))
    {
        if (dryRun)
        {
            if (string.Equals(output, "json", StringComparison.OrdinalIgnoreCase))
            {
                var payload = new { DryRun = true, Side = "SELL", Symbol = s, Volume = volume, Deviation = deviation, SL = sl, TP = tp };
                Console.WriteLine(JsonSerializer.Serialize(payload));
            }
            else
            {
                Console.WriteLine($"[DRY-RUN] SELL {s} vol={volume} dev={deviation} SL={sl} TP={tp}");
            }
            return;
        }

        try
        {
            await ConnectAsync();

            // Ensure symbol visible (best-effort)
            try
            {
                using var visCts = StartOpCts();
                await _mt5Account.EnsureSymbolVisibleAsync(s, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
            }

            using var opCts = StartOpCts();
            // --- preflight for SELL ---
            var q = await CallWithRetry(ct => FirstTickAsync(s, ct), opCts.Token);
            var bid = q.Bid; var ask = q.Ask;

            int? digits = null;             
            double? stopLevelPoints = null; 
            double? point = null;           // TODO: fetch via MarketInfo if 

            PreflightStops(isBuy: false, bid: bid, ask: ask, sl: ref sl, tp: ref tp,
                           digits: digits, stopLevelPoints: stopLevelPoints, point: point);

            // Send order with retry
            var ticket = await CallWithRetry(
                ct => _mt5Account.SendMarketOrderAsync(
                    symbol: s, isBuy: false, volume: volume, deviation: deviation,
                    stopLoss: sl, takeProfit: tp, deadline: null, cancellationToken: ct),
                opCts.Token);

            if (string.Equals(output, "json", StringComparison.OrdinalIgnoreCase))
            {
                var payload = new { Side = "SELL", Symbol = s, Volume = volume, Deviation = deviation, SL = sl, TP = tp, Ticket = ticket };
                Console.WriteLine(JsonSerializer.Serialize(payload));
            }
            else
            {
                _logger.LogInformation("SELL done: ticket={Ticket}", ticket);
            }
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
});

root.AddCommand(sell);


            //====================================
            //==----------- close --------------==
            //====================================


            // close — close by ticket with client-side volume normalization.
            // Flow: validate → resolve symbol (defaults from appsettings, not profile) → optional DRY-RUN → Connect
            // → EnsureSymbolVisible (best-effort) → CloseOrderByTicketAsync(ticket,symbol,volume) with per-op timeout + retry → Disconnect.
            // Notes:
            // - Volume is normalized inside MT5Account.CloseOrderByTicketAsync (min/step/max per symbol).
            // - SL/Freeze distances enforced server-side when closing at market.
            // - For profile-aware defaults, use GetEffectiveOptions(profile).DefaultSymbol.
            // Example: mt5cli close -p default -t 123456 -s EURUSD -v 0.2
            var ticketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Ticket to close") { IsRequired = true };
    var close = new Command("close", "Close by ticket (volume normalized by symbol rules)");
    close.AddAlias("c");

    close.AddOption(profileOpt);
    close.AddOption(ticketOpt);
    close.AddOption(symbolOpt);
    close.AddOption(volumeOpt);
            close.SetHandler(async (string profile, ulong ticket, string? symbol, double volume, int timeoutMs, bool dryRun) =>
        {
            Validators.EnsureProfile(profile);
            Validators.EnsureTicket(ticket);
            Validators.EnsureVolume(volume);

            var s = Validators.EnsureSymbol(symbol ?? GetOptions().DefaultSymbol);
            _selectedProfile = profile;

            using (UseOpTimeout(timeoutMs))
            using (_logger.BeginScope("Cmd:CLOSE Profile:{Profile}", profile))
            using (_logger.BeginScope("Ticket:{Ticket}", ticket))
            using (_logger.BeginScope("Symbol:{Symbol} Vol:{Vol}", s, volume))
            {
                if (dryRun)
                {
                    Console.WriteLine($"[DRY-RUN] CLOSE #{ticket} {s} vol={volume}");
                    return;
                }

                try
                {
                    await ConnectAsync();

                    try
                    {
                        using var visCts = StartOpCts();
                        await _mt5Account.EnsureSymbolVisibleAsync(
                            s, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
                    }

                    using var opCts = StartOpCts();
                    await CallWithRetry(
                        ct => _mt5Account.CloseOrderByTicketAsync(
                            ticket: ticket, symbol: s, volume: volume, deadline: null, cancellationToken: ct),
                        opCts.Token);

                    _logger.LogInformation("CLOSE done: ticket={Ticket}", ticket);
                }
                catch (Exception ex)
                {
                    ErrorPrinter.Print(_logger, ex, IsDetailed());
                    Environment.ExitCode = 1;
                }
                finally
                {
                    try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
                }
            }
        }, profileOpt, ticketOpt, symbolOpt, volumeOpt, timeoutOpt, dryRunOpt);

        root.AddCommand(close);


            //==============================
            //==--------- place ----------==
            //==============================


            // place — create a pending order (limit/stop/stop-limit) with optional SL/TP and TIF.
            // Validates args: limit/stop require --price; stop-limit require both --stop & --limit
            // (buy: limit <= stop; sell: limit >= stop); GTD requires --expire.
            // Ensures symbol visible, then calls PlacePendingOrderAsync or PlaceStopLimitOrderAsync.
            // Supports --dry-run; per-op timeout via UseOpTimeout/StartOpCts; retries transient gRPC errors.
            // NOTE: default symbol comes from appsettings (GetOptions), not the selected profile by default.
            //
            // Examples:
            //   mt5cli place --type buylimit     -s EURUSD --price 1.0830 -v 0.10 --sl 1.0810 --tp 1.0860 --tif GTC
            //   mt5cli place --type sellstop     -s EURUSD --price 1.0800 -v 0.20 --tif DAY
            //   mt5cli place --type buystoplimit -s XAUUSD --stop 2410.0 --limit 2409.5 -v 0.05 --tif GTD --expire 2025-09-01T12:00:00Z
            var placeTypeOpt  = new Option<string>(new[] { "--type" }, "buylimit|selllimit|buystop|sellstop|buystoplimit|sellstoplimit")
{
    IsRequired = true
};

var placePriceOpt = new Option<double?>(new[] { "--price" }, "Entry price for limit/stop");

var placeStopOpt   = new Option<double?>(new[] { "--stop"  }, "Trigger price for stop/stop-limit");
var placeLimitOpt  = new Option<double?>(new[] { "--limit" }, "Limit price for stop-limit");
var placeTifOpt    = new Option<string?>(new[] { "--tif"   }, "Time-in-force: GTC|DAY|GTD");
var placeExpireOpt = new Option<DateTimeOffset?>(new[] { "--expire" }, "Expiry (ISO-8601) when --tif=GTD");

var place = new Command("place", "Place a pending order");
place.AddAlias("pl");

place.AddOption(profileOpt);
place.AddOption(symbolOpt);
place.AddOption(volumeOpt);
place.AddOption(placeTypeOpt);
place.AddOption(placePriceOpt);
place.AddOption(placeStopOpt);
place.AddOption(placeLimitOpt);
place.AddOption(placeTifOpt);
place.AddOption(placeExpireOpt);
place.AddOption(slOpt);
place.AddOption(tpOpt);

place.SetHandler(async (InvocationContext ctx) =>
{
    var profile    = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var symbolArg  = ctx.ParseResult.GetValueForOption(symbolOpt);
    var volume     = ctx.ParseResult.GetValueForOption(volumeOpt);
    var typeStr    = ctx.ParseResult.GetValueForOption(placeTypeOpt)!;   // buylimit|...|sellstoplimit
    var priceOptV  = ctx.ParseResult.GetValueForOption(placePriceOpt);
    var stopV      = ctx.ParseResult.GetValueForOption(placeStopOpt);
    var limitV     = ctx.ParseResult.GetValueForOption(placeLimitOpt);
    var tifStr     = ctx.ParseResult.GetValueForOption(placeTifOpt);
    var expireV    = ctx.ParseResult.GetValueForOption(placeExpireOpt);
    var sl         = ctx.ParseResult.GetValueForOption(slOpt);
    var tp         = ctx.ParseResult.GetValueForOption(tpOpt);
    var timeoutMs  = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun     = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureVolume(volume);

    var s = Validators.EnsureSymbol(symbolArg ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;

    var orderKind = ParseOrderType(typeStr);           // TMT5_ENUM_ORDER_TYPE...
    var tifKind   = ParseTif(tifStr);                  // TMT5_ENUM_ORDER_TYPE_TIME...

    bool isStopLimit = orderKind is
        TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyStopLimit or
        TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellStopLimit;

    bool isLimitOrStop = orderKind is
        TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyLimit or
        TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellLimit or
        TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyStop  or
        TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellStop;


    // -------------------------- Validation of arguments --------------------------

    if (isStopLimit)
    {
        if (!stopV.HasValue || !limitV.HasValue)
            throw new ArgumentException("Stop-limit requires both --stop and --limit.");

        if (orderKind == TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyStopLimit && !(limitV.Value <= stopV.Value))
            throw new ArgumentException("Buy Stop Limit requires --limit <= --stop.");
        if (orderKind == TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellStopLimit && !(limitV.Value >= stopV.Value))
            throw new ArgumentException("Sell Stop Limit requires --limit >= --stop.");

        if (priceOptV.HasValue)
            throw new ArgumentException("Do not pass --price for stop-limit. Use --stop and --limit.");
    }
    else if (isLimitOrStop)
    {
        if (!priceOptV.HasValue || priceOptV.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(placePriceOpt), "For limit/stop orders --price must be > 0.");
    }
    else
    {
        throw new NotSupportedException("Market types are not supported by this 'place' command. Use your market buy/sell commands.");
    }

    if (tifKind == TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecified && !expireV.HasValue)
        throw new ArgumentException("When --tif=GTD is used, --expire must be provided.");

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:PLACE Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol}", s))
    using (_logger.BeginScope("Params Type:{Type} Price:{Price} Stop:{Stop} Limit:{Limit} Vol:{Vol} SL:{SL} TP:{TP} TIF:{TIF} Exp:{Exp}",
                              typeStr, priceOptV, stopV, limitV, volume, sl, tp, tifStr, expireV))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] PLACE {typeStr} {s} price={priceOptV} stop={stopV} limit={limitV} vol={volume} SL={sl} TP={tp} TIF={tifStr} expire={expireV}");
            return;
        }

        try
        {
            await ConnectAsync();

            // best-effort: ensure symbol visible
            try
            {
                using var visCts = StartOpCts();
                await _mt5Account.EnsureSymbolVisibleAsync(
                    s, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
            }

            using var opCts = StartOpCts();

ulong ticket;

            if (isStopLimit)
{
    ticket = await CallWithRetry(
        ct => _mt5Account.PlaceStopLimitOrderAsync(
            symbol: s,
            type: typeStr,               // "buystoplimit" | "sellstoplimit"
            volume: volume,
            stop: stopV!.Value,
            limit: limitV!.Value,
            sl: sl,
            tp: tp,
            tif: tifStr,                 // "GTC" | "DAY" | "GTD" | null
            expire: expireV,
            ct: ct),
        opCts.Token);
}
else
{
    ticket = await CallWithRetry(
        ct => _mt5Account.PlacePendingOrderAsync(
            symbol: s,
            type: typeStr,             // "buylimit"|"selllimit"|"buystop"|"sellstop"
            volume: volume,
            price: priceOptV!.Value,   // entry price
            sl: sl,
            tp: tp,
            tif: tifStr,               // "GTC"|"DAY"|"GTD"|null
            expire: expireV,
            ct: ct),
        opCts.Token);
}


            _logger.LogInformation("PLACE done: ticket={Ticket}", ticket);
        }
        catch (NotSupportedException nse)
        {
            Console.WriteLine($"Pending order API note: {nse.Message}");
            Environment.ExitCode = 2;
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
});

root.AddCommand(place);


            //================================================
            //==--------- symbol (info & utilities) --------==
            //================================================


            // symbol ensure — make symbol visible in the terminal (idempotent).
            // Uses per-op timeout + scoped logging; outputs JSON/text (with error details) and sets ExitCode on failure.
            // Default symbol falls back to appsettings (GetOptions), not the selected profile.
            // Visibility failure is non-fatal to the process (we still disconnect cleanly).
            var symbol = new Command("symbol", "Symbol utilities (ensure-visible, limits, show)");
symbol.AddAlias("sym");

// Reuse common options
symbol.AddOption(profileOpt);
symbol.AddOption(outputOpt);
symbol.AddOption(symbolOpt);


            // ------------------- symbol ensure -------------------

            var symEnsure = new Command("ensure", "Ensure the symbol is visible in terminal");
symEnsure.AddAlias("en");
            symEnsure.SetHandler(async (string profile, string output, string? s, int timeoutMs) =>
            {
                Validators.EnsureProfile(profile);
                var symbolName = Validators.EnsureSymbol(s ?? GetOptions().DefaultSymbol);
                _selectedProfile = profile;

                using (UseOpTimeout(timeoutMs))
                using (_logger.BeginScope("Cmd:SYMBOL-ENSURE Profile:{Profile}", profile))
                using (_logger.BeginScope("Symbol:{Symbol}", symbolName))
                {
                    var report = new Dictionary<string, object?>();
                    try
                    {
                        await ConnectAsync();

                        try
                        {
                            using var visCts = StartOpCts();
                            await _mt5Account.EnsureSymbolVisibleAsync(
                                symbolName,
                                maxWait: TimeSpan.FromSeconds(3),
                                cancellationToken: visCts.Token);

                            report["symbol"] = symbolName;
                            report["visible"] = true;
                            if (IsJson(output)) Console.WriteLine(ToJson(report));
                            else Console.WriteLine($"Symbol '{symbolName}' is visible.");
                        }
                        catch (Exception ex)
                        {
                            report["symbol"] = symbolName;
                            report["visible"] = false;
                            report["error"] = ex.Message;
                            if (IsJson(output)) Console.WriteLine(ToJson(report));
                            else Console.WriteLine($"Failed to ensure visibility: {ex.Message}");
                            Environment.ExitCode = 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorPrinter.Print(_logger, ex, IsDetailed());
                        Environment.ExitCode = 1;
                    }
                    finally
                    {
                        try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
                    }
                }
            }, profileOpt, outputOpt, symbolOpt, timeoutOpt);
            symEnsure.AddOption(profileOpt);
            symEnsure.AddOption(symbolOpt);
            symEnsure.AddOption(outputOpt);
            symEnsure.AddOption(timeoutOpt);
            symbol.AddCommand(symEnsure);


            // ------------------- symbol limits -------------------


            // symbol limits — print min/step/max lot sizes and a normalization example.
            // Flow: validate → Connect → best-effort EnsureSymbolVisible → GetVolumeConstraintsAsync → show example
            // (uses appsettings DefaultVolume, not profile-aware by default).
            // JSON/text output supported; sets ExitCode=1 on errors; always disconnects in finally.
            var symLimits = new Command("limits", "Show min/step/max volume for the symbol");
symLimits.AddAlias("lim");
            symLimits.SetHandler(async (string profile, string output, string? s, int timeoutMs) =>
            {
                Validators.EnsureProfile(profile);
                var symbolName = Validators.EnsureSymbol(s ?? GetOptions().DefaultSymbol);
                _selectedProfile = profile;

                using (UseOpTimeout(timeoutMs))
                using (_logger.BeginScope("Cmd:SYMBOL-LIMITS Profile:{Profile}", profile))
                using (_logger.BeginScope("Symbol:{Symbol}", symbolName))
                {
                    try
                    {
                        await ConnectAsync();

                        // Best-effort visibility (some servers require it)
                        try
                        {
                            using var visCts = StartOpCts();
                            await _mt5Account.EnsureSymbolVisibleAsync(
                                symbolName, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
                        }

                        using var opCts = StartOpCts();
                        var (min, step, max) = await CallWithRetry(
                            ct => _mt5Account.GetVolumeConstraintsAsync(symbolName, deadline: null, cancellationToken: ct),
                            opCts.Token);

                        // Show also a quick normalization example
                        var exampleVol = GetOptions().DefaultVolume;
                        var normalized = MT5Account.NormalizeVolume(exampleVol, min, step, max);

                        if (IsJson(output))
                        {
                            var json = new
                            {
                                symbol = symbolName,
                                volume = new { min, step, max },
                                example = new { requested = exampleVol, normalized }
                            };
                            Console.WriteLine(ToJson(json));
                        }
                        else
                        {
                            Console.WriteLine($"{symbolName} volume limits:");
                            Console.WriteLine($"  min = {min}");
                            Console.WriteLine($"  step= {step}");
                            Console.WriteLine($"  max = {max}");
                            Console.WriteLine($"example: requested {exampleVol} -> normalized {normalized}");
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorPrinter.Print(_logger, ex, IsDetailed());
                        Environment.ExitCode = 1;
                    }
                    finally
                    {
                        try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
                    }
                }
            }, profileOpt, outputOpt, symbolOpt, timeoutOpt);
            symLimits.AddOption(profileOpt);
            symLimits.AddOption(symbolOpt);
            symLimits.AddOption(outputOpt);
            symLimits.AddOption(timeoutOpt);
            symbol.AddCommand(symLimits);


            // ------------------- symbol show -------------------


            // symbol show — short card with Quote + Volume limits.
            // Flow: validate → Connect → best-effort EnsureSymbolVisible → SymbolInfoTickAsync & GetVolumeConstraintsAsync
            // → print JSON/text. Per-op timeouts via StartOpCts(); transient errors retried.
            // NOTE: default symbol comes from appsettings (GetOptions), not the selected profile by default.
            var symShow = new Command("show", "Short card: Quote + Limits");
symShow.AddAlias("sh");
            symShow.SetHandler(async (string profile, string output, string? s, int timeoutMs) =>
            {
                Validators.EnsureProfile(profile);
                var symbolName = Validators.EnsureSymbol(s ?? GetOptions().DefaultSymbol);
                _selectedProfile = profile;

                using (UseOpTimeout(timeoutMs))
                using (_logger.BeginScope("Cmd:SYMBOL-SHOW Profile:{Profile}", profile))
                using (_logger.BeginScope("Symbol:{Symbol}", symbolName))
                {
                    try
                    {
                        await ConnectAsync();

                        // Ensure visible
                        try
                        {
                            using var visCts = StartOpCts();
                            await _mt5Account.EnsureSymbolVisibleAsync(
                                symbolName, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
                        }

                        // Quote
                        using var qCts = StartOpCts();
                        var tick = await CallWithRetry(
                            ct => _mt5Account.SymbolInfoTickAsync(symbolName, deadline: null, cancellationToken: ct),
                            qCts.Token);

                        // Limits
                        using var lCts = StartOpCts();
                        var (min, step, max) = await CallWithRetry(
                            ct => _mt5Account.GetVolumeConstraintsAsync(symbolName, deadline: null, cancellationToken: ct),
                            lCts.Token);

                        if (IsJson(output))
                        {
                            var json = new
                            {
                                symbol = symbolName,
                                quote = new { tick.Bid, tick.Ask, tick.Time },
                                volume = new { min, step, max }
                            };
                            Console.WriteLine(ToJson(json));
                        }
                        else
                        {
                            Console.WriteLine($"{symbolName}:");
                            Console.WriteLine($"  Quote: Bid={tick.Bid} Ask={tick.Ask} Time={tick.Time}");
                            Console.WriteLine($"  Volume: min={min} step={step} max={max}");
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorPrinter.Print(_logger, ex, IsDetailed());
                        Environment.ExitCode = 1;
                    }
                    finally
                    {
                        try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
                    }
                }
            }, profileOpt, outputOpt, symbolOpt, timeoutOpt);
            symShow.AddOption(profileOpt);
            symShow.AddOption(symbolOpt);
            symShow.AddOption(outputOpt);
            symShow.AddOption(timeoutOpt);
            symbol.AddCommand(symShow);

            // register the group
            root.AddCommand(symbol);


            //====================================
            //==------------- ticket -----------==
            //====================================


            // ticket show — lookup details for a ticket.
            // Flow: Connect → check open tickets → try to pull full object from opened aggregate;
            //        if not found → search recent history (last --days).
            // Output: JSON or text; ExitCode=2 when not found, 1 on errors; always disconnects in finally.
            // Timeouts & retries: UseOpTimeout + StartOpCts per RPC; CallWithRetry handles transient gRPC errors.
            // Notes: TryFindByTicketInAggregate/Get<T> are reflection-based; history scan is client-side — may be slow with large windows.
            // Usage: mt5cli ticket show -p default -t 123456  |  mt5cli t sh -t 123456 -d 7 --output json
            var ticketCmd = new Command("ticket", "Work with a specific ticket");
ticketCmd.AddAlias("t");

// ticket show
var tShow = new Command("show", "Show info for the ticket (open or from recent history)");
tShow.AddAlias("sh");

var tOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Ticket id") { IsRequired = true };
var tDaysOpt = new Option<int>(new[] { "--days", "-d" }, () => 30, "If not open, search in last N days history");

tShow.AddOption(profileOpt);
tShow.AddOption(outputOpt);
tShow.AddOption(tOpt);
tShow.AddOption(tDaysOpt);

tShow.SetHandler(async (string profile, string output, ulong ticket, int days, int timeoutMs) =>
{
    Validators.EnsureProfile(profile);
    if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "Days must be > 0.");
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:TICKET-SHOW Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket}", ticket))
    {
        try
        {
            await ConnectAsync();

            // 1) check open sets (orders/positions)
            using var opCts = StartOpCts();
            var openTickets = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersTicketsAsync(deadline: null, cancellationToken: ct),
                opCts.Token);

            bool isOpenOrder    = openTickets.OpenedOrdersTickets.Contains((long)ticket);
bool isOpenPosition = openTickets.OpenedPositionTickets.Contains((long)ticket);


            // 2) try to get full object from opened aggregate
            using var aggCts = StartOpCts();
            var openedAgg = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                aggCts.Token);

            var obj = TryFindByTicketInAggregate(openedAgg, ticket, out var bucket);

            if (obj is not null)
            {
                var symbol = Get<string>(obj, "Symbol");
                var volume = Get<double?>(obj, "Volume") ?? 0.0;
                var price  = Get<double?>(obj, "PriceOpen", "OpenPrice", "Price") ?? 0.0;
                var sl     = Get<double?>(obj, "StopLoss", "SL");
                var tp     = Get<double?>(obj, "TakeProfit", "TP");
                var profit = Get<double?>(obj, "Profit");

                if (IsJson(output))
                {
                    Console.WriteLine(ToJson(new {
                        ticket,
                        state = isOpenPosition ? "position-open" : isOpenOrder ? "pending-open" : "open-unknown",
                        bucket,
                        symbol,
                        volume,
                        priceOpen = price,
                        sl,
                        tp,
                        profit
                    }));
                }
                else
                {
                    Console.WriteLine($"Ticket #{ticket}  [{(isOpenPosition ? "POSITION" : isOpenOrder ? "PENDING" : bucket)}]");
                    Console.WriteLine($"  Symbol: {symbol}");
                    Console.WriteLine($"  Volume: {volume}");
                    Console.WriteLine($"  Price:  {price}");
                    if (sl.HasValue) Console.WriteLine($"  SL:     {sl}");
                    if (tp.HasValue) Console.WriteLine($"  TP:     {tp}");
                    if (profit.HasValue) Console.WriteLine($"  PnL:    {profit}");
                }

                try { await _mt5Account.DisconnectAsync(); } catch { }
                return;
            }

            // 3) not found in open -> search in history
            using var hCts = StartOpCts();
            var from = DateTime.UtcNow.AddDays(-Math.Abs(days));
            var to   = DateTime.UtcNow;
            var hist = await CallWithRetry(
                ct => _mt5Account.OrderHistoryAsync(from, to, deadline: null, cancellationToken: ct),
                hCts.Token);

            var item = hist.HistoryData.FirstOrDefault(h =>
                (h.HistoryOrder?.Ticket ?? 0UL) == ticket ||
                (h.HistoryDeal?.Ticket  ?? 0UL) == ticket);

            if (item is null)
            {
                if (IsJson(output)) Console.WriteLine(ToJson(new { ticket, found = false }));
                else Console.WriteLine($"Ticket #{ticket} not found in open sets or last {days} days.");
                Environment.ExitCode = 2;
            }
            else
            {
                if (item.HistoryOrder is not null)
                {
                    var o = item.HistoryOrder;
                    var setup = o.SetupTime?.ToDateTime();
                    var done  = o.DoneTime?.ToDateTime();

                    if (IsJson(output))
                    {
                        Console.WriteLine(ToJson(new {
                            ticket,
                            type = "order-history",
                            o.Symbol, o.State, o.VolumeInitial, o.VolumeCurrent, o.PriceOpen, setup, done
                        }));
                    }
                    else
                    {
                        Console.WriteLine($"Ticket #{ticket} [ORDER history]");
                        Console.WriteLine($"  {o.Symbol} state={o.State} vol={o.VolumeInitial}->{o.VolumeCurrent} open={o.PriceOpen}");
                        Console.WriteLine($"  setup={setup:O}  done={done:O}");
                    }
                }
                else if (item.HistoryDeal is not null)
                {
                    var d = item.HistoryDeal;
                    var t = d.Time?.ToDateTime();

                    if (IsJson(output))
                    {
                        Console.WriteLine(ToJson(new {
                            ticket,
                            type = "deal-history",
                            d.Symbol, d.Type, d.Volume, d.Price, d.Profit, t
                        }));
                    }
                    else
                    {
                        Console.WriteLine($"Ticket #{ticket} [DEAL history]");
                        Console.WriteLine($"  {d.Symbol} type={d.Type} vol={d.Volume} price={d.Price} pnl={d.Profit} time={t:O}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
}, profileOpt, outputOpt, tOpt, tDaysOpt, timeoutOpt);

ticketCmd.AddCommand(tShow);
root.AddCommand(ticketCmd);


            //=====================================================
            //==--------- cancel (delete pending order) ---------==
            //=====================================================


            // cancel — delete a pending order by ticket.
            // Core: EnsureSymbolVisible (best-effort) → CloseOrderByTicketAsync(ticket, symbol, volume:0) with retry.
            // Notes: `volume=0` is the delete-pending convention; requires broker/server support.
            // Supports --dry-run; per-op timeout via UseOpTimeout/StartOpCts; transient gRPC errors retried.
            //
            // Examples:
            //   mt5cli cancel -p default -t 123456 -s EURUSD
            //   mt5cli x --ticket 987654 --symbol XAUUSD --timeout-ms 8000
            var cancel = new Command("cancel", "Cancel (delete) pending order by ticket");
cancel.AddAlias("x");

var cancelTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Pending order ticket") { IsRequired = true };
var cancelSymbolOpt = new Option<string>(new[] { "--symbol", "-s" }, "Symbol (e.g., EURUSD)") { IsRequired = true };

cancel.AddOption(profileOpt);
cancel.AddOption(cancelTicketOpt);
cancel.AddOption(cancelSymbolOpt);

cancel.SetHandler(async (string profile, ulong ticket, string symbol, int timeoutMs, bool dryRun) =>
{
    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    symbol = Validators.EnsureSymbol(symbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CANCEL Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} Symbol:{Symbol}", ticket, symbol))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] CANCEL pending #{ticket} {symbol}");
            return;
        }

        try
        {
            await ConnectAsync();

            // Best-effort: ensure symbol is visible (some servers require it even for pending ops)
            try
            {
                using var visCts = StartOpCts();
                await _mt5Account.EnsureSymbolVisibleAsync(
                    symbol, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
            }

            // Volume=0 — convention: delete pending order by ticket
            using var opCts = StartOpCts();
            await CallWithRetry(
                ct => _mt5Account.CloseOrderByTicketAsync(
                    ticket: ticket,
                    symbol: symbol,
                    volume: 0.0,               // <<< key: 0 = delete pending
                    deadline: null,
                    cancellationToken: ct),
                opCts.Token);

            _logger.LogInformation("CANCEL done: ticket={Ticket}", ticket);
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
}, profileOpt, cancelTicketOpt, cancelSymbolOpt, timeoutOpt, dryRunOpt);

root.AddCommand(cancel);


            //======================================
            //===------ close-all (unified) -----===
            //======================================


            // close-all (aliases: flatten, close.all) — close ALL open positions, optionally filtered by --filter-symbol.
            // Safety: requires --yes to execute; otherwise shows a preview and exits with code 2. --dry-run prints the plan and exits.
            // Engine: lists positions via ListPositionVolumesAsync(filter) → closes each with ClosePositionFullAsync(ticket, vol, --deviation).
            // Resilience: per-op timeout (UseOpTimeout/StartOpCts) + retry on transient gRPC errors; always disconnects in finally.
            // Exit codes: 0 all closed, 1 some failed, 2 preview/no confirmation.
            // Examples: mt5cli close-all -p default --yes
            // mt5cli flatten -p default -s EURUSD --deviation 15 --yes
            var caSymbolOpt = new Option<string?>(new[] { "--filter-symbol", "-s" }, "Close only positions for this symbol (e.g., EURUSD)");
var caYesOpt    = new Option<bool>(new[] { "--yes", "-y" }, "Do not ask for confirmation");
var caDevOpt    = new Option<int>(new[] { "--deviation" }, () => 10, "Max slippage in points");

var closeAll = new Command("close-all", "Close ALL open positions (optionally filtered by symbol)");
closeAll.AddAlias("flatten");
closeAll.AddAlias("close.all"); 

closeAll.AddOption(profileOpt);
closeAll.AddOption(caSymbolOpt);
closeAll.AddOption(caYesOpt);
closeAll.AddOption(caDevOpt);
closeAll.AddOption(timeoutOpt);
closeAll.AddOption(dryRunOpt);

closeAll.SetHandler(async (InvocationContext ctx) =>
{
    var profile      = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var filterSymbol = ctx.ParseResult.GetValueForOption(caSymbolOpt);
    var yes          = ctx.ParseResult.GetValueForOption(caYesOpt);
    var deviation    = ctx.ParseResult.GetValueForOption(caDevOpt);
    var timeoutMs    = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun       = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (!string.IsNullOrWhiteSpace(filterSymbol)) _ = Validators.EnsureSymbol(filterSymbol);

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CLOSE-ALL Profile:{Profile}", profile))
    using (_logger.BeginScope("FilterSymbol:{Symbol} Dev:{Dev}", filterSymbol ?? "<any>", deviation))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            var map = await CallWithRetry(
                ct => _mt5Account.ListPositionVolumesAsync(filterSymbol, ct),
                opCts.Token);

            if (map.Count == 0)
            {
                Console.WriteLine("No positions to close.");
                return;
            }

            if (!yes || dryRun)
            {
                Console.WriteLine($"Will close {map.Count} position(s){(filterSymbol is null ? "" : $" for {filterSymbol}")}. Deviation={deviation}");
                foreach (var (ticket, vol) in map.Take(10))
                    Console.WriteLine($"  #{ticket} vol={vol}");
                if (map.Count > 10) Console.WriteLine($"  ... and {map.Count - 10} more");
                if (dryRun) return;
                Console.WriteLine("Pass --yes to execute.");
                Environment.ExitCode = 2;
                return;
            }

            int ok = 0, fail = 0;
            foreach (var (ticket, vol) in map)
            {
                try
                {
                    await CallWithRetry(ct => _mt5Account.ClosePositionFullAsync(ticket, vol, deviation, ct), opCts.Token);
                    ok++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Close #{Ticket} vol={Vol} failed: {Msg}", ticket, vol, ex.Message);
                    fail++;
                }
            }

            Console.WriteLine($"Closed OK: {ok}; Failed: {fail}");
            Environment.ExitCode = fail == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
});

root.AddCommand(closeAll);


            //====================================
            //==--------- cancel.all -----------==
            //====================================


            // cancel.all (alias: ca) — cancel ALL pending orders, with optional filters.
            // Filters: --symbol SYMBOL and --type limit|stop|stoplimit|any (default any).
            // Flow: list tickets → optional type filter via GetPendingKindsAsync → dry-run preview or cancel each via CancelPendingOrderAsync.
            // Resilience: per-op timeout (UseOpTimeout/StartOpCts) + retry on transient gRPC errors; prints OK/Failed counts.
            // Examples: mt5cli cancel.all -p default
            //           mt5cli ca -p default -s EURUSD --type stoplimit --dry-run
            var pendSymbolOpt = new Option<string?>(new[] { "--symbol", "-s" }, "Filter by symbol (optional)");
var pendTypeOpt   = new Option<string?>(new[] { "--type" }, "Filter by type: limit|stop|stoplimit|any (default any)");

var cancelAll = new Command("cancel.all", "Cancel all pending orders (optionally filtered)");
cancelAll.AddAlias("ca");

cancelAll.AddOption(profileOpt);
cancelAll.AddOption(pendSymbolOpt);
cancelAll.AddOption(pendTypeOpt);
cancelAll.AddOption(timeoutOpt);
cancelAll.AddOption(dryRunOpt);

cancelAll.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var symbol    = ctx.ParseResult.GetValueForOption(pendSymbolOpt);
    var typeStr   = (ctx.ParseResult.GetValueForOption(pendTypeOpt) ?? "any").Trim().ToLowerInvariant();
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (!string.IsNullOrWhiteSpace(symbol)) _ = Validators.EnsureSymbol(symbol!);

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CANCEL.ALL Profile:{Profile}", profile))
    using (_logger.BeginScope("Filter Symbol:{Symbol} Type:{Type}", symbol ?? "<any>", typeStr))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            var tickets = await CallWithRetry(
                ct => _mt5Account.ListPendingTicketsAsync(symbol, ct),
                opCts.Token);

            if (tickets.Count == 0)
            {
                Console.WriteLine("No pending orders match the filter.");
                return;
            }

            IEnumerable<ulong> list = tickets;
            if (!string.IsNullOrWhiteSpace(typeStr) && typeStr != "any")
            {
                var kinds = await _mt5Account.GetPendingKindsAsync(tickets, opCts.Token);
                bool Want(string k) => typeStr switch
                {
                    "limit"     => k.Contains("limit") && !k.Contains("stoplimit"),
                    "stop"      => k.EndsWith("stop") && !k.Contains("limit"),
                    "stoplimit" => k.Contains("stoplimit"),
                    _           => true
                };
                list = tickets.Where(t => kinds.TryGetValue(t, out var k) && Want(k));
            }

            var toCancel = list.ToList();
            Console.WriteLine($"Found {toCancel.Count} pending order(s) to cancel.");

            if (dryRun)
            {
                foreach (var t in toCancel) Console.WriteLine($"[DRY-RUN] CANCEL ticket={t}");
                return;
            }

            int ok = 0, fail = 0;
            foreach (var t in toCancel)
            {
                try { await CallWithRetry(ct => _mt5Account.CancelPendingOrderAsync(t, ct), opCts.Token); ok++; }
                catch (Exception ex) { _logger.LogWarning("Cancel {Ticket} failed: {Msg}", t, ex.Message); fail++; }
            }

            Console.WriteLine($"✔ Cancelled: {ok}, ✖ Failed: {fail}");
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
});

root.AddCommand(cancelAll);


            //====================================
            //==--------- close.partial --------==
            //====================================


            // close.partial — close a specific volume (lots) by ticket.
            // Validates inputs, supports --dry-run, per-op timeout (UseOpTimeout/StartOpCts) and retry on transient gRPC errors.
            // Calls MT5Account.ClosePositionPartialAsync(ticket, volume, deviation); volume must respect broker lot step.
            // Prints a simple success line; sets ExitCode=1 on errors; always disconnects in finally.
            var cpVolumeOpt = new Option<double>(new[] { "--volume", "-v" }, "Volume (lots) to close")
{
    IsRequired = true
};

var closePartial = new Command("close.partial", "Partially close a position by ticket");
closePartial.AddAlias("cp");

closePartial.AddOption(profileOpt);
closePartial.AddOption(cpTicketOpt);
closePartial.AddOption(cpVolumeOpt);
closePartial.AddOption(devOpt);      // reuse: deviation in points
closePartial.AddOption(timeoutOpt);
closePartial.AddOption(dryRunOpt);

closePartial.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(cpTicketOpt);
    var volume    = ctx.ParseResult.GetValueForOption(cpVolumeOpt);
    var deviation = ctx.ParseResult.GetValueForOption(devOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (ticket == 0) throw new ArgumentOutOfRangeException(nameof(ticket), "Ticket must be > 0.");
    if (volume <= 0) throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be > 0.");

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CLOSE.PARTIAL Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} Vol:{Vol} Dev:{Dev}", ticket, volume, deviation))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] CLOSE.PARTIAL ticket={ticket} volume={volume} deviation={deviation}");
            return;
        }

        try
        {
            await ConnectAsync();

            using var opCts = StartOpCts();
            await CallWithRetry(
                ct => _mt5Account.ClosePositionPartialAsync(ticket, volume, deviation, ct),
                opCts.Token);

            Console.WriteLine("✔ close.partial done");
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
});

root.AddCommand(closePartial);


            //====================================
            //==------ closeby (emulated) ------==
            //====================================


            // closeby — emulate MT5 “Close By” using two partial closes.
            // Core: validates tickets/volume → Connect → CloseByEmulatedAsync(a,b,volume,deviation) with per-op timeout + retry.
            // Caveats: tickets must be same symbol and opposite directions; not atomic — if leg2 fails you may stay partially exposed.
            // Supports --dry-run; prints a simple success line; disconnects in finally.
            // Examples:
            //   mt5cli closeby -p default --a 111111 --b 222222 -v 0.10 --deviation 15
            //   mt5cli closeby --a 111111 --b 222222 -v 0.05 --dry-run
            var cbATicketOpt = new Option<ulong>(new[] { "--a", "-a" }, "Ticket of the first position") { IsRequired = true };
var cbBTicketOpt = new Option<ulong>(new[] { "--b", "-b" }, "Ticket of the opposite position") { IsRequired = true };
var cbVolOpt     = new Option<double>(new[] { "--volume", "-v" }, "Volume (lots) to close on each leg") { IsRequired = true };
var cbDevOpt     = new Option<int>(new[] { "--deviation" }, () => 10, "Max slippage in points");

var closeby = new Command("closeby", "Close a position by the opposite position (emulated with two closes)");
closeby.AddOption(profileOpt);
closeby.AddOption(cbATicketOpt);
closeby.AddOption(cbBTicketOpt);
closeby.AddOption(cbVolOpt);
closeby.AddOption(cbDevOpt);
closeby.AddOption(timeoutOpt);
closeby.AddOption(dryRunOpt);

closeby.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var a         = ctx.ParseResult.GetValueForOption(cbATicketOpt);
    var b         = ctx.ParseResult.GetValueForOption(cbBTicketOpt);
    var volume    = ctx.ParseResult.GetValueForOption(cbVolOpt);
    var deviation = ctx.ParseResult.GetValueForOption(cbDevOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (a == 0 || b == 0) throw new ArgumentOutOfRangeException("tickets", "Tickets must be > 0.");
    if (volume <= 0) throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be > 0.");

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CLOSEBY Profile:{Profile}", profile))
    using (_logger.BeginScope("A:{A} B:{B} Vol:{Vol} Dev:{Dev}", a, b, volume, deviation))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] CLOSEBY a={a} b={b} volume={volume} deviation={deviation}");
            return;
        }

        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            await CallWithRetry(ct => _mt5Account.CloseByEmulatedAsync(a, b, volume, deviation, ct), opCts.Token);

            Console.WriteLine("✔ closeby (emulated) done");
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
});

root.AddCommand(closeby);


            //====================================
            //==------------ reverse -----------==
            //====================================


            // reverse (alias: rv) — flip net exposure on a symbol.
            // Modes: --mode net  → send ONE opposite market order of 2×|net|;
            //        --mode flat → close ALL positions for symbol, then open 1×|net| opposite.
            // Extras: optional --sl/--tp for the new leg, --deviation for slippage, --dry-run preview.
            // Flow: Connect → EnsureSymbolVisible → compute net from opened positions → act per mode; per-op timeout + retry.
            // Caveats: not atomic; in flat mode some closes may fail (logs a warning).
            // Examples: mt5cli reverse -p default -s EURUSD --mode net --tp 1.10
            //           mt5cli rv -s XAUUSD --mode flat --sl 2395.0 --deviation 20
            var modeOpt = new Option<string>(
    name: "--mode",
    description: "Reverse mode: net (single opposite order 2x) | flat (close all for symbol, then open 1x)",
    getDefaultValue: () => "net");

var reverse = new Command("reverse", "Reverse net position for a symbol");
reverse.AddAlias("rv");

reverse.AddOption(profileOpt);
reverse.AddOption(symbolOpt);
reverse.AddOption(modeOpt);
// reuse SL/TP/deviation so user can set protective exits for the new leg
reverse.AddOption(slOpt);
reverse.AddOption(tpOpt);
reverse.AddOption(devOpt);

reverse.SetHandler(async (string profile, string? symbol, string mode, double? sl, double? tp, int deviation, int timeoutMs, bool dryRun) =>
{


    // -------------------------- validate --------------------------

    Validators.EnsureProfile(profile);
    Validators.EnsureDeviation(deviation);
    var s = Validators.EnsureSymbol(symbol ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:REVERSE Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol} Mode:{Mode}", s, mode))
    {
        try
        {
            await ConnectAsync();

            // Ensure visibility
            try
            {
                using var visCts = StartOpCts();
                await _mt5Account.EnsureSymbolVisibleAsync(
                    s, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
            }

            //1) Get all positions by symbol and calculate the net
            using var opCts1 = StartOpCts();
            var opened = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                opCts1.Token);

            var posList = opened.PositionInfos
                .Where(p => string.Equals(p.Symbol, s, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (posList.Count == 0)
            {
                Console.WriteLine($"No positions for {s} to reverse.");
                Environment.ExitCode = 2;
                return;
            }

            double net = 0.0;
            foreach (var p in posList)
            {
                var sign = IsLongPosition(p) ? 1.0 : -1.0;
                net += sign * p.Volume;
            }

            if (Math.Abs(net) < 1e-9)
            {
                Console.WriteLine($"Net position for {s} is zero; nothing to reverse.");
                Environment.ExitCode = 2;
                return;
            }

            // 2) depending on the mode
            mode = mode?.Trim().ToLowerInvariant() ?? "net";
            if (mode != "net" && mode != "flat")
                throw new ArgumentException("Invalid --mode. Use: net | flat");

            if (dryRun)
            {
                if (mode == "net")
                {
                    var volToSend = Math.Abs(net) * 2.0;
                    var side = net > 0 ? "SELL" : "BUY";
                    Console.WriteLine($"[DRY-RUN] REVERSE({mode}) {s}: send {side} vol={volToSend} (deviation={deviation}) SL={sl} TP={tp}");
                }
                else
                {
                    var side = net > 0 ? "SELL" : "BUY";
                    Console.WriteLine($"[DRY-RUN] REVERSE({mode}) {s}: close ALL positions for {s}; then {side} vol={Math.Abs(net)} SL={sl} TP={tp}");
                }
                return;
            }

            if (mode == "net")
            {
                // Single opposite order of 2*|net|
                var volToSend = Math.Abs(net) * 2.0;
                using var opCts = StartOpCts();
                var ticket = await CallWithRetry(
                    ct => _mt5Account.SendMarketOrderAsync(
                        symbol: s,
                        isBuy: net < 0,              // if short net, buy; if long net, sell
                        volume: volToSend,
                        deviation: deviation,
                        stopLoss: sl,
                        takeProfit: tp,
                        deadline: null,
                        cancellationToken: ct),
                    opCts.Token);

                _logger.LogInformation("REVERSE(net) done: ticket={Ticket} newSide={Side} volSent={Vol}",
                    ticket, net < 0 ? "BUY" : "SELL", volToSend);
            }
            else // flat
            {
                // Close all positions for symbol, then open 1*|net| in opposite direction
                var batch = posList.Select(p => (p.Ticket, p.Symbol, p.Volume)).ToList();
                using var opCtsClose = StartOpCts();
                var (ok, fail) = await ClosePositionsAsync(batch, opCtsClose.Token);
                if (fail > 0)
                {
                    Console.WriteLine($"Warning: some positions failed to close. OK={ok} FAIL={fail}");
                }

                using var opCtsOpen = StartOpCts();
                var ticket = await CallWithRetry(
                    ct => _mt5Account.SendMarketOrderAsync(
                        symbol: s,
                        isBuy: net < 0,
                        volume: Math.Abs(net),
                        deviation: deviation,
                        stopLoss: sl,
                        takeProfit: tp,
                        deadline: null,
                        cancellationToken: ct),
                    opCtsOpen.Token);

                _logger.LogInformation("REVERSE(flat) done: ticket={Ticket} side={Side} vol={Vol}",
                    ticket, net < 0 ? "BUY" : "SELL", Math.Abs(net));
            }
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
}, profileOpt, symbolOpt, modeOpt, slOpt, tpOpt, devOpt, timeoutOpt, dryRunOpt);

            root.AddCommand(reverse);


            //====================================
            //==--------- reverse.ticket -------==
            //====================================


            // reverse.ticket (alias: rvt) — reverse a single position by ticket.
            // Flow: Connect → find position → best-effort EnsureSymbolVisible → DRY-RUN or:
            //        ClosePositionPartialAsync(ticket, full vol) → SendMarketOrderAsync opposite side (same vol, optional SL/TP).
            // Resilience: per-op timeout (UseOpTimeout/StartOpCts) + CallWithRetry; ExitCode=2 if ticket not found.
            // Caveats: two non-atomic steps (slippage gap between close/open); lot step/limits must be valid.
            // Example: mt5cli rvt -p default -t 123456 --sl 1.095 --tp 1.105 --deviation 15
            var rvTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket") { IsRequired = true };

var reverseTicket = new Command("reverse.ticket", "Reverse a single position by ticket");
reverseTicket.AddAlias("rvt");

reverseTicket.AddOption(profileOpt);
reverseTicket.AddOption(rvTicketOpt);
reverseTicket.AddOption(slOpt);
reverseTicket.AddOption(tpOpt);
reverseTicket.AddOption(devOpt);
reverseTicket.AddOption(timeoutOpt);
reverseTicket.AddOption(dryRunOpt);

reverseTicket.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(rvTicketOpt);
    var sl        = ctx.ParseResult.GetValueForOption(slOpt);
    var tp        = ctx.ParseResult.GetValueForOption(tpOpt);
    var deviation = ctx.ParseResult.GetValueForOption(devOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:REVERSE.TICKET Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} Dev:{Dev}", ticket, deviation))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            var opened = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                opCts.Token);

            var pos = opened.PositionInfos.FirstOrDefault(p => Convert.ToUInt64(p.Ticket) == ticket);
            if (pos is null)
            {
                Console.WriteLine($"Position #{ticket} not found.");
                Environment.ExitCode = 2;
                return;
            }

            var symbol = pos.Symbol;
            var vol    = pos.Volume;
            var isLong = IsLongPosition(pos); 

            try
            {
                using var visCts = StartOpCts();
                await _mt5Account.EnsureSymbolVisibleAsync(symbol, TimeSpan.FromSeconds(3), null, null, visCts.Token);
            }
            catch { /* ignore */ }

            if (dryRun)
            {
                Console.WriteLine($"[DRY-RUN] REVERSE.TICKET #{ticket} {symbol}: close {vol}, then {(isLong ? "SELL" : "BUY")} {vol} (dev={deviation}) SL={sl} TP={tp}");
                return;
            }

            await CallWithRetry(ct => _mt5Account.ClosePositionPartialAsync(ticket, vol, deviation, ct), opCts.Token);

            await CallWithRetry(ct => _mt5Account.SendMarketOrderAsync(
                    symbol: symbol,
                    isBuy: !isLong,
                    volume: vol,
                    deviation: deviation,
                    stopLoss: sl,
                    takeProfit: tp,
                    deadline: null,
                    cancellationToken: ct),
                opCts.Token);

            Console.WriteLine("✔ reverse.ticket done");
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
});

root.AddCommand(reverseTicket);


            //====================================
            //===---------- breakeven ---------===
            //====================================


            // breakeven (alias: be) — move SL to entry ± offset for one position.
            // Options: --offset (PRICE) or --offset-points (-P), mutually exclusive; --force allows non-improving moves.
            // Flow: connect → find position → compute offset (uses point guess if needed) → ensure symbol visible → DRY-RUN or ModifyPositionSlTpAsync(SL=target, TP unchanged).
            // Safety: by default only improves SL; exit code 2 on “not found” or “no improvement”; per-op timeout + retry.
            // Examples: mt5cli be -p default -t 123456 --offset-points 50
            //           mt5cli be -t 123456 --offset 0.0002 --force
            var beTicketOpt       = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket to move SL to breakeven") { IsRequired = true };
var beOffsetPriceOpt  = new Option<double?>(new[] { "--offset" }, "Offset from entry in PRICE units (e.g., 0.0002)");
var beOffsetPointsOpt = new Option<int?>(new[] { "--offset-points", "-P" }, "Offset from entry in POINTS");
var beForceOpt        = new Option<bool>(new[] { "--force" }, "Allow worsening SL (by default only improve)");

var breakeven = new Command("breakeven", "Move SL to entry ± offset (breakeven) for a position");
breakeven.AddAlias("be");

breakeven.AddOption(profileOpt);
breakeven.AddOption(beTicketOpt);
breakeven.AddOption(beOffsetPriceOpt);
breakeven.AddOption(beOffsetPointsOpt);
breakeven.AddOption(beForceOpt);

breakeven.SetHandler(async (string profile, ulong ticket, double? offsetPrice, int? offsetPoints, bool force, int timeoutMs, bool dryRun) =>
{

    // -------------------- validation --------------------

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);

    if (offsetPrice is not null && offsetPoints is not null)
        throw new ArgumentException("Use either --offset (price) OR --offset-points, not both.");
    if (offsetPrice is not null && offsetPrice < 0)
        throw new ArgumentOutOfRangeException(nameof(offsetPrice), "Offset must be >= 0.");
    if (offsetPoints is not null && offsetPoints < 0)
        throw new ArgumentOutOfRangeException(nameof(offsetPoints), "Offset points must be >= 0.");

    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:BREAKEVEN Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} OffsetPrice:{OffsetPrice} OffsetPoints:{OffsetPoints} Force:{Force}", ticket, offsetPrice, offsetPoints, force))
    {
        try
        {
            await ConnectAsync();

            // 1) get current position by ticket
            using var opCts1 = StartOpCts();
            var opened = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                opCts1.Token);

            var pos = opened.PositionInfos.FirstOrDefault(p =>
                p.Ticket == ticket ||
                unchecked((ulong)p.Ticket) == ticket // guard for long->ulong models
            );

            if (pos is null)
            {
                Console.WriteLine($"Position with ticket #{ticket} not found.");
                Environment.ExitCode = 2;
                return;
            }

            var symbol     = pos.Symbol;
            var entryPrice = pos.PriceOpen;

            // 2) compute offset in PRICE
            double offPrice;
            if (offsetPrice is not null)
            {
                offPrice = offsetPrice.Value;
            }
            else if (offsetPoints is not null)
            {
               var pointSize = _mt5Account.PointGuess(symbol);
if (pointSize <= 0)
    pointSize = symbol.EndsWith("JPY", StringComparison.OrdinalIgnoreCase) ? 0.01 : 0.0001;
                offPrice = offsetPoints.Value * pointSize;
            }
            else
            {
                offPrice = 0.0; // pure breakeven at entry
            }

            // 3) decide target SL depending on side
            var isLong = IsLongPosition(pos);
            var targetSl = isLong ? (entryPrice + offPrice) : (entryPrice - offPrice);

            // 4) only-improve logic (unless --force)
            var currentSl = TryGetDoubleProperty(pos, "StopLoss", "SL", "Sl");
            if (!force && currentSl is not null)
            {
                bool improves = isLong ? targetSl > currentSl.Value
                                       : targetSl < currentSl.Value;

                if (!improves)
                {
                    Console.WriteLine($"No improvement: current SL={currentSl.Value} target={targetSl}. Use --force to override.");
                    Environment.ExitCode = 2;
                    return;
                }
            }

            // Ensure symbol visible (best-effort)
            try
            {
                using var visCts = StartOpCts();
                await _mt5Account.EnsureSymbolVisibleAsync(
                    symbol, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
            }

            if (dryRun)
            {
                Console.WriteLine($"[DRY-RUN] BREAKEVEN #{ticket} {symbol}: SL -> {targetSl}");
                return;
            }

           // 5) modify SL, keep TP as-is (null = no change)
using var opCts2 = StartOpCts();
await CallWithRetry(
    ct => _mt5Account.ModifyPositionSlTpAsync(ticket, targetSl, null, ct),
    opCts2.Token);


_logger.LogInformation("BREAKEVEN done: ticket={Ticket} symbol={Symbol} SL={SL}", ticket, symbol, targetSl);

        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
}, profileOpt, beTicketOpt, beOffsetPriceOpt, beOffsetPointsOpt, beForceOpt, timeoutOpt, dryRunOpt);

root.AddCommand(breakeven);


            //====================================
            //==--------- close-symbol ---------==
            //====================================


            // close-symbol (aliases: cs, flatten-symbol)
            // Purpose:
            //   Close ALL open positions for a given symbol.
            //
            // Behavior:
            //   • If --symbol is omitted, uses the profile’s DefaultSymbol.
            //   • Without --yes: prints a preview of tickets/volumes and exits with code 2 (no changes).
            //   • With --yes: closes each matching position via ClosePositionsAsync and prints summary.
            //   • --dry-run: preview only; no RPCs sent.
            //   • On success prints: "Closed OK: N; Failed: M". Exit code 0 when M == 0.
            //
            // Scope / Notes:
            //   • Affects positions only (pending orders are ignored).
            //   • Uses per-operation timeout (--timeout-ms) and the program’s retry policy.
            //   • Prints "No positions to close" when nothing matches.
            //
            // Examples:
            //   mt5cli close-symbol -p demo -s EURUSD --yes
            //   mt5cli cs --yes                  // uses DefaultSymbol
            //   mt5cli flatten-symbol -s XAUUSD  // preview (needs --yes to execute)
            var closeSymbol = new Command("close-symbol", "Close ALL open positions for a given symbol");
closeSymbol.AddAlias("cs");
closeSymbol.AddAlias("flatten-symbol");

closeSymbol.AddOption(profileOpt);
closeSymbol.AddOption(symbolOpt);
// reuse the global confirmation flag from close-all (caYesOpt)
closeSymbol.AddOption(caYesOpt);

closeSymbol.SetHandler(async (string profile, string? symbol, bool yes, int timeoutMs, bool dryRun) =>
{
    Validators.EnsureProfile(profile);

    // Default to profile's default symbol if not provided
    var s = Validators.EnsureSymbol(symbol ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CLOSE-SYMBOL Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol}", s))
    {
        try
        {
            if (dryRun)
            {
                Console.WriteLine($"[DRY-RUN] Would close all positions for {s}.");
                return;
            }

            await ConnectAsync();
            using var opCts = StartOpCts();

            var opened = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                opCts.Token);

            var positions = opened.PositionInfos
                .Where(p => string.Equals(p.Symbol, s, StringComparison.OrdinalIgnoreCase))
                .Select(p => (p.Ticket, p.Symbol, p.Volume))
                .ToList();

            if (positions.Count == 0)
            {
                Console.WriteLine($"No positions to close for {s}.");
                return;
            }

            if (!yes)
            {
                Console.WriteLine($"Will close {positions.Count} position(s) for {s}:");
                foreach (var (t, sym, v) in positions.Take(10))
                    Console.WriteLine($"  #{t} {sym} vol={v}");
                if (positions.Count > 10) Console.WriteLine($"  ... and {positions.Count - 10} more");
                Console.WriteLine("Pass --yes to execute.");
                Environment.ExitCode = 2; // require confirmation
                return;
            }

            var (ok, fail) = await ClosePositionsAsync(positions, opCts.Token);
            Console.WriteLine($"Closed OK: {ok}; Failed: {fail}");
            Environment.ExitCode = fail == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
}, profileOpt, symbolOpt, caYesOpt, timeoutOpt, dryRunOpt);

root.AddCommand(closeSymbol);


            //====================================
            //==--------- partial-close --------==
            //====================================


            // partial-close (alias: pc) — close part of a position by ticket.
            // Options: exactly one of --percent (-P, 1..100) OR --volume (-v); uses current position volume to compute amount.
            // Flow: connect → find position → compute volToClose → ensure symbol visible → DRY-RUN or CloseOrderByTicketAsync(ticket, symbol, vol).
            // Safety: clamps to current volume; exit code 2 on “not found”/non-positive computed volume; per-op timeout & retry respected.
            // Example: mt5cli pc -p default -t 123456 -P 25   |   mt5cli pc -t 123456 -v 0.10
            var pcTicketOpt  = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket to partially close") { IsRequired = true };
var pcPercentOpt = new Option<int?>(new[] { "--percent", "-P" }, "Percent of current volume to close (1..100)");
var pcVolumeOpt  = new Option<double?>(new[] { "--volume", "-v" }, "Exact volume to close (lots)");

var pclose = new Command("partial-close", "Partially close a position by ticket");
pclose.AddAlias("pc");

pclose.AddOption(profileOpt);
pclose.AddOption(pcTicketOpt);
pclose.AddOption(pcPercentOpt);
pclose.AddOption(pcVolumeOpt);

pclose.SetHandler(async (string profile, ulong ticket, int? percent, double? volume, int timeoutMs, bool dryRun) =>
{

    // --------------------- validation ---------------------

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);

    if ((percent is null && volume is null) || (percent is not null && volume is not null))
        throw new ArgumentException("Specify exactly one: --percent or --volume.");

    if (percent is not null)
    {
        if (percent <= 0 || percent > 100)
            throw new ArgumentOutOfRangeException(nameof(percent), "Percent must be in 1..100.");
    }
    if (volume is not null)
    {
        if (volume <= 0) throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be > 0.");
    }

    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:PARTIAL-CLOSE Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} Percent:{Percent} Volume:{Volume}", ticket, percent, volume))
    {
        try
        {
            await ConnectAsync();

            using var opCts1 = StartOpCts();
            var opened = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                opCts1.Token);

            var pos = opened.PositionInfos.FirstOrDefault(p =>
                p.Ticket == ticket ||
                (p.Ticket is 0ul && unchecked((ulong)(long)p.Ticket) == ticket)
            );

            if (pos is null)
            {
                Console.WriteLine($"Position with ticket #{ticket} not found.");
                Environment.ExitCode = 2;
                return;
            }

            var symbol = pos.Symbol;
            var currentVol = pos.Volume;

            double volToClose = volume ?? Math.Round(currentVol * (percent!.Value / 100.0), 8);
            if (volToClose <= 0)
            {
                Console.WriteLine("Computed volume to close is <= 0.");
                Environment.ExitCode = 2;
                return;
            }
            if (volToClose > currentVol)
            {
                volToClose = currentVol;
            }

            // Best-effort: ensure symbol visible
            try
            {
                using var visCts = StartOpCts();
                await _mt5Account.EnsureSymbolVisibleAsync(
                    symbol, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
            }

            if (dryRun)
            {
                Console.WriteLine($"[DRY-RUN] PARTIAL-CLOSE #{ticket} {symbol} vol={volToClose} (current={currentVol})");
                return;
            }

            using var opCts2 = StartOpCts();
            await CallWithRetry(
                ct => _mt5Account.CloseOrderByTicketAsync(
                    ticket: ticket,
                    symbol: symbol,
                    volume: volToClose,
                    deadline: null,
                    cancellationToken: ct),
                opCts2.Token);

            _logger.LogInformation("PARTIAL-CLOSE done: ticket={Ticket} symbol={Symbol} closedVol={Vol} currentVol(before)={Cur}",
                ticket, symbol, volToClose, currentVol);
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
}, profileOpt, pcTicketOpt, pcPercentOpt, pcVolumeOpt, timeoutOpt, dryRunOpt);

root.AddCommand(pclose);


            //====================================
            //==--------- position.modify ------==
            //====================================

          
            // position.modify (alias: posmod) — update SL/TP for an open position by ticket.
            // Options: --ticket (-t) REQUIRED; specify at least one of --sl or --tp; supports --timeout-ms and --dry-run.
            // Flow: connect → per-op timeout → CallWithRetry(_mt5Account.ModifyPositionSlTpAsync(ticket, sl?, tp?)) → print result.
            // Safety: rejects ticket=0 and missing SL/TP; leaves the unspecified side unchanged (null means “no change”).
            // Examples:
            //   mt5cli position.modify -t 123456 --sl 1.0950
            //   mt5cli posmod -t 123456 --tp 1.1050 --sl 1.0940 --timeout-ms 15000
            var posModTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket") { IsRequired = true };
var posModSlOpt     = new Option<double?>(new[] { "--sl" }, "New Stop Loss (price)");
var posModTpOpt     = new Option<double?>(new[] { "--tp" }, "New Take Profit (price)");

var posModify = new Command("position.modify", "Modify SL/TP for a position by ticket");
posModify.AddAlias("posmod");

posModify.AddOption(profileOpt);
posModify.AddOption(posModTicketOpt);
posModify.AddOption(posModSlOpt);
posModify.AddOption(posModTpOpt);
posModify.AddOption(timeoutOpt);
posModify.AddOption(dryRunOpt);

posModify.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(posModTicketOpt);
    var sl        = ctx.ParseResult.GetValueForOption(posModSlOpt);
    var tp        = ctx.ParseResult.GetValueForOption(posModTpOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (ticket == 0) throw new ArgumentOutOfRangeException(nameof(ticket), "Ticket must be > 0.");
    if (sl is null && tp is null) throw new ArgumentException("Specify at least one of --sl or --tp.");

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:POSITION.MODIFY Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} SL:{SL} TP:{TP}", ticket, sl, tp))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] POSITION.MODIFY ticket={ticket} SL={(sl?.ToString() ?? "-")} TP={(tp?.ToString() ?? "-")}");
            return;
        }

        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            await CallWithRetry(
                ct => _mt5Account.ModifyPositionSlTpAsync(ticket, sl, tp, ct),
                opCts.Token);

            Console.WriteLine("✔ position.modify done");
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
});

root.AddCommand(posModify);


            //=====================================
            //==------------- panic -------------==
            //=====================================


            // panic — emergency flatten: close ALL open positions, then cancel ALL pending orders.
            // Options: --symbol/-s (optional filter), --deviation (max slippage), --timeout-ms, --dry-run.
            // Flow: connect → gather positions & pendings (optionally by symbol) → CLOSE positions first (free margin) → CANCEL pendings.
            // Behavior: uses CallWithRetry; logs and continues on individual failures; prints a concise summary at the end.
            // Examples:
            //   mt5cli panic -p default
            //   mt5cli panic -s EURUSD --deviation 5 --timeout-ms 15000
            var panicSymbolOpt = new Option<string?>(new[] { "--symbol", "-s" }, "Limit to symbol (optional)");
var panicDevOpt    = new Option<int>(new[] { "--deviation" }, () => 10, "Max slippage for closes");

var panic = new Command("panic", "Close ALL positions and cancel ALL pendings (optionally by symbol)");
panic.AddOption(profileOpt);
panic.AddOption(panicSymbolOpt);
panic.AddOption(panicDevOpt);
panic.AddOption(timeoutOpt);
panic.AddOption(dryRunOpt);

panic.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var symbol    = ctx.ParseResult.GetValueForOption(panicSymbolOpt);
    var deviation = ctx.ParseResult.GetValueForOption(panicDevOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (!string.IsNullOrWhiteSpace(symbol)) _ = Validators.EnsureSymbol(symbol!);

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:PANIC Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol} Dev:{Dev}", symbol ?? "<any>", deviation))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            var pos = await CallWithRetry(ct => _mt5Account.ListPositionVolumesAsync(symbol, ct), opCts.Token);

            var pend = await CallWithRetry(ct => _mt5Account.ListPendingTicketsAsync(symbol, ct), opCts.Token);

            Console.WriteLine($"PANIC targets: positions={pos.Count}, pendings={pend.Count}");

            if (dryRun)
            {
                foreach (var kv in pos)  Console.WriteLine($"[DRY-RUN] CLOSE ticket={kv.Key} vol={kv.Value}");
                foreach (var t in pend)   Console.WriteLine($"[DRY-RUN] CANCEL ticket={t}");
                return;
            }

            foreach (var (ticket, vol) in pos)
            {
                try { await CallWithRetry(ct => _mt5Account.ClosePositionFullAsync(ticket, vol, deviation, ct), opCts.Token); }
                catch (Exception ex) { _logger.LogWarning("Close {Ticket} failed: {Msg}", ticket, ex.Message); }
            }

            foreach (var t in pend)
            {
                try { await CallWithRetry(ct => _mt5Account.CancelPendingOrderAsync(t, ct), opCts.Token); }
                catch (Exception ex) { _logger.LogWarning("Cancel {Ticket} failed: {Msg}", t, ex.Message); }
            }

            Console.WriteLine("✔ panic done");
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
});

root.AddCommand(panic);


            //=============================================
            //==--------- position.modify.points --------==
            //=============================================


            // position.modify.points (alias: pmp) — set SL/TP by distance in **symbol POINTS**
            // Base: either entry price or current market (Bid/Ask depending on side).
            // Options:
            //   --ticket/-t <id>          REQUIRED: position ticket
            //   --sl-points <N>           SL distance in points (>= 0)
            //   --tp-points <N>           TP distance in points (>= 0)
            //   --from <entry|market>     Distance measured from entry (default) or live market
            //   --timeout-ms <ms>         Per-op timeout override
            //   --dry-run                 Show what would be sent without modifying
            // Flow: find position → infer side → choose base price (entry/market) → convert points→price (PointGuess fallback) → call ModifyPositionSlTpAsync.
            // Notes: at least one of --sl-points/--tp-points must be provided. Uses CallWithRetry. Ensures symbol visibility implicitly elsewhere.
            // Examples:
            //   mt5cli pmp -t 123456 --sl-points 200
            //   mt5cli pmp -t 123456 --tp-points 350 --from market
            var pmpTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket") { IsRequired = true };
var pmpSlPtsOpt  = new Option<int?>(new[] { "--sl-points" }, "SL distance in POINTS (from base price)");
var pmpTpPtsOpt  = new Option<int?>(new[] { "--tp-points" }, "TP distance in POINTS (from base price)");
var pmpFromOpt   = new Option<string>(new[] { "--from" }, () => "entry", "entry|market"); // default: entry

var posModPts = new Command("position.modify.points", "Set SL/TP by distance in points from entry/market");
posModPts.AddAlias("pmp");

posModPts.AddOption(profileOpt);
posModPts.AddOption(pmpTicketOpt);
posModPts.AddOption(pmpSlPtsOpt);
posModPts.AddOption(pmpTpPtsOpt);
posModPts.AddOption(pmpFromOpt);
posModPts.AddOption(timeoutOpt);
posModPts.AddOption(dryRunOpt);

posModPts.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(pmpTicketOpt);
    var slPts     = ctx.ParseResult.GetValueForOption(pmpSlPtsOpt);
    var tpPts     = ctx.ParseResult.GetValueForOption(pmpTpPtsOpt);
    var fromStr   = (ctx.ParseResult.GetValueForOption(pmpFromOpt) ?? "entry").Trim().ToLowerInvariant();
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    if (fromStr != "entry" && fromStr != "market")
        throw new ArgumentException("Invalid --from. Use entry|market.");
    if (slPts is null && tpPts is null)
        throw new ArgumentException("Specify at least one of --sl-points or --tp-points.");
    if (slPts is not null && slPts < 0) throw new ArgumentOutOfRangeException(nameof(slPts));
    if (tpPts is not null && tpPts < 0) throw new ArgumentOutOfRangeException(nameof(tpPts));

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:POSITION.MODIFY.POINTS Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} SLpts:{SL} TPpts:{TP} From:{From}", ticket, slPts, tpPts, fromStr))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            var opened = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                opCts.Token);

            var pos = opened.PositionInfos.FirstOrDefault(p => Convert.ToUInt64(p.Ticket) == ticket);
            if (pos is null)
            {
                Console.WriteLine($"Position #{ticket} not found.");
                Environment.ExitCode = 2;
                return;
            }

            var symbol = pos.Symbol;

            bool isLong = IsLongPosition(pos);
            double basePrice;
            if (fromStr == "entry")
            {
                basePrice = pos.PriceOpen;
            }
            else
            {
                var q = await CallWithRetry(ct => FirstTickAsync(symbol, ct), opCts.Token);
                basePrice = isLong ? q.Bid : q.Ask;
            }

            var point = _mt5Account.PointGuess(symbol);
            if (point <= 0)
                point = symbol.EndsWith("JPY", StringComparison.OrdinalIgnoreCase) ? 0.01 : 0.0001;

            double? newSl = null, newTp = null;
            if (slPts is not null)
                newSl = isLong ? basePrice - slPts.Value * point : basePrice + slPts.Value * point;
            if (tpPts is not null)
                newTp = isLong ? basePrice + tpPts.Value * point : basePrice - tpPts.Value * point;

            if (dryRun)
            {
                Console.WriteLine($"[DRY-RUN] POSITION.MODIFY.POINTS #{ticket} {symbol} from={fromStr} → SL={(newSl?.ToString() ?? "-")} TP={(newTp?.ToString() ?? "-")}");
                return;
            }

            await CallWithRetry(
                ct => _mt5Account.ModifyPositionSlTpAsync(ticket, newSl, newTp, ct),
                opCts.Token);

            Console.WriteLine($"✔ position.modify.points done ({fromStr}): SL={(newSl?.ToString() ?? "-")} TP={(newTp?.ToString() ?? "-")}");
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
    }
});

root.AddCommand(posModPts);


            //======================================================
            //==--------- modify (change SL/TP by ticket) --------==
            //======================================================


            // modify (alias: m) — change StopLoss / TakeProfit by ticket
            // Purpose: update SL/TP for an existing position/pending order identified by ticket.
            // Behavior:
            //   - Requires at least one of --sl or --tp.
            //   - Optional --symbol is used only to ensure the symbol is visible in the terminal.
            //   - Respects global --timeout-ms and --dry-run.
            //   - Uses ModifyStopsSmartAsync with retry/backoff; logs result.
            //
            // Options:
            //   --ticket/-t <id>   REQUIRED, ticket to modify
            //   --sl <price>       New Stop Loss price
            //   --tp <price>       New Take Profit price
            //   --symbol/-s <sym>  Optional, ensure visibility (e.g., EURUSD)
            //
            // Examples:
            //   mt5cli modify -t 123456 --sl 1.0750
            //   mt5cli modify -t 123456 --tp 1.0900
            //   mt5cli modify -t 123456 --sl 1.0750 --tp 1.0900 -s EURUSD
            //   mt5cli modify -t 123456 --sl 1.0750 --dry-run
            var modify = new Command("modify", "Modify StopLoss / TakeProfit by ticket");
modify.AddAlias("m");

var modTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Ticket to modify") { IsRequired = true };
var modSlOpt     = new Option<double?>("--sl", "New Stop Loss (price)");
var modTpOpt     = new Option<double?>("--tp", "New Take Profit (price)");
// optional symbol: some servers are picky and require symbol visibility in terminal
var modSymbolOpt = new Option<string?>(
    new[] { "--symbol", "-s" },
    description: "Symbol (optional; used to ensure visibility if needed)");

modify.AddOption(profileOpt);
modify.AddOption(modTicketOpt); 
modify.AddOption(modSlOpt);
modify.AddOption(modTpOpt);
modify.AddOption(modSymbolOpt);

modify.SetHandler(async (string profile, ulong ticket, double? sl, double? tp, string? symbol, int timeoutMs, bool dryRun) =>
{
    // ------------------- fast validation -------------------
    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    if (sl is null && tp is null)
        throw new ArgumentException("At least one of --sl or --tp must be provided.");

    string? s = symbol;
    if (!string.IsNullOrWhiteSpace(s))
        s = Validators.EnsureSymbol(s);

    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:MODIFY Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} SL:{SL} TP:{TP}", ticket, sl, tp))
    using (_logger.BeginScope("Symbol:{Symbol}", s))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] MODIFY #{ticket} SL={(sl?.ToString() ?? "-")} TP={(tp?.ToString() ?? "-")}{(s is null ? "" : $" ({s})")}");
            return;
        }

        try
        {
            await ConnectAsync();

            // Best-effort: ensure symbol is visible if provided
            if (!string.IsNullOrWhiteSpace(s))
            {
                try
                {
                    using var visCts = StartOpCts();
                    await _mt5Account.EnsureSymbolVisibleAsync(
                        s!, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
                }
            }

            using var opCts = StartOpCts();
await CallWithRetry(ct => ModifyStopsSmartAsync(ticket, sl, tp, ct), opCts.Token);

            _logger.LogInformation("MODIFY done: ticket={Ticket} SL={SL} TP={TP}", ticket, sl, tp);
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
}, profileOpt, modTicketOpt, modSlOpt, modTpOpt, modSymbolOpt, timeoutOpt, dryRunOpt);

root.AddCommand(modify);


            //======================================
            //==---------- pending.modify --------==
            //======================================


// pending.modify (alias: pm) — change a pending order by ticket
// Purpose: modify entry/trigger/limit price, SL/TP and TIF/expiry for an existing pending order.
// Behavior:
//   - Supports types: buylimit|selllimit|buystop|sellstop|buystoplimit|sellstoplimit (optional --type is for client-side validation).
//   - For limit/stop: require --price > 0.
//   - For stop-limit: require BOTH --stop and --limit; do NOT pass --price.
//     * buystoplimit  ⇒ --limit <= --stop
//     * sellstoplimit ⇒ --limit >= --stop
//   - TIF: --tif GTC|DAY|GTD; when GTD, --expire (ISO-8601) is required.
//   - Optional --symbol is only used to ensure the symbol is visible in the terminal.
//   - Respects global --timeout-ms and --dry-run, uses ModifyPendingOrderAsync with retry/backoff.
//
// Options:
//   --ticket/-t <id>           REQUIRED pending order ticket
//   --type <kind>              buylimit|selllimit|buystop|sellstop|buystoplimit|sellstoplimit (optional)
//   --price <price>            New entry price (limit/stop only)
//   --stop  <price>            Trigger price (stop/stop-limit)
//   --limit <price>            Limit leg (stop-limit)
//   --sl <price>               Stop Loss (absolute)
//   --tp <price>               Take Profit (absolute)
//   --tif <GTC|DAY|GTD>        Time-in-force
//   --expire <ISO-8601>        Expiry when --tif=GTD
//   --symbol/-s <sym>          Optional, ensure visibility (e.g., EURUSD)
//
// Examples:
//   pm -t 101 --type buylimit     --price 1.0750 --sl 1.0700 --tp 1.0820 --tif DAY
//   pm -t 202 --type buystop      --price 1.0760 --tif GTD --expire 2025-09-01T12:00:00Z
//   pm -t 303 --type buystoplimit --stop 1.0760 --limit 1.0755 --sl 1.0720 --tp 1.0830
//   pm -t 404 --type sellstoplimit --stop 1.0740 --limit 1.0745 --dry-run
            var pmTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Pending order ticket") { IsRequired = true };
var pmTypeOpt   = new Option<string?>(new[] { "--type" }, "buylimit|selllimit|buystop|sellstop|buystoplimit|sellstoplimit (optional, for validation)");
var pmPriceOpt  = new Option<double?>(new[] { "--price" }, "New entry price for limit/stop");
var pmStopOpt   = new Option<double?>(new[] { "--stop" }, "New trigger price for stop/stop-limit");
var pmLimitOpt  = new Option<double?>(new[] { "--limit" }, "New limit price for stop-limit");
var pmSlOpt     = new Option<double?>(new[] { "--sl" }, "New Stop Loss (absolute)");
var pmTpOpt     = new Option<double?>(new[] { "--tp" }, "New Take Profit (absolute)");
var pmTifOpt    = new Option<string?>(new[] { "--tif" }, "GTC|DAY|GTD");
var pmExpireOpt = new Option<DateTimeOffset?>(new[] { "--expire" }, "Expiry (ISO-8601) when --tif=GTD");

var pendingModify = new Command("pending.modify", "Modify a pending order (price/stop-limit/SL/TP/expiry)");
pendingModify.AddAlias("pm");

pendingModify.AddOption(profileOpt);
pendingModify.AddOption(symbolOpt);
pendingModify.AddOption(pmTicketOpt);
pendingModify.AddOption(pmTypeOpt);
pendingModify.AddOption(pmPriceOpt);
pendingModify.AddOption(pmStopOpt);
pendingModify.AddOption(pmLimitOpt);
pendingModify.AddOption(pmSlOpt);
pendingModify.AddOption(pmTpOpt);
pendingModify.AddOption(pmTifOpt);
pendingModify.AddOption(pmExpireOpt);

pendingModify.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var symbolArg = ctx.ParseResult.GetValueForOption(symbolOpt);
    var ticket    = ctx.ParseResult.GetValueForOption(pmTicketOpt);
    var typeStr   = ctx.ParseResult.GetValueForOption(pmTypeOpt);
    var price     = ctx.ParseResult.GetValueForOption(pmPriceOpt);
    var stop      = ctx.ParseResult.GetValueForOption(pmStopOpt);
    var limit     = ctx.ParseResult.GetValueForOption(pmLimitOpt);
    var sl        = ctx.ParseResult.GetValueForOption(pmSlOpt);
    var tp        = ctx.ParseResult.GetValueForOption(pmTpOpt);
    var tifStr    = ctx.ParseResult.GetValueForOption(pmTifOpt);
    var expire    = ctx.ParseResult.GetValueForOption(pmExpireOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    _ = ticket;

    var s = symbolArg ?? GetOptions().DefaultSymbol;
    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:PENDING.MODIFY Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket}", ticket))
    using (_logger.BeginScope("Params Type:{Type} Price:{Price} Stop:{Stop} Limit:{Limit} SL:{SL} TP:{TP} TIF:{TIF} Exp:{Exp}",
                              typeStr, price, stop, limit, sl, tp, tifStr, expire))
    {
        if (!string.IsNullOrWhiteSpace(typeStr))
        {
            var k = typeStr.Replace("-", "").Replace(".", "").Trim().ToLowerInvariant();
            var isStopLimit = k is "buystoplimit" or "sellstoplimit";
            var isLimitOrStop = k is "buylimit" or "selllimit" or "buystop" or "sellstop";

            if (isStopLimit)
            {
                if (!stop.HasValue || !limit.HasValue)
                    throw new ArgumentException("Stop-limit modify requires both --stop and --limit.");
                if (k == "buystoplimit" && !(limit!.Value <= stop!.Value))
                    throw new ArgumentException("Buy Stop Limit modify requires --limit <= --stop.");
                if (k == "sellstoplimit" && !(limit!.Value >= stop!.Value))
                    throw new ArgumentException("Sell Stop Limit modify requires --limit >= --stop.");
                if (price.HasValue)
                    throw new ArgumentException("Do not pass --price for stop-limit modify. Use --stop and --limit.");
            }
            else if (isLimitOrStop)
            {
                if (!price.HasValue || price.Value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(pmPriceOpt), "For limit/stop modify, --price must be > 0.");
            }
        }

        if (!string.IsNullOrWhiteSpace(tifStr) && tifStr!.ToUpperInvariant() == "GTD" && !expire.HasValue)
            throw new ArgumentException("When --tif=GTD is used, --expire must be provided.");

        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] PM ticket={ticket} type={typeStr} price={price} stop={stop} limit={limit} SL={sl} TP={tp} TIF={tifStr} expire={expire}");
            return;
        }

        try
        {
            await ConnectAsync();

            if (!string.IsNullOrWhiteSpace(s))
            {
                try
                {
                    using var visCts = StartOpCts();
                    await _mt5Account.EnsureSymbolVisibleAsync(s!, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
                }
            }

            using var opCts = StartOpCts();
            var ok = await CallWithRetry(
                ct => _mt5Account.ModifyPendingOrderAsync(
                    ticket: ticket,
                    type: typeStr, 
                    price: price,
                    stop: stop,
                    limit: limit,
                    sl: sl,
                    tp: tp,
                    tif: tifStr,
                    expire: expire,
                    ct: ct),
                opCts.Token);

            Console.WriteLine(ok ? "✔ pending.modify done" : "✖ pending.modify failed");
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
});

root.AddCommand(pendingModify);


            //====================================
            //==--------- pending.move ---------==
            //====================================


            // pending.move (alias: pmove) — shift a pending order by ±N points
            // Purpose: move entry price (and trigger for stop-limit) by N * SymbolPoint.
            // Requirements: --ticket, --by-points (signed, non-zero). Symbol is auto-read from the order.
            // Behavior:
            //   • Computes delta = byPoints × pointSize (uses SDK point; falls back to JPY=0.01, metals=0.1, else 0.0001).
            //   • Applies the same delta to Price; if present, also to PriceTriggerStopLimit.
            //   • Dry-run prints old → new values; no RPC sent.
            //   • Uses MovePendingByPointsAsync(ticket, symbol, byPoints) with retry/timeout.
            //
            // Options:
            //   --ticket/-t <id>          REQUIRED pending order ticket
            //   --by-points/-P <±N>       REQUIRED signed shift in POINTS (e.g., +15, -20)
            //   --timeout-ms <ms>         Per-op timeout (global)
            //   --dry-run                 Preview only
            //
            // Examples:
            //   pmove -t 12345 -P +15
            //   pmove -t 67890 -P -25 --dry-run
            var pmByPtsOpt  = new Option<int>(new[] { "--by-points", "-P" }, "Shift by points (signed, e.g. +15 or -20)") { IsRequired = true };

var pendingMove = new Command("pending.move", "Move a pending order price(s) by ±N points");
pendingMove.AddAlias("pmove");

pendingMove.AddOption(profileOpt);
pendingMove.AddOption(pmTicketOpt);
pendingMove.AddOption(pmByPtsOpt);
pendingMove.AddOption(timeoutOpt);
pendingMove.AddOption(dryRunOpt);

pendingMove.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(pmTicketOpt);
    var byPoints  = ctx.ParseResult.GetValueForOption(pmByPtsOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    if (byPoints == 0) { Console.WriteLine("Nothing to do: by-points is 0."); return; }

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:PENDING.MOVE Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} ByPoints:{By}", ticket, byPoints))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            var opened = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                opCts.Token);

            var ordObj = TryFindByTicketInAggregate(opened, ticket, out var bucket);
            if (ordObj is null)
            {
                Console.WriteLine($"Pending order #{ticket} not found.");
                Environment.ExitCode = 2;
                return;
            }

            if (!string.IsNullOrEmpty(bucket) &&
                bucket.IndexOf("order", StringComparison.OrdinalIgnoreCase) < 0)
            {
                Console.WriteLine($"Ticket #{ticket} refers to the '{bucket}', not the pending order.");
                Environment.ExitCode = 2;
                return;
            }

            var symbol = Get<string>(ordObj, "Symbol");
            if (string.IsNullOrWhiteSpace(symbol))
            {
                Console.WriteLine($"Cannot read Symbol for pending order #{ticket}.");
                Environment.ExitCode = 2;
                return;
            }

            var point = _mt5Account.PointGuess(symbol);
            if (point <= 0)
                point = symbol.EndsWith("JPY", StringComparison.OrdinalIgnoreCase) ? 0.01 : 0.0001;

            var delta = byPoints * point;

            var priceProp = ordObj.GetType().GetProperty("Price");
            if (priceProp is null)
            {
                Console.WriteLine("Order object doesn't have 'Price' property.");
                Environment.ExitCode = 2;
                return;
            }

            double oldPrice = Convert.ToDouble(priceProp.GetValue(ordObj) ?? 0.0, CultureInfo.InvariantCulture);

            var trigProp = ordObj.GetType().GetProperty("PriceTriggerStopLimit");
            double? oldTrig = trigProp is null
                ? (double?)null
                : Convert.ToDouble(trigProp.GetValue(ordObj) ?? 0.0, CultureInfo.InvariantCulture);

            if (dryRun)
            {
                Console.WriteLine($"[DRY-RUN] PENDING.MOVE #{ticket} {symbol}:");
                Console.WriteLine($"  Price:   {oldPrice} -> {oldPrice + delta}");
                if (oldTrig is not null)
                    Console.WriteLine($"  Trigger: {oldTrig.Value} -> {oldTrig.Value + delta}");
                return;
            }

            await CallWithRetry(
                ct => _mt5Account.MovePendingByPointsAsync(ticket, symbol, byPoints, ct),
                opCts.Token);

            Console.WriteLine("✔ pending.move done");
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
});


root.AddCommand(pendingMove);


            //====================================
            //==----------- stream -------------==
            //====================================


            // stream (alias: st) — subscribe to ticks/trade events with auto-reconnect
            // Purpose: run short-lived streaming session for a symbol, auto-reconnecting on transient gRPC errors.
            // Behavior:
            //   • Ensures symbol visibility (best-effort) then calls StreamWithReconnectAsync(symbol, duration).
            //   • Stops after --seconds or external cancel; logs start/stop and elapsed time.
            //   • Respects per-op timeout (--timeout-ms) for internal RPCs.
            // Options:
            //   --profile/-p <name>     Profile from profiles.json (required upstream)
            //   --seconds/-S <n>        Duration in seconds (default 10; > 0)
            //   --symbol/-s <SYM>       Symbol to stream (defaults to profile/appsettings)
            //   --timeout-ms <ms>       Per-RPC timeout override
            // Examples:
            //   stream -S 15 -s EURUSD
            //   st --seconds 30 --symbol XAUUSD --timeout-ms 8000
            var secsOpt = new Option<int>(new[] { "--seconds", "-S" }, () => 10, "How many seconds to listen to streams");
            var stream = new Command("stream", "Subscribe to trading events/ticks (auto-reconnect)");
            stream.AddAlias("st");

stream.AddOption(profileOpt);
stream.AddOption(secsOpt);
stream.AddOption(symbolOpt);

stream.SetHandler(async (string profile, int seconds, string? symbol, int timeoutMs) =>
{
    Validators.EnsureProfile(profile);
    if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be > 0.");

    var s = Validators.EnsureSymbol(symbol ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:STREAM Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol} Seconds:{Seconds}", s, seconds))
    {
        var startedAt = DateTime.UtcNow;

        try
        {
            await ConnectAsync();

            using var streamCts = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
            _logger.LogInformation("Streaming started (auto-reconnect enabled).");

            await StreamWithReconnectAsync(s, TimeSpan.FromSeconds(seconds), streamCts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Streaming cancelled (likely reached time limit).");
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            var elapsed = DateTime.UtcNow - startedAt;
            _logger.LogInformation("Streaming stopped. Elapsed={ElapsedSec:F1}s", elapsed.TotalSeconds);
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
}, profileOpt, secsOpt, symbolOpt, timeoutOpt);

root.AddCommand(stream);


            //====================================
            //==----------- positions ----------==
            //====================================


            // positions (alias: pos) — list active positions
            // Summary:
            //   Connects, fetches snapshot via OpenedOrdersAsync(), and prints PositionInfos.
            // Output:
            //   --output json  → dumps the full response object;
            //   default text   → prints count + first 10 rows: symbol, ticket, vol, open, pnl.
            // Timeouts/Retry:
            //   Uses per-RPC timeout (--timeout-ms) and CallWithRetry(); always disconnects on exit.
            // Options:
            //   --profile/-p <name>   Profile to use
            //   --output/-o text|json Output format (default: text)
            //   --timeout-ms <ms>     Per-RPC timeout override
            // Examples:
            //   positions -p default
            //   pos --output json --timeout-ms 8000
            var positions = new Command("positions", "List active positions");
    positions.AddAlias("pos");

    positions.AddOption(profileOpt);
    positions.AddOption(outputOpt);
    positions.SetHandler(async (string profile, string output, int timeoutMs) =>
    {
        Validators.EnsureProfile(profile);
        _selectedProfile = profile;

        using (UseOpTimeout(timeoutMs))
        using (_logger.BeginScope("Cmd:POSITIONS Profile:{Profile}", profile))
        {
            try
            {
                await ConnectAsync();
                using var opCts = StartOpCts();

                var opened = await CallWithRetry(
                    ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                    opCts.Token);

                if (IsJson(output)) Console.WriteLine(ToJson(opened));
                else
                {
                    var list = opened.PositionInfos;
                    Console.WriteLine($"Positions: {list.Count}");
                    foreach (var p in list.Take(10))
                        Console.WriteLine($"{p.Symbol}  #{p.Ticket}  vol={p.Volume}  open={p.PriceOpen}  pnl={p.Profit}");
                    if (list.Count > 10) Console.WriteLine($"... and {list.Count - 10} more");
                }
            }
            catch (Exception ex)
            {
                ErrorPrinter.Print(_logger, ex, IsDetailed());
                Environment.ExitCode = 1;
            }
            finally
            {
                try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
            }
        }
    }, profileOpt, outputOpt, timeoutOpt);
    root.AddCommand(positions);


            //====================================
            //==------------- orders -----------==
            //====================================


            // orders (alias: ord) — list IDs of open pendings and positions
            // Summary:
            //   Connects and calls OpenedOrdersTicketsAsync(); prints ticket lists for
            //   pending orders and open positions.
            // Output:
            //   --output json  → dumps full DTO (OpenedOrdersTickets/OpenedPositionTickets)
            //   default text   → prints counts and up to first 20 IDs (then "...").
            // Timeouts/Retry:
            //   Respects --timeout-ms (StartOpCts) and uses CallWithRetry(). Always disconnects.
            // Options:
            //   --profile/-p <name>   Profile to use
            //   --output/-o text|json Output format (default: text)
            //   --timeout-ms <ms>     Per-RPC timeout override
            // Examples:
            //   orders -p default
            //   ord --output json --timeout-ms 7000
            var orders = new Command("orders", "List open orders and positions tickets");
    orders.AddAlias("ord");

    orders.AddOption(profileOpt);
    orders.AddOption(outputOpt);
    orders.SetHandler(async (string profile, string output, int timeoutMs) =>
    {
        Validators.EnsureProfile(profile);
        _selectedProfile = profile;

        using (UseOpTimeout(timeoutMs))
        using (_logger.BeginScope("Cmd:ORDERS Profile:{Profile}", profile))
        {
            try
            {
                await ConnectAsync();
                using var opCts = StartOpCts();

                var tickets = await CallWithRetry(
                    ct => _mt5Account.OpenedOrdersTicketsAsync(deadline: null, cancellationToken: ct),
                    opCts.Token);

                if (IsJson(output)) Console.WriteLine(ToJson(tickets));
                else
                {
                    var o = tickets.OpenedOrdersTickets;
                    var p = tickets.OpenedPositionTickets;

                    Console.WriteLine($"Opened orders:   {o.Count}");
                    if (o.Count > 20) Console.WriteLine(string.Join(", ", o.Take(20)) + " ...");
                    else if (o.Count > 0) Console.WriteLine(string.Join(", ", o));

                    Console.WriteLine($"Opened positions:{p.Count}");
                    if (p.Count > 20) Console.WriteLine(string.Join(", ", p.Take(20)) + " ...");
                    else if (p.Count > 0) Console.WriteLine(string.Join(", ", p));
                }
            }
            catch (Exception ex)
            {
                ErrorPrinter.Print(_logger, ex, IsDetailed());
                Environment.ExitCode = 1;
            }
            finally
            {
                try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
            }
        }
    }, profileOpt, outputOpt, timeoutOpt);
    root.AddCommand(orders);


            //====================================
            //==------------ pending -----------==
            //====================================


            // pending list (aliases: pd ls) — list IDs of pending orders
            // Summary:
            //   Connects and calls OpenedOrdersTicketsAsync(); prints only pending-order
            //   tickets (OpenedOrdersTickets). Position tickets are ignored here.
            // Output:
            //   --output json  → {"count": N, "tickets": [...]} 
            //   default text   → "Pending orders: N" and up to first 20 IDs (then "...").
            // Timeouts/Retry:
            //   Respects --timeout-ms (StartOpCts) and uses CallWithRetry(). Always disconnects.
            // Options:
            //   --profile/-p <name>   Profile to use
            //   --output/-o text|json Output format (default: text)
            //   --timeout-ms <ms>     Per-RPC timeout override
            // Examples:
            //   pending list -p default
            //   pd ls --output json --timeout-ms 7000
            var pending = new Command("pending", "Pending orders utilities");
pending.AddAlias("pd");

// pending list
var pendingList = new Command("list", "List pending order tickets");
pendingList.AddAlias("ls");

pendingList.AddOption(profileOpt);
pendingList.AddOption(outputOpt);

// we reuse the global timeout option added earlier
pendingList.SetHandler(async (string profile, string output, int timeoutMs) =>
{
    Validators.EnsureProfile(profile);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:PENDING/LIST Profile:{Profile}", profile))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            // This call returns both pending-order tickets and open-position tickets
            var tickets = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersTicketsAsync(deadline: null, cancellationToken: ct),
                opCts.Token);

            var pendingTickets  = tickets.OpenedOrdersTickets;     // pending orders
            // var positionTickets = tickets.OpenedPositionTickets; // not used here

            if (IsJson(output))
            {
                var dto = new
                {
                    count   = pendingTickets.Count,
                    tickets = pendingTickets.ToArray()
                };
                Console.WriteLine(ToJson(dto));
            }
            else
            {
                Console.WriteLine($"Pending orders: {pendingTickets.Count}");
                if (pendingTickets.Count > 20)
                    Console.WriteLine(string.Join(", ", pendingTickets.Take(20)) + " ...");
                else if (pendingTickets.Count > 0)
                    Console.WriteLine(string.Join(", ", pendingTickets));
            }
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
}, profileOpt, outputOpt, timeoutOpt);

pending.AddCommand(pendingList);
root.AddCommand(pending);


            //====================================
            //==----------- history ------------==
            //====================================


            // history (alias: h) — fetch orders/deals history for the last N days
            // Summary:
            //   Connects and calls MT5Account.OrderHistoryAsync(from, to).
            //   Prints a compact preview of up to 10 items (ORDER/DEAL) in text mode;
            //   or dumps the full response in JSON mode.
            // Options:
            //   --profile/-p <name>   Profile to use
            //   --output/-o text|json Output format (default: text)
            //   --days/-d <N>         Look back N days (must be > 0)
            //   --timeout-ms <ms>     Per-RPC timeout override
            // Behavior:
            //   Uses StartOpCts() + CallWithRetry(); always disconnects in finally.
            //   Text mode shows count and first 10 entries; JSON prints full payload.
            // Examples:
            //   history -d 7
            //   h --days 30 --output json --timeout-ms 7000
            var history = new Command("history", "Orders/deals history for the last N days");
    history.AddAlias("h");

    history.AddOption(profileOpt);
    history.AddOption(outputOpt);
    history.AddOption(daysOpt);
    history.SetHandler(async (string profile, string output, int days, int timeoutMs) =>
    {
        Validators.EnsureProfile(profile);
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "Days must be > 0.");
        _selectedProfile = profile;

        using (UseOpTimeout(timeoutMs))
        using (_logger.BeginScope("Cmd:HISTORY Profile:{Profile}", profile))
        using (_logger.BeginScope("Days:{Days}", days))
        {
            try
            {
                await ConnectAsync();
                using var opCts = StartOpCts();

                var from = DateTime.UtcNow.AddDays(-Math.Abs(days));
                var to   = DateTime.UtcNow;

                var res = await CallWithRetry(
                    ct => _mt5Account.OrderHistoryAsync(from, to, deadline: null, cancellationToken: ct),
                    opCts.Token);

                if (IsJson(output)) Console.WriteLine(ToJson(res));
                else
                {
                    var items = res.HistoryData;
                    Console.WriteLine($"History items: {items.Count}");
                    foreach (var h in items.Take(10))
                    {
                        if (h.HistoryOrder is not null)
                        {
                            var o = h.HistoryOrder;
                            var setup = o.SetupTime?.ToDateTime();
                            var done  = o.DoneTime?.ToDateTime();
                            Console.WriteLine(
                                $"ORDER  #{o.Ticket}  {o.Symbol}  state={o.State}  vol={o.VolumeInitial}->{o.VolumeCurrent}  open={o.PriceOpen} " +
                                $"setup={setup:O} done={done:O}");
                        }
                        else if (h.HistoryDeal is not null)
                        {
                            var d = h.HistoryDeal;
                            var t = d.Time?.ToDateTime();
                            Console.WriteLine(
                                $"DEAL   #{d.Ticket}  {d.Symbol}  type={d.Type}  vol={d.Volume}  price={d.Price}  pnl={d.Profit}  time={t:O}");
                        }
                    }
                    if (items.Count > 10) Console.WriteLine($"... and {items.Count - 10} more");
                }
            }
            catch (Exception ex)
            {
                ErrorPrinter.Print(_logger, ex, IsDetailed());
                Environment.ExitCode = 1;
            }
            finally
            {
                try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
            }
        }
    }, profileOpt, outputOpt, daysOpt, timeoutOpt);
    root.AddCommand(history);


            //====================================
            //==----------- lot.calc -----------==
            //====================================


            // lot.calc (alias: lc) — compute position volume from risk % and SL distance (in POINTS).
            // Formula:
            //   vol_raw = (balance * riskPct/100) / (slPoints * pointValuePerLot)
            //   vol     = round_to_step(vol_raw, lotStep) clamped to [minLot .. maxLot?]
            // Uses MT5Account.EstimatePointValuePerLotAsync(symbol) for 1-lot point value.
            //
            // Options:
            //   --symbol/-s <SYM>        Symbol (default from config)
            //   --risk-pct <X>           Risk percent of balance (0<X≤100) [required]
            //   --sl-points <N>          Stop-loss distance in POINTS (>0) [required]
            //   --balance <amt>          Account balance (>0) [required]
            //   --min-lot <v>            Min lot (default 0.01)
            //   --lot-step <v>           Lot step (default 0.01)
            //   --max-lot <v?>           Max lot cap (optional)
            //   --timeout-ms <ms>        Per-RPC timeout override
            //   --output/-o text|json    Output format (default text)
            //
            // Output:
            //   text  → short human summary (risk $, SL pts, 1-lot point value, raw & final volume)
            //   json  → structured payload with inputs and computed fields
            //
            // Notes:
            //   • Validates inputs; throws on non-positive values or bad ranges.
            //   • Rounding uses MidpointRounding.AwayFromZero.
            //   • Does not auto-connect: only calls EstimatePointValuePerLotAsync(symbol).
            //
            // Examples:
            //   lc --symbol EURUSD --risk-pct 1 --sl-points 200 --balance 5000
            //   lot.calc -s XAUUSD --risk-pct 0.5 --sl-points 150 --balance 10000 --lot-step 0.1 --min-lot 0.1 --max-lot 5 --output json
            var lcSymbolOpt   = new Option<string>(new[] { "--symbol", "-s" }, () => GetOptions().DefaultSymbol, "Symbol");
var lcRiskPctOpt  = new Option<double>(new[] { "--risk-pct" }, "Risk percent of balance (e.g., 1 for 1%)") { IsRequired = true };
var lcSlPtsOpt    = new Option<int>(new[] { "--sl-points" }, "Stop-loss distance in POINTS") { IsRequired = true };
var lcBalanceOpt  = new Option<double>(new[] { "--balance" }, "Account balance (same currency as risk)") { IsRequired = true };
var lcMinLotOpt   = new Option<double>(new[] { "--min-lot" }, () => 0.01, "Min lot size");
var lcStepLotOpt  = new Option<double>(new[] { "--lot-step" }, () => 0.01, "Lot step");
var lcMaxLotOpt   = new Option<double?>(new[] { "--max-lot" }, "Max lot size cap (optional)");

var lotCalc = new Command("lot.calc", "Calculate position volume by risk % and SL distance (points)");
lotCalc.AddAlias("lc");

lotCalc.AddOption(lcSymbolOpt);
lotCalc.AddOption(lcRiskPctOpt);
lotCalc.AddOption(lcSlPtsOpt);
lotCalc.AddOption(lcBalanceOpt);
lotCalc.AddOption(lcMinLotOpt);
lotCalc.AddOption(lcStepLotOpt);
lotCalc.AddOption(lcMaxLotOpt);
lotCalc.AddOption(timeoutOpt);
lotCalc.AddOption(outputOpt);

lotCalc.SetHandler(async (InvocationContext ctx) =>
{
    var symbol   = Validators.EnsureSymbol(ctx.ParseResult.GetValueForOption(lcSymbolOpt)!);
    var riskPct  = ctx.ParseResult.GetValueForOption(lcRiskPctOpt);
    var slPoints = ctx.ParseResult.GetValueForOption(lcSlPtsOpt);
    var balance  = ctx.ParseResult.GetValueForOption(lcBalanceOpt);
    var minLot   = ctx.ParseResult.GetValueForOption(lcMinLotOpt);
    var lotStep  = ctx.ParseResult.GetValueForOption(lcStepLotOpt);
    var maxLot   = ctx.ParseResult.GetValueForOption(lcMaxLotOpt);
    var timeoutMs= ctx.ParseResult.GetValueForOption(timeoutOpt);
    var output   = (ctx.ParseResult.GetValueForOption(outputOpt) ?? "text").Trim().ToLowerInvariant();

    if (riskPct <= 0 || riskPct > 100) throw new ArgumentOutOfRangeException(nameof(riskPct), "Use (0;100].");
    if (slPoints <= 0) throw new ArgumentOutOfRangeException(nameof(slPoints));
    if (balance <= 0) throw new ArgumentOutOfRangeException(nameof(balance));
    if (minLot <= 0) throw new ArgumentOutOfRangeException(nameof(minLot));
    if (lotStep <= 0) throw new ArgumentOutOfRangeException(nameof(lotStep));
    if (maxLot is not null && maxLot <= 0) throw new ArgumentOutOfRangeException(nameof(maxLot));

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:LOT.CALC Symbol:{Symbol} Risk%:{Risk} SLpts:{SL} Bal:{Bal}", symbol, riskPct, slPoints, balance))
    {
        try
        {
            double pvPerPointPerLot = await _mt5Account.EstimatePointValuePerLotAsync(symbol, default);

            double riskAmt = balance * (riskPct / 100.0);
            double volRaw  = riskAmt / (slPoints * pvPerPointPerLot);

            double vol = Math.Max(minLot, RoundToStep(volRaw, lotStep));
            if (maxLot is not null) vol = Math.Min(vol, maxLot.Value);

            if (output == "json")
            {
                var payload = new
                {
                    symbol,
                    balance,
                    risk_pct = riskPct,
                    sl_points = slPoints,
                    point_value_per_lot = pvPerPointPerLot,
                    volume_raw = volRaw,
                    volume = vol,
                    lot_step = lotStep,
                    min_lot = minLot,
                    max_lot = maxLot
                };
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                    payload, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                Console.WriteLine($"Symbol: {symbol}");
                Console.WriteLine($"Risk:   {riskPct}% of {balance} = {riskAmt}");
                Console.WriteLine($"SL:     {slPoints} points");
                Console.WriteLine($"Point value (1 lot): {pvPerPointPerLot}");
                Console.WriteLine($"Volume raw: {volRaw}");
                Console.WriteLine($"Volume => {vol} (step {lotStep}, min {minLot}{(maxLot is null ? "" : $", max {maxLot}")})");
            }
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
    }

    static double RoundToStep(double value, double step)
    {
        var k = Math.Round(value / step, MidpointRounding.AwayFromZero);
        return k * step;
    }
});

root.AddCommand(lotCalc);


            //====================================
            //==--------- history.export -------==
            //====================================


            // history.export (alias: hexport) - exports the history (orders/transactions) for N days in CSV/JSON format.
            //
            // Parameters:
            // --days / - from <N> a moment ago (>0)
            //--character / -s <SYM> character filter (optional)
            // --in <csv|json> format (csv by default)
            // --file / - f <PATH> path to save (required)
            //--profile / -p, -- timeout-ms <ms>
            //
            // What does:
            // Calls MT5Account.ExportHistoryAsync(days, symbol, to) and writes the string to the file.
            // If the history RPC is not implemented, it returns a hint (NotSupportedException).
            //
            // Examples:
            // hexadecimal translation-d 30-s EURUSD-to csv-f eurusd_30d.csv
            // history.export -to json-f everything.json
            var heDaysOpt  = daysOpt;
var heSymbolOpt= new Option<string?>(new[] { "--symbol", "-s" }, "Filter by symbol (optional)");
var heToOpt    = new Option<string>(new[] { "--to" }, () => "csv", "csv|json");
var heFileOpt  = new Option<string>(new[] { "--file", "-f" }, "Output path") { IsRequired = true };

var histExport = new Command("history.export", "Export trading history (deals/orders) to CSV/JSON");
histExport.AddAlias("hexport");

histExport.AddOption(profileOpt);
histExport.AddOption(heDaysOpt);
histExport.AddOption(heSymbolOpt);
histExport.AddOption(heToOpt);
histExport.AddOption(heFileOpt);
histExport.AddOption(timeoutOpt);

histExport.SetHandler(async (InvocationContext ctx) =>
{
    var profile = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var days    = ctx.ParseResult.GetValueForOption(heDaysOpt);
    var symbol  = ctx.ParseResult.GetValueForOption(heSymbolOpt);
    var to      = (ctx.ParseResult.GetValueForOption(heToOpt) ?? "csv").Trim().ToLowerInvariant();
    var file    = ctx.ParseResult.GetValueForOption(heFileOpt)!;
    var timeout = ctx.ParseResult.GetValueForOption(timeoutOpt);

    if (to != "csv" && to != "json") throw new ArgumentException("Use --to csv|json");

    Validators.EnsureProfile(profile);
    if (!string.IsNullOrWhiteSpace(symbol)) _ = Validators.EnsureSymbol(symbol);

    using (UseOpTimeout(timeout))
    using (_logger.BeginScope("Cmd:HISTORY.EXPORT Profile:{Profile}", profile))
    using (_logger.BeginScope("Days:{Days} Symbol:{Symbol} To:{To} File:{File}", days, symbol ?? "<any>", to, file))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            var data = await CallWithRetry(
                ct => _mt5Account.ExportHistoryAsync(days, symbol, to, ct),
                opCts.Token);

            System.IO.File.WriteAllText(file, data);
            Console.WriteLine($"✔ history.export written to: {file}");
        }
        catch (NotSupportedException nse)
        {
            Console.WriteLine(nse.Message);
            Environment.ExitCode = 2;
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { }
        }
    }
});

root.AddCommand(histExport);


            //====================================
            //==------------ health ------------==
            //====================================


            // health (alias: ping) — quick connectivity & account diagnostics.
            //
            // Options:
            //   --profile/-p <name>   profile to use
            //   --output/-o <text|json>  output format (default text)
            //   --timeout-ms <ms>     per-RPC timeout
            //
            // What it does:
            //   • Collects profile/server info (accountId, server, host:port).
            //   • If Host/Port set — performs a best-effort TCP connect check (~3s).
            //   • Connects to terminal and queries AccountSummary (reports balance).
            //   • Prints a compact report (text or JSON). ExitCode=2 if terminal check fails.
            //
            // Examples:
            //   health -p default
            //   ping --output json
            var health = new Command("health", "Quick connectivity and account diagnostics");
    health.AddAlias("ping");
    
    health.AddOption(profileOpt);
    health.AddOption(outputOpt);
    health.SetHandler(async (string profile, string output, int timeoutMs) =>
    {
        Validators.EnsureProfile(profile);
        _selectedProfile = profile;

        using (UseOpTimeout(timeoutMs))
        using (_logger.BeginScope("Cmd:HEALTH Profile:{Profile}", profile))
        {
            var report = new Dictionary<string, object?>();

            try
            {
                var opt = GetEffectiveOptions(profile);
                report["profile"]    = profile;
                report["accountId"]  = opt.AccountId;
                report["serverName"] = opt.ServerName;
                report["host"]       = opt.Host;
                report["port"]       = opt.Port;

                if (!string.IsNullOrWhiteSpace(opt.Host) && opt.Port > 0)
                {
                    try
                    {
                        using var tcp = new System.Net.Sockets.TcpClient();
                        var connectTask = tcp.ConnectAsync(opt.Host, opt.Port);
                        var completed = await Task.WhenAny(connectTask, Task.Delay(3000));
                        if (completed != connectTask) throw new TimeoutException("TCP connect timeout");
                        report["tcp"] = "ok";
                        _logger.LogInformation("TCP check ok: {Host}:{Port}", opt.Host, opt.Port);
                    }
                    catch (Exception ex)
                    {
                        report["tcp"] = $"fail: {ex.Message}";
                        _logger.LogWarning("TCP check failed: {Msg}", ex.Message);
                    }
                }

                try
                {
                    await ConnectAsync();
                    using var opCts = StartOpCts();

                    var summary = await CallWithRetry(
                        ct => _mt5Account.AccountSummaryAsync(deadline: null, cancellationToken: ct),
                        opCts.Token);

                    report["terminal"] = "ok";
                    report["balance"]  = summary.AccountBalance;
                    _logger.LogInformation("Terminal ok. Balance={Balance}", summary.AccountBalance);
                }
                catch (Exception ex)
                {
                    report["terminal"] = $"fail: {ex.Message}";
                    _logger.LogWarning("Terminal check failed: {Msg}", ex.Message);
                    Environment.ExitCode = 2;
                }
                finally
                {
                    try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
                }

                if (IsJson(output)) Console.WriteLine(ToJson(report));
                else
                {
                    Console.WriteLine("Health report:");
                    foreach (var kv in report) Console.WriteLine($"  {kv.Key}: {kv.Value}");
                }
            }
            catch (Exception ex)
            {
                ErrorPrinter.Print(_logger, ex, IsDetailed());
                Environment.ExitCode = 1;
            }
        }
    }, profileOpt, outputOpt, timeoutOpt);
    root.AddCommand(health);

    // ------------------ return the root -------------------
    return root;
}


        // ConnectAsync — establish a terminal session using the selected profile.
        //
        // What it does:
        //   • Rebuilds MT5Account if missing or if AccountId/Password changed.
        //   • Uses a per-op CTS with _opTimeoutOverride ?? _rpcTimeout.
        //   • Connects by ServerName when provided; otherwise via Host/Port.
        //   • Supplies DefaultSymbol as baseChartSymbol, waits for TerminalIsAlive.
        //   • Logs connection mode and success; throws on failures.
        //
        // Notes:
        //   • API uses timeoutSeconds=30 in addition to the CTS deadline.
        //   • _selectedProfile must be set before calling.
        //   • Caller is responsible for handling exceptions and later DisconnectAsync().
  private async Task ConnectAsync()
{
    var options = GetEffectiveOptions(_selectedProfile);

    using (_logger.BeginScope("Profile:{Profile}", _selectedProfile))
    {
        // gRPC endpoint preference: GrpcServer > Host:Port > SDK default
        var grpc =
            !string.IsNullOrWhiteSpace(options.GrpcServer) ? options.GrpcServer :
            !string.IsNullOrWhiteSpace(options.Host)       ? $"https://{options.Host}:{options.Port}" :
            null;

        // Password must be non-null: options OR ENV
        string password = options.Password
            ?? Environment.GetEnvironmentVariable("MT5_PASSWORD")
            ?? throw new InvalidOperationException(
                "Password is not set. Put it in profiles.json or set MT5_PASSWORD environment variable.");

        // Recreate client if creds/endpoint changed
        if (_mt5Account is null
            || _mt5Account.User != options.AccountId
            || _mt5Account.Password != password
            || !string.Equals(_currentGrpc, grpc, StringComparison.OrdinalIgnoreCase))
        {
            
            _mt5Account = new MT5Account(options.AccountId, password, grpc, id: default, logger: _accLogger);
            _currentGrpc = grpc;
        }

        using (_logger.BeginScope("Grpc:{Grpc}", grpc ?? "(default)"))
        {
            
            using var connectCts = new CancellationTokenSource(_opTimeoutOverride ?? _rpcTimeout);

            var serverName = options.ServerName;
            if (!string.IsNullOrWhiteSpace(serverName))
            {
                _logger.LogInformation("Connecting by ServerName...");
                await _mt5Account.ConnectByServerNameAsync(
                    serverName: serverName,
                    baseChartSymbol: options.DefaultSymbol,
                    waitForTerminalIsAlive: true,
                    timeoutSeconds: 30,
                    deadline: null,
                    cancellationToken: connectCts.Token);
            }
            else
            {
                
                string host = options.Host
                    ?? throw new InvalidOperationException(
                        "Neither ServerName nor Host is set for the selected profile. Set 'ServerName' or 'Host'+'Port' in profiles.json.");

                _logger.LogInformation("Connecting by Host/Port...");
                await _mt5Account.ConnectByHostPortAsync(
                    host: host,
                    port: options.Port,
                    baseChartSymbol: options.DefaultSymbol,
                    waitForTerminalIsAlive: true,
                    timeoutSeconds: 30,
                    deadline: null,
                    cancellationToken: connectCts.Token);
            }
        }

        _logger.LogInformation("Connected. AccountId={AccountId}", options.AccountId);
    }
}

// --- Preflight helper: validate & normalize SL/TP vs current price ---
static void PreflightStops(
    bool isBuy,
    double bid,
    double ask,
    ref double? sl,
    ref double? tp,
    int? digits = null,          // e.g. 5 for EURUSD; if null -> no rounding
    double? stopLevelPoints = null, // broker stop level in points; if null -> skip check
    double? point = null)           // point size (e.g. 0.00001); used only if stopLevelPoints given
{
    // 1) Business rules: relative to current prices
    if (isBuy)
    {
        if (sl.HasValue && sl.Value >= bid)
            throw new ArgumentException($"Invalid SL for BUY: must be < Bid (Bid={bid}).");
        if (tp.HasValue && tp.Value <= ask)
            throw new ArgumentException($"Invalid TP for BUY: must be > Ask (Ask={ask}).");
    }
    else
    {
        if (sl.HasValue && sl.Value <= ask)
            throw new ArgumentException($"Invalid SL for SELL: must be > Ask (Ask={ask}).");
        if (tp.HasValue && tp.Value >= bid)
            throw new ArgumentException($"Invalid TP for SELL: must be < Bid (Bid={bid}).");
    }

    // 2) Broker StopLevel (min distance from price), if provided
    if (stopLevelPoints.HasValue && point.HasValue)
    {
        var minDist = stopLevelPoints.Value * point.Value;
        if (isBuy)
        {
            if (sl.HasValue && (bid - sl.Value) < minDist)
                throw new ArgumentException($"SL too close for BUY: min distance {minDist}");
            if (tp.HasValue && (tp.Value - ask) < minDist)
                throw new ArgumentException($"TP too close for BUY: min distance {minDist}");
        }
        else
        {
            if (sl.HasValue && (sl.Value - ask) < minDist)
                throw new ArgumentException($"SL too close for SELL: min distance {minDist}");
            if (tp.HasValue && (bid - tp.Value) < minDist)
                throw new ArgumentException($"TP too close for SELL: min distance {minDist}");
        }
    }

    // 3) Normalize to symbol precision (digits), if provided
    if (digits.HasValue)
    {
        double round(double v) => Math.Round(v, digits.Value, MidpointRounding.AwayFromZero);
        if (sl.HasValue) sl = round(sl.Value);
        if (tp.HasValue) tp = round(tp.Value);
    }
}


        // DoStreamingAsync — consume multiple MT5 streams concurrently until cancelled.
        //
        // Subscribes to (each in its own Task):
        //   • OnSymbolTickAsync([DefaultSymbol]) → logs Symbol/Ask per tick.
        //   • OnTradeAsync()                     → logs trade event receipt.
        //   • OnPositionProfitAsync(1000, true) → periodic/snapshot PnL updates.
        //   • OnPositionsAndPendingOrdersTicketsAsync(1000) → tickets updates.
        //
        // Behavior:
        //   • All streams honor the provided CancellationToken; method returns when it’s
        //     cancelled (outer StreamWithReconnectAsync handles backoff/reconnect).
        //   • The 5s Task.Delay is only a small grace period before awaiting the workers.
        //
        // Notes:
        //   • Passing `ct` to Task.Run only cancels scheduling; actual cancellation happens
        //     inside the awaited `await foreach (..., ct)` loops.
        //   • For better fault isolation, wrap each `await foreach` in try/catch so one
        //     stream error doesn’t cancel the others.
        //   • If you need a specific symbol (not DefaultSymbol), pass it in via method args.
        //
        // TODO (optional):
        //   • Add throttling/debouncing for tick logs.
        //   • Enrich logs with Bid/Time and trade details as needed.
        private async Task DoStreamingAsync(CancellationToken ct)
        {
            _logger.LogInformation("=== Streaming ===");

            var options = GetOptions();

            var tickTask = Task.Run(async () =>
            {
                await foreach (var tick in _mt5Account.OnSymbolTickAsync(new[] { options.DefaultSymbol }, ct))
                {
                    _logger.LogInformation("OnSymbolTickAsync: Symbol={Symbol} Ask={Ask}", tick.SymbolTick.Symbol, tick.SymbolTick.Ask);
                }
            }, ct);

            var tradeTask = Task.Run(async () =>
            {
                await foreach (var trade in _mt5Account.OnTradeAsync(ct))
                {
                    _logger.LogInformation("OnTradeAsync: Trade event received");
                }
            }, ct);

            var profitTask = Task.Run(async () =>
            {
                await foreach (var profit in _mt5Account.OnPositionProfitAsync(1000, true, ct))
                {
                    _logger.LogInformation("OnPositionProfitAsync: Update received");
                }
            }, ct);

            var ticketsTask = Task.Run(async () =>
            {
                await foreach (var tickets in _mt5Account.OnPositionsAndPendingOrdersTicketsAsync(1000, ct))
                {
                    _logger.LogInformation("OnPositionsAndPendingOrdersTicketsAsync: Update received");
                }
            }, ct);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
                await Task.WhenAll(tickTask, tradeTask, profitTask, ticketsTask);
            }
            catch (OperationCanceledException) { }

            _logger.LogInformation("Streaming stopped.");
        }
    }
}
