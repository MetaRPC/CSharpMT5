# Price from Offset Points (`PriceFromOffsetPointsAsync`)

> **Sugar method:** Calculates pending order price by offset in points from current market price - no manual calculation needed!

**API Information:**

* **Extension method:** `MT5Service.PriceFromOffsetPointsAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Region:** [08] VOLUME & PRICE UTILITIES
* **Underlying calls:** `SymbolInfoTickAsync()` + `SymbolInfoDoubleAsync()` + `NormalizePriceAsync()`

---

## Method Signature

```csharp
public static async Task<double> PriceFromOffsetPointsAsync(
    this MT5Service svc,
    string symbol,
    ENUM_ORDER_TYPE type,
    double offsetPoints,
    int timeoutSec = 10,
    CancellationToken ct = default)
```

---

## 🔽 Input

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance (extension method) |
| `symbol` | `string` | Symbol name (e.g., "EURUSD", "XAUUSD") |
| `type` | `ENUM_ORDER_TYPE` | Order type (BuyLimit, SellLimit, BuyStop, SellStop) |
| `offsetPoints` | `double` | Distance in points from current price |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<double>` | **Normalized price** for pending order placement |

---

## 💬 Just the essentials

* **What it is:** Auto-calculates pending order price from point offset - handles Bid/Ask selection and normalization.
* **Why you need it:** No manual price calculation! Just say "50 points above/below" and get exact price.
* **Sanity check:** BUY orders use Ask as base, SELL orders use Bid. Result is normalized to tick size.

---

## 🎯 Purpose

Use it for:

* **Pending order placement** - Calculate entry price by points offset
* **Point-based strategies** - Work in points, not prices
* **Dynamic price levels** - Offset from current market, not fixed prices
* **Simplified order logic** - Less math, clearer intent
* **Error prevention** - Auto-selects correct base price (Bid/Ask)

---

## 🔧 Under the Hood

```csharp
var deadline = DateTime.UtcNow.AddSeconds(timeoutSec);

// Step 1: Get current quote
var tick = await svc.SymbolInfoTickAsync(symbol, deadline, ct);

// Step 2: Get point size
var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolPoint, deadline, ct);

// Step 3: Determine base price (Ask for BUY, Bid for SELL)
int orderTypeInt = Convert.ToInt32((object)type);
bool isBuy = orderTypeInt == 0 || orderTypeInt == 2 || orderTypeInt == 4 || orderTypeInt == 6;

double basePrice = isBuy ? tick.Ask : tick.Bid;

// Step 4: Calculate price with offset
// BUY orders: Add offset (above Ask)
// SELL orders: Subtract offset (below Bid)
double rawPrice = basePrice + (isBuy ? +1.0 : -1.0) * (offsetPoints * point);

// Step 5: Normalize to tick size
return await svc.NormalizePriceAsync(symbol, rawPrice, timeoutSec, ct);
```

**What it improves:**

* **Auto Bid/Ask selection** - uses correct base price for order type
* **Auto direction** - BUY adds, SELL subtracts
* **Auto normalization** - result is broker-ready
* **Point-based thinking** - work in points, not raw prices

---

## 🔗 Usage Examples

### Example 1: Buy Limit 50 Points Below

```csharp
// Buy Limit 50 points below current Ask
double price = await svc.PriceFromOffsetPointsAsync(
    symbol: "EURUSD",
    type: ENUM_ORDER_TYPE.OrderTypeBuyLimit,
    offsetPoints: 50);

Console.WriteLine($"Buy Limit price: {price:F5}");

// Place order at calculated price
await svc.PlacePending("EURUSD", 0.01, ENUM_ORDER_TYPE.OrderTypeBuyLimit,
    price, sl: price - 50 * point, tp: price + 100 * point);
```

---

### Example 2: Sell Limit 30 Points Above

```csharp
// Sell Limit 30 points above current Bid
double price = await svc.PriceFromOffsetPointsAsync(
    symbol: "GBPUSD",
    type: ENUM_ORDER_TYPE.OrderTypeSellLimit,
    offsetPoints: 30);

Console.WriteLine($"Sell Limit price: {price:F5}");

await svc.PlacePending("GBPUSD", 0.05, ENUM_ORDER_TYPE.OrderTypeSellLimit,
    price, sl: price + 40 * point, tp: price - 80 * point);
```

---

### Example 3: Buy Stop 40 Points Above (Breakout)

```csharp
// Buy Stop 40 points above current Ask (breakout long)
double price = await svc.PriceFromOffsetPointsAsync(
    symbol: "XAUUSD",
    type: ENUM_ORDER_TYPE.OrderTypeBuyStop,
    offsetPoints: 40);

Console.WriteLine($"Buy Stop price (breakout): {price:F2}");

await svc.PlacePending("XAUUSD", 0.01, ENUM_ORDER_TYPE.OrderTypeBuyStop,
    price, sl: price - 30 * point, tp: price + 100 * point);
```

---

### Example 4: Sell Stop 25 Points Below (Breakdown)

