# Getting all open orders

> **Requesting** the current list of open orders from MT5.
### Code Example

```csharp
var openedOrdersData = await _mt5Account.OpenedOrdersAsync();
_logger.LogInformation(
    "OpenedOrdersAsync: Count={Count}",
    openedOrdersData.OpenedOrders.Count);
```
✨**Method Signature:** _Task<OpenedOrdersResponse> OpenedOrdersAsync()_;

**Input:**  _None — this method takes no parameters_.

**Output:** 
* **OpenedOrdersResponse** — an object with a field.
* **OpenedOrders (IReadOnlyList<OrderInfo>)** — list of all current open orders.

**Purpose:**
It allows you to receive all open orders on the account in one universal call, so that you can polish, process or make decisions on them without wasting on a bunch of separate methods. 🚀



