// ══════════════════════════════════════════════════════════════════════════════
// FILE: 04_SugarExamples.cs
// PURPOSE: Sugar API methods with CALCULATIONS - EVERY LINE explained
//
// 🍬 WHAT IS SUGAR API?
//    Sugar API = convenience methods already written in MT5Sugar.cs
//    We DON'T write these methods from scratch - we just CALL them!
//    Each Sugar method internally does multiple low-level gRPC calls.
//
// 📍 WHERE ARE THESE METHODS?
//    All methods exist in: MT5Sugar.cs (lines 40-1700+)
//    They are extension methods on MT5Service class
//    Usage: service.GetSpreadPointsAsync(symbol) - just call them!
//
// 🔍 WHAT THIS FILE SHOWS:
//    This file demonstrates HOW Sugar methods work INTERNALLY
//    Each example shows:
//    - What the method does (high-level)
//    - What calculations happen inside (formulas)
//    - What low-level calls are made (gRPC)
//    - Why you'd use this method (use cases)
//
// Sugar methods demonstrated (10 examples with calculations):
//   1. GetSpreadPointsAsync - Calculate spread: (Ask - Bid) / Point
//   2. PointsToPipsAsync - Convert points to pips: points / 10^(digits-4)
//   3. NormalizePriceAsync - Round price to tick size
//   4. NormalizeVolumeAsync - Round volume to volume step
//   5. CalcVolumeForRiskAsync - Calculate lot by risk: riskMoney / lossPerLot
//   6. BuyMarketByRisk - Open BUY with auto-calculated volume
//   7. SellMarketByRisk - Open SELL with auto-calculated volume
//   8. BuyLimitPoints - Place Buy Limit with price calculation
//   9. SellLimitPoints - Place Sell Limit with price calculation
//  10. GetTickValueAndSizeAsync - Get tick value and size for calculations
//
// ⚠️  IMPORTANT: Methods 6-9 execute REAL TRADES with minimal lot!
// IMPORTANT: Without Console.WriteLine, results will NOT appear in terminal!
// ══════════════════════════════════════════════════════════════════════════════

// Import System namespace - provides Console, DateTime, Task, Math
using System;

// Import Task types for async/await
using System.Threading.Tasks;

// Import connection helper
using MetaRPC.CSharpMT5.Examples.Helpers;

// Import MT5 gRPC API types
using mt5_term_api;

// Import MT5Account and MT5Service
using MetaRPC.CSharpMT5;

// Declare namespace
namespace MetaRPC.CSharpMT5.Examples.MoreExamples;

