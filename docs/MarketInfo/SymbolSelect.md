# Selecting or Deselecting a Symbol

> **Request:** add or remove a symbol from Market Watch

Enable or disable a symbol in the Market Watch list.

### Code Example

```csharp
// Add the symbol to Market Watch
var select = await _mt5Account.SymbolSelectAsync(Constants.DefaultSymbol, true);
_logger.LogInformation(
    "SymbolSelectAsync: Selected={Selected}",
    select.Success);

// Remove the symbol from Market Watch
var deselect = await _mt5Account.SymbolSelectAsync(Constants.DefaultSymbol, false);
_logger.LogInformation(
    "SymbolSelectAsync: Selected={Selected}",
    deselect.Success);
```

✨**Method Signature:**
```csharp
Task<SymbolSelectData> SymbolSelectAsync(
    string symbol,
    bool select,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

 **Input:** 
* **symbol** (`string`): the symbol name to add or remove (e.g., "EURUSD").
* **select** (`bool`):
 * `true` — add the symbol to Market Watch.
 * `false` — remove the symbol from Market Watch.

 **Output:**
* **SymbolSelectData** with property:
 * **Success** (`bool`) — `true` if the operation succeeded; `false` otherwise.

**Purpose:**  Dynamically manage your Market Watch list from code with a single, clear method call. 🚀

