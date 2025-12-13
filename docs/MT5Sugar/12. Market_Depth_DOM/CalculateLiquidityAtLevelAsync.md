# Calculate Liquidity at Level (`CalculateLiquidityAtLevelAsync`)

> **Sugar method:** Calculates total volume available at specific price level in order book - perfect for large order planning!

**API Information:**

* **Extension method:** `MT5Service.CalculateLiquidityAtLevelAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Region:** [12] MARKET DEPTH (DOM)
* **Underlying calls:** `MarketBookGetAsync()` + LINQ aggregation

---

## Method Signature

```csharp
public static async Task<long> CalculateLiquidityAtLevelAsync(
    this MT5Service svc,
    string symbol,
    double price,
    bool isBuy,
    int timeoutSec = 15,
    CancellationToken ct = default)
```

---

## 🔽 Input

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance (extension method) |
| `symbol` | `string` | Symbol to query (e.g., "EURUSD") |
| `price` | `double` | Exact price level to check |
| `isBuy` | `bool` | `true` = check bid side, `false` = check ask side |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<long>` | Total volume (in lots) at the specified price level |

**Returns**: 0 if no liquidity at that exact price level

---

## 💬 Just the essentials

* **What it is:** Sums all volume at a specific price level in the order book.
* **Why you need it:** Check available liquidity before placing large orders to estimate slippage.
* **Price matching**: Uses tolerance of 0.00001 for floating-point comparison.

---

## 🔧 Under the Hood

```csharp
// Step 1: Get order book snapshot
var book = await svc.MarketBookGetAsync(symbol, Dl(timeoutSec), ct);

// Step 2: Determine side (bid or ask)
var targetType = isBuy ? BookType.Buy : BookType.Sell;

// Step 3: Sum volumes at matching price level
return book.MqlBookInfos
    .Where(e => e.Type == targetType && Math.Abs(e.Price - price) < 0.00001)
    .Sum(e => e.Volume);
```

**What it improves:**

* **Auto-aggregation** - sums all entries at same price
* **Floating-point safe** - uses tolerance for price matching
* **Side-specific** - separate bid/ask querying

---

## 🔗 Usage Examples

### Example 1: Check Liquidity at Current Best Bid

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var (bid, ask) = await svc.GetBestBidAskFromBookAsync("EURUSD");

    long bidVolume = await svc.CalculateLiquidityAtLevelAsync(
        "EURUSD",
        price: bid,
        isBuy: true);

    Console.WriteLine($"Best Bid: {bid:F5}");
    Console.WriteLine($"Volume at best bid: {bidVolume} lots");

    if (bidVolume >= 100)
    {
        Console.WriteLine("✅ Strong liquidity - safe for large orders");
    }
    else
    {
        Console.WriteLine("⚠️ Thin liquidity - may cause slippage");
    }
}
```

---

### Example 2: Compare Liquidity Across Multiple Levels

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");

    // Get top 5 ask levels
    var topAsks = book.MqlBookInfos
        .Where(b => b.Type == BookType.Sell)
        .OrderBy(b => b.Price)
        .Take(5)
        .ToList();

    Console.WriteLine("Ask-side liquidity:");
    foreach (var level in topAsks)
    {
        long volume = await svc.CalculateLiquidityAtLevelAsync(
            "EURUSD",
            price: level.Price,
            isBuy: false);

        Console.WriteLine($"  {level.Price:F5}: {volume,6} lots");
    }
}

// Output:
// Ask-side liquidity:
//   1.08523:    120 lots
//   1.08524:    250 lots
//   1.08525:    180 lots
//   1.08526:    150 lots
//   1.08527:     80 lots
```

---

### Example 3: Find "Iceberg" Orders (Large Hidden Volume)

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");

    // Find bid levels with unusually high volume (potential icebergs)
    var bids = book.MqlBookInfos
        .Where(b => b.Type == BookType.Buy)
        .OrderByDescending(b => b.Price)
        .ToList();

    long avgVolume = bids.Any() ? (long)bids.Average(b => b.Volume) : 0;

    Console.WriteLine($"Average bid volume: {avgVolume} lots");
    Console.WriteLine("\nLarge orders (potential icebergs):");

    foreach (var bid in bids)
    {
        if (bid.Volume > avgVolume * 3)  // 3x average = suspicious
        {
            long volume = await svc.CalculateLiquidityAtLevelAsync(
                "EURUSD",
                price: bid.Price,
                isBuy: true);

            Console.WriteLine($"  🧊 {bid.Price:F5}: {volume} lots ({volume / avgVolume:F1}x avg)");
        }
    }
}
```

---

### Example 4: Liquidity Heatmap

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");

    Console.WriteLine("Liquidity Heatmap:");
    Console.WriteLine("Price      | Bids    | Asks");
    Console.WriteLine("-----------|---------|--------");

    var allPrices = book.MqlBookInfos.Select(b => b.Price).Distinct().OrderByDescending(p => p);

    foreach (var price in allPrices.Take(10))
    {
        long bidVol = await svc.CalculateLiquidityAtLevelAsync("EURUSD", price, isBuy: true);
        long askVol = await svc.CalculateLiquidityAtLevelAsync("EURUSD", price, isBuy: false);

        string bidBar = new string('█', (int)(bidVol / 10));
        string askBar = new string('█', (int)(askVol / 10));

        Console.WriteLine($"{price:F5} | {bidBar,-7} {bidVol,4} | {askBar,-7} {askVol,4}");
    }
}
```

