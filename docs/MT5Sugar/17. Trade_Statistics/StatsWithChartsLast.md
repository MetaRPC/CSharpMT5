# ✅ Stats + Equity Curve for N Days (`StatsWithChartsLast`)

> **Sugar method:** Statistics **plus** the equity curve and trades in one call, optionally scoped to the last N days.

**API Information:**

* **Extension method:** `MT5Service.StatsWithChartsLast(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Underlying calls:** `TradeStatsEquityHistoryAsync()`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static Task<StatsWithChartsData> StatsWithChartsLast(
        this MT5Service svc,
        int? days = null, bool excludeSameBars = true,
        int timeoutSec = 20, CancellationToken ct = default);
}
```

---

## 🔽 Input

| Parameter         | Type                | Description                                      |
| ----------------- | ------------------- | ------------------------------------------------ |
| `svc`             | `MT5Service`        | MT5Service instance (extension method)           |
| `days`            | `int?`              | Look‑back window in days. `null` = full history  |
| `excludeSameBars` | `bool`              | Drop repeated equity bars (default: `true`)      |
| `timeoutSec`      | `int`               | Timeout in seconds (default: 20)                 |
| `ct`              | `CancellationToken` | Cancellation token                               |

---

## ⬆️ Output — `StatsWithChartsData`

`Stats` (summary), `Charts` (equity points), `Trades` and `OpenTrades`. Full field tables: [MT5Account · TradeStatsEquityHistory](../../MT5Account/8.%20Trade_Statistics/TradeStatsEquityHistory.md).

---

## 💬 Just the essentials

* **What it is.** Summary + curve + trades in one round‑trip.
* **Why you need it.** Build a full performance view without three separate calls.
* **Broker support.** May be unavailable on some servers — wrap in `try/catch`.

---

## 🎯 Purpose

* Render KPIs next to an equity chart.
* Show a trade blotter alongside the summary.

---

## 🧩 Notes & Tips

* **`excludeSameBars = true`** — recommended for charts (skips flat points, smaller payload).
* **One call** — cheaper than `TradeStatsLast` + `EquityCurveLast` separately.

---

## 🔧 Under the Hood

```csharp
return svc.TradeStatsEquityHistoryAsync(
    days.HasValue ? DateTime.UtcNow.AddDays(-days.Value) : (DateTime?)null,
    excludeSameBars, Dl(timeoutSec), ct);
```

---

## 🔗 Usage Examples

### 1) KPIs + counts

```csharp
// svc — MT5Service instance
var r = await svc.StatsWithChartsLast();
Console.WriteLine($"Equity points: {r.Charts.Count} | Trades: {r.Trades.Count} | Open: {r.OpenTrades.Count}");
Console.WriteLine($"Total profit: {r.Stats.Summary.TotalProfit:F2}");
```

### 2) Last 90 days blotter

```csharp
var r = await svc.StatsWithChartsLast(90);
foreach (var t in r.Trades.Take(10))
    Console.WriteLine($"#{t.Ticket} {(t.IsBuy ? "BUY" : "SELL")} {t.Symbol} {t.Lots:F2} @ {t.OpenPrice:F5}");
```

---

## 🔗 Related Methods

* **Low-level:** [`TradeStatsEquityHistoryAsync()`](../../MT5Account/8.%20Trade_Statistics/TradeStatsEquityHistory.md)
* **Sugar:** `TradeStatsLast()`, `EquityCurveLast()`
