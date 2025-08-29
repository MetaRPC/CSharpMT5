# 🎛️ API Reference — Enums (MT5)

This page lists key **enumerations** used by the MT5 C# SDK (`mt5_term_api`). Names match the `.proto` files; C# members are in PascalCase.


## 📊 Orders

### TMT5\_ENUM\_ORDER\_TYPE (OrderType)

| Name                         | Meaning                 |
| ---------------------------- | ----------------------- |
| `Tmt5OrderTypeBuy`           | Market Buy              |
| `Tmt5OrderTypeSell`          | Market Sell             |
| `Tmt5OrderTypeBuyLimit`      | Pending Buy Limit       |
| `Tmt5OrderTypeSellLimit`     | Pending Sell Limit      |
| `Tmt5OrderTypeBuyStop`       | Pending Buy Stop        |
| `Tmt5OrderTypeSellStop`      | Pending Sell Stop       |
| `Tmt5OrderTypeBuyStopLimit`  | Pending Buy Stop Limit  |
| `Tmt5OrderTypeSellStopLimit` | Pending Sell Stop Limit |

**Alias / Usage:** Used in `OrderSendRequest.Operation`, `OpenedOrderInfo.Type`, and related messages.

---

### TMT5\_ENUM\_ORDER\_TYPE\_TIME (OrderTypeTime / TIF)

| Name                        | Meaning                         |
| --------------------------- | ------------------------------- |
| `Tmt5OrderTimeGtc`          | Good Till Cancel                |
| `Tmt5OrderTimeDay`          | Day (valid for the trading day) |
| `Tmt5OrderTimeSpecified`    | Good Till Date/Specified time   |
| `Tmt5OrderTimeSpecifiedDay` | Until end of specified day      |

**Alias / Usage:** Used in `OrderSendRequest.ExpirationTimeType` and `OrderModifyRequest.ExpirationTimeType`.

---

## 📈 Symbol & Market Info

### SymbolInfoDoubleProperty (SymbolInfoDouble)

| Name               | Meaning                       |
| ------------------ | ----------------------------- |
| `SymbolVolumeMin`  | Minimum allowed volume (lots) |
| `SymbolVolumeMax`  | Maximum allowed volume (lots) |
| `SymbolVolumeStep` | Volume step (lots)            |

**Alias / Usage:** Queried via `SymbolInfoDoubleAsync(symbol, property, ...)` to validate order volume.

---

## 🔧 Order Modification

> These enums are used when creating or modifying orders.

* `TMT5_ENUM_ORDER_TYPE` — order kind (market / pending / stop‑limit).
* `TMT5_ENUM_ORDER_TYPE_TIME` — Time‑in‑Force.

**Common rules:**

* **Buy Stop Limit** requires `Limit ≤ Stop`.
* **Sell Stop Limit** requires `Limit ≥ Stop`.
* TIF **Specified/GTD** requires `ExpirationTime`.

---

## 🧭 Where They Appear (by message)

* **OrderSendRequest**: `Operation`, `ExpirationTimeType`, `ExpirationTime`.
* **OrderModifyRequest**: `Ticket`, `Price`, `StopLimit`, `StopLoss`, `TakeProfit`, `ExpirationTimeType`, `ExpirationTime`.
* **OpenedOrderInfo**: `Type`, `Ticket`, `Symbol`, prices.
* **Market/Symbol**: `SymbolInfoDoubleProperty` used by `SymbolInfoDoubleAsync`.

---

## ✅ Tips

* Use enum names from generated C# (`mt5_term_api`) exactly as defined.
* Validate **volume** using `SymbolVolumeMin/Max/Step` before sending orders.
* When mapping user input, normalize to your canonical names (e.g., "buylimit" → `Tmt5OrderTypeBuyLimit`).
* For TIF = `Specified`/`SpecifiedDay`, always set `ExpirationTime`.
