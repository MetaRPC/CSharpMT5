# Retrieving a Double-Precision Symbol Property

> **Request:** a specific double property (Bid, Ask, Last, etc.) for a symbol from MT5
> Fetch any floating-point market data value for a given symbol using one universal method.

---

### Code Example

```csharp
var doubleProp = await _mt5Account.SymbolInfoDoubleAsync(
    Constants.DefaultSymbol,
    SymbolInfoDoubleProperty.SymbolAsk
);
_logger.LogInformation(
    "SymbolInfoDouble: Ask={Ask}",
    doubleProp.Value
);
```

---

### Method Signature

```csharp
Task<SymbolInfoDoubleData> SymbolInfoDoubleAsync(
    string symbol,
    SymbolInfoDoubleProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

| Parameter  | Type                       | Description                                  |
| ---------- | -------------------------- | -------------------------------------------- |
| `symbol`   | `string`                   | Symbol name (e.g., "EURUSD")                 |
| `property` | `SymbolInfoDoubleProperty` | Enum specifying which double property to get |

### `SymbolInfoDoubleProperty` Enum Values

| Enum Value                      | Description                          |
| ------------------------------- | ------------------------------------ |
| `SymbolBid`                     | Current Bid price                    |
| `SymbolBidHigh`                 | Highest Bid of the day               |
| `SymbolBidLow`                  | Lowest Bid of the day                |
| `SymbolAsk`                     | Current Ask price                    |
| `SymbolAskHigh`                 | Highest Ask of the day               |
| `SymbolAskLow`                  | Lowest Ask of the day                |
| `SymbolLast`                    | Last deal price                      |
| `SymbolLastHigh`                | Highest Last price of the day        |
| `SymbolLastLow`                 | Lowest Last price of the day         |
| `SymbolVolumeReal`              | Last deal volume                     |
| `SymbolVolumeHighReal`          | Highest deal volume of the day       |
| `SymbolVolumeLowReal`           | Lowest deal volume of the day        |
| `SymbolOptionStrike`            | Option strike price                  |
| `SymbolPoint`                   | Point size                           |
| `SymbolTradeTickValue`          | Tick value                           |
| `SymbolTradeTickValueProfit`    | Tick value for profit                |
| `SymbolTradeTickValueLoss`      | Tick value for loss                  |
| `SymbolTradeTickSize`           | Minimal price change                 |
| `SymbolTradeContractSize`       | Contract size per lot                |
| `SymbolTradeAccruedInterest`    | Accrued coupon interest              |
| `SymbolTradeFaceValue`          | Bond face value                      |
| `SymbolTradeLiquidityRate`      | Liquidity rate                       |
| `SymbolVolumeMin`               | Minimum volume per deal              |
| `SymbolMarginMaintenance`       | Maintenance margin per lot           |
| `SymbolSessionVolume`           | Total session deal volume            |
| `SymbolSessionTurnover`         | Total session turnover               |
| `SymbolSessionInterest`         | Open interest                        |
| `SymbolSessionBuyOrdersVolume`  | Buy order volume during session      |
| `SymbolSessionSellOrdersVolume` | Sell order volume during session     |
| `SymbolSessionOpen`             | Session open price                   |
| `SymbolSessionClose`            | Session close price                  |
| `SymbolSessionAw`               | Session average weighted price       |
| `SymbolSessionPriceSettlement`  | Session settlement price             |
| `SymbolSessionPriceLimitMin`    | Minimum session price                |
| `SymbolSessionPriceLimitMax`    | Maximum session price                |
| `SymbolMarginHedged`            | Hedged margin per lot                |
| `SymbolPriceChange`             | Price change from previous close (%) |
| `SymbolPriceVolatility`         | Price volatility (%)                 |
| `SymbolPriceTheoretical`        | Theoretical option price             |
| `SymbolPriceDelta`              | Option delta                         |
| `SymbolPriceTheta`              | Option theta                         |
| `SymbolPriceGamma`              | Option gamma                         |
| `SymbolPriceVega`               | Option vega                          |
| `SymbolPriceRho`                | Option rho                           |
| `SymbolPriceOmega`              | Option omega                         |
| `SymbolPriceSensitivity`        | Option sensitivity                   |
| `SymbolSwapLong`                | Swap size for long position          |
| `SymbolSwapShort`               | Swap size for short position         |
| `SymbolSwapSunday`              | Sunday rollover multiplier           |
| `SymbolSwapMonday`              | Monday rollover multiplier           |
| `SymbolSwapTuesday`             | Tuesday rollover multiplier          |
| `SymbolSwapWednesday`           | Wednesday rollover multiplier        |
| `SymbolSwapThursday`            | Thursday rollover multiplier         |
| `SymbolSwapFriday`              | Friday rollover multiplier           |
| `SymbolSwapSaturday`            | Saturday rollover multiplier         |
| `SymbolVolumeMax`               | Maximum volume per deal              |
| `SymbolVolumeStep`              | Minimum volume step                  |
| `SymbolVolumeLimit`             | Max total volume per direction       |

---

## ⬆️ Output

Returns a **SymbolInfoDoubleData** object:

| Field   | Type     | Description                             |
| ------- | -------- | --------------------------------------- |
| `Value` | `double` | Numeric value of the requested property |

---

## 🎯 Purpose

Use this method to fetch **any floating-point market or trading value** for a given symbol.

Ideal for:

* Building flexible symbol-based UIs
* Pricing/risk modeling based on live data
* Eliminating the need for multiple API endpoints — just use a different enum!
