# Get Position Count (`GetPositionCountAsync`)

> **Monitoring method:** Count open market positions.

**API:** `MT5Service.GetPositionCountAsync(...)`
**Region:** [15] POSITION MONITORING

## Signature

```csharp
public static async Task<int> GetPositionCountAsync(
    this MT5Service svc,
    string? symbol = null,
    int timeoutSec = 20,
    CancellationToken ct = default)
```

## Examples

```csharp
// Total positions
int total = await svc.GetPositionCountAsync();
Console.WriteLine($"Open positions: {total}");

// Per-symbol count
int eurusd = await svc.GetPositionCountAsync(symbol: "EURUSD");

// Position limit enforcement
int maxPositions = 10;
int current = await svc.GetPositionCountAsync();

if (current < maxPositions)
{
    await svc.BuyMarketByRisk("EURUSD", 50, 100);
}
else
{
    Console.WriteLine($"⚠️ Position limit reached: {current}/{maxPositions}");
}
```

## Summary

- ✅ Returns int count
- ✅ Optional symbol filter
- ✅ Perfect for position limit enforcement
