# ✅ Get Symbol Snapshot (`GetSymbolSnapshot`)

> **Sugar method:** Gets symbol tick, point, digits, and margin rate in one convenient call. Returns all as a single record.

**API Information:**

* **Extension method:** `MT5Service.GetSymbolSnapshot(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Underlying calls:** `EnsureSelected()` + `SymbolInfoTickAsync()` + `SymbolInfoDoubleAsync()` + `SymbolInfoIntegerAsync()` + `SymbolInfoMarginRateAsync()`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public sealed record SymbolSnapshot(
        MrpcMqlTick Tick,
        double Point,
        int Digits,
        SymbolInfoMarginRateData MarginRate
    );

    public static async Task<SymbolSnapshot> GetSymbolSnapshot(
        this MT5Service svc,
        string symbol,
        int timeoutSec = 10,
        CancellationToken ct = default);
}
```

---

## 🔽 Input

| Parameter    | Type                | Description                                     |
| ------------ | ------------------- | ----------------------------------------------- |
| `svc`        | `MT5Service`        | MT5Service instance (extension method)          |
| `symbol`     | `string`            | Symbol name (e.g., "EURUSD")                    |
| `timeoutSec` | `int`               | Timeout in seconds (default: 10)                |
| `ct`         | `CancellationToken` | Cancellation token                              |

---

## ⬆️ Output — `SymbolSnapshot`

| Field         | Type                        | Description                           |
| ------------- | --------------------------- | ------------------------------------- |
| `Tick`        | `MrpcMqlTick`               | Current tick (bid, ask, last, volume) |
| `Point`       | `double`                    | Symbol point size                     |
| `Digits`      | `int`                       | Symbol digits (decimal places)        |
| `MarginRate`  | `SymbolInfoMarginRateData`  | Margin rate for BUY orders            |

### `MrpcMqlTick` — Tick information

| Field         | Type        | Description                        |
| ------------- | ----------- | ---------------------------------- |
| `Time`        | `Timestamp` | Tick time                          |
| `Bid`         | `double`    | Current bid price                  |
| `Ask`         | `double`    | Current ask price                  |
| `Last`        | `double`    | Last price                         |
| `Volume`      | `uint64`    | Tick volume                        |
| `TimeMsc`     | `int64`     | Tick time in milliseconds          |
| `Flags`       | `uint32`    | Tick flags                         |
| `VolumeReal`  | `double`    | Real volume                        |

### `SymbolInfoMarginRateData` — Margin rate information

| Field         | Type     | Description                           |
| ------------- | -------- | ------------------------------------- |
| `Initial`     | `double` | Initial margin rate                   |
| `Maintenance` | `double` | Maintenance margin rate               |

---

## 💬 Just the essentials

* **What it is.** One-liner to get complete symbol state: tick prices, point size, digits, and margin requirements.
* **Why you need it.** Avoid multiple separate API calls. Get everything needed for price calculations, order placement, risk management.
* **Sanity check.** Returns record with all symbol properties. Use `Tick.Bid`/`Tick.Ask` for current prices, `Point` for price calculations, `Digits` for formatting.

---

## 🎯 Purpose

Use it for symbol initialization:

* Get current bid/ask prices.
* Calculate price offsets using point size.
* Format prices with correct decimal places.
* Calculate margin requirements.
* Quick symbol information check.

---

## 🧩 Notes & Tips

* **Single call:** Combines 5 API calls internally but feels like one operation.
* **Symbol selection:** Automatically calls `EnsureSelected()` to ensure symbol is selected and synchronized.
* **Atomic snapshot:** All calls use same deadline, so data is relatively synchronized.
* **BUY margin rate:** Returns margin rate for BUY orders (enum value 0). For SELL, call `SymbolInfoMarginRateAsync()` separately.
* **Record type:** Returns immutable `SymbolSnapshot` record with four properties.
* **Point vs Digits:** `Point` is actual tick size (e.g., 0.00001), `Digits` is decimal places (e.g., 5). Use `Point` for calculations, `Digits` for formatting.

