# CSharpMT5 Project Map

> Complete project structure guide. Shows what's where, what's user-facing vs internal, and how components connect.

---

## 🗺️ Project Overview

```
CSharpMT5/
├── 📦 Core API (Internal - 3 layers)
├── 🎯 User Code (Orchestrators, Presets, Examples)
├── 📚 Documentation
└── ⚙️ Configuration & Build

External Dependencies:
└── 🔌 gRPC & Proto (NuGet packages)
```

---

## 📦 Core API (Internal - Root Level)

**What:** Three-tier architecture for MT5 trading automation.

**User interaction:** Import and use, but typically don't modify.

```
/
├── package/                   ← NuGet package source files (auto-generated)
│   └── Helpers/
│       └── MT5Account.cs      ← LAYER 1: Low-level gRPC
│           └── Direct gRPC calls to MT5 terminal
│           └── Connection management with retry logic
│           └── Proto Request/Response handling
│           └── Async/Sync method variants
│           └── Built-in connection resilience
│
├── MT5Service.cs              ← LAYER 2: Wrapper methods
│   └── Simplified signatures (no proto objects)
│   └── Type conversions (proto → C# primitives)
│   └── Direct data returns
│   └── Extension methods for convenience
│
└── MT5Sugar.cs                ← LAYER 3: Convenience layer ⭐
    └── Auto-normalization (volumes, prices)
    └── Risk management (CalculateVolume, BuyByRisk)
    └── Points-based methods (BuyLimitPoints, etc.)
    └── Batch operations (CloseAll, CancelAll)
    └── Snapshots (GetAccountSnapshot, GetSymbolSnapshot)
    └── Smart helpers (conversions, limits)

Errors/
└── ConnectExceptionMT5.cs     ← Connection exception wrapper
```

**Architecture flow:**
```
MT5Sugar → uses → MT5Service → uses → MT5Account → gRPC → MT5 Terminal
```

**User decision:**

- **95% of cases:** Start with `MT5Sugar` (highest level, easiest)
- **Need wrappers:** Drop to `MT5Service` (no auto-normalization)
- **Need raw proto:** Drop to `MT5Account` (full control)

**Documentation:**

- [MT5Account API Reference](API_Reference/MT5Account.API.md)
- [MT5Service API Reference](API_Reference/MT5Service.API.md)
- [MT5Sugar API Reference](API_Reference/MT5Sugar.API.md)

---

## 🎯 User Code (Your Trading Strategies)

### Orchestrators (Examples\Orchestrators\)

**What:** Pre-built trading strategy implementations.

```
Examples\Orchestrators\
├── GridTradingOrchestrator.cs        ← Grid trading (range-bound markets)
├── SimpleScalpingOrchestrator.cs     ← Quick scalping with tight stops
├── QuickHedgeOrchestrator.cs         ← Hedging strategy (high volatility)
├── NewsStraddleOrchestrator.cs       ← Breakout trading around news
└── PendingBreakoutOrchestrator.cs    ← Pending orders for breakouts
```

**Purpose:** Educational examples showing complete strategy workflows:

- Entry logic (risk-based volume where applicable)
- Position monitoring with progress bars
- Exit management and cleanup
- Performance tracking (balance, equity, P/L)
- Configurable parameters via properties

**How to use:**

1. Study existing orchestrators
2. Copy one as template
3. Modify for your strategy
4. Test on demo account

**How to run:**
```bash
dotnet run grid         # GridTradingOrchestrator
dotnet run scalping     # SimpleScalpingOrchestrator
dotnet run hedge        # QuickHedgeOrchestrator
dotnet run news         # NewsStraddleOrchestrator
dotnet run breakout     # PendingBreakoutOrchestrator
```

**Documentation:** [Strategies.Master.Overview.md](./Strategies/Strategies.Master.Overview.md)

---

### Presets (Examples\Presets\)

**What:** Multi-orchestrator combinations with adaptive logic based on market analysis.

**User interaction:** ✅ **Advanced usage** - combine multiple strategies.

