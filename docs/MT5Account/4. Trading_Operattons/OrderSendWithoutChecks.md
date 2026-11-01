# ✅ Fast Order Send (`OrderSendWithoutChecksAsync`)

> **Request:** Send a market or pending order on **MT5** **without** the server‑side pre‑checks that `OrderSend` performs. Faster path — the caller is responsible for the request being valid.

**API Information:**

* **SDK wrapper:** `MT5Account.OrderSendWithoutChecksAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.TradingHelper`
* **Proto definition:** `OrderSendWithoutChecks` (defined in `mt5-term-api-trading-helper.proto`)

### RPC

* **Service:** `mt5_term_api.TradingHelper`
* **Method:** `OrderSendWithoutChecks(OrderSendRequest) → OrderSendReply`
* **Low‑level client (generated):** `TradingHelper.TradingHelperClient.OrderSendWithoutChecks(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<OrderSendData> OrderSendWithoutChecksAsync(
            OrderSendRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`OrderSendRequest { symbol, operation, volume, price, stop_loss, take_profit, slippage, comment, … }` — same message as [`OrderSend`](./OrderSend.md).


**Reply message:**

`OrderSendReply { data: OrderSendData }`

---

## 🔽 Input

| Parameter           | Type                | Description                                               |
| ------------------- | ------------------- | --------------------------------------------------------- |
| `request`           | `OrderSendRequest`  | Same request object as `OrderSend` (symbol, type, volume…)|
| `deadline`          | `DateTime?`         | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | Cooperative cancel for the call/retry loop                |

> See [`OrderSend`](./OrderSend.md) for the full `OrderSendRequest` field table.

---

## ⬆️ Output — `OrderSendData`

| Field                    | Type     | Description                                 |
| ------------------------ | -------- | ------------------------------------------- |
| `ReturnedCode`           | `uint`   | Trade return code (`10009` = done)          |
| `ReturnedCodeDescription`| `string` | Human‑readable return code                  |
| `Order`                  | `ulong`  | Resulting order ticket                      |
| `Deal`                   | `ulong`  | Resulting deal ticket                       |
| `Volume`                 | `double` | Executed volume                             |
| `Price`                  | `double` | Execution price                             |

> Same payload as [`OrderSend`](./OrderSend.md) — only the server‑side validation step differs.

---

## 🧱 Related enums (from proto)

The request is the same `OrderSendRequest` as [`OrderSend`](./OrderSend.md), so it uses the same enums.

### `TMT5_ENUM_ORDER_TYPE` (field `Operation`)

* `Tmt5OrderTypeBuy` — Market Buy order
* `Tmt5OrderTypeSell` — Market Sell order
* `Tmt5OrderTypeBuyLimit` — Buy Limit pending order
* `Tmt5OrderTypeSellLimit` — Sell Limit pending order
* `Tmt5OrderTypeBuyStop` — Buy Stop pending order
* `Tmt5OrderTypeSellStop` — Sell Stop pending order
* `Tmt5OrderTypeBuyStopLimit` — Buy Stop Limit order
* `Tmt5OrderTypeSellStopLimit` — Sell Stop Limit order
* `Tmt5OrderTypeCloseBy` — Close position by opposite one

> For `ExpirationTimeType` (`TMT5_ENUM_ORDER_TYPE_TIME`) and the filling mode, see the full enum tables on [OrderSend](./OrderSend.md).

---

## 💬 Just the essentials

* **What it is.** `OrderSend` minus the server pre‑validation step — lower latency.
* **Why you need it.** Latency‑sensitive flows where inputs are already trusted/validated client‑side.
* **Trade‑off.** No safety net: a malformed request fails at execution instead of being caught early.

---

## 🎯 Purpose

Use it when you have already validated the request yourself and every millisecond counts:

* High‑frequency / scalping loops that pre‑normalize volume and price.
* Re‑sending an order you already checked once with [`OrderCheck`](./OrderCheck.md).

For everyday trading prefer [`OrderSend`](./OrderSend.md) — the extra checks protect you from rejects.

---

## 🧩 Notes & Tips

* **You own validation:** Normalize volume (min/max/step) and respect stops level **before** calling.
* **Same return codes:** You still get `ReturnedCode` — `10009` means done; `10018` market closed, etc.
* **Market vs pending:** Set `Operation`; for a market order leave `Price` at the current Ask/Bid.
* **Pair with [`OrderCheck`](./OrderCheck.md):** Check once, then fast‑send repeatedly.

---

## 🔗 Usage Examples

### 1) Fast market BUY (inputs already normalized)

```csharp
// acc — connected MT5Account; volume/price validated beforehand
var tick = await acc.SymbolInfoTickAsync("EURUSD");

var result = await acc.OrderSendWithoutChecksAsync(new OrderSendRequest
{
    Symbol    = "EURUSD",
    Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
    Volume    = 0.01,
    Price     = tick.Ask,
    Comment   = "fast-send"
});

Console.WriteLine($"{result.ReturnedCode} ({result.ReturnedCodeDescription}) order #{result.Order}");
```

---

### 2) Check once, then fast‑send

```csharp
// Validate the request the first time…
var check = await acc.OrderCheckAsync(new OrderCheckRequest { /* … */ });
if (check.MqlTradeCheckResult.ReturnedCode == 10009)
{
    // …then reuse the fast path for subsequent identical sends
    var sent = await acc.OrderSendWithoutChecksAsync(new OrderSendRequest { /* same params */ });
    Console.WriteLine($"Sent: {sent.ReturnedCode}");
}
```
