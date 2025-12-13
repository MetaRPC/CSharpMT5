# ✅ Getting Ticket IDs of Opened Orders & Positions

> **Request:** list of ticket IDs for all opened orders and positions from **MT5**. Get lightweight snapshot with just ticket numbers (no detailed data).

**API Information:**

* **SDK wrapper:** `MT5Account.OpenedOrdersTicketsAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.AccountHelper`
* **Proto definition:** `OpenedOrdersTickets` (defined in `mt5-term-api-account-helper.proto`)

### RPC

* **Service:** `mt5_term_api.AccountHelper`
* **Method:** `OpenedOrdersTickets(OpenedOrdersTicketsRequest) → OpenedOrdersTicketsReply`
* **Low‑level client (generated):** `AccountHelper.OpenedOrdersTickets(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<OpenedOrdersTicketsData> OpenedOrdersTicketsAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`OpenedOrdersTicketsRequest {}` (empty - no parameters)


**Reply message:**

`OpenedOrdersTicketsReply { data: OpenedOrdersTicketsData }` or `{ error: Error }`

---

## 🔽 Input

No required parameters.

| Parameter           | Type                | Description                                               |
| ------------------- | ------------------- | --------------------------------------------------------- |
| `deadline`          | `DateTime?`         | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `OpenedOrdersTicketsData`

| Field                    | Type          | Description                                  |
| ------------------------ | ------------- | -------------------------------------------- |
| `OpenedOrdersTickets`    | `List<long>`  | List of ticket IDs for pending orders        |
| `OpenedPositionTickets`  | `List<long>`  | List of ticket IDs for open positions        |

---

## 💬 Just the essentials

* **What it is.** Single RPC returning only ticket IDs of active orders and positions.
* **Why you need it.** Fast lightweight check for active trades, verify specific tickets exist, iterate through tickets.
* **Performance advantage.** Much faster than `OpenedOrdersAsync()` - returns only IDs, not full details.
* **Two lists.** Pending order tickets and open position tickets (separate).
* **Use case.** When you need to know WHICH trades exist, not the full details.

---

## 🎯 Purpose

Use this method when you need to:

* Quickly check which order/position tickets are currently active.
* Verify if a specific ticket ID still exists (before modifying/closing).
* Get list of tickets for batch operations.
* Iterate through tickets and fetch details only for specific ones.
* Monitor ticket count changes without fetching full data.
* Check if an EA's orders/positions are still active (by ticket ID).

---

## 🧩 Notes & Tips

* **Much faster** than `OpenedOrdersAsync()` - returns only ticket IDs (long integers).
* Use this method **before** fetching full details to reduce data transfer.
* `OpenedOrdersTickets` contains **pending orders** (not yet filled).
* `OpenedPositionTickets` contains **open positions** (already executing).
* Empty lists mean no active orders or positions.
* Ticket IDs are **unique** within the account.
* Use short timeout (3-5s) - this is a fast operation.
* Combine with `PositionSelectByTicketAsync()` or similar to fetch details for specific tickets.
* Good for polling - check ticket count changes without heavy data load.

---

## 🔗 Usage Examples

### 1) Get all active ticket IDs

```csharp
// Retrieve ticket IDs for all active trades
var data = await acc.OpenedOrdersTicketsAsync(
    deadline: DateTime.UtcNow.AddSeconds(3));

Console.WriteLine($"Pending Orders: {data.OpenedOrdersTickets.Count} tickets");
Console.WriteLine($"Open Positions: {data.OpenedPositionTickets.Count} tickets");

if (data.OpenedOrdersTickets.Count > 0)
{
    Console.WriteLine("Order Tickets: " + string.Join(", ", data.OpenedOrdersTickets));
}

if (data.OpenedPositionTickets.Count > 0)
{
    Console.WriteLine("Position Tickets: " + string.Join(", ", data.OpenedPositionTickets));
}
```

### 2) Check if specific ticket exists

```csharp
// Verify ticket still active before modification
var ticketToCheck = 123456789L;

