# ✅ Validating Trade Requests (`OrderCheckAsync`)

> **Request:** Run a dry‑run check of a trade request on **MT5**. The server simulates the trade and returns balance / equity / margin impact **without actually placing an order**.

**API Information:**

* **SDK wrapper:** `MT5Account.OrderCheckAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.TradeFunctions`
* **Proto definition:** `OrderCheck` (defined in `mt5-term-api-trade-functions.proto`)

### RPC

* **Service:** `mt5_term_api.TradeFunctions`
* **Method:** `OrderCheck(OrderCheckRequest) → OrderCheckReply`
* **Low‑level client (generated):** `TradeFunctions.TradeFunctionsClient.OrderCheck(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace mt5_term_api
{
    public class MT5Account
    {
        public async Task<OrderCheckData> OrderCheckAsync(
            OrderCheckRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`OrderCheckRequest { mql_trade_request: MrpcMqlTradeRequest }`


**Reply message:**

`OrderCheckReply { data: OrderCheckData }`

---

## 🔽 Input

| Parameter           | Type                | Description                                               |
| ------------------- | ------------------- | --------------------------------------------------------- |
| `request`           | `OrderCheckRequest` | Protobuf request wrapping `MrpcMqlTradeRequest`           |
| `deadline`          | `DateTime?`         | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | Cooperative cancel for the call/retry loop                |

### `MrpcMqlTradeRequest` — Core trade description

| Field                      | Type                              | Description                                          |
| -------------------------- | --------------------------------- | ---------------------------------------------------- |
| `Action`                   | `MRPC_ENUM_TRADE_REQUEST_ACTIONS` | Trade operation type (deal, pending order, etc.)     |
| `ExpertAdvisorMagicNumber` | `ulong`                           | EA magic number (use `0` for manual trades)          |
| `Order`                    | `ulong`                           | Order ticket (for modify/remove operations)          |
| `Symbol`                   | `string`                          | Trading symbol, e.g. `"EURUSD"`, `"XAUUSD"`          |
| `Volume`                   | `double`                          | Volume in lots (must respect symbol min/max/step)    |
| `Price`                    | `double`                          | Price for validation. Use current Bid/Ask for market |
| `StopLimit`                | `double`                          | StopLimit price (for StopLimit orders)               |
| `StopLoss`                 | `double`                          | Stop Loss level (optional)                           |
| `TakeProfit`               | `double`                          | Take Profit level (optional)                         |
| `Deviation`                | `ulong`                           | Max price deviation in points (optional)             |
| `OrderType`                | `ENUM_ORDER_TYPE_TF`              | Order type: Buy, Sell, BuyLimit, SellStop, etc.      |
| `TypeFilling`              | `MRPC_ENUM_ORDER_TYPE_FILLING`    | Filling mode: FOK, IOC, Return, BOC                  |
| `TypeTime`                 | `MRPC_ENUM_ORDER_TYPE_TIME`       | Time‑in‑force: GTC, Day, Specified                   |
| `Expiration`               | `Timestamp`                       | Expiration time (for ORDER_TIME_SPECIFIED)           |
| `Comment`                  | `string`                          | Order comment (optional)                             |
| `Position`                 | `ulong`                           | Position ticket (for position operations)            |
| `PositionBy`               | `ulong`                           | Opposite position ticket (for close-by)              |

---

## ⬆️ Output — `OrderCheckData`

| Field                                   | Type     | Description                                         |
| --------------------------------------- | -------- | --------------------------------------------------- |
| `MqlTradeCheckResult.ReturnedCode`      | `uint`   | Reply code (0 = OK, other = rejected/error)         |
| `MqlTradeCheckResult.BalanceAfterDeal`  | `double` | Simulated balance after the trade                   |
| `MqlTradeCheckResult.EquityAfterDeal`   | `double` | Simulated equity after the trade                    |
| `MqlTradeCheckResult.Profit`            | `double` | Simulated floating profit                           |
| `MqlTradeCheckResult.Margin`            | `double` | Required margin for the trade                       |
| `MqlTradeCheckResult.FreeMargin`        | `double` | Simulated free margin after the trade               |
| `MqlTradeCheckResult.MarginLevel`       | `double` | Margin level after the trade (percent)              |
| `MqlTradeCheckResult.Comment`           | `string` | Human‑readable explanation for `ReturnedCode`       |

---

## 🧱 Related enums (from proto)

### `ENUM_ORDER_TYPE_TF`

* `OrderTypeTfBuy` — Market Buy order
* `OrderTypeTfSell` — Market Sell order
* `OrderTypeTfBuyLimit` — Buy Limit pending order
* `OrderTypeTfSellLimit` — Sell Limit pending order
* `OrderTypeTfBuyStop` — Buy Stop pending order
* `OrderTypeTfSellStop` — Sell Stop pending order
* `OrderTypeTfBuyStopLimit` — Buy Stop Limit order
* `OrderTypeTfSellStopLimit` — Sell Stop Limit order
* `OrderTypeTfCloseBy` — Close position by opposite one

### `MRPC_ENUM_ORDER_TYPE_FILLING`

* `OrderFillingFok` — Fill‑or‑Kill (execute full volume or cancel)
* `OrderFillingIoc` — Immediate‑or‑Cancel (execute available volume, cancel rest)
* `OrderFillingReturn` — Return (partial fill allowed, rest remains active)
* `OrderFillingBoc` — Book‑or‑Cancel (place in DOM, don't execute immediately)

### `MRPC_ENUM_ORDER_TYPE_TIME`

* `OrderTimeGtc` — Good‑Till‑Cancelled
* `OrderTimeDay` — Good till current trade day
* `OrderTimeSpecified` — Good till expiration time
* `OrderTimeSpecifiedDay` — Good till 23:59:59 of specified day

### `MRPC_ENUM_TRADE_REQUEST_ACTIONS`

* `TradeActionDeal` — Place market order for immediate execution
* `TradeActionPending` — Place pending order
* `TradeActionSltp` — Modify SL/TP of opened position
* `TradeActionModify` — Modify pending order parameters
* `TradeActionRemove` — Delete pending order
* `TradeActionCloseBy` — Close position by opposite one

---

## 💬 Just the essentials

* **What it is.** Dry‑run validation of a trade request. Returns simulated balance/equity/margin impact without placing a real order.
* **Why you need it.** Catch errors ("not enough money", wrong lot sizes, bad prices) **before** calling `OrderSendAsync`.
* **Sanity check.** If `ReturnedCode == 0` and `Comment` says "OK" → trade can proceed.

---

## 🎯 Purpose

Use it to validate trades before execution:

* Pre‑trade risk checks (margin level, lot size).
* Debug strategy behavior on new brokers.
* Prevent "not enough money" errors.

---

## 🧩 Notes & Tips

* **Broker support:** Some brokers don't support `OrderCheck`. Server returns error → `ApiExceptionMT5`. This is a broker limitation.
* **⚠️ MetaQuotes-Demo server limitation:** On MetaQuotes-Demo broker, OrderCheck returns success (code 0) but all margin/balance/equity values are `-0.00` (negative zeros). The server accepts the request but doesn't simulate real values. **Use `OrderCalcMarginAsync()` instead** for margin calculations on this broker.
* **Market orders:** Use current Bid/Ask as `Price` (Buy → ask, Sell → bid).
* **Respect symbol limits:** Check `SymbolVolumeMin/Max/Step` before calling.
* **Short deadline:** 5–10 seconds is enough. Exotic symbols may be slower.
* **Alternative:** If OrderCheck returns zeros or doesn't work, use `OrderCalcMarginAsync()` which is simpler and more reliable across brokers.

---

## 🔗 Usage Examples

### 1) Basic market BUY validation

```csharp
// acc — an already connected MT5Account instance

