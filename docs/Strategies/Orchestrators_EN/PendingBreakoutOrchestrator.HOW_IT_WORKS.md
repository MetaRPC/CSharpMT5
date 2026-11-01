# HOW PendingBreakoutOrchestrator WORKS - Detailed Analysis

## 🎯 Document Purpose

Show **WHAT** the orchestrator consists of and **HOW EXACTLY** it works at the code, methods and data level. Special attention is paid to the breakout detection logic and OCO mechanism.

---

## 📦 What the orchestrator is made of

### 1. Class structure (lines 13-27)

```csharp
public class PendingBreakoutOrchestrator
{
    
    // │  SINGLE DEPENDENCY                      
    // └─────────────────────────────────────────
    private readonly MT5Service _service;

    
    // │  6 CONFIGURABLE PARAMETERS              
    // └─────────────────────────────────────────
    public string Symbol { get; set; } = "EURUSD";
    public int BreakoutDistancePoints { get; set; } = 25;
    public int StopLossPoints { get; set; } = 15;
    public int TakeProfitPoints { get; set; } = 30;
    public double Volume { get; set; } = 0.01;
    public int MaxWaitMinutes { get; set; } = 30;

    
    // │  DEPENDENCY INJECTION                   
    // └─────────────────────────────────────────
    public PendingBreakoutOrchestrator(MT5Service service)
    {
        _service = service;
    }
}
```

### Dependency visualization

```

│        PendingBreakoutOrchestrator                         
│    
│  │  private readonly MT5Service _service                  
│  └──────────────────────────┬───────────────────────────  
└─────────────────────────┼──────────────────────────────────
                          │
                          ▼
        
        │         MT5Service                  
        │    
        │  │  private MT5Account _account    
        │  └──────────────┬────────────────  
        └─────────────────┼────────────────────
                          │
                          ▼
                
                │      MT5Account         
                │   
                │  │  gRPC Client        
                │  └───────────────────  
                └─────────────────────────
                          │
                          ▼
                    [MT5 Terminal]
```

---

## 🔄 How ExecuteAsync() works - step by step

### Phase 1: Initialization (lines 31-46)

```csharp
public async Task<double> ExecuteAsync(CancellationToken ct = default)
{
    
    // │  STEP 1: Output header                              
    // └─────────────────────────────────────────────────────
    Console.WriteLine("\n+============================================================+");
    Console.WriteLine("|  PENDING BREAKOUT ORCHESTRATOR                            |");
    Console.WriteLine("+============================================================+\n");

    
    // │  STEP 2: Get initial balance                        
    // └─────────────────────────────────────────────────────
    var initialBalance = await _service.GetBalanceAsync();
    Console.WriteLine($"  Starting balance: ${initialBalance:F2}");

    
    // │  STEP 3: Get current price                          
    // └─────────────────────────────────────────────────────
    var tick = await _service.SymbolInfoTickAsync(Symbol);
    Console.WriteLine($"  Current: Bid={tick.Bid:F5}, Ask={tick.Ask:F5}\n");
}
```

---

### Phase 2: Placing BuyStop (lines 48-65)

```csharp

// │  PLACING BUY STOP (upward breakout)                     
// └─────────────────────────────────────────────────────────
Console.WriteLine("  Placing BUY STOP order...");

var buyStopResult = await _service.BuyStopPoints(
    symbol: Symbol,                         // "EURUSD"
    volume: Volume,                         // 0.01
    priceOffsetPoints: BreakoutDistancePoints,  // +25 (POSITIVE!)
    slPoints: StopLossPoints,               // 15
    tpPoints: TakeProfitPoints,             // 30
    comment: "Breakout-Buy"
);


// │  RESULT CHECK                                           
// └─────────────────────────────────────────────────────────
if (buyStopResult.ReturnedCode != 10009)
{
    Console.WriteLine($"  ✗ BUY STOP failed: {buyStopResult.Comment}\n");
    return 0;  // ← EMERGENCY EXIT
}

Console.WriteLine($"  ✓ BUY STOP placed: #{buyStopResult.Order}\n");
```

