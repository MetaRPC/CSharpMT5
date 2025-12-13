# Close All Market Positions (`CloseAllPositions`)

> **⚠️ EXTREMELY DANGEROUS BULK METHOD:** Closes all open market positions with optional symbol/direction filtering. NUCLEAR OPTION!

**API Information:**

* **Extension method:** `MT5Service.CloseAllPositions(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Region:** [11] BULK CONVENIENCE
* **Underlying calls:** `OpenedOrdersAsync()` + `CloseByTicket()` (in loop)

---

## Method Signature

```csharp
public static async Task<int> CloseAllPositions(
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
| `symbol` | `string?` | Optional symbol filter. `null` = close ALL symbols ⚠️⚠️⚠️ |
| `isBuy` | `bool?` | Optional direction filter. `true` = buy positions only, `false` = sell positions only, `null` = both |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 30) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<int>` | Number of market positions successfully closed |

---

## 💬 Just the essentials

* **What it is:** NUCLEAR OPTION - closes all open market positions (BUY/SELL) matching the filter criteria.
* **Why you need it:** Emergency exit, strategy termination, or end-of-day position cleanup.
* **⚠️⚠️⚠️ WARNING:** NO confirmation prompt! Closes ALL positions immediately. Can realize massive losses if used incorrectly!

---

## 🎯 Purpose

Use it for:

* **🚨 EMERGENCY STOP** - Flatten all positions immediately (market crash, news event)
* **End-of-day cleanup** - Close all positions before market close
* **Strategy termination** - Exit all positions when stopping a strategy
* **Symbol-specific exit** - Close all positions for one symbol
* **Direction-specific exit** - Close all buy OR all sell positions

---

## 🔧 Under the Hood

```csharp
// Step 1: Get all opened orders (both market and pending)
var opened = await svc.OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, deadline, ct);

int closed = 0;

// Step 2: Iterate through orders via reflection (proto-agnostic)
foreach (var order in EnumerateOrders(opened))
{
    var typeInt = GetOrderTypeInt(order);

    // Step 3: Skip pending orders (only market positions)
    if (!IsMarketType(typeInt)) continue;
    // Market types: 0-1 (Buy, Sell)

    // Step 4: Apply symbol filter
    var orderSymbol = order.Symbol;
    if (symbol != null && orderSymbol != symbol) continue;

    // Step 5: Apply direction filter
    bool isOrderBuy = IsBuyType(typeInt);
    if (isBuy.HasValue && isOrderBuy != isBuy.Value) continue;

    // Step 6: Close the position at market price
    var ticket = order.Ticket;
    await svc.CloseByTicket(ticket, null, timeoutSec, ct);
    closed++;
}

return closed;
```

**What it improves:**

* **Instant liquidation** - Close entire portfolio in one call
* **Flexible filtering** - By symbol, direction, or both
* **Safe targeting** - Only market positions, never pending orders
* **Returns count** - Know exactly how many positions were closed

---

## 🔗 Usage Examples

### Example 1: Close ALL Positions (⚠️⚠️⚠️ NUCLEAR OPTION)

```csharp
// ⚠️⚠️⚠️ EXTREMELY DANGEROUS: Closes ALL positions across ALL symbols!
// USE ONLY IN EMERGENCY!
int closed = await svc.CloseAllPositions();

Console.WriteLine($"🚨 EMERGENCY EXIT: Closed {closed} positions");
```

---

### Example 2: Close All Positions for One Symbol

```csharp
// Close all EURUSD positions (both buy and sell)
int closed = await svc.CloseAllPositions(symbol: "EURUSD");

Console.WriteLine($"✅ Closed {closed} EURUSD positions");
```

---

### Example 3: Close Only Buy Positions

```csharp
// Close all buy positions across all symbols
int closed = await svc.CloseAllPositions(isBuy: true);

Console.WriteLine($"✅ Closed {closed} buy positions");
```

---

### Example 4: Close Specific Symbol + Direction

```csharp
// Close only GBPUSD sell positions
int closed = await svc.CloseAllPositions(symbol: "GBPUSD", isBuy: false);

