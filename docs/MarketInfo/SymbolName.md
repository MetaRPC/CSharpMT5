# Retrieving Symbol Name by Index

> **Request:** symbol name at a given index from MT5
> Fetch the name of a symbol by its index in the Market Watch or the full symbol list.

---

### Code Example

```csharp
var symbolNameData = await _mt5Account.SymbolNameAsync(
    0,      // index of the first symbol
    false   // include all symbols
);
_logger.LogInformation(
    "SymbolNameAsync: FirstSymbol={FirstSymbol}",
    symbolNameData.Name
);
```

---

### Method Signature

```csharp
Task<SymbolNameData> SymbolNameAsync(
    int index,
    bool selectedOnly,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

| Parameter           | Type                | Description                                                               |
| ------------------- | ------------------- | ------------------------------------------------------------------------- |
| `index`             | `int`               | Zero-based index of the symbol in the list                                |
| `selectedOnly`      | `bool`              | If `true`, search only among Market Watch symbols; if `false`, search all |
| `deadline`          | `DateTime?`         | Optional deadline for request execution                                   |
| `cancellationToken` | `CancellationToken` | Optional cancellation token                                               |

---

## ⬆️ Output

Returns a **SymbolNameData** object:

| Field  | Type     | Description                           |
| ------ | -------- | ------------------------------------- |
| `Name` | `string` | Name of the symbol at the given index |

---

## 🎯 Purpose

Use this method to retrieve **symbol names dynamically by index**, enabling:

* Pagination through symbol lists
* Building custom navigation UIs
* Listing or filtering available trading instruments

Especially useful for platforms that display symbols progressively or allow user-customized symbol panels.
