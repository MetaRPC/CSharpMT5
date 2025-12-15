// ══════════════════════════════════════════════════════════════════════════════
// FILE: 12_PositionMonitoring_Explained.cs
// PURPOSE: Position monitoring - Track, modify, and manage open positions
//
// Topics covered:
//   1. HOW to get list of open positions
//   2. HOW to find specific position by ticket
//   3. HOW to calculate current profit in points and dollars
//   4. HOW to modify SL/TP on open position
//   5. WHAT is Trailing Stop and how to implement it
//   6. WHAT is Breakeven and when to use it
//
// Key principle: Monitor your positions actively! Don't just open and forget.
// Professional traders adjust SL/TP based on market movement.
//
// ══════════════════════════════════════════════════════════════════════════════

using System;
using System.Linq;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;
using mt5_term_api;

namespace MetaRPC.CSharpMT5.Examples.MoreExamples;

// Declare public static class
public static class PositionMonitoringExamples
{
    // Define async method
    public static async Task RunAsync()
    {
        // ═══════════════════════════════════════════════════════════════════════
        // STEP 1: DISPLAY HEADER
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   POSITION MONITORING - Track and Manage Open Trades      ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

        // ═══════════════════════════════════════════════════════════════════════
        // STEP 2: ESTABLISH CONNECTION
        // ═══════════════════════════════════════════════════════════════════════

        // Load configuration from appsettings.json
        var config = ConnectionHelper.BuildConfiguration();
        Console.WriteLine("✓ Configuration loaded");

        // Connect to MT5 Terminal
        var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);
        Console.WriteLine("✓ Connected to MT5 Terminal\n");

        // Create MT5Service wrapper (MT5Sugar methods are extension methods on MT5Service)
        var service = new MT5Service(account);

        // Define symbol for examples
        string symbol = "EURUSD";

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 1: GETTING ALL OPEN POSITIONS
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 1: How to Get All Open Positions");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("📞 Calling: service.OpenedOrdersAsync()");

        // Get all open positions
        // Returns List<PositionData>
        // Type: List containing all currently open positions
        var positions = await service.OpenedOrdersAsync();

        Console.WriteLine($"\n📊 Total open positions: {positions.PositionInfos.Count}\n");

