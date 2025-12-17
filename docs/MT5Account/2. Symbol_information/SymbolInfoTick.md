# ✅ Getting Last Tick Data

> **Request:** current tick data from **MT5**. Get the latest price information (Bid, Ask, Last, Volume) for a symbol.

**API Information:**

* **SDK wrapper:** `MT5Account.SymbolInfoTickAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `SymbolInfoTick` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `SymbolInfoTick(SymbolInfoTickRequest) → SymbolInfoTickRequestReply`
* **Low‑level client (generated):** `MarketInfo.SymbolInfoTick(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace mt5_term_api
{
    public class MT5Account
    {
        public async Task<MrpcMqlTick> SymbolInfoTickAsync(
            string symbolName,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`SymbolInfoTickRequest { symbol: string }`


**Reply message:**

`SymbolInfoTickRequestReply { data: MrpcMqlTick }` or `{ error: Error }`

---

## 🔽 Input

| Parameter           | Type                | Required | Description                                               |
| ------------------- | ------------------- | -------- | --------------------------------------------------------- |
| `symbolName`        | `string`            | ✅       | Symbol name (e.g., `"EURUSD"`)                            |
| `deadline`          | `DateTime?`         | ❌       | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | ❌       | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `MrpcMqlTick`

| Field        | Type     | Description                                                              |
| ------------ | -------- | ------------------------------------------------------------------------ |
| `Time`       | `long`   | Time of the last prices update (Unix timestamp in seconds)               |
| `Bid`        | `double` | Current Bid price (sell price)                                           |
| `Ask`        | `double` | Current Ask price (buy price)                                            |
| `Last`       | `double` | Price of the last deal (Last)                                            |
| `Volume`     | `ulong`  | Volume for the current Last price                                        |
| `TimeMsc`    | `long`   | Time of a price last update in milliseconds (Unix timestamp)             |
| `Flags`      | `uint`   | Tick flags (see MQL5 docs for flag meanings)                             |
| `VolumeReal` | `double` | Volume for the current Last price with greater accuracy (fractional)     |

---

## 💬 Just the essentials

* **What it is.** Single RPC returning the latest tick (price quote) for a symbol.
* **Why you need it.** Get current market prices, calculate spreads, check last update time, verify quotes before trading.
* **Key fields.** `Bid` (sell price), `Ask` (buy price), `Time` (when updated).
* **Performance.** Fast snapshot of current market state.

---

## 🎯 Purpose

Use this method when you need to:

* Get current Bid/Ask prices before placing an order.
* Calculate the current spread (`Ask - Bid`).
* Check when the last price update occurred.
* Verify symbol has active quotes.
* Monitor price changes in real-time (polling).
* Get Last price and volume for exchange-traded instruments.

---

## 🧩 Notes & Tips

* **Bid** = price at which you can **sell** (broker buys from you).
* **Ask** = price at which you can **buy** (broker sells to you).
* **Spread** = `Ask - Bid` (broker's markup, in price units).
* `Time` is Unix timestamp in **seconds** - use `DateTimeOffset.FromUnixTimeSeconds(tick.Time)` to convert.
* `TimeMsc` provides millisecond precision for high-frequency trading.
* `Last` and `Volume` are primarily used for exchange-traded instruments (stocks, futures).
* For Forex, `Bid` and `Ask` are the main prices - `Last` may be 0.
* Ensure symbol is in Market Watch and synchronized before calling.
* Use short timeout (3-5s) as quotes update frequently.
* For real-time monitoring, consider using streaming API (`SubscribeToTicksAsync`) instead of polling.

---

## 🔗 Usage Examples

### 1) Get current Bid/Ask

```csharp
// Get current prices for EURUSD
var tick = await acc.SymbolInfoTickAsync(
    "EURUSD",
    deadline: DateTime.UtcNow.AddSeconds(3));

Console.WriteLine($"EURUSD:");
Console.WriteLine($"  Bid: {tick.Bid:F5}");
Console.WriteLine($"  Ask: {tick.Ask:F5}");
```

### 2) Calculate spread

```csharp
// Calculate current spread
var tick = await acc.SymbolInfoTickAsync("EURUSD");

double spread = tick.Ask - tick.Bid;
Console.WriteLine($"EURUSD Spread: {spread:F5} ({spread * 10000:F1} pips)");
```

### 3) Check quote freshness

```csharp
// Verify quote is recent
var tick = await acc.SymbolInfoTickAsync("GBPUSD");

var quoteTime = DateTimeOffset.FromUnixTimeSeconds(tick.Time).DateTime;
var age = DateTime.UtcNow - quoteTime;

Console.WriteLine($"Quote time: {quoteTime:yyyy-MM-dd HH:mm:ss} UTC");
Console.WriteLine($"Quote age:  {age.TotalSeconds:F1} seconds");

if (age.TotalSeconds > 10)
{
    Console.WriteLine("⚠️ Quote is stale (older than 10 seconds)");
}
else
{
    Console.WriteLine("✅ Quote is fresh");
}
```

### 4) Display multiple symbols

```csharp
// Show current prices for multiple symbols
var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY", "XAUUSD" };

Console.WriteLine("Current Market Prices:");
Console.WriteLine("Symbol      Bid         Ask         Spread");
Console.WriteLine("──────────────────────────────────────────────");

foreach (var sym in symbols)
{
    var tick = await acc.SymbolInfoTickAsync(sym);
    double spread = tick.Ask - tick.Bid;
    Console.WriteLine($"{sym,-10}  {tick.Bid,-10:F5}  {tick.Ask,-10:F5}  {spread:F5}");
}
```

### 5) Monitor price with millisecond precision

```csharp
// High-precision price monitoring
var tick = await acc.SymbolInfoTickAsync("EURUSD");

var timeMsc = DateTimeOffset.FromUnixTimeMilliseconds(tick.TimeMsc).DateTime;

Console.WriteLine($"EURUSD Tick:");
Console.WriteLine($"  Time (ms):   {timeMsc:yyyy-MM-dd HH:mm:ss.fff} UTC");
Console.WriteLine($"  Bid:         {tick.Bid:F5}");
Console.WriteLine($"  Ask:         {tick.Ask:F5}");
Console.WriteLine($"  Last:        {tick.Last:F5}");
Console.WriteLine($"  Volume:      {tick.Volume}");
Console.WriteLine($"  Volume Real: {tick.VolumeReal:F2}");
```
