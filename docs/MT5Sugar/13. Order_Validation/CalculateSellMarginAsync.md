# Calculate Sell Margin (`CalculateSellMarginAsync`)

> **Sugar method:** Calculate exact margin required for SELL order before placing it.

**API:** `MT5Service.CalculateSellMarginAsync(...)`
**Region:** [13] ORDER VALIDATION ⭐

## Signature

```csharp
public static async Task<double> CalculateSellMarginAsync(
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

## Quick Examples

```csharp
// Market order margin
double margin = await svc.CalculateSellMarginAsync("EURUSD", 0.10);
Console.WriteLine($"Sell margin: ${margin:F2}");

// Pending order margin
double margin = await svc.CalculateSellMarginAsync("EURUSD", 0.50, price: 1.09000);

// Check affordability
double freeMargin = await svc.GetFreeMarginAsync();
double required = await svc.CalculateSellMarginAsync("EURUSD", 1.0);
if (freeMargin >= required)
{
    await svc.SellMarketAsync("EURUSD", 1.0);
}
```

## Related Methods

- `CalculateBuyMarginAsync()` - BUY margin calculation
- `CheckMarginAvailabilityAsync()` - Margin check + comparison
- `ValidateOrderAsync()` - Full order validation

## Summary

**CalculateSellMarginAsync** calculates exact SELL margin:

* ✅ Same as BUY margin (usually)
* ✅ Works for market and pending
* ✅ Essential for short trading planning
* ✅ Prevents margin call errors
