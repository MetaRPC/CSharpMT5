# HOW SimpleScalpingOrchestrator WORKS - Detailed Analysis

## 🎯 Document Purpose

Show **WHAT** the orchestrator consists of and **HOW EXACTLY** it works at the code, methods and data level. Special attention is paid to risk-sizing and automatic position size calculation.

---

## 📦 What the orchestrator is made of

### 1. Class structure (lines 13-27)

```csharp
public class SimpleScalpingOrchestrator
{
    
    // │  SINGLE DEPENDENCY                      
    // └─────────────────────────────────────────
    private readonly MT5Service _service;

    
    // │  6 CONFIGURABLE PARAMETERS              
    // └─────────────────────────────────────────
    public string Symbol { get; set; } = "EURUSD";
    public double RiskAmount { get; set; } = 20.0;      // $20 risk
    public int StopLossPoints { get; set; } = 10;
    public int TakeProfitPoints { get; set; } = 20;
    public bool IsBuy { get; set; } = true;
    public int MaxHoldSeconds { get; set; } = 60;       // 60 seconds

    
    // │  DEPENDENCY INJECTION                   
    // └─────────────────────────────────────────
    public SimpleScalpingOrchestrator(MT5Service service)
    {
        _service = service;  // ← Get MT5Service from outside
    }
}
```

### Dependency visualization

```

│        SimpleScalpingOrchestrator                          
│    
│  │  private readonly MT5Service _service                  
│  └───────────────────────────────────────────────────── 
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
    
    // │  STEP 1: Output header                              
    // └─────────────────────────────────────────────────────
    Console.WriteLine("\n+============================================================+");
    Console.WriteLine("|  SIMPLE SCALPING ORCHESTRATOR                             |");
    Console.WriteLine("+============================================================+\n");

    
    // │  STEP 2: Get initial balance                        
    // └─────────────────────────────────────────────────────
    var initialBalance = await _service.GetBalanceAsync();

    
    // │  STEP 3: Output strategy parameters                 
    // └─────────────────────────────────────────────────────
    Console.WriteLine($"  Starting balance: ${initialBalance:F2}");
    Console.WriteLine($"  Symbol: {Symbol}");
    Console.WriteLine($"  Direction: {(IsBuy ? "BUY" : "SELL")}");
    Console.WriteLine($"  Risk: ${RiskAmount:F2}");
    Console.WriteLine($"  SL: {StopLossPoints} pts | TP: {TakeProfitPoints} pts");
    Console.WriteLine($"  Max hold: {MaxHoldSeconds}s\n");
}
```

---

### Phase 2: Opening position with risk-sizing (lines 43-77)

```csharp
try
{
    
    // │  Opening market position                            
    // └─────────────────────────────────────────────────────
    Console.WriteLine("  Opening position...");
    OrderSendData result;

    
    // │  DIRECTION CHOICE: BUY or SELL                      
    // └─────────────────────────────────────────────────────
    if (IsBuy)
    {
        // ═══════════════════════════════════════════════════
        // BUY: Use BuyMarketByRisk
        // ═══════════════════════════════════════════════════
        result = await _service.BuyMarketByRisk(
            symbol: Symbol,             // "EURUSD"
            stopPoints: StopLossPoints, // 10 points
            riskMoney: RiskAmount,      // $20
            tpPoints: TakeProfitPoints, // 20 points
            comment: "Scalper"
        );
    }
    else
    {
        // ═══════════════════════════════════════════════════
        // SELL: Use SellMarketByRisk
        // ═══════════════════════════════════════════════════
        result = await _service.SellMarketByRisk(
            symbol: Symbol,
            stopPoints: StopLossPoints,
            riskMoney: RiskAmount,
            tpPoints: TakeProfitPoints,
            comment: "Scalper"
        );
    }

    
    // │  RESULT CHECK                                       
    // └─────────────────────────────────────────────────────
    if (result.ReturnedCode != 10009)  // 10009 = TRADE_RETCODE_DONE
    {
        Console.WriteLine($"  ✗ Order failed: {result.Comment}");
        return 0;  // ← EMERGENCY EXIT
    }

    Console.WriteLine($"  ✓ Position opened: #{result.Order}");
    Console.WriteLine($"  Volume: {result.Volume:F2} lots\n");
}
```

#### How BuyMarketByRisk() works internally

