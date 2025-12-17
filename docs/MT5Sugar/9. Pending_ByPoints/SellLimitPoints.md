# Sell Limit by Points (`SellLimitPoints`)

> **Sugar method:** Places Sell Limit pending order using point-based offset from current Bid price - no manual price calculation!

**API Information:**

* **Extension method:** `MT5Service.SellLimitPoints(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Region:** [09] PENDING HELPERS (BY POINTS)
* **Underlying calls:** `SymbolInfoTickAsync()` + `SymbolInfoDoubleAsync()` + `NormalizePriceAsync()` + `OrderSendAsync()`

---

## Method Signature

```csharp
public static async Task<OrderSendData> SellLimitPoints(
    this MT5Service svc,
    string symbol,
    double volume,
    double priceOffsetPoints,
    double? slPoints = null,
    double? tpPoints = null,
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
| `symbol` | `string` | Symbol name (e.g., "EURUSD", "XAUUSD") |
| `volume` | `double` | Volume in lots (e.g., 0.01, 0.1, 1.0) |
| `priceOffsetPoints` | `double` | Distance in points **above** current Bid (always positive) |
| `slPoints` | `double?` | Optional stop-loss distance in points **above** entry price |
| `tpPoints` | `double?` | Optional take-profit distance in points **below** entry price |
| `comment` | `string?` | Optional order comment |
| `deviationPoints` | `int` | Maximum price deviation in points (default: 0) |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<OrderSendData>` | Order send result with ticket number and execution details |

**Key fields:**

- `Order` - Order ticket number (ulong)
- `ReturnedCode` - MT5 return code (10009 = success)
- `Comment` - Server response comment

---

## 💬 Just the essentials

* **What it is:** Sell Limit helper - places order ABOVE current Bid using point-based offset instead of absolute price.
* **Why you need it:** No manual price calculation! Just say "50 points above" and SL/TP in points from entry.
* **Sanity check:** Sell Limit = sell ABOVE current price (better price). Offset is always positive, method adds to Bid automatically.

---

## 🎯 Purpose

Use it for:

* **Sell on rallies** - Enter short when price bounces up
* **Resistance level entries** - Place orders at resistance zones
* **Retracement trades** - Sell after pullback in downtrend
* **Mean reversion** - Sell when price gets overbought
* **Point-based strategies** - Work in points, not prices

---

## 🔧 Under the Hood

```csharp
// Step 1: Get current market quote
var tick = await svc.SymbolInfoTickAsync(symbol, deadline, ct);
var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolPoint, deadline, ct);

// Step 2: Calculate entry price (ABOVE Bid for Sell Limit)
var rawPrice = tick.Bid + Math.Abs(priceOffsetPoints) * point;
var price = await svc.NormalizePriceAsync(symbol, rawPrice, timeoutSec, ct);

// Step 3: Calculate SL/TP from entry price
double sl = slPoints.HasValue ? price + slPoints.Value * point : 0;
double tp = tpPoints.HasValue ? price - tpPoints.Value * point : 0;

// Step 4: Build and send pending order request (operationCode: 3 = SellLimit)
var req = BuildPendingRequest(symbol, volume, price, sl, tp, comment, deviationPoints, 3);
return await svc.OrderSendAsync(req, deadline, ct);
```

**What it improves:**

* **Auto price calculation** - converts points to absolute price
* **Auto Bid/Ask selection** - uses Bid as base for Sell orders
* **Auto normalization** - price rounded to tick size
* **Point-based SL/TP** - distances from entry, not from current price

---

## 🔗 Usage Examples

### Example 1: Basic Sell Limit 50 Points Above

```csharp
// Sell Limit 50 points above current Bid
var result = await svc.SellLimitPoints(
    symbol: "EURUSD",
    volume: 0.01,
    priceOffsetPoints: 50);

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Sell Limit placed: Ticket #{result.Order}");
}
```

---

### Example 2: Sell Limit with SL and TP

```csharp
// Sell Limit 30 points above with 20-point SL and 50-point TP
var result = await svc.SellLimitPoints(
    symbol: "EURUSD",
    volume: 0.05,
    priceOffsetPoints: 30,
    slPoints: 20,   // 20 points above entry
    tpPoints: 50);  // 50 points below entry

