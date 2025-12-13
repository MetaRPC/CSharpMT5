# Get Trade Session (`GetTradeSessionAsync`)

> **Utility method:** Get trade session times for a symbol - when you can execute orders.

**API:** `MT5Service.GetTradeSessionAsync(...)`
**Region:** [14] SESSION TIME

## Signature

```csharp
public static Task<SymbolInfoSessionTradeData> GetTradeSessionAsync(
    this MT5Service svc,
    string symbol,
    mt5_term_api.DayOfWeek dayOfWeek,
    int sessionIndex = 0,
    int timeoutSec = 15,
    CancellationToken ct = default)
    => svc.SymbolInfoSessionTradeAsync(symbol, dayOfWeek, sessionIndex, Dl(timeoutSec), ct);
```

## Examples

```csharp
// Get Friday trade session
var session = await svc.GetTradeSessionAsync("EURUSD", DayOfWeek.Friday, sessionIndex: 0);
Console.WriteLine($"Trading hours: {session.From} - {session.To} UTC");

// Check if market is open now
var now = DateTime.UtcNow;
var todaySession = await svc.GetTradeSessionAsync(
    "EURUSD",
    (mt5_term_api.DayOfWeek)(int)now.DayOfWeek,
    0);

var from = TimeSpan.Parse(todaySession.From);
var to = TimeSpan.Parse(todaySession.To);
bool isOpen = now.TimeOfDay >= from && now.TimeOfDay <= to;

Console.WriteLine(isOpen ? "🟢 Market OPEN" : "🔴 Market CLOSED");
```

## Summary

- ✅ Wrapper for `SymbolInfoSessionTradeAsync()`
- ✅ Trade session = when orders can execute
- ✅ Essential for automated trading bots
