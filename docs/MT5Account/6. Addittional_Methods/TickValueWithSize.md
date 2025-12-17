# ✅ Get Tick Values for Multiple Symbols (`TickValueWithSizeAsync`)

> **Request:** Get tick values and sizes for multiple symbols in a single call on **MT5**. Returns tick value, profit/loss tick values, tick size, and contract size.

**API Information:**

* **SDK wrapper:** `MT5Account.TickValueWithSizeAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.AccountHelper`
* **Proto definition:** `TickValueWithSize` (defined in `mt5-term-api-account-helper.proto`)

### RPC

* **Service:** `mt5_term_api.AccountHelper`
* **Method:** `TickValueWithSize(TickValueWithSizeRequest) → TickValueWithSizeReply`
* **Low‑level client (generated):** `AccountHelper.AccountHelperClient.TickValueWithSize(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace mt5_term_api
{
    public class MT5Account
    {
        public async Task<TickValueWithSizeData> TickValueWithSizeAsync(
            TickValueWithSizeRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`TickValueWithSizeRequest { symbol_names }`


**Reply message:**

`TickValueWithSizeReply { data: TickValueWithSizeData }`

---

## 🔽 Input

| Parameter           | Type                       | Description                                               |
| ------------------- | -------------------------- | --------------------------------------------------------- |
| `request`           | `TickValueWithSizeRequest` | Protobuf request with array of symbol names               |
| `deadline`          | `DateTime?`                | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`        | Cooperative cancel for the call/retry loop                |

### `TickValueWithSizeRequest`

| Field         | Type              | Description                                               |
| ------------- | ----------------- | --------------------------------------------------------- |
| `SymbolNames` | `List<string>`    | Array of symbol names (e.g., `["EURUSD", "XAUUSD"]`)      |

---

## ⬆️ Output — `TickValueWithSizeData`

| Field                | Type                      | Description                                |
| -------------------- | ------------------------- | ------------------------------------------ |
| `SymbolTickSizeInfos`| `List<TickSizeSymbol>`    | Array of tick information for each symbol  |

### `TickSizeSymbol` — Tick value information

| Field                  | Type     | Description                                              |
| ---------------------- | -------- | -------------------------------------------------------- |
| `Index`                | `int32`  | Symbol index in request array                            |
| `Name`                 | `string` | Symbol name                                              |
| `TradeTickValue`       | `double` | Tick value (cost of 1 tick for 1 lot)                   |
| `TradeTickValueProfit` | `double` | Tick value for profit calculation                        |
| `TradeTickValueLoss`   | `double` | Tick value for loss calculation                          |
| `TradeTickSize`        | `double` | Tick size (minimum price change)                         |
| `TradeContractSize`    | `double` | Contract size (volume for 1 standard lot)                |

---

## 💬 Just the essentials

* **What it is.** Batch endpoint to get tick values and contract sizes for multiple symbols in one call.
* **Why you need it.** Calculate P&L in account currency, determine pip value, analyze trading costs across multiple symbols efficiently.
* **Sanity check.** If `SymbolTickSizeInfos.Count > 0` → data available. Check `Index` to match symbols with request order.

---

## 🎯 Purpose

Use it to get tick value data:

* Calculate profit/loss in account currency.
* Determine pip/tick value for risk management.
* Batch-fetch tick data for multiple symbols.
* Calculate position sizing based on tick value.

---

## 🧩 Notes & Tips

* **Batch optimization:** More efficient than calling individual symbol info methods multiple times.
* **Tick value vs Tick size:** Tick size is the minimum price change. Tick value is the monetary value of that change for 1 lot.
* **Profit vs Loss tick value:** Some symbols have asymmetric tick values for long/short positions.
* **Contract size:** Used to convert volume (lots) to actual units (e.g., 100,000 for Forex).
* **Account currency:** Tick values are returned in account currency.
* **Use Index:** Match results to request using `Index` field, especially if some symbols fail.

---

