# MT5Service - History Convenience Methods

> 3 simplified history methods with sensible defaults for common scenarios

---

## 🎯 Why These Methods Exist

**Problem**: Getting order history in MT5Account requires many parameters:
```csharp
var history = await account.OrderHistoryAsync(
    from: DateTime.UtcNow.AddDays(-7),
    to: DateTime.UtcNow,
    sortMode: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc,
    pageNumber: 0,
    itemsPerPage: 100,
    deadline: null,
    cancellationToken: default);
```
**7 parameters** just to get last week's orders!

**Solution**: MT5Service provides simplified methods with defaults:
```csharp
var history = await service.GetRecentOrdersAsync(days: 7);
var today = await service.GetTodayOrdersAsync();
```

**Simple, clear intent!**

**Benefits:**

- ✅ Sensible defaults (no need to specify every parameter)
- ✅ Clear method names (`GetTodayOrders` vs manual date calculation)
- ✅ Less code for common scenarios
- ✅ Built-in boolean check for trading permissions

---

## 📋 All 3 History Methods

| Method | What It Does | Default Parameters |
|--------|-------------|-------------------|
| `GetRecentOrdersAsync(days, limit)` | Get orders from last N days | 7 days, 100 orders, sorted by close time desc |
| `GetTodayOrdersAsync()` | Get today's orders only | 1 day, 1000 orders, sorted by close time desc |
| `IsTradingAllowedAsync()` | Check if trading is enabled | Returns bool (1 = allowed) |

---

## 💡 Usage Examples

### Example 1: Get Recent Orders (Last 7 Days)

```csharp
// ❌ BEFORE (MT5Account) - 7 parameters:
var history = await account.OrderHistoryAsync(
    from: DateTime.UtcNow.AddDays(-7),
    to: DateTime.UtcNow,
    sortMode: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc,
    pageNumber: 0,
    itemsPerPage: 100,
    deadline: null,
    cancellationToken: default);

// ✅ AFTER (MT5Service) - 1 parameter:
var history = await service.GetRecentOrdersAsync(days: 7);

// Display results — each entry carries an order and/or an executed deal
foreach (var item in history.HistoryData)
{
    var deal = item.HistoryDeal;
    if (deal == null) continue; // skip entries without an executed deal

    Console.WriteLine($"[{deal.Time.ToDateTime():yyyy-MM-dd HH:mm:ss}] " +
                      $"{deal.Symbol} {deal.Type} " +
                      $"Vol: {deal.Volume} " +
                      $"P/L: ${deal.Profit:F2}");
}
```

**Code reduction: 86%** (7 parameters → 1 parameter)

---

### Example 2: Get Today's Orders

```csharp
// ❌ BEFORE (MT5Account):
var history = await account.OrderHistoryAsync(
    from: DateTime.UtcNow.Date,                // Start of today (UTC)
    to: DateTime.UtcNow,                       // Now
    sortMode: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc,
    pageNumber: 0,
    itemsPerPage: 1000,
    deadline: null,
    cancellationToken: default);

// ✅ AFTER (MT5Service):
var history = await service.GetTodayOrdersAsync();

Console.WriteLine($"Today's orders: {history.HistoryData.Count}");
```

**Readability:** Intent is crystal clear!

---

### Example 3: Custom Date Range (Still Available!)

```csharp
// You can still customize if needed:
var lastMonth = await service.GetRecentOrdersAsync(days: 30, limit: 500);
var lastWeek = await service.GetRecentOrdersAsync(days: 7, limit: 100);
var yesterday = await service.GetRecentOrdersAsync(days: 1, limit: 50);

Console.WriteLine($"Last month: {lastMonth.HistoryData.Count} orders");
Console.WriteLine($"Last week:  {lastWeek.HistoryData.Count} orders");
Console.WriteLine($"Yesterday:  {yesterday.HistoryData.Count} orders");
```

**Flexibility:** Defaults work for 90% of cases, but customizable when needed!

---

### Example 4: Checking Trading Permissions

