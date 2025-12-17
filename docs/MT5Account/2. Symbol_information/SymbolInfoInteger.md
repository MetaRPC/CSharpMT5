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
namespace mt5_term_api
{
    public class MT5Account
    {
        public async Task<SymbolInfoIntegerData> SymbolInfoIntegerAsync(
            string symbolName,
            SymbolInfoIntegerProperty propertyType,
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
| `symbolName`        | `string`                      | ✅       | Symbol name (e.g., `"EURUSD"`)                            |
| `propertyType`      | `SymbolInfoIntegerProperty`   | ✅       | Property to retrieve (see enum below)                     |
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
| `SYMBOL_SUBSCRIPTION_DELAY`  | 0     | Symbol data arrives with a delay. Only for symbols in Market Watch (error 4302 for others)                                              |
| `SYMBOL_SECTOR`              | 1     | The sector of the economy to which the asset belongs (enum value)                                                                        |
| `SYMBOL_INDUSTRY`            | 2     | The industry/branch to which the symbol belongs (enum value)                                                                             |
| `SYMBOL_CUSTOM`              | 3     | Custom symbol flag: `1` if synthetic, `0` if standard                                                                                    |
| `SYMBOL_BACKGROUND_COLOR`    | 4     | Background color for symbol in Market Watch (color code)                                                                                 |
| `SYMBOL_CHART_MODE`          | 5     | Price type for generating bars: Bid or Last (enum)                                                                                       |
| `SYMBOL_EXIST`               | 6     | Symbol with this name exists: `1` = yes, `0` = no                                                                                        |
| `SYMBOL_SELECT`              | 7     | Symbol is selected in Market Watch: `1` = yes, `0` = no                                                                                  |
| `SYMBOL_VISIBLE`             | 8     | Symbol is visible in Market Watch: `1` = yes, `0` = no (some cross-rates are selected but not visible)                                  |
| `SYMBOL_SESSION_DEALS`       | 9     | Number of deals in the current session                                                                                                   |
| `SYMBOL_SESSION_BUY_ORDERS`  | 10    | Number of Buy orders at the moment                                                                                                       |
| `SYMBOL_SESSION_SELL_ORDERS` | 11    | Number of Sell orders at the moment                                                                                                      |
| `SYMBOL_VOLUME`              | 12    | Volume of the last deal (integer)                                                                                                        |
| `SYMBOL_VOLUMEHIGH`          | 13    | Maximal day volume                                                                                                                       |
| `SYMBOL_VOLUMELOW`           | 14    | Minimal day volume                                                                                                                       |
| `SYMBOL_TIME`                | 15    | Time of the last quote (Unix timestamp seconds)                                                                                          |
| `SYMBOL_TIME_MSC`            | 16    | Time of the last quote in milliseconds since 1970.01.01                                                                                  |
| `SYMBOL_DIGITS`              | 17    | Digits after a decimal point (e.g., 5 for EURUSD = 1.12345)                                                                             |
| `SYMBOL_SPREAD_FLOAT`        | 18    | Indication of a floating spread: `1` = floating, `0` = fixed                                                                             |
| `SYMBOL_SPREAD`              | 19    | Spread value in points                                                                                                                   |
| `SYMBOL_TICKS_BOOKDEPTH`     | 20    | Maximal number of requests in Depth of Market. `0` if no DOM available.                                                                 |
| `SYMBOL_TRADE_CALC_MODE`     | 21    | Contract price calculation mode (enum: Forex, CFD, Futures, etc.)                                                                        |
| `SYMBOL_TRADE_MODE`          | 22    | Order execution type (enum: Disabled, LongOnly, ShortOnly, CloseOnly, Full)                                                             |
| `SYMBOL_START_TIME`          | 23    | Date of symbol trade beginning (Unix timestamp) - usually for futures                                                                    |
| `SYMBOL_EXPIRATION_TIME`     | 24    | Date of symbol trade end (Unix timestamp) - usually for futures                                                                          |
| `SYMBOL_TRADE_STOPS_LEVEL`   | 25    | Minimal distance in points from current price to place Stop orders                                                                       |
| `SYMBOL_TRADE_FREEZE_LEVEL`  | 26    | Distance to freeze trade operations in points (can't modify/close orders within this distance)                                           |
| `SYMBOL_TRADE_EXEMODE`       | 27    | Deal execution mode (enum: Request, Instant, Market, Exchange)                                                                           |
| `SYMBOL_SWAP_MODE`           | 28    | Swap calculation model (enum: Disabled, Points, Currency modes, Interest, Reopen)                                                        |
| `SYMBOL_SWAP_ROLLOVER3DAYS`  | 29    | Day of week to charge 3-day swap rollover (enum: SUNDAY=0 ... SATURDAY=6)                                                               |
| `SYMBOL_MARGIN_HEDGED_USE_LEG`| 30   | Calculating hedging margin using the larger leg (Buy or Sell): `1` = yes, `0` = no                                                      |
| `SYMBOL_EXPIRATION_MODE`     | 31    | Flags of allowed order expiration modes (bitmask: GTC, Today, Specified, SpecifiedDay)                                                  |
| `SYMBOL_FILLING_MODE`        | 32    | Flags of allowed order filling modes (bitmask: FOK, IOC, BOC, Return)                                                                   |
| `SYMBOL_ORDER_MODE`          | 33    | Flags of allowed order types (bitmask: Market, Limit, Stop, StopLimit, etc.)                                                            |
| `SYMBOL_ORDER_GTC_MODE`      | 34    | Expiration of SL/TP orders if SYMBOL_EXPIRATION_MODE=GTC (enum: GTC, Daily, DailyExcludingStops)                                        |
| `SYMBOL_OPTION_MODE`         | 35    | Option type (enum: European, American)                                                                                                   |
| `SYMBOL_OPTION_RIGHT`        | 36    | Option right (enum: Call, Put)                                                                                                           |

> **MQL5 Reference:** [SymbolInfoInteger](https://www.mql5.com/en/docs/marketinformation/symbolinfointeger)

---

## 💬 Just the essentials

* **What it is.** Single RPC returning one integer property of a symbol.
* **Why you need it.** Check digits, spread, trading restrictions, execution modes, stops level, symbol status.
* **Most common.** `SYMBOL_DIGITS` (precision), `SYMBOL_SPREAD` (current spread), `SYMBOL_TRADE_STOPS_LEVEL` (min SL/TP distance).
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

* **SYMBOL_DIGITS** - decimal places (5 for EURUSD = 1.12345, 2 for XAUUSD = 1925.42).
* **SYMBOL_SPREAD** - current spread in points (not pips!). For EURUSD with 5 digits, 10 points = 1 pip.
* **SYMBOL_TRADE_STOPS_LEVEL** - minimum distance from current price for SL/TP. `0` = no restriction.
* **SYMBOL_TRADE_FREEZE_LEVEL** - distance within which you cannot modify/close orders.
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
    SymbolInfoIntegerProperty.SYMBOL_DIGITS,
    deadline: DateTime.UtcNow.AddSeconds(3));

Console.WriteLine($"EURUSD Digits: {digits.Value}");
// Output: 5 (prices like 1.12345)
```

### 2) Get current spread

```csharp
// Get spread in points
var spread = await acc.SymbolInfoIntegerAsync(
    "GBPUSD",
    SymbolInfoIntegerProperty.SYMBOL_SPREAD);

var digits = await acc.SymbolInfoIntegerAsync(
    "GBPUSD",
    SymbolInfoIntegerProperty.SYMBOL_DIGITS);

// Convert points to pips
double pips = spread.Value / Math.Pow(10, digits.Value - 1);

Console.WriteLine($"GBPUSD Spread: {spread.Value} points ({pips:F1} pips)");
```

### 3) Check stops level

```csharp
// Get minimum SL/TP distance
var stopsLevel = await acc.SymbolInfoIntegerAsync(
    "EURUSD",
    SymbolInfoIntegerProperty.SYMBOL_TRADE_STOPS_LEVEL);

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
    SymbolInfoIntegerProperty.SYMBOL_SELECT);

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

var digits = await acc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SYMBOL_DIGITS);
var spread = await acc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SYMBOL_SPREAD);
var stopsLevel = await acc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SYMBOL_TRADE_STOPS_LEVEL);
var freezeLevel = await acc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SYMBOL_TRADE_FREEZE_LEVEL);

Console.WriteLine($"{symbol} Trading Parameters:");
Console.WriteLine($"  Digits:       {digits.Value}");
Console.WriteLine($"  Spread:       {spread.Value} points");
Console.WriteLine($"  Stops Level:  {stopsLevel.Value} points");
Console.WriteLine($"  Freeze Level: {freezeLevel.Value} points");
```
