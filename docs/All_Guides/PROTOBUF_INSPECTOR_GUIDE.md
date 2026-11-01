# Protobuf Inspector — Interactive Type Explorer

> **Interactive developer utility for exploring the MetaRPC.MT5 protobuf types, fields and enums of the MT5 gRPC API — straight from the compiled C# assembly.**

---

## 🏛️ What this tool does

The Protobuf Inspector is an **interactive console tool** that reflects over the generated `mt5_term_api` C# types so you can explore the API without a terminal connection:

- Interactive search for types, fields and enums
- Inspect a message type and see its **C# properties** and their types
- Field-level discovery — find which types contain a given field
- Enum value exploration — list all values of an enum
- Type browsing — list all available types

> It reads **C# property names** (PascalCase), e.g. `AccountSummaryData.AccountBalance`, not the proto wire names.

---

## 🏁 Getting Started

### Running the inspector

```bash
dotnet run inspect
```

You'll see a header, a quick-start box, and an interactive prompt:

```
╔════════════════════════════════════════════════════════════════════╗
║          🔍 INTERACTIVE PROTOBUF TYPES INSPECTOR 🔍                ║
║         Explore MT5 gRPC API Types, Fields & Enums                ║
╚════════════════════════════════════════════════════════════════════╝

Assembly: MetaRPC.CSharpMT5 v1.0.0.0

┌────────────────────────────────────────────────────────────────┐
│ QUICK START GUIDE                                              │
│ • Type 'help' for full command list                            │
│ • Type 'list' to see all available types                       │
│ • Type a type name to inspect it (e.g., 'OpenedOrdersData')    │
│ • Type 'exit' to quit                                          │
└────────────────────────────────────────────────────────────────┘

>
```

---

## 🧰 Available Commands

| Command | Description | Example |
|---------|-------------|---------|
| `list` or `ls` | Show all available protobuf types (Data / Request / Reply) | `list` |
| `<TypeName>` | Inspect a type — shows its properties and their types | `OpenedOrdersData` |
| `search <text>` or `find <text>` | Search for types whose name contains the text | `search Order` |
| `field <name>` | Find all types that contain a field | `field Ticket` |
| `enum <name>` | Show all values of an enum | `enum BMT5_ENUM_ORDER_TYPE` |
| `help` or `?` | Show the help message | `help` |
| `exit`, `quit` or `q` | Exit the inspector | `exit` |

Search and type names are **case-insensitive**.

---

## Practical Examples

### Example 1: What properties does `AccountSummaryData` have?

**Command:**
```
> AccountSummaryData
```

**Output:**
```
╔════════════════════════════════════════════════════════════╗
║ TYPE: AccountSummaryData                                  ║
╚════════════════════════════════════════════════════════════╝

📋 Properties (11):

  • AccountBalance                    : double
  • AccountCompanyName                : string
  • AccountCredit                     : double
  • AccountCurrency                   : string
  • AccountEquity                     : double
  • AccountLeverage                   : long
  • AccountLogin                      : long
  • AccountTradeMode                  : MrpcEnumAccountTradeMode
  • AccountUserName                   : string
  • ServerTime                        : Timestamp
  • UtcTimezoneServerTimeShiftMinutes : long
```

> Note: `AccountSummaryData` has **no** margin/profit fields — fetch those via `AccountInfoDouble` (e.g. `AccountMargin`, `AccountProfit`).

---

### Example 2: Inspect `PositionInfo`

**Command:**
```
> PositionInfo
```

**Output (abridged):**
```
TYPE: PositionInfo
------------------------------------------------------------
  Index         : uint
  Ticket        : ulong
  OpenTime      : Timestamp
  Volume        : double
  PriceOpen     : double
  StopLoss      : double
  TakeProfit    : double
  PriceCurrent  : double
  ...
```

---

### Example 3: Find which types have a `Ticket` field

**Command:**
```
> field Ticket
```

**Output (abridged):** a substring match — each type is listed with the matching property and its type:
```
🔍 Searching for field 'Ticket'...

✅ Found in 20 type(s):

📦 OpenedOrderInfo:
   └─ Ticket: UInt64

📦 PositionInfo:
   └─ Ticket: UInt64

📦 OrderCloseRequest:
   └─ Ticket: UInt64

📦 MqlTradeTransaction:
   └─ DealTicket: UInt64
   └─ OrderTicket: UInt64
   └─ PositionTicket: UInt64
  ...
```

---

### Example 4: See all order-type enum values

**Command:**
```
> enum TMT5_ENUM_ORDER_TYPE
```

**Output:**
```
ENUM: TMT5_ENUM_ORDER_TYPE
------------------------------------------------------------
  Tmt5OrderTypeBuy             = 0
  Tmt5OrderTypeSell            = 1
  Tmt5OrderTypeBuyLimit        = 2
  Tmt5OrderTypeSellLimit       = 3
  Tmt5OrderTypeBuyStop         = 4
  Tmt5OrderTypeSellStop        = 5
  Tmt5OrderTypeBuyStopLimit    = 6
  Tmt5OrderTypeSellStopLimit   = 7
  Tmt5OrderTypeCloseBy         = 8
```

> **Tip:** trading functions use `TMT5_ENUM_ORDER_TYPE` / `ENUM_ORDER_TYPE_TF`; streaming events use the `SUB_ENUM_*` enums; historical data uses the `BMT5_ENUM_*` enums.

---

### Example 5: Find all types related to "Order"

**Command:**
```
> search Order
```

