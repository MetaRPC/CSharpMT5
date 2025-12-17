# ✅ Getting Symbol String Properties

> **Request:** string property of a symbol from **MT5**. Get text-based symbol information like description, currencies, exchange, ISIN, etc.

**API Information:**

* **SDK wrapper:** `MT5Account.SymbolInfoStringAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `SymbolInfoString` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `SymbolInfoString(SymbolInfoStringRequest) → SymbolInfoStringReply`
* **Low‑level client (generated):** `MarketInfo.SymbolInfoString(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace mt5_term_api
{
    public class MT5Account
    {
        public async Task<SymbolInfoStringData> SymbolInfoStringAsync(
            string symbolName,
            SymbolInfoStringProperty propertyType,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`SymbolInfoStringRequest { symbol: string, type: SymbolInfoStringProperty }`


**Reply message:**

`SymbolInfoStringReply { data: SymbolInfoStringData }` or `{ error: Error }`

---

## 🔽 Input

| Parameter           | Type                         | Required | Description                                               |
| ------------------- | ---------------------------- | -------- | --------------------------------------------------------- |
| `symbolName`        | `string`                     | ✅       | Symbol name (e.g., `"EURUSD"`)                            |
| `propertyType`      | `SymbolInfoStringProperty`   | ✅       | Property to retrieve (see enum below)                     |
| `deadline`          | `DateTime?`                  | ❌       | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`          | ❌       | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `SymbolInfoStringData`

| Field   | Type     | Description                      |
| ------- | -------- | -------------------------------- |
| `Value` | `string` | The requested property value     |

---

## 🧱 Related enums (from proto)

### `SymbolInfoStringProperty`

| Enum Value                | Value | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | MQL5 Docs                                                          |
| ------------------------- | ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `SYMBOL_BASIS`            | 0     | The underlying asset of a derivative                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | [SymbolInfoString](https://www.mql5.com/en/docs/marketinformation/symbolinfostring) |
| `SYMBOL_CATEGORY`         | 1     | The name of the sector or category to which the financial symbol belongs                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |                                                                    |
| `SYMBOL_COUNTRY`          | 2     | The country to which the financial symbol belongs                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |                                                                    |
| `SYMBOL_SECTOR_NAME`      | 3     | The sector of the economy to which the financial symbol belongs                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |                                                                    |
| `SYMBOL_INDUSTRY_NAME`    | 4     | The industry branch or the industry to which the financial symbol belongs                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |                                                                    |
| `SYMBOL_CURRENCY_BASE`    | 5     | Basic currency of a symbol (e.g., for `EURUSD` this is `EUR`)                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |                                                                    |
| `SYMBOL_CURRENCY_PROFIT`  | 6     | Profit currency (currency in which profit/loss is calculated, e.g., for `EURUSD` this is `USD`)                                                                                                                                                                                                                                                                                                                                                                                                                                          |                                                                    |
| `SYMBOL_CURRENCY_MARGIN`  | 7     | Margin currency (currency used for margin calculations)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |                                                                    |
| `SYMBOL_BANK`             | 8     | Feeder of the current quote (data provider)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |                                                                    |
| `SYMBOL_DESCRIPTION`      | 9     | Symbol description (full name, e.g., `"Euro vs US Dollar"`)                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |                                                                    |
| `SYMBOL_EXCHANGE`         | 10    | The name of the exchange in which the financial symbol is traded                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |                                                                    |
| `SYMBOL_FORMULA`          | 11    | The formula used for custom symbol pricing. If the name starts with a digit or contains special characters (">", ".", "-", "&", "#"), use quotation marks. Examples: Synthetic: `"@ESU19"/EURCAD`, Calendar spread: `"Si-9.13"-"Si-6.13"`, Euro index: `34.38805726 * pow(EURUSD,0.3155) * pow(EURGBP,0.3056) * pow(EURJPY,0.1891) * pow(EURCHF,0.1113) * pow(EURSEK,0.0785)`                                                                                                                                                            |                                                                    |
| `SYMBOL_ISIN`             | 12    | ISIN (International Securities Identification Number) - a 12-digit alphanumeric code that uniquely identifies a security. Presence depends on trade server.                                                                                                                                                                                                                                                                                                                                                                               |                                                                    |
| `SYMBOL_PAGE`             | 13    | The address of the web page containing symbol information. Displayed as a link in terminal symbol properties.                                                                                                                                                                                                                                                                                                                                                                                                                             |                                                                    |
| `SYMBOL_PATH`             | 14    | Path in the symbol tree (folder structure in Market Watch)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |                                                                    |

---

## 💬 Just the essentials

* **What it is.** Single RPC returning one string property of a symbol.
* **Why you need it.** Get symbol description, currencies, metadata for display, logging, or trading logic.
* **Most common.** `SYMBOL_DESCRIPTION` (full name), `SYMBOL_CURRENCY_BASE` (base currency), `SYMBOL_CURRENCY_PROFIT` (profit currency).

---

## 🎯 Purpose

Use this method when you need to:

* Display symbol's full description in UI (e.g., `"Euro vs US Dollar"`).
* Get base and profit currencies for position size calculations.
* Retrieve margin currency for margin calculations.
* Get exchange name for stocks/futures.
* Obtain ISIN code for regulatory compliance.
* Display symbol metadata in logs or reports.

---

## 🧩 Notes & Tips

* **SYMBOL_CURRENCY_BASE** - the first currency in a pair (for `EURUSD` = `EUR`).
* **SYMBOL_CURRENCY_PROFIT** - currency of profit/loss (for `EURUSD` = `USD`).
* **SYMBOL_CURRENCY_MARGIN** - currency used for margin (usually same as profit).
* For Forex pairs: `Base/Profit` (e.g., `EUR/USD` means EUR is base, USD is profit).
* Some properties may return empty strings if not available from broker.
* Use short timeout (3-5s) as this is metadata (cached).
* **SYMBOL_FORMULA** is for custom symbols only - synthetic instruments.
* **SYMBOL_ISIN** availability depends on broker/instrument type.

---

## 🔗 Usage Examples

### 1) Get symbol description

```csharp
// Get human-readable description
var desc = await acc.SymbolInfoStringAsync(
    "EURUSD",
    SymbolInfoStringProperty.SYMBOL_DESCRIPTION,
    deadline: DateTime.UtcNow.AddSeconds(3));
Console.WriteLine($"Description: {desc.Value}");
// Output: "Euro vs US Dollar" or similar
```

### 2) Get base and profit currencies

```csharp
// Retrieve currency pair components
var baseCurr = await acc.SymbolInfoStringAsync(
    "GBPUSD",
    SymbolInfoStringProperty.SYMBOL_CURRENCY_BASE);
var profitCurr = await acc.SymbolInfoStringAsync(
    "GBPUSD",
    SymbolInfoStringProperty.SYMBOL_CURRENCY_PROFIT);

Console.WriteLine($"GBPUSD:");
Console.WriteLine($"  Base currency:   {baseCurr.Value}");   // GBP
Console.WriteLine($"  Profit currency: {profitCurr.Value}"); // USD
```

### 3) Display symbol metadata

```csharp
// Show detailed symbol information
var symbol = "EURUSD";

var description = await acc.SymbolInfoStringAsync(symbol, SymbolInfoStringProperty.SYMBOL_DESCRIPTION);
var baseCurr = await acc.SymbolInfoStringAsync(symbol, SymbolInfoStringProperty.SYMBOL_CURRENCY_BASE);
var profitCurr = await acc.SymbolInfoStringAsync(symbol, SymbolInfoStringProperty.SYMBOL_CURRENCY_PROFIT);
var exchange = await acc.SymbolInfoStringAsync(symbol, SymbolInfoStringProperty.SYMBOL_EXCHANGE);

Console.WriteLine($"Symbol: {symbol}");
Console.WriteLine($"  Description: {description.Value}");
Console.WriteLine($"  Base:        {baseCurr.Value}");
Console.WriteLine($"  Profit:      {profitCurr.Value}");
Console.WriteLine($"  Exchange:    {exchange.Value}");
```

### 4) Get ISIN code

```csharp
// Retrieve ISIN (if available)
var isin = await acc.SymbolInfoStringAsync(
    "AAPL", // Apple stock
    SymbolInfoStringProperty.SYMBOL_ISIN);

if (!string.IsNullOrEmpty(isin.Value))
{
    Console.WriteLine($"ISIN: {isin.Value}");
}
else
{
    Console.WriteLine("ISIN not available");
}
```

### 5) Log symbol details for debugging

```csharp
// Comprehensive symbol info for logs
var symbol = "XAUUSD";

Console.WriteLine($"=== Symbol Info: {symbol} ===");

// Get all string properties
var props = new[]
{
    (SymbolInfoStringProperty.SYMBOL_DESCRIPTION, "Description"),
    (SymbolInfoStringProperty.SYMBOL_CURRENCY_BASE, "Base Currency"),
    (SymbolInfoStringProperty.SYMBOL_CURRENCY_PROFIT, "Profit Currency"),
    (SymbolInfoStringProperty.SYMBOL_CURRENCY_MARGIN, "Margin Currency"),
    (SymbolInfoStringProperty.SYMBOL_EXCHANGE, "Exchange"),
    (SymbolInfoStringProperty.SYMBOL_CATEGORY, "Category"),
    (SymbolInfoStringProperty.SYMBOL_SECTOR_NAME, "Sector"),
    (SymbolInfoStringProperty.SYMBOL_PATH, "Path")
};

foreach (var (prop, label) in props)
{
    var result = await acc.SymbolInfoStringAsync(symbol, prop);
    Console.WriteLine($"  {label,-20}: {result.Value}");
}
```