#### How BuyStopPoints() works internally

```csharp
// MT5Sugar.cs (extension method) — condensed, matching the real implementation
public static async Task<OrderSendData> BuyStopPoints(
    this MT5Service svc, string symbol, double volume, double priceOffsetPoints,
    double? slPoints = null, double? tpPoints = null, string? comment = null,
    int deviationPoints = 0, int timeoutSec = 15, CancellationToken ct = default)
{
    await svc.EnsureSelected(symbol, timeoutSec, ct);

    var tick  = await svc.SymbolInfoTickAsync(symbol);
    var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint);

    // Buy Stop is ALWAYS above Ask → the offset's absolute value is used
    //   Ask = 1.10002, |offset| = 25 → raw = 1.10002 + 25×0.00001 = 1.10027
    var rawPrice = tick.Ask + Math.Abs(priceOffsetPoints) * point;
    var price    = await svc.NormalizePriceAsync(symbol, rawPrice, timeoutSec, ct);

    double sl = slPoints.HasValue ? price - slPoints.Value * point : 0;  // 1.10012
    double tp = tpPoints.HasValue ? price + tpPoints.Value * point : 0;  // 1.10057

    // operationCode for Buy Stop (4)
    var req = BuildPendingRequest(symbol, volume, price, sl, tp, comment, deviationPoints, operationCode: 4);
    return await svc.OrderSendAsync(req, Dl(timeoutSec), ct);
}
```

#### Complete call chain for BuyStop

```

│  USER CODE (PendingBreakoutOrchestrator.cs:50)                 
│  await _service.BuyStopPoints(                                 
│      symbol: "EURUSD",                                         
│      volume: 0.01,                                             
│      priceOffsetPoints: +25,  ← POINTS (POSITIVE)              
│      slPoints: 15,                                             
│      tpPoints: 30,                                             
│      comment: "Breakout-Buy"                                   
│  )                                                             
└──────────────────────────┬─────────────────────────────────────
                           │
                           ▼

│  MT5Sugar.BuyStopPoints() (extension method)                   
│  
│  │ 1. Get Ask: 1.10002                                        
│  │ 2. Get point: 0.00001                                     
│  │ 3. Calculate price: 1.10002 + (25 × 0.00001) = 1.10027    
│  │ 4. Calculate SL: 1.10027 - (15 × 0.00001) = 1.10012      
│  │ 5. Calculate TP: 1.10027 + (30 × 0.00001) = 1.10057      
│  └────────────────────────────────────────────────────────── 
│  var req = BuildPendingRequest(price: 1.10027,                 
│      sl: 1.10012, tp: 1.10057, operationCode: 4 /*BuyStop*/)   
│  await svc.OrderSendAsync(req)                                 
└──────────────────────────┬─────────────────────────────────────
                           │
                           ▼

│  MT5Service.OrderSendAsync()                                   
│  return await _account.OrderSendAsync(request)                 
└──────────────────────────┬─────────────────────────────────────
                           │
                           ▼

│  MT5Account.OrderSendAsync(OrderSendRequest)                   
│   
│  │ var request = new OrderSendRequest {                       
│  │     Symbol = "EURUSD",                                    
│  │     Volume = 0.01,                                         
│  │     Operation = Tmt5OrderTypeBuyStop,  // = 4             
│  │     Price = 1.10027,                                     
│  │     StopLoss = 1.10012,                                   
│  │     TakeProfit = 1.10057,                                 
│  │     Comment = "Breakout-Buy"                              
│  │ }                                                         
│  └────────────────────────────────────────────────────────── 
│  var response = await TradeClient.OrderSend(request, ...);     
└──────────────────────────┬─────────────────────────────────────
                           │
                           ▼
                    
                    │  gRPC NETWORK
                    └──────┬───────
                           │
                           ▼
                  
                  │   MT5 Terminal     
                  │   
                  │  │ Places       
                  │  │ BUY STOP       
                  │  │ @ 1.10027      
                  │  │ SL: 1.10012    
                  │  │ TP: 1.10057   
                  │  │ Ticket:        
                  │  │  123456789     
                  │  └────────────── 
                  └────────┬───────────
                           │
                           ▼
                    
                    │  RESPONSE    
                    │ OrderSendData
                    │ {            
                    │   ReturnedCode
                    │   = 10009,   
                    │   Order =    
                    │   123456789  
                    │ }            
                    └──────────────
```

