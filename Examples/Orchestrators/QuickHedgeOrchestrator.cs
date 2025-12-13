/*══════════════════════════════════════════════════════════════════════════════
 ORCHESTRATOR: QuickHedgeOrchestrator

 PURPOSE:
   Risk management strategy using dynamic hedging.
   Opens initial position, monitors price movement, and automatically opens
   opposite hedge position if price moves adversely beyond trigger threshold.

 STRATEGY:
   • Opens initial BUY or SELL position with risk-based sizing
   • Monitors price movement in real-time
   • If price moves adversely by trigger amount, opens hedge (opposite direction)
   • Hedge uses same volume for full protection
   • Holds both positions briefly, then closes all

 VISUAL EXAMPLE (BUY first, then hedge):

   STEP 1: Open BUY position
   ┌─────────────────────────────────────┐
   │ >>> ENTRY: BUY at 1.10000 <<<       │ ← Initial position
   └─────────────────────────────────────┘

   STEP 2: Price drops -15 pts → trigger hedge
   ┌─────────────────────────────────────┐
   │ Original: BUY at 1.10000            │ ← Losing position
   │ >>> HEDGE: SELL at 1.09985 <<<      │ ← Protection position
   └─────────────────────────────────────┘

   Both positions lock profit/loss at trigger point
   Strategy limits downside while keeping upside potential

 DEMONSTRATED FEATURES:
   [1] BuyMarketByRisk  - Risk-based position opening
   [2] SellMarketByRisk - Risk-based position opening
   [3] BuyMarketAsync   - Manual hedge position (fixed volume)
   [4] SellMarketAsync  - Manual hedge position (fixed volume)
   [5] SymbolInfoTickAsync - Real-time price monitoring
   [6] GetPointAsync - Convert price to points
   [7] CloseAll - Close all positions for symbol

 KEY PARAMETERS:
   • Symbol: Trading pair (default: EURUSD)
   • RiskAmount: Dollar risk for initial position (default: $30)
   • StopLossPoints: SL distance (default: 25 pts)
   • TakeProfitPoints: TP distance (default: 40 pts)
   • OpenBuyFirst: Initial direction - true=BUY, false=SELL (default: true)
   • HedgeTriggerPoints: Adverse movement before hedge (default: 15 pts)

 USE CASE:
   Best for volatile/uncertain market conditions where you want exposure
   but need downside protection. Useful during news events or high volatility.
   Locks in losses at trigger point while maintaining potential upside.

 COMMAND-LINE USAGE:
   dotnet run 12
   dotnet run hedge
   dotnet run quickhedge


 PROGRAMMATIC USAGE:

   ⚙️ PARAMETER CONFIGURATION IS LOCATED IN Program.cs

   WHY THIS SEPARATION EXISTS:
   • QuickHedgeOrchestrator.cs = STRATEGY ENGINE (logic, algorithm)
   • Program.cs → RunOrchestrator_Hedge() = RUNTIME CONFIGURATION (parameters)

   THIS SEPARATION IS NEEDED FOR:
   1️⃣ Code Reusability
      → Same orchestrator class can run with different parameters
      → No need to modify strategy logic to change parameters

   2️⃣ Quick Testing
      → Want to test earlier hedge trigger? Change numbers in Program.cs
      → Want later trigger? Again, only change Program.cs
      → Core algorithm remains untouched

   3️⃣ User Examples
      → Program.cs shows HOW to properly configure the orchestrator
      → All available parameters and their default values are visible

   4️⃣ Centralized Entry Point
      → All strategies launch through Program.cs
      → Single entry point: dotnet run hedge → RunOrchestrator_Hedge()

   📍 WHERE TO CONFIGURE PARAMETERS:
   Program.cs → method RunOrchestrator_Hedge() (lines 532-558)

   CONFIGURATION CODE IN Program.cs:

   private static async Task RunOrchestrator_Hedge()
   {
       var config = ConnectionHelper.BuildConfiguration();
       var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);

       try
       {
           var service = new MT5Service(account);

           var orchestrator = new QuickHedgeOrchestrator(service)
           {
               Symbol = "EURUSD",                 // ← Which trading pair
               RiskAmount = 30.0,                 // ← Dollar risk for initial position
               StopLossPoints = 25,               // ← Stop loss (points)
               TakeProfitPoints = 40,             // ← Take profit (points)
               OpenBuyFirst = true,               // ← Initial direction (true=BUY, false=SELL)
               HedgeTriggerPoints = 15            // ← Adverse move before hedge triggers (points)
           };

           await orchestrator.ExecuteAsync();
       }
       finally
       {
           await account.GrpcChannel.ShutdownAsync();
       }
   }

   💡 EXAMPLE: Adjusting Hedge Sensitivity

   // Option 1: Quick hedge trigger for volatile markets (default in Program.cs)
   HedgeTriggerPoints = 15,       // ← hedges quickly on adverse move
   StopLossPoints = 25,
   RiskAmount = 30.0

   // Option 2: Patient hedge trigger for trending markets (modify in Program.cs)
   HedgeTriggerPoints = 30,       // ← allows more room before hedging
   StopLossPoints = 50,           // ← wider SL gives position breathing room
   RiskAmount = 50.0              // ← larger risk for trending moves

   📝 IMPORTANT:
   • To change parameters → edit Program.cs, NOT this file
   • This file (QuickHedgeOrchestrator.cs) contains only LOGIC
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
    public class QuickHedgeOrchestrator
    {
        private readonly MT5Service _service;

        public string Symbol { get; set; } = "EURUSD";
        public double RiskAmount { get; set; } = 30.0;
        public int StopLossPoints { get; set; } = 25;
        public int TakeProfitPoints { get; set; } = 40;
        public bool OpenBuyFirst { get; set; } = true;
        public int HedgeTriggerPoints { get; set; } = 15;  // Hedge when this many points against us

        public QuickHedgeOrchestrator(MT5Service service)
        {
            _service = service;
        }

        public async Task<double> ExecuteAsync(CancellationToken ct = default)
        {
            Console.WriteLine("\n+============================================================+");
            Console.WriteLine("|  QUICK HEDGE ORCHESTRATOR                                 |");
            Console.WriteLine("+============================================================+\n");

            var initialBalance = await _service.GetBalanceAsync();
            Console.WriteLine($"  Starting balance: ${initialBalance:F2}");
            Console.WriteLine($"  Symbol: {Symbol}");
            Console.WriteLine($"  Initial direction: {(OpenBuyFirst ? "BUY" : "SELL")}");
            Console.WriteLine($"  Risk: ${RiskAmount:F2}");
            Console.WriteLine($"  Hedge trigger: {HedgeTriggerPoints} pts adverse\n");

            try
            {
                // Open initial position
                Console.WriteLine($"  Opening initial {(OpenBuyFirst ? "BUY" : "SELL")} position...");
                OrderSendData initialOrder;

                if (OpenBuyFirst)
                {
                    initialOrder = await _service.BuyMarketByRisk(
                        symbol: Symbol,
                        stopPoints: StopLossPoints,
                        riskMoney: RiskAmount,
                        tpPoints: TakeProfitPoints,
                        comment: "Hedge-Primary"
                    );
                }
                else
                {
                    initialOrder = await _service.SellMarketByRisk(
                        symbol: Symbol,
                        stopPoints: StopLossPoints,
                        riskMoney: RiskAmount,
                        tpPoints: TakeProfitPoints,
                        comment: "Hedge-Primary"
                    );
                }

                if (initialOrder.ReturnedCode != 10009)
                {
                    Console.WriteLine($"  ✗ Initial order failed: {initialOrder.Comment}");
                    return 0;
                }

                Console.WriteLine($"  ✓ Initial position: #{initialOrder.Order}");
                Console.WriteLine($"  Entry price: {initialOrder.Price:F5}");
                Console.WriteLine($"  Volume: {initialOrder.Volume:F2} lots\n");

                var entryPrice = initialOrder.Price;
                var point = await _service.GetPointAsync(Symbol);

                // Monitor price for hedge trigger
                Console.WriteLine($"  Monitoring price for hedge trigger...");
                bool hedgePlaced = false;
                ulong? hedgeTicket = null;
                var monitorStart = DateTime.UtcNow;
                var maxMonitorTime = TimeSpan.FromMinutes(5);

                while (DateTime.UtcNow - monitorStart < maxMonitorTime && !ct.IsCancellationRequested)
                {
                    await Task.Delay(2000, ct);

                    var tick = await _service.SymbolInfoTickAsync(Symbol);
                    var currentPrice = OpenBuyFirst ? tick.Bid : tick.Ask;
                    var priceMovementPoints = Math.Abs((currentPrice - entryPrice) / point);
                    var isAdverse = OpenBuyFirst ? (currentPrice < entryPrice) : (currentPrice > entryPrice);

                    if (isAdverse && priceMovementPoints >= HedgeTriggerPoints)
                    {
                        Console.WriteLine($"\n  ⚠️  Price moved {priceMovementPoints:F1} pts against us!");
                        Console.WriteLine($"  Opening hedge {(OpenBuyFirst ? "SELL" : "BUY")} position...");

                        OrderSendData hedgeOrder;
                        if (OpenBuyFirst)
                        {
                            hedgeOrder = await _service.SellMarketAsync(
                                symbol: Symbol,
                                volume: initialOrder.Volume,  // Same volume for full hedge
                                comment: "Hedge-Protection"
                            );
                        }
                        else
                        {
                            hedgeOrder = await _service.BuyMarketAsync(
                                symbol: Symbol,
                                volume: initialOrder.Volume,
                                comment: "Hedge-Protection"
                            );
                        }

                        if (hedgeOrder.ReturnedCode == 10009)
                        {
                            hedgeTicket = hedgeOrder.Order;
                            hedgePlaced = true;
                            Console.WriteLine($"  ✓ Hedge placed: #{hedgeOrder.Order}\n");
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"  ✗ Hedge failed: {hedgeOrder.Comment}\n");
                        }
                    }
                }

                if (!hedgePlaced)
                {
                    Console.WriteLine("  ✓ No hedge needed - price moved favorably\n");
                }

                // Hold for a bit then close all
                await ProgressBarHelper.WaitWithProgressBar(
                    totalSeconds: 30,
                    message: "Holding positions",
                    ct: ct
                );

                Console.WriteLine("  Closing all positions...");
                await _service.CloseAll(Symbol);
                Console.WriteLine("  ✓ All closed");

                var finalBalance = await _service.GetBalanceAsync();
                var profit = finalBalance - initialBalance;

                Console.WriteLine($"\n  Final balance: ${finalBalance:F2}");
                Console.WriteLine($"  Net Profit/Loss: ${profit:F2}");
                Console.WriteLine($"  {(hedgePlaced ? "Hedged" : "Unhedged")} trade");
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
