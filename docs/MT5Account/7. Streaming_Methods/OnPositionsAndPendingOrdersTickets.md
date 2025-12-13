# ✅ Subscribe to Position/Order Tickets Snapshots (`OnPositionsAndPendingOrdersTicketsAsync`)

> **Stream:** Periodic snapshots of all position and pending order tickets on **MT5**. Returns list of active tickets at specified interval.

**API Information:**

* **SDK wrapper:** `MT5Service.OnPositionsAndPendingOrdersTicketsAsync(...)` (from class `MT5Service`)
* **gRPC service:** `mt5_term_api.SubscriptionService`
* **Proto definition:** `OnPositionsAndPendingOrdersTickets` (defined in `mt5-term-api-subscriptions.proto`)

### RPC

* **Service:** `mt5_term_api.SubscriptionService`
* **Method:** `OnPositionsAndPendingOrdersTickets(OnPositionsAndPendingOrdersTicketsRequest) → stream OnPositionsAndPendingOrdersTicketsReply`
* **Low‑level client (generated):** `SubscriptionService.SubscriptionServiceClient.OnPositionsAndPendingOrdersTickets(request, headers, deadline, cancellationToken)`
* **SDK wrapper:**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Service
    {
        public async IAsyncEnumerable<OnPositionsAndPendingOrdersTicketsData> OnPositionsAndPendingOrdersTicketsAsync(
            int intervalMs = 1000,
            [EnumeratorCancellation] CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`OnPositionsAndPendingOrdersTicketsRequest { timer_period_milliseconds }`

**Reply message (stream):**

`OnPositionsAndPendingOrdersTicketsReply { data: OnPositionsAndPendingOrdersTicketsData }`

---

## 🔽 Input

| Parameter            | Type                  | Description                                                  |
| -------------------- | --------------------- | ------------------------------------------------------------ |
| `intervalMs`         | `int`                 | Update interval in milliseconds (default: 1000ms = 1 second) |
| `cancellationToken`  | `CancellationToken`   | Token to stop the stream                                     |

---

## ⬆️ Output — `OnPositionsAndPendingOrdersTicketsData` (stream)

| Field                    | Type               | Description                                    |
| ------------------------ | ------------------ | ---------------------------------------------- |
| `PositionTickets`        | `List<uint64>`     | Array of all open position tickets             |
| `PendingOrderTickets`    | `List<uint64>`     | Array of all pending order tickets             |
| `ServerTime`             | `Timestamp`        | Server time of snapshot                        |
| `TerminalInstanceGuidId` | `string`           | Terminal instance ID                           |

---

## 💬 Just the essentials

* **What it is.** Periodic polling-based stream that returns lists of all active position and pending order tickets. Lightweight snapshot of account activity.
* **Why you need it.** Quick overview of open positions/orders, detect when positions open/close, monitor account activity without full position details.
* **Sanity check.** Stream sends updates every `intervalMs` milliseconds. Compare ticket lists between updates to detect opens/closes.

---

## 🎯 Purpose

Use it for lightweight account monitoring:

* Quick check of open positions and pending orders.
* Detect position opens/closes by comparing tickets.
* Monitor number of active positions.
* Lightweight alternative to full position data.
* Build watchlists of active tickets.

---

## 🧩 Notes & Tips

* **Polling-based:** Polls at fixed interval (default 1000ms = 1 second). Not event-driven.
* **Tickets only:** Returns only ticket numbers, not full position/order details. Use `OpenedOrdersAsync()` to get full details.
* **Lightweight:** Very efficient - minimal data transfer. Use for quick checks.
* **Comparison logic:** Compare ticket arrays between snapshots to detect changes.
* **Use cases:** 1s interval for monitoring, 5s for dashboards, 100ms for high-frequency checks.
* **Server time:** Each snapshot includes server time for synchronization.

---

## 🔗 Usage Examples

### 1) Basic ticket monitoring

```csharp
// svc — MT5Service instance

var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(30)); // Monitor for 30 seconds

try
{
    await foreach (var snapshot in svc.OnPositionsAndPendingOrdersTicketsAsync(
        intervalMs: 1000,
        cts.Token))
    {
        Console.WriteLine($"[{snapshot.ServerTime.ToDateTime():HH:mm:ss}] " +
                         $"Positions: {snapshot.PositionTickets.Count} | " +
                         $"Pending: {snapshot.PendingOrderTickets.Count}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Stream stopped");
}
```

---

### 2) Detect position opens/closes

```csharp
var cts = new CancellationTokenSource();
HashSet<ulong>? previousPositionTickets = null;

await foreach (var snapshot in svc.OnPositionsAndPendingOrdersTicketsAsync(intervalMs: 1000, cts.Token))
{
    var currentTickets = new HashSet<ulong>(snapshot.PositionTickets);

    if (previousPositionTickets != null)
    {
        // Detect new positions
        var newPositions = currentTickets.Except(previousPositionTickets);
        foreach (var ticket in newPositions)
        {
            Console.WriteLine($"🆕 New position opened: #{ticket}");
        }

        // Detect closed positions
        var closedPositions = previousPositionTickets.Except(currentTickets);
        foreach (var ticket in closedPositions)
        {
            Console.WriteLine($"✅ Position closed: #{ticket}");
        }
    }

    previousPositionTickets = currentTickets;
}
```

---

### 3) Monitor pending order placement/cancellation

```csharp
var cts = new CancellationTokenSource();
HashSet<ulong>? previousPendingTickets = null;

await foreach (var snapshot in svc.OnPositionsAndPendingOrdersTicketsAsync(intervalMs: 1000, cts.Token))
{
    var currentTickets = new HashSet<ulong>(snapshot.PendingOrderTickets);

    if (previousPendingTickets != null)
    {
        // Detect new pending orders
        var newOrders = currentTickets.Except(previousPendingTickets);
        foreach (var ticket in newOrders)
        {
            Console.WriteLine($"📝 New pending order placed: #{ticket}");
        }

        // Detect cancelled/executed pending orders
        var removedOrders = previousPendingTickets.Except(currentTickets);
        foreach (var ticket in removedOrders)
        {
            Console.WriteLine($"❌ Pending order removed: #{ticket}");
        }
    }

    previousPendingTickets = currentTickets;
}
```

---

### 4) Count-based monitoring

```csharp
var maxPositions = 5;
var cts = new CancellationTokenSource();

await foreach (var snapshot in svc.OnPositionsAndPendingOrdersTicketsAsync(intervalMs: 1000, cts.Token))
{
    var posCount = snapshot.PositionTickets.Count;
    var pendingCount = snapshot.PendingOrderTickets.Count;

    Console.WriteLine($"[{snapshot.ServerTime.ToDateTime():HH:mm:ss}] " +
                     $"Positions: {posCount}/{maxPositions} | Pending: {pendingCount}");

    if (posCount >= maxPositions)
    {
        Console.WriteLine($"⚠ WARNING: Maximum position limit ({maxPositions}) reached!");
    }

    if (posCount == 0 && pendingCount == 0)
    {
        Console.WriteLine("✓ No open positions or pending orders");
    }
}
```

---

### 5) Track position activity rate

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromMinutes(1)); // Track for 1 minute

int totalOpens = 0;
int totalCloses = 0;
HashSet<ulong>? previousTickets = null;

try
{
    await foreach (var snapshot in svc.OnPositionsAndPendingOrdersTicketsAsync(intervalMs: 1000, cts.Token))
    {
        if (previousTickets != null)
        {
            var currentTickets = new HashSet<ulong>(snapshot.PositionTickets);

            var opens = currentTickets.Except(previousTickets).Count();
            var closes = previousTickets.Except(currentTickets).Count();

            totalOpens += opens;
            totalCloses += closes;

            if (opens > 0 || closes > 0)
            {
                Console.WriteLine($"Activity: +{opens} opened, -{closes} closed");
            }
        }

        previousTickets = new HashSet<ulong>(snapshot.PositionTickets);
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine($"\n1-minute statistics:");
    Console.WriteLine($"  Total positions opened: {totalOpens}");
    Console.WriteLine($"  Total positions closed: {totalCloses}");
    Console.WriteLine($"  Net change: {totalOpens - totalCloses}");
}
```

---

### 6) Simple dashboard

```csharp
var cts = new CancellationTokenSource();

await foreach (var snapshot in svc.OnPositionsAndPendingOrdersTicketsAsync(intervalMs: 2000, cts.Token))
{
    Console.Clear();
    Console.WriteLine("═══════════════════════════════════════");
    Console.WriteLine("      ACCOUNT ACTIVITY DASHBOARD       ");
    Console.WriteLine("═══════════════════════════════════════");
    Console.WriteLine();

    Console.WriteLine($"Server Time:       {snapshot.ServerTime.ToDateTime():yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"Open Positions:    {snapshot.PositionTickets.Count}");
    Console.WriteLine($"Pending Orders:    {snapshot.PendingOrderTickets.Count}");
    Console.WriteLine();

    if (snapshot.PositionTickets.Count > 0)
    {
        Console.WriteLine("Position Tickets:");
        foreach (var ticket in snapshot.PositionTickets.Take(10))
        {
            Console.WriteLine($"  • #{ticket}");
        }
        if (snapshot.PositionTickets.Count > 10)
        {
            Console.WriteLine($"  ... and {snapshot.PositionTickets.Count - 10} more");
        }
    }

    Console.WriteLine();
    Console.WriteLine($"Last update: {DateTime.Now:HH:mm:ss}");
}
```

---

### 7) Alert on first position open

```csharp
var cts = new CancellationTokenSource();
bool alerted = false;

await foreach (var snapshot in svc.OnPositionsAndPendingOrdersTicketsAsync(intervalMs: 500, cts.Token))
{
    if (!alerted && snapshot.PositionTickets.Count > 0)
    {
        Console.WriteLine($"🔔 ALERT: First position detected!");
        Console.WriteLine($"   Ticket: #{snapshot.PositionTickets[0]}");
        Console.WriteLine($"   Time: {snapshot.ServerTime.ToDateTime():HH:mm:ss}");

        alerted = true;
        cts.Cancel(); // Stop monitoring after first position
    }
}
```

---

### 8) Combine with full position data

```csharp
// acc — MT5Account instance for full data
// svc — MT5Service instance for streaming

var cts = new CancellationTokenSource();

await foreach (var snapshot in svc.OnPositionsAndPendingOrdersTicketsAsync(intervalMs: 2000, cts.Token))
{
    Console.WriteLine($"\nSnapshot at {snapshot.ServerTime.ToDateTime():HH:mm:ss}:");
    Console.WriteLine($"Found {snapshot.PositionTickets.Count} position(s)");

    if (snapshot.PositionTickets.Count > 0)
    {
        // Get full details for all positions
        var fullData = await acc.OpenedOrdersAsync();

        Console.WriteLine("\nPosition Details:");
        foreach (var position in fullData.Positions)
        {
            Console.WriteLine($"  #{position.Ticket}: {position.Symbol} " +
                             $"{position.Volume} lots @ {position.PriceOpen} " +
                             $"(P/L: ${position.Profit:F2})");
        }
    }
}
```

---

### 9) High-frequency check (100ms polling)

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(10)); // 10 seconds of HF monitoring

int snapshotCount = 0;

try
{
    await foreach (var snapshot in svc.OnPositionsAndPendingOrdersTicketsAsync(intervalMs: 100, cts.Token))
    {
        snapshotCount++;

        Console.WriteLine($"[{snapshotCount,4}] " +
                         $"Pos: {snapshot.PositionTickets.Count,2} | " +
                         $"Pending: {snapshot.PendingOrderTickets.Count,2}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine($"\nCollected {snapshotCount} snapshots in 10 seconds");
    Console.WriteLine($"Average rate: {snapshotCount / 10.0:F1} snapshots/second");
}
```