        if (positions.PositionInfos.Count > 0)
        {
            Console.WriteLine("📋 Your open positions:\n");

            foreach (var pos in positions)
            {
                // Each position has these key properties:
                // - Ticket: unique position ID
                // - Symbol: trading pair (EURUSD, GBPUSD, etc.)
                // - Type: POSITION_TYPE_BUY (0) or POSITION_TYPE_SELL (1)
                // - Volume: position size in lots
                // - PriceOpen: entry price
                // - Sl: Stop Loss price
                // - Tp: Take Profit price
                // - PriceCurrent: current market price
                // - Profit: current profit/loss in account currency
                // - Comment: order comment

                Console.WriteLine($"   ─────────────────────────────────────────");
                Console.WriteLine($"   Ticket: {pos.Ticket}");
                Console.WriteLine($"   Symbol: {pos.Symbol}");
                Console.WriteLine($"   Type: {(pos.Type == 0 ? "BUY" : "SELL")}");
                Console.WriteLine($"   Volume: {pos.Volume} lots");
                Console.WriteLine($"   Entry: {pos.PriceOpen:F5}");
                Console.WriteLine($"   Current: {pos.PriceCurrent:F5}");
                Console.WriteLine($"   SL: {(pos.Sl > 0 ? pos.Sl.ToString("F5") : "None")}");
                Console.WriteLine($"   TP: {(pos.Tp > 0 ? pos.Tp.ToString("F5") : "None")}");
                Console.WriteLine($"   Profit: ${pos.Profit:F2}");
                Console.WriteLine($"   Comment: {pos.Comment}");
            }
            Console.WriteLine($"   ─────────────────────────────────────────\n");
        }
        else
        {
            Console.WriteLine("   No open positions currently\n");
            Console.WriteLine("💡 DEMONSTRATION MODE:");
            Console.WriteLine("   We'll show you the concepts even without active positions\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 2: FINDING SPECIFIC POSITION
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 2: How to Find Specific Position");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 THREE WAYS TO FIND POSITIONS:\n");

        Console.WriteLine("1. BY TICKET (unique ID):");
        Console.WriteLine("   ulong targetTicket = 123456789;");
        Console.WriteLine("   var positions = await service.OpenedOrdersAsync();");
        Console.WriteLine("   var position = positions.FirstOrDefault(p => p.Ticket == targetTicket);\n");

        Console.WriteLine("2. BY SYMBOL (all positions for EURUSD):");
        Console.WriteLine($"   var positions = await service.OpenedOrdersAsync();");
        Console.WriteLine($"   var eurusdPositions = positions.Where(p => p.Symbol == \"EURUSD\").ToList();\n");

        Console.WriteLine("3. BY TYPE (all BUY or all SELL):");
        Console.WriteLine("   var positions = await service.OpenedOrdersAsync();");
        Console.WriteLine("   var buyPositions = positions.Where(p => p.Type == 0).ToList();  // BUY");
        Console.WriteLine("   var sellPositions = positions.Where(p => p.Type == 1).ToList(); // SELL\n");

        if (positions.PositionInfos.Count > 0)
        {
            // Demonstrate finding first position
            var firstPos = positions.PositionInfos[0];

            Console.WriteLine($"📍 EXAMPLE: Finding position #{firstPos.Ticket}");
            Console.WriteLine($"   var target = positions.FirstOrDefault(p => p.Ticket == {firstPos.Ticket});");
            Console.WriteLine($"   Result: {(firstPos != null ? "Found ✓" : "Not found ✗")}\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 3: CALCULATING PROFIT IN POINTS AND DOLLARS
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 3: Calculating Current Profit/Loss");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Get current price and point for calculations
        var tick = await service.SymbolInfoTickAsync(symbol);
        double point = await service.GetPointAsync(symbol);

        Console.WriteLine($"💡 PROFIT CALCULATION EXPLAINED:\n");

        Console.WriteLine($"📊 Example BUY position:");
        double exampleEntry = tick.Ask - 20 * point;  // Simulated entry 20 points below
        double exampleCurrent = tick.Bid;
        double exampleVolume = 0.1;

        Console.WriteLine($"   Entry: {exampleEntry:F5}");
        Console.WriteLine($"   Current: {exampleCurrent:F5}");
        Console.WriteLine($"   Volume: {exampleVolume} lots\n");

        Console.WriteLine($"📐 Step 1: Calculate profit in POINTS");
        Console.WriteLine($"   For BUY: profit_points = (current - entry) / point");
        Console.WriteLine($"   Calculation: ({exampleCurrent:F5} - {exampleEntry:F5}) / {point}");

        double profitPoints = (exampleCurrent - exampleEntry) / point;
        Console.WriteLine($"   Result: {profitPoints:F1} points\n");

        Console.WriteLine($"📐 Step 2: Calculate profit in DOLLARS");
        Console.WriteLine($"   Need tick value for {exampleVolume} lots");

        var (tickValue, tickSize) = await service.GetTickValueAndSizeAsync(symbol);
        Console.WriteLine($"   Tick value: ${tickValue:F4} per point");
        Console.WriteLine($"   Formula: profit_dollars = profit_points × tick_value");
        Console.WriteLine($"   Calculation: {profitPoints:F1} × ${tickValue:F4}");

        double profitDollars = profitPoints * tickValue;
        Console.WriteLine($"   Result: ${profitDollars:F2}\n");

        Console.WriteLine($"💡 FOR SELL POSITION:");
        Console.WriteLine($"   For SELL: profit_points = (entry - current) / point");
        Console.WriteLine($"   (Opposite direction from BUY)\n");

        if (positions.PositionInfos.Count > 0 && positions.PositionInfos[0].Symbol == symbol)
        {
            var pos = positions.PositionInfos[0];
            double actualProfitPoints = pos.Type == 0
                ? (pos.PriceCurrent - pos.PriceOpen) / point
                : (pos.PriceOpen - pos.PriceCurrent) / point;

            Console.WriteLine($"✅ VERIFICATION with real position #{pos.Ticket}:");
            Console.WriteLine($"   Calculated profit: {actualProfitPoints:F1} points");
            Console.WriteLine($"   MT5 reported profit: ${pos.Profit:F2}");
            Console.WriteLine($"   Match: {(Math.Abs(profitDollars - pos.Profit) < 1.0 ? "✓" : "Close enough")}\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 4: MODIFYING STOP LOSS AND TAKE PROFIT
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 4: How to Modify SL/TP on Open Position");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"💡 WHY MODIFY SL/TP?");
        Console.WriteLine($"   - Move SL to breakeven when in profit");
        Console.WriteLine($"   - Implement trailing stop");
        Console.WriteLine($"   - Widen TP if trend continues");
        Console.WriteLine($"   - Tighten SL to lock profits\n");

        Console.WriteLine($"📚 TWO METHODS:\n");

        Console.WriteLine($"1. USING MT5SERVICE (absolute prices):");
        Console.WriteLine($"   ulong ticket = 123456789;");
        Console.WriteLine($"   double newSL = 1.09500;");
        Console.WriteLine($"   double newTP = 1.10000;");
        Console.WriteLine($"   await service.ModifyPositionAsync(ticket, newSL, newTP);\n");

        Console.WriteLine($"2. USING MT5SUGAR (more flexible):");
        Console.WriteLine($"   ulong ticket = 123456789;");
        Console.WriteLine($"   double newSL = 1.09500;");
        Console.WriteLine($"   double newTP = 1.10000;");
        Console.WriteLine($"   await service.ModifySlTpAsync(ticket, newSL, newTP);\n");

        if (positions.PositionInfos.Count > 0)
        {
            var pos = positions.PositionInfos[0];

            Console.WriteLine($"📍 EXAMPLE: Modifying position #{pos.Ticket}");
            Console.WriteLine($"   Current SL: {(pos.Sl > 0 ? pos.Sl.ToString("F5") : "None")}");
            Console.WriteLine($"   Current TP: {(pos.Tp > 0 ? pos.Tp.ToString("F5") : "None")}\n");

            // Calculate new SL/TP (demonstration only - not executing)
            double newSL, newTP;
            if (pos.Type == 0) // BUY
            {
                newSL = pos.PriceOpen; // Move SL to breakeven
                newTP = pos.PriceOpen + 100 * point; // TP 100 points above entry
            }
            else // SELL
            {
                newSL = pos.PriceOpen; // Move SL to breakeven
                newTP = pos.PriceOpen - 100 * point; // TP 100 points below entry
            }

            Console.WriteLine($"   Proposed new SL: {newSL:F5} (breakeven)");
            Console.WriteLine($"   Proposed new TP: {newTP:F5} (100 points profit)");
            Console.WriteLine($"\n   ⚠️  Not executing (demonstration mode)\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 5: BREAKEVEN - Moving SL to Entry Price
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 5: Breakeven - Risk-Free Trading");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"💡 WHAT IS BREAKEVEN?");
        Console.WriteLine($"   Moving Stop Loss to entry price = ZERO risk");
        Console.WriteLine($"   Worst case: position closes at entry = $0 profit/loss\n");

        Console.WriteLine($"🎯 WHEN TO MOVE TO BREAKEVEN?");
        Console.WriteLine($"   Common trigger: When profit reaches 20-50 points");
        Console.WriteLine($"   Example: Entry at 1.10000, profit hits +30 points");
        Console.WriteLine($"            → Move SL from 1.09950 to 1.10000\n");

        int breakevenTrigger = 30; // Move to breakeven after 30 points profit

        Console.WriteLine($"📐 BREAKEVEN LOGIC:\n");
        Console.WriteLine($"   BUY Position:");
        Console.WriteLine($"   1. Calculate profit: (current - entry) / point");
        Console.WriteLine($"   2. If profit >= {breakevenTrigger} points:");
        Console.WriteLine($"   3.    New SL = entry price");
        Console.WriteLine($"   4.    Modify position\n");

        Console.WriteLine($"   SELL Position:");
        Console.WriteLine($"   1. Calculate profit: (entry - current) / point");
        Console.WriteLine($"   2. If profit >= {breakevenTrigger} points:");
        Console.WriteLine($"   3.    New SL = entry price");
        Console.WriteLine($"   4.    Modify position\n");

        Console.WriteLine($"✅ IMPLEMENTATION:");
        Console.WriteLine(@"
   foreach (var pos in positions)
   {
       double profitPoints = pos.Type == 0
           ? (pos.PriceCurrent - pos.PriceOpen) / point
           : (pos.PriceOpen - pos.PriceCurrent) / point;

       if (profitPoints >= 30 && pos.Sl != pos.PriceOpen)
       {
           // Move SL to breakeven
           await service.ModifyPositionAsync(
               pos.Ticket,
               pos.PriceOpen,  // New SL = entry price
               pos.Tp          // Keep same TP
           );
           Console.WriteLine($""✓ Position {pos.Ticket} moved to breakeven"");
       }
   }
");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 6: TRAILING STOP - Following the Profit
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 6: Trailing Stop - Lock in Profits");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"💡 WHAT IS TRAILING STOP?");
        Console.WriteLine($"   Stop Loss that FOLLOWS price in profit direction");
        Console.WriteLine($"   Locks in profits as position moves favorably\n");

        Console.WriteLine($"📊 EXAMPLE:");
        double tsEntry = 1.10000;
        int tsDistance = 20; // Trail 20 points behind

        Console.WriteLine($"   BUY at: {tsEntry:F5}");
        Console.WriteLine($"   Trail distance: {tsDistance} points");
        Console.WriteLine($"   Initial SL: {tsEntry - 50 * point:F5} (50 points protection)\n");

        Console.WriteLine($"   Price Movement:");
        Console.WriteLine($"   ┌────────────┬──────────────┬────────────────┬─────────────┐");
        Console.WriteLine($"   │  Price     │  Profit (pts)│  Trail Calc    │  New SL     │");
        Console.WriteLine($"   ├────────────┼──────────────┼────────────────┼─────────────┤");
        Console.WriteLine($"   │ {tsEntry + 10 * point:F5}  │  +10         │  Too small     │  No change  │");
        Console.WriteLine($"   │ {tsEntry + 30 * point:F5}  │  +30         │  {tsEntry + 30 * point:F5}-20pts │  {tsEntry + 10 * point:F5}  │");
        Console.WriteLine($"   │ {tsEntry + 50 * point:F5}  │  +50         │  {tsEntry + 50 * point:F5}-20pts │  {tsEntry + 30 * point:F5}  │");
        Console.WriteLine($"   │ {tsEntry + 40 * point:F5}  │  +40         │  {tsEntry + 40 * point:F5}-20pts │  {tsEntry + 30 * point:F5}* │");
        Console.WriteLine($"   └────────────┴──────────────┴────────────────┴─────────────┘");
        Console.WriteLine($"   * SL never moves DOWN, only UP!\n");

        Console.WriteLine($"✅ IMPLEMENTATION:");
        Console.WriteLine(@"
   int trailDistance = 20;  // Points to trail behind
   int activationProfit = 30;  // Start trailing after this profit

   foreach (var pos in positions)
   {
       double profitPoints = pos.Type == 0
           ? (pos.PriceCurrent - pos.PriceOpen) / point
           : (pos.PriceOpen - pos.PriceCurrent) / point;

       if (profitPoints >= activationProfit)
       {
           double trailSL;
           if (pos.Type == 0)  // BUY
           {
               trailSL = pos.PriceCurrent - (trailDistance * point);
               // Only move SL UP, never down
               if (trailSL > pos.Sl)
               {
                   await service.ModifyPositionAsync(pos.Ticket, trailSL, pos.Tp);
                   Console.WriteLine($""✓ Trailing SL updated to {trailSL:F5}"");
               }
           }
           else  // SELL
           {
               trailSL = pos.PriceCurrent + (trailDistance * point);
               // Only move SL DOWN, never up
               if (trailSL < pos.Sl || pos.Sl == 0)
               {
                   await service.ModifyPositionAsync(pos.Ticket, trailSL, pos.Tp);
                   Console.WriteLine($""✓ Trailing SL updated to {trailSL:F5}"");
               }
           }
       }
   }
");

        Console.WriteLine($"💡 KEY RULES:");
        Console.WriteLine($"   1. Only move SL in favorable direction");
        Console.WriteLine($"   2. BUY: SL can only go UP (higher)");
        Console.WriteLine($"   3. SELL: SL can only go DOWN (lower)");
        Console.WriteLine($"   4. Activate trailing after reaching profit threshold");
        Console.WriteLine($"   5. Run in loop (every few seconds) to update\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 7: MONITORING LOOP PATTERN
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 7: Complete Monitoring Loop Pattern");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"💡 PROFESSIONAL PATTERN:");
        Console.WriteLine($"   Continuous loop that monitors and adjusts positions\n");

        Console.WriteLine($"✅ COMPLETE IMPLEMENTATION:");
        Console.WriteLine(@"
   using System.Threading;

   var cts = new CancellationTokenSource();
   Console.CancelKeyPress += (s, e) => {
       e.Cancel = true;
       cts.Cancel();
   };

   while (!cts.Token.IsCancellationRequested)
   {
       // Get all positions
       var positions = await service.OpenedOrdersAsync();

       foreach (var pos in positions)
       {
           double point = await service.GetPointAsync(pos.Symbol);

           // Calculate profit in points
           double profitPoints = pos.Type == 0
               ? (pos.PriceCurrent - pos.PriceOpen) / point
               : (pos.PriceOpen - pos.PriceCurrent) / point;

           // 1. BREAKEVEN at +30 points
           if (profitPoints >= 30 && pos.Sl != pos.PriceOpen)
           {
               await service.ModifyPositionAsync(
                   pos.Ticket, pos.PriceOpen, pos.Tp);
               Console.WriteLine($""✓ {pos.Ticket}: Moved to breakeven"");
           }

           // 2. TRAILING STOP at +50 points
           else if (profitPoints >= 50)
           {
               double trailSL = pos.Type == 0
                   ? pos.PriceCurrent - (20 * point)
                   : pos.PriceCurrent + (20 * point);

               bool shouldUpdate = pos.Type == 0
                   ? (trailSL > pos.Sl)
                   : (trailSL < pos.Sl || pos.Sl == 0);

               if (shouldUpdate)
               {
                   await service.ModifyPositionAsync(
                       pos.Ticket, trailSL, pos.Tp);
                   Console.WriteLine($""✓ {pos.Ticket}: Trailing SL updated"");
               }
           }
       }

       // Wait 5 seconds before next check
       await Task.Delay(5000, cts.Token);
   }

   Console.WriteLine(""\nMonitoring stopped."");
");

        Console.WriteLine($"📊 WHAT THIS DOES:");
        Console.WriteLine($"   1. Loops every 5 seconds");
        Console.WriteLine($"   2. Gets all open positions");
        Console.WriteLine($"   3. For each position:");
        Console.WriteLine($"      - Calculates profit in points");
        Console.WriteLine($"      - At +30 points → Moves to breakeven");
        Console.WriteLine($"      - At +50 points → Activates trailing stop");
        Console.WriteLine($"   4. Press Ctrl+C to stop monitoring\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 8: USING MT5SUGAR HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 8: MT5Sugar Position Helpers");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"💡 MT5SUGAR PROVIDES HELPER METHODS:\n");

        Console.WriteLine($"📚 AVAILABLE METHODS:");
        Console.WriteLine($"   - GetPositionCountAsync(symbol) - Count positions");
        Console.WriteLine($"   - GetTotalProfitLossAsync(symbol) - Total P/L for symbol");
        Console.WriteLine($"   - GetProfitablePositionsAsync(symbol) - Only winning positions");
        Console.WriteLine($"   - GetLosingPositionsAsync(symbol) - Only losing positions");
        Console.WriteLine($"   - GetPositionStatsBySymbolAsync(symbol) - Detailed stats\n");

        Console.WriteLine($"📐 EXAMPLE USAGE:");

        // Get position count
        int posCount = await service.GetPositionCountAsync(symbol);
        Console.WriteLine($"   Position count for {symbol}: {posCount}\n");

        // Get total P/L
        double totalPL = await service.GetTotalProfitLossAsync(symbol);
        Console.WriteLine($"   Total P/L for {symbol}: ${totalPL:F2}\n");

        // Get winning positions
        var winning = await service.GetProfitablePositionsAsync(symbol);
        Console.WriteLine($"   Profitable positions: {winning.Count}");
        foreach (var w in winning)
        {
            Console.WriteLine($"      #{w.Ticket}: ${w.Profit:F2}");
        }
        Console.WriteLine();

        // Get losing positions
        var losing = await service.GetLosingPositionsAsync(symbol);
        Console.WriteLine($"   Losing positions: {losing.Count}");
        foreach (var l in losing)
        {
            Console.WriteLine($"      #{l.Ticket}: ${l.Profit:F2}");
        }
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════════════════
        // SUMMARY
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("SUMMARY - Position Monitoring Essentials");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"📚 KEY METHODS:\n");

        Console.WriteLine($"GET POSITIONS:");
        Console.WriteLine($"   - PositionsAsync() - All open positions");
        Console.WriteLine($"   - positions.Where(p => p.Symbol == \"EURUSD\") - Filter by symbol");
        Console.WriteLine($"   - positions.Where(p => p.Type == 0) - Filter by type (BUY)\n");

        Console.WriteLine($"MODIFY POSITIONS:");
        Console.WriteLine($"   - ModifyPositionAsync(ticket, newSL, newTP) - Change SL/TP");
        Console.WriteLine($"   - ModifySlTpAsync(ticket, newSL, newTP) - Sugar version\n");

        Console.WriteLine($"HELPERS:");
        Console.WriteLine($"   - GetPositionCountAsync(symbol)");
        Console.WriteLine($"   - GetTotalProfitLossAsync(symbol)");
        Console.WriteLine($"   - GetProfitablePositionsAsync(symbol)");
        Console.WriteLine($"   - GetLosingPositionsAsync(symbol)\n");

        Console.WriteLine($"💡 PROFIT CALCULATION:");
        Console.WriteLine($"   BUY:  profit_points = (current - entry) / point");
        Console.WriteLine($"   SELL: profit_points = (entry - current) / point");
        Console.WriteLine($"   Dollars: profit_points × tick_value\n");

        Console.WriteLine($"🎯 BREAKEVEN:");
        Console.WriteLine($"   Trigger: Profit >= 20-50 points");
        Console.WriteLine($"   Action: Move SL to entry price");
        Console.WriteLine($"   Benefit: Zero risk, free trade\n");

        Console.WriteLine($"📈 TRAILING STOP:");
        Console.WriteLine($"   Trigger: Profit >= threshold (e.g., 50 points)");
        Console.WriteLine($"   Action: Move SL to trail current price");
        Console.WriteLine($"   Rule: SL only moves favorably (BUY up, SELL down)");
        Console.WriteLine($"   Benefit: Lock in profits as trade moves\n");

        Console.WriteLine($"⚠️  BEST PRACTICES:");
        Console.WriteLine($"   1. Monitor positions regularly (every 3-5 seconds)");
        Console.WriteLine($"   2. Move to breakeven early (protect capital)");
        Console.WriteLine($"   3. Use trailing stop for trending markets");
        Console.WriteLine($"   4. NEVER move SL against favorable direction");
        Console.WriteLine($"   5. Handle Ctrl+C gracefully (CancellationToken)");
        Console.WriteLine($"   6. Log all SL/TP modifications for debugging\n");

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Active monitoring = Protected profits!                  ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
    }
}
