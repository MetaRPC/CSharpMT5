# Close Position by Ticket (`CloseByTicket`)

> **Sugar method:** Closes order or position by ticket number with support for partial closure.

**API Information:**

* **Extension method:** `MT5Service.CloseByTicket(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Region:** [06] TRADING — MARKET & PENDING
* **Underlying calls:** `OrderCloseAsync()`

---

## Method Signature

```csharp
public static Task<OrderCloseData> CloseByTicket(
    this MT5Service svc,
    ulong ticket,
    double? volume = null,
    int timeoutSec = 15,
    CancellationToken ct = default)
```

---

## 🔽 Input

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance (extension method) |
| `ticket` | `ulong` | Order or position ticket number to close |
| `volume` | `double?` | Volume to close in lots. Pass null to close entire position (default: null) |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<OrderCloseData>` | Order close result with execution details |

---

## 💬 Just the essentials

* **What it is:** Closes position or pending order by ticket number - full or partial closure.
* **Why you need it:** Simplifies position closing - no manual OrderCloseRequest building.
* **Sanity check:** Pass `volume: null` for full close. For partial, specify exact volume (must not exceed position size).

---

## 🎯 Purpose

Use it for:

* **Full position closure** - exit entire position at market price
* **Partial position closure** - reduce position size (scale out)
* **Pending order cancellation** - cancel pending limit/stop orders
* **Emergency exits** - quickly close specific position
* **Take partial profits** - close portion while letting rest run

---

## 🔧 Under the Hood

```csharp
// Build close request
var req = new OrderCloseRequest
{
    Ticket = ticket,
    Volume = volume ?? 0  // 0 means close entire position
};

// Send close request
return await svc.OrderCloseAsync(req, deadline, ct);
```

**What it improves:**

* **Auto full close** - null volume = close entire position
* **Partial support** - specify exact volume to close
* **No request building** - method handles it
* **Simple API** - just ticket + optional volume

---

## 🔗 Usage Examples

### Example 1: Close Entire Position

```csharp
ulong ticket = 12345;

// Close entire position (volume = null)
var result = await svc.CloseByTicket(ticket);

Console.WriteLine($"✅ Position #{ticket} closed completely");
```

---

### Example 2: Partial Close (50% of Position)

```csharp
ulong ticket = 12345;
double totalVolume = 0.10;  // Current position size

// Close half the position
var result = await svc.CloseByTicket(ticket, volume: 0.05);

Console.WriteLine($"✅ Closed 0.05 lots, remaining: 0.05 lots");
```

---

### Example 3: Scale Out in Stages

```csharp
ulong ticket = 12345;
double totalVolume = 0.10;

// Take profits in 3 stages: 25%, 50%, 25%

// Stage 1: Close 25% at first target
await svc.CloseByTicket(ticket, volume: 0.025);
Console.WriteLine("✅ Stage 1: Closed 25% at TP1");

// Stage 2: Close 50% at second target
await svc.CloseByTicket(ticket, volume: 0.05);
Console.WriteLine("✅ Stage 2: Closed 50% at TP2");

// Stage 3: Close remaining 25% at third target
await svc.CloseByTicket(ticket);  // Close remainder
Console.WriteLine("✅ Stage 3: Closed remaining 25% at TP3");
```

---

### Example 4: Cancel Pending Order

```csharp
ulong pendingOrderTicket = 67890;

// Cancel pending limit/stop order
var result = await svc.CloseByTicket(pendingOrderTicket);

Console.WriteLine($"✅ Pending order #{pendingOrderTicket} cancelled");
```

---

### Example 5: Close Multiple Positions

```csharp
ulong[] tickets = { 12345, 12346, 12347 };

foreach (var ticket in tickets)
{
    try
    {
        await svc.CloseByTicket(ticket);
        Console.WriteLine($"✅ Closed position #{ticket}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Failed to close #{ticket}: {ex.Message}");
    }
}
```

---

### Example 6: Close With Error Handling

```csharp
ulong ticket = 12345;

try
{
    var result = await svc.CloseByTicket(ticket);

    if (result.ReturnedCode == 10009)
    {
        Console.WriteLine($"✅ Position closed successfully");
        Console.WriteLine($"   Deal: #{result.Deal}");
        Console.WriteLine($"   Volume: {result.Volume}");
    }
    else
    {
        Console.WriteLine($"❌ Close failed: {result.Comment}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Exception: {ex.Message}");
}
```

---

### Example 7: Partial Close Based on Profit

