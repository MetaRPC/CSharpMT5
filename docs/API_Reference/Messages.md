# 📘 API Reference — Messages (MT5)

This page documents **message types** used by the MT5 gRPC API (`mt5_term_api`).
Numbers, names and field order match the source `.proto` files.
Enums are listed in the **Enums (MT5)** page.

---

## 🔖 Conventions

* **`google.protobuf.Timestamp`** → ⏰ UTC time.
* **`optional` fields** → present only when explicitly set (proto3 `optional`).
* **Wrappers** → the API mostly uses plain primitives; when wrappers appear, they provide presence.
* **Prices & volumes** → 💹 validate with `SymbolInfoDoubleProperty` (e.g., `SymbolVolumeMin/Max/Step`).
* **Currency** → 💵 monetary values are in **account currency** (see `AccountSummaryData.account_currency`).

---

## 🧾 Account & Orders

### 📊 AccountSummary (AccountSummaryData)

|  # | Field                                    | Type                       |
| -: | ---------------------------------------- | -------------------------- |
|  1 | `account_login`                          | int64                      |
|  2 | `account_balance`                        | double                     |
|  3 | `account_equity`                         | double                     |
|  4 | `account_user_name`                      | string                     |
|  5 | `account_leverage`                       | int64                      |
|  6 | `account_trade_mode`                     | `MrpcEnumAccountTradeMode` |
|  7 | `account_company_name`                   | string                     |
|  8 | `account_currency`                       | string                     |
|  9 | `server_time`                            | Timestamp                  |
| 10 | `utc_timezone_server_time_shift_minutes` | int64                      |
| 11 | `account_credit`                         | double                     |

**Notes:** snapshot for health/risk checks; leverage as integer; `server_time` is UTC.

---

### 📌 OpenedOrderInfo (live orders/pending)

|  # | Field             | Type                           |
| -: | ----------------- | ------------------------------ |
|  1 | `index`           | uint32                         |
|  2 | `ticket`          | uint64                         |
|  3 | `price_current`   | double                         |
|  4 | `price_open`      | double                         |
|  5 | `stop_limit`      | double                         |
|  6 | `stop_loss`       | double                         |
|  7 | `take_profit`     | double                         |
|  8 | `volume_current`  | double                         |
|  9 | `volume_initial`  | double                         |
| 10 | `magic_number`    | int64                          |
| 11 | `reason`          | int32                          |
| 12 | `type`            | `BMT5_ENUM_ORDER_TYPE`         |
| 13 | `state`           | `BMT5_ENUM_ORDER_STATE`        |
| 14 | `time_expiration` | Timestamp                      |
| 15 | `time_setup`      | Timestamp                      |
| 16 | `time_done`       | Timestamp                      |
| 17 | `type_filling`    | `BMT5_ENUM_ORDER_TYPE_FILLING` |
| 18 | `type_time`       | `BMT5_ENUM_ORDER_TYPE_TIME`    |
| 19 | `position_id`     | int64                          |
| 20 | `position_by_id`  | int64                          |
| 21 | `symbol`          | string                         |
| 22 | `external_id`     | string                         |
| 23 | `comment`         | string                         |
| 24 | `account_login`   | int64                          |

**Notes:** used in `OpenedOrders{Request,Reply,Data}`.

---

### 📌 PositionInfo (open positions)

|  # | Field                 | Type                        |
| -: | --------------------- | --------------------------- |
|  1 | `index`               | uint32                      |
|  2 | `ticket`              | uint64                      |
|  3 | `open_time`           | Timestamp                   |
|  4 | `volume`              | double                      |
|  5 | `price_open`          | double                      |
|  6 | `stop_loss`           | double                      |
|  7 | `take_profit`         | double                      |
|  8 | `price_current`       | double                      |
|  9 | `swap`                | double                      |
| 10 | `profit`              | double                      |
| 11 | `last_update_time`    | Timestamp                   |
| 12 | `type`                | `BMT5_ENUM_POSITION_TYPE`   |
| 13 | `magic_number`        | int64                       |
| 14 | `identifier`          | int64                       |
| 15 | `reason`              | `BMT5_ENUM_POSITION_REASON` |
| 16 | `symbol`              | string                      |
| 17 | `comment`             | string                      |
| 18 | `external_id`         | string                      |
| 19 | `position_commission` | double                      |
| 20 | `account_login`       | int64                       |

---

### 📨 OrderSendRequest / OrderSendReply / OrderSendData

**OrderSendRequest**

