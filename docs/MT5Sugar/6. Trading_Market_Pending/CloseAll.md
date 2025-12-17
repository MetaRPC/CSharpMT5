# Close All Positions (`CloseAll`)

> **Sugar method:** Closes all open orders and positions with optional filtering by symbol and direction.

**API Information:**

* **Extension method:** `MT5Service.CloseAll(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Region:** [06] TRADING — MARKET & PENDING
* **Underlying calls:** `OpenedOrdersAsync()` + `CloseByTicket()` for each position

---

## Method Signature

```csharp
public static async Task<int> CloseAll(
    this MT5Service svc,
    string? symbol = null,
    bool? isBuy = null,
    int timeoutSec = 30,
    CancellationToken ct = default)
```

---

## 🔽 Input

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance (extension method) |
| `symbol` | `string?` | Optional symbol filter (e.g., "EURUSD"). Pass null to close all symbols (default: null) |
| `isBuy` | `bool?` | Optional direction filter. true = close only BUY, false = close only SELL, null = close both (default: null) |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 30) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<int>` | Number of orders/positions that were successfully closed |

---

## 💬 Just the essentials

* **What it is:** Bulk close operation - closes all matching positions/orders with optional filters.
* **Why you need it:** Emergency exits, end-of-day cleanup, symbol-specific closures without manual iteration.
* **Sanity check:** **⚠️ USE WITH CAUTION!** Returns count of closed positions. Always use filters to avoid closing unintended positions.

---

## 🎯 Purpose

Use it for:

* **Emergency close all** - panic button to exit all positions
* **Symbol cleanup** - close all positions for specific symbol
* **Directional close** - close all BUY or all SELL positions
* **End-of-day** - flatten all positions before market close
* **Strategy exit** - close all positions when strategy stops

---

## 🔧 Under the Hood

```csharp
// Step 1: Get all opened orders and positions
var opened = await svc.OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, deadline, ct);

// Step 2: Filter by symbol and direction
var orders = EnumerateOrders(opened);  // Combines positions + pending orders
int closed = 0;

foreach (var order in orders)
{
    // Filter by symbol (if provided)
    if (symbol != null && order.Symbol != symbol)
        continue;

    // Filter by direction (if provided)
    if (isBuy.HasValue)
    {
        bool orderIsBuy = IsBuyOrder(order.Type);
        if (orderIsBuy != isBuy.Value)
            continue;
    }

    // Step 3: Close matching orders sequentially
    await svc.CloseByTicket(order.Ticket, null, timeoutSec, ct);
    closed++;
}

return closed;
```

**What it improves:**

* **Bulk operation** - close multiple positions in one call
* **Smart filtering** - by symbol, direction, or both
* **Count result** - returns number of closed positions
* **Sequential closing** - ensures each close completes

---

## 🔗 Usage Examples

### Example 1: Close ALL Positions (Emergency Exit)

```csharp
// ⚠️ DANGER: Closes EVERYTHING!
int closed = await svc.CloseAll();

Console.WriteLine($"🚨 Emergency exit: Closed {closed} positions");
```

**⚠️ WARNING:** This closes ALL positions and pending orders on the account!

---

### Example 2: Close All Positions for Specific Symbol

```csharp
// Close only EURUSD positions
int closed = await svc.CloseAll(symbol: "EURUSD");

Console.WriteLine($"✅ Closed {closed} EURUSD positions");
```

---

### Example 3: Close All BUY Positions

```csharp
// Close only BUY positions (all symbols)
int closed = await svc.CloseAll(isBuy: true);

Console.WriteLine($"✅ Closed {closed} BUY positions");
```

---

### Example 4: Close All SELL Positions

```csharp
// Close only SELL positions (all symbols)
int closed = await svc.CloseAll(isBuy: false);

Console.WriteLine($"✅ Closed {closed} SELL positions");
```

---

### Example 5: Close All BUY Positions on GBPUSD

```csharp
// Specific symbol + direction filter
int closed = await svc.CloseAll(symbol: "GBPUSD", isBuy: true);

Console.WriteLine($"✅ Closed {closed} GBPUSD BUY positions");
```

---

### Example 6: Close All SELL Positions on Multiple Symbols

```csharp
string[] symbols = { "EURUSD", "GBPUSD", "USDJPY" };

foreach (var symbol in symbols)
{
    int closed = await svc.CloseAll(symbol: symbol, isBuy: false);
    Console.WriteLine($"✅ {symbol}: Closed {closed} SELL positions");
}
```

---

### Example 7: End-of-Day Flatten (With Confirmation)

```csharp
// Get count first
var opened = await svc.OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc);

int totalPositions = opened.PositionInfos.Count + opened.OpenedOrders.Count;

Console.WriteLine($"⚠️ About to close {totalPositions} positions. Confirm? (yes/no)");
string? confirm = Console.ReadLine();

if (confirm?.ToLower() == "yes")
{
    int closed = await svc.CloseAll();
    Console.WriteLine($"✅ Closed {closed} positions for end-of-day");
}
else
{
    Console.WriteLine("❌ Cancelled");
}
```

---

### Example 8: Strategy-Specific Close

