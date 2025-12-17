# CSharpMT5 Glossary

> Project-specific terms and concepts. This glossary covers CSharpMT5 architecture, components, and trading automation terminology used throughout the codebase.

---

## 🏗️ Architecture Terms

### Three-Layer Architecture

The core design pattern of CSharpMT5 with three abstraction layers:

- **Layer 1 (MT5Account):** Low-level gRPC communication with MT5 terminal
- **Layer 2 (MT5Service):** Wrapper methods with simplified signatures
- **Layer 3 (MT5Sugar):** High-level convenience methods with auto-normalization

**Usage:** Start with Layer 3 (MT5Sugar), drop down only when needed.

---

### MT5Account
**Layer 1 - Low-level API**

The foundational layer providing direct access to MT5 terminal via gRPC protocol.

**Key characteristics:**

- Raw gRPC calls to MT5 terminal
- Built-in connection resilience with automatic reconnection
- Works with proto-generated Request/Response objects
- Full control, maximum complexity
- Async/Sync method variants

**When to use:** Custom integrations, proto-level control needed, building custom wrappers.

**Location:** `MT5Account.cs`

**Documentation:** [MT5Account.Master.Overview.md](./MT5Account/MT5Account.Master.Overview.md)

---

### MT5Service
**Layer 2 - Wrapper API**

Middle layer providing simplified method signatures without proto complexity.

**Key characteristics:**

