# CSharpMT5 Glossary

> Project-specific terms and concepts. This glossary covers the CSharpMT5 (C#/.NET) SDK architecture, components, and trading-automation terminology used throughout the codebase.

---

## 🏗️ Architectural Terms

### Three-Tier Architecture

Core design pattern with three abstraction levels:

- **Tier 1 (MT5Account):** Low-level gRPC communication with the MT5 terminal
- **Tier 2 (MT5Service):** Wrapper methods returning plain C# types
- **Tier 3 (MT5Sugar):** High-level convenience helpers with ready-made patterns

```
MT5Account (Low-Level)  →  Direct gRPC, proto Request/Reply
      ↓
MT5Service (Mid-Level)  →  Unwraps primitives + convenience shortcuts
      ↓
MT5Sugar  (High-Level)  →  Smart helpers, risk sizing, one-liners
```

---

### MT5Account

**🔹 Tier 1 — Low-Level API**

Base layer providing direct access to the MT5 terminal via the gRPC protocol.

**Key characteristics:**

- Raw gRPC calls to the MT5 terminal
- Works with protobuf Request/Reply objects
- Built-in connection resilience with automatic reconnection on recoverable errors
- Full control, maximum complexity
- **≈45 low-level methods**, each with an **async** and a **sync** variant, covering all MT5 operations
- Returns protobuf `...Data` payloads (`reply.Data`)
- Session management with a terminal-instance id (GUID)

**When to use:** advanced users who need granular control, custom integrations, or building custom wrappers.

**Location:** `package/Helpers/MT5Account.cs` (NuGet package `MetaRPC.MT5`)

**Documentation:** [MT5Account.Master.Overview.md](../MT5Account/MT5Account.Master.Overview.md)

---

### MT5Service

**🔸 Tier 2 — Wrappers API**

Middle layer providing simplified method signatures without proto complexity.

**Key characteristics:**

- Returns clean C# types (`double`, `long`, `string`, `bool`) where possible
- Unwraps protobuf `.Data` payloads (`SymbolInfoDoubleData.Value` → `double`)
- Simplified method names and convenience shortcuts (`GetBidAsync`, `GetBalanceAsync`, `BuyMarketAsync`)
- No auto-normalization (you control precision)
- Primary purpose: ergonomic middleware and the foundation MT5Sugar extends

**When to use:** building custom strategies that need more control than Sugar but less boilerplate than MT5Account.

**Location:** `MT5Service.cs`

**Documentation:** [MT5Service.Overview.md](../MT5Service/MT5Service.Overview.md)

---

### MT5Sugar

**🔺 Tier 3 — Convenience API**

High-level API for common trading operations, implemented as **extension methods on `MT5Service`**.

**Key characteristics:**

- Extension methods: `service.PlaceMarket(...)`, `service.BuyMarketByRisk(...)`
- Smart defaults and one-line operations
- Risk-based position sizing (`CalcVolumeForRiskAsync`)
- Friendly inputs (day windows, string timeframes, points-based pending orders)
- Auto‑selects symbols (`EnsureSelected`) before acting
- **Best starting point** for most use cases (~50+ helpers)

**Key features:**

- `PlaceMarket(symbol, volume, isBuy, sl, tp)` — one call to open a position
- `CalcVolumeForRiskAsync(symbol, stopPoints, riskMoney)` — volume sizing by risk
- `BuyMarketByRisk(symbol, stopPoints, riskMoney, tpPoints)` — open with risk sizing
- `GetBalanceAsync()`, `GetEquityAsync()` (from MT5Service) — account state
- `CloseAllPositions()` — emergency exit

**When to use:** rapid development, prototyping, simple strategies, quick tools.

**Location:** `MT5Sugar.cs`

**Documentation:** [MT5Sugar API Overview](../MT5Sugar/MT5Sugar.API_Overview.md)

---

## 🧩 C#-Specific Concepts

### Async / Await

Every network operation is asynchronous and returns a `Task` / `Task<T>`.

```csharp
using mt5_term_api;
using MetaRPC.CSharpMT5;

var account = new MT5Account(user, password, "https://mt5.mrpc.pro:443", apiKey: "TRIAL");
await account.ConnectByServerNameAsync("MetaQuotes-Demo", "EURUSD");

// Use 'await' for all async operations
double balance = (await account.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.AccountBalance)); // Data.Value already unwrapped in the SDK wrapper
var positions = await account.PositionsTotalAsync();

await account.DisconnectAsync();
```

**Sync variants.** Every async method has a blocking twin (e.g. `AccountSummary()` vs `AccountSummaryAsync()`). Prefer async in 99% of cases — see [Sync vs Async](./SYNC_VS_ASYNC.md).

---

### Protobuf Data Objects

The low-level layer returns generated protobuf message objects, accessed via `reply.Data`. There are no Python-style dataclasses; you work with the generated C# types directly, or let MT5Service unwrap them.

```csharp
// Low-level: returns a rich protobuf object
AccountSummaryData summary = await account.AccountSummaryAsync();
Console.WriteLine($"Balance: {summary.AccountBalance} {summary.AccountCurrency}");

// Mid-level: unwraps to a primitive
double balance = await service.GetBalanceAsync();
```

**Benefits of the generated types:** strong typing, IDE autocomplete, and exact field parity with the proto definitions.

---

### Streaming — `IAsyncEnumerable<T>` + `await foreach`

Streaming methods return `IAsyncEnumerable<T>`; consume them with `await foreach`. Stop by breaking out of the loop or cancelling the `CancellationToken`.

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

await foreach (var tickData in account.OnSymbolTickAsync(new[] { "EURUSD" }, cts.Token))
{
    var t = tickData.SymbolTick;
    Console.WriteLine($"Bid: {t.Bid}, Ask: {t.Ask}");

    if (someCondition) break; // stops the stream (auto cleanup)
}
```

**Critical rules:**

- Use `await foreach` to consume the stream.
- `break` ends the stream; the SDK cleans up the gRPC call.
- Pass a `CancellationToken` for time-based or external cancellation.

**Documentation:** [gRPC Stream Management](./GRPC_STREAM_MANAGEMENT.md)

---

### Connection & Disposal

```csharp
var account = new MT5Account(user, password, "https://mt5.mrpc.pro:443", apiKey: apiKey);

try
{
    await account.ConnectByServerNameAsync("MetaQuotes-Demo", "EURUSD");
    var service = new MT5Service(account);

    double balance = await service.GetBalanceAsync();
    var result = await service.PlaceMarket("EURUSD", 0.01, isBuy: true);
}
finally
{
    await account.DisconnectAsync(); // sends Disconnect and disposes the gRPC channel
}
```

`DisconnectAsync()` releases the persistent gRPC connection. Always call it (e.g. in `finally`) to avoid resource leaks.

---

## 📦 Technical Concepts

### Auto-Normalization

Automatic adjustment of trading parameters to broker requirements.

**What gets normalized:**

- **Volumes:** rounded to the broker's volume step (e.g. 0.01 lot)
- **Prices:** rounded to the symbol's tick size / digits
- **SL/TP:** adjusted to symbol precision

**Where:** the MT5 terminal normalizes server-side; both MT5Service and MT5Sugar benefit.

**Manual normalization if needed:**

```csharp
double step   = await service.GetVolumeStepAsync("EURUSD");
double volume = Math.Round(0.0234 / step) * step; // → 0.02 for step 0.01

long digits = await service.SymbolInfoIntegerAsync("EURUSD", SymbolInfoIntegerProperty.SymbolDigits);
double price = Math.Round(1.09876543, (int)digits); // → 1.09877 for a 5-digit broker
```

MT5Sugar also exposes `NormalizeVolumeAsync(symbol, volume)`.

---

### Risk-Based Volume Calculation

Calculate position size from money risk instead of a fixed lot.

**Formula:** `volume ≈ riskMoney / (stopPoints × pointValue)`

**Methods (MT5Sugar):**

```csharp
// Volume that risks ~$5 with a 50-point stop
double volume = await service.CalcVolumeForRiskAsync("EURUSD", stopPoints: 50, riskMoney: 5.0);

// Open with risk sizing in one call
var result = await service.BuyMarketByRisk("EURUSD", stopPoints: 50, riskMoney: 5.0, tpPoints: 100);
```

**Use case:** consistent risk management across trades.

---

### Points vs Pips

- **Point:** smallest price movement for a symbol (1 tick).
- **Pip:** traditional forex unit (0.0001 for most pairs).

**Relationship:**

- **5-digit brokers:** 1 pip = 10 points (EURUSD 1.10000 → 1.10010 = 1 pip)
- **3-digit brokers:** 1 pip = 1 point (USDJPY 110.00 → 110.01 = 1 pip)

The point-based MT5Sugar helpers (`BuyLimitPoints`, etc.) work in **points**.

```csharp
double point = await service.GetPointAsync("EURUSD"); // 0.00001 for a 5-digit broker
int pointsFor20Pips = 20 * 10;                        // 5-digit pairs
```

---

### Trailing Stop

Dynamic Stop Loss that follows price in the profit direction.

**How it works:** once a position is sufficiently in profit, move the SL toward price at a fixed distance, locking in gains.

```csharp
var opened = await service.OpenedOrdersAsync();
foreach (var pos in opened.PositionInfos)
{
    if (pos.Symbol != "EURUSD" || pos.Profit < 40.0) continue;

    double point = await service.GetPointAsync(pos.Symbol);
    double bid   = await service.GetBidAsync(pos.Symbol);
    double newSl = bid - 200 * point; // 20-pip trail on a 5-digit broker

    if (newSl > pos.StopLoss) // only move SL up for a BUY
        await service.ModifySlTpAsync(pos.Ticket, sl: newSl, tp: pos.TakeProfit);
}
```

**Use case:** locking in profit on trending markets.

---

### Breakeven

Moving the Stop Loss to the entry price to eliminate risk once the trade is in profit.

```csharp
var opened = await service.OpenedOrdersAsync();
foreach (var pos in opened.PositionInfos)
{
    if (pos.Profit >= 40.0) // threshold in account currency
        await service.ModifySlTpAsync(pos.Ticket, sl: pos.PriceOpen, tp: pos.TakeProfit);
}
```

---

### Hedging

Opening an opposite position to lock the current P/L level.

```csharp
var buy  = await service.PlaceMarket("EURUSD", 0.10, isBuy: true);
// ... price moves against you ...
var sell = await service.PlaceMarket("EURUSD", 0.10, isBuy: false); // net exposure ≈ 0
```

**WARNING:** not all brokers/regulations allow hedging (US brokers typically do not).

---

### Pending Order

An order that executes automatically when price reaches a level.

**Types:** BUY LIMIT (below price), SELL LIMIT (above), BUY STOP (above, breakout), SELL STOP (below, breakout).

**Methods (MT5Sugar, points-based):**

```csharp
// Buy Limit 50 points below Ask, SL 100 / TP 200 points
await service.BuyLimitPoints("EURUSD", 0.01, priceOffsetPoints: 50, slPoints: 100, tpPoints: 200);

// Sell Stop 50 points below Bid
await service.SellStopPoints("EURUSD", 0.01, priceOffsetPoints: 50, slPoints: 100, tpPoints: 200);
```

Or build the request yourself with `service.PlacePending(symbol, volume, ENUM_ORDER_TYPE.OrderTypeBuyLimit, price, sl, tp)`.

---

### Grid Trading

Strategy placing multiple pending orders at equal intervals above and below price. Works best in ranging markets.

```csharp
int step = 20, levels = 5; // 20-pip grid
for (int i = 1; i <= levels; i++)
{
    await service.BuyLimitPoints ("EURUSD", 0.01, priceOffsetPoints: i * step, slPoints: 150, tpPoints: 300);
    await service.SellLimitPoints("EURUSD", 0.01, priceOffsetPoints: i * step, slPoints: 150, tpPoints: 300);
}
```

**WARNING:** high volatility can trigger many orders, increasing exposure.

---

### Scalping

Fast trading with small profits (5–25 pips) and tight stops. Needs low spreads.

```csharp
await service.BuyMarketByRisk("EURUSD", stopPoints: 100, riskMoney: 5.0, tpPoints: 150);
```

---

### News Trading

Placing orders on both sides before a major news event, then cancelling the untriggered side.

```csharp
await service.BuyStopPoints ("EURUSD", 0.01, priceOffsetPoints: 300); // +30 pips
await service.SellStopPoints("EURUSD", 0.01, priceOffsetPoints: 300); // -30 pips
// ... after the move, cancel the pending side you don't want:
await service.CancelAll("EURUSD");
```

**WARNING:** slippage during news can trigger both orders.

---

### Volume Limits

Broker restrictions on position size.

```csharp
var (min, max, step) = await service.GetVolumeLimitsAsync("EURUSD");
double volume = Math.Clamp(requested, min, max);
volume = Math.Round(volume / step) * step; // snap to step
```

MT5Sugar exposes `NormalizeVolumeAsync(symbol, volume)` for the same effect.

---

## 🔌 gRPC and Protocol Terms

### gRPC

High-performance RPC framework over HTTP/2.

**In CSharpMT5:**

- `MT5Account` sends gRPC requests to the MT5 terminal via the gateway.
- Request/Reply pattern for unary calls; server-streaming for `On*` subscriptions.
- All calls are async.

```csharp
var account = new MT5Account(user, password, "https://mt5.mrpc.pro:443", apiKey: apiKey);
await account.ConnectByServerNameAsync("MetaQuotes-Demo", "EURUSD");
```

**Documentation:** [gRPC for .NET](https://learn.microsoft.com/aspnet/core/grpc/)

---

### Protobuf

Protocol Buffers — Google's language-neutral serialization format.

**In CSharpMT5:**

- All MT5Account methods use protobuf Request/Reply messages generated into the `mt5_term_api` namespace.
- Enums and messages are strongly typed C# classes.

```csharp
// Low-level returns the unwrapped primitive via the SDK wrapper
double balance = await account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountBalance);

