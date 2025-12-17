# ✅ Get Spread in Points (`GetSpreadPointsAsync`)

> **Sugar method:** Calculates current spread in points from the last tick. Returns (Ask - Bid) / Point.

**API Information:**

* **Extension method:** `MT5Service.GetSpreadPointsAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Underlying calls:** `SymbolInfoTickAsync()` + `SymbolInfoDoubleAsync(SymbolPoint)`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static async Task<double> GetSpreadPointsAsync(
        this MT5Service svc,
        string symbol,
        int timeoutSec = 10,
        CancellationToken ct = default);
}
```

---

## 🔽 Input

| Parameter    | Type                | Description                                     |
| ------------ | ------------------- | ----------------------------------------------- |
| `svc`        | `MT5Service`        | MT5Service instance (extension method)          |
| `symbol`     | `string`            | Symbol name (e.g., "EURUSD")                    |
| `timeoutSec` | `int`               | Timeout in seconds (default: 10)                |
| `ct`         | `CancellationToken` | Cancellation token                              |

---

## ⬆️ Output — `double`

Returns the current spread in points.

**Examples:**

- EURUSD: Ask = 1.10010, Bid = 1.10000, Point = 0.00001 → Spread = 10 points
- USDJPY: Ask = 149.125, Bid = 149.120, Point = 0.001 → Spread = 5 points

---

## 💬 Just the essentials

* **What it is.** One-liner to get current bid-ask spread in points. Fetches latest tick and calculates (Ask - Bid) / Point.
* **Why you need it.** Check trading costs before order placement, monitor spread conditions, filter high-spread periods.
* **Sanity check.** Typical EURUSD spread: 0-2 points (good), 5-10 points (normal), 20+ points (high/avoid). Always positive.

---

## 🎯 Purpose

Use it for spread monitoring:

* Check if spread is acceptable before trading.
* Filter high-spread periods (news events).
* Calculate trading costs.
* Monitor broker conditions.
* Build spread statistics.

---

## 🧩 Notes & Tips

* **Real-time:** Gets current tick, so spread reflects real-time market conditions.
* **Always positive:** Spread is always Ask - Bid, which is always positive (broker markup).
* **Volatility indicator:** Wide spread often indicates high volatility or low liquidity.
* **Cost of trading:** Spread is immediate cost of entering position (slippage).
* **Variable:** Spread changes constantly. Check before each trade.
* **Typical values:** FX major pairs: 0-3 points normal, 5-10 moderate, 20+ high.

---

## 🔧 Under the Hood

This sugar method combines tick fetch and point calculation:

```csharp
var deadline = DateTime.UtcNow.AddSeconds(timeoutSec);

// Step 1: Get current tick (bid and ask prices)
var tick = await svc.SymbolInfoTickAsync(symbol, deadline, ct);

// Step 2: Get point size
var pointResult = await svc.SymbolInfoDoubleAsync(symbol,
    SymbolInfoDoubleProperty.SymbolPoint, deadline, ct);
double point = pointResult.Value;

// Step 3: Calculate spread in points
return (tick.Ask - tick.Bid) / point;
```

**What it improves:**

* **Combines 2 calls** - tick + point in one method
* **Automatic calculation** - no manual spread formula
* **Shared deadline** - both calls use same timeout
* **Returns points directly** - ready to use

---

## 📊 Low-Level Alternative

**WITHOUT sugar:**
```csharp
var deadline = DateTime.UtcNow.AddSeconds(10);

var tick = await svc.SymbolInfoTickAsync("EURUSD", deadline, ct);

var pointResult = await svc.SymbolInfoDoubleAsync("EURUSD",
    SymbolInfoDoubleProperty.SymbolPoint, deadline, ct);
double point = pointResult.Value;

double spreadPoints = (tick.Ask - tick.Bid) / point;
```

**WITH sugar:**
```csharp
double spreadPoints = await svc.GetSpreadPointsAsync("EURUSD");
```

**Benefits:**

* ✅ **6 lines → 1 line**
* ✅ **No manual calculation**
* ✅ **Auto deadline management**
* ✅ **Clearer intent**

---

## 🔗 Usage Examples

### 1) Basic spread check

```csharp
// svc — MT5Service instance

var spread = await svc.GetSpreadPointsAsync("EURUSD");
Console.WriteLine($"EURUSD spread: {spread:F1} points");
// Output: EURUSD spread: 1.2 points
```

---

### 2) Check spread before trading

```csharp
var spread = await svc.GetSpreadPointsAsync("EURUSD");
double maxAllowedSpread = 5.0;

if (spread <= maxAllowedSpread)
{
    Console.WriteLine($"✓ Spread OK: {spread:F1} points");
    // Proceed with trade
}
else
{
    Console.WriteLine($"⚠ Spread too high: {spread:F1} points (max: {maxAllowedSpread})");
    // Skip trade
}
```

