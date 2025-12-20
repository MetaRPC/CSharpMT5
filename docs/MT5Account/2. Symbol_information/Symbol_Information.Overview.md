# MT5Account · Symbol Information - Overview

> Symbol properties, trading sessions, market data, and symbol availability. Use this page to choose the right API for accessing symbol information.

## 📁 What lives here

* **[SymbolInfoDouble](./SymbolInfoDouble.md)** - **single double value** from symbol (bid, ask, spread, tick size, etc.).
* **[SymbolInfoInteger](./SymbolInfoInteger.md)** - **single integer value** from symbol (digits, spread, time flags, etc.).
* **[SymbolInfoString](./SymbolInfoString.md)** - **single string value** from symbol (description, path, category, etc.).
* **[SymbolInfoTick](./SymbolInfoTick.md)** - **latest tick data** for symbol (bid, ask, last, volume, time).
* **[SymbolsTotal](./SymbolsTotal.md)** - **count** of available symbols (all or Market Watch only).
* **[SymbolExist](./SymbolExist.md)** - **check** if symbol exists (standard or custom).
* **[SymbolName](./SymbolName.md)** - **get symbol name** by index from list.
* **[SymbolSelect](./SymbolSelect.md)** - **add/remove** symbol to/from Market Watch.
* **[SymbolIsSynchronized](./SymbolIsSynchronized.md)** - **check** if symbol data is synchronized with server.

---

## 🧭 Plain English

* **SymbolInfo*** (Double/Integer/String) → grab **one specific property** of a symbol.
* **SymbolInfoTick** → get **current market prices** (bid/ask) for a symbol.
* **SymbolsTotal** → count how many symbols are available.
* **SymbolExist** → verify if a symbol exists before trading.
* **SymbolSelect** → enable symbol for quotes and trading.
* **SymbolName** → iterate through symbols by index.
* **SymbolIsSynchronized** → ensure data is fresh before trading.

> Rule of thumb: need **one property** → `SymbolInfo*Async`; need **current prices** → `SymbolInfoTickAsync`; need **symbol management** → Select/Exist/IsSynchronized.

---

## Quick choose

| If you need…                                     | Use                          | Returns                    | Key inputs                          |
| ------------------------------------------------ | ---------------------------- | -------------------------- | ----------------------------------- |
| One numeric value (bid, ask, spread, etc.)       | `SymbolInfoDoubleAsync`      | Single `double`            | Symbol name + Property enum         |
| One integer value (digits, time flags, etc.)     | `SymbolInfoIntegerAsync`     | Single `long`              | Symbol name + Property enum         |
| One text value (description, currency, etc.)     | `SymbolInfoStringAsync`      | Single `string`            | Symbol name + Property enum         |
| Current market prices (bid/ask/last)             | `SymbolInfoTickAsync`        | MrpcMqlTick object         | Symbol name                         |
| Count of available symbols                       | `SymbolsTotalAsync`          | `int`                      | Selected only (true/false)          |
| Check if symbol exists                           | `SymbolExistAsync`           | Exists + IsCustom flags    | Symbol name                         |
| Get symbol name by index                         | `SymbolNameAsync`            | `string`                   | Index + Selected flag               |
| Add/remove symbol from Market Watch              | `SymbolSelectAsync`          | Success flag               | Symbol name + Select flag           |
| Check if symbol data is synchronized             | `SymbolIsSynchronizedAsync`  | `bool`                     | Symbol name                         |

---

## ❌ Cross‑refs & gotchas

* **SymbolSelect** must be called before accessing quotes for some symbols.
* **Spread** is in points, not pips - divide by 10 for 5-digit brokers.
* **Tick size** determines minimum price change - important for pending orders.
* **Contract size** affects lot calculations - 1 lot = contract size units.
* **Digits** determines price precision - use for rounding prices.
* **Session times** vary by broker and symbol - check trading hours.

---

## 🟢 Minimal snippets

```csharp
// Get current bid/ask prices
var tick = await account.SymbolInfoTickAsync("EURUSD");
Console.WriteLine($"EURUSD - Bid: {tick.Bid:F5}, Ask: {tick.Ask:F5}, Spread: {(tick.Ask - tick.Bid) * 100000:F1} points");
```

```csharp
// Get symbol spread
var spread = await account.SymbolInfoIntegerAsync("EURUSD",
    SymbolInfoIntegerProperty.SymbolSpread);
Console.WriteLine($"Spread: {spread} points");
```

```csharp
// Check if symbol exists before trading
var existData = await account.SymbolExistAsync("BTCUSD");
if (existData.Exists)
{
    Console.WriteLine($"Symbol exists! Is custom: {existData.IsCustom}");
}
else
{
    Console.WriteLine("Symbol not found!");
}
```

```csharp
// Add symbol to Market Watch
var success = await account.SymbolSelectAsync("XAUUSD", select: true);
if (success.Success)
{
    // Wait for synchronization
    var synced = await account.SymbolIsSynchronizedAsync("XAUUSD");
    if (synced.Synchronized)
    {
        Console.WriteLine("Symbol ready for trading!");
    }
}
```

```csharp
// Iterate through all symbols in Market Watch
var total = await account.SymbolsTotalAsync(selectedOnly: true);
for (int i = 0; i < total.Total; i++)
{
    var symbolData = await account.SymbolNameAsync(i, selected: true);
    Console.WriteLine($"[{i}] {symbolData.Name}");
}
```

---

## See also

* **Streaming:** [SubscribeToTicks](../7.%20Streaming_Methods/SubscribeToTicks.md) - real-time tick updates
* **Trading:** [OrderSend](../4.%20Trading_Operattons/OrderSend.md) - place orders using symbol names
* **Market Depth:** [MarketBookGet](../5.%20Market_Depth(DOM)/MarketBookGet.md) - order book for symbols
* **Additional:** [SymbolParamsMany](../6.%20Addittional_Methods/SymbolParamsMany.md) - get detailed symbol properties
