# Cancel All Pending Orders (`CancelAll`)

> **⚠️ DANGEROUS BULK METHOD:** Cancels all pending orders with optional symbol/direction filtering. USE WITH CAUTION!

**API Information:**

* **Extension method:** `MT5Service.CancelAll(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Region:** [11] BULK CONVENIENCE
* **Underlying calls:** `OpenedOrdersAsync()` + `CloseByTicket()` (in loop)

---

## Method Signature

```csharp
public static async Task<int> CancelAll(
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
| `symbol` | `string?` | Optional symbol filter. `null` = cancel ALL symbols ⚠️ |
| `isBuy` | `bool?` | Optional direction filter. `true` = buy orders only, `false` = sell orders only, `null` = both |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 30) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<int>` | Number of pending orders successfully cancelled |

---

## 💬 Just the essentials

* **What it is:** Bulk cancellation method - cancels all pending orders (Limit, Stop, StopLimit) matching the filter criteria.
* **Why you need it:** Quickly clear all pending orders when market conditions change or trading session ends.
* **⚠️ WARNING:** NO confirmation prompt! Cancels immediately. Affects ONLY pending orders, NOT market positions.

---

## 🎯 Purpose

Use it for:

* **End-of-day cleanup** - Cancel all pending orders before market close
* **Strategy termination** - Clear all pending orders when exiting a strategy
* **Market condition change** - Remove pending orders when volatility spikes
* **Symbol-specific cleanup** - Cancel all pending orders for one symbol
* **Emergency stop** - Quickly remove all pending entries

---

## 🔧 Under the Hood

```csharp
// Step 1: Get all opened orders (both market and pending)
var opened = await svc.OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, deadline, ct);

int cancelled = 0;

// Step 2: Iterate through orders via reflection (proto-agnostic)
foreach (var order in EnumerateOrders(opened))
{
    var typeInt = GetOrderTypeInt(order);

    // Step 3: Skip market positions (only pending orders)
    if (!IsPendingType(typeInt)) continue;
    // Pending types: 2-7 (BuyLimit, SellLimit, BuyStop, SellStop, BuyStopLimit, SellStopLimit)

    // Step 4: Apply symbol filter
    var orderSymbol = order.Symbol;
    if (symbol != null && orderSymbol != symbol) continue;

    // Step 5: Apply direction filter
    bool isOrderBuy = IsBuyType(typeInt);
    if (isBuy.HasValue && isOrderBuy != isBuy.Value) continue;

    // Step 6: Cancel the order
    var ticket = order.Ticket;
    await svc.CloseByTicket(ticket, null, timeoutSec, ct);
    cancelled++;
}

return cancelled;
```

**What it improves:**

* **Batch operation** - No manual loop needed
* **Flexible filtering** - By symbol, direction, or both
* **Safe targeting** - Only pending orders, never market positions
* **Returns count** - Know exactly how many orders were cancelled

---

## 🔗 Usage Examples

### Example 1: Cancel ALL Pending Orders (⚠️ DANGEROUS)

```csharp
// ⚠️ WARNING: Cancels ALL pending orders across ALL symbols!
int cancelled = await svc.CancelAll();

Console.WriteLine($"✅ Cancelled {cancelled} pending orders");
```

---

### Example 2: Cancel Pending Orders for One Symbol

```csharp
// Cancel all pending orders for EURUSD only
int cancelled = await svc.CancelAll(symbol: "EURUSD");

Console.WriteLine($"✅ Cancelled {cancelled} EURUSD pending orders");
```

---

### Example 3: Cancel Only Buy Pending Orders

```csharp
// Cancel all Buy Limit and Buy Stop orders (all symbols)
int cancelled = await svc.CancelAll(isBuy: true);

Console.WriteLine($"✅ Cancelled {cancelled} buy pending orders");
```

---

### Example 4: Cancel Specific Symbol + Direction

```csharp
// Cancel only GBPUSD Sell pending orders (Sell Limit + Sell Stop)
int cancelled = await svc.CancelAll(symbol: "GBPUSD", isBuy: false);