---

## 🔧 Under the Hood

This sugar method combines five low-level calls:

```csharp
var deadline = DateTime.UtcNow.AddSeconds(timeoutSec);

// Step 1: Ensure symbol is selected and synchronized
await svc.EnsureSelected(symbol, timeoutSec, ct);

// Step 2: Get current tick (bid, ask, last, volume)
var tick = await svc.SymbolInfoTickAsync(symbol, deadline, ct);

// Step 3: Get point size (minimum price change)
var point = (await svc.SymbolInfoDoubleAsync(symbol,
    SymbolInfoDoubleProperty.SymbolPoint, deadline, ct)).Value;

// Step 4: Get digits (decimal precision)
var digits = (int)(await svc.SymbolInfoIntegerAsync(symbol,
    SymbolInfoIntegerProperty.SymbolDigits, deadline, ct)).Value;

// Step 5: Get margin rate for BUY orders
var margin = await svc.SymbolInfoMarginRateAsync(symbol,
    (mt5_term_api.ENUM_ORDER_TYPE)0, deadline, ct);

// Step 6: Return as single record
return new SymbolSnapshot(tick, point, digits, margin);
```

**What it improves:**

* **Combines 5 calls into 1** - much simpler API
* **Automatic symbol selection** - ensures symbol is ready
* **Shared deadline** - all calls use same timeout
* **Single return type** - immutable record with all data
* **Safe enum usage** - uses enum value 0, not hardcoded name

---

## 📊 Low-Level Alternative

**WITHOUT sugar (manual approach):**
```csharp
// You have to do this manually:
var deadline = DateTime.UtcNow.AddSeconds(10);

// Step 1: Select symbol
await svc.SymbolSelectAsync("EURUSD", selected: true, deadline, ct);

// Step 2: Check synchronization
var sync = await svc.SymbolIsSynchronizedAsync("EURUSD", deadline, ct);
if (!sync.Synchronized)
    throw new InvalidOperationException("Symbol not synchronized");

// Step 3: Get tick
var tick = await svc.SymbolInfoTickAsync("EURUSD", deadline, ct);

// Step 4: Get point
var pointResult = await svc.SymbolInfoDoubleAsync("EURUSD",
    SymbolInfoDoubleProperty.SymbolPoint, deadline, ct);
var point = pointResult.Value;

// Step 5: Get digits
var digitsResult = await svc.SymbolInfoIntegerAsync("EURUSD",
    SymbolInfoIntegerProperty.SymbolDigits, deadline, ct);
var digits = (int)digitsResult.Value;

// Step 6: Get margin rate
var margin = await svc.SymbolInfoMarginRateAsync("EURUSD",
    (mt5_term_api.ENUM_ORDER_TYPE)0, deadline, ct);

// Now you have 4 separate variables to manage
```

**WITH sugar (one-liner):**
```csharp
// Sugar method does all of the above:
var snapshot = await svc.GetSymbolSnapshot("EURUSD");

// Work with single object:
Console.WriteLine($"Bid: {snapshot.Tick.Bid}");
Console.WriteLine($"Point: {snapshot.Point}");
Console.WriteLine($"Digits: {snapshot.Digits}");
Console.WriteLine($"Margin: {snapshot.MarginRate.Initial}");
```

**Benefits:**

* ✅ **6 steps → 1 line**
* ✅ **Automatic symbol selection & validation**
* ✅ **Automatic deadline management**
* ✅ **Single object to work with**
* ✅ **Type-safe int conversion for digits**

---

## 🔗 Usage Examples

### 1) Basic symbol snapshot

