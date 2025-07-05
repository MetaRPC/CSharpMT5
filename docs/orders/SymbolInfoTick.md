## Getting current tick for a symbol

> **Requesting** the latest tick information for a given symbol from MT5.

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

✨**Method Signature:** 
```csharp
Task<MrpcMqlTick> SymbolInfoTickAsync(string symbol);
```

* **symbol (string)** — the symbol to retrieve tick data for (e.g., Constants.DefaultSymbol).

* **Output:**
     **MrpcMqlTick — object with properties:**

   * **Bid (double)** — current bid price.

   * **Ask (double)** — current ask price.

   * **Last (double)** — last trade price.

   * **Volume (long)** — tick volume.

   * **Time (DateTime)** — UTC timestamp of the tick.

**Purpose:**
Allows you to fetch the most recent market tick for a specified symbol in one call, enabling real-time pricing, analytics, or triggering trading logic. 🚀