// Get minimal lot for the symbol
var volumeMin = await acc.SymbolInfoDoubleAsync(
    "EURUSD",
    SymbolInfoDoublePropertyType.SymbolVolumeMin);

var symbol = "EURUSD";
var minLot = volumeMin.Value;

// Current ask price (get from your quote feed)
double ask = /* current ask price here */;

// Build trade request
var tradeRequest = new MrpcMqlTradeRequest
{
    Action = MRPC_ENUM_TRADE_REQUEST_ACTIONS.TradeActionDeal,
    Symbol = symbol,
    Volume = minLot,
    Price = ask,
    StopLoss = 0.0,
    TakeProfit = 0.0,
    OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
    TypeFilling = MRPC_ENUM_ORDER_TYPE_FILLING.OrderFillingFok,
    TypeTime = MRPC_ENUM_ORDER_TYPE_TIME.OrderTimeGtc,
    ExpertAdvisorMagicNumber = 12345
};

var checkRequest = new OrderCheckRequest
{
    MqlTradeRequest = tradeRequest
};

var checkResult = await acc.OrderCheckAsync(
    checkRequest,
    deadline: DateTime.UtcNow.AddSeconds(10));

var result = checkResult?.MqlTradeCheckResult;
if (result == null)
{
    Console.WriteLine("OrderCheck returned no data.");
}
else
{
    Console.WriteLine($"Return code:      {result.ReturnedCode}");
    Console.WriteLine($"Balance after:    {result.BalanceAfterDeal:F2}");
    Console.WriteLine($"Equity after:     {result.EquityAfterDeal:F2}");
    Console.WriteLine($"Required margin:  {result.Margin:F2}");
    Console.WriteLine($"Free margin:      {result.FreeMargin:F2}");
    Console.WriteLine($"Margin level:     {result.MarginLevel:F2}%");
    Console.WriteLine($"Comment:          {result.Comment}");
}
```

---

### 2) Send order only if the check passes

```csharp
async Task<bool> TrySendBuyAsync(MT5Account acc, string symbol, double lots)
{
    double ask = /* current ask price */;

    var tradeRequest = new MrpcMqlTradeRequest
    {
        Action = MRPC_ENUM_TRADE_REQUEST_ACTIONS.TradeActionDeal,
        Symbol = symbol,
        Volume = lots,
        Price = ask,
        OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
        TypeFilling = MRPC_ENUM_ORDER_TYPE_FILLING.OrderFillingFok,
        TypeTime = MRPC_ENUM_ORDER_TYPE_TIME.OrderTimeGtc,
        ExpertAdvisorMagicNumber = 202501
    };

    var check = await acc.OrderCheckAsync(new OrderCheckRequest
    {
        MqlTradeRequest = tradeRequest
    });

    var result = check?.MqlTradeCheckResult;
    if (result == null)
        throw new Exception("OrderCheck returned null result");

    var isOk = result.ReturnedCode == 0; // or your own mapping

    if (isOk)
    {
        // Now we can safely send the order
        var sendResult = await acc.OrderSendAsync(new OrderSendRequest
        {
            MqlTradeRequest = tradeRequest
        });

        Console.WriteLine($"Order sent, result: {sendResult.ReturnedCode}");
        return true;
    }

    Console.WriteLine($"Order rejected by OrderCheck: {result.ReturnedCode} / {result.Comment}");
    return false;
}
```

---

### 3) Simple risk rule by margin level

```csharp
// Example: do not trade if margin level after the deal falls below 200%

var check = await acc.OrderCheckAsync(new OrderCheckRequest
{
    MqlTradeRequest = new MrpcMqlTradeRequest
    {
        Action = MRPC_ENUM_TRADE_REQUEST_ACTIONS.TradeActionDeal,
        Symbol = "XAUUSD",
        Volume = 0.10,
        Price = /* current ask */,
        OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
        TypeFilling = MRPC_ENUM_ORDER_TYPE_FILLING.OrderFillingFok,
        TypeTime = MRPC_ENUM_ORDER_TYPE_TIME.OrderTimeGtc
    }
});

var r = check.MqlTradeCheckResult;

if (r.MarginLevel < 200.0)
{
    Console.WriteLine($"Risk rule triggered: margin level would drop to {r.MarginLevel:F2}%");
    // Skip trading here
}
else
{
    Console.WriteLine("Margin level OK, you may proceed with OrderSendAsync.");
}
```
