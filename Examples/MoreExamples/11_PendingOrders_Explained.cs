// ══════════════════════════════════════════════════════════════════════════════
// FILE: 11_PendingOrders_Explained.cs
// PURPOSE: Pending orders - Complete guide to all 4 types
//
// Topics covered:
//   1. WHAT are pending orders and when to use them
//   2. FOUR types: Buy Limit, Sell Limit, Buy Stop, Sell Stop
//   3. WHERE each type is placed relative to current price
//   4. HOW to calculate prices manually and with Points methods
//   5. EXPIRATION - time-limited orders
//   6. HOW to cancel pending orders
//
// Key principle: Pending orders execute automatically when price reaches
// specified level. No need to monitor the market constantly!
//
// ══════════════════════════════════════════════════════════════════════════════

using System;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;
using mt5_term_api;

namespace MetaRPC.CSharpMT5.Examples.MoreExamples;

// Declare public static class
public static class PendingOrdersExamples
{
    // Define async method
    public static async Task RunAsync()
    {
        // ═══════════════════════════════════════════════════════════════════════
        // STEP 1: DISPLAY HEADER
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   PENDING ORDERS - Complete Guide to All 4 Types          ║");
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
        // EXAMPLE 1: WHAT ARE PENDING ORDERS?
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 1: Understanding Pending Orders");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 WHAT IS A PENDING ORDER?");
        Console.WriteLine("   An order that executes AUTOMATICALLY when price reaches");
        Console.WriteLine("   a specified level. You don't need to watch the market!\n");

        Console.WriteLine("📊 TWO CATEGORIES:");
        Console.WriteLine("   LIMIT ORDERS - Enter on pullback/retracement");
        Console.WriteLine("   - Buy Limit: Buy BELOW current price (expecting dip then rise)");
        Console.WriteLine("   - Sell Limit: Sell ABOVE current price (expecting spike then fall)\n");

        Console.WriteLine("   STOP ORDERS - Enter on breakout");
        Console.WriteLine("   - Buy Stop: Buy ABOVE current price (breakout upward)");
        Console.WriteLine("   - Sell Stop: Sell BELOW current price (breakout downward)\n");

