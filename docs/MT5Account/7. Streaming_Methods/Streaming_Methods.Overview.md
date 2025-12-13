# MT5Account · Streaming Methods - Overview

> Real-time continuous data streams: ticks, trade events, position updates, and account changes. Use this page for event-driven applications.

📖 **IMPORTANT:** [Complete Streaming & Subscription Guide](../../GRPC_STREAM_MANAGEMENT.md) - **Read this first!**
Learn how to properly manage streams, prevent memory leaks, and choose the right pattern (7 complete examples from simple to production-ready).

---

## 📁 What lives here

* **[SubscribeToTicks](./SubscribeToTicks.md)** - **real-time tick stream** for multiple symbols (bid/ask updates).
* **[SubscribeToTradeTransaction](./SubscribeToTradeTransaction.md)** - **trade events stream** (orders, deals, position changes).
* **[SubscribeToPositionProfit](./SubscribeToPositionProfit.md)** - **position P/L updates** (profit changes in real-time).
* **[OnTrade](./OnTrade.md)** - **comprehensive trade events** (positions, orders, deals with full details).
* **[OnPositionsAndPendingOrdersTickets](./OnPositionsAndPendingOrdersTickets.md)** - **periodic ticket snapshots** (lightweight polling).

---

## 🧭 Plain English

* **SubscribeToTicks** → live price feed for multiple symbols (like a price ticker).
* **SubscribeToTradeTransaction** → get notified when orders execute, positions change, etc.
* **SubscribeToPositionProfit** → watch profit/loss changes in real-time.
* **OnTrade** → full details of all trading events (new positions, closed orders, etc.).
* **OnPositionsAndPendingOrdersTickets** → lightweight periodic updates of active tickets.

> **Streaming = `await foreach`** - these methods return `IAsyncEnumerable<T>`. Use with async loops and `CancellationToken`.

---

## Quick choose

| If you need…                                     | Use                                        | Returns (stream)                | Key inputs                          |
| ------------------------------------------------ | ------------------------------------------ | ------------------------------- | ----------------------------------- |
| Real-time price ticks                            | `OnSymbolTickAsync`                        | Tick data for symbols           | Array of symbol names               |
| Trade transaction events                         | `OnTradeTransactionAsync`                  | Trade events (orders, deals)    | *(none)*                            |
| Position profit updates                          | `OnPositionProfitAsync`                    | P/L changes (new/updated/closed)| Update interval + ignore empty flag |
| Comprehensive trade events                       | `OnTradeAsync`                             | Full trade event details        | *(none)*                            |
| Periodic ticket snapshots                        | `OnPositionsAndPendingOrdersTicketsAsync`  | Lists of tickets                | Update interval                     |

---

## ❌ Cross‑refs & gotchas

* **Cancellation:** Always use `CancellationToken` to stop streams gracefully.
* **`await foreach`:** Required syntax for processing streaming data.
* **Error handling:** Wrap in try-catch for `OperationCanceledException`.
* **Buffering:** Streams may buffer events - process quickly to avoid delays.
* **Automatic reconnection:** All streaming methods have built-in `ExecuteWithReconnect` - connection issues handled automatically.
* **Performance:** Ticks for multiple symbols = high volume, ensure fast processing.
* **Memory safety:** MT5Account has built-in cleanup (`finally { stream?.Dispose() }`), but still use `CancellationToken` for graceful shutdown.

💡 **See [GRPC_STREAM_MANAGEMENT.md](../../GRPC_STREAM_MANAGEMENT.md) for complete guide with 7 patterns, best practices, and production examples!**

---

## 🟢 Minimal snippets

```csharp
// Real-time tick stream for multiple symbols
var cts = new CancellationTokenSource();
try
{
    await foreach (var tickData in account.OnSymbolTickAsync(
        new[] { "EURUSD", "GBPUSD", "USDJPY" }, cts.Token))
    {
        var tick = tickData.SymbolTick;
        Console.WriteLine($"[{tick.Time.ToDateTime():HH:mm:ss}] {tick.Symbol}: " +
                          $"Bid={tick.Bid:F5}, Ask={tick.Ask:F5}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Stream stopped");
}
```

