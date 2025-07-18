# Sending a New Order

> **Request:** to place a new trade order in MT5
> Fetch ticket ID of the newly created order.

---

### Code Example

```csharp
var symbol      = Constants.DefaultSymbol;
var sendRequest = new OrderSendRequest
{
    Symbol    = symbol,
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
    Volume    = Constants.DefaultVolume,
    Price     = tick.Ask
};
var sendResult = await _mt5Account.OrderSendAsync(sendRequest);
ulong ticket  = sendResult.Order;
_logger.LogInformation("OrderSendAsync: Order={Order}", ticket);
```

---

### Method Signature

```csharp
Task<OrderSendResponse> OrderSendAsync(OrderSendRequest request)
```

---

## 🔽 Input

**OrderSendRequest** — object with the following fields:

| Field       | Type                   | Description                     |
| ----------- | ---------------------- | ------------------------------- |
| `Symbol`    | `string`               | Trading symbol (e.g., "EURUSD") |
| `Operation` | `TMT5_ENUM_ORDER_TYPE` | Order type (market or pending)  |
| `Volume`    | `double`               | Volume in lots                  |
| `Price`     | `double`               | Execution price                 |

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

---

## ⬆️ Output

Returns an **OrderSendResponse** object:

| Field   | Type    | Description                          |
| ------- | ------- | ------------------------------------ |
| `Order` | `ulong` | Ticket ID of the newly created order |

---

## 🎯 Purpose

Use this method to **send a new trade order** (market or pending) to MT5.

It returns the **ticket number** which can be used for further management:

* Logging or tracking
* Modifying the order later
* Closing the order manually or automatically
