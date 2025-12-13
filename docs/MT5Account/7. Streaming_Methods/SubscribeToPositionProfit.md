# ✅ Subscribe to Position Profit Updates (`OnPositionProfitAsync`)

> **Stream:** Periodic snapshots of position profit/loss on **MT5**. Returns profit updates for all open positions at specified interval.

**API Information:**

* **SDK wrapper:** `MT5Service.OnPositionProfitAsync(...)` (from class `MT5Service`)
* **gRPC service:** `mt5_term_api.SubscriptionService`
* **Proto definition:** `OnPositionProfit` (defined in `mt5-term-api-subscriptions.proto`)

### RPC

* **Service:** `mt5_term_api.SubscriptionService`
* **Method:** `OnPositionProfit(OnPositionProfitRequest) → stream OnPositionProfitReply`
* **Low‑level client (generated):** `SubscriptionService.SubscriptionServiceClient.OnPositionProfit(request, headers, deadline, cancellationToken)`
* **SDK wrapper:**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Service
    {
        public async IAsyncEnumerable<OnPositionProfitData> OnPositionProfitAsync(
            int intervalMs = 1000,
            bool ignoreEmpty = false,
            [EnumeratorCancellation] CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`OnPositionProfitRequest { timer_period_milliseconds, ignore_empty_data }`

**Reply message (stream):**

`OnPositionProfitReply { data: OnPositionProfitData }`

---

## 🔽 Input

| Parameter            | Type                  | Description                                                  |
| -------------------- | --------------------- | ------------------------------------------------------------ |
| `intervalMs`         | `int`                 | Update interval in milliseconds (default: 1000ms = 1 second) |
| `ignoreEmpty`        | `bool`                | Skip events when no positions exist (default: false)         |
| `cancellationToken`  | `CancellationToken`   | Token to stop the stream                                     |

---

## ⬆️ Output — `OnPositionProfitData` (stream)

| Field                    | Type                                   | Description                              |
| ------------------------ | -------------------------------------- | ---------------------------------------- |
| `Type`                   | `MT5_SUB_ENUM_EVENT_GROUP_TYPE`        | Event group type                         |
| `NewPositions`           | `List<OnPositionProfitPositionInfo>`   | Newly opened positions since last update |
| `UpdatedPositions`       | `List<OnPositionProfitPositionInfo>`   | Positions with changed profit            |
| `DeletedPositions`       | `List<OnPositionProfitPositionInfo>`   | Closed positions since last update       |
| `AccountInfo`            | `OnEventAccountInfo`                   | Current account state                    |
| `TerminalInstanceGuidId` | `string`                               | Terminal instance ID                     |

### `OnPositionProfitPositionInfo` — Position profit information

| Field            | Type     | Description                   |
| ---------------- | -------- | ----------------------------- |
| `Index`          | `int32`  | Position index                |
| `Ticket`         | `int64`  | Position ticket               |
| `Profit`         | `double` | Current profit/loss           |
| `PositionSymbol` | `string` | Symbol name                   |

### `OnEventAccountInfo` — Account state snapshot

| Field        | Type     | Description              |
| ------------ | -------- | ------------------------ |
| `Balance`    | `double` | Account balance          |
| `Credit`     | `double` | Account credit           |
| `Equity`     | `double` | Account equity           |
| `Margin`     | `double` | Used margin              |
| `FreeMargin` | `double` | Free margin              |
| `Profit`     | `double` | Total current profit     |
| `MarginLevel`| `double` | Margin level (%)         |
| `Login`      | `int64`  | Account login            |

---

## 🧱 Related enums (from proto)

### `MT5_SUB_ENUM_EVENT_GROUP_TYPE`

* `OrderProfit` — Position profit event
* `OrderUpdate` — Order update event

---

## 💬 Just the essentials

* **What it is.** Periodic polling-based stream that sends profit snapshots at specified interval. Shows new/updated/closed positions since last update.
* **Why you need it.** Real-time P&L monitoring, drawdown alerts, profit targeting, risk management dashboards.
* **Sanity check.** Stream sends updates every `intervalMs` milliseconds. Check `NewPositions`, `UpdatedPositions`, `DeletedPositions` arrays for changes.

---

## 🎯 Purpose

Use it for real-time profit monitoring:

* Real-time P/L dashboards.
* Profit/loss alerts and notifications.
* Drawdown monitoring.
* Equity curve tracking.
* Automated profit-taking strategies.

---

## 🧩 Notes & Tips

* **Polling-based:** Unlike tick streams, this polls at fixed interval. Default 1000ms (1 second).
* **Delta updates:** Each event shows what changed since last update (new, updated, deleted positions).
* **Empty events:** If `ignoreEmpty = false`, receives events even when no positions open. Set `true` to skip empty updates.
* **Account snapshot:** Each event includes complete account state (balance, equity, margin, etc.).
* **Profit calculation:** Profit includes both unrealized P/L and swap charges.
* **Performance:** Lower frequency = less CPU usage. Adjust `intervalMs` based on needs.
* **Use cases:** 1s interval for monitoring, 100ms for high-frequency strategies, 5s for dashboards.

---

## 🔗 Usage Examples

### 1) Basic profit monitoring

```csharp
// svc — MT5Service instance

var cts = new CancellationTokenSource();

try
{
    await foreach (var profitUpdate in svc.OnPositionProfitAsync(
        intervalMs: 1000,
        ignoreEmpty: true,
        cts.Token))
    {
        Console.WriteLine($"Total account profit: ${profitUpdate.AccountInfo.Profit:F2}");
        Console.WriteLine($"  Equity: ${profitUpdate.AccountInfo.Equity:F2}");
        Console.WriteLine($"  Margin Level: {profitUpdate.AccountInfo.MarginLevel:F2}%");
        Console.WriteLine();
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Stream stopped");
}
```

---

### 2) Monitor individual position profits

```csharp
var cts = new CancellationTokenSource();

await foreach (var update in svc.OnPositionProfitAsync(intervalMs: 500, cts.Token))
{
    // Show all currently profitable positions
    foreach (var pos in update.UpdatedPositions)
    {
        var status = pos.Profit >= 0 ? "🟢" : "🔴";
        Console.WriteLine($"{status} Position #{pos.Ticket} ({pos.PositionSymbol}): ${pos.Profit:F2}");
    }

    // Show newly opened positions
    foreach (var pos in update.NewPositions)
    {
        Console.WriteLine($"🆕 New position #{pos.Ticket} opened on {pos.PositionSymbol}");
    }

    // Show closed positions
    foreach (var pos in update.DeletedPositions)
    {
        Console.WriteLine($"✅ Position #{pos.Ticket} closed with ${pos.Profit:F2}");
    }

    Console.WriteLine();
}
```

---

### 3) Profit alert system

```csharp
var targetProfit = 100.0; // Alert when account profit reaches $100
var cts = new CancellationTokenSource();

await foreach (var update in svc.OnPositionProfitAsync(intervalMs: 1000, cts.Token))
{
    var totalProfit = update.AccountInfo.Profit;

    Console.WriteLine($"Current profit: ${totalProfit:F2}");

    if (totalProfit >= targetProfit)
    {
        Console.WriteLine($"\n🎯 PROFIT TARGET REACHED: ${totalProfit:F2}!");
        Console.WriteLine("Consider closing positions to lock in profits.");

        // Stop monitoring
        cts.Cancel();
    }
}
```

---

### 4) Drawdown monitoring

```csharp
var cts = new CancellationTokenSource();
var maxDrawdown = -50.0; // Stop if drawdown exceeds $50

await foreach (var update in svc.OnPositionProfitAsync(intervalMs: 1000, cts.Token))
{
    var currentProfit = update.AccountInfo.Profit;

    if (currentProfit < maxDrawdown)
    {
        Console.WriteLine($"⚠ DRAWDOWN ALERT: ${currentProfit:F2}!");
        Console.WriteLine($"Maximum allowed drawdown (${maxDrawdown}) exceeded.");
        Console.WriteLine("STOP TRADING AND REVIEW POSITIONS.");

        // Could automatically close all positions here
        cts.Cancel();
    }
    else
    {
        Console.WriteLine($"P/L: ${currentProfit:F2} (OK)");
    }
}
```

---

### 5) Track equity curve

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromMinutes(5)); // Monitor for 5 minutes

var equityHistory = new List<(DateTime Time, double Equity)>();

try
{
    await foreach (var update in svc.OnPositionProfitAsync(intervalMs: 1000, cts.Token))
    {
        var equity = update.AccountInfo.Equity;
        equityHistory.Add((DateTime.UtcNow, equity));

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Equity: ${equity:F2}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine($"\nEquity curve collected ({equityHistory.Count} data points):");

    var startEquity = equityHistory.First().Equity;
    var endEquity = equityHistory.Last().Equity;
    var maxEquity = equityHistory.Max(e => e.Equity);
    var minEquity = equityHistory.Min(e => e.Equity);

    Console.WriteLine($"  Start: ${startEquity:F2}");
    Console.WriteLine($"  End: ${endEquity:F2}");
    Console.WriteLine($"  Change: ${endEquity - startEquity:F2}");
    Console.WriteLine($"  Max: ${maxEquity:F2}");
    Console.WriteLine($"  Min: ${minEquity:F2}");
}
```

---

### 6) Real-time P/L dashboard

```csharp
var cts = new CancellationTokenSource();

await foreach (var update in svc.OnPositionProfitAsync(intervalMs: 1000, cts.Token))
{
    Console.Clear();
    Console.WriteLine("═══════════════════════════════════════════");
    Console.WriteLine("          REAL-TIME P/L DASHBOARD          ");
    Console.WriteLine("═══════════════════════════════════════════");
    Console.WriteLine();

    var acc = update.AccountInfo;

    Console.WriteLine($"Balance:      ${acc.Balance,12:F2}");
    Console.WriteLine($"Equity:       ${acc.Equity,12:F2}");
    Console.WriteLine($"Profit:       ${acc.Profit,12:F2}");
    Console.WriteLine($"Margin:       ${acc.Margin,12:F2}");
    Console.WriteLine($"Free Margin:  ${acc.FreeMargin,12:F2}");
    Console.WriteLine($"Margin Level: {acc.MarginLevel,12:F2}%");
    Console.WriteLine();

    Console.WriteLine("Active Positions:");
    Console.WriteLine("───────────────────────────────────────────");

    foreach (var pos in update.UpdatedPositions)
    {
        var profitSign = pos.Profit >= 0 ? "+" : "";
        var color = pos.Profit >= 0 ? "🟢" : "🔴";

        Console.WriteLine($"{color} #{pos.Ticket,-10} {pos.PositionSymbol,-8} {profitSign}${pos.Profit,10:F2}");
    }

    Console.WriteLine();
    Console.WriteLine($"Last update: {DateTime.Now:HH:mm:ss}");
}
```

---

### 7) Automated profit-taking

```csharp
var targetProfitPerPosition = 10.0; // Take profit at $10 per position
var cts = new CancellationTokenSource();

await foreach (var update in svc.OnPositionProfitAsync(intervalMs: 500, cts.Token))
{
    foreach (var pos in update.UpdatedPositions)
    {
        if (pos.Profit >= targetProfitPerPosition)
        {
            Console.WriteLine($"🎯 Position #{pos.Ticket} reached target profit: ${pos.Profit:F2}");
            Console.WriteLine($"   Closing position...");

            // Close position (would need MT5Account instance)
            // await account.OrderCloseAsync(new OrderCloseRequest { Ticket = (ulong)pos.Ticket, Volume = 0 });
        }
    }
}
```

---

### 8) Margin level monitoring with alerts

```csharp
var warningLevel = 150.0; // Warn if margin level drops below 150%
var dangerLevel = 100.0;  // Alert if below 100%

var cts = new CancellationTokenSource();

await foreach (var update in svc.OnPositionProfitAsync(intervalMs: 1000, cts.Token))
{
    var marginLevel = update.AccountInfo.MarginLevel;

    Console.Write($"Margin Level: {marginLevel:F2}% ");

    if (marginLevel < dangerLevel)
    {
        Console.WriteLine("🔴 DANGER - STOP OUT RISK!");
    }
    else if (marginLevel < warningLevel)
    {
        Console.WriteLine("⚠ WARNING - Low margin");
    }
    else
    {
        Console.WriteLine("✅ OK");
    }
}
```

---

### 9) Position count monitoring

```csharp
var maxPositions = 5;
var cts = new CancellationTokenSource();

await foreach (var update in svc.OnPositionProfitAsync(intervalMs: 1000, cts.Token))
{
    var positionCount = update.UpdatedPositions.Count + update.NewPositions.Count;

    Console.WriteLine($"Active positions: {positionCount}/{maxPositions}");

    if (positionCount >= maxPositions)
    {
        Console.WriteLine($"⚠ Maximum position limit ({maxPositions}) reached!");
    }

    // Show new positions
    if (update.NewPositions.Count > 0)
    {
        Console.WriteLine($"  🆕 {update.NewPositions.Count} new position(s) opened");
    }

    // Show closed positions
    if (update.DeletedPositions.Count > 0)
    {
        Console.WriteLine($"  ✅ {update.DeletedPositions.Count} position(s) closed");
    }
}
```
