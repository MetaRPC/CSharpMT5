# ✅ Getting Symbol Name by Index

> **Request:** symbol name by index from **MT5**. Retrieve the name of a symbol from the list (selected in Market Watch or all available).

**API Information:**

* **SDK wrapper:** `MT5Account.SymbolNameAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `SymbolName` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `SymbolName(SymbolNameRequest) → SymbolNameReply`
* **Low‑level client (generated):** `MarketInfo.SymbolName(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<SymbolNameData> SymbolNameAsync(
            int index,
            bool selected,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`SymbolNameRequest { index: int32, selected: bool }`


**Reply message:**

`SymbolNameReply { data: SymbolNameData }` or `{ error: Error }`

---

## 🔽 Input

| Parameter           | Type                | Required | Description                                                                                                             |
| ------------------- | ------------------- | -------- | ----------------------------------------------------------------------------------------------------------------------- |
| `index`             | `int`               | ✅       | Order number of a symbol (zero-based index)                                                                             |
| `selected`          | `bool`              | ✅       | `true` - get from Market Watch symbols, `false` - get from all available symbols                                        |
| `deadline`          | `DateTime?`         | ❌       | Absolute per‑call **UTC** deadline → converted to timeout                                                               |
| `cancellationToken` | `CancellationToken` | ❌       | Cooperative cancel for the call/retry loop                                                                              |

---

## ⬆️ Output — `SymbolNameData`

| Field  | Type     | Description                         |
| ------ | -------- | ----------------------------------- |
| `Name` | `string` | Symbol name (e.g., `"EURUSD"`)      |

---

## 💬 Just the essentials

* **What it is.** Single RPC returning symbol name by its index in the list.
* **Why you need it.** Iterate through symbols, enumerate Market Watch, discover available symbols.
* **Two modes.** `selected=true` → symbols in Market Watch, `selected=false` → all available symbols.
* **Index-based.** Use with `SymbolsTotalAsync()` to get the count first, then iterate by index.

---

## 🎯 Purpose

Use this method when you need to:

* Enumerate all symbols in Market Watch (iterate from `0` to `SymbolsTotal-1`).
* Discover all available symbols from your broker.
* Build a list of trading instruments for your application.
* Get the first/last symbol in the list.

---

## 🧩 Notes & Tips

* **Always call `SymbolsTotalAsync()` first** to get the valid index range.
* Index is **zero-based**: first symbol = `0`, last symbol = `total - 1`.
* If index is out of range, you'll get an error.
* Use `selected=true` for symbols currently in Market Watch (faster, smaller list).
* Use `selected=false` to discover all symbols your broker offers.
* The order of symbols may change if Market Watch is modified during iteration.
* Fast operation - use short timeout (3-5s).

---

## 🔗 Usage Examples

### 1) Get first symbol from Market Watch

```csharp
// Get the first symbol in Market Watch
var firstSymbol = await acc.SymbolNameAsync(
    index: 0,
    selected: true,
    deadline: DateTime.UtcNow.AddSeconds(3));
Console.WriteLine($"First symbol: {firstSymbol.Name}");
```

### 2) Enumerate all symbols in Market Watch

```csharp
// Get count and iterate through all Market Watch symbols
var total = await acc.SymbolsTotalAsync(selectedOnly: true);

Console.WriteLine($"Symbols in Market Watch ({total.Total}):");
for (int i = 0; i < total.Total; i++)
{
    var symbolData = await acc.SymbolNameAsync(index: i, selected: true);
    Console.WriteLine($"  [{i}] {symbolData.Name}");
}
```

### 3) Get all available symbols from broker

```csharp
// Enumerate all symbols broker offers (not just Market Watch)
var totalAll = await acc.SymbolsTotalAsync(selectedOnly: false);

Console.WriteLine($"All available symbols ({totalAll.Total}):");
for (int i = 0; i < Math.Min(totalAll.Total, 10); i++) // Show first 10
{
    var symbolData = await acc.SymbolNameAsync(index: i, selected: false);
    Console.WriteLine($"  [{i}] {symbolData.Name}");
}
Console.WriteLine($"... and {totalAll.Total - 10} more");
```

### 4) Build symbol list

```csharp
// Create a list of all symbols in Market Watch
var total = await acc.SymbolsTotalAsync(selectedOnly: true);
var symbolList = new List<string>();

for (int i = 0; i < total.Total; i++)
{
    var symbolData = await acc.SymbolNameAsync(i, selected: true);
    symbolList.Add(symbolData.Name);
}

Console.WriteLine($"Loaded {symbolList.Count} symbols:");
Console.WriteLine(string.Join(", ", symbolList));
```

### 5) Find specific symbol by iteration

```csharp
// Search for a symbol by iterating (not efficient, but demonstrates usage)
var total = await acc.SymbolsTotalAsync(selectedOnly: true);
string targetSymbol = "EURUSD";
int foundIndex = -1;

for (int i = 0; i < total.Total; i++)
{
    var symbolData = await acc.SymbolNameAsync(i, selected: true);
    if (symbolData.Name == targetSymbol)
    {
        foundIndex = i;
        break;
    }
}

if (foundIndex >= 0)
{
    Console.WriteLine($"✅ Found '{targetSymbol}' at index {foundIndex}");
}
else
{
    Console.WriteLine($"❌ '{targetSymbol}' not in Market Watch");
}
```