## 🔗 Usage Examples

### 1) Get tick values for single symbol

```csharp
// acc — connected MT5Account

var result = await acc.TickValueWithSizeAsync(new TickValueWithSizeRequest
{
    SymbolNames = { "EURUSD" }
});

if (result.SymbolTickSizeInfos.Count > 0)
{
    var tick = result.SymbolTickSizeInfos[0];

    Console.WriteLine($"Symbol: {tick.Name}");
    Console.WriteLine($"Tick value: {tick.TradeTickValue:F2}");
    Console.WriteLine($"Tick value (profit): {tick.TradeTickValueProfit:F2}");
    Console.WriteLine($"Tick value (loss): {tick.TradeTickValueLoss:F2}");
    Console.WriteLine($"Tick size: {tick.TradeTickSize:F5}");
    Console.WriteLine($"Contract size: {tick.TradeContractSize:F2}");
}
```

---

### 2) Get tick values for multiple symbols (batch)

```csharp
var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY", "XAUUSD", "BTCUSD" };

var result = await acc.TickValueWithSizeAsync(new TickValueWithSizeRequest
{
    SymbolNames = { symbols }
});

Console.WriteLine($"Tick values for {result.SymbolTickSizeInfos.Count} symbols:\n");

foreach (var tick in result.SymbolTickSizeInfos)
{
    Console.WriteLine($"{tick.Name,-10} Tick value: ${tick.TradeTickValue,8:F2}  Tick size: {tick.TradeTickSize,10:F5}  Contract: {tick.TradeContractSize,10:F0}");
}
```

---

### 3) Calculate pip value for Forex pairs

```csharp
var forexPairs = new[] { "EURUSD", "GBPUSD", "USDJPY", "USDCHF" };

var result = await acc.TickValueWithSizeAsync(new TickValueWithSizeRequest
{
    SymbolNames = { forexPairs }
});

Console.WriteLine("Pip values (for 1 standard lot):\n");

foreach (var tick in result.SymbolTickSizeInfos)
{
    // For most Forex pairs, 1 pip = 10 ticks (0.0001 for 4-digit pairs, 0.01 for JPY pairs)
    var pipValue = tick.TradeTickValue * 10;

    Console.WriteLine($"{tick.Name,-10} 1 pip = ${pipValue:F2}");
}
```

---

### 4) Calculate P&L for position

```csharp
var symbol = "XAUUSD";
var lots = 0.10;
var entryPrice = 2000.00;
var currentPrice = 2010.00;

// Get tick value
var result = await acc.TickValueWithSizeAsync(new TickValueWithSizeRequest
{
    SymbolNames = { symbol }
});

if (result.SymbolTickSizeInfos.Count > 0)
{
    var tick = result.SymbolTickSizeInfos[0];

    // Calculate price change in ticks
    var priceChange = currentPrice - entryPrice;
    var ticksChanged = priceChange / tick.TradeTickSize;

    // Calculate P&L
    var profitLoss = ticksChanged * tick.TradeTickValueProfit * lots;

    Console.WriteLine($"Symbol: {tick.Name}");
    Console.WriteLine($"Entry: {entryPrice}, Current: {currentPrice}");
    Console.WriteLine($"Price change: {priceChange} ({ticksChanged} ticks)");
    Console.WriteLine($"P&L for {lots} lots: ${profitLoss:F2}");
}
```

---

### 5) Position sizing based on risk

