# ✅ Aggregated Trade Statistics (`TradeStatsAsync`)

> **Request:** Get aggregated account performance on **MT5** — profit summary, win/loss, drawdowns and a per‑market breakdown.

**API Information:**

* **SDK wrapper:** `MT5Account.TradeStatsAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.TradeStats`
* **Proto definition:** `TradeStats` (defined in `mt5-term-api-trade-stats.proto`)

### RPC

* **Service:** `mt5_term_api.TradeStats`
* **Method:** `TradeStats(TradeStatsRequest) → TradeStatsReply`
* **Low‑level client (generated):** `TradeStats.TradeStatsClient.TradeStats(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<StatsData> TradeStatsAsync(
            DateTime? since = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`TradeStatsRequest { from }` — `from` is an optional UTC datetime string (`yyyy-MM-ddTHH:mm:ss`).


**Reply message:**

`TradeStatsReply { data: StatsData }`

---

## 🔽 Input

| Parameter           | Type                | Description                                               |
| ------------------- | ------------------- | --------------------------------------------------------- |
| `since`             | `DateTime?`         | Window start (UTC). Omit for **full history**             |
| `deadline`          | `DateTime?`         | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `StatsData`

| Field                        | Type                            | Description                              |
| ---------------------------- | ------------------------------- | ---------------------------------------- |
| `Summary`                    | `TradeSummaryData`              | Profit summary (open/day/week/month/total)|
| `Profitability`              | `ProfitabilityData`             | Won/lost counts and percentages          |
| `MaxBalanceDrawdownRaw`      | `double`                        | Max balance drawdown (absolute)          |
| `MaxBalanceDrawdownRelative` | `double`                        | Max balance drawdown (%)                 |
| `MaxEquityDrawdownRaw`       | `double`                        | Max equity drawdown (absolute)           |
| `MaxEquityDrawdownRelative`  | `double`                        | Max equity drawdown (%)                  |
| `Markets`                    | `repeated MarketTradeCountData` | Per‑market breakdown                     |
| `Pips`                       | `double`                        | Net pips                                 |
| `Lots`                       | `double`                        | Total traded volume (lots)               |
| `Comissions`                 | `double`                        | Total commissions                        |

### `TradeSummaryData`

| Field | Type | Description |
| ----- | ---- | ----------- |
| `OpenTrades` | `int` | Currently open trades |
| `OpenProfit` | `double` | Floating P/L of open trades |
| `DayProfit` / `WeekProfit` / `MonthProfit` | `double` | Realized profit by period |
| `TotalProfit` | `double` | Total realized profit |

### `ProfitabilityData`

| Field | Type | Description |
| ----- | ---- | ----------- |
| `WonTrades` / `LostTrades` | `long` | Winning / losing trade counts |
| `WonTradesPercent` / `LostTradesPercent` | `double` | Win / loss percentage |

### `MarketTradeCountData`

| Field | Type | Description |
| ----- | ---- | ----------- |
| `MarketName` | `string` | Symbol / market |
| `Count` | `int` | Trades on this market |
| `Lots` | `double` | Volume traded |
| `Profit` | `double` | Net profit |
| `WinRate` | `double` | Win rate (%) |
| `WonCount` / `LostCount` | `int` | Won / lost trades |

---

## 💬 Just the essentials

* **What it is.** A performance snapshot of the account over a window.
* **Why you need it.** Dashboards, strategy reports, risk reviews.
* **Related.** [`EquityHistory`](./EquityHistory.md) for the curve; [`TradeStatsEquityHistory`](./TradeStatsEquityHistory.md) for both at once.

---

## 🎯 Purpose

Use it to quantify performance:

* Show profit factor / win rate on a dashboard.
* Compare markets (which symbol pays off).
* Track drawdown against risk limits.

---

## 🧩 Notes & Tips

* **Full vs windowed:** Omit `since` for all‑time; pass a date for a rolling window.
* **UTC:** `since` is treated as UTC and formatted to the proto `from` string by the SDK.
* **Broker support:** May be unavailable on some servers — wrap in `try/catch`.
* **Friendly wrapper:** `MT5Sugar.TradeStatsLast(days)` takes a day count instead of a date.

---

## 🔗 Usage Examples

### 1) All‑time summary

```csharp
// acc — connected MT5Account
var s = await acc.TradeStatsAsync();
Console.WriteLine($"Pips {s.Pips:F1} | Lots {s.Lots:F2} | Commissions {s.Comissions:F2}");
Console.WriteLine($"Win rate {s.Profitability.WonTradesPercent:F1}% | Max equity DD {s.MaxEquityDrawdownRelative:F2}%");
```

---

### 2) Last 30 days, per‑market breakdown

```csharp
var s = await acc.TradeStatsAsync(DateTime.UtcNow.AddDays(-30));
foreach (var m in s.Markets)
    Console.WriteLine($"{m.MarketName,-10} trades {m.Count,4} | P/L {m.Profit,10:F2} | win {m.WinRate:F0}%");
```
