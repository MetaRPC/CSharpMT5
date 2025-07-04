# Getting tickets for open orders

> **Requesting** the list of ticket IDs for all open orders from MT5.

### Code Example

```csharp
var openedTicketsData = await _mt5Account.OpenedOrdersTicketsAsync();
_logger.LogInformation(
    "OpenedOrdersTicketsAsync: {Tickets}",
    string.Join(", ", openedTicketsData.OpenedOrdersTickets)
);
```
✨**Method Signature:** Task<OpenedOrdersTicketsResponse> OpenedOrdersTicketsAsync();

* **Input:None — this method takes no parameters.**
**Output:** 
    * **OpenedOrdersTicketsResponse** — object with property.
    *  **OpenedOrdersTickets (IReadOnlyList<long>)** — a list of ticket numbers for all open orders.

**Purpose:**
Allows you to retrieve just the ticket IDs of open orders in a single call, making it easy to log, filter, or act on orders by their identifiers without extra data. 🚀