```csharp
// ❌ BEFORE (MT5Account) - unwrapping + comparison:
var allowedData = await account.AccountInfoIntegerAsync(
    AccountInfoIntegerPropertyType.AccountTradeAllowed);
bool tradingAllowed = allowedData.Value == 1;

if (!tradingAllowed)
{
    Console.WriteLine("❌ Trading not allowed on this account!");
    return;
}

// ✅ AFTER (MT5Service) - direct boolean:
bool tradingAllowed = await service.IsTradingAllowedAsync();

if (!tradingAllowed)
{
    Console.WriteLine("❌ Trading not allowed on this account!");
    return;
}

Console.WriteLine("✅ Trading is enabled");
```

**Simplicity:** Boolean check, no unwrapping, no comparison!

---

### Example 5: Daily Trading Report

```csharp
// Generate report of today's trading activity
var today = await service.GetTodayOrdersAsync();

double totalProfit = 0;
int winningTrades = 0;
int losingTrades = 0;

foreach (var item in today.HistoryData)
{
    var deal = item.HistoryDeal;
    if (deal == null) continue;

    totalProfit += deal.Profit;

    if (deal.Profit > 0)
        winningTrades++;
    else if (deal.Profit < 0)
        losingTrades++;
}

Console.WriteLine("═══════════════════════════════════");
Console.WriteLine("       Today's Trading Report      ");
Console.WriteLine("═══════════════════════════════════");
int totalTrades = winningTrades + losingTrades;
Console.WriteLine($"Total Trades:    {totalTrades}");
Console.WriteLine($"Winning Trades:  {winningTrades}");
Console.WriteLine($"Losing Trades:   {losingTrades}");
Console.WriteLine($"Win Rate:        {(totalTrades > 0 ? (double)winningTrades / totalTrades * 100 : 0):F1}%");
Console.WriteLine($"Total P/L:       ${totalProfit:F2}");
Console.WriteLine("═══════════════════════════════════");
```

---

### Example 6: Recent Performance Analysis

```csharp
// Analyze last 7 days performance by symbol
var recent = await service.GetRecentOrdersAsync(days: 7);

var symbolStats = recent.HistoryData
    .Where(x => x.HistoryDeal != null)
    .Select(x => x.HistoryDeal)
    .GroupBy(d => d.Symbol)
    .Select(g => new
    {
        Symbol = g.Key,
        Trades = g.Count(),
        TotalProfit = g.Sum(d => d.Profit),
        AvgProfit = g.Average(d => d.Profit),
        WinRate = g.Count(d => d.Profit > 0) / (double)g.Count() * 100
    })
    .OrderByDescending(s => s.TotalProfit);

Console.WriteLine("Last 7 Days - Performance by Symbol:");
Console.WriteLine("────────────────────────────────────────────────────────");
foreach (var stat in symbolStats)
{
    Console.WriteLine($"{stat.Symbol,-10} " +
                      $"Trades: {stat.Trades,3} " +
                      $"P/L: ${stat.TotalProfit,8:F2} " +
                      $"Avg: ${stat.AvgProfit,7:F2} " +
                      $"Win%: {stat.WinRate,5:F1}%");
}
```

---

### Example 7: Pre-Trade Check (Combined with Other Methods)

```csharp
// Complete pre-trade validation
public async Task<bool> CanTradeAsync()
{
    // 1. Check trading is enabled
    bool tradingAllowed = await service.IsTradingAllowedAsync();
    if (!tradingAllowed)
    {
        Console.WriteLine("❌ Trading not allowed on account");
        return false;
    }

    // 2. Check account has free margin
    double freeMargin = await service.GetFreeMarginAsync();
    if (freeMargin < 100.0)
    {
        Console.WriteLine($"❌ Insufficient free margin: ${freeMargin:F2}");
        return false;
    }

    // 3. Check daily loss limit
    var today = await service.GetTodayOrdersAsync();
    double todayProfit = today.HistoryData
        .Where(x => x.HistoryDeal != null)
        .Sum(x => x.HistoryDeal.Profit);
    if (todayProfit < -500.0)
    {
        Console.WriteLine($"❌ Daily loss limit reached: ${todayProfit:F2}");
        return false;
    }

    // 4. Check daily trade limit
    if (today.HistoryData.Count >= 50)
    {
        Console.WriteLine($"❌ Daily trade limit reached: {today.HistoryData.Count}");
        return false;
    }

    Console.WriteLine("✅ All checks passed, ready to trade!");
    return true;
}
```

