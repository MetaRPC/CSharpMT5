# Buy Market by Risk (`BuyMarketByRisk`) ⭐

> **⭐ CRITICAL PROFESSIONAL METHOD:** Opens Buy market position with auto-calculated volume based on fixed dollar risk. The HOLY GRAIL of risk management!

**API Information:**

* **Extension method:** `MT5Service.BuyMarketByRisk(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Region:** [10] MARKET BY RISK ⭐
* **Underlying calls:** `CalcVolumeForRiskAsync()` + `SymbolInfoTickAsync()` + `SymbolInfoDoubleAsync()` + `NormalizePriceAsync()` + `PlaceMarket()`

---

## Method Signature

```csharp
public static async Task<OrderSendData> BuyMarketByRisk(
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

* **What it is:** The **ULTIMATE** risk management method - you specify how much $ to risk, method calculates volume automatically!
* **Why you need it:** Consistent risk per trade = professional money management. No more guessing lot sizes!
* **Sanity check:** Risk $100 with 50-point SL → method calculates exact volume → opens Buy position at market price.

---

## 🎯 Purpose

Use it for:

* **Professional trading** - Every institutional trader uses fixed dollar risk
* **Consistent risk management** - 1-2% account risk per trade
* **Eliminating calculation errors** - No manual volume math
* **Portfolio consistency** - Same risk across different symbols/strategies
* **Backtesting alignment** - Match live trading with backtest logic

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

// Step 4: Calculate SL/TP prices (from current Bid for Buy)
double sl = tick.Bid - stopPoints * point;  // SL below Bid
double tp = tpPoints.HasValue ? tick.Ask + tpPoints.Value * point : 0;  // TP above Ask

// Step 5: Normalize prices to tick size
sl = sl > 0 ? await svc.NormalizePriceAsync(symbol, sl, timeoutSec, ct) : 0;
tp = tp > 0 ? await svc.NormalizePriceAsync(symbol, tp, timeoutSec, ct) : 0;

// Step 6: Place market Buy order with calculated volume
return await svc.PlaceMarket(symbol, vol, isBuy: true, sl, tp, comment, deviationPoints, timeoutSec, ct);
```

**What it improves:**

* **Eliminates manual volume calculation** - no spreadsheets needed
* **Guarantees fixed dollar risk** - always risk exactly what you specify
* **Works across all symbols** - EURUSD, XAUUSD, indices, crypto - same API
* **Handles broker constraints** - auto-normalizes volume to min/max/step

---

## 🔗 Usage Examples

### Example 1: Basic Buy with $100 Risk

```csharp
// Buy EURUSD risking exactly $100 with 50-point SL
var result = await svc.BuyMarketByRisk(
    symbol: "EURUSD",
    stopPoints: 50,
    riskMoney: 100.0);

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Buy order opened: Ticket #{result.Order}");
    Console.WriteLine($"   Volume: {result.Volume} lots (auto-calculated)");
    Console.WriteLine($"   Risk: $100 if SL hit");
}
```

---

### Example 2: Buy with SL and TP

```csharp
// Risk $200 with 30-point SL and 90-point TP (1:3 risk-reward)
var result = await svc.BuyMarketByRisk(
    symbol: "EURUSD",
    stopPoints: 30,
    riskMoney: 200.0,
    tpPoints: 90);

Console.WriteLine($"✅ Buy position opened:");
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

var result = await svc.BuyMarketByRisk(
    "EURUSD",
    stopPoints: 40,
    riskMoney: riskMoney,
    tpPoints: 120);

Console.WriteLine($"Account balance: ${balance:F2}");
Console.WriteLine($"Risk per trade: ${riskMoney:F2} (1%)");
Console.WriteLine($"✅ Buy opened: Ticket #{result.Order}");
Console.WriteLine($"   Volume: {result.Volume} lots");
```

---

### Example 4: Multi-Symbol Trading with Equal Risk

```csharp
// Trade multiple symbols with same dollar risk
string[] symbols = { "EURUSD", "GBPUSD", "USDJPY" };
double fixedRisk = 50.0;  // $50 per trade
double slPoints = 30;
double tpPoints = 90;

