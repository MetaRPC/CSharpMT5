# User Code Sandbox Guide

## What is this?

This is **your sandbox** for writing custom MT5 trading code in **C#**. Connection setup is already done — you only add your trading logic inside `RunAsync()`.

**File:** `Examples/UserCode/ProgramUserCode.cs`

## 🏁 Quick Start

1. **Enable the command** — uncomment the `usercode` case block in `Program.cs` (around line ~172).
2. **Open `Examples/UserCode/ProgramUserCode.cs`** — write your code, or uncomment the ready examples.
3. **Run:**
   ```bash
   dotnet run usercode
   ```

That's it — your code runs with a live MT5 connection (and disconnects cleanly in `finally`).

## How to use

### Option 1 — Uncomment examples

`ProgramUserCode.cs` ships with ready-to-use examples. Just remove the `//`:

```csharp
// Example 1: Get account information
// var summary = await service.AccountSummaryAsync();
// Console.WriteLine($"Balance: {summary.AccountBalance} {summary.AccountCurrency}");
```

### Option 2 — Write your own

Add your logic between the markers:

```csharp
// ═════════════════════════════════════════════════════════════════
// YOUR CODE STARTS HERE ↓
// ═════════════════════════════════════════════════════════════════

// Your trading strategy here...

// ═════════════════════════════════════════════════════════════════
// YOUR CODE ENDS HERE ↑
// ═════════════════════════════════════════════════════════════════
```

## Available commands

```bash
dotnet run usercode
dotnet run user
dotnet run sandbox
```

## What's already configured

- **Connection** — MT5 terminal connected via gRPC (`ConnectionHelper.CreateAndConnectAccountAsync`)
- **Configuration** — loaded from `Config/appsettings.json`
- **`account`** — low-level gRPC client (`MT5Account`)
- **`service`** — mid-level wrapper (`MT5Service`); the high-level **Sugar** helpers are extension methods on it, so you call them as `service.PlaceMarket(...)`, `service.BuyMarketByRisk(...)`, etc.
- **async/await** — all methods are asynchronous (each also has a blocking sync twin)

> There is no separate `sugar` variable: MT5Sugar is a set of extension methods on `MT5Service`, so everything hangs off `service`.

## 🧩 Can I mix API levels?

**Yes** — all three layers are reachable from the two variables:

```csharp
// Low-level (direct gRPC): rich protobuf object
MT5Account acc = account;
AccountSummaryData summary = await acc.AccountSummaryAsync();
double balanceLow = summary.AccountBalance;

// Mid-level (MT5Service): unwrapped primitive
double balanceSvc = await service.GetBalanceAsync();

// High-level (Sugar extension on service)
var result = await service.BuyMarketByRisk("EURUSD", stopPoints: 20, riskMoney: 10.0);
```

- `account` — full control (gRPC protobuf)
- `service` — convenient wrappers **and** Sugar helpers

## Quick reference

### Get account information

```csharp
// Mid-level shortcuts (easiest)
double balance = await service.GetBalanceAsync();
double equity  = await service.GetEquityAsync();
Console.WriteLine($"Balance: {balance:F2} | Equity: {equity:F2}");

// Full snapshot (one call)
var summary = await service.AccountSummaryAsync();
Console.WriteLine($"Balance: {summary.AccountBalance:F2} {summary.AccountCurrency}");
```

### Get current price

```csharp
double bid = await service.GetBidAsync("EURUSD");
double ask = await service.GetAskAsync("EURUSD");
Console.WriteLine($"EURUSD: Bid={bid:F5}, Ask={ask:F5}");

// Or the full tick
var tick = await service.SymbolInfoTickAsync("EURUSD");
Console.WriteLine($"Bid={tick.Bid:F5}, Ask={tick.Ask:F5}");
```

### Open a market order

```csharp
// Simple market BUY
var r1 = await service.BuyMarketAsync("EURUSD", 0.01);

// Generic market order (BUY/SELL) with optional SL/TP
var r2 = await service.PlaceMarket("EURUSD", 0.01, isBuy: true, sl: 1.0800, tp: 1.0900);

// Risk-based sizing: 20-point stop, risk $10
var r3 = await service.BuyMarketByRisk("EURUSD", stopPoints: 20, riskMoney: 10.0, tpPoints: 40);

if (r3.ReturnedCode == 10009)
    Console.WriteLine($"✅ Order #{r3.Order}, volume {r3.Volume}, price {r3.Price:F5}");
else
    Console.WriteLine($"❌ {r3.ReturnedCodeDescription}");
```