---

### Example 8: Export Recent Orders to CSV

```csharp
// Export last 30 days to CSV file
var history = await service.GetRecentOrdersAsync(days: 30, limit: 1000);

using (var writer = new StreamWriter("trading_history.csv"))
{
    // Header
    writer.WriteLine("DealTime,Symbol,Type,Volume,Price,Profit,Comment");

    // Data rows
    foreach (var item in history.HistoryData)
    {
        var deal = item.HistoryDeal;
        if (deal == null) continue;

        writer.WriteLine($"{deal.Time.ToDateTime():yyyy-MM-dd HH:mm:ss}," +
                         $"{deal.Symbol}," +
                         $"{deal.Type}," +
                         $"{deal.Volume}," +
                         $"{deal.Price}," +
                         $"{deal.Profit}," +
                         $"\"{deal.Comment}\"");
    }
}

Console.WriteLine($"✅ Exported {history.HistoryData.Count} entries to trading_history.csv");
```

---

## 🔑 Key Benefits

| Aspect | MT5Account (Low-Level) | MT5Service (Convenience) |
|--------|----------------------|-------------------------|
| **Parameters** | 7 required parameters | 0-2 parameters with defaults |
| **Date calculation** | Manual `DateTime.UtcNow.AddDays(-7)` | Built-in `days: 7` |
| **Sort mode** | Must specify enum | ✅ Sensible default (close time desc) |
| **Page size** | Must specify | ✅ Sensible default (100/1000) |
| **Intent** | `OrderHistoryAsync(from, to, ...)` | `GetTodayOrdersAsync()` |
| **Trading check** | Integer unwrap + compare | ✅ Direct boolean |

---

## 📊 Method Signatures

