# Checking Symbol Synchronization Status

> **Request:** symbol data synchronization status (`bool`) from MT5
> Fetch whether a given symbol’s market data is fully synchronized with the server.

---

### Code Example

```csharp
var syncData = await _mt5Account.SymbolIsSynchronizedAsync(Constants.DefaultSymbol);
_logger.LogInformation(
    "SymbolIsSynchronizedAsync: IsSync={IsSync}",
    syncData.Synchronized
);
```

---

### Method Signature

```csharp
Task<SymbolIsSynchronizedData> SymbolIsSynchronizedAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

| Parameter           | Type                | Description                            |
| ------------------- | ------------------- | -------------------------------------- |
| `symbol`            | `string`            | Symbol name (e.g., "EURUSD", "XAUUSD") |
| `deadline`          | `DateTime?`         | Optional timeout deadline              |
| `cancellationToken` | `CancellationToken` | Optional cancel token                  |

---

## ⬆️ Output

Returns a **SymbolIsSynchronizedData** structure:

| Field          | Type   | Description                                                           |
| -------------- | ------ | --------------------------------------------------------------------- |
| `Synchronized` | `bool` | Whether all market data for the symbol is in sync (`true` or `false`) |

---

## 🎯 Purpose

Use this method to confirm that **symbol market data is live and current** before relying on:

* Quotes
* Tick streams
* Session times
* Trade validation logic

Helps ensure **data integrity** when operating in real-time or mission-critical workflows.
