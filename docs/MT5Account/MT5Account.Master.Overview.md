# MT5Account - Master Overview

> One page to **orient fast**: what lives where, how to choose the right API, and jump links to every **overview** and **method spec** in this docs set.

---

## 🚦 Start here - Section Overviews

* **[Account\_Information - Overview](./1.%20Account_information/Account_Information.Overview.md)**
  Account balance/equity/margin/leverage, complete snapshot or single properties.

* **[Symbol\_Information - Overview](./2.%20Symbol_information/Symbol_Information.Overview.md)**
  Quotes, symbol properties, trading rules, tick values, Market Watch management.

* **[Position\_Orders\_Information - Overview](./3.%20Position_Orders_Information/Position_Orders_Information.Overview.md)**
  Open positions, pending orders, historical deals, order history.

* **[Trading\_Operations - Overview](./4.%20Trading_Operattons/Trading_Operations.Overview.md)**
  Order execution, position management, margin calculations, trade validation.

* **[Market\_Depth\_DOM - Overview](./5.%20Market_Depth(DOM)/Market_Depth.Overview.md)**
  Level II quotes, order book data, market depth subscription.

* **[Additional\_Methods - Overview](./6.%20Addittional_Methods/Additional_Methods.Overview.md)**
  Advanced symbol info, trading sessions, margin rates, batch operations.

* **[Streaming\_Methods - Overview](./7.%20Streaming_Methods/Streaming_Methods.Overview.md)**
  Real-time streams: ticks, trades, profit updates, transaction log.
  📖 **[Complete Streaming Guide](../GRPC_STREAM_MANAGEMENT.md)** - How to properly work with subscriptions (patterns, best practices, memory management)

---

## 🧭 How to pick an API

| If you need…                   | Go to…                      | Typical calls                                                                 |
| ------------------------------ | --------------------------- | ----------------------------------------------------------------------------- |
| Account snapshot               | Account\_Information        | `AccountSummaryAsync`, `AccountInfoDoubleAsync`, `AccountInfoIntegerAsync`    |
| Quotes & symbol properties     | Symbol\_Information         | `SymbolInfoTickAsync`, `SymbolInfoDoubleAsync`, `SymbolsTotalAsync`          |
| Current positions & orders     | Position\_Orders\_Information | `PositionsTotalAsync`, `OpenedOrdersAsync`, `OpenedOrdersTicketsAsync`      |
| Historical trades              | Position\_Orders\_Information | `OrderHistoryAsync`, `PositionsHistoryAsync`                                |
| Level II / Order book          | Market\_Depth\_DOM          | `MarketBookAddAsync`, `MarketBookGetAsync`, `MarketBookReleaseAsync`         |
| Trading operations             | Trading\_Operations         | `OrderSendAsync`, `OrderModifyAsync`, `OrderCloseAsync`                      |
| Pre-trade calculations         | Trading\_Operations         | `OrderCalcMarginAsync`, `OrderCheckAsync`                                    |
| Real-time updates              | Streaming\_Methods          | `OnSymbolTickAsync`, `OnTradeAsync`, `OnPositionProfitAsync`                 |

---

## 🔌 Usage pattern (gRPC protocol)

Every method is available in **two versions**: **Async** (recommended) and **Sync** (rare scenarios).

### Async Version (Recommended - 99% of cases)
```csharp
var summary = await account.AccountSummaryAsync();
var tick = await account.SymbolInfoTickAsync("EURUSD");
```

### Sync Version (Legacy/specific scenarios only)
```csharp
var summary = account.AccountSummary();
var tick = account.SymbolInfoTick("EURUSD");
```

**📖 Read:** **[Sync vs Async - When to Use What](./Sync_vs_Async.md)** - Detailed guide with use cases and performance comparisons.

---

Every method follows the same shape:

* **Proto Service/Method:** `Service.Method(Request) → Reply`
* **C# wrapper:** `await mt5Account.MethodAsync(...)` or `mt5Account.Method()`
* **Reply structure:** `Reply.Data` payload (proto-generated objects)
* **Return codes:** `10009` = success; other codes = check error message

Timestamps = **UTC** (`google.protobuf.Timestamp`). For streaming subscriptions, use **`IAsyncEnumerable<T>`** with `await foreach` pattern.

---

# 📚 Full Index · All Method Specs

---

## 📄 Account Information

