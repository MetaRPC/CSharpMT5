# Validate Order (`ValidateOrderAsync`)

> **⭐ CRITICAL METHOD:** Pre-flight validation for orders - check BEFORE sending to avoid rejections!

**API Information:**

* **Extension method:** `MT5Service.ValidateOrderAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Region:** [13] ORDER VALIDATION ⭐
* **Underlying calls:** `OrderCheckAsync()`

---

## Method Signature

```csharp
public static Task<OrderCheckData> ValidateOrderAsync(
    this MT5Service svc,
    OrderCheckRequest request,
    int timeoutSec = 15,
    CancellationToken ct = default)
    => svc.OrderCheckAsync(request, Dl(timeoutSec), ct);
```

---

## 🔽 Input

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance (extension method) |
| `request` | `OrderCheckRequest` | Request containing `MqlTradeRequest` with order details |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<OrderCheckData>` | Validation result with error codes and details |

**Key fields in `OrderCheckData.MqlTradeCheckResult`:**

- `ReturnedCode` - Validation result code (0 = valid, non-zero = error)
- `Comment` - Human-readable error message
- `Balance` - Account balance after order (if valid)
- `Equity` - Account equity after order (if valid)
- `Margin` - Required margin for this order
- `MarginFree` - Free margin after order
- `MarginLevel` - Margin level percentage after order

---

## 💬 Just the essentials

* **What it is:** Pre-flight check for orders - validates symbol, volume, margin, stop levels BEFORE sending to broker.
* **Why you need it:** Catch errors before order submission. Saves time and prevents broker rejections.
* **Returns code 0** if valid, non-zero with error message if invalid.

---

## 🎯 Purpose

Use it for:

* **Pre-flight validation** - Check order before OrderSendAsync()
* **Margin verification** - Ensure sufficient margin before order
* **Symbol/volume checks** - Validate symbol availability and lot constraints
* **Stop level validation** - Check SL/TP are within broker limits
* **Prevent rejections** - Catch errors client-side before broker sees them

---

## 🔧 Under the Hood

```csharp
// Simple wrapper with timeout handling
return await svc.OrderCheckAsync(request, Dl(timeoutSec), ct);

// OrderCheckAsync sends request to MT5 terminal which:
// 1. Validates symbol exists and is tradable
// 2. Checks volume against min/max/step constraints
// 3. Verifies sufficient margin
// 4. Validates stop levels (SL/TP distances)
// 5. Checks account permissions (hedging, trading allowed, etc.)
// 6. Returns detailed validation result
```

**What it improves:**

* **Clearer naming** - "Validate" vs generic "Check"
* **Timeout parameter** - Easier to specify timeout
* **Catches errors early** - Before broker rejection

---

## 🔗 Usage Examples

### Example 1: Basic Order Validation

```csharp
var tradeReq = new MqlTradeRequest
{
    Symbol = "EURUSD",
    Volume = 0.01,
    Type = ENUM_ORDER_TYPE.OrderTypeBuy,
    Price = 0,  // Market price
    StopLoss = 0,
    TakeProfit = 0
};

var checkReq = new OrderCheckRequest { MqlTradeRequest = tradeReq };
var result = await svc.ValidateOrderAsync(checkReq);

if (result.MqlTradeCheckResult.ReturnedCode == 0)
{
    Console.WriteLine("✅ Order is valid - safe to send");
    Console.WriteLine($"   Required margin: ${result.MqlTradeCheckResult.Margin:F2}");
    Console.WriteLine($"   Free margin after: ${result.MqlTradeCheckResult.MarginFree:F2}");

    // Now send the order
    await svc.OrderSendAsync(new OrderSendRequest { MqlTradeRequest = tradeReq });
}
else
{
    Console.WriteLine($"❌ Order invalid: {result.MqlTradeCheckResult.Comment}");
    Console.WriteLine($"   Error code: {result.MqlTradeCheckResult.ReturnedCode}");
}
```

---

### Example 2: Validate Before Large Order

```csharp
public async Task<bool> ValidateAndPlaceLargeOrder(
    MT5Service svc,
    string symbol,
    double volume)
{
    // Build order request
    var tradeReq = new MqlTradeRequest
    {
        Symbol = symbol,
        Volume = volume,
        Type = ENUM_ORDER_TYPE.OrderTypeBuy,
        Price = 0
    };

    // Validate first
    var checkReq = new OrderCheckRequest { MqlTradeRequest = tradeReq };
    var validation = await svc.ValidateOrderAsync(checkReq);

    if (validation.MqlTradeCheckResult.ReturnedCode != 0)
    {
        Console.WriteLine($"❌ Cannot place {volume} lots:");
        Console.WriteLine($"   {validation.MqlTradeCheckResult.Comment}");

        // Check if margin issue
        if (validation.MqlTradeCheckResult.Comment.Contains("margin"))
        {
            Console.WriteLine($"   Required: ${validation.MqlTradeCheckResult.Margin:F2}");
            Console.WriteLine($"   Available: ${validation.MqlTradeCheckResult.MarginFree:F2}");
            Console.WriteLine("   Consider reducing volume or adding funds");
        }

        return false;
    }

    // Validation passed - send order
    var sendReq = new OrderSendRequest { MqlTradeRequest = tradeReq };
    var result = await svc.OrderSendAsync(sendReq);

    Console.WriteLine($"✅ Order placed: Ticket #{result.Order}");
    return true;
}

// Usage:
bool success = await ValidateAndPlaceLargeOrder(svc, "EURUSD", volume: 10.0);
```

