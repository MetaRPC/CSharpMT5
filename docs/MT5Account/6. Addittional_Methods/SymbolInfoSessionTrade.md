# ✅ Get Trade Session Times (`SymbolInfoSessionTradeAsync`)

> **Request:** Get beginning and end times for a trading session of a specified symbol and day of week on **MT5**.

**API Information:**

* **SDK wrapper:** `MT5Account.SymbolInfoSessionTradeAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `SymbolInfoSessionTrade` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `SymbolInfoSessionTrade(SymbolInfoSessionTradeRequest) → SymbolInfoSessionTradeReply`
* **Low‑level client (generated):** `MarketInfo.MarketInfoClient.SymbolInfoSessionTrade(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<SymbolInfoSessionTradeData> SymbolInfoSessionTradeAsync(
            SymbolInfoSessionTradeRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`SymbolInfoSessionTradeRequest { symbol, day_of_week, session_index }`


**Reply message:**

`SymbolInfoSessionTradeReply { data: SymbolInfoSessionTradeData }`

---

## 🔽 Input

| Parameter           | Type                              | Description                                               |
| ------------------- | --------------------------------- | --------------------------------------------------------- |
| `request`           | `SymbolInfoSessionTradeRequest`   | Protobuf request with session parameters                  |
| `deadline`          | `DateTime?`                       | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`               | Cooperative cancel for the call/retry loop                |

### `SymbolInfoSessionTradeRequest`

| Field          | Type        | Description                                                          |
| -------------- | ----------- | -------------------------------------------------------------------- |
| `Symbol`       | `string`    | Symbol name (e.g., `"EURUSD"`, `"XAUUSD"`) **REQUIRED**              |
| `DayOfWeek`    | `DayOfWeek` | Day of the week (Sunday, Monday, etc.) **REQUIRED**                  |
| `SessionIndex` | `uint32`    | Session index (starts from 0). Most symbols have 1-2 sessions/day    |

---

## ⬆️ Output — `SymbolInfoSessionTradeData`

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

* **What it is.** Returns trading session hours for a symbol on a specific day of week. Trading sessions determine when orders can be executed.
* **Why you need it.** Check if trading is allowed at specific times, schedule strategies around trading sessions, avoid placing orders outside market hours.
* **Sanity check.** If `From` < `To` → valid session. Time is in seconds from midnight. Ignore date component, use only time-of-day.

---

## 🎯 Purpose

Use it to understand trading hours:

* Check when symbol can be traded.
* Determine if market is open at current time.
* Schedule trading strategies around market hours.
* Avoid placing orders outside trading sessions.

---

## 🧩 Notes & Tips

* **Trade vs Quote sessions:** Trading sessions show when orders can be executed. Quoting sessions (SymbolInfoSessionQuote) show when prices are updated. Quote sessions are usually wider than trade sessions.
* **Session index:** Starts from 0. Most symbols have 1 session per day. Some (like futures) may have multiple sessions (e.g., day session + evening session).
* **Time format:** Returned timestamps represent time-of-day only. Date component should be ignored.
* **Extract time:** Use `timestamp.ToDateTime().TimeOfDay` to get time as TimeSpan.
* **Weekend trading:** Most Forex symbols don't trade on weekends (Saturday/Sunday). Some crypto symbols trade 24/7.
* **Error handling:** If session index doesn't exist, server may return error or invalid data.
* **Broker-specific:** Trading hours can vary by broker. Some brokers extend/restrict standard market hours.

---

## 🔗 Usage Examples

### 1) Basic trade session retrieval

```csharp
// acc — connected MT5Account

var result = await acc.SymbolInfoSessionTradeAsync(new SymbolInfoSessionTradeRequest
{
    Symbol = "EURUSD",
    DayOfWeek = DayOfWeek.Monday,
    SessionIndex = 0
});

var fromTime = result.From.ToDateTime().TimeOfDay;
var toTime = result.To.ToDateTime().TimeOfDay;

Console.WriteLine($"EURUSD Monday trading session: {fromTime} - {toTime}");
```

---

### 2) Get all trade sessions for a day

```csharp
var symbol = "ES"; // E-mini S&P 500 futures
var day = DayOfWeek.Wednesday;

Console.WriteLine($"{symbol} trading sessions for {day}:\n");

for (uint sessionIndex = 0; sessionIndex < 10; sessionIndex++)
{
    try
    {
        var result = await acc.SymbolInfoSessionTradeAsync(new SymbolInfoSessionTradeRequest
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

### 3) Check if market is open now

```csharp
var symbol = "XAUUSD";
var now = DateTime.UtcNow;
var currentDay = (DayOfWeek)((int)now.DayOfWeek); // Convert to proto DayOfWeek
var currentTime = now.TimeOfDay;

bool marketOpen = false;

// Check all sessions for current day
for (uint sessionIndex = 0; sessionIndex < 5; sessionIndex++)
{
    try
    {
        var result = await acc.SymbolInfoSessionTradeAsync(new SymbolInfoSessionTradeRequest
        {
            Symbol = symbol,
            DayOfWeek = currentDay,
            SessionIndex = sessionIndex
        });

        var fromTime = result.From.ToDateTime().TimeOfDay;
        var toTime = result.To.ToDateTime().TimeOfDay;

        if (currentTime >= fromTime && currentTime <= toTime)
        {
            marketOpen = true;
            Console.WriteLine($"✓ {symbol} market OPEN (session {sessionIndex}: {fromTime} - {toTime})");
            break;
        }
    }
    catch
    {
        break;
    }
}

if (!marketOpen)
{
    Console.WriteLine($"✗ {symbol} market CLOSED at {currentTime}");
}
```

---

### 4) Get trading hours for entire week

```csharp
var symbol = "BTCUSD";

Console.WriteLine($"Weekly trading schedule for {symbol}:\n");

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
            var result = await acc.SymbolInfoSessionTradeAsync(new SymbolInfoSessionTradeRequest
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
        Console.WriteLine("  Market closed");
    }

    Console.WriteLine();
}
```

---

### 5) Time until market opens

```csharp
var symbol = "EURUSD";
var now = DateTime.UtcNow;
var currentDay = (DayOfWeek)((int)now.DayOfWeek);
var currentTime = now.TimeOfDay;

// Try to find next trading session
TimeSpan? timeUntilOpen = null;
string nextSessionDay = "";

for (int dayOffset = 0; dayOffset < 7; dayOffset++)
{
    var checkDay = (DayOfWeek)(((int)currentDay + dayOffset) % 7);

    for (uint sessionIndex = 0; sessionIndex < 5; sessionIndex++)
    {
        try
        {
            var result = await acc.SymbolInfoSessionTradeAsync(new SymbolInfoSessionTradeRequest
            {
                Symbol = symbol,
                DayOfWeek = checkDay,
                SessionIndex = sessionIndex
            });

            var fromTime = result.From.ToDateTime().TimeOfDay;

            // If same day, check if session hasn't started yet
            if (dayOffset == 0 && fromTime > currentTime)
            {
                timeUntilOpen = fromTime - currentTime;
                nextSessionDay = checkDay.ToString();
                break;
            }
            // If different day, use first session of that day
            else if (dayOffset > 0)
            {
                timeUntilOpen = TimeSpan.FromDays(dayOffset) + fromTime - currentTime;
                nextSessionDay = checkDay.ToString();
                break;
            }
        }
        catch
        {
            break;
        }
    }

    if (timeUntilOpen.HasValue)
        break;
}

if (timeUntilOpen.HasValue)
{
    Console.WriteLine($"{symbol} market opens in: {timeUntilOpen.Value.TotalHours:F1} hours ({nextSessionDay})");
}
else
{
    Console.WriteLine($"{symbol} market is currently open or schedule unavailable");
}
```

---

### 6) Compare quote vs trade sessions

```csharp
var symbol = "XAUUSD";
var day = DayOfWeek.Monday;

var quoteSession = await acc.SymbolInfoSessionQuoteAsync(new SymbolInfoSessionQuoteRequest
{
    Symbol = symbol,
    DayOfWeek = day,
    SessionIndex = 0
});

var tradeSession = await acc.SymbolInfoSessionTradeAsync(new SymbolInfoSessionTradeRequest
{
    Symbol = symbol,
    DayOfWeek = day,
    SessionIndex = 0
});

var quoteFrom = quoteSession.From.ToDateTime().TimeOfDay;
var quoteTo = quoteSession.To.ToDateTime().TimeOfDay;
var tradeFrom = tradeSession.From.ToDateTime().TimeOfDay;
var tradeTo = tradeSession.To.ToDateTime().TimeOfDay;

Console.WriteLine($"{symbol} {day} sessions:\n");
Console.WriteLine($"Quote session: {quoteFrom} - {quoteTo} ({(quoteTo - quoteFrom).TotalHours:F1}h)");
Console.WriteLine($"Trade session: {tradeFrom} - {tradeTo} ({(tradeTo - tradeFrom).TotalHours:F1}h)");

if (quoteFrom < tradeFrom || quoteTo > tradeTo)
{
    Console.WriteLine("\n⚠ Quote session is wider than trade session (pre/post-market quotes available)");
}
```