---

### Phase 3: Placing SellStop (lines 67-86)

```csharp

// │  PLACING SELL STOP (downward breakout)                  
// └─────────────────────────────────────────────────────────
Console.WriteLine("  Placing SELL STOP order...");

var sellStopResult = await _service.SellStopPoints(
    symbol: Symbol,
    volume: Volume,
    priceOffsetPoints: -BreakoutDistancePoints,  // -25 (NEGATIVE!)
    slPoints: StopLossPoints,
    tpPoints: TakeProfitPoints,
    comment: "Breakout-Sell"
);


// │  CRITICALLY IMPORTANT CHECK                             
// └─────────────────────────────────────────────────────────
if (sellStopResult.ReturnedCode != 10009)
{
    Console.WriteLine($"  ✗ SELL STOP failed: {sellStopResult.Comment}");
    Console.WriteLine("  Canceling BUY STOP...");

    
    // │  IMPORTANT: Cancel first order!                 
    // │  Otherwise hanging BuyStop remains without pair 
    // └─────────────────────────────────────────────────
    await _service.CloseByTicket(buyStopResult.Order);
    return 0;
}

Console.WriteLine($"  ✓ SELL STOP placed: #{sellStopResult.Order}\n");
```

#### How SellStopPoints() works internally

```csharp
// MT5Sugar.cs (extension method) — condensed, matching the real implementation
public static async Task<OrderSendData> SellStopPoints(
    this MT5Service svc, string symbol, double volume, double priceOffsetPoints,
    double? slPoints = null, double? tpPoints = null, string? comment = null,
    int deviationPoints = 0, int timeoutSec = 15, CancellationToken ct = default)
{
    await svc.EnsureSelected(symbol, timeoutSec, ct);

    var tick  = await svc.SymbolInfoTickAsync(symbol);
    var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint);

    // Sell Stop is ALWAYS below Bid → the offset's absolute value is used
    //   Bid = 1.10000, |offset| = 25 → raw = 1.10000 - 25×0.00001 = 1.09975
    var rawPrice = tick.Bid - Math.Abs(priceOffsetPoints) * point;
    var price    = await svc.NormalizePriceAsync(symbol, rawPrice, timeoutSec, ct);

    // For SELL: SL above entry, TP below entry
    double sl = slPoints.HasValue ? price + slPoints.Value * point : 0;  // 1.09990
    double tp = tpPoints.HasValue ? price - tpPoints.Value * point : 0;  // 1.09945

    // operationCode for Sell Stop (5)
    var req = BuildPendingRequest(symbol, volume, price, sl, tp, comment, deviationPoints, operationCode: 5);
    return await svc.OrderSendAsync(req, Dl(timeoutSec), ct);
}
```

#### Result of placing both orders

```
After successfully placing both orders in MT5 Terminal:

PENDING ORDERS (2 orders):
┌────────┬─────────────┬─────────┬─────────┬─────────┬──────────────┐
│ Ticket │    Type     │  Price  │   SL    │   TP    │   Comment    │
├────────┼─────────────┼─────────┼─────────┼─────────┼──────────────┤
│ ...789 │ BUY STOP    │ 1.10027 │ 1.10012 │ 1.10057 │ Breakout-Buy │
│ ...790 │ SELL STOP   │ 1.09975 │ 1.09990 │ 1.09945 │ Breakout-Sell│
└────────┴─────────────┴─────────┴─────────┴─────────┴──────────────┘

VISUALIZATION:
          ↑ Price rises
          │
  1.10057 ├─── TP for BuyStop
  1.10027 ├─── BUY STOP (upward breakout)
  1.10012 ├─── SL for BuyStop
          │
  1.10002 ├─── Current Ask
  1.10000 ├─── Current Bid
          │
  1.09990 ├─── SL for SellStop
  1.09975 ├─── SELL STOP (downward breakout)
  1.09945 ├─── TP for SellStop
          │
          ↓ Price falls
```

