# ✅ Subscribe to Market Depth (`MarketBookAddAsync`)

> **Request:** Subscribe to Depth of Market (DOM) updates for a symbol on **MT5**. Opens access to order book data.

**API Information:**

* **SDK wrapper:** `MT5Account.MarketBookAddAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `MarketBookAdd` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `MarketBookAdd(MarketBookAddRequest) → MarketBookAddReply`
* **Low‑level client (generated):** `MarketInfo.MarketInfoClient.MarketBookAdd(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<MarketBookAddData> MarketBookAddAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`MarketBookAddRequest { symbol }`


**Reply message:**

`MarketBookAddReply { data: MarketBookAddData }`

---

## 🔽 Input

| Parameter           | Type                | Description                                               |
| ------------------- | ------------------- | --------------------------------------------------------- |
| `symbol`            | `string`            | Symbol name (e.g., `"EURUSD"`, `"BTCUSD"`)                |
| `deadline`          | `DateTime?`         | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `MarketBookAddData`

| Field                 | Type   | Description                                           |
| --------------------- | ------ | ----------------------------------------------------- |
| `OpenedSuccessfully`  | `bool` | `true` if subscription opened successfully            |

---

## 💬 Just the essentials

* **What it is.** Opens Depth of Market subscription for a symbol. Required before calling `MarketBookGetAsync`.
* **Why you need it.** Access order book data (bid/ask levels, volumes). Required for DOM-based strategies.
* **Sanity check.** If `OpenedSuccessfully == true` → can now call `MarketBookGetAsync`.

---

## 🎯 Purpose

Use it to access market depth data:

* Subscribe to order book updates.
* Enable DOM data retrieval via `MarketBookGetAsync`.
* Analyze bid/ask liquidity levels.

---

## 🧩 Notes & Tips

* **Broker support:** Not all brokers provide DOM data. Forex brokers typically don't offer it for currency pairs.
* **Symbol availability:** DOM usually available for futures, stocks, some crypto. Rare for Forex spot.
* **Subscription required:** Must call this before `MarketBookGetAsync`, otherwise get empty data.
* **Resource usage:** Keep subscriptions minimal. Unsubscribe with `MarketBookReleaseAsync` when done.
* **Return value:** If `false`, broker doesn't provide DOM for this symbol.

---

## 🔗 Usage Examples

### 1) Basic DOM subscription

```csharp
// acc — connected MT5Account

var result = await acc.MarketBookAddAsync("BTCUSD");

if (result.OpenedSuccessfully)
{
    Console.WriteLine("✓ DOM subscription opened for BTCUSD");

    // Now can call MarketBookGetAsync
    var domData = await acc.MarketBookGetAsync("BTCUSD");
    Console.WriteLine($"DOM entries: {domData.MqlBookInfos.Count}");
}
else
{
    Console.WriteLine("✗ DOM not available for BTCUSD");
}
```

---

### 2) Subscribe with error handling

```csharp
try
{
    var result = await acc.MarketBookAddAsync(
        "EURUSD",
        deadline: DateTime.UtcNow.AddSeconds(5));

    if (!result.OpenedSuccessfully)
    {
        Console.WriteLine("⚠ Broker doesn't provide DOM for EURUSD");
    }
}
catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.DeadlineExceeded)
{
    Console.WriteLine("✗ DOM subscription timeout (broker may not support it)");
}
```

---

### 3) Subscribe to multiple symbols

```csharp
var symbols = new[] { "BTCUSD", "ETHUSD", "XAUUSD" };

foreach (var symbol in symbols)
{
    var result = await acc.MarketBookAddAsync(symbol);

    Console.WriteLine($"{symbol}: {(result.OpenedSuccessfully ? "✓ Subscribed" : "✗ Not available")}");
}
```

---

### 4) Subscribe and immediately check data

```csharp
var symbol = "ES"; // E-mini S&P 500 futures

var subResult = await acc.MarketBookAddAsync(symbol);

if (subResult.OpenedSuccessfully)
{
    // Wait a moment for data to populate
    await Task.Delay(100);

    var domData = await acc.MarketBookGetAsync(symbol);

    if (domData.MqlBookInfos.Count > 0)
    {
        Console.WriteLine($"✓ DOM active with {domData.MqlBookInfos.Count} price levels");
    }
    else
    {
        Console.WriteLine("⚠ Subscription opened but no data yet");
    }
}
```

---

### 5) Conditional subscription (check if symbol exists first)

```csharp
var symbol = "BTCUSD";

// First check if symbol exists
var symbolExists = await acc.SymbolExistAsync(symbol);

if (symbolExists.Exist)
{
    var result = await acc.MarketBookAddAsync(symbol);

    if (result.OpenedSuccessfully)
    {
        Console.WriteLine($"✓ DOM subscription opened for {symbol}");
    }
    else
    {
        Console.WriteLine($"⚠ Symbol exists but DOM not available");
    }
}
else
{
    Console.WriteLine($"✗ Symbol {symbol} not found");
}
```