Console.WriteLine($"✅ Closed {closed} GBPUSD sell positions");
```

---

### Example 5: End-of-Day Full Exit

```csharp
public async Task EndOfDayFullExit(MT5Service svc)
{
    Console.WriteLine("⚠️ End of day - closing all positions...");

    // Close ALL market positions
    int closedPositions = await svc.CloseAllPositions();

    // Cancel ALL pending orders
    int cancelledOrders = await svc.CancelAll();

    Console.WriteLine($"✅ Closed {closedPositions} positions");
    Console.WriteLine($"✅ Cancelled {cancelledOrders} pending orders");
    Console.WriteLine("✅ Account fully flat");
}

// Usage at 16:55 (before market close):
await EndOfDayFullExit(svc);
```

---

### Example 6: Emergency Stop with Confirmation

```csharp
// Add DOUBLE confirmation for safety
Console.WriteLine("⚠️⚠️⚠️ EMERGENCY STOP - CLOSE ALL POSITIONS ⚠️⚠️⚠️");
Console.Write("Are you ABSOLUTELY SURE? Type 'YES' to confirm: ");
var input = Console.ReadLine();

if (input == "YES")
{
    int closed = await svc.CloseAllPositions();
    Console.WriteLine($"🚨 EMERGENCY EXIT: Closed {closed} positions");
}
else
{
    Console.WriteLine("❌ Emergency stop aborted");
}
```

---

### Example 7: Close Losing Positions Only

```csharp
// Custom logic: Close only positions with unrealized loss
public async Task<int> CloseLosers(MT5Service svc)
{
    var positions = await svc.OpenedOrdersAllAsync();
    int closed = 0;

    foreach (var pos in positions.Orders)
    {
        // Skip pending orders
        if (pos.Type != ENUM_ORDER_TYPE.OrderTypeBuy &&
            pos.Type != ENUM_ORDER_TYPE.OrderTypeSell)
            continue;

        // Check if position is losing
        if (pos.Profit < 0)
        {
            await svc.CloseByTicket(pos.Ticket);
            closed++;
            Console.WriteLine($"  Closed losing position #{pos.Ticket}: ${pos.Profit:F2}");
        }
    }

    Console.WriteLine($"✅ Closed {closed} losing positions");
    return closed;
}