---

### Phase 4: BREAKOUT MONITORING (lines 89-129)

This is **the most important part** of the orchestrator - the breakout detection logic.

```csharp
Console.WriteLine($"  ⏳ Waiting up to {MaxWaitMinutes} minutes for breakout...\n");


// │  MONITORING INITIALIZATION                              
// └─────────────────────────────────────────────────────────
var startTime = DateTime.UtcNow;
var timeout = TimeSpan.FromMinutes(MaxWaitMinutes);

// Variables to track which order executed
ulong? executedOrder = null;   // Ticket of executed order
ulong? cancelOrder = null;     // Ticket of order to cancel


// │  MAIN MONITORING LOOP                                   
// └─────────────────────────────────────────────────────────
while (DateTime.UtcNow - startTime < timeout && !ct.IsCancellationRequested)
{
    
    // │  Wait 3 seconds before next check              
    // └─────────────────────────────────────────────────
    await Task.Delay(3000, ct);

    
    // │  STEP 1: Get list of PENDING orders             
    // │  IMPORTANT: This is only PENDING orders!        
    // │  Executed orders become positions              
    // └─────────────────────────────────────────────────
    var tickets = await _service.OpenedOrdersTicketsAsync();

    bool buyStillPending = false;
    bool sellStillPending = false;

    
    // │  STEP 2: Check if our orders are in the list    
    // └─────────────────────────────────────────────────
    foreach (var ticket in tickets.OpenedOrdersTickets)
    {
        if (ticket == (long)buyStopResult.Order) buyStillPending = true;
        if (ticket == (long)sellStopResult.Order) sellStillPending = true;
    }

    
    // │  STEP 3: BREAKOUT DETECTION (4 scenarios)           
    // └─────────────────────────────────────────────────────

    // ═══════════════════════════════════════════════════════
    // SCENARIO 1: UPWARD BREAKOUT
    // ═══════════════════════════════════════════════════════
    if (!buyStillPending && sellStillPending)
    {
        // BuyStop DISAPPEARED from list → executed!
        // SellStop STILL in list → not executed

        Console.WriteLine("  🚀 BUY STOP EXECUTED! Upward breakout!");
        executedOrder = buyStopResult.Order;
        cancelOrder = sellStopResult.Order;  // ← Need to cancel
        break;  // Exit loop
    }

    // ═══════════════════════════════════════════════════════
    // SCENARIO 2: DOWNWARD BREAKOUT
    // ═══════════════════════════════════════════════════════
    else if (buyStillPending && !sellStillPending)
    {
        // SellStop DISAPPEARED → executed!
        // BuyStop STILL in list → not executed

        Console.WriteLine("  🚀 SELL STOP EXECUTED! Downward breakout!");
        executedOrder = sellStopResult.Order;
        cancelOrder = buyStopResult.Order;  // ← Need to cancel
        break;
    }

    // ═══════════════════════════════════════════════════════
    // SCENARIO 3: BOTH EXECUTED (rare case)
    // ═══════════════════════════════════════════════════════
    else if (!buyStillPending && !sellStillPending)
    {
        // Both orders disappeared
        // Possible causes:
        // 1. Very strong volatility (both triggered)
        // 2. Connection error (didn't get actual data)
        // 3. Manual cancellation of both orders

        Console.WriteLine("  ✓ Both orders executed or canceled");
        break;
    }

    // ═══════════════════════════════════════════════════════
    // SCENARIO 4: BOTH STILL PENDING (continue waiting)
    // ═══════════════════════════════════════════════════════
    // else: both orders still in list → no breakout → continue loop
}
```

#### Detailed breakout detection logic visualization