var data = await acc.OpenedOrdersTicketsAsync();

bool isPendingOrder = data.OpenedOrdersTickets.Contains(ticketToCheck);
bool isOpenPosition = data.OpenedPositionTickets.Contains(ticketToCheck);

if (isPendingOrder)
{
    Console.WriteLine($"✅ Ticket {ticketToCheck} is a pending order");
}
else if (isOpenPosition)
{
    Console.WriteLine($"✅ Ticket {ticketToCheck} is an open position");
}
else
{
    Console.WriteLine($"❌ Ticket {ticketToCheck} not found (closed or never existed)");
}
```

### 3) Monitor ticket changes

```csharp
// Poll for ticket changes (lightweight)
List<long> previousOrderTickets = null;
List<long> previousPositionTickets = null;

while (true)
{
    var data = await acc.OpenedOrdersTicketsAsync();

    // Check for new orders
    if (previousOrderTickets != null)
    {
        var newOrders = data.OpenedOrdersTickets
            .Except(previousOrderTickets)
            .ToList();

        if (newOrders.Count > 0)
        {
            Console.WriteLine($"📝 New pending orders: {string.Join(", ", newOrders)}");
        }

        var cancelledOrders = previousOrderTickets
            .Except(data.OpenedOrdersTickets)
            .ToList();

        if (cancelledOrders.Count > 0)
        {
            Console.WriteLine($"❌ Cancelled orders: {string.Join(", ", cancelledOrders)}");
        }
    }

    // Check for new positions
    if (previousPositionTickets != null)
    {
        var newPositions = data.OpenedPositionTickets
            .Except(previousPositionTickets)
            .ToList();

        if (newPositions.Count > 0)
        {
            Console.WriteLine($"✅ New positions: {string.Join(", ", newPositions)}");
        }

        var closedPositions = previousPositionTickets
            .Except(data.OpenedPositionTickets)
            .ToList();

        if (closedPositions.Count > 0)
        {
            Console.WriteLine($"🔚 Closed positions: {string.Join(", ", closedPositions)}");
        }
    }

    previousOrderTickets = data.OpenedOrdersTickets;
    previousPositionTickets = data.OpenedPositionTickets;

    await Task.Delay(2000); // Check every 2 seconds
}
```

### 4) Batch close all positions

```csharp
// Get all position tickets, then close them
var data = await acc.OpenedOrdersTicketsAsync();

if (data.OpenedPositionTickets.Count == 0)
{
    Console.WriteLine("No positions to close");
    return;
}

Console.WriteLine($"Closing {data.OpenedPositionTickets.Count} positions...");

foreach (var ticket in data.OpenedPositionTickets)
{
    try
    {
        // Close position by ticket
        // await acc.PositionCloseAsync(ticket);
        Console.WriteLine($"  Closed position {ticket}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Failed to close {ticket}: {ex.Message}");
    }
}
```

### 5) Count active trades

```csharp
// Quick count of active trades
var data = await acc.OpenedOrdersTicketsAsync();

int totalOrders = data.OpenedOrdersTickets.Count;
int totalPositions = data.OpenedPositionTickets.Count;
int totalActive = totalOrders + totalPositions;

Console.WriteLine($"Active Trading Summary:");
Console.WriteLine($"  Pending Orders:  {totalOrders}");
Console.WriteLine($"  Open Positions:  {totalPositions}");
Console.WriteLine($"  Total Active:    {totalActive}");

if (totalActive == 0)
{
    Console.WriteLine("  Status: No active trades");
}
else if (totalActive < 5)
{
    Console.WriteLine("  Status: Low activity");
}
else if (totalActive < 20)
{
    Console.WriteLine("  Status: Moderate activity");
}
else
{
    Console.WriteLine("  Status: High activity");
}
```
