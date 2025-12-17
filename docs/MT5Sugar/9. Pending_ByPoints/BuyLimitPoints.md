# Buy Limit by Points (`BuyLimitPoints`)

> **Sugar method:** Places Buy Limit pending order using point-based offset from current Ask price - no manual price calculation!

**API Information:**

* **Extension method:** `MT5Service.BuyLimitPoints(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Region:** [09] PENDING HELPERS (BY POINTS)
* **Underlying calls:** `SymbolInfoTickAsync()` + `SymbolInfoDoubleAsync()` + `NormalizePriceAsync()` + `OrderSendAsync()`

---

## Method Signature

```csharp
public static async Task<OrderSendData> BuyLimitPoints(
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
| `priceOffsetPoints` | `double` | Distance in points **below** current Ask (always positive) |
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

* **What it is:** Buy Limit helper - places order BELOW current Ask using point-based offset instead of absolute price.
* **Why you need it:** No manual price calculation! Just say "50 points below" and SL/TP in points from entry.
* **Sanity check:** Buy Limit = buy BELOW current price (cheaper). Offset is always positive, method subtracts from Ask automatically.

---

## 🎯 Purpose

Use it for:

* **Buy on dips** - Enter long when price pulls back
* **Support level entries** - Place orders at support zones
* **Retracement trades** - Buy after pullback in uptrend
* **Dollar-cost averaging** - Multiple Buy Limits at different levels
* **Point-based strategies** - Work in points, not prices

---

## 🔧 Under the Hood

```csharp
// Step 1: Get current market quote
var tick = await svc.SymbolInfoTickAsync(symbol, deadline, ct);
var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolPoint, deadline, ct);

// Step 2: Calculate entry price (BELOW Ask for Buy Limit)
var rawPrice = tick.Ask - Math.Abs(priceOffsetPoints) * point;
var price = await svc.NormalizePriceAsync(symbol, rawPrice, timeoutSec, ct);

// Step 3: Calculate SL/TP from entry price
double sl = slPoints.HasValue ? price - slPoints.Value * point : 0;
double tp = tpPoints.HasValue ? price + tpPoints.Value * point : 0;

// Step 4: Build and send pending order request (operationCode: 2 = BuyLimit)
var req = BuildPendingRequest(symbol, volume, price, sl, tp, comment, deviationPoints, 2);
return await svc.OrderSendAsync(req, deadline, ct);
```

**What it improves:**

* **Auto price calculation** - converts points to absolute price
* **Auto Bid/Ask selection** - uses Ask as base for Buy orders
* **Auto normalization** - price rounded to tick size
* **Point-based SL/TP** - distances from entry, not from current price

---

## 🔗 Usage Examples

### Example 1: Basic Buy Limit 50 Points Below

```csharp
// Buy Limit 50 points below current Ask
var result = await svc.BuyLimitPoints(
    symbol: "EURUSD",
    volume: 0.01,
    priceOffsetPoints: 50);

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Buy Limit placed: Ticket #{result.Order}");
}
```

---

### Example 2: Buy Limit with SL and TP

```csharp
// Buy Limit 30 points below with 20-point SL and 50-point TP
var result = await svc.BuyLimitPoints(
    symbol: "EURUSD",
    volume: 0.05,
    priceOffsetPoints: 30,
    slPoints: 20,   // 20 points below entry
    tpPoints: 50);  // 50 points above entry

Console.WriteLine($"✅ Order #{result.Order}");
Console.WriteLine($"   Entry offset: 30 points below Ask");
Console.WriteLine($"   SL: 20 points, TP: 50 points");
```

---

### Example 3: Multiple Buy Limits (Grid Strategy)

```csharp
// Place grid of Buy Limit orders at different levels
int[] offsets = { 20, 40, 60, 80, 100 };

Console.WriteLine("Placing Buy Limit grid:");
foreach (var offset in offsets)
{
    var result = await svc.BuyLimitPoints(
        "EURUSD",
        0.01,
        priceOffsetPoints: offset,
        slPoints: 30,
        tpPoints: 60);

    if (result.ReturnedCode == 10009)
    {
        Console.WriteLine($"  ✅ {offset}pts below: Ticket #{result.Order}");
    }
}

// Output:
// Placing Buy Limit grid:
//   ✅ 20pts below: Ticket #12345
//   ✅ 40pts below: Ticket #12346
//   ✅ 60pts below: Ticket #12347
//   ✅ 80pts below: Ticket #12348
//   ✅ 100pts below: Ticket #12349
```

---

### Example 4: ATR-Based Dynamic Offset

```csharp
// Use ATR for dynamic offset calculation
double atr = 0.0015;  // ATR value for EURUSD
double point = await svc.GetPointAsync("EURUSD");

// Convert ATR to points
double atrPoints = atr / point;

// Place Buy Limit at 0.5x ATR below (pullback entry)
double offset = atrPoints * 0.5;

var result = await svc.BuyLimitPoints(
    "EURUSD",
    0.01,
    priceOffsetPoints: offset,
    slPoints: atrPoints * 1.5,   // 1.5x ATR stop
    tpPoints: atrPoints * 3.0);  // 3x ATR target

Console.WriteLine($"ATR-based Buy Limit: {offset:F0} points below");
Console.WriteLine($"  SL: {atrPoints * 1.5:F0}pts, TP: {atrPoints * 3.0:F0}pts");
```

---

### Example 5: Support Level Entry

```csharp
public async Task PlaceBuyAtSupport(
    MT5Service svc,
    string symbol,
    double supportLevel)
{
    // Get current Ask
    var tick = await svc.SymbolInfoTickAsync(symbol);
    double point = await svc.GetPointAsync(symbol);

    // Calculate offset from current Ask to support level
    double offsetPrice = tick.Ask - supportLevel;
    double offsetPoints = offsetPrice / point;

    if (offsetPoints > 0)  // Support is below current price
    {
        var result = await svc.BuyLimitPoints(
            symbol,
            0.01,
            priceOffsetPoints: offsetPoints,
            slPoints: 50,
            tpPoints: 150);

        Console.WriteLine($"✅ Buy Limit at support {supportLevel:F5}");
        Console.WriteLine($"   Offset: {offsetPoints:F0} points below Ask");
    }
    else
    {
        Console.WriteLine($"⚠️ Support {supportLevel:F5} is above current Ask!");
    }
}

// Usage:
await PlaceBuyAtSupport(svc, "EURUSD", supportLevel: 1.0850);
```

---

### Example 6: With Order Comment

```csharp
// Buy Limit with strategy identifier
var result = await svc.BuyLimitPoints(
    symbol: "GBPUSD",
    volume: 0.02,
    priceOffsetPoints: 40,
    slPoints: 30,
    tpPoints: 90,
    comment: "BuyDip_Strategy_v2");

Console.WriteLine($"✅ Order #{result.Order} - Comment: BuyDip_Strategy_v2");
```

---

### Example 7: Calculate Entry Price Before Placing

```csharp
var tick = await svc.SymbolInfoTickAsync("EURUSD");
double point = await svc.GetPointAsync("EURUSD");
double offsetPoints = 50;

// Calculate what the entry price will be
double entryPrice = tick.Ask - offsetPoints * point;

Console.WriteLine($"Current Ask: {tick.Ask:F5}");
Console.WriteLine($"Entry will be: {entryPrice:F5} (50 points below)");
Console.WriteLine($"Placing order...");

var result = await svc.BuyLimitPoints(
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
    var result = await svc.BuyLimitPoints(
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

### Example 9: Risk-Based Volume with Buy Limit

```csharp
// Calculate volume based on risk
double riskMoney = 100.0;
double slPoints = 30;

double volume = await svc.CalcVolumeForRiskAsync(
    "EURUSD",
    stopPoints: slPoints,
    riskMoney: riskMoney);

// Place Buy Limit with calculated volume
var result = await svc.BuyLimitPoints(
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

### Example 10: Compare Manual vs Auto Calculation

```csharp
var tick = await svc.SymbolInfoTickAsync("EURUSD");
double point = await svc.GetPointAsync("EURUSD");
double offsetPoints = 50;
double slPoints = 20;
double tpPoints = 60;

// ❌ MANUAL WAY (error-prone):
double entryPriceManual = tick.Ask - offsetPoints * point;
entryPriceManual = await svc.NormalizePriceAsync("EURUSD", entryPriceManual);
double slManual = entryPriceManual - slPoints * point;
double tpManual = entryPriceManual + tpPoints * point;

var reqManual = new OrderSendRequest
{
    Symbol = "EURUSD",
    Volume = 0.01,
    Price = entryPriceManual,
    StopLoss = slManual,
    TakeProfit = tpManual,
    Operation = ENUM_ORDER_TYPE.OrderTypeBuyLimit
};
var resultManual = await svc.OrderSendAsync(reqManual);

Console.WriteLine($"Manual way: {resultManual.Order}");

// ✅ AUTO WAY (clean):
var resultAuto = await svc.BuyLimitPoints(
    "EURUSD",
    0.01,
    priceOffsetPoints: 50,
    slPoints: 20,
    tpPoints: 60);

Console.WriteLine($"Auto way: {resultAuto.Order}");
```

---

## 🔗 Related Methods

**📦 Methods used internally:**

* `SymbolInfoTickAsync()` - Gets current Bid/Ask
* `SymbolInfoDoubleAsync(SymbolPoint)` - Gets point size
* `NormalizePriceAsync()` - Normalizes entry price
* `OrderSendAsync()` - Sends pending order request

**🍬 Related Sugar methods:**

* `SellLimitPoints()` - Sell Limit with point-based offset
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
   // ❌ WRONG: Thinking offset is ABOVE Ask
   // Buy Limit is ALWAYS below current Ask!

   // ✅ CORRECT: Offset is distance BELOW Ask
   await svc.BuyLimitPoints("EURUSD", 0.01, priceOffsetPoints: 50);
   // Places order 50 points BELOW current Ask
   ```

2. **Negative offset (method handles it):**
   ```csharp
   // ⚠️ Method uses Math.Abs(), so negative works but is confusing
   await svc.BuyLimitPoints("EURUSD", 0.01, priceOffsetPoints: -50);
   // Same as priceOffsetPoints: 50 (absolute value taken)

   // ✅ BETTER: Always use positive values
   await svc.BuyLimitPoints("EURUSD", 0.01, priceOffsetPoints: 50);
   ```

3. **SL/TP direction confusion:**
   ```csharp
   // ✅ CORRECT: SL/TP are distances FROM ENTRY, not from current price
   await svc.BuyLimitPoints(
       "EURUSD",
       0.01,
       priceOffsetPoints: 50,
       slPoints: 20,  // 20 points BELOW entry price
       tpPoints: 60); // 60 points ABOVE entry price

   // NOT from current Ask! They're relative to calculated entry.
   ```

4. **Using pips instead of points:**
   ```csharp
   // ❌ WRONG: Confusing pips with points
   await svc.BuyLimitPoints("EURUSD", 0.01, priceOffsetPoints: 5);
   // For 5-digit broker, this is 0.5 pips, not 5 pips!

   // ✅ CORRECT: Convert pips to points for 5-digit brokers
   double pips = 5;
   double points = pips * 10;  // 1 pip = 10 points on 5-digit
   await svc.BuyLimitPoints("EURUSD", 0.01, priceOffsetPoints: points);
   ```

5. **Offset too small (broker minimum distance):**
   ```csharp
   // ❌ May fail if offset violates broker's minimum stop level
   await svc.BuyLimitPoints("EURUSD", 0.01, priceOffsetPoints: 5);
   // Broker may require minimum 10 points distance

   // ✅ CORRECT: Check broker requirements
   // Most brokers require 10-50 points minimum for pending orders
   await svc.BuyLimitPoints("EURUSD", 0.01, priceOffsetPoints: 50);
   ```

---

## 💡 Summary

**BuyLimitPoints** simplifies Buy Limit order placement:

* ✅ Work in points, not absolute prices
* ✅ Auto-calculates entry price BELOW Ask
* ✅ SL/TP as point distances from entry
* ✅ Auto-normalizes prices to tick size
* ✅ Perfect for buy-on-dip strategies

```csharp
// Instead of manual calculation:
var tick = await svc.SymbolInfoTickAsync("EURUSD");
double price = tick.Ask - 50 * 0.00001;  // ❌ Manual, error-prone
await svc.PlacePending("EURUSD", 0.01, ENUM_ORDER_TYPE.OrderTypeBuyLimit, price, ...);

// Use one clean call:
await svc.BuyLimitPoints("EURUSD", 0.01, priceOffsetPoints: 50);  // ✅ Clean!
```

**Buy the dip with precision!** 🚀