---

### Example 5: Pre-Trade Liquidity Check

```csharp
public async Task<bool> CanExecuteWithoutSlippage(
    MT5Service svc,
    string symbol,
    double targetVolume,
    bool isBuy)
{
    using (await svc.SubscribeToMarketBookAsync(symbol))
    {
        var (bid, ask) = await svc.GetBestBidAskFromBookAsync(symbol);
        double bestPrice = isBuy ? ask : bid;

        long available = await svc.CalculateLiquidityAtLevelAsync(
            symbol,
            price: bestPrice,
            isBuy: !isBuy);  // Opposite side for execution

        Console.WriteLine($"Target volume: {targetVolume} lots");
        Console.WriteLine($"Available at best price: {available} lots");

        return available >= targetVolume;
    }
}

// Usage:
bool safe = await CanExecuteWithoutSlippage(svc, "EURUSD", targetVolume: 50, isBuy: true);
if (safe)
{
    Console.WriteLine("✅ Sufficient liquidity - execute");
    // await svc.BuyMarketByRisk(...);
}
else
{
    Console.WriteLine("⚠️ Insufficient liquidity - split order");
}
```

---

### Example 6: Support/Resistance from Order Walls

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");

    // Find largest bid cluster (potential support)
    var biggestBid = book.MqlBookInfos
        .Where(b => b.Type == BookType.Buy)
        .OrderByDescending(b => b.Volume)
        .First();

    long supportVolume = await svc.CalculateLiquidityAtLevelAsync(
        "EURUSD",
        price: biggestBid.Price,
        isBuy: true);

    Console.WriteLine($"🛡️ Potential Support: {biggestBid.Price:F5}");
    Console.WriteLine($"   Volume: {supportVolume} lots (order wall)");

    // Find largest ask cluster (potential resistance)
    var biggestAsk = book.MqlBookInfos
        .Where(b => b.Type == BookType.Sell)
        .OrderByDescending(b => b.Volume)
        .First();

    long resistanceVolume = await svc.CalculateLiquidityAtLevelAsync(
        "EURUSD",
        price: biggestAsk.Price,
        isBuy: false);

    Console.WriteLine($"🚧 Potential Resistance: {biggestAsk.Price:F5}");
    Console.WriteLine($"   Volume: {resistanceVolume} lots (order wall)");
}
```

---

### Example 7: Cumulative Liquidity Analysis

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");

    var asks = book.MqlBookInfos
        .Where(b => b.Type == BookType.Sell)
        .OrderBy(b => b.Price)
        .ToList();

    long cumulative = 0;
    double targetVolume = 500;  // Need to buy 5.00 lots

    Console.WriteLine("Cumulative ask-side liquidity:");
    foreach (var ask in asks.Take(10))
    {
        long levelVol = await svc.CalculateLiquidityAtLevelAsync(
            "EURUSD",
            price: ask.Price,
            isBuy: false);

        cumulative += levelVol;

        Console.WriteLine($"{ask.Price:F5}: +{levelVol,4} lots (cumulative: {cumulative,6})");

        if (cumulative >= targetVolume && cumulative - levelVol < targetVolume)
        {
            Console.WriteLine($"  ⬆️ Need to reach this level for {targetVolume} lots");
        }
    }
}
```

---