* **Overview:** [Account\_Information.Overview.md](./1.%20Account_information/Account_Information.Overview.md)

### Complete Snapshot

* [AccountSummary.md](./1.%20Account_information/AccountSummary.md) - All account info at once (balance, equity, margin, etc.)

### Individual Properties

* [AccountInfoDouble.md](./1.%20Account_information/AccountInfoDouble.md) - Single double value (balance, equity, margin, profit, etc.)
* [AccountInfoInteger.md](./1.%20Account_information/AccountInfoInteger.md) - Single integer value (login, leverage, limit orders, etc.)
* [AccountInfoString.md](./1.%20Account_information/AccountInfoString.md) - Single string value (name, server, currency, company)

---

## 📊 Symbol Information

* **Overview:** [Symbol\_Information.Overview.md](./2.%20Symbol_information/Symbol_Information.Overview.md)

### Current Quotes

* [SymbolInfoTick.md](./2.%20Symbol_information/SymbolInfoTick.md) - Current quote for symbol (bid, ask, last, volume, time)

### Symbol Inventory & Management

* [SymbolsTotal.md](./2.%20Symbol_information/SymbolsTotal.md) - Count of available symbols
* [SymbolName.md](./2.%20Symbol_information/SymbolName.md) - Get symbol name by index
* [SymbolSelect.md](./2.%20Symbol_information/SymbolSelect.md) - Enable/disable symbol in Market Watch
* [SymbolExist.md](./2.%20Symbol_information/SymbolExist.md) - Check if symbol exists
* [SymbolIsSynchronized.md](./2.%20Symbol_information/SymbolIsSynchronized.md) - Check symbol data sync status

### Symbol Properties

* [SymbolInfoDouble.md](./2.%20Symbol_information/SymbolInfoDouble.md) - Single double property (bid, ask, point, volume min/max, etc.)
* [SymbolInfoInteger.md](./2.%20Symbol_information/SymbolInfoInteger.md) - Single integer property (digits, spread, stops level, etc.)
* [SymbolInfoString.md](./2.%20Symbol_information/SymbolInfoString.md) - Single string property (description, currency, path)

---

## 📦 Positions & Orders

* **Overview:** [Position\_Orders\_Information.Overview.md](./3.%20Position_Orders_Information/Position_Orders_Information.Overview.md)

### Current State

* [PositionsTotal.md](./3.%20Position_Orders_Information/PositionsTotal.md) - Count of open positions
* [OpenedOrders.md](./3.%20Position_Orders_Information/OpenedOrders.md) - Full details of all open positions and pending orders
* [OpenedOrdersTickets.md](./3.%20Position_Orders_Information/OpenedOrdersTickets.md) - Ticket numbers only (lightweight)

### Historical Data

* [OrderHistory.md](./3.%20Position_Orders_Information/OrderHistory.md) - Historical orders within time range (with pagination)
* [PositionsHistory.md](./3.%20Position_Orders_Information/PositionsHistory.md) - Closed positions within time range (with pagination)

---

## 🛠 Trading Actions

* **Overview:** [Trading\_Operations.Overview.md](./4.%20Trading_Operattons/Trading_Operations.Overview.md)

### Order Execution & Management

* [OrderSend.md](./4.%20Trading_Operattons/OrderSend.md) - Place market or pending orders
* [OrderModify.md](./4.%20Trading_Operattons/OrderModify.md) - Modify SL/TP or order parameters
* [OrderClose.md](./4.%20Trading_Operattons/OrderClose.md) - Close positions (full or partial)

### Pre-Trade Calculations

* [OrderCalcMargin.md](./4.%20Trading_Operattons/OrderCalcMargin.md) - Calculate margin required for trade
* [OrderCheck.md](./4.%20Trading_Operattons/OrderCheck.md) - Validate trade request before execution

---

## 📈 Market Depth (DOM)

* **Overview:** [Market\_Depth\_DOM.Overview.md](./5.%20Market_Depth(DOM)/Market_Depth.Overview.md)

### Level II Quotes

* [MarketBookAdd.md](./5.%20Market_Depth(DOM)/MarketBookAdd.md) - Subscribe to Market Depth for symbol
* [MarketBookGet.md](./5.%20Market_Depth(DOM)/MarketBookGet.md) - Get current order book data
* [MarketBookRelease.md](./5.%20Market_Depth(DOM)/MarketBookRelease.md) - Unsubscribe from Market Depth

