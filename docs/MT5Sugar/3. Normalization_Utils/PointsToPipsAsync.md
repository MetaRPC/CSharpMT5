# ✅ Convert Points to Pips (`PointsToPipsAsync`)

> **Sugar method:** Converts points to pips. For 5-digit FX pairs (EURUSD), 10 points = 1 pip. For 3-digit pairs (USDJPY), 1 point = 1 pip.

**API Information:**

* **Extension method:** `MT5Service.PointsToPipsAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Underlying calls:** `GetDigitsAsync()`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static async Task<double> PointsToPipsAsync(
        this MT5Service svc,
        string symbol,
        double points,
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
| `points`     | `double`            | Number of points to convert                     |
| `timeoutSec` | `int`               | Timeout in seconds (default: 10)                |
| `ct`         | `CancellationToken` | Cancellation token                              |

---

## ⬆️ Output — `double`

Returns the number of pips.

**Examples:**

- EURUSD (5 digits): 100 points → 10 pips
- USDJPY (3 digits): 100 points → 100 pips
- GOLD (2 digits): 100 points → 100 pips

---

## 💬 Just the essentials

* **What it is.** Converts broker points to trader pips. For 5-digit pairs, divides by 10. For others, returns same value.
* **Why you need it.** Display distances in traditional pip terms for traders familiar with classic 4-digit quoting.
* **Sanity check.** For EURUSD: 100 points = 10 pips. For USDJPY: 100 points = 100 pips.

---

## 🎯 Purpose

Use it for display and reporting:

* Show stop loss/take profit in pips.
* Display spread in pips.
* Report trade results in pips.
* Convert broker points to trader language.
* Build trading journals with pip values.

---

## 🧩 Notes & Tips

* **5-digit vs 3-digit:** Formula uses `digits - 4` to determine conversion factor.
* **Conversion formula:** `pips = points / 10^max(0, digits - 4)`
* **EURUSD (5 digits):** 10 points = 1 pip (factor = 10)
* **USDJPY (3 digits):** 1 point = 1 pip (factor = 1)
* **Display only:** Use for reporting, NOT for calculations. Always work in points internally.
* **Classic vs Modern:** Old 4-digit brokers: 1 point = 1 pip. Modern 5-digit: 10 points = 1 pip.

---

## 🔧 Under the Hood

This sugar method gets digits and calculates conversion:

```csharp
// Step 1: Get symbol digits
var digits = await svc.GetDigitsAsync(symbol, timeoutSec, ct);

// Step 2: Calculate conversion factor
// For 5-digit pairs (digits=5): factor = 10^(5-4) = 10
// For 3-digit pairs (digits=3): factor = 10^0 = 1
var factor = Math.Pow(10, Math.Max(0, digits - 4));

// Step 3: Convert points to pips
return points / factor;
```

**What it improves:**

* **Symbol-aware conversion** - handles different digit counts automatically
* **Simple API** - just pass symbol and points
* **Clear intent** - converts for display, not calculation
* **Correct for all symbol types** - FX, metals, crypto

---

## 📊 Low-Level Alternative

**WITHOUT sugar:**
```csharp
var digits = await svc.GetDigitsAsync("EURUSD");
var factor = Math.Pow(10, Math.Max(0, digits - 4));
double pips = points / factor;
```

**WITH sugar:**
```csharp
double pips = await svc.PointsToPipsAsync("EURUSD", points);
```

**Benefits:**

* ✅ **3 lines → 1 line**
* ✅ **No manual factor calculation**
* ✅ **Symbol-aware**
* ✅ **Clearer intent**

---

## 🔗 Usage Examples

### 1) Basic point to pip conversion

```csharp
// svc — MT5Service instance

double points = 100;
double pips = await svc.PointsToPipsAsync("EURUSD", points);

Console.WriteLine($"{points} points = {pips} pips");
// Output: 100 points = 10 pips
```

---

### 2) Convert stop loss to pips

```csharp
var point = await svc.GetPointAsync("EURUSD");

double entryPrice = 1.10000;
double slPrice = 1.09900;

// Calculate SL distance in points
double slPoints = (entryPrice - slPrice) / point;

// Convert to pips for display
double slPips = await svc.PointsToPipsAsync("EURUSD", slPoints);

Console.WriteLine($"SL Distance:");
Console.WriteLine($"  {slPoints:F0} points");
Console.WriteLine($"  {slPips:F1} pips");
```

---

### 3) Display spread in pips

```csharp
var spreadPoints = await svc.GetSpreadPointsAsync("EURUSD");
var spreadPips = await svc.PointsToPipsAsync("EURUSD", spreadPoints);