// Usage:
await CloseLosers(svc);
```

---

### Example 8: Error Handling

```csharp
try
{
    int closed = await svc.CloseAllPositions(symbol: "EURUSD");

    if (closed > 0)
    {
        Console.WriteLine($"✅ Closed {closed} EURUSD positions");
    }
    else
    {
        Console.WriteLine("ℹ️ No open positions to close for EURUSD");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error closing positions: {ex.Message}");
}
```

---

### Example 9: News Event Emergency Exit

```csharp
public async Task HandleNewsEvent(MT5Service svc, string eventName)
{
    Console.WriteLine($"⚠️ HIGH IMPACT NEWS: {eventName}");
    Console.WriteLine("⚠️ Closing all positions for safety...");

    int closed = await svc.CloseAllPositions();
    int cancelled = await svc.CancelAll();

    Console.WriteLine($"✅ Closed {closed} positions");
    Console.WriteLine($"✅ Cancelled {cancelled} pending orders");
    Console.WriteLine("✅ Waiting for news event to pass...");
}

// Usage:
await HandleNewsEvent(svc, "NFP - Non-Farm Payrolls");
```

---

### Example 10: Close with P&L Reporting

```csharp
public async Task CloseAllWithReport(MT5Service svc)
{
    // Get positions before closing
    var positions = await svc.OpenedOrdersAllAsync();
    double totalProfit = 0;
    int count = 0;

    foreach (var pos in positions.Orders)
    {
        if (pos.Type == ENUM_ORDER_TYPE.OrderTypeBuy ||
            pos.Type == ENUM_ORDER_TYPE.OrderTypeSell)
        {
            totalProfit += pos.Profit;
            count++;
        }
    }

    Console.WriteLine($"About to close {count} positions");
    Console.WriteLine($"Current unrealized P&L: ${totalProfit:F2}");
    Console.Write("Continue? (y/n): ");

    if (Console.ReadLine()?.ToLower() == "y")
    {
        int closed = await svc.CloseAllPositions();
        Console.WriteLine($"✅ Closed {closed} positions");
        Console.WriteLine($"✅ Realized P&L: ${totalProfit:F2}");
    }
}

// Usage:
await CloseAllWithReport(svc);
```

---

## 🔗 Related Methods

**📦 Methods used internally:**

* `OpenedOrdersAsync()` - Gets all orders (market + pending)
* `CloseByTicket()` - Closes individual position by ticket

**🍬 Related Sugar methods:**

* `CancelAll()` - Cancels PENDING orders (NOT market positions!)
* `CloseAllPending()` - Alias for CancelAll
* `CloseAll()` - **DIFFERENT METHOD** from Region 06 (similar functionality)

**⚠️ CONFUSION WARNING:**

- `CloseAllPositions()` - Closes MARKET positions (this method)
- `CancelAll()` - Cancels PENDING orders
- `CloseAll()` - From Region 06, also closes market positions

---

## ⚠️ Common Pitfalls

1. **Confusing with CancelAll:**
   ```csharp
   // ❌ CONFUSION: Different methods!
   await svc.CloseAllPositions(); // Closes MARKET positions
   await svc.CancelAll();         // Cancels PENDING orders

   // They do DIFFERENT things!
   ```

2. **No confirmation prompt - instant execution:**
   ```csharp
   // ⚠️⚠️⚠️ EXTREMELY DANGEROUS: Executes immediately!
   await svc.CloseAllPositions();  // No "Are you sure?" prompt!

   // ✅ SAFER: Add manual confirmation
   Console.Write("Close all positions? Type 'YES': ");
   if (Console.ReadLine() == "YES")
   {
       await svc.CloseAllPositions();
   }
   ```

3. **Forgetting symbol parameter = ALL symbols:**
   ```csharp
   // ❌ WRONG: Thinking this only closes EURUSD
   await svc.CloseAllPositions();  // ⚠️ Closes ALL symbols!

   // ✅ CORRECT: Specify symbol
   await svc.CloseAllPositions(symbol: "EURUSD");
   ```

4. **Expecting pending orders to cancel:**
   ```csharp
   // ❌ WRONG: CloseAllPositions does NOT cancel pending orders
   await svc.CloseAllPositions();
   // Your Buy Limit / Sell Stop orders are still active!

   // ✅ CORRECT: Use CancelAll for pending orders
   await svc.CancelAll();
   ```

5. **Not considering slippage on close:**
   ```csharp
   // ⚠️ Market orders = slippage possible
   // Closing 100 positions at once might get bad fills!

   // Especially dangerous during:
   // - High volatility (news events)
   // - Low liquidity (market open/close)
   // - Large position sizes

   // Consider closing incrementally for large portfolios
   ```

6. **Not checking P&L before closing:**
   ```csharp
   // ❌ DANGEROUS: Closing without knowing current P&L
   await svc.CloseAllPositions();
   // Might realize huge losses if positions are underwater!

   // ✅ BETTER: Check P&L first
   var positions = await svc.OpenedOrdersAllAsync();
   double totalPL = positions.Orders.Sum(o => o.Profit);
   Console.WriteLine($"Current P&L: ${totalPL:F2}");
   Console.Write("Close all? (y/n): ");
   ```

7. **Filter logic confusion:**
   ```csharp
   // ⚠️ FILTER LOGIC:
   // symbol: null → ALL symbols
   // isBuy: null → BOTH buy and sell

   await svc.CloseAllPositions(symbol: null, isBuy: true);
   // Closes ALL buy positions across ALL symbols!

   await svc.CloseAllPositions(symbol: "EURUSD", isBuy: null);
   // Closes BOTH buy and sell positions for EURUSD
   ```

---

## 💡 Summary

**CloseAllPositions** provides nuclear-option liquidation of all market positions:

* ✅ Closes all market positions (BUY/SELL)
* ✅ Optional filtering by symbol and direction
* ✅ Returns count of closed positions
* ⚠️ **NO pending orders affected** (only market positions)
* ⚠️⚠️⚠️ **NO confirmation prompt** (executes immediately)
* ⚠️⚠️⚠️ **CAN REALIZE MASSIVE LOSSES** if used incorrectly

```csharp
// Close all positions for EURUSD:
int closed = await svc.CloseAllPositions(symbol: "EURUSD");

// Close only sell positions:
int closed = await svc.CloseAllPositions(isBuy: false);

// Close EVERYTHING (⚠️⚠️⚠️ NUCLEAR OPTION):
int closed = await svc.CloseAllPositions();
```

**This is the PANIC BUTTON. Use with EXTREME caution!** 🚨⚠️🚨