---

### Example 3: Validate Multiple Symbols

```csharp
var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY", "XAUUSD" };
double volume = 0.10;

Console.WriteLine($"Validating {volume} lots for multiple symbols:\n");

foreach (var symbol in symbols)
{
    var tradeReq = new MqlTradeRequest
    {
        Symbol = symbol,
        Volume = volume,
        Type = ENUM_ORDER_TYPE.OrderTypeBuy,
        Price = 0
    };

    var checkReq = new OrderCheckRequest { MqlTradeRequest = tradeReq };
    var result = await svc.ValidateOrderAsync(checkReq);

    if (result.MqlTradeCheckResult.ReturnedCode == 0)
    {
        Console.WriteLine($"✅ {symbol,-8} Margin: ${result.MqlTradeCheckResult.Margin,8:F2}");
    }
    else
    {
        Console.WriteLine($"❌ {symbol,-8} {result.MqlTradeCheckResult.Comment}");
    }
}

// Output:
// Validating 0.1 lots for multiple symbols:
//
// ✅ EURUSD   Margin: $  108.50
// ✅ GBPUSD   Margin: $  127.30
// ✅ USDJPY   Margin: $   99.20
// ❌ XAUUSD   Not enough money
```

---

### Example 4: Validate with SL/TP

```csharp
var tick = await svc.SymbolInfoTickAsync("EURUSD");
double point = await svc.GetPointAsync("EURUSD");

var tradeReq = new MqlTradeRequest
{
    Symbol = "EURUSD",
    Volume = 0.01,
    Type = ENUM_ORDER_TYPE.OrderTypeBuy,
    Price = 0,  // Market
    StopLoss = tick.Bid - 50 * point,   // 50 points below
    TakeProfit = tick.Ask + 100 * point  // 100 points above
};

var checkReq = new OrderCheckRequest { MqlTradeRequest = tradeReq };
var result = await svc.ValidateOrderAsync(checkReq);

if (result.MqlTradeCheckResult.ReturnedCode == 0)
{
    Console.WriteLine("✅ Order with SL/TP is valid");
    Console.WriteLine($"   SL: {tradeReq.StopLoss:F5}");
    Console.WriteLine($"   TP: {tradeReq.TakeProfit:F5}");
}
else
{
    Console.WriteLine($"❌ Invalid SL/TP: {result.MqlTradeCheckResult.Comment}");
    // Common: "Invalid stops" if too close to current price
}
```

---

### Example 5: Check Margin Level

```csharp
var tradeReq = new MqlTradeRequest
{
    Symbol = "EURUSD",
    Volume = 5.0,  // Large volume
    Type = ENUM_ORDER_TYPE.OrderTypeBuy,
    Price = 0
};

var checkReq = new OrderCheckRequest { MqlTradeRequest = tradeReq };
var result = await svc.ValidateOrderAsync(checkReq);

if (result.MqlTradeCheckResult.ReturnedCode == 0)
{
    double marginLevel = result.MqlTradeCheckResult.MarginLevel;

    Console.WriteLine($"After order:");
    Console.WriteLine($"  Balance: ${result.MqlTradeCheckResult.Balance:F2}");
    Console.WriteLine($"  Equity: ${result.MqlTradeCheckResult.Equity:F2}");
    Console.WriteLine($"  Margin Level: {marginLevel:F2}%");

    if (marginLevel < 200)
    {
        Console.WriteLine("⚠️ WARNING: Low margin level after order!");
        Console.WriteLine("   Consider reducing volume");
    }
    else
    {
        Console.WriteLine("✅ Healthy margin level - safe to proceed");
    }
}
```

---

### Example 6: Validate Pending Order