- Direct data returns (no proto objects in signatures)
- Type conversions (proto → C# primitives/objects)
- Simplified method names
- No auto-normalization (you control precision)
- Extension methods for convenience

**When to use:** Need wrappers but not auto-normalization, building custom strategies.

**Location:** `MT5Service.cs`

**Documentation:** [MT5Service.Overview.md](MT5Service/MT5Service.Overview.md)

---

### MT5Sugar
**Layer 3 - Convenience API** ⭐

Highest-level API with ~50 convenience methods for common trading operations.

**Key characteristics:**

- Auto-normalization of volumes and prices
- Risk management helpers
- Batch operations (CloseAll, CancelAll)
- Points-based helpers (BuyLimitPoints, SellStopPoints, etc.)
- Snapshots (GetAccountSnapshot, GetSymbolSnapshot)
- Simplest API, handles edge cases

**When to use:** 95% of cases - easiest starting point.

**Location:** `MT5Sugar.cs`

**Documentation:** [MT5Sugar documentation](./MT5Sugar/MT5Sugar.API_Overview.md)

---

## 🎯 Strategy Components

### Orchestrator
Pre-built trading strategy implementation that automates complete trading workflow.

**Key characteristics:**

- Single strategy focus (grid trading, scalping, hedging, etc.)
- Risk-based position sizing where applicable
- Position monitoring loops with progress bars
- Automatic exits and cleanup on Ctrl+C
- Performance tracking (balance, equity, P/L)
- Configurable parameters via properties

**Examples:**

- `GridTradingOrchestrator` - Grid trading for range-bound markets
- `SimpleScalpingOrchestrator` - Quick scalping with tight stops
- `QuickHedgeOrchestrator` - Hedging strategy for high volatility
- `NewsStraddleOrchestrator` - Breakout trading around news events
- `PendingBreakoutOrchestrator` - Pending orders for breakout trading

**Location:** `Examples\Orchestrators\`

**Documentation:** [Strategies.Master.Overview.md](./Strategies/Strategies.Master.Overview.md)

**Command:** `dotnet run <orchestrator-name>` (e.g., `dotnet run grid`)

---

### Preset
Multi-orchestrator combination with adaptive decision-making logic based on market analysis.

**Key characteristics:**

- Combines multiple orchestrators (2-5 strategies)
- Adaptive logic based on market conditions (volatility, time, news)
- Multi-phase trading sessions
- Performance tracking across phases
- Automatic strategy selection and execution

**Examples:**

- `AdaptiveMarketModePreset` - Intelligent system that analyzes market and selects optimal orchestrator

**Location:** `Examples\Presets\`

**Documentation:** [AdaptiveMarketModePreset.md](./Strategies/Presets/AdaptiveMarketModePreset.md)

**Command:** `dotnet run preset`

---

## 🔧 Technical Concepts

### Auto-Normalization
Automatic adjustment of trading parameters to broker requirements.

**What gets normalized:**

- **Volumes:** Rounded to broker's volume step (e.g., 0.01 lots)
- **Prices:** Rounded to symbol's tick size/digits (e.g., 5 decimal places for EURUSD)
- **Stop Loss / Take Profit:** Adjusted to symbol precision

**Where:** MT5Sugar layer only (MT5Service doesn't auto-normalize)

**Example:**
```csharp
// You pass: volume=0.0234, price=1.09876543
// Sugar normalizes to: volume=0.02, price=1.09877
await sugar.BuyMarket("EURUSD", 0.0234, sl: 1.09876543, tp: 1.10000);
```

**Methods:**

- `NormalizePriceAsync(symbol, price)` - Normalize price to symbol digits
- `NormalizeVolumeAsync(symbol, volume)` - Normalize volume to broker step

---

### Risk-Based Volume Calculation
Position sizing based on dollar risk rather than fixed lot size.

**Formula:** `volume = riskAmount / (stopLossPoints × pointValue)`

**Parameters:**

- `riskAmount` - Dollar amount willing to risk (e.g., $50)
- `stopLossPoints` - Distance to SL in points (e.g., 50 points)
- Result: Lot size that risks exactly $50 if SL hit

**Methods (MT5Sugar):**
```csharp
// Calculate volume for given risk
double volume = await sugar.CalculateVolume(symbol, slPoints, riskAmount);

// Buy with risk-based sizing
var result = await sugar.BuyByRisk(symbol, slPoints, riskAmount, tpPrice);

// Sell with risk-based sizing
var result = await sugar.SellByRisk(symbol, slPoints, riskAmount, tpPrice);
```

**Used in:** `SimpleScalpingOrchestrator`

---

### Points vs Pips
**Point:** Smallest price movement for a symbol (1 tick).

**Pip:** Traditional forex unit (0.0001 for most pairs).

**Relationship:**

- **5-digit brokers:** 1 pip = 10 points (EURUSD: 1.10000 → 1.10010 = 1 pip)
- **3-digit brokers:** 1 pip = 1 point (USDJPY: 110.00 → 110.01 = 1 pip)

**In CSharpMT5:** All APIs use **points** for consistency.

**Conversion:**
```csharp
double point = await service.GetPointAsync("EURUSD");  // 0.00001
double pips = points / 10.0;  // For 5-digit pairs
```

**Why points?** Universal across all instruments (forex, metals, indices, crypto).

---

### Points-Based Methods
Convenience methods that calculate prices using point offsets from current market price.

**Methods:**

- `BuyLimitPoints(symbol, volume, pointsOffset, slPoints, tpPoints)` - Buy Limit below Ask
- `SellLimitPoints(symbol, volume, pointsOffset, slPoints, tpPoints)` - Sell Limit above Bid
- `BuyStopPoints(symbol, volume, pointsOffset, slPoints, tpPoints)` - Buy Stop above Ask
- `SellStopPoints(symbol, volume, pointsOffset, slPoints, tpPoints)` - Sell Stop below Bid

**Example:**
```csharp
// Place Buy Limit 20 points below current Ask
// with 50-point SL and 30-point TP
var result = await sugar.BuyLimitPoints("EURUSD", 0.01,
    priceOffsetPoints: 20,
    slPoints: 50,
    tpPoints: 30);
```

**Benefits:**

- No manual price calculations needed
- Automatic normalization
- Clearer intent in code
- Less error-prone than absolute prices

**Used in:** All orchestrators, pending order operations

---

### Trailing Stop
Dynamic Stop Loss that follows price in profit direction.

**How it works:**

1. Position opens with initial SL
2. When profit reaches threshold (e.g., +40 points)
3. SL moves to breakeven or better
4. SL continues to trail price at fixed distance
5. Locks in profits as price moves favorably

**Implementation:**
```csharp
// In monitoring loop
if (currentProfit >= trailingThreshold)
{
    double newSL = currentPrice - trailingDistance * point;
    if (newSL > currentSL)
    {
        await service.ModifyPosition(ticket, newSL, tp);
    }
}
```

**Used in:** Custom trend-following strategies

---

### Hedging
Opening opposite position to lock in current profit/loss level.

**How it works:**

1. Primary position opened (e.g., BUY EURUSD 0.1 lots)
2. Price moves against you (-50 points)
3. Hedge triggered: SELL EURUSD 0.1 lots
4. Net position = 0 (locked loss at -50 points level)

**Purpose:**

- Lock losses instead of closing at stop loss
- Protect position during volatility/news
- Wait for better exit opportunity

**Used in:** `QuickHedgeOrchestrator`

**⚠️ Note:** Not all brokers/regulations allow hedging. US brokers typically don't support hedging.

---

### Pending Order
Order that executes automatically when price reaches specified level.

**Types:**

- **BUY LIMIT:** Buy at price BELOW current (expecting pullback then up)
- **SELL LIMIT:** Sell at price ABOVE current (expecting rally then down)
- **BUY STOP:** Buy at price ABOVE current (breakout up)
- **SELL STOP:** Sell at price BELOW current (breakout down)

**Methods (absolute price):**
```csharp
await service.BuyLimit(symbol, volume, price, sl, tp);
await service.SellLimit(symbol, volume, price, sl, tp);
```

**Methods (points-based):**
```csharp
await sugar.BuyLimitPoints(symbol, volume, pointsOffset, slPoints, tpPoints);
await sugar.SellStopPoints(symbol, volume, pointsOffset, slPoints, tpPoints);
```

**Used in:** `GridTradingOrchestrator`, `PendingBreakoutOrchestrator`, `NewsStraddleOrchestrator`

---

### Retry Logic with Exponential Backoff
Automatic retry mechanism for handling intermittent connection errors.

**How it works:**

1. Operation fails with connection error (TRADE_RETCODE_CONNECTION = 10031)
2. Wait 1 second, retry
3. If fails again, wait 2 seconds, retry
4. If fails again, wait 4 seconds, retry
5. After 3 attempts, throw exception

**Implementation:**
```csharp
private static async Task<T> PlaceOrderWithRetry<T>(
    Func<Task<T>> orderFunc,
    string orderName,
    int maxRetries = 3)
{
    int delayMs = 1000;
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        var result = await orderFunc();
        if (result.RetCode == 10031 && attempt < maxRetries)
        {
            Console.WriteLine($"⚠ Retry in {delayMs}ms...");
            await Task.Delay(delayMs);
            delayMs *= 2;  // Exponential backoff
            continue;
        }
        return result;
    }
}
```

**Used in:** `Program.Sugar.PendingOrders.cs` (demo file with retry logic)

**Benefits:**

- Handles temporary network issues
- Reduces failed operations
- Prevents cascading failures

---

## 🔌 gRPC & Protocol Terms

### gRPC
High-performance RPC (Remote Procedure Call) framework using HTTP/2.

**In CSharpMT5:**

- MT5Account layer sends gRPC requests to MT5 terminal
- Terminal runs gRPC server (configured via gateway or terminal plugin)
- Request/Response pattern for all operations
- Async/await throughout the stack

**Connection setup:**
```csharp
var config = ConnectionHelper.BuildConfiguration();
var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);
```

**Configuration:** `Config\appsettings.json`

---

### MT5 Gateway (mt5term)
External gateway process that bridges CSharpMT5 to MT5 terminal via gRPC.

**Key points:**

- User connects through gateway, not directly to terminal
- Gateway handles connection pooling and session management
- No need to launch MT5 terminal separately when using gateway
- Requires gateway to be running before connecting

**Connection flow:**
```
CSharpMT5 → gRPC → mt5term Gateway → MT5 Terminal
```

**Rate limiting:** Gateway may limit request frequency to prevent broker throttling.

---

### Async/Await Pattern
C# asynchronous programming pattern used throughout CSharpMT5.

**All methods have two versions:**
```csharp
// Async version (recommended - 99% of cases)
var result = await account.OrderSendAsync(request);

