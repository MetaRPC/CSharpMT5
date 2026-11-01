# ENUMs Usage Reference — MT5Account (C#)

> ENUM presence lookup table for all **MT5Account** low-level methods. Enum **type** names are the proto/C# names; enum **values** are shown as their C# members (e.g. `AccountInfoDoublePropertyType.AccountBalance`, whose proto `OriginalName` is `ACCOUNT_BALANCE`).

---

## ✅ Summary Statistics

| Method Group | With ENUMs | Total | Percentage |
|--------------|------------|-------|------------|
| **Account Information** | 4 | 4 | 100% |
| **Symbol Information** | 8 | 15 | 53% |
| **Positions & Orders** | 4 | 7 | 57% |
| **Market Depth/DOM** | 1 | 3 | 33% |
| **Trading Operations** | 7 | 7 | 100% |
| **Streaming Methods** | 3 | 5 | 60% |
| **Trade Statistics** | 0 | 4 | 0% |
| **TOTAL** | **27** | **45** | **60%** |

---

## 👤 1. Account Information (4 methods)

### 4/4 methods use ENUMs (100%)

| Method | ENUMs Count | Description | Input ENUMs | Output ENUMs |
|--------|-------------|-------------|-------------|--------------|
| **AccountSummary** | **[In: 0, Out: 1]**<br>Total: 1 ENUM (3 values) | Get all account data in one call (RECOMMENDED) | - | `MrpcEnumAccountTradeMode` (3 values) — account type: `MrpcAccountTradeModeDemo`, `MrpcAccountTradeModeContest`, `MrpcAccountTradeModeReal` |
| **AccountInfoDouble** | **[In: 1, Out: 0]**<br>Total: 1 ENUM (14 values) | Get double properties (Balance, Equity, Margin, etc.) | `AccountInfoDoublePropertyType` (14 values): `AccountBalance`, `AccountCredit`, `AccountProfit`, `AccountEquity`, `AccountMargin`, `AccountMarginFree`, `AccountMarginLevel`, `AccountMarginSoCall`, `AccountMarginSoSo`, `AccountMarginInitial`, `AccountMarginMaintenance`, `AccountAssets`, `AccountLiabilities`, `AccountCommissionBlocked` | - |
| **AccountInfoInteger** | **[In: 1, Out: 0]**<br>Total: 1 ENUM (11 values) | Get integer properties (Login, Leverage, etc.) | `AccountInfoIntegerPropertyType` (11 values): `AccountLogin`, `AccountTradeMode`, `AccountLeverage`, `AccountLimitOrders`, `AccountMarginSoMode`, `AccountTradeAllowed`, `AccountTradeExpert`, `AccountMarginMode`, `AccountCurrencyDigits`, `AccountFifoClose`, `AccountHedgeAllowed` | - |
| **AccountInfoString** | **[In: 1, Out: 0]**<br>Total: 1 ENUM (4 values) | Get string properties (Currency, Company, etc.) | `AccountInfoStringPropertyType` (4 values): `AccountName`, `AccountServer`, `AccountCurrency`, `AccountCompany` | - |

---

## 📊 2. Symbol Information (15 methods)

### 8/15 methods use ENUMs (53%)