```csharp
var tick = await svc.SymbolInfoTickAsync("EURUSD");
double point = await svc.GetPointAsync("EURUSD");

// Buy Limit 50 points below current Ask
double entryPrice = tick.Ask - 50 * point;

var tradeReq = new MqlTradeRequest
{
    Symbol = "EURUSD",
    Volume = 0.05,
    Type = ENUM_ORDER_TYPE.OrderTypeBuyLimit,
    Price = entryPrice,
    StopLoss = entryPrice - 30 * point,
    TakeProfit = entryPrice + 90 * point
};

var checkReq = new OrderCheckRequest { MqlTradeRequest = tradeReq };
var result = await svc.ValidateOrderAsync(checkReq);

if (result.MqlTradeCheckResult.ReturnedCode == 0)
{
    Console.WriteLine($"✅ Buy Limit valid at {entryPrice:F5}");
    await svc.OrderSendAsync(new OrderSendRequest { MqlTradeRequest = tradeReq });
}
else
{
    Console.WriteLine($"❌ Pending order invalid: {result.MqlTradeCheckResult.Comment}");
}
```

---

### Example 7: Batch Validation

```csharp
public async Task<List<string>> ValidateBatchOrders(
    MT5Service svc,
    List<MqlTradeRequest> orders)
{
    var validSymbols = new List<string>();

    foreach (var order in orders)
    {
        var checkReq = new OrderCheckRequest { MqlTradeRequest = order };
        var result = await svc.ValidateOrderAsync(checkReq);

        if (result.MqlTradeCheckResult.ReturnedCode == 0)
        {
            validSymbols.Add(order.Symbol);
            Console.WriteLine($"✅ {order.Symbol}: Valid");
        }
        else
        {
            Console.WriteLine($"❌ {order.Symbol}: {result.MqlTradeCheckResult.Comment}");
        }
    }

    Console.WriteLine($"\n{validSymbols.Count}/{orders.Count} orders are valid");
    return validSymbols;
}

// Usage:
var orders = new List<MqlTradeRequest>
{
    new() { Symbol = "EURUSD", Volume = 0.1, Type = ENUM_ORDER_TYPE.OrderTypeBuy },
    new() { Symbol = "GBPUSD", Volume = 0.1, Type = ENUM_ORDER_TYPE.OrderTypeBuy },
    new() { Symbol = "USDJPY", Volume = 0.1, Type = ENUM_ORDER_TYPE.OrderTypeBuy }
};

var valid = await ValidateBatchOrders(svc, orders);
```

---

### Example 8: Error Code Analysis

```csharp
var tradeReq = new MqlTradeRequest
{
    Symbol = "EURUSD",
    Volume = 0.01,
    Type = ENUM_ORDER_TYPE.OrderTypeBuy,
    Price = 0
};

var checkReq = new OrderCheckRequest { MqlTradeRequest = tradeReq };
var result = await svc.ValidateOrderAsync(checkReq);

int code = result.MqlTradeCheckResult.ReturnedCode;

if (code == 0)
{
    Console.WriteLine("✅ Valid");
}
else
{
    Console.WriteLine($"❌ Error code: {code}");

    // Common error codes:
    switch (code)
    {
        case 10004:
            Console.WriteLine("   Requote");
            break;
        case 10006:
            Console.WriteLine("   Rejected");
            break;
        case 10014:
            Console.WriteLine("   Invalid volume");
            break;
        case 10019:
            Console.WriteLine("   Not enough money");
            break;
        case 10021:
            Console.WriteLine("   Market closed");
            break;
        default:
            Console.WriteLine($"   {result.MqlTradeCheckResult.Comment}");
            break;
    }
}
```

---

### Example 9: Retry with Reduced Volume

```csharp
public async Task<double> FindMaxValidVolume(
    MT5Service svc,
    string symbol,
    double startVolume)
{
    double volume = startVolume;
    double minVolume = 0.01;

    while (volume >= minVolume)
    {
        var tradeReq = new MqlTradeRequest
        {
            Symbol = symbol,
            Volume = volume,
            Type = ENUM_ORDER_TYPE.OrderTypeBuy,
            Price = 0
        };

        var checkReq = new OrderCheckRequest { MqlTradeRequest = tradeReq };
        var result = await svc.ValidateOrderAsync(checkReq);

        if (result.MqlTradeCheckResult.ReturnedCode == 0)
        {
            Console.WriteLine($"✅ Maximum valid volume: {volume} lots");
            return volume;
        }

        // Reduce by 10%
        volume *= 0.9;
        volume = Math.Round(volume, 2);

        Console.WriteLine($"   Trying {volume} lots...");
    }

    Console.WriteLine("❌ No valid volume found");
    return 0;
}

// Usage:
double maxVolume = await FindMaxValidVolume(svc, "EURUSD", startVolume: 10.0);
if (maxVolume > 0)
{
    await svc.BuyMarketAsync("EURUSD", maxVolume);
}
```

---

### Example 10: Compare Multiple Order Types

