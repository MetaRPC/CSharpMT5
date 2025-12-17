# ✅ Get Symbol Digits (`GetDigitsAsync`)

> **Sugar method:** Gets number of decimal places for symbol prices. Simple wrapper for `SymbolInfoIntegerAsync(SymbolDigits)` with int conversion.

**API Information:**

* **Extension method:** `MT5Service.GetDigitsAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Underlying calls:** `SymbolInfoIntegerAsync(SymbolDigits)`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static async Task<int> GetDigitsAsync(
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

## ⬆️ Output — `int`

Returns the number of decimal places (digits) for symbol prices.

**Examples:**

- EURUSD: `5` (prices like 1.10000)
- USDJPY: `3` (prices like 149.123)
- GOLD: `2` (prices like 2050.12)

**Exception:** Throws `OverflowException` if value doesn't fit in `int` (extremely rare).

---

## 💬 Just the essentials

* **What it is.** One-liner to get symbol decimal precision - how many digits after decimal point for prices.
* **Why you need it.** Essential for formatting prices correctly, rounding calculations, and displaying prices to users.
* **Sanity check.** Returns small integer like 5 (for EURUSD) or 3 (for USDJPY). Use for FORMATTING only, NOT for calculations.

---

## 🎯 Purpose

Use it for price formatting:

* Format prices for display.
* Round prices to correct precision.
* Validate price strings.
* Format order parameters.
* Build price display templates.

---

## 🧩 Notes & Tips

* **Digits vs Point:** Digits is for FORMATTING (display), Point is for CALCULATIONS. Never use digits for calculating SL/TP.
* **Format strings:** Use `price.ToString($"F{digits}")` to format prices correctly.
* **Rounding:** Use `Math.Round(price, digits)` to round to correct precision.
* **Not for calculations:** Don't use `Math.Pow(10, -digits)` for calculations. Use `GetPointAsync()` instead.
* **Caching:** Digits don't change during session. Cache if calling frequently.
* **5-digit vs 4-digit brokers:** Modern brokers use 5 digits (EURUSD), old brokers used 4 digits.

---

## 🔧 Under the Hood

This sugar method wraps a low-level call with type conversion:

```csharp
var deadline = DateTime.UtcNow.AddSeconds(timeoutSec);

// Get SYMBOL_DIGITS property (returns int64)
var result = await svc.SymbolInfoIntegerAsync(symbol,
    SymbolInfoIntegerProperty.SymbolDigits, deadline, ct);

// Convert int64 to int with overflow check
long value = result.Value;
if (value > int.MaxValue || value < int.MinValue)
    throw new OverflowException($"SymbolDigits={value} does not fit into int.");

return (int)value;
```

**What it improves:**

* **Simpler call** - no need to specify property enum
* **Automatic deadline** - just pass timeout in seconds
* **Safe int conversion** - handles overflow gracefully
* **Returns int directly** - not int64, easier to use

---

## 📊 Low-Level Alternative

**WITHOUT sugar:**
```csharp
var deadline = DateTime.UtcNow.AddSeconds(10);

var result = await svc.SymbolInfoIntegerAsync("EURUSD",
    SymbolInfoIntegerProperty.SymbolDigits, deadline, ct);

long digitsLong = result.Value;
int digits = (int)digitsLong; // Unsafe cast!
```

**WITH sugar:**
```csharp
int digits = await svc.GetDigitsAsync("EURUSD");
```

**Benefits:**

* ✅ **3 lines → 1 line**
* ✅ **No enum lookup needed**
* ✅ **Safe int64→int conversion**
* ✅ **Auto deadline management**

---

## 🔗 Usage Examples

### 1) Basic digits retrieval

```csharp
// svc — MT5Service instance

var digits = await svc.GetDigitsAsync("EURUSD");
Console.WriteLine($"EURUSD digits: {digits}");
// Output: EURUSD digits: 5
```

---

### 2) Format price for display

```csharp
var digits = await svc.GetDigitsAsync("EURUSD");

