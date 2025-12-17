# ✅ Get Market Depth Data (`MarketBookGetAsync`)

> **Request:** Get current Depth of Market (order book) snapshot for a subscribed symbol on **MT5**.

**API Information:**

* **SDK wrapper:** `MT5Account.MarketBookGetAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `MarketBookGet` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `MarketBookGet(MarketBookGetRequest) → MarketBookGetReply`
* **Low‑level client (generated):** `MarketInfo.MarketInfoClient.MarketBookGet(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace mt5_term_api
{
    public class MT5Account
    {
        public async Task<MarketBookGetData> MarketBookGetAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`MarketBookGetRequest { symbol }`


**Reply message:**

`MarketBookGetReply { data: MarketBookGetData }`

---

## 🔽 Input

| Parameter           | Type                | Description                                               |
| ------------------- | ------------------- | --------------------------------------------------------- |
| `symbol`            | `string`            | Symbol name (must be subscribed via `MarketBookAddAsync`) |
| `deadline`          | `DateTime?`         | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `MarketBookGetData`

| Field           | Type                              | Description                           |
| --------------- | --------------------------------- | ------------------------------------- |
| `MqlBookInfos`  | `List<MrpcMqlBookInfo>`           | Array of DOM entries (price levels)   |

### `MrpcMqlBookInfo` — DOM entry structure

| Field         | Type     | Description                                        |
| ------------- | -------- | -------------------------------------------------- |
| `Type`        | `BookType` | Order type (Sell, Buy, SellMarket, BuyMarket)    |
| `Price`       | `double` | Price level                                        |
| `Volume`      | `long`   | Volume at this price (in contracts/lots)           |
| `VolumeReal`  | `double` | Real volume at this price                          |

---

## 🧱 Related enums (from proto)

### `BookType`

* `Sell` (0) — Sell order (Offer/Ask side)
* `Buy` (1) — Buy order (Bid side)
* `SellMarket` (2) — Sell market order
* `BuyMarket` (3) — Buy market order

---

## 💬 Just the essentials

* **What it is.** Returns current order book snapshot with bid/ask price levels and volumes.
* **Why you need it.** Analyze liquidity, identify support/resistance, implement DOM-based strategies.
* **Sanity check.** If `MqlBookInfos.Count > 0` → DOM data available. Empty list = no data or not subscribed.

---

## 🎯 Purpose

Use it to analyze order book:

* Get bid/ask price levels.
* Analyze liquidity distribution.
* Identify large orders (support/resistance).
* Implement order flow strategies.

---

## 🧩 Notes & Tips

* **Subscription required:** Must call `MarketBookAddAsync` first, otherwise get empty list.
* **Snapshot data:** Returns current state at call time. For real-time updates use streaming (SubscribeToBookEvent).
* **Price sorting:** Entries typically sorted by price (best bid/ask first). Verify sorting in your code.
* **Empty result:** If `MqlBookInfos` is empty, either not subscribed or broker doesn't provide DOM.
* **Broker limitations:** Most Forex brokers don't provide DOM. Available mainly for futures, stocks, crypto.

---

## 🔗 Usage Examples

### 1) Basic DOM retrieval

```csharp
// acc — connected MT5Account
// Symbol must be subscribed via MarketBookAddAsync first

var domData = await acc.MarketBookGetAsync("BTCUSD");

Console.WriteLine($"DOM entries: {domData.MqlBookInfos.Count}");

foreach (var entry in domData.MqlBookInfos.Take(10))
{
    var side = entry.Type == BookType.BookTypeBuy ? "BID" : "ASK";
    Console.WriteLine($"{side} | Price: {entry.Price:F2} | Volume: {entry.VolumeReal:F4}");
}
```

---

### 2) Analyze best bid/ask

```csharp
var domData = await acc.MarketBookGetAsync("ETHUSD");

if (domData.MqlBookInfos.Count > 0)
{
    var bids = domData.MqlBookInfos
        .Where(e => e.Type == BookType.BookTypeBuy)
        .OrderByDescending(e => e.Price);

    var asks = domData.MqlBookInfos
        .Where(e => e.Type == BookType.BookTypeSell)
        .OrderBy(e => e.Price);

    var bestBid = bids.FirstOrDefault();
    var bestAsk = asks.FirstOrDefault();

    if (bestBid != null && bestAsk != null)
    {
        var spread = bestAsk.Price - bestBid.Price;
        Console.WriteLine($"Best Bid: {bestBid.Price} | Best Ask: {bestAsk.Price} | Spread: {spread}");
    }
}
```

---

### 3) Find large orders (liquidity analysis)

```csharp
var domData = await acc.MarketBookGetAsync("ES"); // E-mini S&P 500

// Find orders with volume > 100 contracts
var largeOrders = domData.MqlBookInfos
    .Where(e => e.Volume > 100)
    .OrderByDescending(e => e.Volume);

Console.WriteLine("Large orders (potential support/resistance):");
foreach (var order in largeOrders.Take(5))
{
    var side = order.Type == BookType.BookTypeBuy ? "BID" : "ASK";
    Console.WriteLine($"{side} | Price: {order.Price} | Volume: {order.Volume}");
}
```

---

### 4) Calculate total liquidity by side

```csharp
var domData = await acc.MarketBookGetAsync("XAUUSD");

var totalBidVolume = domData.MqlBookInfos
    .Where(e => e.Type == BookType.BookTypeBuy)
    .Sum(e => e.VolumeReal);

var totalAskVolume = domData.MqlBookInfos
    .Where(e => e.Type == BookType.BookTypeSell)
    .Sum(e => e.VolumeReal);

var bidAskRatio = totalBidVolume / (totalAskVolume > 0 ? totalAskVolume : 1);

Console.WriteLine($"Total Bid Liquidity: {totalBidVolume:F2}");
Console.WriteLine($"Total Ask Liquidity: {totalAskVolume:F2}");
Console.WriteLine($"Bid/Ask Ratio: {bidAskRatio:F2}");
```

---

### 5) Continuous DOM monitoring (polling)

```csharp
var symbol = "BTCUSD";

// Subscribe first
var subResult = await acc.MarketBookAddAsync(symbol);
if (!subResult.OpenedSuccessfully)
{
    Console.WriteLine("DOM not available");
    return;
}

// Poll DOM every second for 10 iterations
for (int i = 0; i < 10; i++)
{
    var domData = await acc.MarketBookGetAsync(symbol);

    if (domData.MqlBookInfos.Count > 0)
    {
        var bestBid = domData.MqlBookInfos
            .Where(e => e.Type == BookType.BookTypeBuy)
            .MaxBy(e => e.Price);

        var bestAsk = domData.MqlBookInfos
            .Where(e => e.Type == BookType.BookTypeSell)
            .MinBy(e => e.Price);

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Bid: {bestBid?.Price} | Ask: {bestAsk?.Price}");
    }

    await Task.Delay(1000); // Wait 1 second
}

// Unsubscribe when done
await acc.MarketBookReleaseAsync(symbol);
```

---

### 6) DOM-based entry strategy (example)

```csharp
// Simple strategy: Enter when large order appears near current price
var domData = await acc.MarketBookGetAsync("ES");
var currentPrice = (await acc.SymbolInfoTickAsync("ES")).Bid;

var nearbyLargeOrders = domData.MqlBookInfos
    .Where(e => Math.Abs(e.Price - currentPrice) < 5.0) // Within 5 points
    .Where(e => e.Volume > 200) // Large order
    .ToList();

if (nearbyLargeOrders.Any())
{
    Console.WriteLine($"⚠ {nearbyLargeOrders.Count} large order(s) near price {currentPrice}");

    foreach (var order in nearbyLargeOrders)
    {
        var side = order.Type == BookType.BookTypeBuy ? "Support" : "Resistance";
        Console.WriteLine($"{side} at {order.Price} with volume {order.Volume}");
    }

    // Implement your entry logic here
}
```