```csharp
// svc — MT5Service instance

var snapshot = await svc.GetSymbolSnapshot("EURUSD");

Console.WriteLine("Symbol: EURUSD");
Console.WriteLine($"  Bid:    {snapshot.Tick.Bid}");
Console.WriteLine($"  Ask:    {snapshot.Tick.Ask}");
Console.WriteLine($"  Spread: {(snapshot.Tick.Ask - snapshot.Tick.Bid) / snapshot.Point:F1} points");
Console.WriteLine($"  Point:  {snapshot.Point}");
Console.WriteLine($"  Digits: {snapshot.Digits}");
Console.WriteLine($"  Margin (Initial):     {snapshot.MarginRate.Initial}");
Console.WriteLine($"  Margin (Maintenance): {snapshot.MarginRate.Maintenance}");
```

---

### 2) Calculate price offset in points

```csharp
var snapshot = await svc.GetSymbolSnapshot("GBPUSD");

int stopLossPoints = 100;
double slPrice = snapshot.Tick.Bid - (stopLossPoints * snapshot.Point);

Console.WriteLine($"Current Bid: {snapshot.Tick.Bid}");
Console.WriteLine($"SL (-100 points): {slPrice}");
Console.WriteLine($"Distance: {stopLossPoints} points = ${(stopLossPoints * snapshot.Point):F5}");
```

---

### 3) Format price with correct digits

```csharp
var snapshot = await svc.GetSymbolSnapshot("USDJPY");

// Round price to symbol digits
double rawPrice = 149.12345678;
double normalizedPrice = Math.Round(rawPrice, snapshot.Digits);

Console.WriteLine($"Raw price:        {rawPrice}");
Console.WriteLine($"Normalized price: {normalizedPrice}");
Console.WriteLine($"Formatted:        {normalizedPrice.ToString($"F{snapshot.Digits}")}");
```

---

### 4) Calculate spread in different units

```csharp
var snapshot = await svc.GetSymbolSnapshot("EURUSD");

double spreadPoints = (snapshot.Tick.Ask - snapshot.Tick.Bid) / snapshot.Point;
double spreadPrice = snapshot.Tick.Ask - snapshot.Tick.Bid;

Console.WriteLine($"Bid: {snapshot.Tick.Bid}");
Console.WriteLine($"Ask: {snapshot.Tick.Ask}");
Console.WriteLine($"Spread: {spreadPoints:F1} points");
Console.WriteLine($"Spread: ${spreadPrice:F5}");
```

---

### 5) Check if prices are reasonable

```csharp
var snapshot = await svc.GetSymbolSnapshot("EURUSD");

bool isValid = snapshot.Tick.Bid > 0
            && snapshot.Tick.Ask > 0
            && snapshot.Tick.Ask > snapshot.Tick.Bid
            && snapshot.Point > 0
            && snapshot.Digits > 0;

if (isValid)
{
    Console.WriteLine("✓ Symbol data is valid");
    Console.WriteLine($"  Bid/Ask spread: {(snapshot.Tick.Ask - snapshot.Tick.Bid) / snapshot.Point:F1} points");
}
else
{
    Console.WriteLine("⚠ Invalid symbol data!");
}
```

---

### 6) Compare margin rates across symbols

```csharp
string[] symbols = { "EURUSD", "GBPUSD", "USDJPY" };

Console.WriteLine("Margin Rates Comparison:");
Console.WriteLine("─────────────────────────────────────────────");

foreach (var symbol in symbols)
{
    var snapshot = await svc.GetSymbolSnapshot(symbol);

    Console.WriteLine($"{symbol,-10} Initial: {snapshot.MarginRate.Initial,6:F2} | " +
                     $"Maintenance: {snapshot.MarginRate.Maintenance,6:F2}");
}
```

---

### 7) Calculate take profit price

```csharp
var snapshot = await svc.GetSymbolSnapshot("EURUSD");

int takeProfitPoints = 200;
double tpPrice = snapshot.Tick.Ask + (takeProfitPoints * snapshot.Point);

// Normalize to symbol digits
tpPrice = Math.Round(tpPrice, snapshot.Digits);

Console.WriteLine($"Current Ask: {snapshot.Tick.Ask}");
Console.WriteLine($"TP (+{takeProfitPoints} points): {tpPrice}");
Console.WriteLine($"Expected profit: {takeProfitPoints} points");
```

