# ✅ Modifying Orders and Positions (`OrderModifyAsync`)

> **Request:** Modify SL/TP of an open position or modify parameters of a pending order on **MT5**.

**API Information:**

* **SDK wrapper:** `MT5Account.OrderModifyAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.TradingHelper`
* **Proto definition:** `OrderModify` (defined in `mt5-term-api-trading-helper.proto`)

### RPC

* **Service:** `mt5_term_api.TradingHelper`
* **Method:** `OrderModify(OrderModifyRequest) → OrderModifyReply`
* **Low‑level client (generated):** `TradingHelper.TradingHelperClient.OrderModify(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace mt5_term_api
{
    public class MT5Account
    {
        public async Task<OrderModifyData> OrderModifyAsync(
            OrderModifyRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`OrderModifyRequest { ticket, stop_loss, take_profit, ... }`


**Reply message:**

`OrderModifyReply { data: OrderModifyData }`

---

## 🔽 Input

| Parameter           | Type                 | Description                                               |
| ------------------- | -------------------- | --------------------------------------------------------- |
| `request`           | `OrderModifyRequest` | Protobuf request with modification parameters             |
| `deadline`          | `DateTime?`          | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`  | Cooperative cancel for the call/retry loop                |

### `OrderModifyRequest`

| Field                 | Type                         | Description                                               |
| --------------------- | ---------------------------- | --------------------------------------------------------- |
| `Ticket`              | `ulong`                      | Position or pending order ticket **REQUIRED**              |
| `StopLoss`            | `double`                     | New Stop Loss level (for positions and pending orders)     |
| `TakeProfit`          | `double`                     | New Take Profit level (for positions and pending orders)   |
| `Price`               | `double`                     | New price (ONLY for pending orders)                        |
| `ExpirationTimeType`  | `TMT5_ENUM_ORDER_TYPE_TIME`  | Expiration type (ONLY for pending orders)                  |
| `ExpirationTime`      | `Timestamp`                  | Expiration time (ONLY for pending orders)                  |
| `StopLimit`           | `double`                     | New stop limit (ONLY for pending StopLimit orders)         |

---

## ⬆️ Output — `OrderModifyData`

| Field                      | Type     | Description                                        |
| -------------------------- | -------- | -------------------------------------------------- |
| `ReturnedCode`             | `uint`   | Operation return code (10009 = success)            |
| `Deal`                     | `ulong`  | Deal ticket if executed                            |
| `Order`                    | `ulong`  | Order ticket                                       |
| `Volume`                   | `double` | Confirmed volume                                   |
| `Price`                    | `double` | Confirmed price                                    |
| `Bid`                      | `double` | Current Bid price                                  |
| `Ask`                      | `double` | Current Ask price                                  |
| `Comment`                  | `string` | Broker comment (error description)                 |
| `RequestId`                | `uint`   | Request ID from terminal                           |
| `RetCodeExternal`          | `int`    | External trading system return code                |
| `ReturnedStringCode`       | `string` | String representation of return code               |
| `ReturnedCodeDescription`  | `string` | Human-readable description                         |

---

## 🧱 Related enums (from proto)

### `TMT5_ENUM_ORDER_TYPE_TIME`

* `Tmt5OrderTimeGtc` — Good‑Till‑Cancelled
* `Tmt5OrderTimeDay` — Good till current trade day
* `Tmt5OrderTimeSpecified` — Good till expiration time
* `Tmt5OrderTimeSpecifiedDay` — Good till 23:59:59 of specified day

---

## 💬 Just the essentials

* **What it is.** Modifies SL/TP of open positions or changes parameters of pending orders (price, expiration, stop limit).
* **Why you need it.** Adjust risk levels, move to breakeven, update pending order prices.
* **Sanity check.** If `ReturnedCode == 10009` → modification successful.

---

## 🎯 Purpose

Use it to update trade parameters:

* Modify SL/TP of open positions.
* Change pending order price/expiration.
* Move stops to breakeven.
* Adjust trailing stops manually.

---

## 🧩 Notes & Tips