double price = 1.100005;
string formatted = price.ToString($"F{digits}");

Console.WriteLine($"Raw price:       {price}");
Console.WriteLine($"Formatted price: {formatted}");

// Output:
// Raw price:       1.100005
// Formatted price: 1.10001
```

---

### 3) Round price to correct precision

```csharp
var digits = await svc.GetDigitsAsync("GBPUSD");

double rawPrice = 1.250000123456;
double rounded = Math.Round(rawPrice, digits);

Console.WriteLine($"Raw:     {rawPrice}");
Console.WriteLine($"Rounded: {rounded}");
Console.WriteLine($"Display: {rounded.ToString($"F{digits}")}");
```

---

### 4) Compare digits across symbols

```csharp
string[] symbols = { "EURUSD", "GBPUSD", "USDJPY", "GOLD", "BTCUSD" };

Console.WriteLine("Symbol Digits:");
Console.WriteLine("─────────────────────────────────");

foreach (var symbol in symbols)
{
    var digits = await svc.GetDigitsAsync(symbol);
    var samplePrice = digits == 5 ? "1.10000" :
                      digits == 3 ? "149.123" :
                      digits == 2 ? "2050.12" : "0.00";

    Console.WriteLine($"{symbol,-10} {digits} digits  (e.g., {samplePrice})");
}
```

---

### 5) Validate price string

```csharp
var digits = await svc.GetDigitsAsync("EURUSD");

string priceStr = "1.10005";
bool isValid = false;

if (double.TryParse(priceStr, out double price))
{
    // Check if price has correct number of decimal places
    string[] parts = priceStr.Split('.');
    if (parts.Length == 2 && parts[1].Length == digits)
    {
        isValid = true;
    }
}

Console.WriteLine($"Price: {priceStr}");
Console.WriteLine($"Valid: {isValid}");
```

---

### 6) Create price format template

```csharp
var digits = await svc.GetDigitsAsync("EURUSD");

// Create reusable format string
string formatTemplate = $"F{digits}";

// Use for multiple prices
double[] prices = { 1.10000, 1.10050, 1.10100 };

Console.WriteLine("Formatted Prices:");
foreach (var price in prices)
{
    Console.WriteLine($"  {price.ToString(formatTemplate)}");
}
```

---

### 7) Determine pip position

```csharp
var digits = await svc.GetDigitsAsync("EURUSD");

// For 5-digit pairs, pip is 4th digit (index 3 from right)
// For 3-digit pairs, pip is 2nd digit (index 1 from right)
int pipPosition = digits - 1;

Console.WriteLine($"Symbol:       EURUSD");
Console.WriteLine($"Digits:       {digits}");
Console.WriteLine($"Pip position: {pipPosition}th digit from right");

if (digits == 5)
{
    Console.WriteLine("Example: 1.10[0]05 - pip digit in brackets");
}
```

---

### 8) Format prices in table

```csharp
var symbols = new[] { "EURUSD", "USDJPY", "GOLD" };
var prices = new[] { 1.10000, 149.123, 2050.12 };

Console.WriteLine("Symbol     Price");
Console.WriteLine("─────────────────────");

for (int i = 0; i < symbols.Length; i++)
{
    var digits = await svc.GetDigitsAsync(symbols[i]);
    var formatted = prices[i].ToString($"F{digits}");

    Console.WriteLine($"{symbols[i],-10} {formatted,10}");
}
```

---

### 9) Calculate decimal multiplier (for display)

```csharp
var digits = await svc.GetDigitsAsync("EURUSD");

// Calculate multiplier for decimal conversions (display only!)
double multiplier = Math.Pow(10, digits);

double price = 1.10005;
long priceAsInt = (long)(price * multiplier);

