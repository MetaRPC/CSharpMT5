# Checking Symbol Synchronization Status

> **Request:** symbol data synchronization status (bool) from MT5

Fetch whether a given symbol’s market data is fully synchronized with the server.

### Code Example

```csharp
var sync = await _mt5Account.SymbolIsSynchronizedAsync(Constants.DefaultSymbol);
_logger.LogInformation(
    "SymbolIsSynchronizedAsync: IsSync={IsSync}",
    sync.Synchronized);
```

✨**Method Signature:**
```csharp
Task<SymbolIsSynchronizedData> SymbolIsSynchronizedAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

 **Input:**
* **symbol** (`string`): the symbol name to check, e.g. `EURUSD`, `XAUUSD`.

 **Output:**
* **SymbolIsSynchronizedData** with property:
 * **Synchronized** (`bool`) — `true` if the symbol’s data is up-to-date and in sync; `false` otherwise.

**Purpose:** Quickly verify that real-time quotes and other symbol data are fully synchronized before placing trades or analyzing market data. 🚀