---

### 8) Validate symbol before trading

```csharp
async Task<bool> ValidateSymbol(MT5Service svc, string symbol)
{
    try
    {
        var snapshot = await svc.GetSymbolSnapshot(symbol);

        // Check if tick is recent (within last 5 seconds)
        var tickTime = snapshot.Tick.Time.ToDateTime();
        var age = DateTime.UtcNow - tickTime;

        if (age.TotalSeconds > 5)
        {
            Console.WriteLine($"⚠ Tick is old: {age.TotalSeconds:F1}s");
            return false;
        }

        // Check if spread is reasonable (< 50 points)
        var spreadPoints = (snapshot.Tick.Ask - snapshot.Tick.Bid) / snapshot.Point;
        if (spreadPoints > 50)
        {
            Console.WriteLine($"⚠ Spread too wide: {spreadPoints:F1} points");
            return false;
        }

        Console.WriteLine($"✓ {symbol} validated successfully");
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Validation failed: {ex.Message}");
        return false;
    }
}

// Usage
bool canTrade = await ValidateSymbol(svc, "EURUSD");
```

---

### 9) Build price ladder

```csharp
var snapshot = await svc.GetSymbolSnapshot("EURUSD");

int levels = 5;
int stepPoints = 10;

Console.WriteLine("Price Ladder (Buy):");
Console.WriteLine("─────────────────────────────");

for (int i = -levels; i <= levels; i++)
{
    double price = snapshot.Tick.Ask + (i * stepPoints * snapshot.Point);
    price = Math.Round(price, snapshot.Digits);

    string marker = i == 0 ? " ← Current Ask" : "";
    Console.WriteLine($"{i,3} levels ({i * stepPoints,4} pts): {price}{marker}");
}
```

---

### 10) Symbol information dashboard

```csharp
var snapshot = await svc.GetSymbolSnapshot("EURUSD");

Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("        SYMBOL INFORMATION DASHBOARD       ");
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine($"Symbol:        EURUSD");
Console.WriteLine($"Time:          {snapshot.Tick.Time.ToDateTime():yyyy-MM-dd HH:mm:ss}");
Console.WriteLine();
Console.WriteLine($"Bid:           {snapshot.Tick.Bid}");
Console.WriteLine($"Ask:           {snapshot.Tick.Ask}");
Console.WriteLine($"Last:          {snapshot.Tick.Last}");
Console.WriteLine($"Spread:        {(snapshot.Tick.Ask - snapshot.Tick.Bid) / snapshot.Point:F1} points");
Console.WriteLine();
Console.WriteLine($"Point Size:    {snapshot.Point}");
Console.WriteLine($"Digits:        {snapshot.Digits}");
Console.WriteLine();
Console.WriteLine($"Margin (Initial):     {snapshot.MarginRate.Initial}");
Console.WriteLine($"Margin (Maintenance): {snapshot.MarginRate.Maintenance}");
Console.WriteLine();

// Calculate 1 pip value
double onePipInPoints = snapshot.Digits == 5 || snapshot.Digits == 3 ? 10 : 1;
double onePipInPrice = onePipInPoints * snapshot.Point;

Console.WriteLine($"1 Pip = {onePipInPoints} points = {onePipInPrice:F5}");
```

---

### 11) Monitor multiple symbols

