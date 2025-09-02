# Reverse (`reverse`) & Reverse by Ticket (`reverse.ticket`) 🔄

## What they do

* **`reverse`** — reverses positions by symbol: calculates current **net exposure**, then either sends one opposite order (net) or closes all & reopens one (flat).
* **`reverse.ticket`** — reverses a **single** position by its ticket.

Aliases: `reverse` → `rv`, `reverse.ticket` → `rvt`.

---

## Input Parameters ⬇️

### `reverse`

| Parameter       | Type   | Required | Description                                       |
| --------------- | ------ | -------- | ------------------------------------------------- |
| `--profile, -p` | string | yes      | Profile from `profiles.json`.                     |
| `--symbol, -s`  | string | no       | Target symbol (defaults to app `DefaultSymbol`).  |
| `--mode`        | string | no       | `net` (default) or `flat`.                        |
| `--sl`          | double | no       | Stop Loss for the **new** position.               |
| `--tp`          | double | no       | Take Profit for the **new** position.             |
| `--deviation`   | int    | no       | Slippage tolerance (points), default `10`.        |
| `--timeout-ms`  | int    | no       | Per‑RPC timeout (default `30000`).                |
| `--dry-run`     | flag   | no       | Print action plan **without** sending any orders. |

> There is **no** `--output` option — the command prints text.

### `reverse.ticket`

| Parameter       | Type   | Required | Description                                       |
| --------------- | ------ | -------- | ------------------------------------------------- |
| `--profile, -p` | string | yes      | Profile.                                          |
| `--ticket, -t`  | ulong  | yes      | Position ticket to reverse.                       |
| `--sl`          | double | no       | Stop Loss for the new position.                   |
| `--tp`          | double | no       | Take Profit for the new position.                 |
| `--deviation`   | int    | no       | Slippage tolerance (points), default `10`.        |
| `--timeout-ms`  | int    | no       | Per‑RPC timeout (default `30000`).                |
| `--dry-run`     | flag   | no       | Print action plan **without** sending any orders. |

---

## Output ⬆️ (text)

**`reverse`**

* No positions for symbol → `No positions for <SYM> to reverse.` (exit code `2`).
* Net = 0 → `Net position for <SYM> is zero; nothing to reverse.` (exit code `2`).
* `--dry-run`:

  * `net`: `[DRY-RUN] REVERSE(net) <SYM>: send <BUY/SELL> vol=<2×|net|> (deviation=...) SL=... TP=...`
  * `flat`: `[DRY-RUN] REVERSE(flat) <SYM>: close ALL positions; then <BUY/SELL> vol=<|net|> SL=... TP=...`
* Execution:

  * `net`: log `REVERSE(net) done: ticket=... newSide=... volSent=...`
  * `flat`: warns if some positions failed to close, then logs `REVERSE(flat) done: ticket=... side=... vol=...`

**`reverse.ticket`**

* Ticket not found → `Position #<ticket> not found.` (exit code `2`).
* `--dry-run`: `[DRY-RUN] REVERSE.TICKET #<ticket> <SYM>: close <vol>, then <BUY/SELL> <vol> (dev=...) SL=... TP=...`
* Execution: `✔ reverse.ticket done`

Errors are printed via `ErrorPrinter`; fatal errors set exit code `1`.

---

## How to Use 🛠️

```powershell
# Reverse by symbol (net exposure)
dotnet run -- reverse -p demo -s EURUSD --mode net

# Reverse by symbol (flat: close all, then reopen 1×|net|)
dotnet run -- reverse -p demo -s EURUSD --mode flat --sl 1.0950 --tp 1.1050

# Reverse by ticket
dotnet run -- reverse.ticket -p demo -t 123456 --deviation 15
```

---

## Notes & Safety 🛡️

* **Margin:** ensure free margin is sufficient for the opposite leg.
* **Non‑atomic:** close→open are two separate steps; slippage gaps may occur.
* `flat` may partially fail to close some tickets — result prints `OK/FAIL`.
* Best‑effort `EnsureSymbolVisibleAsync` is always called before trading.

---

## Code Reference 🧩

### `reverse` (`net` vs `flat`)

```csharp
// 1) calculate net exposure by symbol
var opened = await _mt5Account.OpenedOrdersAsync();
var posList = opened.PositionInfos.Where(p => p.Symbol == s).ToList();
var net = posList.Sum(p => (IsLongPosition(p) ? 1.0 : -1.0) * p.Volume);

if (mode == "net")
{
    var volToSend = Math.Abs(net) * 2.0;
    await _mt5Account.SendMarketOrderAsync(
        symbol: s,
        isBuy: net < 0,
        volume: volToSend,
        deviation: deviation,
        stopLoss: sl,
        takeProfit: tp);
}
else // flat
{
    // close all, then open 1×|net|
    var batch = posList.Select(p => (p.Ticket, p.Symbol, p.Volume));
    var (ok, fail) = await ClosePositionsAsync(batch, CancellationToken.None);
    await _mt5Account.SendMarketOrderAsync(
        symbol: s,
        isBuy: net < 0,
        volume: Math.Abs(net),
        deviation: deviation,
        stopLoss: sl,
        takeProfit: tp);
}
```

### `reverse.ticket`

```csharp
var opened = await _mt5Account.OpenedOrdersAsync();
var pos = opened.PositionInfos.FirstOrDefault(p => (ulong)p.Ticket == ticket);
var symbol = pos.Symbol; var vol = pos.Volume; var isLong = IsLongPosition(pos);

// steps: close full → open opposite
await _mt5Account.ClosePositionPartialAsync(ticket, vol, deviation, CancellationToken.None);
await _mt5Account.SendMarketOrderAsync(symbol, isBuy: !isLong, volume: vol, deviation: deviation, stopLoss: sl, takeProfit: tp);
```

📌 In short: `reverse net` sends **one** opposite market order with **2×|net|**; `reverse flat` closes all then opens **1×|net|**; `reverse.ticket` does the same for a single ticket.
