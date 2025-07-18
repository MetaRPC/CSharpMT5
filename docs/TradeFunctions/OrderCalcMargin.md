# Calculating Required Margin

> **Request:** margin calculation for a planned trade
> Compute the margin needed to open a position with given parameters.

---

### Code Example

```csharp
// Calculate margin for a buy order on EURUSD
tick = await _mt5Account.SymbolInfoTickAsync(Constants.DefaultSymbol);
var margin = await _mt5Account.OrderCalcMarginAsync(new OrderCalcMarginRequest
{
    Symbol    = Constants.DefaultSymbol,
    OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
    Volume    = Constants.DefaultVolume,
    OpenPrice = tick.Ask
});
_logger.LogInformation(
    "OrderCalcMargin: Margin={Margin}",
    margin.Margin
);
```

---

### Method Signature

```csharp
Task<OrderCalcMarginData> OrderCalcMarginAsync(
    OrderCalcMarginRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

**OrderCalcMarginRequest** — object with the following fields:

| Field       | Type                 | Description                    |
| ----------- | -------------------- | ------------------------------ |
| `Symbol`    | `string`             | Symbol to calculate margin for |
| `OrderType` | `ENUM_ORDER_TYPE_TF` | Type of trade order            |
| `Volume`    | `double`             | Volume in lots                 |
| `OpenPrice` | `double`             | Expected open price            |

### `ENUM_ORDER_TYPE_TF` Values

| Value                      | Description             |
| -------------------------- | ----------------------- |
| `OrderTypeTfBuy`           | Market Buy              |
| `OrderTypeTfSell`          | Market Sell             |
| `OrderTypeTfBuyLimit`      | Pending Buy Limit       |
| `OrderTypeTfSellLimit`     | Pending Sell Limit      |
| `OrderTypeTfBuyStop`       | Pending Buy Stop        |
| `OrderTypeTfSellStop`      | Pending Sell Stop       |
| `OrderTypeTfBuyStopLimit`  | Pending Buy Stop Limit  |
| `OrderTypeTfSellStopLimit` | Pending Sell Stop Limit |

Optional parameters:

| Parameter           | Type                | Description                 |
| ------------------- | ------------------- | --------------------------- |
| `deadline`          | `DateTime?`         | Optional request timeout    |
| `cancellationToken` | `CancellationToken` | Optional cancellation token |

---

## ⬆️ Output

Returns an **OrderCalcMarginData** object:

| Field    | Type     | Description                         |
| -------- | -------- | ----------------------------------- |
| `Margin` | `double` | Required margin in account currency |

---

## 🎯 Purpose

Use this method to calculate the **required margin** before placing an order.

Perfect for:

* Validating sufficient funds
* Pre-trade simulations and risk controls
* Dynamic lot size calculations