```csharp
// MT5Sugar.cs (extension method) — condensed, matching the real implementation
public static async Task<OrderSendData> BuyMarketByRisk(
    this MT5Service svc, string symbol, double stopPoints, double riskMoney,
    double? tpPoints = null, string? comment = null, int deviationPoints = 0,
    int timeoutSec = 20, CancellationToken ct = default)
{
    await svc.EnsureSelected(symbol, timeoutSec, ct);

    // STEP 1: size the volume from risk — delegated to CalcVolumeForRiskAsync.
    //   Conceptually: volume ≈ riskMoney / (stopPoints × money-per-point)
    //   e.g. $20 / (10 × $10) = 0.2 lots  (it also snaps to the broker's volume step/limits)
    var vol = await svc.CalcVolumeForRiskAsync(symbol, stopPoints, riskMoney, timeoutSec, ct);

    // STEP 2: SL/TP around the current quote
    var tick  = await svc.SymbolInfoTickAsync(symbol);
    var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint);
    double sl = tick.Bid - stopPoints * point;                       // 1.09990
    double tp = tpPoints.HasValue ? tick.Ask + tpPoints.Value * point : 0;  // 1.10020
    sl = await svc.NormalizePriceAsync(symbol, sl, timeoutSec, ct);
    tp = tp > 0 ? await svc.NormalizePriceAsync(symbol, tp, timeoutSec, ct) : 0;

    // STEP 3: send the market order (PlaceMarket wraps OrderSendAsync)
    return await svc.PlaceMarket(symbol, vol, isBuy: true, sl, tp, comment, deviationPoints, timeoutSec, ct);
}
```

