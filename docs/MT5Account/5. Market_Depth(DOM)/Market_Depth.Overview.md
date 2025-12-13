# MT5Account · Market Depth (DOM) - Overview

> Depth of Market (Order Book) data - bid/ask price levels with volumes. Use this page to choose the right API for DOM operations.

## 📁 What lives here

* **[MarketBookAdd](./MarketBookAdd.md)** - **subscribe** to DOM updates for a symbol.
* **[MarketBookGet](./MarketBookGet.md)** - **get current** order book snapshot.
* **[MarketBookRelease](./MarketBookRelease.md)** - **unsubscribe** from DOM updates.

---

## 🧭 Plain English

* **MarketBookAdd** → open subscription to order book (required first step).
* **MarketBookGet** → fetch current bid/ask levels with volumes.
* **MarketBookRelease** → close subscription when done (free resources).

> **Important:** Most Forex brokers don't provide DOM data. Available mainly for futures, stocks, and some crypto symbols.

> Workflow: `MarketBookAddAsync` → `MarketBookGetAsync` (repeatedly) → `MarketBookReleaseAsync` when done.

---

## Quick choose

| If you need…                                     | Use                       | Returns                    | Key inputs                          |
| ------------------------------------------------ | ------------------------- | -------------------------- | ----------------------------------- |
| Subscribe to order book                          | `MarketBookAddAsync`      | Success flag               | Symbol name                         |
| Get current order book snapshot                  | `MarketBookGetAsync`      | List of price levels       | Symbol name (must be subscribed)    |
| Unsubscribe from order book                      | `MarketBookReleaseAsync`  | Success flag               | Symbol name                         |

---

## ❌ Cross‑refs & gotchas

* **Broker support:** Not all brokers provide DOM. Forex spot pairs rarely have DOM.
* **Subscription required:** Must call `MarketBookAddAsync` before `MarketBookGetAsync`.
* **Price levels:** Each level shows price, volume, and type (Buy/Sell/Market).
* **Volume** is in contracts/lots, not in currency.
* **Real-time updates:** For streaming DOM use SubscribeToBookEvent (if available).
* **Resource management:** Always call `MarketBookReleaseAsync` when done.

---

## 🟢 Minimal snippets

```csharp
// Subscribe to DOM
var addResult = await account.MarketBookAddAsync("EURUSD");
if (addResult.OpenedSuccessfully)
{
    Console.WriteLine("✅ DOM subscription opened");
}
else
{
    Console.WriteLine("❌ DOM not available for this symbol");
    return;
}
```

```csharp
// Get order book snapshot
var bookData = await account.MarketBookGetAsync("EURUSD");
if (bookData.MqlBookInfos.Any())
{
    Console.WriteLine("Order Book:");
    foreach (var entry in bookData.MqlBookInfos)
    {
        var side = entry.Type == BookType.Buy ? "BID" : "ASK";
        Console.WriteLine($"{side} {entry.Price:F5} | Volume: {entry.VolumeReal:F2}");
    }
}
else
{
    Console.WriteLine("No DOM data available");
}
```

```csharp
// Analyze liquidity at price levels
var bookData = await account.MarketBookGetAsync("BTCUSD");
var bids = bookData.MqlBookInfos.Where(x => x.Type == BookType.Buy).ToList();
var asks = bookData.MqlBookInfos.Where(x => x.Type == BookType.Sell).ToList();

var bidVolume = bids.Sum(x => x.VolumeReal);
var askVolume = asks.Sum(x => x.VolumeReal);

Console.WriteLine($"Total Bid Volume: {bidVolume:F2}, Total Ask Volume: {askVolume:F2}");
Console.WriteLine($"Bid/Ask Ratio: {(bidVolume / askVolume):F2}");
```

```csharp
// Unsubscribe when done
var releaseResult = await account.MarketBookReleaseAsync("EURUSD");
if (releaseResult.ClosedSuccessfully)
{
    Console.WriteLine("✅ DOM subscription closed");
}
```

```csharp
// Full workflow with error handling
try
{
    var symbol = "XAUUSD";

    // 1. Subscribe
    var added = await account.MarketBookAddAsync(symbol);
    if (!added.OpenedSuccessfully)
    {
        Console.WriteLine($"DOM not available for {symbol}");
        return;
    }

    // 2. Get data (can be called multiple times)
    for (int i = 0; i < 10; i++)
    {
        var book = await account.MarketBookGetAsync(symbol);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Levels: {book.MqlBookInfos.Count}");
        await Task.Delay(1000);
    }

    // 3. Unsubscribe
    await account.MarketBookReleaseAsync(symbol);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

---

## See also

* **Symbols:** [SymbolInfoTick](../2.%20Symbol_information/SymbolInfoTick.md) - get bid/ask prices without DOM
* **Symbols:** [SymbolExist](../2.%20Symbol_information/SymbolExist.md) - check if symbol exists before subscribing
* **Trading:** [OrderSend](../4.%20Trading_Operattons/OrderSend.md) - place orders based on DOM analysis
