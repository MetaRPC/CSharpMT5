# HOW NewsStraddleOrchestrator WORKS - Detailed Analysis

## 🎯 Document Purpose

Show **WHAT** the orchestrator consists of and **HOW EXACTLY** it works at the code, methods and data level. Special attention is paid to the timing of order placement before news and handling three breakout scenarios.

---

## 📦 What the orchestrator is made of

### 1. Class structure (lines 13-28)

```csharp
public class NewsStraddleOrchestrator
{
    
    // │  SINGLE DEPENDENCY                      
    // └─────────────────────────────────────────
    private readonly MT5Service _service;

    
    // │  7 CONFIGURABLE PARAMETERS              
    // └─────────────────────────────────────────
    public string Symbol { get; set; } = "EURUSD";
    public int StraddleDistancePoints { get; set; } = 15;
    public double Volume { get; set; } = 0.02;
    public int StopLossPoints { get; set; } = 20;
    public int TakeProfitPoints { get; set; } = 40;
    public int SecondsBeforeNews { get; set; } = 60;          // ← Countdown timer
    public int MaxWaitAfterNewsSeconds { get; set; } = 180;   // ← Breakout timeout

    public NewsStraddleOrchestrator(MT5Service service)
    {
        _service = service;
    }
}
```

### Dependency visualization

```

│         NewsStraddleOrchestrator                           
│    
│  │  private readonly MT5Service _service                  
│  └──────────────────────┬───────────────────────────────  
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

### Phase 1: Initialization (lines 30-41)

```csharp
public async Task<double> ExecuteAsync(CancellationToken ct = default)
{
    Console.WriteLine("\n+============================================================+");
    Console.WriteLine("|  NEWS STRADDLE ORCHESTRATOR                               |");
    Console.WriteLine("+============================================================+\n");

    var initialBalance = await _service.GetBalanceAsync();
    Console.WriteLine($"  Starting balance: ${initialBalance:F2}");
    Console.WriteLine($"  Symbol: {Symbol}");
    Console.WriteLine($"  Straddle distance: {StraddleDistancePoints} pts");
    Console.WriteLine($"  Volume: {Volume:F2} lots");
    Console.WriteLine($"  SL: {StopLossPoints} pts | TP: {TakeProfitPoints} pts\n");
}
```

---

### Phase 2: Countdown until news (lines 44-51)

```csharp
try
{
    
    // │  CRITICAL TIMING:                                   
    // │  Wait SecondsBeforeNews seconds until the event     
    // └─────────────────────────────────────────────────────
    Console.WriteLine($"  ⏲  Waiting {SecondsBeforeNews}s before news event...\n");
    await Task.Delay(SecondsBeforeNews * 1000, ct);

    
    // │  Get current price IMMEDIATELY before               
    // │  placing orders (maximum accuracy)                  
    // └─────────────────────────────────────────────────────
    var tick = await _service.SymbolInfoTickAsync(Symbol);
    Console.WriteLine($"  📰 NEWS EVENT IMMINENT!");
    Console.WriteLine($"  Current: Bid={tick.Bid:F5}, Ask={tick.Ask:F5}\n");
}
```

**Key timing moment**:

```
EXAMPLE: NFP releases at 13:30:00 UTC

Orchestrator launch:
  User: await orchestrator.ExecuteAsync() @ 13:29:00

Countdown:
  SecondsBeforeNews = 60
  Task.Delay(60000) → wait 60 seconds

Order placement:
  @ 13:30:00 (exactly when news releases!)

IMPORTANT:
- Too early → risk triggering from noise
- Too late → miss the beginning of movement
- 60 seconds = optimal balance
```

---

### Phase 3: Placing the straddle (lines 53-91)

#### 3.1. Placing BuyStop (upper order)

```csharp

// │  BUY STOP: Catches upward breakout                      
// └─────────────────────────────────────────────────────────
Console.WriteLine("  Placing BUY STOP (upper straddle)...");

var buyStopResult = await _service.BuyStopPoints(
    symbol: Symbol,                         // "EURUSD"
    volume: Volume,                         // 0.02
    priceOffsetPoints: StraddleDistancePoints,  // +15 (POSITIVE!)
    slPoints: StopLossPoints,               // 20
    tpPoints: TakeProfitPoints,             // 40
    comment: "News-Buy"
);

