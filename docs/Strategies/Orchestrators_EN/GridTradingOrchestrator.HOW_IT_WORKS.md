# HOW GridTradingOrchestrator WORKS - Detailed Analysis

## 🎯 Document Purpose

Show **WHAT** the orchestrator consists of and **HOW EXACTLY** it works at the code, methods and data level.

---

## 📦 What the orchestrator is made of

### 1. Class structure (lines 13-28)

```csharp
public class GridTradingOrchestrator
{
    // SINGLE DEPENDENCY
    private readonly MT5Service _service;

    // 7 CONFIGURABLE PARAMETERS
    public string Symbol { get; set; } = "EURUSD";
    public int GridLevels { get; set; } = 3;
    public int GridSpacingPoints { get; set; } = 20;
    public double VolumePerLevel { get; set; } = 0.01;
    public int StopLossPoints { get; set; } = 50;
    public int TakeProfitPoints { get; set; } = 30;
    public int MaxRunMinutes { get; set; } = 15;

    // DEPENDENCY INJECTION
    public GridTradingOrchestrator(MT5Service service)
    {
        _service = service;  // ← Get MT5Service from outside
    }
}
```

### Dependency visualization

```
GridTradingOrchestrator
  Contains: private readonly MT5Service _service
    ↓
MT5Service
  Contains: private MT5Account _account
    ↓
MT5Account
  Contains: gRPC Client
    ↓
[MT5 Terminal]
```

---

## 🔄 How ExecuteAsync() works - step by step

### Phase 1: Initialization (lines 32-46)

```csharp
public async Task<double> ExecuteAsync(CancellationToken ct = default)
{
     
    // │  STEP 1: Output header                              
    // └─────────────────────────────
    Console.WriteLine("\n+============================================================+");
    Console.WriteLine("|  GRID TRADING ORCHESTRATOR                                |");
    Console.WriteLine("+============================================================+\n");

    
    // │  STEP 2: Get initial balance                        
    // │  Used: MT5Service.GetBalanceAsync()                 
    // │  ↓ Calls: MT5Account.GetBalanceAsync()              
    // │    ↓ Sends gRPC: GetAccountInfoRequest              
    // │      ↓ Receives: AccountInfoData                    
    // │        ↓ Returns: double balance                    
    // └────────────────────────────────────────────────
    var initialBalance = await _service.GetBalanceAsync();
    Console.WriteLine($"  Starting balance: ${initialBalance:F2}");

    
    // │  STEP 3: Get current price                          
    // │  Used: MT5Service.SymbolInfoTickAsync()             
    // │  ↓ Calls: MT5Account.SymbolInfoTickAsync()          
    // │    ↓ Sends gRPC: SymbolInfoTickRequest              
    // │      ↓ Receives: SymbolInfoTickData                 
    // │        ↓ Fields: Bid, Ask, Time, Volume             
    // └────────────────────────────────────────────────
    var tick = await _service.SymbolInfoTickAsync(Symbol);
    Console.WriteLine($"  Current: Bid={tick.Bid:F5}, Ask={tick.Ask:F5}\n");

    // │  STEP 4: Create list to track orders                
    // └────────────────────────────────────────────────
    var placedOrders = new System.Collections.Generic.List<ulong>();
}
```

---

### Phase 2: Placing Buy Limit grid (lines 50-73)

```csharp

// │  LOOP: Placing BUY LIMIT orders BELOW current price    
// └─────────────────────────────────────────────────────
Console.WriteLine($"  Placing {GridLevels} BUY LIMIT levels...");

for (int i = 1; i <= GridLevels; i++)  // i = 1, 2, 3
{
    // │  Calculate offset in points                 
    // │  IMPORTANT: NEGATIVE value!                 
    // └──────────────────────────────────────────
    var pointsBelow = -(i * GridSpacingPoints);

    // Examples with GridSpacingPoints = 20:
    // i=1 → pointsBelow = -(1 × 20) = -20
    // i=2 → pointsBelow = -(2 × 20) = -40
    // i=3 → pointsBelow = -(3 × 20) = -60

    // │  Call MT5Sugar Extension Method             
    // └──────────────────────────────────────────
    var result = await _service.BuyLimitPoints(
        symbol: Symbol,                    // "EURUSD"
        volume: VolumePerLevel,            // 0.01
        priceOffsetPoints: pointsBelow,    // -20, -40, -60
        slPoints: StopLossPoints,          // 50
        tpPoints: TakeProfitPoints,        // 30
        comment: $"Grid-Buy-{i}"           // "Grid-Buy-1"
    );

    // │  Check result                               
    // └──────────────────────────────────────────
    if (result.ReturnedCode == 10009)  // 10009 = TRADE_RETCODE_DONE
    {
        placedOrders.Add(result.Order);  // Save ticket
        Console.WriteLine($"    ✓ Level {i}: #{result.Order} ({pointsBelow} pts below)");
    }
    else
    {
        Console.WriteLine($"    ✗ Level {i} failed: {result.Comment}");
    }
}
```

