# MT5Service - Mid-Level API Overview

> **Not just a wrapper!** MT5Service eliminates 50-70% of boilerplate code while maintaining full MT5 functionality.

---

## 🎯 What is MT5Service?

MT5Service is a **mid-level convenience layer** between low-level MT5Account (proto/gRPC) and high-level MT5Sugar (business logic).

**Three-Layer Architecture:**
```
MT5Account (Low-Level)  →  Direct gRPC calls, returns Data wrappers
     ↓
MT5Service (Mid-Level)  →  Unwraps primitives + convenience shortcuts  ← YOU ARE HERE
     ↓
MT5Sugar (High-Level)   →  Complex business logic, smart helpers
```

**What makes it special:**

- ✅ **Automatic value unwrapping** - no more `.Value` ceremony
- ✅ **~30 convenience methods** - shortcuts for common operations
- ✅ **Smart combinations** - multiple checks in one call
- ✅ **Ergonomic trading API** - no manual request building
- ✅ **50-70% code reduction** - write less, do more

---

## 🚀 Quick Comparison

### Account Operations

```csharp
// ❌ MT5Account (Low-Level) - 2 lines:
var data = await account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountBalance);
double balance = data.Value;

// ✅ MT5Service (Mid-Level) - 1 line:
double balance = await service.GetBalanceAsync();
```

### Trading Operations

```csharp
// ❌ MT5Account (Low-Level) - 9 lines:
var request = new OrderSendRequest
{
    Symbol = "EURUSD",
    Volume = 0.01,
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
    StopLoss = 1.0800,
    TakeProfit = 1.0900,
    Comment = "My trade"
};
var result = await account.OrderSendAsync(request);

// ✅ MT5Service (Mid-Level) - 1 line:
var result = await service.BuyMarketAsync("EURUSD", 0.01,
    stopLoss: 1.0800, takeProfit: 1.0900, comment: "My trade");
```

### Symbol Availability Check

```csharp
// ❌ MT5Account (Low-Level) - 2 separate calls:
var existsData = await account.SymbolExistAsync("EURUSD");
if (!existsData.Exists) return false;
var syncData = await account.SymbolIsSynchronizedAsync("EURUSD");
bool available = syncData.Synchronized;

// ✅ MT5Service (Mid-Level) - 1 smart call:
bool available = await service.IsSymbolAvailableAsync("EURUSD");
```

---

## 📦 What's Inside: 4 Groups of Methods

### 1️⃣ [Account Convenience Methods](./Account_Convenience_Methods.md) (10 methods)

Direct access to account properties without unwrapping:

```csharp
// Get key account metrics - 1 line each:
double balance = await service.GetBalanceAsync();
double equity = await service.GetEquityAsync();
double margin = await service.GetMarginAsync();
double freeMargin = await service.GetFreeMarginAsync();
double profit = await service.GetProfitAsync();

// Get account info:
long login = await service.GetLoginAsync();
long leverage = await service.GetLeverageAsync();
string name = await service.GetAccountNameAsync();
string server = await service.GetServerNameAsync();
string currency = await service.GetCurrencyAsync();
```

**Benefits:** 50% code reduction, no `.Value`, readable names

---

### 2️⃣ [Symbol Convenience Methods](./Symbol_Convenience_Methods.md) (7 methods + 1 smart)

Quick access to symbol properties and smart availability check:

```csharp
// Get current quote:
double bid = await service.GetBidAsync("EURUSD");
double ask = await service.GetAskAsync("EURUSD");
long spread = await service.GetSpreadAsync("EURUSD");

// Volume constraints:
double minVol = await service.GetVolumeMinAsync("EURUSD");
double maxVol = await service.GetVolumeMaxAsync("EURUSD");
double step = await service.GetVolumeStepAsync("EURUSD");

// Smart combination (2 calls → 1 method):
bool available = await service.IsSymbolAvailableAsync("EURUSD");

// Trading permission check:
bool canTrade = await service.IsTradingAllowedAsync();
```

**Benefits:** 60%+ code reduction, smart combinations, no enums

---

### 3️⃣ [Trading Convenience Methods](./Trading_Convenience_Methods.md) (6 methods)

Simplified order placement without manual request building:

```csharp
// Market orders:
await service.BuyMarketAsync("EURUSD", 0.01, stopLoss: 1.08, takeProfit: 1.09);
await service.SellMarketAsync("GBPUSD", 0.05, stopLoss: 1.26, takeProfit: 1.25);

// Limit orders (price improvement):
await service.BuyLimitAsync("EURUSD", 0.1, price: 1.0850, stopLoss: 1.08, takeProfit: 1.09);
await service.SellLimitAsync("GBPUSD", 0.05, price: 1.2550, stopLoss: 1.26, takeProfit: 1.25);

// Stop orders (breakout):
await service.BuyStopAsync("XAUUSD", 0.01, price: 2010, stopLoss: 2000, takeProfit: 2030);
await service.SellStopAsync("USDJPY", 0.1, price: 149.5, stopLoss: 150, takeProfit: 149);
```

**Benefits:** 70%+ code reduction, self-documenting, named parameters

---

### 4️⃣ [History Convenience Methods](./History_Convenience_Methods.md) (3 methods)

Sensible defaults for common history queries:

```csharp
// Get recent orders:
var lastWeek = await service.GetRecentOrdersAsync(days: 7);
var lastMonth = await service.GetRecentOrdersAsync(days: 30, limit: 500);

// Get today's orders:
var today = await service.GetTodayOrdersAsync();

// Check trading permission:
if (await service.IsTradingAllowedAsync())
{
    // Ready to trade
}
```

**Benefits:** 84% code reduction, sensible defaults, readable intent

---

## 🔧 Key Features Explained

### Feature 1: Automatic Value Unwrapping

**Problem in MT5Account:**
Every call returns a `Data` wrapper object that must be unwrapped:

```csharp
var balanceData = await account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountBalance);
double balance = balanceData.Value;  // ← Must unwrap!

var bidData = await account.SymbolInfoDoubleAsync("EURUSD", SymbolInfoDoubleProperty.SymbolBid);
double bid = bidData.Value;  // ← Must unwrap!

var existsData = await account.SymbolExistAsync("EURUSD");
bool exists = existsData.Exists;  // ← Must unwrap!
```

**Solution in MT5Service:**
Direct primitive returns - no unwrapping needed:

```csharp
double balance = await service.GetBalanceAsync();  // ✅ Already unwrapped
double bid = await service.GetBidAsync("EURUSD");  // ✅ Already unwrapped
bool exists = await service.SymbolExistAsync("EURUSD");  // ✅ Already unwrapped
```

**Impact:**

- ✅ 50% less code for data retrieval
- ✅ Impossible to forget `.Value`
- ✅ Cleaner, more readable code

**Methods with unwrapping:**

- All `AccountInfo*` → returns `double`, `long`, `string` directly
- All `SymbolInfo*` → returns `double`, `long`, `string` directly
- `SymbolsTotal` → returns `int` directly
- `SymbolExist`, `SymbolSelect`, `SymbolIsSynchronized` → returns `bool` directly

---

### Feature 2: Smart Combinations

**Problem in MT5Account:**
Common operations require multiple separate calls:

```csharp
// Check if symbol is ready for trading (2 calls):
var existsData = await account.SymbolExistAsync("BTCUSD");
if (!existsData.Exists)
{
    Console.WriteLine("Symbol doesn't exist!");
    return false;
}

var syncData = await account.SymbolIsSynchronizedAsync("BTCUSD");
if (!syncData.Synchronized)
{
    Console.WriteLine("Symbol not synchronized!");
    return false;
}

return true;
```

**Solution in MT5Service:**
Smart methods combine multiple checks:

```csharp
// 1 smart call replaces 2 separate calls:
bool available = await service.IsSymbolAvailableAsync("BTCUSD");

if (!available)
{
    Console.WriteLine("Symbol not available!");
    return false;
}

return true;
```

**Smart methods:**

- `IsSymbolAvailableAsync(symbol)` - combines `SymbolExist` + `SymbolIsSynchronized`
- `IsTradingAllowedAsync()` - checks `AccountTradeAllowed` and returns boolean

**Impact:**

- ✅ Fewer server round-trips
- ✅ Prevents common mistakes (forgetting one of the checks)
- ✅ Clear intent from method name

---

### Feature 3: Ergonomic Trading API

**Problem in MT5Account:**
Every order requires manual `OrderSendRequest` building:

```csharp
// Want to BUY? Build entire request manually:
var request = new OrderSendRequest
{
    Symbol = "EURUSD",
    Volume = 0.01,
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,  // Must remember enum
    StopLoss = 1.0800,
    TakeProfit = 1.0900,
    Comment = "My trade",
    ExpertId = 12345
};
var result = await account.OrderSendAsync(request);

// Want to SELL? Build another request:
var request2 = new OrderSendRequest
{
    Symbol = "GBPUSD",
    Volume = 0.05,
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSell,  // Different enum
    StopLoss = 1.2600,
    TakeProfit = 1.2500
};
var result2 = await account.OrderSendAsync(request2);
```

**Solution in MT5Service:**
Direct trading methods with named parameters:

```csharp
// BUY - intent clear from method name:
var result = await service.BuyMarketAsync("EURUSD", 0.01,
    stopLoss: 1.0800,
    takeProfit: 1.0900,
    comment: "My trade",
    magic: 12345);

// SELL - intent clear from method name:
var result2 = await service.SellMarketAsync("GBPUSD", 0.05,
    stopLoss: 1.2600,
    takeProfit: 1.2500);
```

**All 6 trading methods:**

- `BuyMarketAsync()` - buy at current ask
- `SellMarketAsync()` - sell at current bid
- `BuyLimitAsync()` - pending buy below current price
- `SellLimitAsync()` - pending sell above current price
- `BuyStopAsync()` - pending buy above current price (breakout)
- `SellStopAsync()` - pending sell below current price (breakdown)

**Impact:**

- ✅ 70-89% code reduction per order
- ✅ Self-documenting (method name = intent)
- ✅ Named parameters show what each value means
- ✅ Can't set wrong `Operation` enum (impossible error)

---

### Feature 4: Sensible Defaults

**Problem in MT5Account:**
Common operations require many parameters:

```csharp
// Want last week's orders? Specify everything:
var history = await account.OrderHistoryAsync(
    from: DateTime.UtcNow.AddDays(-7),                // Calculate manually
    to: DateTime.UtcNow,                              // Now
    sortMode: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc,  // Long enum
    pageNumber: 0,                                    // No pagination
    itemsPerPage: 100,                                // Limit
    deadline: null,                                   // No deadline
    cancellationToken: default);                      // No cancellation

// 7 parameters just to get last week!
```

**Solution in MT5Service:**
Sensible defaults for common scenarios:

```csharp
// Last week's orders - 1 parameter:
var history = await service.GetRecentOrdersAsync(days: 7);

// Today's orders - no parameters:
var today = await service.GetTodayOrdersAsync();

// Custom if needed:
var lastMonth = await service.GetRecentOrdersAsync(days: 30, limit: 500);
```

**Impact:**

- ✅ 84% code reduction for history queries
- ✅ Readable intent (`GetTodayOrders` vs manual date calculation)
- ✅ Defaults work for 90% of cases
- ✅ Still customizable when needed

---

## 📊 Overall Statistics

### Code Reduction by Category

| Category | Average Code Reduction | Key Benefit |
|----------|----------------------|-------------|
| Account operations | **50%** | No `.Value` unwrapping |
| Symbol operations | **60%+** | No enums + smart checks |
| Trading operations | **70-89%** | No request building |
| History operations | **84%** | Sensible defaults |

### Methods Count

| Category | Thin Wrappers | New Convenience Methods | Total |
|----------|--------------|------------------------|-------|
| Account | 4 | 10 | 14 |
| Symbol | 7 | 7 + 1 smart | 15 |
| Trading | 5 | 6 | 11 |
| History | 2 | 2 | 4 |
| Streaming | 5 (pass-through) | 0 | 5 |
| **Total** | **23** | **~30** | **~53** |

---

## 🎓 When to Use Each Layer

### Use MT5Account (Low-Level) when:
- ❌ Need exotic properties not covered by convenience methods
- ❌ Building custom wrapper/framework on top
- ❌ Require absolute control over proto objects
- ❌ Accessing advanced features not in MT5Service

**Example:** Accessing `SymbolInfoSession*` for trading hours

---

### Use MT5Service (Mid-Level) when: ✅
- ✅ **90% of typical trading scenarios**
- ✅ Building trading bots/strategies
- ✅ Rapid development
- ✅ Want clean, readable code
- ✅ Don't need auto-normalization (that's MT5Sugar)

**Example:** Placing orders, getting quotes, checking balances

