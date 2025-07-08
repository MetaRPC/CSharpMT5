# Retrieving Session Trade Times for a Symbol

> **Request:** session trade start/end times for a symbol on a specific day from MT5
> Fetch the market trade session hours (when trading is allowed) for a given symbol and weekday.

### Code Example

```csharp
var sessionTrade = await _mt5Account.SymbolInfoSessionTradeAsync(
    Constants.DefaultSymbol,
    mt5_term_api.DayOfWeek.Monday,
    0
);
var startUtc = sessionTrade.From.ToDateTime(); // UTC
var endUtc   = sessionTrade.To.ToDateTime();
_logger.LogInformation(
    "SymbolInfoSessionTrade: StartUtc={StartUtc:O} EndUtc={EndUtc:O}",
    startUtc,
    endUtc
);
```

✨ **Method Signature:**

```csharp
Task<SymbolInfoSessionTradeData> SymbolInfoSessionTradeAsync(
    string symbol,
    mt5_term_api.DayOfWeek dayOfWeek,
    uint sessionIndex,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## Input

* **symbol** (`string`) — the symbol name (e.g., `"EURUSD"`).

* **dayOfWeek** (`mt5_term_api.DayOfWeek`) — the weekday to query. Possible values:

  * **Sunday** (`0`)
  * **Monday** (`1`)
  * **Tuesday** (`2`)
  * **Wednesday** (`3`)
  * **Thursday** (`4`)
  * **Friday** (`5`)
  * **Saturday** (`6`)

* **sessionIndex** (`uint`) — session index for that day (0-based, usually `0` for the first session).

* **deadline** (`DateTime?`) — optional UTC deadline for the request.

* **cancellationToken** (`CancellationToken`) — optional token to cancel the request.

---

## Output

**`SymbolInfoSessionTradeData`** — structure with the following fields:

* **From** (`Timestamp`) — session start time in UTC.
* **To** (`Timestamp`) — session end time in UTC.

---

## Purpose

Identifies active trading windows for a symbol on a given day, enabling scheduling of order placement or data analysis only during valid market hours. 🚀
