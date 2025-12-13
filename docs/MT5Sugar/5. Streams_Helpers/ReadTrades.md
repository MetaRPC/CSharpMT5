# 📈 Reading Limited Trade Event Stream (`ReadTrades`)

> **Request:** Subscribe to trade events stream and read a limited number of events or until timeout.

## Overview

`ReadTrades` is a convenience wrapper over `OnTradeAsync` that automatically limits the stream by:
- Maximum number of events
- Duration timeout

This is useful for monitoring trade executions, fills, position changes, and order events without managing complex cancellation logic.

---

## Method Signature

```csharp
public static async IAsyncEnumerable<OnTradeData> ReadTrades(
    this MT5Service svc,
    int maxEvents = 20,
    int durationSec = 5,
    [EnumeratorCancellation] CancellationToken ct = default)
```

---

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `svc` | `MT5Service` | - | Extension method target |
| `maxEvents` | `int` | `20` | Maximum number of trade events to read before stopping |
| `durationSec` | `int` | `5` | Maximum duration in seconds before timeout |
| `ct` | `CancellationToken` | `default` | Optional cancellation token |

---

## Return Value

**Type:** `IAsyncEnumerable<OnTradeData>`

Returns an async stream of trade event data. The stream automatically stops when either:

1. `maxEvents` trade events have been received, **OR**
2. `durationSec` seconds have elapsed, **OR**
3. The cancellation token is triggered

---

## How It Works

1. Creates a linked cancellation token source from the provided `ct`
2. Sets a timeout of `durationSec` seconds
3. Subscribes to `OnTradeAsync` for all account trade events
4. Yields each trade event until:
   - Counter reaches `maxEvents`, or
   - Timeout is reached, or
   - Cancellation is requested
5. Automatically disposes the cancellation token source

---

## Common Use Cases

### 1️⃣ Monitor Recent Trade Activity
Watch for next 10 trade events within 30 seconds:

```csharp
Console.WriteLine("Monitoring trade events...\n");

await foreach (var trade in svc.ReadTrades(maxEvents: 10, durationSec: 30))
{
    Console.WriteLine($"[{trade.EventType}] {trade.Symbol} | Volume: {trade.Volume} | Price: {trade.Price:F5}");
}
```

### 2️⃣ Confirm Order Execution
Wait for specific trade event after placing order:

```csharp
// Place a market order
var orderRequest = new MrpcMqlTradeRequest
{
    Action = MRPC_ENUM_TRADE_REQUEST_ACTIONS.TradeActionDeal,
    Symbol = "EURUSD",
    Volume = 0.01,
    Price = ask,
    OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
    TypeFilling = MRPC_ENUM_ORDER_TYPE_FILLING.OrderFillingFok,
    TypeTime = MRPC_ENUM_ORDER_TYPE_TIME.OrderTimeGtc
};

var sendResult = await svc.OrderSendAsync(new OrderSendRequest { MqlTradeRequest = orderRequest });

Console.WriteLine($"Order sent. Ticket: {sendResult.Deal}. Waiting for trade event...");

// Monitor for confirmation
await foreach (var trade in svc.ReadTrades(maxEvents: 5, durationSec: 10))
{
    if (trade.Deal == sendResult.Deal)
    {
        Console.WriteLine($"✓ Trade confirmed! Deal: {trade.Deal}, Price: {trade.Price:F5}, Volume: {trade.Volume}");
        break;
    }
}
```

### 3️⃣ Log Trade Events for Debugging
Capture all trade events during a specific operation:

```csharp
var tradeLog = new List<OnTradeData>();

await foreach (var trade in svc.ReadTrades(maxEvents: 50, durationSec: 60))
{
    tradeLog.Add(trade);
    Console.WriteLine($"{trade.EventType,-20} | {trade.Symbol,-8} | Deal: {trade.Deal} | Order: {trade.Order}");
}

Console.WriteLine($"\nCaptured {tradeLog.Count} trade events.");
```

### 4️⃣ Wait for Position Close
Monitor trades until a specific position is closed:

```csharp
ulong positionTicket = 123456789; // Position ticket to monitor

await foreach (var trade in svc.ReadTrades(maxEvents: 100, durationSec: 120))
{
    Console.WriteLine($"Event: {trade.EventType} | Position: {trade.Position}");

    if (trade.EventType == "TRADE_ACTION_DEAL" && trade.Position == positionTicket)
    {
        Console.WriteLine($"✓ Position {positionTicket} closed! Deal: {trade.Deal}, Profit: {trade.Profit:F2}");
        break;
    }
}
```

### 5️⃣ Count Trades in Time Window
Count how many trades occur in a 30-second window:

```csharp
int tradeCount = 0;

await foreach (var trade in svc.ReadTrades(maxEvents: 1000, durationSec: 30))
{
    tradeCount++;
    Console.WriteLine($"Trade #{tradeCount}: {trade.EventType} on {trade.Symbol}");
}

Console.WriteLine($"\nTotal trades in 30 seconds: {tradeCount}");
```

---

## OnTradeData Structure

Each yielded event contains:

```csharp
public class OnTradeData
{
    public string EventType { get; set; }         // Event type (e.g., "TRADE_ACTION_DEAL")
    public string Symbol { get; set; }            // Symbol name
    public ulong Deal { get; set; }               // Deal ticket
    public ulong Order { get; set; }              // Order ticket
    public ulong Position { get; set; }           // Position ticket
    public double Volume { get; set; }            // Volume in lots
    public double Price { get; set; }             // Execution price
    public double Profit { get; set; }            // Profit/loss
    public double Commission { get; set; }        // Commission charged
    public double Swap { get; set; }              // Swap charged
    public string Comment { get; set; }           // Order/deal comment
    public Google.Protobuf.WellKnownTypes.Timestamp Time { get; set; }  // Event time
    // ... additional fields
}
```

