/*══════════════════════════════════════════════════════════════════════════════
 ORCHESTRATOR: SimpleScalpingOrchestrator

 PURPOSE:
   Quick scalping strategy with tight stops and risk-based position sizing.
   Opens market position, holds for max duration, then closes automatically.

 STRATEGY:
   • Opens BUY or SELL market order using risk-based sizing
   • Sets tight SL/TP (default: 10pt stop, 20pt profit)
   • Holds position for max duration (default: 60 seconds)
   • Closes manually if SL/TP not hit within time limit

 VISUAL EXAMPLE (BUY position):

   ┌─────────────────────────────────────┐
   │ TAKE PROFIT: +20 pts                │ ← Close with profit
   ├─────────────────────────────────────┤
   │                                     │
   │ >>> ENTRY: Market BUY <<<           │ ← Open position NOW
   │                                     │
   ├─────────────────────────────────────┤
   │ STOP LOSS: -10 pts                  │ ← Cut losses
   └─────────────────────────────────────┘

   Risk/Reward: 1:2 (risk 10 pts to gain 20 pts)
   Max Hold: 60 seconds → force close if SL/TP not hit

 DEMONSTRATED FEATURES:
   [1] BuyMarketByRisk  - Risk-based BUY order with auto lot calculation
   [2] SellMarketByRisk - Risk-based SELL order with auto lot calculation
   [3] OpenedOrdersTicketsAsync - Check if position still open
   [4] CloseByTicket    - Manual position close by ticket number

 KEY PARAMETERS:
   • Symbol: Trading pair (default: EURUSD)
   • RiskAmount: Dollar risk per trade (default: $20)
   • StopLossPoints: Stop loss distance (default: 10 pts)
   • TakeProfitPoints: Take profit distance (default: 20 pts)
   • IsBuy: Direction - true=BUY, false=SELL (default: true)
   • MaxHoldSeconds: Max hold time before force close (default: 60s)

 USE CASE:
   Best for high-frequency trading during volatile market conditions.
   Minimizes exposure time while targeting quick profits.

 COMMAND-LINE USAGE:
   dotnet run 13
   dotnet run scalping
   dotnet run simplescalping

 PROGRAMMATIC USAGE:

   ⚙️ PARAMETER CONFIGURATION IS LOCATED IN Program.cs

   WHY THIS SEPARATION EXISTS:
   • SimpleScalpingOrchestrator.cs = STRATEGY ENGINE (logic, algorithm)
   • Program.cs → RunOrchestrator_Scalping() = RUNTIME CONFIGURATION (parameters)

   THIS SEPARATION IS NEEDED FOR:
   1️⃣ Code Reusability
      → Same orchestrator class can run with different parameters
      → No need to modify strategy logic to change parameters

   2️⃣ Quick Testing
      → Want to test tighter stops? Change numbers in Program.cs
      → Want wider stops? Again, only change Program.cs
      → Core algorithm remains untouched

   3️⃣ User Examples
      → Program.cs shows HOW to properly configure the orchestrator
      → All available parameters and their default values are visible

   4️⃣ Centralized Entry Point
      → All strategies launch through Program.cs
      → Single entry point: dotnet run scalping → RunOrchestrator_Scalping()

   📍 WHERE TO CONFIGURE PARAMETERS:
   Program.cs → method RunOrchestrator_Scalping() (lines 560-586)

   CONFIGURATION CODE IN Program.cs:

   private static async Task RunOrchestrator_Scalping()
   {
       var config = ConnectionHelper.BuildConfiguration();
       var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);

       try
       {
           var service = new MT5Service(account);

           var orchestrator = new SimpleScalpingOrchestrator(service)
           {
               Symbol = "EURUSD",              // ← Which trading pair
               RiskAmount = 20.0,              // ← Dollar risk per trade
               StopLossPoints = 10,            // ← Stop loss (points)
               TakeProfitPoints = 20,          // ← Take profit (points)
               IsBuy = true,                   // ← Direction (true=BUY, false=SELL)
               MaxHoldSeconds = 60             // ← Max hold time before force close (seconds)
           };

           await orchestrator.ExecuteAsync();
       }
       finally
       {
           await account.GrpcChannel.ShutdownAsync();
       }
   }

   💡 EXAMPLE: Adjusting for Different Scalping Styles

   // Option 1: Tight scalping for low volatility (default in Program.cs)
   StopLossPoints = 10,       // ← tight stop for quick cuts
   TakeProfitPoints = 20,     // ← 2:1 reward/risk ratio
   MaxHoldSeconds = 60        // ← quick in/out

   // Option 2: Relaxed scalping for higher volatility (modify in Program.cs)
   StopLossPoints = 20,       // ← wider stop for noise tolerance
   TakeProfitPoints = 40,     // ← larger target for bigger moves
   MaxHoldSeconds = 120       // ← more time to develop

   📝 IMPORTANT:
   • To change parameters → edit Program.cs, NOT this file
   • This file (SimpleScalpingOrchestrator.cs) contains only LOGIC
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
    public class SimpleScalpingOrchestrator
    {
        private readonly MT5Service _service;

        public string Symbol { get; set; } = "EURUSD";
        public double RiskAmount { get; set; } = 20.0;  // $20 risk per trade
        public int StopLossPoints { get; set; } = 10;
        public int TakeProfitPoints { get; set; } = 20;
        public bool IsBuy { get; set; } = true;
        public int MaxHoldSeconds { get; set; } = 60;  // Max 60 seconds hold time

        public SimpleScalpingOrchestrator(MT5Service service)
        {
            _service = service;
        }

        public async Task<double> ExecuteAsync(CancellationToken ct = default)
        {
            Console.WriteLine("\n+============================================================+");
            Console.WriteLine("|  SIMPLE SCALPING ORCHESTRATOR                             |");
            Console.WriteLine("+============================================================+\n");

            var initialBalance = await _service.GetBalanceAsync();
            Console.WriteLine($"  Starting balance: ${initialBalance:F2}");
            Console.WriteLine($"  Symbol: {Symbol}");
            Console.WriteLine($"  Direction: {(IsBuy ? "BUY" : "SELL")}");
            Console.WriteLine($"  Risk: ${RiskAmount:F2}");
            Console.WriteLine($"  SL: {StopLossPoints} pts | TP: {TakeProfitPoints} pts");
            Console.WriteLine($"  Max hold: {MaxHoldSeconds}s\n");

            try
            {
                // Place market order using risk-based sizing
                Console.WriteLine("  Opening position...");
                OrderSendData result;

                if (IsBuy)
                {
                    result = await _service.BuyMarketByRisk(
                        symbol: Symbol,
                        stopPoints: StopLossPoints,
                        riskMoney: RiskAmount,
                        tpPoints: TakeProfitPoints,
                        comment: "Scalper"
                    );
                }
                else
                {
                    result = await _service.SellMarketByRisk(
                        symbol: Symbol,
                        stopPoints: StopLossPoints,
                        riskMoney: RiskAmount,
                        tpPoints: TakeProfitPoints,
                        comment: "Scalper"
                    );
                }

                if (result.ReturnedCode != 10009)  // TRADE_RETCODE_DONE
                {
                    Console.WriteLine($"  ✗ Order failed: {result.Comment}");
                    return 0;
                }

                Console.WriteLine($"  ✓ Position opened: #{result.Order}");
                Console.WriteLine($"  Volume: {result.Volume:F2} lots\n");

                // Hold for max duration, then check if still open
                await ProgressBarHelper.WaitWithProgressBar(
                    totalSeconds: MaxHoldSeconds,
                    message: "Holding position",
                    ct: ct
                );

                // Check if position still exists
                var tickets = await _service.OpenedOrdersTicketsAsync();
                bool stillOpen = false;
                foreach (var ticket in tickets.OpenedPositionTickets)
                {
                    if (ticket == (long)result.Order)
                    {
                        stillOpen = true;
                        break;
                    }
                }

                if (stillOpen)
                {
                    Console.WriteLine($"  Position still open after {MaxHoldSeconds}s - closing manually...");
                    await _service.CloseByTicket(result.Order);
                    Console.WriteLine("  ✓ Position closed");
                }
                else
                {
                    Console.WriteLine("  ✓ Position closed automatically (SL/TP hit)");
                }

                var finalBalance = await _service.GetBalanceAsync();
                var profit = finalBalance - initialBalance;

                Console.WriteLine($"\n  Final balance: ${finalBalance:F2}");
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
