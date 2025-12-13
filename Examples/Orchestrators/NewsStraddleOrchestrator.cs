/*══════════════════════════════════════════════════════════════════════════════
 ORCHESTRATOR: NewsStraddleOrchestrator

 PURPOSE:
   News event trading strategy using symmetrical pending orders (straddle).
   Places BUY STOP and SELL STOP orders before high-impact news release to
   capture explosive volatility in either direction.

 STRATEGY:
   • Waits until specified time before news event
   • Places BUY STOP above current price (for upward spike)
   • Places SELL STOP below current price (for downward spike)
   • Monitors which order triggers first during news volatility
   • Cancels opposite order once one executes
   • Lets winning trade run with SL/TP protection

 VISUAL EXAMPLE:

   BEFORE NEWS (60s countdown):
   ┌─────────────────────────────────────┐
   │ BUY STOP: 1.10015 (+15 pts)         │ ← Triggers on upward spike
   ├─────────────────────────────────────┤
   │ >>> CURRENT: 1.10000 <<<            │ ← Waiting for news
   ├─────────────────────────────────────┤
   │ SELL STOP: 1.09985 (-15 pts)        │ ← Triggers on downward spike
   └─────────────────────────────────────┘

   DURING NEWS → price spikes → one order fills → other cancels
   Best for high-impact news: NFP, FOMC, CPI, GDP releases

 DEMONSTRATED FEATURES:
   [1] Task.Delay - Countdown timer to news event
   [2] SymbolInfoTickAsync - Get current price before placing straddle
   [3] BuyStopPoints  - Place BUY STOP for upward breakout
   [4] SellStopPoints - Place SELL STOP for downward breakout
   [5] OpenedOrdersTicketsAsync - Monitor which order triggers
   [6] CloseByTicket - Cancel unfilled order after one executes
   [7] CloseAll - Emergency cleanup of all orders

 KEY PARAMETERS:
   • Symbol: Trading pair (default: EURUSD)
   • StraddleDistancePoints: Distance from price for both orders (default: 15 pts)
   • Volume: Lot size for both orders (default: 0.02)
   • StopLossPoints: SL distance (default: 20 pts)
   • TakeProfitPoints: TP distance (default: 40 pts)
   • SecondsBeforeNews: Countdown before placing orders (default: 60s)
   • MaxWaitAfterNewsSeconds: Max wait for execution (default: 180s)

 USE CASE:
   Best for high-impact news events (NFP, FOMC, CPI, etc.) that cause
   immediate volatile price spikes. Captures momentum in either direction
   without needing to predict news outcome.

 COMMAND-LINE USAGE:
   dotnet run 10
   dotnet run news
   dotnet run newsstraddle


 PROGRAMMATIC USAGE:

   ⚙️ PARAMETER CONFIGURATION IS LOCATED IN Program.cs

   WHY THIS SEPARATION EXISTS:
   • NewsStraddleOrchestrator.cs = STRATEGY ENGINE (logic, algorithm)
   • Program.cs → RunOrchestrator_News() = RUNTIME CONFIGURATION (parameters)

   THIS SEPARATION IS NEEDED FOR:
   1️⃣ Code Reusability
      → Same orchestrator class can run with different parameters
      → No need to modify strategy logic to change parameters

   2️⃣ Quick Testing
      → Want to test tighter straddle? Change numbers in Program.cs
      → Want wider straddle? Again, only change Program.cs
      → Core algorithm remains untouched

   3️⃣ User Examples
      → Program.cs shows HOW to properly configure the orchestrator
      → All available parameters and their default values are visible

   4️⃣ Centralized Entry Point
      → All strategies launch through Program.cs
      → Single entry point: dotnet run news → RunOrchestrator_News()

   📍 WHERE TO CONFIGURE PARAMETERS:
   Program.cs → method RunOrchestrator_News() (lines 475-502)

   CONFIGURATION CODE IN Program.cs:

   private static async Task RunOrchestrator_News()
   {
       var config = ConnectionHelper.BuildConfiguration();
       var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);

       try
       {
           var service = new MT5Service(account);

           var orchestrator = new NewsStraddleOrchestrator(service)
           {
               Symbol = "EURUSD",                    // ← Which trading pair
               StraddleDistancePoints = 15,          // ← Distance from price (points)
               Volume = 0.02,                        // ← Lot size for both orders
               StopLossPoints = 20,                  // ← Stop loss (points)
               TakeProfitPoints = 40,                // ← Take profit (points)
               SecondsBeforeNews = 60,               // ← Countdown before placing orders
               MaxWaitAfterNewsSeconds = 180         // ← Max wait for breakout (seconds)
           };

           await orchestrator.ExecuteAsync();
       }
       finally
       {
           await account.GrpcChannel.ShutdownAsync();
       }
   }

   💡 EXAMPLE: Adjusting for Different News Events

   // Option 1: Tight straddle for moderate news (default in Program.cs)
   StraddleDistancePoints = 15,
   StopLossPoints = 20,
   TakeProfitPoints = 40

   // Option 2: Wide straddle for high-impact news (modify in Program.cs)
   StraddleDistancePoints = 30,      // ← wider distance = less false triggers
   StopLossPoints = 50,              // ← larger SL for volatility
   TakeProfitPoints = 100            // ← larger TP for major moves

   📝 IMPORTANT:
   • To change parameters → edit Program.cs, NOT this file
   • This file (NewsStraddleOrchestrator.cs) contains only LOGIC
   • Program.cs contains CONFIGURATION for specific runs
   • Look for the section: ORCHESTRATOR RUNNERS

══════════════════════════════════════════════════════════════════════════════*/

