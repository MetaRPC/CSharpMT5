# ✅ Calculate Expected Profit (`CalcProfit`)

> **Sugar method:** Estimates the profit of a trade between two prices and returns a plain `double`. No order is placed.

**API Information:**

* **Extension method:** `MT5Service.CalcProfit(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Underlying calls:** `EnsureSelected()` + `OrderCalcProfitAsync()`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static Task<double> CalcProfit(
        this MT5Service svc,
        string symbol, bool isBuy, double volume,
        double openPrice, double closePrice,
        int timeoutSec = 15, CancellationToken ct = default);
}
```

---

## 🔽 Input

| Parameter    | Type                | Description                              |
| ------------ | ------------------- | ---------------------------------------- |
| `svc`        | `MT5Service`        | MT5Service instance (extension method)   |
| `symbol`     | `string`            | Symbol (auto‑selected first)             |
| `isBuy`      | `bool`              | `true` = BUY, `false` = SELL             |
| `volume`     | `double`            | Volume in lots                           |
| `openPrice`  | `double`            | Hypothetical entry price                 |
| `closePrice` | `double`            | Hypothetical exit price                  |
| `timeoutSec` | `int`               | Timeout in seconds (default: 15)         |
| `ct`         | `CancellationToken` | Cancellation token                       |

---

## ⬆️ Output — `double`

Expected profit/loss in **account currency** (the unwrapped `OrderCalcProfitData.Profit`).

---

## 💬 Just the essentials

* **What it is.** Turns a price distance into money, returned as a plain `double`.
* **Why you need it.** Preview reward before trading; size TP/SL in money terms.
* **Pairs with.** `CalculateBuyMarginAsync` / `CalculateSellMarginAsync` (cost side) for risk/reward.

---

## 🎯 Purpose

* Show expected P/L for a planned take‑profit level.
* Compare exit scenarios before sending.
* Build risk/reward ratios alongside margin helpers.

---

## 🧩 Notes & Tips

* **Direction matters** — for a BUY, profit is positive when `closePrice > openPrice`; reversed for SELL.
* **Market entry** — use current Ask (BUY) / Bid (SELL) as `openPrice`.
* **Server‑side** — the server accounts for contract size, tick value and account currency.

---

## 🔧 Under the Hood

```csharp
await svc.EnsureSelected(symbol, timeoutSec, ct);
var data = await svc.OrderCalcProfitAsync(new OrderCalcProfitRequest
{
    Symbol = symbol,
    OrderType = isBuy ? ENUM_ORDER_TYPE_TF.OrderTypeTfBuy : ENUM_ORDER_TYPE_TF.OrderTypeTfSell,
    Volume = volume, OpenPrice = openPrice, ClosePrice = closePrice
}, Dl(timeoutSec), ct);
return data.Profit;
```

---

## 🔗 Usage Examples

### 1) Expected profit of a +100‑point BUY exit

```csharp
// svc — MT5Service instance
var snap = await svc.GetSymbolSnapshot("EURUSD");
double pnl = await svc.CalcProfit("EURUSD", isBuy: true, volume: 0.10,
    openPrice: snap.Tick.Ask,
    closePrice: snap.Tick.Ask + 100 * snap.Point);
Console.WriteLine($"Expected profit: {pnl:F2}");
```

### 2) Risk/reward ratio

```csharp
var snap = await svc.GetSymbolSnapshot("EURUSD");
double entry = snap.Tick.Ask;
double reward = await svc.CalcProfit("EURUSD", true, 0.10, entry, entry + 200 * snap.Point);
double risk   = Math.Abs(await svc.CalcProfit("EURUSD", true, 0.10, entry, entry - 100 * snap.Point));
Console.WriteLine($"R:R = {(risk > 0 ? reward / risk : 0):F2}");
```

---

## 🔗 Related Methods

* **Low-level:** [`OrderCalcProfitAsync()`](../../MT5Account/4.%20Trading_Operattons/OrderCalcProfit.md)
* **Sugar:** `CalculateBuyMarginAsync()`, `CalculateSellMarginAsync()`, `ValidateOrderAsync()`
