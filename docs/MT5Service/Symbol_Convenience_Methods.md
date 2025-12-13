# MT5Service - Symbol Convenience Methods

> 7 convenient methods for quick access to symbol properties + 1 smart combination method

---

## 🎯 Why These Methods Exist

**Problem**: Getting symbol properties in MT5Account requires enum parameters and `.Value` unwrapping:
```csharp
var bidData = await account.SymbolInfoDoubleAsync("EURUSD", SymbolInfoDoubleProperty.SymbolBid);
double bid = bidData.Value;  // ← unwrapping!
```

**Solution**: MT5Service provides direct methods with clear names:
```csharp
double bid = await service.GetBidAsync("EURUSD");  // ✅ clean!
```

**Benefits:**

- ✅ Readable names (`GetBid` vs `SymbolInfoDouble(SymbolBid)`)
- ✅ No enum parameters needed
- ✅ No `.Value` unwrapping
- ✅ Smart combinations (availability check = Exist + Synchronized)

---

## 📋 All 7 Methods + 1 Smart Method

| Method | Returns | Low-Level Equivalent |
|--------|---------|---------------------|
| `GetBidAsync(symbol)` | Current bid price | `SymbolInfoDoubleAsync(symbol, SymbolBid)` |
| `GetAskAsync(symbol)` | Current ask price | `SymbolInfoDoubleAsync(symbol, SymbolAsk)` |
| `GetSpreadAsync(symbol)` | Current spread in points | `SymbolInfoIntegerAsync(symbol, SymbolSpread)` |
| `GetVolumeMinAsync(symbol)` | Minimum lot size | `SymbolInfoDoubleAsync(symbol, SymbolVolumeMin)` |
| `GetVolumeMaxAsync(symbol)` | Maximum lot size | `SymbolInfoDoubleAsync(symbol, SymbolVolumeMax)` |
| `GetVolumeStepAsync(symbol)` | Volume step increment | `SymbolInfoDoubleAsync(symbol, SymbolVolumeStep)` |
| `IsTradingAllowedAsync()` | Trading permission check | `AccountInfoIntegerAsync(AccountTradeAllowed)` |
| **`IsSymbolAvailableAsync(symbol)`** | **Smart: Exist + Synchronized** | **2 calls combined!** |

---

## 💡 Usage Examples

### Example 1: Getting Current Quote (Bid/Ask)

```csharp
// ❌ BEFORE (MT5Account) - 4 lines:
var bidData = await account.SymbolInfoDoubleAsync("EURUSD", SymbolInfoDoubleProperty.SymbolBid);
double bid = bidData.Value;
var askData = await account.SymbolInfoDoubleAsync("EURUSD", SymbolInfoDoubleProperty.SymbolAsk);
double ask = askData.Value;

// ✅ AFTER (MT5Service) - 2 lines:
double bid = await service.GetBidAsync("EURUSD");
double ask = await service.GetAskAsync("EURUSD");

Console.WriteLine($"EURUSD: Bid={bid:F5}, Ask={ask:F5}");
```

**Code reduction: 50%** (4 lines → 2 lines)

---

### Example 2: Calculating Spread

```csharp
// ❌ BEFORE (MT5Account):
var spreadData = await account.SymbolInfoIntegerAsync("GBPUSD", SymbolInfoIntegerProperty.SymbolSpread);
long spread = spreadData.Value;
Console.WriteLine($"Spread: {spread} points");

// ✅ AFTER (MT5Service):
long spread = await service.GetSpreadAsync("GBPUSD");
Console.WriteLine($"Spread: {spread} points");

// Or calculate spread in price:
double bid = await service.GetBidAsync("GBPUSD");
double ask = await service.GetAskAsync("GBPUSD");
double spreadPrice = ask - bid;
Console.WriteLine($"Spread: {spreadPrice:F5} ({spread} points)");
```

---

### Example 3: Validating Volume Before Order

