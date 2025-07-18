# Retrieving Session Trade Times for a Symbol

> **Request:** session trade start/end times for a symbol on a specific day from MT5
> Fetch the market trade session hours (when trading is allowed) for a given symbol and weekday.

---

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

---

### Method Signature

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

## 🔽 Input

| Parameter           | Type                     | Description                                |
| ------------------- | ------------------------ | ------------------------------------------ |
| `symbol`            | `string`                 | Symbol name (e.g., "EURUSD")               |
| `dayOfWeek`         | `mt5_term_api.DayOfWeek` | Day of the week to query (Sunday–Saturday) |
| `sessionIndex`      | `uint`                   | Index of the trade session (0-based)       |
| `deadline`          | `DateTime?`              | Optional timeout deadline                  |
| `cancellationToken` | `CancellationToken`      | Optional cancellation token                |

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

Returns a **SymbolInfoSessionTradeData** object:

| Field  | Type        | Description                  |
| ------ | ----------- | ---------------------------- |
| `From` | `Timestamp` | UTC time when trading starts |
| `To`   | `Timestamp` | UTC time when trading ends   |

---

## 🎯 Purpose

Use this method to determine **when trading is allowed** for a given symbol on any given weekday.

Perfect for:

* Automated scheduling of trade activity
* Avoiding order placement during restricted times
* Building calendar-aware trading systems and UIs
