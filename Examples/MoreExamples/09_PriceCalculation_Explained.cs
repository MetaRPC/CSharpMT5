// ══════════════════════════════════════════════════════════════════════════════
// FILE: 09_PriceCalculation_Explained.cs
// PURPOSE: Price calculations - Understanding prices, points, and pending orders
//
// Topics covered:
//   1. WHAT is Point and why NOT Pip
//   2. DIFFERENCE between Bid and Ask - when to use which
//   3. HOW to calculate prices for pending orders
//   4. WHY price normalization is critical
//   5. MINIMUM distance for stops (SYMBOL_TRADE_STOPS_LEVEL)
//   6. PRACTICAL examples with real numbers
//
// Key principle: In MT5, everything is measured in POINTS, not pips!
// 1 pip = 10 points (for 5-digit brokers like EURUSD at 1.10000)
// 1 pip = 1 point (for 3-digit brokers like USDJPY at 110.00)
//
// ══════════════════════════════════════════════════════════════════════════════


using System;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;

namespace MetaRPC.CSharpMT5.Examples.MoreExamples;

public static class PriceCalculationExamples
{
    // Define async method
    public static async Task RunAsync()
    {
        // ═══════════════════════════════════════════════════════════════════════
        // STEP 1: DISPLAY HEADER
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   PRICE CALCULATIONS - Points, Pips, and Prices           ║");
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

        // Create MT5Service wrapper
        var service = new MT5Service(account);

        // Create MT5Sugar convenience API
        // MT5Sugar methods are extension methods on MT5Service

        // Define symbol for examples
        string symbol = "EURUSD";

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 1: UNDERSTANDING POINT vs PIP
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 1: What is Point and How It Differs from Pip");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Get Point value for the symbol
        // Point = smallest price movement possible for this symbol
        // Type: double
        double point = await service.GetPointAsync(symbol);

        // Get Digits - number of decimal places for this symbol
        // Type: int
        int digits = await service.GetDigitsAsync(symbol);

        Console.WriteLine($"📊 Symbol: {symbol}");
        Console.WriteLine($"   Point: {point}");
        Console.WriteLine($"   Digits: {digits}\n");

        Console.WriteLine($"💡 WHAT THIS MEANS:");
        Console.WriteLine($"   - Point = {point} (smallest price movement)");
        Console.WriteLine($"   - Prices are shown with {digits} decimal places");
        Console.WriteLine($"   - Example price: 1.{new string('0', digits - 1)}00\n");

        // Calculate Pip from Point
        // For EURUSD (5 digits): 1 pip = 10 points = 0.0001
        // For USDJPY (3 digits): 1 pip = 1 point = 0.01
        double pip = point * 10;  // Traditional pip definition

        Console.WriteLine($"📐 POINT vs PIP:");
        Console.WriteLine($"   1 Point = {point}");
        Console.WriteLine($"   1 Pip = {pip} (traditional definition)");
        Console.WriteLine($"   1 Pip = {pip / point} points\n");

        Console.WriteLine($"⚠️  IMPORTANT:");
        Console.WriteLine($"   - MT5 API uses POINTS everywhere, not pips!");
        Console.WriteLine($"   - Stop Loss distance: measured in POINTS");
        Console.WriteLine($"   - Take Profit distance: measured in POINTS");
        Console.WriteLine($"   - Price offsets: measured in POINTS");
        Console.WriteLine($"   - When you see '50 points' for EURUSD, that's 5 pips\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 2: BID vs ASK - When to Use Which
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 2: Bid vs Ask - The Fundamental Difference");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Get current tick data
        // Tick contains both Bid and Ask prices
        var tick = await service.SymbolInfoTickAsync(symbol);

        Console.WriteLine($"📊 Current Market Prices:");
        Console.WriteLine($"   Bid: {tick.Bid:F5} ← Price where you SELL");
        Console.WriteLine($"   Ask: {tick.Ask:F5} ← Price where you BUY\n");

        // Calculate spread in points
        double spreadPoints = await service.GetSpreadPointsAsync(symbol);

        Console.WriteLine($"📏 Spread:");
        Console.WriteLine($"   Spread: {spreadPoints} points");
        Console.WriteLine($"   Spread: {spreadPoints / 10:F1} pips (traditional)");
        Console.WriteLine($"   Spread: {tick.Ask - tick.Bid:F5} in price units\n");

        Console.WriteLine($"💡 WHICH PRICE TO USE:");
        Console.WriteLine($"   FOR BUY OPERATIONS:");
        Console.WriteLine($"   - Market BUY → Opens at ASK ({tick.Ask:F5})");
        Console.WriteLine($"   - Buy Limit → Place BELOW current ASK");
        Console.WriteLine($"   - Buy Stop → Place ABOVE current ASK");
        Console.WriteLine($"   - Close BUY position → Closes at BID\n");

        Console.WriteLine($"   FOR SELL OPERATIONS:");
        Console.WriteLine($"   - Market SELL → Opens at BID ({tick.Bid:F5})");
        Console.WriteLine($"   - Sell Limit → Place ABOVE current BID");
        Console.WriteLine($"   - Sell Stop → Place BELOW current BID");
        Console.WriteLine($"   - Close SELL position → Closes at ASK\n");

        Console.WriteLine($"⚠️  GOLDEN RULE:");
        Console.WriteLine($"   - Want to BUY? → Use ASK price");
        Console.WriteLine($"   - Want to SELL? → Use BID price");
        Console.WriteLine($"   - Getting it wrong = order rejection!\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 3: CALCULATING PRICES FOR PENDING ORDERS
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 3: How to Calculate Prices for Pending Orders");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"📊 Current Market State:");
        Console.WriteLine($"   Ask: {tick.Ask:F5}");
        Console.WriteLine($"   Bid: {tick.Bid:F5}");
        Console.WriteLine($"   Point: {point}\n");

        // SCENARIO: Place pending orders 20 points away from current price
        int offsetPoints = 20;

        Console.WriteLine($"🎯 TASK: Place orders {offsetPoints} points from current price\n");

        // BUY LIMIT - Place BELOW Ask (expecting price to drop then rise)
        Console.WriteLine($"1. BUY LIMIT (entry BELOW current Ask):");
        Console.WriteLine($"   Base price: Ask = {tick.Ask:F5}");
        Console.WriteLine($"   Offset: -{offsetPoints} points");
        Console.WriteLine($"   Calculation: {tick.Ask:F5} - ({offsetPoints} × {point})");

        double buyLimitPrice = tick.Ask - (offsetPoints * point);
        Console.WriteLine($"   Result: {buyLimitPrice:F5}");
        Console.WriteLine($"   → Will execute when Ask DROPS to {buyLimitPrice:F5}\n");

        // SELL LIMIT - Place ABOVE Bid (expecting price to rise then fall)
        Console.WriteLine($"2. SELL LIMIT (entry ABOVE current Bid):");
        Console.WriteLine($"   Base price: Bid = {tick.Bid:F5}");
        Console.WriteLine($"   Offset: +{offsetPoints} points");
        Console.WriteLine($"   Calculation: {tick.Bid:F5} + ({offsetPoints} × {point})");

        double sellLimitPrice = tick.Bid + (offsetPoints * point);
        Console.WriteLine($"   Result: {sellLimitPrice:F5}");
        Console.WriteLine($"   → Will execute when Bid RISES to {sellLimitPrice:F5}\n");

        // BUY STOP - Place ABOVE Ask (breakout up)
        Console.WriteLine($"3. BUY STOP (entry ABOVE current Ask):");
        Console.WriteLine($"   Base price: Ask = {tick.Ask:F5}");
        Console.WriteLine($"   Offset: +{offsetPoints} points");
        Console.WriteLine($"   Calculation: {tick.Ask:F5} + ({offsetPoints} × {point})");

        double buyStopPrice = tick.Ask + (offsetPoints * point);
        Console.WriteLine($"   Result: {buyStopPrice:F5}");
        Console.WriteLine($"   → Will execute when Ask RISES to {buyStopPrice:F5}\n");

        // SELL STOP - Place BELOW Bid (breakout down)
        Console.WriteLine($"4. SELL STOP (entry BELOW current Bid):");
        Console.WriteLine($"   Base price: Bid = {tick.Bid:F5}");
        Console.WriteLine($"   Offset: -{offsetPoints} points");
        Console.WriteLine($"   Calculation: {tick.Bid:F5} - ({offsetPoints} × {point})");

        double sellStopPrice = tick.Bid - (offsetPoints * point);
        Console.WriteLine($"   Result: {sellStopPrice:F5}");
        Console.WriteLine($"   → Will execute when Bid DROPS to {sellStopPrice:F5}\n");

        // Visual representation
        Console.WriteLine($"📈 VISUAL REPRESENTATION:");
        Console.WriteLine($"");
        Console.WriteLine($"          ↑ Price Rising");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {sellLimitPrice:F5} ├─── SELL LIMIT (above Bid)");
        Console.WriteLine($"  {buyStopPrice:F5} ├─── BUY STOP (above Ask)");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {tick.Ask:F5} ├─── Current ASK ← BUY here");
        Console.WriteLine($"  {tick.Bid:F5} ├─── Current BID ← SELL here");
        Console.WriteLine($"          │");
        Console.WriteLine($"  {buyLimitPrice:F5} ├─── BUY LIMIT (below Ask)");
        Console.WriteLine($"  {sellStopPrice:F5} ├─── SELL STOP (below Bid)");
        Console.WriteLine($"          │");
        Console.WriteLine($"          ↓ Price Falling\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 4: CALCULATING STOP LOSS AND TAKE PROFIT
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 4: Calculating Stop Loss and Take Profit Prices");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // SCENARIO: Opening BUY position with SL and TP
        int slPoints = 30;  // Stop Loss 30 points away
        int tpPoints = 60;  // Take Profit 60 points away (2:1 ratio)

        Console.WriteLine($"🎯 SCENARIO: Opening BUY position");
        Console.WriteLine($"   Entry: Ask = {tick.Ask:F5}");
        Console.WriteLine($"   Stop Loss: {slPoints} points");
        Console.WriteLine($"   Take Profit: {tpPoints} points\n");

        Console.WriteLine($"📐 FOR BUY POSITION:");
        Console.WriteLine($"   Entry price: {tick.Ask:F5} (Buy at Ask)");

        // For BUY: SL is BELOW entry (protects from price falling)
        double buySL = tick.Ask - (slPoints * point);
        Console.WriteLine($"   Stop Loss: {tick.Ask:F5} - ({slPoints} × {point}) = {buySL:F5}");
        Console.WriteLine($"   → SL BELOW entry (protects if price falls)");

        // For BUY: TP is ABOVE entry (profit when price rises)
        double buyTP = tick.Ask + (tpPoints * point);
        Console.WriteLine($"   Take Profit: {tick.Ask:F5} + ({tpPoints} × {point}) = {buyTP:F5}");
        Console.WriteLine($"   → TP ABOVE entry (profit when price rises)\n");

        Console.WriteLine($"📐 FOR SELL POSITION:");
        Console.WriteLine($"   Entry price: {tick.Bid:F5} (Sell at Bid)");

        // For SELL: SL is ABOVE entry (protects from price rising)
        double sellSL = tick.Bid + (slPoints * point);
        Console.WriteLine($"   Stop Loss: {tick.Bid:F5} + ({slPoints} × {point}) = {sellSL:F5}");
        Console.WriteLine($"   → SL ABOVE entry (protects if price rises)");

        // For SELL: TP is BELOW entry (profit when price falls)
        double sellTP = tick.Bid - (tpPoints * point);
        Console.WriteLine($"   Take Profit: {tick.Bid:F5} - ({tpPoints} × {point}) = {sellTP:F5}");
        Console.WriteLine($"   → TP BELOW entry (profit when price falls)\n");

        Console.WriteLine($"⚠️  CRITICAL RULE:");
        Console.WriteLine($"   BUY:  SL uses MINUS (-), TP uses PLUS (+)");
        Console.WriteLine($"   SELL: SL uses PLUS (+), TP uses MINUS (-)");
        Console.WriteLine($"   Getting direction wrong = order rejection!\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 5: PRICE NORMALIZATION - Why It Matters
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 5: Price Normalization - Avoiding Rejections");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"💡 THE PROBLEM:");
        Console.WriteLine($"   Brokers only accept prices with EXACT number of decimal places");
        Console.WriteLine($"   For {symbol} with {digits} digits: 1.10000 ✓, 1.100005 ✗\n");

        // Create an invalid price (too many decimals)
        double invalidPrice = 1.098765432;

        Console.WriteLine($"❌ INVALID PRICE:");
        Console.WriteLine($"   Price: {invalidPrice:F9}");
        Console.WriteLine($"   Symbol requires: {digits} decimals");
        Console.WriteLine($"   This has: 9 decimals");
        Console.WriteLine($"   Result: Broker will REJECT with error 10015\n");

        // Normalize the price
        Console.WriteLine($"🔧 NORMALIZING...");
        double normalizedPrice = await service.NormalizePriceAsync(symbol, invalidPrice);

        Console.WriteLine($"   Original: {invalidPrice:F9}");
        Console.WriteLine($"   Normalized: {normalizedPrice:F5}");
        Console.WriteLine($"   Digits: {digits}");
        Console.WriteLine($"   Status: Valid ✓\n");

        Console.WriteLine($"📐 HOW NORMALIZATION WORKS:");
        Console.WriteLine($"   1. Gets symbol's required digits ({digits})");
        Console.WriteLine($"   2. Rounds price to that precision");
        Console.WriteLine($"   3. Example: 1.098765 → 1.09877 (rounded to 5 decimals)\n");

        Console.WriteLine($"✅ WHEN TO NORMALIZE:");
        Console.WriteLine($"   - ALWAYS before sending orders");
        Console.WriteLine($"   - After any price calculations");
        Console.WriteLine($"   - When using calculated SL/TP");
        Console.WriteLine($"   - Use NormalizePriceAsync() or MT5Sugar methods (auto-normalize)\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 6: MINIMUM STOP DISTANCE (STOPS_LEVEL)
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 6: Minimum Stop Distance - Why 5 Points Doesn't Work");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Get SYMBOL_TRADE_STOPS_LEVEL
        // This is the MINIMUM distance allowed for SL/TP from current price
        // Type: int (in points)
        var symbolInfo = await service.SymbolInfoAsync(symbol);
        int stopsLevel = (int)symbolInfo.TradeStopsLevel;

        Console.WriteLine($"📊 Symbol: {symbol}");
        Console.WriteLine($"   Minimum stop distance: {stopsLevel} points");
        Console.WriteLine($"   Current Ask: {tick.Ask:F5}");
        Console.WriteLine($"   Current Bid: {tick.Bid:F5}\n");

        Console.WriteLine($"💡 WHAT THIS MEANS:");
        Console.WriteLine($"   You CANNOT place SL/TP closer than {stopsLevel} points");
        Console.WriteLine($"   From current market price\n");

        // Try placing SL too close
        int tooCloseSL = stopsLevel - 5;  // 5 points closer than allowed

        Console.WriteLine($"❌ INVALID: SL too close");
        Console.WriteLine($"   Trying SL at {tooCloseSL} points");
        Console.WriteLine($"   Minimum allowed: {stopsLevel} points");
        Console.WriteLine($"   Result: ERROR 10016 (Invalid stops)\n");

        // Correct SL distance
        int validSL = stopsLevel + 10;  // 10 points more than minimum

        Console.WriteLine($"✅ VALID: SL respects minimum");
        Console.WriteLine($"   Using SL at {validSL} points");
        Console.WriteLine($"   Minimum allowed: {stopsLevel} points");
        Console.WriteLine($"   Extra buffer: 10 points");
        Console.WriteLine($"   Result: Will be accepted ✓\n");

        double validSLPrice = tick.Ask - (validSL * point);
        Console.WriteLine($"📐 CALCULATION:");
        Console.WriteLine($"   Entry (Ask): {tick.Ask:F5}");
        Console.WriteLine($"   SL distance: {validSL} points");
        Console.WriteLine($"   SL price: {tick.Ask:F5} - ({validSL} × {point}) = {validSLPrice:F5}");
        Console.WriteLine($"   Distance: {(tick.Ask - validSLPrice) / point:F0} points ✓\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 7: USING SUGAR METHODS - The Easy Way
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 7: MT5Sugar Methods - Let API Do The Math");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"💡 GOOD NEWS: You don't need to calculate manually!");
        Console.WriteLine($"   MT5Sugar methods do all calculations for you\n");

        int desiredOffset = 25;
        int desiredSL = 40;
        int desiredTP = 80;

        Console.WriteLine($"✅ EASY WAY: Using Points-Based Methods");
        Console.WriteLine($"   BuyLimitPoints(symbol, volume, {desiredOffset}, {desiredSL}, {desiredTP})");
        Console.WriteLine($"   ↓");
        Console.WriteLine($"   Automatically:");
        Console.WriteLine($"   - Gets current Ask price");
        Console.WriteLine($"   - Calculates entry: Ask - {desiredOffset} points");
        Console.WriteLine($"   - Calculates SL: entry - {desiredSL} points");
        Console.WriteLine($"   - Calculates TP: entry + {desiredTP} points");
        Console.WriteLine($"   - Normalizes all prices");
        Console.WriteLine($"   - Sends order\n");

        Console.WriteLine($"📚 AVAILABLE METHODS:");
        Console.WriteLine($"   - BuyLimitPoints(symbol, volume, offsetPoints, slPoints, tpPoints)");
        Console.WriteLine($"   - SellLimitPoints(symbol, volume, offsetPoints, slPoints, tpPoints)");
        Console.WriteLine($"   - BuyStopPoints(symbol, volume, offsetPoints, slPoints, tpPoints)");
        Console.WriteLine($"   - SellStopPoints(symbol, volume, offsetPoints, slPoints, tpPoints)");
        Console.WriteLine($"   - BuyMarket(symbol, volume, slPoints, tpPoints)");
        Console.WriteLine($"   - SellMarket(symbol, volume, slPoints, tpPoints)\n");

        Console.WriteLine($"💡 BENEFIT:");
        Console.WriteLine($"   - No manual price calculations");
        Console.WriteLine($"   - No normalization needed");
        Console.WriteLine($"   - No Bid/Ask confusion");
        Console.WriteLine($"   - Just specify POINTS and let API handle the rest!\n");

        // ═══════════════════════════════════════════════════════════════════════
        // SUMMARY
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("SUMMARY - Price Calculation Essentials");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("📏 MEASURING DISTANCES:");
        Console.WriteLine($"   - Everything in MT5 uses POINTS, not pips");
        Console.WriteLine($"   - For {symbol}: 1 pip = 10 points");
        Console.WriteLine($"   - Point = {point} (smallest price movement)\n");

        Console.WriteLine("💰 BID vs ASK:");
        Console.WriteLine($"   - BUY operations → Use ASK price");
        Console.WriteLine($"   - SELL operations → Use BID price");
        Console.WriteLine($"   - Spread = Ask - Bid = broker's commission\n");

        Console.WriteLine("📐 PENDING ORDERS:");
        Console.WriteLine($"   - Buy Limit: BELOW Ask (Ask - points × point)");
        Console.WriteLine($"   - Sell Limit: ABOVE Bid (Bid + points × point)");
        Console.WriteLine($"   - Buy Stop: ABOVE Ask (Ask + points × point)");
        Console.WriteLine($"   - Sell Stop: BELOW Bid (Bid - points × point)\n");

        Console.WriteLine("🎯 STOP LOSS & TAKE PROFIT:");
        Console.WriteLine($"   BUY:  SL = entry - (points × point), TP = entry + (points × point)");
        Console.WriteLine($"   SELL: SL = entry + (points × point), TP = entry - (points × point)\n");

        Console.WriteLine("🔧 NORMALIZATION:");
        Console.WriteLine($"   - ALWAYS normalize prices before sending orders");
        Console.WriteLine($"   - Use NormalizePriceAsync() or MT5Sugar methods");
        Console.WriteLine($"   - Symbol digits = {digits}, prices must match!\n");

        Console.WriteLine("⚠️  MINIMUM DISTANCE:");
        Console.WriteLine($"   - SL/TP must be at least {stopsLevel} points from market");
        Console.WriteLine($"   - Check TradeStopsLevel before placing orders");
        Console.WriteLine($"   - Add buffer (10-20 points) for safety\n");

        Console.WriteLine("✅ RECOMMENDED:");
        Console.WriteLine($"   Use MT5Sugar points-based methods - they handle everything!");
        Console.WriteLine($"   BuyLimitPoints(), SellStopPoints(), etc.\n");

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Master price calculations = No more rejected orders!    ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
    }
}
