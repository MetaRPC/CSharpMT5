# ✅ Sending Trade Orders (`OrderSendAsync`)

> **Request:** Send a market or pending order to **MT5**. Opens positions, places pending orders with specified parameters.

**API Information:**

* **SDK wrapper:** `MT5Account.OrderSendAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.TradingHelper`
* **Proto definition:** `OrderSend` (defined in `mt5-term-api-trading-helper.proto`)

### RPC

* **Service:** `mt5_term_api.TradingHelper`
* **Method:** `OrderSend(OrderSendRequest) → OrderSendReply`
* **Low‑level client (generated):** `TradingHelper.TradingHelperClient.OrderSend(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace mt5_term_api
{
    public class MT5Account
    {
        public async Task<OrderSendData> OrderSendAsync(
            OrderSendRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`OrderSendRequest { symbol, operation, volume, ... }`


**Reply message:**

`OrderSendReply { data: OrderSendData }`

---

## 🔽 Input

| Parameter           | Type               | Description                                               |
| ------------------- | ------------------ | --------------------------------------------------------- |
| `request`           | `OrderSendRequest` | Protobuf request with trade parameters                    |
| `deadline`          | `DateTime?`        | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | Cooperative cancel for the call/retry loop                |

### `OrderSendRequest`

| Field                 | Type                         | Description                                               |
| --------------------- | ---------------------------- | --------------------------------------------------------- |
| `Symbol`              | `string`                     | Trading symbol (`"EURUSD"`, `"XAUUSD"`, etc.) **REQUIRED** |
| `Operation`           | `TMT5_ENUM_ORDER_TYPE`       | Order type: Buy, Sell, BuyLimit, etc. **REQUIRED**         |
| `Volume`              | `double`                     | Volume in lots (respect symbol min/max/step) **REQUIRED** |
| `Price`               | `double`                     | Price (for pending orders; use current Bid/Ask for market) |
| `Slippage`            | `ulong`                      | Max price deviation in points (optional)                   |
| `StopLoss`            | `double`                     | Stop Loss level (optional)                                 |
| `TakeProfit`          | `double`                     | Take Profit level (optional)                               |
| `Comment`             | `string`                     | Order comment (optional)                                   |
| `ExpertId`            | `ulong`                      | EA magic number (use `0` for manual trades)                |
| `StopLimitPrice`      | `double`                     | StopLimit price (for StopLimit orders)                     |
| `ExpirationTimeType`  | `TMT5_ENUM_ORDER_TYPE_TIME`  | Order expiration type (GTC, Day, Specified)                |
| `ExpirationTime`      | `Timestamp`                  | Expiration time (for TIME_SPECIFIED orders)                |

---

## ⬆️ Output — `OrderSendData`

| Field                      | Type     | Description                                        |
| -------------------------- | -------- | -------------------------------------------------- |
| `ReturnedCode`             | `uint`   | Operation return code (10009 = success)            |
| `Deal`                     | `ulong`  | Deal ticket if executed                            |
| `Order`                    | `ulong`  | Order ticket if placed (pending orders)            |
| `Volume`                   | `double` | Confirmed deal volume                              |
| `Price`                    | `double` | Confirmed deal price                               |
| `Bid`                      | `double` | Current Bid price at execution                     |
| `Ask`                      | `double` | Current Ask price at execution                     |
| `Comment`                  | `string` | Broker comment (error description)                 |
| `RequestId`                | `uint`   | Request ID from terminal                           |
| `RetCodeExternal`          | `int`    | External trading system return code                |
| `ReturnedStringCode`       | `string` | String representation of return code               |
| `ReturnedCodeDescription`  | `string` | Human-readable description                         |

---

## 🧱 Related enums (from proto)

### `TMT5_ENUM_ORDER_TYPE`

* `Tmt5OrderTypeBuy` — Market Buy order
* `Tmt5OrderTypeSell` — Market Sell order
* `Tmt5OrderTypeBuyLimit` — Buy Limit pending order
* `Tmt5OrderTypeSellLimit` — Sell Limit pending order
* `Tmt5OrderTypeBuyStop` — Buy Stop pending order
* `Tmt5OrderTypeSellStop` — Sell Stop pending order
* `Tmt5OrderTypeBuyStopLimit` — Buy Stop Limit order
* `Tmt5OrderTypeSellStopLimit` — Sell Stop Limit order
* `Tmt5OrderTypeCloseBy` — Close position by opposite one

### `TMT5_ENUM_ORDER_TYPE_TIME`

* `Tmt5OrderTimeGtc` — Good‑Till‑Cancelled
* `Tmt5OrderTimeDay` — Good till current trade day
* `Tmt5OrderTimeSpecified` — Good till expiration time
* `Tmt5OrderTimeSpecifiedDay` — Good till 23:59:59 of specified day

---

## 💬 Just the essentials

* **What it is.** Sends trade orders to MT5 server. Opens market positions or places pending orders with specified parameters.
* **Why you need it.** Main method for executing trades. All trading operations start here.
* **Sanity check.** If `ReturnedCode == 10009` → order executed successfully. Check `Deal` or `Order` ticket.

---

## 🎯 Purpose

Use it to execute trades:

* Open market positions (Buy/Sell).
* Place pending orders (Limit/Stop/StopLimit).
* Set initial SL/TP levels.

---

## 🧩 Notes & Tips

* **Return codes:** 10009 = success. Other codes indicate errors (insufficient margin, invalid price, etc.).
* **Market orders:** Don't set `Price` — server uses current Bid/Ask automatically.
* **Pending orders:** Must specify `Price`. For StopLimit orders also set `StopLimitPrice`.
* **Slippage:** For market orders, allows price deviation. Broker may reject if market moved too much.
* **Volume limits:** Always check `SymbolVolumeMin/Max/Step` before calling.
* **Magic number:** Use unique `ExpertId` per strategy for tracking.

---

## 🔗 Usage Examples

### 1) Basic market BUY order

```csharp
// acc — connected MT5Account

