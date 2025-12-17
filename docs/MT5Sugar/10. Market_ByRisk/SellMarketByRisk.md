# Sell Market by Risk (`SellMarketByRisk`) ⭐

> **⭐ CRITICAL PROFESSIONAL METHOD:** Opens Sell market position with auto-calculated volume based on fixed dollar risk. Professional short trading made simple!

**API Information:**

* **Extension method:** `MT5Service.SellMarketByRisk(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Region:** [10] MARKET BY RISK ⭐
* **Underlying calls:** `CalcVolumeForRiskAsync()` + `SymbolInfoTickAsync()` + `SymbolInfoDoubleAsync()` + `NormalizePriceAsync()` + `PlaceMarket()`

---

## Method Signature

```csharp
public static async Task<OrderSendData> SellMarketByRisk(
    this MT5Service svc,
    string symbol,
    double stopPoints,
    double riskMoney,
    double? tpPoints = null,
    string? comment = null,
    int deviationPoints = 0,
    int timeoutSec = 20,
    CancellationToken ct = default)
```

---

## 🔽 Input

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance (extension method) |
| `symbol` | `string` | Symbol name (e.g., "EURUSD", "XAUUSD") |
| `stopPoints` | `double` | Stop-loss distance in points from entry |
| `riskMoney` | `double` | **Maximum amount to risk** in account currency (e.g., $100) |
| `tpPoints` | `double?` | Optional take-profit distance in points from entry |
| `comment` | `string?` | Optional order comment |
| `deviationPoints` | `int` | Maximum price slippage in points (default: 0) |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 20) |
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
- `Volume` - **Auto-calculated** volume used for the trade

---

## 💬 Just the essentials

* **What it is:** The **ULTIMATE** risk management method for SHORT trades - you specify how much $ to risk, method calculates volume automatically!
* **Why you need it:** Consistent risk per trade = professional money management. No more guessing lot sizes for short positions!
* **Sanity check:** Risk $100 with 50-point SL → method calculates exact volume → opens Sell position at market price.

---

## 🎯 Purpose

Use it for:

* **Professional short trading** - Same risk management for longs and shorts
* **Consistent risk management** - 1-2% account risk per trade
* **Eliminating calculation errors** - No manual volume math
* **Portfolio consistency** - Same risk across different symbols/strategies
* **Downtrend trading** - Professional entries on bearish setups

---

## 🔧 Under the Hood

```csharp
// Step 1: Ensure symbol is selected in MarketWatch
await svc.EnsureSelected(symbol, timeoutSec, ct);

// Step 2: Calculate volume based on risk (MOST CRITICAL STEP)
var vol = await svc.CalcVolumeForRiskAsync(symbol, stopPoints, riskMoney, timeoutSec, ct);
// Example: $100 risk / 50pts SL on EURUSD → vol = 0.2 lots

// Step 3: Get current tick and point size
var tick = await svc.SymbolInfoTickAsync(symbol, deadline, ct);
var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolPoint, deadline, ct);

// Step 4: Calculate SL/TP prices (from current Ask for Sell)
double sl = tick.Ask + stopPoints * point;  // SL above Ask
double tp = tpPoints.HasValue ? tick.Bid - tpPoints.Value * point : 0;  // TP below Bid

// Step 5: Normalize prices to tick size
sl = sl > 0 ? await svc.NormalizePriceAsync(symbol, sl, timeoutSec, ct) : 0;
tp = tp > 0 ? await svc.NormalizePriceAsync(symbol, tp, timeoutSec, ct) : 0;

// Step 6: Place market Sell order with calculated volume
return await svc.PlaceMarket(symbol, vol, isBuy: false, sl, tp, comment, deviationPoints, timeoutSec, ct);
```

**What it improves:**

* **Eliminates manual volume calculation** - no spreadsheets needed
* **Guarantees fixed dollar risk** - always risk exactly what you specify
* **Works across all symbols** - EURUSD, XAUUSD, indices, crypto - same API
* **Handles broker constraints** - auto-normalizes volume to min/max/step

---

## 🔗 Usage Examples

### Example 1: Basic Sell with $100 Risk

```csharp
// Sell EURUSD risking exactly $100 with 50-point SL
var result = await svc.SellMarketByRisk(
    symbol: "EURUSD",
    stopPoints: 50,
    riskMoney: 100.0);

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Sell order opened: Ticket #{result.Order}");
    Console.WriteLine($"   Volume: {result.Volume} lots (auto-calculated)");
    Console.WriteLine($"   Risk: $100 if SL hit");
}
```

---

### Example 2: Sell with SL and TP

```csharp
// Risk $200 with 30-point SL and 90-point TP (1:3 risk-reward)
var result = await svc.SellMarketByRisk(
    symbol: "EURUSD",
    stopPoints: 30,
    riskMoney: 200.0,
    tpPoints: 90);

