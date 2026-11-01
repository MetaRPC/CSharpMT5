# ✅ Place Market Order — Fast Path (`PlaceMarketFast`)

> **Sugar method:** Same shape as `PlaceMarket`, but sends via the no‑pre‑checks fast path (`OrderSendWithoutChecks`). Lower latency; the caller is responsible for valid inputs.

**API Information:**

* **Extension method:** `MT5Service.PlaceMarketFast(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Underlying calls:** `EnsureSelected()` + `OrderSendWithoutChecksAsync()`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static Task<OrderSendData> PlaceMarketFast(
        this MT5Service svc,
        string symbol, double volume, bool isBuy,
        double? sl = null, double? tp = null,
        string? comment = null, int deviationPoints = 0,
        int timeoutSec = 15, CancellationToken ct = default);
}
```

---

## 🔽 Input

| Parameter         | Type                | Description                                  |
| ----------------- | ------------------- | -------------------------------------------- |
| `svc`             | `MT5Service`        | MT5Service instance (extension method)       |
| `symbol`          | `string`            | Symbol to trade (auto‑selected first)        |
| `volume`          | `double`            | Volume in lots                               |
| `isBuy`           | `bool`              | `true` = BUY, `false` = SELL                 |
| `sl` / `tp`       | `double?`           | Optional stop‑loss / take‑profit prices      |
| `comment`         | `string?`           | Optional order comment                       |
| `deviationPoints` | `int`               | Max price deviation (slippage) in points     |
| `timeoutSec`      | `int`               | Timeout in seconds (default: 15)             |
| `ct`              | `CancellationToken` | Cancellation token                           |

---

## ⬆️ Output — `OrderSendData`

| Field | Type | Description |
| ----- | ---- | ----------- |
| `ReturnedCode` | `uint` | Trade return code (`10009` = done) |
| `ReturnedCodeDescription` | `string` | Human‑readable return code |
| `Order` / `Deal` | `ulong` | Resulting order / deal tickets |
| `Price` | `double` | Execution price |

---

## 💬 Just the essentials

* **What it is.** `PlaceMarket` without the server‑side validation step.
* **Why you need it.** Latency‑sensitive flows where inputs are already trusted/normalized.
* **Trade‑off.** No safety net — a malformed request fails at execution instead of being caught early.

---

## 🎯 Purpose

* Scalping / HFT loops that pre‑normalize volume and price.
* Re‑sending an order you already validated once.

For everyday trading prefer `PlaceMarket` — the extra checks protect against rejects.

---

## 🧩 Notes & Tips

* **You own validation** — normalize volume (min/max/step) and respect stops level before calling.
* **Same return codes** — check `ReturnedCode`; `10009` = done, `10018` = market closed, etc.
* **Operation** — set via `isBuy` (reflection sets BUY=0/SELL=1 internally, as in `PlaceMarket`).

---

## 🔧 Under the Hood

```csharp
await svc.EnsureSelected(symbol, timeoutSec, ct);
var req = new OrderSendRequest { Symbol = symbol, Volume = volume, StopLoss = sl ?? 0, TakeProfit = tp ?? 0, /* … */ };
// Operation set via reflection (BUY=0, SELL=1)
return await svc.OrderSendWithoutChecksAsync(req, Dl(timeoutSec), ct);
```

---

## 🔗 Usage Examples

### 1) Fast market BUY, then close

```csharp
// svc — MT5Service instance; volume already normalized
var r = await svc.PlaceMarketFast("EURUSD", 0.01, isBuy: true, comment: "fast");
Console.WriteLine($"{r.ReturnedCode} ({r.ReturnedCodeDescription}) #{r.Order}");

if (r.ReturnedCode == 10009 && r.Order > 0)
    await svc.CloseByTicket(r.Order);
```

### 2) With SL/TP

```csharp
var snap = await svc.GetSymbolSnapshot("EURUSD");
var r = await svc.PlaceMarketFast("EURUSD", 0.02, isBuy: true,
    sl: snap.Tick.Ask - 100 * snap.Point,
    tp: snap.Tick.Ask + 200 * snap.Point);
```

---

## 🔗 Related Methods

* **Low-level:** [`OrderSendWithoutChecksAsync()`](../../MT5Account/4.%20Trading_Operattons/OrderSendWithoutChecks.md)
* **Sugar:** `PlaceMarket()` (validated), `PlacePending()`, `CloseByTicket()`
