// ══════════════════════════════════════════════════════════════════════════════
// FILE: ProgramUserCode.cs - USER SANDBOX FOR CUSTOM TRADING LOGIC
//
// HOW TO RUN:
//   1. First, uncomment the case block in Program.cs (line ~172)
//   2. Then run: dotnet run usercode (or: user, sandbox)
//
// WHAT'S READY:
//   ✓ Connection to MT5 Terminal (from appsettings.json)
//   ✓ MT5Account - low-level gRPC client
//   ✓ MT5Service - high-level wrapper with Sugar API
//
// YOUR TASK:
//   Write your trading logic in RunAsync() method below.
//   Uncomment examples to get started!
//
// DOCUMENTATION:
//   • docs/ReturnCodes_Reference.md - Return codes reference
//   • MT5Service.cs - Available methods
//   • MT5Sugar.cs - Sugar API (45+ convenience methods)
//   • Examples/UserCode/README.md - Full guide
// ══════════════════════════════════════════════════════════════════════════════

using System;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;
using mt5_term_api;

namespace MetaRPC.CSharpMT5.Examples.UserCode;

public static class ProgramUserCode
{
    public static async Task RunAsync()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    USER CODE SANDBOX                             ║");
        Console.WriteLine("║              Your Custom Trading Logic Goes Here                 ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝\n");

        // ═════════════════════════════════════════════════════════════════
        // CONNECTION SETUP (Already Done For You)
        // ═════════════════════════════════════════════════════════════════