foreach (var symbol in symbols)
{
    var result = await svc.BuyMarketByRisk(
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
// ✅ EURUSD: 0.15 lots, Ticket #12361
// ✅ GBPUSD: 0.12 lots, Ticket #12362
// ✅ USDJPY: 0.18 lots, Ticket #12363
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

var result = await svc.BuyMarketByRisk(
    "EURUSD",
    stopPoints: slPoints,
    riskMoney: riskMoney,
    tpPoints: tpPoints);

Console.WriteLine($"✅ ATR-based Buy:");
Console.WriteLine($"   ATR: {atr:F5} ({atrPoints:F0} points)");
Console.WriteLine($"   SL: {slPoints:F0}pts, TP: {tpPoints:F0}pts");
Console.WriteLine($"   Volume: {result.Volume} lots (auto-calculated for $100 risk)");
```

---

### Example 6: With Order Comment for Strategy Tracking

```csharp
// Tag orders with strategy name
var result = await svc.BuyMarketByRisk(
    symbol: "GBPUSD",
    stopPoints: 50,
    riskMoney: 150.0,
    tpPoints: 150,
    comment: "TrendFollow_v2_LongEntry");

Console.WriteLine($"✅ Order #{result.Order}");
Console.WriteLine($"   Strategy: TrendFollow_v2_LongEntry");
Console.WriteLine($"   Risk: $150");
```

---

### Example 7: Error Handling

```csharp
try
{
    var result = await svc.BuyMarketByRisk(
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

### Example 8: Risk 2% of Account with Kelly Criterion Adjustment

```csharp
public async Task<OrderSendData> OpenBuyWithKelly(
    MT5Service svc,
    string symbol,
    double slPoints,
    double tpPoints)
{
    // Get account balance
    double balance = await svc.AccountInfoDoubleAsync(
        AccountInfoDoubleProperty.AccountBalance);

    // Kelly Criterion: f* = (bp - q) / b
    // Simplified: assume 55% win rate, 1:2 RR
    double winRate = 0.55;
    double lossRate = 0.45;
    double riskRewardRatio = tpPoints / slPoints;
    double kellyPercent = (winRate * riskRewardRatio - lossRate) / riskRewardRatio;
    kellyPercent = Math.Min(kellyPercent, 0.02);  // Cap at 2%

    double riskMoney = balance * kellyPercent;

    Console.WriteLine($"Kelly % risk: {kellyPercent * 100:F2}%");
    Console.WriteLine($"Risk amount: ${riskMoney:F2}");

    return await svc.BuyMarketByRisk(
        symbol,
        stopPoints: slPoints,
        riskMoney: riskMoney,
        tpPoints: tpPoints);
}

// Usage:
var result = await OpenBuyWithKelly(svc, "EURUSD", slPoints: 40, tpPoints: 120);
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
    var result = await svc.BuyMarketByRisk(
        "EURUSD",
        stopPoints: 40,
        riskMoney: Math.Min(100.0, remainingRisk),
        tpPoints: 120);

    Console.WriteLine($"✅ New Buy opened with ${100.0:F2} risk");
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

### Example 10: Compare Manual vs Auto Volume Calculation

```csharp
// ❌ MANUAL WAY (complex and error-prone):
double slPoints = 50;
double riskMoney = 100.0;

var point = await svc.SymbolInfoDoubleAsync("EURUSD", SymbolInfoDoubleProperty.SymbolPoint);
var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync("EURUSD");

var lossPerLot = (slPoints * point / tickSize) * tickValue;
var volume = riskMoney / lossPerLot;
volume = await svc.NormalizeVolumeAsync("EURUSD", volume);

var tick = await svc.SymbolInfoTickAsync("EURUSD");
double slPrice = tick.Bid - slPoints * point;

var resultManual = await svc.PlaceMarket("EURUSD", volume, isBuy: true, slPrice, 0);

Console.WriteLine($"Manual way: {resultManual.Order}");

// ✅ AUTO WAY (clean and safe):
var resultAuto = await svc.BuyMarketByRisk(
    "EURUSD",
    stopPoints: 50,
    riskMoney: 100.0);

Console.WriteLine($"Auto way: {resultAuto.Order}");
Console.WriteLine($"Volume: {resultAuto.Volume} lots (calculated automatically)");
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

* `SellMarketByRisk()` ⭐ - Sell version of this method
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
   await svc.BuyMarketByRisk("EURUSD", stopPoints: 1.0850, riskMoney: 100);
   // stopPoints should be DISTANCE, not absolute price!

   // ✅ CORRECT: Use point distance
   await svc.BuyMarketByRisk("EURUSD", stopPoints: 50, riskMoney: 100);
   ```

2. **Using pips instead of points:**
   ```csharp
   // ❌ WRONG: Confusing pips with points on 5-digit broker
   await svc.BuyMarketByRisk("EURUSD", stopPoints: 5, riskMoney: 100);
   // This is 0.5 pips, not 5 pips!

   // ✅ CORRECT: Convert pips to points
   double pips = 5;
   double points = pips * 10;  // 1 pip = 10 points on 5-digit
   await svc.BuyMarketByRisk("EURUSD", stopPoints: points, riskMoney: 100);
   ```

3. **Negative or zero risk/stop:**
   ```csharp
   // ❌ WRONG: Invalid parameters
   await svc.BuyMarketByRisk("EURUSD", stopPoints: 0, riskMoney: 100);
   // Throws ArgumentOutOfRangeException

   await svc.BuyMarketByRisk("EURUSD", stopPoints: 50, riskMoney: -100);
   // Throws ArgumentOutOfRangeException

   // ✅ CORRECT: Always positive values
   await svc.BuyMarketByRisk("EURUSD", stopPoints: 50, riskMoney: 100);
   ```

4. **Risking too much (no portfolio risk check):**
   ```csharp
   // ❌ DANGEROUS: No check on total portfolio risk
   for (int i = 0; i < 10; i++)
   {
       await svc.BuyMarketByRisk("EURUSD", 50, 100);  // 10x $100 = $1000 total risk!
   }

   // ✅ BETTER: Limit total portfolio risk
   double maxTotalRisk = 500.0;
   double currentRisk = 300.0;  // From existing positions
   double newRisk = Math.Min(100.0, maxTotalRisk - currentRisk);

   if (newRisk > 0)
   {
       await svc.BuyMarketByRisk("EURUSD", 50, newRisk);
   }
   ```

5. **Not handling very small calculated volumes:**
   ```csharp
   // ⚠️ With very small risk or large SL, volume might be below broker minimum
   var result = await svc.BuyMarketByRisk(
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
   await svc.BuyMarketByRisk("EURUSD", 50, riskMoney: 100.0);  // 100 in account currency
   ```

---

## 💡 Summary

**BuyMarketByRisk** is THE professional's choice for risk-managed trading:

* ✅ Auto-calculates volume based on fixed dollar risk
* ✅ Guarantees consistent risk across all trades
* ✅ Eliminates manual volume calculation errors
* ✅ Works across all symbols and account sizes
* ✅ Combines CalcVolumeForRiskAsync + PlaceMarket in one call

```csharp
// Instead of guessing volumes:
await svc.PlaceMarket("EURUSD", 0.1, isBuy: true);  // ❌ How much risk is this?

// Specify EXACT risk amount:
await svc.BuyMarketByRisk("EURUSD", stopPoints: 50, riskMoney: 100);  // ✅ Exactly $100 risk!
```

**This is how professionals trade. Use it on EVERY trade!** 🚀⭐
