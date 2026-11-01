# ✅ Getting Positions History

> **Request:** historical closed positions from **MT5**. Get aggregated view of closed positions (not individual orders/deals) with P&L summary.

**API Information:**

* **SDK wrapper:** `MT5Account.PositionsHistoryAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.AccountHelper`
* **Proto definition:** `PositionsHistory` (defined in `mt5-term-api-account-helper.proto`)

### RPC

* **Service:** `mt5_term_api.AccountHelper`
* **Method:** `PositionsHistory(PositionsHistoryRequest) → PositionsHistoryReply`
* **Low‑level client (generated):** `AccountHelper.PositionsHistory(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<PositionsHistoryData> PositionsHistoryAsync(
            AH_ENUM_POSITIONS_HISTORY_SORT_TYPE sortType,
            DateTime? openFrom = null,
            DateTime? openTo = null,
            int page = 0,
            int size = 0,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`PositionsHistoryRequest { sort_type: enum, position_open_time_from: Timestamp?, position_open_time_to: Timestamp?, page_number: int32?, items_per_page: int32? }`


**Reply message:**

`PositionsHistoryReply { data: PositionsHistoryData }` or `{ error: Error }`

---

## 🔽 Input

| Parameter                | Type                                       | Required | Description                                                     |
| ------------------------ | ------------------------------------------ | -------- | --------------------------------------------------------------- |
| `sortType`               | `AH_ENUM_POSITIONS_HISTORY_SORT_TYPE`      | ❌       | Sort order for results (default: by open time ascending)       |
| `openFrom`   | `DateTime?`                                | ❌       | Filter: minimum position open time (**server time**)           |
| `openTo`     | `DateTime?`                                | ❌       | Filter: maximum position open time (**server time**)           |
| `page`             | `int?`                                     | ❌       | Page number for pagination (0-based, null = no pagination)     |
| `size`           | `int?`                                     | ❌       | Items per page (null = no pagination, return all)              |
| `deadline`               | `DateTime?`                                | ❌       | Absolute per‑call **UTC** deadline → converted to timeout      |
| `cancellationToken`      | `CancellationToken`                        | ❌       | Cooperative cancel for the call/retry loop                     |

---

## ⬆️ Output — `PositionsHistoryData`

| Field               | Type                            | Description                                  |
| ------------------- | ------------------------------- | -------------------------------------------- |
| `HistoryPositions`  | `List<PositionHistoryInfo>`     | List of closed positions                     |

---

## 🧱 Related types and enums (from proto)

### `AH_ENUM_POSITIONS_HISTORY_SORT_TYPE` (Sort Options)

| Enum Value                      | Value | Description                           |
| ------------------------------- | ----- | ------------------------------------- |
| `AhPositionOpenTimeAsc`     | 0     | Sort by open time (oldest first)      |
| `AhPositionOpenTimeDesc`    | 1     | Sort by open time (newest first)      |
| `AhPositionTicketAsc`        | 2     | Sort by ticket ID (ascending)         |
| `AhPositionTicketDesc`       | 3     | Sort by ticket ID (descending)        |

### `PositionHistoryInfo` (Closed Position Summary)

| Field            | Type                                    | Description                                      |
| ---------------- | --------------------------------------- | ------------------------------------------------ |
| `Index`          | `int32`                                 | Zero-based index in the result list             |
| `PositionTicket` | `ulong`                                 | Position ticket ID                               |
| `OrderType`      | `AH_ENUM_POSITIONS_HISTORY_ORDER_TYPE`  | Position type (Buy, Sell, etc.)                  |
| `OpenTime`       | `Timestamp`                             | Position open time                               |
| `CloseTime`      | `Timestamp`                             | Position close time                              |
| `Volume`         | `double`                                | Position volume (lots)                           |
| `OpenPrice`      | `double`                                | Entry price                                      |
| `ClosePrice`     | `double`                                | Exit price                                       |
| `StopLoss`       | `double`                                | Stop Loss price (at close)                       |
| `TakeProfit`     | `double`                                | Take Profit price (at close)                     |
| `MarketValue`    | `double`                                | Market value of position                         |
| `Commission`     | `double`                                | Total commission charged                         |
| `Fee`            | `double`                                | Additional fees                                  |
| `Profit`         | `double`                                | Net profit/loss (realized)                       |
| `Swap`           | `double`                                | Swap charged/earned                              |
| `Comment`        | `string`                                | Position comment                                 |
| `Symbol`         | `string`                                | Trading symbol                                   |
| `Magic`          | `long`                                  | Magic number (EA identifier)                     |

### `AH_ENUM_POSITIONS_HISTORY_ORDER_TYPE`

| Enum Value                  | Value | Description                                                        |
| --------------------------- | ----- | ------------------------------------------------------------------ |
| `AhOrderTypeBuy`         | 0     | Market Buy order                                                   |
| `AhOrderTypeSell`        | 1     | Market Sell order                                                  |
| `AhOrderTypeBuyLimit`   | 2     | Buy Limit pending order                                            |
| `AhOrderTypeSellLimit`  | 3     | Sell Limit pending order                                           |
| `AhOrderTypeBuyStop`    | 4     | Buy Stop pending order                                             |
| `AhOrderTypeSellStop`   | 5     | Sell Stop pending order                                            |
| `AhOrderTypeBuyStopLimit` | 6  | Upon reaching order price, Buy Limit placed at StopLimit price     |
| `AhOrderTypeSellStopLimit` | 7 | Upon reaching order price, Sell Limit placed at StopLimit price    |
| `AhOrderTypeCloseBy`    | 8     | Order to close position by opposite one                            |

---

## 💬 Just the essentials

* **What it is.** RPC to retrieve closed positions aggregated by position ticket.
* **Why you need it.** Simpler than `OrderHistoryAsync()` - returns positions (not individual orders/deals).
* **Key difference.** `OrderHistory` = raw orders + deals. `PositionsHistory` = aggregated closed positions.
* **One record per position.** Each `PositionHistoryInfo` represents one complete position lifecycle (open → close).
* **Optional filtering.** Can filter by position open time range.
* **Pagination support.** Handle large histories efficiently.

---

## 🎯 Purpose

Use this method when you need to:

* Get clean list of closed positions (not raw orders/deals).
* Calculate total P&L for closed positions.
* Analyze trading performance by position.
* Generate position-based reports (not order-based).
* Track EA performance (filter by magic number).
* Display closed trades in UI (simpler than parsing OrderHistory).

---

## 🧩 Notes & Tips

* **Position-centric view** - each record is one complete position (open to close).
* Contrast with `OrderHistoryAsync()` which returns individual orders and deals.
* **Time filters** are for **position open time** (not close time).
* Use `null` for time filters to get **all** closed positions.
* `Profit` includes all P&L from position open to close (net result).
* `Commission`, `Fee`, and `Swap` are **separate** from Profit (not included).
* Net P&L = `Profit - Commission - Fee + Swap` (Swap can be positive or negative).
* For **netting accounts**: positions are netted per symbol.
* For **hedging accounts**: multiple positions per symbol possible.
* Use `sortType` to get most recent positions first: `AhPositionOpenTimeDesc`.
* Filter by `Magic` in your code to get EA-specific positions.
* Use pagination for large datasets (months/years of trading).
* Much **faster** than `OrderHistoryAsync()` for position-level analysis.

---

## 🔗 Usage Examples

### 1) Get all closed positions

```csharp
// Retrieve all closed positions (no time filter)
var positions = await acc.PositionsHistoryAsync(
    sortType: AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeDesc,
    deadline: DateTime.UtcNow.AddSeconds(10));