---

## 🔧 Additional Methods

* **Overview:** [Additional\_Methods.Overview.md](./6.%20Addittional_Methods/Additional_Methods.Overview.md)

### Advanced Symbol Information

* [SymbolInfoMarginRate.md](./6.%20Addittional_Methods/SymbolInfoMarginRate.md) - Margin rates for symbol and order type
* [SymbolInfoSessionQuote.md](./6.%20Addittional_Methods/SymbolInfoSessionQuote.md) - Quote session times
* [SymbolInfoSessionTrade.md](./6.%20Addittional_Methods/SymbolInfoSessionTrade.md) - Trade session times
* [SymbolParamsMany.md](./6.%20Addittional_Methods/SymbolParamsMany.md) - Detailed parameters for multiple symbols (112 fields!)

### Trading Calculations

* [TickValueWithSize.md](./6.%20Addittional_Methods/TickValueWithSize.md) - Batch tick values for multiple symbols

---

## 📡 Subscriptions (Streaming)

* **Overview:** [Streaming\_Methods.Overview.md](./7.%20Streaming_Methods/Streaming_Methods.Overview.md)

### Real-Time Price Updates

* [SubscribeToTicks.md](./7.%20Streaming_Methods/SubscribeToTicks.md) - Real-time tick stream for symbols

### Trading Events

* [OnTrade.md](./7.%20Streaming_Methods/OnTrade.md) - Position/order changes (opened, closed, modified)
* [SubscribeToTradeTransaction.md](./7.%20Streaming_Methods/SubscribeToTradeTransaction.md) - Detailed transaction log (complete audit trail)

### Position Monitoring

* [SubscribeToPositionProfit.md](./7.%20Streaming_Methods/SubscribeToPositionProfit.md) - Periodic profit/loss updates
* [OnPositionsAndPendingOrdersTickets.md](./7.%20Streaming_Methods/OnPositionsAndPendingOrdersTickets.md) - Periodic ticket lists (lightweight)

---

## 🎯 Quick Navigation by Use Case