if (buyStopResult.ReturnedCode != 10009)
{
    Console.WriteLine($"  ✗ BUY STOP failed: {buyStopResult.Comment}\n");
    return 0;  // ← EMERGENCY EXIT
}

Console.WriteLine($"  ✓ BUY STOP: #{buyStopResult.Order}\n");
```

#### How BuyStopPoints() works for straddle

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

    // Buy Stop is ALWAYS above Ask → absolute value of the offset is used
    //   Ask = 1.10002, |offset| = 15 → raw = 1.10002 + 15×0.00001 = 1.10017
    var rawPrice = tick.Ask + Math.Abs(priceOffsetPoints) * point;
    var price    = await svc.NormalizePriceAsync(symbol, rawPrice, timeoutSec, ct);

    double sl = slPoints.HasValue ? price - slPoints.Value * point : 0;  // 1.09997
    double tp = tpPoints.HasValue ? price + tpPoints.Value * point : 0;  // 1.10057

    // operationCode for Buy Stop (4)
    var req = BuildPendingRequest(symbol, volume, price, sl, tp, comment, deviationPoints, operationCode: 4);
    return await svc.OrderSendAsync(req, Dl(timeoutSec), ct);
}
```

> As with the other `*Points` helpers, the offset's sign is irrelevant — `Math.Abs(...)` is applied and a Buy Stop is inherently above the Ask (`SellStopPoints` is inherently below the Bid). The orchestrator passes `+Distance` / `-Distance` only as a readable convention.

#### 3.2. Placing SellStop (lower order)

```csharp

// │  SELL STOP: Catches downward breakout                   
// └─────────────────────────────────────────────────────────
Console.WriteLine("  Placing SELL STOP (lower straddle)...");

var sellStopResult = await _service.SellStopPoints(
    symbol: Symbol,
    volume: Volume,
    priceOffsetPoints: -StraddleDistancePoints,  // -15 (NEGATIVE!)
    slPoints: StopLossPoints,
    tpPoints: TakeProfitPoints,
    comment: "News-Sell"
);

if (sellStopResult.ReturnedCode != 10009)
{
    Console.WriteLine($"  ✗ SELL STOP failed: {sellStopResult.Comment}");
    Console.WriteLine("  Canceling BUY STOP...");

    
    // │  CRITICALLY IMPORTANT:                          
    // │  If second order failed → cancel first one      
    // └─────────────────────────────────────────────────
    await _service.CloseByTicket(buyStopResult.Order);
    return 0;
}

Console.WriteLine($"  ✓ SELL STOP: #{sellStopResult.Order}\n");
Console.WriteLine("  ✅ STRADDLE ACTIVE - Waiting for news spike!\n");
```

#### SellStop price calculation

```
Current Bid price: 1.10000
StraddleDistancePoints: 15 (but we use -15)
point: 0.00001

price = bidPrice + (priceOffsetPoints × point)
      = 1.10000 + (-15 × 0.00001)
      = 1.10000 - 0.00015
      = 1.09985

sl = price + (slPoints × point)  ← PLUS for SELL!
   = 1.09985 + (20 × 0.00001)
   = 1.10005

tp = price - (tpPoints × point)  ← MINUS for SELL!
   = 1.09985 - (40 × 0.00001)
   = 1.09945
```

#### Result of straddle placement

```
MT5 Terminal state AFTER straddle placement:

┌──────────────────┬─────────────────────────────
│  PENDING ORDERS  │  OPEN POSITIONS                        
├──────────────────┼─────────────────────────────
│  #123456789:     │  (empty)                               
│  BUY STOP 0.02   │                                        
│  @ 1.10017       │                                        
│  SL: 1.09997     │                                        
│  TP: 1.10057     │                                        
│                  │                                        
│  #123456790:     │                                        
│  SELL STOP 0.02  │                                        
│  @ 1.09985       │                                        
│  SL: 1.10005     │                                        
│  TP: 1.09945     │                                        
└──────────────────┴─────────────────────────────

VISUALIZATION:
          ↑ Price rises
          │
  1.10057 ├─── TP for BuyStop
  1.10017 ├─── BUY STOP (upward breakout)
  1.09997 ├─── SL for BuyStop
          │
  1.10002 ├─── Current Ask
  1.10000 ├─── Current Bid
          │
  1.10005 ├─── SL for SellStop
  1.09985 ├─── SELL STOP (downward breakout)
  1.09945 ├─── TP for SellStop
          │
          ↓ Price falls
```

