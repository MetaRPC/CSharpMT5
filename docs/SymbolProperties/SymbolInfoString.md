# Retrieving a String‑Type Symbol Property

> **Request:** a specific string property (CurrencyBase, CurrencyProfit, Description, etc.) for a symbol from MT5
> Fetch any text‑based market data value for a given symbol using one universal method.

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

✨ **Method Signature:**

```csharp
Task<SymbolInfoStringData> SymbolInfoStringAsync(
    string symbol,
    SymbolInfoStringProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## Input

* **symbol** (`string`) — the symbol name (e.g., `"EURUSD"`, `"XAUUSD"`).
* **property** (`SymbolInfoStringProperty`) — which string property to fetch. Possible values:

  * **SymbolCurrencyBase** (`SYMBOL_CURRENCY_BASE`) — base currency of the symbol (e.g., "EUR").
  * **SymbolCurrencyProfit** (`SYMBOL_CURRENCY_PROFIT`) — currency used to calculate profit (e.g., "USD").
  * **SymbolCurrencyMargin** (`SYMBOL_CURRENCY_MARGIN`) — currency used to calculate margin (e.g., "USD").
  * **SymbolCurrencySwap** (`SYMBOL_CURRENCY_SWAP`) — currency used to calculate swap (e.g., "USD").
  * **SymbolDescription** (`SYMBOL_DESCRIPTION`) — human‑readable description of the symbol (e.g., "Euro vs US Dollar").
  * **SymbolPath** (`SYMBOL_PATH`) — full path of the symbol in Market Watch tree (e.g., "Forex\EURUSD").
  * **SymbolExchange** (`SYMBOL_EXCHANGE`) — name of the exchange or trading platform (e.g., "NYSE").

---

## Output

**`SymbolInfoStringData`** — structure with the following field:

* **Value** (`string`) — the requested string property value.

---

## Purpose

Consolidates all string‑type symbol queries into a single method call; just swap the enum to retrieve any descriptive field. 🚀
