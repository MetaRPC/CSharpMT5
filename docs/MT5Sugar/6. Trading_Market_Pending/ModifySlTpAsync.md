# Modify Stop-Loss/Take-Profit (`ModifySlTpAsync`)

> **Sugar method:** Modifies stop-loss and/or take-profit for existing order or position by ticket number.

**API Information:**

* **Extension method:** `MT5Service.ModifySlTpAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Region:** [06] TRADING — MARKET & PENDING
* **Underlying calls:** `OrderModifyAsync()`

---

## Method Signature

```csharp
public static Task<OrderModifyData> ModifySlTpAsync(
    this MT5Service svc,
    ulong ticket,
    double? slPrice = null,
    double? tpPrice = null,
    int timeoutSec = 10,
    CancellationToken ct = default)
```

---

## 🔽 Input

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance (extension method) |
| `ticket` | `ulong` | Order or position ticket number |
| `slPrice` | `double?` | New stop-loss price (absolute). Pass null to keep unchanged |
| `tpPrice` | `double?` | New take-profit price (absolute). Pass null to keep unchanged |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<OrderModifyData>` | Order modify result with execution details |

---

## 💬 Just the essentials

* **What it is:** Updates SL/TP for existing order or position - modify one or both at once.
* **Why you need it:** Simpler than building OrderModifyRequest manually - just provide new prices.
* **Sanity check:** At least one parameter (slPrice or tpPrice) must be provided. Pass absolute price values, not points.

---

## 🎯 Purpose

Use it for:

* Trailing stop-loss as price moves in your favor
* Moving to breakeven after initial profit
* Adjusting take-profit based on new market conditions
* Tightening stops to lock in profit
* Removing SL/TP (set to 0)

---

## 🔧 Under the Hood

```csharp
// Validation: At least one parameter required
if (slPrice is null && tpPrice is null)
    throw new ArgumentException("Provide at least one: slPrice or tpPrice");

// Build modify request
var req = new OrderModifyRequest { Ticket = ticket };

// Set only explicitly provided fields
if (slPrice is double sl) req.StopLoss = sl;
if (tpPrice is double tp) req.TakeProfit = tp;

// Send modify request
return await svc.OrderModifyAsync(req, deadline, ct);
```

**What it improves:**

* **Partial updates** - modify only SL, only TP, or both
* **No request building** - method handles it automatically
* **Null handling** - only provided values are modified
* **Validation** - throws if both params are null

---

## 🔗 Usage Examples

### Example 1: Modify Only Stop-Loss

```csharp
ulong ticket = 12345;

// Move stop-loss to breakeven
var result = await svc.ModifySlTpAsync(ticket, slPrice: 1.0900);

Console.WriteLine($"✅ Stop-loss updated for ticket #{ticket}");
```

---

### Example 2: Modify Only Take-Profit

```csharp
ulong ticket = 12345;

// Adjust take-profit to new target
var result = await svc.ModifySlTpAsync(ticket, tpPrice: 1.1000);

Console.WriteLine($"✅ Take-profit updated for ticket #{ticket}");
```

---

### Example 3: Modify Both SL and TP

```csharp
ulong ticket = 12345;

// Update both stops at once
var result = await svc.ModifySlTpAsync(
    ticket,
    slPrice: 1.0920,
    tpPrice: 1.1000);

Console.WriteLine($"✅ Both SL and TP updated for ticket #{ticket}");
```

---

### Example 4: Trailing Stop-Loss

```csharp
// Position details
ulong ticket = 12345;
double entryPrice = 1.0900;
double currentBid = 1.0950;
double trailingPoints = 30;  // 30 points trailing

// Calculate new SL (trailing behind current price)
double pointSize = 0.00001;  // for EURUSD
double newSL = currentBid - (trailingPoints * pointSize);

// Only update if new SL is better than current
var result = await svc.ModifySlTpAsync(ticket, slPrice: newSL);

Console.WriteLine($"✅ Trailing stop updated to {newSL:F5}");
```

---

### Example 5: Move to Breakeven After Profit

```csharp
ulong ticket = 12345;
double entryPrice = 1.0900;
double currentBid = 1.0950;
int breakevenTriggerPips = 20;

// Check if price moved 20 pips in profit
double pointSize = 0.00001;
double profitPips = (currentBid - entryPrice) / pointSize;

if (profitPips >= breakevenTriggerPips)
{
    // Move SL to breakeven (entry price)
    var result = await svc.ModifySlTpAsync(ticket, slPrice: entryPrice);
    Console.WriteLine($"✅ Moved to breakeven at {entryPrice:F5}");
}
```

---

### Example 6: Remove Stop-Loss (Set to 0)