```

│  HOW OpenedOrdersTicketsAsync() WORKS                      
└────────────────────────────────────────────────────────────

MT5 Terminal state:
┌──────────────────┬────────────────────────────────────────┐
│  PENDING ORDERS  │  OPEN POSITIONS                        │
├──────────────────┼────────────────────────────────────────┤
│  789: BUY STOP   │  (empty)                               │
│  790: SELL STOP  │                                        │
└──────────────────┴────────────────────────────────────────┘

OpenedOrdersTicketsAsync() returns:
{
    OpenedOrdersTickets: [789, 790],  ← PENDING ORDERS
    OpenedPositionTickets: []         ← OPEN POSITIONS
}

foreach (var ticket in tickets.OpenedOrdersTickets)
{
    if (ticket == 789) buyStillPending = true;   // ✓ found
    if (ticket == 790) sellStillPending = true;  // ✓ found
}

Result: buyStillPending = true, sellStillPending = true
→ Both orders still pending → no breakout → continue waiting

─────────────────────────────────────────────────────────────

5 MINUTES LATER: Price rose to 1.10027

MT5 Terminal state:
┌──────────────────┬────────────────────────────────────────┐
│  PENDING ORDERS  │  OPEN POSITIONS                        │
├──────────────────┼────────────────────────────────────────┤
│  790: SELL STOP  │  789: BUY 0.01 EURUSD @ 1.10027        │
└──────────────────┴────────────────────────────────────────┘
                     ↑
                     BuyStop EXECUTED → became POSITION!

OpenedOrdersTicketsAsync() returns:
{
    OpenedOrdersTickets: [790],       ← only SellStop
    OpenedPositionTickets: [789]      ← BuyStop became position
}

foreach (var ticket in tickets.OpenedOrdersTickets)
{
    // ticket = 790
    if (ticket == 789) buyStillPending = true;   // ✗ NOT found!
    if (ticket == 790) sellStillPending = true;  // ✓ found
}

Result: buyStillPending = false, sellStillPending = true

if (!buyStillPending && sellStillPending)  ← TRUE!
{
    Console.WriteLine("🚀 BUY STOP EXECUTED! Upward breakout!");
    cancelOrder = 790;  ← Need to cancel SellStop
    break;
}
```

---

### Phase 5: Canceling opposite order (lines 131-145)

```csharp

// │  CHECK: Did breakout happen?                            
// └─────────────────────────────────────────────────────────
if (cancelOrder.HasValue)
{
    // ═══════════════════════════════════════════════════════
    // BREAKOUT HAPPENED → Cancel opposite order
    // ═══════════════════════════════════════════════════════

    Console.WriteLine($"  Canceling opposite order #{cancelOrder.Value}...");

    
    // │  CloseByTicket - universal method:              
    // │  - If ticket = pending order → CANCEL           
    // │  - If ticket = position → CLOSE                 
    // └─────────────────────────────────────────────────
    await _service.CloseByTicket(cancelOrder.Value);

    Console.WriteLine("  ✓ Canceled\n");
}
else
{
    // ═══════════════════════════════════════════════════════
    // TIMEOUT → Breakout didn't happen → Cancel BOTH orders
    // ═══════════════════════════════════════════════════════

    Console.WriteLine($"  ⏱ Timeout after {MaxWaitMinutes} minutes - canceling both orders...");

    
    // │  Cancel both orders separately                 
    // └─────────────────────────────────────────────────
    await _service.CloseByTicket(buyStopResult.Order);
    await _service.CloseByTicket(sellStopResult.Order);

    Console.WriteLine("  ✓ Both canceled\n");
}
```

#### How CloseByTicket() works

```csharp
// MT5Sugar.cs (extension method) — condensed, matching the real implementation
public static Task<OrderCloseData> CloseByTicket(
    this MT5Service svc, ulong ticket, double? volume = null,
    int timeoutSec = 15, CancellationToken ct = default)
{
    // One request handles both cases: closing a position and cancelling a pending order.
    var req = new OrderCloseRequest { Ticket = ticket, Volume = volume ?? 0 };
    return svc.OrderCloseAsync(req, Dl(timeoutSec), ct);
}
```

