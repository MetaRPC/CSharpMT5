// ══════════════════════════════════════════════════════════════════════════════
// FILE: 07_ErrorHandling_Explained.cs
// PURPOSE: Error handling - How to handle trading operation failures
//
// Topics covered:
//   1. WHAT is ReturnCode and why it matters MORE than exceptions
//   2. HOW to check ReturnCode after every trading operation
//   3. COMMON error codes and what they mean
//   4. HOW to handle errors properly (retry, report, abort)
//   5. RETRY logic with exponential backoff for connection errors
//
// Key principle: In trading, SUCCESS/FAILURE is determined by ReturnCode,
// NOT by whether the method throws an exception!
//
// ReturnCode 10009 = Success (TRADE_RETCODE_DONE)
// Anything else = Something went wrong (see list below)
//
// ══════════════════════════════════════════════════════════════════════════════


using System;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;
using mt5_term_api;

namespace MetaRPC.CSharpMT5.Examples.MoreExamples;

public static class ErrorHandlingExamples
{
    // Define async method
    public static async Task RunAsync()
    {
        // ═══════════════════════════════════════════════════════════════════════
        // STEP 1: DISPLAY HEADER
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   ERROR HANDLING - How to Handle Trading Failures         ║");
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
        // EXAMPLE 1: UNDERSTANDING ReturnCode
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 1: What is ReturnCode and Why It Matters");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 IMPORTANT CONCEPT:");
        Console.WriteLine("   In MT5 trading, success/failure is determined by ReturnCode,");
        Console.WriteLine("   NOT by exceptions!\n");

        Console.WriteLine("📋 Common Return Codes:");
        Console.WriteLine("   10009 - Success (TRADE_RETCODE_DONE)");
        Console.WriteLine("   10004 - Requote (price changed, need to retry)");
        Console.WriteLine("   10006 - Request rejected by broker");
        Console.WriteLine("   10013 - Invalid request parameters");
        Console.WriteLine("   10014 - Invalid volume");
        Console.WriteLine("   10015 - Invalid price");
        Console.WriteLine("   10016 - Invalid stops (SL/TP too close)");
        Console.WriteLine("   10018 - Market is closed");
        Console.WriteLine("   10019 - Not enough money (insufficient margin)");
        Console.WriteLine("   10031 - No connection with trade server\n");

