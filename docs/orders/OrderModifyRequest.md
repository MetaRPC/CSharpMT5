## Modifying an existing order

> **Requesting** to update parameters of an existing order in MT5.

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

✨**Method Signature:**
```csharp
 Task<OrderModifyResponse> OrderModifyAsync(OrderModifyRequest request);
```

* **Input:**
    * **request (OrderModifyRequest)** — object with properties:
      * **Ticket (ulong)** — ticket ID of the order to modify.
      * **Price (double)** — new execution price.
      * **StopLoss (double)** — new stop-loss level.
      * **TakeProfit (double)** — new take-profit level.


* **Output:**
   * **OrderModifyResponse** — object with properties:
      * **Order (ulong)** — ticket ID of the modified order.

**Purpose:**
Allows you to adjust price, stop-loss, and take-profit of an existing order in a single call, streamlining your risk management workflow. 🚀
