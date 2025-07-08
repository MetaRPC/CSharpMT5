# Getting Tickets for Open Orders

> **Request:** list of ticket IDs for all open orders from MT5
> Fetch ticket IDs of current open orders.

### Code Example

```csharp
var openedTicketsData = await _mt5Account.OpenedOrdersTicketsAsync();
_logger.LogInformation(
    "OpenedOrdersTicketsAsync: {Tickets}",
    string.Join(", ", openedTicketsData.OpenedOrdersTickets)
);
```

✨ **Method Signature:**

```csharp
Task<OpenedOrdersTicketsResponse> OpenedOrdersTicketsAsync()
```

---

## Input

*None* — this method takes no parameters.

---

## Output

**`OpenedOrdersTicketsResponse`** — object with the following property:

* **`OpenedOrdersTickets`** (`IReadOnlyList<long>`) — list of ticket numbers for all currently open orders.

---

## Purpose

Allows you to retrieve just the ticket IDs of open orders in a single call, simplifying logging, filtering, or acting on orders by their identifiers without loading full order details. 🚀
