# ✅ Normalize Price (`NormalizePriceAsync`)

> **Sugar method:** Normalizes price to symbol tick size (strict normalization, not just digits). Ensures price is valid for order placement.

**API Information:**

* **Extension method:** `MT5Service.NormalizePriceAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Underlying calls:** `SymbolInfoDoubleAsync(SymbolTradeTickSize)`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static async Task<double> NormalizePriceAsync(
        this MT5Service svc,
        string symbol,
        double price,
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
| `price`      | `double`            | Price to normalize                              |
| `timeoutSec` | `int`               | Timeout in seconds (default: 10)                |
| `ct`         | `CancellationToken` | Cancellation token                              |

---

## ⬆️ Output — `double`

Returns the normalized price aligned to symbol's tick size.

**Example:**

- Input: `1.100005`
- Tick size: `0.00001`
- Output: `1.10001` (rounded to nearest tick)

**Exception:** Throws `InvalidOperationException` if tick size is 0 or negative.

---

## 💬 Just the essentials

* **What it is.** Strict price normalization using actual tick size (SYMBOL_TRADE_TICK_SIZE), not just rounding by digits.
* **Why you need it.** Broker rejects orders with invalid price precision. This ensures price is valid before sending to MT5.
* **Sanity check.** Returns price rounded to nearest valid tick. ALWAYS normalize calculated prices before order placement.

---

## 🎯 Purpose

Use it before order placement:

* Normalize calculated SL/TP prices.
* Validate prices from user input.
* Ensure pending order prices are valid.
* Fix prices after calculations.
* Avoid "invalid price" errors.

---

## 🧩 Notes & Tips

* **Tick size vs Point:** Tick size can differ from point size. Always use tick size for normalization.
* **Strict normalization:** Uses SYMBOL_TRADE_TICK_SIZE property, not SYMBOL_POINT or digits.
* **Rounding:** Rounds to NEAREST valid tick, not floor/ceiling.
* **Required:** ALWAYS normalize before: `OrderSendAsync()`, `OrderModifyAsync()`, pending order placement.
* **Performance:** Tick size doesn't change. Cache if normalizing many prices.
* **Formula:** `normalized = round(price / tickSize) * tickSize`

---

## 🔧 Under the Hood

This sugar method gets tick size and normalizes:

```csharp
var deadline = DateTime.UtcNow.AddSeconds(timeoutSec);

// Get SYMBOL_TRADE_TICK_SIZE (not SYMBOL_POINT!)
var result = await svc.SymbolInfoDoubleAsync(symbol,
    SymbolInfoDoubleProperty.SymbolTradeTickSize, deadline, ct);

double tickSize = result.Value;

// Validate tick size
if (tickSize <= 0)
    throw new InvalidOperationException("SymbolTradeTickSize is 0");

// Normalize to nearest tick
double steps = Math.Round(price / tickSize);
return steps * tickSize;
```

**What it improves:**

* **Strict normalization** - uses tick size, not just digits
* **Automatic validation** - throws if tick size invalid
* **Simple API** - just pass symbol and price
* **Guaranteed valid price** - broker will accept it

---

## 📊 Low-Level Alternative

**WITHOUT sugar (wrong approach):**
```csharp
// ❌ Many developers do this (WRONG!):
var digits = await svc.GetDigitsAsync("EURUSD");
double normalized = Math.Round(price, digits); // Not strict enough!
```

**WITHOUT sugar (correct but verbose):**
```csharp
var deadline = DateTime.UtcNow.AddSeconds(10);

var result = await svc.SymbolInfoDoubleAsync("EURUSD",
    SymbolInfoDoubleProperty.SymbolTradeTickSize, deadline, ct);

double tickSize = result.Value;
if (tickSize <= 0)
    throw new InvalidOperationException("Invalid tick size");

double normalized = Math.Round(price / tickSize) * tickSize;
```

**WITH sugar:**
```csharp
double normalized = await svc.NormalizePriceAsync("EURUSD", price);
```

**Benefits:**

* ✅ **6 lines → 1 line**
* ✅ **Uses correct property** (tick size, not point or digits)
* ✅ **Automatic validation**
* ✅ **Clearer intent**

---

## 🔗 Usage Examples

### 1) Basic price normalization

```csharp
// svc — MT5Service instance

double rawPrice = 1.100005;
double normalized = await svc.NormalizePriceAsync("EURUSD", rawPrice);

