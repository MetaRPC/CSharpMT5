# Checking Symbol Existence

> **Request:** symbol existence (`bool`) from MT5
> Verify whether a given symbol is available on the server (standard or custom).

---

### Code Example

```csharp
var existsData = await _mt5Account.SymbolExistAsync(Constants.DefaultSymbol);
_logger.LogInformation(
    "SymbolExistAsync: Exists={Exists}",
    existsData.Exists
);
```

---

### Method Signature

```csharp
Task<SymbolExistData> SymbolExistAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

| Parameter           | Type                | Description                            |
| ------------------- | ------------------- | -------------------------------------- |
| `symbol`            | `string`            | Symbol name to verify (e.g., "EURUSD") |
| `deadline`          | `DateTime?`         | Optional timeout deadline              |
| `cancellationToken` | `CancellationToken` | Optional cancel token                  |

---

## ⬆️ Output

Returns a **SymbolExistData** structure:

| Field    | Type   | Description                                  |
| -------- | ------ | -------------------------------------------- |
| `Exists` | `bool` | Whether the symbol exists (`true` / `false`) |

---

## 🎯 Purpose

Use this method to verify if a **symbol is recognized by the broker/server** before attempting to place orders or load data.

Great for:

* Dynamic symbol validation in forms or UIs
* Filtering user inputs before submission
* Fast pre-check without listing all symbols