---

### Phase 4: Monitoring for breakout (lines 94-136)

This is **the key phase** - determining which order triggered after news release.

```csharp
var monitorStart = DateTime.UtcNow;
var timeout = TimeSpan.FromSeconds(MaxWaitAfterNewsSeconds);
ulong? executedOrder = null;
ulong? pendingOrder = null;
string direction = "";


// │  MONITORING LOOP (every 1 sec, max 3 minutes)          
// │  FASTER than PendingBreakout (news requires speed!)    
// └─────────────────────────────────────────────────────────
while (DateTime.UtcNow - monitorStart < timeout && !ct.IsCancellationRequested)
{
    await Task.Delay(1000, ct);  // Every second!

    
    // │  Get list of PENDING ORDERS                     
    // └─────────────────────────────────────────────────
    var tickets = await _service.OpenedOrdersTicketsAsync();

    bool buyStillPending = false;
    bool sellStillPending = false;

    
    // │  Check if our orders are in the list            
    // └─────────────────────────────────────────────────
    foreach (var ticket in tickets.OpenedOrdersTickets)
    {
        if (ticket == (long)buyStopResult.Order) buyStillPending = true;
        if (ticket == (long)sellStopResult.Order) sellStillPending = true;
    }

    
    // │  BREAKOUT SCENARIO DETECTION (4 options)            
    // └─────────────────────────────────────────────────────

    // ═══════════════════════════════════════════════════════
    // SCENARIO 1: UPWARD BREAKOUT
    // ═══════════════════════════════════════════════════════
    if (!buyStillPending && sellStillPending)
    {
        // BuyStop DISAPPEARED (executed) → became position
        // SellStop STILL PENDING → not executed
        executedOrder = buyStopResult.Order;
        pendingOrder = sellStopResult.Order;
        direction = "UPWARD";
        break;
    }

    // ═══════════════════════════════════════════════════════
    // SCENARIO 2: DOWNWARD BREAKOUT
    // ═══════════════════════════════════════════════════════
    else if (buyStillPending && !sellStillPending)
    {
        // SellStop DISAPPEARED (executed)
        // BuyStop STILL PENDING
        executedOrder = sellStopResult.Order;
        pendingOrder = buyStopResult.Order;
        direction = "DOWNWARD";
        break;
    }

    // ═══════════════════════════════════════════════════════
    // SCENARIO 3: BOTH TRIGGERED (extreme volatility!)
    // ═══════════════════════════════════════════════════════
    else if (!buyStillPending && !sellStillPending)
    {
        // Both orders DISAPPEARED → both became positions!
        // Price whipsawed up AND down very quickly
        Console.WriteLine("  ⚡ BOTH ORDERS TRIGGERED - Extreme volatility!");
        direction = "BOTH";
        break;
    }

    // ═══════════════════════════════════════════════════════
    // SCENARIO 4: BOTH STILL PENDING (continue waiting)
    // ═══════════════════════════════════════════════════════
    // else: both orders still in list → news didn't cause breakout
}
```

#### Detailed breakout detection logic

