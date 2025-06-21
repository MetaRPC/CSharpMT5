using Grpc.Core;
using Grpc.Net.Client;
using mt5_term_api;
using System.Runtime.CompilerServices;


namespace MetaRPC.CSharpMT5;
<<<<<<< HEAD
internal class ErrorCodeMT5;
=======
>>>>>>> 5aa7869c868f92a5b508113c1cd0f927f128208f

/// <summary>
/// Represents an MT5 trading account connected via gRPC.
/// </summary>
internal class MT5Account
{
    /// <summary>
    /// Gets the MT5 user account number.
    /// </summary>
    public ulong User { get; }

    /// <summary>
    /// Gets the password for the user account.
    /// </summary>
    public string Password { get; }

    /// <summary>
    /// Gets the MT5 server host.
    /// </summary>
    public string Host { get; internal set; }

    /// <summary>
    /// Gets the the MT5 server port.
    /// </summary>
    public int Port { get; internal set; }

    /// <summary>
    /// Gets the the MT5 server port.
    /// </summary>
    public string ServerName { get; internal set; }
    /// <summary>
    /// 
    /// </summary>
    public string BaseChartSymbol { get; private set; }
    public int ConnectTimeoutSeconds { get;  set; }

    /// <summary>
    /// Gets the gRPC server address used to connect.
    /// </summary>
    public readonly string GrpcServer;

    /// <summary>
    /// Gets the gRPC channel used for communication.
    /// </summary>
    public readonly GrpcChannel GrpcChannel;

    /// <summary>
    /// Gets the gRPC client for connection operations.
    /// </summary>
    public readonly Connection.ConnectionClient ConnectionClient;

    /// <summary>
    /// Gets the gRPC client for subscription services.
    /// </summary>
    public readonly SubscriptionService.SubscriptionServiceClient SubscriptionClient;

    /// <summary>
    /// Gets the gRPC client for account helper operations.
    /// </summary>
    public readonly AccountHelper.AccountHelperClient AccountClient;

    /// <summary>
    /// Gets the gRPC client for trading helper operations.
    /// </summary>
    public readonly TradingHelper.TradingHelperClient TradeClient;

    /// <summary>
    /// Gets the gRPC client for market information queries.
    /// </summary>
    public readonly MarketInfo.MarketInfoClient MarketInfoClient;

    /// <summary>
    /// Gets the gRPC client for trading functions.
    /// </summary>
    public TradeFunctions.TradeFunctionsClient TradeFunctionsClient { get; }

    /// <summary>
    /// Gets the gRPC client for trading functions.
    /// </summary>
    public AccountInformation.AccountInformationClient AccountInformationClient { get; }

    /// <summary>
    /// Gets the unique identifier for the account instance.
    /// </summary>
    public Guid Id { get; private set; } = default;

    private bool Connected => Host is not null || ServerName is not null; 

    /// <summary>
    /// Initializes a new instance of the <see cref="MT5Account"/> class using credentials.
    /// </summary>
    /// <param name="user">The MT5 user account number.</param>
    /// <param name="password">The password for the user account.</param>
    /// <param name="grpcServer">The address of the gRPC server (optional).</param>
    /// <param name="id">An optional unique identifier for the account instance.</param>
    public MT5Account(ulong user, string password, string? grpcServer = null, Guid id = default)
    {
        User = user;
        Password = password;
        GrpcServer = grpcServer ?? "https://mt5.mrpc.pro:443";

        GrpcChannel = GrpcChannel.ForAddress(GrpcServer);
        ConnectionClient = new Connection.ConnectionClient(GrpcChannel);
        SubscriptionClient = new SubscriptionService.SubscriptionServiceClient(GrpcChannel);
        AccountClient = new AccountHelper.AccountHelperClient(GrpcChannel);
        TradeClient = new TradingHelper.TradingHelperClient(GrpcChannel);
        MarketInfoClient = new MarketInfo.MarketInfoClient(GrpcChannel);
        TradeFunctionsClient = new TradeFunctions.TradeFunctionsClient(GrpcChannel);
        AccountInformationClient = new AccountInformation.AccountInformationClient(GrpcChannel);

        Id = id;
    }

    async Task Reconnect(DateTime? deadline, CancellationToken cancellationToken)
    {
        if (ServerName == null)
            await ConnectByHostPortAsync(Host, Port, BaseChartSymbol, true, ConnectTimeoutSeconds, deadline, cancellationToken);
        else
            await ConnectByServerNameAsync(ServerName, BaseChartSymbol, true, ConnectTimeoutSeconds, deadline, cancellationToken);
    }
<<<<<<< HEAD
    
   
       

=======