Console.WriteLine($"✅ Order #{result.Order}");
Console.WriteLine($"   Entry offset: 30 points above Bid");
Console.WriteLine($"   SL: 20 points, TP: 50 points");
```

---

### Example 3: Multiple Sell Limits (Grid Strategy)

```csharp
// Place grid of Sell Limit orders at different levels
int[] offsets = { 20, 40, 60, 80, 100 };

Console.WriteLine("Placing Sell Limit grid:");
foreach (var offset in offsets)
{
    var result = await svc.SellLimitPoints(
        "EURUSD",
        0.01,
        priceOffsetPoints: offset,
        slPoints: 30,
        tpPoints: 60);

    if (result.ReturnedCode == 10009)
    {
        Console.WriteLine($"  ✅ {offset}pts above: Ticket #{result.Order}");
    }
}

// Output:
// Placing Sell Limit grid:
//   ✅ 20pts above: Ticket #12350
//   ✅ 40pts above: Ticket #12351
//   ✅ 60pts above: Ticket #12352
//   ✅ 80pts above: Ticket #12353
//   ✅ 100pts above: Ticket #12354
```

---

### Example 4: ATR-Based Dynamic Offset

```csharp
// Use ATR for dynamic offset calculation
double atr = 0.0015;  // ATR value for EURUSD
double point = await svc.GetPointAsync("EURUSD");

// Convert ATR to points
double atrPoints = atr / point;

// Place Sell Limit at 0.5x ATR above (rally entry)
double offset = atrPoints * 0.5;

var result = await svc.SellLimitPoints(
    "EURUSD",
    0.01,
    priceOffsetPoints: offset,
    slPoints: atrPoints * 1.5,   // 1.5x ATR stop
    tpPoints: atrPoints * 3.0);  // 3x ATR target

Console.WriteLine($"ATR-based Sell Limit: {offset:F0} points above");
Console.WriteLine($"  SL: {atrPoints * 1.5:F0}pts, TP: {atrPoints * 3.0:F0}pts");
```

---

### Example 5: Resistance Level Entry

```csharp
public async Task PlaceSellAtResistance(
    MT5Service svc,
    string symbol,
    double resistanceLevel)
{
    // Get current Bid
    var tick = await svc.SymbolInfoTickAsync(symbol);
    double point = await svc.GetPointAsync(symbol);

    // Calculate offset from current Bid to resistance level
    double offsetPrice = resistanceLevel - tick.Bid;
    double offsetPoints = offsetPrice / point;

    if (offsetPoints > 0)  // Resistance is above current price
    {
        var result = await svc.SellLimitPoints(
            symbol,
            0.01,
            priceOffsetPoints: offsetPoints,
            slPoints: 50,
            tpPoints: 150);

        Console.WriteLine($"✅ Sell Limit at resistance {resistanceLevel:F5}");
        Console.WriteLine($"   Offset: {offsetPoints:F0} points above Bid");
    }
    else
    {
        Console.WriteLine($"⚠️ Resistance {resistanceLevel:F5} is below current Bid!");
    }
}

// Usage:
await PlaceSellAtResistance(svc, "EURUSD", resistanceLevel: 1.0950);
```

---

### Example 6: With Order Comment

```csharp
// Sell Limit with strategy identifier
var result = await svc.SellLimitPoints(
    symbol: "GBPUSD",
    volume: 0.02,
    priceOffsetPoints: 40,
    slPoints: 30,
    tpPoints: 90,
    comment: "SellRally_Strategy_v2");

Console.WriteLine($"✅ Order #{result.Order} - Comment: SellRally_Strategy_v2");
```

---

### Example 7: Calculate Entry Price Before Placing

```csharp
var tick = await svc.SymbolInfoTickAsync("EURUSD");
double point = await svc.GetPointAsync("EURUSD");
double offsetPoints = 50;

