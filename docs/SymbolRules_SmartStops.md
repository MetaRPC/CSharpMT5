# Symbol Rules & Smart Stops ⚙️🎯

## What this solves

When placing market orders with SL/TP, brokers enforce **symbol-specific rules**:

* price precision (digits / point),
* minimum stop distance from market (aka **StopLevel** in *points*),
* correct side of SL/TP for BUY/SELL (SL must reduce risk, TP must take profit),
* rounding to tick size.

To avoid server rejections and “Invalid stops”, the CLI performs **preflight stop validation** (we call it **Smart Stops**) before sending the order.

---

## Where rules come from 📡

We derive constraints from these calls:

* **Latest quote** → `FirstTickAsync(symbol, ct)`
  Gives **Bid/Ask** & time; used as the **market reference** for SL/TP direction and min distance.
* **Point size (guess)** → `_mt5Account.PointGuess(symbol)`
  Fast heuristic when exact meta isn’t available.
* **Symbol limits** (min/step/max lot) → `symbol limits` command
  Useful for sizing; not directly for SL/TP but part of the same “symbol rules” family.
* *(Optional/Recommended)* **Market meta** (if available in your build):

  * `digits` (price precision)
  * `stopLevelPoints` (minimal distance for SL/TP from market/entry)
  * `point` (tick size)
    Your `buy` handler already has placeholders for these:

  ```csharp
  int? digits = null;             // TODO: fetch via MarketInfo if available
  double? stopLevelPoints = null; 
  double? point = null;
  ```

> If `digits/point/stopLevelPoints` are not fetched, **Smart Stops** still work using `quote + PointGuess(symbol)`, but having true broker values makes them stricter and safer.

---

## Smart Stops: what it does 🧠

`PreflightStops(isBuy, bid, ask, ref sl, ref tp, digits?, stopLevelPoints?, point?)`:

1. **Rounding**

   * Rounds `sl`/`tp` to the symbol’s `digits` (if provided).
   * If `digits` is unknown, it derives rounding from `point` (e.g., `0.0001`).

2. **Correct side**

   * **BUY**:

     * **SL** must be **< Ask**
     * **TP** must be **> Ask**
   * **SELL**:

     * **SL** must be **> Bid**
     * **TP** must be **< Bid**
   * If a user-provided stop is on the wrong side, it’s **nudged** to the nearest valid side (or rejected if impossible).

3. **Minimum distance (StopLevel)**

   * If `stopLevelPoints` is provided (or can be estimated from `point`):

     * **BUY**:

       * `SL ≤ Ask - stopLevel*point`
       * `TP ≥ Ask + stopLevel*point`
     * **SELL**:

       * `SL ≥ Bid + stopLevel*point`
       * `TP ≤ Bid - stopLevel*point`
   * If too close, the value is pushed out to the minimal allowed distance.

4. **Idempotence & No surprises**

   * If user skipped SL or TP (`null`), nothing is created implicitly.
   * If both are provided and valid — no changes beyond rounding.

> The goal is to **catch broker rejections upfront** and keep the order flow smooth.

---

## BUY flow ✅

```csharp
// 1) Ensure symbol is visible (best-effort; some servers require this)
await _mt5Account.EnsureSymbolVisibleAsync(s, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);

// 2) Get current market context
var q = await CallWithRetry(ct => FirstTickAsync(s, ct), opCts.Token);
var bid = q.Bid; 
var ask = q.Ask;

// 3) Apply Smart Stops (digits/point/stopLevel can be wired to real meta later)
int? digits = null;
double? stopLevelPoints = null;
double? point = null;

PreflightStops(
  isBuy: true,
  bid: bid,
  ask: ask,
  sl: ref sl,
  tp: ref tp,
  digits: digits,
  stopLevelPoints: stopLevelPoints,
  point: point
);

// 4) Place order (with final, validated SL/TP)
var ticket = await CallWithRetry(
  ct => _mt5Account.SendMarketOrderAsync(
          symbol: s, isBuy: true, volume: volume, deviation: deviation,
          stopLoss: sl, takeProfit: tp, deadline: null, cancellationToken: ct),
  opCts.Token);
```

## Common pitfalls the preflight avoids 🧨

* **“Invalid SL/TP: too close to market”** — enforces `StopLevel` distance.
* **“Invalid SL/TP side”** — e.g., BUY with SL above Ask or TP below Ask.
* **“Bad precision”** — rounds prices to `digits` / `point` tick.
* **“Hidden symbol”** — ensures the symbol is visible first.

---

## CLI examples 🛠️

```powershell
# BUY with clearly valid SL/TP (assuming StopLevel ~ 150 pts)
dotnet run -- buy -p demo -s EURUSD -v 0.10 --sl 1.0700 --tp 1.0800 --deviation 10

# If SL/TP are on the wrong side or too close, Smart Stops will push them to the nearest valid prices.
# (You’ll see only final accepted values in logs or in JSON payload if requested.)
```

> Tip: if you see rejections like “TP must be > Ask”, it means your *input* levels were inconsistent with **current** market. Quotes can move even between validation and send. Smart Stops reduce this risk, but they can’t fix a level that’s fundamentally on the wrong side **at the exact server check**. Re-run quickly or widen distance.

---

## How to wire true meta (optional but recommended) 🔧

If your build exposes market info endpoints, populate the placeholders:

```csharp
int? digits = await _mt5Account.SymbolDigitsAsync(s, ct);         // precision
double? point = await _mt5Account.SymbolPointAsync(s, ct);        // tick size
double? stopLevelPoints = await _mt5Account.SymbolStopsLevelAsync(s, ct); // minimal distance (points)
```

> Names above are illustrative — bind to your actual methods if they exist.
> If not available, keep using `PointGuess(symbol)` + quote.

---

## Related commands 🔗

* `symbol show` — quick **quote + lot limits** card
* `symbol limits` — **min/step/max** lot (sizing/risk)
* `quote` — check **Bid/Ask/Time** before placing an order

---

## TL;DR

* **Always**: get a fresh quote → run `PreflightStops` → send the order.
* Provide **true symbol meta** when possible (digits/point/stopLevel).
* Smart Stops save you from 80% of broker-side rejections on SL/TP.
