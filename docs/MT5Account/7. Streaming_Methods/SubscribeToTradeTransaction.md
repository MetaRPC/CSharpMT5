# ✅ Subscribe to Trade Transaction Events (`OnTradeTransactionAsync`)

> **Stream:** Real-time trade transaction notifications on **MT5**. Continuously sends events for all trade operations (orders, deals, position changes).

**API Information:**

* **SDK wrapper:** `MT5Service.OnTradeTransactionAsync(...)` (from class `MT5Service`)
* **gRPC service:** `mt5_term_api.SubscriptionService`
* **Proto definition:** `OnTradeTransaction` (defined in `mt5-term-api-subscriptions.proto`)

### RPC

* **Service:** `mt5_term_api.SubscriptionService`
* **Method:** `OnTradeTransaction(OnTradeTransactionRequest) → stream OnTradeTransactionReply`
* **Low‑level client (generated):** `SubscriptionService.SubscriptionServiceClient.OnTradeTransaction(request, headers, deadline, cancellationToken)`
* **SDK wrapper:**

```csharp
namespace mt5_term_api
{
    public class MT5Service
    {
        public async IAsyncEnumerable<OnTradeTransactionData> OnTradeTransactionAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`OnTradeTransactionRequest {}` (empty)

**Reply message (stream):**

`OnTradeTransactionReply { data: OnTradeTransactionData }`

---

## 🔽 Input

| Parameter            | Type                  | Description                                         |
| -------------------- | --------------------- | --------------------------------------------------- |
| `cancellationToken`  | `CancellationToken`   | Token to stop the stream                            |

---

## ⬆️ Output — `OnTradeTransactionData` (stream)

| Field                    | Type                                | Description                              |
| ------------------------ | ----------------------------------- | ---------------------------------------- |
| `Type`                   | `MT5_SUB_ENUM_EVENT_GROUP_TYPE`     | Event group type                         |
| `TradeTransaction`       | `MqlTradeTransaction`               | Transaction details                      |
| `TradeRequest`           | `MqlTradeRequest`                   | Trade request that triggered transaction |
| `TradeResult`            | `MqlTradeResult`                    | Result of trade request                  |
| `TerminalInstanceGuidId` | `string`                            | Terminal instance ID                     |
| `AccountInfo`            | `OnEventAccountInfo`                | Current account state                    |

### `MqlTradeTransaction` — Transaction details

| Field                          | Type                              | Description                                |
| ------------------------------ | --------------------------------- | ------------------------------------------ |
| `DealTicket`                   | `uint64`                          | Deal ticket (if deal was executed)         |
| `OrderTicket`                  | `uint64`                          | Order ticket                               |
| `Symbol`                       | `string`                          | Symbol name                                |
| `Type`                         | `SUB_ENUM_TRADE_TRANSACTION_TYPE` | Transaction type (OrderAdd, DealAdd, etc)  |
| `OrderType`                    | `SUB_ENUM_ORDER_TYPE`             | Order type (Buy, Sell, BuyLimit, etc)      |
| `OrderState`                   | `SUB_ENUM_ORDER_STATE`            | Order state (Placed, Filled, etc)          |
| `DealType`                     | `SUB_ENUM_DEAL_TYPE`              | Deal type (Buy, Sell, etc)                 |
| `OrderTimeType`                | `SUB_ENUM_ORDER_TYPE_TIME`        | Order expiration type                      |
| `OrderExpirationTime`          | `Timestamp`                       | Order expiration time                      |
| `Price`                        | `double`                          | Price                                      |
| `PriceTriggerStopLimit`        | `double`                          | Stop limit activation price                |
| `PriceStopLoss`                | `double`                          | Stop Loss level                            |
| `PriceTakeProfit`              | `double`                          | Take Profit level                          |
| `Volume`                       | `double`                          | Volume in lots                             |
| `PositionTicket`               | `uint64`                          | Position ticket                            |
| `PositionByOppositePosition`   | `uint64`                          | Opposite position ticket (for CloseBy)     |

### `MqlTradeRequest` — Request that triggered transaction

| Field                           | Type                              | Description                          |
| ------------------------------- | --------------------------------- | ------------------------------------ |
| `TradeOperationType`            | `SUB_ENUM_TRADE_REQUEST_ACTIONS`  | Trade operation (Deal, Pending, etc) |
| `Magic`                         | `uint64`                          | Expert Advisor ID (magic number)     |
| `OrderTicket`                   | `uint64`                          | Order ticket                         |
| `Symbol`                        | `string`                          | Symbol name                          |
| `RequestedDealVolumeLots`       | `double`                          | Requested volume in lots             |
| `Price`                         | `double`                          | Price                                |
| `StopLimit`                     | `double`                          | StopLimit level                      |
| `StopLoss`                      | `double`                          | Stop Loss level                      |
| `TakeProfit`                    | `double`                          | Take Profit level                    |
| `Deviation`                     | `uint64`                          | Max deviation from requested price   |
| `OrderType`                     | `SUB_ENUM_ORDER_TYPE`             | Order type                           |
| `OrderTypeFilling`              | `SUB_ENUM_ORDER_TYPE_FILLING`     | Order filling mode                   |
| `TypeTime`                      | `SUB_ENUM_ORDER_TYPE_TIME`        | Order expiration type                |
| `OrderExpirationTime`           | `Timestamp`                       | Order expiration time                |
| `OrderComment`                  | `string`                          | Order comment                        |
| `PositionTicket`                | `uint64`                          | Position ticket                      |
| `PositionByOppositePosition`    | `uint64`                          | Opposite position ticket             |

### `MqlTradeResult` — Result of trade operation

| Field                        | Type                 | Description                                |
| ---------------------------- | -------------------- | ------------------------------------------ |
| `TradeReturnIntCode`         | `uint32`             | Return code (10009 = success)              |
| `TradeReturnCode`            | `MqlErrorTradeCode`  | Error code enum                            |
| `DealTicket`                 | `uint64`             | Deal ticket (if executed)                  |
| `OrderTicket`                | `uint64`             | Order ticket (if placed)                   |
| `DealVolume`                 | `double`             | Deal volume confirmed by broker            |
| `DealPrice`                  | `double`             | Deal price confirmed by broker             |
| `CurrentBid`                 | `double`             | Current Bid price                          |
| `CurrentAsk`                 | `double`             | Current Ask price                          |
| `BrokerCommentToOperation`   | `string`             | Broker comment                             |
| `TerminalDispatchRequestId`  | `uint32`             | Request ID from terminal                   |
| `ReturnCodeExternal`         | `int32`              | External system return code                |

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

### `SUB_ENUM_TRADE_TRANSACTION_TYPE`

* `SubTradeTransactionOrderAdd` — Adding new order
* `SubTradeTransactionOrderUpdate` — Updating order
* `SubTradeTransactionOrderDelete` — Removing order
* `SubTradeTransactionDealAdd` — Adding deal to history
* `SubTradeTransactionDealUpdate` — Updating deal in history
* `SubTradeTransactionDealDelete` — Deleting deal from history
* `SubTradeTransactionHistoryAdd` — Adding order to history
* `SubTradeTransactionHistoryUpdate` — Changing order in history
* `SubTradeTransactionHistoryDelete` — Deleting order from history
* `SubTradeTransactionPosition` — Position change (not from deal)
* `SubTradeTransactionRequest` — Trade request processed notification

### `SUB_ENUM_TRADE_REQUEST_ACTIONS`

* `SubTradeActionUndefined` — Undefined
* `SubTradeActionDeal` — Market order execution
* `SubTradeActionPending` — Place pending order
* `SubTradeActionSltp` — Modify SL/TP
* `SubTradeActionModify` — Modify order parameters
* `SubTradeActionRemove` — Remove pending order
* `SubTradeActionCloseBy` — Close position by opposite

### `MT5_SUB_ENUM_EVENT_GROUP_TYPE`

* `OrderProfit` — Position profit event
* `OrderUpdate` — Order update event

---

## 💬 Just the essentials

* **What it is.** Real-time notifications for all trade operations. Tracks complete order lifecycle from placement to execution/cancellation.
* **Why you need it.** Monitor order execution in real-time, track trade history as it happens, build event-driven trading systems.
* **Sanity check.** Stream runs continuously. Check `Type` field to determine transaction type. Use `TradeResult.TradeReturnIntCode` to verify success (10009).

---

## 🎯 Purpose

Use it for real-time trade monitoring:

* Track order execution in real-time.
* Monitor complete order lifecycle.
* Build event-driven trading strategies.
* Log all trade operations.
* React to trade events immediately.

---

## 🧩 Notes & Tips

* **Comprehensive events:** Receives ALL trade events (orders, deals, position changes). Filter by `Type` to process only needed events.
* **Transaction types:** Most common: `OrderAdd` (new order), `DealAdd` (order filled), `HistoryAdd` (order moved to history).
* **Return codes:** Check `TradeResult.TradeReturnIntCode`. 10009 = success, other codes = errors.
* **Account snapshot:** Each event includes current account state (`AccountInfo`). Useful for margin monitoring.
* **Request details:** `TradeRequest` shows what was requested. `TradeResult` shows what actually happened.
* **Performance:** Events arrive frequently during active trading. Keep processing logic fast.
* **Error handling:** Wrap in try-catch. Handle `OperationCanceledException` when stopping.

---

## 🔗 Usage Examples

### 1) Basic transaction monitoring

```csharp
// svc — MT5Service instance