// The raw reply carries reply.Data; most wrappers return .Data (or a field of it) for you
AccountSummaryData summary = await account.AccountSummaryAsync();
```

---

### ℹ️ Return Codes

Trade return codes indicating operation success/failure.

**Common codes:**

- **10009** = Success (TRADE_RETCODE_DONE)
- **10008** = Pending order placed (TRADE_RETCODE_PLACED)
- **10004** = Requote · **10006** = Rejected · **10013** = Invalid request
- **10014** = Invalid volume · **10015** = Invalid price · **10016** = Invalid stops
- **10018** = Market closed · **10019** = Not enough money · **10031** = No connection to trade server

```csharp
var result = await account.OrderSendAsync(request);

if (result.ReturnedCode == 10009)
    Console.WriteLine($"Trade done! Order: {result.Order}");
else
    Console.WriteLine($"Trade failed: {result.ReturnedCode} ({result.ReturnedCodeDescription})");
```

**Documentation:** [Return Codes Reference](./RETURN_CODES_REFERENCE.md)

---

### 📡 Streaming Pattern

Real-time data via `IAsyncEnumerable<T>`.

```csharp
// Ticks
await foreach (var td in account.OnSymbolTickAsync(new[] { "EURUSD" }, ct))
    Console.WriteLine($"Bid: {td.SymbolTick.Bid}, Ask: {td.SymbolTick.Ask}");

