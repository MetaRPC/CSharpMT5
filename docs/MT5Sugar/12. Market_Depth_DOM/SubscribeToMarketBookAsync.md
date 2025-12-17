# Subscribe to Market Book (`SubscribeToMarketBookAsync`)

> **Sugar method:** Subscribes to Market Depth (order book) with automatic cleanup via IDisposable pattern.

**API Information:**

* **Extension method:** `MT5Service.SubscribeToMarketBookAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Region:** [12] MARKET DEPTH (DOM)
* **Underlying calls:** `MarketBookAddAsync()` + custom `MarketBookSubscription` wrapper

---

## Method Signature

```csharp
public static async Task<IDisposable> SubscribeToMarketBookAsync(
    this MT5Service svc,
    string symbol,
    int timeoutSec = 15,
    CancellationToken ct = default)
```

---

## 🔽 Input

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance (extension method) |
| `symbol` | `string` | Symbol to subscribe (e.g., "EURUSD", "XAUUSD") |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<IDisposable>` | Disposable subscription that auto-releases when disposed |

**Key behavior:**

- When disposed, automatically calls `MarketBookReleaseAsync()`
- Use with `using` statement for automatic cleanup

---

## 💬 Just the essentials

* **What it is:** Subscribes to real-time order book (DOM) data for a symbol with automatic cleanup.
* **Why you need it:** Access Level II market data for scalping, liquidity analysis, and advanced order placement.
* **Pattern:** Use with `using` block - automatically unsubscribes when block exits.

---

## 🎯 Purpose

Use it for:

* **Scalping** - See real liquidity and order flow
* **Large order execution** - Check available liquidity before placing big orders
* **Market microstructure analysis** - Study bid/ask dynamics
* **Iceberg order detection** - Find hidden large orders
* **Support/Resistance detection** - Identify order clusters

---

## 🔧 Under the Hood

```csharp
// Step 1: Subscribe to market book
await svc.MarketBookAddAsync(symbol, Dl(timeoutSec), ct);

// Step 2: Return disposable wrapper
return new MarketBookSubscription(svc, symbol, timeoutSec);

// Internal MarketBookSubscription class:
private class MarketBookSubscription : IDisposable
{
    private readonly MT5Service _svc;
    private readonly string _symbol;
    private readonly int _timeoutSec;
    private bool _disposed;

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            // Fire-and-forget release (cannot await in Dispose)
            _ = _svc.MarketBookReleaseAsync(_symbol, Dl(_timeoutSec), default);
        }
    }
}
```

**What it improves:**

* **Automatic cleanup** - No manual unsubscribe needed
* **Resource safety** - Guaranteed cleanup even on exception
* **Clean syntax** - `using` block pattern
* **Fire-and-forget release** - Non-blocking dispose

---

## 🔗 Usage Examples

### Example 1: Basic Subscription

```csharp
// Subscribe to EURUSD order book
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");

    Console.WriteLine($"Order book entries: {book.MqlBookInfos.Count}");

    foreach (var entry in book.MqlBookInfos.Take(5))
    {
        string side = entry.Type == BookType.Buy ? "BID" : "ASK";
        Console.WriteLine($"{side}: {entry.Price:F5} - {entry.Volume} lots");
    }
} // Auto-unsubscribes here

// Output:
// Order book entries: 20
// BID: 1.08520 - 100 lots
// BID: 1.08519 - 250 lots
// BID: 1.08518 - 150 lots
// ASK: 1.08523 - 120 lots
// ASK: 1.08524 - 180 lots
```

---

### Example 2: Find Best Bid/Ask from Book

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var (bestBid, bestAsk) = await svc.GetBestBidAskFromBookAsync("EURUSD");

    double spread = bestAsk - bestBid;
    Console.WriteLine($"Best Bid: {bestBid:F5}");
    Console.WriteLine($"Best Ask: {bestAsk:F5}");
    Console.WriteLine($"Spread: {spread:F5}");
}

// Output:
// Best Bid: 1.08520
// Best Ask: 1.08523
// Spread: 0.00003
```

---

### Example 3: Check Liquidity Before Large Order

```csharp
public async Task<bool> HasEnoughLiquidity(
    MT5Service svc,
    string symbol,
    double targetVolume)
{
    using (await svc.SubscribeToMarketBookAsync(symbol))
    {
        var book = await svc.GetMarketBookSnapshotAsync(symbol);

        // Sum top 3 ask levels
        long totalAskVolume = book.MqlBookInfos
            .Where(b => b.Type == BookType.Sell)
            .OrderBy(b => b.Price)
            .Take(3)
            .Sum(b => b.Volume);

        Console.WriteLine($"Available liquidity (top 3 levels): {totalAskVolume} lots");
        Console.WriteLine($"Target volume: {targetVolume} lots");

        return totalAskVolume >= targetVolume;
    }
}