---

## Trade Event Types

Common event types you'll see in `OnTradeData.EventType`:

| Event Type | Description |
|------------|-------------|
| `TRADE_ACTION_DEAL` | Market order executed (position opened/closed) |
| `TRADE_ACTION_PENDING` | Pending order placed |
| `TRADE_ACTION_SLTP` | Stop Loss / Take Profit modified |
| `TRADE_ACTION_MODIFY` | Pending order modified |
| `TRADE_ACTION_REMOVE` | Pending order cancelled |
| `TRADE_ACTION_CLOSE_BY` | Position closed by opposite position |

---

## Notes & Tips

- **Real-time monitoring:** Trade events arrive in real-time as they occur on the account
- **Auto-termination:** Stream stops automatically when limits are reached
- **Timeout handling:** If no trades occur within `durationSec`, stream terminates
- **Thread-safe:** Uses linked cancellation tokens for clean shutdown
- **Resource management:** Automatically disposes cancellation token source
- **Use case:** Perfect for order execution confirmation, debugging, and short-term monitoring
- **Production:** For long-running monitoring, use `OnTradeAsync` directly

---

## Comparison

| Feature | `ReadTrades` (Sugar) | `OnTradeAsync` (Low-level) |
|---------|----------------------|----------------------------|
| Auto-limit by count | ✅ Built-in | ❌ Manual counter needed |
| Auto-timeout | ✅ Built-in | ❌ Manual `CancelAfter` needed |
| Simplicity | ✅ One-liner | ❌ Requires setup code |
| Flexibility | ⚠️ Limited | ✅ Full control |
| Best for | Testing, confirmation | Production, continuous monitoring |

---

## Related Methods

- [ReadTicks](ReadTicks.md) — Similar helper for tick data streams
- `OnTradeAsync` (MT5Account) — Low-level trade event stream subscription
- `OnSymbolTickAsync` (MT5Account) — Low-level tick stream
- `OrderSendAsync` (MT5Account) — Place orders that generate trade events

---

## Example: Complete Trade Monitoring

```csharp
Console.WriteLine("=== Trade Event Monitor ===");
Console.WriteLine("Monitoring for 60 seconds or 20 events...\n");

var eventCounts = new Dictionary<string, int>();

await foreach (var trade in svc.ReadTrades(maxEvents: 20, durationSec: 60))
{
    // Count event types
    if (!eventCounts.ContainsKey(trade.EventType))
        eventCounts[trade.EventType] = 0;

    eventCounts[trade.EventType]++;

    // Display event details
    Console.WriteLine($"┌─ Trade Event #{eventCounts.Values.Sum()}");
    Console.WriteLine($"│  Type:     {trade.EventType}");
    Console.WriteLine($"│  Symbol:   {trade.Symbol}");
    Console.WriteLine($"│  Deal:     {trade.Deal}");
    Console.WriteLine($"│  Order:    {trade.Order}");
    Console.WriteLine($"│  Position: {trade.Position}");
    Console.WriteLine($"│  Volume:   {trade.Volume:F2} lots");
    Console.WriteLine($"│  Price:    {trade.Price:F5}");
    Console.WriteLine($"│  Profit:   {trade.Profit:F2}");
    Console.WriteLine($"│  Time:     {trade.Time}");
    Console.WriteLine($"└─────────────────────────────\n");
}

Console.WriteLine("\n=== Summary ===");
foreach (var (eventType, count) in eventCounts)
{
    Console.WriteLine($"{eventType}: {count} events");
}
```

**Sample Output:**
```
=== Trade Event Monitor ===
Monitoring for 60 seconds or 20 events...

┌─ Trade Event #1
│  Type:     TRADE_ACTION_DEAL
│  Symbol:   EURUSD
│  Deal:     123456789
│  Order:    987654321
│  Position: 555555555
│  Volume:   0.10 lots
│  Price:    1.08450
│  Profit:   0.00
│  Time:     2025-01-17 10:30:45
└─────────────────────────────

┌─ Trade Event #2
│  Type:     TRADE_ACTION_SLTP
│  Symbol:   EURUSD
│  Deal:     0
│  Order:    987654321
│  Position: 555555555
│  Volume:   0.10 lots
│  Price:    1.08450
│  Profit:   0.00
│  Time:     2025-01-17 10:31:02
└─────────────────────────────

=== Summary ===
TRADE_ACTION_DEAL: 1 events
TRADE_ACTION_SLTP: 1 events
```

---

## Advanced Pattern: Order Execution Tracker

```csharp
public static async Task<OnTradeData?> WaitForOrderFill(
    MT5Service svc,
    ulong orderTicket,
    int timeoutSec = 30)
{
    await foreach (var trade in svc.ReadTrades(maxEvents: 100, durationSec: timeoutSec))
    {
        if (trade.Order == orderTicket && trade.EventType == "TRADE_ACTION_DEAL")
        {
            return trade; // Found the fill event
        }
    }

    return null; // Timeout - order not filled
}

// Usage:
var result = await svc.OrderSendAsync(orderRequest);
var fillEvent = await WaitForOrderFill(svc, result.Order, timeoutSec: 10);

if (fillEvent != null)
{
    Console.WriteLine($"✓ Order filled at {fillEvent.Price:F5}");
}
else
{
    Console.WriteLine("⚠️ Order fill timeout");
}
```