    // Connect methods
>>>>>>> 5aa7869c868f92a5b508113c1cd0f927f128208f

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
        {
            headers = new Metadata { { "id", Id.ToString() } };
        }

        var res = await ConnectionClient.ConnectAsync(connectRequest, headers, deadline, cancellationToken);
        if (res.Error != null)
            throw new ApiExceptionMT5(res.Error);
        Host = host;
        Port = port;
        BaseChartSymbol = baseChartSymbol;
        ConnectTimeoutSeconds = timeoutSeconds;
        Id = Guid.Parse(res.Data.TerminalInstanceGuid);
    }

    /// <summary>
    /// Synchronously connects to the MT5 terminal.
    /// </summary>
    /// <param name="host">The MT5 server host.</param>
    /// <param name="port">The port the server listens on.</param>
    /// <param name="baseChartSymbol">Chart symbol to initialize the session.</param>
    /// <param name="waitForTerminalIsAlive">Wait for terminal readiness flag.</param>
    /// <param name="timeoutSeconds">Timeout duration in seconds.</param>
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
        {
            headers = new Metadata { { "id", Id.ToString() } };
        }

        var res = await ConnectionClient.ConnectExAsync(connectRequest, headers, deadline, cancellationToken);

        if (res.Error != null)
            throw new ApiExceptionMT5(res.Error);
        ServerName = serverName;
        BaseChartSymbol = baseChartSymbol;
        ConnectTimeoutSeconds = timeoutSeconds;
        Id = Guid.Parse(res.Data.TerminalInstanceGuid);
    }

    /// <summary>
    /// Synchronously connects to the MT5 terminal.
    /// </summary>
    /// <param name="serverName">MT5 server name from MT5 Terminal.</param>
    /// <param name="baseChartSymbol">Chart symbol to initialize the session.</param>
    /// <param name="waitForTerminalIsAlive">Wait for terminal readiness flag.</param>
    /// <param name="timeoutSeconds">Timeout duration in seconds.</param>
    public void ConnectByServerName(
        string serverName,
        string baseChartSymbol = "EURUSD",
        bool waitForTerminalIsAlive = true,
        int timeoutSeconds = 30)
    {
        ConnectByServerNameAsync(serverName, baseChartSymbol, waitForTerminalIsAlive, timeoutSeconds).GetAwaiter().GetResult();
    }

    //
    // Account helper methods --------------------------------------------------------------------------------------------------------
    //

    private Metadata GetHeaders()
    {
        return new Metadata { { "id", Id.ToString() } };
    }

    /// <summary>
    /// Gets the summary information for a trading account.
    /// </summary>
    /// <param name="accountId">The unique account identifier.</param>
    /// <param name="deadline">Optional deadline after which the request will be canceled if not completed.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the request.</param>
    /// <returns>The server's response containing account summary data.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected before calling this method.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails due to communication or protocol errors.</exception>
    /// <summary>
    /// Executes a unary gRPC call with automatic reconnection logic on recoverable errors.
    /// </summary>
    /// <typeparam name="T">The result type of the unary gRPC call.</typeparam>
    /// <param name="grpcCall">A delegate that executes the gRPC call with generated headers.</param>
    /// <param name="errorSelector">A delegate that extracts the API error from the result, if any.</param>
    /// <param name="deadline">Optional deadline for the gRPC call.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The gRPC call result if successful; otherwise, an exception is thrown.</returns>
    /// <exception cref="ConnectExceptionMT5">If client is not connected before execution.</exception>
    /// <exception cref="ApiExceptionMT5">If server response contains a known API error.</exception>
    /// <exception cref="RpcException">If gRPC fails with a non-recoverable error.</exception>
    private async Task<T> ExecuteWithReconnect<T>(
        Func<Metadata, T> grpcCall,
        Func<T, Mt5TermApi.Error?> errorSelector,
        DateTime? deadline,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var headers = GetHeaders();
            T res;

            try
            {
                res = grpcCall(headers);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable) // || ex.StatusCode == StatusCode.Internal
            {
                await Task.Delay(500, cancellationToken);
                await Reconnect(deadline, cancellationToken);
                continue; // Retry after reconnect
            }

            var error = errorSelector(res);

            if (error?.ErrorCode == "TERMINAL_INSTANCE_NOT_FOUND" ||
                error?.ErrorCode == "TERMINAL_REGISTRY_TERMINAL_NOT_FOUND")
            {
                await Task.Delay(500, cancellationToken);
                await Reconnect(deadline, cancellationToken);
                continue; // Retry after reconnect
            }

            if (error != null)
                throw new ApiExceptionMT5(error);

            return res; // Success
        }

        throw new OperationCanceledException("The operation was canceled by the caller.");
    }

    public async Task<AccountSummaryData> AccountSummaryAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    {
        if (!Connected)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new AccountSummaryRequest();

        var res = await ExecuteWithReconnect(
            headers => AccountClient.AccountSummary(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken
        );

        return res.Data;
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

        var request = new OpenedOrdersRequest { InputSortMode = sortMode };

        var res = await ExecuteWithReconnect(
            headers => AccountClient.OpenedOrders(request, headers, deadline, cancellationToken),
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
            headers => AccountClient.OrderHistory(request, headers, deadline, cancellationToken),
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
            headers => AccountClient.OpenedOrdersTickets(request, headers, deadline, cancellationToken),
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
            headers => AccountClient.SymbolParamsMany(request, headers, deadline, cancellationToken),
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
            headers => AccountClient.TickValueWithSize(request, headers, deadline, cancellationToken),
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
            headers => AccountClient.PositionsHistory(request, headers, deadline, cancellationToken),
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


    //
    // Trading helper methods --------------------------------------------------------------------------------------------------------
    //

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
            headers => TradeClient.OrderSend(request, headers, deadline, cancellationToken),
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
           headers => TradeClient.OrderModify(request, headers, deadline, cancellationToken),
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
            headers => TradeClient.OrderClose(request, headers, deadline, cancellationToken),
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


    //
    // Streams --------------------------------------------------------------------------------------------------------
    //

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
                stream = streamInvoker(request, GetHeaders(), cancellationToken);
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

                    if (error!= null)
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
            (req, headers, ct) => SubscriptionClient.OnSymbolTick(req, headers, cancellationToken: ct),
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
            (req, headers, ct) => SubscriptionClient.OnTrade(req, headers, cancellationToken: ct),
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
            (req, headers, ct) => SubscriptionClient.OnPositionProfit(req, headers, cancellationToken: ct),
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
            (req, headers, ct) => SubscriptionClient.OnPositionsAndPendingOrdersTickets(req, headers, cancellationToken: ct),
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
            (req, headers, ct) => SubscriptionClient.OnTradeTransaction(req, headers, cancellationToken: ct),
            reply => reply.Error,   // Assumes reply has an Error property of type Mt5TermApi.Error?
            reply => reply.Data,         // Pass the entire reply
            cancellationToken))
        {
            yield return data;
        }
    }

    // Trade functions --------------------------------------------------------------------------------------------------------

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
            headers => TradeFunctionsClient.OrderCalcMargin(request, headers, deadline, cancellationToken),
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
            headers => TradeFunctionsClient.OrderCheck(request, headers, deadline, cancellationToken),
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
            headers => TradeFunctionsClient.PositionsTotal(new Google.Protobuf.WellKnownTypes.Empty(), headers, deadline, cancellationToken),
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


    //
    //  Market info --------------------------------------------------------------------------------------------------------
    //

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
            headers => MarketInfoClient.SymbolsTotal(request, headers, deadline, cancellationToken),
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
            headers => MarketInfoClient.SymbolExist(request, headers, deadline, cancellationToken),
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
            headers => MarketInfoClient.SymbolName(request, headers, deadline, cancellationToken),
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
            headers => MarketInfoClient.SymbolSelect(request, headers, deadline, cancellationToken),
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
            headers => MarketInfoClient.SymbolIsSynchronized(request, headers, deadline, cancellationToken),
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
            headers => MarketInfoClient.SymbolInfoDouble(request, headers, deadline, cancellationToken),
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
            headers => MarketInfoClient.SymbolInfoInteger(request, headers, deadline, cancellationToken),
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
            headers => MarketInfoClient.SymbolInfoString(request, headers, deadline, cancellationToken),
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
            headers => MarketInfoClient.SymbolInfoMarginRate(request, headers, deadline, cancellationToken),
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
            headers => MarketInfoClient.SymbolInfoTick(request, headers, deadline, cancellationToken),
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
            headers => MarketInfoClient.SymbolInfoSessionQuote(request, headers, deadline, cancellationToken),
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
            headers => MarketInfoClient.SymbolInfoSessionTrade(request, headers, deadline, cancellationToken),
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
            headers => MarketInfoClient.MarketBookAdd(request, headers, deadline, cancellationToken),
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
            headers => MarketInfoClient.MarketBookRelease(request, headers, deadline, cancellationToken),
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
            headers => MarketInfoClient.MarketBookGet(request, headers, deadline, cancellationToken),
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


    //
    //  Account info --------------------------------------------------------------------------------------------------------
    //

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
            headers => AccountInformationClient.AccountInfoDouble(request, headers, deadline, cancellationToken),
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
            headers => AccountInformationClient.AccountInfoInteger(request, headers, deadline, cancellationToken),
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
            headers => AccountInformationClient.AccountInfoString(request, headers, deadline, cancellationToken),
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