> The "money-per-point" conversion lives inside `CalcVolumeForRiskAsync` (it uses the symbol's tick value / contract size), so the orchestrator never computes it by hand.

#### Complete call chain for BuyMarketByRisk

```

│  USER CODE (SimpleScalpingOrchestrator.cs:51)                  
│  await _service.BuyMarketByRisk(                               
│      symbol: "EURUSD",                                         
│      stopPoints: 10,         ← SL POINTS                       
│      riskMoney: 20.0,        ← RISK IN DOLLARS                 
│      tpPoints: 20,           ← TP POINTS                       
│      comment: "Scalper"                                        
│  )                                                             
└──────────────────────────┬─────────────────────────────────────
                           │
                           ▼

   MT5Sugar.BuyMarketByRisk() (extension method)                 
│    
│  │ 1. CalcVolumeForRiskAsync(symbol, stopPoints, riskMoney)  
│  │    → sizes volume from risk (money-per-point inside)      
│  │    = $20 / (10 × $10) ≈ 0.20 lots (snapped to step/limits)
│  │                                                            
│  │ 2. SymbolInfoTickAsync() → Bid/Ask                        
│  │    Ask = 1.10000, Bid = 1.10000                            
│  │                                                            
│  │ 3. SymbolInfoDoubleAsync(SymbolPoint) → point = 0.00001   
│  │                                                            
│  │ 4. Compute + normalize SL/TP:                             
│  │    sl = Bid - 10×0.00001 = 1.09990                         
│  │    tp = Ask + 20×0.00001 = 1.10020                         
│  └──────────────────────────────────────────────────────────  
│  await svc.PlaceMarket(                                        
│      symbol: "EURUSD",                                         
│      volume: 0.20,        ← from CalcVolumeForRiskAsync        
│      isBuy: true,                                              
│      sl: 1.09990, tp: 1.10020,                                 
│      comment: "Scalper")                                       
└──────────────────────────┬─────────────────────────────────────
                           │
                           ▼

│  MT5Service.PlaceMarket() (Sugar)                              
│  builds an OrderSendRequest, then                              
│  return await svc.OrderSendAsync(request)                      
└──────────────────────────┬─────────────────────────────────────
                           │
                           ▼

│  MT5Account.OrderSendAsync(OrderSendRequest)                   
│    
│  │ var request = new OrderSendRequest {                       
│  │     Symbol = "EURUSD",                                     
│  │     Volume = 0.20,        ← CALCULATED volume              
│  │     Operation = Tmt5OrderTypeBuy,                          
│  │     Price = Ask,                                           
│  │     StopLoss = 1.09990,                                    
│  │     TakeProfit = 1.10020,                                  
│  │     Comment = "Scalper"                                    
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
                  │  │ Opens          
                  │  │ BUY 0.20       
                  │  │ @ 1.10000      
                  │  │ SL: 1.09990    
                  │  │ TP: 1.10020    
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
                    │   123456789, 
                    │   Volume =   
                    │   0.20        ← CALCULATED!
                    │   Price =    
                    │   1.10000    
                    │ }            
                    └──────┬───────
                           │
                           ▼
         
         │  BACK to SimpleScalpingOrchestrator 
         │  var result = OrderSendData {       
         │      ReturnedCode = 10009           
         │      Order = 123456789              
         │      Volume = 0.20                   ← See calculated volume!
         │  }                                  
         └─────────────────────────────────────
```

---

### Phase 3: Holding position (lines 79-81)

```csharp

// │  Wait MaxHoldSeconds (default 60 seconds)               
// └─────────────────────────────────────────────────────────
Console.WriteLine($"  ⏳ Holding for {MaxHoldSeconds}s...\n");

await Task.Delay(MaxHoldSeconds * 1000, ct);

// MaxHoldSeconds = 60 → Task.Delay(60000 ms) = 60 seconds
```

**What happens during these 60 seconds**:

```
T=0      Position opened BUY 0.20 @ 1.10000
         SL: 1.09990, TP: 1.10020
         │
         ├─► MT5 Terminal AUTOMATICALLY monitors price
         │
T=5s     Price: 1.10005 (+5 pts in profit)
T=10s    Price: 1.10012 (+12 pts in profit)
T=15s    Price: 1.10020 (TP REACHED!)
         │
         └─► MT5 Terminal AUTOMATICALLY closes position
             Profit: +20 pts × 0.20 = +$40

OR

T=5s     Price: 1.09995 (-5 pts in loss)
T=8s     Price: 1.09990 (SL REACHED!)
         │
         └─► MT5 Terminal AUTOMATICALLY closes position
             Loss: -10 pts × 0.20 = -$20 (exactly riskMoney!)

OR

T=60s    Price: 1.10008 (+8 pts in profit)
         Neither SL nor TP triggered
         → Proceed to next phase (status check)
```

---

### Phase 4: Checking position status (lines 83-104)

```csharp

// │  Get list of OPEN POSITIONS                             
// └─────────────────────────────────────────────────────────
var tickets = await _service.OpenedOrdersTicketsAsync();

bool stillOpen = false;


// │  Check: is our position in the list?                    
// └─────────────────────────────────────────────────────────
foreach (var ticket in tickets.OpenedPositionTickets)  // ← POSITIONS, not ORDERS!
{
    if (ticket == (long)result.Order)  // Compare ticket
    {
        stillOpen = true;
        break;
    }
}


// │  CONDITION: Position still open?                        
// └─────────────────────────────────────────────────────────
if (stillOpen)
{
    // ═══════════════════════════════════════════════════════
    // SCENARIO 1: Position NOT closed after 60 seconds
    // ═══════════════════════════════════════════════════════
    Console.WriteLine($"  Position still open after {MaxHoldSeconds}s - closing manually...");

    
    // │  Manual close at current market price           
    // └─────────────────────────────────────────────────
    await _service.CloseByTicket(result.Order);

    Console.WriteLine("  ✓ Position closed");
}
else
{
    // ═══════════════════════════════════════════════════════
    // SCENARIO 2: Position ALREADY closed
    // ═══════════════════════════════════════════════════════
    Console.WriteLine("  ✓ Position closed automatically (SL/TP hit)");
}
```

#### Visualization of status check

```text
AFTER Task.Delay(60 seconds):

MT5 Terminal state — SCENARIO 1 (position still open):
┌──────────────────┬────────────────────────────────────────┐
│  PENDING ORDERS  │  OPEN POSITIONS                        │
├──────────────────┼────────────────────────────────────────┤
│  (empty)         │  123456789: BUY 0.20 EURUSD @ 1.10000  │
│                  │             (in profit +8 pts)         │
└──────────────────┴────────────────────────────────────────┘

OpenedOrdersTicketsAsync() returns:
{
    OpenedOrdersTickets: [],
    OpenedPositionTickets: [123456789]  ← OUR POSITION FOUND!
}

foreach (var ticket in tickets.OpenedPositionTickets)
{
    // ticket = 123456789
    if (ticket == 123456789) stillOpen = true;  ← TRUE!
}

if (stillOpen)  ← TRUE → Manual close
{
    CloseByTicket(123456789);
}

─────────────────────────────────────────────────────────────

MT5 Terminal state — SCENARIO 2 (position already closed):
┌──────────────────┬────────────────────────────────────────┐
│  PENDING ORDERS  │  OPEN POSITIONS                        │
├──────────────────┼────────────────────────────────────────┤
│  (empty)         │  (empty)                               │
└──────────────────┴────────────────────────────────────────┘
                     ↑
                     Position closed (TP triggered @ T=15s)

OpenedOrdersTicketsAsync() returns:
{
    OpenedOrdersTickets: [],
    OpenedPositionTickets: []  ← POSITION NOT FOUND!
}

foreach (var ticket in tickets.OpenedPositionTickets)
{
    // Empty list → loop doesn't execute
}

stillOpen = false;  ← Remains false

if (stillOpen)  ← FALSE → Skip manual close
else
{
    Console.WriteLine("Position closed automatically (SL/TP hit)");
}
```

---

### Phase 5: Finalization (lines 106-113)

```csharp

// │  Get final balance                                      
// └─────────────────────────────────────────────────────────
var finalBalance = await _service.GetBalanceAsync();


// │  Calculate profit/loss                                  
// └─────────────────────────────────────────────────────────
var profit = finalBalance - initialBalance;

Console.WriteLine($"\n  Final balance: ${finalBalance:F2}");
Console.WriteLine($"  Profit/Loss: ${profit:F2}");


// │  Return profit as result of ExecuteAsync()              
// └─────────────────────────────────────────────────────────
return profit;
```

---

## 🎭 Complete Life Cycle (TP triggered scenario)

### Execution timeline

```
T=0      START ExecuteAsync()
         │
         ├─► GetBalanceAsync()           → $10000.00
         │
T=1s     ├─► BuyMarketByRisk(risk=$20, SL=10pts, TP=20pts)
         │   │
         │   ├─► CalcVolumeForRiskAsync("EURUSD", 10, $20)
         │   │   └─► volume ≈ 0.2 lots
         │   │
         │   ├─► SymbolInfoTickAsync("EURUSD") → Ask=1.10000
         │   ├─► SymbolInfoDoubleAsync(SymbolPoint) → 0.00001
         │   │
         │   ├─► CALCULATE SL/TP:
         │   │   sl = 1.10000 - (10 × 0.00001) = 1.09990
         │   │   tp = 1.10000 + (20 × 0.00001) = 1.10020
         │   │
         │   └─► PlaceMarket(vol=0.2, isBuy:true, sl, tp) → OrderSendAsync
         │       └─► gRPC → MT5 Terminal
         │
T=2s     ├─► MT5 Terminal: Position opened!
         │   
         │   │ BUY 0.20 EURUSD @ 1.10000        
         │   │ SL: 1.09990                      
         │   │ TP: 1.10020                      
         │   │ Ticket: 123456789                
         │   └──────────────────────────────────
         │
         │   result.ReturnedCode = 10009 ✓
         │   result.Order = 123456789
         │   result.Volume = 0.20
         │
T=3s     ├─► Task.Delay(60000) START
         │   "Holding for 60s..."
         │
         │   MARKET: Price moves...
         │   1.10000 → 1.10005 → 1.10012 → 1.10020...
         │
T=15s    │   MT5 Terminal: Price reached 1.10020!
         │   TP TRIGGERED!
         │   
         │   │ Position closed automatically    
         │   │ Entry: 1.10000                   
         │   │ Exit:  1.10020                   
         │   │ Profit: +20 pts × 0.20 = +$40    
         │   └──────────────────────────────────
         │
         │   MT5 Terminal state:
         │   ┌──────────────────┬──────────────┐
         │   │ PENDING ORDERS   │ POSITIONS    │
         │   ├──────────────────┼──────────────┤
         │   │ (empty)          │ (empty)      │
         │   └──────────────────┴──────────────┘
         │
T=63s    ├─► Task.Delay(60000) END
         │
         ├─► OpenedOrdersTicketsAsync()
         │   └─► OpenedPositionTickets = []
         │
         ├─► foreach (var ticket in [])  ← Empty list
         │   └─► stillOpen = false
         │
         ├─► if (stillOpen) → FALSE
         │   else → "Position closed automatically (SL/TP hit)"
         │
T=64s    ├─► GetBalanceAsync()           → $10040.00
         ├─► profit = 10040.00 - 10000.00 = +$40.00
         │
         └─► RETURN profit = 40.00
```

---

## 📊 What the result is made of

### Profit calculation (TP triggered)

```
INITIAL BALANCE: $10000.00

POSITION:
- Direction: BUY
- Volume: 0.20 lots (AUTOMATICALLY CALCULATED!)
- Entry: 1.10000
- SL: 1.09990 (-10 pts)
- TP: 1.10020 (+20 pts)

TP TRIGGERED @ T=15s:
- Exit: 1.10020
- Pips: (1.10020 - 1.10000) / 0.00001 = 20 points

PROFIT CALCULATION:
- Profit = Pips × Point Value × Volume
- Point Value for EURUSD (0.20 lots) = 0.20 × $10 = $2 per point
- Profit = 20 × $2 = +$40

FINAL BALANCE: $10040.00
PROFIT = $40.00

return 40.00;
```

### Loss calculation (SL triggered)

```
SL TRIGGERED @ T=8s:
- Exit: 1.09990
- Pips: (1.09990 - 1.10000) / 0.00001 = -10 points

LOSS CALCULATION:
- Loss = -10 × $2 = -$20

FINAL BALANCE: $9980.00
PROFIT = -$20.00

CRITICALLY IMPORTANT:
Loss EXACTLY = riskMoney ($20)!
This proves correctness of Volume calculation through risk-sizing!
```

---

## 🧩 Components and their roles

### 1. SimpleScalpingOrchestrator

**Role**: Scalping strategy coordinator

**Tasks**:

- Stores parameters (Symbol, RiskAmount, SL, TP, IsBuy, MaxHoldSeconds)
- Manages life cycle
- Chooses direction (BUY/SELL)
- **Does NOT calculate Volume** (delegates to BuyMarketByRisk)
- Monitors holding time
- Checks position status
- Closes manually if needed
- Returns result

### 2. MT5Sugar (extension methods)

**Role**: **KEY** component for risk-sizing

**Tasks**:

- `BuyMarketByRisk()` - **AUTOMATICALLY calculates Volume**
- `SellMarketByRisk()` - **AUTOMATICALLY calculates Volume**
- `CloseByTicket()` - universal close

### 3. Risk-sizing formula

```

│  VOLUME CALCULATION FORMULA:                                
│                                                             
│  Volume = RiskMoney / (StopPoints × PointValueUSD)          
│                                                             
│  Where:                                                     
│  - RiskMoney: Maximum risk in dollars ($20)                 
│  - StopPoints: SL size in points (10)                    
│  - PointValueUSD: Value of 1 point for 1.0 lot ($10)        
└─────────────────────────────────────────────────────────────

EXAMPLES:

Example 1: Tight SL
  RiskMoney = $20
  StopPoints = 5  ← Very close SL
  PointValueUSD = $10

  Volume = $20 / (5 × $10) = $20 / $50 = 0.4 lots
  ↑ LARGER volume to compensate small SL!

Example 2: Wide SL
  RiskMoney = $20
  StopPoints = 20  ← Wide SL
  PointValueUSD = $10

  Volume = $20 / (20 × $10) = $20 / $200 = 0.1 lots
  ↑ SMALLER volume to keep risk at $20!

Example 3: High risk
  RiskMoney = $50  ← Increased risk
  StopPoints = 10
  PointValueUSD = $10

  Volume = $50 / (10 × $10) = $50 / $100 = 0.5 lots
  ↑ Proportionally larger volume!
```

---

## 🎯 Summary

**SimpleScalpingOrchestrator is made of**:

1. **1 dependency**: `MT5Service _service`
2. **6 parameters**: Symbol, **RiskAmount**, StopLossPoints, TakeProfitPoints, IsBuy, MaxHoldSeconds
3. **3 key methods**:
   - `BuyMarketByRisk` / `SellMarketByRisk` ← **AUTOMATIC Volume calculation!**
   - `OpenedOrdersTicketsAsync` ← Position status check
   - `CloseByTicket` ← Manual close

**Works through**:

- Market entry with automatic risk-sizing
- Holding position for MaxHoldSeconds seconds
- Check: did position close automatically (SL/TP)?
- Manual close if position still open

**Returns**:

- `double profit` - difference between final and initial balance

**Key insight**:

All the magic of the orchestrator is in **delegating Volume calculation to BuyMarketByRisk method**. Orchestrator only specifies RISK ($20), and position volume is calculated automatically based on StopLoss size. This ensures **constant risk** regardless of SL parameter changes!

**Success formula**:
```
CONSTANT RISK = RiskMoney
VARIABLE VOLUME = f(RiskMoney, StopPoints)

If SL increases → Volume decreases
If SL decreases → Volume increases
Risk ALWAYS remains = $20!
```
