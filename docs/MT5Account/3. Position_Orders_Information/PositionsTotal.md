# ✅ Getting Total Number of Open Positions

> **Request:** count of open positions from **MT5**. Get the total number of currently open positions on the account.

**API Information:**

* **SDK wrapper:** `MT5Account.PositionsTotalAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.TradeFunctions`
* **Proto definition:** `PositionsTotal` (defined in `mt5-term-api-trade-functions.proto`)

### RPC

* **Service:** `mt5_term_api.TradeFunctions`
* **Method:** `PositionsTotal(google.protobuf.Empty) → PositionsTotalReply`
* **Low‑level client (generated):** `TradeFunctions.PositionsTotal(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace mt5_term_api
{
    public class MT5Account
    {
        public async Task<PositionsTotalData> PositionsTotalAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`google.protobuf.Empty {}` (no parameters)


**Reply message:**

`PositionsTotalReply { data: PositionsTotalData }` or `{ error: Error }`

---

## 🔽 Input

No required parameters.

| Parameter           | Type                | Description                                               |
| ------------------- | ------------------- | --------------------------------------------------------- |
| `deadline`          | `DateTime?`         | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `PositionsTotalData`

| Field            | Type    | Description                           |
| ---------------- | ------- | ------------------------------------- |
| `TotalPositions` | `int32` | Total number of currently open positions |

---

## 💬 Just the essentials

* **What it is.** Single RPC returning the count of open positions.
* **Why you need it.** Check if account has open positions, verify positions were opened/closed, iterate through positions.
* **Fast check.** Lightweight operation to quickly see if there are any open trades.

---

## 🎯 Purpose

Use this method when you need to:

* Check if there are any open positions before performing operations.
* Verify that a position was successfully opened (count increases).
* Confirm that a position was closed (count decreases).
* Determine if you need to fetch detailed position information.
* Monitor position count for risk management.
* Iterate through positions using count + index (with other methods).

---

## 🧩 Notes & Tips

* Returns **only open positions**, not pending orders.
* Count includes positions across **all symbols**.
* Use `OpenedOrdersAsync()` to get detailed information about each position.
* Fast operation - use short timeout (3-5s).
* For hedging accounts, same symbol can have multiple positions (buy + sell).
* For netting accounts, one symbol = one net position (buy or sell, not both).
* **Does not** include closed/historical positions.

---

## 🔗 Usage Examples

### 1) Check if account has open positions

```csharp
// Quick check for open positions
var result = await acc.PositionsTotalAsync(
    deadline: DateTime.UtcNow.AddSeconds(3));

if (result.TotalPositions > 0)
{
    Console.WriteLine($"✅ Account has {result.TotalPositions} open position(s)");
}
else
{
    Console.WriteLine("No open positions");
}
```

### 2) Verify position was opened

```csharp
// Count before opening position
var beforeCount = await acc.PositionsTotalAsync();
Console.WriteLine($"Positions before: {beforeCount.TotalPositions}");

// Open a position...
// await acc.OrderSendAsync(...);

// Count after
var afterCount = await acc.PositionsTotalAsync();
Console.WriteLine($"Positions after:  {afterCount.TotalPositions}");

if (afterCount.TotalPositions > beforeCount.TotalPositions)
{
    Console.WriteLine("✅ Position opened successfully");
}
```

### 3) Monitor for position changes

```csharp
// Poll for position count changes
int previousCount = -1;

while (true)
{
    var current = await acc.PositionsTotalAsync();

    if (current.TotalPositions != previousCount)
    {
        Console.WriteLine($"Position count changed: {previousCount} → {current.TotalPositions}");
        previousCount = current.TotalPositions;
    }

    await Task.Delay(1000); // Check every second
}
```

### 4) Wait for all positions to close

```csharp
// Wait until all positions are closed
Console.WriteLine("Waiting for all positions to close...");

while (true)
{
    var result = await acc.PositionsTotalAsync();

    if (result.TotalPositions == 0)
    {
        Console.WriteLine("✅ All positions closed");
        break;
    }

    Console.WriteLine($"  {result.TotalPositions} position(s) still open...");
    await Task.Delay(2000);
}
```

### 5) Check before trading operations

```csharp
// Verify position limits before opening new position
var maxPositions = 10; // Your risk limit

var current = await acc.PositionsTotalAsync();

if (current.TotalPositions >= maxPositions)
{
    Console.WriteLine($"⚠️ Max positions reached ({current.TotalPositions}/{maxPositions})");
    Console.WriteLine("Cannot open new position");
    return;
}

Console.WriteLine($"✅ Can open position ({current.TotalPositions}/{maxPositions})");
// Proceed with trading...
```