```csharp
// Get recent orders
Task<OrdersHistoryData> GetRecentOrdersAsync(
    int days = 7,                         // Default: last 7 days
    int limit = 100,                      // Default: 100 orders
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Get today's orders
Task<OrdersHistoryData> GetTodayOrdersAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Check trading permission
Task<bool> IsTradingAllowedAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

---

## 📊 Code Reduction Statistics

| Task | Lines (MT5Account) | Lines (MT5Service) | Reduction |
|------|-------------------|--------------------|-----------|
| Get last 7 days | 7 parameters | 1 parameter | **86%** |
| Get today's orders | 7 parameters | 0 parameters | **100%** |
| Check trading allowed | 3 lines | 1 line | **67%** |

**Average reduction: 84%** for history operations!

---

## 🎓 When to Use

### ✅ Use MT5Service when:
- Need recent history (last N days)
- Want today's orders specifically
- Checking if trading is enabled
- Building reports/analytics
- Common scenarios (90% of cases)

### ⚠️ Use MT5Account when:
- Need exact date range (not relative days)
- Need custom sort order
- Need pagination (page numbers)
- Building advanced history management

---

## 💡 Implementation Details

### GetRecentOrdersAsync
```csharp
// What it does internally:
public Task<OrdersHistoryData> GetRecentOrdersAsync(
    int days = 7,
    int limit = 100,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
{
    return OrderHistoryAsync(
        from: DateTime.UtcNow.AddDays(-days),
        to: DateTime.UtcNow,
        sortMode: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc,
        pageNumber: 0,
        itemsPerPage: limit,
        deadline: deadline,
        cancellationToken: cancellationToken
    );
}
```

### GetTodayOrdersAsync
```csharp
// Simply calls GetRecentOrdersAsync with days: 1
public Task<OrdersHistoryData> GetTodayOrdersAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
    => GetRecentOrdersAsync(days: 1, limit: 1000, deadline, cancellationToken);
```

### IsTradingAllowedAsync
```csharp
// Checks AccountTradeAllowed and returns boolean
public async Task<bool> IsTradingAllowedAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
{
    var allowed = await AccountInfoIntegerAsync(
        AccountInfoIntegerPropertyType.AccountTradeAllowed,
        deadline,
        cancellationToken);
    return allowed == 1;
}
```

---

## ➕ History & Statistics Methods (new)

Pass-throughs that expose the full order history and the trade-statistics service at the mid-level. Signatures mirror the low-level calls — see the MT5Account pages for full field/enum tables.

| Method | Returns | Low-Level Equivalent |
|--------|---------|---------------------|
| `AllHistoryOrdersAsync(from, to, sort)` | `AllHistoryOrdersData` | [`MT5Account.AllHistoryOrdersAsync`](../MT5Account/3.%20Position_Orders_Information/AllHistoryOrders.md) |
| `TradeStatsAsync(since?)` | `StatsData` | [`MT5Account.TradeStatsAsync`](../MT5Account/8.%20Trade_Statistics/TradeStats.md) |
| `TradeStatsEquityHistoryAsync(since?, excludeSameBars?)` | `StatsWithChartsData` | [`MT5Account.TradeStatsEquityHistoryAsync`](../MT5Account/8.%20Trade_Statistics/TradeStatsEquityHistory.md) |
| `TradeUnrealizedPLAsync(since?)` | `TradeUnrealizedPLData` | [`MT5Account.TradeUnrealizedPLAsync`](../MT5Account/8.%20Trade_Statistics/TradeUnrealizedPL.md) |
| `EquityHistoryAsync(since?, excludeSameBars?)` | `EquityHistoryData` | [`MT5Account.EquityHistoryAsync`](../MT5Account/8.%20Trade_Statistics/EquityHistory.md) |

```csharp
// Full order history for a window (single call, no paging)
var all = await service.AllHistoryOrdersAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
Console.WriteLine($"Total orders: {all.ArrayTotal}");

// Performance analytics (trade-stats service; may be unavailable on some brokers)
var stats = await service.TradeStatsAsync();
Console.WriteLine($"Pips {stats.Pips:F1} | Lots {stats.Lots:F2} | Max equity DD {stats.MaxEquityDrawdownRelative:F2}%");

var equity = await service.EquityHistoryAsync(excludeSameBars: true);
Console.WriteLine($"Equity points: {equity.Points.Count}");
```

> 💡 `MT5Sugar` adds day-window wrappers: `AllHistoryOrdersLast(days)`, `TradeStatsLast(days?)`, `StatsWithChartsLast(days?)`, `UnrealizedExtremaLast(days?)` and `EquityCurveLast(days?)` (the last two return plain lists).

---

## 🔗 See Also

* **[MT5Service Overview](./MT5Service.Overview.md)** - Complete MT5Service improvements
* **[Account Convenience Methods](./Account_Convenience_Methods.md)** - Account shortcuts
* **[Symbol Convenience Methods](./Symbol_Convenience_Methods.md)** - Symbol shortcuts
* **[Trading Convenience Methods](./Trading_Convenience_Methods.md)** - Trading shortcuts
* **[MT5Account History](../MT5Account/MT5Account.Master.Overview.md)** - Low-level reference

---

## 💡 Summary

**3 history methods** with sensible defaults for common scenarios:

```csharp
// Get recent history - simple!
var lastWeek = await service.GetRecentOrdersAsync(days: 7);
var today = await service.GetTodayOrdersAsync();

// Check trading permission - boolean!
if (await service.IsTradingAllowedAsync())
{
    // Ready to trade!
}

// Custom if needed:
var lastMonth = await service.GetRecentOrdersAsync(days: 30, limit: 500);
```

**Sensible defaults, flexible when needed!** 🚀