---

### 3) Convert spread to pips

```csharp
var spreadPoints = await svc.GetSpreadPointsAsync("EURUSD");
var spreadPips = await svc.PointsToPipsAsync("EURUSD", spreadPoints);

Console.WriteLine($"Spread:");
Console.WriteLine($"  {spreadPoints:F1} points");
Console.WriteLine($"  {spreadPips:F2} pips");
```

---

### 4) Compare spreads across symbols

```csharp
string[] symbols = { "EURUSD", "GBPUSD", "USDJPY", "GOLD" };

Console.WriteLine("Current Spreads:");
Console.WriteLine("─────────────────────────────────");

foreach (var symbol in symbols)
{
    var spread = await svc.GetSpreadPointsAsync(symbol);
    var pips = await svc.PointsToPipsAsync(symbol, spread);

    Console.WriteLine($"{symbol,-10} {spread,5:F1} points ({pips,4:F2} pips)");
}
```

---

### 5) Calculate spread cost in money

```csharp
var spread = await svc.GetSpreadPointsAsync("EURUSD");
var point = await svc.GetPointAsync("EURUSD");

double volume = 1.0; // 1 lot
double contractSize = 100000; // Standard lot

// Spread cost = spread in price * volume * contract size
double spreadCost = (spread * point) * volume * contractSize;

Console.WriteLine($"Spread: {spread:F1} points");
Console.WriteLine($"Volume: {volume} lot");
Console.WriteLine($"Cost:   ${spreadCost:F2}");
```

---

### 6) Monitor spread during session

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(30)); // Monitor for 30 seconds

double minSpread = double.MaxValue;
double maxSpread = double.MinValue;
double totalSpread = 0;
int count = 0;

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        var spread = await svc.GetSpreadPointsAsync("EURUSD", ct: cts.Token);

        minSpread = Math.Min(minSpread, spread);
        maxSpread = Math.Max(maxSpread, spread);
        totalSpread += spread;
        count++;

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Spread: {spread:F1} points");

        await Task.Delay(2000, cts.Token);
    }
}
catch (OperationCanceledException)
{
    double avgSpread = totalSpread / count;

    Console.WriteLine("\nSpread Statistics:");
    Console.WriteLine($"  Min: {minSpread:F1} points");
    Console.WriteLine($"  Max: {maxSpread:F1} points");
    Console.WriteLine($"  Avg: {avgSpread:F1} points");
}
```

---

### 7) Alert on high spread

```csharp
double alertThreshold = 10.0;

var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromMinutes(5));

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        var spread = await svc.GetSpreadPointsAsync("EURUSD", ct: cts.Token);

        if (spread > alertThreshold)
        {
            Console.WriteLine($"🔔 ALERT: High spread detected!");
            Console.WriteLine($"   Current: {spread:F1} points");
            Console.WriteLine($"   Threshold: {alertThreshold:F1} points");
            Console.WriteLine($"   Time: {DateTime.Now:HH:mm:ss}");
        }

        await Task.Delay(1000, cts.Token);
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Monitoring stopped");
}
```

---

### 8) Build spread histogram

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromMinutes(1));

var spreadCounts = new Dictionary<int, int>();

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        var spread = await svc.GetSpreadPointsAsync("EURUSD", ct: cts.Token);
        int bucket = (int)Math.Floor(spread);

        if (!spreadCounts.ContainsKey(bucket))
            spreadCounts[bucket] = 0;

        spreadCounts[bucket]++;

        await Task.Delay(500, cts.Token);
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nSpread Distribution:");
    Console.WriteLine("─────────────────────────────────");

    foreach (var bucket in spreadCounts.Keys.OrderBy(k => k))
    {
        var count = spreadCounts[bucket];
        var bar = new string('█', count);

        Console.WriteLine($"{bucket,2} points: {bar} ({count})");
    }
}
```

---

### 9) Compare spread to average

```csharp
// Get current spread
var currentSpread = await svc.GetSpreadPointsAsync("EURUSD");

// Simulate average spread (in real app, calculate from history)
double averageSpread = 1.5;

double deviation = ((currentSpread - averageSpread) / averageSpread) * 100;

Console.WriteLine($"Current spread:  {currentSpread:F1} points");
Console.WriteLine($"Average spread:  {averageSpread:F1} points");
Console.WriteLine($"Deviation:       {deviation:+0.0;-0.0}%");

if (Math.Abs(deviation) > 50)
{
    Console.WriteLine("⚠ Spread is significantly different from average!");
}
```

---

### 10) Multi-symbol spread dashboard

