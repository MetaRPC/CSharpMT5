# 🚀 Place Market Order (`PlaceMarket`)

> **Request:** Place a market BUY or SELL order with sane defaults and automatic symbol selection.

## Overview

`PlaceMarket` is a high-level convenience method that simplifies placing market orders by:

- Automatically ensuring symbol is selected and synchronized
- Using reflection to avoid hard dependency on enum names
- Providing sensible defaults for all optional parameters
- Supporting SL/TP, comments, and slippage/deviation

---

## Method Signature

```csharp
public static async Task<OrderSendData> PlaceMarket(
    this MT5Service svc,
    string symbol,
    double volume,
    bool isBuy,
    double? sl = null,
    double? tp = null,
    string? comment = null,
    int deviationPoints = 0,
    int timeoutSec = 15,
    CancellationToken ct = default)
```

---

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `svc` | `MT5Service` | - | Extension method target |
| `symbol` | `string` | - | Trading symbol (e.g., `"EURUSD"`, `"XAUUSD"`) |
| `volume` | `double` | - | Volume in lots (e.g., `0.01`, `1.0`) |
| `isBuy` | `bool` | - | `true` for BUY, `false` for SELL |
| `sl` | `double?` | `null` | Stop Loss price (absolute price, not points) |
| `tp` | `double?` | `null` | Take Profit price (absolute price, not points) |
| `comment` | `string?` | `null` | Order comment (optional) |
| `deviationPoints` | `int` | `0` | Maximum allowed slippage in points |
| `timeoutSec` | `int` | `15` | Operation timeout in seconds |
| `ct` | `CancellationToken` | `default` | Cancellation token |

---

## Return Value

**Type:** `OrderSendData`

Contains the result of the order placement:

```csharp
public class OrderSendData
{
    public uint ReturnedCode { get; set; }        // 0 = success, other = error code
    public ulong Deal { get; set; }               // Deal ticket (for market orders)
    public ulong Order { get; set; }              // Order ticket
    public double Volume { get; set; }            // Executed volume
    public double Price { get; set; }             // Execution price
    public double Bid { get; set; }               // Bid price at execution
    public double Ask { get; set; }               // Ask price at execution
    public string Comment { get; set; }           // Server comment/error description
    // ... additional fields
}
```

---

## How It Works

1. **Symbol Selection:** Calls `EnsureSelected` to ensure symbol is available and synchronized
2. **Request Building:** Creates `OrderSendRequest` with provided parameters
3. **Reflection Magic:** Uses reflection to set `Operation` field (BUY=0, SELL=1) to avoid hard dependency on enum names
4. **Order Execution:** Calls `OrderSendAsync` with configured timeout
5. **Result:** Returns `OrderSendData` with execution details

---

## Common Use Cases

### 1️⃣ Simple Market BUY
```csharp
var result = await svc.PlaceMarket("EURUSD", volume: 0.01, isBuy: true);

Console.WriteLine($"Order placed! Deal: {result.Deal}, Price: {result.Price:F5}");
```

### 2️⃣ Market SELL with Stop Loss
```csharp
double currentBid = 1.08450;
double stopLoss = currentBid + 0.0010; // 10 pips above (for SELL)

var result = await svc.PlaceMarket(
    symbol: "EURUSD",
    volume: 0.10,
    isBuy: false,          // SELL
    sl: stopLoss
);

Console.WriteLine($"SELL order placed with SL at {stopLoss:F5}");
```

### 3️⃣ Market BUY with SL & TP
```csharp
double currentAsk = 1.08450;
double stopLoss = currentAsk - 0.0020;    // 20 pips below
double takeProfit = currentAsk + 0.0030;  // 30 pips above

var result = await svc.PlaceMarket(
    symbol: "EURUSD",
    volume: 0.05,
    isBuy: true,
    sl: stopLoss,
    tp: takeProfit,
    comment: "My strategy v1.0"
);

if (result.ReturnedCode == 0)
{
    Console.WriteLine($"✓ Order executed at {result.Price:F5}");
    Console.WriteLine($"  SL: {stopLoss:F5}, TP: {takeProfit:F5}");
}
else
{
    Console.WriteLine($"✗ Order failed: {result.Comment}");
}
```

### 4️⃣ Market Order with Slippage Control
```csharp
var result = await svc.PlaceMarket(
    symbol: "XAUUSD",
    volume: 0.01,
    isBuy: true,
    deviationPoints: 50,  // Allow up to 50 points slippage
    timeoutSec: 10
);

Console.WriteLine($"Executed at {result.Price:F2} (Bid: {result.Bid:F2}, Ask: {result.Ask:F2})");
```

