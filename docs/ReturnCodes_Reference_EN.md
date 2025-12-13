# MT5 Return Codes (RetCodes) Reference

## What is RetCode?

**RetCode** (Return Code) is a status code that MT5 Terminal returns after executing a trading operation. The code indicates success or the reason for failure.

RetCodes **only appear in trading methods** because only trading operations go through the broker and can be rejected for various reasons (insufficient margin, invalid price, market closed, etc.).

## RetCode Source

RetCodes are **standard MQL5 codes** defined in the official MetaQuotes documentation:

**MQL5 Documentation**: [Trade Result Codes (ENUM_TRADE_RETCODE)](https://www.mql5.com/en/docs/constants/errorswarnings/enum_trade_return_codes)

These codes are:

- **Unified across all languages** (C#, Python, Java, Node.js, Go, PHP)
- **Unified with MT5 Terminal** - returned directly from the trade server
- **Defined in protobuf** - enum `MqlErrorTradeCode` in the API proto contract

## Where is RetCode Used in the API?

RetCode is returned in three types of trading operations:

### 1. OrderSend (opening orders)
```csharp
var result = await mt5Service.BuyMarketAsync("EURUSD", 0.01, stopLoss: 1.0800);
Console.WriteLine($"RetCode: {result.ReturnedCode}"); // uint32
Console.WriteLine($"String Code: {result.ReturnedStringCode}"); // "TRADE_RETCODE_DONE"
Console.WriteLine($"Description: {result.ReturnedCodeDescription}"); // "Request completed"
```

### 2. OrderModify (modifying SL/TP)
```csharp
var result = await mt5Account.OrderModifyAsync(ticket: 12345, stopLoss: 1.0850);
if (result.Data.ReturnedCode == 10009) {
    Console.WriteLine("Modification successful");
}
```

### 3. OrderClose (closing positions)
```csharp
var result = await mt5Account.OrderCloseAsync(ticket: 12345);
if (result.Data.ReturnedCode != 10009) {
    Console.WriteLine($"Close failed: {result.Data.ReturnedCodeDescription}");
}
```

## Why RetCode Only in Trading Methods?

**Information methods** (getting prices, symbols, balance) **DO NOT return RetCode** because:

- They don't go through the broker
- They can't be "rejected" - either data exists or not (gRPC error)
- They work with local terminal data

**Trading methods** return RetCode because:

- Request is sent to broker via trade server
- Broker validates: margin, symbol rules, trading hours, limits
- Broker can reject request for dozens of reasons
- Each reason has its unique code

## Complete RetCode List

### ✅ Success Codes

| Code | Enum | Description |
|------|------|-------------|
| **0** | `TRADE_RETCODE_SUCCESS` | **Successful execution (non-trading operation)** |
| **10008** | `TRADE_RETCODE_PLACED` | **Pending order placed** |
| **10009** | `TRADE_RETCODE_DONE` | **Request completed (market order opened)** |
| **10010** | `TRADE_RETCODE_DONE_PARTIAL` | **Partial execution** |

### ⚠️ Requote Codes

| Code | Enum | Description |
|------|------|-------------|
| **10004** | `TRADE_RETCODE_REQUOTE` | **Requote** - price changed, need to request again |
| **10020** | `TRADE_RETCODE_PRICE_CHANGED` | **Price changed** - similar to requote |

### ❌ Request Rejection Codes (Validation Errors)

| Code | Enum | Description | Reason |
|------|------|-------------|--------|
| **10006** | `TRADE_RETCODE_REJECT` | **Request rejected** | General rejection by broker |
| **10007** | `TRADE_RETCODE_CANCEL` | **Request canceled by trader** | User canceled request manually |
| **10013** | `TRADE_RETCODE_INVALID` | **Invalid request** | Incorrect parameters |
| **10014** | `TRADE_RETCODE_INVALID_VOLUME` | **Invalid volume** | Volume < MinVolume or > MaxVolume |
| **10015** | `TRADE_RETCODE_INVALID_PRICE` | **Invalid price** | Price doesn't comply with symbol rules |
| **10016** | `TRADE_RETCODE_INVALID_STOPS` | **Invalid stops** | SL/TP too close to price or violates StopLevel |
| **10022** | `TRADE_RETCODE_INVALID_EXPIRATION` | **Invalid expiration date** | Expiration time is incorrect |
| **10030** | `TRADE_RETCODE_INVALID_FILL` | **Invalid fill type** | Fill policy not supported by symbol |
| **10035** | `TRADE_RETCODE_INVALID_ORDER` | **Invalid order type** | Order type prohibited for symbol |
| **10038** | `TRADE_RETCODE_INVALID_CLOSE_VOLUME` | **Invalid close volume** | Volume greater than current position |

### 🚫 Trading Restriction Codes

| Code | Enum | Description | Reason |
|------|------|-------------|--------|
| **10017** | `TRADE_RETCODE_TRADE_DISABLED` | **Trading disabled** | Trading disabled for symbol |
| **10018** | `TRADE_RETCODE_MARKET_CLOSED` | **Market closed** | Outside trading hours |
| **10026** | `TRADE_RETCODE_SERVER_DISABLES_AT` | **Autotrading disabled by server** | Broker disabled autotrading |
| **10027** | `TRADE_RETCODE_CLIENT_DISABLES_AT` | **Autotrading disabled by client** | AutoTrading disabled in terminal |
| **10032** | `TRADE_RETCODE_ONLY_REAL` | **Operation for live accounts only** | Action unavailable on demo |
| **10042** | `TRADE_RETCODE_LONG_ONLY` | **Long positions only** | Short positions prohibited |
| **10043** | `TRADE_RETCODE_SHORT_ONLY` | **Short positions only** | Long positions prohibited |
| **10044** | `TRADE_RETCODE_CLOSE_ONLY` | **Close only** | Opening new positions prohibited |
| **10045** | `TRADE_RETCODE_FIFO_CLOSE` | **FIFO close only** | FIFO rule mandatory (US regulation) |
| **10046** | `TRADE_RETCODE_HEDGE_PROHIBITED` | **Hedging prohibited** | Cannot open opposite positions |

### 💰 Resource Limit Codes

| Code | Enum | Description | Reason |
|------|------|-------------|--------|
| **10019** | `TRADE_RETCODE_NO_MONEY` | **Insufficient funds** | Free Margin < Required Margin |
| **10033** | `TRADE_RETCODE_LIMIT_ORDERS` | **Pending orders limit reached** | Maximum number of orders |
| **10034** | `TRADE_RETCODE_LIMIT_VOLUME` | **Volume limit reached** | Total position volume exceeded |
| **10040** | `TRADE_RETCODE_LIMIT_POSITIONS` | **Open positions limit reached** | Maximum number of positions |

### 🔧 Technical Issue Codes

| Code | Enum | Description | Reason |
|------|------|-------------|--------|
| **10011** | `TRADE_RETCODE_ERROR` | **Request processing error** | Internal server error |
| **10012** | `TRADE_RETCODE_TIMEOUT` | **Request timeout** | Request not processed in time |
| **10021** | `TRADE_RETCODE_PRICE_OFF` | **No quotes** | Symbol price unavailable |
| **10024** | `TRADE_RETCODE_TOO_MANY_REQUESTS` | **Too many requests** | Rate limiting from broker |
| **10028** | `TRADE_RETCODE_LOCKED` | **Request locked for processing** | Previous request still processing |
| **10029** | `TRADE_RETCODE_FROZEN` | **Order/position frozen** | Temporary lock |
| **10031** | `TRADE_RETCODE_CONNECTION` | **No connection with trade server** | Connection lost |

### 🔄 State Management Codes

| Code | Enum | Description | Reason |
|------|------|-------------|--------|
| **10023** | `TRADE_RETCODE_ORDER_CHANGED` | **Order state changed** | Order already modified/closed |
| **10025** | `TRADE_RETCODE_NO_CHANGES` | **No changes in request** | New parameters match current ones |
| **10036** | `TRADE_RETCODE_POSITION_CLOSED` | **Position already closed** | Attempt to close non-existent position |
| **10039** | `TRADE_RETCODE_CLOSE_ORDER_EXIST` | **Close order already exists** | Duplicate close request |
| **10041** | `TRADE_RETCODE_REJECT_CANCEL` | **Pending order activation rejected** | Pending order cannot be activated |

## Usage Examples

### Example 1: Checking Order Opening Success

```csharp
using MetaRPC.CSharpMT5;
using mt5_term_api;

var config = ConnectionHelper.BuildConfiguration();
var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);
var service = new MT5Service(account);

// Open market order
var result = await service.BuyMarketAsync(
    symbol: "EURUSD",
    volume: 0.01,
    stopLoss: 1.0800,
    takeProfit: 1.0900
);

// Check RetCode
if (result.ReturnedCode == 10009) // TRADE_RETCODE_DONE
{
    Console.WriteLine($"✅ Order opened successfully!");
    Console.WriteLine($"   Ticket: #{result.Order}");
    Console.WriteLine($"   Volume: {result.Volume} lots");
    Console.WriteLine($"   Price: {result.Price}");
}
else if (result.ReturnedCode == 10008) // TRADE_RETCODE_PLACED
{
    Console.WriteLine($"✅ Pending order placed!");
    Console.WriteLine($"   Ticket: #{result.Order}");
}
else
{
    Console.WriteLine($"❌ Order failed!");
    Console.WriteLine($"   RetCode: {result.ReturnedCode}");
    Console.WriteLine($"   String: {result.ReturnedStringCode}");
    Console.WriteLine($"   Description: {result.ReturnedCodeDescription}");
}
```

### Example 2: Handling Common Errors

```csharp
var result = await service.SellMarketAsync("GBPUSD", 0.5);

switch (result.ReturnedCode)
{
    case 10009: // Success
        Console.WriteLine($"Order #{result.Order} opened at {result.Price}");
        break;

    case 10019: // No money
        Console.WriteLine("⚠️ Insufficient funds! Reduce volume or add margin.");
        break;

    case 10018: // Market closed
        Console.WriteLine("⚠️ Market is closed. Try again during trading hours.");
        break;

    case 10016: // Invalid stops
        Console.WriteLine("⚠️ Stop Loss or Take Profit too close to market price.");
        Console.WriteLine("   Increase distance between entry and SL/TP.");
        break;

    case 10014: // Invalid volume
        var limits = await service.GetVolumeLimitsAsync("GBPUSD");
        Console.WriteLine($"⚠️ Volume {result.Volume} is invalid.");
        Console.WriteLine($"   Min: {limits.MinVolume}, Max: {limits.MaxVolume}, Step: {limits.VolumeStep}");
        break;

    case 10004: // Requote
    case 10020: // Price changed
        Console.WriteLine("⚠️ Price changed. Retrying with new price...");
        // Retry request
        var retry = await service.SellMarketAsync("GBPUSD", 0.5);
        break;

    default:
        Console.WriteLine($"❌ Unexpected error: {result.ReturnedCodeDescription}");
        break;
}
```

### Example 3: Retry Logic for Requotes

```csharp
public static async Task<OrderSendData> SendWithRetry(
    MT5Service service,
    string symbol,
    double volume,
    int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        var result = await service.BuyMarketAsync(symbol, volume);

        // Success
        if (result.ReturnedCode == 10009 || result.ReturnedCode == 10008)
        {
            return result;
        }

        // Requote - retry
        if (result.ReturnedCode == 10004 || result.ReturnedCode == 10020)
        {
            Console.WriteLine($"Requote on attempt {attempt}, retrying...");
            await Task.Delay(100); // Small delay
            continue;
        }

        // Other error - abort
        Console.WriteLine($"Failed: {result.ReturnedCodeDescription}");
        return result;
    }

    throw new Exception($"Failed after {maxRetries} retries");
}
```

### Example 4: Sugar API with Auto-Check

```csharp
using MetaRPC.CSharpMT5;

var service = new MT5Service(account);

// Sugar API: risk-based order
var result = await service.BuyMarketByRisk(
    symbol: "EURUSD",
    riskAmount: 50.0,      // Risk $50
    stopLossPoints: 20     // SL 20 points away
);

// Sugar API automatically calculates volume
Console.WriteLine($"Calculated volume: {result.Volume} lots");

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Position opened with $50 risk at ticket #{result.Order}");
}
else
{
    Console.WriteLine($"❌ {result.ReturnedCodeDescription}");
}
```

### Example 5: Modify SL/TP with Validation

```csharp
// Modify SL/TP of existing position
var modifyResult = await mt5Account.OrderModifyAsync(
    ticket: 123456,
    stopLoss: 1.0850,
    takeProfit: 1.0950
);

if (modifyResult.Data.ReturnedCode == 10009)
{
    Console.WriteLine("✅ SL/TP updated successfully");
}
else if (modifyResult.Data.ReturnedCode == 10025) // No changes
{
    Console.WriteLine("⚠️ New SL/TP values are same as current - no action taken");
}
else if (modifyResult.Data.ReturnedCode == 10036) // Position closed
{
    Console.WriteLine("⚠️ Position already closed by SL/TP or manually");
}
else
{
    Console.WriteLine($"❌ Modify failed: {modifyResult.Data.ReturnedCodeDescription}");
}
```

## C# Enum

RetCodes are defined in enum `MqlErrorTradeCode`:

```csharp
using Mt5TermApi;

// Using enum instead of magic numbers
if (result.ReturnedCode == (uint32)MqlErrorTradeCode.TradeRetcodeDone)
{
    Console.WriteLine("Success!");
}

// Or simply use numeric constants
if (result.ReturnedCode == 10009)
{
    Console.WriteLine("Success!");
}
```

## Best Practices

1. **Always check RetCode** after trading operations
2. **10009 = success** for market orders, **10008 = success** for pending orders
3. **Requotes (10004, 10020)** - retry the request
4. **10019 (No Money)** - check Free Margin before order
5. **10016 (Invalid Stops)** - increase distance to SL/TP (check StopLevel)
6. **10018 (Market Closed)** - check symbol trading hours
7. **Use `returned_code_description`** to display user-friendly messages
