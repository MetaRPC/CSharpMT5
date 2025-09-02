# 📚 API Reference — Overview (MT5)

Short, navigable entry point to the MT5 API reference. Use this page to jump to the right place and understand naming rules & conventions.

---

## 🗺️ What’s inside

* **[Messages](./Messages.md)** — payload structures (requests, replies, snapshots) with field notes.
* **[Enums](./Enums.md)** — all enumerations with human meanings.
* **[Streaming](./Streaming.md)** — long‑lived gRPC streams and their chunk types.

> Looking for usage? See **Cookbook** recipes next to this section (e.g. Orders/PlaceMarketOrder, MarketInfo/GetQuote, Streaming/SubscribeQuotes).

---

## 🏷️ Naming & readability

* Original proto names are **scoped for MT5** (e.g., enums prefixed with `TMT5_` / `BMT5_`, messages in `mt5_term_api`).
* In headings we show **both**: full name and a short alias in parentheses — e.g. *OpenedOrderInfo (OpenedOrder)*.
* Inside tables and notes we use **short names** for easier reading.

**Why the MT5 prefixes?** They disambiguate MT5 from MT4 artifacts and avoid collisions across modules/languages.

---

## 🧩 Common type legend

* **`Timestamp`** ⏰ — UTC time. Log in RFC3339.
* **Optional fields** 🎛 — proto3 `optional` (or wrappers) indicate presence; omit when not set.
* **Money & PnL** 💵 — in **account currency** (see `AccountSummaryData.account_currency`).
* **Prices & volumes** 💹 — validate with `SymbolInfoDoubleProperty` (`SymbolVolumeMin/Max/Step`, `Point`, `Digits`).

---

## 🔌 API families → where to read

| Area                     | Start here                                                                                                                  |
| ------------------------ | --------------------------------------------------------------------------------------------------------------------------- |
| **Connection & Health**  | `Connect/ConnectEx/Disconnect`, **Health.Check** → see **Messages** → *Connection & Health*                                 |
| **Orders (sync)**        | `OrderSend{Request,Reply,Data}`, `OrderModify/Close`, and opened/pending order payloads → **Messages** → *Account & Orders* |
| **Positions**            | `PositionInfo`, tickets stream, profits snapshots → **Streaming**                                                           |
| **Market info & quotes** | `MrpcMqlTick`, `OnSymbolTick`, `SymbolInfoDouble{Request,Data}` → **Messages** → *Quotes & Market Info*                     |
| **Streaming**            | Quotes, trade updates, tickets, position PnL, charts/history → **Streaming**                                                |

---

## 🚦 Stability notes

* Optional fields may be **omitted** by the server when not applicable.
* Enums can gain new values — handle **unknown** values defensively on the client side.
* Streaming replies typically use `oneof { data = 1, error = 2 }`; surface `error` and transport exceptions to your retry logic.
