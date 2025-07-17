# Getting Tickets for Open Orders

> **Request:** list of ticket IDs for all open orders from MT5
> Fetch ticket IDs of current open orders.

---

### Code Example

```csharp
var openedTicketsData = await _mt5Account.OpenedOrdersTicketsAsync();
_logger.LogInformation(
    "OpenedOrdersTicketsAsync: {Tickets}",
    string.Join(", ", openedTicketsData.OpenedOrdersTickets)
);
```

---

### Method Signature

```csharp
Task<OpenedOrdersTicketsResponse> OpenedOrdersTicketsAsync()
```

---

## 🔽 Input

No input parameters.

---

## ⬆️ Output

Returns an **`OpenedOrdersTicketsResponse`** object with:

| Field                 | Type                  | Description                            |
| --------------------- | --------------------- | -------------------------------------- |
| `OpenedOrdersTickets` | `IReadOnlyList<long>` | List of ticket numbers for open orders |

Each ticket represents a unique identifier for an active order on the MT5 account.

---

## 🎯 Purpose

Use this method when you only need the **IDs** of active orders — useful for:

* Fast filtering and routing
* Linking tickets to external logic
* Executing follow-up actions (e.g. modify, close) without full order fetch

This is a lightweight alternative to retrieving full order data.