```csharp
public async Task TakePartialProfits(MT5Service svc, ulong ticket, double profitPips)
{
    var position = await GetPositionInfo(svc, ticket);

    double currentVolume = position.Volume;

    if (profitPips >= 100)
    {
        // Close 75% at 100 pips profit
        double closeVol = currentVolume * 0.75;
        await svc.CloseByTicket(ticket, volume: closeVol);
        Console.WriteLine($"✅ Took 75% profit at 100 pips");
    }
    else if (profitPips >= 50)
    {
        // Close 50% at 50 pips profit
        double closeVol = currentVolume * 0.50;
        await svc.CloseByTicket(ticket, volume: closeVol);
        Console.WriteLine($"✅ Took 50% profit at 50 pips");
    }
}
```

---

### Example 8: Emergency Close All

```csharp
public async Task EmergencyCloseAll(MT5Service svc)
{
    var opened = await svc.OpenedOrdersAsync(
        BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc);

    // Close all positions immediately
    foreach (var position in opened.PositionInfos)
    {
        await svc.CloseByTicket(position.Ticket);
        Console.WriteLine($"✅ Emergency close: #{position.Ticket}");
    }

    // Cancel all pending orders
    foreach (var order in opened.OpenedOrders)
    {
        await svc.CloseByTicket(order.Ticket);
        Console.WriteLine($"✅ Cancelled pending: #{order.Ticket}");
    }
}
```

---

### Example 9: Close Specific Volume

```csharp
ulong ticket = 12345;

// Close exactly 0.03 lots from position
var result = await svc.CloseByTicket(ticket, volume: 0.03);

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Closed {result.Volume} lots");
    Console.WriteLine($"   Deal price: {result.Price:F5}");
}
```

---

### Example 10: Smart Partial Close

```csharp
public async Task SmartPartialClose(MT5Service svc, ulong ticket)
{
    var position = await GetPositionInfo(svc, ticket);

    double currentVolume = position.Volume;
    double minVolume = 0.01;  // Broker minimum

    if (currentVolume > minVolume)
    {
        // Close 50% but ensure remaining is >= minimum
        double halfVolume = currentVolume / 2.0;

        if (currentVolume - halfVolume >= minVolume)
        {
            await svc.CloseByTicket(ticket, volume: halfVolume);
            Console.WriteLine($"✅ Closed {halfVolume} lots, remaining: {currentVolume - halfVolume}");
        }
        else
        {
            Console.WriteLine($"⚠️ Cannot partial close - remaining would be below minimum");
            Console.WriteLine($"   Closing entire position instead");
            await svc.CloseByTicket(ticket);
        }
    }
}
```

---

## 🔗 Related Methods

**📦 Low-level methods used internally:**

* `OrderCloseAsync()` - Sends close request to MT5 server

**🍬 Alternative Sugar methods:**

* `CloseAll()` - Close all positions with optional filters
* `CloseAllPositions()` - Close all market positions
* `CancelAll()` - Cancel all pending orders
* `ModifySlTpAsync()` - Modify SL/TP instead of closing

---

## ⚠️ Common Pitfalls

1. **Partial volume exceeds position size:**
   ```csharp
   // ❌ WRONG: Trying to close 1.0 lot from 0.5 lot position
   await svc.CloseByTicket(ticket, volume: 1.0);  // Will fail

   // ✅ CORRECT: Check position volume first
   var position = await GetPositionInfo(svc, ticket);
   double closeVolume = Math.Min(0.5, position.Volume);
   await svc.CloseByTicket(ticket, volume: closeVolume);
   ```

2. **Remaining volume below broker minimum:**
   ```csharp
   // ❌ WRONG: Closing 0.09 from 0.10 leaves 0.01 (may be below minimum)
   await svc.CloseByTicket(ticket, volume: 0.09);

   // ✅ CORRECT: Check broker minimum volume
   double minVol = await svc.GetVolumeMinAsync(symbol);
   double remaining = currentVolume - closeVolume;
   if (remaining >= minVol)
   {
       await svc.CloseByTicket(ticket, volume: closeVolume);
   }
   ```

3. **Trying to close already closed position:**
   ```csharp
   // ❌ Will throw exception or fail
   await svc.CloseByTicket(ticket: 99999);  // Non-existent ticket

   // ✅ CORRECT: Check if position exists first
   var opened = await svc.OpenedOrdersAsync(...);
   if (opened.PositionInfos.Any(p => p.Ticket == ticket))
   {
       await svc.CloseByTicket(ticket);
   }
   ```

---

## 💡 Summary

**CloseByTicket** provides clean position closing:

* ✅ Full or partial closure support
* ✅ Works with positions and pending orders
* ✅ No manual request building
* ✅ Simple API - just ticket + optional volume

```csharp
// Full close:
await svc.CloseByTicket(ticket: 12345);

// Partial close:
await svc.CloseByTicket(ticket: 12345, volume: 0.05);

// Cancel pending:
await svc.CloseByTicket(pendingTicket: 67890);
```

**Exit positions with precision!** 🚀
