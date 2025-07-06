## Sending a new order

> **Requesting** to place a new trade order in MT5.

### Code Example

```csharp
var sendRequest = new OrderSendRequest
{
    Symbol    = Constants.DefaultSymbol,
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
    Volume    = Constants.DefaultVolume,
    Price     = tick.Ask
};
var sendResult = await _mt5Account.OrderSendAsync(sendRequest);
ulong ticket = sendResult.Order;
_logger.LogInformation("OrderSendAsync: Order={Order}", ticket);
``` 
✨**Method Signature:** 
```csharp
Task<OrderSendResponse> OrderSendAsync(OrderSendRequest request);
```
 **Input:**
 **request** (`OrderSendRequest`) — object with properties:
 * **Symbol** (`string`) — the symbol to trade (e.g., Constants.DefaultSymbol).
 * **Operation** (`TMT5_ENUM_ORDER_TYPE`) — the type of order (e.g., Tmt5OrderTypeBuy, Tmt5OrderTypeSell).
 * **Volume** (`double`) — the volume of the order in lots.
 * **Price** (`double`) — the execution price (e.g., market Bid/Ask).

 **Output:**
 * **OrderSendResponse** — object with properties:
 * **Order** (`ulong`) — the ticket number of the newly created order.

**Purpose:**
Allows you to send a new trading order to MT5 in one call, returning the ticket for tracking, modification, or closing later. 🚀


