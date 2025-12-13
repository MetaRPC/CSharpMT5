/*══════════════════════════════════════════════════════════════════════════════
 PRESET: AdaptiveMarketModePreset

 PURPOSE:
   Intelligent trading system that automatically selects and executes the most
   appropriate orchestrator based on real-time market condition analysis.
   Combines ALL 5 orchestrators into one adaptive system that switches strategies
   dynamically every cycle.

 HOW IT WORKS:
   Each trading cycle (every 5 minutes):
   1. ANALYZE market conditions (volatility, spread, time, news schedule)
   2. SELECT best orchestrator for current conditions
   3. EXECUTE selected orchestrator with optimized parameters
   4. TRACK profit/loss and repeat

 MARKET CONDITIONS & ORCHESTRATOR SELECTION:

   📊 CONDITION 1: LOW VOLATILITY (< 15 points)
      → Executes: GridTradingOrchestrator
      → Why: Range-bound markets are perfect for grid strategies
      → Parameters: 3 levels, 20pt spacing, tight SL/TP

   📊 CONDITION 2: MEDIUM VOLATILITY (15-40 points)
      → Executes: SimpleScalpingOrchestrator
      → Why: Normal conditions suit quick scalping trades
      → Parameters: Risk-based sizing, tight stops, 60s max hold

   📊 CONDITION 3: HIGH VOLATILITY (> 40 points)
      → Executes: QuickHedgeOrchestrator (PROTECTION MODE)
      → Why: Volatile markets need hedging protection
      → Parameters: Reduced risk (70% of base), 15pt hedge trigger

   📊 CONDITION 4: NEWS EVENT DETECTED (UTC 08:30, 12:30, 14:00, 18:00, 19:00)
      → Executes: NewsStraddleOrchestrator
      → Why: Capture explosive volatility from high-impact news
      → Parameters: 15pt straddle distance, 30s countdown

   📊 CONDITION 5: BREAKOUT SIGNAL (Unusual spread > 3 points)
      → Executes: PendingBreakoutOrchestrator
      → Why: Large spread indicates potential breakout movement
      → Parameters: 20pt breakout distance, 3min wait time

 VOLATILITY ANALYSIS METHOD:
   • Reads current bid/ask spread as volatility proxy
   • Calculates average range estimate (spread × 10)
   • Compares against thresholds (Low: 15pts, High: 40pts)
   • Note: Simplified for demo - production would use historical bar analysis

 NEWS DETECTION:
   • Monitors UTC time for major news events
   • Triggers news mode 5 minutes before scheduled event
   • Default schedule: 08:30, 12:30, 14:00, 18:00, 19:00 UTC
   • Covers: NFP, FOMC, CPI, GDP, ECB announcements

 SAFETY FEATURES:
   ✓ Stop-loss protection: Halts if total loss > 5× base risk ($100 default)
   ✓ 30-second pause between cycles to avoid overtrading
   ✓ Emergency close all positions on error
   ✓ Cycle-by-cycle profit tracking

 VISUAL FLOW:

   ┌─────────────────────────────────────────────────────────────┐
   │  START CYCLE                                                │
   └─────────────────────────────────────────────────────────────┘
                             ↓
   ┌─────────────────────────────────────────────────────────────┐
   │  ANALYZE MARKET                                             │
   │  • Read spread & tick data                                  │
   │  • Calculate volatility estimate                            │
   │  • Check time for news events                               │
   │  • Detect breakout signals                                  │
   └─────────────────────────────────────────────────────────────┘
                             ↓
   ┌─────────────────────────────────────────────────────────────┐
   │  SELECT ORCHESTRATOR                                        │
   │  • News mode? → NewsStraddleOrchestrator                    │
   │  • Breakout? → PendingBreakoutOrchestrator                  │
   │  • Low volatility? → GridTradingOrchestrator                │
   │  • Medium volatility? → SimpleScalpingOrchestrator          │
   │  • High volatility? → QuickHedgeOrchestrator                │
   └─────────────────────────────────────────────────────────────┘
                             ↓
   ┌─────────────────────────────────────────────────────────────┐
   │  EXECUTE SELECTED ORCHESTRATOR                              │
   │  • Run with preset-optimized parameters                     │
   │  • Track profit/loss for this cycle                         │
   └─────────────────────────────────────────────────────────────┘
                             ↓
   ┌─────────────────────────────────────────────────────────────┐
   │  EVALUATE RESULTS                                           │
   │  • Add cycle P/L to total                                   │
   │  • Check safety stop-loss threshold                         │
   │  • Wait 30 seconds before next cycle                        │
   └─────────────────────────────────────────────────────────────┘
                             ↓
                    (Loop back to START)

 KEY PARAMETERS:
   • Symbol: Trading pair (default: EURUSD)
   • BaseRiskAmount: Risk per trade in dollars (default: $20)
   • LowVolatilityThreshold: Max points for grid mode (default: 15 pts)
   • HighVolatilityThreshold: Min points for protection mode (default: 40 pts)
   • EnableNewsMode: Enable news event detection (default: true)
   • MinutesBeforeNews: Switch to news mode N minutes early (default: 5)
   • CycleDurationSeconds: NOT USED - each orchestrator has own duration

 DEMONSTRATED FEATURES:
   [1] Multi-orchestrator coordination - Uses all 5 orchestrators
   [2] Market regime detection - Volatility and time-based analysis
   [3] Dynamic strategy selection - Adaptive switching based on conditions
   [4] Risk management - Position sizing, stop-loss, cycle limits
   [5] News event scheduling - Time-based news detection
   [6] Continuous operation - Infinite loop with safety stops

 USE CASE:
   Perfect for:
   • Traders who want "set and forget" adaptive trading
   • Testing multiple strategies in different market conditions
   • Learning how to combine orchestrators into a complete system
   • Running unattended automated trading with regime detection

 COMMAND-LINE USAGE:
   dotnet run 14
   dotnet run adaptive
   dotnet run preset


 PROGRAMMATIC USAGE:

   ⚙️ PARAMETER CONFIGURATION IS LOCATED IN Program.cs

   WHY THIS SEPARATION EXISTS:
   • AdaptiveMarketModePreset.cs = SYSTEM LOGIC (market analysis, orchestrator selection)
   • Program.cs → RunPreset_Adaptive() = RUNTIME CONFIGURATION (thresholds, risk)

   THIS SEPARATION IS NEEDED FOR:
   1️⃣ Code Reusability
      → Same preset can run with different risk profiles
      → No need to modify logic to change volatility thresholds

   2️⃣ Quick Testing
      → Want to test with higher risk? Change numbers in Program.cs
      → Want different volatility thresholds? Change in Program.cs
      → Core logic remains untouched

   3️⃣ User Examples
      → Program.cs shows HOW to properly configure the preset
      → All available parameters and their default values are visible

   4️⃣ Centralized Entry Point
      → All presets launch through Program.cs
      → Single entry point: dotnet run adaptive → RunPreset_Adaptive()

   📍 WHERE TO CONFIGURE PARAMETERS:
   Program.cs → method RunPreset_Adaptive() (lines 588-613)

   CONFIGURATION CODE IN Program.cs:

   private static async Task RunPreset_Adaptive()
   {
       var config = ConnectionHelper.BuildConfiguration();
       var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);

       try
       {
           var service = new MT5Service(account);

           var preset = new AdaptiveMarketModePreset(service)
           {
               Symbol = "EURUSD",                      // ← Which trading pair
               BaseRiskAmount = 20.0,                  // ← Base risk per trade ($)
               LowVolatilityThreshold = 15.0,          // ← Max volatility for grid mode (points)
               HighVolatilityThreshold = 40.0,         // ← Min volatility for hedge mode (points)
               EnableNewsMode = true                   // ← Enable news event detection
           };

           await preset.ExecuteAsync();
       }
       finally
       {
           await account.GrpcChannel.ShutdownAsync();
       }
   }

   💡 EXAMPLE: Adjusting Risk Profile

   // Option 1: Conservative (default in Program.cs)
   BaseRiskAmount = 20.0,             // ← lower risk per trade
   LowVolatilityThreshold = 15.0,     // ← stricter grid mode entry
   HighVolatilityThreshold = 40.0     // ← stay in scalping longer

   // Option 2: Aggressive (modify in Program.cs)
   BaseRiskAmount = 50.0,             // ← higher risk for larger profits
   LowVolatilityThreshold = 20.0,     // ← more grid opportunities
   HighVolatilityThreshold = 30.0     // ← trigger hedge protection earlier

   📝 IMPORTANT:
   • To change parameters → edit Program.cs, NOT this file
   • This file (AdaptiveMarketModePreset.cs) contains only LOGIC
   • Program.cs contains CONFIGURATION for specific runs
   • Look for the section: PRESETS & TOOLS

   💡 HOW ORCHESTRATORS ARE CONFIGURED INSIDE THE PRESET:

   Each orchestrator receives optimized parameters based on market conditions:

   GridTradingOrchestrator (Low Volatility):
     GridLevels = 3              // Fewer levels for safety
     GridSpacingPoints = 20
     VolumePerLevel = 0.01
     StopLossPoints = 30
     TakeProfitPoints = 50

   SimpleScalpingOrchestrator (Medium Volatility):
     RiskAmount = BaseRiskAmount  // Uses preset's base risk
     StopLossPoints = 15
     TakeProfitPoints = 25
     IsBuy = Random              // Randomly selected direction

   QuickHedgeOrchestrator (High Volatility):
     RiskAmount = BaseRiskAmount × 0.7  // REDUCED RISK (70%)
     StopLossPoints = 25
     TakeProfitPoints = 40
     HedgeTriggerPoints = 15

   NewsStraddleOrchestrator (News Events):
     StraddleDistancePoints = 15
     Volume = 0.02
     SecondsBeforeNews = 30      // Shorter countdown for demo
     MaxWaitAfterNewsSeconds = 120

   PendingBreakoutOrchestrator (Breakout Signals):
     BreakoutDistancePoints = 20
     Volume = 0.01
     StopLossPoints = 20
     TakeProfitPoints = 40
     MaxWaitMinutes = 3

══════════════════════════════════════════════════════════════════════════════*/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;
using MetaRPC.CSharpMT5.Examples.Orchestrators;
using mt5_term_api;

