# Normalize Volume (`NormalizeVolumeAsync`)

> **Sugar method:** Normalizes volume to comply with broker's step size and min/max limits - auto-fix invalid volumes.

**API Information:**

* **Extension method:** `MT5Service.NormalizeVolumeAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Region:** [08] VOLUME & PRICE UTILITIES
* **Underlying calls:** `GetVolumeLimitsAsync()` + math normalization

---

## Method Signature

```csharp
public static async Task<double> NormalizeVolumeAsync(
    this MT5Service svc,
    string symbol,
    double volume,
    int timeoutSec = 10,
    CancellationToken ct = default)
```

---

## 🔽 Input

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance (extension method) |
| `symbol` | `string` | Symbol name (e.g., "EURUSD", "XAUUSD") |
| `volume` | `double` | Desired volume in lots (may be invalid) |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<double>` | **Normalized volume** that broker will accept (valid step + within min/max) |

---

## 💬 Just the essentials

* **What it is:** Auto-fix for invalid volumes - rounds to valid step and clamps to broker limits.
* **Why you need it:** Prevents broker rejection errors (invalid volume, invalid step, etc.).
* **Sanity check:** Always use before placing orders if volume comes from calculations or user input.

---

## 🎯 Purpose

Use it for:

* **User input validation** - Fix volumes entered in trading UI
* **Calculation results** - Round volumes from risk calculations
* **Partial closes** - Ensure partial volume is valid
* **Volume conversions** - Fix volumes from percentage calculations
* **Error prevention** - Catch and fix invalid volumes before broker

---

## 🔧 Under the Hood

```csharp
// Step 1: Get broker volume constraints
var (min, max, step) = await svc.GetVolumeLimitsAsync(symbol, timeoutSec, ct);

// Safety: ensure step is valid
if (step <= 0) step = 0.01;

// Step 2: Round to nearest valid step
double offset = volume - min;
double stepsCount = Math.Round(offset / step);
double normalized = min + (stepsCount * step);

// Step 3: Clamp to min/max range
normalized = Math.Max(min, Math.Min(max, normalized));

return normalized;
```

**What it improves:**

* **Auto-rounding** - aligns to broker step size
* **Auto-clamping** - ensures within min/max range
* **One call** - gets limits + normalizes
* **Error-free** - guaranteed valid volume

---

## 🔗 Usage Examples

### Example 1: Basic Normalization

```csharp
// User enters odd volume
double rawVolume = 0.037;

double normalized = await svc.NormalizeVolumeAsync("EURUSD", rawVolume);

Console.WriteLine($"Raw volume:        {rawVolume}");
Console.WriteLine($"Normalized volume: {normalized}");

// Output:
// Raw volume:        0.037
// Normalized volume: 0.04
```

---

### Example 2: Fix Calculation Result

```csharp
// Volume from risk calculation might not be perfectly aligned
double calculatedVolume = 0.1234567;

double normalized = await svc.NormalizeVolumeAsync("GBPUSD", calculatedVolume);

Console.WriteLine($"Calculated: {calculatedVolume}");
Console.WriteLine($"Normalized: {normalized}");

// Output:
// Calculated: 0.1234567
// Normalized: 0.12
```

---

### Example 3: Clamp to Maximum

```csharp
// User wants huge volume that exceeds broker max
double requestedVolume = 500.0;

double normalized = await svc.NormalizeVolumeAsync("BTCUSD", requestedVolume);

Console.WriteLine($"Requested: {requestedVolume} lots");
Console.WriteLine($"Normalized: {normalized} lots");

// Output (if BTCUSD max is 5.0):
// Requested: 500.0 lots
// Normalized: 5.0 lots
```

---

### Example 4: Clamp to Minimum

```csharp
// Very small volume below broker minimum
double tinyVolume = 0.001;

double normalized = await svc.NormalizeVolumeAsync("EURUSD", tinyVolume);

Console.WriteLine($"Tiny volume:  {tinyVolume}");
Console.WriteLine($"Normalized:   {normalized}");

// Output (if min is 0.01):
// Tiny volume:  0.001
// Normalized:   0.01
```