```csharp
// ❌ BEFORE (MT5Account) - 6 lines:
var minData = await account.SymbolInfoDoubleAsync("XAUUSD", SymbolInfoDoubleProperty.SymbolVolumeMin);
double minVol = minData.Value;
var maxData = await account.SymbolInfoDoubleAsync("XAUUSD", SymbolInfoDoubleProperty.SymbolVolumeMax);
double maxVol = maxData.Value;
var stepData = await account.SymbolInfoDoubleAsync("XAUUSD", SymbolInfoDoubleProperty.SymbolVolumeStep);
double step = stepData.Value;

double requestedVolume = 0.15;
if (requestedVolume < minVol)
{
    Console.WriteLine($"❌ Volume too small! Min: {minVol}");
}
else if (requestedVolume > maxVol)
{
    Console.WriteLine($"❌ Volume too large! Max: {maxVol}");
}
else if ((requestedVolume % step) != 0)
{
    Console.WriteLine($"❌ Invalid volume step! Step: {step}");
}

// ✅ AFTER (MT5Service) - 3 lines:
double minVol = await service.GetVolumeMinAsync("XAUUSD");
double maxVol = await service.GetVolumeMaxAsync("XAUUSD");
double step = await service.GetVolumeStepAsync("XAUUSD");

double requestedVolume = 0.15;
if (requestedVolume < minVol)
{
    Console.WriteLine($"❌ Volume too small! Min: {minVol}");
}
else if (requestedVolume > maxVol)
{
    Console.WriteLine($"❌ Volume too large! Max: {maxVol}");
}
else if ((requestedVolume % step) != 0)
{
    Console.WriteLine($"❌ Invalid volume step! Step: {step}");
}
```

**Code reduction: 50%** (6 lines → 3 lines for data retrieval)

---

### Example 4: Smart Symbol Availability Check ⭐

```csharp
// ❌ BEFORE (MT5Account) - 2 separate calls + unwrapping:
var existsData = await account.SymbolExistAsync("BTCUSD");
if (!existsData.Exists)
{
    Console.WriteLine("❌ Symbol doesn't exist!");
    return;
}

var syncData = await account.SymbolIsSynchronizedAsync("BTCUSD");
if (!syncData.Synchronized)
{
    Console.WriteLine("❌ Symbol not synchronized!");
    return;
}

Console.WriteLine("✅ Symbol ready to trade!");

// ✅ AFTER (MT5Service) - 1 smart call:
bool available = await service.IsSymbolAvailableAsync("BTCUSD");

if (!available)
{
    Console.WriteLine("❌ Symbol not available (doesn't exist or not synchronized)!");
    return;
}

Console.WriteLine("✅ Symbol ready to trade!");
```

**Smart combination:** 2 server calls → 1 method call!

---

### Example 5: Pre-Trade Validation (Full Example)

```csharp
// Complete validation before placing order
string symbol = "EURUSD";
double requestedVolume = 0.05;

// Check trading is allowed
bool tradingAllowed = await service.IsTradingAllowedAsync();
if (!tradingAllowed)
{
    Console.WriteLine("❌ Trading not allowed on this account!");
    return;
}

// Check symbol availability (smart check!)
bool symbolAvailable = await service.IsSymbolAvailableAsync(symbol);
if (!symbolAvailable)
{
    Console.WriteLine($"❌ Symbol {symbol} not available!");
    return;
}

// Check volume constraints
double minVol = await service.GetVolumeMinAsync(symbol);
double maxVol = await service.GetVolumeMaxAsync(symbol);
double step = await service.GetVolumeStepAsync(symbol);

if (requestedVolume < minVol || requestedVolume > maxVol)
{
    Console.WriteLine($"❌ Volume {requestedVolume} out of range [{minVol}, {maxVol}]");
    return;
}

if ((requestedVolume % step) != 0)
{
    Console.WriteLine($"❌ Volume must be multiple of {step}");
    return;
}

// All checks passed!
Console.WriteLine("✅ All validations passed, ready to trade!");

// Place order...
var result = await service.BuyMarketAsync(symbol, requestedVolume);
```

---

### Example 6: Monitoring Multiple Symbols

```csharp
// Monitor quotes for multiple symbols
string[] symbols = { "EURUSD", "GBPUSD", "USDJPY", "XAUUSD" };

foreach (var symbol in symbols)
{
    // Check availability first
    if (!await service.IsSymbolAvailableAsync(symbol))
    {
        Console.WriteLine($"⚠️ {symbol} not available, skipping...");
        continue;
    }

    // Get quote
    double bid = await service.GetBidAsync(symbol);
    double ask = await service.GetAskAsync(symbol);
    long spread = await service.GetSpreadAsync(symbol);

    Console.WriteLine($"{symbol}: Bid={bid:F5}, Ask={ask:F5}, Spread={spread}pts");
}
```