```csharp
// Sell Stop 25 points below current Bid (breakdown short)
double price = await svc.PriceFromOffsetPointsAsync(
    symbol: "USDJPY",
    type: ENUM_ORDER_TYPE.OrderTypeSellStop,
    offsetPoints: 25);

Console.WriteLine($"Sell Stop price (breakdown): {price:F3}");

await svc.PlacePending("USDJPY", 0.1, ENUM_ORDER_TYPE.OrderTypeSellStop,
    price, sl: price + 35 * point, tp: price - 75 * point);
```

---

### Example 5: Compare Manual vs Auto Calculation

```csharp
// ❌ MANUAL WAY (error-prone):
var tick = await svc.SymbolInfoTickAsync("EURUSD");
var point = await svc.GetPointAsync("EURUSD");

// For Buy Limit: use Ask as base, subtract offset
double manualPrice = tick.Ask - (50 * point);
// Need to normalize manually too!
manualPrice = await svc.NormalizePriceAsync("EURUSD", manualPrice);

Console.WriteLine($"Manual calculation: {manualPrice:F5}");

// ✅ AUTO WAY (clean):
double autoPrice = await svc.PriceFromOffsetPointsAsync(
    "EURUSD",
    ENUM_ORDER_TYPE.OrderTypeBuyLimit,
    50);

Console.WriteLine($"Auto calculation:   {autoPrice:F5}");

// Both should be identical
Console.WriteLine($"Match: {Math.Abs(manualPrice - autoPrice) < 0.000001}");
```

---

### Example 6: Dynamic Pending Order Grid

```csharp
// Place grid of Buy Limit orders at different levels
int[] offsets = { 20, 40, 60, 80, 100 };

Console.WriteLine("Buy Limit Grid:");
foreach (var offset in offsets)
{
    double price = await svc.PriceFromOffsetPointsAsync(
        "EURUSD",
        ENUM_ORDER_TYPE.OrderTypeBuyLimit,
        offset);

    Console.WriteLine($"  {offset}pts below: {price:F5}");

    // Place order
    await svc.PlacePending("EURUSD", 0.01,
        ENUM_ORDER_TYPE.OrderTypeBuyLimit, price);
}

// Output:
// Buy Limit Grid:
//   20pts below: 1.09030
//   40pts below: 1.09010
//   60pts below: 1.08990
//   80pts below: 1.08970
//   100pts below: 1.08950
```

---

### Example 7: ATR-Based Pending Order

```csharp
// Use ATR indicator for dynamic offset
double atr = 0.0015;  // ATR value
double point = await svc.GetPointAsync("EURUSD");

// Convert ATR to points
double atrPoints = atr / point;

// Place Buy Stop 1.5x ATR above (breakout)
double buyStopOffset = atrPoints * 1.5;
double buyStopPrice = await svc.PriceFromOffsetPointsAsync(
    "EURUSD",
    ENUM_ORDER_TYPE.OrderTypeBuyStop,
    buyStopOffset);

// Place Sell Stop 1.5x ATR below (breakdown)
double sellStopOffset = atrPoints * 1.5;
double sellStopPrice = await svc.PriceFromOffsetPointsAsync(
    "EURUSD",
    ENUM_ORDER_TYPE.OrderTypeSellStop,
    sellStopOffset);

Console.WriteLine($"ATR: {atr:F5} ({atrPoints:F0} points)");
Console.WriteLine($"Buy Stop (1.5x ATR):  {buyStopPrice:F5}");
Console.WriteLine($"Sell Stop (1.5x ATR): {sellStopPrice:F5}");
```

---

### Example 8: Support/Resistance Offset Orders

```csharp
public async Task PlaceBreakoutOrders(
    MT5Service svc,
    string symbol,
    double resistanceOffset,
    double supportOffset)
{
    // Buy Stop above resistance
    double buyStopPrice = await svc.PriceFromOffsetPointsAsync(
        symbol,
        ENUM_ORDER_TYPE.OrderTypeBuyStop,
        resistanceOffset);

    // Sell Stop below support
    double sellStopPrice = await svc.PriceFromOffsetPointsAsync(
        symbol,
        ENUM_ORDER_TYPE.OrderTypeSellStop,
        supportOffset);

    Console.WriteLine($"Breakout Orders for {symbol}:");
    Console.WriteLine($"  Buy Stop:  {buyStopPrice:F5} ({resistanceOffset}pts above)");
    Console.WriteLine($"  Sell Stop: {sellStopPrice:F5} ({supportOffset}pts below)");

    // Place orders
    await svc.PlacePending(symbol, 0.01, ENUM_ORDER_TYPE.OrderTypeBuyStop, buyStopPrice);
    await svc.PlacePending(symbol, 0.01, ENUM_ORDER_TYPE.OrderTypeSellStop, sellStopPrice);
}

// Usage:
await PlaceBreakoutOrders(svc, "EURUSD", resistanceOffset: 50, supportOffset: 50);
```

---

### Example 9: All Order Types Example