        Console.WriteLine("🔍 HOW TO CHECK:");
        Console.WriteLine("   Every trading method returns an object with ReturnCode/ReturnedCode");
        Console.WriteLine("   You MUST check this value after EVERY operation!");
        Console.WriteLine("   if (result.ReturnedCode == 10009) → Success ✓");
        Console.WriteLine("   if (result.ReturnedCode != 10009) → Something failed ✗\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 2: WRONG WAY - Ignoring ReturnCode
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 2: WRONG WAY - Not Checking ReturnCode");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("❌ BAD CODE (DON'T DO THIS):");
        Console.WriteLine("   var result = await service.BuyMarketAsync(symbol, 0.01, 0, 0);");
        Console.WriteLine("   // Assuming it worked... WRONG!");
        Console.WriteLine("   Console.WriteLine(\"Trade opened!\"); // This prints even if it FAILED!\n");

        Console.WriteLine("💥 WHAT CAN GO WRONG:");
        Console.WriteLine("   - Market is closed → Order fails, but code continues");
        Console.WriteLine("   - Insufficient margin → Order rejected, but you think it worked");
        Console.WriteLine("   - Invalid stops → Order rejected, but no error visible");
        Console.WriteLine("   - You have NO IDEA the trade failed until you check MT5 manually!\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 3: CORRECT WAY - Always Check ReturnCode
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 3: CORRECT WAY - Always Check ReturnCode");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("✅ GOOD CODE (DO THIS):");
        Console.WriteLine("   var result = await service.BuyMarketAsync(symbol, 0.01, 0, 0);");
        Console.WriteLine("   if (result.ReturnedCode == 10009)");
        Console.WriteLine("   {");
        Console.WriteLine("       Console.WriteLine(\"Trade opened successfully!\");");
        Console.WriteLine("   }");
        Console.WriteLine("   else");
        Console.WriteLine("   {");
        Console.WriteLine("       Console.WriteLine($\"Trade FAILED: {result.Comment}\");");
        Console.WriteLine("   }\n");

        Console.WriteLine("💡 BENEFITS:");
        Console.WriteLine("   - You KNOW immediately if trade succeeded");
        Console.WriteLine("   - You can log the error message");
        Console.WriteLine("   - You can take corrective action (retry, adjust, abort)");
        Console.WriteLine("   - Your code behaves PREDICTABLY\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 4: PRACTICAL ERROR CHECKING - Attempting Invalid Trade
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 4: Practical Example - Handling Invalid Volume");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Get broker's volume limits
        // Returns tuple: (minVolume, maxVolume, stepVolume)
        var (minVol, maxVol, stepVol) = await service.GetVolumeLimitsAsync(symbol);

        Console.WriteLine($"📊 Broker's volume limits for {symbol}:");
        Console.WriteLine($"   Minimum: {minVol}");
        Console.WriteLine($"   Maximum: {maxVol}");
        Console.WriteLine($"   Step: {stepVol}\n");

        // Attempt to use INVALID volume (too small)
        double invalidVolume = minVol / 2;  // Half the minimum = guaranteed rejection

        Console.WriteLine($"🔬 Attempting to open position with INVALID volume:");
        Console.WriteLine($"   Volume: {invalidVolume} (below minimum {minVol})");
        Console.WriteLine($"   This will FAIL, but let's see how to handle it...\n");

        // Call BuyMarketAsync with invalid volume
        // This will NOT throw exception, but will return error code
        Console.WriteLine($"📞 Calling: service.BuyMarketAsync()");

        var result = await service.BuyMarketAsync(
            symbol,         // "EURUSD"
            invalidVolume,  // Invalid volume (too small)
            stopLoss: 0,    // No stop loss
            takeProfit: 0   // No take profit
        );

        Console.WriteLine($"   Returned from call\n");

        // CHECK RETURN CODE - This is CRITICAL!
        Console.WriteLine($"🔍 Checking ReturnCode...");
        Console.WriteLine($"   ReturnedCode: {result.ReturnedCode}");
        Console.WriteLine($"   Comment: {result.Comment}\n");

        // Handle the result with IF statement
        if (result.ReturnedCode == 10009)
        {
            // This path won't execute because volume was invalid
            Console.WriteLine($"✅ SUCCESS:");
            Console.WriteLine($"   Position opened with ticket #{result.Order}");
        }
        else
        {
            // This path WILL execute
            Console.WriteLine($"❌ FAILED:");
            Console.WriteLine($"   Error code: {result.ReturnedCode}");
            Console.WriteLine($"   Error message: {result.Comment}");
            Console.WriteLine($"   Expected error: 10014 (Invalid volume)\n");

            Console.WriteLine($"💡 WHAT TO DO:");
            Console.WriteLine($"   - Log the error for debugging");
            Console.WriteLine($"   - Check volume limits again");
            Console.WriteLine($"   - Use NormalizeVolumeAsync() to fix volume");
            Console.WriteLine($"   - Don't assume the trade worked!\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 5: HANDLING SPECIFIC ERROR CODES
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 5: Handling Specific Error Codes");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 BEST PRACTICE: Handle different errors differently\n");

        // Example helper method showing comprehensive error handling
        Console.WriteLine("✅ PROFESSIONAL ERROR HANDLING PATTERN:");
        Console.WriteLine(@"
   var result = await service.BuyMarketAsync(symbol, volume, sl, tp);

   switch (result.ReturnedCode)
   {
       case 10009:  // Success
           Console.WriteLine($""✓ Position opened: {result.Order}"");
           break;

       case 10004:  // Requote - price changed
           Console.WriteLine(""⚠ Requote - retrying with new price..."");
           // Retry the operation
           break;

       case 10018:  // Market closed
           Console.WriteLine(""✗ Market is closed, wait for market open"");
           // Schedule retry for market open time
           break;

       case 10019:  // Insufficient margin
           Console.WriteLine(""✗ Not enough money, reduce volume"");
           // Reduce volume or close positions
           break;

       case 10031:  // No connection
           Console.WriteLine(""✗ Connection lost, retrying..."");
           // Retry with exponential backoff
           break;

       case 10014:  // Invalid volume
           Console.WriteLine(""✗ Invalid volume, normalizing..."");
           // Normalize volume and retry
           break;

       case 10016:  // Invalid stops
           Console.WriteLine(""✗ SL/TP too close, adjusting..."");
           // Widen stops and retry
           break;

       default:
           Console.WriteLine($""✗ Unknown error: {result.Comment}"");
           // Log and investigate
           break;
   }
");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 6: RETRY LOGIC WITH EXPONENTIAL BACKOFF
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 6: Retry Logic for Connection Errors");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 WHEN CONNECTION FAILS (code 10031):");
        Console.WriteLine("   Don't give up immediately!");
        Console.WriteLine("   Retry with EXPONENTIAL BACKOFF:\n");

        Console.WriteLine("🔄 RETRY PATTERN:");
        Console.WriteLine("   Attempt 1: Execute → Failed (10031) → Wait 1 second");
        Console.WriteLine("   Attempt 2: Execute → Failed (10031) → Wait 2 seconds");
        Console.WriteLine("   Attempt 3: Execute → Failed (10031) → Wait 4 seconds");
        Console.WriteLine("   Attempt 4: Give up and report error\n");

        Console.WriteLine("✅ IMPLEMENTATION:");
        Console.WriteLine(@"
   int maxRetries = 3;
   int delayMs = 1000;  // Start with 1 second

   for (int attempt = 1; attempt <= maxRetries; attempt++)
   {
       var result = await service.BuyMarketAsync(symbol, volume, sl, tp);

       if (result.ReturnedCode == 10009)
       {
           // Success!
           Console.WriteLine($""✓ Trade opened on attempt {attempt}"");
           break;
       }
       else if (result.ReturnedCode == 10031 && attempt < maxRetries)
       {
           // Connection error - retry
           Console.WriteLine($""⚠ Attempt {attempt} failed: No connection"");
           Console.WriteLine($""  Waiting {delayMs}ms before retry..."");
           await Task.Delay(delayMs);
           delayMs *= 2;  // Exponential backoff: 1s → 2s → 4s
       }
       else
       {
           // Other error or final attempt - give up
           Console.WriteLine($""✗ Failed after {attempt} attempts"");
           Console.WriteLine($""  Error: {result.Comment}"");
           break;
       }
   }
");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 7: VALIDATING BEFORE EXECUTING
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 7: Prevent Errors BEFORE They Happen");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 BEST PRACTICE: Validate BEFORE sending order\n");

        Console.WriteLine("✅ PRE-FLIGHT CHECKLIST:");
        Console.WriteLine("   1. Check margin availability");
        Console.WriteLine("   2. Normalize volume to broker's step");
        Console.WriteLine("   3. Normalize price to symbol's digits");
        Console.WriteLine("   4. Verify SL/TP distance is valid");
        Console.WriteLine("   5. Ensure market is open\n");

        double testVolume = 0.05;
        Console.WriteLine($"🔬 DEMONSTRATION: Validating trade before execution");
        Console.WriteLine($"   Symbol: {symbol}");
        Console.WriteLine($"   Volume: {testVolume}\n");

        // STEP 1: Normalize volume
        Console.WriteLine($"📐 Step 1: Normalize volume");
        double normalizedVolume = await service.NormalizeVolumeAsync(symbol, testVolume);
        Console.WriteLine($"   Original: {testVolume}");
        Console.WriteLine($"   Normalized: {normalizedVolume}");
        Console.WriteLine($"   Status: {(testVolume == normalizedVolume ? "Already valid ✓" : "Adjusted ✓")}\n");

        // STEP 2: Check margin
        Console.WriteLine($"🔍 Step 2: Check margin availability");
        var (hasEnough, freeMargin, required) = await service.CheckMarginAvailabilityAsync(
            symbol,
            normalizedVolume,
            isBuy: true
        );
        Console.WriteLine($"   Free margin: ${freeMargin:F2}");
        Console.WriteLine($"   Required: ${required:F2}");
        Console.WriteLine($"   Status: {(hasEnough ? "Sufficient ✓" : "Insufficient ✗")}\n");

        // STEP 3: Validate order
        Console.WriteLine($"🔍 Step 3: Validate order with broker");

        // Get current price for the order
        var tick = await service.SymbolInfoTickAsync(symbol);
        double buyPrice = tick.Ask;

        // Call ValidateOrderAsync - simulates the order without executing it
        // Returns same ReturnCode as real order would return
        var validation = await service.ValidateOrderAsync(
            symbol,
            normalizedVolume,
            buyPrice,
            isBuy: true
        );

        Console.WriteLine($"   Validation code: {validation.ReturnedCode}");
        Console.WriteLine($"   Status: {(validation.ReturnedCode == 10009 ? "Will succeed ✓" : $"Will fail: {validation.Comment} ✗")}\n");

        // FINAL DECISION
        if (hasEnough && validation.ReturnedCode == 10009)
        {
            Console.WriteLine($"✅ ALL VALIDATIONS PASSED:");
            Console.WriteLine($"   - Volume is normalized ✓");
            Console.WriteLine($"   - Margin is sufficient ✓");
            Console.WriteLine($"   - Broker will accept order ✓");
            Console.WriteLine($"   → Safe to execute BuyMarketAsync()\n");
        }
        else
        {
            Console.WriteLine($"❌ VALIDATION FAILED:");
            if (!hasEnough)
                Console.WriteLine($"   - Insufficient margin ✗");
            if (validation.ReturnedCode != 10009)
                Console.WriteLine($"   - Broker will reject: {validation.Comment} ✗");
            Console.WriteLine($"   → DO NOT execute trade until issues fixed!\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // SUMMARY
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("SUMMARY - Error Handling Best Practices");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("✅ ALWAYS DO:");
        Console.WriteLine("   1. Check ReturnCode after EVERY trading operation");
        Console.WriteLine("   2. Handle specific error codes appropriately");
        Console.WriteLine("   3. Log errors for debugging");
        Console.WriteLine("   4. Validate orders BEFORE executing (CheckMarginAvailabilityAsync)");
        Console.WriteLine("   5. Use retry logic for connection errors (10031)");
        Console.WriteLine("   6. Normalize volumes and prices before sending\n");

        Console.WriteLine("❌ NEVER DO:");
        Console.WriteLine("   1. Assume operation succeeded without checking ReturnCode");
        Console.WriteLine("   2. Ignore error messages");
        Console.WriteLine("   3. Retry indefinitely without backoff");
        Console.WriteLine("   4. Send orders without pre-validation");
        Console.WriteLine("   5. Use hardcoded volumes without normalization\n");

        Console.WriteLine("📋 CRITICAL RETURN CODES TO REMEMBER:");
        Console.WriteLine("   10009 - SUCCESS (only this means trade worked!)");
        Console.WriteLine("   10031 - No connection (retry with backoff)");
        Console.WriteLine("   10019 - Insufficient margin (reduce volume or close positions)");
        Console.WriteLine("   10018 - Market closed (wait for open)");
        Console.WriteLine("   10016 - Invalid stops (widen SL/TP distance)");
        Console.WriteLine("   10014 - Invalid volume (normalize volume)");
        Console.WriteLine("   10004 - Requote (retry with new price)\n");

        Console.WriteLine("📚 KEY METHODS FOR ERROR PREVENTION:");
        Console.WriteLine("   - CheckMarginAvailabilityAsync() - Check before trading");
        Console.WriteLine("   - ValidateOrderAsync() - Simulate order validation");
        Console.WriteLine("   - NormalizeVolumeAsync() - Fix invalid volumes");
        Console.WriteLine("   - NormalizePriceAsync() - Fix invalid prices\n");

        Console.WriteLine("💡 GOLDEN RULE:");
        Console.WriteLine("   'if (result.ReturnedCode != 10009) → Something is wrong!'");
        Console.WriteLine("   Always check this condition after trading operations.\n");

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Proper error handling = Stable, reliable trading bot    ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
    }
}
