# Sell Stop by Points (`SellStopPoints`)

> **Sugar method:** Places Sell Stop pending order using point-based offset from current Bid price - downside breakout made easy!

**API Information:**

* **Extension method:** `MT5Service.SellStopPoints(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Region:** [09] PENDING HELPERS (BY POINTS)
* **Underlying calls:** `SymbolInfoTickAsync()` + `SymbolInfoDoubleAsync()` + `NormalizePriceAsync()` + `OrderSendAsync()`

---

## Method Signature

```csharp
public static async Task<OrderSendData> SellStopPoints(
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
| `priceOffsetPoints` | `double` | Distance in points **below** current Bid (always positive) |
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

* **What it is:** Sell Stop helper - places order BELOW current Bid using point-based offset. Triggers when price breaks DOWN.
* **Why you need it:** Perfect for breakdown strategies! No manual price calculation - just specify distance below current price.
* **Sanity check:** Sell Stop = sell BELOW current price (chase downside momentum). Order executes when price falls to trigger level.

---

## 🎯 Purpose

Use it for:

* **Support breakdown entries** - Sell when price breaks support
* **Downside momentum** - Chase downward price movement
* **Downtrend continuation** - Enter short when downtrend confirmed
* **Stop and reverse** - Exit long + enter short simultaneously
* **Triggered limit orders** - Activate sell after confirmation

---

## 🔧 Under the Hood

```csharp
// Step 1: Get current market quote
var tick = await svc.SymbolInfoTickAsync(symbol, deadline, ct);
var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolPoint, deadline, ct);

// Step 2: Calculate entry price (BELOW Bid for Sell Stop)
var rawPrice = tick.Bid - Math.Abs(priceOffsetPoints) * point;
var price = await svc.NormalizePriceAsync(symbol, rawPrice, timeoutSec, ct);

// Step 3: Calculate SL/TP from entry price
double sl = slPoints.HasValue ? price + slPoints.Value * point : 0;
double tp = tpPoints.HasValue ? price - tpPoints.Value * point : 0;

// Step 4: Build and send pending order request (operationCode: 5 = SellStop)
var req = BuildPendingRequest(symbol, volume, price, sl, tp, comment, deviationPoints, 5);
return await svc.OrderSendAsync(req, deadline, ct);
```

**What it improves:**

* **Auto price calculation** - converts points to absolute price
* **Auto Bid/Ask selection** - uses Bid as base for Sell orders
* **Auto normalization** - price rounded to tick size
* **Point-based SL/TP** - distances from entry, not from current price

---

## 🔗 Usage Examples

### Example 1: Basic Sell Stop 50 Points Below

```csharp
// Sell Stop 50 points below current Bid
var result = await svc.SellStopPoints(
    symbol: "EURUSD",
    volume: 0.01,
    priceOffsetPoints: 50);

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Sell Stop placed: Ticket #{result.Order}");
    Console.WriteLine($"   Will trigger 50 points below current Bid");
}
```

---

### Example 2: Sell Stop with SL and TP

```csharp
// Sell Stop 30 points below with 20-point SL and 60-point TP
var result = await svc.SellStopPoints(
    symbol: "EURUSD",
    volume: 0.05,
    priceOffsetPoints: 30,
    slPoints: 20,   // 20 points above entry
    tpPoints: 60);  // 60 points below entry

Console.WriteLine($"✅ Order #{result.Order}");
Console.WriteLine($"   Trigger: 30 points below Bid");
Console.WriteLine($"   SL: 20 points, TP: 60 points");
```

---

### Example 3: Support Breakdown Entry

```csharp
public async Task PlaceSellStopAtSupport(
    MT5Service svc,
    string symbol,
    double supportLevel)
{
    // Get current Bid
    var tick = await svc.SymbolInfoTickAsync(symbol);
    double point = await svc.GetPointAsync(symbol);

    // Calculate offset from current Bid to support breakdown
    double offsetPrice = tick.Bid - supportLevel;
    double offsetPoints = offsetPrice / point;

    if (offsetPoints > 0)  // Support is below current price
    {
        // Place Sell Stop just below support
        var result = await svc.SellStopPoints(
            symbol,
            0.01,
            priceOffsetPoints: offsetPoints + 10,  // 10 points below support
            slPoints: 50,
            tpPoints: 150,
            comment: "SupportBreakdown");

        Console.WriteLine($"✅ Sell Stop at {supportLevel:F5} - 10pts");
        Console.WriteLine($"   Ticket: #{result.Order}");
    }
    else
    {
        Console.WriteLine($"⚠️ Support {supportLevel:F5} is above current Bid!");
    }
}

// Usage:
await PlaceSellStopAtSupport(svc, "EURUSD", supportLevel: 1.0850);
```

---

### Example 4: ATR-Based Breakdown

```csharp
// Use ATR for dynamic breakdown threshold
double atr = 0.0015;  // ATR value for EURUSD
double point = await svc.GetPointAsync("EURUSD");

// Convert ATR to points
double atrPoints = atr / point;

// Place Sell Stop at 1x ATR below (strong breakdown)
var result = await svc.SellStopPoints(
    "EURUSD",
    0.01,
    priceOffsetPoints: atrPoints * 1.0,
    slPoints: atrPoints * 0.5,   // Tight stop (already in momentum)
    tpPoints: atrPoints * 2.0);  // 2x ATR target

Console.WriteLine($"ATR-based Sell Stop: {atrPoints:F0} points below");
Console.WriteLine($"  SL: {atrPoints * 0.5:F0}pts, TP: {atrPoints * 2.0:F0}pts");
```

---

### Example 5: Multiple Sell Stops (Scaling In)

```csharp
// Place multiple Sell Stops at different levels
// (scaling into position as downside momentum builds)
int[] offsets = { 30, 60, 90 };
double[] volumes = { 0.01, 0.02, 0.03 };  // Pyramid down

Console.WriteLine("Placing Sell Stop pyramid:");
for (int i = 0; i < offsets.Length; i++)
{
    var result = await svc.SellStopPoints(
        "EURUSD",
        volumes[i],
        priceOffsetPoints: offsets[i],
        slPoints: 40,
        tpPoints: 120);

    if (result.ReturnedCode == 10009)
    {
        Console.WriteLine($"  ✅ {offsets[i]}pts below: {volumes[i]} lots - Ticket #{result.Order}");
    }
}

// Output:
// Placing Sell Stop pyramid:
//   ✅ 30pts below: 0.01 lots - Ticket #12358
//   ✅ 60pts below: 0.02 lots - Ticket #12359
//   ✅ 90pts below: 0.03 lots - Ticket #12360
```

---

### Example 6: Round Number Breakdown

```csharp
public async Task PlaceSellStopAtRoundNumber(
    MT5Service svc,
    string symbol,
    double roundNumber)
{
    var tick = await svc.SymbolInfoTickAsync(symbol);
    double point = await svc.GetPointAsync(symbol);

    // Calculate offset to round number
    double offsetPrice = tick.Bid - roundNumber;
    double offsetPoints = offsetPrice / point;

    if (offsetPoints > 5)  // Only if round number is at least 5 points away
    {
        var result = await svc.SellStopPoints(
            symbol,
            0.01,
            priceOffsetPoints: offsetPoints,
            slPoints: 30,
            tpPoints: 100,
            comment: $"RoundBreak_{roundNumber}");

        Console.WriteLine($"✅ Sell Stop at {roundNumber:F5}");
        Console.WriteLine($"   Offset: {offsetPoints:F0} points below Bid");
    }
}

// Usage - wait for break below 1.0800
await PlaceSellStopAtRoundNumber(svc, "EURUSD", roundNumber: 1.0800);
```

---

### Example 7: With Order Comment

```csharp
// Sell Stop with strategy identifier
var result = await svc.SellStopPoints(
    symbol: "GBPUSD",
    volume: 0.02,
    priceOffsetPoints: 50,
    slPoints: 30,
    tpPoints: 100,
    comment: "Breakdown_Strategy_v3");

Console.WriteLine($"✅ Order #{result.Order} - Comment: Breakdown_Strategy_v3");
```

---

### Example 8: Error Handling

```csharp
try
{
    var result = await svc.SellStopPoints(
        "EURUSD",
        0.01,
        priceOffsetPoints: 50,
        slPoints: 20,
        tpPoints: 80);

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

### Example 9: Risk-Based Volume with Sell Stop

```csharp
// Calculate volume based on risk
double riskMoney = 100.0;
double slPoints = 30;

double volume = await svc.CalcVolumeForRiskAsync(
    "EURUSD",
    stopPoints: slPoints,
    riskMoney: riskMoney);

// Place Sell Stop with calculated volume
var result = await svc.SellStopPoints(
    "EURUSD",
    volume: volume,
    priceOffsetPoints: 40,
    slPoints: slPoints,
    tpPoints: 120);

Console.WriteLine($"✅ Risk-based Sell Stop placed:");
Console.WriteLine($"   Volume: {volume} lots");
Console.WriteLine($"   Risk: ${riskMoney} if SL hit");
Console.WriteLine($"   Ticket: #{result.Order}");
```

---

### Example 10: Calculate Trigger Price Before Placing

```csharp
var tick = await svc.SymbolInfoTickAsync("EURUSD");
double point = await svc.GetPointAsync("EURUSD");
double offsetPoints = 50;

// Calculate what the trigger price will be
double triggerPrice = tick.Bid - offsetPoints * point;

Console.WriteLine($"Current Bid: {tick.Bid:F5}");
Console.WriteLine($"Trigger will be: {triggerPrice:F5} (50 points below)");
Console.WriteLine($"Placing Sell Stop...");

var result = await svc.SellStopPoints(
    "EURUSD",
    0.01,
    priceOffsetPoints: offsetPoints,
    slPoints: 20,
    tpPoints: 80);

Console.WriteLine($"✅ Order #{result.Order} - triggers at ~{triggerPrice:F5}");
Console.WriteLine($"   Waiting for breakdown...");
```

---

## 🔗 Related Methods

**📦 Methods used internally:**

* `SymbolInfoTickAsync()` - Gets current Bid/Ask
* `SymbolInfoDoubleAsync(SymbolPoint)` - Gets point size
* `NormalizePriceAsync()` - Normalizes trigger price
* `OrderSendAsync()` - Sends pending order request

**🍬 Related Sugar methods:**

* `BuyStopPoints()` - Buy Stop with point-based offset
* `BuyLimitPoints()` - Buy Limit with point-based offset
* `SellLimitPoints()` - Sell Limit with point-based offset
* `PlacePending()` - Generic pending order (requires absolute price)
* `PriceFromOffsetPointsAsync()` - Calculate price only (no order placement)

**📊 Alternative approaches:**

* `PlacePending()` - More flexible but requires manual price calculation
* `PriceFromOffsetPointsAsync()` + `PlacePending()` - Two-step approach

---

## ⚠️ Common Pitfalls

1. **Confusing with Sell Limit:**
   ```csharp
   // ❌ CONFUSION: Sell Limit vs Sell Stop
   // Sell Limit = ABOVE current price (sell higher)
   // Sell Stop = BELOW current price (chase downside)

   // ✅ CORRECT usage:
   await svc.SellLimitPoints("EURUSD", 0.01, 50);  // 50 pts ABOVE (rally)
   await svc.SellStopPoints("EURUSD", 0.01, 50);   // 50 pts BELOW (breakdown)
   ```

2. **Negative offset (method handles it):**
   ```csharp
   // ⚠️ Method uses Math.Abs(), so negative works but is confusing
   await svc.SellStopPoints("EURUSD", 0.01, priceOffsetPoints: -50);
   // Same as priceOffsetPoints: 50 (absolute value taken)

   // ✅ BETTER: Always use positive values
   await svc.SellStopPoints("EURUSD", 0.01, priceOffsetPoints: 50);
   ```

3. **SL/TP direction confusion:**
   ```csharp
   // ✅ CORRECT: SL/TP are distances FROM ENTRY, not from current price
   await svc.SellStopPoints(
       "EURUSD",
       0.01,
       priceOffsetPoints: 50,
       slPoints: 20,  // 20 points ABOVE entry price (opposite direction)
       tpPoints: 80); // 80 points BELOW entry price (profit direction)

   // Entry is 50 points below current Bid, then SL/TP from there!
   ```

4. **Using pips instead of points:**
   ```csharp
   // ❌ WRONG: Confusing pips with points
   await svc.SellStopPoints("EURUSD", 0.01, priceOffsetPoints: 5);
   // For 5-digit broker, this is 0.5 pips, not 5 pips!

   // ✅ CORRECT: Convert pips to points for 5-digit brokers
   double pips = 5;
   double points = pips * 10;  // 1 pip = 10 points on 5-digit
   await svc.SellStopPoints("EURUSD", 0.01, priceOffsetPoints: points);
   ```

5. **Offset too small (broker minimum distance):**
   ```csharp
   // ❌ May fail if offset violates broker's minimum stop level
   await svc.SellStopPoints("EURUSD", 0.01, priceOffsetPoints: 5);
   // Broker may require minimum 10-50 points distance

   // ✅ CORRECT: Check broker requirements
   await svc.SellStopPoints("EURUSD", 0.01, priceOffsetPoints: 50);
   ```

6. **Not understanding execution:**
   ```csharp
   // ⚠️ Sell Stop does NOT execute immediately!
   var result = await svc.SellStopPoints("EURUSD", 0.01, 50);
   // Order is PENDING until price falls 50 points

   // Only executes if/when Bid reaches trigger level
   // If price never reaches, order stays pending forever
   ```

---

## 💡 Summary

**SellStopPoints** simplifies Sell Stop order placement:

* ✅ Work in points, not absolute prices
* ✅ Auto-calculates trigger price BELOW Bid
* ✅ SL/TP as point distances from entry
* ✅ Auto-normalizes prices to tick size
* ✅ Perfect for breakdown strategies

```csharp
// Instead of manual calculation:
var tick = await svc.SymbolInfoTickAsync("EURUSD");
double price = tick.Bid - 50 * 0.00001;  // ❌ Manual, error-prone
await svc.PlacePending("EURUSD", 0.01, ENUM_ORDER_TYPE.OrderTypeSellStop, price, ...);

// Use one clean call:
await svc.SellStopPoints("EURUSD", 0.01, priceOffsetPoints: 50);  // ✅ Clean!
```

**Catch the breakdown with confidence!** 🚀
