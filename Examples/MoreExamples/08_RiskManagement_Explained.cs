// ══════════════════════════════════════════════════════════════════════════════
// FILE: 08_RiskManagement_Explained.cs
// PURPOSE: Risk management - HOW NOT TO LOSE YOUR ENTIRE DEPOSIT
//
// ⚠️  THIS IS THE MOST IMPORTANT FILE FOR BEGINNERS! ⚠️
//
// Topics covered:
//   1. WHY you should NEVER use fixed volume (0.01, 0.1, 1.0)
//   2. HOW to calculate position size based on dollar risk
//   3. WHAT is the relationship between risk, stop-loss, and volume
//   4. HOW to check margin BEFORE opening a position
//   5. REAL EXAMPLES with actual numbers
//
// Key principle: ALWAYS calculate volume based on:
//   - How much money you're willing to LOSE ($10, $50, $100)
//   - How far your Stop Loss is from entry (20 points, 50 points, 100 points)
//
// Formula: Volume = RiskAmount / (StopLossPoints × PointValue)
//
// ══════════════════════════════════════════════════════════════════════════════

using System;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;

namespace MetaRPC.CSharpMT5.Examples.MoreExamples;

// Declare public static class
public static class RiskManagementExamples
{
    // Define async method
    public static async Task RunAsync()
    {
        // ═══════════════════════════════════════════════════════════════════════
        // STEP 1: DISPLAY HEADER
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   RISK MANAGEMENT - How NOT to Lose Your Deposit          ║");
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

        // Create MT5Service wrapper (Layer 2)
        var service = new MT5Service(account);

        // Create MT5Sugar convenience API (Layer 3)
        var sugar = new MT5Sugar(service);

        // Define symbol for examples
        string symbol = "EURUSD";

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 1: WHY FIXED VOLUME IS DANGEROUS
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 1: Why Fixed Volume (0.01, 0.1, 1.0) is DANGEROUS");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Get current account balance
        // Type: double
        var balance = await service.GetBalanceAsync();
        Console.WriteLine($"📊 Your current balance: ${balance:F2}\n");

        // SCENARIO: Using fixed volume of 0.1 lots on EURUSD
        double fixedVolume = 0.1;
        int stopLossPoints = 50;  // Stop Loss at 50 points from entry

        Console.WriteLine($"❌ BAD APPROACH: Opening position with FIXED volume {fixedVolume} lots");
        Console.WriteLine($"   Stop Loss: {stopLossPoints} points\n");

        // Calculate point value for this symbol and volume
        // Point value = how much 1 point movement costs in account currency
        // For EURUSD 1.0 lot: 1 point = $10
        // For EURUSD 0.1 lot: 1 point = $1
        // Formula: PointValue = ContractSize × Volume × Point
        var (tickValue, tickSize) = await sugar.GetTickValueAndSizeAsync(symbol, fixedVolume);

        // tickValue = value of 1 tick movement (usually 1 point = 1 tick for EURUSD)
        // tickSize = size of 1 tick in price units (0.00001 for EURUSD)
        double pointValue = tickValue;

        Console.WriteLine($"   Point value for {fixedVolume} lots: ${pointValue:F2} per point");

        // Calculate potential loss if Stop Loss is hit
        // Loss = StopLossPoints × PointValue
        double potentialLoss = stopLossPoints * pointValue;

        Console.WriteLine($"   If SL hit: you lose ${potentialLoss:F2}");

        // Calculate what percentage of balance this is
        double lossPercent = (potentialLoss / balance) * 100;

        Console.WriteLine($"   This is {lossPercent:F2}% of your balance!\n");

        // EXPLANATION: Why this is bad
        Console.WriteLine("💡 WHY THIS IS DANGEROUS:");
        Console.WriteLine($"   - If balance = $100 → {lossPercent:F2}% loss is HUGE!");
        Console.WriteLine($"   - If balance = $10,000 → {lossPercent:F2}% loss is tiny");
        Console.WriteLine("   - SAME volume behaves DIFFERENTLY on different account sizes!");
        Console.WriteLine("   - This is why professionals NEVER use fixed volumes.\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 2: CORRECT APPROACH - Calculate Volume from Risk Amount
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 2: CORRECT APPROACH - Risk-Based Position Sizing");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Define risk parameters
        // QUESTION: "How much am I willing to LOSE on this trade?"
        // ANSWER: Let's say $10
        double riskAmount = 10.0;

        Console.WriteLine($"✅ GOOD APPROACH: Risk-based calculation");
        Console.WriteLine($"   I'm willing to risk: ${riskAmount:F2}");
        Console.WriteLine($"   My Stop Loss: {stopLossPoints} points\n");

        // Call CalcVolumeForRiskAsync() - THE MOST IMPORTANT METHOD!
        // This calculates the lot size that risks EXACTLY $10 if SL is hit
        //
        // How it works internally:
        // 1. Gets tick value for 1.0 lot
        // 2. Calculates point value
        // 3. Formula: volume = riskAmount / (stopLossPoints × pointValue)
        // 4. Normalizes volume to broker's step (0.01, 0.1, etc.)
        //
        // 'await' pauses here until calculation completes
        Console.WriteLine($"📐 Calling: sugar.CalcVolumeForRiskAsync()");

        double calculatedVolume = await sugar.CalcVolumeForRiskAsync(
            symbol,           // "EURUSD"
            stopLossPoints,   // 50 points
            riskAmount        // $10
        );

        Console.WriteLine($"   Result: {calculatedVolume} lots\n");

        // Verify the calculation
        var (calcTickValue, calcTickSize) = await sugar.GetTickValueAndSizeAsync(symbol, calculatedVolume);
        double verifyLoss = stopLossPoints * calcTickValue;

        Console.WriteLine($"   Verification:");
        Console.WriteLine($"   - Volume: {calculatedVolume} lots");
        Console.WriteLine($"   - Point value: ${calcTickValue:F4} per point");
        Console.WriteLine($"   - If SL hit: you lose ${verifyLoss:F2}");
        Console.WriteLine($"   - This is exactly ${riskAmount:F2}! ✓\n");

        // Calculate percentage of balance
        double riskPercent = (riskAmount / balance) * 100;
        Console.WriteLine($"💡 BENEFIT:");
        Console.WriteLine($"   - You risk {riskPercent:F2}% of your balance");
        Console.WriteLine($"   - This percentage stays CONSISTENT regardless of account size");
        Console.WriteLine($"   - You have full CONTROL over your risk\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 3: REAL SCENARIO - Small Account
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 3: Real Scenario - Trading with $500 Account");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Simulated small account balance
        double smallBalance = 500.0;

        Console.WriteLine($"📊 Account balance: ${smallBalance:F2}");
        Console.WriteLine($"   Risk per trade: 2% (professional standard)");

        // Calculate 2% risk
        double twoPercentRisk = smallBalance * 0.02;

        Console.WriteLine($"   2% of ${smallBalance:F2} = ${twoPercentRisk:F2}\n");

        // Scenario 1: Tight stop loss (20 points)
        int tightSL = 20;
        double volumeTightSL = await sugar.CalcVolumeForRiskAsync(symbol, tightSL, twoPercentRisk);

        Console.WriteLine($"   Scenario 1: Tight Stop Loss ({tightSL} points)");
        Console.WriteLine($"   - Risk: ${twoPercentRisk:F2}");
        Console.WriteLine($"   - Volume: {volumeTightSL} lots\n");

        // Scenario 2: Wide stop loss (100 points)
        int wideSL = 100;
        double volumeWideSL = await sugar.CalcVolumeForRiskAsync(symbol, wideSL, twoPercentRisk);

        Console.WriteLine($"   Scenario 2: Wide Stop Loss ({wideSL} points)");
        Console.WriteLine($"   - Risk: ${twoPercentRisk:F2}");
        Console.WriteLine($"   - Volume: {volumeWideSL} lots\n");

        Console.WriteLine($"💡 OBSERVATION:");
        Console.WriteLine($"   - Tighter SL → LARGER volume ({volumeTightSL} lots)");
        Console.WriteLine($"   - Wider SL → SMALLER volume ({volumeWideSL} lots)");
        Console.WriteLine($"   - Both risk EXACTLY ${twoPercentRisk:F2}!");
        Console.WriteLine($"   - This is the PROFESSIONAL way to trade.\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 4: CHECK MARGIN BEFORE OPENING POSITION
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 4: Always Check Margin BEFORE Opening Position");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Define trade parameters
        double testVolume = 0.05;

        Console.WriteLine($"📊 Planning to open:");
        Console.WriteLine($"   Symbol: {symbol}");
        Console.WriteLine($"   Volume: {testVolume} lots\n");

        // Call CheckMarginAvailabilityAsync() - CRITICAL for avoiding rejections!
        // This checks if you have enough free margin to open the position
        //
        // Returns tuple with 3 values:
        // - hasEnough: true if you can afford this trade
        // - freeMargin: current free margin available
        // - required: margin required for this trade
        //
        // 'await' pauses here until check completes
        Console.WriteLine($"🔍 Calling: sugar.CheckMarginAvailabilityAsync()");

        var (hasEnough, freeMargin, required) = await sugar.CheckMarginAvailabilityAsync(
            symbol,      // "EURUSD"
            testVolume,  // 0.05 lots
            isBuy: true  // BUY position (true) or SELL (false)
        );

        Console.WriteLine($"   Result:");
        Console.WriteLine($"   - Free margin: ${freeMargin:F2}");
        Console.WriteLine($"   - Required margin: ${required:F2}");
        Console.WriteLine($"   - Can afford trade: {(hasEnough ? "YES ✓" : "NO ✗")}\n");

        // Check result and explain
        if (hasEnough)
        {
            Console.WriteLine($"✅ SAFE TO TRADE:");
            Console.WriteLine($"   - You have ${freeMargin:F2} free margin");
            Console.WriteLine($"   - This trade needs ${required:F2}");
            Console.WriteLine($"   - After opening: ${freeMargin - required:F2} will remain");
            Console.WriteLine($"   - Order will be accepted by broker ✓\n");
        }
        else
        {
            Console.WriteLine($"❌ NOT ENOUGH MARGIN:");
            Console.WriteLine($"   - You have ${freeMargin:F2} free margin");
            Console.WriteLine($"   - This trade needs ${required:F2}");
            Console.WriteLine($"   - Shortage: ${required - freeMargin:F2}");
            Console.WriteLine($"   - Broker will reject this order with error 10019");
            Console.WriteLine($"   - REDUCE volume or CLOSE some positions first!\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 5: COMPLETE RISK-BASED TRADE FLOW
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 5: Complete Professional Trade Flow");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // STEP 1: Define risk parameters
        double myRiskAmount = 5.0;  // Willing to lose $5
        int mySL = 30;              // Stop Loss at 30 points
        int myTP = 60;              // Take Profit at 60 points (2:1 reward/risk)

        Console.WriteLine($"🎯 Trade Plan:");
        Console.WriteLine($"   Risk amount: ${myRiskAmount:F2}");
        Console.WriteLine($"   Stop Loss: {mySL} points");
        Console.WriteLine($"   Take Profit: {myTP} points");
        Console.WriteLine($"   Risk:Reward ratio: 1:{myTP / mySL}\n");

        // STEP 2: Calculate volume based on risk
        Console.WriteLine($"📐 Step 1: Calculate position size");
        double tradeVolume = await sugar.CalcVolumeForRiskAsync(symbol, mySL, myRiskAmount);
        Console.WriteLine($"   Calculated volume: {tradeVolume} lots\n");

        // STEP 3: Check margin availability
        Console.WriteLine($"🔍 Step 2: Check margin availability");
        var (canTrade, freeMgn, reqMgn) = await sugar.CheckMarginAvailabilityAsync(
            symbol,
            tradeVolume,
            isBuy: true
        );
        Console.WriteLine($"   Free margin: ${freeMgn:F2}");
        Console.WriteLine($"   Required: ${reqMgn:F2}");
        Console.WriteLine($"   Can trade: {(canTrade ? "YES ✓" : "NO ✗")}\n");

        // STEP 4: Decision
        if (canTrade)
        {
            Console.WriteLine($"✅ ALL CHECKS PASSED - Ready to trade!");
            Console.WriteLine($"\n   Now you would call:");
            Console.WriteLine($"   await sugar.BuyMarket(");
            Console.WriteLine($"       symbol: \"{symbol}\",");
            Console.WriteLine($"       volume: {tradeVolume},");
            Console.WriteLine($"       slPoints: {mySL},");
            Console.WriteLine($"       tpPoints: {myTP}");
            Console.WriteLine($"   );\n");

            Console.WriteLine($"💡 WHAT HAPPENS IF SL IS HIT:");
            Console.WriteLine($"   - You lose EXACTLY ${myRiskAmount:F2}");
            Console.WriteLine($"   - Your balance: ${balance:F2} → ${balance - myRiskAmount:F2}");
            Console.WriteLine($"   - Loss: {(myRiskAmount / balance) * 100:F2}% of balance\n");

            Console.WriteLine($"💡 WHAT HAPPENS IF TP IS HIT:");
            double potentialProfit = myRiskAmount * (myTP / (double)mySL);
            Console.WriteLine($"   - You gain ${potentialProfit:F2}");
            Console.WriteLine($"   - Your balance: ${balance:F2} → ${balance + potentialProfit:F2}");
            Console.WriteLine($"   - Profit: {(potentialProfit / balance) * 100:F2}% of balance\n");
        }
        else
        {
            Console.WriteLine($"❌ INSUFFICIENT MARGIN - Cannot trade!");
            Console.WriteLine($"   Need ${reqMgn:F2}, have ${freeMgn:F2}");
            Console.WriteLine($"   Shortage: ${reqMgn - freeMgn:F2}\n");

            Console.WriteLine($"🔧 SOLUTIONS:");
            Console.WriteLine($"   1. REDUCE risk amount (try ${myRiskAmount / 2:F2} instead)");
            Console.WriteLine($"   2. WIDEN stop loss (try {mySL * 2} points instead)");
            Console.WriteLine($"   3. CLOSE some existing positions");
            Console.WriteLine($"   4. DEPOSIT more funds\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // SUMMARY
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("SUMMARY - Risk Management Golden Rules");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("✅ DO:");
        Console.WriteLine("   1. ALWAYS calculate volume from risk amount");
        Console.WriteLine("   2. ALWAYS check margin before opening position");
        Console.WriteLine("   3. Risk 1-2% of balance per trade (professionals use this)");
        Console.WriteLine("   4. Use CalcVolumeForRiskAsync() for position sizing");
        Console.WriteLine("   5. Use CheckMarginAvailabilityAsync() before trading\n");

        Console.WriteLine("❌ DON'T:");
        Console.WriteLine("   1. NEVER use fixed volume (0.01, 0.1, 1.0)");
        Console.WriteLine("   2. NEVER trade without Stop Loss");
        Console.WriteLine("   3. NEVER risk more than 2% per trade");
        Console.WriteLine("   4. NEVER open position without checking margin");
        Console.WriteLine("   5. NEVER assume 'it will work' - always verify!\n");

        Console.WriteLine("📚 KEY METHODS:");
        Console.WriteLine("   - CalcVolumeForRiskAsync(symbol, slPoints, riskAmount)");
        Console.WriteLine("   - CheckMarginAvailabilityAsync(symbol, volume, isBuy)");
        Console.WriteLine("   - GetTickValueAndSizeAsync(symbol, volume)\n");

        Console.WriteLine("💡 REMEMBER:");
        Console.WriteLine("   'Professional traders don't focus on how much they can MAKE,");
        Console.WriteLine("    they focus on how much they can LOSE.'\n");

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Follow these rules and your account will survive!       ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
    }
}