```
STATE BEFORE NEWS (T=0):

MT5 Terminal:
┌──────────────────┬────────────────────────────────────────┐
│  PENDING ORDERS  │  OPEN POSITIONS                        │
├──────────────────┼────────────────────────────────────────┤
│  789: BUY STOP   │  (empty)                               │
│  790: SELL STOP  │                                        │
└──────────────────┴────────────────────────────────────────┘

OpenedOrdersTicketsAsync() → OpenedOrdersTickets: [789, 790]

buyStillPending = true (789 found)
sellStillPending = true (790 found)

→ Both pending → continue waiting

─────────────────────────────────────────────────────────────

NEWS RELEASED (T=1s): NFP better than expected → price up!

Price: 1.10000 → 1.10010 → 1.10017 → 1.10020...

MT5 Terminal (T=2s):
┌──────────────────┬────────────────────────────────────────┐
│  PENDING ORDERS  │  OPEN POSITIONS                        │
├──────────────────┼────────────────────────────────────────┤
│  790: SELL STOP  │  789: BUY 0.02 @ 1.10017               │
│                  │      (BuyStop triggered!)              │
└──────────────────┴────────────────────────────────────────┘

OpenedOrdersTicketsAsync() → OpenedOrdersTickets: [790]
                              (BuyStop 789 DISAPPEARED!)

buyStillPending = false  ← 789 NOT found!
sellStillPending = true  ← 790 still in list

if (!buyStillPending && sellStillPending) → TRUE!
{
    direction = "UPWARD";
    executedOrder = 789;
    pendingOrder = 790;
    break;  ← Exit monitoring
}

─────────────────────────────────────────────────────────────

SCENARIO 3: Extreme volatility

News released UNEXPECTEDLY → price jerked DOWN:
1.10000 → 1.09985 (SellStop triggered!)

Then SHARPLY UP (correction):
1.09985 → 1.10017 (BuyStop also triggered!)

MT5 Terminal:
┌──────────────────┬────────────────────────────────────────┐
│  PENDING ORDERS  │  OPEN POSITIONS                        │
├──────────────────┼────────────────────────────────────────┤
│  (empty)         │  789: BUY 0.02 @ 1.10017               │
│                  │  790: SELL 0.02 @ 1.09985              │
└──────────────────┴────────────────────────────────────────┘

OpenedOrdersTicketsAsync() → OpenedOrdersTickets: []
                              (BOTH DISAPPEARED!)

buyStillPending = false
sellStillPending = false

if (!buyStillPending && !sellStillPending) → TRUE!
{
    Console.WriteLine("⚡ BOTH ORDERS TRIGGERED - Extreme volatility!");
    direction = "BOTH";
    break;
}
```

---

### Phase 5: Handling breakout result (lines 138-161)

```csharp
// ═══════════════════════════════════════════════════════
// SCENARIO A: ONE ORDER TRIGGERED (normal breakout)
// ═══════════════════════════════════════════════════════
if (executedOrder.HasValue && pendingOrder.HasValue)
{
    Console.WriteLine($"  🚀 {direction} BREAKOUT DETECTED!");
    Console.WriteLine($"  Position opened: #{executedOrder.Value}");
    Console.WriteLine($"  Canceling opposite order #{pendingOrder.Value}...");

    
    // │  OCO mechanism: cancel opposite order           
    // └─────────────────────────────────────────────────
    await _service.CloseByTicket(pendingOrder.Value);
    Console.WriteLine("  ✓ Opposite order canceled\n");

    
    // │  Hold position for 60 seconds                   
    // │  SL or TP may trigger during this time          
    // └─────────────────────────────────────────────────
    Console.WriteLine("  ⏳ Holding position for 60 seconds...");
    await Task.Delay(60000, ct);
}

// ═══════════════════════════════════════════════════════
// SCENARIO B: BOTH ORDERS TRIGGERED
// ═══════════════════════════════════════════════════════
else if (direction == "BOTH")
{
    // TWO positions opened (BUY and SELL) → this is a HEDGE!
    // Hold for shorter time (30 sec instead of 60)
    Console.WriteLine("  ⏳ Holding both positions for 30 seconds...");
    await Task.Delay(30000, ct);
}

// ═══════════════════════════════════════════════════════
// SCENARIO C: TIMEOUT (breakout didn't happen)
// ═══════════════════════════════════════════════════════
else
{
    Console.WriteLine($"  ⏱ No breakout after {MaxWaitAfterNewsSeconds}s");
    Console.WriteLine("  Canceling both pending orders...");

    
    // │  Cancel BOTH orders                             
    // └─────────────────────────────────────────────────
    await _service.CloseByTicket(buyStopResult.Order);
    await _service.CloseByTicket(sellStopResult.Order);
}
```

---

### Phase 6: Final closing (lines 163-176)

