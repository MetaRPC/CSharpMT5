# ✅ Get Point Size (`GetPointAsync`)

> **Sugar method:** Gets symbol point size (minimum price change). Simple wrapper for `SymbolInfoDoubleAsync(SymbolPoint)`.

**API Information:**

* **Extension method:** `MT5Service.GetPointAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Underlying calls:** `SymbolInfoDoubleAsync(SymbolPoint)`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static async Task<double> GetPointAsync(
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

Returns the symbol's point size (minimum price change).

**Examples:**

- EURUSD (5 digits): `0.00001`
- USDJPY (3 digits): `0.001`
- GOLD (2 digits): `0.01`

---

## 💬 Just the essentials

* **What it is.** One-liner to get symbol point size - the smallest price movement possible for the symbol.
* **Why you need it.** Essential for calculating stop loss, take profit, and price offsets in points. Use point size for ALL price calculations.
* **Sanity check.** Returns small number like 0.00001 (for 5-digit pairs) or 0.001 (for 3-digit pairs). NEVER use fixed values or Math.Pow(10, -digits).

---

## 🎯 Purpose

Use it for price calculations:

* Calculate SL/TP prices from point offsets.
* Convert price differences to points.
* Validate price precision.
* Build price ladders and levels.
* Calculate pip values.

---

## 🧩 Notes & Tips

* **Point vs Pip:** For 5-digit FX pairs, 1 pip = 10 points. For 3-digit pairs, 1 pip = 1 point.
* **Point vs Digits:** Point is the actual value (0.00001), Digits is just the count (5). Always use Point for calculations.
* **Never hardcode:** Don't use `Math.Pow(10, -digits)`. Always call `GetPointAsync()` or `SymbolInfoDoubleAsync(SymbolPoint)`.
* **Caching:** Point size doesn't change during session. Cache it if calling frequently.
* **Use for calculations:** Point is the building block for all price calculations: `price = basePrice + (points * point)`.

---

## 🔧 Under the Hood

This sugar method is a simple wrapper around one low-level call:

```csharp
var deadline = DateTime.UtcNow.AddSeconds(timeoutSec);

// Get SYMBOL_POINT property
var result = await svc.SymbolInfoDoubleAsync(symbol,
    SymbolInfoDoubleProperty.SymbolPoint, deadline, ct);

return result.Value;
```

**What it improves:**

* **Simpler call** - no need to specify property enum
* **Automatic deadline** - just pass timeout in seconds
* **Returns double directly** - no need to extract `.Value`
* **Clearer intent** - method name tells you exactly what you get

---

## 📊 Low-Level Alternative

**WITHOUT sugar:**
```csharp
var deadline = DateTime.UtcNow.AddSeconds(10);

var result = await svc.SymbolInfoDoubleAsync("EURUSD",
    SymbolInfoDoubleProperty.SymbolPoint, deadline, ct);

double point = result.Value;
```

**WITH sugar:**
```csharp
double point = await svc.GetPointAsync("EURUSD");
```

**Benefits:**

* ✅ **3 lines → 1 line**
* ✅ **No enum lookup needed**
* ✅ **Auto deadline management**
* ✅ **Direct value return**

---

## 🔗 Usage Examples

### 1) Basic point retrieval

```csharp
// svc — MT5Service instance

var point = await svc.GetPointAsync("EURUSD");
Console.WriteLine($"EURUSD point size: {point}");
// Output: EURUSD point size: 0.00001
```

---

### 2) Calculate SL price from points

```csharp
var point = await svc.GetPointAsync("EURUSD");

double entryPrice = 1.10000;
int stopLossPoints = 100;

double slPrice = entryPrice - (stopLossPoints * point);

Console.WriteLine($"Entry:  {entryPrice}");
Console.WriteLine($"SL:     {slPrice}");
Console.WriteLine($"Distance: {stopLossPoints} points");
```

---

### 3) Calculate TP price from points

```csharp
var point = await svc.GetPointAsync("GBPUSD");

double entryPrice = 1.25000;
int takeProfitPoints = 200;

double tpPrice = entryPrice + (takeProfitPoints * point);

Console.WriteLine($"Entry:  {entryPrice}");
Console.WriteLine($"TP:     {tpPrice}");
Console.WriteLine($"Target: +{takeProfitPoints} points");
```

---

### 4) Convert price difference to points

```csharp
var point = await svc.GetPointAsync("EURUSD");

double price1 = 1.10000;
double price2 = 1.10050;

double differenceInPoints = (price2 - price1) / point;

Console.WriteLine($"Price 1: {price1}");
Console.WriteLine($"Price 2: {price2}");
Console.WriteLine($"Difference: {differenceInPoints} points");
```

---

### 5) Compare point sizes across symbols

```csharp
string[] symbols = { "EURUSD", "GBPUSD", "USDJPY", "GOLD" };

Console.WriteLine("Symbol Point Sizes:");
Console.WriteLine("─────────────────────────────────");

foreach (var symbol in symbols)
{
    var point = await svc.GetPointAsync(symbol);
    Console.WriteLine($"{symbol,-10} {point:F8}");
}

// Output:
// EURUSD     0.00001000
// GBPUSD     0.00001000
// USDJPY     0.00100000
// GOLD       0.01000000
```

---

### 6) Validate price precision

```csharp
var point = await svc.GetPointAsync("EURUSD");

double price = 1.100055; // Invalid precision

// Check if price is valid (multiple of point)
double remainder = (price % point);
bool isValid = Math.Abs(remainder) < point * 0.1;

