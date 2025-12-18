using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using mt5_term_api;
using Mt5TermApi;
using DayOfWeek = mt5_term_api.DayOfWeek;

namespace MetaRPC.CSharpMT5
{
	/// <summary>
	/// Represents an MT5 trading account connected via gRPC.
	/// </summary>
	public class MT5Account
	{
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

		public int ConnectTimeoutSeconds { get; set; }

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
		public Guid Id { get; private set; }

		private bool Connected
		{
			get
			{
				if (Host == null)
				{
					return ServerName != null;
				}
				return true;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:mt5_term_api.MT5Account" /> class using credentials.
		/// </summary>
		/// <param name="user">The MT5 user account number.</param>
		/// <param name="password">The password for the user account.</param>
		/// <param name="grpcServer">The address of the gRPC server (optional).</param>
		/// <param name="id">An optional unique identifier for the account instance.</param>
		public MT5Account(ulong user, string password, string? grpcServer = null, Guid id = default(Guid))
		{
			User = user;
			Password = password;
			GrpcServer = grpcServer ?? "https://mt5.mrpc.pro:443";
			GrpcChannel = GrpcChannel.ForAddress(GrpcServer);
			ConnectionClient = new Connection.ConnectionClient((ChannelBase)(object)GrpcChannel);
			SubscriptionClient = new SubscriptionService.SubscriptionServiceClient((ChannelBase)(object)GrpcChannel);
			AccountClient = new AccountHelper.AccountHelperClient((ChannelBase)(object)GrpcChannel);
			TradeClient = new TradingHelper.TradingHelperClient((ChannelBase)(object)GrpcChannel);
			MarketInfoClient = new MarketInfo.MarketInfoClient((ChannelBase)(object)GrpcChannel);
			TradeFunctionsClient = new TradeFunctions.TradeFunctionsClient((ChannelBase)(object)GrpcChannel);
			AccountInformationClient = new AccountInformation.AccountInformationClient((ChannelBase)(object)GrpcChannel);
			Id = id;
		}

		private async Task Reconnect(DateTime? deadline, CancellationToken cancellationToken)
		{
			if (ServerName == null)
			{
				await ConnectByHostPortAsync(Host, Port, BaseChartSymbol, waitForTerminalIsAlive: true, ConnectTimeoutSeconds, deadline, cancellationToken);
			}
			else
			{
				await ConnectByServerNameAsync(ServerName, BaseChartSymbol, waitForTerminalIsAlive: true, ConnectTimeoutSeconds, deadline, cancellationToken);
			}
		}

		/// <summary>
		/// Connects to the MT5 terminal using credentials provided in the constructor.
		/// </summary>
		/// <param name="host">The IP address or domain of the MT5 server.</param>
		/// <param name="port">The port on which the MT5 server listens (default is 443).</param>
		/// <param name="baseChartSymbol">The base chart symbol to use (e.g., "EURUSD").</param>
		/// <param name="waitForTerminalIsAlive">Whether to wait for terminal readiness before returning.</param>
		/// <param name="timeoutSeconds">How long to wait for terminal readiness before timing out.</param>
		/// <param name="deadline">Optional gRPC deadline for the operation.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>A task representing the asynchronous connection operation.</returns>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns an error response.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC connection fails.</exception>
		public async Task ConnectByHostPortAsync(string host, int port = 443, string baseChartSymbol = "EURUSD", bool waitForTerminalIsAlive = true, int timeoutSeconds = 30, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			ConnectRequest request = new ConnectRequest
			{
				User = User,
				Password = Password,
				Host = host,
				Port = port,
				BaseChartSymbol = baseChartSymbol,
				WaitForTerminalIsAlive = waitForTerminalIsAlive,
				TerminalReadinessWaitingTimeoutSeconds = timeoutSeconds
			};
			Metadata headers = null;
			if (Id != default(Guid))
			{
				Metadata val = new Metadata();
				val.Add("id", Id.ToString());
				headers = val;
			}
			ConnectReply connectReply = await ConnectionClient.ConnectAsync(request, headers, deadline, cancellationToken);
			if (connectReply.Error != null)
			{
				throw new ApiExceptionMT5(connectReply.Error);
			}
			Host = host;
			Port = port;
			BaseChartSymbol = baseChartSymbol;
			ConnectTimeoutSeconds = timeoutSeconds;
			Id = Guid.Parse(connectReply.Data.TerminalInstanceGuid);
		}

		/// <summary>
		/// Synchronously connects to the MT5 terminal.
		/// </summary>
		/// <param name="host">The MT5 server host.</param>
		/// <param name="port">The port the server listens on.</param>
		/// <param name="baseChartSymbol">Chart symbol to initialize the session.</param>
		/// <param name="waitForTerminalIsAlive">Wait for terminal readiness flag.</param>
		/// <param name="timeoutSeconds">Timeout duration in seconds.</param>
		public void Connect(string host, int port = 443, string baseChartSymbol = "EURUSD", bool waitForTerminalIsAlive = true, int timeoutSeconds = 30)
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
		/// <param name="deadline">Optional gRPC deadline for the operation.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>A task representing the asynchronous connection operation.</returns>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns an error response.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC connection fails.</exception>
		public async Task ConnectByServerNameAsync(string serverName, string baseChartSymbol = "EURUSD", bool waitForTerminalIsAlive = true, int timeoutSeconds = 30, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			ConnectExRequest request = new ConnectExRequest
			{
				User = User,
				Password = Password,
				MtClusterName = serverName,
				BaseChartSymbol = baseChartSymbol,
				TerminalReadinessWaitingTimeoutSeconds = timeoutSeconds
			};
			Metadata headers = null;
			if (Id != default(Guid))
			{
				Metadata val = new Metadata();
				val.Add("id", Id.ToString());
				headers = val;
			}
			ConnectExReply connectExReply = await ConnectionClient.ConnectExAsync(request, headers, deadline, cancellationToken);
			if (connectExReply.Error != null)
			{
				throw new ApiExceptionMT5(connectExReply.Error);
			}
			ServerName = serverName;
			BaseChartSymbol = baseChartSymbol;
			ConnectTimeoutSeconds = timeoutSeconds;
			Id = Guid.Parse(connectExReply.Data.TerminalInstanceGuid);
		}

		/// <summary>
		/// Synchronously connects to the MT5 terminal.
		/// </summary>
		/// <param name="serverName">MT5 server name from MT5 Terminal.</param>
		/// <param name="baseChartSymbol">Chart symbol to initialize the session.</param>
		/// <param name="waitForTerminalIsAlive">Wait for terminal readiness flag.</param>
		/// <param name="timeoutSeconds">Timeout duration in seconds.</param>
		public void ConnectByServerName(string serverName, string baseChartSymbol = "EURUSD", bool waitForTerminalIsAlive = true, int timeoutSeconds = 30)
		{
			ConnectByServerNameAsync(serverName, baseChartSymbol, waitForTerminalIsAlive, timeoutSeconds).GetAwaiter().GetResult();
		}

		private Metadata GetHeaders()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Expected O, but got Unknown
			Metadata val = new Metadata();
			val.Add("id", Id.ToString());
			return val;
		}

		/// <summary>
		/// Executes a unary gRPC call with automatic reconnection logic on recoverable errors.
		/// </summary>
		/// <typeparam name="T">The result type of the unary gRPC call.</typeparam>
		/// <param name="grpcCall">
		/// A delegate that executes the gRPC call using the provided metadata headers.
		/// </param>
		/// <param name="errorSelector">
		/// A delegate that extracts the API error from the result, if any.
		/// </param>
		/// <param name="deadline">
		/// Optional deadline for the gRPC call. If the deadline passes, the operation is canceled.
		/// </param>
		/// <param name="cancellationToken">
		/// Optional cancellation token to stop execution.
		/// </param>
		/// <returns>
		/// The gRPC call result if successful; otherwise, throws an exception.
		/// </returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">
		/// Thrown if the client is not connected before execution.
		/// </exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">
		/// Thrown if the server response contains a known API error.
		/// </exception>
		/// <exception cref="T:Grpc.Core.RpcException">
		/// Thrown if gRPC fails with a non-recoverable error.
		/// </exception>
		/// <exception cref="T:System.OperationCanceledException">
		/// Thrown if the operation is canceled by the caller.
		/// </exception>
		/// <exception cref="T:Grpc.Core.RpcException">If gRPC fails with a non-recoverable error.</exception>
		private async Task<T> ExecuteWithReconnect<T>(Func<Metadata, T> grpcCall, Func<T, Error?> errorSelector, DateTime? deadline, CancellationToken cancellationToken)
		{
			Unsafe.SkipInit(out object obj2);
			while (!cancellationToken.IsCancellationRequested)
			{
				Metadata headers = GetHeaders();
				T val;
				try
				{
					val = grpcCall(headers);
				}
				catch (RpcException ex) when ((int)ex.StatusCode == 14)
				{
					await Task.Delay(500, cancellationToken);
					await Reconnect(deadline, cancellationToken);
					continue;
				}
				Error error = errorSelector(val);
				if (!(error?.ErrorCode == "TERMINAL_INSTANCE_NOT_FOUND") && !(error?.ErrorCode == "TERMINAL_REGISTRY_TERMINAL_NOT_FOUND"))
				{
					if (error != null)
					{
						throw new ApiExceptionMT5(error);
					}
					return val;
				}
				await Task.Delay(500, cancellationToken);
				await Reconnect(deadline, cancellationToken);
			}
			throw new OperationCanceledException("The operation was canceled by the caller.");
		}

		/// <summary>
		/// Gets the summary information for a trading account synchronously.
		/// </summary>
		/// <param name="deadline">Optional deadline after which the request will be canceled if not completed.</param>
		/// <param name="cancellationToken">Optional cancellation token to cancel the request.</param>
		/// <returns>The server's response containing account summary data.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the account is not connected before calling this method.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails due to communication or protocol errors.</exception>
		public async Task<AccountSummaryData> AccountSummaryAsync(DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (!Connected)
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			AccountSummaryRequest request = new AccountSummaryRequest();
			return (await ExecuteWithReconnect((Metadata headers) => AccountClient.AccountSummary(request, headers, deadline, cancellationToken), (AccountSummaryReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Gets the summary information for a trading account synchronously.
		/// </summary>
		/// <param name="deadline">Optional deadline after which the request will be canceled if not completed.</param>
		/// <param name="cancellationToken">Optional cancellation token to cancel the request.</param>
		/// <returns>The server's response containing account summary data.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the account is not connected before calling this method.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails due to communication or protocol errors.</exception>
		public AccountSummaryData AccountSummary(DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the account is not connected before calling this method.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails due to communication or protocol errors.</exception>
		public async Task<OpenedOrdersData> OpenedOrdersAsync(BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			OpenedOrdersRequest request = new OpenedOrdersRequest
			{
				InputSortMode = sortMode
			};
			return (await ExecuteWithReconnect((Metadata headers) => AccountClient.OpenedOrders(request, headers, deadline, cancellationToken), (OpenedOrdersReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Gets the currently opened orders and positions for the connected account synchronously.
		/// </summary>
		/// <param name="sortMode">The sort mode for the opened orders (0 - open time, 1 - close time, 2 - ticket ID).</param>
		/// <param name="deadline">Optional deadline after which the request will be canceled if not completed.</param>
		/// <param name="cancellationToken">Optional cancellation token to cancel the request.</param>
		/// <returns>The server's response containing opened orders and positions.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the account is not connected before calling this method.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails due to communication or protocol errors.</exception>
		public OpenedOrdersData OpenedOrders(BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<OrdersHistoryData> OrderHistoryAsync(DateTime from, DateTime to, BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sortMode = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc, int pageNumber = 0, int itemsPerPage = 0, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			OrderHistoryRequest request = new OrderHistoryRequest
			{
				InputFrom = Timestamp.FromDateTime(from.ToUniversalTime()),
				InputTo = Timestamp.FromDateTime(to.ToUniversalTime()),
				InputSortMode = sortMode,
				PageNumber = pageNumber,
				ItemsPerPage = itemsPerPage
			};
			return (await ExecuteWithReconnect((Metadata headers) => AccountClient.OrderHistory(request, headers, deadline, cancellationToken), (OrderHistoryReply r) => r.Error, deadline, cancellationToken)).Data;
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public OrdersHistoryData OrderHistory(DateTime from, DateTime to, BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sortMode = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc, int pageNumber = 0, int itemsPerPage = 0, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return OrderHistoryAsync(from, to, sortMode, pageNumber, itemsPerPage, deadline, cancellationToken).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Gets ticket IDs of all currently opened orders and positions asynchronously.
		/// </summary>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>Task containing collection of opened order and position tickets.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<OpenedOrdersTicketsData> OpenedOrdersTicketsAsync(DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			OpenedOrdersTicketsRequest request = new OpenedOrdersTicketsRequest();
			return (await ExecuteWithReconnect((Metadata headers) => AccountClient.OpenedOrdersTickets(request, headers, deadline, cancellationToken), (OpenedOrdersTicketsReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Gets ticket IDs of all currently opened orders and positions synchronously.
		/// </summary>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>Collection of opened order and position tickets.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public OpenedOrdersTicketsData OpenedOrdersTickets(DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<SymbolParamsManyData> SymbolParamsManyAsync(SymbolParamsManyRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			return (await ExecuteWithReconnect((Metadata headers) => AccountClient.SymbolParamsMany(request, headers, deadline, cancellationToken), (SymbolParamsManyReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Retrieves symbol parameters for multiple instruments synchronously.
		/// </summary>
		/// <param name="request">The request containing filters and pagination.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>Symbol parameter details.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public SymbolParamsManyData SymbolParamsMany(SymbolParamsManyRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<TickValueWithSizeData> TickValueWithSizeAsync(IEnumerable<string> symbols, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			TickValueWithSizeRequest request = new TickValueWithSizeRequest();
			request.SymbolNames.AddRange(symbols);
			return (await ExecuteWithReconnect((Metadata headers) => AccountClient.TickValueWithSize(request, headers, deadline, cancellationToken), (TickValueWithSizeReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Gets tick value and tick size data for the given symbols synchronously.
		/// </summary>
		/// <param name="symbols">List of symbol names.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>Tick value and contract size info per symbol.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public TickValueWithSizeData TickValueWithSize(IEnumerable<string> symbols, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<PositionsHistoryData> PositionsHistoryAsync(AH_ENUM_POSITIONS_HISTORY_SORT_TYPE sortType, DateTime? openFrom = null, DateTime? openTo = null, int page = 0, int size = 0, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			PositionsHistoryRequest request = new PositionsHistoryRequest
			{
				SortType = sortType,
				PageNumber = page,
				ItemsPerPage = size
			};
			if (openFrom.HasValue)
			{
				request.PositionOpenTimeFrom = Timestamp.FromDateTime(openFrom.Value.ToUniversalTime());
			}
			if (openTo.HasValue)
			{
				request.PositionOpenTimeTo = Timestamp.FromDateTime(openTo.Value.ToUniversalTime());
			}
			return (await ExecuteWithReconnect((Metadata headers) => AccountClient.PositionsHistory(request, headers, deadline, cancellationToken), (PositionsHistoryReply r) => r.Error, deadline, cancellationToken)).Data;
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public PositionsHistoryData PositionsHistory(AH_ENUM_POSITIONS_HISTORY_SORT_TYPE sortType, DateTime? openFrom = null, DateTime? openTo = null, int page = 0, int size = 0, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the account is not connected.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
		public async Task<OrderSendData> OrderSendAsync(OrderSendRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			return (await ExecuteWithReconnect((Metadata headers) => TradeClient.OrderSend(request, headers, deadline, cancellationToken), (OrderSendReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Sends a market or pending order to the trading server synchronously.
		/// </summary>
		/// <param name="request">The order request to send.</param>
		/// <param name="deadline">Optional deadline for the operation.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>Response containing deal/order confirmation data.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the account is not connected.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
		public OrderSendData OrderSend(OrderSendRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the account is not connected.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
		public async Task<OrderModifyData> OrderModifyAsync(OrderModifyRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			return (await ExecuteWithReconnect((Metadata headers) => TradeClient.OrderModify(request, headers, deadline, cancellationToken), (OrderModifyReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Modifies an existing order or position synchronously.
		/// </summary>
		/// <param name="request">The modification request (SL, TP, price, expiration, etc.).</param>
		/// <param name="deadline">Optional deadline for the operation.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>Response containing updated order/deal info.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the account is not connected.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
		public OrderModifyData OrderModify(OrderModifyRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the account is not connected.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
		public async Task<OrderCloseData> OrderCloseAsync(OrderCloseRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			return (await ExecuteWithReconnect((Metadata headers) => TradeClient.OrderClose(request, headers, deadline, cancellationToken), (OrderCloseReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Closes a market or pending order synchronously.
		/// </summary>
		/// <param name="request">The close request including ticket, volume, and slippage.</param>
		/// <param name="deadline">Optional deadline for the operation.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>Response describing the close result and return codes.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the account is not connected.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns an error in the response.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
		public OrderCloseData OrderClose(OrderCloseRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// and returns an <see cref="T:Grpc.Core.AsyncServerStreamingCall`1" />.
		/// </param>
		/// <param name="getError">
		/// A delegate that extracts the error (if any) from a <typeparamref name="TReply" /> instance.
		/// Return error with <c>"TERMINAL_INSTANCE_NOT_FOUND"</c> to trigger reconnection logic, or any non-null error to throw <see cref="T:mt5_term_api.ApiExceptionMT5" />.
		/// </param>
		/// <param name="getData">
		/// A delegate that extracts the data object from a <typeparamref name="TReply" /> instance.
		/// Return <c>null</c> to skip the current message.
		/// </param>
		/// <param name="cancellationToken">Optional cancellation token to stop streaming and reconnection attempts.</param>
		/// <returns>
		/// An <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" /> of extracted <typeparamref name="TData" /> items streamed from the server.
		/// </returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if reconnection logic fails due to missing account context.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown when the stream response contains a known API error.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if a non-recoverable gRPC error occurs.</exception>
		private async IAsyncEnumerable<TData> ExecuteStreamWithReconnect<TRequest, TReply, TData>(TRequest request, Func<TRequest, Metadata, CancellationToken, AsyncServerStreamingCall<TReply>> streamInvoker, Func<TReply, Error?> getError, Func<TReply, TData> getData, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			Unsafe.SkipInit(out object obj2);
			while (!cancellationToken.IsCancellationRequested)
			{
				bool reconnectRequired = false;
				AsyncServerStreamingCall<TReply> stream = null;
				try
				{
					stream = streamInvoker(request, GetHeaders(), cancellationToken);
					IAsyncStreamReader<TReply> responseStream = stream.ResponseStream;
					while (true)
					{
						TReply current;
						try
						{
							if (!(await responseStream.MoveNext(cancellationToken)))
							{
								break;
							}
							current = responseStream.Current;
							goto IL_0149;
						}
						catch (RpcException ex) when ((int)ex.StatusCode == 14)
						{
							reconnectRequired = true;
						}
						break;
					IL_0149:
						Error error = getError(current);
						if (error?.ErrorCode == "TERMINAL_INSTANCE_NOT_FOUND")
						{
							reconnectRequired = true;
							break;
						}
						if (error?.ErrorCode == "TERMINAL_REGISTRY_TERMINAL_NOT_FOUND")
						{
							reconnectRequired = true;
							break;
						}
						if (error != null)
						{
							throw new ApiExceptionMT5(error);
						}
						TData val = getData(current);
						if (val != null)
						{
							yield return val;
						}
					}
				}
				finally
				{
					stream?.Dispose();
				}
				if (!reconnectRequired)
				{
					break;
				}
				await Task.Delay(500, cancellationToken);
				await Reconnect(null, cancellationToken);
			}
		}

		/// <summary>
		/// Subscribes to real-time tick data for specified symbols.
		/// </summary>
		/// <param name="symbols">The symbol names to subscribe to.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>Async stream of tick data responses.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the account is not connected.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">If the stream fails.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if an error is received from the stream.</exception>
		public async IAsyncEnumerable<OnSymbolTickData> OnSymbolTickAsync(IEnumerable<string> symbols, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			OnSymbolTickRequest onSymbolTickRequest = new OnSymbolTickRequest();
			onSymbolTickRequest.SymbolNames.AddRange(symbols);
			await foreach (OnSymbolTickData item in ExecuteStreamWithReconnect(onSymbolTickRequest, (OnSymbolTickRequest req, Metadata headers, CancellationToken ct) => SubscriptionClient.OnSymbolTick(req, headers, null, ct), (OnSymbolTickReply reply) => reply.Error, (OnSymbolTickReply reply) => reply.Data, cancellationToken))
			{
				yield return item;
			}
		}

		/// <summary>
		/// Subscribes to all trade-related events: orders, deals, positions.
		/// </summary>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>Async stream of trade event data.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the account is not connected.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the stream fails.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the stream returns a known API error.</exception>
		public async IAsyncEnumerable<OnTradeData> OnTradeAsync([EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			OnTradeRequest request = new OnTradeRequest();
			await foreach (OnTradeData item in ExecuteStreamWithReconnect(request, (OnTradeRequest req, Metadata headers, CancellationToken ct) => SubscriptionClient.OnTrade(req, headers, null, ct), (OnTradeReply reply) => reply.Error, (OnTradeReply reply) => reply.Data, cancellationToken))
			{
				yield return item;
			}
		}

		/// <summary>
		/// Subscribes to real-time profit updates for open positions.
		/// </summary>
		/// <param name="intervalMs">Interval in milliseconds to poll server.</param>
		/// <param name="ignoreEmpty">Skip frames with no change.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>Async stream of profit updates.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the stream fails.</exception>
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async IAsyncEnumerable<OnPositionProfitData> OnPositionProfitAsync(int intervalMs, bool ignoreEmpty = true, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			OnPositionProfitRequest request = new OnPositionProfitRequest
			{
				TimerPeriodMilliseconds = intervalMs,
				IgnoreEmptyData = ignoreEmpty
			};
			await foreach (OnPositionProfitData item in ExecuteStreamWithReconnect(request, (OnPositionProfitRequest req, Metadata headers, CancellationToken ct) => SubscriptionClient.OnPositionProfit(req, headers, null, ct), (OnPositionProfitReply reply) => reply.Error, (OnPositionProfitReply reply) => reply.Data, cancellationToken))
			{
				yield return item;
			}
		}

		/// <summary>
		/// Subscribes to updates of position and pending order ticket IDs.
		/// </summary>
		/// <param name="intervalMs">Polling interval in milliseconds.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>Async stream of ticket ID snapshots.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the account is not connected.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the stream fails.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the stream returns a known API error.</exception>
		public async IAsyncEnumerable<OnPositionsAndPendingOrdersTicketsData> OnPositionsAndPendingOrdersTicketsAsync(int intervalMs, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			OnPositionsAndPendingOrdersTicketsRequest request = new OnPositionsAndPendingOrdersTicketsRequest
			{
				TimerPeriodMilliseconds = intervalMs
			};
			await foreach (OnPositionsAndPendingOrdersTicketsData item in ExecuteStreamWithReconnect(request, (OnPositionsAndPendingOrdersTicketsRequest req, Metadata headers, CancellationToken ct) => SubscriptionClient.OnPositionsAndPendingOrdersTickets(req, headers, null, ct), (OnPositionsAndPendingOrdersTicketsReply reply) => reply.Error, (OnPositionsAndPendingOrdersTicketsReply reply) => reply.Data, cancellationToken))
			{
				yield return item;
			}
		}

		/// <summary>
		/// Subscribes to real-time trade transaction events such as order creation, update, or execution.
		/// </summary>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>Async stream of trade transaction replies.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the account is not connected.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the stream fails.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the stream returns a known API error.</exception>
		public async IAsyncEnumerable<OnTradeTransactionData> OnTradeTransactionAsync([EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			OnTradeTransactionRequest request = new OnTradeTransactionRequest();
			await foreach (OnTradeTransactionData item in ExecuteStreamWithReconnect(request, (OnTradeTransactionRequest req, Metadata headers, CancellationToken ct) => SubscriptionClient.OnTradeTransaction(req, headers, null, ct), (OnTradeTransactionReply reply) => reply.Error, (OnTradeTransactionReply reply) => reply.Data, cancellationToken))
			{
				yield return item;
			}
		}

		/// <summary>
		/// Calculates the margin required for a planned trade operation.
		/// </summary>
		/// <param name="request">The request containing symbol, order type, volume, and price.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>The required margin in account currency.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the client is not connected.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns a business error.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if gRPC fails to connect or respond.</exception>
		public async Task<OrderCalcMarginData> OrderCalcMarginAsync(OrderCalcMarginRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			return (await ExecuteWithReconnect((Metadata headers) => TradeFunctionsClient.OrderCalcMargin(request, headers, deadline, cancellationToken), (OrderCalcMarginReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Calculates the margin required for a planned trade operation.
		/// </summary>
		/// <param name="request">The request containing symbol, order type, volume, and price.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>The required margin in account currency.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the client is not connected.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns a business error.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if gRPC fails to connect or respond.</exception>
		public OrderCalcMarginData OrderCalcMargin(OrderCalcMarginRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<OrderCheckData> OrderCheckAsync(OrderCheckRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			return (await ExecuteWithReconnect((Metadata headers) => TradeFunctionsClient.OrderCheck(request, headers, deadline, cancellationToken), (OrderCheckReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Checks whether a trade request can be successfully executed under current market conditions.
		/// </summary>
		/// <param name="request">The trade request to validate.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>Result of the trade request check, including margin and balance details.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public OrderCheckData OrderCheck(OrderCheckRequest request, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return OrderCheckAsync(request, deadline, cancellationToken).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Returns the total number of open positions on the current account.
		/// </summary>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>The total number of open positions.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<PositionsTotalData> PositionsTotalAsync(DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			return (await ExecuteWithReconnect((Metadata headers) => TradeFunctionsClient.PositionsTotal(new Empty(), headers, deadline, cancellationToken), (PositionsTotalReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Returns the total number of open positions on the current account.
		/// </summary>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>The total number of open positions.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public PositionsTotalData PositionsTotal(DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<SymbolsTotalData> SymbolsTotalAsync(bool selectedOnly, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			SymbolsTotalRequest request = new SymbolsTotalRequest
			{
				Mode = selectedOnly
			};
			return (await ExecuteWithReconnect((Metadata headers) => MarketInfoClient.SymbolsTotal(request, headers, deadline, cancellationToken), (SymbolsTotalReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		public SymbolsTotalData SymbolsTotal(bool selectedOnly, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<SymbolExistData> SymbolExistAsync(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			SymbolExistRequest request = new SymbolExistRequest
			{
				Name = symbol
			};
			return (await ExecuteWithReconnect((Metadata headers) => MarketInfoClient.SymbolExist(request, headers, deadline, cancellationToken), (SymbolExistReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Checks if a symbol with the specified name exists (standard or custom).
		/// </summary>
		/// <param name="symbol">The symbol name to check.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>Information about symbol existence and type.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public SymbolExistData SymbolExist(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<SymbolNameData> SymbolNameAsync(int index, bool selected, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			SymbolNameRequest request = new SymbolNameRequest
			{
				Index = index,
				Selected = selected
			};
			return (await ExecuteWithReconnect((Metadata headers) => MarketInfoClient.SymbolName(request, headers, deadline, cancellationToken), (SymbolNameReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Returns the name of a symbol by index.
		/// </summary>
		/// <param name="index">Symbol index (starting at 0).</param>
		/// <param name="selected">True to use only Market Watch symbols.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>The symbol name at the specified index.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public SymbolNameData SymbolName(int index, bool selected, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<SymbolSelectData> SymbolSelectAsync(string symbol, bool select, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			SymbolSelectRequest request = new SymbolSelectRequest
			{
				Symbol = symbol,
				Select = select
			};
			return (await ExecuteWithReconnect((Metadata headers) => MarketInfoClient.SymbolSelect(request, headers, deadline, cancellationToken), (SymbolSelectReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Adds or removes a symbol from Market Watch.
		/// </summary>
		/// <param name="symbol">Symbol name.</param>
		/// <param name="select">True to add, false to remove.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>Success status of the operation.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public SymbolSelectData SymbolSelect(string symbol, bool select, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<SymbolIsSynchronizedData> SymbolIsSynchronizedAsync(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			SymbolIsSynchronizedRequest request = new SymbolIsSynchronizedRequest
			{
				Symbol = symbol
			};
			return (await ExecuteWithReconnect((Metadata headers) => MarketInfoClient.SymbolIsSynchronized(request, headers, deadline, cancellationToken), (SymbolIsSynchronizedReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Checks if the symbol's data is synchronized with the server.
		/// </summary>
		/// <param name="symbol">Symbol name to check.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>True if synchronized, false otherwise.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public SymbolIsSynchronizedData SymbolIsSynchronized(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<SymbolInfoDoubleData> SymbolInfoDoubleAsync(string symbol, SymbolInfoDoubleProperty property, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			SymbolInfoDoubleRequest request = new SymbolInfoDoubleRequest
			{
				Symbol = symbol,
				Type = property
			};
			return (await ExecuteWithReconnect((Metadata headers) => MarketInfoClient.SymbolInfoDouble(request, headers, deadline, cancellationToken), (SymbolInfoDoubleReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Retrieves a double-precision property value of a symbol.
		/// </summary>
		/// <param name="symbol">Symbol name.</param>
		/// <param name="property">The double-type property to retrieve.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>The double property value.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public SymbolInfoDoubleData SymbolInfoDouble(string symbol, SymbolInfoDoubleProperty property, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<SymbolInfoIntegerData> SymbolInfoIntegerAsync(string symbol, SymbolInfoIntegerProperty property, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			SymbolInfoIntegerRequest request = new SymbolInfoIntegerRequest
			{
				Symbol = symbol,
				Type = property
			};
			return (await ExecuteWithReconnect((Metadata headers) => MarketInfoClient.SymbolInfoInteger(request, headers, deadline, cancellationToken), (SymbolInfoIntegerReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Retrieves an integer-type property value of a symbol.
		/// </summary>
		/// <param name="symbol">Symbol name.</param>
		/// <param name="property">The integer property to query.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>The integer property value.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public SymbolInfoIntegerData SymbolInfoInteger(string symbol, SymbolInfoIntegerProperty property, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<SymbolInfoStringData> SymbolInfoStringAsync(string symbol, SymbolInfoStringProperty property, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			SymbolInfoStringRequest request = new SymbolInfoStringRequest
			{
				Symbol = symbol,
				Type = property
			};
			return (await ExecuteWithReconnect((Metadata headers) => MarketInfoClient.SymbolInfoString(request, headers, deadline, cancellationToken), (SymbolInfoStringReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Retrieves a string-type property value of a symbol.
		/// </summary>
		/// <param name="symbol">Symbol name.</param>
		/// <param name="property">The string property to retrieve.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>The string property value.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public SymbolInfoStringData SymbolInfoString(string symbol, SymbolInfoStringProperty property, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<SymbolInfoMarginRateData> SymbolInfoMarginRateAsync(string symbol, ENUM_ORDER_TYPE orderType, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			SymbolInfoMarginRateRequest request = new SymbolInfoMarginRateRequest
			{
				Symbol = symbol,
				OrderType = orderType
			};
			return (await ExecuteWithReconnect((Metadata headers) => MarketInfoClient.SymbolInfoMarginRate(request, headers, deadline, cancellationToken), (SymbolInfoMarginRateReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Retrieves the margin rates for a given symbol and order type.
		/// </summary>
		/// <param name="symbol">Symbol name.</param>
		/// <param name="orderType">The order type (buy/sell/etc).</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>The initial and maintenance margin rates.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public SymbolInfoMarginRateData SymbolInfoMarginRate(string symbol, ENUM_ORDER_TYPE orderType, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<MrpcMqlTick> SymbolInfoTickAsync(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			SymbolInfoTickRequest request = new SymbolInfoTickRequest
			{
				Symbol = symbol
			};
			return (await ExecuteWithReconnect((Metadata headers) => MarketInfoClient.SymbolInfoTick(request, headers, deadline, cancellationToken), (SymbolInfoTickRequestReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Retrieves the current tick data (bid, ask, last, volume) for a given symbol.
		/// </summary>
		/// <param name="symbol">Symbol name to fetch tick info for.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>The latest tick information.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public MrpcMqlTick SymbolInfoTick(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<SymbolInfoSessionQuoteData> SymbolInfoSessionQuoteAsync(string symbol, DayOfWeek dayOfWeek, uint sessionIndex, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			SymbolInfoSessionQuoteRequest request = new SymbolInfoSessionQuoteRequest
			{
				Symbol = symbol,
				DayOfWeek = dayOfWeek,
				SessionIndex = sessionIndex
			};
			return (await ExecuteWithReconnect((Metadata headers) => MarketInfoClient.SymbolInfoSessionQuote(request, headers, deadline, cancellationToken), (SymbolInfoSessionQuoteReply r) => r.Error, deadline, cancellationToken)).Data;
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public SymbolInfoSessionQuoteData SymbolInfoSessionQuote(string symbol, DayOfWeek dayOfWeek, uint sessionIndex, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<SymbolInfoSessionTradeData> SymbolInfoSessionTradeAsync(string symbol, DayOfWeek dayOfWeek, uint sessionIndex, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			SymbolInfoSessionTradeRequest request = new SymbolInfoSessionTradeRequest
			{
				Symbol = symbol,
				DayOfWeek = dayOfWeek,
				SessionIndex = sessionIndex
			};
			return (await ExecuteWithReconnect((Metadata headers) => MarketInfoClient.SymbolInfoSessionTrade(request, headers, deadline, cancellationToken), (SymbolInfoSessionTradeReply r) => r.Error, deadline, cancellationToken)).Data;
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public SymbolInfoSessionTradeData SymbolInfoSessionTrade(string symbol, DayOfWeek dayOfWeek, uint sessionIndex, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<MarketBookAddData> MarketBookAddAsync(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			MarketBookAddRequest request = new MarketBookAddRequest
			{
				Symbol = symbol
			};
			return (await ExecuteWithReconnect((Metadata headers) => MarketInfoClient.MarketBookAdd(request, headers, deadline, cancellationToken), (MarketBookAddReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Opens the Depth of Market (DOM) for a symbol and subscribes to updates.
		/// </summary>
		/// <param name="symbol">Symbol name to subscribe.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>True if DOM subscription was successful.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public MarketBookAddData MarketBookAdd(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<MarketBookReleaseData> MarketBookReleaseAsync(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			MarketBookReleaseRequest request = new MarketBookReleaseRequest
			{
				Symbol = symbol
			};
			return (await ExecuteWithReconnect((Metadata headers) => MarketInfoClient.MarketBookRelease(request, headers, deadline, cancellationToken), (MarketBookReleaseReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Releases the Depth of Market (DOM) for a symbol and stops receiving updates.
		/// </summary>
		/// <param name="symbol">Symbol name to unsubscribe.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>True if DOM release was successful.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public MarketBookReleaseData MarketBookRelease(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public async Task<MarketBookGetData> MarketBookGetAsync(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			MarketBookGetRequest request = new MarketBookGetRequest
			{
				Symbol = symbol
			};
			return (await ExecuteWithReconnect((Metadata headers) => MarketInfoClient.MarketBookGet(request, headers, deadline, cancellationToken), (MarketBookGetReply r) => r.Error, deadline, cancellationToken)).Data;
		}

		/// <summary>
		/// Gets the current Depth of Market (DOM) data for a symbol.
		/// </summary>
		/// <param name="symbol">Symbol name.</param>
		/// <param name="deadline">Optional gRPC deadline.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>A list of book entries for the symbol's DOM.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5" />
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5" />
		/// <exception cref="T:Grpc.Core.RpcException" />
		public MarketBookGetData MarketBookGet(string symbol, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return MarketBookGetAsync(symbol, deadline, cancellationToken).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Retrieves a double-precision account property (e.g. balance, equity, margin).
		/// </summary>
		/// <param name="property">The account double property to retrieve.</param>
		/// <param name="deadline">Optional deadline after which the call will be cancelled.</param>
		/// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
		/// <returns>The double value of the requested account property.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the client is not connected.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns a business error.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
		public async Task<double> AccountInfoDoubleAsync(AccountInfoDoublePropertyType property, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			AccountInfoDoubleRequest request = new AccountInfoDoubleRequest
			{
				PropertyId = property
			};
			return (await ExecuteWithReconnect((Metadata headers) => AccountInformationClient.AccountInfoDouble(request, headers, deadline, cancellationToken), (AccountInfoDoubleReply _) => (Error?)null, deadline, cancellationToken)).Data.RequestedValue;
		}

		/// <summary>
		/// Retrieves a double-precision account property (e.g. balance, equity, margin).
		/// </summary>
		/// <param name="property">The account double property to retrieve.</param>
		/// <param name="deadline">Optional deadline after which the call will be cancelled.</param>
		/// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
		/// <returns>The double value of the requested account property.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the client is not connected.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns a business error.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
		public double AccountInfoDouble(AccountInfoDoublePropertyType property, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the client is not connected.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns a business error.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
		public async Task<long> AccountInfoIntegerAsync(AccountInfoIntegerPropertyType property, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			AccountInfoIntegerRequest request = new AccountInfoIntegerRequest
			{
				PropertyId = property
			};
			return (await ExecuteWithReconnect((Metadata headers) => AccountInformationClient.AccountInfoInteger(request, headers, deadline, cancellationToken), (AccountInfoIntegerReply _) => (Error?)null, deadline, cancellationToken)).Data.RequestedValue;
		}

		/// <summary>
		/// Retrieves an integer account property (e.g. login, leverage, trade mode).
		/// </summary>
		/// <param name="property">The account integer property to retrieve.</param>
		/// <param name="deadline">Optional deadline after which the call will be cancelled.</param>
		/// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
		/// <returns>The integer value of the requested account property.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the client is not connected.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns a business error.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
		public long AccountInfoInteger(AccountInfoIntegerPropertyType property, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the client is not connected.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns a business error.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
		public async Task<string> AccountInfoStringAsync(AccountInfoStringPropertyType property, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Id == default(Guid))
			{
				throw new ConnectExceptionMT5("Please call Connect method firstly");
			}
			AccountInfoStringRequest request = new AccountInfoStringRequest
			{
				PropertyId = property
			};
			return (await ExecuteWithReconnect((Metadata headers) => AccountInformationClient.AccountInfoString(request, headers, deadline, cancellationToken), (AccountInfoStringReply _) => (Error?)null, deadline, cancellationToken)).Data.RequestedValue;
		}

		/// <summary>
		/// Retrieves a string account property (e.g. account name, currency, server).
		/// </summary>
		/// <param name="property">The account string property to retrieve.</param>
		/// <param name="deadline">Optional deadline after which the call will be cancelled.</param>
		/// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
		/// <returns>The string value of the requested account property.</returns>
		/// <exception cref="T:mt5_term_api.ConnectExceptionMT5">Thrown if the client is not connected.</exception>
		/// <exception cref="T:mt5_term_api.ApiExceptionMT5">Thrown if the server returns a business error.</exception>
		/// <exception cref="T:Grpc.Core.RpcException">Thrown if the gRPC call fails.</exception>
		public string AccountInfoString(AccountInfoStringPropertyType property, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return AccountInfoStringAsync(property, deadline, cancellationToken).GetAwaiter().GetResult();
		}
	}
}