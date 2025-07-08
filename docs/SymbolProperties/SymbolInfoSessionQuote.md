# Retrieving Session Quote Times for a Symbol

> **Request:** session quote start/end times for a symbol on a specific day from MT5
> Fetch the market quote session hours (when the symbol’s prices are updated) for a given symbol and weekday.

### Code Example

```csharp
var sessionQuote = await _mt5Account.SymbolInfoSessionQuoteAsync(
    Constants.DefaultSymbol,
    mt5_term_api.DayOfWeek.Monday,
    0
);
var fromUtc = sessionQuote.From.ToDateTime(); // UTC
var toUtc   = sessionQuote.To.ToDateTime();
_logger.LogInformation(
    "SymbolInfoSessionQuote: FromUtc={FromUtc:O} ToUtc={ToUtc:O}",
    fromUtc,
    toUtc
);
```

✨ **Method Signature:**

```csharp
Task<SymbolInfoSessionQuoteData> SymbolInfoSessionQuoteAsync(
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

  * **Sunday** (`0`) — Sunday session
  * **Monday** (`1`) — Monday session
  * **Tuesday** (`2`) — Tuesday session
  * **Wednesday** (`3`) — Wednesday session
  * **Thursday** (`4`) — Thursday session
  * **Friday** (`5`) — Friday session
  * **Saturday** (`6`) — Saturday session

* **sessionIndex** (`uint`) — session index for that day (0-based, usually `0` for the first session).

---

## Output

**`SymbolInfoSessionQuoteData`** — structure with the following fields:

* **From** (`Timestamp`) — session start time in UTC.
* **To** (`Timestamp`) — session end time in UTC.

---

## Purpose

Determines when a symbol’s market quotes are active on a given day, enabling scheduling of trades or data retrieval within live quote sessions. 🚀