| Method | ENUMs Count | Description | Input ENUMs | Output ENUMs |
|--------|-------------|-------------|-------------|--------------|
| **SymbolsTotal** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Count total/selected symbols | - | - |
| **SymbolExist** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Check if symbol exists | - | - |
| **SymbolName** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Get symbol name by index | - | - |
| **SymbolList** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Full list of all available symbol names | - | - |
| **SymbolSelect** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Add/remove symbol from Market Watch | - | - |
| **SymbolIsSynchronized** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Check sync status with server | - | - |
| **SymbolInfoDouble** | **[In: 1, Out: 0]**<br>Total: 1 ENUM (60 values) | Get double properties (Bid, Ask, Point, etc.) | `SymbolInfoDoubleProperty` (60 values) — property selector | - |
| **SymbolInfoInteger** | **[In: 1, Out: 0]**<br>Total: 1 ENUM (37 values) | Get integer properties (Digits, Spread, etc.) | `SymbolInfoIntegerProperty` (37 values) — property selector | - |
| **SymbolInfoString** | **[In: 1, Out: 0]**<br>Total: 1 ENUM (15 values) | Get string properties (Description, etc.) | `SymbolInfoStringProperty` (15 values) — property selector | - |
| **SymbolInfoMarginRate** | **[In: 1, Out: 0]**<br>Total: 1 ENUM (9 values) | Get margin requirements for order types | `ENUM_ORDER_TYPE` (9 values) — order type | - |
| **SymbolInfoTick** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Get last tick data with timestamp | - | - |
| **PriceHistory** | **[In: 1, Out: 0]**<br>Total: 1 ENUM (selector) | Get historical OHLC bars for a symbol/timeframe | `ENUM_TIMEFRAMES` — timeframe, passed as **`int`** (`PERIOD_H1 = 16385`, etc.) | - |
| **SymbolInfoSessionQuote** | **[In: 1, Out: 0]**<br>Total: 1 ENUM (7 values) | Get quote session times | `DayOfWeek` (7 values) — day of week | - |
| **SymbolInfoSessionTrade** | **[In: 1, Out: 0]**<br>Total: 1 ENUM (7 values) | Get trade session times | `DayOfWeek` (7 values) — day of week | - |
| **SymbolParamsMany** | **[In: 1, Out: 13]**<br>Total: 14 ENUMs | Get detailed parameters for multiple symbols | `AH_SYMBOL_PARAMS_MANY_SORT_TYPE` (4 values) — sort mode | **13 ENUMs in SymbolParameters:** `BMT5_ENUM_SYMBOL_SECTOR`, `BMT5_ENUM_SYMBOL_INDUSTRY`, `BMT5_ENUM_SYMBOL_CHART_MODE`, `BMT5_ENUM_SYMBOL_CALC_MODE`, `BMT5_ENUM_SYMBOL_TRADE_MODE`, `BMT5_ENUM_SYMBOL_TRADE_EXECUTION`, `BMT5_ENUM_SYMBOL_SWAP_MODE`, `BMT5_ENUM_DAY_OF_WEEK`, `BMT5_ENUM_ORDER_TYPE_FILLING`, `BMT5_ENUM_ORDER_TYPE`, `BMT5_ENUM_SYMBOL_ORDER_GTC_MODE`, `BMT5_ENUM_SYMBOL_OPTION_MODE`, `BMT5_ENUM_SYMBOL_OPTION_RIGHT` |

---

## 📋 3. Positions & Orders Information (7 methods)

### 4/7 methods use ENUMs (57%)

| Method | ENUMs Count | Description | Input ENUMs | Output ENUMs |
|--------|-------------|-------------|-------------|--------------|
| **PositionsTotal** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Count open positions | - | - |
| **OpenedOrders** | **[In: 1, Out: 6]**<br>Total: 7 ENUMs | Get all opened orders & positions | `BMT5_ENUM_OPENED_ORDER_SORT_TYPE` (4 values) — sort mode | **6 ENUMs:**<br>**In PositionInfo:** `BMT5_ENUM_POSITION_TYPE`, `BMT5_ENUM_POSITION_REASON`<br>**In OpenedOrderInfo:** `BMT5_ENUM_ORDER_TYPE`, `BMT5_ENUM_ORDER_STATE`, `BMT5_ENUM_ORDER_TYPE_FILLING`, `BMT5_ENUM_ORDER_TYPE_TIME` |
| **OpenedOrdersTickets** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Get only ticket numbers (lightweight) | - | - |
| **OrderHistory** | **[In: 1, Out: 7]**<br>Total: 8 ENUMs | Get historical orders with pagination | `BMT5_ENUM_ORDER_HISTORY_SORT_TYPE` (6 values) — sort mode | **7 ENUMs:**<br>**In OrderHistoryData:** `BMT5_ENUM_ORDER_STATE`, `BMT5_ENUM_ORDER_TYPE`, `BMT5_ENUM_ORDER_TYPE_FILLING`, `BMT5_ENUM_ORDER_TYPE_TIME`<br>**In DealHistoryData:** `BMT5_ENUM_DEAL_ENTRY_TYPE`, `BMT5_ENUM_DEAL_TYPE`, `BMT5_ENUM_DEAL_REASON` |
| **AllHistoryOrders** | **[In: 1, Out: 4]**<br>Total: 5 ENUMs | Full order history over a range in one call | `BMT5_ENUM_ORDER_HISTORY_SORT_TYPE` (6 values) — sort mode | **4 ENUMs in HistoryOrderInfo:** `BMT5_ENUM_ORDER_TYPE`, `BMT5_ENUM_ORDER_STATE`, `BMT5_ENUM_ORDER_TYPE_TIME`, `BMT5_ENUM_ORDER_TYPE_FILLING` |
| **PositionsHistory** | **[In: 1, Out: 1]**<br>Total: 2 ENUMs | Get historical positions with P&L | `AH_ENUM_POSITIONS_HISTORY_SORT_TYPE` (4 values) — sort mode | `AH_ENUM_POSITIONS_HISTORY_ORDER_TYPE` (9 values) — order type |
| **TickValueWithSize** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Get tick value and size for symbols | - | - |