---

### Use MT5Sugar (High-Level) when:
- ✅ Need auto-normalization of volumes/prices
- ✅ Want risk-based position sizing
- ✅ Need batch operations
- ✅ Require advanced helpers/validators

**Example:** `OpenPositionWithRisk(symbol, riskPercent: 2.0)`

---

## 💡 Complete Example: Trading Bot

```csharp
using mt5_term_api;

// Initialize
var account = new MT5Account();
var service = new MT5Service(account);

await account.ConnectByHostPortAsync("mt5.mrpc.pro", 443);

// 1. Check account
double balance = await service.GetBalanceAsync();
double equity = await service.GetEquityAsync();
double freeMargin = await service.GetFreeMarginAsync();

Console.WriteLine($"Balance: ${balance:F2}");
Console.WriteLine($"Equity: ${equity:F2}");
Console.WriteLine($"Free Margin: ${freeMargin:F2}");

// 2. Check trading is allowed
if (!await service.IsTradingAllowedAsync())
{
    Console.WriteLine("❌ Trading not allowed!");
    return;
}

// 3. Check symbol
string symbol = "EURUSD";
if (!await service.IsSymbolAvailableAsync(symbol))
{
    Console.WriteLine($"❌ Symbol {symbol} not available!");
    return;
}

// 4. Get quote
double bid = await service.GetBidAsync(symbol);
double ask = await service.GetAskAsync(symbol);
Console.WriteLine($"{symbol}: Bid={bid:F5}, Ask={ask:F5}");

// 5. Validate volume
double volume = 0.01;
double minVol = await service.GetVolumeMinAsync(symbol);
double maxVol = await service.GetVolumeMaxAsync(symbol);
double step = await service.GetVolumeStepAsync(symbol);

if (volume < minVol || volume > maxVol)
{
    Console.WriteLine($"❌ Volume out of range [{minVol}, {maxVol}]");
    return;
}

// 6. Place order
var result = await service.BuyMarketAsync(symbol, volume,
    stopLoss: bid - 0.0050,
    takeProfit: bid + 0.0100,
    comment: "Bot trade");

if (result.RetCode == 10009)
{
    Console.WriteLine($"✅ Order opened: #{result.Order}");
}
else
{
    Console.WriteLine($"❌ Order failed: {result.Comment}");
}

// 7. Check today's performance
var today = await service.GetTodayOrdersAsync();
double todayProfit = today.ClosedOrders.Sum(o => o.Profit);
Console.WriteLine($"Today's P/L: ${todayProfit:F2} ({today.ClosedOrders.Count} trades)");
```

**Clean, readable, maintainable!** 🚀

---

## 🔗 Documentation Index

### Method Groups
1. **[Account Convenience Methods](./Account_Convenience_Methods.md)** - Balance, equity, margin, profit, account info
2. **[Symbol Convenience Methods](./Symbol_Convenience_Methods.md)** - Bid, ask, spread, volumes, availability
3. **[Trading Convenience Methods](./Trading_Convenience_Methods.md)** - Market/limit/stop orders simplified
4. **[History Convenience Methods](./History_Convenience_Methods.md)** - Recent orders, today's orders, trading permissions

---

## 🎯 Key Takeaways

### What MT5Service Does:

1. ✅ **Eliminates `.Value` unwrapping** - returns primitives directly
2. ✅ **Adds ~30 convenience methods** - shortcuts for common operations
3. ✅ **Smart combinations** - multiple checks in one call
4. ✅ **Ergonomic trading API** - no request building
5. ✅ **Sensible defaults** - works for 90% of cases
6. ✅ **50-70% code reduction** - write less, do more

### What MT5Service Does NOT Do:
- ❌ Auto-normalization (that's MT5Sugar)
- ❌ Risk management (that's MT5Sugar)
- ❌ Complex business logic (that's MT5Sugar)
- ❌ Replace MT5Account (it wraps it)

### The Bottom Line:
**MT5Service is the sweet spot** between low-level control and high-level convenience. Use it for 90% of your trading code.

```csharp
// This is MT5Service:
var balance = await service.GetBalanceAsync();
var result = await service.BuyMarketAsync("EURUSD", 0.01, stopLoss: 1.08, takeProfit: 1.09);
var today = await service.GetTodayOrdersAsync();
```

**Simple. Clean. Fast.** 🚀
