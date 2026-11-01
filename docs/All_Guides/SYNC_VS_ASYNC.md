# Synchronous vs Asynchronous Methods - When to Use What (C#)

> Understanding execution models in CSharpMT5: non-blocking streaming vs synchronous execution.

---

## 🎯 Quick Comparison

All I/O, gRPC queries, and streaming in C# use async/await with `Task` and `IAsyncEnumerable<T>`. Synchronous variants exist for legacy desktop scenarios.

| Aspect | Asynchronous Pattern | Synchronous Call |
|--------|----------------------|------------------|
| **Thread Blocking** | ❌ Non-blocking (high concurrency) | ✅ Blocks current thread |
| **Throughput** | ✅ Handles thousands of events/sec | ❌ Limited by thread pool |
| **Real-Time Data** | ✅ Perfect for tick & trade streams | ⚠️ Inefficient for streams |
| **Simplicity** | Requires async runtime awareness | Simple, linear execution |
| **Recommended for** | Automated bots, microservices, GUIs | CLI scripts, notebooks, one-offs |

---

## 🚀 When to Use Asynchronous Methods (Recommended)

### 1. Market Data Streaming

Market ticks arrive at microsecond intervals during peak sessions. Streaming methods return `IAsyncEnumerable<T>`; consume them with `await foreach` so your thread is never blocked while waiting for the next tick:

```csharp
var account = new MT5Account(user, password, "https://mt5.mrpc.pro:443", apiKey);
await account.ConnectByServerNameAsync("YourBroker-Demo", baseChartSymbol: "EURUSD");

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
try
{
    await foreach (var tickData in account.OnSymbolTickAsync(new[] { "EURUSD" }, cts.Token))
        Console.WriteLine($"Bid={tickData.SymbolTick.Bid:F5}, Ask={tickData.SymbolTick.Ask:F5}");
}
catch (OperationCanceledException) { /* stopped after timeout */ }

await account.DisconnectAsync();
```

See the [gRPC Streaming guide](./GRPC_STREAM_MANAGEMENT.md) for full subscription patterns.

### 2. High-Frequency Order Execution

When operating across multiple currency pairs simultaneously, asynchronous dispatch lets your bot fan out requests concurrently instead of sequentially:

```csharp
// Query several symbols at once instead of one-by-one
var ticks = await Task.WhenAll(
    new[] { "EURUSD", "GBPUSD", "USDJPY" }.Select(s => account.SymbolInfoTickAsync(s)));

foreach (var t in ticks)
    Console.WriteLine($"{t.Bid:F5} / {t.Ask:F5}");
```

---

## 🐢 When to Use Synchronous Methods

Every async method has a blocking twin — drop the `Async` suffix: `AccountSummary()`, `SymbolInfoTick("EURUSD")`, `OrderSend(request)`, `ConnectByServerName(...)`, `Disconnect()`. Each simply calls the async version and waits, which is convenient for linear scripts, REPL/notebook exploration, or small one-off CLI tools where concurrency doesn't matter.

```csharp
var account = new MT5Account(user, password, "https://mt5.mrpc.pro:443", apiKey);
account.ConnectByServerName("YourBroker-Demo", "EURUSD");   // blocks until connected

var summary = account.AccountSummary();                     // blocks until the reply arrives
Console.WriteLine($"Balance: {summary.AccountBalance} {summary.AccountCurrency}");

account.Disconnect();
```

**Caution:** don't call a synchronous method on a UI thread or from inside an `async` context — blocking on a `Task` there can deadlock and defeats the concurrency async buys you. In those cases always `await` the async variant. Streaming has **no** synchronous form — subscriptions are always consumed with `await foreach`.

---

## 💡 Summary

Always prefer asynchronous paradigms for production bots, multi-symbol trading, and background services. Use synchronous wrappers for quick setup scripts, testing, or exploratory analysis.
