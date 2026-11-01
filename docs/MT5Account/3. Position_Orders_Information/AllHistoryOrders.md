# ✅ Full Order History (`AllHistoryOrdersAsync`)

> **Request:** Get the complete order history over a time range in a single call on **MT5** (no manual paging).

**API Information:**

* **SDK wrapper:** `MT5Account.AllHistoryOrdersAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.AccountHelper`
* **Proto definition:** `AllHistoryOrders` (defined in `mt5-term-api-account-helper.proto`)

### RPC

* **Service:** `mt5_term_api.AccountHelper`
* **Method:** `AllHistoryOrders(AllHistoryOrdersRequest) → AllHistoryOrdersReply`
* **Low‑level client (generated):** `AccountHelper.AccountHelperClient.AllHistoryOrders(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<AllHistoryOrdersData> AllHistoryOrdersAsync(
            DateTime from,
            DateTime to,
            BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sortMode = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`AllHistoryOrdersRequest { input_sort_mode, input_from, input_to }`


**Reply message:**

`AllHistoryOrdersReply { data: AllHistoryOrdersData }`

---

## 🔽 Input

| Parameter           | Type                                | Description                                               |
| ------------------- | ----------------------------------- | --------------------------------------------------------- |
| `from`              | `DateTime`                          | Window start (converted to UTC `Timestamp`)               |
| `to`                | `DateTime`                          | Window end (converted to UTC `Timestamp`)                 |
| `sortMode`          | `BMT5_ENUM_ORDER_HISTORY_SORT_TYPE` | Sort order (by open/close time or ticket)                 |
| `deadline`          | `DateTime?`                         | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`                 | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `AllHistoryOrdersData`

| Field          | Type                       | Description                               |
| -------------- | -------------------------- | ----------------------------------------- |
| `ArrayTotal`   | `int`                      | Total number of orders in the range       |
| `PageNumber`   | `int`                      | Page index (server‑reported)              |
| `ItemsPerPage` | `int`                      | Items per page (server‑reported)          |
| `HistoryOrders`| `repeated HistoryOrderInfo`| The order records                         |

### `HistoryOrderInfo`

| Field                | Type                        | Description                          |
| -------------------- | --------------------------- | ------------------------------------ |
| `RecordIndex`        | `int`                       | Index within the result set          |
| `Ticket`             | `ulong`                     | Order ticket                         |
| `TimeSetup`          | `Timestamp`                 | When the order was created           |
| `Type`               | `BMT5_ENUM_ORDER_TYPE`      | Order type (Buy/Sell/Limit/Stop/…)   |
| `State`              | `BMT5_ENUM_ORDER_STATE`     | Final order state (Filled, Canceled…)|
| `TimeDone`           | `Timestamp`                 | When the order finished              |
| `ExpirationTimeType` | `BMT5_ENUM_ORDER_TYPE_TIME` | Expiration mode (GTC, Day, …)        |
| `ExpirationTime`     | `Timestamp`                 | Expiration time (if any)             |
| `FillingType`        | `BMT5_ENUM_ORDER_TYPE_FILLING` | Filling policy (IOC/FOK/Return)   |
| `Magic`              | `long`                      | Expert Advisor magic number          |
| `PositionId`         | `long`                      | Position the order belongs to        |

---

## 🧱 Related enums (from proto)

### `BMT5_ENUM_ORDER_HISTORY_SORT_TYPE` (input `sortMode`)

* `Bmt5SortByOpenTimeAsc` — by open time, ascending
* `Bmt5SortByOpenTimeDesc` — by open time, descending
* `Bmt5SortByCloseTimeAsc` — by close time, ascending (**default**)
* `Bmt5SortByCloseTimeDesc` — by close time, descending
* `Bmt5SortByOrderTicketIdAsc` — by ticket id, ascending
* `Bmt5SortByOrderTicketIdDesc` — by ticket id, descending

### Output enums (`HistoryOrderInfo`)

* **`Type`** → `BMT5_ENUM_ORDER_TYPE` — order type (Buy/Sell/Limit/Stop/…). See [OrderHistory](./OrderHistory.md) for the full value list.
* **`State`** → `BMT5_ENUM_ORDER_STATE` — e.g. `Bmt5OrderStateFilled`, `Bmt5OrderStateCanceled`, `Bmt5OrderStateRejected`.
* **`ExpirationTimeType`** → `BMT5_ENUM_ORDER_TYPE_TIME` — GTC / Day / Specified.
* **`FillingType`** → `BMT5_ENUM_ORDER_TYPE_FILLING` — IOC / FOK / Return.

> These output enums share the same definitions used across [OrderHistory](./OrderHistory.md) and [OpenedOrders](./OpenedOrders.md).

---

## 💬 Just the essentials

* **What it is.** The whole order history for a window in one shot.
* **Why you need it.** Journals, audits, analytics without paging loops.
* **Compare with.** [`OrderHistory`](./OrderHistory.md) — same data, but paged; this returns the full set.

---

## 🎯 Purpose

Use it when you want everything at once:

* Export a trading journal for a period.
* Recompute statistics from raw orders.
* Reconcile algo decisions against what actually executed.

---

## 🧩 Notes & Tips

* **UTC times:** `from`/`to` are converted to UTC; `TimeSetup`/`TimeDone` come back as UTC `Timestamp` — use `.ToDateTime()`.
* **Large windows:** For very long ranges prefer paged [`OrderHistory`](./OrderHistory.md) to cap memory.
* **Orders vs deals:** This is **orders**; for closed positions/deals use [`PositionsHistory`](./PositionsHistory.md).
* **Friendly wrapper:** `MT5Sugar.AllHistoryOrdersLast(days)` computes the window for you.

---

## 🔗 Usage Examples

### 1) All orders from the last 7 days

```csharp
// acc — connected MT5Account
var from = DateTime.UtcNow.AddDays(-7);
var to   = DateTime.UtcNow;

var data = await acc.AllHistoryOrdersAsync(from, to);
Console.WriteLine($"Total: {data.ArrayTotal}, returned {data.HistoryOrders.Count}");

foreach (var o in data.HistoryOrders.Take(5))
    Console.WriteLine($"#{o.Ticket} {o.Type} {o.State} @ {o.TimeSetup.ToDateTime():u}");
```

---

### 2) Count filled vs canceled

```csharp
var data = await acc.AllHistoryOrdersAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
int filled   = data.HistoryOrders.Count(o => o.State == BMT5_ENUM_ORDER_STATE.Bmt5OrderStateFilled);
int canceled = data.HistoryOrders.Count(o => o.State == BMT5_ENUM_ORDER_STATE.Bmt5OrderStateCanceled);
Console.WriteLine($"Filled: {filled}, Canceled: {canceled}");
```
