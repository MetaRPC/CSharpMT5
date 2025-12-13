# Get Tick Value and Size (`GetTickValueAndSizeAsync`)

> **Sugar method:** Retrieves tick value (monetary value per tick) and tick size (price increment) for risk/P&L calculations.

**API Information:**

* **Extension method:** `MT5Service.GetTickValueAndSizeAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Region:** [08] VOLUME & PRICE UTILITIES
* **Underlying calls:** `SymbolInfoDoubleAsync()` (2 calls)

---

## Method Signature

```csharp
public static async Task<(double tickValue, double tickSize)> GetTickValueAndSizeAsync(
    this MT5Service svc,
    string symbol,
    int timeoutSec = 10,
    CancellationToken ct = default)
```

---

## 🔽 Input

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance (extension method) |
| `symbol` | `string` | Symbol name (e.g., "EURUSD", "XAUUSD") |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<(double tickValue, double tickSize)>` | Tuple with tick value and tick size |

### Tuple Fields

- **tickValue** (double) - Monetary value of one tick for 1 lot (in account currency)
- **tickSize** (double) - Minimum price increment (e.g., 0.00001 for EURUSD)

---

## 💬 Just the essentials

* **What it is:** Gets tick value (how much $ moves per tick) and tick size (minimum price step).
* **Why you need it:** Essential for accurate P/L calculations and risk management formulas.
* **Sanity check:** Tick value = $ per tick for 1 lot. Tick size = minimum price increment (same as Point for most symbols).

---

## 🎯 Purpose

Use it for:

* **Risk calculations** - Core input for `CalcVolumeForRiskAsync`
* **P/L estimation** - Calculate potential profit/loss
* **Pip value calculations** - Convert pips to money
* **Stop-loss sizing** - Calculate SL distance in $ terms
* **Trading education** - Understand symbol specifications

---

## 🔧 Under the Hood

```csharp
var deadline = DateTime.UtcNow.AddSeconds(timeoutSec);

// Fetch tick value (monetary value per tick for 1 lot)
var tickValue = await svc.SymbolInfoDoubleAsync(symbol,
    SymbolInfoDoubleProperty.SymbolTradeTickValue, deadline, ct);

// Fetch tick size (minimum price increment)
var tickSize = await svc.SymbolInfoDoubleAsync(symbol,
    SymbolInfoDoubleProperty.SymbolTradeTickSize, deadline, ct);

return (tickValue, tickSize);
```

**What it improves:**

* **One call** - returns both values as tuple
* **Shared deadline** - both calls use same timeout
* **Tuple destructuring** - clean code
* **Type-safe** - no magic numbers

---

## 🔗 Usage Examples

### Example 1: Basic Tick Info

```csharp
var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync("EURUSD");

Console.WriteLine($"EURUSD Tick Information:");
Console.WriteLine($"  Tick Value: ${tickValue}");
Console.WriteLine($"  Tick Size:  {tickSize}");

// Output:
// EURUSD Tick Information:
//   Tick Value: $1.00
//   Tick Size:  0.00001
```

---

### Example 2: Calculate P/L for Price Movement

```csharp
var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync("EURUSD");

double volume = 0.10;  // 0.10 lots
double entryPrice = 1.09000;
double currentPrice = 1.09050;

// Calculate price movement in ticks
double priceDiff = currentPrice - entryPrice;
double ticks = priceDiff / tickSize;

// Calculate P/L
double profitLoss = ticks * tickValue * volume;

Console.WriteLine($"Entry:       {entryPrice:F5}");
Console.WriteLine($"Current:     {currentPrice:F5}");
Console.WriteLine($"Difference:  {priceDiff:F5} ({ticks} ticks)");
Console.WriteLine($"P/L:         ${profitLoss:F2}");

// Output:
// Entry:       1.09000
// Current:     1.09050
// Difference:  0.00050 (50 ticks)
// P/L:         $5.00
```

---

### Example 3: Calculate Pip Value

```csharp
var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync("EURUSD");

// For 5-digit broker: 1 pip = 10 points = 10 ticks
double pipSizeInTicks = 10;  // EURUSD: 1 pip = 0.0001 = 10 ticks
double pipValue = tickValue * pipSizeInTicks;

Console.WriteLine($"EURUSD:");
Console.WriteLine($"  Tick value:  ${tickValue} per tick");
Console.WriteLine($"  Pip value:   ${pipValue} per pip (for 1 lot)");

// For different volumes:
double[] volumes = { 0.01, 0.10, 1.00 };
Console.WriteLine($"\nPip value for different volumes:");
foreach (var volume in volumes)
{
    double pipVal = pipValue * volume;
    Console.WriteLine($"  {volume,4} lots: ${pipVal,6:F2} per pip");
}

// Output:
// EURUSD:
//   Tick value:  $1.00 per tick
//   Pip value:   $10.00 per pip (for 1 lot)
//
// Pip value for different volumes:
//   0.01 lots: $  0.10 per pip
//   0.10 lots: $  1.00 per pip
//   1.00 lots: $ 10.00 per pip
```