// Sync version (legacy/specific scenarios)
var result = account.OrderSend(request);
```

**Why async?**
- Non-blocking I/O for gRPC calls
- Better performance with multiple operations
- Scalable for high-frequency trading

**Documentation:** [Sync_vs_Async.md](Sync_vs_Async.md)

---

### Return Codes
Proto return codes indicating operation success/failure.

**Common codes:**

- **10009** = Success (TRADE_RETCODE_DONE)
- **10004** = Requote
- **10006** = Request rejected
- **10013** = Invalid request
- **10014** = Invalid volume
- **10015** = Invalid price
- **10016** = Invalid stops
- **10018** = Market closed
- **10019** = Not enough money
- **10031** = No connection with trade server

**Always check `ReturnedCode` or `RetCode` in trading operations.**

**Documentation:** [ReturnCodes_Reference_EN.md](./ReturnCodes_Reference_EN.md)

---

## 🎓 Trading Terms (Project Context)

### Risk Amount
Dollar amount willing to lose if Stop Loss hit.

**Example:** Risk $50 per trade means if SL triggered, you lose exactly $50.

**Used in:**

- `SimpleScalpingOrchestrator` for position sizing
- `CalculateVolume()`, `BuyByRisk()`, `SellByRisk()` methods

---

### Breakeven
Moving Stop Loss to entry price to eliminate risk.

**Example:**

- Entry: BUY at 1.10000
- Price rises to 1.10050 (+50 points profit)
- Move SL from 1.09950 to 1.10000 (breakeven)
- Now risk-free: worst case = break even

**Implementation:**
```csharp
if (currentProfit >= breakevenThreshold)
{
    await service.ModifyPosition(ticket, entryPrice, tp);
}
```

---

### Grid Trading
Strategy that places multiple pending orders at equal intervals above and below current price.

**How it works:**

1. Place Buy Limit orders below current price (e.g., every 20 points)
2. Place Sell Limit orders above current price (e.g., every 20 points)
3. When price oscillates, orders trigger and close with TP
4. Works best in range-bound markets

**Example:**
```
Price: 1.10000
Buy Limits:  1.09980 (-20 pts), 1.09960 (-40 pts), 1.09940 (-60 pts)
Sell Limits: 1.10020 (+20 pts), 1.10040 (+40 pts), 1.10060 (+60 pts)
```

**Used in:** `GridTradingOrchestrator`

**⚠️ Risk:** High volatility can trigger many orders, increasing exposure.

---

### Scalping
Quick trading strategy with small profits (5-25 points) and tight stops (10-20 points).

**Characteristics:**

- Very short hold times (seconds to minutes)
- High win rate, small profit per trade
- Risk:Reward typically 1:1 to 1:2
- Requires low spreads

**Used in:** `SimpleScalpingOrchestrator`

---

### News Trading (Straddle)
Placing orders on both sides of price before high-impact news events.

**How it works:**

1. Before news: Place Buy Stop above + Sell Stop below
2. News released: Price breaks out in one direction
3. One order triggers, cancel the other
4. Capture volatility spike

**Example:**
```
Current price: 1.10000
Buy Stop:  1.10030 (+30 points)
Sell Stop: 1.09970 (-30 points)
News → Price spikes to 1.10050 → Buy Stop triggers → Cancel Sell Stop
```

**Used in:** `NewsStraddleOrchestrator`

**⚠️ Risk:** Slippage during news can cause both orders to trigger.

---

### Volume Limits
Broker-specific constraints on position sizing.

**Retrieved via:**
```csharp
(double minVol, double maxVol, double stepVol) =
    await service.GetVolumeLimitsAsync(symbol);

