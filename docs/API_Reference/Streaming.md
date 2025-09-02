# 📡 API Reference — Streaming (MT5)

This page documents **server‑streaming APIs** in the MT5 gRPC interface (`mt5_term_api`). Streams are long‑lived calls that push updates until the client cancels or the server closes the stream.

> Most streaming replies follow the pattern `oneof { data = 1, error = 2 }`. Handle `error` and transport `RpcException` in client code.

---

## 🔔 Quotes Streaming

### MrpcSubscriptionMqlTick (Tick with Symbol)

| Field    | Type          |
| -------- | ------------- |
| `symbol` | string        |
| `tick`   | `MrpcMqlTick` |

**MrpcMqlTick** fields: `time:int64`, `bid:double`, `ask:double`, `last:double`, `volume:uint64`, `time_msc:int64`, `flags:uint32`, `volume_real:double`.

**Notes:** spread (points) = `(ask − bid) / Symbol.Point`.

### OnSymbolTickRequest

* Fields: `symbols[]: string` — list of symbols to subscribe.

### OnSymbolTickReply

* `data: MrpcSubscriptionMqlTick` or `error: Error`.
* Stream stays open until canceled/closed.

---

## 🔄 Trade Updates

### OnTradeRequest

* Fields: `symbols[]: string` *(optional)* — limit updates to specific symbols.

### OnTradeEventData

| Field                         | Type              |
| ----------------------------- | ----------------- |
| `new_orders[]`                | `OpenedOrderInfo` |
| `updated_orders[]`            | `OpenedOrderInfo` |
| `disappeared_order_tickets[]` | `uint64`          |
| `new_positions[]`             | `PositionInfo`    |
| `updated_positions[]`         | `PositionInfo`    |
| `closed_position_tickets[]`   | `uint64`          |
| `deals[]` *(if provided)*     | `DealInfo`        |

### OnTradeReply

* `data: OnTradeEventData` or `error: Error`.

**Notes:** Use this stream to drive UI state for orders/positions in real time.

---

## 🎟️ Positions & Pending Tickets

### OnPositionsAndPendingOrdersTicketsRequest

* Fields: `symbols[]: string` *(optional)* — limit to symbols, otherwise all.

### OnPositionsAndPendingOrdersTicketsReply

| Field       | Type     |
| ----------- | -------- |
| `tickets[]` | `uint64` |

**Notes:** useful for quick diffing of currently active tickets without full payloads.

---

## 💰 Position Profit Stream

### OnPositionProfitRequest

* Fields: *(none or symbol filters, depending on server settings)*

### PositionProfitItem

| Field        | Type   |
| ------------ | ------ |
| `ticket`     | uint64 |
| `profit`     | double |
| `swap`       | double |
| `commission` | double |

### OnPositionProfitData

| Field     | Type                 |
| --------- | -------------------- |
| `items[]` | `PositionProfitItem` |
| `time`    | `Timestamp`          |

### OnPositionProfitReply

* `data: OnPositionProfitData` or `error: Error`.

**Notes:** emits snapshots; use to update PnL widgets efficiently.

---

## 📈 Chart/History Streams (overview)

> Concrete chart/history messages may vary by build; common pattern is a request with symbol/period and a reply streaming bars or points.

* **Chart stream** — request: `{ symbol, period }`; reply: `{ bar: ChartBar }`.
* **Chart history stream** — request: `{ symbol, period, chunks: TimeRanges[] }`; reply: `{ bar: ChartBar }`.
