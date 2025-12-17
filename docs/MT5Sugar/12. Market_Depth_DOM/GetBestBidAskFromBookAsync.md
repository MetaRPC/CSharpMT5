# Get Best Bid/Ask from Book (`GetBestBidAskFromBookAsync`)

> **Sugar method:** Extracts best bid and best ask prices from order book - no manual LINQ needed!

**API Information:**

* **Extension method:** `MT5Service.GetBestBidAskFromBookAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Region:** [12] MARKET DEPTH (DOM)
* **Underlying calls:** `MarketBookGetAsync()` + LINQ to find best prices

---

## Method Signature

```csharp
public static async Task<(double bestBid, double bestAsk)> GetBestBidAskFromBookAsync(
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
| `symbol` | `string` | Symbol to query (e.g., "EURUSD") |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<(double, double)>` | Tuple: (bestBid, bestAsk) |

**Return values:**

- `bestBid` - Highest bid price (or 0 if no bids)
- `bestAsk` - Lowest ask price (or 0 if no asks)

---

## 💬 Just the essentials

* **What it is:** Auto-extracts best bid (highest buy price) and best ask (lowest sell price) from order book.
* **Why you need it:** Quick access to top-of-book prices without manual LINQ queries.
* **Returns**: `(0, 0)` if order book is empty.

---

## 🔧 Under the Hood

```csharp
// Step 1: Get order book snapshot
var book = await svc.MarketBookGetAsync(symbol, Dl(timeoutSec), ct);

double bestBid = 0;
double bestAsk = 0;

// Step 2: Find highest bid (BookType.Buy)
var bids = book.MqlBookInfos.Where(b => b.Type == BookType.Buy);
if (bids.Any())
    bestBid = bids.Max(b => b.Price);

// Step 3: Find lowest ask (BookType.Sell)
var asks = book.MqlBookInfos.Where(b => b.Type == BookType.Sell);
if (asks.Any())
    bestAsk = asks.Min(s => s.Price);

return (bestBid, bestAsk);
```

**What it improves:**

* **Auto-finding best prices** - no manual Max/Min queries
* **Handles empty book** - returns (0, 0) safely
* **Clean tuple return** - easy destructuring

---

## 🔗 Usage Examples

### Example 1: Basic Best Bid/Ask

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var (bid, ask) = await svc.GetBestBidAskFromBookAsync("EURUSD");

    Console.WriteLine($"Best Bid: {bid:F5}");
    Console.WriteLine($"Best Ask: {ask:F5}");
    Console.WriteLine($"Spread: {(ask - bid):F5}");
}

// Output:
// Best Bid: 1.08520
// Best Ask: 1.08523
// Spread: 0.00003
```

---

### Example 2: Compare with Tick Data

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    // Get best prices from order book
    var (bookBid, bookAsk) = await svc.GetBestBidAskFromBookAsync("EURUSD");

    // Get tick data
    var tick = await svc.SymbolInfoTickAsync("EURUSD");

    Console.WriteLine("Order Book vs Tick:");
    Console.WriteLine($"  Book Bid: {bookBid:F5} | Tick Bid: {tick.Bid:F5}");
    Console.WriteLine($"  Book Ask: {bookAsk:F5} | Tick Ask: {tick.Ask:F5}");

    // Usually identical, but order book may have slight delays
}
```

---

### Example 3: Spread Monitoring Loop

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    for (int i = 0; i < 10; i++)
    {
        var (bid, ask) = await svc.GetBestBidAskFromBookAsync("EURUSD");
        double spreadPips = (ask - bid) * 100000 / 10;  // For 5-digit EURUSD

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Spread: {spreadPips:F1} pips");

        if (spreadPips > 2.0)
        {
            Console.WriteLine("  ⚠️ High spread - avoid trading");
        }

        await Task.Delay(1000);
    }
}
```

---

### Example 4: Mid-Price Calculation

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var (bid, ask) = await svc.GetBestBidAskFromBookAsync("EURUSD");

    double midPrice = (bid + ask) / 2.0;

    Console.WriteLine($"Bid: {bid:F5}");
    Console.WriteLine($"Ask: {ask:F5}");
    Console.WriteLine($"Mid: {midPrice:F5}");

    // Mid-price used for:
    // - Fair value calculation
    // - TWAP/VWAP benchmarks
    // - Slippage measurement
}
```

---

### Example 5: Multi-Symbol Best Prices

```csharp
var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY" };

foreach (var symbol in symbols)
{
    using (await svc.SubscribeToMarketBookAsync(symbol))
    {
        var (bid, ask) = await svc.GetBestBidAskFromBookAsync(symbol);
        Console.WriteLine($"{symbol,-8} {bid:F5} / {ask:F5}");
    }
}

// Output:
// EURUSD  1.08520 / 1.08523
// GBPUSD  1.27345 / 1.27348
// USDJPY  149.850 / 149.853
```

---

### Example 6: Empty Book Check

```csharp
using (await svc.SubscribeToMarketBookAsync("EXOTIC_PAIR"))
{
    var (bid, ask) = await svc.GetBestBidAskFromBookAsync("EXOTIC_PAIR");

    if (bid == 0 && ask == 0)
    {
        Console.WriteLine("⚠️ Order book is empty or not available");
        Console.WriteLine("  Broker may not provide DOM for this symbol");
    }
    else
    {
        Console.WriteLine($"✅ Best prices: {bid:F5} / {ask:F5}");
    }
}
```

---

### Example 7: Slippage Estimation

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var (bid, ask) = await svc.GetBestBidAskFromBookAsync("EURUSD");

    // For market buy, you pay the ask
    double entryPrice = ask;

    // Estimate slippage if placing large order
    double targetVolume = 500;  // 5.00 lots
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");

    var asks = book.MqlBookInfos
        .Where(b => b.Type == BookType.Sell)
        .OrderBy(b => b.Price)
        .ToList();

    long cumulative = 0;
    double worstFill = ask;

    foreach (var level in asks)
    {
        cumulative += level.Volume;
        worstFill = level.Price;

        if (cumulative >= targetVolume)
            break;
    }

    double slippagePips = (worstFill - ask) * 100000 / 10;

    Console.WriteLine($"Best Ask: {ask:F5}");
    Console.WriteLine($"Worst Fill (500 lots): {worstFill:F5}");
    Console.WriteLine($"Estimated Slippage: {slippagePips:F1} pips");
}
```

