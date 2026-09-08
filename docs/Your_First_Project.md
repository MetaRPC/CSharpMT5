# Your First Project From Scratch

> **Quick Start** - Create your own MT5 trading project in 10 minutes using only the MetaRPC.MT5 NuGet package

---

## Who Is This Guide For?

This document is designed for those who want to:

- **Get started quickly** writing code for MT5 in their own project
- **Avoid cloning** the entire CSharpMT5 repository
- **Create a project from scratch** and connect minimal dependencies
- **Write their first method** and see results immediately

**Difference between this guide and Getting_Started.md:**

| Getting Started | Your First Project (this guide) |
|----------------|--------------------------------|
| Clone a ready-made repository | Create a project from scratch |
| Study architecture and examples | Write working code immediately |
| Long learning path | Quick results |
| For deep dive | For quick start |

> After completing this guide and getting your first results, we recommend studying [Getting Started](Getting_Started.md) to understand the full SDK architecture.

---

## What Will We Do?

In this guide, we'll create a minimalist project that:

1. Connects to MT5 terminal through gRPC gateway
2. Retrieves account balance
3. Outputs results to console

**This takes 10 minutes and requires minimal code.**

---

## Step 1: Install .NET 8 SDK

If you don't have .NET 8 SDK installed yet:

**Download and install:**

