// ══════════════════════════════════════════════════════════════════════════════
// FILE: 02_StreamingExamples.cs
// PURPOSE: Real-time streaming with EVERY LINE explained
//
// This file contains WORKING streaming examples copied from:
// Examples/LowLevel/Program.Streaming.cs
//
// Streams demonstrated (3 most useful):
//   1. OnSymbolTickAsync - Real-time price ticks
//   2. ReadTicks - Convenience helper for ticks (with auto-stop)
//   3. ReadTrades - Convenience helper for trades (with auto-stop)
//
// IMPORTANT: Streams send data continuously until stopped!
// IMPORTANT: Without Console.WriteLine, data will NOT appear!
// ══════════════════════════════════════════════════════════════════════════════

// Import System namespace
using System;

// Import Task types
using System.Threading.Tasks;

// Import Threading for CancellationTokenSource
using System.Threading;

// Import connection helper
using MetaRPC.CSharpMT5.Examples.Helpers;

// Import MT5Service
using MetaRPC.CSharpMT5;

// Declare namespace
namespace MetaRPC.CSharpMT5.Examples.MoreExamples;

// Declare public static class
public static class StreamingExamples
{
    // Define async method
    public static async Task RunAsync()
    {
        // ═══════════════════════════════════════════════════════════════════════
        // STEP 1: DISPLAY HEADER
        // ═══════════════════════════════════════════════════════════════════════

        // Print header
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         STREAMING EXAMPLES - Every Line Explained          ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

        // Print important note
        Console.WriteLine("⚠️  IMPORTANT: Streams run continuously!");
        Console.WriteLine("   Each stream will auto-stop after 10 events or 5 seconds\n");

        // ═══════════════════════════════════════════════════════════════════════
        // STEP 2: ESTABLISH CONNECTION
        // ═══════════════════════════════════════════════════════════════════════

        // Load configuration
        var config = ConnectionHelper.BuildConfiguration();
        Console.WriteLine("✓ Configuration loaded");

        // Connect to MT5
        var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);
        Console.WriteLine("✓ Connected to MT5 Terminal");