```csharp
string[] watchlist = { "EURUSD", "GBPUSD", "USDJPY", "GOLD" };

var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(30));

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        Console.Clear();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Spread Monitor");
        Console.WriteLine("═══════════════════════════════════════════");

        foreach (var symbol in watchlist)
        {
            var spread = await svc.GetSpreadPointsAsync(symbol, ct: cts.Token);
            var pips = await svc.PointsToPipsAsync(symbol, spread);

            // Color-code by spread level
            string status = spread switch
            {
                <= 3.0 => "✓ Good",
                <= 10.0 => "⚡ Normal",
                _ => "⚠ High"
            };

            Console.WriteLine($"{symbol,-10} {spread,5:F1} pts ({pips,4:F2} pips) {status}");
        }

        await Task.Delay(2000, cts.Token);
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nMonitoring stopped");
}
```

---

## 🔗 Related Methods

**📦 Low-level methods used internally:**

* `SymbolInfoTickAsync()` - Get current tick (bid/ask prices) - step 1
* `SymbolInfoDoubleAsync(SymbolPoint)` - Get point size - step 2

**🍬 Other sugar methods:**

* `GetPointAsync()` - Get point size only
* `PointsToPipsAsync()` - Convert spread points to pips for display
* `GetSymbolSnapshot()` - Get tick, point, digits, margin in one call
* Use before trading to check if spread is acceptable

---

## ⚠️ Common Pitfalls

1. **Assuming spread is constant**
   ```csharp
   // ❌ WRONG - caching spread for long time
   var spread = await svc.GetSpreadPointsAsync("EURUSD");
   // ... hours later ...
   if (spread < 5) { /* trade */ } // Old value!

   // ✅ CORRECT - check spread before each trade
   var spread = await svc.GetSpreadPointsAsync("EURUSD");
   if (spread < 5) { /* trade immediately */ }
   ```

2. **Not accounting for spread in calculations**
   ```csharp
   // ❌ WRONG - ignoring spread
   double entryPrice = currentBid;
   double expectedTP = entryPrice + (100 * point); // Ignores spread!

   // ✅ CORRECT - account for spread
   double entryPrice = currentAsk; // Buy at Ask
   var spread = await svc.GetSpreadPointsAsync("EURUSD");
   double expectedTP = entryPrice + (100 * point); // TP accounts for Ask entry
   ```

3. **Comparing spread across different symbols**
   ```csharp
   // ❌ WRONG - comparing raw point values
   var spreadEUR = await svc.GetSpreadPointsAsync("EURUSD");
   var spreadJPY = await svc.GetSpreadPointsAsync("USDJPY");
   if (spreadEUR < spreadJPY) { /* EURUSD is better? Not necessarily! */ }

   // ✅ CORRECT - convert to pips or percentage
   var spreadEUR = await svc.GetSpreadPointsAsync("EURUSD");
   var spreadJPY = await svc.GetSpreadPointsAsync("USDJPY");
   var pipsEUR = await svc.PointsToPipsAsync("EURUSD", spreadEUR);
   var pipsJPY = await svc.PointsToPipsAsync("USDJPY", spreadJPY);
   // Now compare pipsEUR vs pipsJPY
   ```

4. **Using spread for volume calculations**
   ```csharp
   // ❌ WRONG - spread is price difference, not volume
   var spread = await svc.GetSpreadPointsAsync("EURUSD");
   double volume = 1.0 + spread; // Makes no sense!

   // ✅ CORRECT - use spread for cost calculations
   var spread = await svc.GetSpreadPointsAsync("EURUSD");
   var point = await svc.GetPointAsync("EURUSD");
   double spreadCost = (spread * point) * volume * contractSize;
   ```

5. **Not checking for abnormal spreads**
   ```csharp
   // ❌ WRONG - trading without spread check
   await PlaceOrder("EURUSD", volume, sl, tp); // What if spread is 100 points?

   // ✅ CORRECT - validate spread first
   var spread = await svc.GetSpreadPointsAsync("EURUSD");
   if (spread > 10)
   {
       Console.WriteLine($"⚠ Spread too high ({spread:F1} points), skipping trade");
       return;
   }
   await PlaceOrder("EURUSD", volume, sl, tp);
   ```

6. **Confusing spread with slippage**
   ```csharp
   // ❌ WRONG - spread is not slippage
   var spread = await svc.GetSpreadPointsAsync("EURUSD");
   // Spread is Bid-Ask difference (always present)
   // Slippage is difference between expected and executed price (market orders)

   // ✅ CORRECT - understand the difference
   var spread = await svc.GetSpreadPointsAsync("EURUSD"); // Trading cost
   // Slippage is additional cost during execution (fast markets)
   ```
