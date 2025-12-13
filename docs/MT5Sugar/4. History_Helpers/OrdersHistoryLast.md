# ✅ Get Recent Orders History (`OrdersHistoryLast`)

> **Sugar method:** Gets closed orders for the last N days with paging. Simple wrapper that calculates date range automatically.

**API Information:**

* **Extension method:** `MT5Service.OrdersHistoryLast(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Underlying calls:** `OrderHistoryAsync()`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static Task<OrdersHistoryData> OrdersHistoryLast(
        this MT5Service svc,
        int days = 7,
        int page = 0,
        int size = 100,
        BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sort = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
        int timeoutSec = 20,
        CancellationToken ct = default);
}
```

---

## 🔽 Input

| Parameter    | Type                                   | Description                                     |
| ------------ | -------------------------------------- | ----------------------------------------------- |
| `svc`        | `MT5Service`                           | MT5Service instance (extension method)          |
| `days`       | `int`                                  | Number of days to look back (default: 7)        |
| `page`       | `int`                                  | Page number for pagination (default: 0)         |
| `size`       | `int`                                  | Page size (default: 100)                        |
| `sort`       | `BMT5_ENUM_ORDER_HISTORY_SORT_TYPE`    | Sort order (default: by close time ascending)   |
| `timeoutSec` | `int`                                  | Timeout in seconds (default: 20)                |
| `ct`         | `CancellationToken`                    | Cancellation token                              |

---

## ⬆️ Output — `OrdersHistoryData`

Returns closed orders from the last N days.

| Field          | Type                  | Description                           |
| -------------- | --------------------- | ------------------------------------- |
| `Orders`       | `List<OrderHistoryInfo>` | List of closed orders              |
| `TotalCount`   | `int`                 | Total number of matching orders       |

---

## 💬 Just the essentials

* **What it is.** Simple way to get recent closed orders without manually calculating date ranges.
* **Why you need it.** Quickly fetch last week/month of trading history for analysis, reporting, or statistics.
* **Sanity check.** Returns paged results. Check `TotalCount` to see if you need to fetch more pages.

---

## 🎯 Purpose

Use it for history analysis:

* View last week's trading activity.
* Build trading journals.
* Calculate performance statistics.
* Analyze closed trades.
* Export trading history.

---

## 🧩 Notes & Tips

* **Auto date range** - calculates `from` and `to` automatically from current UTC time
* **Paging** - use `page` and `size` for large result sets
* **Default period** - 7 days (last week)
* **Sort options** - by close time (asc/desc), open time, or ticket
* **Longer timeout** - default 20 seconds (history queries can be slow)
* **UTC times** - all dates are in UTC

---

## 🔧 Under the Hood

This sugar method simplifies date range calculation:

```csharp
// Calculate date range automatically
var to = DateTime.UtcNow;                // Now
var from = to.AddDays(-days);            // N days ago

var deadline = DateTime.UtcNow.AddSeconds(timeoutSec);

// Call low-level method with calculated range
return await svc.OrderHistoryAsync(from, to, sort, page, size, deadline, ct);
```

**What it improves:**

* **Auto date calculation** - no manual `DateTime.UtcNow.AddDays(-7)`
* **Simpler API** - just specify number of days
* **Default parameters** - sensible defaults for common use cases
* **Auto deadline** - converts timeout to deadline

---

## 📊 Low-Level Alternative

**WITHOUT sugar:**
```csharp
var to = DateTime.UtcNow;
var from = to.AddDays(-7);
var deadline = DateTime.UtcNow.AddSeconds(20);

var history = await svc.OrderHistoryAsync(
    from,
    to,
    BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
    page: 0,
    size: 100,
    deadline,
    ct);
```

**WITH sugar:**
```csharp
var history = await svc.OrdersHistoryLast(days: 7);
```

**Benefits:**

* ✅ **4 lines → 1 line**
* ✅ **No manual date calculation**
* ✅ **Default parameters**
* ✅ **Clearer intent**

---

## 🔗 Usage Examples

### 1) Get last week's orders

