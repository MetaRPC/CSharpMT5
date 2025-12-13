# 📊 Reading Limited Tick Stream (`ReadTicks`)

> **Request:** Subscribe to symbol tick stream and read a limited number of events or until timeout.

## Overview

`ReadTicks` is a convenience wrapper over `OnSymbolTickAsync` that automatically limits the stream by:
- Maximum number of events
- Duration timeout

This is useful for testing, sampling, or short-term monitoring without having to manually manage cancellation tokens and counters.

---

## Method Signature

```csharp
public static async IAsyncEnumerable<OnSymbolTickData> ReadTicks(
    this MT5Service svc,
    IEnumerable<string> symbols,
    int maxEvents = 50,
    int durationSec = 5,
    [EnumeratorCancellation] CancellationToken ct = default)
```

---

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `svc` | `MT5Service` | - | Extension method target |
| `symbols` | `IEnumerable<string>` | - | Symbols to subscribe to (e.g., `["EURUSD", "GBPUSD"]`) |
| `maxEvents` | `int` | `50` | Maximum number of tick events to read before stopping |
| `durationSec` | `int` | `5` | Maximum duration in seconds before timeout |
| `ct` | `CancellationToken` | `default` | Optional cancellation token |

---

## Return Value

**Type:** `IAsyncEnumerable<OnSymbolTickData>`

Returns an async stream of tick data events. The stream automatically stops when either:

1. `maxEvents` ticks have been received, **OR**
2. `durationSec` seconds have elapsed, **OR**
3. The cancellation token is triggered

---

## How It Works

1. Creates a linked cancellation token source from the provided `ct`
2. Sets a timeout of `durationSec` seconds
3. Subscribes to `OnSymbolTickAsync` for the specified symbols
4. Yields each tick event until:
   - Counter reaches `maxEvents`, or
   - Timeout is reached, or
   - Cancellation is requested
5. Automatically disposes the cancellation token source

---

## Common Use Cases

### 1️⃣ Quick Tick Sample
Capture first 10 ticks from EURUSD within 10 seconds:

```csharp
await foreach (var tick in svc.ReadTicks(new[] { "EURUSD" }, maxEvents: 10, durationSec: 10))
{
    Console.WriteLine($"[{tick.Symbol}] Bid: {tick.Tick.Bid:F5}, Ask: {tick.Tick.Ask:F5}");
}
```

### 2️⃣ Multi-Symbol Monitoring
Monitor multiple symbols for 30 seconds or until 100 ticks:

```csharp
var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY" };

await foreach (var tick in svc.ReadTicks(symbols, maxEvents: 100, durationSec: 30))
{
    Console.WriteLine($"{tick.Symbol} | Bid: {tick.Tick.Bid:F5} | Ask: {tick.Tick.Ask:F5} | Time: {tick.Tick.Time}");
}

Console.WriteLine("Tick sampling complete.");
```

### 3️⃣ Calculate Average Spread
Collect 20 ticks and calculate average spread:

```csharp
var ticks = new List<OnSymbolTickData>();

await foreach (var tick in svc.ReadTicks(new[] { "EURUSD" }, maxEvents: 20, durationSec: 10))
{
    ticks.Add(tick);
}

var avgSpread = ticks.Average(t => t.Tick.Ask - t.Tick.Bid);
Console.WriteLine($"Average spread over {ticks.Count} ticks: {avgSpread:F5}");
```

### 4️⃣ Detect Price Movement
Monitor ticks until price moves by 10 pips or 30 seconds pass:

```csharp
double? firstBid = null;
const double targetMovementPips = 0.0010; // 10 pips for EURUSD

await foreach (var tick in svc.ReadTicks(new[] { "EURUSD" }, maxEvents: 1000, durationSec: 30))
{
    firstBid ??= tick.Tick.Bid;

    var movement = Math.Abs(tick.Tick.Bid - firstBid.Value);
    Console.WriteLine($"Bid: {tick.Tick.Bid:F5}, Movement: {movement:F5}");

    if (movement >= targetMovementPips)
    {
        Console.WriteLine($"Target movement reached! Moved {movement:F5}");
        break;
    }
}
```

### 5️⃣ Early Cancellation
User can cancel early with custom cancellation token:

```csharp
using var cts = new CancellationTokenSource();

// Cancel after 3 seconds (earlier than durationSec)
cts.CancelAfter(TimeSpan.FromSeconds(3));

try
{
    await foreach (var tick in svc.ReadTicks(new[] { "EURUSD" }, maxEvents: 100, durationSec: 10, ct: cts.Token))
    {
        Console.WriteLine($"Bid: {tick.Tick.Bid:F5}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Stream cancelled by user.");
}
```

---

## OnSymbolTickData Structure

Each yielded event contains:

```csharp
public class OnSymbolTickData
{
    public string Symbol { get; set; }           // Symbol name (e.g., "EURUSD")
    public MrpcMqlTick Tick { get; set; }         // Tick data
}

public class MrpcMqlTick
{
    public Google.Protobuf.WellKnownTypes.Timestamp Time { get; set; }  // Server time
    public double Bid { get; set; }                // Current Bid price
    public double Ask { get; set; }                // Current Ask price
    public double Last { get; set; }               // Price of last deal (Bid)
    public ulong Volume { get; set; }              // Volume for current Last price
    public long TimeMs { get; set; }               // Time in milliseconds since Unix epoch
    public uint Flags { get; set; }                // Tick flags (buy/sell)
    public double VolumeReal { get; set; }         // Volume for current Last with more precision
}
```

---

## Notes & Tips

- **Auto-termination:** Stream stops automatically when limits are reached—no need for manual `break` statements
- **Timeout priority:** If timeout is reached before `maxEvents`, stream stops early
- **Thread-safe:** Uses linked cancellation tokens to ensure clean shutdown
- **Resource management:** Automatically disposes cancellation token source via `using`
- **Use case:** Perfect for testing, debugging, and short-term monitoring
- **Production:** For long-running streams, use `OnSymbolTickAsync` directly with custom logic

---

## Comparison

| Feature | `ReadTicks` (Sugar) | `OnSymbolTickAsync` (Low-level) |
|---------|---------------------|----------------------------------|
| Auto-limit by count | ✅ Built-in | ❌ Manual counter needed |
| Auto-timeout | ✅ Built-in | ❌ Manual `CancelAfter` needed |
| Simplicity | ✅ One-liner | ❌ Requires setup code |
| Flexibility | ⚠️ Limited | ✅ Full control |
| Best for | Testing, sampling | Production, long-running |

---

## Related Methods

- [ReadTrades](ReadTrades.md) — Similar helper for trade event streams
- `OnSymbolTickAsync` (MT5Account) — Low-level tick stream subscription
- `OnTradeAsync` (MT5Account) — Low-level trade event stream
- `SymbolInfoTickAsync` (MT5Account) — Get single current tick (non-streaming)

---

## Example: Complete Tick Analysis

```csharp
var symbols = new[] { "EURUSD", "GBPUSD" };
var tickCounts = new Dictionary<string, int>();

Console.WriteLine("Starting tick monitoring for 15 seconds or 50 ticks...\n");

await foreach (var tick in svc.ReadTicks(symbols, maxEvents: 50, durationSec: 15))
{
    if (!tickCounts.ContainsKey(tick.Symbol))
        tickCounts[tick.Symbol] = 0;

    tickCounts[tick.Symbol]++;

    var spread = tick.Tick.Ask - tick.Tick.Bid;
    Console.WriteLine($"{tick.Symbol,-8} | Bid: {tick.Tick.Bid:F5} | Ask: {tick.Tick.Ask:F5} | Spread: {spread:F5}");
}

Console.WriteLine("\n--- Summary ---");
foreach (var (symbol, count) in tickCounts)
{
    Console.WriteLine($"{symbol}: {count} ticks received");
}
```

**Sample Output:**
```
Starting tick monitoring for 15 seconds or 50 ticks...

EURUSD   | Bid: 1.08450 | Ask: 1.08452 | Spread: 0.00002
GBPUSD   | Bid: 1.26320 | Ask: 1.26323 | Spread: 0.00003
EURUSD   | Bid: 1.08451 | Ask: 1.08453 | Spread: 0.00002
...

--- Summary ---
EURUSD: 28 ticks received
GBPUSD: 22 ticks received
```
