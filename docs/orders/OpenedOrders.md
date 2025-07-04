# Getting all open orders

> **Requesting** the current list of open orders from MT5.
### Code Example

```csharp
var openedOrdersData = await _mt5Account.OpenedOrdersAsync();
_logger.LogInformation(
    "OpenedOrdersAsync: Count={Count}",
    openedOrdersData.OpenedOrders.Count);
```
**Method Signature:** Task<OpenedOrdersResponse> OpenedOrdersAsync();
**Input:**  <span style="color:red"> None — this method takes no parameters. </span>

**Output:** OpenedOrdersResponse — объект с полем
OpenedOrders (IReadOnlyList<OrderInfo>) — список всех текущих открытых ордеров.



