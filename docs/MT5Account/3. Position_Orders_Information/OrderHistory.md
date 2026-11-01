# ✅ Getting Order History

> **Request:** historical orders and deals from **MT5**. Get detailed history of closed/cancelled orders and executed deals within a time range with pagination support.

**API Information:**

* **SDK wrapper:** `MT5Account.OrderHistoryAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.AccountHelper`
* **Proto definition:** `OrderHistory` (defined in `mt5-term-api-account-helper.proto`)

### RPC

* **Service:** `mt5_term_api.AccountHelper`
* **Method:** `OrderHistory(OrderHistoryRequest) → OrderHistoryReply`
* **Low‑level client (generated):** `AccountHelper.OrderHistory(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<OrdersHistoryData> OrderHistoryAsync(
            DateTime from,
            DateTime to,
            BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sortMode = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
            int pageNumber = 0,
            int itemsPerPage = 0,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`OrderHistoryRequest { inputFrom: Timestamp, inputTo: Timestamp, inputSortMode: enum, pageNumber: int32, itemsPerPage: int32 }`


**Reply message:**

`OrderHistoryReply { data: OrdersHistoryData }` or `{ error: Error }`

---

## 🔽 Input

| Parameter           | Type                                  | Required | Description                                                     |
| ------------------- | ------------------------------------- | -------- | --------------------------------------------------------------- |
| `from`              | `DateTime`                            | ✅       | Start of time range (**server time**, not UTC)                 |
| `to`                | `DateTime`                            | ✅       | End of time range (**server time**, not UTC)                   |
| `sortMode`          | `BMT5_ENUM_ORDER_HISTORY_SORT_TYPE`   | ❌       | Sort order for results (default: by open time ascending)       |
| `pageNumber`        | `int`                                 | ❌       | Page number for pagination (0-based, default: 0 = all pages)   |
| `itemsPerPage`      | `int`                                 | ❌       | Items per page (default: 0 = no pagination, return all)        |
| `deadline`          | `DateTime?`                           | ❌       | Absolute per‑call **UTC** deadline → converted to timeout      |
| `cancellationToken` | `CancellationToken`                   | ❌       | Cooperative cancel for the call/retry loop                     |

---

## ⬆️ Output — `OrdersHistoryData`

| Field           | Type                  | Description                                              |
| --------------- | --------------------- | -------------------------------------------------------- |
| `ArrayTotal`    | `int32`               | Total number of history items matching criteria          |
| `PageNumber`    | `int32`               | Current page number (0-based)                            |
| `ItemsPerPage`  | `int32`               | Items per page setting                                   |
| `HistoryData`   | `List<HistoryData>`   | List of history records (orders + deals)                 |

---

## 🧱 Related types and enums (from proto)

### `BMT5_ENUM_ORDER_HISTORY_SORT_TYPE` (Sort Options)

| Enum Value                           | Value | Description                           |
| ------------------------------------ | ----- | ------------------------------------- |
| `Bmt5SortByOpenTimeAsc`         | 0     | Sort by open time (oldest first)      |
| `Bmt5SortByOpenTimeDesc`        | 1     | Sort by open time (newest first)      |
| `Bmt5SortByCloseTimeAsc`        | 2     | Sort by close time (oldest first)     |
| `Bmt5SortByCloseTimeDesc`       | 3     | Sort by close time (newest first)     |
| `Bmt5SortByOrderTicketIdAsc`   | 4     | Sort by ticket ID (ascending)         |
| `Bmt5SortByOrderTicketIdDesc`  | 5     | Sort by ticket ID (descending)        |

### `HistoryData` (Container for Order + Deal)

| Field          | Type                 | Description                                    |
| -------------- | -------------------- | ---------------------------------------------- |
| `Index`        | `uint32`             | Zero-based index in the result list           |
| `HistoryOrder` | `OrderHistoryData`   | Order information (may be null)                |
| `HistoryDeal`  | `DealHistoryData`    | Deal information (may be null)                 |

### `OrderHistoryData` (Historical Order)

| Field            | Type                          | Description                                      |
| ---------------- | ----------------------------- | ------------------------------------------------ |
| `Ticket`         | `ulong`                       | Order ticket ID                                  |
| `SetupTime`      | `Timestamp`                   | Order setup time                                 |
| `DoneTime`       | `Timestamp`                   | Order completion time                            |
| `State`          | `BMT5_ENUM_ORDER_STATE`       | Order state (Filled, Cancelled, Expired, etc.)   |
| `PriceCurrent`   | `double`                      | Market price at order time                       |
| `PriceOpen`      | `double`                      | Order price                                      |
| `StopLimit`      | `double`                      | StopLimit price (for STOP_LIMIT orders)          |
| `StopLoss`       | `double`                      | Stop Loss price                                  |
| `TakeProfit`     | `double`                      | Take Profit price                                |
| `VolumeCurrent`  | `double`                      | Remaining volume (unfilled)                      |
| `VolumeInitial`  | `double`                      | Initial order volume                             |
| `MagicNumber`    | `long`                        | Magic number (EA identifier)                     |
| `Type`           | `BMT5_ENUM_ORDER_TYPE`        | Order type (Buy, Sell, BuyLimit, etc.)           |
| `TimeExpiration` | `Timestamp`                   | Order expiration time                            |
| `TypeFilling`    | `BMT5_ENUM_ORDER_TYPE_FILLING`| Filling mode (FOK, IOC, Return, BOC)             |
| `TypeTime`       | `BMT5_ENUM_ORDER_TYPE_TIME`   | Order lifetime (GTC, DAY, SPECIFIED, etc.)       |
| `PositionId`     | `ulong`                       | Position ID this order belongs to                |
| `Symbol`         | `string`                      | Trading symbol                                   |
| `ExternalId`     | `string`                      | External order identifier                        |

### `DealHistoryData` (Executed Deal)

| Field          | Type                          | Description                                     |
| -------------- | ----------------------------- | ----------------------------------------------- |
| `Ticket`       | `ulong`                       | Deal ticket ID                                  |
| `Profit`       | `double`                      | Deal profit/loss                                |
| `Commission`   | `double`                      | Commission charged                              |
| `Fee`          | `double`                      | Additional fee                                  |
| `Price`        | `double`                      | Execution price                                 |
| `StopLoss`     | `double`                      | Stop Loss at deal time                          |
| `TakeProfit`   | `double`                      | Take Profit at deal time                        |
| `Swap`         | `double`                      | Swap charged                                    |
| `Volume`       | `double`                      | Deal volume (lots)                              |
| `EntryType`    | `BMT5_ENUM_DEAL_ENTRY_TYPE`   | Entry type (In, Out, InOut, OutBy)              |
| `Time`         | `Timestamp`                   | Deal execution time                             |
| `Type`         | `BMT5_ENUM_DEAL_TYPE`         | Deal type (Buy, Sell, Balance, etc.)            |
| `Reason`       | `BMT5_ENUM_DEAL_REASON`       | Deal reason (Client, Mobile, Web, Expert)       |
| `PositionId`   | `ulong`                       | Position ID this deal belongs to                |
| `Comment`      | `string`                      | Deal comment                                    |
| `Symbol`       | `string`                      | Trading symbol                                  |
| `ExternalId`   | `string`                      | External deal identifier                        |
| `AccountLogin` | `long`                        | Account login number                            |

### `BMT5_ENUM_DEAL_TYPE`

| Enum Value                              | Value | Description                     |
| --------------------------------------- | ----- | ------------------------------- |
| `Bmt5DealTypeBuy`                    | 0     | Buy deal                        |
| `Bmt5DealTypeSell`                   | 1     | Sell deal                       |
| `Bmt5DealTypeBalance`                | 2     | Balance operation               |
| `Bmt5DealTypeCredit`                 | 3     | Credit operation                |
| `Bmt5DealTypeCharge`                 | 4     | Additional charge               |
| `Bmt5DealTypeCorrection`             | 5     | Correction                      |
| `Bmt5DealTypeBonus`                  | 6     | Bonus                           |
| `Bmt5DealTypeCommission`             | 7     | Additional commission           |
| `Bmt5DealTypeCommissionDaily`       | 8     | Daily commission                |
| `Bmt5DealTypeCommissionMonthly`     | 9     | Monthly commission              |
| `Bmt5DealTypeCommissionAgentDaily` | 10    | Daily agent commission          |
| `Bmt5DealTypeCommissionAgentMonthly`| 11   | Monthly agent commission        |
| `Bmt5DealTypeInterest`               | 12    | Interest rate                   |
| `Bmt5DealTypeBuyCanceled`           | 13    | Canceled buy deal               |
| `Bmt5DealTypeSellCanceled`          | 14    | Canceled sell deal              |
| `Bmt5DealDividend`                    | 15    | Dividend operation              |
| `Bmt5DealDividendFranked`            | 16    | Franked (tax-adjusted) dividend |
| `Bmt5DealTax`                         | 17    | Tax charge                      |

### `BMT5_ENUM_DEAL_ENTRY_TYPE`

| Enum Value                | Value | Description                                    |
| ------------------------- | ----- | ---------------------------------------------- |
| `Bmt5DealEntryIn`      | 0     | Entry into position                            |
| `Bmt5DealEntryOut`     | 1     | Exit from position                             |
| `Bmt5DealEntryInout`   | 2     | Reversal (close and open opposite)             |
| `Bmt5DealEntryOutBy`  | 3     | Close position by opposite position            |

### Related enums from `OpenedOrders.md`:

- `BMT5_ENUM_ORDER_TYPE` (9 values) - see OpenedOrders.md
- `BMT5_ENUM_ORDER_STATE` (10 values) - see OpenedOrders.md
- `BMT5_ENUM_ORDER_TYPE_FILLING` (4 values) - see OpenedOrders.md
- `BMT5_ENUM_ORDER_TYPE_TIME` (4 values) - see OpenedOrders.md
- `BMT5_ENUM_DEAL_REASON` (4 values) - see OpenedOrders.md (similar to POSITION_REASON)

---

## 💬 Just the essentials

* **What it is.** RPC to retrieve historical orders and deals within a time range.
* **Why you need it.** Analyze trading history, generate reports, calculate realized P&L, audit trading activity.
* **Time range required.** Must specify `from` and `to` dates (**server time**, not UTC).
* **Pagination support.** Can retrieve large histories in pages using `pageNumber` and `itemsPerPage`.
* **Two data types.** Each `HistoryData` contains order info + corresponding deal info.
* **6 sort modes.** Sort by open time, close time, or ticket ID (ascending/descending).

---

## 🎯 Purpose

Use this method when you need to:

* Generate trading history reports for a specific period.
* Calculate total realized profit/loss for a time range.
* Analyze closed positions and cancelled orders.
* Audit trading activity (who, when, what).
* Export trading history to external systems.
* Track EA performance over time (filter by magic number).
* Verify executed deals match expected orders.

---

## 🧩 Notes & Tips

* **CRITICAL**: `from` and `to` are **server time**, not UTC. Use `AccountSummaryData.ServerTime` to get current server time.
* **Pagination**: Set `pageNumber=0, itemsPerPage=0` to get **all** results (no pagination).
* **Large datasets**: For long time ranges, use pagination to avoid timeouts.
* Each `HistoryData` may contain **order only**, **deal only**, or **both**.
* **Order** = instruction placed (pending order that was filled/cancelled/expired).
* **Deal** = actual execution (buy/sell transaction that happened).
* One order can generate multiple deals (partial fills).
* `ArrayTotal` shows **total** items matching criteria, not just current page.
* Use `Bmt5SortByCloseTimeDesc` to get most recent closed trades first.
* Filter by `MagicNumber` in your code to get EA-specific history.
* `Profit` in `DealHistoryData` is **realized P&L** (trade already closed).
* `Commission`, `Fee`, and `Swap` are **separate** from Profit.
* Use longer timeout (10-30s) for large time ranges.

---

## 🔗 Usage Examples

### 1) Get last 7 days history

```csharp
// Retrieve trading history for last week
var serverTime = DateTime.UtcNow; // Should get from AccountSummaryData.ServerTime
var from = serverTime.AddDays(-7);
var to = serverTime;

