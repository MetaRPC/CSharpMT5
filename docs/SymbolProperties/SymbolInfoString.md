# Retrieving a String‐Type Symbol Property

> **Request:** a specific string property (CurrencyBase, CurrencyProfit, Description, etc.) for a symbol from MT5

Fetch any text‐based market data value for a given symbol using one universal method.

### Code Example

```csharp
var stringProp = await _mt5Account.SymbolInfoStringAsync(
    Constants.DefaultSymbol,
    SymbolInfoStringProperty.SymbolCurrencyBase);
_logger.LogInformation(
    "SymbolInfoString: BaseCurrency={BaseCurrency}",
    stringProp.Value);
```

✨**Method Signature:**
```csharp
Task<SymbolInfoStringData> SymbolInfoStringAsync(
    string symbol,
    SymbolInfoStringProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

* **Input:**
    * **symbol (string):** the symbol name, e.g. `EURUSD`, `XAUUSD`.
    * **property (SymbolInfoStringProperty):** which string value to fetch.
      * **Examples:**
        * `SymbolCurrencyBase` — base currency (e.g., "EUR").
        * `SymbolCurrencyProfit` — profit currency (e.g., "USD").
        * `SymbolDescription` — human‐readable description.
        * `SymbolExchange` — exchange name, etc.

* **Output:**
    * **SymbolInfoStringData** with property:
   *  **Value (string)** — the requested text property.

**Purpose:** – Consolidate all string‐type symbol queries into a single method call; just swap the enum to get any descriptive field. 🚀



