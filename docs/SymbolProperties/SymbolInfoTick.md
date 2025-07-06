# Retrieving Live Tick Data for a Symbol

> **Request:** current bid and ask prices for a symbol from MT5

Fetch the most recent tick (market snapshot) data for a given symbol.

### Code Example

```csharp
var tick = await _mt5Account.SymbolInfoTickAsync(Constants.DefaultSymbol);
_logger.LogInformation(
    "SymbolInfoTickAsync: Bid={Bid} Ask={Ask}",
    tick.Bid,
    tick.Ask);
```

✨**Method Signature:**
```csharp
Task<MrpcMqlTick> SymbolInfoTickAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

* **Input:**
    * **symbol (string):** the symbol name, e.g. `EURUSD`, `XAUUSD`.

* **Output:**
    * **MrpcMqlTick** with properties:
      * `Bid (double)` — the current best bid price.
      * `Ask (double)` — the current best ask price.
      * (also contains `Last`, `Volume`, etc., if needed)

**Purpose:** Instantly obtain up-to-the-millisecond bid/ask quotes for real-time decision-making in your trading algorithms. 🚀
