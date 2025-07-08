# Checking Symbol Existence

> **Request:** symbol existence (`bool`) from MT5
> Verify whether a given symbol is available on the server (standard or custom).

### Code Example

```csharp
var existsData = await _mt5Account.SymbolExistAsync(Constants.DefaultSymbol);
_logger.LogInformation(
    "SymbolExistAsync: Exists={Exists}",
    existsData.Exists
);
```

✨ **Method Signature:**

```csharp
Task<SymbolExistData> SymbolExistAsync(
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

**`SymbolExistData`** — structure with the following field:

* **`Exists`** (`bool`) — `true` if the symbol exists on the server; `false` otherwise.

---

## Purpose

Allows you to quickly determine if a symbol is supported by the broker/server without loading full symbol lists, enabling dynamic symbol validation in your application. 🚀
