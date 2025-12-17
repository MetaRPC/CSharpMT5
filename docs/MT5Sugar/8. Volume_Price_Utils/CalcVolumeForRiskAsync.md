# Calculate Volume by Risk (`CalcVolumeForRiskAsync`) ⭐

> **Sugar method:** **CORE RISK MANAGEMENT** - Calculates exact position size (lots) based on risk amount and stop-loss distance.

**API Information:**

* **Extension method:** `MT5Service.CalcVolumeForRiskAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Region:** [08] VOLUME & PRICE UTILITIES
* **Underlying calls:** `SymbolInfoDoubleAsync()` + `GetTickValueAndSizeAsync()` + `NormalizeVolumeAsync()`

---

## Method Signature

```csharp
public static async Task<double> CalcVolumeForRiskAsync(
    this MT5Service svc,
    string symbol,
    double stopPoints,
    double riskMoney,
    int timeoutSec = 10,
    CancellationToken ct = default)
```

---

## 🔽 Input

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance (extension method) |
| `symbol` | `string` | Symbol name (e.g., "EURUSD", "XAUUSD") |
| `stopPoints` | `double` | Stop-loss distance in points (must be > 0) |
| `riskMoney` | `double` | Maximum amount to risk in account currency (must be > 0) |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<double>` | **Normalized volume in lots** that matches the specified risk |

**Throws:**

- `ArgumentOutOfRangeException` - If stopPoints or riskMoney <= 0
- `InvalidOperationException` - If tick size invalid or calculation fails

---

## 💬 Just the essentials

* **What it is:** **THE MOST IMPORTANT METHOD** for professional trading - calculates exact lot size based on your risk tolerance.
* **Why you need it:** Instead of guessing volume (0.01? 0.1? 1.0?), this calculates EXACTLY how much to trade so you risk precisely $X if stop-loss hits.
* **Sanity check:** If you want to risk $100 with 50-point SL, this returns exact volume (e.g., 0.20 lots) that will lose exactly $100 at SL.

---

## 🎯 Purpose - Risk Management Foundation

**Problem:** Most traders fail because they use fixed lot sizes without considering risk.

```csharp
// ❌ WRONG - Amateur approach:
double volume = 0.01;  // Always 0.01 lots regardless of risk!

// ❌ WRONG - Guessing:
double volume = 0.05;  // Random guess, no risk calculation

// ✅ CORRECT - Professional risk management:
double volume = await svc.CalcVolumeForRiskAsync("EURUSD",
    stopPoints: 50,
    riskMoney: 100);
// Returns 0.20 lots - EXACTLY $100 risk at 50-point SL
```

**Use it for:**

* Fixed-risk trading (risk same $ amount per trade)
* Percentage-based risk (2% of account per trade)
* Position sizing for different stop-loss distances
* Consistent risk across different symbols
* Professional money management

---

## 🔧 Under the Hood - The Math

```csharp
// Step 1: Get symbol point size
var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolPoint);

// Step 2: Get tick value and tick size
var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync(symbol);

// Step 3: Calculate loss per 1 lot
var lossPerLot = (stopPoints * point / tickSize) * tickValue;
// Example for EURUSD: (50 * 0.00001 / 0.00001) * $1 = $50 per lot

// Step 4: Calculate required volume
var rawVolume = riskMoney / lossPerLot;
// Example: $100 / $50 = 2.0 lots

// Step 5: Normalize to broker constraints (min/max/step)
var normalizedVolume = await svc.NormalizeVolumeAsync(symbol, rawVolume);
// Example: 2.0 → 2.00 (if step is 0.01)

return normalizedVolume;
```

---

## 🔗 Usage Examples

### Example 1: Fixed Dollar Risk

```csharp
// Risk exactly $100 with 50-point stop-loss
double volume = await svc.CalcVolumeForRiskAsync(
    symbol: "EURUSD",
    stopPoints: 50,
    riskMoney: 100);

Console.WriteLine($"Trade {volume} lots to risk $100");
// Output: Trade 0.20 lots to risk $100

// Now place order with calculated volume
await svc.BuyMarket("EURUSD", volume, sl: currentBid - 50 * point);
```

---

### Example 2: Percentage-Based Risk (2% Rule)

