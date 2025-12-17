# ✅ Checking if Symbol Exists

> **Request:** check symbol existence from **MT5**. Verify if a symbol with a specified name exists (standard or custom).

**API Information:**

* **SDK wrapper:** `MT5Account.SymbolExistAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `SymbolExist` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `SymbolExist(SymbolExistRequest) → SymbolExistReply`
* **Low‑level client (generated):** `MarketInfo.SymbolExist(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace mt5_term_api
{
    public class MT5Account
    {
        public async Task<SymbolExistData> SymbolExistAsync(
            string symbolName,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`SymbolExistRequest { name: string }`


**Reply message:**

`SymbolExistReply { data: SymbolExistData }` or `{ error: Error }`

---

## 🔽 Input

| Parameter           | Type                | Required | Description                                               |
| ------------------- | ------------------- | -------- | --------------------------------------------------------- |
| `symbolName`        | `string`            | ✅       | Symbol name to check (e.g., `"EURUSD"`, `"BTCUSD"`)      |
| `deadline`          | `DateTime?`         | ❌       | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | ❌       | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `SymbolExistData`

| Field      | Type   | Description                                                                                                   |
| ---------- | ------ | ------------------------------------------------------------------------------------------------------------- |
| `Exists`   | `bool` | `true` if symbol exists (standard or custom), `false` if not found                                            |
| `IsCustom` | `bool` | `true` if the detected symbol is a custom symbol, `false` for standard symbols (see [MQL5 Custom Symbols](https://www.mql5.com/en/docs/customsymbols)) |

---

## 💬 Just the essentials

* **What it is.** Single RPC checking if a symbol exists and whether it's custom.
* **Why you need it.** Validate symbol names before trading, check if broker supports a symbol, detect custom symbols.
* **Two flags.** `Exists` tells if found, `IsCustom` tells if it's a custom (synthetic) symbol.

---

## 🎯 Purpose

Use this method when you need to:

* Verify a symbol name before attempting to trade or get quotes.
* Check if your broker supports a specific symbol.
* Detect whether a symbol is standard or custom/synthetic.
* Validate user input (symbol names) in your trading application.
* Handle different symbols gracefully (fail early if symbol doesn't exist).

---

## 🧩 Notes & Tips

* Symbol names are **case-sensitive** in some brokers (use exact spelling: `"EURUSD"` not `"eurusd"`).
* If `Exists=false`, the symbol is not available from your broker.
* If `Exists=true` and `IsCustom=true`, the symbol is synthetic/custom - may have different trading rules.
* Use this before `SymbolSelectAsync()` to avoid errors when adding symbols to Market Watch.
* Fast operation - use short timeout (3-5s).
* Standard symbols: forex pairs, stocks, indices, commodities from broker.
* Custom symbols: user-created synthetic instruments, spreads, custom data feeds.

---

## 🔗 Usage Examples

### 1) Check if symbol exists

```csharp
// Verify EURUSD is available
var result = await acc.SymbolExistAsync(
    "EURUSD",
    deadline: DateTime.UtcNow.AddSeconds(3));

if (result.Exists)
{
    Console.WriteLine("✅ EURUSD exists");
}
else
{
    Console.WriteLine("❌ EURUSD not found");
}
```

### 2) Detect custom symbols

```csharp
// Check if symbol is custom/synthetic
var btc = await acc.SymbolExistAsync("BTCUSD");

Console.WriteLine($"Exists:   {btc.Exists}");
Console.WriteLine($"Custom:   {btc.IsCustom}");

if (btc.Exists && btc.IsCustom)
{
    Console.WriteLine("⚠️ This is a custom/synthetic symbol");
}
```

### 3) Validate user input

```csharp
// Validate symbol before trading
string userSymbol = "GBPJPY"; // from user input

var check = await acc.SymbolExistAsync(userSymbol);

if (!check.Exists)
{
    Console.WriteLine($"❌ Error: Symbol '{userSymbol}' not available");
    return;
}

Console.WriteLine($"✅ Symbol '{userSymbol}' is valid");
// Continue with trading logic...
```

### 4) Check multiple symbols

```csharp
// Verify a list of symbols
var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY", "XAUUSD" };

Console.WriteLine("Checking symbols:");
foreach (var sym in symbols)
{
    var result = await acc.SymbolExistAsync(sym);
    var status = result.Exists ? "✅" : "❌";
    var type = result.IsCustom ? "(custom)" : "(standard)";
    Console.WriteLine($"  {status} {sym,-10} {(result.Exists ? type : "")}");
}
```

### 5) Safe symbol selection

```csharp
// Check existence before adding to Market Watch
var symbolName = "EURUSD";
var exists = await acc.SymbolExistAsync(symbolName);

if (!exists.Exists)
{
    Console.WriteLine($"❌ Cannot add '{symbolName}': symbol doesn't exist");
    return;
}

// Symbol exists - safe to add to Market Watch
var selected = await acc.SymbolSelectAsync(symbolName, select: true);
Console.WriteLine($"✅ '{symbolName}' added to Market Watch");
```
