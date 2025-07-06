# Retrieving Margin Rates for a Symbol

> **Request:** initial and maintenance margin rates for a symbol and order type from MT5

Fetch the margin requirements (initial & maintenance) for a given symbol/order direction.

### Code Example

```csharp
var marginRate = await _mt5Account.SymbolInfoMarginRateAsync(
    Constants.DefaultSymbol,
    ENUM_ORDER_TYPE.OrderTypeBuy);
_logger.LogInformation(
    "SymbolInfoMarginRate: InitialMarginRate={InitialMarginRate}",
    marginRate.InitialMarginRate);
```

✨**Method Signature:**
```csharp
Task<SymbolInfoMarginRateData> SymbolInfoMarginRateAsync(
    string symbol,
    ENUM_ORDER_TYPE orderType,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

 **Input:**
 * **symbol** (`string`): the symbol name, e.g. `EURUSD`.
 * **orderType** (`ENUM_ORDER_TYPE`): buy or sell direction, e.g. `OrderTypeBuy` or `OrderTypeSell`.

 **Output:**
* **SymbolInfoMarginRateData** with properties:
* **InitialMarginRate** (`double`) — the margin required to open a new position.
* **MarginMaintenanceRate** (`double`) — the maintenance margin rate.

**Purpose:** Quickly determine margin requirements programmatically for risk checks and position sizing. 🚀

 