// Example: minVol=0.01, maxVol=100.0, stepVol=0.01
```

**Used for:** Auto-normalization in MT5Sugar.

**Validation:**
```csharp
if (volume < minVol)
    volume = minVol;
if (volume > maxVol)
    volume = maxVol;

// Round to step
volume = Math.Round(volume / stepVol) * stepVol;
```

---

## 📁 File Organization Terms

### Examples
Runnable demonstration code showing API usage patterns at different layers.

**Structure:**

- `Examples\LowLevel\` - MT5Account examples (proto level)
- `Examples\Service\` - MT5Service examples (wrapper level)
- `Examples\Sugar\` - MT5Sugar examples (convenience level)
- `Examples\Orchestrators\` - Strategy implementations
- `Examples\Presets\` - Multi-strategy systems

**How to run:**
```bash
dotnet run positions       # Service layer example
dotnet run pendingorders   # Sugar layer example
dotnet run grid            # Grid trading orchestrator
dotnet run preset          # Adaptive preset
```

**Location:** `Examples\`

---

### Program.cs
Main entry point that routes `dotnet run` commands to appropriate examples/orchestrators/presets.

**Key characteristics:**

- Single entry point for all runnable code
- Command routing based on aliases (grid, scalping, preset, etc.)
- Provides helpful error messages for unknown commands
- Handles Ctrl+C gracefully for orchestrators

**How it works:**
```
dotnet run grid
    ↓
