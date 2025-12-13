# Get Losing Positions (`GetLosingPositionsAsync`)

> **Monitoring method:** Filter all positions with profit < 0.

**API:** `MT5Service.GetLosingPositionsAsync(...)`
**Region:** [15] POSITION MONITORING

## Signature

```csharp
public static async Task<List<object>> GetLosingPositionsAsync(
    this MT5Service svc,
    string? symbol = null,
    int timeoutSec = 20,
    CancellationToken ct = default)
```

## Examples

```csharp
// All losers
var losers = await svc.GetLosingPositionsAsync();
Console.WriteLine($"Losing positions: {losers.Count}");

// Cut losses if too many
if (losers.Count > winners.Count)
{
    foreach (var pos in losers)
    {
        var ticket = Convert.ToUInt64(pos.GetType().GetProperty("Ticket")?.GetValue(pos));
        await svc.CloseByTicket(ticket);
    }
}
```

## Summary

- ✅ Returns `List<object>` of losing positions
- ✅ Optional symbol filter
- ✅ Perfect for stop-loss strategies