Console.WriteLine($"Spread:");
Console.WriteLine($"  {spreadPoints:F1} points");
Console.WriteLine($"  {spreadPips:F2} pips");
```

---

### 4) Compare conversions across symbols

```csharp
string[] symbols = { "EURUSD", "GBPUSD", "USDJPY", "GOLD" };
double testPoints = 100;

Console.WriteLine($"Converting {testPoints} points to pips:");
Console.WriteLine("─────────────────────────────────────");

foreach (var symbol in symbols)
{
    var pips = await svc.PointsToPipsAsync(symbol, testPoints);
    var digits = await svc.GetDigitsAsync(symbol);

    Console.WriteLine($"{symbol,-10} ({digits} digits): {testPoints,3} pts = {pips,5:F1} pips");
}

// Output:
// EURUSD     (5 digits): 100 pts =  10.0 pips
// GBPUSD     (5 digits): 100 pts =  10.0 pips
// USDJPY     (3 digits): 100 pts = 100.0 pips
// GOLD       (2 digits): 100 pts = 100.0 pips
```

---

### 5) Report trade result in pips

```csharp
var point = await svc.GetPointAsync("EURUSD");

double entryPrice = 1.10000;
double exitPrice = 1.10150;

// Calculate profit in points
double profitPoints = (exitPrice - entryPrice) / point;

// Convert to pips
double profitPips = await svc.PointsToPipsAsync("EURUSD", profitPoints);

Console.WriteLine($"Trade Result:");
Console.WriteLine($"  Entry: {entryPrice}");
Console.WriteLine($"  Exit:  {exitPrice}");
Console.WriteLine($"  Profit: {profitPoints:F0} points ({profitPips:F1} pips)");
```

---

### 6) Build pip-based statistics

```csharp
// Sample trades with profit in points
var trades = new[]
{
    ("EURUSD", 150.0),  // points
    ("EURUSD", -80.0),
    ("EURUSD", 200.0),
    ("GBPUSD", 120.0),
    ("USDJPY", 500.0)
};

Console.WriteLine("Trade Results:");
Console.WriteLine("─────────────────────────────────────");

double totalPips = 0;

foreach (var (symbol, points) in trades)
{
    var pips = await svc.PointsToPipsAsync(symbol, points);
    totalPips += pips;

    var sign = pips >= 0 ? "+" : "";
    Console.WriteLine($"{symbol,-10} {sign}{points,6:F0} pts = {sign}{pips,5:F1} pips");
}

Console.WriteLine("─────────────────────────────────────");
Console.WriteLine($"Total:      {totalPips,20:F1} pips");
```

---

### 7) Format trade journal entry

```csharp
var point = await svc.GetPointAsync("EURUSD");

double entry = 1.10000;
double sl = 1.09900;
double tp = 1.10200;

// Calculate distances in points
double slPoints = Math.Abs((entry - sl) / point);
double tpPoints = Math.Abs((tp - entry) / point);

// Convert to pips
double slPips = await svc.PointsToPipsAsync("EURUSD", slPoints);
double tpPips = await svc.PointsToPipsAsync("EURUSD", tpPoints);
double rrRatio = tpPips / slPips;

Console.WriteLine("Trade Setup:");
Console.WriteLine($"  Entry:     {entry}");
Console.WriteLine($"  SL:        {sl} ({slPips:F1} pips risk)");
Console.WriteLine($"  TP:        {tp} ({tpPips:F1} pips target)");
Console.WriteLine($"  R:R Ratio: 1:{rrRatio:F2}");
```

---

### 8) Calculate average pip movement

```csharp
// Daily high/low prices
var dailyRanges = new[] { 1.10200 - 1.10050, 1.10150 - 1.10000, 1.10300 - 1.10100 };

var point = await svc.GetPointAsync("EURUSD");
double totalPips = 0;

Console.WriteLine("Daily Ranges:");
foreach (var range in dailyRanges)
{
    double rangePoints = range / point;
    double rangePips = await svc.PointsToPipsAsync("EURUSD", rangePoints);
    totalPips += rangePips;

    Console.WriteLine($"  {rangePips:F1} pips");
}

double averagePips = totalPips / dailyRanges.Length;
Console.WriteLine($"Average: {averagePips:F1} pips/day");
```

---

### 9) Display strategy parameters in pips

```csharp
// Strategy uses points internally
int stopLossPoints = 100;
int takeProfitPoints = 200;
int trailingStopPoints = 50;