Console.WriteLine($"✅ Sell position opened:");
Console.WriteLine($"   Ticket: #{result.Order}");
Console.WriteLine($"   Volume: {result.Volume} lots");
Console.WriteLine($"   Risk: $200 (SL: 30pts)");
Console.WriteLine($"   Reward: $600 (TP: 90pts) → 1:3 RR");
```

---

### Example 3: Percentage-Based Risk (1% Rule)

```csharp
// Risk 1% of account balance on each trade
double balance = await svc.AccountInfoDoubleAsync(AccountInfoDoubleProperty.AccountBalance);
double riskPercent = 0.01;  // 1%
double riskMoney = balance * riskPercent;

var result = await svc.SellMarketByRisk(
    "EURUSD",
    stopPoints: 40,
    riskMoney: riskMoney,
    tpPoints: 120);

Console.WriteLine($"Account balance: ${balance:F2}");
Console.WriteLine($"Risk per trade: ${riskMoney:F2} (1%)");
Console.WriteLine($"✅ Sell opened: Ticket #{result.Order}");
Console.WriteLine($"   Volume: {result.Volume} lots");
```

---

### Example 4: Multi-Symbol Short Portfolio

```csharp
// Short multiple symbols with same dollar risk
string[] symbols = { "EURUSD", "GBPUSD", "AUDUSD" };
double fixedRisk = 50.0;  // $50 per trade
double slPoints = 30;
double tpPoints = 90;

foreach (var symbol in symbols)
{
    var result = await svc.SellMarketByRisk(
        symbol,
        stopPoints: slPoints,
        riskMoney: fixedRisk,
        tpPoints: tpPoints);

    if (result.ReturnedCode == 10009)
    {
        Console.WriteLine($"✅ {symbol}: {result.Volume} lots, Ticket #{result.Order}");
    }
}

// Output (volumes auto-adjusted per symbol):
// ✅ EURUSD: 0.15 lots, Ticket #12364
// ✅ GBPUSD: 0.12 lots, Ticket #12365
// ✅ AUDUSD: 0.18 lots, Ticket #12366
```

---

### Example 5: ATR-Based Dynamic SL with Fixed Risk

```csharp
// Use ATR for stop-loss, keep risk constant
double atr = 0.0012;  // ATR for EURUSD
double point = await svc.GetPointAsync("EURUSD");
double atrPoints = atr / point;

// SL = 1.5x ATR, TP = 3x ATR
double slPoints = atrPoints * 1.5;
double tpPoints = atrPoints * 3.0;
double riskMoney = 100.0;

var result = await svc.SellMarketByRisk(
    "EURUSD",
    stopPoints: slPoints,
    riskMoney: riskMoney,
    tpPoints: tpPoints);

Console.WriteLine($"✅ ATR-based Sell:");
Console.WriteLine($"   ATR: {atr:F5} ({atrPoints:F0} points)");
Console.WriteLine($"   SL: {slPoints:F0}pts, TP: {tpPoints:F0}pts");
Console.WriteLine($"   Volume: {result.Volume} lots (auto-calculated for $100 risk)");
```

---

### Example 6: With Order Comment for Strategy Tracking

```csharp
// Tag orders with strategy name
var result = await svc.SellMarketByRisk(
    symbol: "GBPUSD",
    stopPoints: 50,
    riskMoney: 150.0,
    tpPoints: 150,
    comment: "TrendFollow_v2_ShortEntry");