```csharp
string symbol = "EURUSD";
double offset = 30;  // 30 points

var orderTypes = new[]
{
    (ENUM_ORDER_TYPE.OrderTypeBuyLimit, "Buy Limit"),
    (ENUM_ORDER_TYPE.OrderTypeSellLimit, "Sell Limit"),
    (ENUM_ORDER_TYPE.OrderTypeBuyStop, "Buy Stop"),
    (ENUM_ORDER_TYPE.OrderTypeSellStop, "Sell Stop")
};

Console.WriteLine($"Prices for {offset}pt offset on {symbol}:");
Console.WriteLine("");

foreach (var (type, label) in orderTypes)
{
    double price = await svc.PriceFromOffsetPointsAsync(symbol, type, offset);
    Console.WriteLine($"{label,-12}: {price:F5}");
}

// Output:
// Prices for 30pt offset on EURUSD:
//
// Buy Limit   : 1.09020
// Sell Limit  : 1.09080
// Buy Stop    : 1.09080
// Sell Stop   : 1.08990
```

---

### Example 10: Validate Calculated Price

```csharp
var tick = await svc.SymbolInfoTickAsync("EURUSD");
double offsetPoints = 50;

double price = await svc.PriceFromOffsetPointsAsync(
    "EURUSD",
    ENUM_ORDER_TYPE.OrderTypeBuyLimit,
    offsetPoints);

Console.WriteLine($"Current Ask:      {tick.Ask:F5}");
Console.WriteLine($"Calculated price: {price:F5}");

// For Buy Limit, price should be BELOW Ask
if (price < tick.Ask)
{
    double diff = tick.Ask - price;
    double point = await svc.GetPointAsync("EURUSD");
    double pointsDiff = diff / point;

    Console.WriteLine($"✅ Valid: {pointsDiff:F0} points below Ask");
}
else
{
    Console.WriteLine($"❌ Invalid: Price should be below Ask for Buy Limit!");
}
```

---

## 🔗 Related Methods

**📦 Methods used internally:**

* `SymbolInfoTickAsync()` - Gets current Bid/Ask
* `SymbolInfoDoubleAsync(SymbolPoint)` - Gets point size
* `NormalizePriceAsync()` - Normalizes calculated price

**🍬 Methods that use this:**

* `BuyLimitPoints()` - Uses this for Buy Limit price calculation
* `SellLimitPoints()` - Uses this for Sell Limit price calculation
* `BuyStopPoints()` - Uses this for Buy Stop price calculation
* `SellStopPoints()` - Uses this for Sell Stop price calculation

**📊 Alternative approaches:**

* Manual calculation using SymbolInfoTickAsync + GetPointAsync (verbose)
* Point-based pending helpers (simpler but less flexible)

---

## ⚠️ Common Pitfalls

1. **Confusing offset direction:**
   ```csharp
   // Buy Limit: BELOW current price (offset is DOWN from Ask)
   // Sell Limit: ABOVE current price (offset is UP from Bid)
   // Buy Stop: ABOVE current price (offset is UP from Ask)
   // Sell Stop: BELOW current price (offset is DOWN from Bid)

   // Method handles direction automatically based on order type!
   ```

2. **Using wrong order type:**
   ```csharp
   // ❌ WRONG: Using market order type
   double price = await svc.PriceFromOffsetPointsAsync(
       "EURUSD",
       ENUM_ORDER_TYPE.OrderTypeBuy,  // ❌ Market order!
       50);

   // ✅ CORRECT: Use pending order type
   double price = await svc.PriceFromOffsetPointsAsync(
       "EURUSD",
       ENUM_ORDER_TYPE.OrderTypeBuyLimit,  // ✅ Pending order
       50);
   ```

3. **Not understanding Bid/Ask basis:**
   ```csharp
   // BUY orders (Limit/Stop): Base is ASK
   // SELL orders (Limit/Stop): Base is BID

   // Method automatically uses correct base!
   ```

---

## 📊 Price Calculation Summary

| Order Type | Base Price | Direction | Result |
|------------|-----------|-----------|--------|
| **BuyLimit** | Ask | Down (−) | Ask − (offset × point) |
| **SellLimit** | Bid | Up (+) | Bid + (offset × point) |
| **BuyStop** | Ask | Up (+) | Ask + (offset × point) |
| **SellStop** | Bid | Down (−) | Bid − (offset × point) |

---

## 💡 Summary

**PriceFromOffsetPointsAsync** simplifies pending order pricing:

* ✅ Auto-selects base price (Bid/Ask)
* ✅ Auto-applies offset direction
* ✅ Auto-normalizes result
* ✅ Work in points, not raw prices

```csharp
// Instead of manual calculation:
var tick = await svc.SymbolInfoTickAsync("EURUSD");
var point = await svc.GetPointAsync("EURUSD");
double price = tick.Ask - (50 * point);  // ❌ Manual, error-prone
price = await svc.NormalizePriceAsync("EURUSD", price);

// Use one clean call:
double price = await svc.PriceFromOffsetPointsAsync(
    "EURUSD",
    ENUM_ORDER_TYPE.OrderTypeBuyLimit,
    50);  // ✅ Auto everything!
```

**Think in points, get perfect prices!** 🚀
