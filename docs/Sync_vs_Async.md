# Synchronous vs Asynchronous Methods - When to Use What

> Every method in MT5Account has **two versions**: `Async` (e.g., `AccountSummaryAsync`) and **Sync** (e.g., `AccountSummary`). This guide explains when to use each.

---

## 🎯 Quick Answer

**Use Async version (99% of cases):**
```csharp
var summary = await account.AccountSummaryAsync();  // ✅ Recommended
```

**Use Sync version (rare specific scenarios):**
```csharp
var summary = account.AccountSummary();  // ⚠️ Only when necessary
```

---

## 📊 Side-by-Side Comparison

| Aspect | Async Version (`await MethodAsync()`) | Sync Version (`Method()`) |
|--------|--------------------------------------|---------------------------|
| **Thread blocking** | ❌ Does NOT block thread | ✅ Blocks calling thread |
| **Performance** | ✅ Better (thread pooling) | ❌ Worse (one thread per call) |
| **Scalability** | ✅ High (1 thread = 1000s of operations) | ❌ Low (1 thread = 1 operation) |
| **Responsiveness** | ✅ UI stays responsive | ❌ UI freezes during call |
| **Memory usage** | ✅ Lower (fewer threads) | ❌ Higher (more threads) |
| **Deadlock risk** | ✅ Lower | ❌ Higher (especially in UI) |
| **Recommended by** | ✅ Microsoft, .NET guidelines | ⚠️ Legacy compatibility only |
| **When to use** | Almost always | Very specific scenarios |

---

## 🚀 When to Use ASYNC (Recommended)

### ✅ Use Case 1: ASP.NET / Web APIs
```csharp
// Web API Controller
[HttpGet("balance")]
public async Task<ActionResult<double>> GetBalance()
{
    // ✅ Async frees thread for other requests
    var summary = await _mt5Account.AccountSummaryAsync();
    return Ok(summary.AccountBalance);
}
```

**Why:** In web servers, threads are expensive. Async allows one thread to handle thousands of concurrent requests.

**Result:**

- 🚀 **100 threads** can handle **10,000+ concurrent requests**
- Without async: 100 threads = only 100 concurrent requests

---

### ✅ Use Case 2: Desktop UI (WPF, WinForms, Avalonia)
```csharp
// Button click handler
private async void OnCheckBalanceClick(object sender, EventArgs e)
{
    // ✅ UI thread stays responsive
    var summary = await _mt5Account.AccountSummaryAsync();
    BalanceLabel.Text = $"Balance: ${summary.AccountBalance}";
}
```

**Why:** Async keeps UI responsive. User can still interact while waiting for MT5 response.

**Result:**

- ✅ UI doesn't freeze
- ✅ User can cancel operation
- ✅ Better user experience

---

### ✅ Use Case 3: Trading Bots / Strategy Execution
```csharp
// Trading strategy
public async Task ExecuteStrategyAsync()
{
    // ✅ Can monitor multiple symbols concurrently
    var tasks = symbols.Select(async symbol =>
    {
        var tick = await _mt5Account.SymbolInfoTickAsync(symbol);
        return AnalyzeSignal(tick);
    });

    var signals = await Task.WhenAll(tasks);
}
```

**Why:** Process multiple symbols in parallel without blocking threads.

**Result:**

- 🚀 **10 symbols analyzed in ~1 second** (concurrent)
- Without async: 10 symbols = ~10 seconds (sequential)

---

### ✅ Use Case 4: Real-Time Streaming
```csharp
// Real-time tick monitoring
await foreach (var tick in _mt5Account.OnSymbolTickAsync(symbols, cancellationToken))
{
    // ✅ Non-blocking stream processing
    ProcessTick(tick);
}
```

**Why:** Streaming is inherently asynchronous. Can't do it with sync methods.

**Result:**

- ✅ Continuous data flow
- ✅ Cancellable streams
- ✅ No thread blocking

---

## ⚠️ When to Use SYNC (Rare Cases)

### 🟡 Use Case 1: Console Applications (Quick Scripts)
```csharp
// Simple one-off script
static void Main()
{
    var account = new MT5Account(...);
    account.Connect();

    // ✅ Acceptable for simple scripts
    var summary = account.AccountSummary();
    Console.WriteLine($"Balance: {summary.AccountBalance}");
}
```

**Why:** For simple scripts that run once and exit, blocking is acceptable.

**When acceptable:**

