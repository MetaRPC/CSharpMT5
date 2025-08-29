# 📖 Glossary (MT5 / GoMT5)

Quick reference for common MT5/GoMT5 terms used throughout the docs and code.

---

## 📝 Quick Cheat Sheet

| Term   | Example                 | Meaning                                     |
| ------ | ----------------------- | ------------------------------------------- |
| Symbol | `EURUSD`                | Instrument identifier                       |
| Lot    | `1.0` → 100,000         | Standard trading volume                     |
| SL     | `1.09500`               | Stop Loss                                   |
| TP     | `1.10500`               | Take Profit                                 |
| Ticket | `123456789012` (UInt64) | Unique order ID                             |
| Digits | `5`                     | Quote precision (1.23456)                   |
| Point  | `0.00001`               | Minimum price step                          |
| Margin | `100.00`                | Locked funds for position                   |
| Equity | `1000.00`               | Balance ± open positions PnL                |
| Stream | `SubscribeQuotes()`     | Continuous updates (ticks, orders, profits) |
| TIF    | `GTC/DAY/GTD/...`       | Time in Force for pending orders            |

---

## 📊 Order Lifecycle (MT5)

```text
   ┌───────────┐
   │ New Order │  (market or pending)
   └─────┬─────┘
         │
         │  market → executes immediately
         │  pending → waits for trigger (Stop/Limit, incl. StopLimit)
         ▼
   ┌───────────┐
   │   Open    │  (position or active pending)
   └───┬───┬───┘
       │   │
       │   │  SL hit  → Closed (loss)
       │   │  TP hit  → Closed (profit)
       │   ▼
       │ ┌───────┐
       │ │Closed │
       │ └───────┘
       │
       ▼
   ┌───────────┐
   │ Cancelled │ (pending deleted or expired by TIF)
   └───────────┘
```

---

## 🧑‍💻 Account

* **Login** → Numeric ID of your trading account.
* **Password** → Investor or trader password.
* **Server / ServerName** → Broker server name (e.g., `Broker-MT5-Demo`).
* **Balance / Equity / Margin / Free Margin / Leverage** → Standard account metrics.
* **Id (Guid)** → Local identifier sent in gRPC header `id` for session tracking.

---

## 🔌 Connection & gRPC

* **Endpoint** → Default is `https://mt5.mrpc.pro:443` (`GrpcServer`).
* **Channel** → `GrpcChannel` (lifecycle managed in code).
* **Clients** → gRPC stubs: `Connection`, `SubscriptionService`, `AccountHelper`, `TradingHelper`, `MarketInfo`, `TradeFunctions`, `AccountInformation`.
* **Headers** → `Metadata { "id": <Guid> }` if `Id != Guid.Empty`.
* **IsConnected** → True if channel is alive and all clients are initialized.

---

## 📈 Market Info

* **Symbol** → Instrument identifier (e.g., `EURUSD`).
* **Digits / Point** → Precision and minimum price step.
* **Contract Size / Lot / Lot Step** → Base volume definitions.
* **Stops Level** → Minimum distance for SL/TP/pending orders.
* **Freeze Level** → Zone where modifications are restricted by broker.

---

## 📦 Orders

* **Order / Position** → Instruction and active position.
* **SL / TP** → Protective and target levels.
* **Ticket (UInt64)** → Unique identifier.
* **Comment / Magic** → Free text or EA tag.

---

## 🔄 Order Types (MT5)

**Market Orders**

* `Buy`, `Sell` — executed immediately at market price.

**Pending Orders**

* `Buy Limit` — buy if price drops to X.
* `Sell Limit` — sell if price rises to X.
* `Buy Stop` — buy if price rises to X.
* `Sell Stop` — sell if price falls to X.
* **`Buy Stop Limit` / `Sell Stop Limit`** — two-level orders: trigger (Stop) → place Limit.

  * Buy Stop Limit: `Limit ≤ Stop`.
  * Sell Stop Limit: `Limit ≥ Stop`.

**TIF (Time in Force)**

* `GTC` — Good Till Cancel.
* `DAY` — Valid for the trading day.
* `GTD / SPECIFIED` — Good Till Date.
* `SPECIFIED_DAY` — Until end of a specified day.

---

## 📊 History & Streaming

* **Quotes / Bars** → Historical ticks/bars.
* **Orders / Deals** → Trade history.
* **Subscriptions** → Real-time updates for quotes, trades, balances, etc.

---

## 🛡️ Errors & Codes

* **Transport** → Network/timeouts.
* **Server trade errors** → Broker-side validation (volume, prices, hours, etc.).
* **Slippage / Requote** → Price deviation or counter-quote.
* **Wrapper behavior** → Wrappers normalize server/oneof errors to exceptions.

---

## ✅ Summary

* **Account** → who you are.
* **Market Info** → what you trade.
* **Orders** → how you trade (incl. Stop Limit + TIF).
* **Connection** → how MT5 talks over gRPC.
* **History/Streaming** → how you monitor.
* **Errors** → what can go wrong.
