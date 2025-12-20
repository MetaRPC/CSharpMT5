# MT5Account · Positions & Orders Information - Overview

> Open positions, pending orders, order history, and position history. Use this page to choose the right API for accessing trading activity.

## 📁 What lives here

* **[PositionsTotal](./PositionsTotal.md)** - **count** of currently open positions.
* **[OpenedOrders](./OpenedOrders.md)** - **detailed list** of all open positions and pending orders.
* **[OpenedOrdersTickets](./OpenedOrdersTickets.md)** - **lightweight list** of tickets only (no full details).
* **[OrderHistory](./OrderHistory.md)** - **historical orders and deals** within time range (with pagination).
* **[PositionsHistory](./PositionsHistory.md)** - **closed positions** within time range (with pagination).

---

## 🧭 Plain English

* **PositionsTotal** → quick count of how many positions are open right now.
* **OpenedOrders** → full details of all active positions + pending orders (24 fields per order!).
* **OpenedOrdersTickets** → just ticket IDs for positions/orders (fast, lightweight).
* **OrderHistory** → browse past orders and deals with time filter and pagination.
* **PositionsHistory** → browse closed positions with profit/loss info.

> Rule of thumb: need **quick count** → `PositionsTotalAsync`; need **full details** → `OpenedOrdersAsync`; need **just tickets** → `OpenedOrdersTicketsAsync`; need **history** → OrderHistory/PositionsHistory.

---

## Quick choose

| If you need…                                     | Use                            | Returns                    | Key inputs                          |
| ------------------------------------------------ | ------------------------------ | -------------------------- | ----------------------------------- |
| Just count of open positions                     | `PositionsTotalAsync`          | `int`                      | *(none)*                            |
| Full details of positions + pending orders       | `OpenedOrdersAsync`            | List of positions + orders | Sort mode (optional)                |
| Just ticket IDs (lightweight)                    | `OpenedOrdersTicketsAsync`     | Two lists of tickets       | *(none)*                            |
| Historical orders and deals                      | `OrderHistoryAsync`            | Paginated order history    | Time range + pagination params      |
| Closed positions with P/L                        | `PositionsHistoryAsync`        | List of closed positions   | Time range + pagination params      |

---

## ❌ Cross‑refs & gotchas

* **PositionInfo** has 20 fields including profit, swap, commission, magic number.
* **OpenedOrderInfo** has 24 fields including state, type, volumes, expiration.
* **Tickets** are unique identifiers - use them to modify/close positions.
* **History** is paginated - use PageNumber and ItemsPerPage for large datasets.
* **Sort modes** affect order of results - choose based on your use case.
* **Magic numbers** help identify EA-placed orders vs manual trades.

---

## 🟢 Minimal snippets

```csharp
// Quick check: how many positions are open?
var totalData = await account.PositionsTotalAsync();
Console.WriteLine($"Open positions: {totalData.TotalPositions}");
```

```csharp
// Get full details of all positions and orders
var data = await account.OpenedOrdersAsync();
foreach (var position in data.PositionInfos)
{
    Console.WriteLine($"Position #{position.Ticket} {position.Symbol} - P/L: ${position.Profit:F2}");
}
foreach (var order in data.OpenedOrders)
{
    Console.WriteLine($"Pending Order #{order.Ticket} {order.Symbol} @ {order.PriceOpen:F5}");
}
```

```csharp
// Get just ticket IDs (fast and lightweight)
var tickets = await account.OpenedOrdersTicketsAsync();
Console.WriteLine($"Position tickets: {string.Join(", ", tickets.OpenedPositionTickets)}");
Console.WriteLine($"Order tickets: {string.Join(", ", tickets.OpenedOrdersTickets)}");
```

```csharp
// Get order history for last 7 days
var from = DateTime.Now.AddDays(-7);
var to = DateTime.Now;
var history = await account.OrderHistoryAsync(from, to,
    sortMode: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc);

Console.WriteLine($"Total history records: {history.ArrayTotal}");
foreach (var record in history.HistoryData)
{
    if (record.HistoryDeal != null)
    {
        Console.WriteLine($"Deal #{record.HistoryDeal.Ticket} - Profit: ${record.HistoryDeal.Profit:F2}");
    }
}
```

```csharp
// Get closed positions history
var from = DateTime.Now.AddDays(-30);
var to = DateTime.Now;
var closedPositions = await account.PositionsHistoryAsync(from, to);

foreach (var pos in closedPositions.HistoryPositions)
{
    Console.WriteLine($"Closed: {pos.Symbol} - P/L: ${pos.Profit:F2}, Duration: {pos.CloseTime.ToDateTime() - pos.OpenTime.ToDateTime()}");
}
```

---

## See also

* **Trading:** [OrderSend](../4.%20Trading_Operattons/OrderSend.md) - open new positions
* **Trading:** [OrderModify](../4.%20Trading_Operattons/OrderModify.md) - modify existing orders/positions
* **Trading:** [OrderClose](../4.%20Trading_Operattons/OrderClose.md) - close positions
* **Streaming:** [OnTrade](../7.%20Streaming_Methods/OnTrade.md) - real-time position/order updates
* **Streaming:** [OnPositionsAndPendingOrdersTickets](../7.%20Streaming_Methods/OnPositionsAndPendingOrdersTickets.md) - real-time ticket updates