var history = await acc.OrderHistoryAsync(
    from: from,
    to: to,
    sortMode: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc,
    deadline: DateTime.UtcNow.AddSeconds(15));

Console.WriteLine($"Total history items: {history.ArrayTotal}");
Console.WriteLine($"Retrieved: {history.HistoryData.Count} items");
```

### 2) Calculate total realized P&L

```csharp
// Sum profits from all closed deals
var from = new DateTime(2024, 1, 1); // Server time
var to = DateTime.UtcNow;

var history = await acc.OrderHistoryAsync(from, to);

double totalProfit = 0;
double totalCommission = 0;
double totalSwap = 0;

foreach (var item in history.HistoryData)
{
    if (item.HistoryDeal != null)
    {
        var deal = item.HistoryDeal;

        // Only count trading deals (Buy/Sell)
        if (deal.Type == BMT5_ENUM_DEAL_TYPE.Bmt5DealTypeBuy ||
            deal.Type == BMT5_ENUM_DEAL_TYPE.Bmt5DealTypeSell)
        {
            totalProfit += deal.Profit;
            totalCommission += deal.Commission;
            totalSwap += deal.Swap;
        }
    }
}

double netPL = totalProfit - totalCommission + totalSwap;

Console.WriteLine($"Realized Profit:  {totalProfit:F2}");
Console.WriteLine($"Total Commission: {totalCommission:F2}");
Console.WriteLine($"Total Swap:       {totalSwap:F2}");
Console.WriteLine($"Net P&L:          {netPL:F2}");
```

### 3) List closed positions

```csharp
// Show closed trades
var history = await acc.OrderHistoryAsync(
    from: DateTime.UtcNow.AddDays(-30),
    to: DateTime.UtcNow,
    sortMode: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc);

