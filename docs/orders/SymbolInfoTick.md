# Getting Current Tick for a Symbol

> **Request:** retrieve the latest tick information for a given symbol from MT5.

Fetch market data for a single symbol at the most recent tick.

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

✨ **Method Signature:**

```csharp
Task<MrpcMqlTick> SymbolInfoTickAsync(string symbol)
```

---

## Input

* **`symbol`** (`string`) — the symbol to retrieve tick data for (e.g., `Constants.DefaultSymbol`).

---

## Output

**`MrpcMqlTick`** — structure with the following properties:

* **`Bid`** (`double`) — the current bid price.
* **`Ask`** (`double`) — the current ask price.
* **`Last`** (`double`) — the price of the last executed trade (if available).
* **`Volume`** (`long`) — the tick volume (number of trades at this tick).
* **`Time`** (`DateTime`) — the UTC timestamp when this tick was recorded.

---

## Purpose

Allows you to fetch the most recent market tick for a specified symbol in one call, enabling real-time pricing, analytics, or triggering trading logic. 🚀
