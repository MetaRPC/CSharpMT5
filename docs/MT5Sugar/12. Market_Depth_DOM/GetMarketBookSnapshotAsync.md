# Get Market Book Snapshot (`GetMarketBookSnapshotAsync`)

> **Sugar method:** Gets current order book (DOM) snapshot - wrapper over `MarketBookGetAsync()` with clearer naming.

**API Information:**

* **Extension method:** `MT5Service.GetMarketBookSnapshotAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `MetaRPC.CSharpMT5` library
* **Region:** [12] MARKET DEPTH (DOM)
* **Underlying calls:** `MarketBookGetAsync()`

---

## Method Signature

```csharp
public static Task<MarketBookGetData> GetMarketBookSnapshotAsync(
    this MT5Service svc,
    string symbol,
    int timeoutSec = 15,
    CancellationToken ct = default)
    => svc.MarketBookGetAsync(symbol, Dl(timeoutSec), ct);
```

---

## 🔽 Input

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance (extension method) |
| `symbol` | `string` | Symbol to query (e.g., "EURUSD") |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15) |
| `ct` | `CancellationToken` | Cancellation token |

---

## ⬆️ Output

| Type | Description |
|------|-------------|
| `Task<MarketBookGetData>` | Order book snapshot with bid/ask levels |

**Key fields:**

- `MqlBookInfos` - List of order book entries
  - `Price` - Price level
  - `Volume` - Volume at this level (in lots)
  - `Type` - `BookType.Buy` (bid) or `BookType.Sell` (ask)

---

## 💬 Just the essentials

* **What it is:** Gets current snapshot of order book (all bid/ask levels with volumes).
* **Why you need it:** Alias for `MarketBookGetAsync()` with clearer, more descriptive name.
* **⚠️ Prerequisite:** Must subscribe first using `SubscribeToMarketBookAsync()` or `MarketBookAddAsync()`.

---

## 🔗 Full Documentation

This method is a **simple wrapper** for better naming. For complete DOM usage, see:

* [SubscribeToMarketBookAsync.md](SubscribeToMarketBookAsync.md) - Subscription management
* [GetBestBidAskFromBookAsync.md](GetBestBidAskFromBookAsync.md) - Extract best prices
* [CalculateLiquidityAtLevelAsync.md](CalculateLiquidityAtLevelAsync.md) - Volume analysis

---

## 📋 Quick Example

```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");

    Console.WriteLine($"Order book depth: {book.MqlBookInfos.Count} levels");

    foreach (var entry in book.MqlBookInfos)
    {
        string side = entry.Type == BookType.Buy ? "BID" : "ASK";
        Console.WriteLine($"{side}: {entry.Price:F5} - {entry.Volume} lots");
    }
}
```

---

## 💡 Summary

**GetMarketBookSnapshotAsync** = clearer alias for `MarketBookGetAsync()`:

* ✅ More descriptive method name
* ✅ Same functionality as `MarketBookGetAsync()`
* ✅ Returns full order book snapshot
* ⚠️ Requires active subscription first

```csharp
// These are identical:
var book1 = await svc.MarketBookGetAsync("EURUSD", deadline, ct);
var book2 = await svc.GetMarketBookSnapshotAsync("EURUSD");
```