```csharp

// │  Close all remaining positions                          
// └─────────────────────────────────────────────────────────
Console.WriteLine("\n  Closing all remaining positions...");
await _service.CloseAll(Symbol);
Console.WriteLine("  ✓ All closed");

var finalBalance = await _service.GetBalanceAsync();
var profit = finalBalance - initialBalance;

Console.WriteLine($"\n  Final balance: ${finalBalance:F2}");
Console.WriteLine($"  Profit/Loss: ${profit:F2}");
Console.WriteLine($"  Direction: {(string.IsNullOrEmpty(direction) ? "None" : direction)}");

return profit;
```

---

## 🎭 Complete Life Cycle (Upward breakout scenario)

### Execution timeline

```
T=-60s   START ExecuteAsync()
         │
         ├─► GetBalanceAsync()           → $10000.00
         │
         ├─► "Waiting 60s before news event..."
         ├─► Task.Delay(60000)  ← COUNTDOWN
         │
         │   USER sees countdown until news
         │
T=0      ├─► Task.Delay COMPLETED
         │
         ├─► "NEWS EVENT IMMINENT!"
         ├─► SymbolInfoTickAsync()
         │   └─► Bid=1.10000, Ask=1.10002
         │
T=1s     ├─► BuyStopPoints(+15)
         │   └─► Created BuyStop @ 1.10017
         │       SL: 1.09997, TP: 1.10057
         │
T=2s     ├─► SellStopPoints(-15)
         │   └─► Created SellStop @ 1.09985
         │       SL: 1.10005, TP: 1.09945
         │
         │   "✅ STRADDLE ACTIVE"
         │
         │   MT5 Terminal state:
         │   ┌────────────────┬──────────────┐
         │   │ PENDING ORDERS │ POSITIONS    │
         │   ├────────────────┼──────────────┤
         │   │ 789: BUY STOP  │ (empty)      │
         │   │ 790: SELL STOP │              │
         │   └────────────────┴──────────────┘
         │
T=3s     ├─► MONITORING START (max 180 sec)
         │
T=4s     ├─► Task.Delay(1000)
         │   OpenedOrdersTicketsAsync() → [789, 790]
         │   buyStillPending = true
         │   sellStillPending = true
         │   → Both pending → continue
         │
         │   ┌────────────────────────────────┐
         │   │  NEWS RELEASED!                │
         │   │  NFP: +350K jobs (forecast +200K)│
         │   │  Much better than expected!    │
         │   └────────────────────────────────┘
         │
         │   MARKET: Price sharply UP!
         │   1.10000 → 1.10010 → 1.10017 → 1.10020...
         │
T=5s     ├─► MT5 Terminal: Price reached 1.10017!
         │   BuyStop TRIGGERED!
         │   ┌──────────────────────────────────┐
         │   │ Opened position BUY 0.02         │
         │   │ Entry: 1.10017                   │
         │   │ SL: 1.09997                      │
         │   │ TP: 1.10057                      │
         │   └──────────────────────────────────┘
         │
T=6s     ├─► Task.Delay(1000)
         │   OpenedOrdersTicketsAsync() → [790]
         │   buyStillPending = false  ← 789 DISAPPEARED!
         │   sellStillPending = true
         │
         │   if (!buy && sell) → TRUE!
         │   {
         │       direction = "UPWARD";
         │       executedOrder = 789;
         │       pendingOrder = 790;
         │       break;  ← Exit monitoring
         │   }
         │
         ├─► "🚀 UPWARD BREAKOUT DETECTED!"
         │
         ├─► CloseByTicket(790)  ← Cancel SellStop
         │   └─► OrderCloseAsync(close request for #790)
         │
         │   MT5 Terminal state:
         │   ┌────────────────┬──────────────────────┐
         │   │ PENDING ORDERS │ POSITIONS            │
         │   ├────────────────┼──────────────────────┤
         │   │ (empty)        │ 789: BUY 0.02        │
         │   │                │      @ 1.10017       │
         │   └────────────────┴──────────────────────┘
         │
         ├─► "Holding position for 60 seconds..."
         ├─► Task.Delay(60000)
         │
         │   MARKET: Price continues rising...
         │   1.10020 → 1.10040 → 1.10057...
         │
T=35s    │   MT5 Terminal: Price reached 1.10057!
         │   TP TRIGGERED!
         │   ┌──────────────────────────────────┐
         │   │ Position closed automatically    │
         │   │ Entry: 1.10017                   │
         │   │ Exit:  1.10057                   │
         │   │ Profit: +40 pts × 0.02 = +$8.00  │
         │   └──────────────────────────────────┘
         │
T=66s    ├─► Task.Delay(60000) finished
         │
         ├─► CloseAll("EURUSD")
         │   └─► Position already closed (TP triggered)
         │
T=67s    ├─► GetBalanceAsync()           → $10008.00
         ├─► profit = 10008.00 - 10000.00 = +$8.00
         │
         └─► RETURN profit = 8.00

TOTAL: Caught news breakout WITHOUT predicting direction!
       Profit: +$8.00
```

