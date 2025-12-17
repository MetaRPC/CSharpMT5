# Get Volume Limits (`GetVolumeLimitsAsync`)

> **Sugar method:** Retrieves volume constraints for symbol - minimum, maximum, and step size in one call.

**API Information:**

* **Extension method:** `MT5Service.GetVolumeLimitsAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Region:** [08] VOLUME & PRICE UTILITIES
* **Underlying calls:** `SymbolInfoDoubleAsync()` (3 calls)

---

## Method Signature

```csharp
public static async Task<(double min, double max, double step)> GetVolumeLimitsAsync(
    this MT5Service svc,
    string symbol,
    int timeoutSec = 10,
    CancellationToken ct = default)
```

---

## 🔽 Input

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance (extension method) |
| `symbol` | `string` | Symbol name (e.g., "EURUSD", "XAUUSD") |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<(double min, double max, double step)>` | Tuple with minimum volume, maximum volume, and volume step |

### Tuple Fields

- **min** (double) - Minimum lot size allowed by broker (e.g., 0.01)
- **max** (double) - Maximum lot size allowed by broker (e.g., 100.0)
- **step** (double) - Volume increment step (e.g., 0.01)

---

## 💬 Just the essentials

* **What it is:** Gets broker's volume constraints for symbol - min/max/step in one convenient call.
* **Why you need it:** Validate volumes before placing orders, avoid broker rejections.
* **Sanity check:** Step defines smallest increment (e.g., 0.01 = can use 0.01, 0.02, 0.03 but NOT 0.015).

---

## 🎯 Purpose

Use it for:

* **Volume validation** - Check if requested volume is valid before order
* **Volume normalization** - Round volume to valid step
* **UI constraints** - Set min/max bounds in trading interfaces
* **Risk calculations** - Understand broker limits for position sizing
* **Error prevention** - Catch invalid volumes before broker rejects

---

## 🔧 Under the Hood

```csharp
var deadline = DateTime.UtcNow.AddSeconds(timeoutSec);

// Fetch all three volume properties in parallel potential
var min = await svc.SymbolInfoDoubleAsync(symbol, SymbolVolumeMin, deadline, ct);
var max = await svc.SymbolInfoDoubleAsync(symbol, SymbolVolumeMax, deadline, ct);
var step = await svc.SymbolInfoDoubleAsync(symbol, SymbolVolumeStep, deadline, ct);

return (min, max, step);
```

**What it improves:**

* **One call** - returns all three values as tuple
* **Shared deadline** - all calls use same timeout
* **Tuple destructuring** - easy to extract values
* **Type-safe** - no magic strings or manual parsing

---

## 🔗 Usage Examples

### Example 1: Basic Volume Limits

```csharp
var (min, max, step) = await svc.GetVolumeLimitsAsync("EURUSD");

Console.WriteLine($"EURUSD Volume Constraints:");
Console.WriteLine($"  Minimum: {min} lots");
Console.WriteLine($"  Maximum: {max} lots");
Console.WriteLine($"  Step:    {step} lots");

// Output:
// EURUSD Volume Constraints:
//   Minimum: 0.01 lots
//   Maximum: 100.0 lots
//   Step:    0.01 lots
```

---

### Example 2: Volume Validation

```csharp
double requestedVolume = 0.037;  // User wants this volume

var (min, max, step) = await svc.GetVolumeLimitsAsync("EURUSD");

// Check if within range
if (requestedVolume < min)
{
    Console.WriteLine($"❌ Volume {requestedVolume} below minimum {min}");
}
else if (requestedVolume > max)
{
    Console.WriteLine($"❌ Volume {requestedVolume} above maximum {max}");
}
else if ((requestedVolume % step) != 0)
{
    Console.WriteLine($"⚠️ Volume {requestedVolume} not aligned with step {step}");
    // Need to normalize!
}
else
{
    Console.WriteLine($"✅ Volume {requestedVolume} is valid");
}
```

---

### Example 3: Manual Volume Normalization

```csharp
double rawVolume = 0.037;

var (min, max, step) = await svc.GetVolumeLimitsAsync("EURUSD");

// Normalize to nearest valid step
double normalized = Math.Round((rawVolume - min) / step) * step + min;

// Clamp to min/max range
normalized = Math.Max(min, Math.Min(max, normalized));

Console.WriteLine($"Raw volume:        {rawVolume}");
Console.WriteLine($"Normalized volume: {normalized}");
Console.WriteLine($"Step:              {step}");

// Output:
// Raw volume:        0.037
// Normalized volume: 0.04
// Step:              0.01
```

---

### Example 4: Check Multiple Symbols