Console.WriteLine($"Digits:     {digits}");
Console.WriteLine($"Multiplier: {multiplier}");
Console.WriteLine($"Price:      {price}");
Console.WriteLine($"As integer: {priceAsInt}");

// WARNING: Don't use this for trading calculations!
```

---

### 10) Smart price rounding utility

```csharp
async Task<double> SmartRound(string symbol, double price)
{
    var digits = await svc.GetDigitsAsync(symbol);
    return Math.Round(price, digits);
}

// Usage
double rawPrice1 = 1.100005123;
double rawPrice2 = 149.123456789;

double rounded1 = await SmartRound("EURUSD", rawPrice1);
double rounded2 = await SmartRound("USDJPY", rawPrice2);

Console.WriteLine($"EURUSD: {rawPrice1} → {rounded1}");
Console.WriteLine($"USDJPY: {rawPrice2} → {rounded2}");
```

---

## 🔗 Related Methods

**📦 Low-level method used internally:**

* `SymbolInfoIntegerAsync(SymbolDigits)` - Direct digits query (this is what GetDigitsAsync wraps)

**🍬 Other sugar methods:**

* `GetPointAsync()` - Get symbol point size (use this for calculations, not digits!)
* `GetSymbolSnapshot()` - Get point, digits, tick, and margin in one call
* `NormalizePriceAsync()` - Normalize price to tick size
* `PointsToPipsAsync()` - Convert points to pips (uses digits internally for conversion factor)

---

## ⚠️ Common Pitfalls

1. **Using digits for price calculations**
   ```csharp
   // ❌ WRONG - using digits to calculate point
   var digits = await svc.GetDigitsAsync("EURUSD");
   double point = Math.Pow(10, -digits); // DON'T DO THIS!
   double slPrice = entry - (100 * point);

   // ✅ CORRECT - use point directly
   var point = await svc.GetPointAsync("EURUSD");
   double slPrice = entry - (100 * point);
   ```

2. **Assuming all FX pairs have 5 digits**
   ```csharp
   // ❌ WRONG - hardcoded digits
   double price = 1.10005;
   string formatted = price.ToString("F5"); // What about USDJPY?

   // ✅ CORRECT - query actual digits
   var digits = await svc.GetDigitsAsync(symbol);
   string formatted = price.ToString($"F{digits}");
   ```

3. **Using digits for volume formatting**
   ```csharp
   // ❌ WRONG - digits is for prices, not volumes
   var digits = await svc.GetDigitsAsync("EURUSD");
   string volume = 1.5.ToString($"F{digits}"); // Wrong!

   // ✅ CORRECT - volumes typically use 2 decimal places
   string volume = 1.5.ToString("F2");
   ```

4. **Confusing digits with point**
   ```csharp
   // ❌ WRONG - using wrong property
   var digits = await svc.GetDigitsAsync("EURUSD");
   double distance = (price2 - price1) / digits; // Makes no sense!

   // ✅ CORRECT - use point for calculations
   var point = await svc.GetPointAsync("EURUSD");
   double distanceInPoints = (price2 - price1) / point;
   ```

5. **Not rounding after calculations**
   ```csharp
   // ❌ WRONG - calculated price has wrong precision
   var point = await svc.GetPointAsync("EURUSD");
   double slPrice = entry - (100 * point); // Might have rounding errors

   // ✅ CORRECT - round to digits after calculation
   var point = await svc.GetPointAsync("EURUSD");
   var digits = await svc.GetDigitsAsync("EURUSD");
   double slPrice = Math.Round(entry - (100 * point), digits);
   ```

6. **Using digits to normalize prices**
   ```csharp
   // ❌ WRONG - rounding to digits doesn't respect tick size
   var digits = await svc.GetDigitsAsync("EURUSD");
   double normalized = Math.Round(price, digits);

   // ✅ CORRECT - normalize to actual tick size
   double normalized = await svc.NormalizePriceAsync("EURUSD", price);
   ```
