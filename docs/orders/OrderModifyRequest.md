# Modifying an Existing Order

> **Request:** update parameters of an existing order in MT5

---

### Code Example

```csharp
var modifyRequest = new OrderModifyRequest
{
    Ticket     = ticket,
    Price      = tick.Ask,
    StopLoss   = tick.Ask - 0.0010,
    TakeProfit = tick.Ask + 0.0010
};
var modifyResult = await _mt5Account.OrderModifyAsync(modifyRequest);
_logger.LogInformation("OrderModifyAsync: OrderId={Order}", modifyResult.Order);
```

---

### Method Signature

```csharp
Task<OrderModifyResponse> OrderModifyAsync(OrderModifyRequest request)
```

---

## 🔽 Input

**OrderModifyRequest** — object with the following fields:

| Field        | Type     | Description                                   |
| ------------ | -------- | --------------------------------------------- |
| `Ticket`     | `ulong`  | Ticket ID of the order to modify              |
| `Price`      | `double` | New price to update (used for pending orders) |
| `StopLoss`   | `double` | Updated Stop Loss level                       |
| `TakeProfit` | `double` | Updated Take Profit level                     |

---

## ⬆️ Output

Returns an **OrderModifyResponse** object:

| Field   | Type    | Description                                     |
| ------- | ------- | ----------------------------------------------- |
| `Order` | `ulong` | Ticket ID of the modified order (same as input) |

---

## 🎯 Purpose

Use this method to update order parameters such as **execution price**, **stop-loss**, and **take-profit** in a single call.

It is essential for:

* Managing pending orders
* Adjusting trade protection levels
* Implementing automated trailing logic or user-driven changes