        // Create MT5Service wrapper
        // MT5Service wraps MT5Account and provides streaming methods
        var service = new MT5Service(account);
        Console.WriteLine("✓ MT5Service created\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 1: OnSymbolTickAsync - Real-time price ticks
        // ═══════════════════════════════════════════════════════════════════════
        // Copied from: Examples/LowLevel/Program.Streaming.cs line 378

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 1: Real-Time Price Ticks (OnSymbolTickAsync)");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Print what we're doing
        Console.WriteLine("📊 Subscribing to tick stream for EURUSD and GBPUSD...");
        Console.WriteLine("   Will stop after 10 ticks or 5 seconds\n");

        // Create array of symbols to monitor
        // Type: string[]
        // We want to receive ticks for both EURUSD and GBPUSD
        var symbols = new[] { "EURUSD", "GBPUSD" };

        // Create CancellationTokenSource - used to stop the stream
        // This object allows us to cancel async operations
        var cts1 = new CancellationTokenSource();

        // Set auto-cancel after 5 seconds
        // If we don't stop manually, stream auto-stops after 5 seconds
        cts1.CancelAfter(TimeSpan.FromSeconds(5));

        // Initialize counter for received ticks
        // We'll increment this for each tick received
        int tickCount = 0;

        // Define max number of ticks to receive
        // Stop after 10 ticks (for demo purposes)
        int maxTicks = 10;

        // Start try block to handle cancellation
        try
        {
            // Call OnSymbolTickAsync() - subscribes to real-time ticks
            // Parameter 1: symbols[] - which symbols to monitor
            // Parameter 2: CancellationToken - how to stop the stream
            // This is a STREAMING method - returns IAsyncEnumerable
            // 'await foreach' processes each tick as it arrives
            // ⚠️  This runs CONTINUOUSLY until cancelled or break!
            await foreach (var tick in service.OnSymbolTickAsync(symbols, cts1.Token))
            {
                // We're inside the loop - a new tick just arrived!
                // tick object contains data, but INVISIBLE until printed!

                // Increment counter
                // ++ means add 1 to tickCount
                tickCount++;

                // Extract SymbolTick from the tick object
                // Type: MqlTick
                // Contains: Symbol, Bid, Ask, Last, Volume, Time, etc.
                var t = tick.SymbolTick;

                // Extract Symbol name from tick
                // Type: string
                // This tells us which symbol this tick is for
                var tickSymbol = t.Symbol;

                // Extract Bid price - price to SELL at
                // Type: double
                var bid = t.Bid;

                // Extract Ask price - price to BUY at
                // Type: double
                var ask = t.Ask;

                // Extract TimeMsc - timestamp in milliseconds
                // Type: long (Unix timestamp in milliseconds)
                var timeMsc = t.TimeMsc;

                // Convert timestamp to DateTime
                // DateTimeOffset.FromUnixTimeMilliseconds() converts Unix time
                // .DateTime gets the DateTime portion
                var time = DateTimeOffset.FromUnixTimeMilliseconds(timeMsc).DateTime;

                // Print tick data - WITHOUT this, tick is INVISIBLE!
                // {tickCount}: event number
                // {tickSymbol,-7}: symbol name, left-aligned, 7 characters wide
                // {bid:F5}: bid price with 5 decimal places
                // {ask:F5}: ask price with 5 decimal places
                // {time:HH:mm:ss.fff}: time with milliseconds
                Console.WriteLine($"[TICK] #{tickCount}: {tickSymbol,-7} | Bid: {bid:F5} | Ask: {ask:F5} | Time: {time:HH:mm:ss.fff}");

                // Check if we've received enough ticks
                if (tickCount >= maxTicks)
                {
                    // We've reached the limit

                    // Print stop message
                    Console.WriteLine($"[TICK] Reached {maxTicks} ticks limit, stopping...");

                    // Break out of loop - stops the stream
                    // 'break' exits await foreach and disposes the stream
                    break;
                }

                // Loop continues - waiting for next tick...
            }
            // Loop ended - either by break or cancellation
        }
        catch (OperationCanceledException)
        {
            // Stream was cancelled (timeout or manual cancel)
            // This is NORMAL and EXPECTED when timeout occurs
            Console.WriteLine($"[TICK] Stream cancelled after {tickCount} ticks (timeout)");
        }
        catch (Exception ex)
        {
            // Some other error occurred
            Console.WriteLine($"[TICK] Error: {ex.Message}");
        }

        // Print summary
        Console.WriteLine($"\nReceived total: {tickCount} ticks\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 2: ReadTicks - Convenience helper with auto-limits
        // ═══════════════════════════════════════════════════════════════════════
        // Copied from: Examples/LowLevel/Program.Streaming.cs line 313

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 2: ReadTicks Helper (with auto-stop)");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Print explanation
        Console.WriteLine("📊 ReadTicks is a CONVENIENCE wrapper around OnSymbolTickAsync");
        Console.WriteLine("   Difference: Auto-stops after maxEvents OR durationSec");
        Console.WriteLine("   No need for manual CancellationToken management!\n");

        // Initialize counter for this example
        int readTicksCount = 0;

        // Start try block
        try
        {
            // Call ReadTicks() - simplified tick stream with auto-limits
            // Parameter 1: symbols[] - which symbols to monitor
            // Parameter 2: maxEvents - maximum number of ticks to receive
            // Parameter 3: durationSec - maximum seconds to run
            // Automatically stops when EITHER limit is reached
            // Returns IAsyncEnumerable<TickStreamData>
            // ⚠️  Runs continuously until limit reached!
            await foreach (var tick in service.ReadTicks(
                symbols: new[] { "EURUSD", "GBPUSD" },  // Symbols to monitor
                maxEvents: 10,                           // Stop after 10 ticks
                durationSec: 5))                         // Or after 5 seconds
            {
                // New tick arrived!

                // Increment counter
                readTicksCount++;

                // Extract tick data
                var t = tick.SymbolTick;

                // Get symbol, bid, ask (same as Example 1)
                var tickSymbol = t.Symbol;
                var bid = t.Bid;
                var ask = t.Ask;

                // Get time
                var time = DateTimeOffset.FromUnixTimeMilliseconds(t.TimeMsc).DateTime;

                // Print tick - WITHOUT this, INVISIBLE!
                Console.WriteLine($"[ReadTicks] #{readTicksCount}: {tickSymbol,-7} | Bid: {bid:F5} | Ask: {ask:F5} | {time:HH:mm:ss.fff}");

                // No need for manual break - ReadTicks stops automatically!
            }

            // Loop ended - limit reached automatically
            Console.WriteLine($"[ReadTicks] Completed with {readTicksCount} ticks");
        }
        catch (Exception ex)
        {
            // Error occurred
            Console.WriteLine($"[ReadTicks] Error: {ex.Message}");
        }

        // Print blank line
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 3: ReadTrades - Convenience helper for trade events
        // ═══════════════════════════════════════════════════════════════════════
        // Copied from: Examples/LowLevel/Program.Streaming.cs line 337

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 3: ReadTrades Helper (monitors your trades)");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Print explanation
        Console.WriteLine("🔔 ReadTrades monitors YOUR trading activity");
        Console.WriteLine("   Triggers when YOU open or close positions");
        Console.WriteLine("   Will wait 5 seconds for trading activity...\n");

        // Print note
        Console.WriteLine("⚠️  If you don't trade during these 5 seconds, stream will complete with 0 events");
        Console.WriteLine("   This is NORMAL - stream waits for YOUR trading activity\n");

        // Initialize counter
        int tradesCount = 0;

        // Start try block
        try
        {
            // Call ReadTrades() - monitors trade execution events
            // Parameter 1: maxEvents - stop after this many trade events
            // Parameter 2: durationSec - or stop after this many seconds
            // Automatically stops when either limit reached
            // Returns IAsyncEnumerable<TradeStreamDataOneof>
            // ⚠️  Only fires when YOU open/close positions!
            await foreach (var trade in service.ReadTrades(
                maxEvents: 10,      // Stop after 10 trade events
                durationSec: 5))    // Or after 5 seconds
            {
                // A trade event occurred!
                // This means YOU just opened or closed a position

                // Increment counter
                tradesCount++;

                // Get the type name of the trade event
                // Type: string
                // Examples: "MqlTradeResult", "TradeTransaction", etc.
                var eventType = trade.GetType().Name;

                // Print event - WITHOUT this, INVISIBLE!
                Console.WriteLine($"[ReadTrades] Event #{tradesCount}: {eventType}");

                // You could extract more details from 'trade' object here
                // For example: trade.Symbol, trade.Profit, etc.
                // (depends on the event type)
            }

            // Loop ended
            Console.WriteLine($"[ReadTrades] Completed with {tradesCount} trade events");
        }
        catch (OperationCanceledException)
        {
            // Stream cancelled (timeout - normal if no trading)
            if (tradesCount > 0)
            {
                Console.WriteLine($"[ReadTrades] Completed with {tradesCount} events");
            }
            else
            {
                Console.WriteLine("[ReadTrades] No trades detected (this is normal if you didn't trade)");
            }
        }
        catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.Cancelled)
        {
            // Stream cancelled by server - normal when no trades occur
            Console.WriteLine("[ReadTrades] Stream completed (no trading activity)");
        }
        catch (Exception ex)
        {
            // Real error (not cancellation)
            // Only print if not a "Cancelled" message
            if (!ex.Message.Contains("Cancelled") && !ex.Message.Contains("canceled"))
            {
                Console.WriteLine($"[ReadTrades] Error: {ex.Message}");
            }
            else
            {
                Console.WriteLine("[ReadTrades] Stream completed (no trading activity)");
            }
        }

        // Print blank line
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════════════════
        // SUMMARY
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("SUMMARY - What We Did:");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("✓ Monitored real-time price ticks (OnSymbolTickAsync)");
        Console.WriteLine("✓ Used ReadTicks helper with auto-stop");
        Console.WriteLine("✓ Used ReadTrades helper to monitor trade events");
        Console.WriteLine();

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("KEY TAKEAWAYS:");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("1. Streams run CONTINUOUSLY until stopped");
        Console.WriteLine("2. Use 'await foreach' to process each event");
        Console.WriteLine("3. OnSymbolTickAsync = manual control with CancellationToken");
        Console.WriteLine("4. ReadTicks/ReadTrades = auto-stop with maxEvents/durationSec");
        Console.WriteLine("5. ALWAYS use Console.WriteLine to see stream data!");
        Console.WriteLine("6. OperationCanceledException is NORMAL for streams");
        Console.WriteLine("7. Trade streams fire only when YOU trade");
        Console.WriteLine("8. Tick streams fire continuously as prices change");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("⚠️  CRITICAL REMINDER:");
        Console.WriteLine("   await foreach (var tick in stream)  // Receives data");
        Console.WriteLine("   {");
        Console.WriteLine("       // WITHOUT Console.WriteLine, tick is INVISIBLE!");
        Console.WriteLine("       Console.WriteLine(tick.Bid);    // ✅ Now visible");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("✓ All streaming examples completed!");
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// STREAMS DEMONSTRATED (copied from working examples):
//
// 1. OnSymbolTickAsync(symbols[], cancellationToken)
//    - Real-time price ticks for specified symbols
//    - Runs continuously until cancelled or break
//    - Source: Examples/LowLevel/Program.Streaming.cs line 378
//
// 2. ReadTicks(symbols[], maxEvents, durationSec)
//    - Convenience wrapper with auto-stop limits
//    - Automatically stops after maxEvents or durationSec
//    - Source: Examples/LowLevel/Program.Streaming.cs line 313
//
// 3. ReadTrades(maxEvents, durationSec)
//    - Monitors trade execution events
//    - Automatically stops after maxEvents or durationSec
//    - Only fires when YOU open/close positions
//    - Source: Examples/LowLevel/Program.Streaming.cs line 337
//
// ALL EXAMPLES ARE WORKING CODE - COPIED FROM PRODUCTION EXAMPLES!
// ══════════════════════════════════════════════════════════════════════════════