---

### Example 4: Compare Different Symbols

```csharp
string[] symbols = { "EURUSD", "GBPUSD", "XAUUSD", "USDJPY" };

Console.WriteLine("Symbol   | Tick Value | Tick Size");
Console.WriteLine("---------|------------|----------");

foreach (var symbol in symbols)
{
    var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync(symbol);
    Console.WriteLine($"{symbol,-8} | ${tickValue,9:F2} | {tickSize,8:F5}");
}

// Output:
// Symbol   | Tick Value | Tick Size
// ---------|------------|----------
// EURUSD   |      $1.00 |  0.00001
// GBPUSD   |      $1.00 |  0.00001
// XAUUSD   |      $0.01 |  0.01000
// USDJPY   |      $0.01 |  0.00100
```

---

### Example 5: Calculate Stop-Loss Cost

```csharp
var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync("EURUSD");

double volume = 0.10;
double stopLossPoints = 50;  // 50 points SL

// Calculate number of ticks
double ticks = stopLossPoints;  // For EURUSD, 1 point = 1 tick

// Calculate cost if SL hits
double slCost = ticks * tickValue * volume;

Console.WriteLine($"Stop-Loss Analysis:");
Console.WriteLine($"  Symbol:     EURUSD");
Console.WriteLine($"  Volume:     {volume} lots");
Console.WriteLine($"  SL Points:  {stopLossPoints}");
Console.WriteLine($"  SL Cost:    ${slCost:F2}");

// Output:
// Stop-Loss Analysis:
//   Symbol:     EURUSD
//   Volume:     0.10 lots
//   SL Points:  50
//   SL Cost:    $5.00
```

---

### Example 6: Risk Calculation (Manual)

```csharp
// Manual implementation of risk-based volume calculation
var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync("EURUSD");

double riskMoney = 100.0;      // Want to risk $100
double stopLossPoints = 50;    // 50 points SL
double point = await svc.GetPointAsync("EURUSD");

// Calculate loss per lot
double lossPerLot = (stopLossPoints * point / tickSize) * tickValue;

// Calculate required volume
double volume = riskMoney / lossPerLot;

Console.WriteLine($"Risk Calculation:");
Console.WriteLine($"  Risk:       ${riskMoney}");
Console.WriteLine($"  SL Points:  {stopLossPoints}");
Console.WriteLine($"  Loss/lot:   ${lossPerLot:F2}");
Console.WriteLine($"  Volume:     {volume:F2} lots");

// Verify: If we trade 'volume' lots with 50-point SL, we risk exactly $100
double actualRisk = lossPerLot * volume;
Console.WriteLine($"  Actual risk: ${actualRisk:F2}");
```

---

### Example 7: Profit Target Calculator

```csharp
public async Task CalculateProfitTarget(
    MT5Service svc,
    string symbol,
    double volume,
    double targetMoney)
{
    var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync(symbol);

    // How many ticks needed to reach target profit?
    double ticksNeeded = targetMoney / (tickValue * volume);

    // Convert to price distance
    double priceDistance = ticksNeeded * tickSize;

    // Convert to pips (for display)
    double point = await svc.GetPointAsync(symbol);
    double pipsNeeded = priceDistance / point;

    Console.WriteLine($"Profit Target Calculator:");
    Console.WriteLine($"  Symbol:        {symbol}");
    Console.WriteLine($"  Volume:        {volume} lots");
    Console.WriteLine($"  Target Profit: ${targetMoney}");
    Console.WriteLine($"  Required:");
    Console.WriteLine($"    {ticksNeeded:F0} ticks");
    Console.WriteLine($"    {priceDistance:F5} price distance");
    Console.WriteLine($"    {pipsNeeded:F1} points");
}

// Usage:
await CalculateProfitTarget(svc, "EURUSD", volume: 0.10, targetMoney: 50.0);

// Output:
// Profit Target Calculator:
//   Symbol:        EURUSD
//   Volume:        0.10 lots
//   Target Profit: $50
//   Required:
//     500 ticks
//     0.00500 price distance
//     500.0 points
```

---

### Example 8: Position P/L Monitor

```csharp
public async Task MonitorPositionPL(
    MT5Service svc,
    ulong ticket)
{
    var opened = await svc.OpenedOrdersAsync(...);
    var position = opened.PositionInfos.First(p => p.Ticket == ticket);

    var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync(position.PositionSymbol);

    // Get current price
    var tick = await svc.SymbolInfoTickAsync(position.PositionSymbol);
    double currentPrice = position.Type == 0 ? tick.Bid : tick.Ask;  // BUY uses Bid, SELL uses Ask

    // Calculate P/L
    double priceDiff = currentPrice - position.PriceOpen;
    if (position.Type == 1) priceDiff = -priceDiff;  // Reverse for SELL

    double ticks = priceDiff / tickSize;
    double profitLoss = ticks * tickValue * position.Volume;

    Console.WriteLine($"Position #{ticket}:");
    Console.WriteLine($"  Symbol:    {position.PositionSymbol}");
    Console.WriteLine($"  Type:      {(position.Type == 0 ? "BUY" : "SELL")}");
    Console.WriteLine($"  Volume:    {position.Volume} lots");
    Console.WriteLine($"  Open:      {position.PriceOpen:F5}");
    Console.WriteLine($"  Current:   {currentPrice:F5}");
    Console.WriteLine($"  P/L:       ${profitLoss:F2} ({ticks:F0} ticks)");
}
```

