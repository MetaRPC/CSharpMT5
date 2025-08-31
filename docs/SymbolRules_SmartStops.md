# Symbol Rules & Smart Stops ⚙️📈

This page explains how the CLI validates and auto-adjusts SL/TP before sending orders to MT5. It reflects the behavior used by `buy`/`sell` and other commands where we call `PreflightStops(...)` right before the RPC.

---

## Why this exists

Brokers enforce **symbol-specific rules**:

* minimum distance from the current price to SL/TP (**StopLevel**),
* price precision (**Digits**) and **point size** (`Point`),
* correct side (e.g., *BUY*: `SL < Bid`, `TP > Ask`).

If you pass an invalid SL/TP, the server rejects the order. Our *smart stops* preflight makes a best effort to validate & correct the levels **client-side** and only then submit.

---

## Where rules come from

We combine live price data and symbol metadata:

* **Quote** → `Bid` / `Ask` (via `FirstTickAsync`).
* **Digits / Point** → from Market Info (or a safe fallback via `PointGuess(symbol)`).
* **StopLevel (points)** → from Market Info when available; otherwise we treat it as unknown and do minimum checks only.

> In the `buy`/`sell` handlers we call:
>
> ```csharp
> var q = await CallWithRetry(ct => FirstTickAsync(s, ct), opCts.Token);
> var bid = q.Bid; var ask = q.Ask;
> int? digits = null;              // TODO: fill from MarketInfo
> double? stopLevelPoints = null;  // TODO: fill from MarketInfo
> double? point = null;            // TODO: fill from MarketInfo (or PointGuess)
> PreflightStops(isBuy: true/false, bid: bid, ask: ask,
>                sl: ref sl, tp: ref tp,
>                digits: digits, stopLevelPoints: stopLevelPoints, point: point);
> ```

---

## What *Smart Stops* do

1. **Side validation**

* BUY: `SL` must be **below** `Bid`, `TP` must be **above** `Ask`.
* SELL: `SL` must be **above** `Ask`, `TP` must be **below** `Bid`.

If levels are on the wrong side, they are **rejected** (or adjusted if that’s clearly a rounding issue and we have `Point/Digits`).

2. **Stop Level distance**

If `StopLevel` is known, we ensure:

* BUY: `Bid - SL ≥ StopLevel*Point`, `TP - Ask ≥ StopLevel*Point`.
* SELL: `SL - Ask ≥ StopLevel*Point`, `Bid - TP ≥ StopLevel*Point`.

If too close, we **push** SL/TP just enough to respect the limit.

3. **Precision**

If `Digits` is provided, we **round** SL/TP to that precision (banker’s rounding avoided for prices). Rounding happens *after* distance adjustments.

4. **No-ops for missing inputs**

If `sl`/`tp` were not provided by the user (`null`), nothing is changed/added.

---

## Examples (BUY) 🛒

Assume `Bid=1.16825`, `Ask=1.16889`, `Point=0.0001`, `Digits=5`, `StopLevel=20 pts`.

| Input (user) | Check                                                      | Result                                                         |
| ------------ | ---------------------------------------------------------- | -------------------------------------------------------------- |
| `SL=1.16820` | Distance: `Bid - SL = 0.00005 = 0.5 pt` < 20 pts           | SL shifted to `Bid - 20*Point = 1.16625` → rounded to 5 digits |
| `TP=1.16880` | Distance: `TP - Ask = -0.00009` (wrong side)               | Error: TP must be > Ask                                        |
| `TP=1.16920` | Distance: `1.16920 - 1.16889 = 0.00031 = 3.1 pts` < 20 pts | TP shifted to `Ask + 20*Point = 1.17089` → rounded             |

**SELL** behaves symmetrically.

---

## CLI workflow 🔁

* You specify SL/TP in **prices**:

```powershell
# BUY: levels will be validated & auto-pushed as needed
mt5 buy -p demo -s EURUSD -v 0.10 --sl 1.0700 --tp 1.0800 --deviation 10
```

* For rules visibility use symbol utilities:

```powershell
# lot limits & precision
mt5 symbol limits -p demo -s EURUSD

# quick price reference
mt5 quote -p demo -s EURUSD
```

---

## Interaction with deviations & re-quotes

`--deviation` controls allowed slippage for the **entry price** (market order). It does **not** change SL/TP checks. Smart stops ensure SL/TP are valid *before* the order is sent; deviation applies when the server executes the price.

---

## Pitfalls & tips 🧭

* If quotes are **stale** (e.g., `[STALE >5s]`), distance math can be misleading. Prefer fresh quotes.
* If Market Info is not available, we rely on **safe guesses** (`PointGuess`, default Digits). In such cases we only guarantee side checks and rounding; StopLevel enforcement may be partial.
* Always prefer **`symbol ensure`** before trading a new symbol, then run `symbol limits` to see the exact broker constraints.
* If you get *“Invalid TP for BUY: must be > Ask”*, it means the requested TP is on the wrong side relative to the latest **Ask** (or too close after rounding).

---

## What to wire next (nice-to-have) 🔧

* Plug Market Info resolvers so `digits`, `point`, `stopLevelPoints` are always filled.
* Log the **original** and **adjusted** SL/TP when smart stops make changes (debug level) — this helps debugging broker validations.
* Add a `--no-smart-stops` flag for strict/purist mode (fail fast instead of auto-fix).
