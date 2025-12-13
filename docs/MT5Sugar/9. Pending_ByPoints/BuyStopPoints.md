# Buy Stop by Points (`BuyStopPoints`)

> **Sugar method:** Places Buy Stop pending order using point-based offset from current Ask price - breakout entry made easy!

**API Information:**

* **Extension method:** `MT5Service.BuyStopPoints(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Region:** [09] PENDING HELPERS (BY POINTS)
* **Underlying calls:** `SymbolInfoTickAsync()` + `SymbolInfoDoubleAsync()` + `NormalizePriceAsync()` + `OrderSendAsync()`

---

## Method Signature

```csharp
public static async Task<OrderSendData> BuyStopPoints(
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
| `priceOffsetPoints` | `double` | Distance in points **above** current Ask (always positive) |
| `slPoints` | `double?` | Optional stop-loss distance in points **below** entry price |
| `tpPoints` | `double?` | Optional take-profit distance in points **above** entry price |
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

* **What it is:** Buy Stop helper - places order ABOVE current Ask using point-based offset. Triggers when price breaks UP.
* **Why you need it:** Perfect for breakout strategies! No manual price calculation - just specify distance above current price.
* **Sanity check:** Buy Stop = buy ABOVE current price (chase momentum). Order executes when price rises to trigger level.

---

## 🎯 Purpose

Use it for:

* **Breakout entries** - Buy when price breaks resistance
* **Momentum trading** - Chase upward price movement
* **Trend continuation** - Enter long when uptrend confirmed
* **Stop and reverse** - Exit short + enter long simultaneously
* **Triggered limit orders** - Activate buy after confirmation

---

## 🔧 Under the Hood

```csharp
// Step 1: Get current market quote
var tick = await svc.SymbolInfoTickAsync(symbol, deadline, ct);
var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolPoint, deadline, ct);

// Step 2: Calculate entry price (ABOVE Ask for Buy Stop)
var rawPrice = tick.Ask + Math.Abs(priceOffsetPoints) * point;
var price = await svc.NormalizePriceAsync(symbol, rawPrice, timeoutSec, ct);

// Step 3: Calculate SL/TP from entry price
double sl = slPoints.HasValue ? price - slPoints.Value * point : 0;
double tp = tpPoints.HasValue ? price + tpPoints.Value * point : 0;

// Step 4: Build and send pending order request (operationCode: 4 = BuyStop)
var req = BuildPendingRequest(symbol, volume, price, sl, tp, comment, deviationPoints, 4);
return await svc.OrderSendAsync(req, deadline, ct);
```

**What it improves:**

* **Auto price calculation** - converts points to absolute price
* **Auto Bid/Ask selection** - uses Ask as base for Buy orders
* **Auto normalization** - price rounded to tick size
* **Point-based SL/TP** - distances from entry, not from current price

---

## 🔗 Usage Examples

### Example 1: Basic Buy Stop 50 Points Above

```csharp
// Buy Stop 50 points above current Ask
var result = await svc.BuyStopPoints(
    symbol: "EURUSD",
    volume: 0.01,
    priceOffsetPoints: 50);

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Buy Stop placed: Ticket #{result.Order}");
    Console.WriteLine($"   Will trigger 50 points above current Ask");
}
```

---

### Example 2: Buy Stop with SL and TP

```csharp
// Buy Stop 30 points above with 20-point SL and 60-point TP
var result = await svc.BuyStopPoints(
    symbol: "EURUSD",
    volume: 0.05,
    priceOffsetPoints: 30,
    slPoints: 20,   // 20 points below entry
    tpPoints: 60);  // 60 points above entry

Console.WriteLine($"✅ Order #{result.Order}");
Console.WriteLine($"   Trigger: 30 points above Ask");
Console.WriteLine($"   SL: 20 points, TP: 60 points");
```

---

### Example 3: Resistance Breakout Entry

```csharp
public async Task PlaceBuyStopAtResistance(
    MT5Service svc,
    string symbol,
    double resistanceLevel)
{
    // Get current Ask
    var tick = await svc.SymbolInfoTickAsync(symbol);
    double point = await svc.GetPointAsync(symbol);

    // Calculate offset from current Ask to resistance breakout
    double offsetPrice = resistanceLevel - tick.Ask;
    double offsetPoints = offsetPrice / point;

    if (offsetPoints > 0)  // Resistance is above current price
    {
        // Place Buy Stop just above resistance
        var result = await svc.BuyStopPoints(
            symbol,
            0.01,
            priceOffsetPoints: offsetPoints + 10,  // 10 points above resistance
            slPoints: 50,
            tpPoints: 150,
            comment: "ResistanceBreakout");

        Console.WriteLine($"✅ Buy Stop at {resistanceLevel:F5} + 10pts");
        Console.WriteLine($"   Ticket: #{result.Order}");
    }
    else
    {
        Console.WriteLine($"⚠️ Resistance {resistanceLevel:F5} is below current Ask!");
    }
}

// Usage:
await PlaceBuyStopAtResistance(svc, "EURUSD", resistanceLevel: 1.0950);
```

---

### Example 4: ATR-Based Breakout

```csharp
// Use ATR for dynamic breakout threshold
double atr = 0.0015;  // ATR value for EURUSD
double point = await svc.GetPointAsync("EURUSD");

// Convert ATR to points
double atrPoints = atr / point;

// Place Buy Stop at 1x ATR above (strong breakout)
var result = await svc.BuyStopPoints(
    "EURUSD",
    0.01,
    priceOffsetPoints: atrPoints * 1.0,
    slPoints: atrPoints * 0.5,   // Tight stop (already in momentum)
    tpPoints: atrPoints * 2.0);  // 2x ATR target

Console.WriteLine($"ATR-based Buy Stop: {atrPoints:F0} points above");
Console.WriteLine($"  SL: {atrPoints * 0.5:F0}pts, TP: {atrPoints * 2.0:F0}pts");
```

---

### Example 5: Multiple Buy Stops (Scaling In)

```csharp
// Place multiple Buy Stops at different levels
// (scaling into position as momentum builds)
int[] offsets = { 30, 60, 90 };
double[] volumes = { 0.01, 0.02, 0.03 };  // Pyramid up

Console.WriteLine("Placing Buy Stop pyramid:");
for (int i = 0; i < offsets.Length; i++)
{
    var result = await svc.BuyStopPoints(
        "EURUSD",
        volumes[i],
        priceOffsetPoints: offsets[i],
        slPoints: 40,
        tpPoints: 120);

    if (result.ReturnedCode == 10009)
    {
        Console.WriteLine($"  ✅ {offsets[i]}pts above: {volumes[i]} lots - Ticket #{result.Order}");
    }
}

// Output:
// Placing Buy Stop pyramid:
//   ✅ 30pts above: 0.01 lots - Ticket #12355
//   ✅ 60pts above: 0.02 lots - Ticket #12356
//   ✅ 90pts above: 0.03 lots - Ticket #12357
```

---

### Example 6: Round Number Breakout

```csharp
public async Task PlaceBuyStopAtRoundNumber(
    MT5Service svc,
    string symbol,
    double roundNumber)
{
    var tick = await svc.SymbolInfoTickAsync(symbol);
    double point = await svc.GetPointAsync(symbol);

    // Calculate offset to round number
    double offsetPrice = roundNumber - tick.Ask;
    double offsetPoints = offsetPrice / point;

    if (offsetPoints > 5)  // Only if round number is at least 5 points away
    {
        var result = await svc.BuyStopPoints(
            symbol,
            0.01,
            priceOffsetPoints: offsetPoints,
            slPoints: 30,
            tpPoints: 100,
            comment: $"RoundBreak_{roundNumber}");

        Console.WriteLine($"✅ Buy Stop at {roundNumber:F5}");
        Console.WriteLine($"   Offset: {offsetPoints:F0} points above Ask");
    }
}

// Usage - wait for break above 1.1000
await PlaceBuyStopAtRoundNumber(svc, "EURUSD", roundNumber: 1.1000);
```

---

### Example 7: With Order Comment

```csharp
// Buy Stop with strategy identifier
var result = await svc.BuyStopPoints(
    symbol: "GBPUSD",
    volume: 0.02,
    priceOffsetPoints: 50,
    slPoints: 30,
    tpPoints: 100,
    comment: "Breakout_Strategy_v3");

Console.WriteLine($"✅ Order #{result.Order} - Comment: Breakout_Strategy_v3");
```

---

### Example 8: Error Handling

```csharp
try
{
    var result = await svc.BuyStopPoints(
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

### Example 9: Risk-Based Volume with Buy Stop

```csharp
// Calculate volume based on risk
double riskMoney = 100.0;
double slPoints = 30;

double volume = await svc.CalcVolumeForRiskAsync(
    "EURUSD",
    stopPoints: slPoints,
    riskMoney: riskMoney);

// Place Buy Stop with calculated volume
var result = await svc.BuyStopPoints(
    "EURUSD",
    volume: volume,
    priceOffsetPoints: 40,
    slPoints: slPoints,
    tpPoints: 120);

Console.WriteLine($"✅ Risk-based Buy Stop placed:");
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
double triggerPrice = tick.Ask + offsetPoints * point;

Console.WriteLine($"Current Ask: {tick.Ask:F5}");
Console.WriteLine($"Trigger will be: {triggerPrice:F5} (50 points above)");
Console.WriteLine($"Placing Buy Stop...");

var result = await svc.BuyStopPoints(
    "EURUSD",
    0.01,
    priceOffsetPoints: offsetPoints,
    slPoints: 20,
    tpPoints: 80);

Console.WriteLine($"✅ Order #{result.Order} - triggers at ~{triggerPrice:F5}");
Console.WriteLine($"   Waiting for breakout...");
```

---

## 🔗 Related Methods

**📦 Methods used internally:**

* `SymbolInfoTickAsync()` - Gets current Bid/Ask
* `SymbolInfoDoubleAsync(SymbolPoint)` - Gets point size
* `NormalizePriceAsync()` - Normalizes trigger price
* `OrderSendAsync()` - Sends pending order request

**🍬 Related Sugar methods:**

* `SellStopPoints()` - Sell Stop with point-based offset
* `BuyLimitPoints()` - Buy Limit with point-based offset
* `SellLimitPoints()` - Sell Limit with point-based offset
* `PlacePending()` - Generic pending order (requires absolute price)
* `PriceFromOffsetPointsAsync()` - Calculate price only (no order placement)

**📊 Alternative approaches:**

* `PlacePending()` - More flexible but requires manual price calculation
* `PriceFromOffsetPointsAsync()` + `PlacePending()` - Two-step approach

---

## ⚠️ Common Pitfalls

1. **Confusing with Buy Limit:**
   ```csharp
   // ❌ CONFUSION: Buy Limit vs Buy Stop
   // Buy Limit = BELOW current price (buy cheaper)
   // Buy Stop = ABOVE current price (chase momentum)

   // ✅ CORRECT usage:
   await svc.BuyLimitPoints("EURUSD", 0.01, 50);  // 50 pts BELOW (pullback)
   await svc.BuyStopPoints("EURUSD", 0.01, 50);   // 50 pts ABOVE (breakout)
   ```

2. **Negative offset (method handles it):**
   ```csharp
   // ⚠️ Method uses Math.Abs(), so negative works but is confusing
   await svc.BuyStopPoints("EURUSD", 0.01, priceOffsetPoints: -50);
   // Same as priceOffsetPoints: 50 (absolute value taken)

   // ✅ BETTER: Always use positive values
   await svc.BuyStopPoints("EURUSD", 0.01, priceOffsetPoints: 50);
   ```

3. **SL/TP direction confusion:**
   ```csharp
   // ✅ CORRECT: SL/TP are distances FROM ENTRY, not from current price
   await svc.BuyStopPoints(
       "EURUSD",
       0.01,
       priceOffsetPoints: 50,
       slPoints: 20,  // 20 points BELOW entry price
       tpPoints: 80); // 80 points ABOVE entry price

   // Entry is 50 points above current Ask, then SL/TP from there!
   ```

4. **Using pips instead of points:**
   ```csharp
   // ❌ WRONG: Confusing pips with points
   await svc.BuyStopPoints("EURUSD", 0.01, priceOffsetPoints: 5);
   // For 5-digit broker, this is 0.5 pips, not 5 pips!

   // ✅ CORRECT: Convert pips to points for 5-digit brokers
   double pips = 5;
   double points = pips * 10;  // 1 pip = 10 points on 5-digit
   await svc.BuyStopPoints("EURUSD", 0.01, priceOffsetPoints: points);
   ```

5. **Offset too small (broker minimum distance):**
   ```csharp
   // ❌ May fail if offset violates broker's minimum stop level
   await svc.BuyStopPoints("EURUSD", 0.01, priceOffsetPoints: 5);
   // Broker may require minimum 10-50 points distance

   // ✅ CORRECT: Check broker requirements
   await svc.BuyStopPoints("EURUSD", 0.01, priceOffsetPoints: 50);
   ```

6. **Not understanding execution:**
   ```csharp
   // ⚠️ Buy Stop does NOT execute immediately!
   var result = await svc.BuyStopPoints("EURUSD", 0.01, 50);
   // Order is PENDING until price rises 50 points

   // Only executes if/when Ask reaches trigger level
   // If price never reaches, order stays pending forever
   ```

---

## 💡 Summary

**BuyStopPoints** simplifies Buy Stop order placement:

* ✅ Work in points, not absolute prices
* ✅ Auto-calculates trigger price ABOVE Ask
* ✅ SL/TP as point distances from entry
* ✅ Auto-normalizes prices to tick size
* ✅ Perfect for breakout strategies

```csharp
// Instead of manual calculation:
var tick = await svc.SymbolInfoTickAsync("EURUSD");
double price = tick.Ask + 50 * 0.00001;  // ❌ Manual, error-prone
await svc.PlacePending("EURUSD", 0.01, ENUM_ORDER_TYPE.OrderTypeBuyStop, price, ...);

// Use one clean call:
await svc.BuyStopPoints("EURUSD", 0.01, priceOffsetPoints: 50);  // ✅ Clean!
```

**Chase the breakout with confidence!** 🚀