#### How BuyLimitPoints() works - inside MT5Sugar

```csharp
// MT5Sugar.cs (extension method) — condensed, matching the real implementation
public static async Task<OrderSendData> BuyLimitPoints(
    this MT5Service svc, string symbol, double volume, double priceOffsetPoints,
    double? slPoints = null, double? tpPoints = null, string? comment = null,
    int deviationPoints = 0, int timeoutSec = 15, CancellationToken ct = default)
{
    await svc.EnsureSelected(symbol, timeoutSec, ct);

    // STEP 1: current tick + point size
    var tick  = await svc.SymbolInfoTickAsync(symbol);
    var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint);

    // STEP 2: Buy Limit is ALWAYS below Ask → the offset's ABSOLUTE value is used
    //   Ask = 1.10002, |offset| = 20 → raw = 1.10002 - 20×0.00001 = 1.09982
    var rawPrice = tick.Ask - Math.Abs(priceOffsetPoints) * point;
    var price    = await svc.NormalizePriceAsync(symbol, rawPrice, timeoutSec, ct);

    // STEP 3: SL below / TP above the entry
    double sl = slPoints.HasValue ? price - slPoints.Value * point : 0;  // 1.09932
    double tp = tpPoints.HasValue ? price + tpPoints.Value * point : 0;  // 1.10012

    // STEP 4: build the pending request (operationCode 2 = Buy Limit) and send it
    var req = BuildPendingRequest(symbol, volume, price, sl, tp, comment, deviationPoints, operationCode: 2);
    return await svc.OrderSendAsync(req, Dl(timeoutSec), ct);
}
```

> Note: the sign of `priceOffsetPoints` does **not** flip the direction — `Math.Abs(...)` is applied, and Buy Limit is inherently below the Ask. The grid passes `-20` only as a readable convention.

---

### Phase 3: Placing Sell Limit grid (lines 77-100)

```csharp

// │  LOOP: Placing SELL LIMIT orders ABOVE current price   
// └─────────────────────────────────────────────────────────
Console.WriteLine($"  Placing {GridLevels} SELL LIMIT levels...");

for (int i = 1; i <= GridLevels; i++)
{
    // │  Calculate offset in points                 
    // │  IMPORTANT: POSITIVE value!                 
    // └─────────────────────────────────────────────
    var pointsAbove = i * GridSpacingPoints;

    // Examples with GridSpacingPoints = 20:
    // i=1 → pointsAbove = 1 × 20 = +20
    // i=2 → pointsAbove = 2 × 20 = +40
    // i=3 → pointsAbove = 3 × 20 = +60

    var result = await _service.SellLimitPoints(
        symbol: Symbol,
        volume: VolumePerLevel,
        priceOffsetPoints: pointsAbove,    // +20, +40, +60
        slPoints: StopLossPoints,
        tpPoints: TakeProfitPoints,
        comment: $"Grid-Sell-{i}"
    );

    if (result.ReturnedCode == 10009)
    {
        placedOrders.Add(result.Order);
        Console.WriteLine($"    ✓ Level {i}: #{result.Order} ({pointsAbove} pts above)");
    }
}
```

#### How SellLimitPoints() works