// Trade events
await foreach (var ev in account.OnTradeAsync(ct))
    Console.WriteLine($"Trade event: {ev.Type}");

// Position profit (poll every 500 ms)
await foreach (var p in account.OnPositionProfitAsync(intervalMs: 500, cancellationToken: ct))
    foreach (var pos in p.UpdatedPositions)
        Console.WriteLine($"Ticket {pos.Ticket}: {pos.Profit}");
```

**Documentation:** [gRPC Stream Management](./GRPC_STREAM_MANAGEMENT.md)

---

## 🏛️ Trading Terms

### Basic Trading Terms

Core concepts every trader should know before working with MT5.

| Term | Description | Example / Details |
|------|-------------|-------------------|
| **Bid** | Price at which the broker **buys** from you (you **sell** at Bid) | EURUSD Bid: 1.10000 |
| **Ask** | Price at which the broker **sells** to you (you **buy** at Ask) | EURUSD Ask: 1.10002 |
| **Spread** | Difference between Ask and Bid | Ask 1.10002 − Bid 1.10000 = 2 pips |
| **Long / Buy** | BUY position (profit when price rises) | Buy 1.10000, close 1.10050 = +50 pips |
| **Short / Sell** | SELL position (profit when price falls) | Sell 1.10000, close 1.09950 = +50 pips |
| **Position** | Open trade that can profit or lose | BUY 0.1 lot EURUSD at 1.10000 |
| **Order** | Command to open a position | Market (immediate) or Pending (at a price) |
| **Market Order** | Executes immediately at current price | BUY EURUSD at Ask |
| **Pending Order** | Triggers when price reaches a level | BUY LIMIT at 1.09950 |
| **Lot / Volume** | Position size (1 lot = 100,000 base units) | 0.01 lot = 1,000 units (micro) |
| **Stop Loss (SL)** | Auto loss-limiting exit level | SL 1.09950 limits loss to 50 pips |
| **Take Profit (TP)** | Auto profit-taking exit level | TP 1.10050 locks 50 pips |
| **Pip** | Standard unit of movement (0.0001 for most forex) | 1.10000 → 1.10001 = 1 pip |
| **Point** | Smallest movement (1 tick) | 5-digit broker: 1 pip = 10 points |
| **Leverage** | Borrowed capital (e.g. 1:100) | $1,000 margin controls $100,000 |
| **Margin** | Collateral locked for open positions | 1 lot EURUSD @ 1:100 ≈ $1,000 margin |
| **Free Margin** | Funds available for new positions | Balance − Margin |
| **Balance** | Funds excluding open positions | Deposit + realized P/L |
| **Equity** | Balance ± floating P/L | $10,000 + $500 floating = $10,500 |
| **Floating P/L** | Unrealized P/L of open positions | Position currently +$50 |
| **Margin Level** | Equity / Margin (%) | $10,500 / $1,000 = 1050% |
| **Margin Call** | Warning when Margin Level drops low | e.g. at 100% |
| **Stop Out** | Forced close when Margin Level critical | e.g. at 50% |
| **Drawdown** | Decline from peak balance/equity | Peak 11,000 → 10,000 = 9.09% |
| **Slippage** | Expected vs executed price difference | Requested 1.10000, filled 1.10003 |

**Critical for API usage:**

```csharp
// Prices
double bid = await service.GetBidAsync("EURUSD");
double ask = await service.GetAskAsync("EURUSD");

