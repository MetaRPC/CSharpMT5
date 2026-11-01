# ✅ Getting Symbol Double Properties

> **Request:** double (numeric) property of a symbol from **MT5**. Get prices, volumes, swaps, margins, and other numeric symbol parameters.

**API Information:**

* **SDK wrapper:** `MT5Account.SymbolInfoDoubleAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `SymbolInfoDouble` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `SymbolInfoDouble(SymbolInfoDoubleRequest) → SymbolInfoDoubleReply`
* **Low‑level client (generated):** `MarketInfo.SymbolInfoDouble(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<SymbolInfoDoubleData> SymbolInfoDoubleAsync(
            string symbol,
            SymbolInfoDoubleProperty property,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`SymbolInfoDoubleRequest { symbol: string, type: SymbolInfoDoubleProperty }`


**Reply message:**

`SymbolInfoDoubleReply { data: SymbolInfoDoubleData }` or `{ error: Error }`

---

## 🔽 Input

| Parameter           | Type                        | Required | Description                                               |
| ------------------- | --------------------------- | -------- | --------------------------------------------------------- |
| `symbol`        | `string`                    | ✅       | Symbol name (e.g., `"EURUSD"`)                            |
| `property`      | `SymbolInfoDoubleProperty`  | ✅       | Property to retrieve (see enum below)                     |
| `deadline`          | `DateTime?`                 | ❌       | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`         | ❌       | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `SymbolInfoDoubleData`

| Field   | Type     | Description                  |
| ------- | -------- | ---------------------------- |
| `Value` | `double` | The requested property value |

---

## 🧱 Related enums (from proto)

### `SymbolInfoDoubleProperty` - Grouped by Category

#### **Current Prices (Bid/Ask)**
| Enum Value        | Value | Description                       |
| ----------------- | ----- | --------------------------------- |
| `SymbolBid`      | 0     | Bid - best sell offer             |
| `SymbolBidhigh`  | 1     | Maximal Bid of the day            |
| `SymbolBidlow`   | 2     | Minimal Bid of the day            |
| `SymbolAsk`      | 3     | Ask - best buy offer              |
| `SymbolAskhigh`  | 4     | Maximal Ask of the day            |
| `SymbolAsklow`   | 5     | Minimal Ask of the day            |

#### **Last Price & Volume**
| Enum Value              | Value | Description                     |
| ----------------------- | ----- | ------------------------------- |
| `SymbolLast`           | 6     | Price of the last deal          |
| `SymbolLasthigh`       | 7     | Maximal Last of the day         |
| `SymbolLastlow`        | 8     | Minimal Last of the day         |
| `SymbolVolumeReal`    | 9     | Volume of the last deal         |
| `SymbolVolumehighReal`| 10    | Maximum Volume of the day       |
| `SymbolVolumelowReal` | 11    | Minimum Volume of the day       |

#### **Trading Parameters**
| Enum Value                  | Value | Description                                                  |
| --------------------------- | ----- | ------------------------------------------------------------ |
| `SymbolPoint`              | 13    | Symbol point value (minimum price change)                    |
| `SymbolTradeTickSize`    | 17    | Minimal price change (tick size)                             |
| `SymbolTradeContractSize`| 18    | Trade contract size (lot size in base currency units)        |
| `SymbolVolumeMin`         | 22    | Minimal volume for a deal                                    |
| `SymbolVolumeMax`         | 23    | Maximal volume for a deal                                    |
| `SymbolVolumeStep`        | 24    | Minimal volume change step for deal execution                |
| `SymbolVolumeLimit`       | 25    | Maximum allowed aggregate volume in one direction            |

#### **Tick Values**
| Enum Value                      | Value | Description                                   |
| ------------------------------- | ----- | --------------------------------------------- |
| `SymbolTradeTickValue`       | 14    | Value of SymbolTradeTickValueProfit       |
| `SymbolTradeTickValueProfit`| 15    | Calculated tick price for a profitable position |
| `SymbolTradeTickValueLoss`  | 16    | Calculated tick price for a losing position   |

#### **Swap Values**
| Enum Value           | Value | Description                                                        |
| -------------------- | ----- | ------------------------------------------------------------------ |
| `SymbolSwapLong`   | 26    | Long swap value                                                    |
| `SymbolSwapShort`  | 27    | Short swap value                                                   |
| `SymbolSwapSunday` | 28    | Swap ratio for positions rolled over from Sunday (0, 1, or 3)      |
| `SymbolSwapMonday` | 29    | Swap ratio for Monday → Tuesday                                    |
| `SymbolSwapTuesday`| 30    | Swap ratio for Tuesday → Wednesday                                 |
| `SymbolSwapWednesday`| 31  | Swap ratio for Wednesday → Thursday                                |
| `SymbolSwapThursday` | 32  | Swap ratio for Thursday → Friday                                   |
| `SymbolSwapFriday`   | 33  | Swap ratio for Friday → Saturday                                   |
| `SymbolSwapSaturday` | 34  | Swap ratio for Saturday → Sunday                                   |

#### **Margin Requirements**
| Enum Value                 | Value | Description                                                               |
| -------------------------- | ----- | ------------------------------------------------------------------------- |
| `SymbolMarginInitial`    | 35    | Initial margin for opening a position with 1 lot                          |
| `SymbolMarginMaintenance`| 36    | Maintenance margin for keeping position open                              |
| `SymbolMarginHedged`     | 48    | Hedged margin (for opposite positions)                                    |

#### **Session Data**
| Enum Value                       | Value | Description                                  |
| -------------------------------- | ----- | -------------------------------------------- |
| `SymbolSessionVolume`          | 37    | Summary volume of current session deals      |
| `SymbolSessionTurnover`        | 38    | Summary turnover of the current session      |
| `SymbolSessionInterest`        | 39    | Summary open interest                        |
| `SymbolSessionBuyOrdersVolume`| 40   | Current volume of Buy orders                 |
| `SymbolSessionSellOrdersVolume`| 41  | Current volume of Sell orders                |
| `SymbolSessionOpen`            | 42    | Open price of the current session            |
| `SymbolSessionClose`           | 43    | Close price of the current session           |
| `SymbolSessionAw`              | 44    | Average weighted price of current session    |
| `SymbolSessionPriceSettlement`| 45    | Settlement price of the current session      |
| `SymbolSessionPriceLimitMin` | 46    | Minimal price of the current session         |
| `SymbolSessionPriceLimitMax` | 47    | Maximal price of the current session         |

#### **Price Statistics**
| Enum Value              | Value | Description                                                    |
| ----------------------- | ----- | -------------------------------------------------------------- |
| `SymbolPriceChange`   | 49    | Change relative to previous day end in %                       |
| `SymbolPriceVolatility`| 50   | Price volatility in %                                          |

#### **Options/Derivatives**
| Enum Value                  | Value | Description                                                |
| --------------------------- | ----- | ---------------------------------------------------------- |
| `SymbolOptionStrike`      | 12    | Strike price of an option                                  |
| `SymbolPriceTheoretical`  | 51    | Theoretical option price                                   |
| `SymbolPriceDelta`        | 52    | Option delta (price change per 1 unit of underlying)      |
| `SymbolPriceTheta`        | 53    | Option theta (time decay per day)                          |
| `SymbolPriceGamma`        | 54    | Option gamma (delta change rate)                           |
| `SymbolPriceVega`         | 55    | Option vega (price change per 1% volatility)               |
| `SymbolPriceRho`          | 56    | Option rho (price change per 1% interest rate)             |
| `SymbolPriceOmega`        | 57    | Option omega (elasticity)                                  |
| `SymbolPriceSensitivity`  | 58    | Option sensitivity                                         |

#### **Bonds/Special**
| Enum Value                     | Value | Description                              |
| ------------------------------ | ----- | ---------------------------------------- |
| `SymbolTradeAccruedInterest`| 19    | Accrued interest (bonds)                 |
| `SymbolTradeFaceValue`      | 20    | Face value (bonds)                       |
| `SymbolTradeLiquidityRate`  | 21    | Liquidity rate (asset share for margin)  |

#### **Internal**
| Enum Value     | Value | Description                       |
| -------------- | ----- | --------------------------------- |
| `SymbolCount` | 59    | The count of symbol properties    |

> **MQL5 Reference:** [SymbolInfoDouble](https://www.mql5.com/en/docs/marketinformation/symbolinfodouble)

---

## 💬 Just the essentials

* **What it is.** Single RPC returning one numeric property of a symbol.
* **Why you need it.** Get current prices, calculate position sizes, check trading constraints, retrieve margin requirements.
* **Most common.** `SymbolBid`, `SymbolAsk`, `SymbolPoint`, `SymbolVolumeMin/MAX/STEP`, `SymbolTradeContractSize`.
* **60 properties** covering prices, volumes, swaps, margins, sessions, options.

---

## 🎯 Purpose

Use this method when you need to:

* Get current Bid/Ask prices (alternative to `SymbolInfoTickAsync`).
* Calculate position sizes based on volume constraints.
* Retrieve tick value for profit calculations.
* Check swap values before holding overnight positions.
* Get margin requirements for risk management.
* Obtain contract size for lot-to-units conversion.

---

## 🧩 Notes & Tips

* **SymbolBid/ASK** - use `SymbolInfoTickAsync()` for faster Bid/Ask retrieval.
* **SymbolPoint** - minimum price change (e.g., 0.00001 for EURUSD).
* **SymbolVolumeMin/MAX/STEP** - critical for validating lot sizes before trading.
* **SymbolTradeContractSize** - lot size in base currency units (e.g., 100,000 for Forex standard lot).
* **Swap values** - charged for positions held overnight (positive = you earn, negative = you pay).
* **Margin requirements** - used for calculating required margin before opening positions.
* Properties are cached - use short timeout (3-5s).
* Some properties may return 0 if not applicable (e.g., options Greeks for Forex).

---

## 🔗 Usage Examples

### 1) Get Bid/Ask prices

```csharp
// Get current Bid and Ask
var bid = await acc.SymbolInfoDoubleAsync(
    "EURUSD",
    SymbolInfoDoubleProperty.SymbolBid,
    deadline: DateTime.UtcNow.AddSeconds(3));
var ask = await acc.SymbolInfoDoubleAsync(
    "EURUSD",
    SymbolInfoDoubleProperty.SymbolAsk);

Console.WriteLine($"EURUSD Bid: {bid.Value:F5}, Ask: {ask.Value:F5}");
```

### 2) Get volume constraints

```csharp
// Retrieve trading volume limits
var volumeMin = await acc.SymbolInfoDoubleAsync(
    "GBPUSD",
    SymbolInfoDoubleProperty.SymbolVolumeMin);
var volumeMax = await acc.SymbolInfoDoubleAsync(
    "GBPUSD",
    SymbolInfoDoubleProperty.SymbolVolumeMax);
var volumeStep = await acc.SymbolInfoDoubleAsync(
    "GBPUSD",
    SymbolInfoDoubleProperty.SymbolVolumeStep);

Console.WriteLine($"GBPUSD Volume:");
Console.WriteLine($"  Min:  {volumeMin.Value:F2} lots");
Console.WriteLine($"  Max:  {volumeMax.Value:F2} lots");
Console.WriteLine($"  Step: {volumeStep.Value:F2} lots");
```

### 3) Calculate position size

```csharp
// Get parameters for position size calculation
var symbol = "EURUSD";

var point = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint);
var contractSize = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolTradeContractSize);
var volumeMin = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeMin);