```
Examples\Presets\
└── AdaptiveMarketModePreset.cs    ← Intelligent multi-strategy system
```

**Purpose:** Show how to:

- Chain multiple orchestrators
- Adaptive decision-making (volatility → strategy)
- Market condition analysis (simplified demo)
- Multi-phase trading sessions
- Performance tracking across phases

**How to run:**
```bash
dotnet run preset       # AdaptiveMarketModePreset
dotnet run adaptive     # Same as above
```

**Documentation:** [AdaptiveMarketModePreset.md](./Strategies/Presets/AdaptiveMarketModePreset.md)

---

### Examples (Examples\)

**What:** Runnable examples demonstrating API usage at different layers.

**User interaction:** ✅ **Learning materials** - run to understand APIs.

```
Examples\
├── LowLevel\                          ← MT5Account examples (proto level)
│   └── Program.LowLevel.Positions.cs  ← Low-level position operations
│
├── Service\                           ← MT5Service examples (wrapper level)
│   └── Program.Service.Positions.cs   ← Service layer positions demo
│
└── Sugar\                             ← MT5Sugar examples (convenience level)
    ├── Program.Sugar.MarketOrders.cs  ← Market orders demo
    └── Program.Sugar.PendingOrders.cs ← Pending orders + retry logic demo
```

**How to run:**
```bash
dotnet run positions       # Service layer positions
dotnet run market          # Sugar market orders
dotnet run pendingorders   # Sugar pending orders
```

---

### Program.cs (Root)

**What:** Main entry point that routes `dotnet run` commands to appropriate examples/orchestrators/presets.

**User interaction:** 📋 **Runner + Documentation** - launches everything.

```
Program.cs
├── Main()                              ← Entry point, parses args
├── RouteCommand()                      ← Maps aliases to runners
├── RunOrchestrator()                   ← Launches orchestrators
├── RunPreset()                         ← Launches presets
├── RunExample()                        ← Launches examples
└── Header documentation                ← Complete command reference
```

**How it works:**

```
dotnet run grid
    ↓
Program.cs Main(args)  // args[0] = "grid"
    ↓
RouteCommand("grid")
    ↓
RunOrchestrator("grid")
    ↓
GridTradingOrchestrator.RunAsync()
```

**Purpose:**

- Single entry point for all runnable code
- Command routing with aliases (grid, scalping, preset, etc.)
- Helpful error messages for unknown commands
- Ctrl+C handling for graceful shutdowns

**Available commands:** See header comment in `Program.cs` for complete list.

---

### Helpers (Examples\Helpers\)

**What:** Utility classes for examples and orchestrators.

```
Examples\Helpers\
├── ConnectionHelper.cs        ← MT5 connection setup
└── ProgressBarHelper.cs       ← Visual progress bars
```

**ConnectionHelper:**
```csharp
// Build configuration from appsettings.json
var config = ConnectionHelper.BuildConfiguration();

// Create and connect to MT5
var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);
var service = new MT5Service(account);
```

**ProgressBarHelper:**
```csharp
// Visual countdown during orchestrator runtime
await ProgressBarHelper.ShowProgressBar(
    durationSeconds: 60,
    message: "Monitoring positions",
    cancellationToken: cts.Token
);
```

---

## 📚 Documentation (docs\)

**What:** Complete API and strategy documentation.

**User interaction:** 📖 **Read first!** Comprehensive reference.

