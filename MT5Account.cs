using Grpc.Core;
using Grpc.Net.Client;
using mt5_term_api;
using System.Runtime.CompilerServices;
using System.Net.Http;


namespace MetaRPC.CSharpMT5;


/// <summary>
/// MT5 Account API Class
///
/// Low-level C# client for MetaTrader 5 terminal via gRPC.
/// Provides direct access to MT5 terminal functions with automatic reconnection support.
/// </summary>
public class MT5Account
{
    #region FIELDS

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
    public string? Host { get; internal set; }

    /// <summary>
    /// Gets the MT5 server port.
    /// </summary>
    public int Port { get; internal set; }

    /// <summary>
    /// Gets the MT5 server name.
    /// </summary>
    public string? ServerName { get; internal set; }

    /// <summary>
    /// Gets or sets the base chart symbol.
    /// </summary>
    public string? BaseChartSymbol { get; private set; }

    /// <summary>
    /// Gets or sets the connection timeout in seconds.
    /// </summary>
    public int ConnectTimeoutSeconds { get; set; }

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
    /// Gets the gRPC client for account information.
    /// </summary>
    public AccountInformation.AccountInformationClient AccountInformationClient { get; }

    /// <summary>
    /// Gets the unique identifier for the account instance.
    /// </summary>
    public Guid Id { get; private set; } = default;

    /// <summary>
    /// Gets whether the account is connected.
    /// </summary>
    private bool Connected => Host is not null || ServerName is not null;

    #endregion

    #region CONSTRUCTORS

    /// <summary>
    /// Initializes a new instance of the <see cref="MT5Account"/> class using credentials.
    /// </summary>
    /// <param name="user">The MT5 user account number.</param>
    /// <param name="password">The password for the user account.</param>
    /// <param name="grpcServer">The address of the gRPC server (optional).</param>
    /// <param name="id">An optional unique identifier for the account instance.</param>
    public MT5Account(ulong user, string password, string? grpcServer = null, Guid id = default)
    {
        // Required fields
        User = user;
        Password = password;

        // gRPC endpoint (nullable in signature -> fallback)
        // IMPORTANT: Must match DLL version exactly - includes https:// prefix
        GrpcServer = grpcServer ?? "https://mt5.mrpc.pro:443";

        // Configure HTTP handler for gRPC with SSL support
        var httpHandler = new HttpClientHandler();
        // TEMPORARY FIX: Accept any SSL certificate (for development/testing only!)
        httpHandler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        // Create gRPC channel with HTTP handler
        GrpcChannel = GrpcChannel.ForAddress(GrpcServer, new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        });
        ConnectionClient = new Connection.ConnectionClient(GrpcChannel);
        SubscriptionClient = new SubscriptionService.SubscriptionServiceClient(GrpcChannel);
        AccountClient = new AccountHelper.AccountHelperClient(GrpcChannel);
        TradeClient = new TradingHelper.TradingHelperClient(GrpcChannel);
        MarketInfoClient = new MarketInfo.MarketInfoClient(GrpcChannel);
        TradeFunctionsClient = new TradeFunctions.TradeFunctionsClient(GrpcChannel);
        AccountInformationClient = new AccountInformation.AccountInformationClient(GrpcChannel);