var cts = new CancellationTokenSource();

try
{
    await foreach (var tx in svc.OnTradeTransactionAsync(cts.Token))
    {
        Console.WriteLine($"Transaction type: {tx.TradeTransaction.Type}");
        Console.WriteLine($"  Symbol: {tx.TradeTransaction.Symbol}");
        Console.WriteLine($"  Order: {tx.TradeTransaction.OrderTicket}");
        Console.WriteLine($"  State: {tx.TradeTransaction.OrderState}");
        Console.WriteLine();
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Stream stopped");
}
```

---

### 2) Monitor order fills only

```csharp
var cts = new CancellationTokenSource();

await foreach (var tx in svc.OnTradeTransactionAsync(cts.Token))
{
    // Filter for deal additions (order fills)
    if (tx.TradeTransaction.Type == SUB_ENUM_TRADE_TRANSACTION_TYPE.SubTradeTransactionDealAdd)
    {
        var t = tx.TradeTransaction;
        Console.WriteLine($"✓ Order filled:");
        Console.WriteLine($"  Deal #{t.DealTicket}");
        Console.WriteLine($"  Symbol: {t.Symbol}");
        Console.WriteLine($"  Type: {t.DealType}");
        Console.WriteLine($"  Volume: {t.Volume} lots");
        Console.WriteLine($"  Price: {t.Price}");
        Console.WriteLine();
    }
}
```

---

### 3) Track order lifecycle

```csharp
var cts = new CancellationTokenSource();

await foreach (var tx in svc.OnTradeTransactionAsync(cts.Token))
{
    var t = tx.TradeTransaction;

    switch (t.Type)
    {
        case SUB_ENUM_TRADE_TRANSACTION_TYPE.SubTradeTransactionOrderAdd:
            Console.WriteLine($"🆕 New order #{t.OrderTicket} placed for {t.Symbol}");
            break;

        case SUB_ENUM_TRADE_TRANSACTION_TYPE.SubTradeTransactionOrderUpdate:
            Console.WriteLine($"🔄 Order #{t.OrderTicket} updated: {t.OrderState}");
            break;

        case SUB_ENUM_TRADE_TRANSACTION_TYPE.SubTradeTransactionDealAdd:
            Console.WriteLine($"✅ Order #{t.OrderTicket} filled: {t.Volume} lots @ {t.Price}");
            break;

        case SUB_ENUM_TRADE_TRANSACTION_TYPE.SubTradeTransactionOrderDelete:
            Console.WriteLine($"❌ Order #{t.OrderTicket} cancelled/removed");
            break;
    }
}
```

---

### 4) Monitor account state on each trade

```csharp
var cts = new CancellationTokenSource();

await foreach (var tx in svc.OnTradeTransactionAsync(cts.Token))
{
    // Only process deal executions
    if (tx.TradeTransaction.Type == SUB_ENUM_TRADE_TRANSACTION_TYPE.SubTradeTransactionDealAdd)
    {
        var acc = tx.AccountInfo;

        Console.WriteLine($"Trade executed:");
        Console.WriteLine($"  Balance: ${acc.Balance:F2}");
        Console.WriteLine($"  Equity: ${acc.Equity:F2}");
        Console.WriteLine($"  Margin: ${acc.Margin:F2}");
        Console.WriteLine($"  Free Margin: ${acc.FreeMargin:F2}");
        Console.WriteLine($"  Margin Level: {acc.MarginLevel:F2}%");
        Console.WriteLine($"  Profit: ${acc.Profit:F2}");
        Console.WriteLine();

        // Alert if margin level low
        if (acc.MarginLevel < 100)
        {
            Console.WriteLine($"⚠ WARNING: Low margin level ({acc.MarginLevel:F2}%)!");
        }
    }
}
```

---

### 5) Log all trade operations

```csharp
var cts = new CancellationTokenSource();
var logFile = "trade_log.txt";

await foreach (var tx in svc.OnTradeTransactionAsync(cts.Token))
{
    var t = tx.TradeTransaction;
    var timestamp = DateTime.UtcNow;

    var logEntry = $"[{timestamp:yyyy-MM-dd HH:mm:ss}] " +
                   $"Type: {t.Type} | " +
                   $"Symbol: {t.Symbol} | " +
                   $"Order: {t.OrderTicket} | " +
                   $"State: {t.OrderState} | " +
                   $"Volume: {t.Volume}";

    File.AppendAllText(logFile, logEntry + Environment.NewLine);
    Console.WriteLine(logEntry);
}
```

---

### 6) Check trade request results

```csharp
var cts = new CancellationTokenSource();

await foreach (var tx in svc.OnTradeTransactionAsync(cts.Token))
{
    // Process request completion events
    if (tx.TradeTransaction.Type == SUB_ENUM_TRADE_TRANSACTION_TYPE.SubTradeTransactionRequest)
    {
        var result = tx.TradeResult;
        var request = tx.TradeRequest;

        if (result.TradeReturnIntCode == 10009)
        {
            Console.WriteLine($"✓ Trade request successful:");
            Console.WriteLine($"  Operation: {request.TradeOperationType}");
            Console.WriteLine($"  Symbol: {request.Symbol}");
            Console.WriteLine($"  Order: #{result.OrderTicket}");
            if (result.DealTicket > 0)
            {
                Console.WriteLine($"  Deal: #{result.DealTicket}");
                Console.WriteLine($"  Fill price: {result.DealPrice}");
            }
        }
        else
        {
            Console.WriteLine($"✗ Trade request failed:");
            Console.WriteLine($"  Code: {result.TradeReturnIntCode}");
            Console.WriteLine($"  Comment: {result.BrokerCommentToOperation}");
        }
        Console.WriteLine();
    }
}
```

---

### 7) Position change notifications

```csharp
var cts = new CancellationTokenSource();

await foreach (var tx in svc.OnTradeTransactionAsync(cts.Token))
{
    // Monitor position changes
    if (tx.TradeTransaction.Type == SUB_ENUM_TRADE_TRANSACTION_TYPE.SubTradeTransactionPosition)
    {
        var t = tx.TradeTransaction;

        Console.WriteLine($"📊 Position changed:");
        Console.WriteLine($"  Position: #{t.PositionTicket}");
        Console.WriteLine($"  Symbol: {t.Symbol}");
        Console.WriteLine($"  Volume: {t.Volume}");
        Console.WriteLine($"  SL: {t.PriceStopLoss}");
        Console.WriteLine($"  TP: {t.PriceTakeProfit}");
        Console.WriteLine();
    }
}
```

---

### 8) Stop-loss / Take-profit hit detection

```csharp
var cts = new CancellationTokenSource();

await foreach (var tx in svc.OnTradeTransactionAsync(cts.Token))
{
    if (tx.TradeTransaction.Type == SUB_ENUM_TRADE_TRANSACTION_TYPE.SubTradeTransactionDealAdd)
    {
        var dealType = tx.TradeTransaction.DealType;

        // Check if position was closed by SL/TP
        if (dealType == SUB_ENUM_DEAL_TYPE.SubDealTypeBuy ||
            dealType == SUB_ENUM_DEAL_TYPE.SubDealTypeSell)
        {
            var t = tx.TradeTransaction;

            Console.WriteLine($"Position closed:");
            Console.WriteLine($"  Symbol: {t.Symbol}");
            Console.WriteLine($"  Exit price: {t.Price}");

            // Check if it was SL or TP hit (would need additional context to determine)
            if (tx.AccountInfo.Profit < 0)
            {
                Console.WriteLine($"  ❌ Loss: ${tx.AccountInfo.Profit:F2} (likely SL hit)");
            }
            else
            {
                Console.WriteLine($"  ✅ Profit: ${tx.AccountInfo.Profit:F2} (likely TP hit)");
            }
        }
    }
}
```
