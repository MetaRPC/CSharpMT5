# Place Pending Order (`PlacePending`)

> **Sugar method:** Places a pending order (Buy/Sell Limit or Stop) at specified price with automatic symbol preparation.

**API Information:**

* **Extension method:** `MT5Service.PlacePending(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Region:** [06] TRADING — MARKET & PENDING
* **Underlying calls:** `EnsureSelected()` + `OrderSendAsync()`

---

## Method Signature

```csharp
public static async Task<OrderSendData> PlacePending(
    this MT5Service svc,
    string symbol,
    double volume,
    ENUM_ORDER_TYPE type,
    double price,
    double? sl = null,
    double? tp = null,
    string? comment = null,
    int deviationPoints = 0,
    int timeoutSec = 15,
    CancellationToken ct = default)
```

---

## 🔽 Input

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance (extension method) |
| `symbol` | `string` | Symbol to trade (e.g., "EURUSD", "XAUUSD") |
| `volume` | `double` | Volume in lots |
| `type` | `ENUM_ORDER_TYPE` | Order type (BuyLimit, SellLimit, BuyStop, SellStop) |
| `price` | `double` | Entry price for pending order |
| `sl` | `double?` | Optional stop-loss price (default: null) |
| `tp` | `double?` | Optional take-profit price (default: null) |
| `comment` | `string?` | Optional order comment (default: null) |
| `deviationPoints` | `int` | Maximum price deviation in points (default: 0) |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<OrderSendData>` | Order send result with ticket number and execution details |

### `OrderSendData` Structure

Key fields:

- `Order` (ulong) - Ticket number of created pending order
- `ReturnedCode` (uint) - Execution result code (10009 = success)
- `Comment` (string) - Server response message
- `Volume` (double) - Actual volume executed
- `Price` (double) - Order entry price

---

## 💬 Just the essentials

* **What it is:** Places pending order (Limit/Stop) at specified price with automatic symbol selection.
* **Why you need it:** Simplifies pending order placement - handles symbol preparation and request building automatically.
* **Sanity check:** Check `result.ReturnedCode == 10009` for success. Pending order activated when price reaches specified level.

---

## 🎯 Purpose

Use it for:

* **Buy Limit** - Buy when price drops to specified level (expect dip before rally)
* **Sell Limit** - Sell when price rises to specified level (expect rise before drop)
* **Buy Stop** - Buy when price breaks above specified level (breakout up)
* **Sell Stop** - Sell when price breaks below specified level (breakdown)

---

## 🔧 Under the Hood

```csharp
// Step 1: Ensure symbol is selected and synchronized
await svc.EnsureSelected(symbol, timeoutSec, ct);

// Step 2: Build pending order request
var req = new OrderSendRequest
{
    Symbol = symbol,
    Volume = volume,
    Price = price,                    // Entry price
    StopLoss = sl ?? 0,
    TakeProfit = tp ?? 0,
    Comment = comment ?? string.Empty,
    Slippage = (ulong)Math.Max(0, deviationPoints),
    Operation = type  // BuyLimit/SellLimit/BuyStop/SellStop
};

// Step 3: Send order
return await svc.OrderSendAsync(req, deadline, ct);
```

**What it improves:**

* **Auto symbol selection** - ensures symbol ready before order
* **Request building** - no manual OrderSendRequest construction
* **Type safety** - uses ENUM_ORDER_TYPE enum
* **Reflection-based** - sets Operation via reflection to avoid hard enum dependency

---

## 🔗 Usage Examples

### Example 1: Buy Limit (Buy on Dip)

```csharp
// Current price: 1.0900
// Want to buy at 1.0850 (below current price)

var result = await svc.PlacePending(
    symbol: "EURUSD",
    volume: 0.01,
    type: ENUM_ORDER_TYPE.OrderTypeBuyLimit,
    price: 1.0850,
    sl: 1.0800,      // 50 pips below entry
    tp: 1.0950);     // 100 pips above entry

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Buy Limit placed: #{result.Order} at 1.0850");
}
```

---

### Example 2: Sell Limit (Sell on Rise)

```csharp
// Current price: 1.2500
// Want to sell at 1.2550 (above current price)

var result = await svc.PlacePending(
    symbol: "GBPUSD",
    volume: 0.05,
    type: ENUM_ORDER_TYPE.OrderTypeSellLimit,
    price: 1.2550,
    sl: 1.2600,      // 50 pips above entry
    tp: 1.2450);     // 100 pips below entry

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Sell Limit placed: #{result.Order} at 1.2550");
}
```

---

### Example 3: Buy Stop (Breakout Up)

```csharp
// Current price: 2000.0
// Want to buy if price breaks above 2010.0 (resistance)

var result = await svc.PlacePending(
    symbol: "XAUUSD",
    volume: 0.01,
    type: ENUM_ORDER_TYPE.OrderTypeBuyStop,
    price: 2010.0,
    sl: 2000.0,      // Back to current price
    tp: 2030.0);     // Target above breakout

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Buy Stop placed: #{result.Order} at 2010.0");
    Console.WriteLine("Will activate on breakout above resistance");
}
```

---

### Example 4: Sell Stop (Breakdown)

