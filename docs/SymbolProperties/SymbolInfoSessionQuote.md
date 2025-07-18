# Retrieving Session Quote Times for a Symbol

> **Request:** session quote start/end times for a symbol on a specific day from MT5
> Fetch the market quote session hours (when the symbol’s prices are updated) for a given symbol and weekday.

---

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

---

### Method Signature

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

## 🔽 Input

| Parameter      | Type                     | Description                                |
| -------------- | ------------------------ | ------------------------------------------ |
| `symbol`       | `string`                 | Symbol name (e.g., "EURUSD")               |
| `dayOfWeek`    | `mt5_term_api.DayOfWeek` | Day of the week to query (Sunday–Saturday) |
| `sessionIndex` | `uint`                   | Index of the quote session (0-based)       |

### `DayOfWeek` Enum

| Value | Name      |
| ----- | --------- |
| `0`   | Sunday    |
| `1`   | Monday    |
| `2`   | Tuesday   |
| `3`   | Wednesday |
| `4`   | Thursday  |
| `5`   | Friday    |
| `6`   | Saturday  |

---

## ⬆️ Output

Returns a **SymbolInfoSessionQuoteData** object:

| Field  | Type        | Description                      |
| ------ | ----------- | -------------------------------- |
| `From` | `Timestamp` | UTC timestamp when session opens |
| `To`   | `Timestamp` | UTC timestamp when session ends  |

---

## 🎯 Purpose

Use this method to find out **when quotes are available** for a given symbol on a specific weekday.

Perfect for:

* Time-based trading schedules
* Preventing trade operations outside of quote sessions
* Informing UI timers and dashboards with session hours
