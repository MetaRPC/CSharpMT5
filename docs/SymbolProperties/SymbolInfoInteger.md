# Retrieving an Integer-Precision Symbol Property

> **Request:** a specific integer property (Visible, Digits, Spread, etc.) for a symbol from MT5
> Fetch any integer‐type market data value for a given symbol using one universal method.

---

### Code Example

```csharp
var intProp = await _mt5Account.SymbolInfoIntegerAsync(
    Constants.DefaultSymbol,
    SymbolInfoIntegerProperty.SymbolVisible
);
_logger.LogInformation(
    "SymbolInfoInteger: Visible={Visible}",
    intProp.Value
);
```

---

### Method Signature

```csharp
Task<SymbolInfoIntegerData> SymbolInfoIntegerAsync(
    string symbol,
    SymbolInfoIntegerProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

| Parameter  | Type                        | Description                                   |
| ---------- | --------------------------- | --------------------------------------------- |
| `symbol`   | `string`                    | Symbol name (e.g., "EURUSD")                  |
| `property` | `SymbolInfoIntegerProperty` | Enum specifying which integer property to get |

### `SymbolInfoIntegerProperty` Enum Values

| Enum Value                 | MQL5 Const                     | Description                        |
| -------------------------- | ------------------------------ | ---------------------------------- |
| `SymbolSubscriptionDelay`  | `SYMBOL_SUBSCRIPTION_DELAY`    | Whether the data feed has a delay  |
| `SymbolSector`             | `SYMBOL_SECTOR`                | Sector code of the asset           |
| `SymbolIndustry`           | `SYMBOL_INDUSTRY`              | Industry code                      |
| `SymbolCustom`             | `SYMBOL_CUSTOM`                | Custom (synthetic) symbol          |
| `SymbolBackgroundColor`    | `SYMBOL_BACKGROUND_COLOR`      | Background color for symbol        |
| `SymbolChartMode`          | `SYMBOL_CHART_MODE`            | Price type for bar chart           |
| `SymbolExist`              | `SYMBOL_EXIST`                 | Whether the symbol exists          |
| `SymbolSelect`             | `SYMBOL_SELECT`                | Whether selected in Market Watch   |
| `SymbolVisible`            | `SYMBOL_VISIBLE`               | Whether visible in Market Watch    |
| `SymbolSessionDeals`       | `SYMBOL_SESSION_DEALS`         | Number of deals in current session |
| `SymbolSessionBuyOrders`   | `SYMBOL_SESSION_BUY_ORDERS`    | Count of buy orders in session     |
| `SymbolSessionSellOrders`  | `SYMBOL_SESSION_SELL_ORDERS`   | Count of sell orders in session    |
| `SymbolVolume`             | `SYMBOL_VOLUME`                | Last deal volume                   |
| `SymbolVolumeHigh`         | `SYMBOL_VOLUMEHIGH`            | Max volume of the day              |
| `SymbolVolumeLow`          | `SYMBOL_VOLUMELOW`             | Min volume of the day              |
| `SymbolTime`               | `SYMBOL_TIME`                  | Time of last quote                 |
| `SymbolTimeMsc`            | `SYMBOL_TIME_MSC`              | Time of last quote (ms since 1970) |
| `SymbolDigits`             | `SYMBOL_DIGITS`                | Number of decimal digits           |
| `SymbolSpreadFloat`        | `SYMBOL_SPREAD_FLOAT`          | Whether spread is floating         |
| `SymbolSpread`             | `SYMBOL_SPREAD`                | Spread in points                   |
| `SymbolTicksBookDepth`     | `SYMBOL_TICKS_BOOKDEPTH`       | Market depth                       |
| `SymbolTradeCalcMode`      | `SYMBOL_TRADE_CALC_MODE`       | Calculation mode                   |
| `SymbolTradeMode`          | `SYMBOL_TRADE_MODE`            | Trading mode                       |
| `SymbolStartTime`          | `SYMBOL_START_TIME`            | Trading start time                 |
| `SymbolExpirationTime`     | `SYMBOL_EXPIRATION_TIME`       | Expiration time                    |
| `SymbolTradeStopsLevel`    | `SYMBOL_TRADE_STOPS_LEVEL`     | Minimum distance for stops         |
| `SymbolTradeFreezeLevel`   | `SYMBOL_TRADE_FREEZE_LEVEL`    | Freeze level in points             |
| `SymbolTradeExemode`       | `SYMBOL_TRADE_EXEMODE`         | Order execution mode               |
| `SymbolSwapMode`           | `SYMBOL_SWAP_MODE`             | Swap calculation mode              |
| `SymbolSwapRollover3Days`  | `SYMBOL_SWAP_ROLLOVER3DAYS`    | 3-day rollover day                 |
| `SymbolMarginHedgedUseLeg` | `SYMBOL_MARGIN_HEDGED_USE_LEG` | Margin hedge calculation setting   |
| `SymbolExpirationMode`     | `SYMBOL_EXPIRATION_MODE`       | Expiration mode flags              |
| `SymbolFillingMode`        | `SYMBOL_FILLING_MODE`          | Allowed filling modes              |
| `SymbolOrderMode`          | `SYMBOL_ORDER_MODE`            | Allowed order types                |
| `SymbolOrderGtcMode`       | `SYMBOL_ORDER_GTC_MODE`        | Order expiration mode (GTC)        |
| `SymbolOptionMode`         | `SYMBOL_OPTION_MODE`           | Option type                        |
| `SymbolOptionRight`        | `SYMBOL_OPTION_RIGHT`          | Option right (Call/Put)            |

---

## ⬆️ Output

Returns a **SymbolInfoIntegerData** object:

| Field   | Type   | Description                             |
| ------- | ------ | --------------------------------------- |
| `Value` | `long` | Integer value of the requested property |

---

## 🎯 Purpose

Use this method to fetch **any integer-type symbol property** in a centralized and flexible way.

Ideal for:

* Symbol metadata introspection
* Visibility control in UI
* Contract spec modeling without hardcoding constant values