- Script runs once and exits
- No UI, no web server
- Not performance-critical
- **Note:** Even here, async Main is better (C# 7.1+)

**Better alternative:**
```csharp
static async Task Main()
{
    var account = new MT5Account(...);
    await account.ConnectAsync();

    var summary = await account.AccountSummaryAsync();  // Still better!
    Console.WriteLine($"Balance: {summary.AccountBalance}");
}
```

---

### 🟡 Use Case 2: Legacy Code Integration
```csharp
// Old library that doesn't support async
public class LegacyTradingSystem
{
    private MT5Account _account;

    // Old interface - can't change signature
    public double GetBalance()
    {
        // ⚠️ Forced to use sync version
        return _account.AccountSummary().AccountBalance;
    }
}
```

**Why:** Existing codebase can't be modified to support async/await.

**When acceptable:**

- Third-party library constraints
- Can't change method signatures
- Gradual migration to async

**Better alternative:**

- Wrap in async layer when possible
- Plan migration to async API

---

### 🟡 Use Case 3: Synchronous Constructors
```csharp
public class TradingContext
{
    public double InitialBalance { get; }

    public TradingContext(MT5Account account)
    {
        // ❌ Can't use async in constructor
        // ⚠️ Forced to use sync version
        InitialBalance = account.AccountSummary().AccountBalance;
    }
}
```

**Why:** C# constructors can't be async.

**When acceptable:**

- Initialization code in constructors
- Static initializers

**Better alternative:**
```csharp
public class TradingContext
{
    public double InitialBalance { get; private set; }

    private TradingContext() { }

    // ✅ Static async factory method
    public static async Task<TradingContext> CreateAsync(MT5Account account)
    {
        var context = new TradingContext();
        var summary = await account.AccountSummaryAsync();
        context.InitialBalance = summary.AccountBalance;
        return context;
    }
}
```

---

### 🟡 Use Case 4: Unit Tests (Rare)
```csharp
[Test]
public void TestAccountBalance()
{
    var account = CreateMockAccount();

    // ⚠️ Some test frameworks don't support async tests well
    var balance = account.AccountSummary().AccountBalance;

    Assert.AreEqual(10000.0, balance);
}
```

**Why:** Some older test frameworks have poor async support.

**Better alternative:**
```csharp
[Test]
public async Task TestAccountBalance()
{
    var account = CreateMockAccount();

    // ✅ Modern test frameworks support async
    var summary = await account.AccountSummaryAsync();

    Assert.AreEqual(10000.0, summary.AccountBalance);
}
```

---

## ❌ When NOT to Use SYNC (Common Mistakes)

### ❌ Mistake 1: UI Thread Blocking
```csharp
// ❌ WRONG - Freezes UI!
private void OnButtonClick(object sender, EventArgs e)
{
    var summary = _account.AccountSummary();  // UI freezes here!
    BalanceLabel.Text = $"${summary.AccountBalance}";
}

// ✅ CORRECT - UI stays responsive
private async void OnButtonClick(object sender, EventArgs e)
{
    var summary = await _account.AccountSummaryAsync();
    BalanceLabel.Text = $"${summary.AccountBalance}";
}
```

---

### ❌ Mistake 2: Deadlock in ASP.NET
```csharp
// ❌ WRONG - Can cause deadlock!
[HttpGet("balance")]
public ActionResult<double> GetBalance()
{
    var summary = _account.AccountSummary();  // Deadlock risk!
    return Ok(summary.AccountBalance);
}

// ✅ CORRECT - No deadlock
[HttpGet("balance")]
public async Task<ActionResult<double>> GetBalance()
{
    var summary = await _account.AccountSummaryAsync();
    return Ok(summary.AccountBalance);
}
```

---

### ❌ Mistake 3: Poor Scalability
```csharp
// ❌ WRONG - Blocks 10 threads!
var results = symbols.Select(symbol =>
{
    return _account.SymbolInfoTick(symbol);  // Each call blocks a thread
}).ToList();

// ✅ CORRECT - Concurrent, no blocking
var tasks = symbols.Select(symbol =>
    _account.SymbolInfoTickAsync(symbol)
);
var results = await Task.WhenAll(tasks);
```

---

## 🔬 Technical Deep Dive

### How Async Works
```csharp
// When you call:
var summary = await account.AccountSummaryAsync();

// What happens:
// 1. Thread sends gRPC request to MT5
// 2. Thread is RELEASED back to thread pool (can handle other work)
// 3. When MT5 responds, ANY available thread picks up the result
// 4. Execution continues after 'await'
```

**Key point:** Thread doesn't wait idle. It's freed to do other work.

---

### How Sync Works
```csharp
// When you call:
var summary = account.AccountSummary();

// What happens:
// 1. Thread sends gRPC request to MT5
// 2. Thread BLOCKS and waits (can't do anything else)
// 3. When MT5 responds, same thread continues
// 4. Thread was wasted during wait time
```

**Key point:** Thread is blocked and wasted during I/O wait.

---

## 📈 Performance Impact

### Scenario: Web API with 1000 concurrent requests

**Using Async:**
```
Threads needed: ~10-20
Memory usage: ~50 MB
Response time: ~100ms average
Result: ✅ All 1000 requests handled smoothly
```

**Using Sync:**
```
Threads needed: 1000
Memory usage: ~1 GB
Response time: ~500ms average (thread starvation)
Result: ❌ Server crashes or rejects requests
```

---

## 🎓 Best Practices

### ✅ DO:
- Use async/await for ALL I/O operations (MT5 calls, database, HTTP)
- Use `ConfigureAwait(false)` in libraries (not in UI code)
- Always pass `CancellationToken` for long-running operations
- Use async Main in console apps (C# 7.1+)

### ❌ DON'T:
- Don't use `.Result` or `.Wait()` on async methods (causes deadlocks)
- Don't mix sync and async code unnecessarily
- Don't use sync methods in UI threads
- Don't use sync methods in ASP.NET controllers

---

## 🔗 Method Naming Convention

All methods follow this pattern:

| Pattern | Example | When to Use |
|---------|---------|-------------|
| `MethodAsync()` | `AccountSummaryAsync()` | Default choice (99% of cases) |
| `Method()` | `AccountSummary()` | Rare specific scenarios only |

**Suffix `Async`** = This method is asynchronous (recommended)
**No suffix** = This method is synchronous (compatibility only)

---

## 📚 Real-World Examples

### Example 1: Trading Bot (Async - Correct)
```csharp
public class TradingBot
{
    private readonly MT5Account _account;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        // ✅ Monitors multiple streams concurrently
        await foreach (var tick in _account.OnSymbolTickAsync(_symbols, cancellationToken))
        {
            // Analyze tick
            var signal = await AnalyzeTickAsync(tick);

            if (signal.ShouldTrade)
            {
                // Place order
                await _account.OrderSendAsync(CreateOrder(signal));
            }
        }
    }
}
```

---

### Example 2: Price Monitor Dashboard (Async - Correct)
```csharp
public class PriceMonitor
{
    public async Task<List<SymbolPrice>> GetAllPricesAsync(string[] symbols)
    {
        // ✅ Fetch all symbols concurrently (parallel)
        var tasks = symbols.Select(async symbol =>
        {
            var tick = await _account.SymbolInfoTickAsync(symbol);
            return new SymbolPrice
            {
                Symbol = symbol,
                Bid = tick.Bid,
                Ask = tick.Ask,
                Time = tick.Time.ToDateTime()
            };
        });

        return (await Task.WhenAll(tasks)).ToList();
    }
}
```

**Performance:**

- Async: Fetches 100 symbols in ~1 second (parallel)
- Sync: Fetches 100 symbols in ~100 seconds (sequential)

---

## 🎯 Decision Tree

```
Need to call MT5 method?
│
├─ Is this a UI application?
│  └─ YES → Use Async (keeps UI responsive)
│
├─ Is this a web application/API?
│  └─ YES → Use Async (better scalability)
│
├─ Is this a trading bot/long-running service?
│  └─ YES → Use Async (better performance)
│
├─ Is this a simple one-off script?
│  └─ YES → Use Async Main (C# 7.1+) OR Sync if really necessary
│
└─ Are you forced by legacy constraints?
   └─ YES → Use Sync temporarily, plan migration to Async
```

---

## 📖 Further Reading

* [Microsoft: Async/Await Best Practices](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
* [Stephen Cleary: Don't Block on Async Code](https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html)
* [Task-based Asynchronous Pattern (TAP)](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)

---

## 💡 Summary

| Question | Answer |
|----------|--------|
| **Which should I use?** | Async in 99% of cases |
| **Why Async?** | Better performance, scalability, responsiveness |
| **When is Sync okay?** | Simple scripts, legacy integration, constructors (rare) |
| **Main rule?** | If doing I/O (network, disk, database) → use Async |

---

**Remember:** Async is not harder, it's just different. Once you understand `async/await`, you'll never want to go back to blocking code! 🚀