using System;
using System.Threading;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;
using mt5_term_api;

namespace MetaRPC.CSharpMT5.Examples.Orchestrators
{
    public class NewsStraddleOrchestrator
    {
        private readonly MT5Service _service;

        public string Symbol { get; set; } = "EURUSD";
        public int StraddleDistancePoints { get; set; } = 15;  // Distance from current price
        public double Volume { get; set; } = 0.02;
        public int StopLossPoints { get; set; } = 20;
        public int TakeProfitPoints { get; set; } = 40;
        public int SecondsBeforeNews { get; set; } = 60;  // Place orders 60s before news
        public int MaxWaitAfterNewsSeconds { get; set; } = 180;  // Wait 3 minutes max

        public NewsStraddleOrchestrator(MT5Service service)
        {
            _service = service;
        }

        public async Task<double> ExecuteAsync(CancellationToken ct = default)
        {
            Console.WriteLine("\n+============================================================+");
            Console.WriteLine("|  NEWS STRADDLE ORCHESTRATOR                               |");
            Console.WriteLine("+============================================================+\n");

            var initialBalance = await _service.GetBalanceAsync();
            Console.WriteLine($"  Starting balance: ${initialBalance:F2}");
            Console.WriteLine($"  Symbol: {Symbol}");
            Console.WriteLine($"  Straddle distance: {StraddleDistancePoints} pts");
            Console.WriteLine($"  Volume: {Volume:F2} lots");
            Console.WriteLine($"  SL: {StopLossPoints} pts | TP: {TakeProfitPoints} pts\n");

            try
            {
                // Simulate countdown to news event
                await ProgressBarHelper.WaitWithProgressBar(
                    totalSeconds: SecondsBeforeNews,
                    message: "Countdown to news event",
                    ct: ct
                );

                var tick = await _service.SymbolInfoTickAsync(Symbol);
                Console.WriteLine($"  📰 NEWS EVENT IMMINENT!");
                Console.WriteLine($"  Current: Bid={tick.Bid:F5}, Ask={tick.Ask:F5}\n");

                // Place BUY STOP above current price
                Console.WriteLine("  Placing BUY STOP (upper straddle)...");
                var buyStopResult = await _service.BuyStopPoints(
                    symbol: Symbol,
                    volume: Volume,
                    priceOffsetPoints: StraddleDistancePoints,
                    slPoints: StopLossPoints,
                    tpPoints: TakeProfitPoints,
                    comment: "News-Buy"
                );

                if (buyStopResult.ReturnedCode != 10009)
                {
                    Console.WriteLine($"  ✗ BUY STOP failed: {buyStopResult.Comment}\n");
                    return 0;
                }

                Console.WriteLine($"  ✓ BUY STOP: #{buyStopResult.Order}\n");

                // Place SELL STOP below current price
                Console.WriteLine("  Placing SELL STOP (lower straddle)...");
                var sellStopResult = await _service.SellStopPoints(
                    symbol: Symbol,
                    volume: Volume,
                    priceOffsetPoints: -StraddleDistancePoints,  // Negative for below
                    slPoints: StopLossPoints,
                    tpPoints: TakeProfitPoints,
                    comment: "News-Sell"
                );

                if (sellStopResult.ReturnedCode != 10009)
                {
                    Console.WriteLine($"  ✗ SELL STOP failed: {sellStopResult.Comment}");
                    Console.WriteLine("  Canceling BUY STOP...");
                    await _service.CloseByTicket(buyStopResult.Order);
                    return 0;
                }

                Console.WriteLine($"  ✓ SELL STOP: #{sellStopResult.Order}\n");
                Console.WriteLine("  ✅ STRADDLE ACTIVE - Waiting for news spike!\n");

                // Monitor for execution
                var monitorStart = DateTime.UtcNow;
                var timeout = TimeSpan.FromSeconds(MaxWaitAfterNewsSeconds);
                ulong? executedOrder = null;
                ulong? pendingOrder = null;
                string direction = "";

                while (DateTime.UtcNow - monitorStart < timeout && !ct.IsCancellationRequested)
                {
                    await Task.Delay(1000, ct);

                    var tickets = await _service.OpenedOrdersTicketsAsync();
                    bool buyStillPending = false;
                    bool sellStillPending = false;

                    foreach (var ticket in tickets.OpenedOrdersTickets)
                    {
                        if (ticket == (long)buyStopResult.Order) buyStillPending = true;
                        if (ticket == (long)sellStopResult.Order) sellStillPending = true;
                    }

                    // Check if one order triggered
                    if (!buyStillPending && sellStillPending)
                    {
                        executedOrder = buyStopResult.Order;
                        pendingOrder = sellStopResult.Order;
                        direction = "UPWARD";
                        break;
                    }
                    else if (buyStillPending && !sellStillPending)
                    {
                        executedOrder = sellStopResult.Order;
                        pendingOrder = buyStopResult.Order;
                        direction = "DOWNWARD";
                        break;
                    }
                    else if (!buyStillPending && !sellStillPending)
                    {
                        Console.WriteLine("  ⚡ BOTH ORDERS TRIGGERED - Extreme volatility!");
                        direction = "BOTH";
                        break;
                    }
                }

                if (executedOrder.HasValue && pendingOrder.HasValue)
                {
                    Console.WriteLine($"  🚀 {direction} BREAKOUT DETECTED!");
                    Console.WriteLine($"  Position opened: #{executedOrder.Value}");
                    Console.WriteLine($"  Canceling opposite order #{pendingOrder.Value}...");
                    await _service.CloseByTicket(pendingOrder.Value);
                    Console.WriteLine("  ✓ Opposite order canceled\n");

                    // Hold position for a bit
                    Console.WriteLine("  ⏳ Holding position for 60 seconds...");
                    await Task.Delay(60000, ct);
                }
                else if (direction == "BOTH")
                {
                    Console.WriteLine("  ⏳ Holding both positions for 30 seconds...");
                    await Task.Delay(30000, ct);
                }
                else
                {
                    Console.WriteLine($"  ⏱ No breakout after {MaxWaitAfterNewsSeconds}s");
                    Console.WriteLine("  Canceling both pending orders...");
                    await _service.CloseByTicket(buyStopResult.Order);
                    await _service.CloseByTicket(sellStopResult.Order);
                }

                // Close all remaining positions
                Console.WriteLine("\n  Closing all remaining positions...");
                await _service.CloseAll(Symbol);
                Console.WriteLine("  ✓ All closed");

                var finalBalance = await _service.GetBalanceAsync();
                var profit = finalBalance - initialBalance;

                Console.WriteLine($"\n  Final balance: ${finalBalance:F2}");
                Console.WriteLine($"  Profit/Loss: ${profit:F2}");
                Console.WriteLine($"  Direction: {(string.IsNullOrEmpty(direction) ? "None" : direction)}");
                Console.WriteLine("\n+============================================================+\n");

                return profit;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n  ✗ Error: {ex.Message}");
                try
                {
                    await _service.CloseAll(Symbol);
                }
                catch { }
                Console.WriteLine("+============================================================+\n");
                return 0;
            }
        }
    }
}
