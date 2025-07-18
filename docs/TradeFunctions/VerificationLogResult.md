# Verifying a Trade and Logging Margin vs Free Margin

> **Request:** simulate a trade and retrieve required margin and remaining free margin.

---

### Code Example

```csharp
var checkResult = await _mt5Account.OrderCheckAsync(checkRequest);
_logger.LogInformation(
    "OrderCheck: Margin={Margin}, FreeMargin={FreeMargin}",
    checkResult.MqlTradeCheckResult.Margin,
    checkResult.MqlTradeCheckResult.FreeMargin
);
```

---

### Method Signature

```csharp
Task<OrderCheckData> OrderCheckAsync(
    OrderCheckRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

**OrderCheckRequest** — object with:

| Field             | Type                  | Description                           |
| ----------------- | --------------------- | ------------------------------------- |
| `MqlTradeRequest` | `MrpcMqlTradeRequest` | Trade parameters to simulate/validate |

### `MrpcMqlTradeRequest` Fields

| Field        | Type                 | Description                                   |
| ------------ | -------------------- | --------------------------------------------- |
| `Symbol`     | `string`             | Trading symbol (e.g., "EURUSD")               |
| `Volume`     | `double`             | Volume in lots                                |
| `Price`      | `double`             | Desired execution price                       |
| `StopLimit`  | `double?`            | Stop limit price (optional)                   |
| `StopLoss`   | `double?`            | Stop Loss level (optional)                    |
| `TakeProfit` | `double?`            | Take Profit level (optional)                  |
| `Deviation`  | `int`                | Max price slippage in points                  |
| `OrderType`  | `ENUM_ORDER_TYPE_TF` | Order type                                    |
| `Expiration` | `DateTime?`          | Expiration time for pending orders (optional) |
| `Comment`    | `string`             | Order comment (optional)                      |
| `Position`   | `ulong`              | Order to be modified (optional)               |
| `PositionBy` | `ulong`              | Secondary reference (optional)                |

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

---

## ⬆️ Output

Returns an **OrderCheckData** object:

| Field                 | Type                      | Description                |
| --------------------- | ------------------------- | -------------------------- |
| `MqlTradeCheckResult` | `MqlTradeCheckResultData` | Result of trade simulation |

### `MqlTradeCheckResultData` Fields

| Field        | Type     | Description                            |
| ------------ | -------- | -------------------------------------- |
| `Margin`     | `double` | Required margin for this trade request |
| `FreeMargin` | `double` | Remaining free margin after execution  |

---

## 🎯 Purpose

Use this method to simulate a trade **and** view:

* How much margin would be required
* What free margin would remain

Ideal for:

* Building visual risk previews
* Pre-trade eligibility checks
* Risk-informed trading decisions