### Place a pending order (points-based)

```csharp
// Buy Limit 50 points below Ask, SL 100 / TP 200 points
await service.BuyLimitPoints("EURUSD", 0.01, priceOffsetPoints: 50, slPoints: 100, tpPoints: 200);

// Sell Stop 50 points below Bid
await service.SellStopPoints("EURUSD", 0.01, priceOffsetPoints: 50, slPoints: 100, tpPoints: 200);
```

### Get open positions

```csharp
var opened = await service.OpenedOrdersAsync();
Console.WriteLine($"Open positions: {opened.PositionInfos.Count}");
foreach (var pos in opened.PositionInfos)
    Console.WriteLine($"  #{pos.Ticket} {pos.Symbol} {pos.Volume:F2} lot, Profit: {pos.Profit:F2}");
```

### Close positions

```csharp
await service.CloseByTicket(ticket);          // specific position
await service.CloseAllPositions("EURUSD");     // all market positions for a symbol
await service.CloseAllPositions();             // all positions (all symbols)
await service.CancelAll("EURUSD");             // cancel pending orders for a symbol
```

### Modify a position

```csharp
await service.ModifySlTpAsync(ticket, sl: 1.0850, tp: 1.0950);
```

### Calculate position size by risk

```csharp
double volume = await service.CalcVolumeForRiskAsync("EURUSD", stopPoints: 200, riskMoney: 10.0);
Console.WriteLine($"Volume for $10 risk with 200-point SL: {volume:F2} lot");
var result = await service.PlaceMarket("EURUSD", volume, isBuy: true);
```

## Return Codes (RetCodes)

**Always check `ReturnedCode` after trading operations!**

```csharp
using mt5_term_api;

var request = new OrderSendRequest
{
    Symbol    = "EURUSD",
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
    Volume    = 0.01,
    Price     = (await service.GetAskAsync("EURUSD"))  // market BUY uses current Ask
};

var result = await service.OrderSendAsync(request);

if (result.ReturnedCode == 10009)       // market order executed
    Console.WriteLine($"[OK] Order #{result.Order}");
else if (result.ReturnedCode == 10008)  // pending order placed
    Console.WriteLine($"[OK] Pending order #{result.Order}");
else
    Console.WriteLine($"[X] Failed: {result.ReturnedCodeDescription} (code {result.ReturnedCode})");
```

**Common RetCodes:**

- `10009` — market order executed successfully
- `10008` — pending order placed successfully
- `10019` — not enough money (insufficient margin)
- `10016` — invalid stops (SL/TP too close to price)
- `10018` — market closed

Full list: [RETURN_CODES_REFERENCE.md](./RETURN_CODES_REFERENCE.md)

## ℹ️ Error handling

Two independent things can go wrong — handle **both**:

### 1. Connection / network errors → exceptions

```csharp
using Grpc.Core;

try
{
    var result = await service.OrderSendAsync(request);
}
catch (ApiExceptionMT5 ex)   // business error from the server
{
    Console.WriteLine($"API error: {ex.ErrorCode}");
}
catch (RpcException ex)       // transport/network error
{
    Console.WriteLine($"gRPC error: {ex.Status}");
}
```

### 2. Trade rejections → `ReturnedCode`

```csharp
var result = await service.OrderSendAsync(request);
if (result.ReturnedCode != 10009)
    Console.WriteLine($"Trade rejected: {result.ReturnedCodeDescription}");
```

### Recommended pattern

```csharp
try
{
    var result = await service.OrderSendAsync(request);
    if (result.ReturnedCode == 10009)
        Console.WriteLine($"[OK] Order #{result.Order}");
    else
        Console.WriteLine($"[X] {result.ReturnedCodeDescription} (code {result.ReturnedCode})");
}
catch (Exception ex)
{
    Console.WriteLine($"[X] Error: {ex.Message}");
}
```

## 📝 Documentation

