# Get Quote Session (`GetQuoteSessionAsync`)

> **Utility method:** Get quote session times for a symbol - when you can see prices.

**API:** `MT5Service.GetQuoteSessionAsync(...)`
**Region:** [14] SESSION TIME

## Signature

```csharp
public static Task<SymbolInfoSessionQuoteData> GetQuoteSessionAsync(
    this MT5Service svc,
    string symbol,
    mt5_term_api.DayOfWeek dayOfWeek,
    int sessionIndex = 0,
    int timeoutSec = 15,
    CancellationToken ct = default)
    => svc.SymbolInfoSessionQuoteAsync(symbol, dayOfWeek, sessionIndex, Dl(timeoutSec), ct);
```

## Examples

```csharp
// Get Monday quote session
var session = await svc.GetQuoteSessionAsync("EURUSD", DayOfWeek.Monday, sessionIndex: 0);
Console.WriteLine($"Quote hours: {session.From} - {session.To} UTC");

// Check all weekdays
for (int day = 1; day <= 5; day++)
{
    var s = await svc.GetQuoteSessionAsync("XAUUSD", (DayOfWeek)day, 0);
    Console.WriteLine($"{(DayOfWeek)day}: {s.From} - {s.To}");
}
```

## Summary

- ✅ Wrapper for `SymbolInfoSessionQuoteAsync()`
- ✅ Quote session = when prices are available
- ✅ Usually same as trade session