| I want to... | Use this method |
|-------------|-----------------|
| **ACCOUNT INFORMATION** |
| Get complete account snapshot | [AccountSummaryAsync](./1.%20Account_information/AccountSummary.md) |
| Get account balance | [AccountInfoDoubleAsync](./1.%20Account_information/AccountInfoDouble.md) (BALANCE) |
| Get account equity | [AccountInfoDoubleAsync](./1.%20Account_information/AccountInfoDouble.md) (EQUITY) |
| Get account leverage | [AccountInfoIntegerAsync](./1.%20Account_information/AccountInfoInteger.md) (LEVERAGE) |
| Get account currency | [AccountInfoStringAsync](./1.%20Account_information/AccountInfoString.md) (CURRENCY) |
| **SYMBOL INFORMATION** |
| Get current price for symbol | [SymbolInfoTickAsync](./2.%20Symbol_information/SymbolInfoTick.md) |
| List all available symbols | [SymbolsTotalAsync](./2.%20Symbol_information/SymbolsTotal.md) + [SymbolNameAsync](./2.%20Symbol_information/SymbolName.md) |
| Add symbol to Market Watch | [SymbolSelectAsync](./2.%20Symbol_information/SymbolSelect.md) (true) |
| Get symbol digits (decimal places) | [SymbolInfoIntegerAsync](./2.%20Symbol_information/SymbolInfoInteger.md) (DIGITS) |
| Get point size for symbol | [SymbolInfoDoubleAsync](./2.%20Symbol_information/SymbolInfoDouble.md) (POINT) |
| Get symbol volume limits | [SymbolInfoDoubleAsync](./2.%20Symbol_information/SymbolInfoDouble.md) (VOLUME_MIN/MAX/STEP) |
| Get tick values for symbols | [TickValueWithSizeAsync](./6.%20Addittional_Methods/TickValueWithSize.md) |
| **POSITIONS & ORDERS** |
| Count open positions | [PositionsTotalAsync](./3.%20Position_Orders_Information/PositionsTotal.md) |
| Get all open positions (full details) | [OpenedOrdersAsync](./3.%20Position_Orders_Information/OpenedOrders.md) |
| Get position ticket numbers only | [OpenedOrdersTicketsAsync](./3.%20Position_Orders_Information/OpenedOrdersTickets.md) |
| Get historical orders | [OrderHistoryAsync](./3.%20Position_Orders_Information/OrderHistory.md) |
| Get historical deals/trades | [PositionsHistoryAsync](./3.%20Position_Orders_Information/PositionsHistory.md) |
| **MARKET DEPTH** |
| Subscribe to Level II quotes | [MarketBookAddAsync](./5.%20Market_Depth(DOM)/MarketBookAdd.md) |
| Get order book data | [MarketBookGetAsync](./5.%20Market_Depth(DOM)/MarketBookGet.md) |
| Unsubscribe from Level II | [MarketBookReleaseAsync](./5.%20Market_Depth(DOM)/MarketBookRelease.md) |
| **TRADING OPERATIONS** |
| Open market BUY position | [OrderSendAsync](./4.%20Trading_Operattons/OrderSend.md) (type=BUY) |
| Open market SELL position | [OrderSendAsync](./4.%20Trading_Operattons/OrderSend.md) (type=SELL) |
| Place BUY LIMIT order | [OrderSendAsync](./4.%20Trading_Operattons/OrderSend.md) (type=BUY_LIMIT) |
| Place SELL LIMIT order | [OrderSendAsync](./4.%20Trading_Operattons/OrderSend.md) (type=SELL_LIMIT) |
| Place BUY STOP order | [OrderSendAsync](./4.%20Trading_Operattons/OrderSend.md) (type=BUY_STOP) |
| Place SELL STOP order | [OrderSendAsync](./4.%20Trading_Operattons/OrderSend.md) (type=SELL_STOP) |
| Modify SL/TP of position | [OrderModifyAsync](./4.%20Trading_Operattons/OrderModify.md) |
| Close a position | [OrderCloseAsync](./4.%20Trading_Operattons/OrderClose.md) |
| Calculate margin before trade | [OrderCalcMarginAsync](./4.%20Trading_Operattons/OrderCalcMargin.md) |
| Validate trade before execution | [OrderCheckAsync](./4.%20Trading_Operattons/OrderCheck.md) |
| **REAL-TIME SUBSCRIPTIONS** |
| Stream live prices | [OnSymbolTickAsync](./7.%20Streaming_Methods/SubscribeToTicks.md) |
| Monitor trade events | [OnTradeAsync](./7.%20Streaming_Methods/OnTrade.md) |
| Track profit changes | [OnPositionProfitAsync](./7.%20Streaming_Methods/SubscribeToPositionProfit.md) |
| Monitor ticket changes | [OnPositionsAndPendingOrdersTicketsAsync](./7.%20Streaming_Methods/OnPositionsAndPendingOrdersTickets.md) |
| Detailed transaction log | [OnTradeTransactionAsync](./7.%20Streaming_Methods/SubscribeToTradeTransaction.md) |

---

## 🏗️ API Architecture

### Layer 1: MT5Account (Low-Level)

**What:** Direct proto/gRPC communication with MT5 terminal.

**When to use:**
- Need full control over protocol
- Building custom wrappers
- Proto-level integration required

**Characteristics:**
- Works with proto Request/Response objects
- Raw gRPC method calls
- Complete access to all MT5 functions
- Highest complexity

**Location:** `MT5Account.cs`

**Documentation:** This folder (you are here!)

---

### Layer 2: MT5Service

**What:** Simplified wrapper methods without proto complexity.

**When to use:**
- Want simplified API but not auto-normalization
- Building custom convenience layers
- Need direct data returns

