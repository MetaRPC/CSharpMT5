using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using MetaRPC.CSharpMT5;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mt5_term_api;
using static Google.Rpc.Context.AttributeContext.Types;

namespace MetaRPC.CSharpMT5
{
    public class Program
    {
        private readonly MT5Account _mt5Account;
        private readonly ILogger<Program> _logger;
        private readonly IConfiguration _configuration;
        private bool _disposed;

        private static class Constants
        {
            public const string DefaultSymbol = "EURUSD";
            public const double DefaultVolume = 0.1;
            public const int DefaultPageSize = 100;
            public const string DefaultServer = "MetaQuotes-Demo";
            public const int ConnectionRetryAttempts = 3;
            public const int ConnectionRetryDelay = 1000; // milliseconds
        }

        public Program()

        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
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

        static async Task Main(string[] args)
        {

        }

        async Task Run(string[] args)
        {
            try
            {
                if (!await TryConnectAsync())
                {
                    _logger.LogError("Failed to connect to MT5 server");
                    return;
                }

                await ExecuteTradeOperations();

                _logger.LogInformation("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Application error occurred");
            }
        }

        private async Task ExecuteTradeOperations()
        {
            using var cts = new CancellationTokenSource();

            await CheckOpenedOrders();
            await CheckOrderHistoryAsync();
            ShowEnumValues();

            // Добавляем вызовы других методов
            await OrderSend();
            await RealTimeQuotes();
            GetSymbolParamsSync();
            GetTickDataSync();
            SendOrderSync();
            ModifyOrderSync();
            CloseOrderSync();

            // Start quote monitoring in background
            var monitoringTask = MonitorQuotes(cts.Token);

            // Execute other operations
            await Task.WhenAll(
                GetTickDataAsync(),
                SendOrderAsync(),
                GetSymbolParamsAsync()
            );

            // Cancel quote monitoring
            cts.Cancel();
            await monitoringTask;
        }