namespace MetaRPC.CSharpMT5.Examples.Presets
{
    public class AdaptiveMarketModePreset
    {
        private readonly MT5Service _service;

        public string Symbol { get; set; } = "EURUSD";
        public int VolatilityLookbackBars { get; set; } = 20;  // Bars to analyze for volatility
        public double LowVolatilityThreshold { get; set; } = 15.0;  // Points
        public double HighVolatilityThreshold { get; set; } = 40.0;  // Points
        public double BaseRiskAmount { get; set; } = 20.0;  // Base risk per trade
        public bool EnableNewsMode { get; set; } = true;
        public int MinutesBeforeNews { get; set; } = 5;  // Switch to news mode N minutes before
        public int CycleDurationSeconds { get; set; } = 300;  // Run each cycle for 5 minutes

        public AdaptiveMarketModePreset(MT5Service service)
        {
            _service = service;
        }

        public async Task<double> ExecuteAsync(CancellationToken ct = default)
        {
            Console.WriteLine("\n+============================================================+");
            Console.WriteLine("|  ADAPTIVE MARKET MODE PRESET                              |");
            Console.WriteLine("+============================================================+\n");

            var initialBalance = await _service.GetBalanceAsync();
            Console.WriteLine($"  💰 Starting balance: ${initialBalance:F2}");
            Console.WriteLine($"  📊 Symbol: {Symbol}");
            Console.WriteLine($"  🎯 Base risk: ${BaseRiskAmount:F2}");
            Console.WriteLine($"  📈 Volatility thresholds: Low < {LowVolatilityThreshold} pts < Medium < {HighVolatilityThreshold} pts < High\n");

            double totalProfit = 0;
            int cycleNumber = 0;

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    cycleNumber++;
                    Console.WriteLine($"╔════════════════════════════════════════════════════════════╗");
                    Console.WriteLine($"║  CYCLE #{cycleNumber}                                           ");
                    Console.WriteLine($"╚════════════════════════════════════════════════════════════╝\n");

                    // Step 1: Analyze market conditions
                    var marketMode = await AnalyzeMarketConditions();
                    Console.WriteLine($"  🔍 Market Analysis Complete:");
                    Console.WriteLine($"     Mode: {marketMode.Mode}");
                    Console.WriteLine($"     Volatility: {marketMode.VolatilityPoints:F1} points");
                    Console.WriteLine($"     Reason: {marketMode.Reason}\n");

                    // Step 2: Select and execute appropriate orchestrator
                    double cycleProfit = 0;

                    switch (marketMode.Mode)
                    {
                        case MarketMode.Grid:
                            cycleProfit = await ExecuteGridMode();
                            break;

                        case MarketMode.Scalping:
                            cycleProfit = await ExecuteScalpingMode();
                            break;

                        case MarketMode.HighVolatility:
                            cycleProfit = await ExecuteHighVolatilityMode();
                            break;

                        case MarketMode.News:
                            cycleProfit = await ExecuteNewsMode();
                            break;

                        case MarketMode.Breakout:
                            cycleProfit = await ExecuteBreakoutMode();
                            break;

                        default:
                            Console.WriteLine("  ⚠️  Unknown market mode, skipping cycle\n");
                            await Task.Delay(10000, ct);
                            continue;
                    }

                    totalProfit += cycleProfit;

                    Console.WriteLine($"\n  📊 Cycle #{cycleNumber} Result:");
                    Console.WriteLine($"     Profit: ${cycleProfit:F2}");
                    Console.WriteLine($"     Total P/L: ${totalProfit:F2}");
                    Console.WriteLine($"     Current Balance: ${await _service.GetBalanceAsync():F2}\n");

                    // Safety check: stop if significant loss
                    if (totalProfit < -BaseRiskAmount * 5)
                    {
                        Console.WriteLine($"  🛑 STOP: Total loss exceeds ${BaseRiskAmount * 5:F2}");
                        break;
                    }

                    // Small pause between cycles
                    await ProgressBarHelper.WaitWithProgressBar(
                        totalSeconds: 30,
                        message: "Pause before next cycle",
                        ct: ct
                    );
                }