// Usage:
bool canExecute = await HasEnoughLiquidity(svc, "EURUSD", targetVolume: 500);
if (canExecute)
{
    Console.WriteLine("✅ Sufficient liquidity - placing order");
}
else
{
    Console.WriteLine("⚠️ Insufficient liquidity - may cause slippage");
}
```

---

### Example 4: Find Order Book Imbalance

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");

    long totalBidVolume = book.MqlBookInfos
        .Where(b => b.Type == BookType.Buy)
        .Sum(b => b.Volume);

    long totalAskVolume = book.MqlBookInfos
        .Where(b => b.Type == BookType.Sell)
        .Sum(b => b.Volume);

    double imbalance = (double)totalBidVolume / totalAskVolume;

    Console.WriteLine($"Total Bid Volume: {totalBidVolume}");
    Console.WriteLine($"Total Ask Volume: {totalAskVolume}");
    Console.WriteLine($"Bid/Ask Imbalance: {imbalance:F2}");

    if (imbalance > 1.5)
    {
        Console.WriteLine("🔼 Strong buying pressure");
    }
    else if (imbalance < 0.67)
    {
        Console.WriteLine("🔽 Strong selling pressure");
    }
    else
    {
        Console.WriteLine("➡️ Balanced order book");
    }
}
```

---

### Example 5: Monitor Order Book in Real-Time Loop

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    for (int i = 0; i < 10; i++)
    {
        var (bid, ask) = await svc.GetBestBidAskFromBookAsync("EURUSD");
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Bid: {bid:F5}, Ask: {ask:F5}");

        await Task.Delay(1000);  // Update every second
    }
} // Auto-cleanup after loop

// Output:
// [14:30:00] Bid: 1.08520, Ask: 1.08523
// [14:30:01] Bid: 1.08521, Ask: 1.08523
// [14:30:02] Bid: 1.08520, Ask: 1.08524
// ...
```

---

### Example 6: Find Support/Resistance from Order Clusters

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");

    // Find largest bid cluster (potential support)
    var largestBid = book.MqlBookInfos
        .Where(b => b.Type == BookType.Buy)
        .OrderByDescending(b => b.Volume)
        .First();

    // Find largest ask cluster (potential resistance)
    var largestAsk = book.MqlBookInfos
        .Where(b => b.Type == BookType.Sell)
        .OrderByDescending(b => b.Volume)
        .First();

    Console.WriteLine($"🛡️ Support (largest bid): {largestBid.Price:F5} ({largestBid.Volume} lots)");
    Console.WriteLine($"🚧 Resistance (largest ask): {largestAsk.Price:F5} ({largestAsk.Volume} lots)");
}
```

---

### Example 7: Multiple Symbol Subscriptions

```csharp
// Subscribe to multiple symbols simultaneously
var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY" };
var subscriptions = new List<IDisposable>();

try
{
    // Subscribe to all
    foreach (var symbol in symbols)
    {
        subscriptions.Add(await svc.SubscribeToMarketBookAsync(symbol));
    }

    // Analyze all books
    foreach (var symbol in symbols)
    {
        var (bid, ask) = await svc.GetBestBidAskFromBookAsync(symbol);
        Console.WriteLine($"{symbol}: {bid:F5} / {ask:F5}");
    }
}
finally
{
    // Cleanup all subscriptions
    foreach (var sub in subscriptions)
    {
        sub.Dispose();
    }
}
```

---

### Example 8: Exception Handling with Guaranteed Cleanup

```csharp
try
{
    using (await svc.SubscribeToMarketBookAsync("EURUSD"))
    {
        var book = await svc.GetMarketBookSnapshotAsync("EURUSD");

        if (book.MqlBookInfos.Count == 0)
        {
            throw new InvalidOperationException("Empty order book!");
        }

        // Process book...
    }
} // Dispose called even if exception occurs
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
}
```

---