Program.cs Main(args)
    ↓
RouteCommand("grid")
    ↓
Examples.Orchestrators.GridTradingOrchestrator.RunAsync()
```

**Location:** `Program.cs`

**Available commands:** See header comment in `Program.cs` for complete list.

---

### ConnectionHelper
Utility class for building MT5 connection configuration and establishing connections.

**Methods:**
```csharp
// Build configuration from appsettings.json
var config = ConnectionHelper.BuildConfiguration();

// Create and connect to MT5
var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);
```

**What it does:**

- Reads `Config\appsettings.json`
- Validates connection settings
- Creates MT5Account instance
- Establishes gRPC connection
- Returns connected account

**Location:** `Examples\Helpers\ConnectionHelper.cs`

---

### ProgressBarHelper
Visual progress bar for orchestrators during waiting periods.

**Features:**

- Shows countdown with time remaining
- Displays progress bar ([=====>    ])
- Clears line to prevent duplication
- Works well with terminal scrolling

**Usage:**
```csharp
await ProgressBarHelper.ShowProgressBar(
    durationSeconds: 60,
    message: "Monitoring positions",
    cancellationToken: cts.Token
);
```

**Location:** `Examples\Helpers\ProgressBarHelper.cs`

**Used in:** All orchestrators for visual feedback during runtime.

---

## ⚙️ Configuration Terms

### appsettings.json
Central configuration file for MT5 connection settings.

**Key settings:**

- `Host` - MT5 terminal/gateway host (usually localhost)
- `Port` - gRPC server port (e.g., 5555)
- `Login` - MT5 account number
- `Password` - MT5 account password
- `Symbol` - Default trading symbol (e.g., "EURUSD")
- `UseSSL` - SSL/TLS encryption (true/false)
- `TimeoutSeconds` - Request timeout (default 30)

**Location:** `Config\appsettings.json`

**⚠️ Security:** Never commit real credentials to git (add to .gitignore).

**Example:**
```json
{
  "MT5Connection": {
    "Host": "localhost",
    "Port": 5555,
    "Login": 12345678,
    "Password": "your_password",
    "Symbol": "EURUSD",
    "UseSSL": false,
    "TimeoutSeconds": 30
  }
}
```

---

### .csproj File
C# project file containing build configuration, dependencies, and metadata.

**Key elements:**

- `<TargetFramework>net8.0</TargetFramework>` - .NET version
- `<PackageReference>` - NuGet package dependencies (Grpc.Net.Client, etc.)
- `<PropertyGroup>` - Compiler settings

**Location:** `mt5_term_api.csproj`

**Build commands:**
```bash
dotnet build        # Compile project
dotnet clean        # Clean build artifacts
dotnet restore      # Restore NuGet packages
```

---

### bin/ and obj/ Folders
Build output directories containing compiled files.

**Contents:**

- `bin/` - Final compiled assemblies (.dll, .exe)
- `obj/` - Intermediate build files

**Purpose:**

- Auto-generated during `dotnet build` or `dotnet run`
- Can be safely deleted for clean rebuild
- Excluded from git via .gitignore

**Troubleshooting:**
```bash
# If you get weird compilation errors:
dotnet clean        # Clean build outputs
dotnet build        # Rebuild from scratch
```

---

## 🔗 Cross-Component Terms

### Snapshot
Complete state capture at a point in time.

**Types:**

- **Account Snapshot:** Balance, equity, margin, profit, positions count, leverage
- **Symbol Snapshot:** Bid, ask, spread, point size, digits, volume limits, tick data

**Methods (MT5Sugar):**
```csharp
var accountSnap = await sugar.GetAccountSnapshot();
Console.WriteLine($"Balance: {accountSnap.Summary.AccountBalance}");
Console.WriteLine($"Equity: {accountSnap.Summary.AccountEquity}");

