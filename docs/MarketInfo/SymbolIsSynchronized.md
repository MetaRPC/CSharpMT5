# Checking Symbol Synchronization Status

> **Request:** symbol data synchronization status (`bool`) from MT5
> Fetch whether a given symbol’s market data is fully synchronized with the server.

### Code Example

```csharp
var syncData = await _mt5Account.SymbolIsSynchronizedAsync(Constants.DefaultSymbol);
_logger.LogInformation(
    "SymbolIsSynchronizedAsync: IsSync={IsSync}",
    syncData.Synchronized
);
```

✨ **Method Signature:**

```csharp
Task<SymbolIsSynchronizedData> SymbolIsSynchronizedAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## Input

* **symbol** (`string`) — the name of the symbol to check (e.g., `"EURUSD"`, `"XAUUSD"`).

Optional parameters:

* **deadline** (`DateTime?`) — optional UTC deadline for the request.
* **cancellationToken** (`CancellationToken`) — optional token to cancel the request.

---

## Output

**`SymbolIsSynchronizedData`** — structure with the following field:

* **`Synchronized`** (`bool`) — `true` if the symbol’s data is fully synchronized (quotes, ticks, session data) with the server; `false` otherwise.

---

## Purpose

Quickly verify that real-time quotes and other symbol data are in sync before placing trades or performing analysis, ensuring your application uses up-to-date market information. 🚀
