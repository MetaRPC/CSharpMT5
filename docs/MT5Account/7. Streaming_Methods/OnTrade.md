# ✅ Subscribe to Trade Events (`OnTradeAsync`)

> **Stream:** Real-time trade execution and closure events on **MT5**. Continuously sends notifications when orders are filled, positions opened/closed, or orders modified.

**API Information:**

* **SDK wrapper:** `MT5Service.OnTradeAsync(...)` (from class `MT5Service`)
* **gRPC service:** `mt5_term_api.SubscriptionService`
* **Proto definition:** `OnTrade` (defined in `mt5-term-api-subscriptions.proto`)

### RPC

* **Service:** `mt5_term_api.SubscriptionService`
* **Method:** `OnTrade(OnTradeRequest) → stream OnTradeReply`
* **Low‑level client (generated):** `SubscriptionService.SubscriptionServiceClient.OnTrade(request, headers, deadline, cancellationToken)`
* **SDK wrapper:**

```csharp
namespace mt5_term_api
{
    public class MT5Service
    {
        public async IAsyncEnumerable<OnTradeData> OnTradeAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`OnTradeRequest {}` (empty)

**Reply message (stream):**

`OnTradeReply { data: OnTradeData }`

---

## 🔽 Input

| Parameter            | Type                  | Description                                         |
| -------------------- | --------------------- | --------------------------------------------------- |
| `cancellationToken`  | `CancellationToken`   | Token to stop the stream                            |

---

## ⬆️ Output — `OnTradeData` (stream)

| Field                    | Type                                | Description                              |
| ------------------------ | ----------------------------------- | ---------------------------------------- |
| `Type`                   | `MT5_SUB_ENUM_EVENT_GROUP_TYPE`     | Event group type                         |
| `EventData`              | `OnTradeEventData`                  | Trade event details                      |
| `AccountInfo`            | `OnEventAccountInfo`                | Current account state                    |
| `TerminalInstanceGuidId` | `string`                            | Terminal instance ID                     |

### `OnTradeEventData` — Complete trade event information

| Field                      | Type                                     | Description                                    |
| -------------------------- | ---------------------------------------- | ---------------------------------------------- |
| `NewOrders`                | `List<OnTradeOrderInfo>`                 | Newly placed orders                            |
| `DisappearedOrders`        | `List<OnTradeOrderInfo>`                 | Orders that were removed/cancelled             |
| `StateChangedOrders`       | `List<OnTradeOrderStateChange>`          | Orders with state changes                      |
| `NewHistoryOrders`         | `List<OnTradeHistoryOrderInfo>`          | New orders in history (executed/cancelled)     |
| `DisappearedHistoryOrders` | `List<OnTradeHistoryOrderInfo>`          | History orders that disappeared                |
| `UpdatedHistoryOrders`     | `List<OnTradeHistoryOrderUpdate>`        | History orders that were updated               |
| `NewHistoryDeals`          | `List<OnTradeHistoryDealInfo>`           | New deals in history                           |
| `DisappearedHistoryDeals`  | `List<OnTradeHistoryDealInfo>`           | History deals that disappeared                 |
| `UpdatedHistoryDeals`      | `List<OnTradeHistoryDealUpdate>`         | History deals that were updated                |
| `NewPositions`             | `List<OnTradePositionInfo>`              | Newly opened positions                         |
| `DisappearedPositions`     | `List<OnTradePositionInfo>`              | Positions that were closed                     |
| `UpdatedPositions`         | `List<OnTradePositionUpdate>`            | Positions that were modified                   |

### `OnTradePositionInfo` — Position information

| Field              | Type                          | Description                        |
| ------------------ | ----------------------------- | ---------------------------------- |
| `Index`            | `int32`                       | Position index                     |
| `Ticket`           | `int64`                       | Position ticket                    |
| `Type`             | `SUB_ENUM_POSITION_TYPE`      | Position type (Buy/Sell)           |
| `PositionTime`     | `Timestamp`                   | Position open time                 |
| `LastUpdateTime`   | `Timestamp`                   | Last update time                   |
| `PriceOpen`        | `double`                      | Open price                         |
| `Profit`           | `double`                      | Current profit                     |
| `Sl`               | `double`                      | Stop Loss                          |
| `Tp`               | `double`                      | Take Profit                        |
| `Volume`           | `double`                      | Position volume                    |
| `Swap`             | `double`                      | Swap charges                       |
| `Comment`          | `string`                      | Position comment                   |
| `Symbol`           | `string`                      | Symbol name                        |
| `Magic`            | `int64`                       | Magic number                       |
| `PriceCurrent`     | `double`                      | Current price                      |
| `AccountLogin`     | `int64`                       | Account login                      |
| `Reason`           | `SUB_ENUM_POSITION_REASON`    | Position open reason               |
| `FromPendingOrder` | `bool`                        | Opened from pending order          |

### `OnTradeOrderInfo` — Order information

| Field               | Type                              | Description                         |
| ------------------- | --------------------------------- | ----------------------------------- |
| `Index`             | `int32`                           | Order index                         |
| `Ticket`            | `int64`                           | Order ticket                        |
| `State`             | `SUB_ENUM_ORDER_STATE`            | Order state                         |
| `SetupTimeMsc`      | `int64`                           | Setup time (milliseconds)           |
| `StopLoss`          | `double`                          | Stop Loss                           |
| `TakeProfit`        | `double`                          | Take Profit                         |
| `StopLimit`         | `double`                          | Stop Limit price                    |
| `PriceCurrent`      | `double`                          | Current price                       |
| `TimeExpiration`    | `Timestamp`                       | Expiration time                     |
| `TimeType`          | `SUB_ENUM_ORDER_TYPE_TIME`        | Time type (GTC, Day, etc)           |
| `Comment`           | `string`                          | Order comment                       |
| `Symbol`            | `string`                          | Symbol name                         |
| `Magic`             | `int64`                           | Magic number                        |
| `PriceOpen`         | `double`                          | Open price                          |
| `SetupTime`         | `Timestamp`                       | Setup time                          |
| `TimeExpirationSeconds` | `int64`                       | Expiration (seconds)                |
| `VolumeCurrent`     | `double`                          | Current volume                      |
| `VolumeInitial`     | `double`                          | Initial volume                      |
| `AccountLogin`      | `int64`                           | Account login                       |
| `OrderType`         | `SUB_ENUM_ORDER_TYPE`             | Order type                          |
| `OrderTypeFilling`  | `SUB_ENUM_ORDER_TYPE_FILLING`     | Filling mode                        |
| `OrderReason`       | `SUB_ENUM_ORDER_REASON`           | Order reason                        |
| `PositionId`        | `int64`                           | Position ID                         |
| `PositionById`      | `int64`                           | Opposite position ID                |

### `OnTradeHistoryDealInfo` — History deal information

| Field            | Type                          | Description                        |
| ---------------- | ----------------------------- | ---------------------------------- |
| `Index`          | `int32`                       | Deal index                         |
| `Ticket`         | `uint64`                      | Deal ticket                        |
| `OrderTicket`    | `int64`                       | Order ticket                       |
| `Type`           | `SUB_ENUM_DEAL_TYPE`          | Deal type                          |
| `DealTime`       | `Timestamp`                   | Deal execution time                |
| `Entry`          | `SUB_ENUM_DEAL_ENTRY`         | Deal entry (In/Out/InOut/OutBy)    |
| `DealPositionId` | `int64`                       | Position ID                        |
| `Commission`     | `double`                      | Commission                         |
| `Fee`            | `double`                      | Fee                                |
| `Price`          | `double`                      | Execution price                    |
| `Profit`         | `double`                      | Profit                             |
| `Sl`             | `double`                      | Stop Loss                          |
| `Tp`             | `double`                      | Take Profit                        |
| `Volume`         | `double`                      | Deal volume                        |
| `Comment`        | `string`                      | Deal comment                       |
| `Symbol`         | `string`                      | Symbol name                        |
| `Swap`           | `double`                      | Swap                               |
| `Reason`         | `SUB_ENUM_DEAL_REASON`        | Deal reason                        |
| `Magic`          | `int64`                       | Magic number                       |
| `AccountLogin`   | `int64`                       | Account login                      |

### `OnEventAccountInfo` — Account state snapshot

| Field        | Type     | Description              |
| ------------ | -------- | ------------------------ |
| `Balance`    | `double` | Account balance          |
| `Credit`     | `double` | Account credit           |
| `Equity`     | `double` | Account equity           |
| `Margin`     | `double` | Used margin              |
| `FreeMargin` | `double` | Free margin              |
| `Profit`     | `double` | Current profit           |
| `MarginLevel`| `double` | Margin level (%)         |
| `Login`      | `int64`  | Account login            |

---

## 🧱 Related enums (from proto)

### `MT5_SUB_ENUM_EVENT_GROUP_TYPE` (Event Group Type)

| Enum Value | Description |
|------------|-------------|
| `OrderProfit` | Position profit event |
| `OrderUpdate` | Order update event |

---

### `SUB_ENUM_POSITION_TYPE` (Position Type)

| Enum Value | Description |
|------------|-------------|
| `SubPositionTypeBuy` | Buy position |
| `SubPositionTypeSell` | Sell position |

---

### `SUB_ENUM_POSITION_REASON` (Position Open Reason)

| Enum Value | Description |
|------------|-------------|
| `SUB_POSITION_REASON_CLIENT` | Position opened from desktop terminal |
| `SUB_POSITION_REASON_MOBILE` | Position opened from mobile app |
| `SUB_POSITION_REASON_WEB` | Position opened from web platform |
| `SUB_POSITION_REASON_EXPERT` | Position opened by Expert Advisor |

---

### `SUB_ENUM_ORDER_STATE` (Order State)

| Enum Value | Description |
|------------|-------------|
| `SubOrderStateStarted` | Order checked, not yet accepted by broker |
| `SubOrderStatePlaced` | Order accepted |
| `SubOrderStateCanceled` | Order canceled by client |
| `SubOrderStatePartial` | Order partially executed |
| `SubOrderStateFilled` | Order fully executed |
| `SubOrderStateRejected` | Order rejected |
| `SubOrderStateExpired` | Order expired |
| `SubOrderStateRequestAdd` | Order is being registered (placing to the trading system) |
| `SubOrderStateRequestModify` | Order is being modified (changing its parameters) |
| `SubOrderStateRequestCancel` | Order is being deleted (deleting from the trading system) |

---

### `SUB_ENUM_ORDER_TYPE_TIME` (Order Time Type)

| Enum Value | Description |
|------------|-------------|
| `SUB_ORDER_TIME_GTC` | Good till cancel order |
| `SUB_ORDER_TIME_DAY` | Good till current trade day order |
| `SUB_ORDER_TIME_SPECIFIED` | Good till expired order |
| `SUB_ORDER_TIME_SPECIFIED_DAY` | The order will be effective till 23:59:59 of the specified day |

---

### `SUB_ENUM_ORDER_TYPE` (Order Type)

| Enum Value | Description |
|------------|-------------|
| `SUB_ORDER_TYPE_BUY` | Market buy order |
| `SUB_ORDER_TYPE_SELL` | Market sell order |
| `SUB_ORDER_TYPE_BUY_LIMIT` | Buy Limit pending order |
| `SUB_ORDER_TYPE_SELL_LIMIT` | Sell Limit pending order |
| `SUB_ORDER_TYPE_BUY_STOP` | Buy Stop pending order |
| `SUB_ORDER_TYPE_SELL_STOP` | Sell Stop pending order |
| `SUB_ORDER_TYPE_BUY_STOP_LIMIT` | Buy Stop Limit pending order |
| `SUB_ORDER_TYPE_SELL_STOP_LIMIT` | Sell Stop Limit pending order |
| `SUB_ORDER_TYPE_CLOSE_BY` | Close by opposite position order |

---

### `SUB_ENUM_ORDER_TYPE_FILLING` (Order Filling Mode)

| Enum Value | Description |
|------------|-------------|
| `SUB_ORDER_FILLING_FOK` | Fill or Kill - order must be filled completely or canceled |
| `SUB_ORDER_FILLING_IOC` | Immediate or Cancel - partial fills allowed, remainder canceled |
| `SUB_ORDER_FILLING_BOC` | Book or Cancel - order can only be placed in the order book |
| `SUB_ORDER_FILLING_RETURN` | Return - used for market orders on exchange |

---

### `SUB_ENUM_ORDER_REASON` (Order Reason)

| Enum Value | Description |
|------------|-------------|
| `SUB_ORDER_REASON_CLIENT` | Order placed from desktop terminal |
| `SUB_ORDER_REASON_MOBILE` | Order placed from mobile app |
| `SUB_ORDER_REASON_WEB` | Order placed from web platform |
| `SUB_ORDER_REASON_EXPERT` | Order placed by Expert Advisor |
| `SUB_ORDER_REASON_SL` | Order triggered by Stop Loss |
| `SUB_ORDER_REASON_TP` | Order triggered by Take Profit |
| `SUB_ORDER_REASON_SO` | Order triggered by Stop Out |

---

### `SUB_ENUM_DEAL_TYPE` (Deal Type)

| Enum Value | Description |
|------------|-------------|
| `SubDealTypeBuy` | Buy deal |
| `SubDealTypeSell` | Sell deal |
| `SubDealTypeBalance` | Balance operation |
| `SubDealTypeCredit` | Credit operation |
| `SUB_DEAL_TYPE_CHARGE` | Additional charge |
| `SUB_DEAL_TYPE_CORRECTION` | Correction |
| `SUB_DEAL_TYPE_BONUS` | Bonus |
| `SUB_DEAL_TYPE_COMMISSION` | Additional commission |
| `SUB_DEAL_TYPE_COMMISSION_DAILY` | Daily commission |
| `SUB_DEAL_TYPE_COMMISSION_MONTHLY` | Monthly commission |
| `SUB_DEAL_TYPE_COMMISSION_AGENT_DAILY` | Daily agent commission |
| `SUB_DEAL_TYPE_COMMISSION_AGENT_MONTHLY` | Monthly agent commission |
| `SUB_DEAL_TYPE_INTEREST` | Interest rate |
| `SUB_DEAL_TYPE_BUY_CANCELED` | Canceled buy deal |
| `SUB_DEAL_TYPE_SELL_CANCELED` | Canceled sell deal |

---

### `SUB_ENUM_DEAL_ENTRY` (Deal Entry Direction)

| Enum Value | Description |
|------------|-------------|
| `SubDealEntryIn` | Entry in (opening position) |
| `SubDealEntryOut` | Entry out (closing position) |
| `SubDealEntryInout` | Reverse (reversing position) |
| `SubDealEntryOutBy` | Close a position by an opposite one |

---

### `SUB_ENUM_DEAL_REASON` (Deal Reason)

| Enum Value | Description |
|------------|-------------|
| `SUB_DEAL_REASON_CLIENT` | Deal executed from desktop terminal |
| `SUB_DEAL_REASON_MOBILE` | Deal executed from mobile app |
| `SUB_DEAL_REASON_WEB` | Deal executed from web platform |
| `SUB_DEAL_REASON_EXPERT` | Deal executed by Expert Advisor |
| `SUB_DEAL_REASON_SL` | Deal executed by Stop Loss |
| `SUB_DEAL_REASON_TP` | Deal executed by Take Profit |
| `SUB_DEAL_REASON_SO` | Deal executed by Stop Out |
| `SUB_DEAL_REASON_ROLLOVER` | Deal executed due to rollover |
| `SUB_DEAL_REASON_VMARGIN` | Deal executed due to variation margin |
| `SUB_DEAL_REASON_SPLIT` | Deal executed due to split |
| `SUB_DEAL_REASON_CORPORATE_ACTION` | Deal executed due to corporate action |

---

## 💬 Just the essentials

* **What it is.** Real-time notifications for all trade-related events: order placement, execution, position opens/closes, modifications.
* **Why you need it.** Monitor trade execution in real-time, track position lifecycle, build event-driven systems without polling.
* **Sanity check.** Check `EventData` arrays to see what changed: `NewPositions`, `DisappearedPositions`, `NewHistoryDeals`, etc.

---

## 🎯 Purpose

Use it for real-time trade monitoring:

* Monitor order execution as it happens.
* Track position opens and closes.
* Detect SL/TP hits immediately.
* Build trade journaling systems.
* React to trade events in real-time.

---

## 🧩 Notes & Tips

* **Comprehensive events:** Includes orders, positions, deals, and history. Filter arrays you need.
* **Delta updates:** Each event shows what changed since last update.
* **Account snapshot:** Includes current account state with each event.
* **Performance:** Events arrive immediately when trades execute. Keep processing fast.
* **Comparison with OnTradeTransaction:** `OnTrade` is simpler, focused on trades. `OnTradeTransaction` is more detailed with full transaction lifecycle.

---

## 🔗 Usage Examples

### 1) Monitor new positions

```csharp
// svc — MT5Service instance