---

## 📊 What the result is made of

### Profit calculation (upward breakout, TP triggered)

```
INITIAL BALANCE: $10000.00

STRADDLE PLACED:
- BuyStop @ 1.10017 (SL: 1.09997, TP: 1.10057)
- SellStop @ 1.09985 (canceled after upward breakout)

UPWARD BREAKOUT:
- BuyStop triggered @ 1.10017
- Opened position BUY 0.02 lots

TP TRIGGERED:
- Exit @ 1.10057
- Pips: (1.10057 - 1.10017) / 0.00001 = 40 points

PROFIT CALCULATION:
- Profit = Pips × Volume × PointValue
- PointValue for EURUSD (0.02 lots) = 0.02 × $10 = $0.20 per point
- Profit = 40 × $0.20 = +$8.00

FINAL BALANCE: $10008.00
PROFIT = $8.00

return 8.00;
```

### P/L calculation (scenario "BOTH" - both orders triggered)

```
EXTREME VOLATILITY:

News caused whipsaw (jerked both directions):

1. Price fell to 1.09985 → SellStop triggered
2. Price rose to 1.10017 → BuyStop triggered

TWO POSITIONS OPENED:
- BUY 0.02 @ 1.10017
- SELL 0.02 @ 1.09985

FINAL PRICE (after 30 sec): 1.10005

CLOSING:

BUY position:
  Entry: 1.10017
  Exit:  1.10005
  Pips:  (1.10005 - 1.10017) / 0.00001 = -12 points
  P/L:   -12 × $0.20 = -$2.40

SELL position:
  Entry: 1.09985
  Exit:  1.10005
  Pips:  (1.09985 - 1.10005) / 0.00001 = -20 points
  P/L:   -20 × $0.20 = -$4.00

TOTAL:
  BUY:  -$2.40
  SELL: -$4.00
  ─────────────
  TOTAL: -$6.40

FINAL BALANCE: $9993.60
PROFIT = -$6.40

This is the WORST scenario (whipsaw) - both orders triggered, both in loss.
Probability: ~5-10% for strong news.
```

### P/L calculation (timeout - breakout didn't happen)

```
WEAK NEWS:

News released, but data within forecast.
Price moves in narrow range:
1.10000 → 1.10005 → 1.09998 → 1.10003...

NO ORDER TRIGGERED:
- BuyStop @ 1.10017 (price didn't reach)
- SellStop @ 1.09985 (price didn't reach)

TIMEOUT (180 seconds):
- Both orders STILL pending
- direction = ""
- executedOrder = null
- pendingOrder = null

CANCELING BOTH ORDERS:
- CloseByTicket(789) → BuyStop canceled
- CloseByTicket(790) → SellStop canceled

FINAL BALANCE: $10000.00
PROFIT = $0.00

Orders didn't execute → no losses!
```

---

## 🧩 Components and their roles

### 1. NewsStraddleOrchestrator

**Role**: News straddle strategy coordinator

**Tasks**:

- Manages timing (countdown until news)
- Places symmetric straddle (BuyStop + SellStop)
- **MONITORS every SECOND** (faster than regular PendingBreakout)
- Detects 3 breakout scenarios (UPWARD, DOWNWARD, BOTH)
- Cancels opposite order (OCO mechanism)
- Handles timeout

### 2. Key timing parameters

```csharp
public int SecondsBeforeNews { get; set; } = 60;
// ↑ Countdown until news
// Launch orchestrator 60 seconds before exact time

public int MaxWaitAfterNewsSeconds { get; set; } = 180;
// ↑ Maximum time waiting for breakout
// News acts fast → 3 minutes is enough
```