        Id = id;
    }

    #endregion

    #region HELPER METHODS

    /// <summary>
    /// Attach to existing terminal instance by ID.
    /// </summary>
    /// <param name="instanceId">Terminal instance GUID to attach to.</param>
    public void AttachByInstanceId(Guid instanceId)
    {
        this.Id = instanceId;
    }

    /// <summary>
    /// Creates metadata headers with authentication info.
    /// </summary>
    private Metadata GetHeaders()
    {
        return new Metadata { { "id", Id.ToString() } };
    }

    #endregion

    #region PROTECTED HELPER METHODS (Automatic Reconnection)

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
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
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
    /// <param name="getError">
    /// A delegate that extracts the error (if any) from a <typeparamref name="TReply"/> instance.
    /// Return an error to trigger reconnection logic or throw <see cref="ApiExceptionMT5"/>.
    /// </param>
    /// <param name="getData">
    /// A delegate that extracts the data object from a <typeparamref name="TReply"/> instance.
    /// Return <c>null</c> to skip the current message.
    /// </param>
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
                    catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
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
                        yield return data;
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
    /// Reconnect to MT5 terminal (recreate terminal instance).
    /// Used internally by ExecuteWithReconnect and ExecuteStreamWithReconnect.
    /// </summary>
    /// <param name="deadline">Optional deadline for the reconnection attempt.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <exception cref="ConnectExceptionMT5">If reconnection fails.</exception>
    async Task Reconnect(DateTime? deadline, CancellationToken cancellationToken)
    {
        if (ServerName is not null)
        {
            if (BaseChartSymbol is null) throw new ConnectExceptionMT5("BaseChartSymbol is not set.");
            await ConnectByServerNameAsync(ServerName, BaseChartSymbol, true, ConnectTimeoutSeconds, deadline, cancellationToken);
        }
        else
        {
            if (Host is null || BaseChartSymbol is null) throw new ConnectExceptionMT5("Host/BaseChartSymbol is not set.");
            await ConnectByHostPortAsync(Host, Port, BaseChartSymbol, true, ConnectTimeoutSeconds, deadline, cancellationToken);
        }
    }

    #endregion

    #region CONNECTION METHODS

    /// <summary>
    /// Connects to the MT5 terminal using host and port asynchronously.
    /// </summary>
    /// <param name="host">The IP address or domain of the MT5 server.</param>
    /// <param name="port">The port on which the MT5 server listens (default is 443).</param>
    /// <param name="baseChartSymbol">The base chart symbol to use (e.g., "EURUSD").</param>
    /// <param name="waitForTerminalIsAlive">Whether to wait for terminal readiness before returning.</param>
    /// <param name="timeoutSeconds">How long to wait for terminal readiness before timing out.</param>
    /// <param name="deadline">Optional deadline for the gRPC call.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
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

        ConnectReply reply;

        if (Id != default)
        {
            var headers = GetHeaders();
            reply = await ConnectionClient.ConnectAsync(connectRequest, headers, deadline, cancellationToken);
        }
        else
        {
            reply = await ConnectionClient.ConnectAsync(connectRequest, deadline: deadline, cancellationToken: cancellationToken);
        }

        if (reply.Error != null)
            throw new ApiExceptionMT5(reply.Error);

        Host = host;
        Port = port;
        BaseChartSymbol = baseChartSymbol;
        ConnectTimeoutSeconds = timeoutSeconds;
        Id = Guid.Parse(reply.Data.TerminalInstanceGuid);
    }

    /// <summary>
    /// Connects to the MT5 terminal using host and port synchronously.
    /// </summary>
    /// <param name="host">The IP address or domain of the MT5 server.</param>
    /// <param name="port">The port on which the MT5 server listens (default is 443).</param>
    /// <param name="baseChartSymbol">The base chart symbol to use (e.g., "EURUSD").</param>
    /// <param name="waitForTerminalIsAlive">Whether to wait for terminal readiness before returning.</param>
    /// <param name="timeoutSeconds">How long to wait for terminal readiness before timing out.</param>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error response.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC connection fails.</exception>
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
    /// Connects to the MT5 terminal using server name asynchronously.
    /// </summary>
    /// <param name="serverName">The MT5 server cluster name.</param>
    /// <param name="baseChartSymbol">The base chart symbol to use (e.g., "EURUSD").</param>
    /// <param name="waitForTerminalIsAlive">Whether to wait for terminal readiness before returning.</param>
    /// <param name="timeoutSeconds">How long to wait for terminal readiness before timing out.</param>
    /// <param name="deadline">Optional deadline for the gRPC call.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
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
        var connectExRequest = new ConnectExRequest
        {
            User = User,
            Password = Password,
            MtClusterName = serverName,
            BaseChartSymbol = baseChartSymbol,
            TerminalReadinessWaitingTimeoutSeconds = timeoutSeconds
        };

        ConnectExReply reply;

        if (Id != default)
        {
            var headers = GetHeaders();
            reply = await ConnectionClient.ConnectExAsync(connectExRequest, headers, deadline, cancellationToken);
        }
        else
        {
            reply = await ConnectionClient.ConnectExAsync(connectExRequest, deadline: deadline, cancellationToken: cancellationToken);
        }

        if (reply.Error != null)
            throw new ApiExceptionMT5(reply.Error);

        ServerName = serverName;
        BaseChartSymbol = baseChartSymbol;
        ConnectTimeoutSeconds = timeoutSeconds;
        Id = Guid.Parse(reply.Data.TerminalInstanceGuid);
    }

    /// <summary>
    /// Connects to the MT5 terminal using server name synchronously.
    /// </summary>
    /// <param name="serverName">The MT5 server cluster name.</param>
    /// <param name="baseChartSymbol">The base chart symbol to use (e.g., "EURUSD").</param>
    /// <param name="waitForTerminalIsAlive">Whether to wait for terminal readiness before returning.</param>
    /// <param name="timeoutSeconds">How long to wait for terminal readiness before timing out.</param>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error response.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC connection fails.</exception>
    public void ConnectByServerName(
        string serverName,
        string baseChartSymbol = "EURUSD",
        bool waitForTerminalIsAlive = true,
        int timeoutSeconds = 30)
    {
        ConnectByServerNameAsync(serverName, baseChartSymbol, waitForTerminalIsAlive, timeoutSeconds).GetAwaiter().GetResult();
    }

    #endregion

    #region ACCOUNT INFORMATION

    /// <summary>
    /// Gets the complete summary of the trading account in a single call.
    /// Returns all essential account information including balance, equity, margin, profit, leverage, and currency.
    /// This is the recommended method for retrieving account data as it minimizes network calls.
    /// </summary>
    /// <param name="deadline">Optional deadline after which the request will be canceled if not completed.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the request.</param>
    /// <returns>The server's response containing account summary data.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected before calling this method.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails due to communication or protocol errors.</exception>
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
    /// Gets the complete summary of the trading account synchronously.
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
    /// Retrieves a specific double-precision property of the trading account.
    /// Use this to get individual numeric values such as BALANCE, EQUITY, MARGIN, PROFIT, etc.
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

        return res.Data.RequestedValue;
    }

    /// <summary>
    /// Retrieves a specific double-precision property of the trading account synchronously.
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
    /// Retrieves a specific integer property of the trading account.
    /// Use this to get values such as LOGIN, LEVERAGE, TRADE_MODE, LIMIT_ORDERS, etc.
    /// </summary>
    /// <param name="property">The account integer property to retrieve.</param>
    /// <param name="deadline">Optional deadline after which the call will be cancelled.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>The integer value of the requested account property.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<long> AccountInfoIntegerAsync(
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

        return res.Data.RequestedValue;
    }

    /// <summary>
    /// Retrieves a specific integer property of the trading account synchronously.
    /// </summary>
    /// <param name="property">The account integer property to retrieve.</param>
    /// <param name="deadline">Optional deadline after which the call will be cancelled.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>The integer value of the requested account property.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public long AccountInfoInteger(
        AccountInfoIntegerPropertyType property,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return AccountInfoIntegerAsync(property, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Retrieves a specific string property of the trading account.
    /// Use this to get textual information such as account NAME, SERVER, CURRENCY, or COMPANY.
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

        return res.Data.RequestedValue;
    }

    /// <summary>
    /// Retrieves a specific string property of the trading account synchronously.
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

    #endregion

    #region SYMBOL INFORMATION & OPERATIONS

    /// <summary>
    /// Gets the total count of available symbols on the MT5 server.
    /// Returns either all symbols known to the server or only those currently shown in the MarketWatch window.
    /// Use this to determine how many symbols are available before requesting detailed symbol information.
    /// </summary>
    /// <param name="selectedOnly">If true, returns only symbols visible in MarketWatch; if false, returns all available symbols.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Total number of symbols matching the filter criteria.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<SymbolsTotalData> SymbolsTotalAsync(bool selectedOnly, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolsTotalRequest { Mode = selectedOnly };

        var res = await ExecuteWithReconnect(
            headers => MarketInfoClient.SymbolsTotal(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Gets the total count of available symbols on the MT5 server synchronously.
    /// </summary>
    /// <param name="selectedOnly">If true, returns only symbols visible in MarketWatch; if false, returns all available symbols.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Total number of symbols matching the filter criteria.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public SymbolsTotalData SymbolsTotal(bool selectedOnly, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return SymbolsTotalAsync(selectedOnly, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Checks if a symbol exists on the MT5 server.
    /// Returns whether the specified symbol name is available for trading.
    /// </summary>
    /// <param name="symbol">Symbol name to check (e.g., "EURUSD").</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Reply with existence status.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<SymbolExistData> SymbolExistAsync(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolExistRequest { Name = symbol };

        var res = await ExecuteWithReconnect(
            headers => MarketInfoClient.SymbolExist(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Checks if a symbol exists on the MT5 server synchronously.
    /// </summary>
    /// <param name="symbol">Symbol name to check (e.g., "EURUSD").</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Reply with existence status.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public SymbolExistData SymbolExist(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return SymbolExistAsync(symbol, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the symbol name by its position in the symbols list.
    /// Returns the name of the symbol at the specified index.
    /// </summary>
    /// <param name="index">Position in the symbols list (0-based index).</param>
    /// <param name="selected">If true, searches only in Market Watch; if false, searches all symbols.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Reply with symbol name.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<SymbolNameData> SymbolNameAsync(int index, bool selected, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolNameRequest
        {
            Index = index,
            Selected = selected
        };

        var res = await ExecuteWithReconnect(
            headers => MarketInfoClient.SymbolName(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Gets the symbol name by its position in the symbols list synchronously.
    /// </summary>
    /// <param name="index">Position in the symbols list (0-based index).</param>
    /// <param name="selected">If true, searches only in Market Watch; if false, searches all symbols.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Reply with symbol name.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public SymbolNameData SymbolName(int index, bool selected, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return SymbolNameAsync(index, selected, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Selects or deselects a symbol in the Market Watch window.
    /// Symbols must be selected in Market Watch to receive price updates and place trades.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="select">True to select symbol, false to remove from Market Watch.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Reply with success status.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<SymbolSelectData> SymbolSelectAsync(string symbol, bool select, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolSelectRequest
        {
            Symbol = symbol,
            Select = select
        };

        var res = await ExecuteWithReconnect(
            headers => MarketInfoClient.SymbolSelect(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Selects or deselects a symbol in the Market Watch window synchronously.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="select">True to select symbol, false to remove from Market Watch.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Reply with success status.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public SymbolSelectData SymbolSelect(string symbol, bool select, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return SymbolSelectAsync(symbol, select, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Checks if symbol data is synchronized with the trade server.
    /// Returns whether the symbol's price and market data is currently up to date.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Reply with synchronization status.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<SymbolIsSynchronizedData> SymbolIsSynchronizedAsync(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolIsSynchronizedRequest { Symbol = symbol };

        var res = await ExecuteWithReconnect(
            headers => MarketInfoClient.SymbolIsSynchronized(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Checks if symbol data is synchronized with the trade server synchronously.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Reply with synchronization status.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public SymbolIsSynchronizedData SymbolIsSynchronized(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return SymbolIsSynchronizedAsync(symbol, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets a double property value for a specified symbol.
    /// Used to retrieve numeric properties like point size, spread, volume limits, etc.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="property">Property type (SYMBOL_POINT, SYMBOL_VOLUME_MIN, etc.).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Reply with the property value.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<SymbolInfoDoubleData> SymbolInfoDoubleAsync(
        string symbol,
        SymbolInfoDoubleProperty property,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolInfoDoubleRequest
        {
            Symbol = symbol,
            Type = property
        };

        var res = await ExecuteWithReconnect(
            headers => MarketInfoClient.SymbolInfoDouble(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Gets a double property value for a specified symbol synchronously.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="property">Property type (SYMBOL_POINT, SYMBOL_VOLUME_MIN, etc.).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Reply with the property value.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public SymbolInfoDoubleData SymbolInfoDouble(
        string symbol,
        SymbolInfoDoubleProperty property,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return SymbolInfoDoubleAsync(symbol, property, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets an integer property value for a specified symbol.
    /// Used to retrieve integer properties like digits, spread in points, trade mode, etc.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="property">Property type (SYMBOL_DIGITS, SYMBOL_SPREAD, etc.).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Reply with the property value.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<SymbolInfoIntegerData> SymbolInfoIntegerAsync(
        string symbol,
        SymbolInfoIntegerProperty property,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolInfoIntegerRequest
        {
            Symbol = symbol,
            Type = property
        };

        var res = await ExecuteWithReconnect(
            headers => MarketInfoClient.SymbolInfoInteger(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Gets an integer property value for a specified symbol synchronously.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="property">Property type (SYMBOL_DIGITS, SYMBOL_SPREAD, etc.).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Reply with the property value.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public SymbolInfoIntegerData SymbolInfoInteger(
        string symbol,
        SymbolInfoIntegerProperty property,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return SymbolInfoIntegerAsync(symbol, property, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets a string property value for a specified symbol.
    /// Used to retrieve string properties like base currency, description, path, etc.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="property">Property type (SYMBOL_CURRENCY_BASE, SYMBOL_DESCRIPTION, etc.).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Reply with the property value.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<SymbolInfoStringData> SymbolInfoStringAsync(
        string symbol,
        SymbolInfoStringProperty property,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolInfoStringRequest
        {
            Symbol = symbol,
            Type = property
        };

        var res = await ExecuteWithReconnect(
            headers => MarketInfoClient.SymbolInfoString(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Gets a string property value for a specified symbol synchronously.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="property">Property type (SYMBOL_CURRENCY_BASE, SYMBOL_DESCRIPTION, etc.).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Reply with the property value.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public SymbolInfoStringData SymbolInfoString(
        string symbol,
        SymbolInfoStringProperty property,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return SymbolInfoStringAsync(symbol, property, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the margin rate required for opening positions on a specified symbol.
    /// Returns the margin multiplier applied for buy and sell orders, which varies based on order type.
    /// Use this to calculate the exact margin requirement before placing orders.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="orderType">Type of order (BUY, SELL, etc.).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Margin rate information including initial and maintenance margin multipliers.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<SymbolInfoMarginRateData> SymbolInfoMarginRateAsync(
        string symbol,
        ENUM_ORDER_TYPE orderType,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolInfoMarginRateRequest
        {
            Symbol = symbol,
            OrderType = orderType
        };

        var res = await ExecuteWithReconnect(
            headers => MarketInfoClient.SymbolInfoMarginRate(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Gets the margin rate required for opening positions on a specified symbol synchronously.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="orderType">Type of order (BUY, SELL, etc.).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Margin rate information including initial and maintenance margin multipliers.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public SymbolInfoMarginRateData SymbolInfoMarginRate(
        string symbol,
        ENUM_ORDER_TYPE orderType,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return SymbolInfoMarginRateAsync(symbol, orderType, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the latest tick data for a specified symbol.
    /// Returns real-time market information including current bid/ask prices, last trade price, volume, and timestamp.
    /// This is the primary method for retrieving current market prices for trading decisions.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD", "GBPUSD").</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Latest tick data with bid/ask prices, last price, volume, and time.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
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
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Gets the latest tick data for a specified symbol synchronously.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD", "GBPUSD").</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Latest tick data with bid/ask prices, last price, volume, and time.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public MrpcMqlTick SymbolInfoTick(
        string symbol,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return SymbolInfoTickAsync(symbol, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the quote (pricing) session schedule for a symbol on a specific day.
    /// Returns the start and end times when price quotes are available for the symbol.
    /// Use this to determine when you can expect to receive price updates for market data.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="dayOfWeek">Day of the week (SUNDAY=0, MONDAY=1, ..., SATURDAY=6).</param>
    /// <param name="sessionIndex">Session index (0 for first session, 1 for second session if multiple sessions exist).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Quote session times including start and end times in seconds since midnight.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<SymbolInfoSessionQuoteData> SymbolInfoSessionQuoteAsync(
        string symbol,
        mt5_term_api.DayOfWeek dayOfWeek,
        int sessionIndex,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolInfoSessionQuoteRequest
        {
            Symbol = symbol,
            DayOfWeek = (mt5_term_api.DayOfWeek)(int)dayOfWeek,
            SessionIndex = (uint)sessionIndex
        };

        var res = await ExecuteWithReconnect(
            headers => MarketInfoClient.SymbolInfoSessionQuote(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Gets the quote (pricing) session schedule for a symbol on a specific day synchronously.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="dayOfWeek">Day of the week (SUNDAY=0, MONDAY=1, ..., SATURDAY=6).</param>
    /// <param name="sessionIndex">Session index (0 for first session, 1 for second session if multiple sessions exist).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Quote session times including start and end times in seconds since midnight.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public SymbolInfoSessionQuoteData SymbolInfoSessionQuote(
        string symbol,
        mt5_term_api.DayOfWeek dayOfWeek,
        int sessionIndex,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return SymbolInfoSessionQuoteAsync(symbol, dayOfWeek, sessionIndex, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the trading session schedule for a symbol on a specific day.
    /// Returns the start and end times when trading operations are allowed for the symbol.
    /// Use this to determine when you can open/close positions and place orders.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="dayOfWeek">Day of the week (SUNDAY=0, MONDAY=1, ..., SATURDAY=6).</param>
    /// <param name="sessionIndex">Session index (0 for first session, 1 for second session if multiple sessions exist).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Trade session times including start and end times in seconds since midnight.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public async Task<SymbolInfoSessionTradeData> SymbolInfoSessionTradeAsync(
        string symbol,
        mt5_term_api.DayOfWeek dayOfWeek,
        int sessionIndex,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (Id == default)
            throw new ConnectExceptionMT5("Please call Connect method firstly");

        var request = new SymbolInfoSessionTradeRequest
        {
            Symbol = symbol,
            DayOfWeek = (mt5_term_api.DayOfWeek)(int)dayOfWeek,
            SessionIndex = (uint)sessionIndex
        };

        var res = await ExecuteWithReconnect(
            headers => MarketInfoClient.SymbolInfoSessionTrade(request, headers, deadline, cancellationToken),
            r => r.Error,
            deadline,
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Gets the trading session schedule for a symbol on a specific day synchronously.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="dayOfWeek">Day of the week (SUNDAY=0, MONDAY=1, ..., SATURDAY=6).</param>
    /// <param name="sessionIndex">Session index (0 for first session, 1 for second session if multiple sessions exist).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Trade session times including start and end times in seconds since midnight.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public SymbolInfoSessionTradeData SymbolInfoSessionTrade(
        string symbol,
        mt5_term_api.DayOfWeek dayOfWeek,
        int sessionIndex,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return SymbolInfoSessionTradeAsync(symbol, dayOfWeek, sessionIndex, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    #endregion

    #region POSITIONS & ORDERS INFORMATION

    /// <summary>
    /// Gets the currently opened orders and positions for the connected account asynchronously.
    /// </summary>
    /// <param name="sortMode">The sort mode for the opened orders.</param>
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
    /// <param name="sortMode">The sort mode for the opened orders.</param>
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
    /// <param name="sortMode">The sort mode: by open time, close time, or ticket ID.</param>
    /// <param name="pageNumber">The page number for paginated results (default 0).</param>
    /// <param name="itemsPerPage">The number of items per page (default 0 = all).</param>
    /// <param name="deadline">Optional deadline after which the request will be canceled if not completed.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the request.</param>
    /// <returns>A task representing the asynchronous operation. The result contains paged historical order data.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
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
    /// <param name="sortMode">The sort mode: by open time, close time, or ticket ID.</param>
    /// <param name="pageNumber">The page number for paginated results (default 0).</param>
    /// <param name="itemsPerPage">The number of items per page (default 0 = all).</param>
    /// <param name="deadline">Optional deadline after which the request will be canceled if not completed.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the request.</param>
    /// <returns>The server's response containing paged historical order data.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
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
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
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
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public OpenedOrdersTicketsData OpenedOrdersTickets(DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return OpenedOrdersTicketsAsync(deadline, cancellationToken).GetAwaiter().GetResult();
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
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
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
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
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
    /// Gets the total count of currently open positions on the account.
    /// Returns a simple count of all active positions regardless of symbol.
    /// Use this for quick checks of position count before retrieving detailed position information.
    /// </summary>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Total number of open positions.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
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
    /// Gets the total count of currently open positions on the account synchronously.
    /// </summary>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Total number of open positions.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public PositionsTotalData PositionsTotal(DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return PositionsTotalAsync(deadline, cancellationToken).GetAwaiter().GetResult();
    }

    #endregion

    #region MARKET DEPTH (DOM)

    /// <summary>
    /// Subscribes to Market Depth (DOM/Level II) updates for a specified symbol.
    /// After subscription, you can retrieve current order book data showing pending buy and sell orders.
    /// Use this to access liquidity information and see the market depth before placing large orders.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Subscription confirmation response.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
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
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Subscribes to Market Depth (DOM/Level II) updates for a specified symbol synchronously.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Subscription confirmation response.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public MarketBookAddData MarketBookAdd(
        string symbol,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return MarketBookAddAsync(symbol, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the current Market Depth (order book) data for a subscribed symbol.
    /// Returns pending buy and sell orders with prices and volumes from the order book.
    /// Use this to analyze liquidity, identify support/resistance levels, or optimize order placement.
    /// </summary>
    /// <param name="symbol">Symbol name (must be subscribed via MarketBookAdd first).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Market book data containing arrays of buy and sell orders with prices and volumes.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
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
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Gets the current Market Depth (order book) data for a subscribed symbol synchronously.
    /// </summary>
    /// <param name="symbol">Symbol name (must be subscribed via MarketBookAdd first).</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Market book data containing arrays of buy and sell orders with prices and volumes.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public MarketBookGetData MarketBookGet(
        string symbol,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return MarketBookGetAsync(symbol, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Unsubscribes from Market Depth updates for a specified symbol.
    /// Stops receiving order book data and releases associated resources.
    /// Use this when you no longer need DOM data for a symbol to free up resources.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Unsubscription confirmation response.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
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
            cancellationToken);

        return res.Data;
    }

    /// <summary>
    /// Unsubscribes from Market Depth updates for a specified symbol synchronously.
    /// </summary>
    /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Unsubscription confirmation response.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public MarketBookReleaseData MarketBookRelease(
        string symbol,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return MarketBookReleaseAsync(symbol, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    #endregion

    #region MARKET DATA & HISTORY

    /// <summary>
    /// Gets tick value and tick size data for the given symbols asynchronously.
    /// Returns tick values, contract size, and tick size for trading calculations.
    /// </summary>
    /// <param name="symbols">List of symbol names.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task containing tick value and contract size info per symbol.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
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
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public TickValueWithSizeData TickValueWithSize(IEnumerable<string> symbols, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return TickValueWithSizeAsync(symbols, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Retrieves symbol parameters for multiple instruments asynchronously.
    /// Gets comprehensive parameter details for one or more symbols with pagination support.
    /// Returns extensive symbol information including contract specifications, trading conditions, and current state.
    /// </summary>
    /// <param name="request">The request containing filters and pagination.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task containing symbol parameter details.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
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
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns an error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
    public SymbolParamsManyData SymbolParamsMany(SymbolParamsManyRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return SymbolParamsManyAsync(request, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    #endregion

    #region TRADING OPERATIONS

    /// <summary>
    /// Sends a trading order to the MT5 server (market or pending order).
    /// Use this method to open new positions or place pending orders with specified parameters
    /// including symbol, volume, price, stop loss, and take profit levels.
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
    /// Sends a trading order to the MT5 server synchronously.
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
    /// Modifies an existing order or position parameters.
    /// Use this to update stop loss, take profit, price, or other parameters of an open position or pending order.
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
    /// Closes an open position or deletes a pending order.
    /// For positions, you can specify partial closure by providing a volume less than the total position size.
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
    /// Closes an open position or deletes a pending order synchronously.
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

    #endregion

    #region TRADING CALCULATIONS

    /// <summary>
    /// Calculates the margin required to open a position with specified parameters.
    /// Returns the amount of funds needed in account currency to maintain the position.
    /// Use this before placing orders to verify sufficient margin and avoid margin call risks.
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
    /// Calculates the margin required for a planned trade operation synchronously.
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
    /// Validates a trading request and checks if there are sufficient funds to execute it.
    /// Returns detailed calculations including margin requirements, expected profit, and resulting balance.
    /// Use this to verify order validity before sending, preventing rejected orders due to insufficient funds.
    /// </summary>
    /// <param name="request">The order check request.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Validation result with margin, profit, balance calculations, and any error codes.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if gRPC fails to connect or respond.</exception>
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
    /// Validates a trading request and checks if there are sufficient funds to execute it synchronously.
    /// </summary>
    /// <param name="request">The order check request.</param>
    /// <param name="deadline">Optional gRPC deadline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Validation result with margin, profit, balance calculations, and any error codes.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the client is not connected.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the server returns a business error.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if gRPC fails to connect or respond.</exception>
    public OrderCheckData OrderCheck(OrderCheckRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        return OrderCheckAsync(request, deadline, cancellationToken).GetAwaiter().GetResult();
    }

    #endregion

    #region REAL-TIME SUBSCRIPTIONS

    /// <summary>
    /// Subscribes to real-time tick updates for one or more symbols.
    /// Receives a continuous stream of price updates (bid, ask, last, volume) whenever prices change.
    /// Use this for real-time price monitoring, tick-based trading strategies, or market data feeds.
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
    /// Subscribes to trade events whenever a trading operation occurs.
    /// Receives notifications when orders are opened, closed, modified, or deleted.
    /// Use this to track all trading activity in real-time and react to order execution events.
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
            reply => reply.Error,
            reply => reply.Data,
            cancellationToken))
        {
            yield return data;
        }
    }

    /// <summary>
    /// Subscribes to periodic updates of position profit/loss values.
    /// Receives profit updates at regular intervals for all open positions.
    /// Use this to monitor unrealized PnL in real-time and implement profit-based exit strategies.
    /// </summary>
    /// <param name="intervalMs">Interval in milliseconds to poll server.</param>
    /// <param name="ignoreEmpty">Skip frames with no change.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Async stream of profit updates.</returns>
    /// <exception cref="ConnectExceptionMT5">Thrown if the account is not connected.</exception>
    /// <exception cref="Grpc.Core.RpcException">Thrown if the stream fails.</exception>
    /// <exception cref="ApiExceptionMT5">Thrown if the stream returns an error.</exception>
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
            reply => reply.Error,
            reply => reply.Data,
            cancellationToken))
        {
            yield return data;
        }
    }

    /// <summary>
    /// Subscribes to periodic updates of position and pending order ticket numbers.
    /// Receives lists of currently open position tickets and pending order tickets at regular intervals.
    /// Use this to efficiently track which positions/orders exist without retrieving full details.
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
            reply => reply.Error,
            reply => reply.Data,
            cancellationToken))
        {
            yield return data;
        }
    }

    /// <summary>
    /// Subscribes to detailed trade transaction events.
    /// Receives comprehensive information about every trade operation including request, result, and execution details.
    /// Use this for detailed trade auditing, debugging order execution, or building advanced trading analytics.
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
            reply => reply.Error,
            reply => reply.Data,
            cancellationToken))
        {
            yield return data;
        }
    }

    #endregion

    #region API METHODS OVERVIEW

    /*
      CONNECTION (2 methods):
        - ConnectByHostPortAsync() / Connect() : Connect to MT5 terminal by host/port
        - ConnectByServerNameAsync() / ConnectByServerName() : Connect by MT5 server cluster name
     
      ACCOUNT INFORMATION (4 methods):
        - AccountSummaryAsync() / AccountSummary() : Get complete account info (balance, equity, margin, etc.)
        - AccountInfoDoubleAsync() / AccountInfoDouble() : Get specific double property (BALANCE, EQUITY, etc.)
        - AccountInfoIntegerAsync() / AccountInfoInteger() : Get specific integer property (LOGIN, LEVERAGE, etc.)
        - AccountInfoStringAsync() / AccountInfoString() : Get specific string property (NAME, SERVER, CURRENCY)
     
      SYMBOL INFORMATION & OPERATIONS (14 methods):
        - SymbolsTotalAsync() / SymbolsTotal() : Get count of available symbols
        - SymbolExistAsync() / SymbolExist() : Check if symbol exists on server
        - SymbolNameAsync() / SymbolName() : Get symbol name by index
        - SymbolSelectAsync() / SymbolSelect() : Select/deselect symbol in Market Watch
        - SymbolIsSynchronizedAsync() / SymbolIsSynchronized() : Check if symbol data is synchronized
        - SymbolInfoDoubleAsync() / SymbolInfoDouble() : Get symbol double property (POINT, VOLUME_MIN, etc.)
        - SymbolInfoIntegerAsync() / SymbolInfoInteger() : Get symbol integer property (DIGITS, SPREAD, etc.)
        - SymbolInfoStringAsync() / SymbolInfoString() : Get symbol string property (CURRENCY_BASE, DESCRIPTION, etc.)
        - SymbolInfoMarginRateAsync() / SymbolInfoMarginRate() : Get margin requirements for symbol
        - SymbolInfoTickAsync() / SymbolInfoTick() : Get latest tick data with prices and volume
        - SymbolInfoSessionQuoteAsync() / SymbolInfoSessionQuote() : Get quote session schedule
        - SymbolInfoSessionTradeAsync() / SymbolInfoSessionTrade() : Get trading session schedule
     
      POSITIONS & ORDERS INFORMATION (5 methods):
        - OpenedOrdersAsync() / OpenedOrders() : Get all open positions/orders with details
        - OrderHistoryAsync() / OrderHistory() : Get historical orders with pagination
        - OpenedOrdersTicketsAsync() / OpenedOrdersTickets() : Get ticket numbers only (lightweight)
        - PositionsHistoryAsync() / PositionsHistory() : Get closed positions history
        - PositionsTotalAsync() / PositionsTotal() : Get count of open positions
     
      MARKET DEPTH (3 methods):
        - MarketBookAddAsync() / MarketBookAdd() : Subscribe to market depth (DOM)
        - MarketBookGetAsync() / MarketBookGet() : Get current order book data
        - MarketBookReleaseAsync() / MarketBookRelease() : Unsubscribe from market depth
     
      MARKET DATA & HISTORY (2 methods):
        - TickValueWithSizeAsync() / TickValueWithSize() : Get tick value/size for calculations
        - SymbolParamsManyAsync() / SymbolParamsMany() : Get detailed symbol specs with pagination
     
      TRADING OPERATIONS (3 methods):
        - OrderSendAsync() / OrderSend() : Send market or pending order
        - OrderModifyAsync() / OrderModify() : Modify existing order/position
        - OrderCloseAsync() / OrderClose() : Close position or delete pending order
     
      TRADING CALCULATIONS (2 methods):
        - OrderCalcMarginAsync() / OrderCalcMargin() : Calculate required margin for order
        - OrderCheckAsync() / OrderCheck() : Validate order and check funds
     
      REAL-TIME SUBSCRIPTIONS (5 methods):
        - OnSymbolTickAsync() : Stream real-time tick updates
        - OnTradeAsync() : Stream trade events
        - OnPositionProfitAsync() : Stream position profit updates
        - OnPositionsAndPendingOrdersTicketsAsync() : Stream ticket lists
        - OnTradeTransactionAsync() : Stream detailed trade transactions
     
      Total: 40 methods (28 async/sync pairs + 5 streaming methods + helper methods)
     */

    #endregion
}
