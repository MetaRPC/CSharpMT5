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
            string symbolName,
            SymbolInfoDoubleProperty propertyType,
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
| `symbolName`        | `string`                    | ✅       | Symbol name (e.g., `"EURUSD"`)                            |
| `propertyType`      | `SymbolInfoDoubleProperty`  | ✅       | Property to retrieve (see enum below)                     |
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
| `SYMBOL_BID`      | 0     | Bid - best sell offer             |
| `SYMBOL_BIDHIGH`  | 1     | Maximal Bid of the day            |
| `SYMBOL_BIDLOW`   | 2     | Minimal Bid of the day            |
| `SYMBOL_ASK`      | 3     | Ask - best buy offer              |
| `SYMBOL_ASKHIGH`  | 4     | Maximal Ask of the day            |
| `SYMBOL_ASKLOW`   | 5     | Minimal Ask of the day            |

#### **Last Price & Volume**
| Enum Value              | Value | Description                     |
| ----------------------- | ----- | ------------------------------- |
| `SYMBOL_LAST`           | 6     | Price of the last deal          |
| `SYMBOL_LASTHIGH`       | 7     | Maximal Last of the day         |
| `SYMBOL_LASTLOW`        | 8     | Minimal Last of the day         |
| `SYMBOL_VOLUME_REAL`    | 9     | Volume of the last deal         |
| `SYMBOL_VOLUMEHIGH_REAL`| 10    | Maximum Volume of the day       |
| `SYMBOL_VOLUMELOW_REAL` | 11    | Minimum Volume of the day       |

#### **Trading Parameters**
| Enum Value                  | Value | Description                                                  |
| --------------------------- | ----- | ------------------------------------------------------------ |
| `SYMBOL_POINT`              | 13    | Symbol point value (minimum price change)                    |
| `SYMBOL_TRADE_TICK_SIZE`    | 17    | Minimal price change (tick size)                             |
| `SYMBOL_TRADE_CONTRACT_SIZE`| 18    | Trade contract size (lot size in base currency units)        |
| `SYMBOL_VOLUME_MIN`         | 22    | Minimal volume for a deal                                    |
| `SYMBOL_VOLUME_MAX`         | 23    | Maximal volume for a deal                                    |
| `SYMBOL_VOLUME_STEP`        | 24    | Minimal volume change step for deal execution                |
| `SYMBOL_VOLUME_LIMIT`       | 25    | Maximum allowed aggregate volume in one direction            |

#### **Tick Values**
| Enum Value                      | Value | Description                                   |
| ------------------------------- | ----- | --------------------------------------------- |
| `SYMBOL_TRADE_TICK_VALUE`       | 14    | Value of SYMBOL_TRADE_TICK_VALUE_PROFIT       |
| `SYMBOL_TRADE_TICK_VALUE_PROFIT`| 15    | Calculated tick price for a profitable position |
| `SYMBOL_TRADE_TICK_VALUE_LOSS`  | 16    | Calculated tick price for a losing position   |

#### **Swap Values**
| Enum Value           | Value | Description                                                        |
| -------------------- | ----- | ------------------------------------------------------------------ |
| `SYMBOL_SWAP_LONG`   | 26    | Long swap value                                                    |
| `SYMBOL_SWAP_SHORT`  | 27    | Short swap value                                                   |
| `SYMBOL_SWAP_SUNDAY` | 28    | Swap ratio for positions rolled over from Sunday (0, 1, or 3)      |
| `SYMBOL_SWAP_MONDAY` | 29    | Swap ratio for Monday → Tuesday                                    |
| `SYMBOL_SWAP_TUESDAY`| 30    | Swap ratio for Tuesday → Wednesday                                 |
| `SYMBOL_SWAP_WEDNESDAY`| 31  | Swap ratio for Wednesday → Thursday                                |
| `SYMBOL_SWAP_THURSDAY` | 32  | Swap ratio for Thursday → Friday                                   |
| `SYMBOL_SWAP_FRIDAY`   | 33  | Swap ratio for Friday → Saturday                                   |
| `SYMBOL_SWAP_SATURDAY` | 34  | Swap ratio for Saturday → Sunday                                   |

#### **Margin Requirements**
| Enum Value                 | Value | Description                                                               |
| -------------------------- | ----- | ------------------------------------------------------------------------- |
| `SYMBOL_MARGIN_INITIAL`    | 35    | Initial margin for opening a position with 1 lot                          |
| `SYMBOL_MARGIN_MAINTENANCE`| 36    | Maintenance margin for keeping position open                              |
| `SYMBOL_MARGIN_HEDGED`     | 48    | Hedged margin (for opposite positions)                                    |

#### **Session Data**
| Enum Value                       | Value | Description                                  |
| -------------------------------- | ----- | -------------------------------------------- |
| `SYMBOL_SESSION_VOLUME`          | 37    | Summary volume of current session deals      |
| `SYMBOL_SESSION_TURNOVER`        | 38    | Summary turnover of the current session      |
| `SYMBOL_SESSION_INTEREST`        | 39    | Summary open interest                        |
| `SYMBOL_SESSION_BUY_ORDERS_VOLUME`| 40   | Current volume of Buy orders                 |
| `SYMBOL_SESSION_SELL_ORDERS_VOLUME`| 41  | Current volume of Sell orders                |
| `SYMBOL_SESSION_OPEN`            | 42    | Open price of the current session            |
| `SYMBOL_SESSION_CLOSE`           | 43    | Close price of the current session           |
| `SYMBOL_SESSION_AW`              | 44    | Average weighted price of current session    |
| `SYMBOL_SESSION_PRICE_SETTLEMENT`| 45    | Settlement price of the current session      |
| `SYMBOL_SESSION_PRICE_LIMIT_MIN` | 46    | Minimal price of the current session         |
| `SYMBOL_SESSION_PRICE_LIMIT_MAX` | 47    | Maximal price of the current session         |