Console.WriteLine($"Total closed positions: {positions.HistoryPositions.Count}");
```

### 2) Get positions from last 30 days

```csharp
// Filter by position open time
var from = DateTime.UtcNow.AddDays(-30); // Server time
var to = DateTime.UtcNow;

var positions = await acc.PositionsHistoryAsync(
    openFrom: from,
    openTo: to,
    sortType: AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeDesc);

Console.WriteLine($"Positions from last 30 days: {positions.HistoryPositions.Count}");
```

### 3) Calculate total realized P&L

```csharp
// Sum all closed position profits
var positions = await acc.PositionsHistoryAsync();

double totalProfit = 0;
double totalCommission = 0;
double totalSwap = 0;
double totalFee = 0;

foreach (var pos in positions.HistoryPositions)
{
    totalProfit += pos.Profit;
    totalCommission += pos.Commission;
    totalSwap += pos.Swap;
    totalFee += pos.Fee;
}

double netPL = totalProfit - totalCommission - totalFee + totalSwap;

Console.WriteLine($"Total Profit:     {totalProfit:F2}");
Console.WriteLine($"Total Commission: {totalCommission:F2}");
Console.WriteLine($"Total Fee:        {totalFee:F2}");
Console.WriteLine($"Total Swap:       {totalSwap:F2}");
Console.WriteLine($"───────────────────────────────");
Console.WriteLine($"Net P&L:          {netPL:F2}");
```

### 4) Display closed positions table

```csharp
// Show recent closed positions
var positions = await acc.PositionsHistoryAsync(
    sortType: AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeDesc,
    openFrom: DateTime.UtcNow.AddDays(-7));

