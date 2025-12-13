# User Code Sandbox Guide

## What is This?

This is **your sandbox** for writing custom MT5 trading code. Connection setup is already done - you just add your trading logic!

## Quick Start

1. **Uncomment the case block in Program.cs**
   - Open `Program.cs` (root folder)
   - Find the `USER CODE SANDBOX` section (around line 172)
   - Uncomment the case block for "usercode", "user", "sandbox"

2. **Open `ProgramUserCode.cs`**
   - Write your code or uncomment examples

3. **Run:**
   ```bash
   dotnet run usercode
   ```

That's it! Your code will execute with full MT5 connection.

## How to Use

### Option 1: Uncomment Examples

The file contains 9 ready-to-use examples:

```csharp
// Example 1: Get account information
// var accountInfo = await service.GetAccountInfoAsync();
// Console.WriteLine($"Balance: ${accountInfo.Balance}");
```

Just remove `//` to activate!

### Option 2: Write Your Own Code

Add your logic between the markers:

```csharp
// ═══════════════════════════════════════
// YOUR CODE STARTS HERE ↓
// ═══════════════════════════════════════

// Your trading strategy here...

// ═══════════════════════════════════════
// YOUR CODE ENDS HERE ↑
// ═══════════════════════════════════════
```

## Available Commands

Run your code with any of these:

```bash
dotnet run usercode
dotnet run user
dotnet run sandbox
```

## What's Already Set Up

✓ **Connection** - MT5 Terminal connected via gRPC

✓ **Configuration** - Loaded from `appsettings.json`

✓ **MT5Account** - Low-level gRPC client (variable: `account`)

✓ **MT5Service** - High-level wrapper with Sugar API (variable: `service`)

## Can You Mix API Levels?

**Yes!** You can use all three levels in one file:

```csharp
// Low-level (direct gRPC)
var positions = await account.PositionsGetAsync(new Empty());

// Mid-level (MT5Service)
var accountInfo = await service.GetAccountInfoAsync();

// High-level (Sugar API)
var result = await service.BuyMarketByRisk("EURUSD", 10.0, 20);
```

All three variables are available simultaneously:
- `account` - for full control (gRPC)
- `service` - for convenient methods (Service + Sugar)

## Quick Reference

### Get Account Info
```csharp
var accountInfo = await service.GetAccountInfoAsync();
Console.WriteLine($"Balance: ${accountInfo.Balance}");
Console.WriteLine($"Equity: ${accountInfo.Equity}");
```

### Get Current Price
```csharp
var tick = await service.SymbolInfoTickAsync("EURUSD");
Console.WriteLine($"EURUSD Bid: {tick.Bid}, Ask: {tick.Ask}");
```

### Open Market Order (Risk-Based)
```csharp
var result = await service.BuyMarketByRisk(
    symbol: "EURUSD",
    riskAmount: 10.0,        // Risk $10
    stopLossPoints: 20       // SL 20 points away
);

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Order opened: #{result.Order}");
}
else
{
    Console.WriteLine($"❌ Failed: {result.ReturnedCodeDescription}");
}
```

### Place Pending Order
```csharp
var result = await service.BuyLimitPoints(
    symbol: "EURUSD",
    volume: 0.01,
    offsetPoints: 20,        // 20 points below Ask
    stopLossPoints: 15,
    takeProfitPoints: 30
);
```

### Get Open Positions
```csharp
var positions = await account.PositionsGetAsync(new Empty());
Console.WriteLine($"Open positions: {positions.Data.Positions.Count}");

foreach (var pos in positions.Data.Positions)
{
    Console.WriteLine($"  #{pos.Ticket} {pos.Symbol} {pos.Type} {pos.Volume} lots");
}
```

### Close All Positions
```csharp
await service.CloseAll("EURUSD");
```

## Return Codes (RetCodes)

**Always check RetCode after trading operations!**

```csharp
if (result.ReturnedCode == 10009)  // Success for market orders
if (result.ReturnedCode == 10008)  // Success for pending orders
```

Complete list: [ReturnCodes_Reference_EN.md](ReturnCodes_Reference_EN.md)

## Documentation

- [MT5Account API](API_Reference/MT5Account.API.md) - Low-level gRPC client (72 methods)
- [MT5Service API](API_Reference/MT5Service.API.md) - Mid-level convenience wrapper (66 methods)
- [MT5Sugar API](API_Reference/MT5Sugar.API.md) - High-level extension methods (48 methods + 2 records)

## Tips

1. **Start simple** - Uncomment one example at a time
2. **Check RetCode** - Always validate trading operations
3. **Use Sugar API** - Methods like `BuyMarketByRisk()` are easier than low-level
4. **Test on demo** - Make sure you're using a demo account first!
5. **Read docs** - [ReturnCodes_Reference_EN.md](ReturnCodes_Reference_EN.md) explains all error codes