```csharp
// MT5Sugar.cs (extension method) — condensed, matching the real implementation
public static async Task<OrderSendData> SellLimitPoints(
    this MT5Service svc, string symbol, double volume, double priceOffsetPoints,
    double? slPoints = null, double? tpPoints = null, string? comment = null,
    int deviationPoints = 0, int timeoutSec = 15, CancellationToken ct = default)
{
    await svc.EnsureSelected(symbol, timeoutSec, ct);

    var tick  = await svc.SymbolInfoTickAsync(symbol);
    var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint);

    // Sell Limit is ALWAYS above Bid → the offset's ABSOLUTE value is used
    //   Bid = 1.10000, |offset| = 20 → raw = 1.10000 + 20×0.00001 = 1.10020
    var rawPrice = tick.Bid + Math.Abs(priceOffsetPoints) * point;
    var price    = await svc.NormalizePriceAsync(symbol, rawPrice, timeoutSec, ct);

    // For SELL: SL above entry, TP below entry
    double sl = slPoints.HasValue ? price + slPoints.Value * point : 0;  // 1.10070
    double tp = tpPoints.HasValue ? price - tpPoints.Value * point : 0;  // 1.09990

    // operationCode 3 = Sell Limit
    var req = BuildPendingRequest(symbol, volume, price, sl, tp, comment, deviationPoints, operationCode: 3);
    return await svc.OrderSendAsync(req, Dl(timeoutSec), ct);
}
```

#### Result of placing grid

```
After both loops in MT5 Terminal:

PENDING ORDERS (6 orders):
┌────────┬─────────────┬─────────┬─────────┬─────────┬─────────┐
│ Ticket │    Type     │  Price  │   SL    │   TP    │ Comment │
├────────┼─────────────┼─────────┼─────────┼─────────┼─────────┤
│ ...789 │ BUY LIMIT   │ 1.09982 │ 1.09932 │ 1.10012 │ Grid-B-1│
│ ...790 │ BUY LIMIT   │ 1.09962 │ 1.09912 │ 1.09992 │ Grid-B-2│
│ ...791 │ BUY LIMIT   │ 1.09942 │ 1.09892 │ 1.09972 │ Grid-B-3│
│ ...792 │ SELL LIMIT  │ 1.10020 │ 1.10070 │ 1.09990 │ Grid-S-1│
│ ...793 │ SELL LIMIT  │ 1.10040 │ 1.10090 │ 1.10010 │ Grid-S-2│
│ ...794 │ SELL LIMIT  │ 1.10060 │ 1.10110 │ 1.10030 │ Grid-S-3│
└────────┴─────────────┴─────────┴─────────┴─────────┴─────────┘

placedOrders = [789, 790, 791, 792, 793, 794]
```

---

### Phase 4: Monitoring (lines 105-114)

```csharp
Console.WriteLine($"\n  ✓ Grid placed: {placedOrders.Count} pending orders");
Console.WriteLine($"  ⏳ Running for {MaxRunMinutes} minutes...\n");

// │  Calculate end time                                     
// │  MaxRunMinutes = 15                                     
// │  endTime = 12:00 + 15 min = 12:15                       
// └─────────────────────────────────────────────────────────
var endTime = DateTime.UtcNow.AddMinutes(MaxRunMinutes);


// │  MONITORING LOOP: runs until time expires               
// │  or CancellationToken triggers                          
// └─────────────────────────────────────────────────────────
while (DateTime.UtcNow < endTime && !ct.IsCancellationRequested)
{
    
    // │  Wait 5 seconds                                 
    // └─────────────────────────────────────────────────
    await Task.Delay(5000, ct);

    
    // │  Get current balance                            
    // └─────────────────────────────────────────────────
    var currentBalance = await _service.GetBalanceAsync();

    
    // │  Calculate profit/loss                          
    // │  initialBalance = 10000.00                      
    // │  currentBalance = 10012.50                      
    // │  currentProfit = 10012.50 - 10000.00 = +12.50   
    // └─────────────────────────────────────────────────
    var currentProfit = currentBalance - initialBalance;

    Console.WriteLine($"  Current P/L: ${currentProfit:F2}");
}
```

#### What happens during monitoring

```
TIME      ACTION                           BALANCE    P/L
────────  ───────────────────────────────  ─────────  ─────
12:00:00  Grid placed                      10000.00   0.00
12:00:05  Waiting...                       10000.00   0.00
12:00:10  Waiting...                       10000.00   0.00
12:00:15  Price 1.09980 → BUY-1 triggered  10000.00   0.00
12:00:20  Position opened                  10000.00   0.00
12:00:25  Price 1.09995                    10001.50  +1.50
12:00:30  Price 1.10010 → TP triggered     10003.00  +3.00
12:00:35  Position closed                  10003.00  +3.00
...
12:15:00  Time expired → exit loop         10012.50 +12.50
```

---

### Phase 5: Closing (lines 117-128)

