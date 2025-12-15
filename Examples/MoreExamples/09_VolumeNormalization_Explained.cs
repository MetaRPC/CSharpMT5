// ══════════════════════════════════════════════════════════════════════════════
// FILE: 10_VolumeNormalization_Explained.cs
// PURPOSE: Volume normalization - Why brokers reject "wrong" volumes
//
// Topics covered:
//   1. WHAT are volume limits (min, max, step)
//   2. WHY broker rejects volumes like 0.0234 or 0.15
//   3. HOW to normalize volume to broker's requirements
//   4. WHAT to do if calculated volume < minimum
//   5. REAL examples with different broker settings
//
// Key principle: Brokers only accept volumes that are multiples of STEP
// starting from MINIMUM and not exceeding MAXIMUM.
//
// Example: If step = 0.01, valid volumes: 0.01, 0.02, 0.03...
//          Invalid: 0.015, 0.0234, 0.999
//
// ══════════════════════════════════════════════════════════════════════════════

using System;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;

namespace MetaRPC.CSharpMT5.Examples.MoreExamples;

// Declare public static class
public static class VolumeNormalizationExamples
{
    // Define async method
    public static async Task RunAsync()
    {
        // ═══════════════════════════════════════════════════════════════════════
        // STEP 1: DISPLAY HEADER
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   VOLUME NORMALIZATION - Understanding Broker Rules       ║");
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
        // EXAMPLE 1: UNDERSTANDING VOLUME LIMITS
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 1: What Are Volume Limits?");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Get volume limits for the symbol
        // Returns tuple: (minVolume, maxVolume, stepVolume)
        // Type: (double, double, double)
        Console.WriteLine($"📞 Calling: service.GetVolumeLimitsAsync(\"{symbol}\")");

        var (minVol, maxVol, stepVol) = await service.GetVolumeLimitsAsync(symbol);

        Console.WriteLine($"\n📊 Broker's Volume Rules for {symbol}:");
        Console.WriteLine($"   Minimum: {minVol} lots");
        Console.WriteLine($"   Maximum: {maxVol} lots");
        Console.WriteLine($"   Step: {stepVol} lots\n");

        Console.WriteLine($"💡 WHAT THIS MEANS:");
        Console.WriteLine($"   MINIMUM ({minVol}):");
        Console.WriteLine($"   - You CANNOT trade less than {minVol} lots");
        Console.WriteLine($"   - Trying 0.001 when min = {minVol} → ERROR 10014\n");

        Console.WriteLine($"   MAXIMUM ({maxVol}):");
        Console.WriteLine($"   - You CANNOT trade more than {maxVol} lots in single order");
        Console.WriteLine($"   - Want bigger position? → Open multiple orders\n");

        Console.WriteLine($"   STEP ({stepVol}):");
        Console.WriteLine($"   - Volume MUST be multiple of {stepVol}");
        Console.WriteLine($"   - Valid: {minVol}, {minVol + stepVol}, {minVol + 2 * stepVol}...");
        Console.WriteLine($"   - Invalid: anything that's NOT a multiple of {stepVol}\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 2: WHY VOLUMES GET REJECTED
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 2: Common Volume Errors and Why They Happen");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"📋 Broker rules: min={minVol}, max={maxVol}, step={stepVol}\n");

        // Test case 1: Volume too small
        double tooSmall = minVol / 2;
        Console.WriteLine($"❌ CASE 1: Volume too small");
        Console.WriteLine($"   Trying: {tooSmall} lots");
        Console.WriteLine($"   Minimum: {minVol} lots");
        Console.WriteLine($"   Problem: {tooSmall} < {minVol}");
        Console.WriteLine($"   Result: ERROR 10014 (Invalid volume)");
        Console.WriteLine($"   Fix: Use {minVol} or higher\n");

        // Test case 2: Volume too large
        double tooLarge = maxVol + 1.0;
        Console.WriteLine($"❌ CASE 2: Volume too large");
        Console.WriteLine($"   Trying: {tooLarge} lots");
        Console.WriteLine($"   Maximum: {maxVol} lots");
        Console.WriteLine($"   Problem: {tooLarge} > {maxVol}");
        Console.WriteLine($"   Result: ERROR 10014 (Invalid volume)");
        Console.WriteLine($"   Fix: Split into multiple orders or use {maxVol}\n");

        // Test case 3: Volume not multiple of step
        double wrongStep = minVol + (stepVol / 2);  // Between valid steps
        Console.WriteLine($"❌ CASE 3: Volume not multiple of step");
        Console.WriteLine($"   Trying: {wrongStep} lots");
        Console.WriteLine($"   Step: {stepVol} lots");
        Console.WriteLine($"   Problem: {wrongStep} is NOT a multiple of {stepVol}");
        Console.WriteLine($"   Result: ERROR 10014 (Invalid volume)");
        Console.WriteLine($"   Fix: Round to nearest valid step\n");

        // Show valid volumes
        Console.WriteLine($"✅ VALID VOLUMES (first 10 examples):");
        for (int i = 0; i < 10; i++)
        {
            double validVol = minVol + (i * stepVol);
            if (validVol > maxVol) break;
            Console.WriteLine($"   {validVol:F2} lots ✓");
        }
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 3: HOW NORMALIZATION WORKS
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 3: How Volume Normalization Works");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"💡 NORMALIZATION ALGORITHM:");
        Console.WriteLine($"   1. If volume < min → Set to min");
        Console.WriteLine($"   2. If volume > max → Set to max");
        Console.WriteLine($"   3. Round to nearest multiple of step");
        Console.WriteLine($"   4. Return normalized volume\n");

        // Test several invalid volumes
        double[] testVolumes = { 0.0234, 0.155, 0.999, 1.234 };

        Console.WriteLine($"🔬 TESTING NORMALIZATION:\n");

        foreach (var testVol in testVolumes)
        {
            Console.WriteLine($"   Original volume: {testVol}");

            // Call NormalizeVolumeAsync() - performs normalization
            // This is THE method to use before every trade!
            double normalized = await service.NormalizeVolumeAsync(symbol, testVol);

            Console.WriteLine($"   Normalized to: {normalized}");

            // Explain the change
            if (testVol < minVol)
            {
                Console.WriteLine($"   Reason: {testVol} < min ({minVol}), raised to minimum");
            }
            else if (testVol > maxVol)
            {
                Console.WriteLine($"   Reason: {testVol} > max ({maxVol}), capped to maximum");
            }
            else
            {
                Console.WriteLine($"   Reason: Rounded to nearest step ({stepVol})");
            }

            Console.WriteLine($"   Status: {(normalized == testVol ? "Already valid ✓" : "Corrected ✓")}\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 4: MANUAL NORMALIZATION FORMULA
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 4: Understanding the Normalization Math");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        double exampleVolume = 0.0234;

        Console.WriteLine($"📐 MANUAL CALCULATION for {exampleVolume}:");
        Console.WriteLine($"   Broker rules: min={minVol}, max={maxVol}, step={stepVol}\n");

        // Step 1: Check minimum
        Console.WriteLine($"   Step 1: Check minimum");
        double vol = exampleVolume;
        if (vol < minVol)
        {
            Console.WriteLine($"   {vol} < {minVol} → Raise to {minVol}");
            vol = minVol;
        }
        else
        {
            Console.WriteLine($"   {vol} >= {minVol} → OK, keep {vol}");
        }
        Console.WriteLine();

        // Step 2: Check maximum
        Console.WriteLine($"   Step 2: Check maximum");
        if (vol > maxVol)
        {
            Console.WriteLine($"   {vol} > {maxVol} → Lower to {maxVol}");
            vol = maxVol;
        }
        else
        {
            Console.WriteLine($"   {vol} <= {maxVol} → OK, keep {vol}");
        }
        Console.WriteLine();

        // Step 3: Round to step
        Console.WriteLine($"   Step 3: Round to nearest step");
        Console.WriteLine($"   Formula: Math.Round(volume / step) × step");
        Console.WriteLine($"   Calculation: Math.Round({vol} / {stepVol}) × {stepVol}");

        double steps = Math.Round(vol / stepVol);
        double finalVol = steps * stepVol;

        Console.WriteLine($"   Number of steps: {steps}");
        Console.WriteLine($"   Final volume: {finalVol}\n");

        Console.WriteLine($"✅ RESULT: {exampleVolume} → {finalVol} lots\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 5: RISK-BASED VOLUME CALCULATION WITH NORMALIZATION
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 5: Real Scenario - Risk Calculation + Normalization");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Get account balance
        double balance = await service.GetBalanceAsync();
        Console.WriteLine($"📊 Account balance: ${balance:F2}\n");

        // Define risk parameters
        double riskPercent = 1.0;  // 1% risk
        double riskAmount = balance * (riskPercent / 100.0);
        int stopLossPoints = 50;

        Console.WriteLine($"🎯 Trade Parameters:");
        Console.WriteLine($"   Risk: {riskPercent}% of balance");
        Console.WriteLine($"   Risk amount: ${riskAmount:F2}");
        Console.WriteLine($"   Stop Loss: {stopLossPoints} points\n");

        // Calculate raw volume based on risk
        Console.WriteLine($"📐 Step 1: Calculate volume from risk");
        double rawVolume = await service.CalcVolumeForRiskAsync(symbol, stopLossPoints, riskAmount);
        Console.WriteLine($"   Raw calculated volume: {rawVolume:F8} lots\n");

        Console.WriteLine($"💡 PROBLEM:");
        Console.WriteLine($"   Broker requires step of {stepVol}");
        Console.WriteLine($"   Raw volume {rawVolume:F8} is NOT a multiple of {stepVol}");
        Console.WriteLine($"   Sending this → ERROR 10014\n");

        // Normalize the volume
        Console.WriteLine($"🔧 Step 2: Normalize volume");
        double normalizedVolume = await service.NormalizeVolumeAsync(symbol, rawVolume);
        Console.WriteLine($"   Normalized volume: {normalizedVolume} lots\n");

        Console.WriteLine($"✅ SOLUTION:");
        Console.WriteLine($"   Use {normalizedVolume} lots instead of {rawVolume:F8}");
        Console.WriteLine($"   This is valid and will be accepted ✓\n");

        // Verify the risk with normalized volume
        var (tickValue, tickSize) = await service.GetTickValueAndSizeAsync(symbol);
        double actualRisk = stopLossPoints * tickValue;

        Console.WriteLine($"📊 Risk comparison:");
        Console.WriteLine($"   Target risk: ${riskAmount:F2}");
        Console.WriteLine($"   Actual risk with normalized volume: ${actualRisk:F2}");
        Console.WriteLine($"   Difference: ${Math.Abs(actualRisk - riskAmount):F2}");
        Console.WriteLine($"   Status: {(Math.Abs(actualRisk - riskAmount) < 1.0 ? "Very close ✓" : "Acceptable ✓")}\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 6: WHAT IF CALCULATED VOLUME < MINIMUM?
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 6: When Calculated Volume Is Below Minimum");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Scenario: Very small risk with wide stop loss
        double tinyRisk = 1.0;  // Only $1 risk
        int wideSL = 200;       // Wide 200-point stop loss

        Console.WriteLine($"🎯 Conservative Parameters:");
        Console.WriteLine($"   Risk: ${tinyRisk:F2}");
        Console.WriteLine($"   Stop Loss: {wideSL} points\n");

        // Calculate volume
        double tinyVolume = await service.CalcVolumeForRiskAsync(symbol, wideSL, tinyRisk);
        Console.WriteLine($"📐 Calculated volume: {tinyVolume:F8} lots\n");

        // Normalize
        double normalizedTiny = await service.NormalizeVolumeAsync(symbol, tinyVolume);
        Console.WriteLine($"🔧 Normalized volume: {normalizedTiny} lots\n");

        // Check if it was raised to minimum
        if (normalizedTiny > tinyVolume)
        {
            Console.WriteLine($"⚠️  WHAT HAPPENED:");
            Console.WriteLine($"   Calculated: {tinyVolume:F8} lots");
            Console.WriteLine($"   Minimum allowed: {minVol} lots");
            Console.WriteLine($"   Normalized raised to: {normalizedTiny} lots\n");

            // Calculate actual risk with minimum volume
            var (minTickValue, minTickSize) = await service.GetTickValueAndSizeAsync(symbol);
            double actualMinRisk = wideSL * minTickValue;

            Console.WriteLine($"💡 CONSEQUENCE:");
            Console.WriteLine($"   You wanted to risk: ${tinyRisk:F2}");
            Console.WriteLine($"   You'll actually risk: ${actualMinRisk:F2}");
            Console.WriteLine($"   Difference: ${actualMinRisk - tinyRisk:F2} extra\n");

            Console.WriteLine($"🔧 SOLUTIONS:");
            Console.WriteLine($"   1. Accept higher risk (${actualMinRisk:F2})");
            Console.WriteLine($"   2. Tighten Stop Loss (make it smaller than {wideSL} points)");
            Console.WriteLine($"   3. Increase risk amount (more than ${tinyRisk:F2})");
            Console.WriteLine($"   4. Use different symbol with smaller minimum volume\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 7: DIFFERENT BROKER SETTINGS (SIMULATION)
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 7: How Different Brokers Affect Volume Requirements");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        double sampleVolume = 0.0234;

        Console.WriteLine($"📊 Normalizing {sampleVolume} with different broker rules:\n");

        // Broker 1: Standard retail (most common)
        Console.WriteLine($"   Broker 1 (Standard Retail):");
        Console.WriteLine($"   min=0.01, max=100.0, step=0.01");
        double broker1 = Math.Max(0.01, Math.Round(sampleVolume / 0.01) * 0.01);
        Console.WriteLine($"   Result: {broker1} lots\n");

        // Broker 2: Micro account
        Console.WriteLine($"   Broker 2 (Micro Account):");
        Console.WriteLine($"   min=0.001, max=10.0, step=0.001");
        double broker2 = Math.Max(0.001, Math.Round(sampleVolume / 0.001) * 0.001);
        Console.WriteLine($"   Result: {broker2} lots\n");

        // Broker 3: Large step
        Console.WriteLine($"   Broker 3 (Large Step):");
        Console.WriteLine($"   min=0.1, max=500.0, step=0.1");
        double broker3 = Math.Max(0.1, Math.Round(sampleVolume / 0.1) * 0.1);
        Console.WriteLine($"   Result: {broker3} lots\n");

        Console.WriteLine($"💡 OBSERVATION:");
        Console.WriteLine($"   Same desired volume ({sampleVolume})");
        Console.WriteLine($"   Different brokers → Different actual volumes");
        Console.WriteLine($"   This is why you MUST normalize for YOUR broker!\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 8: AUTOMATIC NORMALIZATION IN MT5SUGAR
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 8: MT5Sugar Methods - Auto-Normalization");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"✅ GOOD NEWS: MT5Sugar methods normalize automatically!\n");

        Console.WriteLine($"📚 METHODS WITH AUTO-NORMALIZATION:");
        Console.WriteLine($"   - BuyMarket() - normalizes volume");
        Console.WriteLine($"   - SellMarket() - normalizes volume");
        Console.WriteLine($"   - BuyLimitPoints() - normalizes volume");
        Console.WriteLine($"   - SellLimitPoints() - normalizes volume");
        Console.WriteLine($"   - BuyStopPoints() - normalizes volume");
        Console.WriteLine($"   - SellStopPoints() - normalizes volume");
        Console.WriteLine($"   - BuyByRisk() - calculates AND normalizes volume");
        Console.WriteLine($"   - SellByRisk() - calculates AND normalizes volume\n");

        Console.WriteLine($"💡 EXAMPLE:");
        Console.WriteLine($"   await service.BuyMarket(");
        Console.WriteLine($"       symbol: \"{symbol}\",");
        Console.WriteLine($"       volume: 0.0234,  ← Invalid volume");
        Console.WriteLine($"       slPoints: 50,");
        Console.WriteLine($"       tpPoints: 100");
        Console.WriteLine($"   );");
        Console.WriteLine($"   ↓");
        Console.WriteLine($"   Internally normalizes 0.0234 → {await service.NormalizeVolumeAsync(symbol, 0.0234)}");
        Console.WriteLine($"   Then sends order with valid volume ✓\n");

        Console.WriteLine($"⚠️  EXCEPTION: MT5Service methods DON'T auto-normalize");
        Console.WriteLine($"   If using MT5Service layer, YOU must normalize manually:");
        Console.WriteLine($"   double vol = await service.NormalizeVolumeAsync(symbol, 0.0234);");
        Console.WriteLine($"   await service.BuyMarketAsync(symbol, vol, 0, 0);\n");

        // ═══════════════════════════════════════════════════════════════════════
        // SUMMARY
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("SUMMARY - Volume Normalization Essentials");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine($"📋 BROKER RULES (for {symbol}):");
        Console.WriteLine($"   Minimum: {minVol} lots");
        Console.WriteLine($"   Maximum: {maxVol} lots");
        Console.WriteLine($"   Step: {stepVol} lots");
        Console.WriteLine($"   Valid volumes MUST be multiples of step!\n");

        Console.WriteLine($"🔧 NORMALIZATION STEPS:");
        Console.WriteLine($"   1. If volume < min → Set to min");
        Console.WriteLine($"   2. If volume > max → Set to max");
        Console.WriteLine($"   3. Round to nearest multiple of step");
        Console.WriteLine($"   4. Result: Valid volume that broker will accept\n");

        Console.WriteLine($"⚠️  COMMON ERRORS:");
        Console.WriteLine($"   ERROR 10014 (Invalid volume) means:");
        Console.WriteLine($"   - Volume too small (< {minVol})");
        Console.WriteLine($"   - Volume too large (> {maxVol})");
        Console.WriteLine($"   - Volume not multiple of step ({stepVol})\n");

        Console.WriteLine($"✅ BEST PRACTICES:");
        Console.WriteLine($"   1. ALWAYS normalize volume before trading");
        Console.WriteLine($"   2. Use NormalizeVolumeAsync() explicitly");
        Console.WriteLine($"   3. Or use MT5Sugar methods (auto-normalize)");
        Console.WriteLine($"   4. Check GetVolumeLimitsAsync() to understand broker rules");
        Console.WriteLine($"   5. If normalized volume > desired → Accept or adjust strategy\n");

        Console.WriteLine($"📚 KEY METHODS:");
        Console.WriteLine($"   - GetVolumeLimitsAsync() - Get broker's rules");
        Console.WriteLine($"   - NormalizeVolumeAsync() - Fix invalid volume");
        Console.WriteLine($"   - MT5Sugar methods - Auto-normalize for you\n");

        Console.WriteLine($"💡 REMEMBER:");
        Console.WriteLine($"   Don't assume your calculated volume is valid!");
        Console.WriteLine($"   ALWAYS normalize before sending to broker.\n");

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Normalize volumes = No more 10014 errors!               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
    }
}
