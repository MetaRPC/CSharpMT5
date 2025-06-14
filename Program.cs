using System;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using mt5_term_api;

namespace MetaRPC.CSharpMT5;

internal class Program
{
    static async Task Main(string[] args)
    {
        await new Program().Run(args);
    }

    async Task Run(string[] args)
    {
        try
        {
            
            //ConnectByServerName();
            //ConnectByHostPort();
            await RealTimeQuotes();
            //await OrderSend();
            //await ShowOpenedOrders(); (Может добавим его сюда?)
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        catch (ApiExceptionMT5 apiEx)
        {
            // Handle errors returned by the MT5 API
            Console.WriteLine($"ApiException: {apiEx.ErrorCode}");
        }
        catch (RpcException rpcEx)
        {
            // Handle gRPC communication errors
            Console.WriteLine($"RpcException: {rpcEx.Message}");
        }
        catch (Exception ex)
        {
            // Handle all other unexpected errors
            Console.WriteLine($"Exception: {ex.Message}");
        }

        // Inform the user that the application is ready to exit
        Console.WriteLine("Press any key to exit...");

        // Wait for user input before closing the console window
        Console.ReadKey();
    }

    void ConnectByServerName()
    {
        Console.WriteLine("Connecting to mt5 server...");
        MT5Account account = new MT5Account(5036292718, "_0AeXaFk");
        account.ConnectByServerName("MetaQuotes-Demo");
        Console.WriteLine($"Connected Account balance = {account.AccountSummary().AccountBalance}, terminal id = {account.Id}");
    }

    void ConnectByHostPort()
    {
        Console.WriteLine("Connecting to mt5 server...");
        MT5Account account = new MT5Account(5036292718, "_0AeXaFk");
        account.Connect("78.140.180.198", 443);
        Console.WriteLine("Connected Account balance = " + account.AccountSummary().AccountBalance);
    }

    async Task OrderSend()
    {
        MT5Account account = new MT5Account(62333850, "tecimil4");
        account.ConnectByServerName("MetaQuotes-Demo");
        Console.WriteLine("Connected Account balance = " + account.AccountSummary().AccountBalance);
        var symbol = "EURUSD";
        // wait for first quote as terminal may not have terminal just started
        await foreach (var update in account.OnSymbolTickAsync(new string[] { symbol }))
        {
            Console.WriteLine("Got first quote for " + update?.SymbolTick.Symbol);
            break;
        }
        var ask = account.SymbolInfoTick(symbol).Ask;
        Console.WriteLine(symbol + " ask = " + ask);
        var req = new OrderSendRequest();
        req.Symbol = symbol;
        req.Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy;
        req.Volume = 0.1;
        req.Price = ask;
        var result = account.OrderSend(req);
        Console.WriteLine($"Position {result.Order} opened");
    }

    async Task RealTimeQuotes()
    {
        MT5Account account = new MT5Account(10006638498, "TwJ*X0Gb");
        account.ConnectByServerName("MetaQuotes-Demo");
        Console.WriteLine("Connected Account balance = " + account.AccountSummary().AccountBalance);
        await foreach (var update in account.OnSymbolTickAsync(new string[] { "EURUSD" }))
        {
            Console.WriteLine(update?.SymbolTick?.Ask);

        }

    }
    async Task ShowAccountSummary()
    {
        try
        {
            MT5Account account = new MT5Account(10006638498, "TwJ*X0Gb");
            account.ConnectByServerName("MetaQuotes-Demo");

            var summaryData = await account.AccountSummaryAsync();

            Console.WriteLine("=== Account Summary ===");
            Console.WriteLine($"Balance: {summaryData.AccountBalance}");
            Console.WriteLine($"Equity: {summaryData.AccountEquity}");

            //будут ли входные данные в будущем присутствова в этом методе, такие как?
            //Console.WriteLine($"Margin: {summaryDataAsync.AccountMargin}");
            //Console.WriteLine($"Free Margin: {summaryDataAsync.AccountMarginFree}");
            //Console.WriteLine($"Margin Level: {summaryDataAsync.AccountMarginLevel}%");
            //Console.WriteLine($"Profit: {summaryDataAsync.AccountProfit}")


        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting account summary: {ex.Message}");
        }
    }
    async Task ShowOpenedOrders()
    {
        MT5Account account = new MT5Account(10006638498, "TwJ*X0Gb");
        account.ConnectByServerName("MetaQuotes-Demo");
        Console.WriteLine("Connected Account balance = " + account.AccountSummary().AccountBalance);

        var orders = await account.OpenedOrdersAsync();

        Console.WriteLine("=== Opened Orders ===");
        foreach (var order in orders.Orders)
        {
            Console.WriteLine($"Order ID: {order.Order}");
            Console.WriteLine($"Symbol: {order.Symbol}");
            Console.WriteLine($"Type: {order.Type}");
            Console.WriteLine($"Volume: {order.Volume}");
            Console.WriteLine($"Price: {order.PriceOrder}");
            Console.WriteLine("------------------------");
        }
    }
    /* async Task ShowOrderHistory() 
     {
         try
         {
             MT5Account account = new MT5Account(10006638498, "TwJ*X0Gb");
             account.ConnectByServerName("MetaQuotes-Demo");
             Console.WriteLine("Connected Account balance = " + account.AccountSummary().AccountBalance);

             // Получаем историю ордеров за последние 7 дней
             //Нужен ли нам такие данные вообще?
             var endDate = DateTime.UtcNow;
             var startDate = endDate.AddDays(-7);

             var history = await account.OrderHistoryAsync(
                 from: startDate,
                 to: endDate,
                 sortMode: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
                 pageNumber: 0,
                 itemsPerPage: 100
             );

             Console.WriteLine("\n=== Order History ===");
             foreach (var order in history.Orders)
             {
                 Console.WriteLine($"Order ID: {order.Order}");
                 Console.WriteLine($"Symbol: {order.Symbol}");
                 Console.WriteLine($"Type: {order.Type}");
                 Console.WriteLine($"Volume: {order.Volume}");
                 Console.WriteLine($"Price: {order.PriceOrder}");
                 Console.WriteLine($"Close Price: {order.PriceClose}");
                 Console.WriteLine($"Profit: {order.Profit}");
                 Console.WriteLine("------------------------");
             }
         }
         catch (Exception ex)
         {
             Console.WriteLine($"Error getting order history: {ex.Message}");
         }
     }
    */
    //Метод DemonstrateOrderHistory более полный, так как он показывает обе версии (синхронную и асинхронную)
    //и может заменить ShowOrderHistory().
    // Асинхронная версия метода
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

    // Синхронная версия метода
    public OrdersHistoryData OrderHistory(
        DateTime from,
        DateTime to,
        BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sortMode = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
        int pageNumber = 0,
        int itemsPerPage = 0,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        return OrderHistoryAsync(from, to, sortMode, pageNumber, itemsPerPage, deadline, cancellationToken)
            .GetAwaiter()
            .GetResult();
    }

    // Демонстрационный метод для использования в Program.cs
    public void DemonstrateOrderHistory()
    {
        try
        {
            MT5Account account = new MT5Account(10006638498, "TwJ*X0Gb");
            account.ConnectByServerName("MetaQuotes-Demo");

            // Использование синхронной версии
            var historySync = account.OrderHistory(
                from: DateTime.UtcNow.AddDays(-7),
                to: DateTime.UtcNow
            );

            Console.WriteLine("\n=== Order History (Sync) ===");
            foreach (var order in historySync.Orders)
            {
                Console.WriteLine($"Order ID: {order.Order}");
                Console.WriteLine($"Symbol: {order.Symbol}");
                Console.WriteLine($"Type: {order.Type}");
                Console.WriteLine($"Volume: {order.Volume}");
                Console.WriteLine($"Price: {order.PriceOrder}");
                Console.WriteLine($"Close Price: {order.PriceClose}");
                Console.WriteLine($"Profit: {order.Profit}");
                Console.WriteLine("------------------------");
            }

            // Использование асинхронной версии
            var historyAsync = account.OrderHistoryAsync(
                from: DateTime.UtcNow.AddDays(-7),
                to: DateTime.UtcNow
            ).GetAwaiter().GetResult();

            Console.WriteLine("\n=== Order History (Async) ===");
            foreach (var order in historyAsync.Orders)
            {
                Console.WriteLine($"Order ID: {order.Order}");
                Console.WriteLine($"Symbol: {order.Symbol}");
                Console.WriteLine($"Type: {order.Type}");
                Console.WriteLine($"Volume: {order.Volume}");
                Console.WriteLine($"Price: {order.PriceOrder}");
                Console.WriteLine($"Close Price: {order.PriceClose}");
                Console.WriteLine($"Profit: {order.Profit}");
                Console.WriteLine("------------------------");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error demonstrating order history: {ex.Message}");
        }
    }
