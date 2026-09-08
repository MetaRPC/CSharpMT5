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
Market ticks arrive at microsecond intervals during peak sessions. Asynchronous handlers ensure zero tick drops without freezing your execution thread:

```
var account = new MT5Account(user, password, grpcServer, null);
await account.ConnectByServerNameAsync(serverName, "EURUSD", 30);
var summary = await account.AccountSummaryAsync();
Console.WriteLine($"Balance: {summary.AccountBalance}, Equity: {summary.AccountEquity}");
```

### 2. High-Frequency Order Execution
When operating across multiple currency pairs simultaneously, asynchronous dispatch allows your bot to send orders concurrently rather than sequentially.

---

## 💡 Summary

Always prefer asynchronous paradigms for production bots, multi-symbol trading, and background services. Use synchronous wrappers for quick setup scripts, testing, or exploratory analysis.