### 5️⃣ Error Handling
```csharp
try
{
    var result = await svc.PlaceMarket("EURUSD", 0.01, isBuy: true);

    if (result.ReturnedCode == 0)
    {
        Console.WriteLine($"✓ Success! Deal: {result.Deal}");
    }
    else
    {
        Console.WriteLine($"✗ Trade rejected: Code {result.ReturnedCode} - {result.Comment}");
    }
}
catch (ApiExceptionMT5 ex)
{
    Console.WriteLine($"MT5 Error: {ex.ErrorCode} - {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

---

## Notes & Tips

- **Price not required:** For market orders, the server automatically executes at current market price
- **SL/TP are absolute prices:** Not distances in points/pips
- **Deviation:** Slippage protection—order rejected if execution price deviates more than specified points
- **Symbol selection:** Automatically handled via `EnsureSelected`
- **Timeout:** Default 15 seconds should be sufficient for most markets
- **Return code:** Always check `ReturnedCode == 0` for success
- **Risk-based trading:** Consider using `BuyMarketByRisk()` / `SellMarketByRisk()` for automatic position sizing

---

## Return Codes

| Code | Description |
|------|-------------|
| `0` | Success - order executed |
| `10004` | Trade server is busy |
| `10006` | Request rejected |
| `10013` | Invalid request |
| `10014` | Invalid volume |
| `10015` | Invalid price |
| `10016` | Invalid stops (SL/TP) |
| `10019` | No money (insufficient margin) |
| `10027` | Trade disabled for symbol |

Full list: [Return Codes Reference](../../ReturnCodes_Reference_EN.md)

---

## Comparison with Low-Level API

| Feature | `PlaceMarket` (Sugar) | `OrderSendAsync` (Low-level) |
|---------|-----------------------|------------------------------|
| Symbol selection | ✅ Automatic | ❌ Manual |
| Default parameters | ✅ Yes | ❌ All required |
| Syntax | ✅ Clean | ⚠️ Verbose |
| Flexibility | ⚠️ Limited | ✅ Full control |
| Best for | Quick trades | Complex strategies |

---

## Related Methods

- `BuyMarketByRisk()` — BUY with risk-based position sizing ⭐
- `SellMarketByRisk()` — SELL with risk-based position sizing ⭐
- [PlacePending](PlacePending.md) — Place limit/stop pending orders
- [ModifySlTpAsync](ModifySlTpAsync.md) — Modify SL/TP after placement
- [CloseByTicket](CloseByTicket.md) — Close position by ticket
- `OrderSendAsync` (MT5Account) — Low-level order placement

---

## Example: Complete Trading Flow

```csharp
// Get current prices
var tick = await svc.SymbolInfoTickAsync("EURUSD");
double ask = tick.Ask;
double bid = tick.Bid;

// Calculate SL/TP levels (20 pips SL, 30 pips TP)
double point = await svc.GetPointAsync("EURUSD");
double slDistance = 20 * 10 * point; // 20 pips
double tpDistance = 30 * 10 * point; // 30 pips

double sl = ask - slDistance;  // BUY SL below ask
double tp = ask + tpDistance;  // BUY TP above ask

Console.WriteLine($"Placing BUY order...");
Console.WriteLine($"  Entry: {ask:F5}");
Console.WriteLine($"  SL:    {sl:F5} (-{slDistance / point:F1} points)");
Console.WriteLine($"  TP:    {tp:F5} (+{tpDistance / point:F1} points)");

var result = await svc.PlaceMarket(
    symbol: "EURUSD",
    volume: 0.10,
    isBuy: true,
    sl: sl,
    tp: tp,
    comment: "Auto trade",
    deviationPoints: 10
);

if (result.ReturnedCode == 0)
{
    Console.WriteLine($"\n✓ Order executed successfully!");
    Console.WriteLine($"  Deal:   {result.Deal}");
    Console.WriteLine($"  Price:  {result.Price:F5}");
    Console.WriteLine($"  Volume: {result.Volume:F2} lots");
}
else
{
    Console.WriteLine($"\n✗ Order failed!");
    Console.WriteLine($"  Code:    {result.ReturnedCode}");
    Console.WriteLine($"  Comment: {result.Comment}");
}
```

**Sample Output:**
```
Placing BUY order...
  Entry: 1.08450
  SL:    1.08250 (-200.0 points)
  TP:    1.08750 (+300.0 points)

✓ Order executed successfully!
  Deal:   123456789
  Price:  1.08451
  Volume: 0.10 lots
```