### Example 8: Zero Liquidity Detection

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    double testPrice = 1.08500;  // Round number

    long volume = await svc.CalculateLiquidityAtLevelAsync(
        "EURUSD",
        price: testPrice,
        isBuy: true);

    if (volume == 0)
    {
        Console.WriteLine($"⚠️ No liquidity at {testPrice:F5}");
        Console.WriteLine("  Price level not in order book");
    }
    else
    {
        Console.WriteLine($"✅ Found {volume} lots at {testPrice:F5}");
    }
}
```

---

### Example 9: Bid-Ask Imbalance at Specific Level

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var (bid, ask) = await svc.GetBestBidAskFromBookAsync("EURUSD");

    long bidVolume = await svc.CalculateLiquidityAtLevelAsync("EURUSD", bid, isBuy: true);
    long askVolume = await svc.CalculateLiquidityAtLevelAsync("EURUSD", ask, isBuy: false);

    double ratio = (double)bidVolume / askVolume;

    Console.WriteLine($"Best Bid Volume: {bidVolume} lots");
    Console.WriteLine($"Best Ask Volume: {askVolume} lots");
    Console.WriteLine($"Bid/Ask Ratio: {ratio:F2}");

    if (ratio > 2.0)
    {
        Console.WriteLine("🔼 Strong buying pressure (2:1 bid dominance)");
    }
    else if (ratio < 0.5)
    {
        Console.WriteLine("🔽 Strong selling pressure (2:1 ask dominance)");
    }
    else
    {
        Console.WriteLine("➡️ Balanced market");
    }
}
```

---

### Example 10: Liquidity Monitoring Over Time

```csharp
public async Task MonitorLiquidityAsync(MT5Service svc, string symbol, double priceLevel)
{
    using (await svc.SubscribeToMarketBookAsync(symbol))
    {
        for (int i = 0; i < 20; i++)
        {
            long bidVol = await svc.CalculateLiquidityAtLevelAsync(
                symbol,
                price: priceLevel,
                isBuy: true);

            long askVol = await svc.CalculateLiquidityAtLevelAsync(
                symbol,
                price: priceLevel,
                isBuy: false);

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {priceLevel:F5}: Bid={bidVol,4} Ask={askVol,4}");

            if (bidVol == 0 && askVol == 0)
            {
                Console.WriteLine("  💨 Level disappeared from book");
            }

            await Task.Delay(1000);
        }
    }
}

// Usage:
await MonitorLiquidityAsync(svc, "EURUSD", priceLevel: 1.08520);
```

---

## 🔗 Related Methods

**📦 Methods used internally:**

* `MarketBookGetAsync()` - Gets order book snapshot

**🍬 Related Sugar methods:**

* `SubscribeToMarketBookAsync()` - Subscribe to order book
* `GetMarketBookSnapshotAsync()` - Get full order book
* `GetBestBidAskFromBookAsync()` - Get best prices

**📊 Use cases:**

* Pre-trade analysis before large orders
* Iceberg order detection
* Support/Resistance level identification
* Slippage estimation

---

## ⚠️ Common Pitfalls

1. **Exact price matching issues:**
   ```csharp
   // ⚠️ Floating-point precision - method handles this with tolerance
   long volume = await svc.CalculateLiquidityAtLevelAsync(
       "EURUSD",
       price: 1.085200000001,  // Close but not exact
       isBuy: true);
   // Method uses Math.Abs(e.Price - price) < 0.00001 for matching
   ```

2. **Wrong side parameter:**
   ```csharp
   var (bid, ask) = await svc.GetBestBidAskFromBookAsync("EURUSD");

   // ❌ WRONG: Checking bid volume on ask side
   long wrong = await svc.CalculateLiquidityAtLevelAsync("EURUSD", bid, isBuy: false);

   // ✅ CORRECT: Check bid volume on bid side
   long correct = await svc.CalculateLiquidityAtLevelAsync("EURUSD", bid, isBuy: true);
   ```

3. **Not checking for zero volume:**
   ```csharp
   long volume = await svc.CalculateLiquidityAtLevelAsync("EURUSD", 1.08500, isBuy: true);

   if (volume == 0)
   {
       Console.WriteLine("⚠️ No liquidity at this level");
   }
   ```

4. **Using without subscription:**
   ```csharp
   // ❌ WRONG: No subscription
   long volume = await svc.CalculateLiquidityAtLevelAsync("EURUSD", 1.08520, true);

   // ✅ CORRECT: Subscribe first
   using (await svc.SubscribeToMarketBookAsync("EURUSD"))
   {
       long volume = await svc.CalculateLiquidityAtLevelAsync("EURUSD", 1.08520, true);
   }
   ```

---

## 💡 Summary

**CalculateLiquidityAtLevelAsync** provides price-level volume analysis:

* ✅ Sums total volume at specific price level
* ✅ Floating-point safe price matching (0.00001 tolerance)
* ✅ Side-specific (bid or ask)
* ✅ Essential for large order execution planning
* ✅ Perfect for support/resistance detection from order clusters

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var (bid, ask) = await svc.GetBestBidAskFromBookAsync("EURUSD");

    long bidVolume = await svc.CalculateLiquidityAtLevelAsync("EURUSD", bid, isBuy: true);
    Console.WriteLine($"Liquidity at best bid: {bidVolume} lots");
}
```

**Know your liquidity before you trade!** 📊