var symbolSnap = await sugar.GetSymbolSnapshot("EURUSD");
Console.WriteLine($"Bid: {symbolSnap.Tick.Bid}");
Console.WriteLine($"Ask: {symbolSnap.Tick.Ask}");
Console.WriteLine($"Spread: {(symbolSnap.Tick.Ask - symbolSnap.Tick.Bid) / symbolSnap.Point} points");
```

**Use case:** Dashboards, logging, performance tracking, orchestrator monitoring.

---

### Batch Operations
Execute action on multiple positions/orders at once.

**Methods (MT5Sugar):**
```csharp
// Close positions
int closed = await sugar.CloseAll();                    // All positions
int closedBuy = await sugar.CloseAllBuy();              // Only BUY
int closedSell = await sugar.CloseAllSell();            // Only SELL
int closedSymbol = await sugar.CloseAllPositions(symbol); // By symbol

// Cancel orders
int cancelled = await sugar.CancelAll();                // All pending orders
int cancelledSymbol = await sugar.CancelAll(symbol);    // By symbol
```

**Use case:** Emergency exits, end-of-session cleanup, strategy resets.

**Used in:** All orchestrators for cleanup on exit.

---

### History Queries
Retrieve past orders and positions for analysis.

**Methods (MT5Account):**
```csharp
// Get order history with pagination
var orders = await account.OrderHistoryAsync(
    fromDate,
    toDate,
    page,
    itemsPerPage
);

