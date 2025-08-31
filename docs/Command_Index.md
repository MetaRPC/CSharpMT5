## 📖 Command Index (Full Reference)

> All available commands in **CSharpMT5 CLI**, grouped by category.
> Use with:
>
> ```powershell
> dotnet run -- <command> [options]
> ```

---

### ⚙️ Profiles & Setup

* **profiles**

  * `profiles` — Working with profiles
  * `list` — Show profile names from `profiles.json`
  * `show` — Show profile parameters (respects `MT5_PASSWORD`)

---

### 🧾 Account & Diagnostics

* **info** — Show account summary
* **health** — Quick connectivity & account diagnostics
* **quote** — Get a snapshot quote (Bid/Ask/Time)
* **stream** — Subscribe to trading events/ticks (auto-reconnect)

---

### 💹 Market Orders

* **buy** — Market buy
* **sell** — Market sell

---

### 🔒 SL/TP, Breakeven & Trailing

* **modify**

  * `modify` — Modify StopLoss / TakeProfit by ticket
* **position**

  * `position.modify` — Modify SL/TP for a position by ticket
  * `position.modify.points` — Set SL/TP by distance in points from entry/market
* **breakeven** — Move SL to entry ± offset (breakeven)
* **trail**

  * `trail.start` — Start local trailing stop
  * `trail.stop` — Stop local trailing stop

---

### 🔄 Closing / Reversing

* **close**

  * `close` — Close by ticket (volume normalized)
  * `close.percent` — Close % of a position by ticket
  * `close.half` — Close half (alias of close.percent --pct 50)
  * `close.partial` — Partially close exact volume
* **close-all** — Close ALL open positions (optionally by symbol)
* **close-symbol** — Close ALL positions for a symbol
* **closeby** — Close a position by the opposite position (emulated)
* **reverse**

  * `reverse` — Reverse net position by symbol (`--mode net|flat`)
  * `reverse.ticket` — Reverse a specific position by ticket

---

### 📑 Pending Orders

* **place** — Place a pending order (limit/stop/stop-limit)
* **pending**

  * `pending` — Pending utilities
  * `pending.modify` — Modify pending (price/SL/TP/expiry)
  * `pending.move` — Move pending by ±N points
  * `pending list` — List pending tickets
* **cancel**

  * `cancel` — Cancel pending by ticket
  * `cancel.all` — Cancel all pendings (optional filters)

---

### 🎟️ Tickets / Orders / Positions

* **ticket**

  * `ticket` — Work with a specific ticket
  * `ticket show` — Show info (open or recent history)
* **orders** — List open orders & positions tickets
* **positions** — List active positions

---

### 📊 Symbols

* **symbol**

  * `symbol` — Symbol utilities
  * `symbol ensure` — Ensure symbol is visible in terminal
  * `symbol limits` — Show min/step/max volume
  * `symbol show` — Short card: Quote + Limits

---

### 🕒 History & Tools

* **history**

  * `history` — Orders/deals history (last N days)
  * `history.export` — Export history to CSV/JSON
* **lot**

  * `lot.calc` — Calculate position volume by risk % and SL distance (points)
* **panic** — Close ALL positions and cancel ALL pendings (optionally filtered)

---