```csharp

// │  Time expired - close all remaining orders              
// └─────────────────────────────────────────────────────────
Console.WriteLine("\n  ⏱ Time expired - closing all remaining orders...");


// │  Call MT5Sugar extension method: CloseAll()            
// └─────────────────────────────────────────────────────────
await _service.CloseAll(Symbol);

Console.WriteLine("  ✓ All closed");


// │  Get final balance                                      
// └─────────────────────────────────────────────────────────
var finalBalance = await _service.GetBalanceAsync();


// │  Calculate total profit                                 
// └─────────────────────────────────────────────────────────
var profit = finalBalance - initialBalance;

Console.WriteLine($"\n  Final balance: ${finalBalance:F2}");
Console.WriteLine($"  Total Profit/Loss: ${profit:F2}");


// │  Return profit as result of ExecuteAsync()              
// └─────────────────────────────────────────────────────────
return profit;
```

#### How CloseAll() works

```csharp
// MT5Sugar.cs (extension method) — condensed, matching the real implementation
public static async Task<int> CloseAll(
    this MT5Service svc,
    string? symbol = null,   // null = all symbols
    bool? isBuy = null,      // null = both directions
    int timeoutSec = 30, CancellationToken ct = default)
{
    // STEP 1: fetch everything currently open (positions + pending orders)
    var opened = await svc.OpenedOrdersAsync(
        BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, Dl(timeoutSec), ct);

    int closed = 0;

    // STEP 2: close/cancel each item matching the symbol/direction filter
    //   (positions are closed, pending orders are cancelled) via OrderCloseAsync
    foreach (var item in EnumerateOrders(opened))   // positions + pending
    {
        // ... build the close request and send it ...
        closed++;
    }

    return closed;  // number of items closed/cancelled
}
```

---

## 🎭 Complete Life Cycle

### Execution timeline

```
T=0      START ExecuteAsync()
         │
         ├─► GetBalanceAsync()           → 10000.00
         ├─► SymbolInfoTickAsync()       → Bid:1.10000, Ask:1.10002
         │
T=1s     ├─► LOOP 1: BuyLimitPoints (i=1)
         │   └─► Order 789: BUY LIMIT @ 1.09982
         │
T=2s     ├─► LOOP 1: BuyLimitPoints (i=2)
         │   └─► Order 790: BUY LIMIT @ 1.09962
         │
T=3s     ├─► LOOP 1: BuyLimitPoints (i=3)
         │   └─► Order 791: BUY LIMIT @ 1.09942
         │
T=4s     ├─► LOOP 2: SellLimitPoints (i=1)
         │   └─► Order 792: SELL LIMIT @ 1.10020
         │
T=5s     ├─► LOOP 2: SellLimitPoints (i=2)
         │   └─► Order 793: SELL LIMIT @ 1.10040
         │
T=6s     ├─► LOOP 2: SellLimitPoints (i=3)
         │   └─► Order 794: SELL LIMIT @ 1.10060
         │
T=7s     ├─► MONITORING START
         │   endTime = DateTime.Now + 15 minutes
         │
T=12s    ├─► Task.Delay(5000) → output P/L: $0.00
T=17s    ├─► Task.Delay(5000) → output P/L: $0.00
T=22s    ├─► Task.Delay(5000) → output P/L: $1.50
         │   (Market moved, position opened)
         ...
         ...
T=15min  ├─► MONITORING END (time expired)
         │
         ├─► CloseAll("EURUSD")
         │   ├─► Closed positions: 2
         │   └─► Canceled orders: 4
         │
         ├─► GetBalanceAsync()           → 10012.50
         ├─► profit = 10012.50 - 10000.00 = +12.50
         │
         └─► RETURN profit = 12.50
```

---

## 📊 What the result is made of

### Profit calculation

```
INITIAL BALANCE: $10000.00

TRIGGERED ORDERS:

1. Order 789: BUY LIMIT @ 1.09982
   → Price fell to 1.09980 → opened position BUY
   → Price rose to 1.10012 → TP triggered
   → Profit: (1.10012 - 1.09982) × 100000 × 0.01 = +$3.00

2. Order 792: SELL LIMIT @ 1.10020
   → Price rose to 1.10022 → opened position SELL
   → Price fell to 1.09990 → TP triggered
   → Profit: (1.10020 - 1.09990) × 100000 × 0.01 = +$3.00

NOT TRIGGERED (canceled at CloseAll):
- Order 790: BUY LIMIT @ 1.09962
- Order 791: BUY LIMIT @ 1.09942
- Order 793: SELL LIMIT @ 1.10040
- Order 794: SELL LIMIT @ 1.10060

FINAL RESULT:
- Profit from positions: $3.00 + $3.00 = $6.00
- Commissions/swap: -$0.50
- FINAL BALANCE: $10005.50
- PROFIT = $5.50

return 5.50;
```