```csharp
ulong ticket = 12345;

// Remove stop-loss by setting it to 0
var result = await svc.ModifySlTpAsync(ticket, slPrice: 0);

Console.WriteLine($"⚠️ Stop-loss removed for ticket #{ticket}");
```

---

### Example 7: Tighten Stops to Lock Profit

```csharp
ulong ticket = 12345;
double entryPrice = 1.0900;
double currentBid = 1.0970;
double lockedProfitPips = 50;

// Lock in 50 pips profit
double pointSize = 0.00001;
double newSL = entryPrice + (lockedProfitPips * pointSize);

var result = await svc.ModifySlTpAsync(ticket, slPrice: newSL);

Console.WriteLine($"✅ Locked {lockedProfitPips} pips profit. New SL: {newSL:F5}");
```

---

### Example 8: Update Multiple Positions

```csharp
// Update SL for multiple positions
ulong[] tickets = { 12345, 12346, 12347 };
double newSL = 1.0920;

foreach (var ticket in tickets)
{
    try
    {
        await svc.ModifySlTpAsync(ticket, slPrice: newSL);
        Console.WriteLine($"✅ Updated ticket #{ticket}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Failed to update ticket #{ticket}: {ex.Message}");
    }
}
```

---

### Example 9: Error Handling

```csharp
ulong ticket = 12345;

try
{
    var result = await svc.ModifySlTpAsync(ticket, slPrice: 1.0920, tpPrice: 1.1000);
    Console.WriteLine($"✅ Modified successfully");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"❌ Invalid arguments: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Modification failed: {ex.Message}");
}
```

---

### Example 10: Conditional Modification Strategy

```csharp
public async Task ManagePosition(MT5Service svc, ulong ticket, double currentPrice)
{
    var position = await GetPositionInfo(svc, ticket);

    double entryPrice = position.OpenPrice;
    double currentSL = position.StopLoss;
    double profitPoints = (currentPrice - entryPrice) / 0.00001;

    // Strategy: Move to BE at 20 pips, trail at 50 pips
    if (profitPoints >= 50 && currentSL < entryPrice + 0.0020)
    {
        // Trail 20 pips behind
        double newSL = currentPrice - 0.0020;
        await svc.ModifySlTpAsync(ticket, slPrice: newSL);
        Console.WriteLine($"✅ Trailing stop activated: {newSL:F5}");
    }
    else if (profitPoints >= 20 && currentSL < entryPrice)
    {
        // Move to breakeven
        await svc.ModifySlTpAsync(ticket, slPrice: entryPrice);
        Console.WriteLine($"✅ Moved to breakeven: {entryPrice:F5}");
    }
}
```

---

## 🔗 Related Methods

**📦 Low-level methods used internally:**

* `OrderModifyAsync()` - Sends modify request to MT5 server

**🍬 Alternative Sugar methods:**

* `PlaceMarket()` - Place market order with SL/TP
* `PlacePending()` - Place pending order with SL/TP
* `CloseByTicket()` - Close position by ticket

---

## ⚠️ Common Pitfalls

1. **Forgetting to provide at least one parameter:**
   ```csharp
   // ❌ WRONG: Both null
   await svc.ModifySlTpAsync(ticket: 12345);
   // Throws: ArgumentException

   // ✅ CORRECT: Provide at least one
   await svc.ModifySlTpAsync(ticket: 12345, slPrice: 1.0920);
   ```

2. **Using relative points instead of absolute prices:**
   ```csharp
   // ❌ WRONG: Points offset
   await svc.ModifySlTpAsync(ticket: 12345, slPrice: 50);  // Not 50 points!

   // ✅ CORRECT: Absolute price
   double currentBid = 1.0950;
   double sl = currentBid - 50 * 0.00001;  // 50 points below
   await svc.ModifySlTpAsync(ticket: 12345, slPrice: sl);
   ```

3. **Invalid SL/TP levels (too close to current price):**
   ```csharp
   // ❌ May fail if SL too close to market price
   // Check broker's minimum stop level (e.g., 10 points)

   // ✅ CORRECT: Use proper distance
   double minStopLevel = 10 * pointSize;
   double newSL = currentPrice - minStopLevel;
   ```

---

## 💡 Summary

**ModifySlTpAsync** provides clean stop modification:

* ✅ Modify SL only, TP only, or both
* ✅ No manual request building
* ✅ Null handling for partial updates
* ✅ Validation prevents empty modifications

```csharp
// Simple modifications:
await svc.ModifySlTpAsync(ticket, slPrice: 1.0920);           // Update SL
await svc.ModifySlTpAsync(ticket, tpPrice: 1.1000);           // Update TP
await svc.ModifySlTpAsync(ticket, slPrice: 1.0920, tpPrice: 1.1000);  // Both
```

**Manage positions like a pro!** 🚀