// Account state
double balance    = await service.GetBalanceAsync();
double equity     = await service.GetEquityAsync();
double freeMargin = await service.GetFreeMarginAsync();

// Open / modify / close
var buy = await service.PlaceMarket("EURUSD", 0.01, isBuy: true);
if (buy.Order > 0)
{
    await service.ModifySlTpAsync(buy.Order, sl: 1.09950, tp: 1.10050);
    await service.CloseByTicket(buy.Order);
}
```

---

### Risk Amount

The money you are willing to lose if the Stop Loss hits.

```csharp
// Volume that risks ~$5 with a 50-point stop, then open with it
double volume = await service.CalcVolumeForRiskAsync("EURUSD", stopPoints: 50, riskMoney: 5.0);
var result = await service.PlaceMarket("EURUSD", volume, isBuy: true);
```

---

## 🔑 Configuration Terms

### appsettings.json

The examples read connection settings from `Config/appsettings.json`.

```json
{
  "MT5": {
    "User": 5056776453,
    "Password": "...",
    "ServerName": "MetaQuotes-Demo",
    "GrpcServer": "https://mt5.mrpc.pro:443",
    "ApiKey": "mrpc_live_...",
    "ConnectTimeoutSeconds": 60
  }
}
```

Loaded via `ConnectionHelper.BuildConfiguration()` / `CreateAndConnectAccountAsync(config)`.

### Environment variable override

`MRPC_API_KEY` overrides the config API key at runtime (useful for not committing a real key):

```powershell
$env:MRPC_API_KEY = "TRIAL"   # or your account key
dotnet run lowlevel
```

**WARNING:** never commit real credentials. Keep live keys out of source control.

---

## 🔁 Cross-Component Terms

### Batch Operations

Acting on many positions/orders at once (MT5Sugar):

```csharp
int closed    = await service.CloseAllPositions();          // all positions
int closedEur = await service.CloseAllPositions("EURUSD");  // by symbol
int cancelled = await service.CancelAll();                  // all pending orders
```

**Use case:** emergency exits, end-of-session cleanup, strategy resets.

---

### 📝 History Queries

Retrieving past orders/positions for analysis (MT5Account / MT5Service).

```csharp
var from = DateTime.UtcNow.AddDays(-30);
var to   = DateTime.UtcNow;

