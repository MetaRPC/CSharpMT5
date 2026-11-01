# MT5Account · Trading Operations - Overview

> Place orders, modify positions, close trades, check orders, and calculate margin. Use this page to choose the right API for trading operations.

## 📁 What lives here

* **[OrderSend](./OrderSend.md)** - **place market or pending orders** (buy, sell, limit, stop, stop-limit).
* **[OrderModify](./OrderModify.md)** - **modify** existing orders or positions (price, SL, TP, expiration).
* **[OrderClose](./OrderClose.md)** - **close** positions (full or partial).
* **[OrderCheck](./OrderCheck.md)** - **validate** order before sending (check margin, free margin, etc.).
* **[OrderCalcMargin](./OrderCalcMargin.md)** - **calculate** required margin for an order.
* **[OrderCalcProfit](./OrderCalcProfit.md)** - **calculate** expected profit of a trade between two prices.
* **[OrderSendWithoutChecks](./OrderSendWithoutChecks.md)** - **fast send** without server-side pre-checks (latency path).

---

## 🧭 Plain English

* **OrderSend** → the **main trading method** - opens positions, places pending orders.
* **OrderModify** → change SL/TP levels, order prices, or expiration times.
* **OrderClose** → close positions (with optional partial close).
* **OrderCheck** → pre-flight check before sending order (validates margin, volume, etc.).
* **OrderCalcMargin** → calculate how much margin you need before trading.

> Rule of thumb: **always** use `OrderCheckAsync` or `OrderCalcMarginAsync` before sending large orders to avoid margin errors.

---

## Quick choose

| If you need…                                     | Use                     | Returns                    | Key inputs                          |
| ------------------------------------------------ | ----------------------- | -------------------------- | ----------------------------------- |
| Open position or place pending order             | `OrderSendAsync`        | OrderSendData (ticket, deal, code) | Symbol, operation, volume, price, SL/TP |
| Modify existing order/position                   | `OrderModifyAsync`      | Success code               | Ticket, new price, SL, TP, expiration |
| Close position (full or partial)                 | `OrderCloseAsync`       | Success code               | Ticket, volume, slippage            |
| Validate order before sending                    | `OrderCheckAsync`       | Check result (margin, profit, errors) | MqlTradeRequest object              |
| Calculate required margin                        | `OrderCalcMarginAsync`  | Margin value               | Symbol, order type, volume, price   |

---

## ❌ Cross‑refs & gotchas

* **OrderSend** return code `10009` (TradeRetcodeDone) = success.
* **Slippage** in points - set reasonable value (5-10 points typical).
* **Stop Loss/Take Profit** must respect symbol's STOPLEVEL (minimum distance from price).
* **Volume** must be within symbol's min/max lot size and step.
* **OrderCheck** doesn't actually place order - just validates parameters.
* **Partial close** - specify volume less than position size.
* **Expiration** only works for TIME_SPECIFIED order type.

---

## 🟢 Minimal snippets

```csharp
// Place market buy order
var request = new OrderSendRequest
{
    Symbol = "EURUSD",
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
    Volume = 0.1,
    StopLoss = 1.08500,
    TakeProfit = 1.09500,
    Comment = "My trade"
};
var result = await account.OrderSendAsync(request);
if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✅ Order placed! Ticket: {result.Order}, Deal: {result.Deal}");
}
else
{
    Console.WriteLine($"❌ Error: {result.Comment}");
}
```

```csharp
// Modify position - update SL/TP
var modifyResult = await account.OrderModifyAsync(new OrderModifyRequest
{
    Ticket = 123456789,
    StopLoss = 1.08000,
    TakeProfit = 1.10000
});
Console.WriteLine($"Modify result: {modifyResult.ReturnedCode}");
```

```csharp
// Close position
var closeResult = await account.OrderCloseAsync(new OrderCloseRequest
{
    Ticket = 123456789,
    Volume = 0.1,  // Full volume
    Slippage = 10   // 10 points
});
Console.WriteLine($"Close result: {closeResult.ReturnedCode}");
```

```csharp
// Check order before sending
var tradeRequest = new MrpcMqlTradeRequest
{
    Action = MRPC_ENUM_TRADE_REQUEST_ACTIONS.TradeActionDeal,
    Symbol = "EURUSD",
    Volume = 1.0,
    OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
    Price = 1.09000
};
var checkResult = await account.OrderCheckAsync(new OrderCheckRequest
{
    MqlTradeRequest = tradeRequest
});
var checkData = checkResult.MqlTradeCheckResult;
Console.WriteLine($"Margin needed: ${checkData.Margin:F2}, Free margin after: ${checkData.FreeMargin:F2}");
```

```csharp
// Calculate margin for 1 lot EURUSD
var marginData = await account.OrderCalcMarginAsync(
    symbol: "EURUSD",
    orderType: ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
    volume: 1.0,
    openPrice: 1.09000
);
Console.WriteLine($"Required margin: ${marginData.Margin:F2}");
```

---

## See also

* **Positions:** [OpenedOrders](../3.%20Position_Orders_Information/OpenedOrders.md) - view current positions before modifying
* **Account:** [AccountSummary](../1.%20Account_information/AccountSummary.md) - check free margin before trading
* **Symbols:** [SymbolInfoDouble](../2.%20Symbol_information/SymbolInfoDouble.md) - get current bid/ask prices
* **Streaming:** [SubscribeToTradeTransaction](../7.%20Streaming_Methods/SubscribeToTradeTransaction.md) - real-time trade events
