// ══════════════════════════════════════════════════════════════════════════════
// FILE: 13_MultiplePositions_Explained.cs
// PURPOSE: Multiple positions - Managing several trades simultaneously
//
// Topics covered:
//   1. HEDGING vs NETTING mode - what's the difference
//   2. HOW to open multiple positions on same symbol
//   3. HOW to close all positions at once
//   4. HOW to close only BUY or only SELL positions
//   5. WHAT is partial close and when to use it
//   6. HOW to calculate total exposure across positions
//
// Key principle: In HEDGING mode you can have multiple positions on same symbol
// (both BUY and SELL simultaneously). This is powerful but requires careful management!
//
// ══════════════════════════════════════════════════════════════════════════════

using System;
using System.Linq;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;

namespace MetaRPC.CSharpMT5.Examples.MoreExamples;

// Declare public static class
public static class MultiplePositionsExamples
{
    // Define async method
    public static async Task RunAsync()
    {
        // ═══════════════════════════════════════════════════════════════════════
        // STEP 1: DISPLAY HEADER
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   MULTIPLE POSITIONS - Managing Several Trades at Once    ║");
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
        // EXAMPLE 1: HEDGING vs NETTING MODE
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 1: Understanding HEDGING vs NETTING");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Get account info to check mode
        var accountInfo = await account.AccountSummaryAsync();

        // Check margin mode
        // ACCOUNT_MARGIN_MODE_RETAIL_HEDGING = 3 (hedging allowed)
        // ACCOUNT_MARGIN_MODE_RETAIL_NETTING = 4 (netting only)
        bool isHedging = accountInfo.MarginMode == 3;

        Console.WriteLine($"📊 Your Account Mode:");
        Console.WriteLine($"   Margin Mode: {accountInfo.MarginMode}");
        Console.WriteLine($"   Mode: {(isHedging ? "HEDGING ✓" : "NETTING")}\n");

        Console.WriteLine($"💡 HEDGING MODE:");
        Console.WriteLine($"   - Can have MULTIPLE positions on same symbol");
        Console.WriteLine($"   - Can have BUY and SELL simultaneously");
        Console.WriteLine($"   - Each position has unique ticket");
        Console.WriteLine($"   - Example: 3 BUY EURUSD + 2 SELL EURUSD = 5 positions");
        Console.WriteLine($"   - Used by: Most non-US brokers\n");

        Console.WriteLine($"💡 NETTING MODE:");
        Console.WriteLine($"   - Can have ONLY ONE position per symbol");
        Console.WriteLine($"   - Opening opposite direction = reduces/reverses position");
        Console.WriteLine($"   - Example: BUY 0.1 + BUY 0.1 = BUY 0.2 (merged)");
        Console.WriteLine($"   - Example: BUY 0.1 + SELL 0.05 = BUY 0.05 (reduced)");
        Console.WriteLine($"   - Used by: US brokers (FIFO rule)\n");

        Console.WriteLine($"⚠️  THIS FILE FOCUSES ON HEDGING MODE");
        Console.WriteLine($"   Most global brokers use hedging mode\n");

        if (!isHedging)
        {
            Console.WriteLine($"ℹ️  NOTE: Your account is in NETTING mode");
            Console.WriteLine($"   Some examples may behave differently\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 2: OPENING MULTIPLE POSITIONS ON SAME SYMBOL
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 2: Opening Multiple Positions (Hedging Mode)");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"💡 IN HEDGING MODE you can:");
        Console.WriteLine($"   - Open multiple BUY positions on EURUSD");
        Console.WriteLine($"   - Open multiple SELL positions on EURUSD");
        Console.WriteLine($"   - Have BUY and SELL on EURUSD at the same time\n");

        Console.WriteLine($"📊 EXAMPLE SCENARIO:");
        Console.WriteLine($"   Strategy: Scalping with 3 positions at different levels\n");

        Console.WriteLine($"   Position 1: BUY 0.01 lots @ 1.10000, SL 20pts, TP 30pts");
        Console.WriteLine($"   Position 2: BUY 0.01 lots @ 1.09980, SL 20pts, TP 40pts");
        Console.WriteLine($"   Position 3: BUY 0.01 lots @ 1.09960, SL 20pts, TP 50pts\n");

        Console.WriteLine($"📞 CODE TO OPEN MULTIPLE POSITIONS:");
        Console.WriteLine(@"
   // Open first position
   var result1 = await service.BuyMarket(symbol, 0.01, 20, 30);
   Console.WriteLine($""Position 1: #{result1.Order}"");

   // Open second position
   var result2 = await service.BuyMarket(symbol, 0.01, 20, 40);
   Console.WriteLine($""Position 2: #{result2.Order}"");

   // Open third position
   var result3 = await service.BuyMarket(symbol, 0.01, 20, 50);
   Console.WriteLine($""Position 3: #{result3.Order}"");

   // Now you have 3 independent BUY positions!
");

        Console.WriteLine($"💡 EACH POSITION:");
        Console.WriteLine($"   - Has unique ticket number");
        Console.WriteLine($"   - Can be closed independently");
        Console.WriteLine($"   - Has its own SL/TP");
        Console.WriteLine($"   - Contributes to total exposure\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 3: CHECKING CURRENT POSITIONS
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 3: Analyzing Your Current Positions");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Get all positions
        var positions = await service.OpenedOrdersAsync();

        Console.WriteLine($"📊 Total open positions: {positions.PositionInfos.Count}\n");

        if (positions.PositionInfos.Count > 0)
        {
            // Group by symbol
            var bySymbol = positions.GroupBy(p => p.Symbol);

            Console.WriteLine($"📋 Breakdown by Symbol:\n");

            foreach (var group in bySymbol)
            {
                var buyCount = group.Count(p => p.Type == 0);
                var sellCount = group.Count(p => p.Type == 1);
                var totalVolume = group.Sum(p => p.Volume);
                var totalProfit = group.Sum(p => p.Profit);

                Console.WriteLine($"   {group.Key}:");
                Console.WriteLine($"      Total positions: {group.Count()}");
                Console.WriteLine($"      BUY: {buyCount} position(s)");
                Console.WriteLine($"      SELL: {sellCount} position(s)");
                Console.WriteLine($"      Total volume: {totalVolume} lots");
                Console.WriteLine($"      Total P/L: ${totalProfit:F2}\n");
            }

            // Show first few positions in detail
            Console.WriteLine($"📋 Position Details (first {Math.Min(5, positions.PositionInfos.Count)}):\n");

            foreach (var pos in positions.Take(5))
            {
                Console.WriteLine($"   ─────────────────────────────────────────");
                Console.WriteLine($"   #{pos.Ticket} - {pos.Symbol} {(pos.Type == 0 ? "BUY" : "SELL")}");
                Console.WriteLine($"   Volume: {pos.Volume} lots");
                Console.WriteLine($"   Entry: {pos.PriceOpen:F5}");
                Console.WriteLine($"   Current: {pos.PriceCurrent:F5}");
                Console.WriteLine($"   Profit: ${pos.Profit:F2}");
            }
            Console.WriteLine($"   ─────────────────────────────────────────\n");
        }
        else
        {
            Console.WriteLine($"   No open positions currently\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 4: CLOSING ALL POSITIONS AT ONCE
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 4: Closing All Positions Simultaneously");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"💡 WHEN TO CLOSE ALL:");
        Console.WriteLine($"   - Emergency exit (bad news, major event)");
        Console.WriteLine($"   - End of trading day");
        Console.WriteLine($"   - Risk limit reached");
        Console.WriteLine($"   - Strategy completed\n");

        Console.WriteLine($"📚 THREE METHODS:\n");

        Console.WriteLine($"1. CLOSE ALL POSITIONS (all symbols):");
        Console.WriteLine($"   int closed = await service.CloseAll();");
        Console.WriteLine($"   Console.WriteLine($\"Closed {{closed}} positions\");\n");

        Console.WriteLine($"2. CLOSE ALL FOR SPECIFIC SYMBOL:");
        Console.WriteLine($"   int closed = await service.CloseAllPositions(\"{symbol}\");");
        Console.WriteLine($"   Console.WriteLine($\"Closed {{closed}} {symbol} positions\");\n");

        Console.WriteLine($"3. MANUAL LOOP (more control):");
        Console.WriteLine(@"
   var positions = await service.OpenedOrdersAsync();
   int count = 0;

   foreach (var pos in positions)
   {
       await service.PositionCloseAsync(pos.Ticket);
       count++;
       Console.WriteLine($""Closed #{pos.Ticket}"");
   }

   Console.WriteLine($""Total closed: {count}"");
");

        Console.WriteLine($"⚠️  IMPORTANT:");
        Console.WriteLine($"   - CloseAll() is IMMEDIATE - all positions close NOW");
        Console.WriteLine($"   - Cannot undo after execution");
        Console.WriteLine($"   - Returns number of successfully closed positions");
        Console.WriteLine($"   - Failed closes are logged but don't stop the process\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 5: SELECTIVE CLOSING - BUY or SELL ONLY
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 5: Closing Only BUY or Only SELL Positions");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"💡 SCENARIO:");
        Console.WriteLine($"   You have: 3 BUY + 2 SELL on EURUSD");
        Console.WriteLine($"   Market turns bearish → Close all BUY, keep SELL\n");

        Console.WriteLine($"📚 METHODS:\n");

        Console.WriteLine($"1. CLOSE ALL BUY POSITIONS:");
        Console.WriteLine($"   int closed = await service.CloseAllBuy();");
        Console.WriteLine($"   Console.WriteLine($\"Closed {{closed}} BUY positions\");\n");

        Console.WriteLine($"2. CLOSE ALL SELL POSITIONS:");
        Console.WriteLine($"   int closed = await service.CloseAllSell();");
        Console.WriteLine($"   Console.WriteLine($\"Closed {{closed}} SELL positions\");\n");

        Console.WriteLine($"3. CLOSE BUY FOR SPECIFIC SYMBOL:");
        Console.WriteLine(@"
   var positions = await service.OpenedOrdersAsync();
   var buyPositions = positions.Where(p =>
       p.Symbol == ""EURUSD"" && p.Type == 0  // Type 0 = BUY
   ).ToList();

   int count = 0;
   foreach (var pos in buyPositions)
   {
       await service.PositionCloseAsync(pos.Ticket);
       count++;
   }

   Console.WriteLine($""Closed {count} BUY EURUSD positions"");
");

        Console.WriteLine($"💡 USE CASES:");
        Console.WriteLine($"   - Close BUY when trend reverses down");
        Console.WriteLine($"   - Close SELL when trend reverses up");
        Console.WriteLine($"   - Keep hedging positions, close directional ones");
        Console.WriteLine($"   - Reduce exposure in one direction only\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 6: CALCULATING TOTAL EXPOSURE
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 6: Calculating Total Exposure and Net Position");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"💡 EXPOSURE EXPLAINED:");
        Console.WriteLine($"   In hedging mode, positions don't cancel each other");
        Console.WriteLine($"   Need to calculate NET exposure manually\n");

        if (positions.PositionInfos.Count > 0)
        {
            Console.WriteLine($"📊 ANALYZING {symbol} EXPOSURE:\n");

            // Filter positions for this symbol
            var symbolPositions = positions.Where(p => p.Symbol == symbol).ToList();

            if (symbolPositions.Count > 0)
            {
                // Calculate totals
                var buyPositions = symbolPositions.Where(p => p.Type == 0).ToList();
                var sellPositions = symbolPositions.Where(p => p.Type == 1).ToList();

                double buyVolume = buyPositions.Sum(p => p.Volume);
                double sellVolume = sellPositions.Sum(p => p.Volume);
                double netVolume = buyVolume - sellVolume;

                double buyProfit = buyPositions.Sum(p => p.Profit);
                double sellProfit = sellPositions.Sum(p => p.Profit);
                double totalProfit = buyProfit + sellProfit;

                Console.WriteLine($"   BUY Positions: {buyPositions.Count}");
                Console.WriteLine($"      Total volume: {buyVolume} lots");
                Console.WriteLine($"      Total P/L: ${buyProfit:F2}\n");

                Console.WriteLine($"   SELL Positions: {sellPositions.Count}");
                Console.WriteLine($"      Total volume: {sellVolume} lots");
                Console.WriteLine($"      Total P/L: ${sellProfit:F2}\n");

                Console.WriteLine($"   NET EXPOSURE:");
                Console.WriteLine($"      {Math.Abs(netVolume)} lots {(netVolume > 0 ? "LONG (BUY)" : netVolume < 0 ? "SHORT (SELL)" : "FLAT (hedged)")}");
                Console.WriteLine($"      Total P/L: ${totalProfit:F2}\n");

                Console.WriteLine($"💡 INTERPRETATION:");
                if (Math.Abs(netVolume) < 0.01)
                {
                    Console.WriteLine($"   ✅ HEDGED: BUY and SELL cancel out");
                    Console.WriteLine($"   You have NO directional exposure");
                    Console.WriteLine($"   Market movement doesn't affect you much\n");
                }
                else if (netVolume > 0)
                {
                    Console.WriteLine($"   📈 LONG: Net {netVolume} lots BUY exposure");
                    Console.WriteLine($"   You benefit if price RISES");
                    Console.WriteLine($"   You lose if price FALLS\n");
                }
                else
                {
                    Console.WriteLine($"   📉 SHORT: Net {Math.Abs(netVolume)} lots SELL exposure");
                    Console.WriteLine($"   You benefit if price FALLS");
                    Console.WriteLine($"   You lose if price RISES\n");
                }
            }
        }
        else
        {
            Console.WriteLine($"   No positions to analyze\n");
        }

        Console.WriteLine($"✅ CALCULATION FORMULA:");
        Console.WriteLine(@"
   var positions = await service.OpenedOrdersAsync();
   var symbolPos = positions.Where(p => p.Symbol == symbol);

   double buyVolume = symbolPos.Where(p => p.Type == 0).Sum(p => p.Volume);
   double sellVolume = symbolPos.Where(p => p.Type == 1).Sum(p => p.Volume);
   double netExposure = buyVolume - sellVolume;

   if (netExposure > 0)
       Console.WriteLine($""Long {netExposure} lots"");
   else if (netExposure < 0)
       Console.WriteLine($""Short {Math.Abs(netExposure)} lots"");
   else
       Console.WriteLine(""Fully hedged (no net exposure)"");
");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 7: PARTIAL CLOSING
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 7: Partial Position Close (Reducing Size)");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"💡 WHAT IS PARTIAL CLOSE?");
        Console.WriteLine($"   Closing PART of a position, keeping the rest open");
        Console.WriteLine($"   Example: Have 1.0 lot → Close 0.5 lot → Keep 0.5 lot\n");

        Console.WriteLine($"⚠️  IMPORTANT LIMITATION:");
        Console.WriteLine($"   MT5 does NOT support direct partial close!");
        Console.WriteLine($"   Workaround: Close current + Open new smaller position\n");

        Console.WriteLine($"📊 SCENARIO:");
        Console.WriteLine($"   Position: BUY 1.0 lot EURUSD @ 1.10000");
        Console.WriteLine($"   Want to: Take profit on 0.6 lot, keep 0.4 lot\n");

        Console.WriteLine($"✅ WORKAROUND METHOD:");
        Console.WriteLine(@"
   // Original position
   ulong ticket = 123456789;
   double originalVolume = 1.0;
   double closeVolume = 0.6;      // Close this much
   double remainVolume = 0.4;     // Keep this much

   var positions = await service.OpenedOrdersAsync();
   var pos = positions.FirstOrDefault(p => p.Ticket == ticket);

   if (pos != null)
   {
       // Step 1: Close original position completely
       await service.PositionCloseAsync(pos.Ticket);
       Console.WriteLine($""Closed original {originalVolume} lot position"");

       // Step 2: Immediately re-open with remaining volume
       var newPos = await service.BuyMarketAsync(
           pos.Symbol,
           remainVolume,  // 0.4 lots
           pos.Sl,        // Same SL
           pos.Tp         // Same TP
       );

       if (newPos.ReturnedCode == 10009)
       {
           Console.WriteLine($""Re-opened {remainVolume} lot position #{newPos.Order}"");
           Console.WriteLine($""Effectively closed {closeVolume} lots"");
       }
   }
");

        Console.WriteLine($"💡 WHY THIS WORKS:");
        Console.WriteLine($"   1. Close 1.0 lot → realizes profit on 0.6 + 0.4");
        Console.WriteLine($"   2. Re-open 0.4 lot → back in market with smaller size");
        Console.WriteLine($"   3. Net effect: 0.6 lot closed, 0.4 lot still running\n");

        Console.WriteLine($"⚠️  RISKS:");
        Console.WriteLine($"   - Small time gap between close and re-open (slippage)");
        Console.WriteLine($"   - If re-open fails, you're out completely");
        Console.WriteLine($"   - Better to plan position sizes in advance\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 8: POSITION MANAGEMENT STRATEGIES
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 8: Professional Multi-Position Strategies");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"💡 STRATEGY 1: PYRAMID (Adding to Winners)");
        Console.WriteLine($"   1. Open initial 0.1 lot BUY");
        Console.WriteLine($"   2. If +20 points profit → Add 0.1 lot BUY");
        Console.WriteLine($"   3. If +40 points profit → Add 0.1 lot BUY");
        Console.WriteLine($"   4. Total: 0.3 lots with average better than initial entry\n");

        Console.WriteLine($"💡 STRATEGY 2: SCALING IN (Averaging Down - RISKY!)");
        Console.WriteLine($"   1. Open 0.1 lot BUY @ 1.10000");
        Console.WriteLine($"   2. Price drops to 1.09950 → Add 0.1 lot BUY");
        Console.WriteLine($"   3. Price drops to 1.09900 → Add 0.1 lot BUY");
        Console.WriteLine($"   4. Average entry now 1.09950 instead of 1.10000");
        Console.WriteLine($"   ⚠️  WARNING: Can blow account in strong trend!\n");

        Console.WriteLine($"💡 STRATEGY 3: HEDGING (Locking Losses)");
        Console.WriteLine($"   1. Have BUY 0.1 lot in -50 point loss");
        Console.WriteLine($"   2. Instead of closing → Open SELL 0.1 lot");
        Console.WriteLine($"   3. Net exposure = 0 (locked at current loss level)");
        Console.WriteLine($"   4. Wait for better moment to exit both\n");

        Console.WriteLine($"💡 STRATEGY 4: GRID TRADING");
        Console.WriteLine($"   1. Place BUY orders every 20 points below: 1.09980, 1.09960, 1.09940");
        Console.WriteLine($"   2. Place SELL orders every 20 points above: 1.10020, 1.10040, 1.10060");
        Console.WriteLine($"   3. As price oscillates, positions open and close with TP");
        Console.WriteLine($"   4. Profit from range-bound market\n");

        // ═══════════════════════════════════════════════════════════════════════
        // SUMMARY
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("SUMMARY - Multiple Position Management");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"📋 KEY CONCEPTS:\n");

        Console.WriteLine($"HEDGING MODE:");
        Console.WriteLine($"   - Multiple positions per symbol allowed");
        Console.WriteLine($"   - BUY and SELL can coexist");
        Console.WriteLine($"   - Each position has unique ticket");
        Console.WriteLine($"   - Used by most non-US brokers\n");

        Console.WriteLine($"NETTING MODE:");
        Console.WriteLine($"   - One position per symbol only");
        Console.WriteLine($"   - Opposite orders merge/reduce position");
        Console.WriteLine($"   - Used by US brokers (FIFO)\n");

        Console.WriteLine($"📚 KEY METHODS:\n");

        Console.WriteLine($"CLOSE ALL:");
        Console.WriteLine($"   - CloseAll() - All positions, all symbols");
        Console.WriteLine($"   - CloseAllPositions(symbol) - All for symbol");
        Console.WriteLine($"   - CloseAllBuy() - All BUY positions");
        Console.WriteLine($"   - CloseAllSell() - All SELL positions\n");

        Console.WriteLine($"ANALYZE:");
        Console.WriteLine($"   - PositionsAsync() - Get all positions");
        Console.WriteLine($"   - positions.Where(p => p.Symbol == \"EURUSD\") - Filter");
        Console.WriteLine($"   - positions.GroupBy(p => p.Symbol) - Group by symbol");
        Console.WriteLine($"   - Sum volumes and profits for exposure\n");

        Console.WriteLine($"💡 EXPOSURE CALCULATION:");
        Console.WriteLine($"   buyVol = BUY positions total volume");
        Console.WriteLine($"   sellVol = SELL positions total volume");
        Console.WriteLine($"   netExposure = buyVol - sellVol");
        Console.WriteLine($"   > 0 = LONG, < 0 = SHORT, = 0 = HEDGED\n");

        Console.WriteLine($"⚠️  BEST PRACTICES:");
        Console.WriteLine($"   1. Track net exposure across all positions");
        Console.WriteLine($"   2. Don't over-leverage with many positions");
        Console.WriteLine($"   3. Close losing positions early, let winners run");
        Console.WriteLine($"   4. Use CloseAll() for emergency exits");
        Console.WriteLine($"   5. Be careful with hedging (can lock losses)");
        Console.WriteLine($"   6. Plan position sizes before opening");
        Console.WriteLine($"   7. Monitor margin usage with multiple positions\n");

        Console.WriteLine($"❌ COMMON MISTAKES:");
        Console.WriteLine($"   - Opening too many positions → Margin call");
        Console.WriteLine($"   - Not tracking net exposure");
        Console.WriteLine($"   - Hedging losses instead of taking stop loss");
        Console.WriteLine($"   - Averaging down in strong trend (blows account)");
        Console.WriteLine($"   - Forgetting to close all positions at day end\n");

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Manage multiple positions wisely = Controlled trading!  ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
    }
}