### 3. MT5Sugar Extension Methods

```csharp
// Straddle placement:
BuyStopPoints(priceOffsetPoints: +15)   // Above price
SellStopPoints(priceOffsetPoints: -15)  // Below price

// Cancellation:
CloseByTicket(ticket)  // Cancel pending order
CloseAll(symbol)       // Close all positions
```

### 4. Breakout detection logic

```
KEY MECHANISM: Check ticket presence in pending list

OpenedOrdersTicketsAsync() returns list of ONLY pending orders.
When order EXECUTES → it becomes POSITION → disappears from list.

if (!buyStillPending && sellStillPending)
   → BuyStop DISAPPEARED (executed) → UPWARD breakout

else if (buyStillPending && !sellStillPending)
   → SellStop DISAPPEARED (executed) → DOWNWARD breakout

else if (!buyStillPending && !sellStillPending)
   → BOTH DISAPPEARED → BOTH (extreme volatility)
```

---

## 🔍 Final Dependency Diagram

```

│  USER CODE                                                  
│  var orch = new NewsStraddleOrchestrator(service);          
│  orch.SecondsBeforeNews = 60;  ← Launch 60 sec before       
│  await orch.ExecuteAsync();    ← @ 13:29:00 (NFP @ 13:30)   
└──────────────────────────┬──────────────────────────────────
                           │
                           ▼

│  NewsStraddleOrchestrator                                   
│    
│  │  ExecuteAsync() {                                       
│  │    1. Task.Delay(SecondsBeforeNews × 1000)              
│  │    2. SymbolInfoTickAsync() → current price            
│  │    3. BuyStopPoints(+StraddleDistance)                  
│  │    4. SellStopPoints(-StraddleDistance)                 
│  │    5. LOOP (every 1 sec, max 3 min):                    
│  │       - OpenedOrdersTicketsAsync()                      
│  │       - Check: which order disappeared?                 
│  │       - IF one disappeared → CloseByTicket(other)       
│  │    6. Task.Delay(60000)  ← Hold position                
│  │    7. CloseAll()                                        
│  │  }                                                      
│  └─────────────────────────────────────────────────────── 
└──────────────────────────┬──────────────────────────────────
                           │
                           ▼

│  MT5Sugar Extension Methods                                 
│  - BuyStopPoints(+offsetPoints)  → above Ask                
│  - SellStopPoints(-offsetPoints) → below Bid                
│  - CloseByTicket(ticket) → OCO cancellation                 
│  - CloseAll(symbol) → final closing                         
└──────────────────────────┬──────────────────────────────────
                           │
                           ▼
                    [MT5 Terminal]
```

---

## 🎯 Summary

**NewsStraddleOrchestrator is made of**:

1. **1 dependency**: `MT5Service _service`

2. **7 parameters**: Symbol, StraddleDistancePoints, Volume, SL, TP, **SecondsBeforeNews**, **MaxWaitAfterNewsSeconds**

3. **Key logic**:

   - Countdown until news (Task.Delay)
   - Symmetric straddle (BuyStop + SellStop)
   - **Monitoring every second** (faster than regular breakout!)
   - 3 scenarios: UPWARD, DOWNWARD, BOTH
   - OCO mechanism (cancel opposite)

**Works through**:

- Timing: launch 60 seconds before news
- Placing straddle right before news
- Fast monitoring (1 second instead of 3)
- Detecting breakout through ticket disappearance from pending list
- Automatic opposite order cancellation

**Returns**:
- `double profit` - difference between final and initial balance

**Key insight**:

Straddle allows **catching news volatility WITHOUT predicting direction**. By placing orders in both directions, we're guaranteed to catch the movement if it's strong enough. Fast monitoring (every second) is critically important for news - they act INSTANTLY!

**Success mathematics**:
```
R:R ratio = 1:2 (SL=20, TP=40)

Even with 40% win rate on news → profitable:
- 6 trades: 4 losses (-$32) + 2 wins (+$32) = $0
- 7 trades: 4 losses (-$32) + 3 wins (+$48) = +$16

News often gives strong directional movements → TP achievable!
```
