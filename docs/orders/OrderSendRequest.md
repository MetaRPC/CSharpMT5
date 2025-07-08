# Sending a New Order

> **Request:** to place a new trade order in MT5.
> Fetch ticket ID of the newly created order.

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

✨ **Method Signature:**

```csharp
Task<OrderSendResponse> OrderSendAsync(OrderSendRequest request)
```

---

## Input

**`OrderSendRequest`** — structure with the following fields:

* **`Symbol`** (`string`) — trading symbol to place the order on (e.g., `"EURUSD"`).
* **`Operation`** (`TMT5_ENUM_ORDER_TYPE`) — type of order to execute. Possible values:

  * **`Tmt5OrderTypeBuy`** — market Buy order
  * **`Tmt5OrderTypeSell`** — market Sell order
  * **`Tmt5OrderTypeBuyLimit`** — pending Buy Limit order
  * **`Tmt5OrderTypeSellLimit`** — pending Sell Limit order
  * **`Tmt5OrderTypeBuyStop`** — pending Buy Stop order
  * **`Tmt5OrderTypeSellStop`** — pending Sell Stop order
  * **`Tmt5OrderTypeBuyStopLimit`** — pending Buy Stop Limit order
  * **`Tmt5OrderTypeSellStopLimit`** — pending Sell Stop Limit order
* **`Volume`** (`double`) — volume in lots to trade (e.g., `0.1`).
* **`Price`** (`double`) — execution price for the order (e.g., `tick.Ask` for Buy).

---

## Output

**`OrderSendResponse`** — structure with the following field:

* **`Order`** (`ulong`) — ticket ID of the newly created order.

---

## Purpose

Allows you to send a new trading order to MT5 in a single call, returning the ticket for tracking, modification, or closing later. 🚀
