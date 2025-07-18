## Market Info

### Getting the Total Number of Symbols

> **Request:** fetch the count of symbols (either all available or only those selected in Market Watch) from MT5.

---

### Code Example

```csharp
// Total number of all symbols
var total = await _mt5Account.SymbolsTotalAsync(false);

// Total number of symbols selected in Market Watch
var selectedTotal = await _mt5Account.SymbolsTotalAsync(true);
```

---

### Method Signature

```csharp
Task<SymbolsTotalData> SymbolsTotalAsync(
    bool selectedOnly,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
);
```

---

## 🔽 Input

| Parameter           | Type                | Description                                                                 |
| ------------------- | ------------------- | --------------------------------------------------------------------------- |
| `selectedOnly`      | `bool`              | If `true`, return only Market Watch symbols; if `false`, return all symbols |
| `deadline`          | `DateTime?`         | Optional UTC deadline to complete the request                               |
| `cancellationToken` | `CancellationToken` | Optional cancellation token                                                 |

---

## ⬆️ Output

Returns a **SymbolsTotalData** object:

| Field   | Type  | Description                                           |
| ------- | ----- | ----------------------------------------------------- |
| `Total` | `int` | Number of symbols based on the selectedOnly condition |

---

## 🎯 Purpose

This method provides a **quick count of available or watched trading symbols** with a simple flag — great for:

* Dynamic UI updates
* Pagination logic
* Building symbol selectors or dashboards

It streamlines symbol counting and avoids redundant filtering logic in client apps.
