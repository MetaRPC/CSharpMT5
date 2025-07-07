# Retrieving Margin Rates for a Symbol

> **Request:** initial and maintenance margin rates for a symbol and order type from MT5
> Fetch the margin requirements (initial & maintenance) for a given symbol/order direction.

### Code Example

```csharp
var marginRate = await _mt5Account.SymbolInfoMarginRateAsync(
    Constants.DefaultSymbol,
    ENUM_ORDER_TYPE.OrderTypeBuy
);
_logger.LogInformation(
    "SymbolInfoMarginRate: InitialMarginRate={InitialMarginRate}, MaintenanceMarginRate={MaintenanceMarginRate}",
    marginRate.InitialMarginRate,
    marginRate.MarginMaintenanceRate
);
```

✨ **Method Signature:**

```csharp
Task<SymbolInfoMarginRateData> SymbolInfoMarginRateAsync(
    string symbol,
    ENUM_ORDER_TYPE orderType,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## Input

* **symbol** (`string`) — the name of the financial instrument (e.g., `"EURUSD"`, `"XAUUSD"`).
* **orderType** (`ENUM_ORDER_TYPE`) — direction/type of order. Possible values:

  * **OrderTypeBuy** — market Buy order
  * **OrderTypeSell** — market Sell order
  * **OrderTypeBuyLimit** — pending Buy Limit order
  * **OrderTypeSellLimit** — pending Sell Limit order
  * **OrderTypeBuyStop** — pending Buy Stop order
  * **OrderTypeSellStop** — pending Sell Stop order
  * **OrderTypeBuyStopLimit** — pending Buy Stop Limit order
  * **OrderTypeSellStopLimit** — pending Sell Stop Limit order

---

## Output

**`SymbolInfoMarginRateData`** — structure with the following properties:

* **InitialMarginRate** (`double`) — required margin rate to open a new position (in fraction of volume).
* **MarginMaintenanceRate** (`double`) — required maintenance margin rate after opening a position.

---

## Purpose

Quickly determine margin requirements programmatically for risk checks and position sizing. 🚀
