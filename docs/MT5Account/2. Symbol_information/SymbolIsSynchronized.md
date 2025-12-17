# ✅ Checking Symbol Data Synchronization

> **Request:** check if symbol data is synchronized with trade server from **MT5**. Verify that symbol quotes and data are up-to-date.

**API Information:**

* **SDK wrapper:** `MT5Account.SymbolIsSynchronizedAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `SymbolIsSynchronized` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `SymbolIsSynchronized(SymbolIsSynchronizedRequest) → SymbolIsSynchronizedReply`
* **Low‑level client (generated):** `MarketInfo.SymbolIsSynchronized(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace mt5_term_api
{
    public class MT5Account
    {
        public async Task<SymbolIsSynchronizedData> SymbolIsSynchronizedAsync(
            string symbolName,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`SymbolIsSynchronizedRequest { symbol: string }`


**Reply message:**

`SymbolIsSynchronizedReply { data: SymbolIsSynchronizedData }` or `{ error: Error }`

---

## 🔽 Input

| Parameter           | Type                | Required | Description                                               |
| ------------------- | ------------------- | -------- | --------------------------------------------------------- |
| `symbolName`        | `string`            | ✅       | Symbol name to check (e.g., `"EURUSD"`)                  |
| `deadline`          | `DateTime?`         | ❌       | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | ❌       | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `SymbolIsSynchronizedData`

| Field          | Type   | Description                                                                      |
| -------------- | ------ | -------------------------------------------------------------------------------- |
| `Synchronized` | `bool` | `true` if data are synchronized with server, `false` if not synchronized or stale |

---

## 💬 Just the essentials

* **What it is.** Single RPC checking if symbol's market data is synchronized with the trade server.
* **Why you need it.** Ensure quotes are fresh before trading, detect connection issues, wait for data after adding symbol.
* **Use case.** After `SymbolSelectAsync()`, check if data is synchronized before requesting quotes or placing orders.

---

## 🎯 Purpose

Use this method when you need to:

* Verify symbol data is up-to-date before trading operations.
* Wait for synchronization after adding a symbol to Market Watch.
* Detect connection/data feed issues for specific symbols.
* Ensure quotes are fresh before calculating trading parameters.
* Diagnose why quotes might be missing or stale.

---

## 🧩 Notes & Tips

* **Check after `SymbolSelectAsync()`** - newly added symbols may take time to sync.
* If `Synchronized=false`, symbol data might be stale or connection is poor.
* Use retry logic with delays when waiting for synchronization (e.g., after adding symbol).
* Fast operation - use short timeout (3-5s).
* Symbols not in Market Watch may always return `false`.
* During market close, some symbols may not be synchronized (normal behavior).
* **Don't trade if not synchronized** - quotes may be outdated.

---

## 🔗 Usage Examples

### 1) Check if symbol is synchronized

```csharp
// Check EURUSD synchronization status
var result = await acc.SymbolIsSynchronizedAsync(
    symbolName: "EURUSD",
    deadline: DateTime.UtcNow.AddSeconds(3));

if (result.Synchronized)
{
    Console.WriteLine("✅ EURUSD data is synchronized");
}
else
{
    Console.WriteLine("⚠️ EURUSD data is NOT synchronized");
}
```

### 2) Wait for synchronization after adding symbol

```csharp
// Add symbol and wait for synchronization
var symbol = "GBPJPY";

// Add to Market Watch
await acc.SymbolSelectAsync(symbol, select: true);
Console.WriteLine($"Added {symbol} to Market Watch");

// Wait for synchronization (with retries)
bool isSynced = false;
for (int attempt = 0; attempt < 10; attempt++)
{
    var result = await acc.SymbolIsSynchronizedAsync(symbol);
    if (result.Synchronized)
    {
        isSynced = true;
        break;
    }

    Console.WriteLine($"  Waiting for sync... (attempt {attempt + 1})");
    await Task.Delay(500); // Wait 500ms before retry
}

if (isSynced)
{
    Console.WriteLine($"✅ {symbol} synchronized and ready");
}
else
{
    Console.WriteLine($"⚠️ {symbol} failed to synchronize");
}
```

### 3) Verify before trading

```csharp
// Ensure symbol is synchronized before placing order
var symbol = "EURUSD";

// Check synchronization
var syncStatus = await acc.SymbolIsSynchronizedAsync(symbol);

if (!syncStatus.Synchronized)
{
    Console.WriteLine($"❌ Cannot trade {symbol}: data not synchronized");
    return;
}

// Safe to get quotes and trade
var tick = await acc.SymbolInfoTickAsync(symbol);
Console.WriteLine($"✅ {symbol} Bid: {tick.Bid}, Ask: {tick.Ask}");
// Continue with trading...
```

### 4) Check multiple symbols

```csharp
// Verify synchronization for multiple symbols
var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY", "XAUUSD" };

Console.WriteLine("Symbol synchronization status:");
foreach (var sym in symbols)
{
    var result = await acc.SymbolIsSynchronizedAsync(sym);
    var status = result.Synchronized ? "✅" : "⚠️";
    var text = result.Synchronized ? "synced" : "NOT synced";
    Console.WriteLine($"  {status} {sym,-10} {text}");
}
```

### 5) Diagnostic check for connection issues

```csharp
// Check if any selected symbols are not synchronized (connection issue)
var total = await acc.SymbolsTotalAsync(selectedOnly: true);
int notSyncedCount = 0;

for (int i = 0; i < total.Total; i++)
{
    var symbolData = await acc.SymbolNameAsync(i, selected: true);
    var syncStatus = await acc.SymbolIsSynchronizedAsync(symbolData.Name);

    if (!syncStatus.Synchronized)
    {
        Console.WriteLine($"⚠️ {symbolData.Name} not synchronized");
        notSyncedCount++;
    }
}

if (notSyncedCount == 0)
{
    Console.WriteLine($"✅ All {total.Total} symbols synchronized");
}
else
{
    Console.WriteLine($"⚠️ {notSyncedCount} of {total.Total} symbols not synchronized");
}
```
