# ✅ Adding/Removing Symbol from Market Watch

> **Request:** select or deselect symbol in Market Watch from **MT5**. Add a symbol to Market Watch or remove it.

**API Information:**

* **SDK wrapper:** `MT5Account.SymbolSelectAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `SymbolSelect` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `SymbolSelect(SymbolSelectRequest) → SymbolSelectReply`
* **Low‑level client (generated):** `MarketInfo.SymbolSelect(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace mt5_term_api
{
    public class MT5Account
    {
        public async Task<SymbolSelectData> SymbolSelectAsync(
            string symbolName,
            bool select,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`SymbolSelectRequest { symbol: string, select: bool }`


**Reply message:**

`SymbolSelectReply { data: SymbolSelectData }` or `{ error: Error }`

---

## 🔽 Input

| Parameter           | Type                | Required | Description                                                                                                                                         |
| ------------------- | ------------------- | -------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| `symbolName`        | `string`            | ✅       | Symbol name (e.g., `"EURUSD"`)                                                                                                                      |
| `select`            | `bool`              | ✅       | `true` - add symbol to Market Watch, `false` - remove symbol from Market Watch                                                                      |
| `deadline`          | `DateTime?`         | ❌       | Absolute per‑call **UTC** deadline → converted to timeout                                                                                           |
| `cancellationToken` | `CancellationToken` | ❌       | Cooperative cancel for the call/retry loop                                                                                                          |

---

## ⬆️ Output — `SymbolSelectData`

| Field     | Type   | Description                                                       |
| --------- | ------ | ----------------------------------------------------------------- |
| `Success` | `bool` | `true` if operation succeeded, `false` if failed                  |

---

## 💬 Just the essentials

* **What it is.** Single RPC to add/remove symbols from Market Watch.
* **Why you need it.** Enable symbols for trading, receive quotes, ensure symbol data is available.
* **Two modes.** `select=true` → add to Market Watch, `select=false` → remove from Market Watch.
* **Important.** Symbol cannot be removed if chart is open or there are open positions.

---

## 🎯 Purpose

Use this method when you need to:

* Add symbols to Market Watch before trading or getting quotes.
* Ensure a symbol is visible and receiving data updates.
* Clean up Market Watch by removing unused symbols.
* Prepare symbols for trading operations.
* Dynamically manage Market Watch list based on trading strategy.

---

## 🧩 Notes & Tips

* **Always add symbols to Market Watch before trading** - many operations require the symbol to be selected.
* If `Success=false`, check: symbol might not exist, chart might be open, or positions might be active.
* **Cannot remove symbol if:**
  - A chart for this symbol is open in MT5 terminal
  - There are open positions for this symbol
  - Symbol is being used by an active order
* Use `SymbolExistAsync()` first to verify symbol exists before selecting.
* Selecting an already-selected symbol returns `Success=true` (idempotent operation).
* Fast operation - use short timeout (3-5s).
* Some brokers automatically select symbols when you open positions - check with `SymbolsTotalAsync()`.

---

## 🔗 Usage Examples

### 1) Add symbol to Market Watch

```csharp
// Add EURUSD to Market Watch
var result = await acc.SymbolSelectAsync(
    symbolName: "EURUSD",
    select: true,
    deadline: DateTime.UtcNow.AddSeconds(3));

if (result.Success)
{
    Console.WriteLine("✅ EURUSD added to Market Watch");
}
else
{
    Console.WriteLine("❌ Failed to add EURUSD");
}
```

### 2) Remove symbol from Market Watch

```csharp
// Remove GBPJPY from Market Watch
var result = await acc.SymbolSelectAsync(
    symbolName: "GBPJPY",
    select: false);

if (result.Success)
{
    Console.WriteLine("✅ GBPJPY removed from Market Watch");
}
else
{
    Console.WriteLine("⚠️ Cannot remove GBPJPY (chart open or positions active?)");
}
```

### 3) Ensure symbol is available before trading

```csharp
// Ensure symbol is in Market Watch before placing order
var symbol = "EURUSD";

// Check if exists
var exists = await acc.SymbolExistAsync(symbol);
if (!exists.Exists)
{
    Console.WriteLine($"❌ Symbol '{symbol}' doesn't exist");
    return;
}

// Add to Market Watch
var selected = await acc.SymbolSelectAsync(symbol, select: true);
if (!selected.Success)
{
    Console.WriteLine($"❌ Failed to add '{symbol}' to Market Watch");
    return;
}

Console.WriteLine($"✅ '{symbol}' ready for trading");
// Continue with trading operations...
```

### 4) Add multiple symbols

```csharp
// Add a list of symbols to Market Watch
var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY", "XAUUSD" };

Console.WriteLine("Adding symbols to Market Watch:");
foreach (var sym in symbols)
{
    var result = await acc.SymbolSelectAsync(sym, select: true);
    var status = result.Success ? "✅" : "❌";
    Console.WriteLine($"  {status} {sym}");
}
```

### 5) Toggle symbol visibility

```csharp
// Toggle symbol in Market Watch
var symbol = "BTCUSD";
bool shouldAdd = true; // from user input or logic

var result = await acc.SymbolSelectAsync(symbol, select: shouldAdd);

if (result.Success)
{
    var action = shouldAdd ? "added to" : "removed from";
    Console.WriteLine($"✅ {symbol} {action} Market Watch");
}
else
{
    Console.WriteLine($"❌ Failed to modify {symbol}");
}
```
