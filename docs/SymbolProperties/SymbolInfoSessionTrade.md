# Retrieving Session Trade Times for a Symbol

> **Request:** session trade start/end times for a symbol on a specific day from MT5

Fetch the market trade session hours (when trading is allowed) for a given symbol and weekday.

### Code Example

```csharp
var sessionTrade = await _mt5Account.SymbolInfoSessionTradeAsync(
    Constants.DefaultSymbol,
    mt5_term_api.DayOfWeek.Monday,
    0);
var startUtc = sessionTrade.From.ToDateTime(); // UTC
var endUtc   = sessionTrade.To.ToDateTime();
_logger.LogInformation(
    "SymbolInfoSessionTrade: StartUtc={StartUtc:O} EndUtc={EndUtc:O}",
    startUtc,
    endUtc);
```

✨**Method Signature:**
```csharp
Task<SymbolInfoSessionTradeData> SymbolInfoSessionTradeAsync(
    string symbol,
    mt5_term_api.DayOfWeek dayOfWeek,
    uint sessionIndex,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

* **Input:**
    * **symbol (string):** the symbol name (e.g., `EURUSD`).
    * **dayOfWeek (DayOfWeek):** the weekday to query (e.g., `DayOfWeek.Monday`).
    * **sessionIndex (uint):** trade session index for that day (usually 0 for the first session).

* **Output:**
    * **SymbolInfoSessionTradeData** with properties:
      * **From** (`Timestamp`) — session start time (UTC).
      * **To** (`Timestamp`) — session end time (UTC).

**Purpose:** Identify active trading windows for a symbol, enabling you to schedule order placement or analysis only during valid market hours. 🚀
