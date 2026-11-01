# MT5 Return Codes (RetCodes) Reference (C#)

## 🛡️ What is RetCode?

**RetCode** (Return Code) is a status code the MT5 terminal returns after a trading operation. It indicates success or the reason for failure.

RetCodes **appear only in trading methods** because only trading operations go through the broker and can be rejected (insufficient margin, invalid price, market closed, etc.).

### Quick Example: proper RetCode handling

```csharp
var result = await service.OrderSendAsync(request);

// ✅ ALWAYS check ReturnedCode explicitly
if (result.ReturnedCode == 10009) // TRADE_RETCODE_DONE
    Console.WriteLine($"✓ Trade successful! Order #{result.Order}");
else
    Console.WriteLine($"✗ Trade failed: {result.ReturnedCodeDescription} (code {result.ReturnedCode})");
```

**Key point:** no exception = the API call worked, but you still **check `ReturnedCode`** to know whether the **trade** succeeded.

---

## RetCode Source

RetCodes are **standard MQL5 codes** defined in official MetaQuotes documentation:

**MQL5 Documentation**: [Trade Server Return Codes (ENUM_TRADE_RETCODE)](https://www.mql5.com/en/docs/constants/errorswarnings/enum_trade_return_codes)

These codes are:

- **Unified across all languages** (C#, Python, Java, Node.js, Go, PHP)
- **Unified with the MT5 terminal** — returned directly from the trading server
- **Defined in protobuf** — delivered as the `uint` field `ReturnedCode` in trading result messages
- **Paired with text** — the reply also carries `ReturnedCodeDescription`, a ready-made human-readable message (no helper lookup needed)

---

## 📥 Where is RetCode used in the API?

RetCode is returned by **all trading operations** that modify positions or orders:

### 1. OrderSend (opening orders)

```csharp
using mt5_term_api;

var request = new OrderSendRequest
{
    Symbol    = "EURUSD",
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
    Volume    = 0.01,
    Price     = await service.GetAskAsync("EURUSD")
};

var result = await service.OrderSendAsync(request);
if (result.ReturnedCode == 10009)
    Console.WriteLine($"[OK] Order opened! Ticket: {result.Order}");
else
    Console.WriteLine($"[X] Order failed: {result.ReturnedCodeDescription}");
```

### 2. OrderModify (modifying SL/TP)

```csharp
var result = await service.ModifySlTpAsync(ticket, sl: 1.0850, tp: 1.0950);
if (result.ReturnedCode == 10009)
    Console.WriteLine("[OK] SL/TP modified");
else
    Console.WriteLine($"[X] Modification failed: {result.ReturnedCodeDescription}");
```

### 3. OrderClose (closing positions)

```csharp
var result = await service.CloseByTicket(ticket);
if (result.ReturnedCode == 10009)
    Console.WriteLine("[OK] Position closed");
else
    Console.WriteLine($"[X] Close failed: {result.ReturnedCodeDescription}");
```

> **Exceptions vs codes.** Depending on the server, a rejected trade may instead surface as an `ApiExceptionMT5`. That exception exposes the same trade code via `ex.MqlErrorTradeIntCode` and `ex.MqlErrorTradeDescription`. Handle **both** paths (see [Error handling](#error-handling)).

---

## 👁️ Why RetCode only in trading methods?

**Informational methods** (prices, symbols, balance) **do not** return a RetCode because:

- They don't go through the broker.
- They can't be "rejected" — either the data exists or there's a gRPC error.
- They read local terminal data.

**Trading methods** return a RetCode because:

- The request is sent to the broker via the trading server.
- The broker validates margin, symbol rules, trading hours, limits.
- The broker can reject for dozens of reasons — each with a unique code.

---

## Complete RetCode List

> The **MQL5 Name** column is the official `ENUM_TRADE_RETCODE` identifier (language-neutral). C# returns the raw `uint`; if you prefer named constants, see [Defining constants in C#](#defining-constants-in-c).

### Success Codes

| Code | MQL5 Name | Description | When Returned |
|------|-----------|-------------|---------------|
| **10009** | `TRADE_RETCODE_DONE` | **Request completed successfully** | Market order opened/closed |
| **10010** | `TRADE_RETCODE_DONE_PARTIAL` | **Partial execution** | Only part of volume executed |
| **10008** | `TRADE_RETCODE_PLACED` | **Pending order placed** | Limit/Stop order placed |

### Requote Codes (retry recommended)

| Code | MQL5 Name | Description | Action |
|------|-----------|-------------|--------|
| **10004** | `TRADE_RETCODE_REQUOTE` | **Requote** | Price changed, retry request |
| **10020** | `TRADE_RETCODE_PRICE_CHANGED` | **Price changed** | Similar to requote, retry |

### ⚠️ Request Validation Errors

| Code | MQL5 Name | Description | Common Cause |
|------|-----------|-------------|--------------|
| **10013** | `TRADE_RETCODE_INVALID_REQUEST` | **Invalid request** | Incorrect parameters |
| **10014** | `TRADE_RETCODE_INVALID_VOLUME` | **Invalid volume** | Volume < Min or > Max |
| **10015** | `TRADE_RETCODE_INVALID_PRICE` | **Invalid price** | Price doesn't match symbol rules |
| **10016** | `TRADE_RETCODE_INVALID_STOPS` | **Invalid stops** | SL/TP too close to price (check StopLevel) |
| **10022** | `TRADE_RETCODE_INVALID_EXPIRATION` | **Invalid expiration** | Expiration time incorrect |
| **10030** | `TRADE_RETCODE_INVALID_FILL` | **Invalid order filling type** | Fill type not allowed |
| **10035** | `TRADE_RETCODE_INVALID_ORDER` | **Invalid order type** | Order type prohibited for symbol |
| **10038** | `TRADE_RETCODE_INVALID_CLOSE_VOLUME` | **Invalid close volume** | Close volume exceeds position volume |

### 🚫 Trading Restrictions

| Code | MQL5 Name | Description | Reason |
|------|-----------|-------------|--------|
| **10017** | `TRADE_RETCODE_TRADE_DISABLED` | **Trading disabled** | Trading disabled for symbol |
| **10018** | `TRADE_RETCODE_MARKET_CLOSED` | **Market closed** | Outside trading hours |
| **10026** | `TRADE_RETCODE_SERVER_DISABLES_AT` | **Autotrading disabled by server** | Server disabled auto-trading |
| **10027** | `TRADE_RETCODE_CLIENT_DISABLES_AT` | **Autotrading disabled by client** | Terminal disabled auto-trading |
| **10032** | `TRADE_RETCODE_ONLY_REAL` | **Only real accounts** | Action unavailable on demo |
| **10042** | `TRADE_RETCODE_LONG_ONLY` | **Only long positions allowed** | Short positions prohibited |
| **10043** | `TRADE_RETCODE_SHORT_ONLY` | **Only short positions allowed** | Long positions prohibited |
| **10044** | `TRADE_RETCODE_CLOSE_ONLY` | **Only position closing allowed** | Opening new positions prohibited |
| **10045** | `TRADE_RETCODE_FIFO_CLOSE` | **Position close only by FIFO rule** | FIFO rule enforced |
| **10046** | `TRADE_RETCODE_HEDGE_PROHIBITED` | **Opposite positions prohibited** | Hedging disabled |

### 💰 Resource Limits

| Code | MQL5 Name | Description | Solution |
|------|-----------|-------------|----------|
| **10019** | `TRADE_RETCODE_NO_MONEY` | **Insufficient funds** | Check free margin |
| **10033** | `TRADE_RETCODE_LIMIT_ORDERS` | **Pending orders limit** | Close some pending orders |
| **10034** | `TRADE_RETCODE_LIMIT_VOLUME` | **Volume limit** | Reduce position volume |
| **10040** | `TRADE_RETCODE_LIMIT_POSITIONS` | **Positions limit** | Close some positions |

### 🔄 Technical Issues (Retryable)

| Code | MQL5 Name | Description | Retryable |
|------|-----------|-------------|-----------|
| **10011** | `TRADE_RETCODE_ERROR` | **Processing error** | No |
| **10012** | `TRADE_RETCODE_TIMEOUT` | **Request timeout** | Yes |
| **10021** | `TRADE_RETCODE_NO_QUOTES` | **No quotes** | Yes |
| **10024** | `TRADE_RETCODE_TOO_MANY_REQUESTS` | **Rate limiting** | Yes (with delay) |
| **10028** | `TRADE_RETCODE_LOCKED` | **Request locked for processing** | Yes |
| **10029** | `TRADE_RETCODE_FROZEN` | **Order or position frozen** | Yes |
| **10031** | `TRADE_RETCODE_NO_CONNECTION` | **No connection** | Yes |

### State Management

| Code | MQL5 Name | Description | Meaning |
|------|-----------|-------------|---------|
| **10023** | `TRADE_RETCODE_ORDER_CHANGED` | **Order state changed** | Order already modified/closed |
| **10025** | `TRADE_RETCODE_NO_CHANGES` | **No changes** | New parameters = current parameters |
| **10036** | `TRADE_RETCODE_POSITION_CLOSED` | **Position closed** | Position doesn't exist |
| **10039** | `TRADE_RETCODE_CLOSE_ORDER_EXIST` | **Close order exists** | Cannot close more than position volume |

### ❌ Rejection Codes

| Code | MQL5 Name | Description | Reason |
|------|-----------|-------------|--------|
| **10006** | `TRADE_RETCODE_REJECT` | **Request rejected** | Broker rejected |
| **10007** | `TRADE_RETCODE_CANCEL` | **Request canceled** | Canceled by trader |
| **10041** | `TRADE_RETCODE_REJECT_CANCEL` | **Pending order activation rejected** | Activation rejected and canceled |

---

## Reading the result in C#

The SDK does **not** ship a helper module or named-constant class for RetCodes — you read them directly off the reply:

```csharp
var result = await service.OrderSendAsync(request);

Console.WriteLine(result.ReturnedCode);            // e.g. 10018 (uint)
Console.WriteLine(result.ReturnedCodeDescription); // e.g. "Market is closed" (server-provided)
```

`ReturnedCodeDescription` already gives you the human-readable text, so there's no `get_retcode_message()`-style lookup to call.

### When to explicitly check RetCode

**CRITICAL:** check `ReturnedCode` after every trading operation:

```csharp
// ❌ WRONG — no exception does NOT mean the trade succeeded
var result = await service.OrderSendAsync(request);
Console.WriteLine($"Order placed: {result.Order}"); // Order may be 0 if rejected!

// ✅ CORRECT — always check ReturnedCode
var result = await service.OrderSendAsync(request);
if (result.ReturnedCode == 10009)
    Console.WriteLine($"✓ Order #{result.Order}");
else
    Console.WriteLine($"✗ {result.ReturnedCodeDescription} (code {result.ReturnedCode})");
```

Because: no exception means the **API call** succeeded (connection OK, terminal responded) — it does **not** mean the **trade** succeeded. The `ReturnedCode` is the actual trade result.

### Defining constants in C#

If you dislike magic numbers, drop a small helper into your project:

```csharp
public static class TradeRetcode
{
    public const uint Done = 10009, DonePartial = 10010, Placed = 10008;
    public const uint Requote = 10004, PriceChanged = 10020;
    public const uint NoMoney = 10019, MarketClosed = 10018, InvalidStops = 10016, InvalidVolume = 10014;

    public static bool IsSuccess(uint c)   => c == Done || c == Placed;
    public static bool IsRequote(uint c)   => c is Requote or PriceChanged;
    public static bool IsRetryable(uint c) => c is 10012 or 10021 or 10024 or 10028 or 10029 or 10031;
}

// Usage
if (TradeRetcode.IsSuccess(result.ReturnedCode)) { /* ... */ }
```

---

## Error handling

Two independent things can go wrong — handle **both**:

```csharp
using Grpc.Core;
using mt5_term_api;

try
{
    var result = await service.OrderSendAsync(request);

    if (result.ReturnedCode == 10009)
        Console.WriteLine($"[OK] Order #{result.Order}");
    else
        Console.WriteLine($"[X] Trade rejected: {result.ReturnedCodeDescription} (code {result.ReturnedCode})");
}
catch (ApiExceptionMT5 ex)   // business error surfaced as an exception (carries the trade code)
{
    Console.WriteLine($"[X] Trade error: {ex.MqlErrorTradeIntCode} — {ex.MqlErrorTradeDescription}");
}
catch (RpcException ex)      // transport/network error
{
    Console.WriteLine($"[X] gRPC error: {ex.Status}");
}
```

---

## Usage Examples

### Example 1 — Basic order placement with RetCode check

```csharp
using mt5_term_api;
using MetaRPC.CSharpMT5;

var account = new MT5Account(12345678, "your_password", "https://mt5.mrpc.pro:443", apiKey);
await account.ConnectByServerNameAsync("MetaQuotes-Demo", baseChartSymbol: "EURUSD", timeoutSeconds: 120);

try
{
    var service = new MT5Service(account);

    var request = new OrderSendRequest
    {
        Symbol    = "EURUSD",
        Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
        Volume    = 0.01,
        Price     = await service.GetAskAsync("EURUSD")
    };

    var result = await service.OrderSendAsync(request);
    if (result.ReturnedCode == 10009)
        Console.WriteLine($"[OK] Order #{result.Order}, volume {result.Volume}, price {result.Price:F5}");
    else
        Console.WriteLine($"[X] {result.ReturnedCodeDescription} (code {result.ReturnedCode})");
}
finally
{
    await account.DisconnectAsync();
}
```

### Example 2 — Handling common errors (switch)

```csharp
var result = await service.OrderSendAsync(request);

switch (result.ReturnedCode)
{
    case 10009:
        Console.WriteLine($"[OK] Order #{result.Order}");
        break;
    case 10019: // NO_MONEY
        Console.WriteLine("[WARN] Insufficient funds — reduce volume or add margin");
        break;
    case 10018: // MARKET_CLOSED
        Console.WriteLine("[WARN] Market closed — try during trading hours");
        break;
    case 10016: // INVALID_STOPS
        Console.WriteLine("[WARN] SL/TP too close — increase distance (check StopLevel)");
        break;
    case 10014: // INVALID_VOLUME
        var (min, max, step) = await service.GetVolumeLimitsAsync("EURUSD");
        Console.WriteLine($"[WARN] Invalid volume. Min {min}, Max {max}, Step {step}");
        break;
    case 10004: case 10020: // REQUOTE / PRICE_CHANGED
        Console.WriteLine("[WARN] Price changed — retry");
        break;
    default:
        Console.WriteLine($"[X] {result.ReturnedCodeDescription} (code {result.ReturnedCode})");
        break;
}
```

### Example 3 — Retry logic for transient errors

```csharp
async Task<OrderSendData> PlaceWithRetryAsync(MT5Service service, OrderSendRequest request, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        var result = await service.OrderSendAsync(request);

        if (result.ReturnedCode == 10009) return result;                 // success

        if (result.ReturnedCode is 10004 or 10020)                       // requote → retry now
        {
            Console.WriteLine($"Requote on attempt {attempt}, retrying...");
            await Task.Delay(100);
            request.Price = await service.GetAskAsync(request.Symbol);   // refresh price
            continue;
        }

        if (result.ReturnedCode is 10012 or 10021 or 10024 or 10028 or 10029 or 10031) // transient
        {
            Console.WriteLine($"Transient error on attempt {attempt}, waiting {attempt}s...");
            await Task.Delay(TimeSpan.FromSeconds(attempt));             // simple backoff
            continue;
        }

        throw new InvalidOperationException($"Permanent error: {result.ReturnedCodeDescription} ({result.ReturnedCode})");
    }
    throw new InvalidOperationException($"Failed after {maxRetries} attempts");
}
```

### Example 4 — Modifying SL/TP with validation

```csharp
var result = await service.ModifySlTpAsync(123456, sl: 1.0850, tp: 1.0950);

switch (result.ReturnedCode)
{
    case 10009: Console.WriteLine("[OK] SL/TP updated"); break;
    case 10025: Console.WriteLine("[WARN] No changes — new SL/TP equals current"); break;      // NO_CHANGES
    case 10036: Console.WriteLine("[WARN] Position already closed"); break;                     // POSITION_CLOSED
    case 10016: Console.WriteLine("[X] Invalid SL/TP distance (check StopLevel)"); break;       // INVALID_STOPS
    default:    Console.WriteLine($"[X] {result.ReturnedCodeDescription} ({result.ReturnedCode})"); break;
}
```

---

## Best Practices

### ✅ DO

1. **Always check `ReturnedCode`** after trading operations.
2. **Use `ReturnedCodeDescription`** for the message — it's already human-readable.
3. **Retry requotes** (10004, 10020) after refreshing the price.
4. **Check margin before trading** to avoid 10019:
   ```csharp
   double freeMargin = await service.GetFreeMarginAsync();
   double required   = await service.CalculateBuyMarginAsync("EURUSD", 0.01);
   if (freeMargin < required) Console.WriteLine("Insufficient margin!");
   ```

### ❌ DON'T

1. **Don't assume no exception = success** — check `ReturnedCode` too.
2. **Don't retry permanent errors** (insufficient margin, market closed).
3. **Don't swallow `ApiExceptionMT5` / `RpcException`** silently — a rejected trade may arrive as an exception carrying `MqlErrorTradeIntCode`.
4. **Don't hard-code bare numbers everywhere** — centralize them (see [Defining constants in C#](#defining-constants-in-c)).

---

## ℹ️ Quick Reference Table

| Category | RetCodes | Action |
|----------|----------|--------|
| **Success** | 10008, 10009, 10010 | Continue normally |
| **Requote** | 10004, 10020 | Retry immediately (refresh price) |
| **Temporary** | 10012, 10021, 10024, 10028, 10029, 10031 | Retry with delay |
| **Validation** | 10013–10016, 10022, 10030, 10035, 10038 | Fix parameters |
| **Restrictions** | 10017, 10018, 10026, 10027, 10032, 10042–10046 | Check trading conditions |
| **Limits** | 10019, 10033, 10034, 10040 | Reduce volume/positions |
| **State** | 10023, 10025, 10036, 10039 | Check current state |
| **Rejection** | 10006, 10007, 10041 | Check request |
| **Technical** | 10011 | Contact support |

---

## See Also

- [User Code Sandbox Guide](./USERCODE_SANDBOX_GUIDE.md) — where to write and test trading code
- [Enums Usage Reference](./ENUMS_USAGE_REFERENCE.md) — order-type and other enums used in requests
- [MT5Account — Trading Operations](../MT5Account/4.%20Trading_Operattons/Trading_Operations.Overview.md) — OrderSend/Modify/Close reference
