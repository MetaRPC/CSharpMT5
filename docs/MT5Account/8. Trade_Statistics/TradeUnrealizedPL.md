# ✅ Unrealized P/L Extrema (`TradeUnrealizedPLAsync`)

> **Request:** Get per‑open‑position unrealized‑P/L **extrema** (maximum favorable / maximum adverse excursion — MFE/MAE) on **MT5**.

**API Information:**

* **SDK wrapper:** `MT5Account.TradeUnrealizedPLAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.TradeStats`
* **Proto definition:** `TradeUnrealizedPL` (defined in `mt5-term-api-trade-stats.proto`)

### RPC

* **Service:** `mt5_term_api.TradeStats`
* **Method:** `TradeUnrealizedPL(TradeUnrealizedPLRequest) → TradeUnrealizedPLReply`
* **Low‑level client (generated):** `TradeStats.TradeStatsClient.TradeUnrealizedPL(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<TradeUnrealizedPLData> TradeUnrealizedPLAsync(
            DateTime? since = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`TradeUnrealizedPLRequest { from }`


**Reply message:**

`TradeUnrealizedPLReply { data: TradeUnrealizedPLData }`

---

## 🔽 Input

| Parameter           | Type                | Description                                               |
| ------------------- | ------------------- | --------------------------------------------------------- |
| `since`             | `DateTime?`         | Window start (UTC). Omit for **full history**             |
| `deadline`          | `DateTime?`         | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `TradeUnrealizedPLData`

| Field     | Type                                 | Description                       |
| --------- | ------------------------------------ | --------------------------------- |
| `Extrema` | `repeated TradeUnrealizedExtremaData`| One entry per position            |

### `TradeUnrealizedExtremaData`

| Field               | Type        | Description                                        |
| ------------------- | ----------- | -------------------------------------------------- |
| `Ticket`            | `long`      | Position ticket                                    |
| `Symbol`            | `string`    | Symbol                                             |
| `MaxUnrealizedTime` | `Timestamp` | When the best floating profit occurred (MFE)       |
| `MaxUnrealizedPl`   | `double`    | Maximum favorable excursion (best floating profit) |
| `MinUnrealizedTime` | `Timestamp` | When the worst floating loss occurred (MAE)        |
| `MinUnrealizedPl`   | `double`    | Maximum adverse excursion (worst floating loss)    |

---

## 💬 Just the essentials

* **What it is.** Best/worst floating P/L each position reached (MFE/MAE).
* **Why you need it.** Evaluate exit timing and stop placement quality.
* **Note.** This is **unrealized** (path extrema), not final realized P/L.

---

## 🎯 Purpose

Use it to study trade behavior:

* Were stops too tight (large MFE but closed in loss)?
* How much heat (MAE) did winners take before turning around?
* Tune SL/TP distances from real excursions.

---

## 🧩 Notes & Tips

* **MFE/MAE:** `MaxUnrealizedPl` = best floating profit; `MinUnrealizedPl` = worst floating loss along the position’s life.
* **UTC:** The `*Time` fields are UTC `Timestamp` — use `.ToDateTime()`.
* **Broker support:** May be unavailable on some servers — wrap in `try/catch`.
* **Friendly wrapper:** `MT5Sugar.UnrealizedExtremaLast(days)` returns the extrema as a plain list.

---

## 🔗 Usage Examples

### 1) List extrema per position

```csharp
// acc — connected MT5Account
var data = await acc.TradeUnrealizedPLAsync();
Console.WriteLine($"Positions: {data.Extrema.Count}");
foreach (var e in data.Extrema.Take(10))
    Console.WriteLine($"#{e.Ticket} {e.Symbol} | MFE {e.MaxUnrealizedPl:F2} | MAE {e.MinUnrealizedPl:F2}");
```

---

### 2) Find positions that gave back the most profit

```csharp
var data = await acc.TradeUnrealizedPLAsync();
var gaveBack = data.Extrema
    .Where(e => e.MaxUnrealizedPl > 0)
    .OrderByDescending(e => e.MaxUnrealizedPl)
    .Take(5);

foreach (var e in gaveBack)
    Console.WriteLine($"#{e.Ticket} {e.Symbol}: peaked at {e.MaxUnrealizedPl:F2}");
```
