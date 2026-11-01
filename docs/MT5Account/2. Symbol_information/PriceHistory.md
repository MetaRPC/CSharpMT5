# ✅ Historical Price Bars (`PriceHistoryAsync`)

> **Request:** Get historical OHLC bars for a symbol and timeframe over a time window on **MT5**.

**API Information:**

* **SDK wrapper:** `MT5Account.PriceHistoryAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `PriceHistory` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `PriceHistory(PriceHistoryRequest) → PriceHistoryReply`
* **Low‑level client (generated):** `MarketInfo.MarketInfoClient.PriceHistory(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<PriceHistoryData> PriceHistoryAsync(
            PriceHistoryRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`PriceHistoryRequest { symbol, timeframe, from_time, to_time }`


**Reply message:**

`PriceHistoryReply { data: PriceHistoryData }`

---

## 🔽 Input

| Parameter           | Type                  | Description                                               |
| ------------------- | --------------------- | --------------------------------------------------------- |
| `request`           | `PriceHistoryRequest` | Protobuf request with symbol, timeframe and time range    |
| `deadline`          | `DateTime?`           | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`   | Cooperative cancel for the call/retry loop                |

### `PriceHistoryRequest`

| Field        | Type     | Description                                                                 |
| ------------ | -------- | --------------------------------------------------------------------------- |
| `Symbol`     | `string` | Trading symbol                                                              |
| `Timeframe`  | `int`    | Raw **MQL5 `ENUM_TIMEFRAMES`** value (see table below)                      |
| `FromTime`   | `long`   | Window start, **Unix seconds**. Leave `From`/`To` at `0` for newest bars    |
| `ToTime`     | `long`   | Window end, **Unix seconds**                                               |

#### Common `Timeframe` values (MQL5 `ENUM_TIMEFRAMES`)

| Timeframe | Value |
| --------- | ----- |
| `PERIOD_M1`  | `1`     |
| `PERIOD_M5`  | `5`     |
| `PERIOD_M15` | `15`    |
| `PERIOD_M30` | `30`    |
| `PERIOD_H1`  | `16385` |
| `PERIOD_H4`  | `16388` |
| `PERIOD_D1`  | `16408` |
| `PERIOD_W1`  | `32769` |
| `PERIOD_MN1` | `49153` |

---

## ⬆️ Output — `PriceHistoryData`

| Field  | Type                 | Description            |
| ------ | -------------------- | ---------------------- |
| `Bars` | `repeated PriceBar`  | OHLC bars in the window |

### `PriceBar`

| Field        | Type     | Description                         |
| ------------ | -------- | ----------------------------------- |
| `Time`       | `long`   | Bar open time (**Unix seconds**)    |
| `Open`       | `double` | Open price                          |
| `High`       | `double` | High price                          |
| `Low`        | `double` | Low price                           |
| `Close`      | `double` | Close price                         |
| `TickVolume` | `long`   | Tick volume                         |
| `Spread`     | `int`    | Spread in points                    |
| `RealVolume` | `long`   | Real (exchange) volume, if provided |

---

## 💬 Just the essentials

* **What it is.** OHLC candles for a symbol over a time window.
* **Why you need it.** Indicators, backtests, charts — anything needing bar history.
* **Timeframe is raw:** Pass the MQL5 enum value (`H1 = 16385`), not minutes.

---

## 🎯 Purpose

Use it to pull candles for analysis:

* Compute indicators (MA, RSI) over recent bars.
* Seed a chart or a backtest window.
* Detect ranges/breakouts from the last N bars.

---

## 🧩 Notes & Tips

* **Unix seconds:** Convert with `DateTimeOffset.ToUnixTimeSeconds()`; read back with `DateTimeOffset.FromUnixTimeSeconds(bar.Time)`.
* **Newest bars shortcut:** Leaving `FromTime`/`ToTime` at `0` returns the newest bars — handy for "just give me the latest".
* **Select first:** Ensure the symbol is in Market Watch ([`SymbolSelect`](./SymbolSelect.md)) so history is available.
* **Friendly wrapper:** `MT5Sugar.PriceBarsLast(symbol, "H1", days)` accepts a string timeframe and a days window.

---

## 🔗 Usage Examples

### 1) Last 7 days of H1 bars

```csharp
// acc — connected MT5Account
var req = new PriceHistoryRequest
{
    Symbol    = "EURUSD",
    Timeframe = 16385, // PERIOD_H1
    FromTime  = DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds(),
    ToTime    = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};

var data = await acc.PriceHistoryAsync(req);
Console.WriteLine($"Bars: {data.Bars.Count}");

var last = data.Bars[^1];
Console.WriteLine($"{DateTimeOffset.FromUnixTimeSeconds(last.Time).UtcDateTime}: " +
                  $"O {last.Open:F5} H {last.High:F5} L {last.Low:F5} C {last.Close:F5}");
```

---

### 2) Simple moving average over the window

```csharp
var data = await acc.PriceHistoryAsync(new PriceHistoryRequest
{
    Symbol = "EURUSD", Timeframe = 16385,
    FromTime = DateTimeOffset.UtcNow.AddDays(-5).ToUnixTimeSeconds(),
    ToTime   = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
});

if (data.Bars.Count > 0)
{
    double sma = data.Bars.Average(b => b.Close);
    Console.WriteLine($"SMA(close) over {data.Bars.Count} bars = {sma:F5}");
}
```
