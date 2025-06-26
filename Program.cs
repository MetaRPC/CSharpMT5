
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mt5_term_api;

namespace MetaRPC.CSharpMT5
{
    public class MT5Options
    {
        public ulong AccountId { get; set; }
        public string Password { get; set; }
    }

    public class Program
    {
        private readonly ILogger<Program> _logger;
        private readonly IConfiguration _configuration;
        private readonly MT5Account _mt5Account;

        private static class Constants
        {
            public const string DefaultSymbol = "EURUSD";
            public const double DefaultVolume = 0.1;
            public const string DefaultServer = "MetaQuotes-Demo";
        }

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
                _logger.LogInformation("Connecting to server...");
                _mt5Account.ConnectByServerName(Constants.DefaultServer);

                await ShowAccountInfo();
                await DoOrderOperations();
                await ShowPositions();
                await ShowMarketInfo();
                await ShowSymbolProperties();
                await ShowTradeFunctions();
                await DoStreaming();       

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
            _logger.LogInformation($"Account Summary: Balance={summary.AccountBalance}");

            var balance = await _mt5Account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountBalance);
            _logger.LogInformation($"AccountInfoDouble: Balance={balance}");

            var leverage = await _mt5Account.AccountInfoIntegerAsync(AccountInfoIntegerPropertyType.AccountLeverage);
            _logger.LogInformation($"AccountInfoInteger: Leverage={leverage}");

