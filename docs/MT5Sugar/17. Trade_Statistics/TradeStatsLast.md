# ✅ Aggregated Stats for N Days (`TradeStatsLast`)

> **Sugar method:** Aggregated trade statistics, optionally scoped to the last N days (day count instead of a date).

**API Information:**

* **Extension method:** `MT5Service.TradeStatsLast(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Underlying calls:** `TradeStatsAsync()`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static Task<StatsData> TradeStatsLast(
        this MT5Service svc,
        int? days = null, int timeoutSec = 20, CancellationToken ct = default);
}
```

---

## 🔽 Input

| Parameter    | Type                | Description                                      |
| ------------ | ------------------- | ------------------------------------------------ |
| `svc`        | `MT5Service`        | MT5Service instance (extension method)           |
| `days`       | `int?`              | Look‑back window in days. `null` = full history  |
| `timeoutSec` | `int`               | Timeout in seconds (default: 20)                 |
| `ct`         | `CancellationToken` | Cancellation token                               |

---

## ⬆️ Output — `StatsData`

Profit summary, win/loss, drawdowns, per‑market breakdown, pips/lots/commissions. Full field tables: [MT5Account · TradeStats](../../MT5Account/8.%20Trade_Statistics/TradeStats.md).

---

## 💬 Just the essentials

* **What it is.** Performance snapshot for a day window (or all time).
* **Why you need it.** Dashboards, strategy reports, risk reviews.
* **Broker support.** The trade‑stats service may be unavailable on some servers — wrap in `try/catch`.

---

## 🎯 Purpose

* Show profit factor / win rate on a dashboard.
* Compare markets; track drawdown against limits.

---

## 🧩 Notes & Tips

* **`days = null`** → full history; a value → `since = now - days` (UTC).
* **Related** — `EquityCurveLast` (curve), `StatsWithChartsLast` (both at once).

---

## 🔧 Under the Hood

```csharp
return svc.TradeStatsAsync(days.HasValue ? DateTime.UtcNow.AddDays(-days.Value) : (DateTime?)null, Dl(timeoutSec), ct);
```

---

## 🔗 Usage Examples

### 1) All‑time summary

```csharp
// svc — MT5Service instance
var s = await svc.TradeStatsLast();
Console.WriteLine($"Pips {s.Pips:F1} | Lots {s.Lots:F2} | Win {s.Profitability.WonTradesPercent:F1}% | Max eq DD {s.MaxEquityDrawdownRelative:F2}%");
```

### 2) Last 30 days, per market

```csharp
var s = await svc.TradeStatsLast(30);
foreach (var m in s.Markets)
    Console.WriteLine($"{m.MarketName,-10} {m.Count,4} trades | P/L {m.Profit,10:F2} | win {m.WinRate:F0}%");
```

---

## 🔗 Related Methods

* **Low-level:** [`TradeStatsAsync()`](../../MT5Account/8.%20Trade_Statistics/TradeStats.md)
* **Sugar:** `StatsWithChartsLast()`, `EquityCurveLast()`, `UnrealizedExtremaLast()`
