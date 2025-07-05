# Checking Symbol Existence

> **Request:** symbol existence (bool) from MT5

Verify whether a given symbol is available on the server (standard or custom).

### Code Example

```csharp
var exists = await _mt5Account.SymbolExistAsync(Constants.DefaultSymbol);
_logger.LogInformation(
    "SymbolExistAsync: Exists={Exists}",
    exists.Exists);
```

✨**Method Signature:**
```csharp
Task<SymbolExistData> SymbolExistAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```
* **Input:** 
    * **symbol (string):** the symbol name to check, for example `EURUSD` or `XAUUSD`.
* **Output:**
    * **SymbolExistData with property:**
      * **Exists (bool)** — `true` if the symbol exists on the server; `false` otherwise.

**Purpose:** Instantly know if a symbol is supported without scanning full symbol lists; just one boolean call. 🚀
