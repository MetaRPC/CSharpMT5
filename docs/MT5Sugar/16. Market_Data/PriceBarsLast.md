# ✅ Recent Price Bars (`PriceBarsLast`)

> **Sugar method:** Historical OHLC bars for the last N days using a **friendly timeframe string** (`"H1"`), returned as a plain `IReadOnlyList<PriceBar>`.

**API Information:**

* **Extension method:** `MT5Service.PriceBarsLast(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Underlying calls:** `EnsureSelected()` + `PriceHistoryAsync()`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static Task<IReadOnlyList<PriceBar>> PriceBarsLast(
        this MT5Service svc,
        string symbol, string timeframe = "H1", int days = 7,
        int timeoutSec = 20, CancellationToken ct = default);
}
```

---

## 🔽 Input

| Parameter    | Type                | Description                                                       |
| ------------ | ------------------- | ---------------------------------------------------------------- |
| `svc`        | `MT5Service`        | MT5Service instance (extension method)                           |
| `symbol`     | `string`            | Symbol to query (auto‑selected first)                            |
| `timeframe`  | `string`            | `M1`, `M5`, `M15`, `M30`, `H1`, `H4`, `D1`, `W1`, `MN1` (def `H1`)|
| `days`       | `int`               | Days to look back from now (default: 7)                          |
| `timeoutSec` | `int`               | Timeout in seconds (default: 20)                                 |
| `ct`         | `CancellationToken` | Cancellation token                                               |

> The friendly string is mapped to the raw MQL5 `ENUM_TIMEFRAMES` value (`H1 → 16385`) internally. An unknown string throws `ArgumentException`.

---

## ⬆️ Output — `IReadOnlyList<PriceBar>`

Bars in the window (unwrapped from `PriceHistoryData.Bars`). Each `PriceBar`: `Time` (Unix seconds), `Open`, `High`, `Low`, `Close`, `TickVolume`, `Spread`, `RealVolume`.

---

## 💬 Just the essentials

* **What it is.** Recent candles as a plain list, with a human timeframe.
* **Why you need it.** Indicators/backtests without raw enum values or manual Unix math.
* **Difference from low‑level.** String timeframe + days window + unwrapped list.

---

## 🎯 Purpose

* Compute indicators (MA, RSI) over recent bars.
* Seed a chart or a quick backtest window.
* Detect ranges/breakouts from the last N bars.

---

## 🧩 Notes & Tips

* **Timeframe strings** — `M1/M5/M15/M30/H1/H4/D1/W1/MN1`; anything else throws.
* **Auto‑select** — the symbol is ensured in Market Watch before the request.
* **Unix time** — `bar.Time` is Unix seconds; `DateTimeOffset.FromUnixTimeSeconds(bar.Time)` to read.

---

## 🔧 Under the Hood

```csharp
await svc.EnsureSelected(symbol, timeoutSec, ct);
var to = DateTimeOffset.UtcNow;
var req = new PriceHistoryRequest
{
    Symbol = symbol,
    Timeframe = TimeframeToMql(timeframe), // "H1" -> 16385
    FromTime = to.AddDays(-days).ToUnixTimeSeconds(),
    ToTime   = to.ToUnixTimeSeconds()
};
var data = await svc.PriceHistoryAsync(req, Dl(timeoutSec), ct);
return data.Bars.ToList();
```

---

## 🔗 Usage Examples

### 1) Last 7 days of H1 bars

```csharp
// svc — MT5Service instance
var bars = await svc.PriceBarsLast("EURUSD", "H1", 7);
Console.WriteLine($"{bars.Count} H1 bars");

var last = bars[^1];
Console.WriteLine($"O {last.Open:F5} H {last.High:F5} L {last.Low:F5} C {last.Close:F5}");
```

### 2) Simple moving average

```csharp
var bars = await svc.PriceBarsLast("EURUSD", "H1", 5);
if (bars.Count > 0)
    Console.WriteLine($"SMA(close) = {bars.Average(b => b.Close):F5}");
```

### 3) Daily range of the last day

```csharp
var daily = await svc.PriceBarsLast("EURUSD", "D1", 2);
var d = daily[^1];
Console.WriteLine($"Day range: {(d.High - d.Low):F5}");
```

---

## 🔗 Related Methods

* **Low-level:** [`PriceHistoryAsync()`](../../MT5Account/2.%20Symbol_information/PriceHistory.md)
* **Sugar:** `SymbolNamesAsync()` (symbol inventory), `GetSymbolSnapshot()` (current tick)