// Convert for display
double slPips = await svc.PointsToPipsAsync("EURUSD", stopLossPoints);
double tpPips = await svc.PointsToPipsAsync("EURUSD", takeProfitPoints);
double tsPips = await svc.PointsToPipsAsync("EURUSD", trailingStopPoints);

Console.WriteLine("Strategy Parameters:");
Console.WriteLine($"  Stop Loss:     {slPips:F0} pips ({stopLossPoints} points)");
Console.WriteLine($"  Take Profit:   {tpPips:F0} pips ({takeProfitPoints} points)");
Console.WriteLine($"  Trailing Stop: {tsPips:F0} pips ({trailingStopPoints} points)");
```

---

### 10) Build conversion utility

```csharp
async Task DisplayConversion(string symbol, double points)
{
    var pips = await svc.PointsToPipsAsync(symbol, points);
    var digits = await svc.GetDigitsAsync(symbol);

    string factor = digits switch
    {
        5 => "÷10",
        3 => "×1",
        _ => "×1"
    };

    Console.WriteLine($"{symbol} ({digits} digits):");
    Console.WriteLine($"  {points} points {factor} = {pips:F2} pips");
}

// Usage
await DisplayConversion("EURUSD", 100);
await DisplayConversion("USDJPY", 100);
await DisplayConversion("GOLD", 50);

// Output:
// EURUSD (5 digits):
//   100 points ÷10 = 10.00 pips
// USDJPY (3 digits):
//   100 points ×1 = 100.00 pips
// GOLD (2 digits):
//   50 points ×1 = 50.00 pips
```

---

## 🔗 Related Methods

**📦 Low-level method used internally:**

* `GetDigitsAsync()` - Get symbol digits (used to calculate conversion factor)

**🍬 Other sugar methods:**

* `GetPointAsync()` - Get point size (use points for calculations, convert to pips for display)
* `GetSpreadPointsAsync()` - Get spread in points (can convert result to pips with this method)
* `GetSymbolSnapshot()` - Get tick, point, digits, margin
* Use this method to convert any point-based values for user display

---

## ⚠️ Common Pitfalls

1. **Using pips for calculations**
   ```csharp
   // ❌ WRONG - calculating in pips
   double slPips = 10;
   double slPrice = entry - (slPips * ???); // How to convert back?

   // ✅ CORRECT - calculate in points, display in pips
   double slPoints = 100;
   double slPrice = entry - (slPoints * point);
   double slPips = await svc.PointsToPipsAsync("EURUSD", slPoints); // For display
   ```

2. **Hardcoding conversion factor**
   ```csharp
   // ❌ WRONG - assuming all pairs divide by 10
   double pips = points / 10; // Wrong for USDJPY!

   // ✅ CORRECT - using symbol-aware conversion
   double pips = await svc.PointsToPipsAsync(symbol, points);
   ```

3. **Confusing points and pips in code**
   ```csharp
   // ❌ WRONG - mixing units
   int stopLoss = 10; // Is this points or pips? Unclear!

   // ✅ CORRECT - explicit naming
   int stopLossPoints = 100; // Clear: this is points
   double stopLossPips = await svc.PointsToPipsAsync("EURUSD", stopLossPoints);
   ```

4. **Not considering symbol digits**
   ```csharp
   // ❌ WRONG - assuming 5 digits for all
   double pips = points / 10; // What about USDJPY (3 digits)?

   // ✅ CORRECT - symbol-specific conversion
   double pips = await svc.PointsToPipsAsync(symbol, points);
   ```

5. **Converting pips back to points manually**
   ```csharp
   // ❌ WRONG - manual conversion back
   double pips = 10;
   double points = pips * 10; // What if symbol is not 5-digit?

   // ✅ CORRECT - work in points, convert to pips only for display
   double points = 100; // Use points internally
   double pips = await svc.PointsToPipsAsync("EURUSD", points); // Display
   ```

6. **Using for volume or money calculations**
   ```csharp
   // ❌ WRONG - this is for price units only
   double volume = 1.5;
   double volumePips = await svc.PointsToPipsAsync("EURUSD", volume); // Makes no sense!

   // ✅ CORRECT - use only for price-related points
   double pricePoints = 100;
   double pricePips = await svc.PointsToPipsAsync("EURUSD", pricePoints);
   ```
