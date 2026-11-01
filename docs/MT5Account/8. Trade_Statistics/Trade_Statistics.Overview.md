# Trade Statistics — Overview

> Account **performance analytics** served by the `mt5_term_api.TradeStats` service: aggregated statistics, the equity curve, and per‑position unrealized‑P/L extrema.

These methods read historical performance — they never place trades. Each accepts an optional `since` start time (omit for full history).

---

## 📄 Methods in this section

* [TradeStats.md](./TradeStats.md) — aggregated statistics (profit factor, win/loss, drawdowns, per‑market breakdown)
* [TradeStatsEquityHistory.md](./TradeStatsEquityHistory.md) — statistics **plus** equity curve and trades in one call
* [TradeUnrealizedPL.md](./TradeUnrealizedPL.md) — per‑open‑position unrealized‑P/L extrema (MFE/MAE)
* [EquityHistory.md](./EquityHistory.md) — the account equity curve (balance/equity points over time)

---

## 🧭 How to pick

| If you need…                                  | Use                                               |
| --------------------------------------------- | ------------------------------------------------- |
| One‑shot performance summary                  | [`TradeStats`](./TradeStats.md)                   |
| Summary **and** chart data together           | [`TradeStatsEquityHistory`](./TradeStatsEquityHistory.md) |
| Just the equity/balance curve                 | [`EquityHistory`](./EquityHistory.md)             |
| Best/worst floating P/L per open position     | [`TradeUnrealizedPL`](./TradeUnrealizedPL.md)     |

---

## 🔌 Shape

* **Service:** `mt5_term_api.TradeStats` (wired in `MT5Account` as `TradeStatsClient`).
* **Time window:** optional `since` (`DateTime?`). The SDK formats it as the proto’s `from` string (`yyyy-MM-ddTHH:mm:ss`, UTC); omit for full history.
* **Payload shrink:** `TradeStatsEquityHistory` and `EquityHistory` accept `excludeSameBars` to drop repeated equity bars.

```csharp
var stats  = await acc.TradeStatsAsync();                       // full history
var curve  = await acc.EquityHistoryAsync(excludeSameBars: true);
var recent = await acc.TradeStatsAsync(DateTime.UtcNow.AddDays(-30));
```

> ⚠️ **Broker/server support:** The trade‑stats service may be disabled on some brokers/servers. Wrap calls in `try/catch` and degrade gracefully if unavailable.

---

## 🍬 Friendly wrappers (MT5Sugar)

| Low‑level (`MT5Account`)        | Sugar (`MT5Service` extension)                     |
| ------------------------------- | -------------------------------------------------- |
| `TradeStatsAsync(since?)`       | `TradeStatsLast(days?)`                            |
| `TradeStatsEquityHistoryAsync`  | `StatsWithChartsLast(days?, excludeSameBars)`      |
| `TradeUnrealizedPLAsync`        | `UnrealizedExtremaLast(days?)` → plain list        |
| `EquityHistoryAsync`            | `EquityCurveLast(days?)` → plain list of points    |

---

[⬅️ Back to MT5Account Master Overview](../MT5Account.Master.Overview.md)