Console.WriteLine("=== CLOSED POSITIONS (Last 7 Days) ===");
Console.WriteLine("Ticket      Symbol      Type  Volume   Open       Close      Profit");
Console.WriteLine("─────────────────────────────────────────────────────────────────────");

foreach (var pos in positions.HistoryPositions)
{
    var type = pos.OrderType == AH_ENUM_POSITIONS_HISTORY_ORDER_TYPE.AhOrderTypeBuy ? "BUY " : "SELL";
    var openTime = DateTimeOffset.FromUnixTimeSeconds(pos.OpenTime.Seconds).DateTime;

    Console.WriteLine($"{pos.PositionTicket,-10}  {pos.Symbol,-10}  {type}  {pos.Volume,6:F2}  {pos.OpenPrice,8:F5}  {pos.ClosePrice,8:F5}  {pos.Profit,8:F2}");
}
```

### 5) Analyze winning vs losing positions

```csharp
// Calculate win rate
var positions = await acc.PositionsHistoryAsync(
    openFrom: DateTime.UtcNow.AddMonths(-1));

int winners = 0;
int losers = 0;
double winningPL = 0;
double losingPL = 0;

foreach (var pos in positions.HistoryPositions)
{
    double netPL = pos.Profit - pos.Commission - pos.Fee + pos.Swap;

    if (netPL > 0)
    {
        winners++;
        winningPL += netPL;
    }
    else if (netPL < 0)
    {
        losers++;
        losingPL += netPL;
    }
}

int total = winners + losers;
double winRate = total > 0 ? (double)winners / total * 100 : 0;
double avgWin = winners > 0 ? winningPL / winners : 0;
double avgLoss = losers > 0 ? losingPL / losers : 0;

Console.WriteLine($"Trading Statistics (Last Month):");
Console.WriteLine($"  Total Positions:  {total}");
Console.WriteLine($"  Winners:          {winners} ({winRate:F1}%)");
Console.WriteLine($"  Losers:           {losers}");
Console.WriteLine($"  Average Win:      {avgWin:F2}");
Console.WriteLine($"  Average Loss:     {avgLoss:F2}");
Console.WriteLine($"  Profit Factor:    {(Math.Abs(losingPL) > 0 ? winningPL / Math.Abs(losingPL) : 0):F2}");
```

### 6) Filter by EA magic number

```csharp
// Get positions for specific EA
var eaMagic = 123456L;
var positions = await acc.PositionsHistoryAsync();

var eaPositions = positions.HistoryPositions
    .Where(p => p.Magic == eaMagic)
    .ToList();

double eaPL = eaPositions.Sum(p => p.Profit - p.Commission - p.Fee + p.Swap);

Console.WriteLine($"EA {eaMagic} Performance:");
Console.WriteLine($"  Closed Positions: {eaPositions.Count}");
Console.WriteLine($"  Net P&L:          {eaPL:F2}");
```

### 7) Group by symbol

```csharp
// Analyze performance by symbol
var positions = await acc.PositionsHistoryAsync(
    openFrom: DateTime.UtcNow.AddMonths(-3));

var bySymbol = positions.HistoryPositions
    .GroupBy(p => p.Symbol)
    .Select(g => new
    {
        Symbol = g.Key,
        Count = g.Count(),
        TotalPL = g.Sum(p => p.Profit - p.Commission - p.Fee + p.Swap),
        AvgPL = g.Average(p => p.Profit - p.Commission - p.Fee + p.Swap)
    })
    .OrderByDescending(x => x.TotalPL);

Console.WriteLine("=== PERFORMANCE BY SYMBOL ===");
Console.WriteLine("Symbol      Trades    Total P&L    Avg P&L");
Console.WriteLine("─────────────────────────────────────────────");

foreach (var item in bySymbol)
{
    Console.WriteLine($"{item.Symbol,-10}  {item.Count,5}  {item.TotalPL,12:F2}  {item.AvgPL,10:F2}");
}
```

### 8) Pagination for large histories

```csharp
// Retrieve large history in pages
var size = 50;
var currentPage = 0;
var allPositions = new List<PositionHistoryInfo>();

while (true)
{
    var positions = await acc.PositionsHistoryAsync(
        page: currentPage,
        size: size,
        sortType: AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeDesc);

    Console.WriteLine($"Page {currentPage + 1}: {positions.HistoryPositions.Count} positions");

    if (positions.HistoryPositions.Count == 0)
        break;

    allPositions.AddRange(positions.HistoryPositions);

    if (positions.HistoryPositions.Count < size)
        break; // Last page

    currentPage++;
}

Console.WriteLine($"Total positions retrieved: {allPositions.Count}");
```