- [MT5Account Master Overview](../MT5Account/MT5Account.Master.Overview.md) — complete low-level API reference
- [MT5Service Overview](../MT5Service/MT5Service.Overview.md) — mid-level wrappers
- [MT5Sugar API Overview](../MT5Sugar/MT5Sugar.API_Overview.md) — high-level Sugar helpers
- [ENUMs Usage Reference](./ENUMS_USAGE_REFERENCE.md) — all enums in one place

## 🔑 Configuration

### Method 1 — appsettings.json (recommended)

`Config/appsettings.json`:

```json
{
  "MT5": {
    "User": 5056776453,
    "Password": "YourPassword",
    "ServerName": "MetaQuotes-Demo",
    "GrpcServer": "https://mt5.mrpc.pro:443",
    "ApiKey": "your-api-key",
    "ConnectTimeoutSeconds": 60
  }
}
```

### Method 2 — environment variable override

`MRPC_API_KEY` overrides the config API key at runtime (keeps real keys out of source control):

```powershell
# Windows PowerShell
$env:MRPC_API_KEY = "your-api-key"   # or "TRIAL" for trial access
dotnet run usercode
```

```bash
# Linux/Mac
export MRPC_API_KEY="your-api-key"
dotnet run usercode
```

## Tips

1. **Start simple** — uncomment one example at a time.
2. **Use Sugar helpers** — `BuyMarketByRisk(...)` and `CalcVolumeForRiskAsync(...)` are easier than hand-building requests.
3. **Check `ReturnedCode`** — always validate trading operations (`10009` = success).
4. **Test on demo** — make sure you're on a demo account first.
5. **Read the return codes** — [RETURN_CODES_REFERENCE.md](./RETURN_CODES_REFERENCE.md) explains every code.
6. **Use async/await** — every method is async; don't forget `await`.
7. **IDE autocomplete** — the strongly-typed API surfaces everything as you type.
8. **Browse examples** — see the `Examples/` folder (LowLevel, Service, Sugar, Orchestrators).

## Common mistakes

❌ **Forgot `await`**
```csharp
var balance = service.GetBalanceAsync(); // returns a Task, not the value!
```

✅ **Always `await`**
```csharp
var balance = await service.GetBalanceAsync();
```

❌ **Not handling exceptions**
```csharp
var r = await service.OrderSendAsync(request); // may throw on network/API error
```

✅ **Wrap in try/catch**
```csharp
try { var r = await service.OrderSendAsync(request); }
catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
```

❌ **Not checking `ReturnedCode`**
```csharp
var r = await service.OrderSendAsync(request); // assuming success!
```

✅ **Check it**
```csharp
var r = await service.OrderSendAsync(request);
if (r.ReturnedCode != 10009) { Console.WriteLine($"Failed: {r.ReturnedCodeDescription}"); return; }
```

## Getting help

- **Protobuf types:** see the generated files in `package/` (the `mt5_term_api` namespace).
- **Return codes:** [RETURN_CODES_REFERENCE.md](./RETURN_CODES_REFERENCE.md)
- **Examples:** the `Examples/` folder has working LowLevel / Service / Sugar / Orchestrator programs.
- **API docs:** the `docs/` folder has the full per-layer reference.

## Example: complete sandbox body

A minimal strategy with proper error handling (the connection scaffolding is already provided by `ProgramUserCode.RunAsync`):

```csharp
// account (MT5Account) and service (MT5Service) are already connected.

// 1) Account state
double balance = await service.GetBalanceAsync();
Console.WriteLine($"Balance: {balance:F2}");

// 2) Size by risk: $ (1% of balance), 50-point stop
double risk   = balance * 0.01;
double volume = await service.CalcVolumeForRiskAsync("EURUSD", stopPoints: 50, riskMoney: risk);

// 3) Open and validate
try
{
    var result = await service.PlaceMarket("EURUSD", volume, isBuy: true);
    if (result.ReturnedCode == 10009)
        Console.WriteLine($"[OK] Position #{result.Order} with {volume:F2} lot");
    else
        Console.WriteLine($"[X] {result.ReturnedCodeDescription} (code {result.ReturnedCode})");
}
catch (Exception ex)
{
    Console.WriteLine($"[X] Error: {ex.Message}");
}
```

**Happy trading!**