---

### Example 5: Percentage-Based Volume

```csharp
// Close 33.3% of position
double totalVolume = 0.15;
double closePercent = 33.3;
double rawCloseVolume = totalVolume * (closePercent / 100.0);

Console.WriteLine($"Total volume: {totalVolume}");
Console.WriteLine($"33.3% of {totalVolume} = {rawCloseVolume}");

double normalized = await svc.NormalizeVolumeAsync("EURUSD", rawCloseVolume);

Console.WriteLine($"Normalized close volume: {normalized}");

// Output:
// Total volume: 0.15
// 33.3% of 0.15 = 0.04995
// Normalized close volume: 0.05
```

---

### Example 6: Risk-Based Volume Normalization

```csharp
// CalcVolumeForRiskAsync might return non-perfect value
double riskBasedVolume = 0.2134;

double normalized = await svc.NormalizeVolumeAsync("XAUUSD", riskBasedVolume);

Console.WriteLine($"Risk calculation: {riskBasedVolume}");
Console.WriteLine($"Broker-ready:     {normalized}");

// Place order with normalized volume
await svc.BuyMarket("XAUUSD", normalized, sl: 2000.0, tp: 2050.0);
```

---

### Example 7: Multiple Volumes Batch Normalization

```csharp
double[] rawVolumes = { 0.037, 0.125, 0.003, 500.0, 0.5555 };

Console.WriteLine("Volume Normalization Results:");
Console.WriteLine("Raw      | Normalized");
Console.WriteLine("---------|----------");

foreach (var raw in rawVolumes)
{
    double normalized = await svc.NormalizeVolumeAsync("EURUSD", raw);
    Console.WriteLine($"{raw,8:F3} | {normalized,10:F2}");
}

// Output:
// Volume Normalization Results:
// Raw      | Normalized
// ---------|----------
//    0.037 |       0.04
//    0.125 |       0.13
//    0.003 |       0.01
//  500.000 |     100.00
//    0.556 |       0.56
```

---

### Example 8: Validation + Normalization

```csharp
public async Task<double> GetSafeVolume(
    MT5Service svc,
    string symbol,
    double desiredVolume)
{
    // Normalize
    double normalized = await svc.NormalizeVolumeAsync(symbol, desiredVolume);

    // Check if normalization changed the value
    if (Math.Abs(normalized - desiredVolume) > 0.0001)
    {
        Console.WriteLine($"⚠️ Volume adjusted: {desiredVolume} → {normalized}");
    }
    else
    {
        Console.WriteLine($"✅ Volume valid: {normalized}");
    }

    return normalized;
}

// Usage:
double safeVolume = await GetSafeVolume(svc, "GBPUSD", 0.037);
// Output: ⚠️ Volume adjusted: 0.037 → 0.04
// Returns: 0.04
```

---

### Example 9: Pre-Trade Normalization

```csharp
public async Task<OrderSendData> PlaceNormalizedOrder(
    MT5Service svc,
    string symbol,
    double rawVolume,
    bool isBuy)
{
    // Always normalize before placing order
    double volume = await svc.NormalizeVolumeAsync(symbol, rawVolume);

    Console.WriteLine($"Placing order:");
    Console.WriteLine($"  Symbol:    {symbol}");
    Console.WriteLine($"  Direction: {(isBuy ? "BUY" : "SELL")}");
    Console.WriteLine($"  Volume:    {volume} lots (normalized from {rawVolume})");

    return await svc.PlaceMarket(symbol, volume, isBuy);
}

// Usage:
await PlaceNormalizedOrder(svc, "EURUSD", 0.0347, isBuy: true);
```

---

### Example 10: Scale Out with Normalization