Console.WriteLine($"Raw:        {rawPrice}");
Console.WriteLine($"Normalized: {normalized}");
// Output:
// Raw:        1.100005
// Normalized: 1.10001
```

---

### 2) Normalize SL before order placement

```csharp
var point = await svc.GetPointAsync("EURUSD");

double entryPrice = 1.10000;
double rawSL = entryPrice - (100 * point); // Might have rounding errors

// Normalize before sending
double normalizedSL = await svc.NormalizePriceAsync("EURUSD", rawSL);

Console.WriteLine($"Entry:         {entryPrice}");
Console.WriteLine($"Raw SL:        {rawSL}");
Console.WriteLine($"Normalized SL: {normalizedSL}");

// Use normalizedSL in order
```

---

### 3) Normalize TP after calculation

```csharp
var point = await svc.GetPointAsync("GBPUSD");

double entryPrice = 1.25000;
double rawTP = entryPrice + (200 * point);

// Normalize to valid tick
double normalizedTP = await svc.NormalizePriceAsync("GBPUSD", rawTP);

Console.WriteLine($"Entry:         {entryPrice}");
Console.WriteLine($"Raw TP:        {rawTP}");
Console.WriteLine($"Normalized TP: {normalizedTP}");
```

---

### 4) Validate user input price

```csharp
string userInput = "1.100055";

if (double.TryParse(userInput, out double userPrice))
{
    double normalized = await svc.NormalizePriceAsync("EURUSD", userPrice);

    if (Math.Abs(normalized - userPrice) > 0.0000001)
    {
        Console.WriteLine($"⚠ Price adjusted from {userPrice} to {normalized}");
    }
    else
    {
        Console.WriteLine($"✓ Price {userPrice} is valid");
    }

    // Use normalized price
}
```

---

### 5) Normalize pending order price

```csharp
var tick = await svc.SymbolInfoTickAsync("EURUSD", ...);
var point = await svc.GetPointAsync("EURUSD");

// Place Buy Limit 50 points below current Ask
double rawPrice = tick.Ask - (50 * point);
double normalizedPrice = await svc.NormalizePriceAsync("EURUSD", rawPrice);

Console.WriteLine($"Current Ask:   {tick.Ask}");
Console.WriteLine($"Raw Limit:     {rawPrice}");
Console.WriteLine($"Normalized:    {normalizedPrice}");

// Use normalizedPrice for pending order
```

---

### 6) Normalize price ladder

```csharp
var point = await svc.GetPointAsync("EURUSD");

double basePrice = 1.10000;
int levels = 5;

Console.WriteLine("Normalized Price Ladder:");
Console.WriteLine("─────────────────────────────────");

for (int i = 0; i <= levels; i++)
{
    double rawPrice = basePrice + (i * 10 * point);
    double normalized = await svc.NormalizePriceAsync("EURUSD", rawPrice);

    Console.WriteLine($"Level {i}: {normalized:F5}");
}
```

---

### 7) Fix prices from external source

```csharp
// Prices from external API or calculation
double[] externalPrices = { 1.100005, 1.100123, 1.099987 };

Console.WriteLine("Fixing External Prices:");
Console.WriteLine("─────────────────────────────────");

foreach (var rawPrice in externalPrices)
{
    double normalized = await svc.NormalizePriceAsync("EURUSD", rawPrice);

    Console.WriteLine($"{rawPrice:F6} → {normalized:F5}");
}
```

---

### 8) Batch normalization

```csharp
async Task<double[]> NormalizeBatch(string symbol, double[] prices)
{
    var result = new double[prices.Length];

    for (int i = 0; i < prices.Length; i++)
    {
        result[i] = await svc.NormalizePriceAsync(symbol, prices[i]);
    }

    return result;
}

// Usage
double[] rawPrices = { 1.10005, 1.10015, 1.10025 };
double[] normalized = await NormalizeBatch("EURUSD", rawPrices);

Console.WriteLine("Batch Normalization:");
for (int i = 0; i < rawPrices.Length; i++)
{
    Console.WriteLine($"  {rawPrices[i]:F6} → {normalized[i]:F5}");
}
```

---

### 9) Compare normalization vs rounding

```csharp
var digits = await svc.GetDigitsAsync("EURUSD");
double rawPrice = 1.100055;

// Method 1: Round by digits (WRONG for orders!)
double roundedByDigits = Math.Round(rawPrice, digits);