---

## 🧩 Components and their roles

### 1. GridTradingOrchestrator

**Role**: Strategy coordinator

**Tasks**:

- Stores parameters (Symbol, GridLevels, etc.)
- Manages life cycle
- Calls MT5Service methods
- Handles errors
- Returns result

### 2. MT5Service

**Role**: Service layer

**Tasks**:

- Provides high-level methods
- Delegates calls to MT5Account
- Contains no business logic

### 3. MT5Sugar (extension methods)

**Role**: Simplifying layer

**Tasks**:

- Converts points to prices
- Calculates SL/TP
- Makes API convenient
- Reduces code amount

### 4. MT5Account

**Role**: gRPC client

**Tasks**:

- Direct communication with MT5 Terminal
- Serialization/deserialization
- Connection management

### 5. MT5 Terminal

**Role**: Executor

**Tasks**:

- Places orders on market
- Monitors execution
- Manages positions

---

## 🔍 Final Dependency Diagram

```

│  USER CODE                                                  
│  var orchestrator = new GridTradingOrchestrator(service);   
│  var profit = await orchestrator.ExecuteAsync();            
└──────────────────────────┬──────────────────────────────────
                           │
                           ▼

│  GridTradingOrchestrator                                    
│    
│  │  - Symbol, GridLevels, GridSpacingPoints, ...           
│  │  - ExecuteAsync()                                       
│  └───────────────────────────────────────────────────────  
└──────────────────────────┬──────────────────────────────────
                           │ _service
                           ▼

│  MT5Service                                                 
│   
│  │  Methods (used here):                                   
│  │  - GetBalanceAsync()                                    
│  │  - SymbolInfoTickAsync()                                
│  │  - OrderSendAsync()      (via Sugar *Points)            
│  │  - OpenedOrdersAsync()   (via Sugar CloseAll)           
│  │  - OrderCloseAsync()     (via Sugar CloseAll)           
│  └───────────────────────────────────────────────────────  
└──────────────────────────┬──────────────────────────────────
                           │ _account
                           ▼

│  MT5Account                                                 
│    
│  │  gRPC Client:                                           
│  │  - OrderSendAsync(OrderSendRequest)                     
│  │  - AccountInfoDoubleAsync(AccountInfoDoubleRequest)     
│  │  - SymbolInfoTickAsync(SymbolInfoTickRequest)           
│  └───────────────────────────────────────────────────────  
└──────────────────────────┬──────────────────────────────────
                           │ gRPC
                           ▼
                    
                    │ MT5 Terminal 
                    └──────────────


│  MT5Sugar (static extension methods)                        
│    
│  │  Extension Methods on MT5Service:                       
│  │  - BuyLimitPoints(priceOffsetPoints, slPoints, ...)     
│  │  - SellLimitPoints(priceOffsetPoints, tpPoints, ...)    
│  │  - CloseAll(symbol)                                     
│  │  - BuyMarketByRisk(riskMoney, stopPoints, ...)          
│  │                                                         
│  │  Role: Convert points → prices                          
│  └───────────────────────────────────────────────────────  
└─────────────────────────────────────────────────────────────
         │                          │
         └──────────┬───────────────┘
                    │ Calls low-level methods
                    ▼
              [MT5Service methods]
```

---

## 🎯 Summary

**GridTradingOrchestrator is made of**:

1. **1 dependency**: `MT5Service _service` (via DI)
2. **7 parameters**: Symbol, GridLevels, GridSpacingPoints, Volume, SL, TP, MaxRunMinutes
3. **3 MT5Sugar methods**: `BuyLimitPoints`, `SellLimitPoints`, `CloseAll`
4. **2 MT5Service methods used directly**: `GetBalanceAsync`, `SymbolInfoTickAsync` (the Sugar methods call `OrderSendAsync` / `OpenedOrdersAsync` underneath)
5. **gRPC protocol**: Communication with MT5 Terminal

**Works through**:

- 2 loops for placing orders (Buy + Sell)
- 1 monitoring loop with Task.Delay(5000)
- 1 final close of all orders
- Error handling via try-catch

**Returns**:

- `double profit` - difference between final and initial balance
