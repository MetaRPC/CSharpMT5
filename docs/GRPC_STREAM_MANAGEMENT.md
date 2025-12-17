# gRPC Stream Management & Subscription Guide

> **Complete guide** to working with real-time subscriptions in C# MT5

This document covers:

- ✅ **How to properly subscribe** to market data streams
- ✅ **How to stop subscriptions** without memory leaks
- ✅ **Common patterns** from simple to advanced
- ✅ **Architecture** and built-in safety mechanisms
- ✅ **Troubleshooting** and best practices

---

## Table of Contents

1. [Quick Start - Simple Subscription](#quick-start---simple-subscription)
2. [Available Streaming Methods](#available-streaming-methods)
3. [Complete Patterns (Simple → Advanced)](#complete-patterns-simple--advanced)
4. [Problem: Why Streams Need Management](#problem-why-streams-need-management)
5. [Solutions & Best Practices](#solutions--best-practices)
6. [Architecture & Safety](#architecture-notes)
7. [Troubleshooting](#testing-for-memory-leaks)

---

## Quick Start - Simple Subscription

### 1️⃣ Simplest Pattern (Bounded Streaming) ⭐

**Use MT5Sugar helpers** - automatically stops after N events or timeout:

```csharp
using mt5_term_api;

var account = new MT5Account(user, password, grpcServer, null);
await account.ConnectByServerNameAsync(serverName, "EURUSD", 30);

var svc = new MT5Service(account);

// ✅ Read 20 ticks, max 10 seconds - automatically stops!
await foreach (var tick in svc.ReadTicks(
    symbols: new[] { "EURUSD", "GBPUSD" },
    maxEvents: 20,
    durationSec: 10))
{
    Console.WriteLine($"{tick.Symbol}: Bid={tick.Bid}, Ask={tick.Ask}");
}
// Done! No cleanup needed - stream auto-stopped ✅
```

**When to use:** Quick samples, testing, short monitoring sessions

---

### 2️⃣ Manual Control Pattern

**For full control** - you manage when to stop:

```csharp
using var cts = new CancellationTokenSource();

// Start monitoring
var monitorTask = Task.Run(async () =>
{
    try
    {
        await foreach (var tick in account.OnSymbolTickAsync(
            new[] { "EURUSD" }, cts.Token))
        {
            Console.WriteLine($"Price: {tick.Bid}");
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Monitoring stopped");
    }
});

// ... do other work ...

// Stop when needed
cts.Cancel();
await monitorTask;
```

**When to use:** Long-running monitoring, background services, production apps

---

## Available Streaming Methods

### MT5Account (Low-Level Streams)

| Method | Description | Returns |
|--------|-------------|---------|
| `OnSymbolTickAsync()` | Real-time price ticks for symbols | `IAsyncEnumerable<OnSymbolTickData>` |
| `OnTradeAsync()` | Trade events (orders filled, modified, etc.) | `IAsyncEnumerable<OnTradeData>` |
| `OnBookEventAsync()` | Market depth (DOM) events | `IAsyncEnumerable<OnBookEventData>` |
| `OnPositionProfitAsync()` | Position P&L updates | `IAsyncEnumerable<OnPositionProfitData>` |
| `OnPositionsAndPendingOrdersTicketsAsync()` | Order/position tickets | `IAsyncEnumerable<OnPositionsAndPendingOrdersTicketsData>` |

**All require `CancellationToken` for stopping!**

See: [MT5Account Streaming Overview](MT5Account/7.%20Streaming_Methods/Streaming_Methods.Overview.md)

---

### MT5Sugar (Bounded Helpers) ⭐

| Method | Description | Auto-Stops After |
|--------|-------------|------------------|
| `ReadTicks()` | Limited tick streaming | N events OR timeout |
| `ReadTrades()` | Limited trade event streaming | N events OR timeout |
| `SubscribeToMarketBookAsync()` | DOM subscription (IDisposable) | `using` block exits |

**Recommended for most use cases!**

See: [ReadTicks](MT5Sugar/5.%20Streams_Helpers/ReadTicks.md) | [ReadTrades](MT5Sugar/5.%20Streams_Helpers/ReadTrades.md)

---

## Complete Patterns (Simple → Advanced)

### Pattern 1: Quick Sample (5-10 seconds)

```csharp
// ✅ Automatic timeout - perfect for testing
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

await foreach (var tick in account.OnSymbolTickAsync(
    new[] { "EURUSD" }, cts.Token))
{
    Console.WriteLine($"Tick: {tick.Bid}");
}
// Stops after 10 seconds automatically ✅
```

---

### Pattern 2: Event-Limited Streaming

```csharp
// ✅ Stop after processing N events
using var cts = new CancellationTokenSource();
int count = 0;
const int MAX_EVENTS = 100;

await foreach (var tick in account.OnSymbolTickAsync(
    new[] { "EURUSD" }, cts.Token))
{
    Console.WriteLine($"[{++count}] {tick.Symbol}: {tick.Bid}");

    if (count >= MAX_EVENTS)
    {
        cts.Cancel();
        break;
    }
}
```

---

### Pattern 3: Condition-Based Stopping

```csharp
// ✅ Stop when specific condition met
using var cts = new CancellationTokenSource();

await foreach (var tick in account.OnSymbolTickAsync(
    new[] { "EURUSD" }, cts.Token))
{
    Console.WriteLine($"Price: {tick.Bid}");

    // Stop if price crosses threshold
    if (tick.Bid > 1.10000)
    {
        Console.WriteLine("Target price reached!");
        cts.Cancel();
        break;
    }
}
```

---

### Pattern 4: Background Service with Manual Control

```csharp
public class PriceMonitor : IDisposable
{
    private CancellationTokenSource? _cts;
    private Task? _monitorTask;

    public void Start(MT5Account account, string[] symbols)
    {
        _cts = new CancellationTokenSource();
        _monitorTask = MonitorPricesAsync(account, symbols, _cts.Token);
    }

    private async Task MonitorPricesAsync(
        MT5Account account,
        string[] symbols,
        CancellationToken ct)
    {
        try
        {
            await foreach (var tick in account.OnSymbolTickAsync(symbols, ct))
            {
                ProcessTick(tick);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Monitoring stopped gracefully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void ProcessTick(OnSymbolTickData tick)
    {
        Console.WriteLine($"{tick.Symbol}: {tick.Bid} / {tick.Ask}");
        // Your logic here...
    }

    public void Stop()
    {
        _cts?.Cancel();
        _monitorTask?.Wait(TimeSpan.FromSeconds(5));
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
    }
}

// Usage:
using var monitor = new PriceMonitor();
monitor.Start(account, new[] { "EURUSD", "GBPUSD" });

Console.WriteLine("Press any key to stop monitoring...");
Console.ReadKey();

monitor.Stop();
```

---

### Pattern 5: Multiple Concurrent Streams

```csharp
using var cts = new CancellationTokenSource();

// Start multiple streams concurrently
var tickTask = Task.Run(async () =>
{
    await foreach (var tick in account.OnSymbolTickAsync(
        new[] { "EURUSD" }, cts.Token))
    {
        Console.WriteLine($"Tick: {tick.Bid}");
    }
});

var tradeTask = Task.Run(async () =>
{
    await foreach (var trade in account.OnTradeAsync(cts.Token))
    {
        Console.WriteLine($"Trade: {trade.Type}");
    }
});

// Wait for both or timeout
await Task.WhenAny(
    Task.WhenAll(tickTask, tradeTask),
    Task.Delay(TimeSpan.FromSeconds(30)));

// Stop all streams
cts.Cancel();

// Ensure all tasks completed
await Task.WhenAll(tickTask, tradeTask);
```

---

### Pattern 6: DOM (Market Depth) Subscription

```csharp
// ✅ IDisposable pattern - auto-cleanup with 'using'
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    // While inside 'using' block, subscription is active
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");

    Console.WriteLine("Order Book:");
    foreach (var item in book.MqlBookInfo)
    {
        Console.WriteLine($"  {item.Type}: {item.Price} x {item.Volume}");
    }

    var (bestBid, bestAsk) = await svc.GetBestBidAskFromBookAsync("EURUSD");
    Console.WriteLine($"Best: {bestBid} / {bestAsk}");

} // ✅ Automatically unsubscribes here!

// Subscription is now inactive - clean exit
```

---

### Pattern 7: Error Handling & Reconnection

```csharp
using var cts = new CancellationTokenSource();
int reconnectCount = 0;
const int MAX_RECONNECTS = 3;

while (reconnectCount < MAX_RECONNECTS && !cts.Token.IsCancellationRequested)
{
    try
    {
        await foreach (var tick in account.OnSymbolTickAsync(
            new[] { "EURUSD" }, cts.Token))
        {
            Console.WriteLine($"Price: {tick.Bid}");
            reconnectCount = 0; // Reset on successful stream
        }
        break; // Normal exit
    }
    catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
    {
        reconnectCount++;
        Console.WriteLine($"Connection lost. Reconnecting... ({reconnectCount}/{MAX_RECONNECTS})");
        await Task.Delay(2000, cts.Token);

        // MT5Account.ExecuteStreamWithReconnect handles this automatically!
        // This pattern is for demonstration - usually not needed
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Cancelled by user");
        break;
    }
}
```

---

## Problem: Why Streams Need Management

When working with gRPC streaming in CSharpMT5, understanding stream lifecycle is critical:

**Stream subscriptions** (`OnSymbolTickAsync`, `OnTradeAsync`, etc.) are **active network connections** that continue running even after exiting the method or losing the reference.

---

## The Problem Explained

### Current Implementation

```csharp
public IAsyncEnumerable<OnSymbolTickData> OnSymbolTickAsync(
    IEnumerable<string> symbolNames,
    CancellationToken ct = default)
{
    // Stream starts...
    // ❌ No way to stop it except CancellationToken
}
```

### What Happens Without Proper Cleanup

```csharp
// ❌ BAD: Stream continues running forever
await foreach (var tick in account.OnSymbolTickAsync(symbols))
{
    Console.WriteLine($"{tick.Symbol}: {tick.Bid}");

    if (someCondition)
        break;  // ❌ PROBLEM: Stream still running in background!
}
```

**Result:**

- Background gRPC call continues consuming resources
- MT5 terminal keeps sending updates
- Memory gradually accumulates
- Multiple abandoned streams = **memory leak**

---

## Solution 1: Always Use CancellationToken ✅

### Pattern 1: CancellationTokenSource

```csharp
using var cts = new CancellationTokenSource();

try
{
    await foreach (var tick in account.OnSymbolTickAsync(symbols, cts.Token))
    {
        Console.WriteLine($"{tick.Symbol}: {tick.Bid}");

        if (someCondition)
        {
            cts.Cancel();  // ✅ CORRECT: Stops stream
            break;
        }
    }
}
catch (OperationCanceledException)
{
    // Expected when we cancel
    Console.WriteLine("Stream stopped");
}
```

### Pattern 2: Timeout with CancellationToken

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    await foreach (var tick in account.OnSymbolTickAsync(symbols, cts.Token))
    {
        Console.WriteLine($"{tick.Symbol}: {tick.Bid}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Stream timed out or was cancelled");
}
```

### Pattern 3: Manual Cancellation

```csharp
private CancellationTokenSource? _streamCts;

public async Task StartMonitoring(MT5Account account, string[] symbols)
{
    _streamCts = new CancellationTokenSource();

    try
    {
        await foreach (var tick in account.OnSymbolTickAsync(symbols, _streamCts.Token))
        {
            ProcessTick(tick);
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Monitoring stopped");
    }
}

public void StopMonitoring()
{
    _streamCts?.Cancel();  // ✅ Stops the stream
    _streamCts?.Dispose();
}
```

---

## Solution 2: MT5Sugar Bounded Streaming Helpers ⭐

MT5Sugar provides **bounded streaming** methods that automatically limit execution:

### ReadTicks - Limited Tick Streaming

```csharp
// ✅ Automatically stops after 50 events OR 5 seconds
await foreach (var tick in svc.ReadTicks(
    symbols: new[] { "EURUSD", "GBPUSD" },
    maxEvents: 50,
    durationSec: 5))
{
    Console.WriteLine($"{tick.Symbol}: {tick.Bid}");
}
// Stream automatically terminates - no leak!
```

### ReadTrades - Limited Trade Streaming

```csharp
// ✅ Automatically stops after 10 events OR 30 seconds
await foreach (var trade in svc.ReadTrades(
    maxEvents: 10,
    durationSec: 30))
{
    Console.WriteLine($"Trade: {trade.Type}");
}
// Stream automatically terminates - no leak!
```

**See documentation:**
- [ReadTicks.md](MT5Sugar/5.%20Streams_Helpers/ReadTicks.md)
- [ReadTrades.md](MT5Sugar/5.%20Streams_Helpers/ReadTrades.md)

---

## Choosing the Right Pattern

| Your Use Case | Recommended Pattern | Why |
|---------------|---------------------|-----|
| **Quick test/demo** | Pattern 1 (Quick Sample) | Simple, automatic timeout |
| **Sample N events** | Pattern 2 (Event-Limited) | Clear exit condition |
| **Monitor until condition** | Pattern 3 (Condition-Based) | Business logic controls lifetime |
| **Background service** | Pattern 4 (Service Class) | Clean OOP design, reusable |
| **Multiple streams** | Pattern 5 (Concurrent) | Coordinated shutdown |
| **Market depth (DOM)** | Pattern 6 (IDisposable) | Built-in cleanup |
| **Production app** | Pattern 4 or 7 | Error handling + graceful shutdown |

**For most cases:** Use **MT5Sugar bounded helpers** (`ReadTicks`, `ReadTrades`) - they handle everything!

---

## Solutions & Best Practices

## Solution 3: IDisposable Pattern for DOM Subscription

Market Book (DOM) subscription uses **IDisposable** for automatic cleanup:

```csharp
// ✅ CORRECT: using statement ensures cleanup
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");
    // Process order book...
}  // ✅ Automatically unsubscribes here
```

**Without `using` - memory leak:**
```csharp
// ❌ BAD: Subscription never cleaned up
await svc.SubscribeToMarketBookAsync("EURUSD");
var book = await svc.GetMarketBookSnapshotAsync("EURUSD");
// ❌ Subscription still active in background!
```

---

## Common Streaming Pitfalls

### ❌ Pitfall 1: No Cancellation Token

```csharp
// ❌ WRONG: Infinite loop, no way to stop
await foreach (var tick in account.OnSymbolTickAsync(symbols))
{
    Console.WriteLine($"{tick.Symbol}: {tick.Bid}");
    // If exception occurs or program exits, stream keeps running!
}
```

**Fix:**
```csharp
// ✅ CORRECT: Can cancel anytime
using var cts = new CancellationTokenSource();
await foreach (var tick in account.OnSymbolTickAsync(symbols, cts.Token))
{
    Console.WriteLine($"{tick.Symbol}: {tick.Bid}");
}
```

---

### ❌ Pitfall 2: Breaking Without Cancellation

```csharp
// ❌ WRONG: Break doesn't stop the stream
await foreach (var tick in account.OnSymbolTickAsync(symbols))
{
    if (tick.Symbol == "EURUSD")
        break;  // ❌ Stream still running!
}
```

**Fix:**
```csharp
// ✅ CORRECT: Cancel before breaking
using var cts = new CancellationTokenSource();
await foreach (var tick in account.OnSymbolTickAsync(symbols, cts.Token))
{
    if (tick.Symbol == "EURUSD")
    {
        cts.Cancel();  // ✅ Stop stream
        break;
    }
}
```

---

### ❌ Pitfall 3: Exception Without Cleanup

```csharp
// ❌ WRONG: Exception leaves stream running
await foreach (var tick in account.OnSymbolTickAsync(symbols))
{
    ProcessTick(tick);  // If this throws, stream keeps running!
}
```

**Fix:**
```csharp
// ✅ CORRECT: try-finally ensures cleanup
using var cts = new CancellationTokenSource();
try
{
    await foreach (var tick in account.OnSymbolTickAsync(symbols, cts.Token))
    {
        ProcessTick(tick);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
finally
{
    cts.Cancel();  // ✅ Ensure stream stops
}
```

---

## Complete Example: Proper Stream Management

### Example 1: Simple Tick Monitoring with Timeout

```csharp
public async Task MonitorTicksAsync(MT5Account account, string[] symbols)
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    int tickCount = 0;
    const int MAX_TICKS = 100;

    try
    {
        Console.WriteLine($"Starting tick monitoring (max {MAX_TICKS} ticks or 30 seconds)...");

        await foreach (var tick in account.OnSymbolTickAsync(symbols, cts.Token))
        {
            Console.WriteLine($"[{++tickCount}] {tick.Symbol}: Bid={tick.Bid}, Ask={tick.Ask}");

            if (tickCount >= MAX_TICKS)
            {
                Console.WriteLine("Max ticks reached - stopping");
                cts.Cancel();
                break;
            }
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine($"Monitoring stopped ({tickCount} ticks processed)");
    }
}
```

---

### Example 2: Trade Event Monitoring with Manual Stop

```csharp
public class TradeMonitor : IDisposable
{
    private CancellationTokenSource? _cts;
    private Task? _monitoringTask;

    public void Start(MT5Account account)
    {
        _cts = new CancellationTokenSource();
        _monitoringTask = MonitorTradesAsync(account, _cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    private async Task MonitorTradesAsync(MT5Account account, CancellationToken ct)
    {
        try
        {
            await foreach (var trade in account.OnTradeAsync(ct))
            {
                Console.WriteLine($"Trade event: {trade.Type}");
                // Process trade...
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Trade monitoring stopped");
        }
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
        _monitoringTask?.Wait(TimeSpan.FromSeconds(5));
    }
}

// Usage:
using var monitor = new TradeMonitor();
monitor.Start(account);

// ... do other work ...

monitor.Stop();  // ✅ Gracefully stops monitoring
```

---

### Example 3: Using MT5Sugar Bounded Helpers (Recommended)

```csharp
public async Task QuickTickSampleAsync(MT5Service svc)
{
    Console.WriteLine("Reading 20 tick updates (max 10 seconds)...");

    // ✅ No CancellationToken needed - automatically bounded
    await foreach (var tick in svc.ReadTicks(
        symbols: new[] { "EURUSD", "GBPUSD" },
        maxEvents: 20,
        durationSec: 10))
    {
        Console.WriteLine($"{tick.Symbol}: {tick.Bid}");
    }

    Console.WriteLine("Done - stream automatically stopped");
    // ✅ No memory leak - stream properly terminated
}
```

---

## Recommendations

### ✅ DO:

1. **Always use `CancellationToken`** with streaming methods
2. **Use `using` statement** for `CancellationTokenSource`
3. **Set timeout** via `CancellationTokenSource(TimeSpan)`
4. **Use MT5Sugar bounded helpers** (`ReadTicks`, `ReadTrades`) when possible
5. **Use `using` with IDisposable** (e.g., DOM subscription)
6. **Wrap in try-catch** to handle `OperationCanceledException`

### ❌ DON'T:

1. **Never start streaming without CancellationToken**
2. **Never break from loop without calling `Cancel()`**
3. **Never ignore `OperationCanceledException`**
4. **Never forget to dispose `CancellationTokenSource`**
5. **Never assume stream stops automatically on exception**

---

## Testing for Memory Leaks

### Check for Abandoned Streams

```csharp
// Monitor active gRPC calls before/after streaming
var initialCalls = Process.GetCurrentProcess().Threads.Count;

// Your streaming code here...

var finalCalls = Process.GetCurrentProcess().Threads.Count;
Console.WriteLine($"Thread delta: {finalCalls - initialCalls}");
// Should be ~0 if properly cleaned up
```

### Use Diagnostic Tools

- **Visual Studio Diagnostic Tools** - Monitor live objects
- **dotMemory** - Track memory growth over time
- **PerfView** - Analyze thread activity

---

## Architecture Notes

### How C# Streaming Works in MT5Account

**Data flow:**

```
User Code (await foreach)
    ↓
IAsyncEnumerable<T> (MT5Account.OnSymbolTickAsync)
    ↓
ExecuteStreamWithReconnect (try-finally with stream.Dispose) ← ✅ Auto-cleanup here!
    ↓
gRPC AsyncServerStreamingCall (Protobuf streaming)
    ↓
Network (to MT5 terminal)
```

**CancellationToken propagates:**
```
User Code → IAsyncEnumerable → gRPC Call → Network
```

**When you cancel (or break):**

1. `CancellationToken` triggers OR iterator exits
2. `IAsyncEnumerable` stops yielding
3. **`finally { stream?.Dispose() }` executes** ✅ ← BUILT-IN CLEANUP!
4. gRPC call is cancelled
5. Network connection closes
6. Resources freed ✅

**MT5Account has built-in cleanup** - safer than raw gRPC!

### MT5Account Cleanup Mechanism

The `ExecuteStreamWithReconnect` method ensures proper cleanup:

```csharp
// In MT5Account.cs (lines 254-323)
private async IAsyncEnumerable<TData> ExecuteStreamWithReconnect<TRequest, TReply, TData>(
    TRequest request,
    Func<TRequest, Metadata, CancellationToken, AsyncServerStreamingCall<TReply>> streamInvoker,
    Func<TReply, Mt5TermApi.Error?> getError,
    Func<TReply, TData?> getData,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        AsyncServerStreamingCall<TReply>? stream = null;
        try
        {
            stream = streamInvoker(request, GetHeaders(), cancellationToken);
            // ... streaming logic ...
        }
        finally
        {
            stream?.Dispose();  // ✅ ALWAYS executes - no leaks!
        }
    }
}
```

**This means:**

- ✅ Even if you `break` without calling `Cancel()`, cleanup still happens
- ✅ Even if exception occurs, `finally` ensures cleanup
- ✅ The implementation is **safer than raw gRPC** streaming
- ✅ **You still should use `CancellationToken` for clean shutdown**, but forgetting won't cause a leak

---

## Summary

### Quick Decision Tree

```
Need streaming?
├─ Yes → Short sample (< 1 minute)?
│  ├─ Yes → Use ReadTicks() or ReadTrades() ✅ EASIEST
│  └─ No → Need background service?
│     ├─ Yes → Use Pattern 4 (Service Class) ✅ PRODUCTION
│     └─ No → Use Pattern 1-3 with CancellationToken ✅ SIMPLE
└─ No → Use regular async methods
```

### Golden Rules for Streaming

1. **Prefer MT5Sugar bounded helpers** (`ReadTicks`, `ReadTrades`) - handles everything automatically
2. **Always pass `CancellationToken`** when using low-level `OnSymbolTickAsync` etc.
3. **Use `using` for `CancellationTokenSource`** - ensures disposal
4. **Call `Cancel()` before exiting** - graceful shutdown
5. **Wrap in try-catch** for `OperationCanceledException`
6. **Test for leaks** during development

### Key Takeaways

✅ **MT5Account has built-in safety:**

- `finally { stream?.Dispose() }` prevents leaks even if you forget to cancel
- Still recommend using `CancellationToken` for clean shutdown

✅ **Best practices:**

- **Quick tests:** `ReadTicks()` / `ReadTrades()`
- **Production:** Pattern 4 (Service class with IDisposable)
- **Custom logic:** Patterns 1-3 with `CancellationToken`

✅ **What to avoid:**

- ❌ Streaming without `CancellationToken` (works but not clean)
- ❌ Breaking loop without calling `Cancel()` first
- ❌ Ignoring `OperationCanceledException`
- ❌ DOM subscription without `using` statement

**Remember:**

- gRPC streams are **active network connections**
- MT5Account's `finally` block **prevents memory leaks**
- But `CancellationToken` provides **graceful shutdown**
- Use **MT5Sugar bounded helpers** whenever possible!

---

## See Also

- **[MT5Sugar ReadTicks](MT5Sugar/5.%20Streams_Helpers/ReadTicks.md)** - Bounded tick streaming
- **[MT5Sugar ReadTrades](MT5Sugar/5.%20Streams_Helpers/ReadTrades.md)** - Bounded trade streaming
- **[GRPC Best Practices](https://grpc.io/docs/guides/performance/)** - Official gRPC guidance

🎯 **Bottom line:** Always use `CancellationToken` with streaming, or use MT5Sugar bounded helpers!
