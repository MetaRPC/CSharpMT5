# MT5Service - Trading Convenience Methods

> 6 simplified trading methods that eliminate manual `OrderSendRequest` building

---

## 🎯 Why These Methods Exist

**Problem**: In MT5Account, placing any order requires manually building `OrderSendRequest`:
```csharp
var request = new OrderSendRequest
{
    Symbol = "EURUSD",
    Volume = 0.01,
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
    StopLoss = 1.0800,
    TakeProfit = 1.0900,
    Comment = "My trade",
    ExpertId = 12345
};
var result = await account.OrderSendAsync(request);
```
**9 lines of boilerplate** for every trade!

**Solution**: MT5Service provides direct trading methods:
```csharp
var result = await service.BuyMarketAsync("EURUSD", 0.01,
    stopLoss: 1.0800, takeProfit: 1.0900,
    comment: "My trade", magic: 12345);
```

**1 line** - intent is crystal clear!

**Benefits:**

- ✅ **70% less code** for order placement
- ✅ No manual request building
- ✅ Named parameters = self-documenting
- ✅ Can't forget required fields
- ✅ Clear intent from method name

---

## 📋 All 6 Trading Methods

| Method | Order Type | When to Use |
|--------|-----------|-------------|
| `BuyMarketAsync()` | Market BUY | Buy at current ask price (immediate execution) |
| `SellMarketAsync()` | Market SELL | Sell at current bid price (immediate execution) |
| `BuyLimitAsync()` | Pending BUY LIMIT | Buy when price drops to specified level |
| `SellLimitAsync()` | Pending SELL LIMIT | Sell when price rises to specified level |
| `BuyStopAsync()` | Pending BUY STOP | Buy when price breaks above specified level |
| `SellStopAsync()` | Pending SELL STOP | Sell when price breaks below specified level |

---

## 💡 Usage Examples

### Example 1: Market Order (BUY/SELL)

```csharp
// ❌ BEFORE (MT5Account) - 9 lines:
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

// ✅ AFTER (MT5Service) - 1 line:
var result = await service.BuyMarketAsync("EURUSD", 0.01,
    stopLoss: 1.0800, takeProfit: 1.0900, comment: "My trade");

// Check result
if (result.RetCode == 10009) // DONE
{
    Console.WriteLine($"✅ Order opened: #{result.Order}");
}
```

**Code reduction: 89%** (9 lines → 1 line)

---

### Example 2: Sell Market Order

```csharp
// ❌ BEFORE (MT5Account):
var request = new OrderSendRequest
{
    Symbol = "GBPUSD",
    Volume = 0.05,
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSell,
    StopLoss = 1.2600,
    TakeProfit = 1.2500
};
var result = await account.OrderSendAsync(request);

// ✅ AFTER (MT5Service):
var result = await service.SellMarketAsync("GBPUSD", 0.05,
    stopLoss: 1.2600, takeProfit: 1.2500);

if (result.RetCode == 10009)
{
    Console.WriteLine($"✅ Sell order: #{result.Order}");
}
```

---

### Example 3: BUY LIMIT (Pending Order)

```csharp
// Scenario: EURUSD is at 1.0900, want to buy at 1.0850 (below current price)

// ❌ BEFORE (MT5Account):
var request = new OrderSendRequest
{
    Symbol = "EURUSD",
    Volume = 0.1,
    Price = 1.0850,
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyLimit,
    StopLoss = 1.0800,
    TakeProfit = 1.0900
};
var result = await account.OrderSendAsync(request);

// ✅ AFTER (MT5Service):
var result = await service.BuyLimitAsync("EURUSD", 0.1, price: 1.0850,
    stopLoss: 1.0800, takeProfit: 1.0900);

if (result.RetCode == 10009)
{
    Console.WriteLine($"✅ BUY LIMIT order placed: #{result.Order}");
    Console.WriteLine($"   Will activate when price drops to 1.0850");
}
```

**Use case:** Expect price to fall before going up (buy the dip).

---

### Example 4: SELL LIMIT (Pending Order)