```
docs\
├── index.md                           ← ⭐ Homepage - project introduction
│
├── Getting_Started.md                 ← ⭐ Start here! Setup & first steps
│
├── PROJECT_MAP.md                     ← ⭐ This file - complete structure
│
├── Glossary.md                        ← ⭐ Terms and definitions
│
├── MT5_For_Beginners.md               ← Creating demo account for testing
│
├── Your_First_Project.ru.md           ← Your first project guide
│
├── ReturnCodes_Reference_EN.md        ← Proto return codes reference
│
├── UserCode_Sandbox_Guide.md          ← How to write custom strategies
│
├── GRPC_STREAM_MANAGEMENT.md          ← Streaming subscriptions guide
│
├── Sync_vs_Async.md                   ← Async/sync patterns explained
│
├── ProtobufInspector.README.EN.md     ← Protobuf inspector tool guide
│
├── Strategies\                        ← Strategy documentation
│   ├── Strategies.Master.Overview.md  ← ⭐ ALL orchestrators & presets
│   ├── Orchestrators_EN\              ← Individual orchestrator docs
│   │   ├── GridTradingOrchestrator.md
│   │   ├── GridTradingOrchestrator.HOW_IT_WORKS.md
│   │   ├── SimpleScalpingOrchestrator.md
│   │   ├── SimpleScalpingOrchestrator.HOW_IT_WORKS.md
│   │   ├── QuickHedgeOrchestrator.md
│   │   ├── QuickHedgeOrchestrator.HOW_IT_WORKS.md
│   │   ├── NewsStraddleOrchestrator.md
│   │   ├── NewsStraddleOrchestrator.HOW_IT_WORKS.md
│   │   ├── PendingBreakoutOrchestrator.md
│   │   └── PendingBreakoutOrchestrator.HOW_IT_WORKS.md
│   └── Presets\
│       └── AdaptiveMarketModePreset.md
│
├── API_Reference\                     ← API documentation
│   ├── MT5Account.API.md              ← Layer 1 API reference
│   ├── MT5Service.API.md              ← Layer 2 API reference
│   └── MT5Sugar.API.md                ← Layer 3 API reference
│
├── MT5Account\                        ← Low-level proto API docs
│   ├── MT5Account.Master.Overview.md  ← ⭐ Complete API reference
│   ├── 1. Account_information\        ← Account methods
│   ├── 2. Symbol_information\         ← Symbol/market data methods
│   ├── 3. Position_Orders_Information\ ← Position/order methods
│   ├── 4. Trading_Operations\         ← Trading execution methods
│   ├── 5. Market_Depth(DOM)\          ← Market depth methods
│   ├── 6. Additional_Methods\         ← Additional helpers
│   └── 7. Streaming_Methods\          ← Real-time subscriptions
│
├── MT5Service\                        ← Service layer method docs
│   ├── MT5Service.Overview.md          ← ⭐ Complete Service API reference
│   ├── Account_Convenience_Methods.md  ← Account helper methods
│   ├── Symbol_Convenience_Methods.md   ← Symbol helper methods
│   ├── Trading_Convenience_Methods.md  ← Trading helper methods
│   └── History_Convenience_Methods.md  ← History helper methods
│
└── MT5Sugar\                          ← Sugar layer method docs
    ├── MT5Sugar.API_Overview.md        ← ⭐ Complete Sugar API reference
    ├── 1. Infrastructure\              ← Core infrastructure methods
    ├── 2. Snapshots\                   ← Account/Symbol snapshots
    ├── 3. Normalization_Utils\         ← Price/volume normalization
    ├── 4. History_Helpers\             ← History retrieval helpers
    ├── 5. Streams_Helpers\             ← Bounded streaming methods
    ├── 6. Trading_Market_Pending\      ← Market & pending orders
    ├── 8. Volume_Price_Utils\          ← Volume calculation & pricing
    ├── 9. Pending_ByPoints\            ← Pending orders by points
    ├── 10. Market_ByRisk\              ← Market orders by risk
    ├── 11. Bulk_Convenience\           ← Bulk operations (close/cancel all)
    ├── 12. Market_Depth_DOM\           ← Market depth (DOM) methods
    ├── 13. Order_Validation\           ← Pre-flight order validation
    ├── 14. Session_Time\               ← Trading session info
    └── 15. Position_Monitoring\        ← Position monitoring & stats
```

**Structure:**

- Each method has its own `.md` file with examples
- Overview files (`*.Master.Overview.md`) provide navigation
- `HOW_IT_WORKS.md` files explain algorithms step-by-step
- Links between related methods
- Usage examples in every file

---

