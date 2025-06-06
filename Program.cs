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
        MT5Account account = new MT5Account(5036292718, "_0AeXaFk");
        account.ConnectByServerName("MetaQuotes-Demo");
        Console.WriteLine("Connected Account balance = " + account.AccountSummary().AccountBalance);
        await foreach (var update in account.OnSymbolTickAsync(new string[] { "EURUSD" }))
        {
            Console.WriteLine(update?.SymbolTick?.Ask);
        }
    }
}