---

### Example 7: Parallel Quote Retrieval

```csharp
// Get bid/ask for multiple symbols in parallel
var tasks = new[]
{
    service.GetBidAsync("EURUSD"),
    service.GetBidAsync("GBPUSD"),
    service.GetBidAsync("USDJPY")
};

await Task.WhenAll(tasks);

Console.WriteLine($"EURUSD Bid: {tasks[0].Result:F5}");
Console.WriteLine($"GBPUSD Bid: {tasks[1].Result:F5}");
Console.WriteLine($"USDJPY Bid: {tasks[2].Result:F5}");
```

**Performance:** All requests execute concurrently!

---

## 🔑 Key Benefits

| Aspect | MT5Account (Low-Level) | MT5Service (Convenience) |
|--------|----------------------|-------------------------|
| **Code** | 2 lines per value | 1 line per value |
| **Enums** | Always required | ❌ Not needed |
| **Unwrapping** | Always `.Value` | ❌ Not needed |
| **Availability Check** | 2 separate calls | ✅ 1 smart method |
| **Readability** | `SymbolInfoDouble(SymbolBid)` | `GetBid()` |
| **Error-prone** | Can forget `.Value` | ✅ Impossible |

---

## ⭐ Special: IsSymbolAvailableAsync()

This is a **smart combination method** that does more than just unwrap values!

```csharp
// What it does internally:
public async Task<bool> IsSymbolAvailableAsync(string symbol)
{
    // 1. Check if symbol exists
    var exists = await SymbolExistAsync(symbol);
    if (!exists) return false;

    // 2. Check if symbol is synchronized with server
    return await SymbolIsSynchronizedAsync(symbol);
}
```

**Benefits:**

- ✅ 2 server calls → 1 method call
- ✅ Clear intent: "Can I trade this symbol?"
- ✅ Prevents common mistake (checking Exist but forgetting Synchronized)
- ✅ Less code, fewer errors

---

## 📊 Code Reduction Statistics

| Task | Lines (MT5Account) | Lines (MT5Service) | Reduction |
|------|-------------------|--------------------|-----------|
| Get 1 value | 2 | 1 | **50%** |
| Get Bid+Ask | 4 | 2 | **50%** |
| Get volume constraints | 6 | 3 | **50%** |
| Check availability | 8 | 1 | **87.5%** |

**Average reduction: 60%+** for symbol operations!

---

## 🎓 When to Use

### ✅ Use MT5Service when:
- Need quick access to common symbol properties (bid, ask, spread)
- Validating volumes before placing orders
- Checking symbol availability
- Writing trading strategies
- Want clean, readable code

### ⚠️ Use MT5Account when:
- Need exotic symbol properties (no convenience method)
- Building custom wrappers
- Require low-level control

---

## 🔗 See Also

* **[MT5Service Overview](./MT5Service.Overview.md)** - Complete MT5Service improvements
* **[Account Convenience Methods](./Account_Convenience_Methods.md)** - Account shortcuts
* **[Trading Convenience Methods](./Trading_Convenience_Methods.md)** - Trading shortcuts
* **[MT5Account Symbol Methods](../MT5Account/2.%20Symbol_information/Symbol_Information.Overview.md)** - Low-level reference

---

## 💡 Summary

**7 convenience methods + 1 smart combination** make symbol operations cleaner and safer:

```csharp
// Quick quote check:
double bid = await service.GetBidAsync("EURUSD");
double ask = await service.GetAskAsync("EURUSD");

// Smart availability check:
if (await service.IsSymbolAvailableAsync("EURUSD"))
{
    // Ready to trade!
}

// Volume validation:
double min = await service.GetVolumeMinAsync("EURUSD");
double max = await service.GetVolumeMaxAsync("EURUSD");
double step = await service.GetVolumeStepAsync("EURUSD");
```

**Simple, safe, fast!** 🚀