```csharp
var orderTypes = new[]
{
    ENUM_ORDER_TYPE.OrderTypeBuy,
    ENUM_ORDER_TYPE.OrderTypeSell,
    ENUM_ORDER_TYPE.OrderTypeBuyLimit,
    ENUM_ORDER_TYPE.OrderTypeSellLimit
};

Console.WriteLine("Comparing margin requirements:\n");

foreach (var type in orderTypes)
{
    var tradeReq = new MqlTradeRequest
    {
        Symbol = "EURUSD",
        Volume = 1.0,
        Type = type,
        Price = type == ENUM_ORDER_TYPE.OrderTypeBuy || type == ENUM_ORDER_TYPE.OrderTypeSell
            ? 0
            : 1.08500  // Pending order price
    };

    var checkReq = new OrderCheckRequest { MqlTradeRequest = tradeReq };
    var result = await svc.ValidateOrderAsync(checkReq);

    if (result.MqlTradeCheckResult.ReturnedCode == 0)
    {
        Console.WriteLine($"{type,-20} Margin: ${result.MqlTradeCheckResult.Margin:F2}");
    }
}

// Output:
// Comparing margin requirements:
//
// OrderTypeBuy         Margin: $1085.00
// OrderTypeSell        Margin: $1085.00
// OrderTypeBuyLimit    Margin: $1085.00
// OrderTypeSellLimit   Margin: $1085.00
```

---

## 🔗 Related Methods

**📦 Methods used internally:**

* `OrderCheckAsync()` - Low-level validation RPC call

**🍬 Related Sugar methods:**

* `CheckMarginAvailabilityAsync()` - Simpler margin-only check
* `CalculateBuyMarginAsync()` - Calculate margin for BUY
* `CalculateSellMarginAsync()` - Calculate margin for SELL

**📊 When to use:**

* Use `ValidateOrderAsync()` for full validation (symbol, volume, margin, stops)
* Use `CheckMarginAvailabilityAsync()` for quick margin-only check
* Use `CalculateBuyMarginAsync()` / `CalculateSellMarginAsync()` for margin estimation only

---

## ⚠️ Common Pitfalls

1. **Not checking return code:**
   ```csharp
   // ❌ WRONG: Ignoring validation result
   await svc.ValidateOrderAsync(checkReq);
   await svc.OrderSendAsync(sendReq);  // May still fail!

   // ✅ CORRECT: Check return code
   var result = await svc.ValidateOrderAsync(checkReq);
   if (result.MqlTradeCheckResult.ReturnedCode == 0)
   {
       await svc.OrderSendAsync(sendReq);
   }
   ```

2. **Assuming validation guarantees execution:**
   ```csharp
   // ⚠️ Validation passed, but order can still fail due to:
   // - Price movement between validation and execution
   // - Margin changes from other positions
   // - Market closure
   // - Broker rejection

   var result = await svc.ValidateOrderAsync(checkReq);
   if (result.MqlTradeCheckResult.ReturnedCode == 0)
   {
       // Still handle OrderSendAsync errors!
       try
       {
           await svc.OrderSendAsync(sendReq);
       }
       catch (Exception ex)
       {
           Console.WriteLine($"Order failed despite validation: {ex.Message}");
       }
   }
   ```

3. **Forgetting Price parameter for pending orders:**
   ```csharp
   // ❌ WRONG: Price = 0 for pending order
   var tradeReq = new MqlTradeRequest
   {
       Type = ENUM_ORDER_TYPE.OrderTypeBuyLimit,
       Price = 0  // Invalid for pending orders!
   };

   // ✅ CORRECT: Specify entry price
   var tradeReq = new MqlTradeRequest
   {
       Type = ENUM_ORDER_TYPE.OrderTypeBuyLimit,
       Price = 1.08500  // Must specify for pending
   };
   ```

4. **Not normalizing prices:**
   ```csharp
   // ⚠️ Prices should be normalized to tick size
   double slPrice = 1.085234567;  // Too many decimals

   // ✅ BETTER: Normalize first
   double slPrice = await svc.NormalizePriceAsync("EURUSD", 1.085234567);
   ```

---

## 💡 Summary

**ValidateOrderAsync** provides pre-flight order validation:

* ✅ Validates symbol, volume, margin, stops
* ✅ Returns detailed error codes and messages
* ✅ Shows account state after order (balance, equity, margin level)
* ✅ Prevents broker rejections by catching errors early
* ⭐ **Use before ALL orders to ensure they'll be accepted**

```csharp
// Professional pattern:
var checkReq = new OrderCheckRequest { MqlTradeRequest = tradeReq };
var validation = await svc.ValidateOrderAsync(checkReq);

if (validation.MqlTradeCheckResult.ReturnedCode == 0)
{
    await svc.OrderSendAsync(new OrderSendRequest { MqlTradeRequest = tradeReq });
}
else
{
    Console.WriteLine($"❌ {validation.MqlTradeCheckResult.Comment}");
}
```

**Validate first, trade safely!** 🛡️