if (!isValid)
{
    Console.WriteLine($"⚠ Price {price} is not aligned to point size {point}");

    // Normalize to nearest point
    double normalized = Math.Round(price / point) * point;
    Console.WriteLine($"  Normalized: {normalized}");
}
```

---

### 7) Build price ladder

```csharp
var point = await svc.GetPointAsync("EURUSD");

double basePrice = 1.10000;
int levels = 5;
int stepPoints = 10;

Console.WriteLine("Price Ladder:");
Console.WriteLine("─────────────────────────────────");

for (int i = -levels; i <= levels; i++)
{
    double price = basePrice + (i * stepPoints * point);
    string marker = i == 0 ? " ← Entry" : "";
    Console.WriteLine($"{i,3}: {price:F5} ({i * stepPoints,4} pts){marker}");
}
```

---

### 8) Calculate pip value for 5-digit pairs

```csharp
var point = await svc.GetPointAsync("EURUSD");
var digits = await svc.GetDigitsAsync("EURUSD");

// For 5-digit pairs, 1 pip = 10 points
int pointsPerPip = (digits == 5 || digits == 3) ? 10 : 1;
double pipValue = pointsPerPip * point;

Console.WriteLine($"Symbol:    EURUSD");
Console.WriteLine($"Digits:    {digits}");
Console.WriteLine($"Point:     {point}");
Console.WriteLine($"Pip value: {pipValue}");
Console.WriteLine($"1 pip = {pointsPerPip} points");
```

---

### 9) Cache point size for performance

```csharp
// Cache point size for multiple calculations
var symbolCache = new Dictionary<string, double>();

async Task<double> GetPointCached(string symbol)
{
    if (!symbolCache.ContainsKey(symbol))
    {
        symbolCache[symbol] = await svc.GetPointAsync(symbol);
        Console.WriteLine($"Cached point for {symbol}: {symbolCache[symbol]}");
    }
    return symbolCache[symbol];
}

// Use cached value
var point1 = await GetPointCached("EURUSD"); // Fetches from API
var point2 = await GetPointCached("EURUSD"); // Returns cached value
var point3 = await GetPointCached("EURUSD"); // Returns cached value

Console.WriteLine($"All three calls returned: {point1}");
```

---

### 10) Calculate multiple price levels

```csharp
var point = await svc.GetPointAsync("GBPUSD");

double entryPrice = 1.25000;

var levels = new[]
{
    ("Entry", 0),
    ("SL", -100),
    ("TP1", 50),
    ("TP2", 100),
    ("TP3", 150)
};

Console.WriteLine("Price Levels:");
Console.WriteLine("─────────────────────────────────");

foreach (var (label, points) in levels)
{
    double price = entryPrice + (points * point);
    Console.WriteLine($"{label,-8} {price:F5} ({points,4} points)");
}
```

---

## 🔗 Related Methods

**📦 Low-level method used internally:**

* `SymbolInfoDoubleAsync(SymbolPoint)` - Direct point size query (this is what GetPointAsync wraps)

**🍬 Other sugar methods:**

* `GetDigitsAsync()` - Get symbol digits (for formatting, not calculations)
* `GetSymbolSnapshot()` - Get point, digits, tick, and margin in one call
* `NormalizePriceAsync()` - Normalize price to tick size (uses point internally)
* `PointsToPipsAsync()` - Convert points to pips for display
* `GetSpreadPointsAsync()` - Get current spread in points (uses point internally)
* All price calculation methods use point size

---

## ⚠️ Common Pitfalls

1. **Using Math.Pow(10, -digits) instead of point**
   ```csharp
   // ❌ WRONG - calculating point from digits
   var digits = await svc.GetDigitsAsync("EURUSD");
   var point = Math.Pow(10, -digits); // Don't do this!

   // ✅ CORRECT - getting actual point size
   var point = await svc.GetPointAsync("EURUSD");
   ```

2. **Hardcoding point values**
   ```csharp
   // ❌ WRONG - hardcoded point
   double point = 0.00001; // What if symbol changes?

   // ✅ CORRECT - query from broker
   var point = await svc.GetPointAsync("EURUSD");
   ```

3. **Confusing points and pips**
   ```csharp
   // ❌ WRONG - treating 50 pips as 50 points
   var point = await svc.GetPointAsync("EURUSD");
   double slPrice = entry - (50 * point); // This is 50 points, not 50 pips!

   // ✅ CORRECT - for 5-digit pairs, 50 pips = 500 points
   var point = await svc.GetPointAsync("EURUSD");
   double slPrice = entry - (500 * point); // 50 pips
   ```

4. **Not caching point size**
   ```csharp
   // ❌ WRONG - fetching point multiple times
   for (int i = 0; i < 100; i++)
   {
       var point = await svc.GetPointAsync("EURUSD"); // 100 API calls!
       prices[i] = basePrice + (i * point);
   }

   // ✅ CORRECT - fetch once, reuse
   var point = await svc.GetPointAsync("EURUSD");
   for (int i = 0; i < 100; i++)
   {
       prices[i] = basePrice + (i * point);
   }
   ```

5. **Using point for volume calculations**
   ```csharp
   // ❌ WRONG - point is for prices, not volumes
   var point = await svc.GetPointAsync("EURUSD");
   double volume = 1.5 + point; // Makes no sense!

   // ✅ CORRECT - use volume step for volumes
   var volumeStep = await svc.SymbolInfoDoubleAsync("EURUSD",
       SymbolInfoDoubleProperty.SymbolVolumeStep, ...);
   double volume = Math.Round(1.5 / volumeStep) * volumeStep;
   ```