// Orders (paged)
var orders = await service.OrderHistoryAsync(from, to,
    BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc, pageNumber: 0, itemsPerPage: 100);

// Full order history in one call
var all = await service.AllHistoryOrdersAsync(from, to);

// Closed positions
var positions = await service.PositionsHistoryAsync(
    AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeDesc, openFrom: from, openTo: to, page: 0, size: 100);
```

Friendly Sugar forms: `OrdersHistoryLast(days)`, `AllHistoryOrdersLast(days)`, `PositionsHistoryPaged(...)`.

---

### Educational Project

The CSharpMT5 examples, orchestrators and helpers are educational materials and API demonstrations, **not** production trading systems.

**Implications:**

- Study code and patterns ✅
- Modify for your needs ✅
- Test on demo accounts ✅
- Use as templates for your own strategies ✅
- Do **not** run as-is with real money ❌
- Do **not** expect production-grade risk management or exhaustive error handling ❌

---

## 🧱 Enums

The SDK uses generated protobuf enums instead of magic numbers. They live in the `mt5_term_api` namespace; the C# member is PascalCase (its proto `OriginalName` is the SCREAMING_SNAKE form).

```csharp
using mt5_term_api;

double bid = await service.SymbolInfoDoubleAsync("EURUSD", SymbolInfoDoubleProperty.SymbolBid);
double bal = await account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountBalance);

var req = new OrderSendRequest
{
    Symbol = "EURUSD",
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy
};
```

**Documentation:** [Enums Usage Reference](./ENUMS_USAGE_REFERENCE.md)

---

## See Also

- **[MT5Account Master Overview](../MT5Account/MT5Account.Master.Overview.md)** — complete low-level API reference
- **[MT5Service API Overview](../MT5Service/MT5Service.Overview.md)** — mid-level wrappers
- **[MT5Sugar API Reference](../MT5Sugar/MT5Sugar.API_Overview.md)** — high-level Sugar API
- **[gRPC Stream Management](./GRPC_STREAM_MANAGEMENT.md)** — streaming subscriptions
- **[Return Codes Reference](./RETURN_CODES_REFERENCE.md)** — complete return codes
- **[Enums Usage Reference](./ENUMS_USAGE_REFERENCE.md)** — all available enums
- **[Sync vs Async](./SYNC_VS_ASYNC.md)** — when to use each
- **[Your First Project](./Your_First_Project.md)** — setup and quick start

---

"Trade safely, code cleanly, and may your async tasks always complete successfully."