Console.WriteLine("=== CLOSED POSITIONS (Last 30 Days) ===");
Console.WriteLine("Date                Symbol      Type    Volume   Price      Profit");
Console.WriteLine("────────────────────────────────────────────────────────────────────");

foreach (var item in history.HistoryData)
{
    if (item.HistoryDeal != null)
    {
        var deal = item.HistoryDeal;

        if (deal.Type == BMT5_ENUM_DEAL_TYPE.Bmt5DealTypeBuy ||
            deal.Type == BMT5_ENUM_DEAL_TYPE.Bmt5DealTypeSell)
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(deal.Time.Seconds).DateTime;
            var type = deal.Type == BMT5_ENUM_DEAL_TYPE.Bmt5DealTypeBuy ? "BUY " : "SELL";

            Console.WriteLine($"{time:yyyy-MM-dd HH:mm}  {deal.Symbol,-10}  {type}  {deal.Volume,6:F2}  {deal.Price,8:F5}  {deal.Profit,8:F2}");
        }
    }
}
```

### 4) Pagination example

```csharp
// Retrieve large history in pages
var from = DateTime.UtcNow.AddMonths(-6);
var to = DateTime.UtcNow;
var itemsPerPage = 100;
var currentPage = 0;
var totalProcessed = 0;

while (true)
{
    var history = await acc.OrderHistoryAsync(
        from: from,
        to: to,
        pageNumber: currentPage,
        itemsPerPage: itemsPerPage);

    Console.WriteLine($"Page {currentPage + 1}: {history.HistoryData.Count} items");

    if (history.HistoryData.Count == 0)
        break;

    // Process this page
    foreach (var item in history.HistoryData)
    {
        // ... process item ...
        totalProcessed++;
    }

    // Check if this was the last page
    if (history.HistoryData.Count < itemsPerPage)
        break;

    currentPage++;
}

Console.WriteLine($"Total processed: {totalProcessed} items");
```

### 5) Filter by EA magic number

```csharp
// Get history for specific EA
var eaMagic = 123456L;
var history = await acc.OrderHistoryAsync(
    from: DateTime.UtcNow.AddDays(-7),
    to: DateTime.UtcNow);

var eaOrders = history.HistoryData
    .Where(h => h.HistoryOrder != null && h.HistoryOrder.MagicNumber == eaMagic)
    .ToList();

Console.WriteLine($"Orders from EA {eaMagic}: {eaOrders.Count}");

foreach (var item in eaOrders)
{
    var order = item.HistoryOrder;
    var setupTime = DateTimeOffset.FromUnixTimeSeconds(order.SetupTime.Seconds).DateTime;
    var state = order.State.ToString().Replace("BMT5_ORDER_STATE_", "");

    Console.WriteLine($"  {setupTime:yyyy-MM-dd HH:mm} - Ticket {order.Ticket}: {order.Symbol} {state}");
}
```
