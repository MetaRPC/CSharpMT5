# ✅ Full Order History for N Days (`AllHistoryOrdersLast`)

> **Sugar method:** Gets the complete order history for the last N days in one call. Computes the date window automatically.

**API Information:**

* **Extension method:** `MT5Service.AllHistoryOrdersLast(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Underlying calls:** `AllHistoryOrdersAsync()`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static Task<AllHistoryOrdersData> AllHistoryOrdersLast(
        this MT5Service svc,
        int days = 7,
        BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sort = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
        int timeoutSec = 20,
        CancellationToken ct = default);
}
```

---

## 🔽 Input

| Parameter    | Type                                | Description                                   |
| ------------ | ----------------------------------- | --------------------------------------------- |
| `svc`        | `MT5Service`                        | MT5Service instance (extension method)        |
| `days`       | `int`                               | Days to look back from now (default: 7)       |
| `sort`       | `BMT5_ENUM_ORDER_HISTORY_SORT_TYPE` | Sort order (default: by close time ascending) |
| `timeoutSec` | `int`                               | Timeout in seconds (default: 20)              |
| `ct`         | `CancellationToken`                 | Cancellation token                            |

---

## ⬆️ Output — `AllHistoryOrdersData`

| Field          | Type                        | Description                     |
| -------------- | --------------------------- | ------------------------------- |
| `ArrayTotal`   | `int`                       | Total orders in the range       |
| `HistoryOrders`| `repeated HistoryOrderInfo` | The order records               |

> Full field/enum tables: [MT5Account · AllHistoryOrders](../../MT5Account/3.%20Position_Orders_Information/AllHistoryOrders.md).

---

## 💬 Just the essentials

* **What it is.** The whole order history for the last N days, no paging.
* **Why you need it.** Journals/analytics over a window without manual date math.
* **Difference from `OrdersHistoryLast`.** That one pages; this returns the full set in one call.

---

## 🎯 Purpose

* Export a trading journal for the last week/month.
* Recompute statistics from raw orders.
* Reconcile algo decisions against executed orders.

---

## 🧩 Notes & Tips

* **Auto window** — `from = now - days`, `to = now` (UTC).
* **Large ranges** — for very long windows prefer paged `OrdersHistoryLast` to cap memory.
* **Orders vs deals** — this is **orders**; for closed positions use `PositionsHistoryPaged`.

---

## 🔧 Under the Hood

```csharp
var to = DateTime.UtcNow;
return svc.AllHistoryOrdersAsync(to.AddDays(-days), to, sort, Dl(timeoutSec), ct);
```

---

## 🔗 Usage Examples

### 1) Last 7 days

```csharp
// svc — MT5Service instance
var all = await svc.AllHistoryOrdersLast(days: 7);
Console.WriteLine($"Total: {all.ArrayTotal}, returned {all.HistoryOrders.Count}");
```

### 2) Count filled vs canceled (last 30 days)

```csharp
var all = await svc.AllHistoryOrdersLast(days: 30);
int filled   = all.HistoryOrders.Count(o => o.State == BMT5_ENUM_ORDER_STATE.Bmt5OrderStateFilled);
int canceled = all.HistoryOrders.Count(o => o.State == BMT5_ENUM_ORDER_STATE.Bmt5OrderStateCanceled);
Console.WriteLine($"Filled: {filled}, Canceled: {canceled}");
```

### 3) Newest first

```csharp
var all = await svc.AllHistoryOrdersLast(days: 7, sort: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc);
foreach (var o in all.HistoryOrders.Take(10))
    Console.WriteLine($"#{o.Ticket} {o.Type} {o.State} @ {o.TimeSetup.ToDateTime():u}");
```

---

## 🔗 Related Methods

* **Low-level:** [`AllHistoryOrdersAsync()`](../../MT5Account/3.%20Position_Orders_Information/AllHistoryOrders.md)
* **Sugar:** `OrdersHistoryLast()` (paged orders), `PositionsHistoryPaged()` (closed positions)
