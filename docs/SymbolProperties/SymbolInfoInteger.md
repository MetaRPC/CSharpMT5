# Retrieving an Integer-Precision Symbol Property

> **Request:** a specific integer property (Visible, Digits, Spread, etc.) for a symbol from MT5
> Fetch any integer‐type market data value for a given symbol using one universal method.

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

✨ **Method Signature:**

```csharp
Task<SymbolInfoIntegerData> SymbolInfoIntegerAsync(
    string symbol,
    SymbolInfoIntegerProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## Input

* **`symbol`** (`string`) — the financial instrument name (e.g., `"EURUSD"`).
* **`property`** (`SymbolInfoIntegerProperty`) — which integer property to fetch. Possible values:

  * **SymbolSubscriptionDelay** (`SYMBOL_SUBSCRIPTION_DELAY`) — data arrives with a delay (`bool`).
  * **SymbolSector** (`SYMBOL_SECTOR`) — economic sector of the asset (`ENUM_SYMBOL_SECTOR`).
  * **SymbolIndustry** (`SYMBOL_INDUSTRY`) — industry branch (`ENUM_SYMBOL_INDUSTRY`).
  * **SymbolCustom** (`SYMBOL_CUSTOM`) — is a synthetic/custom symbol (`bool`).
  * **SymbolBackgroundColor** (`SYMBOL_BACKGROUND_COLOR`) — background color in Market Watch (`color`).
  * **SymbolChartMode** (`SYMBOL_CHART_MODE`) — price type for bars (`ENUM_SYMBOL_CHART_MODE`).
  * **SymbolExist** (`SYMBOL_EXIST`) — symbol exists (`bool`).
  * **SymbolSelect** (`SYMBOL_SELECT`) — symbol selected in Market Watch (`bool`).
  * **SymbolVisible** (`SYMBOL_VISIBLE`) — symbol visible in Market Watch (`bool`).
  * **SymbolSessionDeals** (`SYMBOL_SESSION_DEALS`) — deals count in current session (`long`).
  * **SymbolSessionBuyOrders** (`SYMBOL_SESSION_BUY_ORDERS`) — buy orders count (`long`).
  * **SymbolSessionSellOrders** (`SYMBOL_SESSION_SELL_ORDERS`) — sell orders count (`long`).
  * **SymbolVolume** (`SYMBOL_VOLUME`) — volume of last deal (`long`).
  * **SymbolVolumeHigh** (`SYMBOL_VOLUMEHIGH`) — maximal day volume (`long`).
  * **SymbolVolumeLow** (`SYMBOL_VOLUMELOW`) — minimal day volume (`long`).
  * **SymbolTime** (`SYMBOL_TIME`) — time of last quote (`DateTime`).
  * **SymbolTimeMsc** (`SYMBOL_TIME_MSC`) — last quote time in ms since 1970 (`long`).
  * **SymbolDigits** (`SYMBOL_DIGITS`) — number of decimal places (`int`).
  * **SymbolSpreadFloat** (`SYMBOL_SPREAD_FLOAT`) — floating spread indication (`bool`).
  * **SymbolSpread** (`SYMBOL_SPREAD`) — spread in points (`int`).
  * **SymbolTicksBookDepth** (`SYMBOL_TICKS_BOOKDEPTH`) — depth-of-market request depth (`int`).
  * **SymbolTradeCalcMode** (`SYMBOL_TRADE_CALC_MODE`) — contract price calculation mode (`ENUM_SYMBOL_CALC_MODE`).
  * **SymbolTradeMode** (`SYMBOL_TRADE_MODE`) — order execution type (`ENUM_SYMBOL_TRADE_MODE`).
  * **SymbolStartTime** (`SYMBOL_START_TIME`) — symbol trading start time (`DateTime`).
  * **SymbolExpirationTime** (`SYMBOL_EXPIRATION_TIME`) — trading end time for futures (`DateTime`).
  * **SymbolTradeStopsLevel** (`SYMBOL_TRADE_STOPS_LEVEL`) — minimal stop order distance (`int`).
  * **SymbolTradeFreezeLevel** (`SYMBOL_TRADE_FREEZE_LEVEL`) — trade freeze distance (`int`).
  * **SymbolTradeExemode** (`SYMBOL_TRADE_EXEMODE`) — trade execution mode (`ENUM_SYMBOL_TRADE_EXECUTION`).
  * **SymbolSwapMode** (`SYMBOL_SWAP_MODE`) — swap calculation model (`ENUM_SYMBOL_SWAP_MODE`).
  * **SymbolSwapRollover3Days** (`SYMBOL_SWAP_ROLLOVER3DAYS`) — 3-day swap rollover day (`ENUM_DAY_OF_WEEK`).
  * **SymbolMarginHedgedUseLeg** (`SYMBOL_MARGIN_HEDGED_USE_LEG`) — hedged margin calculation flag (`bool`).
  * **SymbolExpirationMode** (`SYMBOL_EXPIRATION_MODE`) — allowed expiration modes flags (`int`).
  * **SymbolFillingMode** (`SYMBOL_FILLING_MODE`) — allowed filling modes flags (`int`).
  * **SymbolOrderMode** (`SYMBOL_ORDER_MODE`) — allowed order types flags (`int`).
  * **SymbolOrderGtcMode** (`SYMBOL_ORDER_GTC_MODE`) — GTC order expiration mode (`ENUM_SYMBOL_ORDER_GTC_MODE`).
  * **SymbolOptionMode** (`SYMBOL_OPTION_MODE`) — option type (`ENUM_SYMBOL_OPTION_MODE`).
  * **SymbolOptionRight** (`SYMBOL_OPTION_RIGHT`) — option right (Call/Put) (`ENUM_SYMBOL_OPTION_RIGHT`).

---

## Output

**`SymbolInfoIntegerData`** — structure with the following field:

* **Value** (`long`) — the requested integer property value.

---

## Purpose

Use a single, consistent endpoint for all integer‐type symbol properties; simply swap the enum to retrieve any integer metric. 🚀