Console.WriteLine($"✅ Cancelled {cancelled} GBPUSD sell pending orders");
```

---

### Example 5: End-of-Day Cleanup

```csharp
public async Task EndOfDayCleanup(MT5Service svc)
{
    Console.WriteLine("End of day cleanup...");

    // Cancel all pending orders
    int cancelled = await svc.CancelAll();

    Console.WriteLine($"✅ Cancelled {cancelled} pending orders");
    Console.WriteLine("✅ Market positions remain open");
    Console.WriteLine("✅ Cleanup complete");
}

// Usage at 16:55 (before market close):
await EndOfDayCleanup(svc);
```

---

### Example 6: Cancel with Safety Confirmation

```csharp
// Add manual confirmation before cancelling
Console.Write("Cancel all pending orders? (y/n): ");
var input = Console.ReadLine();

if (input?.ToLower() == "y")
{
    int cancelled = await svc.CancelAll();
    Console.WriteLine($"✅ Cancelled {cancelled} pending orders");
}
else
{
    Console.WriteLine("❌ Cancelled operation aborted");
}
```

---

### Example 7: Count Before Cancel

```csharp
// Check how many orders will be cancelled before doing it
var opened = await svc.OpenedOrdersAllAsync();
int pendingCount = opened.Orders
    .Count(o => o.Type != ENUM_ORDER_TYPE.OrderTypeBuy &&
                o.Type != ENUM_ORDER_TYPE.OrderTypeSell);

Console.WriteLine($"Found {pendingCount} pending orders");
Console.Write("Cancel all? (y/n): ");