```csharp
// svc — MT5Service instance

var history = await svc.OrdersHistoryLast(days: 7);

Console.WriteLine($"Closed orders (last 7 days): {history.TotalCount}");

foreach (var order in history.Orders)
{
    Console.WriteLine($"  Ticket: {order.Ticket}, Symbol: {order.Symbol}, " +
                     $"Profit: {order.Profit:F2}");
}
```

---

### 2) Get last month's orders

```csharp
var history = await svc.OrdersHistoryLast(days: 30);

Console.WriteLine($"Orders in last 30 days: {history.TotalCount}");
Console.WriteLine($"Retrieved: {history.Orders.Count}");
```

---

### 3) Paginate through all orders

```csharp
int days = 7;
int pageSize = 100;
int page = 0;

var allOrders = new List<OrderHistoryInfo>();

while (true)
{
    var history = await svc.OrdersHistoryLast(days, page, pageSize);

    allOrders.AddRange(history.Orders);

    Console.WriteLine($"Page {page}: {history.Orders.Count} orders");

    if (history.Orders.Count < pageSize)
        break; // Last page

    page++;
}

Console.WriteLine($"Total orders retrieved: {allOrders.Count}");
```

---

### 4) Sort by close time descending (newest first)

```csharp
var history = await svc.OrdersHistoryLast(
    days: 7,
    sort: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc);

Console.WriteLine("Recent orders (newest first):");

foreach (var order in history.Orders.Take(10))
{
    Console.WriteLine($"  {order.CloseTime}: {order.Symbol} " +
                     $"Profit: {order.Profit:F2}");
}
```

---

### 5) Calculate total profit

```csharp
var history = await svc.OrdersHistoryLast(days: 7);

double totalProfit = history.Orders.Sum(o => o.Profit);
int winningTrades = history.Orders.Count(o => o.Profit > 0);
int losingTrades = history.Orders.Count(o => o.Profit < 0);

Console.WriteLine($"Last 7 days statistics:");
Console.WriteLine($"  Total orders:   {history.Orders.Count}");
Console.WriteLine($"  Winning trades: {winningTrades}");
Console.WriteLine($"  Losing trades:  {losingTrades}");
Console.WriteLine($"  Total profit:   {totalProfit:F2}");
```

---

### 6) Filter by symbol

```csharp
var history = await svc.OrdersHistoryLast(days: 30);

var eurusdOrders = history.Orders.Where(o => o.Symbol == "EURUSD").ToList();

Console.WriteLine($"EURUSD orders: {eurusdOrders.Count}");

double eurusdProfit = eurusdOrders.Sum(o => o.Profit);
Console.WriteLine($"EURUSD profit: {eurusdProfit:F2}");
```

---

### 7) Export to CSV

```csharp
var history = await svc.OrdersHistoryLast(days: 30);

var csv = new StringBuilder();
csv.AppendLine("Ticket,Symbol,Type,OpenTime,CloseTime,Volume,OpenPrice,ClosePrice,Profit");

foreach (var order in history.Orders)
{
    csv.AppendLine($"{order.Ticket},{order.Symbol},{order.Type}," +
                  $"{order.OpenTime},{order.CloseTime},{order.Volume}," +
                  $"{order.OpenPrice},{order.ClosePrice},{order.Profit}");
}

File.WriteAllText("orders_history.csv", csv.ToString());
Console.WriteLine("Exported to orders_history.csv");
```

---

### 8) Find best and worst trades

```csharp
var history = await svc.OrdersHistoryLast(days: 30);

if (history.Orders.Any())
{
    var bestTrade = history.Orders.OrderByDescending(o => o.Profit).First();
    var worstTrade = history.Orders.OrderBy(o => o.Profit).First();

    Console.WriteLine("Best trade:");
    Console.WriteLine($"  {bestTrade.Symbol} ticket {bestTrade.Ticket}: " +
                     $"+{bestTrade.Profit:F2}");

    Console.WriteLine("Worst trade:");
    Console.WriteLine($"  {worstTrade.Symbol} ticket {worstTrade.Ticket}: " +
                     $"{worstTrade.Profit:F2}");
}
```

