# Getting Started with CSharpMT5

> **Welcome to CSharpMT5** - a comprehensive educational project for learning MT5 trading automation from the ground up using C# and .NET.

---

## 🚀 Prerequisites and Setup

Before you start working with CSharpMT5, you need to set up your development environment.

### Step 1: Install .NET 8 SDK

CSharpMT5 requires .NET 8 SDK or higher.

**Download and Install:**

- **Official:** [.NET 8 SDK Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- Choose the installer for your platform (Windows, macOS, Linux)

**Verify installation:**

```bash
dotnet --version
# Should show: 8.0.x or higher
```

---

### Step 2: Install a Code Editor

**Visual Studio Code (Recommended):**

- **Download:** [VS Code](https://code.visualstudio.com/)

- **Extensions to install:**

  - C# Dev Kit (official Microsoft extension)
  - C# (OmniSharp)

**Visual Studio 2022 (Alternative):**

- **Download:** [Visual Studio 2022](https://visualstudio.microsoft.com/)

- Choose "Community" edition (free)

- Select ".NET desktop development" workload during installation

**You can also use:**

- JetBrains Rider
- Visual Studio for Mac
- Any text editor + command line

---

### Step 3: Clone the Repository

Clone the CSharpMT5 project from GitHub:

```bash
git clone https://github.com/MetaRPC/CSharpMT5.git
cd CSharpMT5
```

**If you don't have Git installed:**

- Download from [git-scm.com](https://git-scm.com/)
- Or download the project as ZIP from GitHub and extract it

---

### Step 4: Understanding the Connection Flow

CSharpMT5 connects to MT5 terminal via **gRPC gateway**.

**Connection flow:**
```
CSharpMT5 → gRPC → mt5term Gateway → MT5 Terminal
```

**What is mt5term Gateway?**

- External gateway process that bridges CSharpMT5 to MT5 terminal
- Handles connection pooling and session management
- Gateway must be running before connecting
- You'll configure connection details in `Config\appsettings.json`

### Configuration File: appsettings.json

Before running the project, you need to configure your connection settings in `Config\appsettings.json`:

```json
{
  "MT5": {
    "User": 591129415,
    "Password": "IpoHj17tYu67@",
    "ServerName": "FxPro-MT5 Demo",
    "Host": "mt5.mrpc.pro",
    "Port": 443,
    "GrpcServer": "https://mt5.mrpc.pro:443",
    "BaseChartSymbol": "EURUSD",
    "InstanceId": null,
    "ConnectTimeoutSeconds": 120
  }
}
```

**Configuration parameters explained:**

| Parameter | Description | Example |
|-----------|-------------|---------|
| **User** | Your MT5 account login number | `591129415` |
| **Password** | Your MT5 account password (master password) | `"IpoHj17tYu67@"` |
| **ServerName** | MT5 server name from your broker | `"FxPro-MT5 Demo"` |
| **Host** | Gateway host address (provided by MetaRPC) | `"mt5.mrpc.pro"` |
| **Port** | Gateway port number | `443` (HTTPS) or `5555` |
| **GrpcServer** | Full gRPC server URL (combines Host + Port) | `"https://mt5.mrpc.pro:443"` |
| **BaseChartSymbol** | Default trading symbol for examples | `"EURUSD"` |
| **InstanceId** | Optional instance identifier (leave `null` for auto) | `null` |
| **ConnectTimeoutSeconds** | Connection timeout in seconds | `120` |

**Important notes:**

- **User, Password, ServerName** - These are your MT5 account credentials
- **Host, Port** - Provided by MetaRPC team (gateway connection details)
- **GrpcServer** - Should match `https://Host:Port` format
- **BaseChartSymbol** - Change to your preferred trading symbol
- **ConnectTimeoutSeconds** - Increase if you have slow connection

---

## 📋 MT5 Account Setup

If you don't have an MT5 demo account yet or need help creating one, please refer to our beginner's guide:

👉 **[MT5 for Beginners - Creating a Demo Account](MT5_For_Beginners/)**

This guide covers:

- Downloading and installing MT5 terminal
- Creating a demo account step-by-step
- Understanding master and investor passwords
- Choosing a broker (optional)

---

## 🎯 About This Project

This project is a **demonstration of the capabilities** of our team's gateway for reproducing methods and functionalities. It's designed to help you build your own trading logic system in the future.

We'll walk you through all the major aspects - from basic manual trading to a fully customizable algorithmic trading system. This journey will unlock the full potential of your acquired knowledge and fundamental understanding of trading and markets.

**What you'll learn:**

- What gRPC methods do and how to use them directly
- How methods can be modified to fit your needs
- How to optimize your code for performance
- How to create convenient input/output systems
- How to monitor positions by symbols effectively
- How to build intelligent risk management systems

**All we ask from you is:**

> **A desire to learn, learn, and learn some more.** In the end, this will lead to significant results and, most importantly, a solid foundation of knowledge in algorithmic trading.

---

## 🏗️ Project Architecture: Three-Layer System

The project consists of **three interconnected files** in the root directory, each building upon the previous one. Understanding this chain is key to mastering CSharpMT5.

### Layer 1: MT5Account.cs - Low-Level gRPC Foundation

**What it is:** Direct gRPC calls to the MT5 terminal - the absolute foundation of everything.

**[📖 MT5Account Overview](MT5Account/MT5Account.Master.Overview.md)**

- Raw protocol buffer messages and gRPC communication
- Maximum control and flexibility over every request/response
- **All other layers use this internally**
- **Best for:** Advanced users who need fine-grained control

### Layer 2: MT5Service.cs - Convenient Wrappers

**What it is:** Wrapper methods that simplify working with MT5Account's gRPC calls.

**[📖 MT5Service Overview](MT5Service/MT5Service.Overview.md)**

- Simplified error handling and response parsing
- Pre-configured common operations
- Easier to work with than raw gRPC
- **Best for:** Most common trading scenarios

### Layer 3: MT5Sugar.cs - High-Level Helpers

**What it is:** Syntactic sugar and convenience methods for maximum productivity.

**[📖 MT5Sugar API Overview](MT5Sugar/MT5Sugar.API_Overview.md)**

- Chainable operations and fluent interfaces
- Smart defaults and parameter inference
- Most intuitive and beginner-friendly
- **Best for:** Quick prototyping and simple strategies

---

### 📚 Understanding the Chain

This three-file chain represents the evolution from low-level control to high-level convenience:

```
MT5Sugar.cs (easiest, highest abstraction)
    ↓ uses
MT5Service.cs (convenient wrappers)
    ↓ uses
MT5Account.cs (raw gRPC, foundation)
    ↓ communicates with
MT5 Terminal (via gateway)
```

**Each Overview document includes:**

- Detailed method descriptions with parameters
- Return types and error handling
- Usage examples and best practices
- Common patterns and pitfalls to avoid

---

### 🎓 Recommended Learning Paths

**Path A: For Developers (Bottom-Up Approach)**

If you have programming experience and want to understand everything deeply:

1. **Start with MT5Account** - Learn the gRPC foundation
2. **Move to MT5Service** - Understand convenient wrappers
3. **Finish with MT5Sugar** - Appreciate the high-level abstractions

✅ This path gives you complete control and deep understanding.

**Path B: For Traders (Top-Down Approach)**

If you're new to trading automation and want to get results quickly:

1. **Start with MT5Sugar** - Easy, intuitive methods to trade fast
2. **Move to MT5Service** - Learn more advanced patterns when needed
3. **Deep dive into MT5Account** - Understand the foundation for full control

✅ This path gets you trading quickly while leaving room to grow.

---

## 📂 Demonstration Examples

You can explore demonstration files that showcase different aspects of the SDK. These files are organized by complexity level and are located in the `Examples/` folder.

**Each file includes internal code comments explaining what each operation does.**

### Examples/LowLevel/

Low-level gRPC demonstrations using MT5Account:

- **Program.LowLevel.cs** - Basic low-level gRPC operations

- **Program.Streaming.cs** - Real-time streaming (ticks, events)

- **Program.Trading.cs** - Trading operations at protocol level

### Examples/Services/

Wrapper layer demonstrations using MT5Service:

- **Program.Service.cs** - Common trading scenarios with wrappers

### Examples/Sugar/

High-level convenience methods using MT5Sugar:

- **Program.Sugar.cs** - Basic Sugar API usage
- **Program.Sugar.Monitor.cs** - Position monitoring and management
- **Program.Sugar.PendingOrders.cs** - Pending orders with retry logic
- **Program.Sugar.Scalper.cs** - Quick scalping example

### Examples/Orchestrators/

Complete trading strategy implementations:

- **GridTradingOrchestrator.cs** - Grid trading for range-bound markets

- **SimpleScalpingOrchestrator.cs** - Quick scalping with tight stops

- **QuickHedgeOrchestrator.cs** - Hedging strategy for high volatility

- **NewsStraddleOrchestrator.cs** - Breakout trading around news events

- **PendingBreakoutOrchestrator.cs** - Pending orders for breakouts

### Examples/Presets/

Multi-strategy adaptive systems:

- **AdaptiveMarketModePreset.cs** - Intelligent system that analyzes market conditions and selects optimal orchestrator

### Examples/UserCode/

Sandbox for your custom code:

- **ProgramUserCode.cs** - Template file for your own trading logic after learning the basics

---

## 🎮 Running Examples with Program.cs

All demonstration files are launched from `Program.cs`, which acts as the **heart of the project**. It allows you to run any example or orchestrator with simple commands.

**Program.cs features:**

- Launches demonstration files by command
- Manages orchestrator lifecycle and configuration
- Handles user input and strategy parameters

**How to run examples:**

```bash
# Run Sugar API examples
dotnet run market              # Market orders demonstration
dotnet run positions           # Position monitoring
dotnet run pendingorders       # Pending orders with retry

# Run Orchestrators (strategies)
dotnet run grid                # Grid trading orchestrator
dotnet run scalping            # Scalping orchestrator
dotnet run hedge               # Hedge orchestrator

# Run Presets
dotnet run preset              # Adaptive market mode preset
```

**Note:** You can find the complete list of available commands in the header comments of `Program.cs`.

---

## 🎯 Learning Advanced Features

After you've mastered the three-layer chain (MT5Account → MT5Service → MT5Sugar), you can explore advanced features:

### Orchestrators (Trading Strategies)

Orchestrators are complete trading strategy implementations that show you how to:

- Structure automated trading logic
- Manage risk and position sizing
- Handle entry and exit automation
- Monitor performance in real-time

**[📖 Orchestrators Overview](Strategies/Strategies.Master.Overview.md)**

### Presets (Multi-Strategy Systems)

Presets combine multiple orchestrators with adaptive logic to create intelligent trading systems:

**[📖 Adaptive Market Mode Preset](Strategies/Presets/AdaptiveMarketModePreset.md)**

---

## 🛠️ Building Your Own System

After studying all the examples and understanding the architecture, you can start building your own trading system:

**Sandbox location:** `Examples/UserCode/ProgramUserCode.cs`

This file is prepared as a starting template for your custom code. Before you start coding here, make sure you've studied:

1. The three-layer API (MT5Account → MT5Service → MT5Sugar)
2. At least one orchestrator to understand strategy structure
3. Risk management patterns from the examples

**[📖 User Code Sandbox Guide](UserCode_Sandbox_Guide.md)**

---

## 🔍 Helpful Tools and References

### Protobuf Inspector

If you're having trouble finding properties or don't want to dig through documentation:

**Tool location:** `Examples/Helpers/ProtobufInspector.cs`

**[📖 Protobuf Inspector Guide](ProtobufInspector.README.EN.md)**

This tool helps you explore protobuf message structures and available properties interactively.

### Sync vs Async Methods

All methods in MT5Account are available in both synchronous and asynchronous versions. Learn when to use each:

**[📖 Sync vs Async Guide](Sync_vs_Async.md)**

### gRPC Stream Management

If you're working with real-time streaming (ticks, events):

**[📖 gRPC Stream Management](GRPC_STREAM_MANAGEMENT.md)**

### Return Codes Reference

Understanding what return codes mean when trading operations execute:

**[📖 Return Codes Reference](ReturnCodes_Reference_EN.md)**

---

## 💬 Support and Community

If you encounter issues with the code or have questions about the gateway:

**Support Repository:** [https://github.com/Moongoord/MetaRPC-Gateway-Support](https://github.com/Moongoord/MetaRPC-Gateway-Support)

This is a discussion repository where you can:

- Ask questions about the gateway
- Report issues
- Share your experiences
- Get help from the community

**Note:** This repository is currently in development and will be available to everyone soon.

---

## 🎓 Conclusion

The **MetaRPC team** is committed to creating favorable conditions for learning the fundamental principles of trading and building algorithmic trading systems.

We believe that with dedication and a desire to learn, you can master everything from low-level protocol communication to sophisticated multi-strategy trading systems.

**Your journey starts here:**

1. Set up your environment (above)
2. Create or configure your MT5 demo account ([MT5 for Beginners](MT5_For_Beginners/))
3. Choose your learning path (bottom-up or top-down)
4. Run your first example: `dotnet run market`
5. Study the code, experiment, and build

**Good luck on your algorithmic trading journey!**

> "The foundation of success in algorithmic trading is not just understanding markets, but understanding the code that interacts with them. Master both, and you'll have unlimited possibilities."
>
> — MetaRPC Team

---

**Next steps:**

- 📖 [MT5 for Beginners](MT5_For_Beginners/) - If you need MT5 account setup.

- 📖 [MT5Account Overview](MT5Account/MT5Account.Master.Overview.md) - Start learning the foundation.

- 📖 [MT5Sugar Overview](MT5Sugar/MT5Sugar.API_Overview.md) - Start trading quickly.

- 🎯 [Orchestrators Overview](Strategies/Strategies.Master.Overview.md) - Learn strategy implementation.

---
