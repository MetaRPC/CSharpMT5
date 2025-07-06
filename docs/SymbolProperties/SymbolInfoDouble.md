# Retrieving a Double-Precision Symbol Property

> **Request:** a specific double property (Bid, Ask, Last, etc.) for a symbol from MT5

Fetch any floating-point market data value for a given symbol using one universal method.

### Code Example

```csharp
var doubleProp = await _mt5Account.SymbolInfoDoubleAsync(
    Constants.DefaultSymbol,
    SymbolInfoDoubleProperty.SymbolAsk);
_logger.LogInformation(
    "SymbolInfoDouble: Ask={Ask}",
    doubleProp.Value);
```

✨**Method Signature:**
```csharp
Task<SymbolInfoDoubleData> SymbolInfoDoubleAsync(
    string symbol,
    SymbolInfoDoubleProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

 **Input:**
* **symbol** (`string`): the symbol name, e.g. `EURUSD`, `XAUUSD`.
* **property** (`SymbolInfoDoubleProperty`): which double value to fetch.
* **Examples:** `SymbolBid`, `SymbolAsk`, `SymbolLast`, `SymbolBidhigh`, `SymbolBidlow`, etc.

 **Output:**
* **SymbolInfoDoubleData** with property:
* **Value** (`double`) — the requested numeric property (e.g., ask price = 1.2345).

**Purpose:** Keep your code DRY by using a single endpoint for all double-type symbol properties; just swap the enum and you’re set! 🚀