---

### 9) Group by symbol

```csharp
var history = await svc.OrdersHistoryLast(days: 30);

var bySymbol = history.Orders
    .GroupBy(o => o.Symbol)
    .Select(g => new
    {
        Symbol = g.Key,
        Count = g.Count(),
        TotalProfit = g.Sum(o => o.Profit)
    })
    .OrderByDescending(x => x.TotalProfit);

Console.WriteLine("Performance by symbol:");
Console.WriteLine("─────────────────────────────────");

foreach (var item in bySymbol)
{
    Console.WriteLine($"{item.Symbol,-10} {item.Count,3} trades  " +
                     $"Profit: {item.TotalProfit,8:F2}");
}
```

---

### 10) Calculate win rate

```csharp
var history = await svc.OrdersHistoryLast(days: 30);

if (history.Orders.Any())
{
    int total = history.Orders.Count;
    int winning = history.Orders.Count(o => o.Profit > 0);
    int losing = history.Orders.Count(o => o.Profit < 0);
    int breakeven = history.Orders.Count(o => o.Profit == 0);

    double winRate = (double)winning / total * 100;

    Console.WriteLine("Trading Statistics (last 30 days):");
    Console.WriteLine($"  Total trades: {total}");
    Console.WriteLine($"  Winning:      {winning} ({winRate:F1}%)");
    Console.WriteLine($"  Losing:       {losing} ({(double)losing / total * 100:F1}%)");
    Console.WriteLine($"  Breakeven:    {breakeven}");
}
```

---

## 🔗 Related Methods

**📦 Low-level method used internally:**

* `OrderHistoryAsync()` - Get orders history with explicit date range

**🍬 Other sugar methods:**

* `PositionsHistoryPaged()` - Get positions history (closed positions, not orders)
* `GetAccountSnapshot()` - Get current account state (not history)

---

## ⚠️ Common Pitfalls

1. **Not handling pagination**
   ```csharp
   // ❌ WRONG - only gets first 100 orders
   var history = await svc.OrdersHistoryLast(days: 365);
   // If you have 500 orders, you only got 100!

   // ✅ CORRECT - paginate through all
   var allOrders = new List<OrderHistoryInfo>();
   int page = 0;
   while (true)
   {
       var history = await svc.OrdersHistoryLast(365, page, 100);
       allOrders.AddRange(history.Orders);
       if (history.Orders.Count < 100) break;
       page++;
   }
   ```

2. **Confusing orders with positions**
   ```csharp
   // ❌ WRONG - orders are not the same as positions
   var orders = await svc.OrdersHistoryLast(days: 7);
   // This returns order history, not position history

   // ✅ CORRECT - use correct method for positions
   var positions = await svc.PositionsHistoryPaged();
   ```

3. **Ignoring TotalCount**
   ```csharp
   // ❌ WRONG - not checking if there's more data
   var history = await svc.OrdersHistoryLast(days: 30);
   Console.WriteLine($"Total: {history.Orders.Count}"); // Wrong!

   // ✅ CORRECT - check TotalCount
   var history = await svc.OrdersHistoryLast(days: 30);
   Console.WriteLine($"Retrieved: {history.Orders.Count}");
   Console.WriteLine($"Total: {history.TotalCount}");
   ```

4. **Using local time instead of UTC**
   ```csharp
   // ❌ WRONG - comparing with local time
   var history = await svc.OrdersHistoryLast(days: 1);
   var today = history.Orders.Where(o => o.CloseTime.Date == DateTime.Now.Date);

   // ✅ CORRECT - use UTC
   var history = await svc.OrdersHistoryLast(days: 1);
   var today = history.Orders.Where(o => o.CloseTime.Date == DateTime.UtcNow.Date);
   ```

5. **Not specifying enough timeout for large queries**
   ```csharp
   // ❌ WRONG - default timeout might not be enough
   var history = await svc.OrdersHistoryLast(days: 365);

   // ✅ CORRECT - use longer timeout for large queries
   var history = await svc.OrdersHistoryLast(days: 365, timeoutSec: 60);
   ```