#### Detailed order cancellation chain

```
SITUATION: Upward breakout, need to cancel SellStop #790

USER CODE:
    await _service.CloseByTicket(790);
        │
        ▼
MT5Sugar.CloseByTicket():
    
    │ 1. Build OrderCloseRequest { Ticket = 790 }  
    │ 2. return svc.OrderCloseAsync(req)           
    └─────────────────────────────────────────────
        │
        ▼
MT5Service.OrderCloseAsync(request):
    return await _account.OrderCloseAsync(request);
        │
        ▼
MT5Account.OrderCloseAsync(OrderCloseRequest):
    var response = await TradeClient.OrderClose(request, ...);
        │
        ▼
    gRPC → MT5 Terminal
        │
        ▼
MT5 Terminal:
    
    │ PENDING ORDERS                          
    │ ✗ 790: SELL STOP @ 1.09975 (DELETED)   
    └─────────────────────────────────────────

RESULT:
MT5 Terminal state:
┌──────────────────┬────────────────────────────────────────┐
│  PENDING ORDERS  │  OPEN POSITIONS                        │
├──────────────────┼────────────────────────────────────────┤
│  (empty)         │  789: BUY 0.01 EURUSD @ 1.10027        │
└──────────────────┴────────────────────────────────────────┘

Now only ONE position (BUY) is open.
SellStop successfully canceled → OCO strategy completed.
```

---

### Phase 6: Finalization (lines 147-154)

```csharp

// │  Get final balance                                      
// └─────────────────────────────────────────────────────────
var finalBalance = await _service.GetBalanceAsync();


// │  Calculate profit/loss                                  
// └─────────────────────────────────────────────────────────
var profit = finalBalance - initialBalance;

Console.WriteLine($"  Final balance: ${finalBalance:F2}");
Console.WriteLine($"  Profit/Loss: ${profit:F2}");
Console.WriteLine("\n+============================================================+\n");


// │  Return profit as result of ExecuteAsync()              
// └─────────────────────────────────────────────────────────
return profit;
```

---

## 🎭 Complete Life Cycle (Upward breakout scenario)

### Execution timeline

