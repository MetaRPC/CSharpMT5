# ✅ Equity Curve as a List (`EquityCurveLast`)

> **Sugar method:** The account equity curve as a plain `IReadOnlyList<EquityPointData>`, optionally scoped to the last N days.

**API Information:**

* **Extension method:** `MT5Service.EquityCurveLast(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Underlying calls:** `EquityHistoryAsync()`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static Task<IReadOnlyList<EquityPointData>> EquityCurveLast(
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

## ⬆️ Output — `IReadOnlyList<EquityPointData>`

Points along the curve (unwrapped from `EquityHistoryData.Points`). Each: `Time`, `Balance`, `Equity`, drawdown fields, `RealizedPl`, `UnrealizedPl`. Full tables: [MT5Account · EquityHistory](../../MT5Account/8.%20Trade_Statistics/EquityHistory.md).

---

## 💬 Just the essentials

* **What it is.** The balance/equity time series as a plain list.
* **Why you need it.** Draw an equity chart; compute running drawdown.
* **Related.** `StatsWithChartsLast` returns this curve plus summary and trades.

---

## 🎯 Purpose

* Plot equity vs balance to see floating exposure.
* Track peak‑to‑trough drawdown.

---

## 🧩 Notes & Tips

* **`excludeSameBars = true`** — recommended before charting (drops flat points).
* **Balance vs equity** — the gap is open floating P/L at that point.
* **Broker support** — may be unavailable on some servers — wrap in `try/catch`.

---

## 🔧 Under the Hood

```csharp
var data = await svc.EquityHistoryAsync(
    days.HasValue ? DateTime.UtcNow.AddDays(-days.Value) : (DateTime?)null,
    excludeSameBars, Dl(timeoutSec), ct);
return data.Points.ToList();
```

---

## 🔗 Usage Examples

### 1) Latest equity point

```csharp
// svc — MT5Service instance
var curve = await svc.EquityCurveLast();
Console.WriteLine($"Equity points: {curve.Count}");
if (curve.Count > 0)
{
    var p = curve[^1];
    Console.WriteLine($"{p.Time.ToDateTime():u}: Balance {p.Balance:F2}, Equity {p.Equity:F2}");
}
```

### 2) Max equity drawdown (last 90 days)

```csharp
var curve = await svc.EquityCurveLast(90);
double maxDd = curve.Count == 0 ? 0 : curve.Max(p => p.EquityDrawdownRelative);
Console.WriteLine($"Max equity drawdown: {maxDd:F2}%");
```

---

## 🔗 Related Methods

* **Low-level:** [`EquityHistoryAsync()`](../../MT5Account/8.%20Trade_Statistics/EquityHistory.md)
* **Sugar:** `StatsWithChartsLast()`, `TradeStatsLast()`
