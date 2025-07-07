# Streaming: OnSymbolTickAsync

> **Request:** real-time tick updates for one or more symbols

### Code Example

```csharp
await foreach (var tick in _mt5Account.OnSymbolTickAsync(
    new[] { Constants.DefaultSymbol }, 
    cts.Token))
{
    _logger.LogInformation(
        "OnSymbolTickAsync: Symbol={Symbol} Ask={Ask}",
        tick.SymbolTick.Symbol,
        tick.SymbolTick.Ask);
}
```

✨**Method Signature:**

```csharp
IAsyncEnumerable<OnSymbolTickData> OnSymbolTickAsync(
    IEnumerable<string> symbols,
    CancellationToken cancellationToken = default
)
```

---

## Input

* **`symbols`** (`IEnumerable<string>`) — list of symbol names (e.g., `"EURUSD"`, `"XAUUSD"`).
* **`cancellationToken`** (`CancellationToken`) — token to cancel the streaming subscription.

## Output

**`OnSymbolTickData`** — structure containing:

* **`SymbolTick`** (`MrpcMqlTick`) — the tick data for each symbol, with fields:

  * **`Symbol`** (`string`) — the symbol name.
  * **`Bid`** (`double`) — current best bid price.
  * **`Ask`** (`double`) — current best ask price.
  * **`Last`** (`double`) — last trade price.
  * **`Volume`** (`long`) — tick volume.
  * **`Time`** (`DateTime`) — UTC timestamp of the tick.

## Purpose

Allows you to subscribe to live market tick updates for multiple symbols simultaneously, enabling real-time price monitoring, analytics, or algorithmic trading triggers. 🚀
