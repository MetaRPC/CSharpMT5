# Retrieving an Integer-Precision Symbol Property

> **Request:** a specific integer property (Visible, Digits, Spread, etc.) for a symbol from MT5

Fetch any integer‐type market data value for a given symbol using one universal method.

### Code Example

```csharp
var intProp = await _mt5Account.SymbolInfoIntegerAsync(
    Constants.DefaultSymbol,
    SymbolInfoIntegerProperty.SymbolVisible);
_logger.LogInformation(
    "SymbolInfoInteger: Visible={Visible}",
    intProp.Value);
```

✨**Method Signature:**
```csharp
Task<SymbolInfoIntegerData> SymbolInfoIntegerAsync(
    string symbol,
    SymbolInfoIntegerProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```
* **Input:**
    * **symbol (string):** the symbol name, e.g. `EURUSD`, `XAUUSD`.
    * **property (SymbolInfoIntegerProperty)**: which integer value to fetch.
      * Examples: `SymbolVisible`, `SymbolDigits`, `SymbolSpread`, `SymbolVolumeDecimals`, etc.

* **Output:**
    * **SymbolInfoIntegerData** with property:
      * `Value (int)` — the requested numeric property (e.g., visibility flag = 1).

**Purpose:** Use a single, consistent endpoint for all integer‐type symbol properties; simply swap the enum to retrieve any integer metric. 🚀
