/*══════════════════════════════════════════════════════════════════════════════
 FILE: Examples/LowLevel/Program.Streaming.cs - STREAMING SUBSCRIPTIONS DEMO
 PURPOSE:
   Comprehensive demonstration of ALL real-time streaming capabilities in the
   MT5 gRPC API. Shows how to subscribe to live market data, trade events,
   transaction notifications, and position updates using async streaming.

 🎯 WHO SHOULD USE THIS:
   • Developers building real-time trading applications
   • Algorithmic traders needing live price feeds
   • Applications monitoring trade execution in real-time
   • Systems requiring position P/L updates
   • Anyone building event-driven trading systems

 📡 WHAT IS STREAMING:
   Streaming = Real-time data subscriptions via gRPC server streaming
   Instead of polling (repeated requests), you subscribe once and receive
   continuous updates as they happen on the server.

   Benefits:
   ✓ Low latency - Events arrive immediately when they happen
   ✓ Efficient - No repeated polling overhead
   ✓ Real-time - Perfect for live trading and monitoring
   ✓ Async - Non-blocking, multiple streams can run in parallel

 📚 WHAT THIS DEMO COVERS (7 Streaming Methods):

   PART 1: BASE STREAMS (MT5Service) - Running in parallel:
   ─────────────────────────────────────────────────────────────────────────
   1. 📊 OnSymbolTickAsync(symbols[], ct)
      • Real-time price ticks for specified symbols
      • Example: EURUSD, GBPUSD tick-by-tick data
      • Use case: Price monitoring, tick charts, high-frequency strategies

   2. 🔔 OnTradeAsync(ct)
      • Trade execution and closure events
      • Notifies when orders are filled or positions closed
      • Use case: Track order execution, monitor trade lifecycle

   3. 🔄 OnTradeTransactionAsync(ct)
      • Transaction events (complete order lifecycle)
      • More detailed than OnTradeAsync, includes all state changes
      • Use case: Order state tracking, detailed execution history

   4. 💰 OnPositionProfitAsync(intervalMs, ignoreEmpty, ct)
      • Periodic snapshots of position P/L
      • Poll-based stream with configurable interval (default: 1000ms)
      • Use case: Real-time P/L monitoring, drawdown alerts

   5. 🎫 OnPositionsAndPendingOrdersTicketsAsync(intervalMs, ct)
      • Periodic snapshots of all position and pending order tickets
      • Poll-based stream with configurable interval (default: 1000ms)
      • Use case: Monitor active positions and pending orders

   PART 2: EXTENSION HELPERS (MT5Service.Extensions) - Sequential:
   ─────────────────────────────────────────────────────────────────────────
   6. 🍬 ReadTicks(symbols[], maxEvents, durationSec)
      • Convenience wrapper with built-in limits
      • Automatically stops after maxEvents or durationSec
      • Use case: Quick tick sampling, testing, demos

   7. 🍬 ReadTrades(maxEvents, durationSec)
      • Convenience wrapper with built-in limits
      • Automatically stops after maxEvents or durationSec
      • Use case: Quick trade monitoring, testing, demos

 ⚠️  IMPORTANT - STREAMING BEST PRACTICES:
   • Always use CancellationToken to stop streams gracefully
   • Handle OperationCanceledException (normal when stream stops)
   • Use try-catch around streams to handle connection errors
   • Limit event processing time to avoid blocking stream
   • Run multiple streams in parallel using Task.WhenAll()
   • Filter "Cancelled" exceptions - they're normal for trade streams with no activity

 📊 STREAM TYPES (FAST vs SLOW):
   This demo distinguishes between two types of streams:

   FAST STREAMS (High-frequency market data):
   • OnSymbolTickAsync - Price ticks arrive continuously (10+ per second)
   • Limited to 10 events for demo purposes
   • Quick completion (~1 second)

   SLOW STREAMS (User action dependent):
   • OnTradeAsync - Only fires when YOU open/close positions
   • OnTradeTransactionAsync - Only fires on YOUR order lifecycle events
   • OnPositionProfitAsync - Fires only if you have open positions
   • OnPositionsAndPendingOrdersTicketsAsync - Periodic snapshots
   • Run full timeout (5 seconds) waiting for your trading activity
   • Normal to complete with 0 events if no trading occurs

 🔄 STREAMING LIFECYCLE:
   Phase 1: SUBSCRIPTION
     → Call stream method (OnSymbolTickAsync, OnTradeAsync, etc.)
     → Server begins sending events

   Phase 2: EVENT PROCESSING
     → Process events in await foreach loop
     → Events arrive in real-time as they happen
     → Non-blocking - other streams continue in parallel

   Phase 3: TERMINATION
     → Stop via CancellationToken.Cancel()
     → Or break from loop when condition met
     → Or timeout via CancelAfter()
     → Always handle OperationCanceledException

 💡 WHEN TO USE STREAMING:
   • Real-time price monitoring (use OnSymbolTickAsync)
   • Trade execution notifications (use OnTradeAsync)
   • Order state tracking (use OnTradeTransactionAsync)
   • Live P/L monitoring (use OnPositionProfitAsync)
   • Position/order snapshots (use OnPositionsAndPendingOrdersTicketsAsync)
   • Event-driven strategies that react to market/trade events

 USAGE:
   dotnet run 3
   dotnet run streaming
   dotnet run streams
══════════════════════════════════════════════════════════════════════════════*/