|  # | Field                  | Type                        | Optional |
| -: | ---------------------- | --------------------------- | :------: |
|  1 | `symbol`               | string                      |          |
|  2 | `operation`            | `TMT5_ENUM_ORDER_TYPE`      |          |
|  3 | `volume`               | double                      |          |
|  4 | `price`                | double                      |     ✓    |
|  5 | `slippage`             | uint64                      |     ✓    |
|  6 | `stop_loss`            | double                      |     ✓    |
|  7 | `take_profit`          | double                      |     ✓    |
|  8 | `comment`              | string                      |     ✓    |
|  9 | `expert_id`            | uint64                      |     ✓    |
| 10 | `stop_limit_price`     | double                      |     ✓    |
| 11 | `expiration_time_type` | `TMT5_ENUM_ORDER_TYPE_TIME` |     ✓    |
| 12 | `expiration_time`      | Timestamp                   |     ✓    |

**OrderSendReply**

|  # | Field   | Type            |
| -: | ------- | --------------- |
|  1 | `data`  | `OrderSendData` |
|  2 | `error` | `Error`         |

**OrderSendData**

|  # | Field                       | Type   |
| -: | --------------------------- | ------ |
|  1 | `returned_code`             | uint32 |
|  2 | `deal`                      | uint64 |
|  3 | `order`                     | uint64 |
|  4 | `volume`                    | double |
|  5 | `price`                     | double |
|  6 | `bid`                       | double |
|  7 | `ask`                       | double |
|  8 | `comment`                   | string |
|  9 | `request_id`                | uint32 |
| 10 | `ret_code_external`         | int32  |
| 11 | `returned_string_code`      | string |
| 12 | `returned_code_description` | string |

---

### ✏️ OrderModifyRequest / OrderModifyReply / OrderModifyData

**OrderModifyRequest**

|  # | Field                  | Type                        | Optional |
| -: | ---------------------- | --------------------------- | :------: |
|  1 | `ticket`               | uint64                      |          |
|  2 | `stop_loss`            | double                      |     ✓    |
|  3 | `take_profit`          | double                      |     ✓    |
|  4 | `price`                | double                      |     ✓    |
|  5 | `expiration_time_type` | `TMT5_ENUM_ORDER_TYPE_TIME` |     ✓    |
|  6 | `expiration_time`      | Timestamp                   |     ✓    |
|  8 | `stop_limit`           | double                      |     ✓    |

**OrderModifyReply**

|  # | Field   | Type              |
| -: | ------- | ----------------- |
|  1 | `data`  | `OrderModifyData` |
|  2 | `error` | `Error`           |

**OrderModifyData** — same shape as `OrderSendData` (fields `1..12`).

---

### ❌ OrderCloseRequest / OrderCloseReply / OrderCloseData

**OrderCloseRequest**

|  # | Field      | Type   |
| -: | ---------- | ------ |
|  1 | `ticket`   | uint64 |
|  2 | `volume`   | double |
|  5 | `slippage` | int32  |

**OrderCloseReply**

|  # | Field   | Type             |
| -: | ------- | ---------------- |
|  1 | `data`  | `OrderCloseData` |
|  2 | `error` | `Error`          |

**OrderCloseData**

|  # | Field                       | Type                    |
| -: | --------------------------- | ----------------------- |
|  1 | `returned_code`             | uint32                  |
|  2 | `returned_string_code`      | string                  |
|  3 | `returned_code_description` | string                  |
|  4 | `close_mode`                | `MRPC_ORDER_CLOSE_MODE` |

---

## 💹 Quotes & Market Info

### 💱 MrpcMqlTick (tick)

|  # | Field         | Type   |
| -: | ------------- | ------ |
|  1 | `time`        | int64  |
|  2 | `bid`         | double |
|  3 | `ask`         | double |
|  4 | `last`        | double |
|  5 | `volume`      | uint64 |
|  6 | `time_msc`    | int64  |
|  7 | `flags`       | uint32 |
|  8 | `volume_real` | double |

**Notes:** spread in points = `(ask - bid) / Symbol.Point`.

---

### 🔎 SymbolInfoDouble{Request,Reply,Data}

**SymbolInfoDoubleRequest**

|  # | Field    | Type                       |
| -: | -------- | -------------------------- |
|  1 | `symbol` | string                     |
|  2 | `type`   | `SymbolInfoDoubleProperty` |

**SymbolInfoDoubleReply** — oneof { `data` (SymbolInfoDoubleData = 1), `error` (Error = 2) }

**SymbolInfoDoubleData**

|  # | Field   | Type   |
| -: | ------- | ------ |
|  1 | `value` | double |