```csharp
string[] watchlist = { "EURUSD", "GBPUSD", "USDJPY", "GOLD" };

var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(30)); // Monitor for 30 seconds

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        Console.Clear();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Market Watch");
        Console.WriteLine("─────────────────────────────────────────────");

        foreach (var symbol in watchlist)
        {
            var snapshot = await svc.GetSymbolSnapshot(symbol, ct: cts.Token);
            var spread = (snapshot.Tick.Ask - snapshot.Tick.Bid) / snapshot.Point;

            Console.WriteLine($"{symbol,-10} Bid: {snapshot.Tick.Bid,10:F5} | " +
                             $"Ask: {snapshot.Tick.Ask,10:F5} | " +
                             $"Spread: {spread,5:F1} pts");
        }

        await Task.Delay(2000, cts.Token); // Update every 2 seconds
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nMonitoring stopped");
}
```

---

### 12) Calculate required margin

```csharp
var snapshot = await svc.GetSymbolSnapshot("EURUSD");

double volume = 1.0; // 1 lot
double contractSize = 100000; // Standard lot for EURUSD

// Margin = (Volume * ContractSize * Price) / Leverage * MarginRate
// Simplified: assuming leverage is applied elsewhere
double requiredMargin = volume * contractSize * snapshot.Tick.Ask * snapshot.MarginRate.Initial;

Console.WriteLine($"Symbol:           EURUSD");
Console.WriteLine($"Volume:           {volume} lot");
Console.WriteLine($"Entry Price:      {snapshot.Tick.Ask}");
Console.WriteLine($"Margin Rate:      {snapshot.MarginRate.Initial}");
Console.WriteLine($"Required Margin:  ${requiredMargin:F2}");
```

---

## 🔗 Related Methods

**📦 Low-level methods used internally:**

* `EnsureSelected()` - Ensure symbol is selected and synchronized - step 1
* `SymbolInfoTickAsync()` - Get current tick (bid, ask, last) - step 2
* `SymbolInfoDoubleAsync(SymbolPoint)` - Get point size - step 3
* `SymbolInfoIntegerAsync(SymbolDigits)` - Get decimal digits - step 4
* `SymbolInfoMarginRateAsync()` - Get margin rates - step 5

**🍬 Other sugar methods:**

* `GetAccountSnapshot()` - Get account data snapshot (balance, equity, positions)
* `GetPointAsync()` - Get point size only (simpler if you only need point)
* `GetDigitsAsync()` - Get digits only (simpler if you only need digits)
* All trading methods can use this snapshot for symbol data

---

## ⚠️ Common Pitfalls

1. **Using Digits for calculations instead of Point**
   ```csharp
   // ❌ WRONG - using digits
   double slPrice = bid - (100 * Math.Pow(10, -digits));

   // ✅ CORRECT - using point
   double slPrice = bid - (100 * snapshot.Point);
   ```

2. **Not checking tick freshness**
   ```csharp
   // ❌ WRONG - using old tick
   var snapshot = await svc.GetSymbolSnapshot("EURUSD");
   // ... hours later ...
   double price = snapshot.Tick.Ask; // Old price!

   // ✅ CORRECT - get fresh snapshot
   snapshot = await svc.GetSymbolSnapshot("EURUSD");
   double price = snapshot.Tick.Ask;
   ```

3. **Ignoring spread**
   ```csharp
   // ❌ WRONG - using Bid for buy orders
   double entryPrice = snapshot.Tick.Bid; // Should use Ask!

   // ✅ CORRECT - use Ask for buy, Bid for sell
   double buyPrice = snapshot.Tick.Ask;
   double sellPrice = snapshot.Tick.Bid;
   ```

4. **Assuming margin rate is 1.0**
   ```csharp
   // ❌ WRONG - ignoring margin rate
   double margin = volume * price;

   // ✅ CORRECT - apply margin rate
   double margin = volume * price * snapshot.MarginRate.Initial;
   ```

5. **Not normalizing calculated prices**
   ```csharp
   // ❌ WRONG - raw calculation
   double tpPrice = snapshot.Tick.Ask + (100 * snapshot.Point);

   // ✅ CORRECT - normalize to symbol digits
   double tpPrice = Math.Round(
       snapshot.Tick.Ask + (100 * snapshot.Point),
       snapshot.Digits);
   ```