```csharp
// Scenario: GBPUSD is at 1.2500, want to sell at 1.2550 (above current price)

// ❌ BEFORE (MT5Account):
var request = new OrderSendRequest
{
    Symbol = "GBPUSD",
    Volume = 0.05,
    Price = 1.2550,
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellLimit,
    StopLoss = 1.2600,
    TakeProfit = 1.2500
};
var result = await account.OrderSendAsync(request);

// ✅ AFTER (MT5Service):
var result = await service.SellLimitAsync("GBPUSD", 0.05, price: 1.2550,
    stopLoss: 1.2600, takeProfit: 1.2500);

if (result.RetCode == 10009)
{
    Console.WriteLine($"✅ SELL LIMIT order placed: #{result.Order}");
    Console.WriteLine($"   Will activate when price rises to 1.2550");
}
```

**Use case:** Expect price to rise before going down (sell at resistance).

---

### Example 5: BUY STOP (Breakout Order)

```csharp
// Scenario: XAUUSD is at 2000, want to buy if it breaks above 2010 (resistance)

// ❌ BEFORE (MT5Account):
var request = new OrderSendRequest
{
    Symbol = "XAUUSD",
    Volume = 0.01,
    Price = 2010.0,
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyStop,
    StopLoss = 2000.0,
    TakeProfit = 2030.0
};
var result = await account.OrderSendAsync(request);

// ✅ AFTER (MT5Service):
var result = await service.BuyStopAsync("XAUUSD", 0.01, price: 2010.0,
    stopLoss: 2000.0, takeProfit: 2030.0);

if (result.RetCode == 10009)
{
    Console.WriteLine($"✅ BUY STOP order placed: #{result.Order}");
    Console.WriteLine($"   Will activate on breakout above 2010");
}
```

**Use case:** Breakout strategy - buy when price breaks resistance.

---

### Example 6: SELL STOP (Breakdown Order)

```csharp
// Scenario: USDJPY is at 150.00, want to sell if it breaks below 149.50 (support)

// ❌ BEFORE (MT5Account):
var request = new OrderSendRequest
{
    Symbol = "USDJPY",
    Volume = 0.1,
    Price = 149.50,
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellStop,
    StopLoss = 150.00,
    TakeProfit = 149.00
};
var result = await account.OrderSendAsync(request);

// ✅ AFTER (MT5Service):
var result = await service.SellStopAsync("USDJPY", 0.1, price: 149.50,
    stopLoss: 150.00, takeProfit: 149.00);

if (result.RetCode == 10009)
{
    Console.WriteLine($"✅ SELL STOP order placed: #{result.Order}");
    Console.WriteLine($"   Will activate on breakdown below 149.50");
}
```

**Use case:** Breakdown strategy - sell when price breaks support.

---

### Example 7: Optional Parameters

All parameters except `symbol` and `volume` are optional:

```csharp
// Minimal: No SL/TP
var result1 = await service.BuyMarketAsync("EURUSD", 0.01);

// With SL only
var result2 = await service.BuyMarketAsync("EURUSD", 0.01, stopLoss: 1.0800);

// With TP only
var result3 = await service.BuyMarketAsync("EURUSD", 0.01, takeProfit: 1.0900);

// With SL + TP
var result4 = await service.BuyMarketAsync("EURUSD", 0.01,
    stopLoss: 1.0800, takeProfit: 1.0900);

// With all parameters
var result5 = await service.BuyMarketAsync("EURUSD", 0.01,
    stopLoss: 1.0800,
    takeProfit: 1.0900,
    comment: "My strategy",
    magic: 12345);
```

**Flexibility:** Use only what you need!

---

### Example 8: Complete Trading Flow

```csharp
// Full example: Check conditions → Place order → Verify result

string symbol = "EURUSD";
double volume = 0.01;

// 1. Check account has free margin
double freeMargin = await service.GetFreeMarginAsync();
if (freeMargin < 100.0)
{
    Console.WriteLine("❌ Not enough free margin!");
    return;
}

// 2. Check symbol is available
bool available = await service.IsSymbolAvailableAsync(symbol);
if (!available)
{
    Console.WriteLine($"❌ Symbol {symbol} not available!");
    return;
}

// 3. Get current quote
double bid = await service.GetBidAsync(symbol);
double ask = await service.GetAskAsync(symbol);
Console.WriteLine($"Current quote: Bid={bid:F5}, Ask={ask:F5}");

// 4. Calculate SL/TP
double stopLoss = bid - 0.0050;    // 50 pips below
double takeProfit = bid + 0.0100;  // 100 pips above

// 5. Place BUY order
var result = await service.BuyMarketAsync(symbol, volume,
    stopLoss: stopLoss,
    takeProfit: takeProfit,
    comment: "My Strategy",
    magic: 12345);

// 6. Check result
if (result.RetCode == 10009) // DONE
{
    Console.WriteLine($"✅ Order opened successfully!");
    Console.WriteLine($"   Ticket: #{result.Order}");
    Console.WriteLine($"   Deal: #{result.Deal}");
    Console.WriteLine($"   Price: {result.Price:F5}");
    Console.WriteLine($"   Volume: {result.Volume}");
}
else
{
    Console.WriteLine($"❌ Order failed!");
    Console.WriteLine($"   Return Code: {result.RetCode}");
    Console.WriteLine($"   Comment: {result.Comment}");
}
```

