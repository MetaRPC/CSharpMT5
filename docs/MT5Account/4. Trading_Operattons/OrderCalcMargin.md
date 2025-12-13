# ✅ Calculating Required Margin (`OrderCalcMarginAsync`)

> **Request:** Calculate the margin required to open a trade with given parameters on **MT5**. Returns required margin in account currency.

**API Information:**

* **SDK wrapper:** `MT5Account.OrderCalcMarginAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.TradeFunctions`
* **Proto definition:** `OrderCalcMargin` (defined in `mt5-term-api-trade-functions.proto`)

### RPC

* **Service:** `mt5_term_api.TradeFunctions`
* **Method:** `OrderCalcMargin(OrderCalcMarginRequest) → OrderCalcMarginReply`
* **Low‑level client (generated):** `TradeFunctions.TradeFunctionsClient.OrderCalcMargin(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<OrderCalcMarginData> OrderCalcMarginAsync(
            OrderCalcMarginRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`OrderCalcMarginRequest { symbol, order_type, volume, open_price }`


**Reply message:**

`OrderCalcMarginReply { data: OrderCalcMarginData }`

---

## 🔽 Input

| Parameter           | Type                     | Description                                               |
| ------------------- | ------------------------ | --------------------------------------------------------- |
| `request`           | `OrderCalcMarginRequest` | Protobuf request with symbol, order type, volume, price  |
| `deadline`          | `DateTime?`              | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`      | Cooperative cancel for the call/retry loop                |

### `OrderCalcMarginRequest`

| Field       | Type                 | Description                                          |
| ----------- | -------------------- | ---------------------------------------------------- |
| `Symbol`    | `string`             | Trading symbol (`"EURUSD"`, `"XAUUSD"`, etc.)        |
| `OrderType` | `ENUM_ORDER_TYPE_TF` | Order type (Buy, Sell, BuyLimit, SellStop, etc.)     |
| `Volume`    | `double`             | Trade volume in lots (respect symbol min/max/step)   |
| `OpenPrice` | `double`             | Planned open price (use current Bid/Ask for market)  |

---

## ⬆️ Output — `OrderCalcMarginData`

| Field    | Type     | Description                             |
| -------- | -------- | --------------------------------------- |
| `Margin` | `double` | Required margin in **account currency** |

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

---

## 💬 Just the essentials

* **What it is.** Calculates required margin for a planned trade with given symbol, order type, volume and price.
* **Why you need it.** Validate free margin before sending orders; implement dynamic position sizing based on risk %.
* **Sanity check.** If `Margin` ≤ `AccountFreeMargin` → trade is feasible.

---

## 🎯 Purpose

Use it to calculate margin requirements:

* Check if planned lot size is feasible.
* Implement risk‑based lot sizing (e.g. "2% of balance per trade").
* Pre‑compute margin impact for basket trades.

---

## 🧩 Notes & Tips

* **Server calculation:** Margin depends on leverage, contract size, account currency, broker settings. Let MT5 server calculate it.
* **Market orders:** Use current Bid/Ask as `OpenPrice` (Buy → ask, Sell → bid).
* **Same parameters:** Pass same `OrderType`/`Volume`/`OpenPrice` that you'll use in `OrderSendAsync`.
* **Combine with OrderCheck:** Use `OrderCalcMargin` for margin impact, `OrderCheck` for full trade validation.

---

## 🔗 Usage Examples

### 1) Basic margin calculation for a market BUY

```csharp
// acc — connected MT5Account

// Get current tick for default symbol
var tick = await acc.SymbolInfoTickAsync(Constants.DefaultSymbol);

var margin = await acc.OrderCalcMarginAsync(new OrderCalcMarginRequest
{
    Symbol    = Constants.DefaultSymbol,
    OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
    Volume    = Constants.DefaultVolume,
    OpenPrice = tick.Ask
});

Console.WriteLine($"Required margin: {margin.Margin:F2}");
```

---

### 2) Check if we have enough free margin for a planned trade

```csharp
async Task<bool> HasEnoughMarginAsync(MT5Account acc, string symbol, double lots)
{
    var tick = await acc.SymbolInfoTickAsync(symbol);

    var margin = await acc.OrderCalcMarginAsync(new OrderCalcMarginRequest
    {
        Symbol    = symbol,
        OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
        Volume    = lots,
        OpenPrice = tick.Ask
    });

    var accountInfo = await acc.AccountSummaryAsync();
    var freeMargin  = accountInfo.FreeMargin;

    Console.WriteLine($"Free margin: {freeMargin:F2}, required: {margin.Margin:F2}");

    return freeMargin >= margin.Margin;
}
```

---

### 3) Simple risk‑based lot sizing

```csharp
// Example: risk up to 2% of balance on this trade

async Task<double> CalculateLotsByRiskAsync(MT5Account acc, string symbol, double stopLossPoints, double riskPercent)
{
    var account = await acc.AccountSummaryAsync();
    var balance = account.Balance;

    var tick = await acc.SymbolInfoTickAsync(symbol);

    // Start with 0.01 lot and scale up while margin is acceptable
    double lotStep = 0.01;
    double lots    = lotStep;

    while (true)
    {
        var marginData = await acc.OrderCalcMarginAsync(new OrderCalcMarginRequest
        {
            Symbol    = symbol,
            OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
            Volume    = lots,
            OpenPrice = tick.Ask
        });

        // Simple cap: margin must not exceed (riskPercent of balance * 10) for example
        if (marginData.Margin > balance * (riskPercent / 100.0) * 10.0)
            break;

        lots += lotStep;
    }

    // Round down to broker’s volume step in production code
    return Math.Max(lotStep, lots - lotStep);
}
```
