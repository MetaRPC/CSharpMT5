# Get Profitable Positions (`GetProfitablePositionsAsync`)

> **Monitoring method:** Filter all positions with profit > 0.

**API:** `MT5Service.GetProfitablePositionsAsync(...)`
**Region:** [15] POSITION MONITORING

## Signature

```csharp
public static async Task<List<object>> GetProfitablePositionsAsync(
    this MT5Service svc,
    string? symbol = null,
    int timeoutSec = 20,
    CancellationToken ct = default)
```

## Examples

```csharp
// All winners
var winners = await svc.GetProfitablePositionsAsync();
Console.WriteLine($"Winning positions: {winners.Count}");

// EURUSD winners only
var eurusdWinners = await svc.GetProfitablePositionsAsync(symbol: "EURUSD");

// Close all winners
foreach (var pos in winners)
{
    var ticket = Convert.ToUInt64(pos.GetType().GetProperty("Ticket")?.GetValue(pos));
    await svc.CloseByTicket(ticket);
}
```

## Summary

- ✅ Returns `List<object>` of winning positions
- ✅ Optional symbol filter
- ✅ Perfect for profit-taking strategies