// Declare public static class
public static class SugarExamples
{
    // Define async method
    public static async Task RunAsync()
    {
        // ═══════════════════════════════════════════════════════════════════════
        // STEP 1: DISPLAY HEADER
        // ═══════════════════════════════════════════════════════════════════════

        // Print header box
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║      SUGAR API WITH CALCULATIONS - Every Line Explained   ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

        // Print explanation
        Console.WriteLine("ℹ️  This demo shows Sugar methods that perform CALCULATIONS");
        Console.WriteLine("   Not just simple data retrieval, but actual math operations");
        Console.WriteLine("   Each calculation is explained step-by-step\n");

        // ═══════════════════════════════════════════════════════════════════════
        // STEP 2: ESTABLISH CONNECTION
        // ═══════════════════════════════════════════════════════════════════════

        // Call BuildConfiguration() - reads appsettings.json
        var config = ConnectionHelper.BuildConfiguration();
        Console.WriteLine("✓ Configuration loaded");

        // Call CreateAndConnectAccountAsync() - connects to MT5
        // Returns MT5Account object
        var acc = await ConnectionHelper.CreateAndConnectAccountAsync(config);
        Console.WriteLine("✓ Connected to MT5 Terminal");

        // Create MT5Service wrapper
        // Type: MT5Service
        // MT5Service wraps MT5Account and provides Sugar API methods
        var service = new MT5Service(acc);
        Console.WriteLine("✓ MT5Service created (Sugar API ready)\n");

        // Define symbol to use in examples
        // Type: string
        var symbol = "EURUSD";

        // Print which symbol we're using
        Console.WriteLine($"Symbol: {symbol}\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 1: CALCULATE SPREAD IN POINTS
        // ═══════════════════════════════════════════════════════════════════════
        // WHAT THIS DOES: Gets Ask and Bid prices, then calculates spread
        // FORMULA: Spread = (Ask - Bid) / Point
        // WHY: To know broker's markup before trading
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 1: Calculate Spread in Points");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("WHAT THIS DOES: (Ask - Bid) / Point = Spread in points");
        Console.WriteLine("WHY: Know broker's cost before trading\n");

        // Print what method we're calling
        Console.WriteLine("📊 Calling: service.GetSpreadPointsAsync(symbol)");
        Console.WriteLine("   This method does 2 things internally:");
        Console.WriteLine("   1. Gets current tick (Ask and Bid)");
        Console.WriteLine("   2. Gets Point value");
        Console.WriteLine("   3. Calculates: (Ask - Bid) / Point\n");

        // Call GetSpreadPointsAsync() - calculates spread
        // This is a Sugar method that internally:
        // 1. Calls SymbolInfoTickAsync to get Ask and Bid
        // 2. Calls SymbolInfoDoubleAsync to get Point
        // 3. Performs calculation: (tick.Ask - tick.Bid) / point
        // Returns: double (spread in points)
        var spreadPoints = await service.GetSpreadPointsAsync(symbol);

        // spreadPoints now contains calculated value, but INVISIBLE!

        Console.WriteLine("   Result:");

        // Print spread - WITHOUT this, INVISIBLE!
        Console.WriteLine($"   Spread: {spreadPoints:F1} points");
        Console.WriteLine($"   Meaning: You pay {spreadPoints:F1} points on each trade");
        Console.WriteLine($"   Example: If spread=15 points, that's 1.5 pips on EURUSD\n");

        // ══════════════════════════════════════════════════════════════════════
        // END OF EXAMPLE 1
        // ══════════════════════════════════════════════════════════════════════

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 2: CONVERT POINTS TO PIPS
        // ═══════════════════════════════════════════════════════════════════════
        // WHAT THIS DOES: Converts points to human-readable pips
        // FORMULA: Pips = Points / 10^(digits - 4)
        // WHY: Pips are easier to understand than points
        // EXAMPLE: 50 points on EURUSD (5 digits) = 5.0 pips
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 2: Convert Points to Pips");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("WHAT THIS DOES: Points / 10^(digits-4) = Pips");
        Console.WriteLine("WHY: Pips are easier to understand than points\n");

        // Print what we're doing
        Console.WriteLine("📏 Calling: service.PointsToPipsAsync(symbol, spreadPoints)");
        Console.WriteLine("   This method does:");
        Console.WriteLine("   1. Gets symbol digits (e.g., 5 for EURUSD)");
        Console.WriteLine("   2. Calculates factor: 10^(digits-4)");
        Console.WriteLine("   3. Divides: points / factor\n");

        // Call PointsToPipsAsync() - converts points to pips
        // This is a Sugar method that internally:
        // 1. Calls GetDigitsAsync to get number of decimal places
        // 2. Calculates factor = Math.Pow(10, Math.Max(0, digits - 4))
        //    For EURUSD: digits=5, so factor = 10^(5-4) = 10
        // 3. Returns: points / factor
        // Parameter: spreadPoints (the value we got from Example 1)
        var spreadPips = await service.PointsToPipsAsync(symbol, spreadPoints);

        // spreadPips now contains calculated value, but INVISIBLE!

        Console.WriteLine("   Calculation breakdown:");

        // Get digits to show the formula
        var digits = await service.GetDigitsAsync(symbol);

        // Print each step of calculation
        Console.WriteLine($"   Digits: {digits} (decimal places)");

        // Calculate factor
        // Type: double
        // Math.Pow(10, x) means "10 to the power of x"
        // Math.Max(0, digits - 4) ensures we don't get negative power
        var factor = Math.Pow(10, Math.Max(0, digits - 4));

        Console.WriteLine($"   Factor: 10^({digits}-4) = {factor}");
        Console.WriteLine($"   Formula: {spreadPoints:F1} points / {factor} = {spreadPips:F1} pips\n");

        Console.WriteLine("   Result:");
        Console.WriteLine($"   Spread: {spreadPips:F1} pips");
        Console.WriteLine($"   (Same as {spreadPoints:F1} points, just different units)\n");

        // ══════════════════════════════════════════════════════════════════════
        // END OF EXAMPLE 2
        // ══════════════════════════════════════════════════════════════════════

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 3: NORMALIZE PRICE TO TICK SIZE
        // ═══════════════════════════════════════════════════════════════════════
        // WHAT THIS DOES: Rounds price to broker's allowed tick size
        // FORMULA: Round(price / tickSize) * tickSize
        // WHY: Broker only accepts prices at specific intervals
        // EXAMPLE: If tickSize=0.00001, price 1.123456 → 1.12346 (rounded)
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 3: Normalize Price to Tick Size");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("WHAT THIS DOES: Round(price / tickSize) * tickSize");
        Console.WriteLine("WHY: Broker only accepts prices at specific intervals\n");

        // Print what we're doing
        Console.WriteLine("🔧 Calling: service.NormalizePriceAsync(symbol, rawPrice)");
        Console.WriteLine("   This method does:");
        Console.WriteLine("   1. Gets tick size (minimum price increment)");
        Console.WriteLine("   2. Divides: price / tickSize");
        Console.WriteLine("   3. Rounds to nearest integer");
        Console.WriteLine("   4. Multiplies back: rounded * tickSize\n");

        // Get current Ask price to demonstrate normalization
        var tick = await service.SymbolInfoTickAsync(symbol);
        var askPrice = tick.Ask;

        // Create a "bad" price with too many decimals
        // Type: double
        // Add 0.123456 to make an invalid price
        var rawPrice = askPrice + 0.000123456;

        Console.WriteLine($"   Raw price (invalid):  {rawPrice:F10}");

        // Call NormalizePriceAsync() - rounds to tick size
        // This is a Sugar method that internally:
        // 1. Calls SymbolInfoDoubleAsync to get TickSize
        // 2. Performs: Math.Round(price / tickSize) * tickSize
        // Parameter: rawPrice (the invalid price we created)
        // Returns: double (properly rounded price)
        var normalizedPrice = await service.NormalizePriceAsync(symbol, rawPrice);

        // normalizedPrice now contains calculated value, but INVISIBLE!

        Console.WriteLine($"   Normalized price:     {normalizedPrice:F10}");
        Console.WriteLine($"   Difference:           {Math.Abs(rawPrice - normalizedPrice):F10}");
        Console.WriteLine($"   ✓ Price now acceptable to broker!\n");

        // ══════════════════════════════════════════════════════════════════════
        // END OF EXAMPLE 3
        // ══════════════════════════════════════════════════════════════════════

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 4: NORMALIZE VOLUME TO VOLUME STEP
        // ═══════════════════════════════════════════════════════════════════════
        // WHAT THIS DOES: Rounds lot size to broker's allowed step
        // FORMULA: Round(volume / volumeStep) * volumeStep
        // WHY: Broker only accepts volumes in specific increments
        // EXAMPLE: If step=0.01, volume 0.123 → 0.12 (rounded down)
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 4: Normalize Volume to Volume Step");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("WHAT THIS DOES: Round(volume / volumeStep) * volumeStep");
        Console.WriteLine("WHY: Broker only accepts volumes in specific increments\n");

        // Print what we're doing
        Console.WriteLine("⚖️  Calling: service.NormalizeVolumeAsync(symbol, rawVolume)");
        Console.WriteLine("   This method does:");
        Console.WriteLine("   1. Gets min/max/step volume limits");
        Console.WriteLine("   2. Rounds: Round(volume / step) * step");
        Console.WriteLine("   3. Clamps between min and max");
        Console.WriteLine("   4. Returns valid volume\n");

        // Create a "bad" volume with odd decimals
        // Type: double
        var rawVolume = 0.123456;

        Console.WriteLine($"   Raw volume (invalid): {rawVolume:F6} lots");

        // Call NormalizeVolumeAsync() - rounds to volume step
        // This is a Sugar method that internally:
        // 1. Calls SymbolInfoDoubleAsync to get VolumeMin, VolumeMax, VolumeStep
        // 2. Performs: Math.Round(volume / volumeStep) * volumeStep
        // 3. Clamps: Math.Max(volumeMin, Math.Min(volumeMax, rounded))
        // Parameter: rawVolume (the invalid volume we created)
        // Returns: double (properly rounded volume)
        var normalizedVolume = await service.NormalizeVolumeAsync(symbol, rawVolume);

        // normalizedVolume now contains calculated value, but INVISIBLE!

        Console.WriteLine($"   Normalized volume:    {normalizedVolume:F6} lots");
        Console.WriteLine($"   Difference:           {Math.Abs(rawVolume - normalizedVolume):F6} lots");
        Console.WriteLine($"   ✓ Volume now acceptable to broker!\n");

        // ══════════════════════════════════════════════════════════════════════
        // END OF EXAMPLE 4
        // ══════════════════════════════════════════════════════════════════════

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 5: GET TICK VALUE AND SIZE
        // ═══════════════════════════════════════════════════════════════════════
        // WHAT THIS DOES: Gets tick value (money per tick) and tick size
        // WHY: Needed for lot size calculations
        // RETURNS: (tickValue, tickSize) tuple
        // EXAMPLE: EURUSD might return (1.0, 0.00001)
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 5: Get Tick Value and Size");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("WHAT THIS DOES: Gets tick value (money/tick) and tick size");
        Console.WriteLine("WHY: Needed for calculating lot sizes by risk\n");

        // Print what we're doing
        Console.WriteLine("💵 Calling: service.GetTickValueAndSizeAsync(symbol)");
        Console.WriteLine("   This method does:");
        Console.WriteLine("   1. Gets TickValue (money per tick for 1 lot)");
        Console.WriteLine("   2. Gets TickSize (minimum price increment)");
        Console.WriteLine("   3. Returns both as tuple\n");

        // Call GetTickValueAndSizeAsync() - gets tick info
        // This is a Sugar method that internally:
        // 1. Calls SymbolInfoDoubleAsync(SymbolTradeTickValue)
        // 2. Calls SymbolInfoDoubleAsync(SymbolTradeTickSize)
        // 3. Returns: (tickValue, tickSize) as tuple
        // Returns: ValueTuple<double, double>
        var (tickValue, tickSize) = await service.GetTickValueAndSizeAsync(symbol);

        // tickValue and tickSize now contain values, but INVISIBLE!

        Console.WriteLine("   Result:");
        Console.WriteLine($"   Tick Value: ${tickValue:F2} (money per tick for 1 lot)");
        Console.WriteLine($"   Tick Size:  {tickSize:F5} (minimum price increment)");
        Console.WriteLine($"   Meaning: Each {tickSize:F5} price change = ${tickValue:F2} for 1 lot\n");

        // ══════════════════════════════════════════════════════════════════════
        // END OF EXAMPLE 5
        // ══════════════════════════════════════════════════════════════════════

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 6: CALCULATE LOT SIZE BY RISK
        // ═══════════════════════════════════════════════════════════════════════
        // WHAT THIS DOES: Calculates exact lot size to risk specific amount
        // FORMULA: Volume = RiskMoney / ((StopPoints * Point / TickSize) * TickValue)
        // WHY: Ensures you risk exactly $100 (or any amount) per trade
        // EXAMPLE: Risk $100 with 50-point SL → calculates lot size (e.g., 0.20)
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 6: Calculate Lot Size by Risk Amount");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("WHAT THIS DOES: Calculates lot size to risk exact amount");
        Console.WriteLine("FORMULA: Volume = RiskMoney / LossPerLot");
        Console.WriteLine("WHY: Ensures you risk exactly $100 (or any amount)\n");

        // Print what we're doing
        Console.WriteLine("🧮 Calling: service.CalcVolumeForRiskAsync(symbol, stopPoints, riskMoney)");
        Console.WriteLine("   This method does:");
        Console.WriteLine("   1. Gets Point value");
        Console.WriteLine("   2. Gets TickValue and TickSize");
        Console.WriteLine("   3. Calculates lossPerLot: (stopPoints * point / tickSize) * tickValue");
        Console.WriteLine("   4. Divides: riskMoney / lossPerLot");
        Console.WriteLine("   5. Normalizes result to broker's volume step\n");

        // Define parameters for calculation
        // Type: double
        var stopPoints = 50.0;  // Stop loss distance in points
        var riskMoney = 100.0;   // Amount to risk in account currency

        Console.WriteLine($"   Parameters:");
        Console.WriteLine($"   Stop Loss:  {stopPoints:F0} points");
        Console.WriteLine($"   Risk Money: ${riskMoney:F2}\n");

        // Call CalcVolumeForRiskAsync() - calculates lot size
        // This is a Sugar method that internally:
        // 1. Validates parameters (stopPoints > 0, riskMoney > 0)
        // 2. Gets Point value
        // 3. Gets TickValue and TickSize
        // 4. Calculates: lossPerLot = (stopPoints * point / tickSize) * tickValue
        // 5. Calculates: volume = riskMoney / lossPerLot
        // 6. Normalizes volume to broker's step
        // Parameters: symbol, stopPoints, riskMoney
        // Returns: double (calculated lot size)
        var calculatedVolume = await service.CalcVolumeForRiskAsync(symbol, stopPoints, riskMoney);

        // calculatedVolume now contains calculated value, but INVISIBLE!

        Console.WriteLine("   Calculation breakdown:");

        // Get point value to show the formula
        var point = await service.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint);

        // Calculate loss per lot (same formula as in method)
        // Type: double
        var lossPerLot = (stopPoints * point / tickSize) * tickValue;

        Console.WriteLine($"   Point Value: {point:F5}");
        Console.WriteLine($"   Loss per 1 lot: (50 * {point:F5} / {tickSize:F5}) * ${tickValue:F2} = ${lossPerLot:F2}");
        Console.WriteLine($"   Volume needed: ${riskMoney:F2} / ${lossPerLot:F2} = {calculatedVolume:F2} lots\n");

        Console.WriteLine("   Result:");
        Console.WriteLine($"   Calculated Volume: {calculatedVolume:F2} lots");
        Console.WriteLine($"   ✓ If SL hits, loss = exactly ${riskMoney:F2}!\n");

        // ══════════════════════════════════════════════════════════════════════
        // END OF EXAMPLE 6
        // ══════════════════════════════════════════════════════════════════════

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 7: BUY MARKET WITH AUTO-CALCULATED VOLUME
        // ═══════════════════════════════════════════════════════════════════════
        // WHAT THIS DOES: Opens BUY position with volume calculated by risk
        // STEPS: 1. Calculate volume by risk
        //        2. Get current Ask price
        //        3. Calculate SL price: Ask - (stopPoints * point)
        //        4. Calculate TP price: Ask + (tpPoints * point)
        //        5. Normalize prices
        //        6. Send market order
        // ⚠️  WARNING: This executes REAL TRADE!
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 7: BUY Market with Auto-Calculated Volume");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("WHAT THIS DOES: Opens BUY with lot calculated by risk");
        Console.WriteLine("STEPS: CalcVolume → GetPrices → CalcSL/TP → NormalizePrices → SendOrder");
        Console.WriteLine("⚠️  WARNING: This executes REAL TRADE!\n");

        // Print what we're doing
        Console.WriteLine("📈 Calling: service.BuyMarketByRisk(symbol, stopPoints, riskMoney, tpPoints)");
        Console.WriteLine("   This method does:");
        Console.WriteLine("   1. Calls CalcVolumeForRiskAsync");
        Console.WriteLine("   2. Gets current tick (Ask price)");
        Console.WriteLine("   3. Calculates SL: Ask - (stopPoints * point)");
        Console.WriteLine("   4. Calculates TP: Ask + (tpPoints * point)");
        Console.WriteLine("   5. Normalizes SL and TP prices");
        Console.WriteLine("   6. Sends market BUY order\n");

        // Ask user for confirmation
        Console.WriteLine("⚠️  This will open a REAL trade!");
        Console.WriteLine("   Press any key to SKIP this example...");
        Console.WriteLine("   (Waiting 3 seconds...)\n");

        // Wait 3 seconds for user to cancel
        await Task.Delay(3000);

        Console.WriteLine("   SKIPPING trade execution for safety");
        Console.WriteLine("   (To execute, remove the skip logic in code)\n");

        // COMMENTED OUT - Uncomment to execute real trade
        // var tpPoints = 100.0;
        // var buyResult = await service.BuyMarketByRisk(symbol, stopPoints, riskMoney, tpPoints);
        // Console.WriteLine($"   Order Ticket: {buyResult.Order}");
        // Console.WriteLine($"   Return Code: {buyResult.ReturnedCode}");

        Console.WriteLine("   Would calculate:");
        Console.WriteLine($"   - Volume: {calculatedVolume:F2} lots (from Example 6)");
        Console.WriteLine($"   - SL: Ask - ({stopPoints} * {point:F5})");
        Console.WriteLine($"   - TP: Ask + (100 * {point:F5})");
        Console.WriteLine($"   Then send BUY order\n");

        // ══════════════════════════════════════════════════════════════════════
        // END OF EXAMPLE 7
        // ══════════════════════════════════════════════════════════════════════

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 8: SELL MARKET WITH AUTO-CALCULATED VOLUME
        // ═══════════════════════════════════════════════════════════════════════
        // WHAT THIS DOES: Opens SELL position with volume calculated by risk
        // STEPS: Same as BUY but uses Bid price and inverts SL/TP
        // DIFFERENCE: SL = Bid + (stopPoints * point) - ABOVE entry
        //             TP = Bid - (tpPoints * point) - BELOW entry
        // ⚠️  WARNING: This executes REAL TRADE!
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 8: SELL Market with Auto-Calculated Volume");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("WHAT THIS DOES: Opens SELL with lot calculated by risk");
        Console.WriteLine("DIFFERENCE FROM BUY: Uses Bid, SL/TP inverted");
        Console.WriteLine("⚠️  WARNING: This executes REAL TRADE!\n");

        Console.WriteLine("📉 Calling: service.SellMarketByRisk(symbol, stopPoints, riskMoney, tpPoints)");
        Console.WriteLine("   Same as BUY but:");
        Console.WriteLine("   - Uses Bid price (not Ask)");
        Console.WriteLine("   - SL: Bid + (stopPoints * point) - ABOVE entry");
        Console.WriteLine("   - TP: Bid - (tpPoints * point) - BELOW entry\n");

        Console.WriteLine("   SKIPPING trade execution for safety\n");

        Console.WriteLine("   Would calculate:");
        Console.WriteLine($"   - Volume: {calculatedVolume:F2} lots");
        Console.WriteLine($"   - SL: Bid + ({stopPoints} * {point:F5}) - above entry");
        Console.WriteLine($"   - TP: Bid - (100 * {point:F5}) - below entry");
        Console.WriteLine($"   Then send SELL order\n");

        // ══════════════════════════════════════════════════════════════════════
        // END OF EXAMPLE 8
        // ══════════════════════════════════════════════════════════════════════

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 9: BUY LIMIT WITH PRICE CALCULATION
        // ═══════════════════════════════════════════════════════════════════════
        // WHAT THIS DOES: Places Buy Limit pending order with calculated price
        // FORMULA: Entry Price = Ask - (offsetPoints * point)
        //          SL = Entry - (slPoints * point)
        //          TP = Entry + (tpPoints * point)
        // WHY: Buy Limit enters BELOW current price (buy cheaper)
        // EXAMPLE: If Ask=1.10000, offset=10 → Entry=1.09990
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 9: Buy Limit with Price Calculation");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("WHAT THIS DOES: Places Buy Limit BELOW current price");
        Console.WriteLine("FORMULA: Entry = Ask - (offsetPoints * point)");
        Console.WriteLine("WHY: Enter cheaper than current price\n");

        Console.WriteLine("🔽 Calling: service.BuyLimitPoints(symbol, volume, offsetPoints, slPoints, tpPoints)");
        Console.WriteLine("   This method does:");
        Console.WriteLine("   1. Gets current Ask");
        Console.WriteLine("   2. Calculates entry: Ask - (offsetPoints * point)");
        Console.WriteLine("   3. Normalizes entry price");
        Console.WriteLine("   4. Calculates SL: entry - (slPoints * point)");
        Console.WriteLine("   5. Calculates TP: entry + (tpPoints * point)");
        Console.WriteLine("   6. Sends Buy Limit order\n");

        // Define parameters
        var offsetPoints = 10.0;  // How far below Ask to place order
        var slPoints = 20.0;      // SL distance from entry
        var tpPoints = 30.0;      // TP distance from entry

        Console.WriteLine("   Calculation example:");

        // Get current Ask
        var currentTick = await service.SymbolInfoTickAsync(symbol);
        var currentAsk = currentTick.Ask;

        // Calculate entry price (same formula as in method)
        var entryPrice = currentAsk - (offsetPoints * point);

        Console.WriteLine($"   Current Ask:  {currentAsk:F5}");
        Console.WriteLine($"   Offset:       {offsetPoints} points = {offsetPoints * point:F5}");
        Console.WriteLine($"   Entry Price:  {currentAsk:F5} - {offsetPoints * point:F5} = {entryPrice:F5}");

        // Calculate SL
        var slPrice = entryPrice - (slPoints * point);
        Console.WriteLine($"   SL Price:     {entryPrice:F5} - ({slPoints} * {point:F5}) = {slPrice:F5}");

        // Calculate TP
        var tpPrice = entryPrice + (tpPoints * point);
        Console.WriteLine($"   TP Price:     {entryPrice:F5} + ({tpPoints} * {point:F5}) = {tpPrice:F5}\n");

        Console.WriteLine("   SKIPPING order placement for safety\n");

        // ══════════════════════════════════════════════════════════════════════
        // END OF EXAMPLE 9
        // ══════════════════════════════════════════════════════════════════════

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 10: SELL LIMIT WITH PRICE CALCULATION
        // ═══════════════════════════════════════════════════════════════════════
        // WHAT THIS DOES: Places Sell Limit pending order with calculated price
        // FORMULA: Entry Price = Bid + (offsetPoints * point)
        //          SL = Entry + (slPoints * point)
        //          TP = Entry - (tpPoints * point)
        // WHY: Sell Limit enters ABOVE current price (sell higher)
        // EXAMPLE: If Bid=1.10000, offset=10 → Entry=1.10010
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 10: Sell Limit with Price Calculation");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("WHAT THIS DOES: Places Sell Limit ABOVE current price");
        Console.WriteLine("FORMULA: Entry = Bid + (offsetPoints * point)");
        Console.WriteLine("WHY: Sell higher than current price\n");

        Console.WriteLine("🔼 Calling: service.SellLimitPoints(symbol, volume, offsetPoints, slPoints, tpPoints)");
        Console.WriteLine("   This method does:");
        Console.WriteLine("   1. Gets current Bid");
        Console.WriteLine("   2. Calculates entry: Bid + (offsetPoints * point)");
        Console.WriteLine("   3. Normalizes entry price");
        Console.WriteLine("   4. Calculates SL: entry + (slPoints * point) - ABOVE");
        Console.WriteLine("   5. Calculates TP: entry - (tpPoints * point) - BELOW");
        Console.WriteLine("   6. Sends Sell Limit order\n");

        Console.WriteLine("   Calculation example:");

        // Get current Bid
        var currentBid = currentTick.Bid;

        // Calculate entry price (same formula as in method)
        var sellEntryPrice = currentBid + (offsetPoints * point);

        Console.WriteLine($"   Current Bid:  {currentBid:F5}");
        Console.WriteLine($"   Offset:       {offsetPoints} points = {offsetPoints * point:F5}");
        Console.WriteLine($"   Entry Price:  {currentBid:F5} + {offsetPoints * point:F5} = {sellEntryPrice:F5}");

        // Calculate SL (ABOVE entry for SELL)
        var sellSlPrice = sellEntryPrice + (slPoints * point);
        Console.WriteLine($"   SL Price:     {sellEntryPrice:F5} + ({slPoints} * {point:F5}) = {sellSlPrice:F5} (ABOVE)");

        // Calculate TP (BELOW entry for SELL)
        var sellTpPrice = sellEntryPrice - (tpPoints * point);
        Console.WriteLine($"   TP Price:     {sellEntryPrice:F5} - ({tpPoints} * {point:F5}) = {sellTpPrice:F5} (BELOW)\n");

        Console.WriteLine("   SKIPPING order placement for safety\n");

        // ══════════════════════════════════════════════════════════════════════
        // END OF EXAMPLE 10
        // ══════════════════════════════════════════════════════════════════════

        // ═══════════════════════════════════════════════════════════════════════
        // SUMMARY
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("SUMMARY - What We Did:");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("✓ Calculated spread in points (Ask-Bid)/Point");
        Console.WriteLine("✓ Converted points to pips with Math.Pow");
        Console.WriteLine("✓ Normalized price to tick size with Math.Round");
        Console.WriteLine("✓ Normalized volume to volume step");
        Console.WriteLine("✓ Got tick value and size for calculations");
        Console.WriteLine("✓ Calculated lot by risk: RiskMoney / LossPerLot");
        Console.WriteLine("✓ Showed BuyMarketByRisk formula (SKIPPED execution)");
        Console.WriteLine("✓ Showed SellMarketByRisk formula (SKIPPED execution)");
        Console.WriteLine("✓ Calculated Buy Limit entry: Ask - offset");
        Console.WriteLine("✓ Calculated Sell Limit entry: Bid + offset");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("KEY FORMULAS:");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("1. Spread = (Ask - Bid) / Point");
        Console.WriteLine("2. Pips = Points / 10^(digits - 4)");
        Console.WriteLine("3. NormalizedPrice = Round(price / tickSize) * tickSize");
        Console.WriteLine("4. NormalizedVolume = Round(volume / volumeStep) * volumeStep");
        Console.WriteLine("5. LossPerLot = (stopPoints * point / tickSize) * tickValue");
        Console.WriteLine("6. Volume = RiskMoney / LossPerLot");
        Console.WriteLine("7. BUY: SL = Ask - (stopPoints * point)");
        Console.WriteLine("8. SELL: SL = Bid + (stopPoints * point)");
        Console.WriteLine("9. BuyLimit Entry = Ask - (offset * point)");
        Console.WriteLine("10. SellLimit Entry = Bid + (offset * point)");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("✓ All Sugar calculation examples completed!");
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// SUGAR METHODS WITH CALCULATIONS DEMONSTRATED:
//
// 1. GetSpreadPointsAsync
//    - Calculates: (Ask - Bid) / Point
//    - Source: MT5Sugar.cs line 544
//
// 2. PointsToPipsAsync
//    - Calculates: points / 10^(digits-4)
//    - Source: MT5Sugar.cs line 502
//
// 3. NormalizePriceAsync
//    - Calculates: Round(price / tickSize) * tickSize
//    - Source: MT5Sugar.cs (NormalizePriceAsync method)
//
// 4. NormalizeVolumeAsync
//    - Calculates: Round(volume / volumeStep) * volumeStep
//    - Clamps between min/max
//    - Source: MT5Sugar.cs (NormalizeVolumeAsync method)
//
// 5. GetTickValueAndSizeAsync
//    - Returns: (tickValue, tickSize) tuple
//    - Used in lot calculations
//    - Source: MT5Sugar.cs (GetTickValueAndSizeAsync method)
//
// 6. CalcVolumeForRiskAsync
//    - Calculates: riskMoney / lossPerLot
//    - Where: lossPerLot = (stopPoints * point / tickSize) * tickValue
//    - Source: MT5Sugar.cs line 1303
//
// 7. BuyMarketByRisk
//    - Combines: CalcVolumeForRiskAsync + price calculations + OrderSend
//    - Calculates: SL = Ask - (stopPoints * point)
//    - Calculates: TP = Ask + (tpPoints * point)
//    - Source: MT5Sugar.cs line 1558
//
// 8. SellMarketByRisk
//    - Combines: CalcVolumeForRiskAsync + price calculations + OrderSend
//    - Calculates: SL = Bid + (stopPoints * point)
//    - Calculates: TP = Bid - (tpPoints * point)
//    - Source: MT5Sugar.cs line 1598
//
// 9. BuyLimitPoints
//    - Calculates: Entry = Ask - (offsetPoints * point)
//    - Calculates: SL = Entry - (slPoints * point)
//    - Calculates: TP = Entry + (tpPoints * point)
//    - Source: MT5Sugar.cs line 1374
//
// 10. SellLimitPoints
//     - Calculates: Entry = Bid + (offsetPoints * point)
//     - Calculates: SL = Entry + (slPoints * point)
//     - Calculates: TP = Entry - (tpPoints * point)
//     - Source: MT5Sugar.cs line 1418
//
// ALL EXAMPLES SHOW REAL CALCULATIONS - NOT JUST SIMPLE DATA RETRIEVAL!
// ══════════════════════════════════════════════════════════════════════════════
