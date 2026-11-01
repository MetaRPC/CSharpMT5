# ✅ Statistics + Equity Curve (`TradeStatsEquityHistoryAsync`)

> **Request:** Get aggregated statistics **together with** the equity curve and the trade list in a single call on **MT5**.

**API Information:**

* **SDK wrapper:** `MT5Account.TradeStatsEquityHistoryAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.TradeStats`
* **Proto definition:** `TradeStatsEquityHistory` (defined in `mt5-term-api-trade-stats.proto`)

### RPC

* **Service:** `mt5_term_api.TradeStats`
* **Method:** `TradeStatsEquityHistory(TradeStatsEquityHistoryRequest) → TradeStatsEquityHistoryReply`
* **Low‑level client (generated):** `TradeStats.TradeStatsClient.TradeStatsEquityHistory(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<StatsWithChartsData> TradeStatsEquityHistoryAsync(
            DateTime? since = null,
            bool? excludeSameBars = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`TradeStatsEquityHistoryRequest { from, exclude_same_bars }`


**Reply message:**

`TradeStatsEquityHistoryReply { data: StatsWithChartsData }`

---

## 🔽 Input

| Parameter           | Type                | Description                                                     |
| ------------------- | ------------------- | -------------------------------------------------------------- |
| `since`             | `DateTime?`         | Window start (UTC). Omit for **full history**                  |
| `excludeSameBars`   | `bool?`             | Drop repeated equity bars to shrink the payload                |
| `deadline`          | `DateTime?`         | Absolute per‑call **UTC** deadline → converted to timeout      |
| `cancellationToken` | `CancellationToken` | Cooperative cancel for the call/retry loop                     |

---

## ⬆️ Output — `StatsWithChartsData`

| Field        | Type                          | Description                                  |
| ------------ | ----------------------------- | -------------------------------------------- |
| `Stats`      | `StatsData`                   | Same summary as [`TradeStats`](./TradeStats.md) |
| `Charts`     | `repeated EquityPointData`    | Equity‑curve points (balance/equity over time) |
| `Trades`     | `repeated TradeHistoryItemData`| Closed trades in the window                  |
| `OpenTrades` | `repeated TradeHistoryItemData`| Currently open trades                        |

### `TradeHistoryItemData`

| Field | Type | Description |
| ----- | ---- | ----------- |
| `Ticket` | `long` | Trade ticket |
| `Symbol` | `string` | Symbol |
| `IsBuy` | `bool` | Direction |
| `Lots` | `double` | Volume |
| `OpenPrice` / `ClosePrice` | `double` | Entry / exit price |
| `OpenTime` | `Timestamp` | Entry time (UTC) |

> For `EquityPointData` fields, see [`EquityHistory`](./EquityHistory.md).

---

## 💬 Just the essentials

* **What it is.** Summary + curve + trades in one round‑trip.
* **Why you need it.** Build a full performance view without three separate calls.
* **Related.** [`TradeStats`](./TradeStats.md) (summary only), [`EquityHistory`](./EquityHistory.md) (curve only).

---

## 🎯 Purpose

Use it to render a performance report in one shot:

* KPIs (from `Stats`) next to an equity chart (from `Charts`).
* Trade blotter (from `Trades`) alongside the summary.

---

## 🧩 Notes & Tips

* **`excludeSameBars`:** Set `true` for charts — skips flat repeated points and reduces payload.
* **One call:** Cheaper than calling `TradeStats` + `EquityHistory` separately.
* **Broker support:** May be unavailable on some servers — wrap in `try/catch`.
* **Friendly wrapper:** `MT5Sugar.StatsWithChartsLast(days, excludeSameBars)`.

---

## 🔗 Usage Examples

### 1) KPIs + equity points

```csharp
// acc — connected MT5Account
var r = await acc.TradeStatsEquityHistoryAsync(excludeSameBars: true);
Console.WriteLine($"Equity points: {r.Charts.Count} | Trades: {r.Trades.Count} | Open: {r.OpenTrades.Count}");
Console.WriteLine($"Total profit: {r.Stats.Summary.TotalProfit:F2}");
```

---

### 2) Last 90 days, dump the trade blotter

```csharp
var r = await acc.TradeStatsEquityHistoryAsync(DateTime.UtcNow.AddDays(-90), excludeSameBars: true);
foreach (var t in r.Trades.Take(10))
    Console.WriteLine($"#{t.Ticket} {(t.IsBuy ? "BUY" : "SELL")} {t.Symbol} {t.Lots:F2} @ {t.OpenPrice:F5}");
```