```csharp
// Current price: 150.00
// Want to sell if price breaks below 149.50 (support)

var result = await svc.PlacePending(
    symbol: "USDJPY",
    volume: 0.1,
    type: ENUM_ORDER_TYPE.OrderTypeSellStop,
    price: 149.50,
    sl: 150.50,      // Above current price
    tp: 148.50);     // Target below breakdown

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Sell Stop placed: #{result.Order} at 149.50");
    Console.WriteLine("Will activate on breakdown below support");
}
```

---

### Example 5: Pending Order Without SL/TP

```csharp
// Place pending order without stop-loss or take-profit
var result = await svc.PlacePending(
    symbol: "EURUSD",
    volume: 0.01,
    type: ENUM_ORDER_TYPE.OrderTypeBuyLimit,
    price: 1.0850);

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Pending order placed: #{result.Order}");
    Console.WriteLine("No SL/TP - will manage manually");
}
```

---

### Example 6: With Custom Comment

```csharp
var result = await svc.PlacePending(
    symbol: "BTCUSD",
    volume: 0.01,
    type: ENUM_ORDER_TYPE.OrderTypeBuyLimit,
    price: 50000.0,
    sl: 49000.0,
    tp: 52000.0,
    comment: "Strategy: Support Bounce");

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Order #{result.Order}: {result.Comment}");
}
```

---

### Example 7: Error Handling

```csharp
var result = await svc.PlacePending(
    symbol: "EURUSD",
    volume: 0.01,
    type: ENUM_ORDER_TYPE.OrderTypeBuyLimit,
    price: 1.0850,
    sl: 1.0800,
    tp: 1.0950);

switch (result.ReturnedCode)
{
    case 10009:
        Console.WriteLine($"✅ Success! Order #{result.Order}");
        break;
    case 10015:
        Console.WriteLine($"❌ Invalid price: {result.Comment}");
        break;
    case 10016:
        Console.WriteLine($"❌ Invalid stops: {result.Comment}");
        break;
    default:
        Console.WriteLine($"❌ Failed: Code {result.ReturnedCode}, {result.Comment}");
        break;
}
```

---

## 🔗 Related Methods

**📦 Low-level methods used internally:**

* `EnsureSelected()` - Ensures symbol is selected before order placement
* `OrderSendAsync()` - Sends the order request to MT5 server

**🍬 Alternative Sugar methods:**

* `BuyLimitPoints()` - Buy Limit by points offset (no manual price calculation)
* `SellLimitPoints()` - Sell Limit by points offset
* `BuyStopPoints()` - Buy Stop by points offset
* `SellStopPoints()` - Sell Stop by points offset
* `PlaceMarket()` - Place market order (immediate execution)

---

## ⚠️ Common Pitfalls

1. **Wrong price level for order type:**
   ```csharp
   // ❌ WRONG: Buy Limit above current price
   // Current: 1.0900, trying to place Buy Limit at 1.0950
   // Buy Limit must be BELOW current price!

   // ✅ CORRECT:
   await svc.PlacePending("EURUSD", 0.01,
       ENUM_ORDER_TYPE.OrderTypeBuyLimit,
       price: 1.0850);  // Below current 1.0900
   ```

2. **Confusing Limit vs Stop:**
   - **Limit** = Price must improve (buy cheaper, sell higher)
   - **Stop** = Price breaks level (buy on breakout, sell on breakdown)

3. **SL/TP in wrong direction:**
   ```csharp
   // ❌ WRONG: Buy order with SL above entry
   await svc.PlacePending("EURUSD", 0.01,
       ENUM_ORDER_TYPE.OrderTypeBuyLimit,
       price: 1.0850,
       sl: 1.0900);  // ❌ SL should be below for BUY!

   // ✅ CORRECT:
   await svc.PlacePending("EURUSD", 0.01,
       ENUM_ORDER_TYPE.OrderTypeBuyLimit,
       price: 1.0850,
       sl: 1.0800);  // ✅ SL below entry
   ```

---

## 📊 Order Type Quick Reference

| Type | Direction | Price Requirement | When Activated | Use Case |
|------|-----------|-------------------|----------------|----------|
| **BuyLimit** | Buy | Below current | Price drops to level | Buy the dip |
| **SellLimit** | Sell | Above current | Price rises to level | Sell at resistance |
| **BuyStop** | Buy | Above current | Price breaks up | Breakout long |
| **SellStop** | Sell | Below current | Price breaks down | Breakdown short |

---

## 💡 Summary

**PlacePending** simplifies pending order placement with:

* ✅ Automatic symbol selection
* ✅ Request building handled automatically
* ✅ Support for all pending order types
* ✅ Optional SL/TP configuration

```csharp
// One method for all pending order types:
await svc.PlacePending("EURUSD", 0.01, ENUM_ORDER_TYPE.OrderTypeBuyLimit, 1.0850);
await svc.PlacePending("GBPUSD", 0.05, ENUM_ORDER_TYPE.OrderTypeSellLimit, 1.2550);
await svc.PlacePending("XAUUSD", 0.01, ENUM_ORDER_TYPE.OrderTypeBuyStop, 2010.0);
await svc.PlacePending("USDJPY", 0.1, ENUM_ORDER_TYPE.OrderTypeSellStop, 149.5);
```

**Clean, flexible, powerful!** 🚀
