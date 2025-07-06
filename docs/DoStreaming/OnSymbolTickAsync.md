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
* **Input:**
    * **symbols** (`IEnumerable<string>`) — a list of character names.
    * **CancellationToken** (`CancellationToken`) — cancellation token.

* **Output:**
    * Stream of **OnSymbolTickData** objects with fields:
      * **SymbolTick.Symbol** (`string`) — symbol name.
      * **SymbolTick.Bid/Ask** (`double`) — prices.

The purpose is to connect to the live tick channel and receive updates on several symbols at once.🚀
