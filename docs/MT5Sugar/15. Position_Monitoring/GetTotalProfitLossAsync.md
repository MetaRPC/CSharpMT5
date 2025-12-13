# Get Total Profit/Loss (`GetTotalProfitLossAsync`) ⭐

> **⭐ CRITICAL METHOD:** Calculate total unrealized P&L across all open positions.

**API:** `MT5Service.GetTotalProfitLossAsync(...)`
**Region:** [15] POSITION MONITORING

## Signature

```csharp
public static async Task<double> GetTotalProfitLossAsync(
    this MT5Service svc,
    string? symbol = null,
    int timeoutSec = 20,
    CancellationToken ct = default)
```

## Examples

```csharp
// Total P&L
double totalPL = await svc.GetTotalProfitLossAsync();
Console.WriteLine($"Portfolio P&L: ${totalPL:F2}");

// EUR USD P&L only
double eurusdPL = await svc.GetTotalProfitLossAsync(symbol: "EURUSD");

// Risk alert
if (totalPL < -500)
{
    Console.WriteLine("🚨 Portfolio down $500 - emergency close!");
    await svc.CloseAllPositions();
}

// Real-time monitoring
while (true)
{
    double pl = await svc.GetTotalProfitLossAsync();
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] P&L: ${pl:F2}");
    await Task.Delay(5000);
}
```

## Summary

- ✅ Returns total unrealized P&L (double)
- ✅ Optional symbol filter
- ⭐ **Essential for portfolio risk management**
