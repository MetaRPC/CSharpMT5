// ══════════════════════════════════════════════════════════════════════════════
// FILE: 15_Streaming_Explained.cs
// PURPOSE: Streaming subscriptions - Real-time data from MT5
//
// Topics covered:
//   1. WHAT is streaming vs polling
//   2. HOW to subscribe to tick data (price updates)
//   3. HOW to subscribe to trade events
//   4. HOW to subscribe to position profit changes
//   5. HOW to unsubscribe properly
//   6. WHEN to use streaming vs polling
//
// Key principle: Streaming = MT5 PUSHES data to you automatically
// Polling = YOU ASK MT5 for data repeatedly
//
// Streaming is MORE EFFICIENT for real-time monitoring!
//
// ══════════════════════════════════════════════════════════════════════════════

using System;
using System.Threading;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;
using mt5_term_api;

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

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   STREAMING - Real-Time Data from MT5                     ║");
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
        var sugar = new MT5Sugar(service);

        // Define symbol for examples
        string symbol = "EURUSD";

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 1: STREAMING vs POLLING
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 1: Understanding Streaming vs Polling");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 TWO WAYS TO GET REAL-TIME DATA:\n");

        Console.WriteLine("📊 POLLING (requesting repeatedly):");
        Console.WriteLine(@"
   while (true)
   {
       var tick = await service.SymbolInfoTickAsync(symbol);
       Console.WriteLine($""Price: {tick.Bid:F5}"");
       await Task.Delay(1000);  // Wait 1 second
   }
");
        Console.WriteLine("   How it works:");
        Console.WriteLine("   - YOU ask MT5 for data every second");
        Console.WriteLine("   - MT5 responds with current value");
        Console.WriteLine("   - Repeat forever\n");

        Console.WriteLine("   ❌ PROBLEMS:");
        Console.WriteLine("   - Wastes resources (many requests)");
        Console.WriteLine("   - Delays (1 second between updates)");
        Console.WriteLine("   - Might miss fast price changes");
        Console.WriteLine("   - Network overhead\n");

        Console.WriteLine("📡 STREAMING (subscription):");
        Console.WriteLine(@"
   void OnTick(Tick tick)
   {
       Console.WriteLine($""Price: {tick.Bid:F5}"");
   }

   await account.SubscribeToTicksAsync(symbol, OnTick);
   // Now MT5 will AUTOMATICALLY call OnTick() on every price change!
");
        Console.WriteLine("   How it works:");
        Console.WriteLine("   - MT5 PUSHES data to you when it changes");
        Console.WriteLine("   - You get updates IMMEDIATELY");
        Console.WriteLine("   - No polling loop needed\n");

        Console.WriteLine("   ✅ BENEFITS:");
        Console.WriteLine("   - More efficient (no wasted requests)");
        Console.WriteLine("   - Instant updates (no delay)");
        Console.WriteLine("   - Never miss changes");
        Console.WriteLine("   - Less network traffic\n");

        Console.WriteLine("💡 CONCLUSION:");
        Console.WriteLine("   Use STREAMING for real-time monitoring");
        Console.WriteLine("   Use POLLING for occasional checks\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 2: SUBSCRIBING TO TICK DATA
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 2: Subscribe to Tick Data (Price Updates)");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 TICK SUBSCRIPTION:");
        Console.WriteLine("   Get notified EVERY TIME price changes\n");

        Console.WriteLine("✅ IMPLEMENTATION:\n");

        // Counter for demonstration
        int tickCount = 0;
        DateTime startTime = DateTime.UtcNow;

        Console.WriteLine($"📞 Subscribing to {symbol} ticks for 10 seconds...\n");

        // Create cancellation token for stopping after 10 seconds
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        try
        {
            // Subscribe to ticks
            // This creates a background stream that calls our callback
            var tickStream = account.SubscribeToTicksAsync(
                new SymbolRequest { Symbol = symbol },
                cancellationToken: cts.Token
            );

            Console.WriteLine("📡 Streaming started... Press Ctrl+C to stop\n");
            Console.WriteLine("   Time     │  Bid      │  Ask      │  Last     │  Spread");
            Console.WriteLine("   ─────────┼───────────┼───────────┼───────────┼─────────");

            // Read ticks from stream
            await foreach (var tick in tickStream.WithCancellation(cts.Token))
            {
                tickCount++;

                // Calculate spread in points
                double point = await sugar.GetPointAsync(symbol);
                double spread = (tick.Ask - tick.Bid) / point;

                // Format time
                var time = DateTime.Now.ToString("HH:mm:ss");

                // Display tick
                Console.WriteLine($"   {time} │ {tick.Bid:F5} │ {tick.Ask:F5} │ {tick.Last:F5} │ {spread:F1} pts");

                // Only show first 20 ticks for demonstration
                if (tickCount >= 20)
                {
                    Console.WriteLine("\n   (Showing first 20 ticks only...)\n");
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n⚠️  Error: {ex.Message}\n");
        }

        var duration = DateTime.UtcNow - startTime;
        Console.WriteLine($"📊 STATISTICS:");
        Console.WriteLine($"   Duration: {duration.TotalSeconds:F1} seconds");
        Console.WriteLine($"   Ticks received: {tickCount}");
        Console.WriteLine($"   Average rate: {tickCount / duration.TotalSeconds:F1} ticks/second\n");

        Console.WriteLine("💡 WHAT HAPPENED:");
        Console.WriteLine("   - MT5 pushed EVERY price change to us");
        Console.WriteLine("   - No polling loop needed");
        Console.WriteLine("   - Updates were INSTANT");
        Console.WriteLine("   - Stream stopped when token was canceled\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 3: SUBSCRIBING TO TRADE EVENTS
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 3: Subscribe to Trade Events");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 TRADE EVENT SUBSCRIPTION:");
        Console.WriteLine("   Get notified when:");
        Console.WriteLine("   - Order placed");
        Console.WriteLine("   - Order executed");
        Console.WriteLine("   - Position opened");
        Console.WriteLine("   - Position modified");
        Console.WriteLine("   - Position closed\n");

        Console.WriteLine("✅ IMPLEMENTATION PATTERN:");
        Console.WriteLine(@"
   var cts = new CancellationTokenSource();

   var tradeStream = account.SubscribeToTradeTransactionAsync(
       cancellationToken: cts.Token
   );

   await foreach (var trade in tradeStream)
   {
       // trade.Type tells you what happened
       switch (trade.Type)
       {
           case 0:  // ORDER_TYPE_BUY
               Console.WriteLine($""BUY order placed: #{trade.Order}"");
               break;

           case 1:  // ORDER_TYPE_SELL
               Console.WriteLine($""SELL order placed: #{trade.Order}"");
               break;

           case 2:  // TRADE_TRANSACTION_DEAL_ADD
               Console.WriteLine($""Deal executed: #{trade.Deal}"");
               Console.WriteLine($""  Symbol: {trade.Symbol}"");
               Console.WriteLine($""  Volume: {trade.Volume}"");
               Console.WriteLine($""  Price: {trade.Price:F5}"");
               break;

           case 3:  // TRADE_TRANSACTION_DEAL_UPDATE
               Console.WriteLine($""Deal updated: #{trade.Deal}"");
               break;
       }
   }
");

        Console.WriteLine("⚠️  NOT EXECUTING (demonstration only)");
        Console.WriteLine("   Requires active trading to generate events\n");

        Console.WriteLine("💡 USE CASES:");
        Console.WriteLine("   - Monitor all account activity");
        Console.WriteLine("   - Log every trade for audit");
        Console.WriteLine("   - Alert on position changes");
        Console.WriteLine("   - Track order execution quality\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 4: SUBSCRIBING TO POSITION PROFIT
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 4: Subscribe to Position Profit Changes");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 PROFIT SUBSCRIPTION:");
        Console.WriteLine("   Get notified when position profit changes");
        Console.WriteLine("   Useful for trailing stops, alerts, etc.\n");

        // Check if there are any positions
        var positions = await service.PositionsAsync();

        if (positions.Count > 0)
        {
            var pos = positions[0];

            Console.WriteLine($"📊 Monitoring position #{pos.Ticket}:");
            Console.WriteLine($"   Symbol: {pos.Symbol}");
            Console.WriteLine($"   Type: {(pos.Type == 0 ? "BUY" : "SELL")}");
            Console.WriteLine($"   Volume: {pos.Volume} lots");
            Console.WriteLine($"   Entry: {pos.PriceOpen:F5}");
            Console.WriteLine($"   Current profit: ${pos.Profit:F2}\n");

            Console.WriteLine("✅ IMPLEMENTATION PATTERN:");
            Console.WriteLine(@"
   var cts = new CancellationTokenSource();
   cts.CancelAfter(TimeSpan.FromSeconds(30));  // Monitor for 30 seconds

   var profitStream = account.SubscribeToPositionProfitAsync(
       ticket,
       cancellationToken: cts.Token
   );

   Console.WriteLine(""Monitoring profit... Press Ctrl+C to stop\n"");

   await foreach (var update in profitStream)
   {
       Console.WriteLine($""Time: {DateTime.Now:HH:mm:ss}"");
       Console.WriteLine($""  Profit: ${update.Profit:F2}"");
       Console.WriteLine($""  Equity: ${update.Equity:F2}"");

       // Example: Alert if profit exceeds threshold
       if (update.Profit > 10.0)
       {
           Console.WriteLine(""  🎉 Target profit reached!"");
           cts.Cancel();  // Stop monitoring
       }

       // Example: Alert if loss exceeds threshold
       if (update.Profit < -5.0)
       {
           Console.WriteLine(""  ⚠️  Stop loss threshold reached!"");
           cts.Cancel();
       }
   }
");

            Console.WriteLine($"\n⚠️  NOT EXECUTING (demonstration only)");
            Console.WriteLine($"   Showing pattern for monitoring position #{pos.Ticket}\n");
        }
        else
        {
            Console.WriteLine("ℹ️  No open positions to monitor\n");

            Console.WriteLine("💡 WHEN YOU HAVE POSITIONS:");
            Console.WriteLine("   Use SubscribeToPositionProfitAsync() to:");
            Console.WriteLine("   - Monitor profit in real-time");
            Console.WriteLine("   - Implement trailing stops");
            Console.WriteLine("   - Send alerts on thresholds");
            Console.WriteLine("   - Auto-close on targets\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 5: GETTING POSITION/ORDER TICKETS IN REAL-TIME
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 5: Subscribe to Position/Order Changes");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 TICKET SUBSCRIPTION:");
        Console.WriteLine("   Get notified when:");
        Console.WriteLine("   - New position opened");
        Console.WriteLine("   - Position closed");
        Console.WriteLine("   - Pending order placed");
        Console.WriteLine("   - Pending order canceled\n");

        Console.WriteLine("✅ IMPLEMENTATION PATTERN:");
        Console.WriteLine(@"
   var cts = new CancellationTokenSource();

   var ticketsStream = account.OnPositionsAndPendingOrdersTicketsAsync(
       cancellationToken: cts.Token
   );

   Console.WriteLine(""Monitoring account activity...\n"");

   await foreach (var update in ticketsStream)
   {
       Console.WriteLine($""Update received at {DateTime.Now:HH:mm:ss}"");
       Console.WriteLine($""  Open positions: {update.OpenedPositionTickets.Count}"");
       Console.WriteLine($""  Pending orders: {update.OpenedOrdersTickets.Count}"");

       // Show position tickets
       if (update.OpenedPositionTickets.Count > 0)
       {
           Console.WriteLine(""  Position tickets:"");
           foreach (var ticket in update.OpenedPositionTickets)
           {
               Console.WriteLine($""    #{ticket}"");
           }
       }

       // Show order tickets
       if (update.OpenedOrdersTickets.Count > 0)
       {
           Console.WriteLine(""  Order tickets:"");
           foreach (var ticket in update.OpenedOrdersTickets)
           {
               Console.WriteLine($""    #{ticket}"");
           }
       }

       Console.WriteLine();
   }
");

        Console.WriteLine("⚠️  NOT EXECUTING (demonstration only)\n");

        Console.WriteLine("💡 USE CASE:");
        Console.WriteLine("   Perfect for detecting when:");
        Console.WriteLine("   - New positions appear (strategy executed)");
        Console.WriteLine("   - Positions disappear (SL/TP hit)");
        Console.WriteLine("   - Pending orders execute");
        Console.WriteLine("   - Track portfolio changes in real-time\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 6: PROPER CLEANUP AND UNSUBSCRIBE
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 6: Proper Cleanup - How to Stop Streaming");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 IMPORTANT: Always clean up subscriptions!\n");

        Console.WriteLine("✅ METHOD 1: Using CancellationToken (Recommended)");
        Console.WriteLine(@"
   var cts = new CancellationTokenSource();

   // User can press Ctrl+C to cancel
   Console.CancelKeyPress += (s, e) => {
       e.Cancel = true;
       cts.Cancel();
   };

   try
   {
       var stream = account.SubscribeToTicksAsync(
           new SymbolRequest { Symbol = symbol },
           cancellationToken: cts.Token
       );

       await foreach (var tick in stream.WithCancellation(cts.Token))
       {
           // Process tick
       }
   }
   catch (OperationCanceledException)
   {
       Console.WriteLine(""Subscription canceled cleanly ✓"");
   }

   // Stream automatically closed!
");

        Console.WriteLine("✅ METHOD 2: Timeout with CancelAfter");
        Console.WriteLine(@"
   var cts = new CancellationTokenSource();
   cts.CancelAfter(TimeSpan.FromMinutes(5));  // Auto-stop after 5 minutes

   var stream = account.SubscribeToTicksAsync(
       new SymbolRequest { Symbol = symbol },
       cancellationToken: cts.Token
   );

   await foreach (var tick in stream.WithCancellation(cts.Token))
   {
       // Process tick
   }

   // Automatically stops after 5 minutes
");

        Console.WriteLine("✅ METHOD 3: Manual break from loop");
        Console.WriteLine(@"
   var cts = new CancellationTokenSource();
   int count = 0;

   var stream = account.SubscribeToTicksAsync(
       new SymbolRequest { Symbol = symbol },
       cancellationToken: cts.Token
   );

   await foreach (var tick in stream)
   {
       count++;
       // Process tick

       if (count >= 100)  // Stop after 100 ticks
       {
           cts.Cancel();  // Cancel subscription
           break;         // Exit loop
       }
   }
");

        Console.WriteLine("⚠️  WHAT NOT TO DO:");
        Console.WriteLine(@"
   // ❌ BAD: No cleanup
   var stream = account.SubscribeToTicksAsync(...);
   // Stream runs forever, wastes resources!

   // ❌ BAD: Forgot to use CancellationToken
   while (true)
   {
       // No way to stop this!
   }
");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 7: WHEN TO USE STREAMING vs POLLING
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 7: Choosing Between Streaming and Polling");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("✅ USE STREAMING WHEN:\n");

        Console.WriteLine("   1. REAL-TIME MONITORING:");
        Console.WriteLine("      - Scalping strategies");
        Console.WriteLine("      - Trailing stops");
        Console.WriteLine("      - High-frequency trading\n");

        Console.WriteLine("   2. EVENT-DRIVEN LOGIC:");
        Console.WriteLine("      - React to every price tick");
        Console.WriteLine("      - Monitor trade executions");
        Console.WriteLine("      - Track position changes\n");

        Console.WriteLine("   3. NEED IMMEDIATE UPDATES:");
        Console.WriteLine("      - Can't afford 1-second delay");
        Console.WriteLine("      - Every tick matters");
        Console.WriteLine("      - Example: Market making\n");

        Console.WriteLine("   4. CONTINUOUS OPERATION:");
        Console.WriteLine("      - Bot runs 24/7");
        Console.WriteLine("      - Always monitoring market");
        Console.WriteLine("      - Example: Grid trading bot\n");

        Console.WriteLine("❌ USE POLLING WHEN:\n");

        Console.WriteLine("   1. OCCASIONAL CHECKS:");
        Console.WriteLine("      - Check price every 5 minutes");
        Console.WriteLine("      - Daily position review");
        Console.WriteLine("      - Example: Swing trading\n");

        Console.WriteLine("   2. SIMPLE SCRIPTS:");
        Console.WriteLine("      - One-time tasks");
        Console.WriteLine("      - Manual execution");
        Console.WriteLine("      - Example: Close all positions script\n");

        Console.WriteLine("   3. RESOURCE CONSTRAINTS:");
        Console.WriteLine("      - Limited bandwidth");
        Console.WriteLine("      - Shared server");
        Console.WriteLine("      - Polling = more predictable load\n");

        Console.WriteLine("   4. DEBUGGING:");
        Console.WriteLine("      - Easier to step through code");
        Console.WriteLine("      - More control over timing");
        Console.WriteLine("      - Example: Testing strategy logic\n");

        // ═══════════════════════════════════════════════════════════════════════
        // SUMMARY
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("SUMMARY - Streaming Subscriptions");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("📡 STREAMING vs POLLING:\n");

        Console.WriteLine("   STREAMING (Recommended for real-time):");
        Console.WriteLine("   ✅ Instant updates (no delay)");
        Console.WriteLine("   ✅ Efficient (no repeated requests)");
        Console.WriteLine("   ✅ Never miss changes");
        Console.WriteLine("   ✅ Less network traffic");
        Console.WriteLine("   ❌ Slightly more complex code\n");

        Console.WriteLine("   POLLING (Simpler, occasional use):");
        Console.WriteLine("   ✅ Simple to understand");
        Console.WriteLine("   ✅ Easy to control timing");
        Console.WriteLine("   ✅ Good for debugging");
        Console.WriteLine("   ❌ Delays between updates");
        Console.WriteLine("   ❌ Wastes resources");
        Console.WriteLine("   ❌ Might miss fast changes\n");

        Console.WriteLine("📚 AVAILABLE SUBSCRIPTIONS:\n");

        Console.WriteLine("   TICKS (price updates):");
        Console.WriteLine("   - SubscribeToTicksAsync(symbol, cancellationToken)");
        Console.WriteLine("   - Updates: Every price change");
        Console.WriteLine("   - Use: Scalping, real-time monitoring\n");

        Console.WriteLine("   TRADES (order/position events):");
        Console.WriteLine("   - SubscribeToTradeTransactionAsync(cancellationToken)");
        Console.WriteLine("   - Updates: Order placed, executed, modified");
        Console.WriteLine("   - Use: Audit log, execution tracking\n");

        Console.WriteLine("   POSITION PROFIT:");
        Console.WriteLine("   - SubscribeToPositionProfitAsync(ticket, cancellationToken)");
        Console.WriteLine("   - Updates: Profit changes for specific position");
        Console.WriteLine("   - Use: Trailing stops, profit alerts\n");

        Console.WriteLine("   TICKETS (position/order list):");
        Console.WriteLine("   - OnPositionsAndPendingOrdersTicketsAsync(cancellationToken)");
        Console.WriteLine("   - Updates: Position opened/closed, order placed/canceled");
        Console.WriteLine("   - Use: Portfolio monitoring\n");

        Console.WriteLine("🔧 BEST PRACTICES:\n");

        Console.WriteLine("   1. ALWAYS use CancellationToken");
        Console.WriteLine("   2. Handle OperationCanceledException properly");
        Console.WriteLine("   3. Clean up subscriptions when done");
        Console.WriteLine("   4. Use try/catch for network issues");
        Console.WriteLine("   5. Don't subscribe to same stream twice");
        Console.WriteLine("   6. Cancel token before application exit\n");

        Console.WriteLine("✅ CLEANUP PATTERN:");
        Console.WriteLine(@"
   var cts = new CancellationTokenSource();

   try
   {
       var stream = account.SubscribeToTicksAsync(..., cts.Token);
       await foreach (var item in stream.WithCancellation(cts.Token))
       {
           // Process item
       }
   }
   catch (OperationCanceledException)
   {
       // Normal cleanup
   }
   finally
   {
       cts.Dispose();  // Clean up resources
   }
");

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Streaming = Real-time responsiveness!                   ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
    }
}