* **Return codes:** 10009 = success. Other codes indicate errors (invalid stops, market closed, etc.).
* **Position vs Pending:** Use same `Ticket` for both positions and pending orders. Server determines type automatically.
* **Stop levels:** Broker enforces minimum distance from current price (`SYMBOL_TRADE_STOPS_LEVEL`). Check before modifying.
* **Partial parameters:** Set only fields you want to change. Leave others as `0` or `null`.
* **Price field:** Only for pending orders. Ignored for open positions.

---

## 🔗 Usage Examples

### 1) Modify SL/TP of open position

```csharp
// acc — connected MT5Account
// positionTicket — ticket from OrderSendAsync or OpenedOrdersAsync

var result = await acc.OrderModifyAsync(new OrderModifyRequest
{
    Ticket = positionTicket,
    StopLoss = 1.1050,
    TakeProfit = 1.1150
});

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✓ Position modified: SL={result.Price}");
}
else
{
    Console.WriteLine($"✗ Modification failed: {result.ReturnedCodeDescription}");
}
```

---

### 2) Move position to breakeven

```csharp
// Get current position
var positions = await acc.OpenedOrdersAsync();
var position = positions.Positions.First();

// Get entry price from position
var entryPrice = position.PriceOpen;

var result = await acc.OrderModifyAsync(new OrderModifyRequest
{
    Ticket = (ulong)position.Ticket,
    StopLoss = entryPrice,  // Move SL to entry price
    TakeProfit = position.TakeProfit  // Keep existing TP
});

Console.WriteLine($"Moved to breakeven: {result.ReturnedCodeDescription}");
```

---

### 3) Remove SL/TP (set to zero)

```csharp
var result = await acc.OrderModifyAsync(new OrderModifyRequest
{
    Ticket = positionTicket,
    StopLoss = 0,    // Remove SL
    TakeProfit = 0   // Remove TP
});

Console.WriteLine($"Removed SL/TP: {result.ReturnedCodeDescription}");
```

---

### 4) Modify pending order price

```csharp
// pendingOrderTicket — ticket of pending Buy Limit order

var tick = await acc.SymbolInfoTickAsync("EURUSD");
var newPrice = tick.Bid - 0.0020; // 20 pips below current

var result = await acc.OrderModifyAsync(new OrderModifyRequest
{
    Ticket = pendingOrderTicket,
    Price = newPrice,
    StopLoss = newPrice - 0.0010,
    TakeProfit = newPrice + 0.0030
});

Console.WriteLine($"Pending order updated: {result.ReturnedCodeDescription}");
```

---

### 5) Change pending order expiration

```csharp
var result = await acc.OrderModifyAsync(new OrderModifyRequest
{
    Ticket = pendingOrderTicket,
    ExpirationTimeType = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecified,
    ExpirationTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(
        DateTime.UtcNow.AddDays(1))
});

Console.WriteLine($"Order expires in 1 day: {result.ReturnedCodeDescription}");
```

---

### 6) Trailing stop example (manual)

```csharp
// Simple trailing stop: if position in profit >= 30 pips, move SL to +10 pips
var positions = await acc.OpenedOrdersAsync();
var position = positions.Positions.FirstOrDefault(p => p.Symbol == "EURUSD");

if (position != null)
{
    var currentProfit = position.Profit;
    var currentPrice = position.Type == 0 ? position.PriceCurrent : position.PriceCurrent; // Bid for Buy, Ask for Sell
    var entryPrice = position.PriceOpen;

    var profitPips = Math.Abs(currentPrice - entryPrice) * 10000;

    if (profitPips >= 30)
    {
        var newSL = position.Type == 0
            ? entryPrice + 0.0010  // Buy: SL 10 pips above entry
            : entryPrice - 0.0010; // Sell: SL 10 pips below entry

        var result = await acc.OrderModifyAsync(new OrderModifyRequest
        {
            Ticket = (ulong)position.Ticket,
            StopLoss = newSL,
            TakeProfit = position.TakeProfit
        });

        Console.WriteLine($"Trailing stop updated: {result.ReturnedCodeDescription}");
    }
}
```
