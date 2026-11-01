# gRPC Streaming and Subscription Management Guide

> **Complete guide** to working with real-time subscriptions in CSharpMT5 (C# / .NET).

This document covers:

- ✅ **How to subscribe properly** to market-data streams
- ✅ **How to stop subscriptions** without leaking the gRPC call
- ✅ **Common patterns** from simple to advanced
- ✅ **Architecture** and built-in safety mechanisms
- ✅ **Troubleshooting** and best practices

---

## Table of Contents

1. [Quick Start](#quick-start)
2. [Available Streaming Methods](#available-streaming-methods)
3. [Complete Patterns](#complete-patterns)
4. [Why Streams Need Management](#why-streams-need-management)
5. [Best Practices](#best-practices)
6. [Advanced Examples with Enums](#advanced-examples-with-enums)
7. [Multiple Concurrent Streams](#multiple-concurrent-streams)
8. [Architecture and Safety](#architecture-and-safety)
9. [Troubleshooting](#troubleshooting)

---

## Quick Start

In C# every streaming method returns an **`IAsyncEnumerable<T>`** that you consume with **`await foreach`**. The stream advances only while you consume it, so stopping consumption (a `break`, or cancelling the token) stops the stream.

### 1️⃣ Simplest pattern — stop after N events

```csharp
using mt5_term_api;
using MetaRPC.CSharpMT5;

var account = new MT5Account(user, password, "https://mt5.mrpc.pro:443", apiKey);
await account.ConnectByServerNameAsync("YourBroker-Demo", "EURUSD");

try
{
    int count = 0;
    await foreach (var tickData in account.OnSymbolTickAsync(new[] { "EURUSD", "GBPUSD" }))
    {
        var t = tickData.SymbolTick;
        Console.WriteLine($"[{++count}] Bid={t.Bid:F5}, Ask={t.Ask:F5}");

        if (count >= 10) break; // stops the stream; the enumerator is disposed → gRPC call cancelled
    }
}
finally
{
    await account.DisconnectAsync();
}
```

**When to use:** quick examples, testing, short monitoring sessions.

### 2️⃣ Auto-timeout — `CancellationTokenSource(TimeSpan)`

A `break` only runs when the next message arrives. To stop a **quiet** stream after a fixed time, drive it with a cancellation token:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // auto-cancels after 10s

try
{
    await foreach (var tickData in account.OnSymbolTickAsync(new[] { "EURUSD" }, cts.Token))
        Console.WriteLine($"Bid={tickData.SymbolTick.Bid:F5}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Stream stopped (timeout)");
}
finally
{
    await account.DisconnectAsync();
}
```

### 3️⃣ Manual control — cancel from elsewhere

```csharp
using var cts = new CancellationTokenSource();

// Run the stream in the background
var monitor = Task.Run(async () =>
{
    await foreach (var tickData in account.OnSymbolTickAsync(new[] { "EURUSD" }, cts.Token))
        Console.WriteLine($"Price: {tickData.SymbolTick.Bid:F5}");
});

// ... do other work ...
await Task.Delay(5000);
cts.Cancel();                 // signal the stream to stop
try { await monitor; } catch (OperationCanceledException) { }

await account.DisconnectAsync();
```

**When to use:** long-term monitoring, background services, production apps.

---

## Available Streaming Methods

### MT5Account (low-level streams)

All streaming methods return an **`IAsyncEnumerable<T>`** and accept a **`CancellationToken`**.

| Method | Description | Yields |
|--------|-------------|--------|
| `OnSymbolTickAsync(symbols, ct)` | Real-time price ticks | `OnSymbolTickData` |
| `OnTradeAsync(ct)` | Trade events (orders created/changed/closed) | `OnTradeData` |
| `OnPositionProfitAsync(intervalMs, ignoreEmpty, ct)` | Position P&L updates (polled) | `OnPositionProfitData` |
| `OnPositionsAndPendingOrdersTicketsAsync(intervalMs, ct)` | Position/pending ticket snapshots | `OnPositionsAndPendingOrdersTicketsData` |
| `OnTradeTransactionAsync(ct)` | Low-level trade transaction events | `OnTradeTransactionData` |

**Documentation:**
- [Streaming Methods Overview](../MT5Account/7.%20Streaming_Methods/Streaming_Methods.Overview.md)
- [SubscribeToTicks](../MT5Account/7.%20Streaming_Methods/SubscribeToTicks.md) · [OnTrade](../MT5Account/7.%20Streaming_Methods/OnTrade.md) · [SubscribeToPositionProfit](../MT5Account/7.%20Streaming_Methods/SubscribeToPositionProfit.md)

**Examples:** `Examples/LowLevel/Program.Streaming.cs`, and the Sugar helpers `ReadTicks` / `ReadTrades` (`Examples/Sugar/Program.Sugar.cs`).

---

## Complete Patterns

### Pattern 1 — Quick example (count + timeout)

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
try
{
    int count = 0;
    await foreach (var td in account.OnSymbolTickAsync(new[] { "EURUSD" }, cts.Token))
    {
        Console.WriteLine($"Tick #{++count}: {td.SymbolTick.Bid:F5}");
        if (count >= 5) break;
    }
}
catch (OperationCanceledException) { /* timed out */ }
finally { await account.DisconnectAsync(); }
```

### Pattern 2 — Event-limited streaming

```csharp
const int maxEvents = 100;
int count = 0;
await foreach (var td in account.OnSymbolTickAsync(new[] { "EURUSD" }))
{
    Console.WriteLine($"[{++count}] {td.SymbolTick.Bid:F5}");
    if (count >= maxEvents) break;
}
```

### Pattern 3 — Condition-based stop

```csharp
await foreach (var td in account.OnSymbolTickAsync(new[] { "EURUSD" }))
{
    double bid = td.SymbolTick.Bid;
    Console.WriteLine($"Price: {bid:F5}");
    if (bid > 1.10000) { Console.WriteLine("Target reached!"); break; }
}
```

### Pattern 4 — Background service with manual control

```csharp
public sealed class PriceMonitor
{
    private readonly MT5Account _account;
    private readonly CancellationTokenSource _cts = new();
    private Task? _task;

    public PriceMonitor(MT5Account account) => _account = account;

    public void Start(IEnumerable<string> symbols) =>
        _task = Task.Run(() => MonitorAsync(symbols, _cts.Token));

    private async Task MonitorAsync(IEnumerable<string> symbols, CancellationToken ct)
    {
        try
        {
            await foreach (var td in _account.OnSymbolTickAsync(symbols, ct))
                Console.WriteLine($"{td.SymbolTick.Bid:F5} / {td.SymbolTick.Ask:F5}");
        }
        catch (OperationCanceledException) { /* expected on stop */ }
    }

    public async Task StopAsync()
    {
        _cts.Cancel();
        if (_task is not null) { try { await _task; } catch (OperationCanceledException) { } }
    }
}

// Usage
var monitor = new PriceMonitor(account);
monitor.Start(new[] { "EURUSD", "GBPUSD" });
await Task.Delay(TimeSpan.FromSeconds(30));
await monitor.StopAsync();
await account.DisconnectAsync();
```

---

## Why Streams Need Management

A streaming subscription is a live gRPC server-stream. Two things matter:

1. **The stream is consumer-driven.** The underlying loop only advances when your `await foreach` asks for the next item. When you `break` (or the loop ends), the async enumerator is disposed and the SDK cancels the current gRPC call in its `finally`. So **`break` is enough to stop the stream** — there is no orphaned background task as in some other runtimes.

2. **A blocked wait needs a token to interrupt it.** If no new message is arriving, `await foreach` is parked inside `MoveNextAsync` and your `break`/condition cannot run yet. Pass a **`CancellationToken`** (from a `CancellationTokenSource`) so you can interrupt that wait — on cancel, `MoveNextAsync` throws `OperationCanceledException`, the enumerator is disposed, and the stream ends.

```csharp
// ❌ Can't stop a quiet stream promptly — nothing interrupts the wait:
await foreach (var td in account.OnSymbolTickAsync(new[] { "EURUSD" }))
{
    if (DateTime.UtcNow > deadline) break; // only checked when a tick arrives
}

// ✅ Token interrupts the wait immediately:
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
try
{
    await foreach (var td in account.OnSymbolTickAsync(new[] { "EURUSD" }, cts.Token)) { /* ... */ }
}
catch (OperationCanceledException) { }
```

**Rule of thumb:** use `break` for event/condition limits; add a `CancellationToken` whenever you need a time limit or external stop, or whenever the stream may go quiet.

---

## Best Practices

### ✅ DO

1. Consume every stream with `await foreach`.
2. Pass a `CancellationToken` for time limits or external cancellation.
3. Wrap token-driven loops in `try { … } catch (OperationCanceledException) { }`.
4. Always `break` (or cancel) when you have what you need — don't loop forever by accident.
5. Call `await account.DisconnectAsync()` in a `finally` to release the channel when you're done with the account.
6. Share **one** `CancellationTokenSource` across several streams to stop them together.

### ❌ DON'T

1. Don't rely on a `break` alone to stop a stream that may be quiet — add a token.
2. Don't forget `catch (OperationCanceledException)` when you cancel via a token (otherwise it surfaces as an error).
3. Don't `Dispose()` the `MT5Account`/channel while a stream is still iterating — stop the stream first.
4. Don't swallow real stream errors (`RpcException`, `ApiExceptionMT5`) — only the cancellation is expected.

---

## Advanced Examples with Enums

### Example 1 — Filter trade transactions by type

```csharp
using mt5_term_api;

using var cts = new CancellationTokenSource();
try
{
    await foreach (var data in account.OnTradeTransactionAsync(cts.Token))
    {
        var tx = data.TradeTransaction;
        if (tx is null) continue;

        switch (tx.Type)
        {
            case SUB_ENUM_TRADE_TRANSACTION_TYPE.SubTradeTransactionOrderAdd:
                Console.WriteLine($"[NEW ORDER] #{tx.OrderTicket} {tx.Symbol} ({tx.OrderType})");
                break;
            case SUB_ENUM_TRADE_TRANSACTION_TYPE.SubTradeTransactionOrderDelete:
                Console.WriteLine($"[ORDER DELETED] #{tx.OrderTicket}");
                break;
            case SUB_ENUM_TRADE_TRANSACTION_TYPE.SubTradeTransactionDealAdd:
                Console.WriteLine($"[NEW DEAL] deal #{tx.DealTicket} order #{tx.OrderTicket} {tx.Symbol}");
                break;
        }
    }
}
catch (OperationCanceledException) { }
finally { await account.DisconnectAsync(); }
```

### Example 2 — Positions sorted by profit

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
try
{
    await foreach (var p in account.OnPositionProfitAsync(intervalMs: 1000, ignoreEmpty: true, cancellationToken: cts.Token))
    {
        var positions = p.UpdatedPositions.OrderByDescending(x => x.Profit).ToList();
        if (positions.Count == 0) continue;

        double total = positions.Sum(x => x.Profit);
        int winners  = positions.Count(x => x.Profit > 0);

        Console.WriteLine($"\n[UPDATE] {positions.Count} positions | Total {total:+0.00;-0.00} | Winners {winners}");
        foreach (var pos in positions.Take(5))
            Console.WriteLine($"  {pos.PositionSymbol,-10} #{pos.Ticket,-10} {pos.Profit,10:F2}");
    }
}
catch (OperationCanceledException) { }
finally { await account.DisconnectAsync(); }
```

> Enum/field names come straight from the generated proto types in the `mt5_term_api` namespace — see [Enums Usage Reference](./ENUMS_USAGE_REFERENCE.md).

---

## Multiple Concurrent Streams

Run several streams at once and stop them all with one token via `Task.WhenAll`.

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // auto-stop all after 30s

async Task StreamTicks()
{
    try { await foreach (var td in account.OnSymbolTickAsync(new[] { "EURUSD" }, cts.Token))
        Console.WriteLine($"[TICK] {td.SymbolTick.Bid:F5}"); }
    catch (OperationCanceledException) { }
}

async Task StreamTrades()
{
    try { await foreach (var ev in account.OnTradeAsync(cts.Token))
        Console.WriteLine($"[TRADE] {ev.Type}"); }
    catch (OperationCanceledException) { }
}

async Task StreamProfits()
{
    try { await foreach (var p in account.OnPositionProfitAsync(2000, true, cts.Token))
        Console.WriteLine($"[PROFIT] {p.UpdatedPositions.Count} positions updated"); }
    catch (OperationCanceledException) { }
}

try
{
    await Task.WhenAll(StreamTicks(), StreamTrades(), StreamProfits());
}
finally
{
    await account.DisconnectAsync();
    Console.WriteLine("[DONE] all streams stopped");
}
```

A single `cts.Cancel()` (or the timeout) stops every stream that shares the token.

---

## Architecture and Safety

### How streaming works inside MT5Account

All streaming methods (`OnSymbolTickAsync`, `OnTradeAsync`, `OnPositionProfitAsync`, `OnTradeTransactionAsync`, `OnPositionsAndPendingOrdersTicketsAsync`) share the same internal helper: **`ExecuteStreamWithReconnect`**.

**Data flow:**

```
Your code (await foreach)
    ↓ pulls items
ExecuteStreamWithReconnect  (reconnect loop + error handling)
    ↓
gRPC server stream  →  Network  →  MT5 terminal
```

Simplified structure (C#):

```csharp
private async IAsyncEnumerable<TData> ExecuteStreamWithReconnect<...>(
    TRequest request, ...,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        bool reconnectRequired = false;
        AsyncServerStreamingCall<TReply> stream = null;
        try
        {
            stream = streamInvoker(request, GetHeaders(), cancellationToken);
            while (await stream.ResponseStream.MoveNext(cancellationToken))
            {
                var error = getError(stream.ResponseStream.Current);
                if (error?.ErrorCode is "TERMINAL_INSTANCE_NOT_FOUND" or "TERMINAL_REGISTRY_TERMINAL_NOT_FOUND")
                { reconnectRequired = true; break; }
                if (error != null) throw new ApiExceptionMT5(error);

                var data = getData(stream.ResponseStream.Current);
                if (data != null) yield return data;   // ← handed to your await foreach
            }
        }
        catch (RpcException ex) when ((int)ex.StatusCode == 14 /* UNAVAILABLE */)
        {
            reconnectRequired = true;
        }
        finally
        {
            stream?.Dispose();                           // ✅ always closes the current gRPC stream
        }

        if (!reconnectRequired) break;                   // normal end
        await Task.Delay(500, cancellationToken);
        await Reconnect(null, cancellationToken);         // transient network error → reconnect
    }
}
```

**What it does:**

- ✅ Disposes the current gRPC stream automatically (`finally`).
- ✅ Reconnects transparently on a recoverable network error (`StatusCode 14`, UNAVAILABLE) and on `TERMINAL_INSTANCE_NOT_FOUND`.
- ✅ Surfaces real API errors as `ApiExceptionMT5`.

**What you control:**

- The loop runs while the consumer keeps pulling **and** the token isn't cancelled. Because it's an iterator, it does **not** run on its own in the background — stop consuming (break) or cancel the token and it ends. Then call `DisconnectAsync()` to release the channel.

---

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Stream never stops on a quiet market | Relying on `break`/condition only; the wait is parked | Pass a `CancellationToken` (e.g. `new CancellationTokenSource(TimeSpan…)`) |
| `OperationCanceledException` bubbles up as an error | You cancelled via token but didn't catch it | Wrap the loop in `try { … } catch (OperationCanceledException) { }` |
| `RpcException` StatusCode `Unavailable` mid-stream | Transient network drop | Handled automatically — the SDK reconnects; no action needed |
| `ObjectDisposedException` on the channel | Account/channel disposed while a stream still iterates | Stop the stream first, then `DisconnectAsync()` |
| Stream ends immediately with an API error | Real server error (e.g. not connected) | Inspect the `ApiExceptionMT5` message; ensure you connected first |

### Verifying a stream stopped

```csharp
using var cts = new CancellationTokenSource();
var task = Task.Run(async () =>
{
    try { await foreach (var td in account.OnSymbolTickAsync(new[] { "EURUSD" }, cts.Token)) { } }
    catch (OperationCanceledException) { }
});

await Task.Delay(2000);
cts.Cancel();
await task;                      // completes once the stream has unwound
Console.WriteLine("Stream fully stopped");
```

---

## See Also

* **[Streaming Methods Overview](../MT5Account/7.%20Streaming_Methods/Streaming_Methods.Overview.md)** — all streaming methods
* **[SubscribeToTicks](../MT5Account/7.%20Streaming_Methods/SubscribeToTicks.md)** — tick streaming reference
* **[OnTrade](../MT5Account/7.%20Streaming_Methods/OnTrade.md)** — trade-events reference
* **[Enums Usage Reference](./ENUMS_USAGE_REFERENCE.md)** — enum types used in stream payloads
* **[Sync vs Async](./SYNC_VS_ASYNC.md)** — async patterns in the SDK
* **[gRPC for .NET](https://learn.microsoft.com/aspnet/core/grpc/)** — official .NET gRPC docs

---

**Remember:** C# streams are consumer-driven — `break` stops them, and a `CancellationToken` lets you interrupt a quiet wait or stop from outside. Catch `OperationCanceledException`, disconnect when done, and your streaming code stays robust and leak-free. 🚀