        var config = ConnectionHelper.BuildConfiguration();
        var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);
        var service = new MT5Service(account);

        Console.WriteLine("✓ Connected to MT5 Terminal");
        Console.WriteLine("✓ MT5Service initialized\n");

        // ═════════════════════════════════════════════════════════════════
        // YOUR CODE STARTS HERE ↓
        // ═════════════════════════════════════════════════════════════════

        // TODO: Write your trading logic below
        // Uncomment examples to get started:

        // Example 1: Get account information
        // var accountInfo = await service.GetAccountInfoAsync();
        // Console.WriteLine($"Balance: ${accountInfo.Balance}");
        // Console.WriteLine($"Equity: ${accountInfo.Equity}");
        // Console.WriteLine($"Free Margin: ${accountInfo.FreeMargin}");

        // Example 2: Get current price
        // var tick = await service.SymbolInfoTickAsync("EURUSD");
        // Console.WriteLine($"EURUSD Bid: {tick.Bid}, Ask: {tick.Ask}");

        // Example 3: Open a market order with risk management
        // var result = await service.BuyMarketByRisk(
        //     symbol: "EURUSD",
        //     riskAmount: 10.0,        // Risk $10
        //     stopLossPoints: 20       // SL 20 points away
        // );
        //
        // if (result.ReturnedCode == 10009)
        // {
        //     Console.WriteLine($"✅ Order opened: #{result.Order}");
        //     Console.WriteLine($"   Volume: {result.Volume} lots");
        //     Console.WriteLine($"   Price: {result.Price}");
        // }
        // else
        // {
        //     Console.WriteLine($"❌ Order failed: {result.ReturnedCodeDescription}");
        // }

        // Example 4: Place a pending order
        // var pendingResult = await service.BuyLimitPoints(
        //     symbol: "EURUSD",
        //     volume: 0.01,
        //     offsetPoints: 20,        // 20 points below Ask
        //     stopLossPoints: 15,
        //     takeProfitPoints: 30
        // );

        // Example 5: Get list of open positions
        // var positions = await account.PositionsGetAsync(new Empty());
        // Console.WriteLine($"Open positions: {positions.Data.Positions.Count}");
        // foreach (var pos in positions.Data.Positions)
        // {
        //     Console.WriteLine($"  #{pos.Ticket} {pos.Symbol} {pos.Type} {pos.Volume} lots");
        // }

        // Example 6: Close all positions for a symbol
        // await service.CloseAll("EURUSD");

        // Example 7: Mix Low-Level + High-Level API (You can use both!)
        // // Get positions using low-level gRPC
        // var positionsReply = await account.PositionsGetAsync(new Empty());
        // Console.WriteLine($"Open positions (via account): {positionsReply.Data.Positions.Count}");
        //
        // // Get account info using high-level Service
        // var accountInfo = await service.GetAccountInfoAsync();
        // Console.WriteLine($"Balance (via service): ${accountInfo.Balance}");
        //
        // // Open order using Sugar API
        // var result = await service.BuyMarketByRisk("EURUSD", 10.0, 20);
        // Console.WriteLine($"Order result (via sugar): {result.ReturnedCodeDescription}");

        // Example 8: Retry logic for requotes
        // int maxRetries = 3;
        // for (int attempt = 1; attempt <= maxRetries; attempt++)
        // {
        //     var result = await service.BuyMarketAsync("EURUSD", 0.01);
        //
        //     if (result.ReturnedCode == 10009) // Success
        //     {
        //         Console.WriteLine($"✅ Order opened on attempt {attempt}: #{result.Order}");
        //         break;
        //     }
        //     else if (result.ReturnedCode == 10004 || result.ReturnedCode == 10020) // Requote
        //     {
        //         Console.WriteLine($"⚠️ Requote on attempt {attempt}, retrying...");
        //         await Task.Delay(100); // Small delay before retry
        //     }
        //     else
        //     {
        //         Console.WriteLine($"❌ Failed: {result.ReturnedCodeDescription}");
        //         break;
        //     }
        // }

        // Example 9: Monitor positions in a loop (simple bot skeleton)
        // Console.WriteLine("Monitoring positions for 30 seconds...");
        // var endTime = DateTime.Now.AddSeconds(30);
        //
        // while (DateTime.Now < endTime)
        // {
        //     var positions = await account.PositionsGetAsync(new Empty());
        //     var accountInfo = await service.GetAccountInfoAsync();
        //
        //     Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] " +
        //                       $"Positions: {positions.Data.Positions.Count}, " +
        //                       $"Balance: ${accountInfo.Balance:F2}, " +
        //                       $"Equity: ${accountInfo.Equity:F2}");
        //
        //     await Task.Delay(5000); // Check every 5 seconds
        // }
        //
        // Console.WriteLine("Monitoring completed!");

        // ═════════════════════════════════════════════════════════════════
        // YOUR CODE ENDS HERE ↑
        // ═════════════════════════════════════════════════════════════════

        Console.WriteLine("\n═══════════════════════════════════════════════════════════════════");
        Console.WriteLine("User code execution completed!");
        Console.WriteLine("═══════════════════════════════════════════════════════════════════");
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// QUICK REFERENCE: Most Useful Methods
//
// ACCOUNT & SYMBOL INFO:
//   await service.GetAccountInfoAsync()          → Balance, equity, margin
//   await service.SymbolInfoTickAsync("EURUSD")  → Current Bid/Ask
//   await service.GetPointAsync("EURUSD")        → Symbol point size
//   await service.GetSpreadPointsAsync("EURUSD") → Current spread
//
// MARKET ORDERS (Sugar API - Recommended):
//   await service.BuyMarketByRisk(symbol, riskAmount, stopLossPoints)
//     → Opens BUY with auto lot size based on $ risk
//
//   await service.SellMarketByRisk(symbol, riskAmount, stopLossPoints)
//     → Opens SELL with auto lot size based on $ risk
//
//   await service.PlaceMarket(symbol, volume, isBuy, sl, tp)
//     → Generic market order (BUY or SELL)
//
// PENDING ORDERS (Points-Based):
//   await service.BuyLimitPoints(symbol, volume, offsetPoints, sl, tp)
//   await service.SellLimitPoints(symbol, volume, offsetPoints, sl, tp)
//   await service.BuyStopPoints(symbol, volume, offsetPoints, sl, tp)
//   await service.SellStopPoints(symbol, volume, offsetPoints, sl, tp)
//
// POSITION MANAGEMENT:
//   await account.PositionsGetAsync(new Empty())  → List all open positions
//   await service.CloseByTicket(ticket)           → Close specific position
//   await service.CloseAll(symbol)                → Close all for symbol
//   await service.CloseAllPositions(symbol)       → Close only market positions
//   await service.CancelAll(symbol)               → Cancel all pending orders
//   await service.ModifySlTpAsync(ticket, sl, tp) → Modify SL/TP
//
// VALIDATION & SAFETY:
//   await service.ValidateOrderAsync(symbol, orderType, volume, sl, tp)
//   await service.CalculateBuyMarginAsync(symbol, volume)
//   await service.GetVolumeLimitsAsync(symbol)
//
// RETCODE CHECKING (Always Check After Trading!):
//   if (result.ReturnedCode == 10009)  // Success for market orders
//   if (result.ReturnedCode == 10008)  // Success for pending orders
//   See docs/ReturnCodes_Reference.md for complete list of 43 return codes
// ══════════════════════════════════════════════════════════════════════════════