        Console.WriteLine("✅ BENEFITS:");
        Console.WriteLine("   - Don't need to watch screen 24/7");
        Console.WriteLine("   - Automated execution at desired price");
        Console.WriteLine("   - Can set multiple orders at different levels");
        Console.WriteLine("   - Can set expiration time");
        Console.WriteLine("   - Better entry prices than market orders\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 2: MARKET STATE - WHERE ARE WE NOW?
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 2: Current Market Situation");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Get current price
        var tick = await service.SymbolInfoTickAsync(symbol);
        double point = await service.GetPointAsync(symbol);

        Console.WriteLine($"📊 Current Market ({symbol}):");
        Console.WriteLine($"   Ask (Buy price): {tick.Ask:F5}");
        Console.WriteLine($"   Bid (Sell price): {tick.Bid:F5}");
        Console.WriteLine($"   Point size: {point}\n");

        Console.WriteLine($"📏 Reference Points (for examples):");
        Console.WriteLine($"   We'll place orders 20 points away from current price\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 3: BUY LIMIT - Buying Below Current Price
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 3: BUY LIMIT - Entry Below Current Ask");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"🎯 SCENARIO:");
        Console.WriteLine($"   Current Ask: {tick.Ask:F5}");
        Console.WriteLine($"   Strategy: 'I think price will dip to {tick.Ask - 20 * point:F5},");
        Console.WriteLine($"             then rise. I want to buy at that dip.'\n");

        Console.WriteLine($"💡 BUY LIMIT EXPLAINED:");
        Console.WriteLine($"   Placement: BELOW current Ask");
        Console.WriteLine($"   Execution: When Ask DROPS to your price");
        Console.WriteLine($"   Purpose: Get better entry than current market");
        Console.WriteLine($"   Use case: Expecting temporary pullback before uptrend\n");

        int buyLimitOffset = 20;  // 20 points below Ask
        int buyLimitSL = 30;      // Stop Loss 30 points
        int buyLimitTP = 60;      // Take Profit 60 points

        Console.WriteLine($"📐 CALCULATION:");
        Console.WriteLine($"   Base: Ask = {tick.Ask:F5}");
        Console.WriteLine($"   Offset: -{buyLimitOffset} points");
        Console.WriteLine($"   Entry price: {tick.Ask:F5} - ({buyLimitOffset} × {point}) = {tick.Ask - buyLimitOffset * point:F5}\n");

        Console.WriteLine($"📞 PLACING ORDER (demonstration - won't execute):");
        Console.WriteLine($"   Method: service.BuyLimitPoints()");
        Console.WriteLine($"   Symbol: {symbol}");
        Console.WriteLine($"   Volume: 0.01 lots");
        Console.WriteLine($"   Offset: {buyLimitOffset} points below Ask");
        Console.WriteLine($"   SL: {buyLimitSL} points");
        Console.WriteLine($"   TP: {buyLimitTP} points\n");

        Console.WriteLine($"📈 VISUALIZATION:");
        Console.WriteLine($"");
        Console.WriteLine($"          ↑ Price");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Ask + buyLimitTP * point:F5} ├─── Take Profit (+{buyLimitTP} from entry)");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Ask:F5} ├─── Current ASK (too high, wait for dip)");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Ask - buyLimitOffset * point:F5} ├─── BUY LIMIT (entry when price dips here)");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Ask - buyLimitOffset * point - buyLimitSL * point:F5} ├─── Stop Loss (-{buyLimitSL} from entry)");
        Console.WriteLine($"          │");
        Console.WriteLine($"          ↓\n");

        Console.WriteLine($"💭 WHAT HAPPENS:");
        Console.WriteLine($"   1. Order placed at {tick.Ask - buyLimitOffset * point:F5}");
        Console.WriteLine($"   2. Price drops → Touches {tick.Ask - buyLimitOffset * point:F5} → Order EXECUTES");
        Console.WriteLine($"   3. Now you have BUY position with SL and TP already set");
        Console.WriteLine($"   4. If price never drops to {tick.Ask - buyLimitOffset * point:F5} → Order stays pending\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 4: SELL LIMIT - Selling Above Current Price
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 4: SELL LIMIT - Entry Above Current Bid");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"🎯 SCENARIO:");
        Console.WriteLine($"   Current Bid: {tick.Bid:F5}");
        Console.WriteLine($"   Strategy: 'I think price will spike to {tick.Bid + 20 * point:F5},");
        Console.WriteLine($"             then fall. I want to sell at that spike.'\n");

        Console.WriteLine($"💡 SELL LIMIT EXPLAINED:");
        Console.WriteLine($"   Placement: ABOVE current Bid");
        Console.WriteLine($"   Execution: When Bid RISES to your price");
        Console.WriteLine($"   Purpose: Get better entry than current market");
        Console.WriteLine($"   Use case: Expecting temporary spike before downtrend\n");

        int sellLimitOffset = 20;
        int sellLimitSL = 30;
        int sellLimitTP = 60;

        Console.WriteLine($"📐 CALCULATION:");
        Console.WriteLine($"   Base: Bid = {tick.Bid:F5}");
        Console.WriteLine($"   Offset: +{sellLimitOffset} points");
        Console.WriteLine($"   Entry price: {tick.Bid:F5} + ({sellLimitOffset} × {point}) = {tick.Bid + sellLimitOffset * point:F5}\n");

        Console.WriteLine($"📞 PLACING ORDER (demonstration):");
        Console.WriteLine($"   Method: service.SellLimitPoints()");
        Console.WriteLine($"   Symbol: {symbol}");
        Console.WriteLine($"   Volume: 0.01 lots");
        Console.WriteLine($"   Offset: {sellLimitOffset} points above Bid");
        Console.WriteLine($"   SL: {sellLimitSL} points");
        Console.WriteLine($"   TP: {sellLimitTP} points\n");

        Console.WriteLine($"📈 VISUALIZATION:");
        Console.WriteLine($"");
        Console.WriteLine($"          ↑ Price");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Bid + sellLimitOffset * point + sellLimitSL * point:F5} ├─── Stop Loss (+{sellLimitSL} from entry)");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Bid + sellLimitOffset * point:F5} ├─── SELL LIMIT (entry when price spikes here)");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Bid:F5} ├─── Current BID (too low, wait for spike)");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Bid + sellLimitOffset * point - sellLimitTP * point:F5} ├─── Take Profit (-{sellLimitTP} from entry)");
        Console.WriteLine($"          │");
        Console.WriteLine($"          ↓\n");

        Console.WriteLine($"💭 WHAT HAPPENS:");
        Console.WriteLine($"   1. Order placed at {tick.Bid + sellLimitOffset * point:F5}");
        Console.WriteLine($"   2. Price rises → Touches {tick.Bid + sellLimitOffset * point:F5} → Order EXECUTES");
        Console.WriteLine($"   3. Now you have SELL position with SL and TP already set");
        Console.WriteLine($"   4. If price never rises to {tick.Bid + sellLimitOffset * point:F5} → Order stays pending\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 5: BUY STOP - Buying Above Current Price (Breakout)
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 5: BUY STOP - Breakout Entry Above Current Ask");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"🎯 SCENARIO:");
        Console.WriteLine($"   Current Ask: {tick.Ask:F5}");
        Console.WriteLine($"   Resistance at: {tick.Ask + 20 * point:F5}");
        Console.WriteLine($"   Strategy: 'If price breaks ABOVE {tick.Ask + 20 * point:F5},");
        Console.WriteLine($"             it will continue rising. Buy on breakout!'\n");

        Console.WriteLine($"💡 BUY STOP EXPLAINED:");
        Console.WriteLine($"   Placement: ABOVE current Ask");
        Console.WriteLine($"   Execution: When Ask RISES to your price");
        Console.WriteLine($"   Purpose: Catch upward breakout momentum");
        Console.WriteLine($"   Use case: Breakout trading, momentum trading\n");

        int buyStopOffset = 20;
        int buyStopSL = 30;
        int buyStopTP = 60;

        Console.WriteLine($"📐 CALCULATION:");
        Console.WriteLine($"   Base: Ask = {tick.Ask:F5}");
        Console.WriteLine($"   Offset: +{buyStopOffset} points");
        Console.WriteLine($"   Entry price: {tick.Ask:F5} + ({buyStopOffset} × {point}) = {tick.Ask + buyStopOffset * point:F5}\n");

        Console.WriteLine($"📞 PLACING ORDER (demonstration):");
        Console.WriteLine($"   Method: service.BuyStopPoints()");
        Console.WriteLine($"   Symbol: {symbol}");
        Console.WriteLine($"   Volume: 0.01 lots");
        Console.WriteLine($"   Offset: {buyStopOffset} points above Ask");
        Console.WriteLine($"   SL: {buyStopSL} points");
        Console.WriteLine($"   TP: {buyStopTP} points\n");

        Console.WriteLine($"📈 VISUALIZATION:");
        Console.WriteLine($"");
        Console.WriteLine($"          ↑ Price (Breakout direction)");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Ask + buyStopOffset * point + buyStopTP * point:F5} ├─── Take Profit (+{buyStopTP} from entry)");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Ask + buyStopOffset * point:F5} ├─── BUY STOP (entry on breakout ABOVE)");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Ask:F5} ├─── Current ASK (waiting for breakout)");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Ask + buyStopOffset * point - buyStopSL * point:F5} ├─── Stop Loss (-{buyStopSL} from entry)");
        Console.WriteLine($"          │");
        Console.WriteLine($"          ↓\n");

        Console.WriteLine($"💭 WHAT HAPPENS:");
        Console.WriteLine($"   1. Order placed at {tick.Ask + buyStopOffset * point:F5} (above current price)");
        Console.WriteLine($"   2. Price rises → Breaks ABOVE {tick.Ask + buyStopOffset * point:F5} → Order EXECUTES");
        Console.WriteLine($"   3. You're now in the upward move (riding momentum)");
        Console.WriteLine($"   4. If price doesn't break above → Order stays pending\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 6: SELL STOP - Selling Below Current Price (Breakdown)
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 6: SELL STOP - Breakout Entry Below Current Bid");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"🎯 SCENARIO:");
        Console.WriteLine($"   Current Bid: {tick.Bid:F5}");
        Console.WriteLine($"   Support at: {tick.Bid - 20 * point:F5}");
        Console.WriteLine($"   Strategy: 'If price breaks BELOW {tick.Bid - 20 * point:F5},");
        Console.WriteLine($"             it will continue falling. Sell on breakdown!'\n");

        Console.WriteLine($"💡 SELL STOP EXPLAINED:");
        Console.WriteLine($"   Placement: BELOW current Bid");
        Console.WriteLine($"   Execution: When Bid DROPS to your price");
        Console.WriteLine($"   Purpose: Catch downward breakout momentum");
        Console.WriteLine($"   Use case: Breakdown trading, momentum trading\n");

        int sellStopOffset = 20;
        int sellStopSL = 30;
        int sellStopTP = 60;

        Console.WriteLine($"📐 CALCULATION:");
        Console.WriteLine($"   Base: Bid = {tick.Bid:F5}");
        Console.WriteLine($"   Offset: -{sellStopOffset} points");
        Console.WriteLine($"   Entry price: {tick.Bid:F5} - ({sellStopOffset} × {point}) = {tick.Bid - sellStopOffset * point:F5}\n");

        Console.WriteLine($"📞 PLACING ORDER (demonstration):");
        Console.WriteLine($"   Method: service.SellStopPoints()");
        Console.WriteLine($"   Symbol: {symbol}");
        Console.WriteLine($"   Volume: 0.01 lots");
        Console.WriteLine($"   Offset: {sellStopOffset} points below Bid");
        Console.WriteLine($"   SL: {sellStopSL} points");
        Console.WriteLine($"   TP: {sellStopTP} points\n");

        Console.WriteLine($"📈 VISUALIZATION:");
        Console.WriteLine($"");
        Console.WriteLine($"          ↑ Price");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Bid - sellStopOffset * point + sellStopSL * point:F5} ├─── Stop Loss (+{sellStopSL} from entry)");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Bid:F5} ├─── Current BID (waiting for breakdown)");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Bid - sellStopOffset * point:F5} ├─── SELL STOP (entry on breakout BELOW)");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Bid - sellStopOffset * point - sellStopTP * point:F5} ├─── Take Profit (-{sellStopTP} from entry)");
        Console.WriteLine($"          │");
        Console.WriteLine($"          ↓ Price (Breakdown direction)\n");

        Console.WriteLine($"💭 WHAT HAPPENS:");
        Console.WriteLine($"   1. Order placed at {tick.Bid - sellStopOffset * point:F5} (below current price)");
        Console.WriteLine($"   2. Price falls → Breaks BELOW {tick.Bid - sellStopOffset * point:F5} → Order EXECUTES");
        Console.WriteLine($"   3. You're now in the downward move (riding momentum)");
        Console.WriteLine($"   4. If price doesn't break below → Order stays pending\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 7: COMPLETE VISUALIZATION - All 4 Types Together
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 7: All 4 Pending Order Types - Complete Picture");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"📊 CURRENT MARKET:");
        Console.WriteLine($"   Ask: {tick.Ask:F5}");
        Console.WriteLine($"   Bid: {tick.Bid:F5}\n");

        Console.WriteLine($"📈 ALL PENDING ORDERS AT 20 POINTS OFFSET:\n");
        Console.WriteLine($"");
        Console.WriteLine($"          ↑ Price Rising");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Bid + 20 * point:F5} ├─── SELL LIMIT (above Bid) - Sell on spike");
        Console.WriteLine($"  {tick.Ask + 20 * point:F5} ├─── BUY STOP (above Ask) - Buy on breakout UP");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Ask:F5} ├─── Current ASK ← Market BUY executes here");
        Console.WriteLine($"  {tick.Bid:F5} ├─── Current BID ← Market SELL executes here");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Ask - 20 * point:F5} ├─── BUY LIMIT (below Ask) - Buy on dip");
        Console.WriteLine($"  {tick.Bid - 20 * point:F5} ├─── SELL STOP (below Bid) - Sell on breakout DOWN");
        Console.WriteLine($"          │");
        Console.WriteLine($"          ↓ Price Falling\n");

        Console.WriteLine($"💡 QUICK REFERENCE:");
        Console.WriteLine($"   Buy Limit:  BELOW Ask → pullback/dip entry");
        Console.WriteLine($"   Sell Limit: ABOVE Bid → spike/rally entry");
        Console.WriteLine($"   Buy Stop:   ABOVE Ask → upward breakout");
        Console.WriteLine($"   Sell Stop:  BELOW Bid → downward breakout\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 8: CHECKING PENDING ORDERS
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 8: How to Check Your Pending Orders");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"📞 Getting list of pending orders:");
        Console.WriteLine($"   var orders = await service.OpenedOrdersAsync();\n");

        // Get current pending orders
        var ordersData = await service.OpenedOrdersAsync();

        Console.WriteLine($"📊 Current pending orders: {ordersData.OpenedOrders.Count}\n");

        if (ordersData.OpenedOrders.Count > 0)
        {
            Console.WriteLine($"📋 Your pending orders:");
            foreach (var order in ordersData.OpenedOrders)
            {
                Console.WriteLine($"   Ticket: {order.Ticket}");
                Console.WriteLine($"   Type: {order.Type}");
                Console.WriteLine($"   Symbol: {order.Symbol}");
                Console.WriteLine($"   Volume: {order.VolumeCurrent} lots");
                Console.WriteLine($"   Price: {order.PriceOpen:F5}");
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine($"   No pending orders currently");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 9: CANCELING PENDING ORDERS
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 9: How to Cancel Pending Orders");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"💡 THREE WAYS TO CANCEL:\n");

        Console.WriteLine($"1. CANCEL SPECIFIC ORDER BY TICKET:");
        Console.WriteLine($"   ulong ticket = 123456789;");
        Console.WriteLine($"   await service.OrderDeleteAsync(ticket);\n");

        Console.WriteLine($"2. CANCEL ALL PENDING ORDERS:");
        Console.WriteLine($"   int canceled = await service.CancelAll();");
        Console.WriteLine($"   Console.WriteLine($\"Canceled {{canceled}} orders\");\n");

        Console.WriteLine($"3. CANCEL ALL FOR SPECIFIC SYMBOL:");
        Console.WriteLine($"   int canceled = await service.CancelAll(\"{symbol}\");");
        Console.WriteLine($"   Console.WriteLine($\"Canceled {{canceled}} {symbol} orders\");\n");

        Console.WriteLine($"⚠️  IMPORTANT:");
        Console.WriteLine($"   - Can only cancel PENDING orders");
        Console.WriteLine($"   - Once executed → becomes position (use Close, not Cancel)");
        Console.WriteLine($"   - Use CloseByTicket() - works for both pending and positions\n");

        // ═══════════════════════════════════════════════════════════════════════
        // SUMMARY
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("SUMMARY - Pending Orders Quick Reference");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"📋 FOUR TYPES:\n");

        Console.WriteLine($"┌────────────┬──────────────┬──────────────┬────────────────────┐");
        Console.WriteLine($"│ Order Type │  Placement   │  Base Price  │  Use Case          │");
        Console.WriteLine($"├────────────┼──────────────┼──────────────┼────────────────────┤");
        Console.WriteLine($"│ Buy Limit  │  BELOW       │  Ask         │  Pullback entry    │");
        Console.WriteLine($"│ Sell Limit │  ABOVE       │  Bid         │  Spike entry       │");
        Console.WriteLine($"│ Buy Stop   │  ABOVE       │  Ask         │  Upward breakout   │");
        Console.WriteLine($"│ Sell Stop  │  BELOW       │  Bid         │  Downward breakout │");
        Console.WriteLine($"└────────────┴──────────────┴──────────────┴────────────────────┘\n");

        Console.WriteLine($"📚 KEY METHODS:\n");

        Console.WriteLine($"POINTS-BASED (Recommended):");
        Console.WriteLine($"   - BuyLimitPoints(symbol, volume, offsetPoints, slPoints, tpPoints)");
        Console.WriteLine($"   - SellLimitPoints(symbol, volume, offsetPoints, slPoints, tpPoints)");
        Console.WriteLine($"   - BuyStopPoints(symbol, volume, offsetPoints, slPoints, tpPoints)");
        Console.WriteLine($"   - SellStopPoints(symbol, volume, offsetPoints, slPoints, tpPoints)\n");

        Console.WriteLine($"PRICE-BASED (Manual):");
        Console.WriteLine($"   - BuyLimitAsync(symbol, volume, price, sl, tp)");
        Console.WriteLine($"   - SellLimitAsync(symbol, volume, price, sl, tp)");
        Console.WriteLine($"   - BuyStopAsync(symbol, volume, price, sl, tp)");
        Console.WriteLine($"   - SellStopAsync(symbol, volume, price, sl, tp)\n");

        Console.WriteLine($"MANAGEMENT:");
        Console.WriteLine($"   - OrdersAsync() - Get all pending orders");
        Console.WriteLine($"   - OrderDeleteAsync(ticket) - Cancel specific order");
        Console.WriteLine($"   - CancelAll() - Cancel all pending orders");
        Console.WriteLine($"   - CancelAll(symbol) - Cancel all for symbol\n");

        Console.WriteLine($"💡 BEST PRACTICES:");
        Console.WriteLine($"   1. Use Points-based methods (easier, auto-calculate prices)");
        Console.WriteLine($"   2. Always set SL and TP when placing order");
        Console.WriteLine($"   3. Check margin BEFORE placing (CheckMarginAvailabilityAsync)");
        Console.WriteLine($"   4. Monitor pending orders (some may never execute)");
        Console.WriteLine($"   5. Cancel unused orders to free up margin");
        Console.WriteLine($"   6. Consider expiration time for time-sensitive strategies\n");

        Console.WriteLine($"⚠️  COMMON MISTAKES:");
        Console.WriteLine($"   - Buy Limit ABOVE Ask → Will reject (should be below)");
        Console.WriteLine($"   - Sell Limit BELOW Bid → Will reject (should be above)");
        Console.WriteLine($"   - Buy Stop BELOW Ask → Will reject (should be above)");
        Console.WriteLine($"   - Sell Stop ABOVE Bid → Will reject (should be below)");
        Console.WriteLine($"   - Forgetting to cancel old orders → Wastes margin\n");

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Master pending orders = Automated trading entries!      ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
    }
}