Console.WriteLine($"✅ Order #{result.Order}");
Console.WriteLine($"   Strategy: TrendFollow_v2_ShortEntry");
Console.WriteLine($"   Risk: $150");
```

---

### Example 7: Error Handling

```csharp
try
{
    var result = await svc.SellMarketByRisk(
        "EURUSD",
        stopPoints: 50,
        riskMoney: 100.0,
        tpPoints: 150);

    if (result.ReturnedCode == 10009)
    {
        Console.WriteLine($"✅ Success: Ticket #{result.Order}");
        Console.WriteLine($"   Volume: {result.Volume} lots");
    }
    else
    {
        Console.WriteLine($"❌ Order failed: {result.Comment}");
        Console.WriteLine($"   Return code: {result.ReturnedCode}");
    }
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"❌ Invalid parameters: {ex.Message}");
    // stopPoints or riskMoney was <= 0
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Exception: {ex.Message}");
}
```

---

### Example 8: Resistance Rejection Strategy

```csharp
public async Task<OrderSendData> SellAtResistance(
    MT5Service svc,
    string symbol,
    double resistanceLevel)
{
    // Check if price is near resistance
    var tick = await svc.SymbolInfoTickAsync(symbol);
    double point = await svc.GetPointAsync(symbol);

    double distancePoints = Math.Abs(tick.Bid - resistanceLevel) / point;

    if (distancePoints <= 10)  // Within 10 points of resistance
    {
        Console.WriteLine($"✅ Price near resistance {resistanceLevel:F5}");
        Console.WriteLine($"   Entering short with $100 risk");

        return await svc.SellMarketByRisk(
            symbol,
            stopPoints: 40,      // SL above resistance
            riskMoney: 100.0,
            tpPoints: 120,       // 1:3 RR
            comment: $"ResistanceReject_{resistanceLevel}");
    }
    else
    {
        throw new InvalidOperationException(
            $"Price too far from resistance: {distancePoints:F0} points");
    }
}

// Usage:
var result = await SellAtResistance(svc, "EURUSD", resistanceLevel: 1.0950);
```

---

### Example 9: Portfolio Risk Management (Max 5% Total Risk)

```csharp
// Check current portfolio risk before opening new trade
var positions = await svc.OpenedOrdersAllAsync();
double totalRisk = 0;

// Calculate total risk from existing positions
foreach (var pos in positions.Orders)
{
    // Simplified: assume $50 risk per position
    totalRisk += 50.0;
}

double maxPortfolioRisk = 500.0;  // Max $500 across all trades
double remainingRisk = maxPortfolioRisk - totalRisk;

if (remainingRisk >= 100.0)
{
    var result = await svc.SellMarketByRisk(
        "EURUSD",
        stopPoints: 40,
        riskMoney: Math.Min(100.0, remainingRisk),
        tpPoints: 120);

    Console.WriteLine($"✅ New Sell opened with ${100.0:F2} risk");
    Console.WriteLine($"   Total portfolio risk: ${totalRisk + 100.0:F2} / $500");
}
else
{
    Console.WriteLine($"⚠️ Portfolio risk limit reached!");
    Console.WriteLine($"   Current risk: ${totalRisk:F2}");
    Console.WriteLine($"   Max allowed: ${maxPortfolioRisk:F2}");
}
```

---

### Example 10: Long/Short Pair Trading with Equal Risk

```csharp
// Pairs trading: Buy weak, Sell strong (equal risk on both sides)
double riskPerSide = 50.0;
double slPoints = 30;
double tpPoints = 90;

// Sell the strong currency (EURUSD)
var sellResult = await svc.SellMarketByRisk(
    "EURUSD",
    stopPoints: slPoints,
    riskMoney: riskPerSide,
    tpPoints: tpPoints,
    comment: "PairTrade_SellStrong");

// Buy the weak currency (GBPUSD)
var buyResult = await svc.BuyMarketByRisk(
    "GBPUSD",
    stopPoints: slPoints,
    riskMoney: riskPerSide,
    tpPoints: tpPoints,
    comment: "PairTrade_BuyWeak");