// Method 2: Normalize by tick size (CORRECT!)
double normalizedByTick = await svc.NormalizePriceAsync("EURUSD", rawPrice);

Console.WriteLine($"Raw price:          {rawPrice:F6}");
Console.WriteLine($"Rounded by digits:  {roundedByDigits:F5}");
Console.WriteLine($"Normalized by tick: {normalizedByTick:F5}");

if (roundedByDigits != normalizedByTick)
{
    Console.WriteLine("⚠ Methods give different results!");
}
```

---

### 10) Cached normalization for performance

```csharp
// Cache tick size for multiple normalizations
Dictionary<string, double> tickSizeCache = new();

async Task<double> NormalizeCached(string symbol, double price)
{
    if (!tickSizeCache.ContainsKey(symbol))
    {
        var tickSize = (await svc.SymbolInfoDoubleAsync(symbol,
            SymbolInfoDoubleProperty.SymbolTradeTickSize,
            DateTime.UtcNow.AddSeconds(10),
            default)).Value;

        tickSizeCache[symbol] = tickSize;
    }

    double tick = tickSizeCache[symbol];
    if (tick <= 0) throw new InvalidOperationException("Invalid tick size");

    return Math.Round(price / tick) * tick;
}

// Usage - first call fetches, subsequent calls use cache
var p1 = await NormalizeCached("EURUSD", 1.10005); // Fetches tick size
var p2 = await NormalizeCached("EURUSD", 1.10015); // Uses cache
var p3 = await NormalizeCached("EURUSD", 1.10025); // Uses cache

Console.WriteLine($"Normalized: {p1:F5}, {p2:F5}, {p3:F5}");
```

---

## 🔗 Related Methods

**📦 Low-level method used internally:**

* `SymbolInfoDoubleAsync(SymbolTradeTickSize)` - Get tick size for normalization

**🍬 Other sugar methods:**

* `GetPointAsync()` - Get point size (for calculations, not normalization)
* `GetDigitsAsync()` - Get digits (for display only, NOT for normalization)
* `GetSymbolSnapshot()` - Get tick, point, digits, and margin
* `NormalizeVolumeAsync()` - Normalize volume (similar concept for volumes)
* All order placement methods should use this to normalize SL/TP prices

---

## ⚠️ Common Pitfalls

1. **Using Math.Round(price, digits) instead**
   ```csharp
   // ❌ WRONG - rounding by digits
   var digits = await svc.GetDigitsAsync("EURUSD");
   double normalized = Math.Round(price, digits); // Not valid for orders!

   // ✅ CORRECT - normalizing by tick size
   double normalized = await svc.NormalizePriceAsync("EURUSD", price);
   ```

2. **Assuming tick size equals point size**
   ```csharp
   // ❌ WRONG - using point instead of tick
   var point = await svc.GetPointAsync("EURUSD");
   double normalized = Math.Round(price / point) * point; // Might differ!

   // ✅ CORRECT - using actual tick size
   double normalized = await svc.NormalizePriceAsync("EURUSD", price);
   ```

3. **Not normalizing calculated prices**
   ```csharp
   // ❌ WRONG - sending calculated price directly
   var point = await svc.GetPointAsync("EURUSD");
   double slPrice = entry - (100 * point);
   // OrderSendAsync(..., sl: slPrice, ...); // Might be invalid!

   // ✅ CORRECT - normalize before sending
   var point = await svc.GetPointAsync("EURUSD");
   double slPrice = await svc.NormalizePriceAsync("EURUSD",
       entry - (100 * point));
   // OrderSendAsync(..., sl: slPrice, ...);
   ```

4. **Normalizing volumes with this method**
   ```csharp
   // ❌ WRONG - this is for prices, not volumes
   double volume = 1.55;
   double normalized = await svc.NormalizePriceAsync("EURUSD", volume); // Wrong!

   // ✅ CORRECT - use NormalizeVolumeAsync for volumes
   double normalized = await svc.NormalizeVolumeAsync("EURUSD", volume);
   ```

5. **Not checking for errors**
   ```csharp
   // ❌ WRONG - no error handling
   double normalized = await svc.NormalizePriceAsync("EURUSD", price);

   // ✅ CORRECT - handle potential errors
   try
   {
       double normalized = await svc.NormalizePriceAsync("EURUSD", price);
       // Use normalized price
   }
   catch (InvalidOperationException ex)
   {
       Console.WriteLine($"Normalization failed: {ex.Message}");
       // Handle error
   }
   ```