```csharp
// Trade transaction events (orders, deals)
var cts = new CancellationTokenSource();
try
{
    await foreach (var tradeEvent in account.OnTradeTransactionAsync(cts.Token))
    {
        Console.WriteLine($"Event Type: {tradeEvent.Type}");

        if (tradeEvent.TradeTransaction != null)
        {
            var tx = tradeEvent.TradeTransaction;
            Console.WriteLine($"Transaction: {tx.Type} - Order #{tx.OrderTicket}, " +
                              $"Symbol: {tx.Symbol}, Price: {tx.Price:F5}");
        }

        if (tradeEvent.TradeResult != null)
        {
            var result = tradeEvent.TradeResult;
            Console.WriteLine($"Result: Code={result.TradeReturnCode}, " +
                              $"Deal #{result.DealTicket}, Order #{result.OrderTicket}");
        }
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Transaction stream stopped");
}
```

```csharp
// Position profit updates (real-time P/L)
var cts = new CancellationTokenSource();
try
{
    await foreach (var profitUpdate in account.OnPositionProfitAsync(
        intervalMs: 500, ignoreEmptyData: true, cts.Token))
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Profit Update:");

        foreach (var pos in profitUpdate.NewPositions)
        {
            Console.WriteLine($"  NEW: {pos.PositionSymbol} #{pos.Ticket} - P/L: ${pos.Profit:F2}");
        }

        foreach (var pos in profitUpdate.UpdatedPositions)
        {
            Console.WriteLine($"  UPD: {pos.PositionSymbol} #{pos.Ticket} - P/L: ${pos.Profit:F2}");
        }

        foreach (var pos in profitUpdate.DeletedPositions)
        {
            Console.WriteLine($"  DEL: {pos.PositionSymbol} #{pos.Ticket} - Final P/L: ${pos.Profit:F2}");
        }
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Profit stream stopped");
}
```

```csharp
// Periodic ticket snapshots (lightweight)
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // Auto-stop after 30s
try
{
    await foreach (var snapshot in account.OnPositionsAndPendingOrdersTicketsAsync(
        intervalMs: 1000, cts.Token))
    {
        Console.WriteLine($"[{snapshot.ServerTime.ToDateTime():HH:mm:ss}] " +
                          $"Positions: {snapshot.PositionTickets.Count}, " +
                          $"Orders: {snapshot.PendingOrderTickets.Count}");

        if (snapshot.PositionTickets.Any())
        {
            Console.WriteLine($"  Position tickets: {string.Join(", ", snapshot.PositionTickets)}");
        }
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Ticket stream stopped");
}
```

```csharp
// Comprehensive trade events with full details
var cts = new CancellationTokenSource();
try
{
    await foreach (var tradeData in account.OnTradeAsync(cts.Token))
    {
        var events = tradeData.EventData;

        foreach (var pos in events.NewPositions)
        {
            Console.WriteLine($"NEW Position: {pos.PositionSymbol} #{pos.PositionTicket} @ {pos.PriceOpen:F5}");
        }

        foreach (var order in events.NewOrders)
        {
            Console.WriteLine($"NEW Order: {order.OrderSymbol} #{order.OrderTicket} - {order.OrderType}");
        }

        foreach (var deal in events.NewHistoryDeals)
        {
            Console.WriteLine($"NEW Deal: #{deal.Ticket} - Profit: ${deal.Profit:F2}");
        }
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Trade stream stopped");
}
```

```csharp
// Stop stream after condition met
var cts = new CancellationTokenSource();
var maxProfit = 100.0;

try
{
    await foreach (var profitUpdate in account.OnPositionProfitAsync(
        intervalMs: 500, ignoreEmptyData: true, cts.Token))
    {
        var totalProfit = profitUpdate.UpdatedPositions.Sum(p => p.Profit);
        Console.WriteLine($"Total Profit: ${totalProfit:F2}");

        if (totalProfit >= maxProfit)
        {
            Console.WriteLine($"✅ Target profit ${maxProfit} reached! Stopping stream...");
            cts.Cancel();
        }
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Stream stopped by target condition");
}
```

---

## See also

* 📖 **[GRPC Stream Management Guide](../../GRPC_STREAM_MANAGEMENT.md)** - Complete guide to streaming (MUST READ!)
* **Request/Reply:** [OpenedOrders](../3.%20Position_Orders_Information/OpenedOrders.md) - one-time snapshot vs continuous stream
* **Request/Reply:** [SymbolInfoTick](../2.%20Symbol_information/SymbolInfoTick.md) - single tick vs tick stream
* **Account:** [AccountSummary](../1.%20Account_information/AccountSummary.md) - account snapshot
* **Trading:** [OrderSend](../4.%20Trading_Operattons/OrderSend.md) - place orders based on stream events
* **MT5Sugar Helpers:** [ReadTicks](../../MT5Sugar/5.%20Streams_Helpers/ReadTicks.md) & [ReadTrades](../../MT5Sugar/5.%20Streams_Helpers/ReadTrades.md) - Bounded streaming (recommended!)