        private async Task<bool> TryConnectAsync(int attempts = Constants.ConnectionRetryAttempts)
        {
            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    _mt5Account.ConnectByServerName(Constants.DefaultServer);
                    _logger.LogInformation($"Connected successfully. Account balance: {_mt5Account.AccountSummary().AccountBalance}");
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Connection attempt {i + 1} failed: {ex.Message}");
                    await Task.Delay(Constants.ConnectionRetryDelay);
                }
            }
            return false;
        }

        private async Task CheckOpenedOrders()
        {
            try
            {
                var openedOrders = await _mt5Account.OpenedOrdersAsync();
                _logger.LogInformation($"Opened orders: {openedOrders}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking opened orders");
                throw;
            }
        }

        private async Task MonitorQuotes(CancellationToken cancellationToken = default)
        {
            try
            {
                await foreach (var update in _mt5Account.OnSymbolTickAsync(new[] { Constants.DefaultSymbol })
                    .WithCancellation(cancellationToken))
                {
                    _logger.LogInformation($"Quote update for {update?.SymbolTick?.Symbol}: {update?.SymbolTick?.Ask}");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Quote monitoring cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring quotes");
            }
        }

        private async Task SendOrderAsync()
        {
            try
            {
                var request = new OrderSendRequest
                {
                    Symbol = Constants.DefaultSymbol,
                    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
                    Volume = Constants.DefaultVolume,
                    Price = _mt5Account.SymbolInfoTick(Constants.DefaultSymbol).Ask
                };

                var result = await _mt5Account.OrderSendAsync(request);
                _logger.LogInformation($"Order sent successfully. Order ID: {result.Order}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order");
                throw;
            }
        }

        private async Task CheckOrderHistoryAsync()
        {
            try
            {
                var from = DateTime.Now.AddDays(-7);
                var to = DateTime.Now;

                var orderHistory = await _mt5Account.OrderHistoryAsync(
                    from: from,
                    to: to,
                    sortMode: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
                    pageNumber: 0,
                    itemsPerPage: Constants.DefaultPageSize
                );

                _logger.LogInformation($"Order History from {from} to {to}: {orderHistory}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking order history");
                throw;
            }
        }

        private async Task GetSymbolParamsAsync()
        {
            try
            {
                var request = new SymbolParamsManyRequest
                {
                    // Configure request parameters
                };

                var symbolParams = await _mt5Account.SymbolParamsManyAsync(request);
                _logger.LogInformation($"Symbol Parameters: {symbolParams}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting symbol parameters");
                throw;
            }
        }

        private async Task GetTickDataAsync()
        {
            try
            {
                var symbols = new[] { Constants.DefaultSymbol };
                var tickData = await _mt5Account.TickValueWithSizeAsync(symbols);
                _logger.LogInformation($"Tick data: {tickData}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tick data");
                throw;
            }
        }

        private void ShowEnumValues()
        {
            _logger.LogInformation("Available values for AH_ENUM_POSITIONS_HISTORY_SORT_TYPE:");
            foreach (var value in Enum.GetValues(typeof(AH_ENUM_POSITIONS_HISTORY_SORT_TYPE)))
            {
                _logger.LogInformation(value.ToString());
            }
        }

        private async Task ExecuteTradeOperation(Func<Task> operation)
        {
            try
            {
                await operation();
            }
            catch (ApiExceptionMT5 ex)
            {
                _logger.LogError($"MT5 API Error: {ex.ErrorCode} - {ex.Message}");
                throw;
            }
            catch (RpcException ex)
            {
                _logger.LogError($"RPC Error: {ex.StatusCode} - {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                throw;
            }
        }

        private void ConnectByServerName()
        {
            _logger.LogInformation("Connecting to mt5 server...");
            MT5Account account = new MT5Account(5036292718, "_0AeXaFk");
            account.ConnectByServerName("MetaQuotes-Demo");
            _logger.LogInformation($"Connected Account balance = {account.AccountSummary().AccountBalance}");
        }

        private void ConnectByHostPort()
        {
            _logger.LogInformation("Connecting to mt5 server...");
            MT5Account account = new MT5Account(5036292718, "_0AeXaFk");
            account.Connect("78.140.180.198", 443);
            _logger.LogInformation($"Connected Account balance = {account.AccountSummary().AccountBalance}");
        }

        private async Task OrderSend()
        {
            MT5Account account = new MT5Account(62333850, "tecimil4");
            account.ConnectByServerName("MetaQuotes-Demo");
            _logger.LogInformation($"Connected Account balance = {account.AccountSummary().AccountBalance}");
            var symbol = "EURUSD";

            await foreach (var update in account.OnSymbolTickAsync(new string[] { symbol }))
            {
                _logger.LogInformation($"Got first quote for {update?.SymbolTick.Symbol}");
                break;
            }

            var ask = account.SymbolInfoTick(symbol).Ask;
            _logger.LogInformation($"{symbol} ask = {ask}");

            var req = new OrderSendRequest
            {
                Symbol = symbol,
                Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
                Volume = 0.1,
                Price = ask
            };

            var result = account.OrderSend(req);
            _logger.LogInformation($"Position {result.Order} opened");
        }

        private async Task RealTimeQuotes()
        {
            MT5Account account = new MT5Account(10006638498, "TwJ*X0Gb");
            account.ConnectByServerName("MetaQuotes-Demo");
            _logger.LogInformation($"Connected Account balance = {account.AccountSummary().AccountBalance}");

            await foreach (var update in account.OnSymbolTickAsync(new string[] { "EURUSD" }))
            {
                _logger.LogInformation($"Quote: {update?.SymbolTick?.Ask}");
            }
        }

        private void CheckOrderHistory()
        {
            DateTime from = DateTime.Now.AddDays(-7);
            DateTime to = DateTime.Now;

            var orderHistory = _mt5Account.OrderHistory(
                from: from,
                to: to,
                sortMode: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
                pageNumber: 0,
                itemsPerPage: 100
            );

            _logger.LogInformation($"Order History (sync) from {from} to {to}: {orderHistory}");
        }

        private void GetSymbolParamsSync()
        {
            var request = new SymbolParamsManyRequest();
            try
            {
                var symbolParams = _mt5Account.SymbolParamsMany(request);
                _logger.LogInformation($"Symbol Parameters (sync): {symbolParams}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting symbol parameters: {ex.Message}");
            }
        }

        private void GetTickDataSync()
        {
            IEnumerable<string> symbols = new List<string>() { "EURUSD", "GBPUSD", "USDJPY" };
            try
            {
                var tickData = _mt5Account.TickValueWithSize(symbols);
                _logger.LogInformation($"Tick data (sync): {tickData}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting tick data: {ex.Message}");
            }
        }

        private void SendOrderSync()
        {
            var request = new OrderSendRequest
            {
                Symbol = "EURUSD",
                Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
                Volume = 0.1,
                Price = _mt5Account.SymbolInfoTick("EURUSD").Ask
            };

            var result = _mt5Account.OrderSend(request);
            _logger.LogInformation($"Order sent (sync), result: {result}");
        }

        private void ModifyOrderSync()
        {
            var request = new OrderModifyRequest
            {
                Ticket = 123456,
                StopLoss = 1.2300,
                TakeProfit = 1.2400,
                Price = 1.2345
            };

            var result = _mt5Account.OrderModify(request);
            _logger.LogInformation($"Order modified (sync), result: {result}");
        }

        private void CloseOrderSync()
        {
            var request = new OrderCloseRequest
            {
                Ticket = 123456
            };

            var result = _mt5Account.OrderClose(request);
            _logger.LogInformation($"Order closed (sync), result: {result}");
        }


        public async Task CallOnSymbolTickAsync(IEnumerable<string> symbols)
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            await foreach (var tickData in _mt5Account.OnSymbolTickAsync(symbols, cancellationToken))
            {

                Console.WriteLine(tickData);
            }
        }
      
        private async Task ProcessTradesAsync(MT5Account account, CancellationToken cancellationToken = default)
        {
            try
            {
                // Если аккаунт не подключен, сначала подключитесь
                if (!account.IConnected) // предполагаем, что есть такое свойство
                {
                    await account.Connect(); // предполагаем, что есть такой метод
                }

                // Подписываемся на поток торговых данных
                await foreach (var tradeData in account.OnTradeAsync(cancellationToken))
                {
                    // Обрабатываем каждое торговое событие
                    ProcessTradeData(tradeData);

                    // Если нужно прервать цикл по какому-то условию
                    if (ShouldStop(tradeData))
                    {
                        break;
                    }
                }
            }
            catch (ConnectExceptionMT5 ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
                // Обработка ошибки подключения
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
                // Обработка других исключений
            }
        }

        private void ProcessTradeData(OnTradeData tradeData)
        {
            // Ваша логика обработки торговых данных
            Console.WriteLine($"Получены данные о сделке: {tradeData}");
        }

        private bool ShouldStop(OnTradeData tradeData)
        {
            
            return false; 
        }

        private async Task MonitorPositionProfitAsync(MT5Account account, CancellationToken cancellationToken = default)
        {
            try
            {
                // Если аккаунт не подключен, сначала подключитесь
                if (!account.IConnected) // предполагаем, что есть такое свойство
                {
                    await account.Connect(); // предполагаем, что есть такой метод
                }

                // Интервал обновления в миллисекундах (например, каждую секунду)
                int updateIntervalMs = 1000;

                // Подписываемся на поток данных о прибыли позиций
                await foreach (var profitData in account.OnPositionProfitAsync(
                    intervalMs: updateIntervalMs,
                    ignoreEmpty: true,
                    cancellationToken: cancellationToken))
                {
                    // Обрабатываем каждое обновление прибыли
                    ProcessPositionProfitData(profitData);

                    // Если нужно прервать цикл по какому-то условию
                    if (ShouldStopMonitoring(profitData))
                    {
                        break;
                    }
                }
            }
            catch (ConnectExceptionMT5 ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
                // Обработка ошибки подключения
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
                // Обработка других исключений
            }
        }

        private void ProcessPositionProfitData(OnPositionProfitData profitData)
        {
            // Ваша логика обработки данных о прибыли позиций
            Console.WriteLine($"Получены данные о прибыли позиций: {profitData}");

            // Например, можно обновить UI или записать данные в базу
            foreach (var position in profitData.Positions)
            {
                Console.WriteLine($"Позиция {position.Ticket}: Прибыль = {position.Profit}");
            }
        }

        private bool ShouldStopMonitoring(OnPositionProfitData profitData)
        {
            // Логика для определения, нужно ли прекратить мониторинг
            return false; // Пример: никогда не останавливаемся
        }
    }




    public class MT5Options
    {
        public string ServerName { get; set; }
        public ulong AccountId { get; set; }
        public string Password { get; set; }
    }
}