```csharp
public async Task ScaleOutPosition(
    MT5Service svc,
    ulong ticket,
    double[] percentages)
{
    // Get current position volume
    var opened = await svc.OpenedOrdersAsync(...);
    var position = opened.PositionInfos.First(p => p.Ticket == ticket);
    double totalVolume = position.Volume;

    Console.WriteLine($"Scaling out position #{ticket} (Total: {totalVolume} lots)");
    Console.WriteLine("");

    foreach (var percent in percentages)
    {
        // Calculate raw close volume
        double rawClose = totalVolume * (percent / 100.0);

        // Normalize to broker constraints
        double normalized = await svc.NormalizeVolumeAsync(position.PositionSymbol, rawClose);

        // Close partial
        await svc.CloseByTicket(ticket, volume: normalized);

        Console.WriteLine($"✅ Closed {percent}%: {rawClose:F4} → {normalized:F2} lots");

        totalVolume -= normalized;
    }

    Console.WriteLine($"\nRemaining: {totalVolume:F2} lots");
}

// Usage: Scale out in 25%, 50%, 25% stages
await ScaleOutPosition(svc, ticket: 12345, new[] { 25.0, 50.0, 25.0 });
```

---

## 🔗 Related Methods

**📦 Methods used internally:**

* `GetVolumeLimitsAsync()` - Retrieves min/max/step for normalization

**🍬 Methods that use this:**

* `CalcVolumeForRiskAsync()` - Auto-normalizes calculated volume
* `BuyMarketByRisk()` - Uses this after volume calculation
* `SellMarketByRisk()` - Uses this after volume calculation

**📊 Alternative approaches:**

* Manual normalization using GetVolumeLimitsAsync + math (more verbose)

---

## ⚠️ Common Pitfalls

1. **Not normalizing calculation results:**
   ```csharp
   // ❌ WRONG: Using raw calculation result
   double volume = balance * 0.02 / stopLoss;  // Might be 0.0374
   await svc.BuyMarket("EURUSD", volume);  // ❌ May fail!

   // ✅ CORRECT: Normalize first
   double rawVolume = balance * 0.02 / stopLoss;
   double volume = await svc.NormalizeVolumeAsync("EURUSD", rawVolume);
   await svc.BuyMarket("EURUSD", volume);  // ✅ Safe!
   ```

2. **Forgetting normalization on user input:**
   ```csharp
   // ❌ WRONG: Using user input directly
   double volume = GetUserInput();  // User enters 0.037
   await svc.PlaceMarket("EURUSD", volume, true);  // ❌ Invalid!

   // ✅ CORRECT: Normalize user input
   double rawVolume = GetUserInput();
   double volume = await svc.NormalizeVolumeAsync("EURUSD", rawVolume);
   await svc.PlaceMarket("EURUSD", volume, true);  // ✅ Valid!
   ```

3. **Assuming normalization preserves exact value:**
   ```csharp
   // ⚠️ Normalization changes value!
   double original = 0.037;
   double normalized = await svc.NormalizeVolumeAsync("EURUSD", original);
   // normalized is now 0.04, NOT 0.037!

   // Always check if value changed
   if (normalized != original)
   {
       Console.WriteLine($"Volume adjusted: {original} → {normalized}");
   }
   ```

---

## 📊 Normalization Examples

| Raw Volume | Step | Min | Max | Normalized | Reason |
|------------|------|-----|-----|------------|--------|
| 0.037 | 0.01 | 0.01 | 100 | 0.04 | Rounded to step |
| 0.003 | 0.01 | 0.01 | 100 | 0.01 | Clamped to min |
| 500.0 | 0.01 | 0.01 | 100 | 100.0 | Clamped to max |
| 0.125 | 0.01 | 0.01 | 100 | 0.13 | Rounded to step |
| 0.0999 | 0.01 | 0.01 | 100 | 0.10 | Rounded to step |

---

## 💡 Summary

**NormalizeVolumeAsync** is essential for error-free trading:

* ✅ Auto-rounds to valid step size
* ✅ Auto-clamps to min/max limits
* ✅ Prevents broker rejections
* ✅ Works with any raw volume input

```csharp
// Always normalize before trading:
double rawVolume = 0.037;  // From calculation or user input
double volume = await svc.NormalizeVolumeAsync("EURUSD", rawVolume);
// volume is now 0.04 - guaranteed valid!

await svc.BuyMarket("EURUSD", volume);  // ✅ No rejection!
```

**Normalize volumes, avoid errors!** 🚀