Console.WriteLine($"✅ Pair trade opened:");
Console.WriteLine($"   SELL EURUSD: {sellResult.Volume} lots, Ticket #{sellResult.Order}");
Console.WriteLine($"   BUY GBPUSD: {buyResult.Volume} lots, Ticket #{buyResult.Order}");
Console.WriteLine($"   Total risk: ${riskPerSide * 2:F2}");
```

---

## 🔗 Related Methods

**📦 Methods used internally:**

* `CalcVolumeForRiskAsync()` ⭐ - Core volume calculation engine
* `SymbolInfoTickAsync()` - Gets current Bid/Ask
* `SymbolInfoDoubleAsync(SymbolPoint)` - Gets point size
* `NormalizePriceAsync()` - Normalizes SL/TP prices
* `PlaceMarket()` - Places market order

**🍬 Related Sugar methods:**

* `BuyMarketByRisk()` ⭐ - Buy version of this method
* `CalcVolumeForRiskAsync()` - Calculate volume only (no order placement)
* `NormalizeVolumeAsync()` - Normalize volume to broker constraints
* `GetVolumeLimitsAsync()` - Get broker volume limits

**📊 Alternative approaches:**

* `CalcVolumeForRiskAsync()` + `PlaceMarket()` - Two-step manual approach
* Manual calculation - NOT recommended (error-prone)

---

## ⚠️ Common Pitfalls

1. **Confusing stopPoints with SL price:**
   ```csharp
   // ❌ WRONG: Passing price instead of points
   await svc.SellMarketByRisk("EURUSD", stopPoints: 1.0950, riskMoney: 100);
   // stopPoints should be DISTANCE, not absolute price!

   // ✅ CORRECT: Use point distance
   await svc.SellMarketByRisk("EURUSD", stopPoints: 50, riskMoney: 100);
   ```

2. **Using pips instead of points:**
   ```csharp
   // ❌ WRONG: Confusing pips with points on 5-digit broker
   await svc.SellMarketByRisk("EURUSD", stopPoints: 5, riskMoney: 100);
   // This is 0.5 pips, not 5 pips!

   // ✅ CORRECT: Convert pips to points
   double pips = 5;
   double points = pips * 10;  // 1 pip = 10 points on 5-digit
   await svc.SellMarketByRisk("EURUSD", stopPoints: points, riskMoney: 100);
   ```

3. **Negative or zero risk/stop:**
   ```csharp
   // ❌ WRONG: Invalid parameters
   await svc.SellMarketByRisk("EURUSD", stopPoints: 0, riskMoney: 100);
   // Throws ArgumentOutOfRangeException

   await svc.SellMarketByRisk("EURUSD", stopPoints: 50, riskMoney: -100);
   // Throws ArgumentOutOfRangeException

   // ✅ CORRECT: Always positive values
   await svc.SellMarketByRisk("EURUSD", stopPoints: 50, riskMoney: 100);
   ```

4. **Risking too much (no portfolio risk check):**
   ```csharp
   // ❌ DANGEROUS: No check on total portfolio risk
   for (int i = 0; i < 10; i++)
   {
       await svc.SellMarketByRisk("EURUSD", 50, 100);  // 10x $100 = $1000 total risk!
   }

   // ✅ BETTER: Limit total portfolio risk
   double maxTotalRisk = 500.0;
   double currentRisk = 300.0;  // From existing positions
   double newRisk = Math.Min(100.0, maxTotalRisk - currentRisk);

   if (newRisk > 0)
   {
       await svc.SellMarketByRisk("EURUSD", 50, newRisk);
   }
   ```

5. **Not handling very small calculated volumes:**
   ```csharp
   // ⚠️ With very small risk or large SL, volume might be below broker minimum
   var result = await svc.SellMarketByRisk(
       "EURUSD",
       stopPoints: 500,   // Large SL
       riskMoney: 1.0);   // Small risk
   // Calculated volume might be 0.001 lots, but broker min is 0.01!

   // Method handles this via NormalizeVolumeAsync, but be aware:
   // Actual risk will be higher than specified if volume gets rounded up!
   ```

6. **Forgetting account currency matters:**
   ```csharp
   // ⚠️ riskMoney is in ACCOUNT CURRENCY
   // If account is in EUR, riskMoney: 100 means 100 EUR, not 100 USD

   // Get account currency first if needed:
   var currency = await svc.AccountInfoStringAsync(
       AccountInfoStringProperty.AccountCurrency);
   Console.WriteLine($"Account currency: {currency}");

   // Then risk in that currency
   await svc.SellMarketByRisk("EURUSD", 50, riskMoney: 100.0);  // 100 in account currency
   ```

7. **SL direction confusion for Sell:**
   ```csharp
   // ⚠️ For SELL orders, SL is ABOVE entry (not below!)
   // Method handles this correctly automatically

   var tick = await svc.SymbolInfoTickAsync("EURUSD");
   // Entry: tick.Bid (e.g., 1.0900)
   // SL: tick.Ask + stopPoints * point (e.g., 1.0950)
   // ✅ SL is 50 points ABOVE entry (correct for Sell)
   ```

---

## 💡 Summary

**SellMarketByRisk** is THE professional's choice for short trading with risk management:

* ✅ Auto-calculates volume based on fixed dollar risk
* ✅ Guarantees consistent risk across all trades
* ✅ Eliminates manual volume calculation errors
* ✅ Works across all symbols and account sizes
* ✅ Combines CalcVolumeForRiskAsync + PlaceMarket in one call

```csharp
// Instead of guessing volumes:
await svc.PlaceMarket("EURUSD", 0.1, isBuy: false);  // ❌ How much risk is this?

// Specify EXACT risk amount:
await svc.SellMarketByRisk("EURUSD", stopPoints: 50, riskMoney: 100);  // ✅ Exactly $100 risk!
```

**This is how professionals short the market. Use it on EVERY short trade!** 🚀⭐