var cts = new CancellationTokenSource();

try
{
    await foreach (var trade in svc.OnTradeAsync(cts.Token))
    {
        foreach (var position in trade.EventData.NewPositions)
        {
            Console.WriteLine($"🆕 New position opened:");
            Console.WriteLine($"  Ticket: #{position.Ticket}");
            Console.WriteLine($"  Symbol: {position.Symbol}");
            Console.WriteLine($"  Type: {position.Type}");
            Console.WriteLine($"  Volume: {position.Volume}");
            Console.WriteLine($"  Price: {position.PriceOpen}");
            Console.WriteLine();
        }
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Stream stopped");
}
```

---

### 2) Monitor position closes

```csharp
var cts = new CancellationTokenSource();

await foreach (var trade in svc.OnTradeAsync(cts.Token))
{
    foreach (var position in trade.EventData.DisappearedPositions)
    {
        var profitSign = position.Profit >= 0 ? "+" : "";

        Console.WriteLine($"✅ Position closed:");
        Console.WriteLine($"  Ticket: #{position.Ticket}");
        Console.WriteLine($"  Symbol: {position.Symbol}");
        Console.WriteLine($"  Profit: {profitSign}${position.Profit:F2}");
        Console.WriteLine($"  Swap: ${position.Swap:F2}");
        Console.WriteLine();
    }
}
```

---

### 3) Track all trade activity

```csharp
var cts = new CancellationTokenSource();

await foreach (var trade in svc.OnTradeAsync(cts.Token))
{
    var e = trade.EventData;

    if (e.NewOrders.Count > 0)
        Console.WriteLine($"📝 {e.NewOrders.Count} new order(s) placed");

    if (e.NewPositions.Count > 0)
        Console.WriteLine($"🆕 {e.NewPositions.Count} position(s) opened");

    if (e.DisappearedPositions.Count > 0)
        Console.WriteLine($"✅ {e.DisappearedPositions.Count} position(s) closed");

    if (e.NewHistoryDeals.Count > 0)
        Console.WriteLine($"💼 {e.NewHistoryDeals.Count} new deal(s) in history");

    if (e.UpdatedPositions.Count > 0)
        Console.WriteLine($"🔄 {e.UpdatedPositions.Count} position(s) modified");
}
```

---

### 4) Log trade executions

```csharp
var cts = new CancellationTokenSource();
var logFile = "trade_executions.csv";

// Write CSV header
File.WriteAllText(logFile, "Timestamp,Ticket,Symbol,Type,Volume,Price,Profit\n");

await foreach (var trade in svc.OnTradeAsync(cts.Token))
{
    foreach (var deal in trade.EventData.NewHistoryDeals)
    {
        var timestamp = DateTime.UtcNow;
        var logEntry = $"{timestamp:yyyy-MM-dd HH:mm:ss}," +
                      $"{deal.Ticket}," +
                      $"{deal.Symbol}," +
                      $"{deal.Type}," +
                      $"{deal.Volume}," +
                      $"{deal.Price}," +
                      $"{deal.Profit}\n";

        File.AppendAllText(logFile, logEntry);
        Console.WriteLine($"Deal logged: {deal.Ticket} on {deal.Symbol}");
    }
}
```

---

### 5) Monitor account balance changes

```csharp
var cts = new CancellationTokenSource();
double? previousBalance = null;

await foreach (var trade in svc.OnTradeAsync(cts.Token))
{
    var currentBalance = trade.AccountInfo.Balance;

    if (previousBalance.HasValue && currentBalance != previousBalance)
    {
        var change = currentBalance - previousBalance.Value;
        var sign = change >= 0 ? "+" : "";

        Console.WriteLine($"💰 Balance changed: ${previousBalance:F2} → ${currentBalance:F2} ({sign}${change:F2})");
    }

    previousBalance = currentBalance;
}
```

---

### 6) Detect SL/TP modifications

```csharp
var cts = new CancellationTokenSource();

await foreach (var trade in svc.OnTradeAsync(cts.Token))
{
    foreach (var update in trade.EventData.UpdatedPositions)
    {
        var prev = update.PreviousPosition;
        var curr = update.CurrentPosition;

        if (prev.Sl != curr.Sl)
        {
            Console.WriteLine($"🔄 Position #{curr.Ticket} SL modified:");
            Console.WriteLine($"   {prev.Sl} → {curr.Sl}");
        }

        if (prev.Tp != curr.Tp)
        {
            Console.WriteLine($"🔄 Position #{curr.Ticket} TP modified:");
            Console.WriteLine($"   {prev.Tp} → {curr.Tp}");
        }
    }
}
```

---

### 7) Real-time trade statistics

```csharp
var cts = new CancellationTokenSource();

int totalTrades = 0;
int winningTrades = 0;
int losingTrades = 0;
double totalProfit = 0;

await foreach (var trade in svc.OnTradeAsync(cts.Token))
{
    foreach (var deal in trade.EventData.NewHistoryDeals)
    {
        // Count only entry/exit deals (not balance operations)
        if (deal.Entry == SUB_ENUM_DEAL_ENTRY.SubDealEntryOut)
        {
            totalTrades++;
            totalProfit += deal.Profit;

            if (deal.Profit >= 0)
                winningTrades++;
            else
                losingTrades++;

            var winRate = totalTrades > 0 ? (winningTrades * 100.0 / totalTrades) : 0;

            Console.WriteLine($"\n📊 Trade Statistics:");
            Console.WriteLine($"   Total trades: {totalTrades}");
            Console.WriteLine($"   Wins: {winningTrades} | Losses: {losingTrades}");
            Console.WriteLine($"   Win rate: {winRate:F1}%");
            Console.WriteLine($"   Total P/L: ${totalProfit:F2}");
        }
    }
}
```