---

## 📚 4. Market Depth / DOM (3 methods)

### 1/3 methods use ENUMs (33%)

| Method | ENUMs Count | Description | Input ENUMs | Output ENUMs |
|--------|-------------|-------------|-------------|--------------|
| **MarketBookAdd** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Subscribe to Depth of Market updates | - | - |
| **MarketBookRelease** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Unsubscribe from DOM | - | - |
| **MarketBookGet** | **[In: 0, Out: 1]**<br>Total: 1 ENUM (4 values) | Get current market depth snapshot | - | `BookType` (4 values) — order type in book: `Sell`, `Buy`, `SellMarket`, `BuyMarket` |

---

## 💰 5. Trading Operations (7 methods)

### 7/7 methods use ENUMs (100%)

| Method | ENUMs Count | Description | Input ENUMs | Output ENUMs |
|--------|-------------|-------------|-------------|--------------|
| **OrderSend** | **[In: 2, Out: 0]**<br>Total: 2 ENUMs (13 values) | Send market or pending order | **2 ENUMs:**<br>`TMT5_ENUM_ORDER_TYPE` (9 values) — order type<br>`TMT5_ENUM_ORDER_TYPE_TIME` (4 values) — order lifetime | - |
| **OrderSendWithoutChecks** | **[In: 2, Out: 0]**<br>Total: 2 ENUMs (13 values) | Send order via fast no-pre-checks path | **2 ENUMs (same `OrderSendRequest`):**<br>`TMT5_ENUM_ORDER_TYPE` (9 values)<br>`TMT5_ENUM_ORDER_TYPE_TIME` (4 values) | - |
| **OrderModify** | **[In: 1, Out: 0]**<br>Total: 1 ENUM (4 values) | Modify existing order parameters | `TMT5_ENUM_ORDER_TYPE_TIME` (4 values) — order lifetime | - |
| **OrderClose** | **[In: 0, Out: 1]**<br>Total: 1 ENUM (3 values) | Close market or pending order | - | `MRPC_ORDER_CLOSE_MODE` (3 values): `MrpcMarketOrderClose`, `MrpcMarketOrderPartialClose`, `MrpcPendingOrderRemove` |
| **OrderCheck** | **[In: 4, Out: 0]**<br>Total: 4 ENUMs (23 values) | Validate order before sending | **4 ENUMs in MrpcMqlTradeRequest:**<br>`MRPC_ENUM_TRADE_REQUEST_ACTIONS` (6 values) — trade action type<br>`ENUM_ORDER_TYPE_TF` (9 values) — order type<br>`MRPC_ENUM_ORDER_TYPE_FILLING` (4 values) — filling policy<br>`MRPC_ENUM_ORDER_TYPE_TIME` (4 values) — order lifetime | - |
| **OrderCalcMargin** | **[In: 1, Out: 0]**<br>Total: 1 ENUM (9 values) | Calculate required margin | `ENUM_ORDER_TYPE_TF` (9 values) — order type | - |
| **OrderCalcProfit** | **[In: 1, Out: 0]**<br>Total: 1 ENUM (9 values) | Calculate expected profit/loss | `ENUM_ORDER_TYPE_TF` (9 values) — order type | - |

---

## 📡 6. Streaming Methods (5 methods)

### 3/5 methods use ENUMs (60%)

| Method | ENUMs Count | Description | Input ENUMs | Output ENUMs |
|--------|-------------|-------------|-------------|--------------|
| **OnSymbolTick** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Stream tick data (Bid/Ask updates) | - | - |
| **OnTrade** | **[In: 0, Out: 1 direct]**<br>Total: 11 ENUMs (75 values) | Stream trade events | - | **1 direct + 10 in nested structures:**<br>1. `MT5_SUB_ENUM_EVENT_GROUP_TYPE` (2 values) — OnTradeData.type<br>2-11. **In event_data nested structures:** `SUB_ENUM_POSITION_TYPE` (2), `SUB_ENUM_POSITION_REASON` (4), `SUB_ENUM_ORDER_TYPE` (9), `SUB_ENUM_ORDER_STATE` (10), `SUB_ENUM_DEAL_TYPE` (18), `SUB_ENUM_ORDER_TYPE_TIME` (4), `SUB_ENUM_ORDER_TYPE_FILLING` (4), `SUB_ENUM_ORDER_REASON` (7), `SUB_ENUM_DEAL_ENTRY` (4), `SUB_ENUM_DEAL_REASON` (11) |
| **OnPositionProfit** | **[In: 0, Out: 1]**<br>Total: 1 ENUM (2 values) | Stream position P&L updates | - | `MT5_SUB_ENUM_EVENT_GROUP_TYPE` (2 values) — event type in OnPositionProfitData.type |
| **OnPositionsAndPendingOrdersTickets** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Stream ticket changes | - | - |
| **OnTradeTransaction** | **[In: 0, Out: 11]**<br>Total: 9 unique ENUM types (107 values) | Stream trade transaction events | - | **11 ENUM fields from 9 unique types:**<br>1. `MT5_SUB_ENUM_EVENT_GROUP_TYPE` (2) — OnTradeTransactionData.type<br>2-6. **In MqlTradeTransaction:** `SUB_ENUM_TRADE_TRANSACTION_TYPE` (11), `SUB_ENUM_ORDER_TYPE` (9), `SUB_ENUM_ORDER_STATE` (10), `SUB_ENUM_DEAL_TYPE` (18), `SUB_ENUM_ORDER_TYPE_TIME` (4)<br>7-10. **In MqlTradeRequest:** `SUB_ENUM_TRADE_REQUEST_ACTIONS` (7), `SUB_ENUM_ORDER_TYPE` (9), `SUB_ENUM_ORDER_TYPE_FILLING` (4), `SUB_ENUM_ORDER_TYPE_TIME` (4)<br>11. **In MqlTradeResult:** `MqlErrorTradeCode` (42) |