// Calculate what the entry price will be
double entryPrice = tick.Bid + offsetPoints * point;

Console.WriteLine($"Current Bid: {tick.Bid:F5}");
Console.WriteLine($"Entry will be: {entryPrice:F5} (50 points above)");
Console.WriteLine($"Placing order...");

var result = await svc.SellLimitPoints(
    "EURUSD",
    0.01,
    priceOffsetPoints: offsetPoints,
    slPoints: 20,
    tpPoints: 60);

Console.WriteLine($"✅ Order #{result.Order} placed at ~{entryPrice:F5}");
```

---

### Example 8: Error Handling

```csharp
try
{
    var result = await svc.SellLimitPoints(
        "EURUSD",
        0.01,
        priceOffsetPoints: 50,
        slPoints: 20,
        tpPoints: 60);

    if (result.ReturnedCode == 10009)
    {
        Console.WriteLine($"✅ Success: Ticket #{result.Order}");
    }
    else
    {
        Console.WriteLine($"❌ Order failed: {result.Comment}");
        Console.WriteLine($"   Return code: {result.ReturnedCode}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Exception: {ex.Message}");
}
```

---

### Example 9: Risk-Based Volume with Sell Limit

```csharp
// Calculate volume based on risk
double riskMoney = 100.0;
double slPoints = 30;

double volume = await svc.CalcVolumeForRiskAsync(
    "EURUSD",
    stopPoints: slPoints,
    riskMoney: riskMoney);

// Place Sell Limit with calculated volume
var result = await svc.SellLimitPoints(
    "EURUSD",
    volume: volume,
    priceOffsetPoints: 40,
    slPoints: slPoints,
    tpPoints: 100);

Console.WriteLine($"✅ Risk-based order placed:");
Console.WriteLine($"   Volume: {volume} lots");
Console.WriteLine($"   Risk: ${riskMoney} if SL hit");
Console.WriteLine($"   Ticket: #{result.Order}");
```

---

### Example 10: Overbought Sell Strategy

```csharp
public async Task SellWhenOverbought(MT5Service svc, string symbol)
{
    // Assume we have RSI indicator showing overbought
    double rsi = 75.0;  // Overbought threshold: 70+

    if (rsi > 70)
    {
        Console.WriteLine($"RSI {rsi} is overbought. Placing Sell Limit above.");

        // Place Sell Limit slightly above current price
        // to catch the rally before reversal
        var result = await svc.SellLimitPoints(
            symbol,
            0.01,
            priceOffsetPoints: 20,  // Small offset - expect reversal soon
            slPoints: 40,           // Tight stop
            tpPoints: 100,          // Larger target
            comment: $"OverboughtSell_RSI{rsi:F0}");

        if (result.ReturnedCode == 10009)
        {
            Console.WriteLine($"✅ Sell Limit placed: #{result.Order}");
            Console.WriteLine($"   Waiting for rally to {20} points above");
        }
    }
}

// Usage:
await SellWhenOverbought(svc, "EURUSD");
```

---

## 🔗 Related Methods

**📦 Methods used internally:**

* `SymbolInfoTickAsync()` - Gets current Bid/Ask
* `SymbolInfoDoubleAsync(SymbolPoint)` - Gets point size
* `NormalizePriceAsync()` - Normalizes entry price
* `OrderSendAsync()` - Sends pending order request

**🍬 Related Sugar methods:**

* `BuyLimitPoints()` - Buy Limit with point-based offset
* `BuyStopPoints()` - Buy Stop with point-based offset
* `SellStopPoints()` - Sell Stop with point-based offset
* `PlacePending()` - Generic pending order (requires absolute price)
* `PriceFromOffsetPointsAsync()` - Calculate price only (no order placement)

**📊 Alternative approaches:**

* `PlacePending()` - More flexible but requires manual price calculation
* `PriceFromOffsetPointsAsync()` + `PlacePending()` - Two-step approach

---

## ⚠️ Common Pitfalls

1. **Confusing offset direction:**
   ```csharp
   // ❌ WRONG: Thinking offset is BELOW Bid
   // Sell Limit is ALWAYS above current Bid!

   // ✅ CORRECT: Offset is distance ABOVE Bid
   await svc.SellLimitPoints("EURUSD", 0.01, priceOffsetPoints: 50);
   // Places order 50 points ABOVE current Bid
   ```

2. **Negative offset (method handles it):**
   ```csharp
   // ⚠️ Method uses Math.Abs(), so negative works but is confusing
   await svc.SellLimitPoints("EURUSD", 0.01, priceOffsetPoints: -50);
   // Same as priceOffsetPoints: 50 (absolute value taken)

   // ✅ BETTER: Always use positive values
   await svc.SellLimitPoints("EURUSD", 0.01, priceOffsetPoints: 50);
   ```

3. **SL/TP direction confusion:**
   ```csharp
   // ✅ CORRECT: SL/TP are distances FROM ENTRY, not from current price
   await svc.SellLimitPoints(
       "EURUSD",
       0.01,
       priceOffsetPoints: 50,
       slPoints: 20,  // 20 points ABOVE entry price (opposite direction)
       tpPoints: 60); // 60 points BELOW entry price (profit direction)

   // NOT from current Bid! They're relative to calculated entry.
   ```

4. **Using pips instead of points:**
   ```csharp
   // ❌ WRONG: Confusing pips with points
   await svc.SellLimitPoints("EURUSD", 0.01, priceOffsetPoints: 5);
   // For 5-digit broker, this is 0.5 pips, not 5 pips!

   // ✅ CORRECT: Convert pips to points for 5-digit brokers
   double pips = 5;
   double points = pips * 10;  // 1 pip = 10 points on 5-digit
   await svc.SellLimitPoints("EURUSD", 0.01, priceOffsetPoints: points);
   ```

5. **Offset too small (broker minimum distance):**
   ```csharp
   // ❌ May fail if offset violates broker's minimum stop level
   await svc.SellLimitPoints("EURUSD", 0.01, priceOffsetPoints: 5);
   // Broker may require minimum 10 points distance

   // ✅ CORRECT: Check broker requirements
   // Most brokers require 10-50 points minimum for pending orders
   await svc.SellLimitPoints("EURUSD", 0.01, priceOffsetPoints: 50);
   ```

6. **Confusing with Buy Limit:**
   ```csharp
   // ❌ WRONG: Using Buy Limit logic for Sell
   // Buy Limit = BELOW current price
   // Sell Limit = ABOVE current price

   // ✅ CORRECT: Remember the logic
   await svc.BuyLimitPoints("EURUSD", 0.01, priceOffsetPoints: 50);
   // -> 50 points BELOW Ask (buy cheaper)

   await svc.SellLimitPoints("EURUSD", 0.01, priceOffsetPoints: 50);
   // -> 50 points ABOVE Bid (sell higher)
   ```

---

## 💡 Summary

**SellLimitPoints** simplifies Sell Limit order placement:

* ✅ Work in points, not absolute prices
* ✅ Auto-calculates entry price ABOVE Bid
* ✅ SL/TP as point distances from entry
* ✅ Auto-normalizes prices to tick size
* ✅ Perfect for sell-on-rally strategies

```csharp
// Instead of manual calculation:
var tick = await svc.SymbolInfoTickAsync("EURUSD");
double price = tick.Bid + 50 * 0.00001;  // ❌ Manual, error-prone
await svc.PlacePending("EURUSD", 0.01, ENUM_ORDER_TYPE.OrderTypeSellLimit, price, ...);

// Use one clean call:
await svc.SellLimitPoints("EURUSD", 0.01, priceOffsetPoints: 50);  // ✅ Clean!
```

**Sell the rally with precision!** 🚀