using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace MetaRPC.CSharpMT5.Examples.LowLevel
{
    public static partial class Program
    {
        private const int MAX_TICK_EVENTS = 10;      // Fast stream - ticks only
        private const int MAX_TRADE_EVENTS = 100;    // Slow stream - trading events (usually not reached)
        private const int TIMEOUT_SECONDS = 5;

        // ═════════════════════════════════════════════════════════════════
        // MAIN ENTRY POINT
        // ═════════════════════════════════════════════════════════════════

        public static async Task RunStreamingAsync()
        {
            PrintStreamingBanner();

            try
            {
                // ─── [01] LOAD CONFIGURATION ─────────────────────────────
                var config = ConnectionHelper.BuildConfiguration();

                // ─── [02] CREATE & CONNECT ACCOUNT ───────────────────────
                var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);

                // ─── [03] WRAP WITH MT5SERVICE ───────────────────────────
                ConsoleHelper.PrintInfo("\n→ Creating MT5Service wrapper...");
                var service = new MT5Service(account);
                ConsoleHelper.PrintSuccess("✓ MT5Service wrapper created!");

                // ─── [04] RUN ALL STREAMS IN PARALLEL ────────────────────
                // ⚠️ IMPORTANT: Streams are automatically cleaned up when:
                //    1. CancellationToken fires (timeout or manual cancel)
                //    2. await foreach loop breaks (event limit reached)
                //    3. Task.WhenAll completes (all streams finished)
                //    → Each stream calls stream?.Dispose() in finally block
                await RunAllStreamsAsync(service);

                // ─── [05] CLEANUP ─────────────────────────────────────────
                // Note: MT5Account doesn't implement IDisposable, but gRPC
                // channels are managed internally. Connection stays open for
                // potential reuse. If you need explicit cleanup, call Disconnect.
                // For production: Consider adding explicit Disconnect() or
                // implementing IDisposable pattern in MT5Account class.
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    ConsoleHelper.PrintError($"Inner: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // RUN ALL STREAMS IN PARALLEL
        // ═════════════════════════════════════════════════════════════════

        private static async Task RunAllStreamsAsync(MT5Service svc)
        {
            // ══════════════════════════════════════════════════════════════
            // PART 1: BASE STREAMS (MT5Service)
            //    Real-time data subscriptions running in parallel.
            //    Subscribe to price ticks, trade events, transactions,
            //    position P/L updates, and active tickets monitoring.
            //    All streams run concurrently for maximum efficiency.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("PART 1: BASE STREAMS (MT5Service)");
            ConsoleHelper.PrintInfo($"Tick stream: {MAX_TICK_EVENTS} events limit (fast)");
            ConsoleHelper.PrintInfo($"Trade streams: {TIMEOUT_SECONDS} seconds timeout (wait for your trades)");
            ConsoleHelper.PrintInfo("Press Ctrl+C to stop all streams early\n");

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(TIMEOUT_SECONDS + 2)); // Extra buffer

            // Start all streams in parallel
            var tasks = new List<Task>
            {
                RunTickStream(svc, cts.Token),
                RunTradeStream(svc, cts.Token),
                RunTransactionStream(svc, cts.Token),
                RunProfitStream(svc, cts.Token),
                RunTicketsStream(svc, cts.Token)
            };

            var sw = Stopwatch.StartNew();

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // Expected - timeout or manual cancellation
            }
            catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.Cancelled)
            {
                // Expected - stream was cancelled (no events received)
            }

            sw.Stop();

            ConsoleHelper.PrintSection("PART 1 COMPLETED");
            ConsoleHelper.PrintSuccess($"All base streams completed in {sw.Elapsed.TotalSeconds:F1} seconds\n");

            // ══════════════════════════════════════════════════════════════
            // PART 2: EXTENSION HELPERS (ReadTicks, ReadTrades)
            //    Convenience wrappers for quick streaming with built-in limits.
            //    Automatically stop after reaching max events or timeout.
            //    Perfect for testing, sampling, and quick data collection.
            // ══════════════════════════════════════════════════════════════

            ConsoleHelper.PrintSection("PART 2: EXTENSION HELPERS (MT5Service.Extensions)");
            ConsoleHelper.PrintInfo("These are convenience wrappers with built-in limits\n");

            await RunExtensionHelpersAsync(svc);

            ConsoleHelper.PrintSection("ALL STREAMING DEMOS COMPLETED");
            ConsoleHelper.PrintSuccess("✓ All 5 base streams + 2 extension helpers demonstrated!");
        }

        // ═════════════════════════════════════════════════════════════════
        // RUN EXTENSION HELPERS
        // ═════════════════════════════════════════════════════════════════

        private static async Task RunExtensionHelpersAsync(MT5Service svc)
        {
            // ReadTicks - Convenience wrapper with limits
            Console.WriteLine("[ReadTicks] Starting ReadTicks() extension...");
            Console.WriteLine($"            Limit: {MAX_TICK_EVENTS} events OR {TIMEOUT_SECONDS} seconds\n");

            int tickCount = 0;
            try
            {
                await foreach (var tick in svc.ReadTicks(
                    symbols: new[] { "EURUSD", "GBPUSD" },
                    maxEvents: MAX_TICK_EVENTS,
                    durationSec: TIMEOUT_SECONDS))
                {
                    tickCount++;
                    var t = tick.SymbolTick;
                    var time = DateTimeOffset.FromUnixTimeMilliseconds(t.TimeMsc).DateTime;
                    Console.WriteLine($"[ReadTicks] #{tickCount}: {t.Symbol,-7} | Bid: {t.Bid:F5} | Ask: {t.Ask:F5} | {time:HH:mm:ss.fff}");
                }
                Console.WriteLine($"[ReadTicks] Completed with {tickCount} events\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReadTicks] Error: {ex.Message}\n");
            }

            // ReadTrades - Convenience wrapper with limits
            Console.WriteLine("[ReadTrades] Starting ReadTrades() extension...");
            Console.WriteLine($"             Limit: {MAX_TICK_EVENTS} events OR {TIMEOUT_SECONDS} seconds\n");

            int tradeCount = 0;
            try
            {
                await foreach (var trade in svc.ReadTrades(
                    maxEvents: MAX_TICK_EVENTS,
                    durationSec: TIMEOUT_SECONDS))
                {
                    tradeCount++;
                    Console.WriteLine($"[ReadTrades] Event #{tradeCount}: {trade.GetType().Name}");
                }
                Console.WriteLine($"[ReadTrades] Completed with {tradeCount} events\n");
            }
            catch (OperationCanceledException)
            {
                if (tradeCount > 0)
                    Console.WriteLine($"[ReadTrades] Completed with {tradeCount} events\n");
                // If count == 0, it's okay - there simply was no trading
            }
            catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.Cancelled)
            {
                //As expected - the stream was cancelled, there was no trading
            }
            catch (Exception ex)
            {
                // We only show real errors, not cancelled ones
                if (!ex.Message.Contains("Cancelled") && !ex.Message.Contains("canceled"))
                    Console.WriteLine($"[ReadTrades] Error: {ex.Message}\n");
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // STREAM 1: TICK DATA (FAST STREAM)
        // ═════════════════════════════════════════════════════════════════
        // 📊 OnSymbolTickAsync() - Real-time price tick stream
        //
        // TYPE: FAST STREAM (high-frequency market data)
        // TRIGGERS: Continuously as market prices change (10+ ticks/second)
        // STOPS: When event limit reached (MAX_TICK_EVENTS) or timeout
        //
        // RESOURCE CLEANUP:
        // ✅ Stream automatically disposed in finally block (MT5Account.cs:918)
        // ✅ break statement triggers IAsyncEnumerable cleanup
        // ✅ CancellationToken propagates to gRPC layer

        private static async Task RunTickStream(MT5Service svc, CancellationToken ct)
        {
            var symbols = new[] { "EURUSD", "GBPUSD" };
            Console.WriteLine($"[TICK] Starting tick stream for {string.Join(", ", symbols)}...");

            int count = 0;

            try
            {
                // Subscribe to real-time tick updates for multiple symbols
                await foreach (var tick in svc.OnSymbolTickAsync(symbols, ct))
                {
                    count++;
                    var t = tick.SymbolTick;
                    var time = DateTimeOffset.FromUnixTimeMilliseconds(t.TimeMsc).DateTime;
                    Console.WriteLine($"[TICK] #{count}: {t.Symbol,-7} | Bid: {t.Bid:F5} | Ask: {t.Ask:F5} | Time: {time:HH:mm:ss.fff}");

                    // Stop after receiving enough events (demo purposes)
                    if (count >= MAX_TICK_EVENTS)
                    {
                        Console.WriteLine($"[TICK] Reached {MAX_TICK_EVENTS} events limit");
                        break;  // ← Triggers stream cleanup via IAsyncEnumerable.DisposeAsync()
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[TICK] Cancelled after {count} events");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TICK] Error: {ex.Message}");
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // STREAM 2: TRADE EVENTS
        // ═════════════════════════════════════════════════════════════════

        private static async Task RunTradeStream(MT5Service svc, CancellationToken ct)
        {
            Console.WriteLine("[TRADE] Starting trade events stream...");

            int count = 0;

            try
            {
                await foreach (var trade in svc.OnTradeAsync(ct))
                {
                    Console.WriteLine($"[TRADE] Event #{++count}: {trade.GetType().Name} received");
                    // No break - works for the whole timeout, waits for your trades
                }
            }
            catch (OperationCanceledException)
            {
                if (count > 0)
                    Console.WriteLine($"[TRADE] Stopped after {count} events");
                else
                    Console.WriteLine($"[TRADE] No trade events (open/close a position to see events)");
            }
            catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.Cancelled)
            {
                if (count == 0)
                    Console.WriteLine($"[TRADE] No trade events (open/close a position to see events)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TRADE] Error: {ex.Message}");
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // STREAM 3: TRANSACTION EVENTS
        // ═════════════════════════════════════════════════════════════════

        private static async Task RunTransactionStream(MT5Service svc, CancellationToken ct)
        {
            Console.WriteLine("[TX] Starting transaction events stream...");

            int count = 0;

            try
            {
                await foreach (var tx in svc.OnTradeTransactionAsync(ct))
                {
                    Console.WriteLine($"[TX] Event #{++count}: {tx.GetType().Name} received");
                }
            }
            catch (OperationCanceledException)
            {
                if (count > 0)
                    Console.WriteLine($"[TX] Stopped after {count} events");
                else
                    Console.WriteLine($"[TX] No transaction events (create/modify/cancel orders to see events)");
            }
            catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.Cancelled)
            {
                if (count == 0)
                    Console.WriteLine($"[TX] No transaction events (create/modify/cancel orders to see events)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TX] Error: {ex.Message}");
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // STREAM 4: POSITION PROFIT
        // ═════════════════════════════════════════════════════════════════

        private static async Task RunProfitStream(MT5Service svc, CancellationToken ct)
        {
            Console.WriteLine("[P/L] Starting position profit stream (1000ms interval)...");

            int count = 0;

            try
            {
                await foreach (var profit in svc.OnPositionProfitAsync(
                    intervalMs: 1000,
                    ignoreEmpty: true,
                    ct))
                {
                    Console.WriteLine($"[P/L] Event #{++count}: {profit.GetType().Name} received");
                }
            }
            catch (OperationCanceledException)
            {
                if (count > 0)
                    Console.WriteLine($"[P/L] Stopped after {count} events");
                else
                    Console.WriteLine($"[P/L] No profit events (open positions to see P/L updates)");
            }
            catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.Cancelled)
            {
                if (count == 0)
                    Console.WriteLine($"[P/L] No profit events (open positions to see P/L updates)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[P/L] Error: {ex.Message}");
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // STREAM 5: TICKETS SNAPSHOT
        // ═════════════════════════════════════════════════════════════════

        private static async Task RunTicketsStream(MT5Service svc, CancellationToken ct)
        {
            Console.WriteLine("[TICKETS] Starting tickets snapshot stream (1000ms interval)...");

            int count = 0;

            try
            {
                await foreach (var snapshot in svc.OnPositionsAndPendingOrdersTicketsAsync(
                    intervalMs: 1000,
                    ct))
                {
                    Console.WriteLine($"[TICKETS] Event #{++count}: {snapshot.GetType().Name} received");
                }
            }
            catch (OperationCanceledException)
            {
                if (count > 0)
                {
                    Console.WriteLine($"[TICKETS] Stopped after {count} events");
                }
            }
            catch (Exception ex) when (ex is not Grpc.Core.RpcException rpcEx || rpcEx.StatusCode != Grpc.Core.StatusCode.Cancelled)
            {
                Console.WriteLine($"[TICKETS] Error: {ex.Message}");
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // BANNER
        // ═════════════════════════════════════════════════════════════════

        private static void PrintStreamingBanner()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║              STREAMING SUBSCRIPTIONS - LIVE DEMO                 ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║  PART 1: Base Streams (MT5Service) - 5 streams in parallel:      ║");
            Console.WriteLine("║    [TICK]    → Real-time price ticks (EURUSD, GBPUSD)            ║");
            Console.WriteLine("║    [TRADE]   → Trade execution/closure events                    ║");
            Console.WriteLine("║    [TX]      → Transaction events (order lifecycle)              ║");
            Console.WriteLine("║    [P/L]     → Position profit snapshots (1s interval)           ║");
            Console.WriteLine("║    [TICKETS] → Ticket snapshots (1s interval)                    ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║  PART 2: Extension Helpers (MT5Service.Extensions) - sequential: ║");
            Console.WriteLine("║    [ReadTicks]  → Convenience wrapper with built-in limits       ║");
            Console.WriteLine("║    [ReadTrades] → Convenience wrapper with built-in limits       ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine($"║  Limits: {TIMEOUT_SECONDS} seconds OR {MAX_TICK_EVENTS} events per stream                     ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }

    }
}

/*══════════════════════════════════════════════════════════════════════════════
 =============================================================================
       STREAMING REFERENCE GUIDE - Resource Management & Troubleshooting
 =============================================================================

  +-----------------------------------------------------------------------------+
  |                RESOURCE CLEANUP - How streams are disposed                  |
  +-----------------------------------------------------------------------------+

 Q: Are streaming subscriptions automatically cleaned up?
 A: YES! Here's how resource cleanup works:

 1. STREAM DISPOSAL (MT5Account.cs:916-919):
    - Every stream call wraps gRPC AsyncServerStreamingCall in try-finally
    - finally { stream?.Dispose(); } ALWAYS executes
    - Even if exception occurs, stream is cleaned up

 2. WHEN STREAMS ARE DISPOSED:
    - When await foreach breaks (event limit reached)
    - When CancellationToken fires (timeout or manual cancel)
    - When IAsyncEnumerable.DisposeAsync() is called
    - When Task.WhenAll completes (all tasks finished)
    - When exception throws out of stream processing

 3. WHAT GETS CLEANED UP:
    [YES] gRPC server streaming call (AsyncServerStreamingCall<T>)
    [YES] Response stream reader (IAsyncStreamReader<T>)
    [YES] Network resources associated with stream
    [NO]  MT5Account object itself (not IDisposable)
    [NO]  gRPC channel (managed internally, stays open for reuse)

 Example of automatic cleanup:
   await foreach (var tick in svc.OnSymbolTickAsync(symbols, ct))
   {
       if (count >= 10) break;  // Triggers cleanup automatically
   }
   // Stream is disposed here, even if you don't explicitly call anything


 +-----------------------------------------------------------------------------+
 |              FAST vs SLOW STREAMS - Why you may see 0 events                |
 +-----------------------------------------------------------------------------+

 FAST STREAMS (Market Data):
   OnSymbolTickAsync()
      - Fires continuously as market prices change
      - Expect 10+ events per second during active trading
      - Will ALWAYS produce events (market never stops ticking)
      - Demo limits to 10 events to avoid flooding console

 SLOW STREAMS (User Action Dependent):
   OnTradeAsync()
      - Fires ONLY when YOU open/close positions manually
      - 0 events is NORMAL if you don't trade during demo
      - To see events: Open a position in MT5 terminal while demo runs

   OnTradeTransactionAsync()
      - Fires on ANY order lifecycle event (create/modify/cancel)
      - 0 events is NORMAL if you don't interact with orders
      - To see events: Place/modify/cancel orders while demo runs

   OnPositionProfitAsync(intervalMs: 1000)
      - Polls position P/L every 1 second (poll-based, not event-driven)
      - Fires ONLY if you have open positions
      - 0 events is NORMAL if account has no positions
      - To see events: Open a position before running demo

   OnPositionsAndPendingOrdersTicketsAsync(intervalMs: 1000)
      - Polls tickets every 1 second
      - Always fires (even with empty positions/orders)
      - Returns snapshot of current state


 +-----------------------------------------------------------------------------+
 |            COMMON MISTAKES - Resource leaks and how to avoid them           |
 +-----------------------------------------------------------------------------+

 1. [BAD] MISTAKE: Fire-and-forget stream tasks
    var task = RunTickStream(svc, ct); // Started but never awaited
    await SomethingElse();              // Do other work
    // Result: Stream keeps running in background, may leak resources

    [GOOD] FIX: Always await Task.WhenAll
    await Task.WhenAll(
        RunTickStream(svc, ct),
        RunTradeStream(svc, ct)
    );
    // Result: All streams complete before proceeding

 2. [BAD] MISTAKE: Ignoring CancellationToken
    await foreach (var tick in svc.OnSymbolTickAsync(symbols)) // No ct!
    {
        // Process forever...
    }
    // Result: Stream never stops, runs until process terminates

    [GOOD] FIX: Always pass CancellationToken
    var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(10));
    await foreach (var tick in svc.OnSymbolTickAsync(symbols, cts.Token))
    {
        // Automatically stops after 10 seconds
    }

 3. [BAD] MISTAKE: Long-running processing blocks stream
    await foreach (var tick in svc.OnSymbolTickAsync(symbols, ct))
    {
        await Task.Delay(5000); // Block for 5 seconds!
        // Result: Server keeps sending events but they queue up
        // Result: May cause backpressure or disconnection
    }

    [GOOD] FIX: Process quickly or offload to background
    await foreach (var tick in svc.OnSymbolTickAsync(symbols, ct))
    {
        _ = Task.Run(() => ProcessTickAsync(tick)); // Fire-and-forget processing
        // Or use Channel<T> for buffering
    }

 4. [WARNING] LIMITATION: MT5Account doesn't implement IDisposable
    var account = await MT5Account.ConnectAsync(...);
    // Use account...
    // No Dispose() method available!

    CURRENT BEHAVIOR:
    - gRPC channels managed internally
    - Connection stays open for potential reuse
    - GC will eventually clean up when account goes out of scope

    RECOMMENDATION FOR PRODUCTION:
    - Consider implementing IDisposable in MT5Account class
    - Or explicitly call Disconnect() when done
    - Or use 'using var account = ...' pattern if IDisposable added


 +-----------------------------------------------------------------------------+
 |                 TROUBLESHOOTING - Stream errors and solutions               |
 +-----------------------------------------------------------------------------+

 ERROR: "Grpc.Core.RpcException: Status(StatusCode="Cancelled")"
 CAUSE: Stream was cancelled (normal behavior)
 FIX: Catch RpcException with StatusCode.Cancelled - this is expected
      catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled) { }

 ERROR: "Stream completed with 0 events"
 CAUSE: SLOW stream waiting for user action that never happened
 FIX: This is NORMAL! Trade/Transaction/Profit streams need trading activity
      Open positions manually in MT5 terminal to see events

 ERROR: "Operation was canceled"
 CAUSE: CancellationToken timeout reached
 FIX: Expected behavior when CancelAfter() timeout fires
      catch (OperationCanceledException) { /* Normal *

 ERROR: "TERMINAL_INSTANCE_NOT_FOUND" or "TERMINAL_REGISTRY_TERMINAL_NOT_FOUND"
 CAUSE: MT5 terminal disconnected or restarted
 FIX: Auto-reconnect is built-in (see MT5Account.cs line 897-906)
      Stream automatically retries with 500ms delay

 ERROR: Multiple streams interfere with each other
 CAUSE: Shared resources or improper parallel execution
 FIX: Always run streams with Task.WhenAll, not sequentially
      Correct pattern: await Task.WhenAll(task1, task2, task3);
      Wrong pattern: await task1; await task2; await task3;


 +-----------------------------------------------------------------------------+
 |                 BEST PRACTICES - Production streaming patterns              |
 +-----------------------------------------------------------------------------+

 1. Use CancellationTokenSource with timeout
    var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(30));
    await ProcessStreamsAsync(service, cts.Token);

 2. Run multiple streams in parallel
    await Task.WhenAll(
        MonitorTicks(service, ct),
        MonitorTrades(service, ct),
        MonitorProfit(service, ct)
    );

 3. Handle both cancellation exceptions
     try
    {
        await foreach (var data in stream) { }
    }
    catch (OperationCanceledException) { /* Timeout  
    catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled) { /* Normal 

 4. Use ReadTicks/ReadTrades helpers for quick sampling
    await foreach (var tick in svc.ReadTicks(symbols, maxEvents: 100, durationSec: 30))
    {
        // Automatically stops after 100 events OR 30 seconds
    }

 5. For long-running streams, add reconnection logic
    while (!ct.IsCancellationRequested)
    {
        try
        {
            await foreach (var data in stream)
            {
                // Process data
            }
        }
        catch (RpcException) when (!ct.IsCancellationRequested)
        {
            await Task.Delay(1000); // Backoff before retry
        }
    }

══════════════════════════════════════════════════════════════════════════════*/