### Example 9: Calculate Weighted Average Price from Book

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");

    // Calculate VWAP (Volume Weighted Average Price) from ask side
    var asks = book.MqlBookInfos.Where(b => b.Type == BookType.Sell).ToList();

    double totalValue = asks.Sum(a => a.Price * a.Volume);
    long totalVolume = asks.Sum(a => a.Volume);

    double vwap = totalValue / totalVolume;

    Console.WriteLine($"Ask-side VWAP: {vwap:F5}");
    Console.WriteLine($"Based on {totalVolume} lots across {asks.Count} levels");
}
```

---

### Example 10: Async Using Declaration (C# 8.0+)

```csharp
// Modern C# 8.0+ syntax - auto-disposes at end of scope
await using var subscription = await svc.SubscribeToMarketBookAsync("EURUSD");

var book = await svc.GetMarketBookSnapshotAsync("EURUSD");
Console.WriteLine($"Book depth: {book.MqlBookInfos.Count} levels");

// Process book data...

// Auto-disposes here (end of method/scope)
```

---

## 🔗 Related Methods

**📦 Methods used internally:**

* `MarketBookAddAsync()` - Low-level subscription
* `MarketBookReleaseAsync()` - Low-level unsubscription

**🍬 Related Sugar methods:**

* `GetMarketBookSnapshotAsync()` - Get current order book snapshot
* `GetBestBidAskFromBookAsync()` - Extract best bid/ask from book
* `CalculateLiquidityAtLevelAsync()` - Calculate volume at price level

**📊 Alternative approaches:**

* `MarketBookAddAsync()` + manual `MarketBookReleaseAsync()` - More control, but manual cleanup

---

## ⚠️ Common Pitfalls

1. **Forgetting to dispose:**
   ```csharp
   // ❌ WRONG: Memory leak - never unsubscribes!
   var sub = await svc.SubscribeToMarketBookAsync("EURUSD");
   var book = await svc.GetMarketBookSnapshotAsync("EURUSD");
   // sub never disposed!

   // ✅ CORRECT: Use using block
   using (await svc.SubscribeToMarketBookAsync("EURUSD"))
   {
       var book = await svc.GetMarketBookSnapshotAsync("EURUSD");
   } // Auto-cleanup
   ```

2. **Using book data after unsubscribe:**
   ```csharp
   MarketBookGetData book;

   using (await svc.SubscribeToMarketBookAsync("EURUSD"))
   {
       book = await svc.GetMarketBookSnapshotAsync("EURUSD");
   } // Unsubscribed here

   // ⚠️ Book data may be stale - subscription is closed
   Console.WriteLine(book.MqlBookInfos.Count);

   // ✅ CORRECT: Process data inside using block
   ```

3. **Not checking if broker supports DOM:**
   ```csharp
   // ⚠️ Not all brokers provide Level II data
   try
   {
       using (await svc.SubscribeToMarketBookAsync("EURUSD"))
       {
           var book = await svc.GetMarketBookSnapshotAsync("EURUSD");

           if (book.MqlBookInfos.Count == 0)
           {
               Console.WriteLine("⚠️ Broker doesn't provide DOM data");
           }
       }
   }
   catch (Exception ex)
   {
       Console.WriteLine($"❌ DOM not supported: {ex.Message}");
   }
   ```

4. **Subscribing to same symbol multiple times:**
   ```csharp
   // ⚠️ Redundant - MT5 handles multiple subscriptions, but wasteful
   using (await svc.SubscribeToMarketBookAsync("EURUSD"))
   using (await svc.SubscribeToMarketBookAsync("EURUSD"))  // Redundant
   {
       // Only need one subscription per symbol
   }

   // ✅ CORRECT: One subscription per symbol
   using (await svc.SubscribeToMarketBookAsync("EURUSD"))
   {
       // Use single subscription
   }
   ```

5. **Assuming real-time updates without polling:**
   ```csharp
   // ⚠️ GetMarketBookSnapshotAsync returns snapshot, not live stream
   using (await svc.SubscribeToMarketBookAsync("EURUSD"))
   {
       var book = await svc.GetMarketBookSnapshotAsync("EURUSD");
       // This book is a snapshot - won't update automatically

       // Must poll to see changes:
       await Task.Delay(1000);
       book = await svc.GetMarketBookSnapshotAsync("EURUSD");  // New snapshot
   }
   ```

---

## 💡 Summary

**SubscribeToMarketBookAsync** provides clean DOM subscription with automatic cleanup:

* ✅ IDisposable pattern for automatic unsubscribe
* ✅ Use with `using` block for guaranteed cleanup
* ✅ Access to Level II market data
* ✅ Essential for scalping and large order execution
* ✅ Works with all other DOM methods

```csharp
// Clean pattern:
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");
    // Analyze order book...
} // Auto-cleanup guaranteed
```

**Professional DOM access made simple!** 📊
