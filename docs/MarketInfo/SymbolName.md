# Retrieving Symbol Name by Index

> **Request:** symbol name at a given index from MT5

Fetch the name of a symbol by its index in the Market Watch or full list.

### Code Example

```csharp
var symbolName = await _mt5Account.SymbolNameAsync(0, false);
_logger.LogInformation(
    "SymbolNameAsync: FirstSymbol={FirstSymbol}",
    symbolName.Name);
```

✨**Method Signature:**
```csharp
Task<SymbolNameData> SymbolNameAsync(
    int index,
    bool selectedOnly,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

 **Input:**
* **index** (`int`): zero-based position of the symbol in the list.
* **selectedOnly** (`bool`):
  * `false` — search within all symbols.
  * `true` — search only within Market Watch symbols.

 **Output:**
* **SymbolNameData with property:**
 * **Name** (`string`) — the symbol name at the specified index.

**Purpose:** Quickly retrieve a symbol’s name based on its position, enabling dynamic list navigation or pagination. 🚀