**Characteristics:**
- Simple method signatures
- Type conversions (proto → C# primitives)
- No proto objects in return values
- No auto-normalization

**Location:** `MT5Service.cs`

**Documentation:** [MT5Service.Overview.md](../MT5Service/MT5Service.Overview.md)

---

### Layer 3: MT5Sugar

**What:** High-level convenience API with ~50 smart methods.

**When to use:**
- Most trading scenarios (95% of cases)
- Want auto-normalization
- Need risk management helpers
- Building strategies quickly

**Characteristics:**
- Auto-normalization of volumes/prices
- Risk-based position sizing
- Batch operations
- Smart helpers

**Location:** `MT5Sugar.cs`

**Documentation:** [MT5Sugar.API_Overview.md](../MT5Sugar/MT5Sugar.API_Overview.md)

---

## 🎓 Learning Path

**Recommended sequence:** Start from foundation (MT5Account) → Build up to convenience layers (MT5Service → MT5Sugar)

### Step 1: Master the Foundation (MT5Account)

**Why first:** MT5Account is the foundation - everything else is built on top of it. Understanding the protocol level gives you complete control.

```
1. Read: This documentation folder (docs/MT5Account/)
2. Study: Proto definitions and gRPC communication
3. Understand: Request/Response patterns
4. Learn: Return codes and error handling
5. Practice: Low-level method calls
```

**Goal:** Deep understanding of MT5 protocol and terminal communication.

**Documentation:** [MT5Account.Master.Overview.md](./MT5Account.Master.Overview.md) (you are here!)

---

### Step 2: Understand Wrappers (MT5Service)

**Why second:** Once you know the foundation, see how MT5Service simplifies it by wrapping proto objects.

```
1. Study: How MT5Service wraps MT5Account methods
2. Compare: Wrapper vs low-level implementations
3. Learn: Type conversions (proto → C# primitives)
4. Practice: Simplified method calls without proto objects
```

**Goal:** Learn to build clean API wrappers on top of complex protocols.

**Documentation:** [MT5Service.Overview.md](../MT5Service/MT5Service.Overview.md)

---

### Step 3: Use Convenience Layer (MT5Sugar)

**Why last:** With foundation + wrappers understood, appreciate how MT5Sugar adds auto-normalization and smart helpers.

```
1. Study: MT5Sugar convenience methods (~50 methods)
2. Learn: Auto-normalization, risk management, batch operations
3. Use: High-level methods for rapid strategy development
4. Build: Trading strategies using Sugar API
```

**Goal:** Rapid strategy development with production-ready convenience methods.

**Documentation:** [MT5Sugar.API_Overview.md](../MT5Sugar/MT5Sugar.API_Overview.md)

---

**Summary:** MT5Account (foundation) → MT5Service (wrappers) → MT5Sugar (convenience)

---

## 💡 Key Concepts

### Proto Return Codes

* **10009** = Success / DONE
* **10004** = Requote
* **10006** = Request rejected
* **10013** = Invalid request
* **10014** = Invalid volume
* **10015** = Invalid price
* **10016** = Invalid stops
* **10018** = Market closed
* **10019** = Not enough money
* **10031** = No connection with trade server

Always check `ReturnedCode` field in trading operations.

**Complete Reference:** [ReturnCodes_Reference_EN.md](../ReturnCodes_Reference_EN.md)

---

### C# Async Patterns

**Request/Reply methods:**
```csharp
var result = await account.MethodAsync(parameters, deadline, cancellationToken);
```

**Streaming methods:**
```csharp
await foreach (var data in account.OnMethodAsync(parameters, cancellationToken))
{
    // Process streaming data
}
```

**Detailed Guide:** [Sync_vs_Async.md](../Sync_vs_Async.md) - When to use async vs sync, performance comparisons, best practices

---

## ⚠️ Important Notes

* **Demo account first:** Always test on demo before live trading.
* **Check return codes:** Every trading operation returns status code.
* **Validate parameters:** Use `OrderCheckAsync()` before `OrderSendAsync()`.
* **Handle errors:** Network/protocol errors can occur.
* **Thread safety:** Streams execute on background threads.
* **Resource cleanup:** Unsubscribe from streams when done (use CancellationToken).
* **UTC timestamps:** All times are in UTC, not local time.
* **Broker limitations:** Not all brokers support all features (DOM, hedging, etc.).
* **Async/await:** Always use `await` with async methods.
* **CancellationToken:** Always provide cancellation tokens for long-running operations.

---

## 📋 Best Practices

### Error Handling
```csharp
try
{
    var result = await account.OrderSendAsync(request);
    if (result.ReturnedCode == 10009)
    {
        Console.WriteLine("✅ Success");
    }
    else
    {
        Console.WriteLine($"❌ Error: {result.Comment}");
    }
}
catch (RpcException ex)
{
    Console.WriteLine($"gRPC Error: {ex.Status}");
}
```

### Streaming with Cancellation
```csharp
var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
try
{
    await foreach (var tick in account.OnSymbolTickAsync(symbols, cts.Token))
    {
        // Process tick
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Stream stopped");
}
```

---

"Trade safe, code clean, and may your async operations always complete successfully."
