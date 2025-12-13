# Check Margin Availability (`CheckMarginAvailabilityAsync`) ⭐

> **⭐ CRITICAL METHOD:** Check if account has enough free margin for order - one-shot pre-flight check!

**API:** `MT5Service.CheckMarginAvailabilityAsync(...)`
**Region:** [13] ORDER VALIDATION ⭐

## Signature

```csharp
public static async Task<(bool hasEnough, double freeMargin, double required)>
    CheckMarginAvailabilityAsync(
        this MT5Service svc,
        string symbol,
        double volume,
        bool isBuy,
        double price = 0,
        int timeoutSec = 20,
        CancellationToken ct = default)
```

## Returns

**Tuple:** `(bool hasEnough, double freeMargin, double required)`

- `hasEnough` - `true` if can afford order
- `freeMargin` - Current free margin
- `required` - Required margin for this order

## Quick Examples

### 1. Basic Check
```csharp
var (hasEnough, free, required) = await svc.CheckMarginAvailabilityAsync(
    "EURUSD",
    volume: 1.0,
    isBuy: true);

if (hasEnough)
{
    Console.WriteLine($"✅ Can trade: ${free:F2} >= ${required:F2}");
    await svc.BuyMarketAsync("EURUSD", 1.0);
}
else
{
    Console.WriteLine($"❌ Insufficient: ${free:F2} < ${required:F2}");
}
```

### 2. With Risk Management
```csharp
var (hasMargin, _, _) = await svc.CheckMarginAvailabilityAsync("EURUSD", 10.0, true);
if (hasMargin)
{
    await svc.BuyMarketByRisk("EURUSD", stopPoints: 50, riskMoney: 100);
}
else
{
    Console.WriteLine("⚠️ Insufficient margin - skipping trade");
}
```

### 3. Compare Buy vs Sell
```csharp
var (canBuy, freeBuy, reqBuy) = await svc.CheckMarginAvailabilityAsync(
    "EURUSD", 1.0, isBuy: true);
var (canSell, freeSell, reqSell) = await svc.CheckMarginAvailabilityAsync(
    "EURUSD", 1.0, isBuy: false);

Console.WriteLine($"BUY:  ${reqBuy:F2} - {(canBuy ? "✅" : "❌")}");
Console.WriteLine($"SELL: ${reqSell:F2} - {(canSell ? "✅" : "❌")}");
```

### 4. Multi-Symbol Portfolio Check
```csharp
var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY" };
double totalRequired = 0;

foreach (var symbol in symbols)
{
    var (_, _, required) = await svc.CheckMarginAvailabilityAsync(symbol, 0.10, true);
    totalRequired += required;
}

var (hasEnough, freeMargin, _) = await svc.CheckMarginAvailabilityAsync(
    symbols[0], 0, true);  // Get current free margin

Console.WriteLine($"Total margin needed: ${totalRequired:F2}");
Console.WriteLine($"Free margin: ${freeMargin:F2}");
Console.WriteLine(hasEnough ? "✅ Can trade all" : "❌ Insufficient for portfolio");
```

### 5. Progressive Volume Increase
```csharp
public async Task<double> FindMaxAffordableVolume(
    MT5Service svc,
    string symbol,
    bool isBuy)
{
    double volume = 0.01;
    double maxVolume = 0;

    while (volume <= 100.0)
    {
        var (hasEnough, _, _) = await svc.CheckMarginAvailabilityAsync(
            symbol, volume, isBuy);

        if (hasEnough)
        {
            maxVolume = volume;
            volume *= 2;  // Double volume
        }
        else
        {
            break;
        }
    }

    Console.WriteLine($"Max affordable volume: {maxVolume} lots");
    return maxVolume;
}

// Usage:
double maxVol = await FindMaxAffordableVolume(svc, "EURUSD", isBuy: true);
```

## Under the Hood

```csharp
// Step 1: Calculate required margin (parallel with free margin query)
var marginReq = new OrderCalcMarginRequest
{
    Symbol = symbol,
    OrderType = isBuy ? ENUM_ORDER_TYPE_TF.OrderTypeTfBuy : ENUM_ORDER_TYPE_TF.OrderTypeTfSell,
    Volume = volume,
    OpenPrice = price
};

var marginTask = svc.OrderCalcMarginAsync(marginReq, deadline, ct);
var freeMarginTask = svc.GetFreeMarginAsync(deadline, ct);

await Task.WhenAll(marginTask, freeMarginTask);  // Parallel execution

// Step 2: Compare
var requiredMargin = (await marginTask).Margin;
var freeMargin = await freeMarginTask;
bool hasEnough = freeMargin >= requiredMargin;

return (hasEnough, freeMargin, requiredMargin);
```

**Performance:** Executes margin calculation and free margin query in **parallel** for speed!

## Related Methods

- `CalculateBuyMarginAsync()` - BUY margin only
- `CalculateSellMarginAsync()` - SELL margin only
- `ValidateOrderAsync()` - Full validation (slower, more comprehensive)

## When to Use

| Method | Use When |
|--------|----------|
| `CheckMarginAvailabilityAsync()` ⭐ | **Quick margin check** before placing order |
| `CalculateBuyMarginAsync()` | Only need margin amount (no comparison) |
| `ValidateOrderAsync()` | Need full validation (symbol, stops, etc.) |

## Common Pitfalls

```csharp
// ❌ WRONG: Ignoring the check
await svc.BuyMarketAsync("EURUSD", 10.0);  // May fail!

// ✅ CORRECT: Always check first
var (hasEnough, _, _) = await svc.CheckMarginAvailabilityAsync("EURUSD", 10.0, true);
if (hasEnough)
{
    await svc.BuyMarketAsync("EURUSD", 10.0);
}
```

## Summary

**CheckMarginAvailabilityAsync** provides one-shot margin verification:

* ✅ Returns bool + both values (free and required)
* ✅ **Parallel execution** for speed
* ✅ Perfect for pre-trade validation
* ⭐ **Use before ALL large orders!**

```csharp
// Professional pattern:
var (hasMargin, free, required) = await svc.CheckMarginAvailabilityAsync(
    "EURUSD", 1.0, isBuy: true);

if (hasMargin)
{
    await svc.BuyMarketByRisk("EURUSD", 50, 100);
}
else
{
    Console.WriteLine($"Need ${required:F2}, have ${free:F2}");
}
```

**Check margin first, avoid margin calls!** 🛡️