                var finalBalance = await _service.GetBalanceAsync();
                Console.WriteLine("\n+============================================================+");
                Console.WriteLine("|  FINAL RESULTS                                            |");
                Console.WriteLine("+============================================================+");
                Console.WriteLine($"  Initial Balance: ${initialBalance:F2}");
                Console.WriteLine($"  Final Balance: ${finalBalance:F2}");
                Console.WriteLine($"  Total Profit/Loss: ${totalProfit:F2}");
                Console.WriteLine($"  Cycles Completed: {cycleNumber}");
                Console.WriteLine("+============================================================+\n");

                return totalProfit;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n  ✗ Error: {ex.Message}");
                try
                {
                    await _service.CloseAll(Symbol);
                }
                catch { }
                return totalProfit;
            }
        }

        private async Task<MarketCondition> AnalyzeMarketConditions()
        {
            // Simplified volatility analysis using tick spread
            var tick = await _service.SymbolInfoTickAsync(Symbol);
            var point = await _service.GetPointAsync(Symbol);

            // Calculate current spread as proxy for volatility
            var spreadPoints = (tick.Ask - tick.Bid) / point;

            // Estimate volatility (simplified - in real system would use historical bars)
            var avgRangePoints = spreadPoints * 10;  // Rough estimate

            // Check if we're near a news event (simulated for demo)
            var currentHour = DateTime.UtcNow.Hour;
            var currentMinute = DateTime.UtcNow.Minute;
            bool isNewsTime = EnableNewsMode && IsNearNewsEvent(currentHour, currentMinute);

            if (isNewsTime)
            {
                return new MarketCondition
                {
                    Mode = MarketMode.News,
                    VolatilityPoints = avgRangePoints,
                    Reason = $"News event detected (UTC {currentHour:D2}:{currentMinute:D2})"
                };
            }

            // Detect breakout conditions (large spread = potential volatility)
            if (spreadPoints > 3.0)  // Unusually large spread
            {
                return new MarketCondition
                {
                    Mode = MarketMode.Breakout,
                    VolatilityPoints = avgRangePoints,
                    Reason = $"Breakout potential detected (spread: {spreadPoints:F1} pts)"
                };
            }

            // Classify by volatility
            if (avgRangePoints < LowVolatilityThreshold)
            {
                return new MarketCondition
                {
                    Mode = MarketMode.Grid,
                    VolatilityPoints = avgRangePoints,
                    Reason = $"Low volatility - range-bound market"
                };
            }
            else if (avgRangePoints < HighVolatilityThreshold)
            {
                return new MarketCondition
                {
                    Mode = MarketMode.Scalping,
                    VolatilityPoints = avgRangePoints,
                    Reason = $"Medium volatility - normal market conditions"
                };
            }
            else
            {
                return new MarketCondition
                {
                    Mode = MarketMode.HighVolatility,
                    VolatilityPoints = avgRangePoints,
                    Reason = $"High volatility - protective mode activated"
                };
            }
        }

        private bool IsNearNewsEvent(int hour, int minute)
        {
            // Simplified news schedule (UTC times)
            // Major news typically at: 08:30, 12:30, 14:00, 18:00, 19:00
            int[] newsHours = { 8, 12, 14, 18, 19 };

            foreach (var newsHour in newsHours)
            {
                if (hour == newsHour && minute >= (30 - MinutesBeforeNews) && minute <= 45)
                    return true;
            }

            return false;
        }

        private async Task<double> ExecuteGridMode()
        {
            Console.WriteLine("  🎯 Executing: GRID TRADING ORCHESTRATOR\n");

            var orchestrator = new GridTradingOrchestrator(_service)
            {
                Symbol = Symbol,
                GridLevels = 3,  // Fewer levels for safety
                GridSpacingPoints = 20,
                VolumePerLevel = 0.01,
                StopLossPoints = 30,
                TakeProfitPoints = 50
            };

            return await orchestrator.ExecuteAsync();
        }

        private async Task<double> ExecuteScalpingMode()
        {
            Console.WriteLine("  🎯 Executing: SIMPLE SCALPING ORCHESTRATOR\n");

            var orchestrator = new SimpleScalpingOrchestrator(_service)
            {
                Symbol = Symbol,
                RiskAmount = BaseRiskAmount,
                StopLossPoints = 15,
                TakeProfitPoints = 25,
                IsBuy = DateTime.UtcNow.Second % 2 == 0  // Random direction
            };

            return await orchestrator.ExecuteAsync();
        }

        private async Task<double> ExecuteHighVolatilityMode()
        {
            Console.WriteLine("  🎯 Executing: QUICK HEDGE ORCHESTRATOR (Protection Mode)\n");

            var orchestrator = new QuickHedgeOrchestrator(_service)
            {
                Symbol = Symbol,
                RiskAmount = BaseRiskAmount * 0.7,  // Reduce risk in volatile conditions
                StopLossPoints = 25,
                TakeProfitPoints = 40,
                OpenBuyFirst = DateTime.UtcNow.Second % 2 == 0,
                HedgeTriggerPoints = 15
            };

            return await orchestrator.ExecuteAsync();
        }

        private async Task<double> ExecuteNewsMode()
        {
            Console.WriteLine("  🎯 Executing: NEWS STRADDLE ORCHESTRATOR\n");

            var orchestrator = new NewsStraddleOrchestrator(_service)
            {
                Symbol = Symbol,
                StraddleDistancePoints = 15,
                Volume = 0.02,
                StopLossPoints = 20,
                TakeProfitPoints = 40,
                SecondsBeforeNews = 30,  // Shorter for demo
                MaxWaitAfterNewsSeconds = 120
            };

            return await orchestrator.ExecuteAsync();
        }

        private async Task<double> ExecuteBreakoutMode()
        {
            Console.WriteLine("  🎯 Executing: PENDING BREAKOUT ORCHESTRATOR\n");

            var orchestrator = new PendingBreakoutOrchestrator(_service)
            {
                Symbol = Symbol,
                Volume = 0.01,
                BreakoutDistancePoints = 20,
                StopLossPoints = 20,
                TakeProfitPoints = 40,
                MaxWaitMinutes = 3
            };

            return await orchestrator.ExecuteAsync();
        }

        private class MarketCondition
        {
            public MarketMode Mode { get; set; }
            public double VolatilityPoints { get; set; }
            public string Reason { get; set; } = string.Empty;
        }

        private enum MarketMode
        {
            Grid,           // Low volatility, range-bound
            Scalping,       // Medium volatility, normal conditions
            HighVolatility, // High volatility, need protection
            News,           // Before news event
            Breakout        // Breakout detected
        }
    }
}
