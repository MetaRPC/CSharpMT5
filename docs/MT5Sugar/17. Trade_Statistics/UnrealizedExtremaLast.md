# ✅ Unrealized‑P/L Extrema as a List (`UnrealizedExtremaLast`)

> **Sugar method:** Per‑open‑position unrealized‑P/L extrema (MFE/MAE) as a plain `IReadOnlyList<TradeUnrealizedExtremaData>`.

**API Information:**

* **Extension method:** `MT5Service.UnrealizedExtremaLast(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Underlying calls:** `TradeUnrealizedPLAsync()`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static Task<IReadOnlyList<TradeUnrealizedExtremaData>> UnrealizedExtremaLast(
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

## ⬆️ Output — `IReadOnlyList<TradeUnrealizedExtremaData>`

One entry per position (unwrapped from `TradeUnrealizedPLData.Extrema`). Each: `Ticket`, `Symbol`, `MaxUnrealizedPl` (MFE), `MinUnrealizedPl` (MAE), and their timestamps. Full tables: [MT5Account · TradeUnrealizedPL](../../MT5Account/8.%20Trade_Statistics/TradeUnrealizedPL.md).

---

## 💬 Just the essentials

* **What it is.** Best/worst floating P/L each position reached, as a plain list.
* **Why you need it.** Evaluate exit timing and stop placement quality.
* **Note.** This is **unrealized** (path extrema), not final realized P/L.

---

## 🎯 Purpose

* Were stops too tight (large MFE but closed in loss)?
* How much heat (MAE) did winners take before turning around?

---

## 🧩 Notes & Tips

* **`days = null`** → full history; a value → `since = now - days`.
* **Broker support** — may be unavailable on some servers — wrap in `try/catch`.

---

## 🔧 Under the Hood

```csharp
var data = await svc.TradeUnrealizedPLAsync(days.HasValue ? DateTime.UtcNow.AddDays(-days.Value) : (DateTime?)null, Dl(timeoutSec), ct);
return data.Extrema.ToList();
```

---

## 🔗 Usage Examples

### 1) List extrema

```csharp
// svc — MT5Service instance
var extrema = await svc.UnrealizedExtremaLast();
Console.WriteLine($"Positions: {extrema.Count}");
foreach (var e in extrema.Take(10))
    Console.WriteLine($"#{e.Ticket} {e.Symbol} | MFE {e.MaxUnrealizedPl:F2} | MAE {e.MinUnrealizedPl:F2}");
```

### 2) Positions that gave back the most

```csharp
var top = (await svc.UnrealizedExtremaLast())
    .Where(e => e.MaxUnrealizedPl > 0)
    .OrderByDescending(e => e.MaxUnrealizedPl)
    .Take(5);
foreach (var e in top)
    Console.WriteLine($"#{e.Ticket} {e.Symbol}: peaked at {e.MaxUnrealizedPl:F2}");
```

---

## 🔗 Related Methods

* **Low-level:** [`TradeUnrealizedPLAsync()`](../../MT5Account/8.%20Trade_Statistics/TradeUnrealizedPL.md)
* **Sugar:** `TradeStatsLast()`, `EquityCurveLast()`