## 🔌 gRPC & Proto (NuGet Dependencies)

**What:** Protocol Buffer and gRPC libraries for MT5 terminal communication.

**User interaction:** 📋 **Reference only** - managed by NuGet.

**Key NuGet packages:**

- `Grpc.Net.Client` - gRPC client library
- `Google.Protobuf` - Protocol Buffers runtime
- `Grpc.Tools` - Proto compilation tools

**How it works:**

1. NuGet restores packages on build
2. Proto files compiled by Grpc.Tools (if present)
3. Generated C# classes available for import
4. MT5Account layer uses proto-generated types

**Proto-generated types:**

- `mt5_term_api.*` - Trading API types
- Request/Response message types
- Enum definitions
- Service contracts

**Purpose:**

- Define gRPC service contracts
- Type-safe communication with MT5 terminal
- Used by MT5Account layer
- Hidden by MT5Service and MT5Sugar layers

---

## 📊 Component Interaction Diagram

```
YOUR CODE (User-facing)
  ├─ Orchestrators (strategy implementations)
  ├─ Presets (multi-strategy combinations)
  └─ Examples (learning materials)
                  │
                  │ uses
                  ↓
MT5Sugar (Layer 3 - Convenience)
  ├─ Auto-normalization
  ├─ Risk management
  ├─ Points-based methods
  └─ Batch operations
                  │
                  │ uses
                  ↓
MT5Service (Layer 2 - Wrappers)
  ├─ Direct data returns
  ├─ Type conversions
  └─ Simplified signatures
                  │
                  │ uses
                  ↓
MT5Account (Layer 1 - Low-level)
  ├─ Proto Request/Response
  ├─ gRPC communication
  ├─ Connection management
  └─ Auto-reconnection
                  │
                  │ gRPC
                  ↓
MT5 Gateway (mt5term) or MT5 Terminal
  └─ MetaTrader 5 with gRPC server
```

---

## 🔍 File Naming Conventions

### Core API (Root Level)

- `MT5Account` - Layer 1 (low-level gRPC, located in `package/Helpers/MT5Account.cs`)
- `MT5Service.cs` - Layer 2 (wrapper methods)
- `MT5Sugar.cs` - Layer 3 (convenience API)
- `*ExceptionMT5.cs` - Exception types

### User Code (Examples\)
- `*Orchestrator.cs` - Single-strategy implementations
- `*Preset.cs` - Multi-strategy combinations
- `Program.*.cs` - Runnable examples at different layers
- `*Helper.cs` - Utility classes (ConnectionHelper, ProgressBarHelper)

### Documentation (docs\)
- `*.Master.Overview.md` - Complete category overviews
- `*.Overview.md` - Section overviews
- `MethodName.md` - Individual method documentation
- `*.HOW_IT_WORKS.md` - Algorithm explanations

---

## 📂 What to Modify vs What to Leave Alone

### ✅ MODIFY (User Code)

```
Examples\Orchestrators\        ← Copy and customize for your strategies
Examples\Presets\              ← Create your own multi-strategy systems
Examples\LowLevel\             ← Add your own low-level examples
Examples\Service\              ← Add your own service examples
Examples\Sugar\                ← Add your own sugar examples
Examples\Helpers\              ← Add your own helper utilities
Config\appsettings.json        ← Configure for your MT5 terminal/gateway
Program.cs                     ← Add new command routing if needed
README.md                      ← Update with your changes
```

### 📖 READ (Core API)

```
package/Helpers/MT5Account.cs  ← Use but don't modify (import and call)
MT5Service.cs                  ← Use but don't modify
MT5Sugar.cs                    ← Use but don't modify
docs\                          ← Reference documentation
```

### 🔒 LEAVE ALONE (Generated/Build)

```
package\                       ← NuGet package source (auto-generated by CI/CD)
bin\                           ← Compiled assemblies (auto-generated)
obj\                           ← Intermediate build files (auto-generated)
.vs\                           ← Visual Studio cache (auto-generated)
*.csproj.user                  ← User-specific project settings
```

