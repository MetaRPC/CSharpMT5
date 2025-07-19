using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mt5_term_api;

namespace MetaRPC.CSharpMT5
{
    public class MT5Options 
    {
        public string ServerName { get; set; }
        public ulong AccountId { get; set; }
        public string Password { get; set; }

        public string Host { get; set; }         
        public int Port { get; set; } = 443;
    }

    public class Program
    {
        private readonly ILogger<Program> _logger;
        private readonly IConfiguration _configuration;
        private readonly MT5Account _mt5Account;
                                  

        public Program()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });
            _logger = loggerFactory.CreateLogger<Program>();

            var options = _configuration.GetSection("MT5Options").Get<MT5Options>();
            _mt5Account = new MT5Account(options.AccountId, options.Password);
        }

        public static async Task Main(string[] args)
        {
            var program = new Program();
            await program.Run();
        }

        public async Task Run()
        {
            try
            {
                // 1) Connecting to the server
                _logger.LogInformation("Connecting to server...");
                var options = _configuration.GetSection("MT5Options").Get<MT5Options>();
                await _mt5Account.ConnectByHostPortAsync(
                    host: options.Host,
                    port: options.Port,
                    baseChartSymbol: "EURUSD",
                    waitForTerminalIsAlive: true
                );

                // 2) Launching all methods
                await ShowAccountInfo();
                await DoOrderOperations();
                await ShowPositions();
                await ShowMarketInfo();
                await ShowSymbolProperties();
                await ShowTradeFunctions();
                await DoStreaming();

                // 3) Everything is ready — waiting for a key press to prevent the console from closing instantly
                _logger.LogInformation("All examples completed! Press any key to exit.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in program");
            }
        }
        private async Task ShowAccountInfo()
        {
            _logger.LogInformation("=== Account Info ===");

            var summary = await _mt5Account.AccountSummaryAsync();
            _logger.LogInformation(
                "Account Summary: Balance={Balance}",
                summary.AccountBalance);

            var balance = await _mt5Account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountBalance);
            _logger.LogInformation(
                "AccountInfoDouble: Balance={Balance}",
                balance);

            var leverage = await _mt5Account.AccountInfoIntegerAsync(AccountInfoIntegerPropertyType.AccountLeverage);
            _logger.LogInformation(
                "AccountInfoInteger: Leverage={Leverage}",
                leverage);

            var currency = await _mt5Account.AccountInfoStringAsync(AccountInfoStringPropertyType.AccountCurrency);
            _logger.LogInformation(
                "AccountInfoString: Currency={Currency}",
                currency);
        }


        private async Task DoOrderOperations()
        {
            _logger.LogInformation("=== Orders ===");

            // 1) Open orders
            var openedOrdersData = await _mt5Account.OpenedOrdersAsync();
            _logger.LogInformation(
                "OpenedOrdersAsync: Count={Count}",
                openedOrdersData.OpenedOrders.Count);

            // 2) Tickets for open orders
            var openedTicketsData = await _mt5Account.OpenedOrdersTicketsAsync();
            _logger.LogInformation(
                "OpenedOrdersTicketsAsync: {Tickets}",
                string.Join(", ", openedTicketsData.OpenedOrdersTickets));

            // 3) Order history for the last 7 days
            var historyData = await _mt5Account.OrderHistoryAsync(
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);
            _logger.LogInformation(
                "OrderHistoryAsync: Count={Count}",
                historyData.HistoryData.Count);

            // 4) Current tick
            var symbol = Constants.DefaultSymbol;
            var tick = await _mt5Account.SymbolInfoTickAsync(symbol);

            _logger.LogInformation(
                "SymbolInfoTickAsync: Symbol={Symbol}, Bid={Bid}, Ask={Ask}, Last={Last}, Volume={Volume}, Time={Time}",
                symbol,         
                tick.Bid,
                tick.Ask,
                tick.Last,
                tick.Volume,
                tick.Time
            );

            // 5) Opening an order
            var sendRequest = new OrderSendRequest
            {
                Symbol = Constants.DefaultSymbol,
                Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
                Volume = Constants.DefaultVolume,
                Price = tick.Ask
            };
            var sendResult = await _mt5Account.OrderSendAsync(sendRequest);
            ulong ticket = sendResult.Order;
            _logger.LogInformation("OrderSendAsync: Order={Order}", ticket);

            // 6) Modification
            var modifyRequest = new OrderModifyRequest
            {
                Ticket = ticket,
                Price = tick.Ask,
                StopLoss = tick.Ask - 0.0010,
                TakeProfit = tick.Ask + 0.0010
            };
            var modifyResult = await _mt5Account.OrderModifyAsync(modifyRequest);
            _logger.LogInformation("OrderModifyAsync: OrderId={Order}", modifyResult.Order);

            // 7) Closure
            var closeRequest = new OrderCloseRequest
            {
                Ticket = ticket,
                Volume = Constants.DefaultVolume
            };
            var closeResult = await _mt5Account.OrderCloseAsync(closeRequest);
            _logger.LogInformation(
                "OrderCloseAsync: Ticket={Ticket} ReturnCode={Code} Description={Desc}",
                ticket,
                closeResult.ReturnedCode,
                closeResult.ReturnedCodeDescription);
        }


        private async Task ShowPositions()
        {
            _logger.LogInformation("=== Positions ===");

            var positions = await _mt5Account.PositionsTotalAsync();
            _logger.LogInformation("PositionsTotalAsync full: {@positions}", positions);


            var history = await _mt5Account.PositionsHistoryAsync(
                AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeAsc,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow);
            _logger.LogInformation("PositionsHistoryAsync: Count={Count}", history.HistoryPositions.Count);
        }

        private async Task ShowMarketInfo()
        {
            _logger.LogInformation("=== Market Info ===");

            var total = await _mt5Account.SymbolsTotalAsync(false);
            _logger.LogInformation(
                "SymbolsTotal: All={All}",
                total.Total);

            var selectedTotal = await _mt5Account.SymbolsTotalAsync(true);
            _logger.LogInformation(
                "SymbolsTotal: Selected={Selected}",
                selectedTotal.Total);

            var exists = await _mt5Account.SymbolExistAsync(Constants.DefaultSymbol);
            _logger.LogInformation(
                "SymbolExistAsync: Exists={Exists}",
                exists.Exists);

            var symbolName = await _mt5Account.SymbolNameAsync(0, false);
            _logger.LogInformation(
                "SymbolNameAsync: FirstSymbol={FirstSymbol}",
                symbolName.Name);

            var request = new SymbolParamsManyRequest();
            var symbols = await _mt5Account.SymbolParamsManyAsync(request);
            _logger.LogInformation(
                "SymbolParamsManyAsync: Count={Count}",
                symbols.SymbolInfos.Count);

            var select = await _mt5Account.SymbolSelectAsync(Constants.DefaultSymbol, true);
            _logger.LogInformation(
                "SymbolSelectAsync: Selected={Selected}",
                select.Success);

            var sync = await _mt5Account.SymbolIsSynchronizedAsync(Constants.DefaultSymbol);
            _logger.LogInformation(
                "SymbolIsSynchronizedAsync: IsSync={IsSync}",
                sync.Synchronized);
        }


        private async Task ShowSymbolProperties()
        {
            _logger.LogInformation("=== Symbol Properties ===");

            var doubleProp = await _mt5Account.SymbolInfoDoubleAsync(
                Constants.DefaultSymbol,
                SymbolInfoDoubleProperty.SymbolAsk);
            _logger.LogInformation(
                "SymbolInfoDouble: Ask={Ask}",
                doubleProp.Value);

            var intProp = await _mt5Account.SymbolInfoIntegerAsync(
                Constants.DefaultSymbol,
                SymbolInfoIntegerProperty.SymbolVisible);
            _logger.LogInformation(
                "SymbolInfoInteger: Visible={Visible}",
                intProp.Value);

            var stringProp = await _mt5Account.SymbolInfoStringAsync(
                Constants.DefaultSymbol,
                SymbolInfoStringProperty.SymbolCurrencyBase);
            _logger.LogInformation(
                "SymbolInfoString: BaseCurrency={BaseCurrency}",
                stringProp.Value);

            var marginRate = await _mt5Account.SymbolInfoMarginRateAsync(
                Constants.DefaultSymbol,
                ENUM_ORDER_TYPE.OrderTypeBuy);
            _logger.LogInformation(
                "SymbolInfoMarginRate: InitialMarginRate={InitialMarginRate}",
                marginRate.InitialMarginRate);

            var tick = await _mt5Account.SymbolInfoTickAsync(Constants.DefaultSymbol);
            _logger.LogInformation(
                "SymbolInfoTickAsync: Bid={Bid} Ask={Ask}",
                tick.Bid,
                tick.Ask);

            var sessionQuote = await _mt5Account.SymbolInfoSessionQuoteAsync(
                Constants.DefaultSymbol,
                mt5_term_api.DayOfWeek.Monday,
                0);
            var fromUtc = sessionQuote.From.ToDateTime(); // UTC
            var toUtc = sessionQuote.To.ToDateTime();
            _logger.LogInformation(
                "SymbolInfoSessionQuote: FromUtc={FromUtc:O} ToUtc={ToUtc:O}",
                fromUtc,
                toUtc);

            var sessionTrade = await _mt5Account.SymbolInfoSessionTradeAsync(
                Constants.DefaultSymbol,
                mt5_term_api.DayOfWeek.Monday,
                0);
            var startUtc = sessionTrade.From.ToDateTime(); // UTC
            var endUtc = sessionTrade.To.ToDateTime();
            _logger.LogInformation(
                "SymbolInfoSessionTrade: StartUtc={StartUtc:O} EndUtc={EndUtc:O}",
                startUtc,
                endUtc);
        }


        private async Task ShowTradeFunctions()
        {
            _logger.LogInformation("=== Trade Functions ===");

            // 1) Getting the current tick
            var tick = await _mt5Account.SymbolInfoTickAsync(Constants.DefaultSymbol);

            await _mt5Account.SymbolSelectAsync(Constants.DefaultSymbol, true);
            _logger.LogInformation("Preparing OrderCheck: Symbol={Symbol}, Ask={Ask}", Constants.DefaultSymbol, tick.Ask);


            // 2) We calculate the required margin
            var margin = await _mt5Account.OrderCalcMarginAsync(new OrderCalcMarginRequest
            {
                Symbol = Constants.DefaultSymbol,
                OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,   // TF-enum
                Volume = Constants.DefaultVolume,
                OpenPrice = tick.Ask
            });
            _logger.LogInformation("OrderCalcMargin: Margin={Margin}", margin.Margin);

            // 3) We are forming a request for verification of the application
            var checkRequest = new OrderCheckRequest
            {
                MqlTradeRequest = new MrpcMqlTradeRequest
                {
                    Symbol = Constants.DefaultSymbol,
                    Volume = Constants.DefaultVolume,
                    Price = tick.Ask,
                    StopLimit = tick.Ask + 0.0005,              // examples of values
                    StopLoss = tick.Ask - 0.0010,
                    TakeProfit = tick.Ask + 0.0010,
                    Deviation = 10,                             // max. slippage
                    OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,                                               
                    Expiration = Timestamp.FromDateTime(DateTime.UtcNow),
                    Comment = string.Empty,
                    Position = 0,
                    PositionBy = 0,

                    
                }
            };

            // 4) We perform the verification and log the result.
            var checkResult = await _mt5Account.OrderCheckAsync(checkRequest);
            _logger.LogInformation(
                "OrderCheck: Margin={Margin}, FreeMargin={FreeMargin}",
                checkResult.MqlTradeCheckResult.Margin,
                checkResult.MqlTradeCheckResult.FreeMargin
            );
        }

        private async Task DoStreaming()
        {
            _logger.LogInformation("=== Streaming ===");

            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var tickTask = Task.Run(async () =>
            {
                await foreach (var tick in _mt5Account.OnSymbolTickAsync(new[] { Constants.DefaultSymbol }, token))
                {
                    _logger.LogInformation(
                        "OnSymbolTickAsync: Symbol={Symbol} Ask={Ask}",
                        tick.SymbolTick.Symbol,
                        tick.SymbolTick.Ask);
                }
            }, token);

            var tradeTask = Task.Run(async () =>
            {
                await foreach (var trade in _mt5Account.OnTradeAsync(token))
                {
                    _logger.LogInformation("OnTradeAsync: Trade event received");
                }
            }, token);

            var profitTask = Task.Run(async () =>
            {
                await foreach (var profit in _mt5Account.OnPositionProfitAsync(1000, true, token))
                {
                    _logger.LogInformation("OnPositionProfitAsync: Update received");
                }
            }, token);

            var ticketsTask = Task.Run(async () =>
            {
                await foreach (var tickets in _mt5Account.OnPositionsAndPendingOrdersTicketsAsync(1000, token))
                {
                    _logger.LogInformation("OnPositionsAndPendingOrdersTicketsAsync: Update received");
                }
            }, token);

            _logger.LogInformation("Streaming for {Seconds} seconds...", 5);
            await Task.Delay(TimeSpan.FromSeconds(5));
            cts.Cancel();

            await Task.WhenAll(tickTask, tradeTask, profitTask, ticketsTask);

            _logger.LogInformation("Streaming stopped.");
        }

    }
}
