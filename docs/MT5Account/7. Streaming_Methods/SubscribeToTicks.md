# ✅ Subscribe to Real-Time Tick Data (`OnSymbolTickAsync`)

> **Stream:** Real-time price tick updates for specified symbols on **MT5**. Returns continuous stream of bid/ask prices as they change.

**API Information:**

* **SDK wrapper:** `MT5Service.OnSymbolTickAsync(...)` (from class `MT5Service`)
* **gRPC service:** `mt5_term_api.SubscriptionService`
* **Proto definition:** `OnSymbolTick` (defined in `mt5-term-api-subscriptions.proto`)

### RPC

* **Service:** `mt5_term_api.SubscriptionService`
* **Method:** `OnSymbolTick(OnSymbolTickRequest) → stream OnSymbolTickReply`
* **Low‑level client (generated):** `SubscriptionService.SubscriptionServiceClient.OnSymbolTick(request, headers, deadline, cancellationToken)`
* **SDK wrapper:**

```csharp
namespace mt5_term_api
{
    public class MT5Service
    {
        public async IAsyncEnumerable<OnSymbolTickData> OnSymbolTickAsync(
            string[] symbols,
            [EnumeratorCancellation] CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`OnSymbolTickRequest { symbol_names }`
**Reply message (stream):** `OnSymbolTickReply { data: OnSymbolTickData }`

---

## 🔽 Input

| Parameter            | Type                  | Description                                         |
| -------------------- | --------------------- | --------------------------------------------------- |
| `symbols`            | `string[]`            | Array of symbol names (e.g., `["EURUSD", "GBPUSD"]`) |
| `cancellationToken`  | `CancellationToken`   | Token to stop the stream                            |

---

## ⬆️ Output — `OnSymbolTickData` (stream)

| Field                    | Type                        | Description                              |
| ------------------------ | --------------------------- | ---------------------------------------- |
| `SymbolTick`             | `MrpcSubscriptionMqlTick`   | Tick data for symbol                     |
| `TerminalInstanceGuidId` | `string`                    | Terminal instance ID                     |

### `MrpcSubscriptionMqlTick` — Tick structure

| Field        | Type        | Description                                    |
| ------------ | ----------- | ---------------------------------------------- |
| `Time`       | `Timestamp` | Time of last price update (UTC timestamp)      |
| `Bid`        | `double`    | Current Bid price                              |
| `Ask`        | `double`    | Current Ask price                              |
| `Last`       | `double`    | Price of last deal                             |
| `Volume`     | `uint64`    | Volume for current Last price                  |
| `TimeMsc`    | `int64`     | Time of price update in milliseconds           |
| `Flags`      | `uint32`    | Tick flags                                     |
| `VolumeReal` | `double`    | Real volume with greater accuracy              |
| `Symbol`     | `string`    | Symbol name                                    |

---

## 💬 Just the essentials

* **What it is.** Real-time streaming of price ticks. Continuously sends bid/ask updates as prices change on server.
* **Why you need it.** Build real-time price monitoring, tick charts, high-frequency strategies, market scanners.
* **Sanity check.** Stream runs until cancelled. Use `CancellationToken` to stop. Handle `OperationCanceledException` when stopping.

---

## 🎯 Purpose

Use it for real-time price monitoring:

* Real-time price displays and charts.
* High-frequency trading strategies.
* Market scanners monitoring multiple symbols.
* Price alert systems.
* Tick-by-tick data collection.

---

## 🧩 Notes & Tips

* **Streaming model:** Use `await foreach` to process events. Stream runs continuously until cancelled.
* **Cancellation:** Always use `CancellationToken` to stop stream gracefully. Call `cts.Cancel()` or `cts.CancelAfter(timeout)`.
* **Multiple symbols:** Can subscribe to multiple symbols in one stream. Ticks arrive as they happen for any subscribed symbol.
* **Performance:** Ticks arrive in real-time. Keep processing logic fast to avoid blocking stream.
* **Error handling:** Wrap in try-catch. Handle `OperationCanceledException` (normal when stopping).
* **Parallel streams:** Can run multiple streams in parallel using `Task.WhenAll()`.
* **Time format:** `TimeMsc` is Unix timestamp in milliseconds. Convert using `DateTimeOffset.FromUnixTimeMilliseconds()`.

---

## 🔗 Usage Examples

### 1) Basic tick streaming

```csharp
// svc — MT5Service instance

var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(10)); // Stop after 10 seconds

