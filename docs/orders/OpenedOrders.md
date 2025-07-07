# Getting All Open Orders

> **Request:** current list of open orders from MT5
> Fetch all open orders on the account in one call.

### Code Example

```csharp
var openedOrdersData = await _mt5Account.OpenedOrdersAsync();
_logger.LogInformation(
    "OpenedOrdersAsync: Count={Count}",
    openedOrdersData.OpenedOrders.Count
);
```

✨ **Method Signature:**

```csharp
Task<OpenedOrdersResponse> OpenedOrdersAsync()
```

---

## Input

*None* — this method takes no parameters.

---

## Output

**`OpenedOrdersResponse`** — object with the following property:

* **`OpenedOrders`** (`IReadOnlyList<OrderInfo>`) — list of all current open orders.

### `OrderInfo` Structure

Each item in `OpenedOrders` has the following fields:

* **`Ticket`** (`ulong`) — unique ticket number of the order.
* **`Symbol`** (`string`) — trading symbol (e.g., "EURUSD").
* **`Type`** (`TMT5_ENUM_ORDER_TYPE`) — order type: e.g., `Tmt5OrderTypeBuy` or `Tmt5OrderTypeSell`.
* **`Volume`** (`double`) — volume of the order in lots.
* **`PriceOpen`** (`double`) — price at which the order was opened.
* **`StopLoss`** (`double`) — current stop-loss level.
* **`TakeProfit`** (`double`) — current take-profit level.
* **`TimeSetup`** (`DateTime`) — UTC timestamp when the order was placed.
* **`State`** (`TMT5_ENUM_ORDER_STATE`) — current state of the order (e.g., placed, activated).
* **`Profit`** (`double`) — the current floating profit or loss of the order.

---

## Purpose

Allows you to receive all open orders on the account in one universal call, so that you can log, process, or make decisions on them without juggling multiple methods. 🚀
