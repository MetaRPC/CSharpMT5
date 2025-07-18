# Selecting or Deselecting a Symbol

> **Request:** add or remove a symbol from Market Watch
> Enable or disable a symbol in the Market Watch list.

---

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

---

### Method Signature

```csharp
Task<SymbolSelectData> SymbolSelectAsync(
    string symbol,
    bool select,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

| Parameter           | Type                | Description                                      |
| ------------------- | ------------------- | ------------------------------------------------ |
| `symbol`            | `string`            | Name of the symbol (e.g., "EURUSD")              |
| `select`            | `bool`              | `true` to add to Market Watch, `false` to remove |
| `deadline`          | `DateTime?`         | Optional UTC deadline for timeout                |
| `cancellationToken` | `CancellationToken` | Optional token to cancel the request             |

---

## ⬆️ Output

Returns a **SymbolSelectData** object:

| Field     | Type   | Description                                      |
| --------- | ------ | ------------------------------------------------ |
| `Success` | `bool` | Whether the selection/deselection was successful |

---

## 🎯 Purpose

Use this method to programmatically **manage the list of visible symbols in Market Watch**:

* Add new trading instruments dynamically
* Clean up or minimize the list for performance
* Control what symbols are actively tracked