```csharp
string[] symbols = { "EURUSD", "GBPUSD", "XAUUSD", "BTCUSD" };

Console.WriteLine("Symbol   | Min    | Max     | Step");
Console.WriteLine("---------|--------|---------|-------");

foreach (var symbol in symbols)
{
    var (min, max, step) = await svc.GetVolumeLimitsAsync(symbol);
    Console.WriteLine($"{symbol,-8} | {min,6} | {max,7} | {step,5}");
}

// Output:
// Symbol   | Min    | Max     | Step
// ---------|--------|---------|-------
// EURUSD   |   0.01 |   100.0 |  0.01
// GBPUSD   |   0.01 |   100.0 |  0.01
// XAUUSD   |   0.01 |    50.0 |  0.01
// BTCUSD   |   0.01 |     5.0 |  0.01
```

---

### Example 5: UI Volume Selector

```csharp
public async Task<VolumeSelector> CreateVolumeSelector(MT5Service svc, string symbol)
{
    var (min, max, step) = await svc.GetVolumeLimitsAsync(symbol);

    return new VolumeSelector
    {
        Symbol = symbol,
        MinValue = min,
        MaxValue = max,
        StepSize = step,
        DefaultValue = min  // Start at minimum
    };
}

// Usage in UI:
var selector = await CreateVolumeSelector(svc, "EURUSD");
// Configure slider/spinner: min=0.01, max=100.0, step=0.01
```

---

### Example 6: Smart Volume Adjustment

```csharp
public async Task<double> AdjustVolumeToLimits(
    MT5Service svc,
    string symbol,
    double desiredVolume)
{
    var (min, max, step) = await svc.GetVolumeLimitsAsync(symbol);

    // Too small?
    if (desiredVolume < min)
    {
        Console.WriteLine($"⚠️ Desired {desiredVolume} below min {min}, using {min}");
        return min;
    }

    // Too large?
    if (desiredVolume > max)
    {
        Console.WriteLine($"⚠️ Desired {desiredVolume} above max {max}, using {max}");
        return max;
    }

    // Round to step
    double rounded = Math.Round((desiredVolume - min) / step) * step + min;

    if (rounded != desiredVolume)
    {
        Console.WriteLine($"⚠️ Adjusted {desiredVolume} → {rounded} (step: {step})");
    }

    return rounded;
}

// Usage:
double adjusted = await AdjustVolumeToLimits(svc, "EURUSD", 0.037);
// Output: ⚠️ Adjusted 0.037 → 0.04 (step: 0.01)
// Returns: 0.04
```

---

### Example 7: Calculate Available Steps

```csharp
var (min, max, step) = await svc.GetVolumeLimitsAsync("EURUSD");

// Calculate how many valid volumes exist
int numberOfSteps = (int)Math.Round((max - min) / step) + 1;

Console.WriteLine($"Symbol: EURUSD");
Console.WriteLine($"Range: {min} - {max} lots");
Console.WriteLine($"Step:  {step} lots");
Console.WriteLine($"Valid volumes: {numberOfSteps} different values");

// First 10 valid volumes
Console.WriteLine("\nFirst 10 valid volumes:");
for (int i = 0; i < 10; i++)
{
    double volume = min + (i * step);
    Console.WriteLine($"  {i + 1}. {volume:F2} lots");
}
```

---

### Example 8: Pre-Trade Volume Check

```csharp
public async Task<bool> IsVolumeValid(
    MT5Service svc,
    string symbol,
    double volume)
{
    var (min, max, step) = await svc.GetVolumeLimitsAsync(symbol);

    // Check min/max
    if (volume < min || volume > max)
    {
        Console.WriteLine($"❌ Volume {volume} outside range [{min}, {max}]");
        return false;
    }

    // Check step alignment
    double offset = volume - min;
    double remainder = offset % step;

    if (Math.Abs(remainder) > 0.0000001)  // Floating point tolerance
    {
        Console.WriteLine($"❌ Volume {volume} not aligned with step {step}");
        return false;
    }

    Console.WriteLine($"✅ Volume {volume} is valid");
    return true;
}

// Usage:
if (await IsVolumeValid(svc, "EURUSD", 0.03))
{
    // Place order
}
```

---

### Example 9: Display Volume Range

```csharp
var (min, max, step) = await svc.GetVolumeLimitsAsync("XAUUSD");

Console.WriteLine($"Gold (XAUUSD) Trading Limits:");
Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine($"Minimum lot:  {min}");
Console.WriteLine($"Maximum lot:  {max}");
Console.WriteLine($"Step size:    {step}");
Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine($"\nExample valid volumes:");
Console.WriteLine($"  {min} (minimum)");
Console.WriteLine($"  {min + step}");
Console.WriteLine($"  {min + 2 * step}");
Console.WriteLine($"  ...");
Console.WriteLine($"  {max} (maximum)");
```