**Output (abridged):** matches both classes and enums, each tagged:
```
🔍 Searching for types containing 'Order'...

✅ Found 60 type(s):

  [Class]  AllHistoryOrdersData
  [Class]  OpenedOrdersData
  [Class]  OrderCalcMarginRequest
  [Class]  OrderCalcProfitRequest
  [Class]  OrderCloseRequest
  [Class]  OrderSendRequest
  [Class]  OrdersHistoryData
  [Enum]   BMT5_ENUM_ORDER_TYPE
  [Enum]   ENUM_ORDER_TYPE_TF
  [Enum]   TMT5_ENUM_ORDER_TYPE
  ...
```

---

### Example 6: Browse everything

**Command:**
```
> list
```

**Output (abridged):**
```
╔════════════════════════════════════════════════════════════╗
║              ALL PROTOBUF TYPES                           ║
╚════════════════════════════════════════════════════════════╝

📦 Found N types:

  [Data]    AccountSummaryData
  [Data]    OpenedOrdersData
  [Data]    OrdersHistoryData
  [Request] OrderSendRequest
  [Request] OrderCloseRequest
  [Reply]   OrderSendReply
  [Reply]   AccountSummaryReply
  ...
```

`list` shows the public `mt5_term_api` classes whose names end in `Data`, `Request` or `Reply`, tagged by category (it does **not** list enums — use `search` or `enum` for those).

---

## 📦 Common Use Cases

### "Getting a wrong-property error"

**Problem:** your code has `summary.Ballance` but it won't compile.

```
> AccountSummaryData
```
→ shows the correct property is `AccountBalance`.

### "What data is in X?"

```
> OpenedOrdersData
```
→ lists every property and its type (e.g. `PositionInfos`, `OpenedOrders`).

### "What enum value do I pass?"

```
> enum ENUM_ORDER_TYPE_TF
```
→ shows `OrderTypeTfBuy = 0`, `OrderTypeTfSell = 1`, …

### "Which type holds this field?"

```
> field Profit
```
→ shows `PositionInfo`, `OnPositionProfitPositionInfo`, … (substring match, so it also finds related fields).

### "Just exploring the API"

```
> list
```
→ browse all available types, then inspect any by name.

---

## Frequently used enums

| Enum | Context | Notes |
|------|---------|-------|
| `ENUM_ORDER_TYPE_TF` | Trade functions (`OrderCalcMargin`, `OrderCalcProfit`, `OrderCheck`) | `OrderTypeTfBuy`, `OrderTypeTfSell`, … (9 values) |
| `TMT5_ENUM_ORDER_TYPE` | `OrderSend` / `OrderSendWithoutChecks` request operation | `Tmt5OrderTypeBuy`, … (9 values) |
| `TMT5_ENUM_ORDER_TYPE_TIME` | Order lifetime | GTC / Day / Specified (4 values) |
| `BMT5_ENUM_ORDER_TYPE` | History orders | order type in `HistoryOrderInfo` |
| `BMT5_ENUM_ORDER_HISTORY_SORT_TYPE` | `OrderHistory` / `AllHistoryOrders` sort | 6 values |
| `SUB_ENUM_ORDER_TYPE`, `SUB_ENUM_ORDER_STATE`, `SUB_ENUM_DEAL_TYPE` | Streaming events (`OnTrade`, `OnTradeTransaction`) | the `SUB_ENUM_*` family |
| `SymbolInfoDoubleProperty` | `SymbolInfoDouble` selector | 60 values (BID, ASK, POINT, …) |
| `SymbolInfoIntegerProperty` | `SymbolInfoInteger` selector | 37 values |
| `AccountInfoDoublePropertyType` | `AccountInfoDouble` selector | 14 values |

**Prefix meaning:**

- `TMT5_*` — trading-helper request enums (OrderSend)
- `ENUM_*_TF` — trade-functions enums (margin/profit/check)
- `BMT5_*` — base MT5 enums (history, symbol params)
- `SUB_*` — subscription/streaming enums (real-time events)

Full tables: [Enums Usage Reference](./ENUMS_USAGE_REFERENCE.md).

---

## Technical Notes

- **No MT5 connection required** — the inspector reflects over the compiled `mt5_term_api` assembly; it never connects to a terminal.
- **C# names** — it reports C# property and enum-member names (PascalCase), which is exactly what you write in code.
- **Development only** — a utility for exploring the API, not for production use.
- **Source file:** `Examples/Helpers/ProtobufInspector.cs` (entry point `ProtobufInspector.Run()`, invoked by `dotnet run inspect`).

---

## When to use this tool

**Use it when:**

- Learning the MT5 gRPC API structure
- Debugging a wrong property/field name
- Looking up enum values before building a request
- Finding the right Request/Reply/Data type for a call
- Understanding a message's shape before writing code

**Don't use it for:**

- Inspecting runtime data (use the debugger)
- Production code (it's a dev utility)
- Testing connectivity (use a demo connection)

---

## Tips

1. **Start with `search`** when you know the general area (`search Position`).
2. **Use `field <name>`** when you see a field but don't know its type.
3. **Explore enums early** — `enum <name>` saves debugging time later.
4. **`list` when stuck** — browse everything, then inspect by name.
5. **Case doesn't matter** — type commands and names in any case.

---

## Command Quick Reference

```
  list / ls          - show all types
  <TypeName>         - inspect a type's properties
  search <text>      - find types by name (alias: find)
  field <name>       - find types containing a field
  enum <name>        - show enum values
  help / ?           - show help
  exit / quit / q    - exit
```

---

## See Also

- [Enums Usage Reference](./ENUMS_USAGE_REFERENCE.md) — all enums and where they're used
- [MT5Account Master Overview](../MT5Account/MT5Account.Master.Overview.md) — complete API reference
- [Return Codes Reference](./RETURN_CODES_REFERENCE.md) — trade return codes
- [User Code Sandbox Guide](./USERCODE_SANDBOX_GUIDE.md) — write and test your own code