if (Console.ReadLine()?.ToLower() == "y")
{
    int cancelled = await svc.CancelAll();
    Console.WriteLine($"✅ Cancelled {cancelled} orders");
}
```

---

### Example 8: Error Handling

```csharp
try
{
    int cancelled = await svc.CancelAll(symbol: "EURUSD");

    if (cancelled > 0)
    {
        Console.WriteLine($"✅ Cancelled {cancelled} EURUSD pending orders");
    }
    else
    {
        Console.WriteLine("ℹ️ No pending orders to cancel for EURUSD");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error cancelling orders: {ex.Message}");
}
```

---

### Example 9: Cancel by Strategy (Using Comment Filter)

```csharp
// NOTE: CancelAll doesn't filter by comment, so we implement custom logic
public async Task<int> CancelByStrategy(
    MT5Service svc,
    string strategyName)
{
    var opened = await svc.OpenedOrdersAllAsync();
    int cancelled = 0;

    foreach (var order in opened.Orders)
    {
        // Skip market positions
        if (order.Type == ENUM_ORDER_TYPE.OrderTypeBuy ||
            order.Type == ENUM_ORDER_TYPE.OrderTypeSell)
            continue;

        // Check comment matches strategy
        if (order.Comment?.Contains(strategyName) == true)
        {
            await svc.CloseByTicket(order.Ticket);
            cancelled++;
        }
    }

    Console.WriteLine($"✅ Cancelled {cancelled} orders for strategy '{strategyName}'");
    return cancelled;
}

// Usage:
await CancelByStrategy(svc, "GridTrading_v2");
```

---

### Example 10: Volatility Spike Cleanup

```csharp
public async Task HandleVolatilitySpike(MT5Service svc, string symbol)
{
    // Measure current spread
    double spread = await svc.GetSpreadPointsAsync(symbol);
    double point = await svc.GetPointAsync(symbol);
    double spreadPips = spread / (point * 10);  // Convert to pips

    if (spreadPips > 5.0)  // Spread > 5 pips = too volatile
    {
        Console.WriteLine($"⚠️ High spread detected: {spreadPips:F1} pips");
        Console.WriteLine($"Cancelling all pending {symbol} orders...");

        int cancelled = await svc.CancelAll(symbol: symbol);

        Console.WriteLine($"✅ Cancelled {cancelled} pending orders");
        Console.WriteLine("Waiting for spread to normalize...");
    }
}

// Usage:
await HandleVolatilitySpike(svc, "GBPUSD");
```

---

## 🔗 Related Methods

**📦 Methods used internally:**

* `OpenedOrdersAsync()` - Gets all orders (market + pending)
* `CloseByTicket()` - Cancels individual order by ticket

**🍬 Related Sugar methods:**

* `CloseAllPending()` - Alias for CancelAll (same functionality)
* `CloseAllPositions()` - Closes market positions (NOT pending orders)
* `CloseAll()` - **DIFFERENT METHOD** - closes market positions, not from Region 11!

**⚠️ CONFUSION WARNING:**

- `CancelAll()` - Cancels PENDING orders (this method)
- `CloseAllPositions()` - Closes MARKET positions
- `CloseAll()` - From Region 06, closes market positions with filters

---

## ⚠️ Common Pitfalls

1. **Confusing with CloseAllPositions:**
   ```csharp
   // ❌ CONFUSION: Different methods!
   await svc.CancelAll();         // Cancels PENDING orders
   await svc.CloseAllPositions(); // Closes MARKET positions

   // They do DIFFERENT things!
   ```

2. **No confirmation prompt:**
   ```csharp
   // ⚠️ DANGEROUS: Executes immediately!
   await svc.CancelAll();  // No "Are you sure?" prompt!

   // ✅ SAFER: Add manual confirmation
   Console.Write("Cancel all? (y/n): ");
   if (Console.ReadLine()?.ToLower() == "y")
   {
       await svc.CancelAll();
   }
   ```

3. **Forgetting symbol parameter = ALL symbols:**
   ```csharp
   // ❌ WRONG: Thinking this only cancels EURUSD
   await svc.CancelAll();  // ⚠️ Cancels ALL symbols!

   // ✅ CORRECT: Specify symbol
   await svc.CancelAll(symbol: "EURUSD");
   ```

4. **Expecting market positions to close:**
   ```csharp
   // ❌ WRONG: CancelAll does NOT close market positions
   await svc.CancelAll();
   // Your open BUY/SELL positions are still open!

   // ✅ CORRECT: Use CloseAllPositions for market positions
   await svc.CloseAllPositions();
   ```

5. **Not checking return count:**
   ```csharp
   // ❌ SUBOPTIMAL: Not checking if anything was cancelled
   await svc.CancelAll(symbol: "EURUSD");

   // ✅ BETTER: Check result
   int cancelled = await svc.CancelAll(symbol: "EURUSD");
   if (cancelled == 0)
   {
       Console.WriteLine("ℹ️ No pending orders to cancel");
   }
   ```

6. **Filter logic confusion:**
   ```csharp
   // ⚠️ FILTER LOGIC:
   // symbol: null → ALL symbols
   // isBuy: null → BOTH buy and sell

   await svc.CancelAll(symbol: null, isBuy: true);
   // Cancels ALL buy pending orders across ALL symbols!

   await svc.CancelAll(symbol: "EURUSD", isBuy: null);
   // Cancels BOTH buy and sell pending orders for EURUSD
   ```

---

## 💡 Summary

**CancelAll** provides bulk cancellation of pending orders:

* ✅ Cancels all pending orders (Limit, Stop, StopLimit)
* ✅ Optional filtering by symbol and direction
* ✅ Returns count of cancelled orders
* ⚠️ **NO market positions affected** (only pending orders)
* ⚠️ **NO confirmation prompt** (executes immediately)

```csharp
// Cancel all pending orders for EURUSD:
int cancelled = await svc.CancelAll(symbol: "EURUSD");

// Cancel only sell pending orders:
int cancelled = await svc.CancelAll(isBuy: false);

// Cancel everything (⚠️ DANGEROUS):
int cancelled = await svc.CancelAll();
```

**Use with caution - this is a powerful bulk operation!** ⚠️