---

### 📌 SymbolSelect{Request,Reply,Data}

**SymbolSelectRequest**

|  # | Field    | Type   |
| -: | -------- | ------ |
|  1 | `symbol` | string |
|  2 | `select` | bool   |

**SymbolSelectReply** — oneof { `data` (SymbolSelectData = 1), `error` (Error = 2) }

**SymbolSelectData**

|  # | Field     | Type |
| -: | --------- | ---- |
|  1 | `success` | bool |

---

### 🔄 SymbolIsSynchronized{Request,Reply,Data}

**SymbolIsSynchronizedRequest**

|  # | Field    | Type   |
| -: | -------- | ------ |
|  1 | `symbol` | string |

**SymbolIsSynchronizedReply** — oneof { `data` (SymbolIsSynchronizedData = 1), `error` (Error = 2) }

**SymbolIsSynchronizedData**

|  # | Field          | Type |
| -: | -------------- | ---- |
|  1 | `synchronized` | bool |

---

### #️⃣ SymbolsTotal{Request,Reply,Data}

**SymbolsTotalRequest**

|  # | Field  | Type |
| -: | ------ | ---- |
|  1 | `mode` | bool |

**SymbolsTotalReply** — oneof { `data` (SymbolsTotalData = 1), `error` (Error = 2) }

**SymbolsTotalData**

|  # | Field   | Type  |
| -: | ------- | ----- |
|  1 | `total` | int32 |

---

## 🔌 Connection & Health

### 🔐 Connection.ConnectEx / Connect

**ConnectExRequest**

|  # | Field                                        | Type                       |
| -: | -------------------------------------------- | -------------------------- |
|  1 | `user`                                       | uint64                     |
|  2 | `password`                                   | string                     |
|  3 | `mt_cluster_name`                            | string                     |
|  4 | `base_chart_symbol`                          | string                     |
|  5 | `terminal_readiness_waiting_timeout_seconds` | int32                      |
|  6 | `experts_to_add`                             | `ExpertAdviser` (repeated) |

**ConnectExReply** — oneof { `data` (ConnectData = 1), `error` (Error = 2) }

**ConnectRequest**

|  # | Field                                        | Type                       |
| -: | -------------------------------------------- | -------------------------- |
|  1 | `user`                                       | uint64                     |
|  2 | `password`                                   | string                     |
|  3 | `host`                                       | string                     |
|  4 | `port`                                       | int32                      |
|  5 | `base_chart_symbol`                          | string                     |
|  6 | `wait_for_terminal_is_alive`                 | bool                       |
|  7 | `terminal_readiness_waiting_timeout_seconds` | int32                      |
|  8 | `experts_to_add`                             | `ExpertAdviser` (repeated) |

**ConnectReply** — oneof { `data` (ConnectData = 1), `error` (Error = 2) }

**ConnectData**

|  # | Field                    | Type           |
| -: | ------------------------ | -------------- |
|  1 | `terminal_instance_guid` | string         |
|  3 | `terminal_type`          | `TerminalType` |

**DisconnectRequest** — (empty)

**DisconnectReply** — oneof { `data` (DisconnectData = 1), `error` (Error = 2) }

**DisconnectData**

|  # | Field                    | Type   |
| -: | ------------------------ | ------ |
|  1 | `unique_identifier`      | string |
|  2 | `full_life_time_seconds` | int64  |

**Health.Check**

**HealthCheckRequest** — (empty)

**HealthCheckReply**

|  # | Field                    | Type  |
| -: | ------------------------ | ----- |
|  1 | `is_connected_to_server` | bool  |
|  2 | `server_time_seconds`    | int64 |

---

## 📡 Streaming payload helpers (overview)

> Detailed stream methods & chunk types are in the **Streaming (MT5)** page.

* **Quotes**: `OnSymbolTick{Request,Reply,Data}` with `MrpcSubscriptionMqlTick`.
* **Trades**: `OnTrade{Request,Reply,Data}` with `OnTadeEventData` (new/disappeared/updated orders, deals, positions).
* **Position PnL snapshots**: `OnPositionProfit{Request,Reply,Data}`.
* **Tickets stream**: `OnPositionsAndPendingOrdersTickets{Request,Reply}`.

---

📌 **Tip:** When building requests, use the correct enums for fields (e.g., `TMT5_ENUM_ORDER_TYPE`, `TMT5_ENUM_ORDER_TYPE_TIME`).
Validate volumes (`SymbolVolumeMin/Max/Step`) and prices (broker `Stops`/`Freeze` levels) to avoid rejections.
