# Getting Current Tick for a Symbol

> **Request:** retrieve the latest tick information for a given symbol from MT5
> Fetch market data for a single symbol at the most recent tick.

---

### Code Example

```csharp
var symbol = Constants.DefaultSymbol;
var tick   = await _mt5Account.SymbolInfoTickAsync(symbol);

_logger.LogInformation(
    "SymbolInfoTickAsync: Symbol={Symbol}, Bid={Bid}, Ask={Ask}, Last={Last}, Volume={Volume}, Time={Time}",
    symbol,
    tick.Bid,
    tick.Ask,
    tick.Last,
    tick.Volume,
    tick.Time
);
```

---

### Method Signature

```csharp
Task<MrpcMqlTick> SymbolInfoTickAsync(string symbol)
```

---

## 🔽 Input

| Parameter | Type     | Description                                       |
| --------- | -------- | ------------------------------------------------- |
| `symbol`  | `string` | Symbol to retrieve tick data for (e.g., "EURUSD") |

---

## ⬆️ Output

Returns a **MrpcMqlTick** object:

| Field    | Type       | Description                                |
| -------- | ---------- | ------------------------------------------ |
| `Bid`    | `double`   | Current bid price                          |
| `Ask`    | `double`   | Current ask price                          |
| `Last`   | `double`   | Last traded price (if available)           |
| `Volume` | `long`     | Tick volume (number of deals in this tick) |
| `Time`   | `DateTime` | UTC timestamp of the latest tick           |

---

## 🎯 Purpose

Use this method to retrieve **real-time market data** for a given symbol, typically for:

* Updating UI pricing components
* Triggering trading logic
* Performing real-time analysis or alerts

It gives the latest snapshot of bid, ask, last price, and tick volume.
