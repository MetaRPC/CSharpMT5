# ✅ Get Quote Session Times (`SymbolInfoSessionQuoteAsync`)

> **Request:** Get beginning and end times for a quoting session of a specified symbol and day of week on **MT5**.

**API Information:**

* **SDK wrapper:** `MT5Account.SymbolInfoSessionQuoteAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `SymbolInfoSessionQuote` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `SymbolInfoSessionQuote(SymbolInfoSessionQuoteRequest) → SymbolInfoSessionQuoteReply`
* **Low‑level client (generated):** `MarketInfo.MarketInfoClient.SymbolInfoSessionQuote(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<SymbolInfoSessionQuoteData> SymbolInfoSessionQuoteAsync(
            SymbolInfoSessionQuoteRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`SymbolInfoSessionQuoteRequest { symbol, day_of_week, session_index }`


**Reply message:**

`SymbolInfoSessionQuoteReply { data: SymbolInfoSessionQuoteData }`

---

## 🔽 Input

| Parameter           | Type                              | Description                                               |
| ------------------- | --------------------------------- | --------------------------------------------------------- |
| `request`           | `SymbolInfoSessionQuoteRequest`   | Protobuf request with session parameters                  |
| `deadline`          | `DateTime?`                       | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`               | Cooperative cancel for the call/retry loop                |

### `SymbolInfoSessionQuoteRequest`

| Field          | Type        | Description                                                          |
| -------------- | ----------- | -------------------------------------------------------------------- |
| `Symbol`       | `string`    | Symbol name (e.g., `"EURUSD"`, `"XAUUSD"`) **REQUIRED**              |
| `DayOfWeek`    | `DayOfWeek` | Day of the week (Sunday, Monday, etc.) **REQUIRED**                  |
| `SessionIndex` | `uint32`    | Session index (starts from 0). Most symbols have 1-2 sessions/day    |

---

## ⬆️ Output — `SymbolInfoSessionQuoteData`

| Field  | Type        | Description                                                                     |
| ------ | ----------- | ------------------------------------------------------------------------------- |
| `From` | `Timestamp` | Session beginning time (seconds from 00:00, date should be ignored)             |
| `To`   | `Timestamp` | Session end time (seconds from 00:00, date should be ignored)                   |

---

## 🧱 Related enums (from proto)

### `DayOfWeek`

* `Sunday` — Sunday (0)
* `Monday` — Monday (1)
* `Tuesday` — Tuesday (2)
* `Wednesday` — Wednesday (3)
* `Thursday` — Thursday (4)
* `Friday` — Friday (5)
* `Saturday` — Saturday (6)

---

## 💬 Just the essentials

* **What it is.** Returns quoting session hours for a symbol on a specific day of week. Quoting sessions determine when price updates are available.
* **Why you need it.** Check if quotes are available at specific times, schedule quote-dependent strategies, understand symbol's market hours.
* **Sanity check.** If `From` < `To` → valid session. Time is in seconds from midnight. Ignore date component, use only time-of-day.

---

## 🎯 Purpose

Use it to understand quoting hours:

* Check when symbol receives price updates.
* Determine if quotes are available at current time.
* Schedule strategies around quoting sessions.
* Understand market hours for different symbols.

---

## 🧩 Notes & Tips

* **Quote vs Trade sessions:** Quoting sessions show when prices are updated. Trading sessions (SymbolInfoSessionTrade) show when trades can be executed.
* **Session index:** Starts from 0. Most symbols have 1 session per day. Some (like futures) may have multiple sessions.
* **Time format:** Returned timestamps represent time-of-day only. Date component should be ignored.
* **Extract time:** Use `timestamp.ToDateTime().TimeOfDay` to get time as TimeSpan.
* **Weekend sessions:** Some symbols have no quoting sessions on weekends (Saturday/Sunday).
* **Error handling:** If session index doesn't exist, server may return error or invalid data.

---

## 🔗 Usage Examples

### 1) Basic quote session retrieval

```csharp
// acc — connected MT5Account

var result = await acc.SymbolInfoSessionQuoteAsync(new SymbolInfoSessionQuoteRequest
{
    Symbol = "EURUSD",
    DayOfWeek = DayOfWeek.Monday,
    SessionIndex = 0
});

var fromTime = result.From.ToDateTime().TimeOfDay;
var toTime = result.To.ToDateTime().TimeOfDay;

Console.WriteLine($"EURUSD Monday quote session: {fromTime} - {toTime}");
```

---

### 2) Get all quote sessions for a day

```csharp
var symbol = "ES"; // E-mini S&P 500 futures
var day = DayOfWeek.Wednesday;

Console.WriteLine($"{symbol} quote sessions for {day}:\n");

for (uint sessionIndex = 0; sessionIndex < 10; sessionIndex++)
{
    try
    {
        var result = await acc.SymbolInfoSessionQuoteAsync(new SymbolInfoSessionQuoteRequest
        {
            Symbol = symbol,
            DayOfWeek = day,
            SessionIndex = sessionIndex
        });

        var fromTime = result.From.ToDateTime().TimeOfDay;
        var toTime = result.To.ToDateTime().TimeOfDay;

        Console.WriteLine($"Session {sessionIndex}: {fromTime} - {toTime}");
    }
    catch
    {
        // No more sessions
        break;
    }
}
```

---

### 3) Check if quotes are available now

```csharp
var symbol = "XAUUSD";
var now = DateTime.UtcNow;
var currentDay = (DayOfWeek)((int)now.DayOfWeek); // Convert to proto DayOfWeek
var currentTime = now.TimeOfDay;

bool quotesAvailable = false;

// Check all sessions for current day
for (uint sessionIndex = 0; sessionIndex < 5; sessionIndex++)
{
    try
    {
        var result = await acc.SymbolInfoSessionQuoteAsync(new SymbolInfoSessionQuoteRequest
        {
            Symbol = symbol,
            DayOfWeek = currentDay,
            SessionIndex = sessionIndex
        });

        var fromTime = result.From.ToDateTime().TimeOfDay;
        var toTime = result.To.ToDateTime().TimeOfDay;

        if (currentTime >= fromTime && currentTime <= toTime)
        {
            quotesAvailable = true;
            Console.WriteLine($"✓ {symbol} quotes available (session {sessionIndex}: {fromTime} - {toTime})");
            break;
        }
    }
    catch
    {
        break;
    }
}

if (!quotesAvailable)
{
    Console.WriteLine($"✗ {symbol} quotes not available at {currentTime}");
}
```

---

### 4) Get quote hours for entire week

```csharp
var symbol = "BTCUSD";

Console.WriteLine($"Weekly quote schedule for {symbol}:\n");

var days = new[]
{
    DayOfWeek.Sunday,
    DayOfWeek.Monday,
    DayOfWeek.Tuesday,
    DayOfWeek.Wednesday,
    DayOfWeek.Thursday,
    DayOfWeek.Friday,
    DayOfWeek.Saturday
};

foreach (var day in days)
{
    Console.WriteLine($"{day}:");

    bool hasSession = false;
    for (uint sessionIndex = 0; sessionIndex < 5; sessionIndex++)
    {
        try
        {
            var result = await acc.SymbolInfoSessionQuoteAsync(new SymbolInfoSessionQuoteRequest
            {
                Symbol = symbol,
                DayOfWeek = day,
                SessionIndex = sessionIndex
            });

            var fromTime = result.From.ToDateTime().TimeOfDay;
            var toTime = result.To.ToDateTime().TimeOfDay;

            Console.WriteLine($"  Session {sessionIndex}: {fromTime:hh\\:mm} - {toTime:hh\\:mm}");
            hasSession = true;
        }
        catch
        {
            break;
        }
    }

    if (!hasSession)
    {
        Console.WriteLine("  No quote sessions");
    }

    Console.WriteLine();
}
```

---

### 5) Calculate time until next quote session

```csharp
var symbol = "EURUSD";
var now = DateTime.UtcNow;
var currentDay = (DayOfWeek)((int)now.DayOfWeek);
var currentTime = now.TimeOfDay;

// Try to find next session
TimeSpan? timeUntilNextSession = null;

for (int dayOffset = 0; dayOffset < 7; dayOffset++)
{
    var checkDay = (DayOfWeek)(((int)currentDay + dayOffset) % 7);

    for (uint sessionIndex = 0; sessionIndex < 5; sessionIndex++)
    {
        try
        {
            var result = await acc.SymbolInfoSessionQuoteAsync(new SymbolInfoSessionQuoteRequest
            {
                Symbol = symbol,
                DayOfWeek = checkDay,
                SessionIndex = sessionIndex
            });

            var fromTime = result.From.ToDateTime().TimeOfDay;

            // If same day, check if session hasn't started yet
            if (dayOffset == 0 && fromTime > currentTime)
            {
                timeUntilNextSession = fromTime - currentTime;
                break;
            }
            // If different day, use first session of that day
            else if (dayOffset > 0)
            {
                timeUntilNextSession = TimeSpan.FromDays(dayOffset) + fromTime - currentTime;
                break;
            }
        }
        catch
        {
            break;
        }
    }

    if (timeUntilNextSession.HasValue)
        break;
}

if (timeUntilNextSession.HasValue)
{
    Console.WriteLine($"Next {symbol} quote session starts in: {timeUntilNextSession.Value.TotalHours:F1} hours");
}
```

---

### 6) Compare quote sessions across symbols

```csharp
var symbols = new[] { "EURUSD", "USDJPY", "XAUUSD", "BTCUSD" };
var day = DayOfWeek.Monday;

Console.WriteLine($"Quote sessions for {day}:\n");

foreach (var symbol in symbols)
{
    try
    {
        var result = await acc.SymbolInfoSessionQuoteAsync(new SymbolInfoSessionQuoteRequest
        {
            Symbol = symbol,
            DayOfWeek = day,
            SessionIndex = 0
        });

        var fromTime = result.From.ToDateTime().TimeOfDay;
        var toTime = result.To.ToDateTime().TimeOfDay;
        var duration = toTime - fromTime;

        Console.WriteLine($"{symbol,-10} {fromTime:hh\\:mm} - {toTime:hh\\:mm} (duration: {duration.TotalHours:F1}h)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{symbol,-10} Error: {ex.Message}");
    }
}
```