var result = await acc.OrderSendAsync(new OrderSendRequest
{
    Symbol = "EURUSD",
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
    Volume = 0.01
});

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✓ Position opened: Deal #{result.Deal}, Price: {result.Price}");
}
else
{
    Console.WriteLine($"✗ Order failed: {result.ReturnedCodeDescription}");
}
```

---

### 2) Market BUY with SL/TP

```csharp
var tick = await acc.SymbolInfoTickAsync("EURUSD");

var result = await acc.OrderSendAsync(new OrderSendRequest
{
    Symbol = "EURUSD",
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
    Volume = 0.01,
    StopLoss = tick.Ask - 0.0010,    // 10 pips below
    TakeProfit = tick.Ask + 0.0020,  // 20 pips above
    Comment = "Buy with SL/TP"
});

Console.WriteLine($"Order: {result.ReturnedCodeDescription}");
```

---

### 3) Pending Buy Limit order

```csharp
var tick = await acc.SymbolInfoTickAsync("EURUSD");
var limitPrice = tick.Bid - 0.0010; // 10 pips below current price

var result = await acc.OrderSendAsync(new OrderSendRequest
{
    Symbol = "EURUSD",
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyLimit,
    Volume = 0.01,
    Price = limitPrice,
    ExpirationTimeType = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeGtc,
    Comment = "Buy Limit pending"
});

if (result.ReturnedCode == 10009)
{
    Console.WriteLine($"✓ Pending order placed: Order #{result.Order}");
}
```

---

### 4) Buy Stop Limit order

```csharp
var tick = await acc.SymbolInfoTickAsync("XAUUSD");

var result = await acc.OrderSendAsync(new OrderSendRequest
{
    Symbol = "XAUUSD",
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyStopLimit,
    Volume = 0.01,
    Price = tick.Ask + 5.0,          // Activation price (stop)
    StopLimitPrice = tick.Ask + 4.0, // Limit order price after activation
    StopLoss = tick.Ask + 3.0,
    TakeProfit = tick.Ask + 10.0,
    ExpirationTimeType = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeGtc
});

Console.WriteLine($"StopLimit order: {result.ReturnedCodeDescription}");
```

---

### 5) With EA magic number and expiration

```csharp
var result = await acc.OrderSendAsync(new OrderSendRequest
{
    Symbol = "GBPUSD",
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellLimit,
    Volume = 0.01,
    Price = 1.2550,
    ExpertId = 202501,  // EA magic number
    ExpirationTimeType = TMT5_ENUM_ORDER_TYPE_TIME.Tmt5OrderTimeSpecified,
    ExpirationTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(
        DateTime.UtcNow.AddHours(4))
});

Console.WriteLine($"Order expires in 4 hours. Ticket: {result.Order}");
```
