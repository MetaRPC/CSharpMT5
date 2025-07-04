# Getting order history

> **Requesting** the list of historical orders from MT5 for a given time range.

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

✨**Method Signature:** Task<OrderHistoryResponse> OrderHistoryAsync(DateTime from, DateTime to);

* **Input:**  
  - **from (DateTime)** — beginning of the period (UTC).  
  - **to (DateTime)** — end of the period (UTC).

* **Output:**  
  - **OrderHistoryResponse** — an object with a field:  
  - **HistoryData (IReadOnlyList<OrderInfo>)** — list of historical orders for the specified period.

**Purpose:**
It allows you to receive all closed and executed orders in one call for the required time interval for subsequent audit, reporting or analysis. 🚀

