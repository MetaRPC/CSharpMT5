# Calculate Buy Margin (`CalculateBuyMarginAsync`)

> **Sugar method:** Calculate exact margin required for BUY order before placing it.

**API:** `MT5Service.CalculateBuyMarginAsync(...)`  
**Region:** [13] ORDER VALIDATION ⭐

## Signature

```csharp
public static async Task<double> CalculateBuyMarginAsync(
    this MT5Service svc,
    string symbol,
    double volume,
    double price = 0,
    int timeoutSec = 15,
    CancellationToken ct = default)
```

## Parameters

| Param | Type | Description |
|-------|------|-------------|
| `symbol` | `string` | Symbol (e.g., "EURUSD") |
| `volume` | `double` | Volume in lots |
| `price` | `double` | Entry price (0 = market price) |

## Returns

`Task<double>` - Required margin in account currency

## Examples

### 1. Market Order Margin
```csharp
double margin = await svc.CalculateBuyMarginAsync("EURUSD", volume: 0.10);
Console.WriteLine($"Margin for 0.10 lots: ${margin:F2}");
// Output: Margin for 0.10 lots: $10.85
```

### 2. Pending Order Margin
```csharp
double margin = await svc.CalculateBuyMarginAsync(
    "EURUSD",
    volume: 0.50,
    price: 1.08500);
Console.WriteLine($"Margin at 1.08500: ${margin:F2}");
```

### 3. Compare Multiple Volumes
```csharp
double[] volumes = { 0.01, 0.10, 1.0, 10.0 };
Console.WriteLine("Volume  | Margin");
Console.WriteLine("--------|--------");

foreach (var vol in volumes)
{
    double margin = await svc.CalculateBuyMarginAsync("EURUSD", vol);
    Console.WriteLine($"{vol,7} | ${margin,7:F2}");
}
```

### 4. Check Affordability
```csharp
double freeMargin = await svc.GetFreeMarginAsync();
double requiredMargin = await svc.CalculateBuyMarginAsync("EURUSD", 1.0);

if (freeMargin >= requiredMargin)
{
    Console.WriteLine($"✅ Can afford: ${freeMargin:F2} >= ${requiredMargin:F2}");
    await svc.BuyMarketAsync("EURUSD", 1.0);
}
else
{
    Console.WriteLine($"❌ Insufficient: ${freeMargin:F2} < ${requiredMargin:F2}");
}
```

### 5. Multi-Symbol Margin Planning
```csharp
var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY" };
double totalMargin = 0;

foreach (var symbol in symbols)
{
    double margin = await svc.CalculateBuyMarginAsync(symbol, 0.10);
    totalMargin += margin;
    Console.WriteLine($"{symbol}: ${margin:F2}");
}

Console.WriteLine($"Total margin needed: ${totalMargin:F2}");
```

## Related Methods

- `CalculateSellMarginAsync()` - SELL margin calculation
- `CheckMarginAvailabilityAsync()` - Margin check + comparison
- `ValidateOrderAsync()` - Full order validation

## Summary

**CalculateBuyMarginAsync** calculates exact BUY margin:

* ✅ Quick margin estimation
* ✅ Works for market and pending orders  
* ✅ Essential for portfolio planning
* ✅ Prevents "Not enough money" errors

```csharp
double margin = await svc.CalculateBuyMarginAsync("EURUSD", 0.10);
if (await svc.GetFreeMarginAsync() >= margin)
{
    await svc.BuyMarketAsync("EURUSD", 0.10);
}
```