```
T=0      START ExecuteAsync()
         │
         ├─► GetBalanceAsync()           → $10000.00
         ├─► SymbolInfoTickAsync()       → Bid:1.10000, Ask:1.10002
         │
T=1s     ├─► BuyStopPoints(+25)
         │   └─► Created BuyStop #789 @ 1.10027
         │
T=2s     ├─► SellStopPoints(-25)
         │   └─► Created SellStop #790 @ 1.09975
         │
         │   MT5 Terminal state:
         │   ┌────────────────┬──────────────┐
         │   │ PENDING ORDERS │ POSITIONS    │
         │   ├────────────────┼──────────────┤
         │   │ 789: BUY STOP  │ (empty)      │
         │   │ 790: SELL STOP │              │
         │   └────────────────┴──────────────┘
         │
T=3s     ├─► MONITORING START
         │
T=6s     ├─► Task.Delay(3000)
         │   ├─► OpenedOrdersTicketsAsync() → [789, 790]
         │   ├─► buyStillPending = true
         │   ├─► sellStillPending = true
         │   └─► Both pending → continue
         │
T=9s     ├─► Task.Delay(3000)
         │   └─► [789, 790] → both pending → continue
         │
         │   MARKET: Price rises...
         │   1.10005 → 1.10015 → 1.10020 → 1.10027...
         │
T=12s    ├─► Task.Delay(3000)
         │   │
         │   │  MT5 Terminal: Price reached 1.10027!
         │   │  BuyStop #789 EXECUTED → became position
         │   │
         │   │  MT5 Terminal state:
         │   │  ┌────────────────┬──────────────────────┐
         │   │  │ PENDING ORDERS │ POSITIONS            │
         │   │  ├────────────────┼──────────────────────┤
         │   │  │ 790: SELL STOP │ 789: BUY 0.01        │
         │   │  │                │      @ 1.10027       │
         │   │  └────────────────┴──────────────────────┘
         │   │
         │   ├─► OpenedOrdersTicketsAsync() → [790]
         │   ├─► buyStillPending = false  ← NOT found!
         │   ├─► sellStillPending = true
         │   │
         │   └─► if (!buy && sell) → TRUE!
         │       ├─► "🚀 BUY STOP EXECUTED! Upward breakout!"
         │       ├─► executedOrder = 789
         │       ├─► cancelOrder = 790
         │       └─► break (exit loop)
         │
T=13s    ├─► CANCEL opposite order
         │   ├─► CloseByTicket(790)
         │   │   └─► OrderCloseAsync(close request for #790)
         │   │       └─► SellStop #790 CANCELED
         │   │
         │   │  MT5 Terminal state:
         │   │  ┌────────────────┬──────────────────────┐
         │   │  │ PENDING ORDERS │ POSITIONS            │
         │   │  ├────────────────┼──────────────────────┤
         │   │  │ (empty)        │ 789: BUY 0.01        │
         │   │  │                │      @ 1.10027       │
         │   │  │                │      SL: 1.10012     │
         │   │  │                │      TP: 1.10057     │
         │   │  └────────────────┴──────────────────────┘
         │
         │   OCO STRATEGY COMPLETED!
         │   Position opened, opposite order canceled.
         │
         │   MARKET: Price continues rising...
         │   1.10030 → 1.10040 → 1.10050 → 1.10057...
         │
T=180s   │   MT5 Terminal: Price reached 1.10057!
(3 min)  │   TP triggered → position closed
         │
         │   MT5 Terminal state:
         │   ┌────────────────┬──────────────────────┐
         │   │ PENDING ORDERS │ POSITIONS            │
         │   ├────────────────┼──────────────────────┤
         │   │ (empty)        │ (empty)              │
         │   └────────────────┴──────────────────────┘
         │
         │   CLOSED POSITION:
         │   Entry: 1.10027
         │   Exit:  1.10057
         │   Profit: (1.10057 - 1.10027) × 100000 × 0.01
         │          = 0.00030 × 100000 × 0.01
         │          = 30 × 0.01
         │          = $3.00
         │
T=183s   ├─► GetBalanceAsync()           → $10003.00
         ├─► profit = 10003.00 - 10000.00 = +$3.00
         │
         └─► RETURN profit = 3.00
```

---

## 📊 What the result is made of

### Profit calculation (upward breakout)

```
INITIAL BALANCE: $10000.00

UPWARD BREAKOUT:

1. BuyStop #789 @ 1.10027 EXECUTED
   → Opened position BUY 0.01 lots
   → SL: 1.10012 (protection -15 points)
   → TP: 1.10057 (target +30 points)

2. SellStop #790 CANCELED
   → Cancellation fee: $0.00

POSITION OUTCOME (TP triggered):
   Entry: 1.10027
   Exit:  1.10057
   Pips:  (1.10057 - 1.10027) / 0.00001 = 30 points

PROFIT CALCULATION:
   Profit = Pips × Point Value × Volume
   Point Value for EURUSD (1.0 lot) = $10
   Point Value for 0.01 lot = $0.10

   Profit = 30 × $0.10 = $3.00

FINAL BALANCE: $10003.00
PROFIT = $3.00

return 3.00;
```

### Loss calculation (SL triggered)

```
IF SL HAD TRIGGERED:
   Entry: 1.10027
   Exit:  1.10012 (SL)
   Pips:  (1.10012 - 1.10027) / 0.00001 = -15 points

   Loss = -15 × $0.10 = -$1.50

FINAL BALANCE: $9998.50
PROFIT = -$1.50

R:R ratio = 1.50 / 3.00 = 1:2 ✓
```

---

## 🧩 Components and their roles

### 1. PendingBreakoutOrchestrator

