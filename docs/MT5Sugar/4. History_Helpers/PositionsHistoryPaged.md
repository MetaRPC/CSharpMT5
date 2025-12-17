# ✅ Get Positions History with Paging (`PositionsHistoryPaged`)

> **Sugar method:** Gets closed positions history with optional open date filter and paging. Direct wrapper with default parameters.

**API Information:**

* **Extension method:** `MT5Service.PositionsHistoryPaged(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Underlying calls:** `PositionsHistoryAsync()`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static Task<PositionsHistoryData> PositionsHistoryPaged(
        this MT5Service svc,
        AH_ENUM_POSITIONS_HISTORY_SORT_TYPE sort = AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeAsc,
        DateTime? openFrom = null,
        DateTime? openTo = null,
        int page = 0,
        int size = 100,
        int timeoutSec = 20,
        CancellationToken ct = default);
}
```

---

## 🔽 Input

| Parameter    | Type                                      | Description                                     |
| ------------ | ----------------------------------------- | ----------------------------------------------- |
| `svc`        | `MT5Service`                              | MT5Service instance (extension method)          |
| `sort`       | `AH_ENUM_POSITIONS_HISTORY_SORT_TYPE`     | Sort order (default: by open time ascending)    |
| `openFrom`   | `DateTime?`                               | Filter by open time from (optional)             |
| `openTo`     | `DateTime?`                               | Filter by open time to (optional)               |
| `page`       | `int`                                     | Page number for pagination (default: 0)         |
| `size`       | `int`                                     | Page size (default: 100)                        |
| `timeoutSec` | `int`                                     | Timeout in seconds (default: 20)                |
| `ct`         | `CancellationToken`                       | Cancellation token                              |

---

## ⬆️ Output — `PositionsHistoryData`

Returns closed positions with pagination.

| Field          | Type                        | Description                           |
| -------------- | --------------------------- | ------------------------------------- |
| `Positions`    | `List<PositionHistoryInfo>` | List of closed positions              |
| `TotalCount`   | `int`                       | Total number of matching positions    |

---

## 💬 Just the essentials

* **What it is.** Simple wrapper for getting closed positions history with sensible defaults and optional date filtering.
* **Why you need it.** Query position history with pagination - easier than OrdersHistoryAsync (position vs order).
* **Sanity check.** Returns paged results. Check `TotalCount` to see if you need more pages. Positions are different from orders.

---

## 🎯 Purpose

Use it for position history analysis:

* View all closed positions.
* Filter positions by open date range.
* Build performance reports.
* Calculate statistics per position (not per order).
* Export closed positions data.

---

## 🧩 Notes & Tips

* **Positions vs Orders** - position can have multiple orders (entry, partial closes, full close)
* **Optional filtering** - `openFrom`/`openTo` are optional (null = no filter)
* **Paging** - use `page` and `size` for large datasets
* **Default sort** - by open time ascending (oldest first)
* **Longer timeout** - default 20 seconds (history queries can be slow)
* **UTC dates** - all DateTime parameters should be UTC

---

## 🔧 Under the Hood

This sugar method is a simple wrapper with default parameters:

```csharp
var deadline = DateTime.UtcNow.AddSeconds(timeoutSec);

// Direct call to low-level method
return await svc.PositionsHistoryAsync(
    sort,
    openFrom,
    openTo,
    page,
    size,
    deadline,
    ct);
```

**What it improves:**

* **Named method** - clearer than generic "PositionsHistoryAsync"
* **Default parameters** - sensible defaults for common cases
* **Auto deadline** - converts timeout to deadline
* **Clear intent** - "paged" indicates pagination support

---

## 📊 Low-Level Alternative

**WITHOUT sugar:**
```csharp
var deadline = DateTime.UtcNow.AddSeconds(20);

var history = await svc.PositionsHistoryAsync(
    AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeAsc,
    openFrom: null,
    openTo: null,
    page: 0,
    size: 100,
    deadline,
    ct);
```

**WITH sugar:**
```csharp
var history = await svc.PositionsHistoryPaged();
```

**Benefits:**

* ✅ **Shorter call**
* ✅ **Default parameters**
* ✅ **Auto deadline**
* ✅ **Clearer name**

---

## 🔗 Usage Examples

### 1) Get all closed positions (first page)

```csharp
// svc — MT5Service instance

var history = await svc.PositionsHistoryPaged();

Console.WriteLine($"Closed positions: {history.TotalCount}");
Console.WriteLine($"Retrieved: {history.Positions.Count}");

foreach (var pos in history.Positions)
{
    Console.WriteLine($"  Ticket: {pos.Ticket}, Symbol: {pos.Symbol}, " +
                     $"Profit: {pos.Profit:F2}");
}
```