```csharp
// Risk 2% of account balance
var balance = await svc.GetBalanceAsync();
double riskPercent = 2.0;
double riskMoney = balance * (riskPercent / 100.0);

// Calculate volume for 2% risk with 30-point SL
double volume = await svc.CalcVolumeForRiskAsync(
    symbol: "GBPUSD",
    stopPoints: 30,
    riskMoney: riskMoney);

Console.WriteLine($"Balance: ${balance:F2}");
Console.WriteLine($"Risk (2%): ${riskMoney:F2}");
Console.WriteLine($"Volume: {volume} lots");

// Place trade
await svc.SellMarket("GBPUSD", volume, sl: currentAsk + 30 * point);
```

---

### Example 3: Different Symbols, Same Risk

```csharp
// Risk same $50 on different symbols with different stop distances
var symbols = new[]
{
    ("EURUSD", 40),  // 40 points SL
    ("XAUUSD", 100), // 100 points SL (gold more volatile)
    ("USDJPY", 20)   // 20 points SL (tight stop)
};

foreach (var (symbol, stopPoints) in symbols)
{
    double volume = await svc.CalcVolumeForRiskAsync(
        symbol: symbol,
        stopPoints: stopPoints,
        riskMoney: 50);

    Console.WriteLine($"{symbol}: {volume} lots (SL: {stopPoints}pts, Risk: $50)");
}

// Output:
// EURUSD: 0.13 lots (SL: 40pts, Risk: $50)
// XAUUSD: 0.05 lots (SL: 100pts, Risk: $50)
// USDJPY: 0.25 lots (SL: 20pts, Risk: $50)
```

---

### Example 4: ATR-Based Stop with Fixed Risk

```csharp
// Use ATR for dynamic stop-loss, fixed risk
double atr = 0.0015;  // ATR value for EURUSD
double point = await svc.GetPointAsync("EURUSD");
double stopPoints = atr / point * 1.5;  // 1.5x ATR as SL

double volume = await svc.CalcVolumeForRiskAsync(
    symbol: "EURUSD",
    stopPoints: stopPoints,
    riskMoney: 75);

Console.WriteLine($"ATR-based SL: {stopPoints:F0} points");
Console.WriteLine($"Volume for $75 risk: {volume} lots");
```

---

### Example 5: Complete Trading Strategy

```csharp
public async Task<OrderSendData> OpenPositionWithRisk(
    MT5Service svc,
    string symbol,
    bool isBuy,
    double stopPoints,
    double? takeProfitPoints = null)
{
    // Fixed risk per trade
    const double RISK_MONEY = 100.0;

    // Step 1: Calculate volume based on risk
    double volume = await svc.CalcVolumeForRiskAsync(
        symbol,
        stopPoints,
        RISK_MONEY);

    Console.WriteLine($"Calculated volume: {volume} lots for ${RISK_MONEY} risk");

    // Step 2: Calculate SL/TP prices
    var tick = await svc.SymbolInfoTickAsync(symbol);
    double point = await svc.GetPointAsync(symbol);

    double sl = isBuy
        ? tick.Bid - stopPoints * point
        : tick.Ask + stopPoints * point;

    double? tp = null;
    if (takeProfitPoints.HasValue)
    {
        tp = isBuy
            ? tick.Ask + takeProfitPoints.Value * point
            : tick.Bid - takeProfitPoints.Value * point;
    }

    // Step 3: Place order with calculated volume
    var result = await svc.PlaceMarket(symbol, volume, isBuy, sl, tp);

    if (result.ReturnedCode == 10009)
    {
        Console.WriteLine($"✅ Order placed: #{result.Order}");
        Console.WriteLine($"   Volume: {volume} lots");
        Console.WriteLine($"   Risk: ${RISK_MONEY}");
    }

    return result;
}

// Usage:
await OpenPositionWithRisk(svc, "EURUSD", isBuy: true,
    stopPoints: 50, takeProfitPoints: 150);
```

---

### Example 6: Risk Calculation Before Trade

```csharp
// Pre-calculate risk for different scenarios
var scenarios = new[]
{
    (sl: 30, label: "Tight SL"),
    (sl: 50, label: "Normal SL"),
    (sl: 100, label: "Wide SL")
};

double riskMoney = 50.0;

Console.WriteLine($"Risk: ${riskMoney} on EURUSD");
Console.WriteLine("Scenario | SL (pts) | Volume");
Console.WriteLine("---------|----------|-------");

foreach (var (sl, label) in scenarios)
{
    double volume = await svc.CalcVolumeForRiskAsync(
        "EURUSD",
        stopPoints: sl,
        riskMoney: riskMoney);

    Console.WriteLine($"{label,-9}| {sl,8} | {volume:F2}");
}

// Output:
// Risk: $50 on EURUSD
// Scenario | SL (pts) | Volume
// ---------|----------|-------
// Tight SL |       30 | 0.17
// Normal SL|       50 | 0.10
// Wide SL  |      100 | 0.05
```