- [.NET 8 SDK Download](https://dotnet.microsoft.com/download/dotnet/8.0)

**Verify installation:**

```bash
dotnet --version
# Should show: 8.0.x or higher
```

---

## Step 2: Create a New Console Project

Open terminal (command prompt) and execute:

```bash
# Create project folder and navigate into it
mkdir MyMT5Project
cd MyMT5Project

# Create new console project in current folder
dotnet new console
```

**What happened:**

- Created `MyMT5Project` folder
- Created .NET console project with files:
  - `MyMT5Project.csproj` - project file
  - `Program.cs` - main code file

---

**Opening the Project in VS Code:**

If you're working through VS Code, the editor won't automatically open the created directory. To do this:

**Method 1: Via VS Code Menu**
- File → Open Folder
- Select `C:\Users\[your_name]\MyMT5Project`
- Click "Select Folder"

**Method 2: Via Terminal**
```bash
# While in the project folder, execute:
code .
```
(the dot means "open current folder in VS Code")

**Method 3: Drag and Drop**
- Drag the `MyMT5Project` folder onto the VS Code icon

After opening, you'll see the project structure:
```
MyMT5Project/
├── Program.cs
├── MyMT5Project.csproj
└── obj/
```

## Step 3: Install MetaRPC.MT5 NuGet Package

This is the most important step - installing the package that contains everything needed:

```bash
dotnet add package MetaRPC.MT5
```

**What this package includes:**

- Proto files (Protocol Buffers schemas for gRPC)
- `MT5Account` class for low-level interaction with MT5
- All necessary dependencies (Grpc.Net.Client, Grpc.Core, etc.)

> **Important:** This package is ALL you need to work with MT5. No additional files need to be cloned.

---

## Step 4: Install Packages for Configuration Management

We'll need packages to read `appsettings.json`:

```bash
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Microsoft.Extensions.Configuration.Binder
```

---

## Step 5: Create appsettings.json Configuration File

Create an `appsettings.json` file in the project root (next to `Program.cs`):

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

> ⚠️ **IMPORTANT:** The `User`, `Password`, and `ServerName` values in the example above are **placeholders**! You **MUST** replace them with **your** MT5 account credentials, otherwise the connection won't work.

**Parameter explanation:**

| Parameter | Description | Example |
|----------|----------|--------|
| **User** | Your MT5 account number (login) | `591129415` |
| **Password** | Master password for MT5 account | `"IpoHj17tYu67@"` |
| **ServerName** | Your broker's server name | `"FxPro-MT5 Demo"` |
| **Host** | gRPC gateway address (provided by MetaRPC) | `"mt5.mrpc.pro"` |
| **Port** | Gateway port | `443` |
| **GrpcServer** | Full gateway URL | `"https://mt5.mrpc.pro:443"` |
| **BaseChartSymbol** | Default trading symbol | `"EURUSD"` |
| **InstanceId** | Instance ID (leave `null` for auto) | `null` |
| **ConnectTimeoutSeconds** | Connection timeout in seconds | `120` |

**What needs to be replaced:**

✏️ **MUST replace with YOUR data:**

- `User` - your MT5 account login
- `Password` - your MT5 account password
- `ServerName` - your broker's server name (shown in MT5 terminal)

✅ **Leave unchanged:**

- `Host`, `Port`, `GrpcServer` - MetaRPC public gateway address
- `BaseChartSymbol`, `InstanceId`, `ConnectTimeoutSeconds` - standard settings

> **Don't have an MT5 account?** Read [MT5 for Beginners](MT5_For_Beginners.md) - it shows step-by-step how to create a demo account.

---

## Step 6: Configure appsettings.json Copying to bin

Open the `MyMT5Project.csproj` file and add this section inside `<Project>`:

```xml
<ItemGroup>
  <None Remove="appsettings.json" />
  <Content Include="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

**Complete csproj file example:**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MetaRPC.MT5" Version="1.0.942" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="8.0.2" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
  </ItemGroup>

  <!-- Copy appsettings.json to bin -->
  <ItemGroup>
    <None Remove="appsettings.json" />
    <Content Include="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

</Project>
```

---

## Step 7: Write Code to Connect and Retrieve Balance

Open `Program.cs` and replace its content with the following code:

```csharp
using Microsoft.Extensions.Configuration;
using mt5_term_api;

// ============================================================================
// CONFIGURATION - Load settings from appsettings.json
// ============================================================================

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var mt5Config = configuration.GetSection("MT5");

ulong user = mt5Config.GetValue<ulong>("User");
string password = mt5Config.GetValue<string>("Password") ?? "";
string serverName = mt5Config.GetValue<string>("ServerName") ?? "";
string grpcServer = mt5Config.GetValue<string>("GrpcServer") ?? "";
string baseSymbol = mt5Config.GetValue<string>("BaseChartSymbol") ?? "EURUSD";
int connectTimeout = mt5Config.GetValue<int>("ConnectTimeoutSeconds", 120);

Console.WriteLine("=== MT5 Connection Configuration ===");
Console.WriteLine($"User: {user}");
Console.WriteLine($"Server: {serverName}");
Console.WriteLine($"gRPC: {grpcServer}");
Console.WriteLine($"Symbol: {baseSymbol}");
Console.WriteLine("====================================\n");

// ============================================================================
// CONNECTION - Create MT5Account (LOW-LEVEL API from NuGet package)
// ============================================================================

Console.WriteLine("Connecting to MT5 gateway...");

// Create MT5Account - pass credentials to constructor
var mt5Account = new MT5Account(user, password, grpcServer, Guid.NewGuid());

// Connect to MT5 terminal via ServerName
await mt5Account.ConnectByServerNameAsync(
    serverName: serverName,
    baseChartSymbol: baseSymbol,
    waitForTerminalIsAlive: true,
    timeoutSeconds: connectTimeout
);

Console.WriteLine("✓ Connected successfully!");
Console.WriteLine($"Account ID: {mt5Account.Id}\n");

// ============================================================================
// FETCH BALANCE - Use AccountSummaryAsync (LOW-LEVEL method)
// ============================================================================

Console.WriteLine("Fetching account information...");

var accountSummary = await mt5Account.AccountSummaryAsync(
    deadline: DateTime.UtcNow.AddSeconds(30),
    cancellationToken: CancellationToken.None
);

// Output account information using REAL field names from proto
Console.WriteLine("=== Account Information ===");
Console.WriteLine($"Login: {accountSummary.AccountLogin}");
Console.WriteLine($"Name: {accountSummary.AccountUserName}");
Console.WriteLine($"Company: {accountSummary.AccountCompanyName}");
Console.WriteLine($"Currency: {accountSummary.AccountCurrency}");
Console.WriteLine($"Leverage: 1:{accountSummary.AccountLeverage}");
Console.WriteLine($"Balance: {accountSummary.AccountBalance:F2}");
Console.WriteLine($"Credit: {accountSummary.AccountCredit:F2}");
Console.WriteLine($"Equity: {accountSummary.AccountEquity:F2}");
Console.WriteLine($"Trade Mode: {accountSummary.AccountTradeMode}");
Console.WriteLine("===========================\n");

Console.WriteLine("✓ Success! Your first MT5 connection using LOW-LEVEL API is complete.");
```

---

## Step 8: Run the Project

Save all files and execute:

```bash
dotnet run
```

**Expected output:**

```
=== MT5 Connection Configuration ===
User: 591129415
Server: FxPro-MT5 Demo
gRPC: https://mt5.mrpc.pro:443
Symbol: EURUSD
====================================

Connecting to MT5 gateway...
✓ Connected successfully!
Account ID: 9afb34d7-45a3-4433-b6f5-45b32e52e6bd

Fetching account information...
=== Account Information ===
Login: your login
Name: your name
Company: FXPRO Financial Services Ltd
Currency: EUR
Leverage: 1:30
Balance: 9519.02
Credit: 0.00
Equity: 9519.02
Trade Mode: MrpcAccountTradeModeDemo
===========================

✓ Success! Your first MT5 connection using LOW-LEVEL API is complete.
```

---

## Congratulations! You Did It!

You just:

✅ Created a new .NET project from scratch

✅ Connected the MetaRPC.MT5 NuGet package

✅ Configured connection settings

✅ Connected to MT5 terminal via gRPC

✅ Retrieved account balance programmatically


**This was a low-level approach** with direct use of `MT5Account` and gRPC.

---

## What's Next?

Now that you have a working project, you can:

### 1. Study the Full SDK Architecture

Read [Getting Started](Getting_Started.md) to learn about:

- **MT5Account** (Low-Level) - what you just used
- **MT5Service** (Wrappers) - convenient wrappers over MT5Account
- **MT5Sugar** (High-Level) - syntactic sugar for rapid development

### 2. Add More Functionality

**Examples of what you can do with MT5Account (Low-Level API):**

```csharp
// Get all open positions and orders
var openedOrders = await mt5Account.OpenedOrdersAsync(
    sortMode: BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc
);

// Send a market order
var orderRequest = new OrderSendRequest
{
    Symbol = "EURUSD",
    Volume = 0.01,
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy
};
var orderResponse = await mt5Account.OrderSendAsync(orderRequest);
Console.WriteLine($"Order: {orderResponse.Order}, Deal: {orderResponse.Deal}");

// Get real-time ticks (streaming)
var symbols = new[] { "EURUSD", "GBPUSD" };
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
try
{
    await foreach (var tickData in mt5Account.OnSymbolTickAsync(symbols, cts.Token))
    {
        var tick = tickData.SymbolTick;
        Console.WriteLine($"Symbol: {tick.Symbol}, Bid: {tick.Bid}, Ask: {tick.Ask}");
        // Use break to exit the loop
    }
}
catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Cancelled)
{
    Console.WriteLine("Timeout expired");
}

// Get position history
var positionsHistory = await mt5Account.PositionsHistoryAsync(
    sortType: AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeAsc,
    openFrom: DateTime.UtcNow.AddDays(-7),
    openTo: DateTime.UtcNow,
    page: 0,
    size: 100
);
foreach (var position in positionsHistory.HistoryPositions)
{
    Console.WriteLine($"Ticket: {position.PositionTicket}, Profit: {position.Profit}");
}
```

### 3. Copy Ready-Made Classes from Repository

If you want to use **MT5Service** or **MT5Sugar** in your project:

1. Clone the CSharpMT5 repository
2. Copy `MT5Service.cs` and/or `MT5Sugar.cs` files to your project
3. Use convenient high-level methods

**Example with MT5Sugar:**

```csharp
var sugar = new MT5Sugar(mt5Account, instanceId);

// Open Buy position
await sugar.Buy("EURUSD", 0.01);

// Close all positions for a symbol
await sugar.CloseAllPositions("EURUSD");

// Get balance in one line
var balance = await sugar.GetBalance();
```

### 4. Study Ready-Made Examples

The CSharpMT5 repository contains many examples:

- [Orchestrators](Strategies/Strategies.Master.Overview.md) - ready-made trading strategies
- [Adaptive Preset](Strategies/Presets/AdaptiveMarketModePreset.md) - smart multi-strategy
- [User Code Sandbox](UserCode_Sandbox_Guide.md) - template for your strategies

### 5. Read Additional Guides

- [Sync vs Async](Sync_vs_Async.md) - when to use synchronous/asynchronous methods
- [gRPC Stream Management](GRPC_STREAM_MANAGEMENT.md) - working with streaming data
- [Return Codes Reference](ReturnCodes_Reference_EN.md) - operation return codes

---


### Do I Need to Install MT5 Terminal?

**No!** The MetaRPC gateway connects to MT5 servers itself. You only need:

- MT5 account login/password
- Broker server name
- Access to gRPC gateway

---

## Your Project Structure

After completing all steps, your project structure should look like this:

```
MyMT5Project/
├── appsettings.json          # Connection configuration
├── MyMT5Project.csproj       # Project file with dependencies
├── Program.cs                # Main application code
└── bin/                      # Compiled files (created automatically)
    └── Debug/
        └── net8.0/
            ├── MyMT5Project.exe
            └── appsettings.json
```

---

## Summary: What We Did

In this guide, you created a minimalist project that:

1. **Uses only NuGet package** - no repository cloning required
2. **Connects to MT5** through gRPC gateway using `MT5Account` class from package
3. **Reads configuration** from `appsettings.json`
4. **Calls real low-level methods** - `ConnectByServerNameAsync()`, `AccountSummaryAsync()`
5. **Works with proto structures** - `AccountSummaryData` with fields like `AccountBalance`, `AccountEquity`

**This is the foundation** for any of your MT5 projects in C# using direct low-level API.

---

## Next Steps

Now you're ready for:

- 📖 [Getting Started](Getting_Started.md) - Complete SDK architecture study
- 📖 [MT5Account API](API_Reference/MT5Account.API.md) - Low-level API reference
- 📖 [MT5Service API](API_Reference/MT5Service.API.md) - Convenient wrappers
- 📖 [MT5Sugar API](API_Reference/MT5Sugar.API.md) - High-level API
- 🎯 [Orchestrators](Strategies/Strategies.Master.Overview.md) - Ready-made trading strategies

---

**Good luck developing your trading systems!**

> "The best way to learn something is to build it from scratch. Now you have the foundation. Build."
>
> — MetaRPC Team