try
{
    await foreach (var tick in svc.OnSymbolTickAsync(new[] { "EURUSD" }, cts.Token))
    {
        var t = tick.SymbolTick;
        Console.WriteLine($"{t.Symbol}: Bid={t.Bid:F5} | Ask={t.Ask:F5}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Stream stopped");
}
```

---

### 2) Monitor multiple symbols

```csharp
var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY" };
var cts = new CancellationTokenSource();

await foreach (var tick in svc.OnSymbolTickAsync(symbols, cts.Token))
{
    var t = tick.SymbolTick;
    var time = DateTimeOffset.FromUnixTimeMilliseconds(t.TimeMsc).DateTime;

    Console.WriteLine($"[{time:HH:mm:ss.fff}] {t.Symbol,-7} | Bid: {t.Bid:F5} | Ask: {t.Ask:F5} | Spread: {(t.Ask - t.Bid):F5}");
}
```

---

### 3) Price alert system

```csharp
var symbol = "XAUUSD";
var alertLevel = 2000.0;

var cts = new CancellationTokenSource();

await foreach (var tick in svc.OnSymbolTickAsync(new[] { symbol }, cts.Token))
{
    var t = tick.SymbolTick;

    if (t.Bid >= alertLevel)
    {
        Console.WriteLine($"⚠ ALERT: {symbol} reached ${alertLevel}!");
        Console.WriteLine($"   Bid: {t.Bid:F2} | Ask: {t.Ask:F2}");

        // Stop stream after alert
        cts.Cancel();
    }
}
```

---

### 4) Collect tick data with limit

```csharp
var symbols = new[] { "EURUSD", "GBPUSD" };
var maxTicks = 100;
var tickCount = 0;

var cts = new CancellationTokenSource();

await foreach (var tick in svc.OnSymbolTickAsync(symbols, cts.Token))
{
    tickCount++;
    var t = tick.SymbolTick;

    Console.WriteLine($"Tick #{tickCount}: {t.Symbol} @ {t.Bid:F5}");

    if (tickCount >= maxTicks)
    {
        Console.WriteLine($"Collected {maxTicks} ticks, stopping...");
        break; // Exit loop (stream stops automatically)
    }
}
```

---

### 5) Calculate spread statistics

```csharp
var symbol = "EURUSD";
var spreads = new List<double>();
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(60)); // Collect for 1 minute

try
{
    await foreach (var tick in svc.OnSymbolTickAsync(new[] { symbol }, cts.Token))
    {
        var t = tick.SymbolTick;
        var spread = (t.Ask - t.Bid) * 100000; // Spread in pips (for 5-digit pairs)

        spreads.Add(spread);

        if (spreads.Count % 10 == 0)
        {
            var avgSpread = spreads.Average();
            var minSpread = spreads.Min();
            var maxSpread = spreads.Max();

            Console.WriteLine($"Spreads: Avg={avgSpread:F1} | Min={minSpread:F1} | Max={maxSpread:F1} pips");
        }
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine($"\nFinal statistics ({spreads.Count} ticks):");
    Console.WriteLine($"  Average spread: {spreads.Average():F2} pips");
    Console.WriteLine($"  Min spread: {spreads.Min():F2} pips");
    Console.WriteLine($"  Max spread: {spreads.Max():F2} pips");
}
```

---

### 6) Run multiple streams in parallel

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(30));

// Task 1: Monitor EURUSD
var task1 = Task.Run(async () =>
{
    await foreach (var tick in svc.OnSymbolTickAsync(new[] { "EURUSD" }, cts.Token))
    {
        Console.WriteLine($"[EUR] {tick.SymbolTick.Bid:F5}");
    }
}, cts.Token);

// Task 2: Monitor GBPUSD
var task2 = Task.Run(async () =>
{
    await foreach (var tick in svc.OnSymbolTickAsync(new[] { "GBPUSD" }, cts.Token))
    {
        Console.WriteLine($"[GBP] {tick.SymbolTick.Bid:F5}");
    }
}, cts.Token);

try
{
    await Task.WhenAll(task1, task2);
}
catch (OperationCanceledException)
{
    Console.WriteLine("All streams stopped");
}
```

---

### 7) Using ReadTicks extension helper (convenience wrapper)

```csharp
// Extension method with built-in limits
int maxEvents = 20;
int durationSec = 5;

await foreach (var tick in svc.ReadTicks(
    symbols: new[] { "EURUSD", "GBPUSD" },
    maxEvents: maxEvents,
    durationSec: durationSec))
{
    var t = tick.SymbolTick;
    Console.WriteLine($"{t.Symbol}: {t.Bid:F5}");

    // Stream automatically stops after 20 events OR 5 seconds (whichever comes first)
}
```

---

### 8) Tick-based trading signal

```csharp
var symbol = "EURUSD";
var cts = new CancellationTokenSource();

double? previousBid = null;
int consecutiveUps = 0;

await foreach (var tick in svc.OnSymbolTickAsync(new[] { symbol }, cts.Token))
{
    var t = tick.SymbolTick;

    if (previousBid.HasValue)
    {
        if (t.Bid > previousBid)
        {
            consecutiveUps++;

            if (consecutiveUps >= 5)
            {
                Console.WriteLine($"🟢 BUY SIGNAL: {symbol} price increased {consecutiveUps} ticks in a row");
                consecutiveUps = 0; // Reset counter
            }
        }
        else
        {
            consecutiveUps = 0;
        }
    }

    previousBid = t.Bid;
}
```
