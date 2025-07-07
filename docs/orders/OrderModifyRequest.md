## Modifying an Existing Order

> **Request:** update parameters of an existing order in MT5.

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

✨ **Method Signature:**

```csharp
Task<OrderModifyResponse> OrderModifyAsync(OrderModifyRequest request)
```

---

## Input

**`OrderModifyRequest`** — structure with the following fields:

* **`Ticket`** (`ulong`) — unique ticket identifier of the order to modify.
* **`Price`** (`double`) — new execution price for the order.
* **`StopLoss`** (`double`) — updated stop-loss level.
* **`TakeProfit`** (`double`) — updated take-profit level.

---

## Output

**`OrderModifyResponse`** — structure with the following field:

* **`Order`** (`ulong`) — ticket identifier of the modified order (matches the input `Ticket`).

---

## Purpose

Allows you to adjust the execution price, stop-loss, and take-profit of an existing order in a single call, streamlining your position management and risk control workflows. 🚀
