# ✅ Equity Curve (`EquityHistoryAsync`)

> **Request:** Get the account **equity curve** — balance/equity points over time — on **MT5**.

**API Information:**

* **SDK wrapper:** `MT5Account.EquityHistoryAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.TradeStats`
* **Proto definition:** `EquityHistory` (defined in `mt5-term-api-trade-stats.proto`)

### RPC

* **Service:** `mt5_term_api.TradeStats`
* **Method:** `EquityHistory(EquityHistoryRequest) → EquityHistoryReply`
* **Low‑level client (generated):** `TradeStats.TradeStatsClient.EquityHistory(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<EquityHistoryData> EquityHistoryAsync(
            DateTime? since = null,
            bool? excludeSameBars = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`EquityHistoryRequest { from, exclude_same_bars }`


**Reply message:**

`EquityHistoryReply { data: EquityHistoryData }`

---

## 🔽 Input

| Parameter           | Type                | Description                                                |
| ------------------- | ------------------- | ---------------------------------------------------------- |
| `since`             | `DateTime?`         | Window start (UTC). Omit for **full history**              |
| `excludeSameBars`   | `bool?`             | Drop repeated equity bars to shrink the payload            |
| `deadline`          | `DateTime?`         | Absolute per‑call **UTC** deadline → converted to timeout  |
| `cancellationToken` | `CancellationToken` | Cooperative cancel for the call/retry loop                 |

---

## ⬆️ Output — `EquityHistoryData`

| Field    | Type                       | Description              |
| -------- | -------------------------- | ------------------------ |
| `Points` | `repeated EquityPointData` | Points along the curve   |

### `EquityPointData`

| Field                     | Type        | Description                          |
| ------------------------- | ----------- | ------------------------------------ |
| `Time`                    | `Timestamp` | Point timestamp (UTC)                |
| `Balance`                 | `double`    | Account balance at this point        |
| `Equity`                  | `double`    | Account equity at this point         |
| `BalanceDrawdownRaw`      | `double`    | Balance drawdown (absolute)          |
| `BalanceDrawdownRelative` | `double`    | Balance drawdown (%)                 |
| `EquityDrawdownRaw`       | `double`    | Equity drawdown (absolute)           |
| `EquityDrawdownRelative`  | `double`    | Equity drawdown (%)                  |
| `RealizedPl`              | `double`    | Realized P/L at this point           |
| `UnrealizedPl`            | `double`    | Floating P/L at this point           |

---

## 💬 Just the essentials

* **What it is.** The balance/equity time series for the account.
* **Why you need it.** Draw an equity chart; compute running drawdown.
* **Related.** [`TradeStatsEquityHistory`](./TradeStatsEquityHistory.md) returns this curve **plus** summary and trades.

---

## 🎯 Purpose

Use it to visualize and measure the account over time:

* Plot equity vs balance to see floating exposure.
* Track peak‑to‑trough drawdown.
* Feed a performance dashboard.

---

## 🧩 Notes & Tips

* **`excludeSameBars`:** Set `true` to drop flat repeated points before charting.
* **UTC:** `Time` is a UTC `Timestamp` — use `.ToDateTime()`.
* **Balance vs equity:** Gap between them = open floating P/L at that point.
* **Friendly wrapper:** `MT5Sugar.EquityCurveLast(days)` returns the points as a plain list.

---

## 🔗 Usage Examples

### 1) Latest equity point

```csharp
// acc — connected MT5Account
var eq = await acc.EquityHistoryAsync(excludeSameBars: true);
Console.WriteLine($"Equity points: {eq.Points.Count}");

if (eq.Points.Count > 0)
{
    var last = eq.Points[^1];
    Console.WriteLine($"{last.Time.ToDateTime():u}: Balance {last.Balance:F2}, Equity {last.Equity:F2}");
}
```

---

### 2) Maximum equity drawdown over the curve

```csharp
var eq = await acc.EquityHistoryAsync(DateTime.UtcNow.AddDays(-90), excludeSameBars: true);
double maxDd = eq.Points.Count == 0 ? 0 : eq.Points.Max(p => p.EquityDrawdownRelative);
Console.WriteLine($"Max equity drawdown over window: {maxDd:F2}%");
```
