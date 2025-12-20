# MT5Account · Additional Methods - Overview

> Advanced symbol information, trading sessions, margin rates, tick values, and batch operations. Use this page for specialized symbol analysis.

## 📁 What lives here

* **[SymbolInfoMarginRate](./SymbolInfoMarginRate.md)** - **margin requirements** (initial & maintenance) for order types.
* **[SymbolInfoSessionQuote](./SymbolInfoSessionQuote.md)** - **quote session times** for specific day and symbol.
* **[SymbolInfoSessionTrade](./SymbolInfoSessionTrade.md)** - **trade session times** for specific day and symbol.
* **[SymbolParamsMany](./SymbolParamsMany.md)** - **detailed parameters** for multiple symbols (112 fields per symbol!).
* **[TickValueWithSize](./TickValueWithSize.md)** - **tick values and contract sizes** for multiple symbols in one call.

---

## 🧭 Plain English

* **SymbolInfoMarginRate** → find out margin requirements before trading different order types.
* **SymbolInfoSession*** (Quote/Trade) → check when market is open for quotes or trading.
* **SymbolParamsMany** → get **everything** about symbols (bid, ask, spread, volumes, margins, sessions, etc.).
* **TickValueWithSize** → batch-fetch pip values and contract sizes for risk calculations.

> These are **advanced methods** - most traders only need basic SymbolInfo methods. Use these for:
> - Risk management calculations
> - Trading time validation
> - Multi-symbol analysis
> - Detailed symbol research

---

## Quick choose

| If you need…                                     | Use                            | Returns                    | Key inputs                          |
| ------------------------------------------------ | ------------------------------ | -------------------------- | ----------------------------------- |
| Margin rates for order type                      | `SymbolInfoMarginRateAsync`    | Initial + Maintenance rates | Symbol + Order type                 |
| Quote session times                              | `SymbolInfoSessionQuoteAsync`  | Session start/end times    | Symbol + Day + Session index        |
| Trade session times                              | `SymbolInfoSessionTradeAsync`  | Session start/end times    | Symbol + Day + Session index        |
| Detailed symbol info (112 fields!)               | `SymbolParamsManyAsync`        | SymbolParameters objects   | Optional: symbol filter, pagination |
| Tick values for multiple symbols                 | `TickValueWithSizeAsync`       | Tick values + contract sizes | List of symbol names                |

---

## ❌ Cross‑refs & gotchas

* **Margin rates** differ by order type (Buy vs Sell may have different rates).
* **Session times** are in seconds from 00:00 - extract hours/minutes.
* **Multiple sessions** possible per day (e.g., pre-market, regular, after-hours).
* **SymbolParamsMany** returns 112 fields - filter what you need.
* **Tick value** helps convert points to account currency for P/L calculations.
* **Contract size** determines lot size (1 lot = contract size units of base currency).

---

## 🟢 Minimal snippets

```csharp
// Get margin requirements for Buy order
var marginData = await account.SymbolInfoMarginRateAsync(new SymbolInfoMarginRateRequest
{
    Symbol = "EURUSD",
    OrderType = ENUM_ORDER_TYPE.OrderTypeBuy
});
Console.WriteLine($"Initial Margin Rate: {marginData.InitialMarginRate:F2}");
Console.WriteLine($"Maintenance Margin Rate: {marginData.MaintenanceMarginRate:F2}");
```

```csharp
// Check if market is open for trading (Monday session 0)
var sessionData = await account.SymbolInfoSessionTradeAsync(new SymbolInfoSessionTradeRequest
{
    Symbol = "EURUSD",
    DayOfWeek = DayOfWeek.Monday,
    SessionIndex = 0
});
var from = sessionData.From.ToDateTime();
var to = sessionData.To.ToDateTime();
Console.WriteLine($"Monday trading session: {from:HH:mm} - {to:HH:mm}");
```

```csharp
// Get detailed info for multiple symbols
var symbolsData = await account.SymbolParamsManyAsync(new SymbolParamsManyRequest
{
    // Optional: filter by symbol name
    // SymbolName = "EUR",  // Returns symbols containing "EUR"
    ItemsPerPage = 10,
    PageNumber = 0
});

foreach (var symbol in symbolsData.SymbolInfos)
{
    Console.WriteLine($"{symbol.Name}: Bid={symbol.Bid:F5}, Ask={symbol.Ask:F5}, " +
                      $"Spread={symbol.Spread} pts, Volume={symbol.VolumeReal:F2}");
}
Console.WriteLine($"Total symbols: {symbolsData.SymbolsTotal}");
```

```csharp
// Calculate pip value for multiple symbols at once
var tickData = await account.TickValueWithSizeAsync(new TickValueWithSizeRequest
{
    SymbolNames = { "EURUSD", "GBPUSD", "USDJPY" }
});

foreach (var symbolInfo in tickData.SymbolTickSizeInfos)
{
    Console.WriteLine($"{symbolInfo.Name}:");
    Console.WriteLine($"  Tick Value: ${symbolInfo.TradeTickValue:F2}");
    Console.WriteLine($"  Tick Size: {symbolInfo.TradeTickSize:F5}");
    Console.WriteLine($"  Contract Size: {symbolInfo.TradeContractSize:F0}");
    Console.WriteLine($"  Pip Value (Profit): ${symbolInfo.TradeTickValueProfit:F2}");
    Console.WriteLine($"  Pip Value (Loss): ${symbolInfo.TradeTickValueLoss:F2}");
}
```

```csharp
// Check if XAUUSD is available for trading now
var now = DateTime.Now;
var sessionData = await account.SymbolInfoSessionTradeAsync(new SymbolInfoSessionTradeRequest
{
    Symbol = "XAUUSD",
    DayOfWeek = now.DayOfWeek,
    SessionIndex = 0  // First session of the day
});

var sessionStart = sessionData.From.ToDateTime();
var sessionEnd = sessionData.To.ToDateTime();
var currentTime = TimeSpan.Parse(now.ToString("HH:mm:ss"));

if (currentTime >= sessionStart.TimeOfDay && currentTime <= sessionEnd.TimeOfDay)
{
    Console.WriteLine("✅ Market is OPEN for trading");
}
else
{
    Console.WriteLine($"❌ Market CLOSED - Opens at {sessionStart:HH:mm}");
}
```

---

## See also

* **Basic Symbol Info:** [SymbolInfoDouble/Integer/String](../2.%20Symbol_information/Symbol_Information.Overview.md) - simpler methods
* **Trading:** [OrderCalcMargin](../4.%20Trading_Operattons/OrderCalcMargin.md) - calculate margin for specific order
* **Account:** [AccountSummary](../1.%20Account_information/AccountSummary.md) - check available margin
