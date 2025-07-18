# Retrieving Live Tick Data for a Symbol

> **Request:** current bid and ask prices for a symbol from MT5
> Fetch the most recent tick (market snapshot) data for a given symbol.

---

### Code Example

```csharp
var symbol = Constants.DefaultSymbol;
var tick   = await _mt5Account.SymbolInfoTickAsync(symbol);
_logger.LogInformation(
    "SymbolInfoTickAsync: Bid={Bid} Ask={Ask}",
    tick.Bid,
    tick.Ask
);
```

---

### Method Signature

```csharp
Task<MrpcMqlTick> SymbolInfoTickAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

| Parameter           | Type                | Description                            |
| ------------------- | ------------------- | -------------------------------------- |
| `symbol`            | `string`            | Symbol name (e.g., "EURUSD", "XAUUSD") |
| `deadline`          | `DateTime?`         | Optional timeout deadline              |
| `cancellationToken` | `CancellationToken` | Optional cancel token                  |

---

## ⬆️ Output

Returns a **MrpcMqlTick** structure:

| Field    | Type       | Description                       |
| -------- | ---------- | --------------------------------- |
| `Bid`    | `double`   | Current best bid price            |
| `Ask`    | `double`   | Current best ask price            |
| `Last`   | `double`   | Last executed trade price         |
| `Volume` | `long`     | Tick volume at this point in time |
| `Time`   | `DateTime` | UTC timestamp of the tick         |

---

## 🎯 Purpose

Use this method to get the **latest tick data** — including bid, ask, and last prices — for **real-time pricing** and **decision-making** in trading strategies.

Ideal for:

* Up-to-the-millisecond execution logic
* Real-time dashboards and trade UIs
* Price validation before sending orders
