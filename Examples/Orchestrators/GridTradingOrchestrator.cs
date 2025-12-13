/*══════════════════════════════════════════════════════════════════════════════
 ORCHESTRATOR: GridTradingOrchestrator

 PURPOSE:
   Grid trading strategy for ranging markets with multiple pending orders.
   Places BUY LIMIT orders below and SELL LIMIT orders above current price
   at regular intervals to capture range-bound price movements.

 STRATEGY:
   • Places grid of BUY LIMIT orders below current Ask price
   • Places grid of SELL LIMIT orders above current Bid price
   • Each level has same volume, SL, and TP settings
   • Monitors profit/loss during runtime
   • Closes all remaining orders after max duration

 VISUAL EXAMPLE (3 levels, 20pt spacing):

   SELL LIMIT orders (above price):
   ┌─────────────────────────────────────┐
   │ Level 3: 1.10060 (SELL)             │ +60 pts
   │ Level 2: 1.10040 (SELL)             │ +40 pts
   │ Level 1: 1.10020 (SELL)             │ +20 pts
   ├─────────────────────────────────────┤
   │ >>> CURRENT PRICE: 1.10000 <<<      │ ← Market price
   ├─────────────────────────────────────┤
   │ Level 1: 1.09980 (BUY)              │ -20 pts
   │ Level 2: 1.09960 (BUY)              │ -40 pts
   │ Level 3: 1.09940 (BUY)              │ -60 pts
   └─────────────────────────────────────┘
   BUY LIMIT orders (below price)

   Price oscillates → orders fill at multiple levels → capture range bounces
   Runs for 15 minutes, then closes all remaining orders

 DEMONSTRATED FEATURES:
   [1] SymbolInfoTickAsync - Get current Bid/Ask prices
   [2] BuyLimitPoints  - Place BUY LIMIT with points-based offset
   [3] SellLimitPoints - Place SELL LIMIT with points-based offset
   [4] GetBalanceAsync - Monitor account balance changes
   [5] CloseAll - Bulk close all positions and pending orders for symbol

 KEY PARAMETERS:
   • Symbol: Trading pair (default: EURUSD)
   • GridLevels: Number of levels above and below (default: 3)
   • GridSpacingPoints: Distance between levels (default: 20 pts)
   • VolumePerLevel: Lot size for each level (default: 0.01)
   • StopLossPoints: SL distance (default: 50 pts)
   • TakeProfitPoints: TP distance (default: 30 pts)
   • MaxRunMinutes: Max runtime before cleanup (default: 15 min)

 USE CASE:
   Best for sideways/ranging markets where price oscillates within a range.
   Captures small profits from repeated bounces at support/resistance levels.

 COMMAND-LINE USAGE:
   dotnet run 9
   dotnet run grid
   dotnet run gridtrading


 PROGRAMMATIC USAGE:

   ⚙️ PARAMETER CONFIGURATION IS LOCATED IN Program.cs

   WHY THIS SEPARATION EXISTS:
   • GridTradingOrchestrator.cs = STRATEGY ENGINE (logic, algorithm)
   • Program.cs → RunOrchestrator_Grid() = RUNTIME CONFIGURATION (parameters)

   THIS SEPARATION IS NEEDED FOR:
   1️⃣ Code Reusability
      → Same orchestrator class can run with different parameters
      → No need to modify strategy logic to change parameters

   2️⃣ Quick Testing
      → Want to test aggressive grid? Change numbers in Program.cs
      → Want conservative grid? Again, only change Program.cs
      → Core algorithm remains untouched

   3️⃣ User Examples
      → Program.cs shows HOW to properly configure the orchestrator
      → All available parameters and their default values are visible

   4️⃣ Centralized Entry Point
      → All strategies launch through Program.cs
      → Single entry point: dotnet run grid → RunOrchestrator_Grid()

   📍 WHERE TO CONFIGURE PARAMETERS:
   Program.cs → method RunOrchestrator_Grid() (lines 447-473)

   CONFIGURATION CODE IN Program.cs:

   private static async Task RunOrchestrator_Grid()
   {
       var config = ConnectionHelper.BuildConfiguration();
       var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);

       try
       {
           var service = new MT5Service(account);

           var orchestrator = new GridTradingOrchestrator(service)
           {
               Symbol = "EURUSD",              // ← Which trading pair
               GridLevels = 3,                 // ← Number of grid levels (above and below)
               GridSpacingPoints = 20,         // ← Distance between levels (points)
               VolumePerLevel = 0.01,          // ← Volume per level (lots)
               StopLossPoints = 50,            // ← Stop loss (points)
               TakeProfitPoints = 30,          // ← Take profit (points)
               MaxRunMinutes = 15              // ← Max run time (minutes)
           };

           await orchestrator.ExecuteAsync();
       }
       finally
       {
           await account.GrpcChannel.ShutdownAsync();
       }
   }

   💡 EXAMPLE: Adjusting Grid Strategy

   // Option 1: Conservative grid (default in Program.cs)
   GridLevels = 3,
   GridSpacingPoints = 20,
   VolumePerLevel = 0.01

   // Option 2: Aggressive grid (modify in Program.cs)
   GridLevels = 5,           // ← more levels
   GridSpacingPoints = 10,   // ← tighter spacing = more orders
   VolumePerLevel = 0.02     // ← higher volume = more risk/reward

   📝 IMPORTANT:
   • To change parameters → edit Program.cs, NOT this file
   • This file (GridTradingOrchestrator.cs) contains only LOGIC
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
    public class GridTradingOrchestrator
    {
        private readonly MT5Service _service;

        public string Symbol { get; set; } = "EURUSD";
        public int GridLevels { get; set; } = 3;  // Number of levels above and below
        public int GridSpacingPoints { get; set; } = 20;  // Distance between levels
        public double VolumePerLevel { get; set; } = 0.01;
        public int StopLossPoints { get; set; } = 50;
        public int TakeProfitPoints { get; set; } = 30;
        public int MaxRunMinutes { get; set; } = 15;

        public GridTradingOrchestrator(MT5Service service)
        {
            _service = service;
        }

        public async Task<double> ExecuteAsync(CancellationToken ct = default)
        {
            Console.WriteLine("\n+============================================================+");
            Console.WriteLine("|  GRID TRADING ORCHESTRATOR                                |");
            Console.WriteLine("+============================================================+\n");

            var initialBalance = await _service.GetBalanceAsync();
            Console.WriteLine($"  Starting balance: ${initialBalance:F2}");
            Console.WriteLine($"  Symbol: {Symbol}");
            Console.WriteLine($"  Grid: {GridLevels} levels × {GridSpacingPoints} pts");
            Console.WriteLine($"  Volume per level: {VolumePerLevel:F2} lots");
            Console.WriteLine($"  SL: {StopLossPoints} pts | TP: {TakeProfitPoints} pts\n");

            try
            {
                var tick = await _service.SymbolInfoTickAsync(Symbol);
                Console.WriteLine($"  Current: Bid={tick.Bid:F5}, Ask={tick.Ask:F5}\n");

                var placedOrders = new System.Collections.Generic.List<ulong>();

                // Place BUY LIMIT orders below current price
                Console.WriteLine($"  Placing {GridLevels} BUY LIMIT levels...");
                for (int i = 1; i <= GridLevels; i++)
                {
                    var pointsBelow = -(i * GridSpacingPoints);  // Negative for below price
                    var result = await _service.BuyLimitPoints(
                        symbol: Symbol,
                        volume: VolumePerLevel,
                        priceOffsetPoints: pointsBelow,
                        slPoints: StopLossPoints,
                        tpPoints: TakeProfitPoints,
                        comment: $"Grid-Buy-{i}"
                    );

                    if (result.ReturnedCode == 10009)
                    {
                        placedOrders.Add(result.Order);
                        Console.WriteLine($"    ✓ Level {i}: #{result.Order} ({pointsBelow} pts below)");
                    }
                    else
                    {
                        Console.WriteLine($"    ✗ Level {i} failed: {result.Comment}");
                    }
                }

                Console.WriteLine();

                // Place SELL LIMIT orders above current price
                Console.WriteLine($"  Placing {GridLevels} SELL LIMIT levels...");
                for (int i = 1; i <= GridLevels; i++)
                {
                    var pointsAbove = i * GridSpacingPoints;  // Positive for above price
                    var result = await _service.SellLimitPoints(
                        symbol: Symbol,
                        volume: VolumePerLevel,
                        priceOffsetPoints: pointsAbove,
                        slPoints: StopLossPoints,
                        tpPoints: TakeProfitPoints,
                        comment: $"Grid-Sell-{i}"
                    );

                    if (result.ReturnedCode == 10009)
                    {
                        placedOrders.Add(result.Order);
                        Console.WriteLine($"    ✓ Level {i}: #{result.Order} ({pointsAbove} pts above)");
                    }
                    else
                    {
                        Console.WriteLine($"    ✗ Level {i} failed: {result.Comment}");
                    }
                }

                Console.WriteLine($"\n  ✓ Grid placed: {placedOrders.Count} pending orders");
                Console.WriteLine($"  ⏳ Running for {MaxRunMinutes} minutes...\n");

                // Monitor for duration
                var endTime = DateTime.UtcNow.AddMinutes(MaxRunMinutes);
                while (DateTime.UtcNow < endTime && !ct.IsCancellationRequested)
                {
                    await Task.Delay(5000, ct);

                    var currentBalance = await _service.GetBalanceAsync();
                    var currentProfit = currentBalance - initialBalance;
                    Console.WriteLine($"  Current P/L: ${currentProfit:F2}");
                }

                // Close all remaining orders
                Console.WriteLine("\n  ⏱ Time expired - closing all remaining orders...");
                await _service.CloseAll(Symbol);
                Console.WriteLine("  ✓ All closed");

                var finalBalance = await _service.GetBalanceAsync();
                var profit = finalBalance - initialBalance;

                Console.WriteLine($"\n  Final balance: ${finalBalance:F2}");
                Console.WriteLine($"  Total Profit/Loss: ${profit:F2}");
                Console.WriteLine("\n+============================================================+\n");

                return profit;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n  ✗ Error: {ex.Message}");
                // Emergency close all
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