---

## 📈 7. Trade Statistics (4 methods)

### 0/4 methods use ENUMs (0%)

The trade-stats family takes an optional `since` (`DateTime?`) and (for curve calls) an `excludeSameBars` flag — no enums on input or output.

| Method | ENUMs Count | Description | Input ENUMs | Output ENUMs |
|--------|-------------|-------------|-------------|--------------|
| **TradeStats** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Aggregated statistics (profit factor, drawdowns, per-market) | - | - |
| **TradeStatsEquityHistory** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Statistics + equity curve + trades in one call | - | - |
| **TradeUnrealizedPL** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Per-position unrealized-P/L extrema (MFE/MAE) | - | - |
| **EquityHistory** | **[In: 0, Out: 0]**<br>Total: 0 ENUMs | Account equity curve (balance/equity over time) | - | - |

---

## Important Notes

### C# ENUM Access

Enums live in the `mt5_term_api` namespace and are used directly as C# enum values:

```csharp
using mt5_term_api;

// Enum as a method argument
double balance = await account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountBalance);
var    tick    = await account.SymbolInfoDoubleAsync("EURUSD", SymbolInfoDoubleProperty.SymbolBid);

// Enum inside a request object
var margin = await account.OrderCalcMarginAsync(new OrderCalcMarginRequest
{
    Symbol    = "EURUSD",
    OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
    Volume    = 0.10,
    OpenPrice = tick.Ask
});
```

> **Proto vs C# names.** The generated C# member is PascalCase (`AccountInfoDoublePropertyType.AccountBalance`); its proto `OriginalName` (as seen on the wire / in `.proto` files) is `ACCOUNT_BALANCE`. This table lists the **C# member** names you write in code.

### Special Cases

**1. AccountInfoInteger and SymbolInfoInteger:**

- Some properties return values that **represent** an enum but come back as `long` / `int64`.
- These are **not typed enums** in the reply — just numbers you interpret.

**2. ReturnedCode:**

- The `ReturnedCode` field in trading replies is **not an enum**.
- It is a plain `uint` trade return code (`10009` = done; see [Return Codes](./RETURN_CODES_REFERENCE.md)).

**3. PriceHistory timeframe:**

- `PriceHistoryRequest.Timeframe` is an `int` carrying the MQL5 `ENUM_TIMEFRAMES` value (e.g. `PERIOD_H1 = 16385`), not a generated C# enum. `MT5Sugar.PriceBarsLast` accepts a friendly string (`"H1"`) instead.

---

## See Also

### Documentation by Group

- [Account Information Overview](../MT5Account/1.%20Account_information/Account_Information.Overview.md)
- [Symbol Information Overview](../MT5Account/2.%20Symbol_information/Symbol_Information.Overview.md)
- [Positions & Orders Overview](../MT5Account/3.%20Position_Orders_Information/Position_Orders_Information.Overview.md)
- [Market Depth Overview](../MT5Account/5.%20Market_Depth(DOM)/Market_Depth.Overview.md)
- [Trading Operations Overview](../MT5Account/4.%20Trading_Operattons/Trading_Operations.Overview.md)
- [Streaming Methods Overview](../MT5Account/7.%20Streaming_Methods/Streaming_Methods.Overview.md)
- [Trade Statistics Overview](../MT5Account/8.%20Trade_Statistics/Trade_Statistics.Overview.md)
- [MT5Account Master Overview](../MT5Account/MT5Account.Master.Overview.md)
