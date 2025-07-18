# Retrieving Margin Rates for a Symbol

> **Request:** initial and maintenance margin rates for a symbol and order type from MT5
> Fetch the margin requirements (initial & maintenance) for a given symbol/order direction.

---

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

---

### Method Signature

```csharp
Task<SymbolInfoMarginRateData> SymbolInfoMarginRateAsync(
    string symbol,
    ENUM_ORDER_TYPE orderType,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

| Parameter   | Type              | Description                                  |
| ----------- | ----------------- | -------------------------------------------- |
| `symbol`    | `string`          | Symbol name (e.g., "EURUSD", "XAUUSD")       |
| `orderType` | `ENUM_ORDER_TYPE` | Type of order used to determine margin rates |

### `ENUM_ORDER_TYPE` Values

| Value                    | Description              |
| ------------------------ | ------------------------ |
| `OrderTypeBuy`           | Market Buy order         |
| `OrderTypeSell`          | Market Sell order        |
| `OrderTypeBuyLimit`      | Pending Buy Limit order  |
| `OrderTypeSellLimit`     | Pending Sell Limit order |
| `OrderTypeBuyStop`       | Pending Buy Stop order   |
| `OrderTypeSellStop`      | Pending Sell Stop order  |
| `OrderTypeBuyStopLimit`  | Pending Buy Stop Limit   |
| `OrderTypeSellStopLimit` | Pending Sell Stop Limit  |

---

## ⬆️ Output

Returns a **SymbolInfoMarginRateData** object:

| Field                   | Type     | Description                                   |
| ----------------------- | -------- | --------------------------------------------- |
| `InitialMarginRate`     | `double` | Margin rate required to open a new position   |
| `MarginMaintenanceRate` | `double` | Margin rate required to maintain the position |

---

## 🎯 Purpose

Use this method to determine **margin requirements** based on symbol and order type.

Perfect for:

* Pre-trade risk analysis
* Calculating how much margin will be used and retained
* Building margin-aware trading UIs or risk engines