---

### 2) Get positions opened in specific date range

```csharp
var from = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
var to = new DateTime(2024, 1, 31, 23, 59, 59, DateTimeKind.Utc);

var history = await svc.PositionsHistoryPaged(
    openFrom: from,
    openTo: to);

Console.WriteLine($"Positions opened in January 2024: {history.TotalCount}");
```

---

### 3) Paginate through all positions

```csharp
int pageSize = 100;
int page = 0;

var allPositions = new List<PositionHistoryInfo>();

while (true)
{
    var history = await svc.PositionsHistoryPaged(page: page, size: pageSize);

    allPositions.AddRange(history.Positions);

    Console.WriteLine($"Page {page}: {history.Positions.Count} positions");

    if (history.Positions.Count < pageSize)
        break; // Last page

    page++;
}

Console.WriteLine($"Total positions retrieved: {allPositions.Count}");
```

---

### 4) Sort by close time descending (newest closed first)

```csharp
var history = await svc.PositionsHistoryPaged(
    sort: AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionCloseTimeDesc);

Console.WriteLine("Recent closed positions (newest first):");

foreach (var pos in history.Positions.Take(10))
{
    Console.WriteLine($"  Closed: {pos.CloseTime}, {pos.Symbol}, " +
                     $"Profit: {pos.Profit:F2}");
}
```

---

### 5) Calculate total profit and statistics

```csharp
var history = await svc.PositionsHistoryPaged();

if (history.Positions.Any())
{
    double totalProfit = history.Positions.Sum(p => p.Profit);
    int winners = history.Positions.Count(p => p.Profit > 0);
    int losers = history.Positions.Count(p => p.Profit < 0);
    double winRate = (double)winners / history.Positions.Count * 100;

    Console.WriteLine("Position Statistics:");
    Console.WriteLine($"  Total positions: {history.Positions.Count}");
    Console.WriteLine($"  Winners:         {winners} ({winRate:F1}%)");
    Console.WriteLine($"  Losers:          {losers}");
    Console.WriteLine($"  Total profit:    {totalProfit:F2}");
}
```

---

### 6) Group by symbol

```csharp
var history = await svc.PositionsHistoryPaged();

var bySymbol = history.Positions
    .GroupBy(p => p.Symbol)
    .Select(g => new
    {
        Symbol = g.Key,
        Count = g.Count(),
        TotalProfit = g.Sum(p => p.Profit),
        AvgProfit = g.Average(p => p.Profit)
    })
    .OrderByDescending(x => x.TotalProfit);

Console.WriteLine("Performance by symbol:");
Console.WriteLine("─────────────────────────────────────────────");

foreach (var item in bySymbol)
{
    Console.WriteLine($"{item.Symbol,-10} {item.Count,3} pos  " +
                     $"Total: {item.TotalProfit,8:F2}  Avg: {item.AvgProfit,7:F2}");
}
```

---

### 7) Find longest holding period

```csharp
var history = await svc.PositionsHistoryPaged();

if (history.Positions.Any())
{
    var longest = history.Positions
        .OrderByDescending(p => (p.CloseTime - p.OpenTime).TotalHours)
        .First();

    var duration = longest.CloseTime - longest.OpenTime;

    Console.WriteLine("Longest held position:");
    Console.WriteLine($"  Symbol: {longest.Symbol}");
    Console.WriteLine($"  Ticket: {longest.Ticket}");
    Console.WriteLine($"  Duration: {duration.TotalDays:F1} days");
    Console.WriteLine($"  Profit: {longest.Profit:F2}");
}
```

---

### 8) Export to CSV

```csharp
var history = await svc.PositionsHistoryPaged();

var csv = new StringBuilder();
csv.AppendLine("Ticket,Symbol,Type,OpenTime,CloseTime,Volume,OpenPrice,ClosePrice,Profit,Duration");

foreach (var pos in history.Positions)
{
    var duration = (pos.CloseTime - pos.OpenTime).TotalHours;

    csv.AppendLine($"{pos.Ticket},{pos.Symbol},{pos.Type}," +
                  $"{pos.OpenTime:yyyy-MM-dd HH:mm:ss}," +
                  $"{pos.CloseTime:yyyy-MM-dd HH:mm:ss}," +
                  $"{pos.Volume},{pos.OpenPrice},{pos.ClosePrice}," +
                  $"{pos.Profit:F2},{duration:F1}");
}

File.WriteAllText("positions_history.csv", csv.ToString());
Console.WriteLine("Exported to positions_history.csv");
```

---

