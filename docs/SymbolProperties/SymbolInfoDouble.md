# Retrieving a Double-Precision Symbol Property

> **Request:** a specific double property (Bid, Ask, Last, etc.) for a symbol from MT5
> Fetch any floating-point market data value for a given symbol using one universal method.

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

✨ **Method Signature:**

```csharp
Task<SymbolInfoDoubleData> SymbolInfoDoubleAsync(
    string symbol,
    SymbolInfoDoubleProperty property,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## Input

* **symbol** (`string`) — the symbol name (e.g., `Constants.DefaultSymbol`).
* **property** (`SymbolInfoDoubleProperty`) — which double value to fetch. Possible values:

  * **SymbolBid** (`SYMBOL_BID`) — Bid - best sell offer.
  * **SymbolBidHigh** (`SYMBOL_BIDHIGH`) — maximal Bid of the day.
  * **SymbolBidLow** (`SYMBOL_BIDLOW`) — minimal Bid of the day.
  * **SymbolAsk** (`SYMBOL_ASK`) — Ask - best buy offer.
  * **SymbolAskHigh** (`SYMBOL_ASKHIGH`) — maximal Ask of the day.
  * **SymbolAskLow** (`SYMBOL_ASKLOW`) — minimal Ask of the day.
  * **SymbolLast** (`SYMBOL_LAST`) — price of the last deal.
  * **SymbolLastHigh** (`SYMBOL_LASTHIGH`) — maximal Last of the day.
  * **SymbolLastLow** (`SYMBOL_LASTLOW`) — minimal Last of the day.
  * **SymbolVolumeReal** (`SYMBOL_VOLUME_REAL`) — volume of the last deal.
  * **SymbolVolumeHighReal** (`SYMBOL_VOLUMEHIGH_REAL`) — maximum volume of the day.
  * **SymbolVolumeLowReal** (`SYMBOL_VOLUMELOW_REAL`) — minimum volume of the day.
  * **SymbolOptionStrike** (`SYMBOL_OPTION_STRIKE`) — strike price of an option.
  * **SymbolPoint** (`SYMBOL_POINT`) — symbol point value.
  * **SymbolTradeTickValue** (`SYMBOL_TRADE_TICK_VALUE`) — base tick price derivation ([mql5.com](https://www.mql5.com/en/docs/constants/environment_state/marketinfoconstants))
  * **SymbolTradeTickValueProfit** (`SYMBOL_TRADE_TICK_VALUE_PROFIT`) — tick price for profitable position.
  * **SymbolTradeTickValueLoss** (`SYMBOL_TRADE_TICK_VALUE_LOSS`) — tick price for losing position.
  * **SymbolTradeTickSize** (`SYMBOL_TRADE_TICK_SIZE`) — minimal price change.
  * **SymbolTradeContractSize** (`SYMBOL_TRADE_CONTRACT_SIZE`) — contract size per lot.
  * **SymbolTradeAccruedInterest** (`SYMBOL_TRADE_ACCRUED_INTEREST`) — accumulated coupon interest.
  * **SymbolTradeFaceValue** (`SYMBOL_TRADE_FACE_VALUE`) — bond face value.
  * **SymbolTradeLiquidityRate** (`SYMBOL_TRADE_LIQUIDITY_RATE`) — liquidity rate.
  * **SymbolVolumeMin** (`SYMBOL_VOLUME_MIN`) — minimal volume for a deal.
  * **SymbolMarginMaintenance** (`SYMBOL_MARGIN_MAINTENANCE`) — maintenance margin per lot.
  * **SymbolSessionVolume** (`SYMBOL_SESSION_VOLUME`) — summary volume of session deals.
  * **SymbolSessionTurnover** (`SYMBOL_SESSION_TURNOVER`) — total session turnover.
  * **SymbolSessionInterest** (`SYMBOL_SESSION_INTEREST`) — open interest.
  * **SymbolSessionBuyOrdersVolume** (`SYMBOL_SESSION_BUY_ORDERS_VOLUME`) — buy orders volume.
  * **SymbolSessionSellOrdersVolume** (`SYMBOL_SESSION_SELL_ORDERS_VOLUME`) — sell orders volume.
  * **SymbolSessionOpen** (`SYMBOL_SESSION_OPEN`) — session open price.
  * **SymbolSessionClose** (`SYMBOL_SESSION_CLOSE`) — session close price.
  * **SymbolSessionAw** (`SYMBOL_SESSION_AW`) — average weighted session price.
  * **SymbolSessionPriceSettlement** (`SYMBOL_SESSION_PRICE_SETTLEMENT`) — session settlement price.
  * **SymbolSessionPriceLimitMin** (`SYMBOL_SESSION_PRICE_LIMIT_MIN`) — session minimum price.
  * **SymbolSessionPriceLimitMax** (`SYMBOL_SESSION_PRICE_LIMIT_MAX`) — session maximum price.
  * **SymbolMarginHedged** (`SYMBOL_MARGIN_HEDGED`) — hedged margin per lot.
  * **SymbolPriceChange** (`SYMBOL_PRICE_CHANGE`) — price change since previous close (%).
  * **SymbolPriceVolatility** (`SYMBOL_PRICE_VOLATILITY`) — price volatility (%).
  * **SymbolPriceTheoretical** (`SYMBOL_PRICE_THEORETICAL`) — theoretical option price.
  * **SymbolPriceDelta** (`SYMBOL_PRICE_DELTA`) — option delta.
  * **SymbolPriceTheta** (`SYMBOL_PRICE_THETA`) — option theta.
  * **SymbolPriceGamma** (`SYMBOL_PRICE_GAMMA`) — option gamma.
  * **SymbolPriceVega** (`SYMBOL_PRICE_VEGA`) — option vega.
  * **SymbolPriceRho** (`SYMBOL_PRICE_RHO`) — option rho.
  * **SymbolPriceOmega** (`SYMBOL_PRICE_OMEGA`) — option omega.
  * **SymbolPriceSensitivity** (`SYMBOL_PRICE_SENSITIVITY`) — option sensitivity ([mql5.com](https://www.mql5.com/en/docs/constants/environment_state/marketinfoconstants))
  * **SymbolSwapLong** (`SYMBOL_SWAP_LONG`) — swap size for a long position.
  * **SymbolSwapShort** (`SYMBOL_SWAP_SHORT`) — swap size for a short position.
  * **SymbolSwapSunday** (`SYMBOL_SWAP_SUNDAY`) — Sunday rollover multiplier (0/1/3).
  * …and so on for Monday…Saturday.
  * **SymbolVolumeMax** (`SYMBOL_VOLUME_MAX`) — maximal volume for a single deal.
  * **SymbolVolumeStep** (`SYMBOL_VOLUME_STEP`) — minimal volume change increment.
  * **SymbolVolumeLimit** (`SYMBOL_VOLUME_LIMIT`) — max aggregate volume per direction.

---

## Output

**`SymbolInfoDoubleData`** — structure with the following field:

* **Value** (`double`) — the requested numeric property value.

---

## Purpose

Keep your code DRY by using a single endpoint for all double-type symbol properties; just swap the enum and you’re set! 🚀