**Role**: OCO strategy coordinator

**Tasks**:

- Stores parameters (Symbol, BreakoutDistancePoints, etc.)
- Manages life cycle
- Places both orders
- **Monitors breakout through OpenedOrdersTicketsAsync()**
- Cancels opposite order
- Handles placement errors
- Returns result

### 2. MT5Service

**Role**: Service layer

**Tasks**:

- Provides methods like `SymbolInfoTickAsync`, `OrderSendAsync`, `OrderCloseAsync`, `OpenedOrdersTicketsAsync`
- Delegates calls to MT5Account

### 3. MT5Sugar (extension methods)

**Role**: Simplifying layer

**Tasks**:

- `BuyStopPoints()` - converts points to prices for BuyStop
- `SellStopPoints()` - converts points to prices for SellStop
- `CloseByTicket()` - universal cancel/close

### 4. OpenedOrdersTicketsAsync()

**Role**: **CRITICAL** component for breakout detection

**How it works**:

```csharp
// Returns TWO lists:
OpenedOrdersTicketsData {
    OpenedOrdersTickets: [list of pending order tickets],
    OpenedPositionTickets: [list of open position tickets]
}

// KEY MOMENT:
// When pending order executes → it disappears from OpenedOrdersTickets
// and appears in OpenedPositionTickets

// Check ticket presence in pending list:
foreach (var ticket in tickets.OpenedOrdersTickets)
{
    if (ticket == ourBuyStopTicket) buyStillPending = true;
}

// If ticket NOT found → order executed!
if (!buyStillPending) {
    // BuyStop no longer pending → breakout happened!
}
```

---

## 🔍 Final Dependency Diagram

```

│  USER CODE                                                   
│  var orch = new PendingBreakoutOrchestrator(service);       
│  var profit = await orch.ExecuteAsync();                    
└──────────────────────────┬──────────────────────────────────
                           │
                           ▼

│  PendingBreakoutOrchestrator                                
│   
│  │  ExecuteAsync() {                                       
│  │    1. BuyStopPoints(+25)                                
│  │    2. SellStopPoints(-25)                               
│  │    3. LOOP: OpenedOrdersTicketsAsync()                  
│  │       └─► Check ticket presence                         
│  │    4. CloseByTicket(opposite)                           
│  │  }                                                      
│  └───────────────────────────────────────────────────────  
└──────────────────────────┬──────────────────────────────────
                           │ _service
                           ▼

│  MT5Service                                                 
│  - OpenedOrdersTicketsAsync()  ← KEY method!                
└──────────────────────────┬──────────────────────────────────
                           │
                           ▼

│  MT5Sugar (extension methods)                               
│  - BuyStopPoints(+offsetPoints)  → price ABOVE Ask          
│  - SellStopPoints(-offsetPoints) → price BELOW Bid          
│  - CloseByTicket(ticket)         → cancel/close             
└──────────────────────────┬──────────────────────────────────
                           │
                           ▼
                    [MT5 Terminal]
```

---

## 🎯 Summary

**PendingBreakoutOrchestrator is made of**:

1. **1 dependency**: `MT5Service _service`
2. **6 parameters**: Symbol, BreakoutDistancePoints, StopLoss, TakeProfit, Volume, MaxWaitMinutes
3. **4 MT5Sugar methods**: `BuyStopPoints`, `SellStopPoints`, `CloseByTicket`, `OpenedOrdersTicketsAsync`
4. **OCO logic**: Breakout detection through checking ticket presence in pending orders list

**Works through**:

- Placing 2 opposite Stop orders
- **Monitoring pending orders list every 3 seconds**
- **Detecting breakout: if ticket disappeared from list → order executed**
- Canceling opposite order
- Timeout handling

**Returns**:

- `double profit` - difference between final and initial balance

**Key insight**:

All the magic of breakout detection is based on a simple fact: **when a pending order executes, it disappears from the OpenedOrdersTickets list**. By checking for our tickets' presence in this list, we know exactly which order triggered and which is still waiting for execution.