// Get position history
var positions = await account.PositionsHistoryAsync(
    fromDate,
    toDate,
    page,
    itemsPerPage
);
```

**Use case:** Performance analysis, trade logs, strategy backtesting validation.

---

## 🎯 Project Philosophy Terms

### Progressive Complexity
Design principle: start simple, access complexity only when needed.

**In CSharpMT5:**

- Start with MT5Sugar (simplest)
- Drop to MT5Service if need wrappers without auto-normalization
- Drop to MT5Account for proto-level control

**User journey:** Sugar → Service → Account (as needed).

**Benefits:**

- Lower learning curve
- Faster development for common tasks
- Full power available when needed

---

### Educational Project

CSharpMT5 orchestrators and presets are learning materials and API demonstrations, NOT production trading systems.

**Implications:**

- ✅ Study code and patterns
- ✅ Modify for your needs
- ✅ Test on demo accounts
- ✅ Use as templates for building your strategies
- ❌ Don't use as-is with real money
- ❌ Don't expect production-grade risk management
- ❌ Don't expect proper error handling for all edge cases

**See:** Disclaimer in [Strategies.Master.Overview.md](./Strategies/Strategies.Master.Overview.md)

---

### Demo Account First

**ALWAYS test on demo accounts before live trading.**

**Why:**

- No financial risk
- Test strategies without pressure
- Validate code works with your broker
- Practice execution and monitoring
- Build confidence

**How to get demo account:**

1. Open MT5 terminal
2. File → Open Account
3. Select broker → Demo Account
4. Fill registration form
5. Note credentials for appsettings.json

---

## 📚 See Also

- **[Strategies.Master.Overview.md](./Strategies/Strategies.Master.Overview.md)** - All orchestrators and presets
- **[MT5Account.Master.Overview.md](./MT5Account/MT5Account.Master.Overview.md)** - Low-level API reference
- **[GRPC_STREAM_MANAGEMENT.md](./GRPC_STREAM_MANAGEMENT.md)** - Streaming subscriptions guide
- **[ReturnCodes_Reference_EN.md](./ReturnCodes_Reference_EN.md)** - Complete return codes reference
- **[UserCode_Sandbox_Guide.md](./UserCode_Sandbox_Guide.md)** - How to write custom strategies

---

## 💡 Quick Term Lookup

| Term | Category | Definition |
|------|----------|------------|
| MT5Account | Architecture | Layer 1 - Low-level gRPC API |
| MT5Service | Architecture | Layer 2 - Wrapper methods |
| MT5Sugar | Architecture | Layer 3 - Convenience API ⭐ |
| Orchestrator | Strategy | Single-strategy implementation |
| Preset | Strategy | Multi-orchestrator adaptive system |
| Auto-normalization | Technical | Automatic parameter adjustment to broker rules |
| Risk-based sizing | Trading | Position size from dollar risk amount |
| Point | Trading | Smallest price movement (1 tick) |
| Pip | Trading | Traditional forex unit (0.0001 for most pairs) |
| Points-based methods | Technical | Methods using point offsets (BuyLimitPoints, etc.) |
| Trailing stop | Trading | SL that follows profit |
| Hedging | Trading | Opposite position to lock P/L |
| Pending order | Trading | Order at future price level |
| Grid trading | Strategy | Orders at regular intervals above/below price |
| Scalping | Strategy | Quick trades with small profits |
| News trading | Strategy | Straddle orders before high-impact news |
| gRPC | Technical | RPC framework for communication |
| mt5term | Technical | Gateway process bridging to MT5 terminal |
| Async/await | Technical | C# asynchronous programming pattern |
| Return codes | Technical | Proto codes indicating success/failure |
| Snapshot | Technical | Complete state capture (account/symbol) |
| Batch operation | Technical | Action on multiple items at once |
| Retry logic | Technical | Exponential backoff for connection errors |
| ConnectionHelper | Utility | Builds config and connects to MT5 |
| ProgressBarHelper | Utility | Visual countdown for orchestrators |
| Progressive complexity | Philosophy | Start simple, access complexity as needed |
| Educational project | Philosophy | Learning materials, not production systems |

---

> 💡 **New to CSharpMT5?**
> 1. Read [Strategies.Master.Overview.md](./Strategies/Strategies.Master.Overview.md) to understand orchestrators and presets
> 2. Check [MT5Account.Master.Overview.md](./MT5Account/MT5Account.Master.Overview.md) for complete API reference
> 3. Explore examples in `Examples\` folder to see API usage patterns
> 4. Start with MT5Sugar for the easiest API - drop down only when needed
> 5. **ALWAYS test on demo accounts first!**

---

"Trade safe, code clean, and may your async operations always complete successfully."
