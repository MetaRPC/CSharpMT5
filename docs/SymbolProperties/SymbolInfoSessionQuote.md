# Retrieving Session Quote Times for a Symbol

> **Request:** session quote start/end times for a symbol on a specific day from MT5

Fetch the market quote session hours (when the symbol’s prices are updated) for a given symbol and weekday.

### Code Example

```csharp
var sessionQuote = await _mt5Account.SymbolInfoSessionQuoteAsync(
    Constants.DefaultSymbol,
    mt5_term_api.DayOfWeek.Monday,
    0);
var fromUtc = sessionQuote.From.ToDateTime(); // UTC
var toUtc   = sessionQuote.To.ToDateTime();
_logger.LogInformation(
    "SymbolInfoSessionQuote: FromUtc={FromUtc:O} ToUtc={ToUtc:O}",
    fromUtc,
    toUtc);
```

✨**Method Signature:**
```csharp
Task<SymbolInfoSessionQuoteData> SymbolInfoSessionQuoteAsync(
    string symbol,
    mt5_term_api.DayOfWeek dayOfWeek,
    uint sessionIndex,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

* **Input:**
   * **symbol (string):** the symbol name (e.g. `EURUSD`).
   * **dayOfWeek (DayOfWeek):** the weekday to query (e.g. `DayOfWeek.Monday`).
   * **sessionIndex (uint):** session index for that day (usually 0 for the first session).

* **Output:**
   * **SymbolInfoSessionQuoteData** with properties:
     * **From** (`Timestamp`) — session start time (UTC).
     * **To** (`Timestamp`) — session end time (UTC).

**Purpose:** Determine when a symbol’s market quotes are active on a given day, so you can schedule trades or data pulls within live quote windows. 🚀



    
