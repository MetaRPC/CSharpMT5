# 🏗️ Architecture & Data Flow (C# / MT5)

This section describes the structure of the C# MT5 project and how data flows between components.

---

## General Diagram

```
          ┌─────────────────────────────┐
          │        💻 MT5 Server        │
          │ (broker connection, quotes, │
          │  orders, account handling)  │
          └──────────────┬──────────────┘
                         │  gRPC
                         ▼
          ┌─────────────────────────────┐
          │   🚀 C# MT5 Client Library │
          │ (MT5Account + gRPC stubs)  │
          └───────┬───────────┬────────┘
                  │           │
                  ▼           ▼
       ┌────────────────┐   ┌───────────────────┐
       │ 👩‍💻 User Code  │   │ 🔄 Streaming      │
       │ (apps, bots)   │   │ (quotes, orders,  │
       │                │   │ balances, history)│
       └────────────────┘   └───────────────────┘

📄 config.json → stores login, password, server, and default symbol.  
📦 mt5_term_api → generated C# classes from `.proto` files.  
```

---

## ⚙️ Components

* **💻 MT5 Server**
  Provided by the broker. Executes trades, streams quotes, and holds account data.

* **🚀 C# MT5 Client Library**
  Contains `MT5Account` class and gRPC clients (`ConnectionClient`, `SubscriptionServiceClient`, `AccountHelperClient`, `TradingHelperClient`, `MarketInfoClient`, `TradeFunctionsClient`, `AccountInformationClient`).  Manages connection lifecycle and request building.

* **📦 mt5\_term\_api (generated code)**
  Auto-generated C# code from protobuf (`.proto`). Includes messages like `OrderSendRequest`, `OrderModifyRequest` and enums like `TMT5_ENUM_ORDER_TYPE`.

* **📄 config.json**
  Stores credentials (login, password, server), default chart symbol, and connection settings.

* **👩‍💻 User Code**
  Applications, bots, or tools that use the `MT5Account` class to trade, query account state, or subscribe to market data.

---

## 🔀 Data Flow

1. **📡 RPC call**
   User code calls a method (e.g., `PlaceStopLimitOrderAsync`) on `MT5Account`.

2. **⚙️ Client library**
   Maps parameters into protobuf request objects and sends them via gRPC client.

3. **💻 MT5 Server**
   Executes the request (order placement, quote fetch, etc.).

4. **⬅️ Return path**
   Response is returned as a protobuf reply, mapped back into C# structures.

5. **🔄 Streaming calls**
   Subscriptions (quotes, balances, orders) maintain an open channel to push updates.

---

## ✨ Highlights

* All tickets are `ulong` (UInt64).
* Orders support Stop Limit and Time in Force (GTC, DAY, GTD, Specified).
* `EnsureSymbolVisibleAsync` guarantees that symbol is active before use.
* Connection lifecycle handled by `ConnectAsync()` / `DisconnectAsync()`.
* Streaming keeps charts and UI in sync in real time.

---

## 🛠️ Developer Notes

* Main entry for apps: `MT5Account` class.
* Protobuf code lives in `mt5_term_api` namespace.
* When adding new RPCs → update `.proto`, regenerate C# classes.
* Error handling: server retcodes are exposed via wrapper exceptions.
* Timeout control: all RPC calls support `CancellationToken` and per-call deadlines.