```csharp
public class TradingStrategy
{
    private readonly MT5Service _svc;
    private readonly string[] _symbols;

    public async Task StopStrategyAsync()
    {
        Console.WriteLine("Stopping strategy - closing all positions...");

        // Close positions for strategy symbols only
        int totalClosed = 0;
        foreach (var symbol in _symbols)
        {
            int closed = await _svc.CloseAll(symbol: symbol);
            totalClosed += closed;
            Console.WriteLine($"  ✅ {symbol}: {closed} positions closed");
        }

        Console.WriteLine($"Strategy stopped. Total closed: {totalClosed}");
    }
}
```

---

### Example 9: Close With Error Handling

```csharp
try
{
    int closed = await svc.CloseAll(symbol: "EURUSD");

    if (closed > 0)
    {
        Console.WriteLine($"✅ Successfully closed {closed} positions");
    }
    else
    {
        Console.WriteLine($"ℹ️ No positions to close");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error closing positions: {ex.Message}");
}
```

---

### Example 10: Conditional Close Based on Time

```csharp
public async Task CloseBeforeWeekend(MT5Service svc)
{
    var now = DateTime.Now;

    // Close all positions on Friday after 22:00 UTC
    if (now.DayOfWeek == DayOfWeek.Friday && now.Hour >= 22)
    {
        Console.WriteLine("⚠️ Closing all positions before weekend...");

        int closed = await svc.CloseAll();

        Console.WriteLine($"✅ Weekend protection: Closed {closed} positions");
    }
}
```

---

### Example 11: Smart Close with Loss Limit

```csharp
public async Task CloseLosingPositions(MT5Service svc)
{
    var opened = await svc.OpenedOrdersAsync(
        BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc);

    int closedCount = 0;

    // Close only positions with loss > $50
    foreach (var position in opened.PositionInfos)
    {
        if (position.Profit < -50.0)
        {
            await svc.CloseByTicket(position.Ticket);
            closedCount++;
            Console.WriteLine($"✅ Closed losing position #{position.Ticket}, Loss: ${position.Profit:F2}");
        }
    }

    Console.WriteLine($"Total losing positions closed: {closedCount}");
}
```

---

### Example 12: With Cancellation Token

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(20));  // Max 20 seconds

try
{
    int closed = await svc.CloseAll(symbol: "EURUSD", ct: cts.Token);
    Console.WriteLine($"✅ Closed {closed} positions");
}
catch (OperationCanceledException)
{
    Console.WriteLine($"⚠️ Close operation timed out after 20 seconds");
}
```

---

## 🔗 Related Methods

**📦 Low-level methods used internally:**

* `OpenedOrdersAsync()` - Gets all current positions and orders
* `CloseByTicket()` - Closes individual positions

**🍬 Alternative Sugar methods:**

* `CloseAllPositions()` - Alias for CloseAll (market positions only)
* `CloseAllPending()` - Alias for CancelAll (pending orders only)
* `CancelAll()` - Cancel all pending orders
* `CloseByTicket()` - Close specific position by ticket

---

## ⚠️ Common Pitfalls & Safety

### ❌ CRITICAL WARNING - Read Before Using!

```csharp
// ❌ EXTREMELY DANGEROUS - Closes EVERYTHING!
await svc.CloseAll();

// This will close:
// - All BUY and SELL market positions
// - All pending orders (limits, stops)
// - Across ALL symbols
// - No confirmation, no undo!
```

### ✅ SAFE USAGE PATTERNS:

1. **Always use filters:**
   ```csharp
   // ✅ GOOD: Symbol-specific
   await svc.CloseAll(symbol: "EURUSD");

   // ✅ GOOD: Direction-specific
   await svc.CloseAll(isBuy: true);

   // ✅ GOOD: Both filters
   await svc.CloseAll(symbol: "GBPUSD", isBuy: false);
   ```

2. **Add confirmation in production:**
   ```csharp
   Console.WriteLine("⚠️ Confirm close all? (yes/no)");
   if (Console.ReadLine() == "yes")
   {
       await svc.CloseAll();
   }
   ```

3. **Test on demo first:**
   ```csharp
   #if DEBUG
       await svc.CloseAll();  // Only in debug mode
   #endif
   ```

4. **Use specific methods for specific cases:**
   ```csharp
   // ✅ For one position: Use CloseByTicket
   await svc.CloseByTicket(ticket: 12345);

   // ✅ For filtered close: Use CloseAll with filters
   await svc.CloseAll(symbol: "EURUSD", isBuy: true);
   ```

---

## 💡 Summary

**CloseAll** is a powerful but dangerous bulk close tool:

* ✅ Closes multiple positions in one call
* ✅ Optional filters: symbol, direction, or both
* ✅ Returns count of closed positions
* ⚠️ **USE WITH EXTREME CAUTION** - can close all account positions!

```csharp
// Filtered usage (safe):
await svc.CloseAll(symbol: "EURUSD");           // Close EURUSD only
await svc.CloseAll(isBuy: true);                // Close all BUYs
await svc.CloseAll(symbol: "GBPUSD", isBuy: false);  // Close GBPUSD SELLs

// Unfiltered (DANGEROUS - use with confirmation):
await svc.CloseAll();  // ⚠️ Closes EVERYTHING!
```

**Power tool - use wisely!** 🚀⚠️