            var currency = await _mt5Account.AccountInfoStringAsync(AccountInfoStringPropertyType.AccountCurrency);
            _logger.LogInformation($"AccountInfoString: Currency={currency}");
        }

        private async Task DoOrderOperations()
        {
            _logger.LogInformation("=== Orders ===");

            var opened = await _mt5Account.OpenedOrdersAsync();
            _logger.LogInformation($"OpenedOrdersAsync: Count={opened.OpenedOrders.Count}");

            var openedTickets = await _mt5Account.OpenedOrdersTicketsAsync();
            _logger.LogInformation($"OpenedOrdersTicketsAsync: {string.Join(", ", openedTickets.OpenedOrdersTickets)}");

            var history = await _mt5Account.OrderHistoryAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
            _logger.LogInformation($"OrderHistoryAsync: Count={history.HistoryData.Count}");

            var tick = await _mt5Account.SymbolInfoTickAsync(Constants.DefaultSymbol);

            var orderRequest = new OrderSendRequest
            {
                Symbol = Constants.DefaultSymbol,
                Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
                Volume = Constants.DefaultVolume,
                Price = tick.Ask
            };
            var orderResult = await _mt5Account.OrderSendAsync(orderRequest);
            _logger.LogInformation($"OrderSendAsync: Order={orderResult.Order}");

            var modifyRequest = new OrderModifyRequest
            {
                Ticket = orderResult.Order
                // add modify fields here
            };
            var modifyResult = await _mt5Account.OrderModifyAsync(modifyRequest);
            _logger.LogInformation("OrderModifyAsync: Ticket={Ticket}", modifyResult.Ticket); // BP1
            _logger.LogInformation("Full: {@Object}", modifyResult);

            var closeRequest = new OrderCloseRequest
            {
                Ticket = orderResult.Order,
                Volume = Constants.DefaultVolume
            };
            var closeResult = await _mt5Account.OrderCloseAsync(closeRequest);
            _logger.LogInformation("OrderCloseAsync: Ticket={Ticket}", closeResult.Ticke);

        }

        private async Task ShowPositions()
        {
            _logger.LogInformation("=== Positions ===");
            var positions = await _mt5Account.PositionsTotalAsync();
            //  _logger.LogInformation($"PositionsTotalAsync: Total={positions.Count}");
            _logger.LogInformation("PositionsTotalAsync full: {@positions}", positions); //BP2


            var history = await _mt5Account.PositionsHistoryAsync(
                AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.PositionsHistorySortByCloseTimeAsc


,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow);
            _logger.LogInformation($"PositionsHistoryAsync: Count={history.Positions.Count}");
        }

        private async Task ShowMarketInfo()
        {
            _logger.LogInformation("=== Market Info ===");

            var total = await _mt5Account.SymbolsTotalAsync(false);
            _logger.LogInformation($"SymbolsTotal: All={total.Total}");

            var selectedTotal = await _mt5Account.SymbolsTotalAsync(true);
            _logger.LogInformation($"SymbolsTotal: Selected={selectedTotal.Total}");

            var exists = await _mt5Account.SymbolExistAsync(Constants.DefaultSymbol);
            _logger.LogInformation($"SymbolExistAsync: Exists={exists.Exists}");

            var symbolName = await _mt5Account.SymbolNameAsync(0, false);
            _logger.LogInformation($"SymbolNameAsync: First symbol={symbolName.Name}");

            var request = new SymbolParamsManyRequest();
            var symbols = await _mt5Account.SymbolParamsManyAsync(request);
            _logger.LogInformation($"SymbolParamsManyAsync: Count={symbols.Symbols.Count}");

            var select = await _mt5Account.SymbolSelectAsync(Constants.DefaultSymbol, true);
            _logger.LogInformation($"SymbolSelectAsync: Selected={select.Success}");

            var sync = await _mt5Account.SymbolIsSynchronizedAsync(Constants.DefaultSymbol);
            _logger.LogInformation($"SymbolIsSynchronizedAsync: IsSync={sync.IsSynchronized}");
        }

        private async Task ShowSymbolProperties()
        {
            _logger.LogInformation("=== Symbol Properties ===");

            var doubleProp = await _mt5Account.SymbolInfoDoubleAsync(Constants.DefaultSymbol, SymbolInfoDoubleProperty.Ask);
            _logger.LogInformation($"SymbolInfoDouble: Ask={doubleProp.Value}");

            var intProp = await _mt5Account.SymbolInfoIntegerAsync(Constants.DefaultSymbol, SymbolInfoIntegerProperty.Visible);
            _logger.LogInformation($"SymbolInfoInteger: Visible={intProp.Value}");

            var stringProp = await _mt5Account.SymbolInfoStringAsync(Constants.DefaultSymbol, SymbolInfoStringProperty.CurrencyBase);
            _logger.LogInformation($"SymbolInfoString: BaseCurrency={stringProp.Value}");

            var marginRate = await _mt5Account.SymbolInfoMarginRateAsync(Constants.DefaultSymbol, ENUM_ORDER_TYPE.OrderTypeBuy);
            _logger.LogInformation($"SymbolInfoMarginRate: InitialMargin={marginRate.MarginInitial}");

            var tick = await _mt5Account.SymbolInfoTickAsync(Constants.DefaultSymbol);
            _logger.LogInformation($"SymbolInfoTickAsync: Bid={tick.Bid} Ask={tick.Ask}");

            var sessionQuote = await _mt5Account.SymbolInfoSessionQuoteAsync(Constants.DefaultSymbol, mt5_term_api.DayOfWeek.Monday, 0);
            _logger.LogInformation($"SymbolInfoSessionQuote: Start={sessionQuote.StartTime}");

            var sessionTrade = await _mt5Account.SymbolInfoSessionTradeAsync(Constants.DefaultSymbol, mt5_term_api.DayOfWeek.Monday, 0);
            _logger.LogInformation($"SymbolInfoSessionTrade: Start={sessionTrade.StartTime}");
        }

        private async Task ShowTradeFunctions()
        {
            _logger.LogInformation("=== Trade Functions ===");

            var tick = await _mt5Account.SymbolInfoTickAsync(Constants.DefaultSymbol);

            var margin = await _mt5Account.OrderCalcMarginAsync(new OrderCalcMarginRequest
            {
                Symbol = Constants.DefaultSymbol,
                Type = ENUM_ORDER_TYPE.OrderTypeBuy,
                Lots = Constants.DefaultVolume,
                PriceAsk = tick.Ask
            });
            _logger.LogInformation($"OrderCalcMargin: Margin={margin.Margin}");

            var check = await _mt5Account.OrderCheckAsync(new OrderCheckRequest
            {
                Symbol = Constants.DefaultSymbol,
                OrderType = ENUM_ORDER_TYPE.OrderTypeBuy,
                Volume = Constants.DefaultVolume,
                Price = tick.Ask
            });
            _logger.LogInformation($"OrderCheck: Margin={check.Margin}");

            var total = await _mt5Account.PositionsTotalAsync();
            _logger.LogInformation($"PositionsTotalAsync: {total.PositionsTotal}");
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
                    _logger.LogInformation($"OnSymbolTickAsync: {tick.SymbolTick.Symbol} Ask={tick.SymbolTick.Ask}");
                }
            }, token);

            var tradeTask = Task.Run(async () =>
            {
                await foreach (var trade in _mt5Account.OnTradeAsync(token))
                {
                    _logger.LogInformation($"OnTradeAsync: Trade event received");
                }
            }, token);

            var profitTask = Task.Run(async () =>
            {
                await foreach (var profit in _mt5Account.OnPositionProfitAsync(1000, true, token))
                {
                    _logger.LogInformation($"OnPositionProfitAsync: Update received");
                }
            }, token);

            var ticketsTask = Task.Run(async () =>
            {
                await foreach (var tickets in _mt5Account.OnPositionsAndPendingOrdersTicketsAsync(1000, token))
                {
                    _logger.LogInformation($"OnPositionsAndPendingOrdersTicketsAsync: Update received");
                }
            }, token);

            _logger.LogInformation("Streaming for 5 seconds...");
            await Task.Delay(5000);
            cts.Cancel();

            await Task.WhenAll(tickTask, tradeTask, profitTask, ticketsTask);

            _logger.LogInformation("Streaming stopped.");
        }
    }
  
    }