### 9) Calculate average holding time

```csharp
var history = await svc.PositionsHistoryPaged();

if (history.Positions.Any())
{
    var avgHoldingTime = history.Positions
        .Average(p => (p.CloseTime - p.OpenTime).TotalHours);

    Console.WriteLine($"Average holding time: {avgHoldingTime:F1} hours " +
                     $"({avgHoldingTime / 24:F1} days)");

    // By symbol
    var bySymbol = history.Positions
        .GroupBy(p => p.Symbol)
        .Select(g => new
        {
            Symbol = g.Key,
            AvgHours = g.Average(p => (p.CloseTime - p.OpenTime).TotalHours)
        })
        .OrderBy(x => x.Symbol);

    Console.WriteLine("\nAverage holding time by symbol:");
    foreach (var item in bySymbol)
    {
        Console.WriteLine($"  {item.Symbol,-10} {item.AvgHours,6:F1} hours");
    }
}
```

---

### 10) Filter positions opened last month

```csharp
var now = DateTime.UtcNow;
var lastMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
    .AddMonths(-1);
var lastMonthEnd = lastMonthStart.AddMonths(1).AddSeconds(-1);

var history = await svc.PositionsHistoryPaged(
    openFrom: lastMonthStart,
    openTo: lastMonthEnd);

Console.WriteLine($"Positions opened last month: {history.TotalCount}");

if (history.Positions.Any())
{
    double monthProfit = history.Positions.Sum(p => p.Profit);
    Console.WriteLine($"Total profit: {monthProfit:F2}");
}
```

---

## 🔗 Related Methods

**📦 Low-level method used internally:**

* `PositionsHistoryAsync()` - Get positions history (this is what PositionsHistoryPaged wraps)

**🍬 Other sugar methods:**

* `OrdersHistoryLast()` - Get closed orders (not positions) for last N days
* `GetAccountSnapshot()` - Get current open positions (not history)

---

## ⚠️ Common Pitfalls

1. **Confusing positions with orders**
   ```csharp
   // ❌ WRONG - positions are not orders
   // Position = one trade (can have multiple orders)
   // Order = individual operation (entry, partial close, etc)

   var history = await svc.PositionsHistoryPaged();
   // This returns positions, not orders!

   // ✅ CORRECT - use appropriate method
   var positions = await svc.PositionsHistoryPaged(); // Positions
   var orders = await svc.OrdersHistoryLast(days: 7); // Orders
   ```

2. **Not handling pagination**
   ```csharp
   // ❌ WRONG - only gets first 100 positions
   var history = await svc.PositionsHistoryPaged();
   if (history.TotalCount > 100) {
       // You're missing data!
   }

   // ✅ CORRECT - paginate through all
   var allPositions = new List<PositionHistoryInfo>();
   int page = 0;
   while (true)
   {
       var history = await svc.PositionsHistoryPaged(page: page);
       allPositions.AddRange(history.Positions);
       if (history.Positions.Count < 100) break;
       page++;
   }
   ```

3. **Using local time for filtering**
   ```csharp
   // ❌ WRONG - using local time
   var from = DateTime.Now.AddDays(-30); // Local time!

   // ✅ CORRECT - use UTC
   var from = DateTime.UtcNow.AddDays(-30);
   var history = await svc.PositionsHistoryPaged(openFrom: from);
   ```

4. **Ignoring TotalCount**
   ```csharp
   // ❌ WRONG - assuming you got all data
   var history = await svc.PositionsHistoryPaged();
   Console.WriteLine($"Total: {history.Positions.Count}"); // Wrong!

   // ✅ CORRECT - check TotalCount
   var history = await svc.PositionsHistoryPaged();
   Console.WriteLine($"Retrieved: {history.Positions.Count}");
   Console.WriteLine($"Total in DB: {history.TotalCount}");
   ```

5. **Wrong date range specification**
   ```csharp
   // ❌ WRONG - unspecified DateTimeKind
   var from = new DateTime(2024, 1, 1); // Kind = Unspecified!

   // ✅ CORRECT - explicit UTC
   var from = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
   var history = await svc.PositionsHistoryPaged(openFrom: from);
   ```

6. **Not filtering when you should**
   ```csharp
   // ❌ WRONG - fetching all positions when you only need recent
   var history = await svc.PositionsHistoryPaged();
   var recent = history.Positions
       .Where(p => p.OpenTime >= DateTime.UtcNow.AddDays(-7))
       .ToList();

   // ✅ CORRECT - filter on server side
   var from = DateTime.UtcNow.AddDays(-7);
   var history = await svc.PositionsHistoryPaged(openFrom: from);
   ```