---

### Example 8: Market Direction from Book Pressure

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var (bid, ask) = await svc.GetBestBidAskFromBookAsync("EURUSD");
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");

    // Compare volume at best bid vs best ask
    long bidVolume = await svc.CalculateLiquidityAtLevelAsync("EURUSD", bid, isBuy: true);
    long askVolume = await svc.CalculateLiquidityAtLevelAsync("EURUSD", ask, isBuy: false);

    Console.WriteLine($"Best Bid ({bid:F5}): {bidVolume} lots");
    Console.WriteLine($"Best Ask ({ask:F5}): {askVolume} lots");

    if (bidVolume > askVolume * 1.5)
    {
        Console.WriteLine("🔼 Strong buying pressure at top of book");
    }
    else if (askVolume > bidVolume * 1.5)
    {
        Console.WriteLine("🔽 Strong selling pressure at top of book");
    }
    else
    {
        Console.WriteLine("➡️ Balanced pressure");
    }
}
```

---

### Example 9: Real-Time Spread Alert

```csharp
public async Task MonitorSpreadAsync(MT5Service svc, string symbol, double maxSpreadPips)
{
    using (await svc.SubscribeToMarketBookAsync(symbol))
    {
        while (true)
        {
            var (bid, ask) = await svc.GetBestBidAskFromBookAsync(symbol);
            double spreadPips = (ask - bid) * 100000 / 10;

            if (spreadPips > maxSpreadPips)
            {
                Console.WriteLine($"🚨 ALERT: Spread {spreadPips:F1} pips > {maxSpreadPips} pips");
            }

            await Task.Delay(500);  // Check every 500ms
        }
    }
}

// Usage:
await MonitorSpreadAsync(svc, "EURUSD", maxSpreadPips: 2.0);
```

---

### Example 10: Compare Manual vs Auto Extraction

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    // ❌ MANUAL WAY (verbose):
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");
    var bids = book.MqlBookInfos.Where(b => b.Type == BookType.Buy);
    var asks = book.MqlBookInfos.Where(b => b.Type == BookType.Sell);
    double manualBid = bids.Any() ? bids.Max(b => b.Price) : 0;
    double manualAsk = asks.Any() ? asks.Min(b => b.Price) : 0;

    Console.WriteLine($"Manual: {manualBid:F5} / {manualAsk:F5}");

    // ✅ AUTO WAY (clean):
    var (autoBid, autoAsk) = await svc.GetBestBidAskFromBookAsync("EURUSD");

    Console.WriteLine($"Auto: {autoBid:F5} / {autoAsk:F5}");

    // Same result, cleaner code!
}
```

---

## 🔗 Related Methods

**📦 Methods used internally:**

* `MarketBookGetAsync()` - Gets order book snapshot

**🍬 Related Sugar methods:**

* `SubscribeToMarketBookAsync()` - Subscribe to order book
* `GetMarketBookSnapshotAsync()` - Get full order book
* `CalculateLiquidityAtLevelAsync()` - Get volume at specific price

**📊 Alternative approaches:**

* `SymbolInfoTickAsync()` - Gets bid/ask from tick data (faster, but no depth)

---

## ⚠️ Common Pitfalls

1. **Using without subscription:**
   ```csharp
   // ❌ WRONG: Will fail - no subscription!
   var (bid, ask) = await svc.GetBestBidAskFromBookAsync("EURUSD");

   // ✅ CORRECT: Subscribe first
   using (await svc.SubscribeToMarketBookAsync("EURUSD"))
   {
       var (bid, ask) = await svc.GetBestBidAskFromBookAsync("EURUSD");
   }
   ```

2. **Not checking for empty book:**
   ```csharp
   // ⚠️ Returns (0, 0) if book is empty
   var (bid, ask) = await svc.GetBestBidAskFromBookAsync("EURUSD");

   if (bid == 0 || ask == 0)
   {
       Console.WriteLine("⚠️ Empty order book");
       return;
   }
   ```

3. **Confusing with tick prices:**
   ```csharp
   // DOM prices vs Tick prices:
   var (bookBid, bookAsk) = await svc.GetBestBidAskFromBookAsync("EURUSD");
   var tick = await svc.SymbolInfoTickAsync("EURUSD");

   // Usually same, but book may lag slightly
   // Use tick for immediate prices, book for depth analysis
   ```

---

## 💡 Summary

**GetBestBidAskFromBookAsync** extracts top-of-book prices automatically:

* ✅ Auto-finds highest bid and lowest ask
* ✅ Clean tuple return for destructuring
* ✅ Handles empty book gracefully (returns 0, 0)
* ✅ Essential for spread monitoring and mid-price calculation

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var (bid, ask) = await svc.GetBestBidAskFromBookAsync("EURUSD");
    double spread = ask - bid;
}
```

**Top-of-book access made simple!** 📊
