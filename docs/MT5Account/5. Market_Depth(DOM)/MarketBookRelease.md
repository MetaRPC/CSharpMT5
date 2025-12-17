# ✅ Unsubscribe from Market Depth (`MarketBookReleaseAsync`)

> **Request:** Unsubscribe from Depth of Market (DOM) updates for a symbol on **MT5**. Closes DOM subscription and frees resources.

**API Information:**

* **SDK wrapper:** `MT5Account.MarketBookReleaseAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `MarketBookRelease` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `MarketBookRelease(MarketBookReleaseRequest) → MarketBookReleaseReply`
* **Low‑level client (generated):** `MarketInfo.MarketInfoClient.MarketBookRelease(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace mt5_term_api
{
    public class MT5Account
    {
        public async Task<MarketBookReleaseData> MarketBookReleaseAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`MarketBookReleaseRequest { symbol }`


**Reply message:**

`MarketBookReleaseReply { data: MarketBookReleaseData }`

---

## 🔽 Input

| Parameter           | Type                | Description                                               |
| ------------------- | ------------------- | --------------------------------------------------------- |
| `symbol`            | `string`            | Symbol name (previously subscribed via `MarketBookAddAsync`) |
| `deadline`          | `DateTime?`         | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `MarketBookReleaseData`

| Field                 | Type   | Description                                           |
| --------------------- | ------ | ----------------------------------------------------- |
| `ClosedSuccessfully`  | `bool` | `true` if unsubscription successful                   |

---

## 💬 Just the essentials

* **What it is.** Closes DOM subscription for a symbol. Stops receiving order book updates and frees resources.
* **Why you need it.** Clean up subscriptions when done. Reduce server/terminal load by unsubscribing unused symbols.
* **Sanity check.** If `ClosedSuccessfully == true` → subscription closed. Can no longer get DOM via `MarketBookGetAsync`.

---

## 🎯 Purpose

Use it to clean up DOM subscriptions:

* Free terminal/server resources.
* Stop receiving DOM updates for unused symbols.
* Proper cleanup before shutdown.

---

## 🧩 Notes & Tips

* **Resource management:** Always unsubscribe when done to avoid resource leaks.
* **Multiple symbols:** Call separately for each subscribed symbol.
* **Already unsubscribed:** Returns `true` even if symbol wasn't subscribed.
* **No penalty:** Safe to call multiple times for same symbol.
* **Cleanup order:** Unsubscribe before disconnecting from MT5.

---

## 🔗 Usage Examples

### 1) Basic unsubscribe

```csharp
// acc — connected MT5Account

var result = await acc.MarketBookReleaseAsync("BTCUSD");

if (result.ClosedSuccessfully)
{
    Console.WriteLine("✓ DOM subscription closed for BTCUSD");
}
else
{
    Console.WriteLine("⚠ Failed to close DOM subscription");
}
```

---

### 2) Subscribe, use, then unsubscribe pattern

```csharp
var symbol = "ETHUSD";

// 1. Subscribe
var subResult = await acc.MarketBookAddAsync(symbol);
if (!subResult.OpenedSuccessfully)
{
    Console.WriteLine("DOM not available");
    return;
}

try
{
    // 2. Use DOM data
    var domData = await acc.MarketBookGetAsync(symbol);
    Console.WriteLine($"DOM entries: {domData.MqlBookInfos.Count}");

    // Process DOM data...
}
finally
{
    // 3. Always unsubscribe (cleanup)
    var releaseResult = await acc.MarketBookReleaseAsync(symbol);
    Console.WriteLine($"Cleanup: {releaseResult.ClosedSuccessfully}");
}
```

---

### 3) Unsubscribe from multiple symbols

```csharp
var symbols = new[] { "BTCUSD", "ETHUSD", "XAUUSD" };

foreach (var symbol in symbols)
{
    var result = await acc.MarketBookReleaseAsync(symbol);
    Console.WriteLine($"{symbol}: {(result.ClosedSuccessfully ? "✓ Unsubscribed" : "✗ Failed")}");
}
```

---

### 4) Using statement pattern (automatic cleanup)

```csharp
async Task UseDomData(MT5Account acc, string symbol)
{
    // Subscribe
    var subResult = await acc.MarketBookAddAsync(symbol);
    if (!subResult.OpenedSuccessfully)
        return;

    try
    {
        // Use DOM
        for (int i = 0; i < 5; i++)
        {
            var domData = await acc.MarketBookGetAsync(symbol);
            Console.WriteLine($"Iteration {i}: {domData.MqlBookInfos.Count} entries");
            await Task.Delay(1000);
        }
    }
    finally
    {
        // Cleanup
        await acc.MarketBookReleaseAsync(symbol);
    }
}

// Call
await UseDomData(acc, "BTCUSD");
```

---

### 5) Graceful shutdown with error handling

```csharp
var subscribedSymbols = new List<string> { "ES", "NQ", "YM" };

// Subscribe to all
foreach (var symbol in subscribedSymbols)
{
    await acc.MarketBookAddAsync(symbol);
}

// ... use DOM data ...

// Graceful shutdown: unsubscribe from all
Console.WriteLine("Shutting down, cleaning up DOM subscriptions...");

foreach (var symbol in subscribedSymbols)
{
    try
    {
        var result = await acc.MarketBookReleaseAsync(
            symbol,
            deadline: DateTime.UtcNow.AddSeconds(3));

        if (result.ClosedSuccessfully)
        {
            Console.WriteLine($"✓ {symbol} unsubscribed");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ {symbol} unsubscribe failed: {ex.Message}");
    }
}

Console.WriteLine("Cleanup complete");
```

---

### 6) Conditional unsubscribe (check if subscribed first)

```csharp
var symbol = "BTCUSD";

// Try to get DOM data first
try
{
    var domData = await acc.MarketBookGetAsync(symbol);

    if (domData.MqlBookInfos.Count > 0)
    {
        // Has data, so was subscribed
        var result = await acc.MarketBookReleaseAsync(symbol);
        Console.WriteLine($"✓ Unsubscribed from {symbol}");
    }
    else
    {
        Console.WriteLine($"⚠ {symbol} not subscribed or no data");
    }
}
catch
{
    Console.WriteLine($"⚠ {symbol} not subscribed");
}
```
