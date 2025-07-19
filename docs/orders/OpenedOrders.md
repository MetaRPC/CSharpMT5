# Getting All Open Orders

> **Request:** current list of open orders from MT5
> Fetch all open orders on the account in one call.

---

### Code Example1

```csharp
var openedOrdersData = await _mt5Account.OpenedOrdersAsync();
_logger.LogInformation(
    "OpenedOrdersAsync: Count={Count}",
    openedOrdersData.OpenedOrders.Count
);
```

---

### Method Signature

```csharp
Task<OpenedOrdersResponse> OpenedOrdersAsync()
```

---

## 🔽 Input

No input parameters.

---

## ⬆️ Output

Returns an **`OpenedOrdersResponse`** object with:

| Field          | Type                       | Description                       |
| -------------- | -------------------------- | --------------------------------- |
| `OpenedOrders` | `IReadOnlyList<OrderInfo>` | List of all currently open orders |

### `OrderInfo` Structure

Each order includes:

| Field           | Type                    | Description                         |
| --------------- | ----------------------- | ----------------------------------- |
| `Ticket`        | `ulong`                 | Unique ticket number                |
| `Symbol`        | `string`                | Trading symbol (e.g., "EURUSD")     |
| `Type`          | `TMT5_ENUM_ORDER_TYPE`  | Type of the order (Buy, Sell, etc.) |
| `VolumeInitial` | `double`                | Volume at order placement           |
| `VolumeCurrent` | `double`                | Remaining open volume               |
| `PriceOpen`     | `double`                | Open price                          |
| `PriceCurrent`  | `double`                | Current price                       |
| `StopLoss`      | `double`                | Stop Loss level                     |
| `TakeProfit`    | `double`                | Take Profit level                   |
| `Commission`    | `double`                | Accrued commission                  |
| `Swap`          | `double`                | Accrued swap                        |
| `Profit`        | `double`                | Current floating profit/loss        |
| `TimeSetup`     | `DateTime`              | Time order was placed (UTC)         |
| `State`         | `TMT5_ENUM_ORDER_STATE` | Order status                        |

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

| Value                     | Description         |
| ------------------------- | ------------------- |
| `Tmt5OrderStatePlaced`    | Order is placed     |
| `Tmt5OrderStateExecuted`  | Order is filled     |
| `Tmt5OrderStateCancelled` | Order was cancelled |
| `Tmt5OrderStateRejected`  | Order was rejected  |
| `Tmt5OrderStateExpired`   | Order expired       |

---

## 🎯 Purpose

Use this method to retrieve all currently active orders on the account.
It is ideal for:

* Monitoring trading state
* Building dashboards
* Running logic based on open exposure

All order details are returned in one efficient call.
