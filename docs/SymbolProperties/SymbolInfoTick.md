# Retrieving Live Tick Data for a Symbol

> **Request:** current bid and ask prices for a symbol from MT5
> Fetch the most recent tick (market snapshot) data for a given symbol.

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

✨ **Method Signature:**

```csharp
Task<MrpcMqlTick> SymbolInfoTickAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## Input

* **symbol** (`string`) — the symbol name to query (e.g., `"EURUSD"`, `"XAUUSD"`).

---

## Output

**`MrpcMqlTick`** — structure with the following properties:

* **`Bid`** (`double`) — the current best bid price.
* **`Ask`** (`double`) — the current best ask price.
* **`Last`** (`double`) — the price of the last executed trade.
* **`Volume`** (`long`) — tick volume (number of trades at this tick).
* **`Time`** (`DateTime`) — UTC timestamp when the tick was recorded.

---

## Purpose

Instants fetching of up-to-the-millisecond bid/ask quotes (and full tick details) for real-time decision-making in trading algorithms. 🚀