```csharp
var symbol = "EURUSD";
var accountBalance = 10000.0; // $10,000
var riskPercent = 1.0; // Risk 1% per trade
var stopLossPips = 50; // 50 pip stop loss

// Get tick value
var result = await acc.TickValueWithSizeAsync(new TickValueWithSizeRequest
{
    SymbolNames = { symbol }
});

if (result.SymbolTickSizeInfos.Count > 0)
{
    var tick = result.SymbolTickSizeInfos[0];

    // Calculate pip value (1 pip = 10 ticks for most pairs)
    var pipValue = tick.TradeTickValue * 10;

    // Calculate risk amount
    var riskAmount = accountBalance * (riskPercent / 100.0);

    // Calculate lot size
    var lotSize = riskAmount / (stopLossPips * pipValue);

    // Round to valid lot size (e.g., 0.01 step)
    lotSize = Math.Round(lotSize, 2);

    Console.WriteLine($"Account: ${accountBalance:F2}");
    Console.WriteLine($"Risk: {riskPercent}% (${riskAmount:F2})");
    Console.WriteLine($"Stop loss: {stopLossPips} pips");
    Console.WriteLine($"Pip value (1 lot): ${pipValue:F2}");
    Console.WriteLine($"Recommended lot size: {lotSize:F2}");
}
```

---

### 6) Compare tick values across symbol types

```csharp
var symbols = new[] { "EURUSD", "XAUUSD", "BTCUSD", "ES", "NQ" };

var result = await acc.TickValueWithSizeAsync(new TickValueWithSizeRequest
{
    SymbolNames = { symbols }
});

Console.WriteLine("Tick value comparison:\n");
Console.WriteLine($"{"Symbol",-10} {"Tick Value",-12} {"Tick Size",-12} {"Contract Size",-15}");
Console.WriteLine(new string('-', 60));

foreach (var tick in result.SymbolTickSizeInfos)
{
    Console.WriteLine($"{tick.Name,-10} ${tick.TradeTickValue,-12:F2} {tick.TradeTickSize,-12:F5} {tick.TradeContractSize,-15:F0}");
}

Console.WriteLine("\nNotes:");
Console.WriteLine("- Forex (EURUSD): Contract size 100,000, tick size 0.00001");
Console.WriteLine("- Gold (XAUUSD): Contract size 100, tick size varies");
Console.WriteLine("- Crypto (BTCUSD): Contract size varies by broker");
Console.WriteLine("- Futures (ES/NQ): Contract size and tick size vary by contract");
```

---

### 7) Calculate margin requirement using tick values

```csharp
var symbol = "XAUUSD";
var lots = 1.0;

// Get tick values
var result = await acc.TickValueWithSizeAsync(new TickValueWithSizeRequest
{
    SymbolNames = { symbol }
});

if (result.SymbolTickSizeInfos.Count > 0)
{
    var tick = result.SymbolTickSizeInfos[0];

    // Get current price (assuming you have it from SymbolInfoTick)
    var currentPrice = 2000.0; // Example

    // Calculate position value
    var positionValue = lots * tick.TradeContractSize * currentPrice;

    Console.WriteLine($"Symbol: {tick.Name}");
    Console.WriteLine($"Lots: {lots}");
    Console.WriteLine($"Contract size: {tick.TradeContractSize}");
    Console.WriteLine($"Current price: ${currentPrice}");
    Console.WriteLine($"Position value: ${positionValue:F2}");
    Console.WriteLine($"\nNote: Actual margin depends on leverage and margin requirements");
}
```

---

### 8) Validate tick value asymmetry

```csharp
var symbols = new[] { "EURUSD", "USDJPY", "XAUUSD" };

var result = await acc.TickValueWithSizeAsync(new TickValueWithSizeRequest
{
    SymbolNames = { symbols }
});

Console.WriteLine("Checking for asymmetric tick values:\n");

foreach (var tick in result.SymbolTickSizeInfos)
{
    var isSymmetric = Math.Abs(tick.TradeTickValueProfit - tick.TradeTickValueLoss) < 0.0001;

    Console.WriteLine($"{tick.Name,-10}:");
    Console.WriteLine($"  Profit tick value: ${tick.TradeTickValueProfit:F2}");
    Console.WriteLine($"  Loss tick value:   ${tick.TradeTickValueLoss:F2}");
    Console.WriteLine($"  Status: {(isSymmetric ? "Symmetric" : "⚠ Asymmetric - long/short have different pip values")}");
    Console.WriteLine();
}
```
