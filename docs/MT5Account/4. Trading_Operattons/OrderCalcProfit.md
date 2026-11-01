# ✅ Calculating Expected Profit (`OrderCalcProfitAsync`)

> **Request:** Calculate the profit a trade would make between an open and a close price on **MT5**. Returns the profit in account currency.

**API Information:**

* **SDK wrapper:** `MT5Account.OrderCalcProfitAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.TradeFunctions`
* **Proto definition:** `OrderCalcProfit` (defined in `mt5-term-api-trade-functions.proto`)

### RPC

* **Service:** `mt5_term_api.TradeFunctions`
* **Method:** `OrderCalcProfit(OrderCalcProfitRequest) → OrderCalcProfitReply`
* **Low‑level client (generated):** `TradeFunctions.TradeFunctionsClient.OrderCalcProfit(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<OrderCalcProfitData> OrderCalcProfitAsync(
            OrderCalcProfitRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`OrderCalcProfitRequest { order_type, symbol, volume, open_price, close_price }`


**Reply message:**

`OrderCalcProfitReply { data: OrderCalcProfitData }`

---

## 🔽 Input

| Parameter           | Type                     | Description                                               |
| ------------------- | ------------------------ | --------------------------------------------------------- |
| `request`           | `OrderCalcProfitRequest` | Protobuf request with symbol, order type, volume, prices  |
| `deadline`          | `DateTime?`              | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`      | Cooperative cancel for the call/retry loop                |

### `OrderCalcProfitRequest`

| Field        | Type                 | Description                                        |
| ------------ | -------------------- | -------------------------------------------------- |
| `Symbol`     | `string`             | Trading symbol (`"EURUSD"`, `"XAUUSD"`, etc.)      |
| `OrderType`  | `ENUM_ORDER_TYPE_TF` | Direction of the hypothetical trade (Buy/Sell/…)   |
| `Volume`     | `double`             | Trade volume in lots                               |
| `OpenPrice`  | `double`             | Hypothetical entry price                           |
| `ClosePrice` | `double`             | Hypothetical exit price                            |

---

## ⬆️ Output — `OrderCalcProfitData`

| Field    | Type     | Description                                 |
| -------- | -------- | ------------------------------------------- |
| `Profit` | `double` | Expected profit/loss in **account currency** |

---

## 🧱 Related enums (from proto)

### `ENUM_ORDER_TYPE_TF`

* `OrderTypeTfBuy` — Market Buy order
* `OrderTypeTfSell` — Market Sell order
* `OrderTypeTfBuyLimit` — Buy Limit pending order
* `OrderTypeTfSellLimit` — Sell Limit pending order
* `OrderTypeTfBuyStop` — Buy Stop pending order
* `OrderTypeTfSellStop` — Sell Stop pending order

---

## 💬 Just the essentials

* **What it is.** Asks the server what profit a trade would yield between two prices.
* **Why you need it.** Preview reward before trading; size take‑profit/stop‑loss in money terms.
* **Pairs with.** [`OrderCalcMargin`](./OrderCalcMargin.md) (cost side) — profit here, margin there.

---

## 🎯 Purpose

Use it to turn price distance into money:

* Show expected P/L for a planned TP level.
* Compare scenarios (different volumes / exit prices) before sending.
* Build risk/reward ratios alongside `OrderCalcMargin`.

---

## 🧩 Notes & Tips

* **Server calculation:** Profit depends on contract size, tick value and account currency — let the server compute it.
* **Direction matters:** For a Buy, profit is positive when `ClosePrice > OpenPrice`; reversed for a Sell.
* **Market prices:** For a market entry use current Ask (Buy) / Bid (Sell) as `OpenPrice`.

---

## 🔗 Usage Examples

### 1) Expected profit of a +100‑point take‑profit on a BUY

```csharp
// acc — connected MT5Account
var tick  = await acc.SymbolInfoTickAsync("EURUSD");
var point = (await acc.SymbolInfoDoubleAsync("EURUSD", SymbolInfoDoubleProperty.SymbolPoint)).Value;

var calc = await acc.OrderCalcProfitAsync(new OrderCalcProfitRequest
{
    Symbol     = "EURUSD",
    OrderType  = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
    Volume     = 0.10,
    OpenPrice  = tick.Ask,
    ClosePrice = tick.Ask + (100 * point)
});

Console.WriteLine($"Expected profit: {calc.Profit:F2}");
```

---

### 2) Risk/reward from margin + profit

```csharp
async Task<double> RiskRewardAsync(MT5Account acc, string symbol, double lots, double entry, double sl, double tp)
{
    var reward = (await acc.OrderCalcProfitAsync(new OrderCalcProfitRequest
    {
        Symbol = symbol, OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
        Volume = lots, OpenPrice = entry, ClosePrice = tp
    })).Profit;

    var risk = Math.Abs((await acc.OrderCalcProfitAsync(new OrderCalcProfitRequest
    {
        Symbol = symbol, OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
        Volume = lots, OpenPrice = entry, ClosePrice = sl
    })).Profit);

    return risk > 0 ? reward / risk : 0;
}
```