#### **Price Statistics**
| Enum Value              | Value | Description                                                    |
| ----------------------- | ----- | -------------------------------------------------------------- |
| `SYMBOL_PRICE_CHANGE`   | 49    | Change relative to previous day end in %                       |
| `SYMBOL_PRICE_VOLATILITY`| 50   | Price volatility in %                                          |

#### **Options/Derivatives**
| Enum Value                  | Value | Description                                                |
| --------------------------- | ----- | ---------------------------------------------------------- |
| `SYMBOL_OPTION_STRIKE`      | 12    | Strike price of an option                                  |
| `SYMBOL_PRICE_THEORETICAL`  | 51    | Theoretical option price                                   |
| `SYMBOL_PRICE_DELTA`        | 52    | Option delta (price change per 1 unit of underlying)      |
| `SYMBOL_PRICE_THETA`        | 53    | Option theta (time decay per day)                          |
| `SYMBOL_PRICE_GAMMA`        | 54    | Option gamma (delta change rate)                           |
| `SYMBOL_PRICE_VEGA`         | 55    | Option vega (price change per 1% volatility)               |
| `SYMBOL_PRICE_RHO`          | 56    | Option rho (price change per 1% interest rate)             |
| `SYMBOL_PRICE_OMEGA`        | 57    | Option omega (elasticity)                                  |
| `SYMBOL_PRICE_SENSITIVITY`  | 58    | Option sensitivity                                         |

#### **Bonds/Special**
| Enum Value                     | Value | Description                              |
| ------------------------------ | ----- | ---------------------------------------- |
| `SYMBOL_TRADE_ACCRUED_INTEREST`| 19    | Accrued interest (bonds)                 |
| `SYMBOL_TRADE_FACE_VALUE`      | 20    | Face value (bonds)                       |
| `SYMBOL_TRADE_LIQUIDITY_RATE`  | 21    | Liquidity rate (asset share for margin)  |

#### **Internal**
| Enum Value     | Value | Description                       |
| -------------- | ----- | --------------------------------- |
| `SYMBOL_COUNT` | 59    | The count of symbol properties    |

> **MQL5 Reference:** [SymbolInfoDouble](https://www.mql5.com/en/docs/marketinformation/symbolinfodouble)

---

## 💬 Just the essentials

* **What it is.** Single RPC returning one numeric property of a symbol.
* **Why you need it.** Get current prices, calculate position sizes, check trading constraints, retrieve margin requirements.
* **Most common.** `SYMBOL_BID`, `SYMBOL_ASK`, `SYMBOL_POINT`, `SYMBOL_VOLUME_MIN/MAX/STEP`, `SYMBOL_TRADE_CONTRACT_SIZE`.
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

* **SYMBOL_BID/ASK** - use `SymbolInfoTickAsync()` for faster Bid/Ask retrieval.
* **SYMBOL_POINT** - minimum price change (e.g., 0.00001 for EURUSD).
* **SYMBOL_VOLUME_MIN/MAX/STEP** - critical for validating lot sizes before trading.
* **SYMBOL_TRADE_CONTRACT_SIZE** - lot size in base currency units (e.g., 100,000 for Forex standard lot).
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
    SymbolInfoDoubleProperty.SYMBOL_BID,
    deadline: DateTime.UtcNow.AddSeconds(3));
var ask = await acc.SymbolInfoDoubleAsync(
    "EURUSD",
    SymbolInfoDoubleProperty.SYMBOL_ASK);

Console.WriteLine($"EURUSD Bid: {bid.Value:F5}, Ask: {ask.Value:F5}");
```

### 2) Get volume constraints

```csharp
// Retrieve trading volume limits
var volumeMin = await acc.SymbolInfoDoubleAsync(
    "GBPUSD",
    SymbolInfoDoubleProperty.SYMBOL_VOLUME_MIN);
var volumeMax = await acc.SymbolInfoDoubleAsync(
    "GBPUSD",
    SymbolInfoDoubleProperty.SYMBOL_VOLUME_MAX);
var volumeStep = await acc.SymbolInfoDoubleAsync(
    "GBPUSD",
    SymbolInfoDoubleProperty.SYMBOL_VOLUME_STEP);

Console.WriteLine($"GBPUSD Volume:");
Console.WriteLine($"  Min:  {volumeMin.Value:F2} lots");
Console.WriteLine($"  Max:  {volumeMax.Value:F2} lots");
Console.WriteLine($"  Step: {volumeStep.Value:F2} lots");
```

### 3) Calculate position size

```csharp
// Get parameters for position size calculation
var symbol = "EURUSD";

var point = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SYMBOL_POINT);
var contractSize = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SYMBOL_TRADE_CONTRACT_SIZE);
var volumeMin = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SYMBOL_VOLUME_MIN);

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
    SymbolInfoDoubleProperty.SYMBOL_SWAP_LONG);
var swapShort = await acc.SymbolInfoDoubleAsync(
    "EURUSD",
    SymbolInfoDoubleProperty.SYMBOL_SWAP_SHORT);

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
    SymbolInfoDoubleProperty.SYMBOL_MARGIN_INITIAL);

Console.WriteLine($"XAUUSD Initial Margin: {marginInit.Value:F2}");
Console.WriteLine($"(Required margin for opening 1 lot)");
```
