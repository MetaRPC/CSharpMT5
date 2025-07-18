# Retrieving a String‑Type Symbol Property

> **Request:** a specific string property (CurrencyBase, CurrencyProfit, Description, etc.) for a symbol from MT5
> Fetch any text‑based market data value for a given symbol using one universal method.

---

### Code Example

```csharp
var stringProp = await _mt5Account.SymbolInfoStringAsync(
    Constants.DefaultSymbol,
    SymbolInfoStringProperty.SymbolCurrencyBase
);
_logger.LogInformation(
    "SymbolInfoString: BaseCurrency={BaseCurrency}",
    stringProp.Value
);
```

---

### Method Signature

```csharp
Task<SymbolInfoStringData> SymbolInfoStringAsync(
    string symbol,
    SymbolInfoStringProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

| Parameter  | Type                       | Description                               |
| ---------- | -------------------------- | ----------------------------------------- |
| `symbol`   | `string`                   | Symbol name (e.g., "EURUSD", "XAUUSD")    |
| `property` | `SymbolInfoStringProperty` | Enum specifying which string field to get |

### `SymbolInfoStringProperty` Enum Values

| Enum Value             | MQL5 Const               | Description                                                   |
| ---------------------- | ------------------------ | ------------------------------------------------------------- |
| `SymbolCurrencyBase`   | `SYMBOL_CURRENCY_BASE`   | Base currency of the symbol (e.g., "EUR")                     |
| `SymbolCurrencyProfit` | `SYMBOL_CURRENCY_PROFIT` | Currency for profit calculation (e.g., "USD")                 |
| `SymbolCurrencyMargin` | `SYMBOL_CURRENCY_MARGIN` | Currency for margin calculation (e.g., "USD")                 |
| `SymbolCurrencySwap`   | `SYMBOL_CURRENCY_SWAP`   | Currency used for swap calculations (e.g., "USD")             |
| `SymbolDescription`    | `SYMBOL_DESCRIPTION`     | Human-readable symbol description (e.g., "Euro vs US Dollar") |
| `SymbolPath`           | `SYMBOL_PATH`            | Full symbol path in Market Watch (e.g., "Forex\EURUSD")       |
| `SymbolExchange`       | `SYMBOL_EXCHANGE`        | Exchange or platform name (e.g., "NYSE")                      |

---

## ⬆️ Output

Returns a **SymbolInfoStringData** object:

| Field   | Type     | Description                          |
| ------- | -------- | ------------------------------------ |
| `Value` | `string` | Text value of the requested property |

---

## 🎯 Purpose

Use this method to retrieve any **string-type descriptive attribute** of a trading symbol.

Great for:

* Displaying market metadata
* Logging and audit information
* Platform-specific routing and symbol grouping