---

## 🔑 Key Benefits

| Aspect | MT5Account (Low-Level) | MT5Service (Convenience) |
|--------|----------------------|-------------------------|
| **Code per order** | 9 lines | 1-3 lines |
| **Request building** | Manual `OrderSendRequest` | ❌ Not needed |
| **Operation enum** | Must specify `Tmt5OrderType...` | ❌ Method name is intent |
| **Readability** | `Operation = Tmt5OrderTypeBuy` | `BuyMarketAsync()` |
| **Named parameters** | ❌ Not available | ✅ Self-documenting |
| **Error-prone** | Can set wrong Operation | ✅ Impossible |

---

## 📊 Method Signatures

All methods follow the same pattern:

```csharp
Task<OrderSendData> BuyMarketAsync(
    string symbol,                    // Required
    double volume,                    // Required
    double? stopLoss = null,          // Optional
    double? takeProfit = null,        // Optional
    string comment = "",              // Optional
    ulong magic = 0,                  // Optional
    DateTime? deadline = null,        // Optional (gRPC timeout)
    CancellationToken ct = default)   // Optional (cancellation)
```

**Pattern applies to all 6 methods!**

---

## 📊 Code Reduction Statistics

| Task | Lines (MT5Account) | Lines (MT5Service) | Reduction |
|------|-------------------|--------------------|-----------|
| Place 1 market order | 9 | 1 | **89%** |
| Place 1 pending order | 9 | 1 | **89%** |
| Place 3 different orders | 27 | 3 | **89%** |
| Complete trading flow | 35 | 25 | **29%** |

**Average reduction: 70%+** for trading operations!

---

## 🎓 Order Type Quick Reference

### Market Orders (Immediate Execution)
- **BuyMarketAsync**: Buy at **current ASK** price
- **SellMarketAsync**: Sell at **current BID** price

### Limit Orders (Price Must Improve)
- **BuyLimitAsync**: Buy below current price (expect dip)
- **SellLimitAsync**: Sell above current price (expect rise)

### Stop Orders (Breakout/Breakdown)
- **BuyStopAsync**: Buy above current price (breakout up)
- **SellStopAsync**: Sell below current price (breakdown)

---

## 🎓 When to Use

### ✅ Use MT5Service when:
- Placing standard market/pending orders
- Want clean, readable code
- Don't need exotic order types
- Building trading strategies
- Rapid development

### ⚠️ Use MT5Account when:
- Need advanced order types (not covered by convenience methods)
- Require full control over OrderSendRequest
- Building complex order management systems

---

## 🔗 See Also

* **[MT5Service Overview](./MT5Service.Overview.md)** - Complete MT5Service improvements
* **[Account Convenience Methods](./Account_Convenience_Methods.md)** - Account shortcuts
* **[Symbol Convenience Methods](./Symbol_Convenience_Methods.md)** - Symbol shortcuts
* **[History Convenience Methods](./History_Convenience_Methods.md)** - History shortcuts
* **[MT5Account Trading](../MT5Account/4.%20Trading_Operattons/Trading_Operations.Overview.md)** - Low-level reference

---

## 💡 Summary

**6 trading methods** that eliminate 70%+ of boilerplate code:

```csharp
// Market orders - 1 line each:
await service.BuyMarketAsync("EURUSD", 0.01, stopLoss: 1.08, takeProfit: 1.09);
await service.SellMarketAsync("GBPUSD", 0.05, stopLoss: 1.26, takeProfit: 1.25);

// Pending orders - crystal clear intent:
await service.BuyLimitAsync("EURUSD", 0.1, price: 1.0850, stopLoss: 1.08, takeProfit: 1.09);
await service.SellStopAsync("USDJPY", 0.1, price: 149.5, stopLoss: 150, takeProfit: 149);
```

**No ceremony, just trading!** 🚀