---

### Example 7: Account Size Aware Risk

```csharp
public async Task<double> CalculateSmartVolume(
    MT5Service svc,
    string symbol,
    double stopPoints)
{
    // Get account size
    var balance = await svc.GetBalanceAsync();

    // Risk rules based on account size
    double riskPercent;
    if (balance < 1000)
        riskPercent = 1.0;      // 1% for small accounts
    else if (balance < 10000)
        riskPercent = 1.5;      // 1.5% for medium accounts
    else
        riskPercent = 2.0;      // 2% for large accounts

    double riskMoney = balance * (riskPercent / 100.0);

    // Calculate volume
    double volume = await svc.CalcVolumeForRiskAsync(
        symbol,
        stopPoints,
        riskMoney);

    Console.WriteLine($"Account: ${balance:F2}");
    Console.WriteLine($"Risk: {riskPercent}% = ${riskMoney:F2}");
    Console.WriteLine($"Volume: {volume} lots");

    return volume;
}
```

---

### Example 8: Error Handling

```csharp
try
{
    double volume = await svc.CalcVolumeForRiskAsync(
        symbol: "EURUSD",
        stopPoints: 50,
        riskMoney: 100);

    Console.WriteLine($"✅ Calculated volume: {volume} lots");
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"❌ Invalid input: {ex.Message}");
    // stopPoints or riskMoney was <= 0
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"❌ Calculation error: {ex.Message}");
    // Tick size invalid or calculation failed
}
```

---

## 🔗 Related Methods

**📦 Used internally:**

* `SymbolInfoDoubleAsync()` - Gets point size
* `GetTickValueAndSizeAsync()` - Gets tick value/size for calculation
* `NormalizeVolumeAsync()` - Normalizes result to broker constraints

**🍬 Methods that use this:**

* `BuyMarketByRisk()` - Uses this + auto-places order
* `SellMarketByRisk()` - Uses this + auto-places order

**📊 Supporting methods:**

* `GetVolumeLimitsAsync()` - Get min/max/step for validation
* `NormalizeVolumeAsync()` - Round volume to valid step

---

## ⚠️ Common Pitfalls

1. **Using pips instead of points:**
   ```csharp
   // ❌ WRONG: Confusing pips with points
   double stopPips = 50;
   double volume = await svc.CalcVolumeForRiskAsync("EURUSD", stopPips, 100);
   // For 5-digit broker, 50 pips = 500 points!

   // ✅ CORRECT: Convert pips to points
   double stopPips = 50;
   double stopPoints = stopPips * 10;  // 5-digit: 1 pip = 10 points
   double volume = await svc.CalcVolumeForRiskAsync("EURUSD", stopPoints, 100);
   ```

2. **Negative or zero risk:**
   ```csharp
   // ❌ WRONG: Invalid risk
   double volume = await svc.CalcVolumeForRiskAsync("EURUSD", 50, 0);
   // Throws ArgumentOutOfRangeException

   // ✅ CORRECT: Positive risk
   double volume = await svc.CalcVolumeForRiskAsync("EURUSD", 50, 100);
   ```

3. **Not considering broker limits:**
   ```csharp
   // ⚠️ Calculated volume might exceed broker maximum!
   double volume = await svc.CalcVolumeForRiskAsync("EURUSD", 10, 1000);
   // Might return 10.0 lots, but broker max could be 5.0

   // ✅ BETTER: Check limits
   var (min, max, step) = await svc.GetVolumeLimitsAsync("EURUSD");
   double volume = await svc.CalcVolumeForRiskAsync("EURUSD", 10, 1000);
   if (volume > max)
   {
       Console.WriteLine($"⚠️ Calculated {volume} exceeds max {max}");
       volume = max;
   }
   ```

---

## 💡 Summary

**CalcVolumeForRiskAsync** is THE foundation of professional trading:

* ⭐ **CORE METHOD** for risk management
* ✅ Calculates exact volume to risk specified $ amount
* ✅ Works with any symbol, any stop distance
* ✅ Auto-normalizes to broker constraints
* ✅ Enables percentage-based risk strategies

```csharp
// Professional risk management in one line:
double volume = await svc.CalcVolumeForRiskAsync("EURUSD",
    stopPoints: 50,
    riskMoney: 100);

// Instead of guessing:
double volume = 0.01;  // ❌ Amateur
```

**This is HOW professionals trade!** 🚀⭐
