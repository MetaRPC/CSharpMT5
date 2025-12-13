/*══════════════════════════════════════════════════════════════════════════════
 ORCHESTRATOR: PendingBreakoutOrchestrator

 PURPOSE:
   Breakout trading strategy using pending BUY STOP and SELL STOP orders.
   Places orders on both sides of current price to catch breakout movement
   in either direction, then cancels the opposite order when one triggers.

 STRATEGY:
   • Places BUY STOP order above current Ask price (for upward breakout)
   • Places SELL STOP order below current Bid price (for downward breakout)
   • Monitors which order triggers first
   • Immediately cancels the opposite unfilled order
   • Lets triggered position run with SL/TP

 VISUAL EXAMPLE:

   ┌─────────────────────────────────────┐
   │ BUY STOP: +25 pts above             │ ← Triggers on upward breakout
   ├─────────────────────────────────────┤
   │                                     │
   │ >>> CURRENT PRICE: 1.10000 <<<      │ ← Market consolidating
   │                                     │
   ├─────────────────────────────────────┤
   │ SELL STOP: -25 pts below            │ ← Triggers on downward breakout
   └─────────────────────────────────────┘

   When price breaks out → one order triggers → opposite order cancels
   Strategy captures momentum in either direction

 DEMONSTRATED FEATURES:
   [1] SymbolInfoTickAsync - Get current price for order placement
   [2] BuyStopPoints  - Place BUY STOP with points-based offset
   [3] SellStopPoints - Place SELL STOP with points-based offset
   [4] OpenedOrdersTicketsAsync - Monitor pending order status
   [5] CloseByTicket - Cancel unfilled pending order

 KEY PARAMETERS:
   • Symbol: Trading pair (default: EURUSD)
   • BreakoutDistancePoints: Distance from price to place orders (default: 25 pts)
   • Volume: Lot size for both orders (default: 0.01)
   • StopLossPoints: SL distance (default: 15 pts)
   • TakeProfitPoints: TP distance (default: 30 pts)
   • MaxWaitMinutes: Max wait for breakout before canceling (default: 30 min)

 USE CASE:
   Best for consolidation periods expecting a breakout.
   Ideal before news events or at key support/resistance levels.
   Captures momentum regardless of breakout direction.

 COMMAND-LINE USAGE:
   dotnet run 11
   dotnet run breakout
  

 PROGRAMMATIC USAGE:

   ⚙️ PARAMETER CONFIGURATION IS LOCATED IN Program.cs

   WHY THIS SEPARATION EXISTS:
   • PendingBreakoutOrchestrator.cs = STRATEGY ENGINE (logic, algorithm)
   • Program.cs → RunOrchestrator_Breakout() = RUNTIME CONFIGURATION (parameters)

   THIS SEPARATION IS NEEDED FOR:
   1️⃣ Code Reusability
      → Same orchestrator class can run with different parameters
      → No need to modify strategy logic to change parameters

   2️⃣ Quick Testing
      → Want to test tighter breakout range? Change numbers in Program.cs
      → Want wider range? Again, only change Program.cs
      → Core algorithm remains untouched

   3️⃣ User Examples
      → Program.cs shows HOW to properly configure the orchestrator
      → All available parameters and their default values are visible

   4️⃣ Centralized Entry Point
      → All strategies launch through Program.cs
      → Single entry point: dotnet run breakout → RunOrchestrator_Breakout()

   📍 WHERE TO CONFIGURE PARAMETERS:
   Program.cs → method RunOrchestrator_Breakout() (lines 504-530)

   CONFIGURATION CODE IN Program.cs:

   private static async Task RunOrchestrator_Breakout()
   {
       var config = ConnectionHelper.BuildConfiguration();
       var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);

       try
       {
           var service = new MT5Service(account);

           var orchestrator = new PendingBreakoutOrchestrator(service)
           {
               Symbol = "EURUSD",                  // ← Which trading pair
               BreakoutDistancePoints = 25,        // ← Distance from price (points)
               Volume = 0.01,                      // ← Lot size for both orders
               StopLossPoints = 15,                // ← Stop loss (points)
               TakeProfitPoints = 30,              // ← Take profit (points)
               MaxWaitMinutes = 30                 // ← Max wait for breakout (minutes)
           };

           await orchestrator.ExecuteAsync();
       }
       finally
       {
           await account.GrpcChannel.ShutdownAsync();
       }
   }

   💡 EXAMPLE: Adjusting for Different Market Conditions

   // Option 1: Tight breakout for ranging market (default in Program.cs)
   BreakoutDistancePoints = 25,
   StopLossPoints = 15,
   MaxWaitMinutes = 30

   // Option 2: Wide breakout for consolidation before major move (modify in Program.cs)
   BreakoutDistancePoints = 50,      // ← wider range = stronger breakout signal
   StopLossPoints = 30,              // ← larger SL for volatility after breakout
   MaxWaitMinutes = 60               // ← longer wait for genuine breakout

   📝 IMPORTANT:
   • To change parameters → edit Program.cs, NOT this file
   • This file (PendingBreakoutOrchestrator.cs) contains only LOGIC
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
    public class PendingBreakoutOrchestrator
    {
        private readonly MT5Service _service;

        public string Symbol { get; set; } = "EURUSD";
        public int BreakoutDistancePoints { get; set; } = 25;
        public int StopLossPoints { get; set; } = 15;
        public int TakeProfitPoints { get; set; } = 30;
        public double Volume { get; set; } = 0.01;
        public int MaxWaitMinutes { get; set; } = 30;

        public PendingBreakoutOrchestrator(MT5Service service)
        {
            _service = service;
        }

        public async Task<double> ExecuteAsync(CancellationToken ct = default)
        {
            Console.WriteLine("\n+============================================================+");
            Console.WriteLine("|  PENDING BREAKOUT ORCHESTRATOR                            |");
            Console.WriteLine("+============================================================+\n");

            var initialBalance = await _service.GetBalanceAsync();
            Console.WriteLine($"  Starting balance: ${initialBalance:F2}");
            Console.WriteLine($"  Symbol: {Symbol}");
            Console.WriteLine($"  Breakout distance: {BreakoutDistancePoints} pts");
            Console.WriteLine($"  Volume: {Volume:F2} lots");
            Console.WriteLine($"  SL: {StopLossPoints} pts | TP: {TakeProfitPoints} pts\n");

            try
            {
                // Get current price
                var tick = await _service.SymbolInfoTickAsync(Symbol);
                Console.WriteLine($"  Current: Bid={tick.Bid:F5}, Ask={tick.Ask:F5}\n");

                // Place BUY STOP above current price
                Console.WriteLine("  Placing BUY STOP order...");
                var buyStopResult = await _service.BuyStopPoints(
                    symbol: Symbol,
                    volume: Volume,
                    priceOffsetPoints: BreakoutDistancePoints,
                    slPoints: StopLossPoints,
                    tpPoints: TakeProfitPoints,
                    comment: "Breakout-Buy"
                );

                if (buyStopResult.ReturnedCode != 10009)
                {
                    Console.WriteLine($"  ✗ BUY STOP failed: {buyStopResult.Comment}\n");
                    return 0;
                }

                Console.WriteLine($"  ✓ BUY STOP placed: #{buyStopResult.Order}\n");

                // Place SELL STOP below current price
                Console.WriteLine("  Placing SELL STOP order...");
                var sellStopResult = await _service.SellStopPoints(
                    symbol: Symbol,
                    volume: Volume,
                    priceOffsetPoints: -BreakoutDistancePoints,  // Negative for below
                    slPoints: StopLossPoints,
                    tpPoints: TakeProfitPoints,
                    comment: "Breakout-Sell"
                );

                if (sellStopResult.ReturnedCode != 10009)
                {
                    Console.WriteLine($"  ✗ SELL STOP failed: {sellStopResult.Comment}");
                    Console.WriteLine("  Canceling BUY STOP...");
                    await _service.CloseByTicket(buyStopResult.Order);
                    return 0;
                }

                Console.WriteLine($"  ✓ SELL STOP placed: #{sellStopResult.Order}\n");
                Console.WriteLine($"  ⏳ Waiting up to {MaxWaitMinutes} minutes for breakout...\n");

                // Monitor until one order triggers or timeout
                var startTime = DateTime.UtcNow;
                var timeout = TimeSpan.FromMinutes(MaxWaitMinutes);
                ulong? executedOrder = null;
                ulong? cancelOrder = null;

                while (DateTime.UtcNow - startTime < timeout && !ct.IsCancellationRequested)
                {
                    await Task.Delay(3000, ct);

                    var tickets = await _service.OpenedOrdersTicketsAsync();
                    bool buyStillPending = false;
                    bool sellStillPending = false;

                    foreach (var ticket in tickets.OpenedOrdersTickets)
                    {
                        if (ticket == (long)buyStopResult.Order) buyStillPending = true;
                        if (ticket == (long)sellStopResult.Order) sellStillPending = true;
                    }

                    // Check if one executed
                    if (!buyStillPending && sellStillPending)
                    {
                        Console.WriteLine("  🚀 BUY STOP EXECUTED! Upward breakout!");
                        executedOrder = buyStopResult.Order;
                        cancelOrder = sellStopResult.Order;
                        break;
                    }
                    else if (buyStillPending && !sellStillPending)
                    {
                        Console.WriteLine("  🚀 SELL STOP EXECUTED! Downward breakout!");
                        executedOrder = sellStopResult.Order;
                        cancelOrder = buyStopResult.Order;
                        break;
                    }
                    else if (!buyStillPending && !sellStillPending)
                    {
                        Console.WriteLine("  ✓ Both orders executed or canceled");
                        break;
                    }
                }

                // Cancel the opposite order if one triggered
                if (cancelOrder.HasValue)
                {
                    Console.WriteLine($"  Canceling opposite order #{cancelOrder.Value}...");
                    await _service.CloseByTicket(cancelOrder.Value);
                    Console.WriteLine("  ✓ Canceled\n");
                }
                else
                {
                    // Timeout - cancel both
                    Console.WriteLine($"  ⏱ Timeout after {MaxWaitMinutes} minutes - canceling both orders...");
                    await _service.CloseByTicket(buyStopResult.Order);
                    await _service.CloseByTicket(sellStopResult.Order);
                    Console.WriteLine("  ✓ Both canceled\n");
                }

                var finalBalance = await _service.GetBalanceAsync();
                var profit = finalBalance - initialBalance;

                Console.WriteLine($"  Final balance: ${finalBalance:F2}");
                Console.WriteLine($"  Profit/Loss: ${profit:F2}");
                Console.WriteLine("\n+============================================================+\n");

                return profit;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n  ✗ Error: {ex.Message}");
                Console.WriteLine("+============================================================+\n");
                return 0;
            }
        }
    }
}