**Note about `package/` folder:**

This folder contains decompiled source code from the NuGet package and is **automatically generated by GitLab Runner** during CI/CD builds. It includes:

- `package/Helpers/MT5Account.cs` - Core gRPC layer
- `package/Helpers/ApiExceptionMT5.cs`, `ConnectExceptionMT5.cs` - Exception classes
- Proto-generated files: `Mt5TermApiAccountHelper.cs`, `Mt5TermApiMarketInfo.cs`, `Mt5TermApiConnection.cs`, etc.
- gRPC client stubs: `Mt5-term-api-*Grpc.cs` files
- Error types: `MrpcMt5Error.cs`
- Project files: `MetaRPC.MT5.csproj`, `MetaRPC.MT5.sln`

⚠️ **Do not manually modify files in `package/`** - changes will be overwritten on next CI/CD run. This folder is included in the repository for transparency and debugging purposes.

---

## 🎯 Project Philosophy

**Goal:** Make MT5 trading automation accessible through progressive complexity.

**Three-tier design:**

1. **Low-level (MT5Account):** Full control, proto/gRPC
2. **Wrapper (MT5Service):** Simplified method calls
3. **Convenience (MT5Sugar):** Auto-everything, batteries included

**User code:**

- **Orchestrators:** Pre-built strategy templates
- **Presets:** Multi-strategy adaptive systems
- **Examples:** Learning materials at all layers

**Start high (MT5Sugar), drop down only when needed.**

---

## 🛠️ Troubleshooting

### Build Issues

```bash
# Clean and rebuild
dotnet clean
dotnet build

# Restore NuGet packages
dotnet restore

# Check .NET version
dotnet --version   # Should be 8.0 or higher
```

### Connection Issues

```
1. Check appsettings.json (host, port, credentials)
2. Verify MT5 terminal/gateway is running
3. Check firewall/antivirus isn't blocking port
4. Try different port if 5555 is in use
5. Check MT5 terminal logs for errors
```

### Runtime Issues

```
1. Always test on demo account first
2. Check return codes (10009 = success, 10031 = connection error)
3. Monitor console output for errors
4. Use retry logic for intermittent issues
5. Check broker allows your strategy type (hedging, etc.)
```

---

## 📈 Performance Considerations

### Connection Management
- Single gRPC connection shared across operations
- Built-in automatic reconnection handles temporary failures
- Retry logic with exponential backoff (1s → 2s → 4s)

### Rate Limiting
- 3-second delays between order placements (demo examples)
- Gateway may enforce additional rate limits
- Adjust delays based on broker requirements

### Resource Usage
- Async/await throughout for non-blocking I/O
- CancellationToken for graceful shutdowns
- Proper cleanup in finally blocks

---

## 📝 Best Practices

### Code Organization
```
✅ DO: Separate concerns (analysis, execution, monitoring)
✅ DO: Use async/await for all I/O operations
✅ DO: Add comprehensive error handling
✅ DO: Document your strategy logic clearly
✅ DO: Use ProgressBarHelper for long-running operations

❌ DON'T: Mix strategy logic with API calls
❌ DON'T: Use Thread.Sleep (use await Task.Delay)
❌ DON'T: Ignore return codes
❌ DON'T: Test on live accounts without extensive demo testing
```

### Strategy Development
```
✅ DO: Start with existing orchestrator as template
✅ DO: Test each component separately
✅ DO: Log all trading decisions and results
✅ DO: Use demo accounts for development
✅ DO: Implement proper risk management

❌ DON'T: Over-optimize on limited data
❌ DON'T: Ignore edge cases and failures
❌ DON'T: Use fixed lot sizes without risk calculation
❌ DON'T: Deploy without backtesting and forward testing
```

---

> 💡 **Remember:** This is an educational project. All orchestrators and presets are demonstration examples, not production-ready trading systems. Always test on demo accounts, understand the code thoroughly, and implement proper risk management before considering live trading.

---

"Trade safe, code clean, and may your async operations always complete successfully."
