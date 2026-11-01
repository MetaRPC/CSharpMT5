# ✅ Getting Symbol Integer Properties

> **Request:** integer (long) property of a symbol from **MT5**. Get digits, spread, execution modes, time parameters, and other integer symbol properties.

**API Information:**

* **SDK wrapper:** `MT5Account.SymbolInfoIntegerAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `SymbolInfoInteger` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `SymbolInfoInteger(SymbolInfoIntegerRequest) → SymbolInfoIntegerReply`
* **Low‑level client (generated):** `MarketInfo.SymbolInfoInteger(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<SymbolInfoIntegerData> SymbolInfoIntegerAsync(
            string symbol,
            SymbolInfoIntegerProperty property,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`SymbolInfoIntegerRequest { symbol: string, type: SymbolInfoIntegerProperty }`


**Reply message:**

`SymbolInfoIntegerReply { data: SymbolInfoIntegerData }` or `{ error: Error }`

---

## 🔽 Input

| Parameter           | Type                          | Required | Description                                               |
| ------------------- | ----------------------------- | -------- | --------------------------------------------------------- |
| `symbol`        | `string`                      | ✅       | Symbol name (e.g., `"EURUSD"`)                            |
| `property`      | `SymbolInfoIntegerProperty`   | ✅       | Property to retrieve (see enum below)                     |
| `deadline`          | `DateTime?`                   | ❌       | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`           | ❌       | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `SymbolInfoIntegerData`

| Field   | Type   | Description                  |
| ------- | ------ | ---------------------------- |
| `Value` | `long` | The requested property value |

---

## 🧱 Related enums (from proto)

### `SymbolInfoIntegerProperty`

| Enum Value                   | Value | Description                                                                                                                              |
| ---------------------------- | ----- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| `SymbolSubscriptionDelay`  | 0     | Symbol data arrives with a delay. Only for symbols in Market Watch (error 4302 for others)                                              |
| `SymbolSector`              | 1     | The sector of the economy to which the asset belongs (enum value)                                                                        |
| `SymbolIndustry`            | 2     | The industry/branch to which the symbol belongs (enum value)                                                                             |
| `SymbolCustom`              | 3     | Custom symbol flag: `1` if synthetic, `0` if standard                                                                                    |
| `SymbolBackgroundColor`    | 4     | Background color for symbol in Market Watch (color code)                                                                                 |
| `SymbolChartMode`          | 5     | Price type for generating bars: Bid or Last (enum)                                                                                       |
| `SymbolExist`               | 6     | Symbol with this name exists: `1` = yes, `0` = no                                                                                        |
| `SymbolSelect`              | 7     | Symbol is selected in Market Watch: `1` = yes, `0` = no                                                                                  |
| `SymbolVisible`             | 8     | Symbol is visible in Market Watch: `1` = yes, `0` = no (some cross-rates are selected but not visible)                                  |
| `SymbolSessionDeals`       | 9     | Number of deals in the current session                                                                                                   |
| `SymbolSessionBuyOrders`  | 10    | Number of Buy orders at the moment                                                                                                       |
| `SymbolSessionSellOrders` | 11    | Number of Sell orders at the moment                                                                                                      |
| `SymbolVolume`              | 12    | Volume of the last deal (integer)                                                                                                        |
| `SymbolVolumehigh`          | 13    | Maximal day volume                                                                                                                       |
| `SymbolVolumelow`           | 14    | Minimal day volume                                                                                                                       |
| `SymbolTime`                | 15    | Time of the last quote (Unix timestamp seconds)                                                                                          |
| `SymbolTimeMsc`            | 16    | Time of the last quote in milliseconds since 1970.01.01                                                                                  |
| `SymbolDigits`              | 17    | Digits after a decimal point (e.g., 5 for EURUSD = 1.12345)                                                                             |
| `SymbolSpreadFloat`        | 18    | Indication of a floating spread: `1` = floating, `0` = fixed                                                                             |
| `SymbolSpread`              | 19    | Spread value in points                                                                                                                   |
| `SymbolTicksBookdepth`     | 20    | Maximal number of requests in Depth of Market. `0` if no DOM available.                                                                 |
| `SymbolTradeCalcMode`     | 21    | Contract price calculation mode (enum: Forex, CFD, Futures, etc.)                                                                        |
| `SymbolTradeMode`          | 22    | Order execution type (enum: Disabled, LongOnly, ShortOnly, CloseOnly, Full)                                                             |
| `SymbolStartTime`          | 23    | Date of symbol trade beginning (Unix timestamp) - usually for futures                                                                    |
| `SymbolExpirationTime`     | 24    | Date of symbol trade end (Unix timestamp) - usually for futures                                                                          |
| `SymbolTradeStopsLevel`   | 25    | Minimal distance in points from current price to place Stop orders                                                                       |
| `SymbolTradeFreezeLevel`  | 26    | Distance to freeze trade operations in points (can't modify/close orders within this distance)                                           |
| `SymbolTradeExemode`       | 27    | Deal execution mode (enum: Request, Instant, Market, Exchange)                                                                           |
| `SymbolSwapMode`           | 28    | Swap calculation model (enum: Disabled, Points, Currency modes, Interest, Reopen)                                                        |
| `SymbolSwapRollover3Days`  | 29    | Day of week to charge 3-day swap rollover (enum: SUNDAY=0 ... SATURDAY=6)                                                               |
| `SymbolMarginHedgedUseLeg`| 30   | Calculating hedging margin using the larger leg (Buy or Sell): `1` = yes, `0` = no                                                      |
| `SymbolExpirationMode`     | 31    | Flags of allowed order expiration modes (bitmask: GTC, Today, Specified, SpecifiedDay)                                                  |
| `SymbolFillingMode`        | 32    | Flags of allowed order filling modes (bitmask: FOK, IOC, BOC, Return)                                                                   |
| `SymbolOrderMode`          | 33    | Flags of allowed order types (bitmask: Market, Limit, Stop, StopLimit, etc.)                                                            |
| `SymbolOrderGtcMode`      | 34    | Expiration of SL/TP orders if SymbolExpirationMode=GTC (enum: GTC, Daily, DailyExcludingStops)                                        |
| `SymbolOptionMode`         | 35    | Option type (enum: European, American)                                                                                                   |
| `SymbolOptionRight`        | 36    | Option right (enum: Call, Put)                                                                                                           |

> **MQL5 Reference:** [SymbolInfoInteger](https://www.mql5.com/en/docs/marketinformation/symbolinfointeger)

---

## 💬 Just the essentials

* **What it is.** Single RPC returning one integer property of a symbol.
* **Why you need it.** Check digits, spread, trading restrictions, execution modes, stops level, symbol status.
* **Most common.** `SymbolDigits` (precision), `SymbolSpread` (current spread), `SymbolTradeStopsLevel` (min SL/TP distance).
* **37 properties** covering precision, trading modes, status flags, execution parameters.

---

## 🎯 Purpose

Use this method when you need to:

* Get symbol precision (digits) for price formatting.
* Check current spread in points.
* Verify minimum stops level before setting SL/TP.
* Check if symbol is selected/visible in Market Watch.
* Determine execution mode and trading restrictions.
* Get freeze level to avoid order modification errors.
* Check expiration/filling modes supported by symbol.

---

## 🧩 Notes & Tips

* **SymbolDigits** - decimal places (5 for EURUSD = 1.12345, 2 for XAUUSD = 1925.42).
* **SymbolSpread** - current spread in points (not pips!). For EURUSD with 5 digits, 10 points = 1 pip.
* **SymbolTradeStopsLevel** - minimum distance from current price for SL/TP. `0` = no restriction.
* **SymbolTradeFreezeLevel** - distance within which you cannot modify/close orders.
* **Boolean flags** return `1` (true) or `0` (false) as `long`.
* **Enum properties** return integer enum values - need to map to enum types.
* **Bitmask properties** (EXPIRATION_MODE, FILLING_MODE, ORDER_MODE) use bitwise flags.
* Properties are cached - use short timeout (3-5s).

---

## 🔗 Usage Examples

### 1) Get symbol precision

```csharp
// Get number of decimal places
var digits = await acc.SymbolInfoIntegerAsync(
    "EURUSD",
    SymbolInfoIntegerProperty.SymbolDigits,
    deadline: DateTime.UtcNow.AddSeconds(3));

Console.WriteLine($"EURUSD Digits: {digits.Value}");
// Output: 5 (prices like 1.12345)
```

### 2) Get current spread

```csharp
// Get spread in points
var spread = await acc.SymbolInfoIntegerAsync(
    "GBPUSD",
    SymbolInfoIntegerProperty.SymbolSpread);

var digits = await acc.SymbolInfoIntegerAsync(
    "GBPUSD",
    SymbolInfoIntegerProperty.SymbolDigits);

// Convert points to pips
double pips = spread.Value / Math.Pow(10, digits.Value - 1);

Console.WriteLine($"GBPUSD Spread: {spread.Value} points ({pips:F1} pips)");
```

### 3) Check stops level

```csharp
// Get minimum SL/TP distance
var stopsLevel = await acc.SymbolInfoIntegerAsync(
    "EURUSD",
    SymbolInfoIntegerProperty.SymbolTradeStopsLevel);

if (stopsLevel.Value == 0)
{
    Console.WriteLine("✅ No stops level restriction");
}
else
{
    Console.WriteLine($"⚠️ Minimum SL/TP distance: {stopsLevel.Value} points");
}
```

### 4) Check if symbol is in Market Watch

```csharp
// Verify symbol is selected
var isSelected = await acc.SymbolInfoIntegerAsync(
    "XAUUSD",
    SymbolInfoIntegerProperty.SymbolSelect);

if (isSelected.Value == 1)
{
    Console.WriteLine("✅ XAUUSD is in Market Watch");
}
else
{
    Console.WriteLine("❌ XAUUSD not in Market Watch");
}
```

### 5) Get trading parameters

```csharp
// Retrieve key trading info
var symbol = "EURUSD";

var digits = await acc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolDigits);
var spread = await acc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolSpread);
var stopsLevel = await acc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolTradeStopsLevel);
var freezeLevel = await acc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolTradeFreezeLevel);

Console.WriteLine($"{symbol} Trading Parameters:");
Console.WriteLine($"  Digits:       {digits.Value}");
Console.WriteLine($"  Spread:       {spread.Value} points");
Console.WriteLine($"  Stops Level:  {stopsLevel.Value} points");
Console.WriteLine($"  Freeze Level: {freezeLevel.Value} points");
```
