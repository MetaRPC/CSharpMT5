# Getting Order History

> **Request:** list of historical orders from MT5 for a given time range
> Fetch all closed and executed orders in the specified time window.

---

### Code Example

```csharp
var historyData = await _mt5Account.OrderHistoryAsync(
    DateTime.UtcNow.AddDays(-7),
    DateTime.UtcNow
);
_logger.LogInformation(
    "OrderHistoryAsync: Count={Count}",
    historyData.HistoryData.Count
);
```

---

### Method Signature

```csharp
Task<OrderHistoryResponse> OrderHistoryAsync(DateTime from, DateTime to)
```

---

## 🔽 Input

| Parameter | Type       | Description                          |
| --------- | ---------- | ------------------------------------ |
| `from`    | `DateTime` | Start of the historical period (UTC) |
| `to`      | `DateTime` | End of the historical period (UTC)   |

---

## ⬆️ Output

Returns an **OrderHistoryResponse** object with:

| Field         | Type                       | Description                             |
| ------------- | -------------------------- | --------------------------------------- |
| `HistoryData` | `IReadOnlyList<OrderInfo>` | Historical orders for the time interval |

### `OrderInfo` Structure

| Field        | Type                    | Description                              |
| ------------ | ----------------------- | ---------------------------------------- |
| `Ticket`     | `ulong`                 | Unique ticket number                     |
| `Symbol`     | `string`                | Trading symbol (e.g., "EURUSD")          |
| `Type`       | `TMT5_ENUM_ORDER_TYPE`  | Type of order (Buy, Sell, etc.)          |
| `Volume`     | `double`                | Executed volume in lots                  |
| `PriceOpen`  | `double`                | Price at which the order was opened      |
| `StopLoss`   | `double`                | Stop Loss level                          |
| `TakeProfit` | `double`                | Take Profit level                        |
| `TimeSetup`  | `DateTime`              | Order placement time (UTC)               |
| `State`      | `TMT5_ENUM_ORDER_STATE` | Final state of the order                 |
| `Profit`     | `double`                | Realized profit/loss in deposit currency |

### `TMT5_ENUM_ORDER_TYPE` Values

| Value                        | Description             |
| ---------------------------- | ----------------------- |
| `Tmt5OrderTypeBuy`           | Market Buy              |
| `Tmt5OrderTypeSell`          | Market Sell             |
| `Tmt5OrderTypeBuyLimit`      | Pending Buy Limit       |
| `Tmt5OrderTypeSellLimit`     | Pending Sell Limit      |
| `Tmt5OrderTypeBuyStop`       | Pending Buy Stop        |
| `Tmt5OrderTypeSellStop`      | Pending Sell Stop       |
| `Tmt5OrderTypeBuyStopLimit`  | Pending Buy Stop Limit  |
| `Tmt5OrderTypeSellStopLimit` | Pending Sell Stop Limit |

### `TMT5_ENUM_ORDER_STATE` Values

| Value                     | Description    |
| ------------------------- | -------------- |
| `Tmt5OrderStatePlaced`    | Order placed   |
| `Tmt5OrderStateExecuted`  | Order executed |
| `Tmt5OrderStateCancelled` | Order canceled |
| `Tmt5OrderStateRejected`  | Order rejected |
| `Tmt5OrderStateExpired`   | Order expired  |

---

## 🎯 Purpose

Use this method to retrieve all **historical closed orders** within a specific date range.

It is ideal for:

* Trade history review and auditing
* Generating reports
* Performance and strategy analysis

Provides full detail per order in a single call.