---

### Example 10: Compare Limits Across Symbols

```csharp
var symbols = new[] { "EURUSD", "XAUUSD", "BTCUSD" };

Console.WriteLine("Comparing Volume Limits:");
Console.WriteLine("");

foreach (var symbol in symbols)
{
    var (min, max, step) = await svc.GetVolumeLimitsAsync(symbol);

    double range = max - min;
    int steps = (int)((max - min) / step);

    Console.WriteLine($"{symbol}:");
    Console.WriteLine($"  Range:     {min} - {max} ({range} lots)");
    Console.WriteLine($"  Step:      {step}");
    Console.WriteLine($"  # of steps: {steps}");
    Console.WriteLine("");
}
```

---

## 🔗 Related Methods

**📦 Low-level methods used internally:**

* `SymbolInfoDoubleAsync(SymbolVolumeMin)` - Minimum lot size
* `SymbolInfoDoubleAsync(SymbolVolumeMax)` - Maximum lot size
* `SymbolInfoDoubleAsync(SymbolVolumeStep)` - Volume increment

**🍬 Methods that use this:**

* `NormalizeVolumeAsync()` - Uses limits to normalize volume
* `CalcVolumeForRiskAsync()` - Uses limits for final normalization

**📊 Alternative approaches:**

* Call `SymbolInfoDoubleAsync()` three times manually (more verbose)

---

## ⚠️ Common Pitfalls

1. **Assuming fixed limits:**
   ```csharp
   // ❌ WRONG: Hardcoding limits
   double min = 0.01;  // May be different for other symbols!
   double max = 100.0;

   // ✅ CORRECT: Query actual limits
   var (min, max, step) = await svc.GetVolumeLimitsAsync(symbol);
   ```

2. **Ignoring step size:**
   ```csharp
   // ❌ WRONG: Using volume not aligned with step
   double volume = 0.015;  // If step is 0.01, this is INVALID

   // ✅ CORRECT: Round to step
   var (min, max, step) = await svc.GetVolumeLimitsAsync(symbol);
   double volume = Math.Round(0.015 / step) * step;  // → 0.02
   ```

3. **Not checking maximum:**
   ```csharp
   // ❌ WRONG: Assuming large volumes allowed
   double volume = 500.0;  // May exceed broker max!

   // ✅ CORRECT: Check against max
   var (min, max, step) = await svc.GetVolumeLimitsAsync(symbol);
   if (volume > max)
   {
       Console.WriteLine($"⚠️ Volume {volume} exceeds max {max}");
       volume = max;
   }
   ```

4. **Different symbols have different limits:**
   ```csharp
   // ⚠️ EURUSD might allow 100 lots, but BTCUSD might only allow 5!
   var (minEUR, maxEUR, stepEUR) = await svc.GetVolumeLimitsAsync("EURUSD");
   var (minBTC, maxBTC, stepBTC) = await svc.GetVolumeLimitsAsync("BTCUSD");

   Console.WriteLine($"EURUSD max: {maxEUR}");  // → 100.0
   Console.WriteLine($"BTCUSD max: {maxBTC}");  // → 5.0
   ```

---

## 📊 Typical Values by Broker Type

### Standard Forex Brokers
- Min: 0.01 lots
- Max: 100.0 - 500.0 lots
- Step: 0.01 lots

### Crypto Brokers
- Min: 0.01 lots
- Max: 5.0 - 50.0 lots (lower due to volatility)
- Step: 0.01 lots

### Prop Firms / Funded Accounts
- Min: 0.01 lots
- Max: Varies (often 10-20 lots max)
- Step: 0.01 lots

**Always query actual limits - don't assume!**

---

## 💡 Summary

**GetVolumeLimitsAsync** provides essential volume constraints:

* ✅ One call returns all three limits (min/max/step)
* ✅ Tuple destructuring for clean code
* ✅ Essential for volume validation
* ✅ Prevents broker rejections

```csharp
// Get all limits in one call:
var (min, max, step) = await svc.GetVolumeLimitsAsync("EURUSD");

// Validate volume:
bool valid = volume >= min && volume <= max && (volume % step) == 0;

// Or use NormalizeVolumeAsync for automatic adjustment:
double normalized = await svc.NormalizeVolumeAsync("EURUSD", rawVolume);
```

**Know your limits, trade smart!** 🚀