---

### Example 9: Educational Display

```csharp
public async Task DisplayTickInfo(MT5Service svc, string symbol)
{
    var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync(symbol);
    double point = await svc.GetPointAsync(symbol);

    Console.WriteLine($"╔══════════════════════════════════════════╗");
    Console.WriteLine($"║  {symbol,-38} ║");
    Console.WriteLine($"╚══════════════════════════════════════════╝");
    Console.WriteLine($"");
    Console.WriteLine($"Tick Size:  {tickSize:F5}");
    Console.WriteLine($"Tick Value: ${tickValue:F2} per tick (for 1 lot)");
    Console.WriteLine($"Point Size: {point:F5}");
    Console.WriteLine($"");
    Console.WriteLine($"What this means:");
    Console.WriteLine($"  • Each {tickSize:F5} price movement = 1 tick");
    Console.WriteLine($"  • Each tick = ${tickValue:F2} for 1 lot");
    Console.WriteLine($"  • 10 ticks = ${tickValue * 10:F2} for 1 lot");
    Console.WriteLine($"  • 100 ticks = ${tickValue * 100:F2} for 1 lot");
    Console.WriteLine($"");
    Console.WriteLine($"Example: 0.10 lots trading");
    Console.WriteLine($"  • 1 tick = ${tickValue * 0.10:F2}");
    Console.WriteLine($"  • 50 ticks = ${tickValue * 50 * 0.10:F2}");
    Console.WriteLine($"  • 100 ticks = ${tickValue * 100 * 0.10:F2}");
}

// Usage:
await DisplayTickInfo(svc, "EURUSD");
```

---

### Example 10: Validate Tick Data

```csharp
var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync("EURUSD");

// Validation
if (tickValue <= 0)
{
    Console.WriteLine($"❌ Invalid tick value: {tickValue}");
}
else if (tickSize <= 0)
{
    Console.WriteLine($"❌ Invalid tick size: {tickSize}");
}
else
{
    Console.WriteLine($"✅ Tick data valid:");
    Console.WriteLine($"   Tick Value: ${tickValue}");
    Console.WriteLine($"   Tick Size:  {tickSize}");
}
```

---

## 🔗 Related Methods

**📦 Low-level methods used internally:**

* `SymbolInfoDoubleAsync(SymbolTradeTickValue)` - Tick value
* `SymbolInfoDoubleAsync(SymbolTradeTickSize)` - Tick size

**🍬 Methods that use this:**

* `CalcVolumeForRiskAsync()` - Uses tick value/size for volume calculation
* `BuyMarketByRisk()` - Indirectly uses via CalcVolumeForRiskAsync
* `SellMarketByRisk()` - Indirectly uses via CalcVolumeForRiskAsync

**📊 Related methods:**

* `GetPointAsync()` - Gets point size (often same as tick size)
* `GetDigitsAsync()` - Gets decimal digits for price display

---

## ⚠️ Common Pitfalls

1. **Confusing tick size with pip size:**
   ```csharp
   // For 5-digit EURUSD:
   // Tick size = 0.00001 (1 point)
   // Pip size  = 0.0001  (10 points)
   // They are NOT the same!

   var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync("EURUSD");
   double pipSize = tickSize * 10;  // 1 pip = 10 ticks for 5-digit
   ```

2. **Not accounting for volume in P/L:**
   ```csharp
   // ❌ WRONG: Forgetting volume multiplier
   double profitPerTick = tickValue;  // Only for 1 lot!

   // ✅ CORRECT: Include volume
   double volume = 0.10;
   double profitPerTick = tickValue * volume;
   ```

3. **Different symbols have different tick values:**
   ```csharp
   // EURUSD: $1.00 per tick
   // XAUUSD: $0.01 per tick
   // USDJPY: $0.01 per tick
   // Always query per symbol!
   ```

---

## 💡 Summary

**GetTickValueAndSizeAsync** provides essential pricing information:

* ✅ One call returns both tick value and size
* ✅ Essential for P/L calculations
* ✅ Core input for risk management
* ✅ Tuple destructuring for clean code

```csharp
// Get tick info:
var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync("EURUSD");

// Calculate P/L:
double ticks = priceDifference / tickSize;
double profitLoss = ticks * tickValue * volume;

// Used internally by CalcVolumeForRiskAsync:
var volume = await svc.CalcVolumeForRiskAsync(symbol, stopPoints, riskMoney);
```

**Know your ticks, calculate with precision!** 🚀