Console.WriteLine($"{symbol} Trading Parameters:");
Console.WriteLine($"  Point:         {point.Value}");
Console.WriteLine($"  Contract Size: {contractSize.Value:F0}");
Console.WriteLine($"  Min Volume:    {volumeMin.Value:F2}");
```

### 4) Check swap costs

```csharp
// Get swap values for overnight positions
var swapLong = await acc.SymbolInfoDoubleAsync(
    "EURUSD",
    SymbolInfoDoubleProperty.SymbolSwapLong);
var swapShort = await acc.SymbolInfoDoubleAsync(
    "EURUSD",
    SymbolInfoDoubleProperty.SymbolSwapShort);

Console.WriteLine($"EURUSD Swap:");
Console.WriteLine($"  Long:  {swapLong.Value:F2}");
Console.WriteLine($"  Short: {swapShort.Value:F2}");

if (swapLong.Value < 0)
    Console.WriteLine("  ⚠️ Long positions pay swap");
else
    Console.WriteLine("  ✅ Long positions earn swap");
```

### 5) Get margin requirements

```csharp
// Check margin needed for 1 lot
var marginInit = await acc.SymbolInfoDoubleAsync(
    "XAUUSD",
    SymbolInfoDoubleProperty.SymbolMarginInitial);

Console.WriteLine($"XAUUSD Initial Margin: {marginInit.Value:F2}");
Console.WriteLine($"(Required margin for opening 1 lot)");
```
