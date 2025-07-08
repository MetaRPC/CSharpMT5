# Getting Order History

> **Request:** list of historical orders from MT5 for a given time range.
> Fetch all closed and executed orders in the specified time window.

### Code Example

```csharp
var historyData = await _mt5Account.OrderHistoryAsync(
    DateTime.UtcNow.AddDays(-7),
    DateTime.UtcNow
);
_logger.LogInformation(
    "OrderHistoryAsync: Count={Count}",
    historyData.HistoryData.Count
);
```

✨ **Method Signature:**

```csharp
Task<OrderHistoryResponse> OrderHistoryAsync(DateTime from, DateTime to)
```

---

## Input

* **from** (`DateTime`) — start of the period (UTC).
* **to** (`DateTime`) — end of the period (UTC).

---

## Output

**`OrderHistoryResponse`** — object with the following property:

* **`HistoryData`** (`IReadOnlyList<OrderInfo>`) — list of historical orders for the specified period.

### `OrderInfo` Structure

Each item in `HistoryData` has the following fields:

* **`Ticket`** (`ulong`) — unique ticket number of the order.
* **`Symbol`** (`string`) — trading symbol (e.g., "EURUSD").
* **`Type`** (`TMT5_ENUM_ORDER_TYPE`) — order type (`Tmt5OrderTypeBuy` / `Tmt5OrderTypeSell`).
* **`Volume`** (`double`) — volume of the order in lots.
* **`PriceOpen`** (`double`) — price at which the order was opened.
* **`StopLoss`** (`double`) — stop-loss level set for the order.
* **`TakeProfit`** (`double`) — take-profit level set for the order.
* **`TimeSetup`** (`DateTime`) — UTC timestamp when the order was placed.
* **`State`** (`TMT5_ENUM_ORDER_STATE`) — final state of the order (e.g., executed, canceled).
* **`Profit`** (`double`) — realized profit or loss in the deposit currency.

---

## Purpose

Allows you to fetch all closed and executed orders in a single call for the given time interval, enabling audit, reporting, or analysis workflows. 🚀
