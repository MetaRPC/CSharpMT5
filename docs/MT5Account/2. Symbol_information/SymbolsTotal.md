# ✅ Getting Total Number of Symbols

> **Request:** count of available symbols from **MT5**. Get the total number of symbols (all available or only selected in Market Watch).

**API Information:**

* **SDK wrapper:** `MT5Account.SymbolsTotalAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `SymbolsTotal` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `SymbolsTotal(SymbolsTotalRequest) → SymbolsTotalReply`
* **Low‑level client (generated):** `MarketInfo.SymbolsTotal(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<SymbolsTotalData> SymbolsTotalAsync(
            bool selectedOnly,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`SymbolsTotalRequest { mode: bool }`


**Reply message:**

`SymbolsTotalReply { data: SymbolsTotalData }` or `{ error: Error }`

---

## 🔽 Input

| Parameter           | Type                | Required | Description                                                                                         |
| ------------------- | ------------------- | -------- | --------------------------------------------------------------------------------------------------- |
| `selectedOnly`      | `bool`              | ✅       | `true` - count only symbols in Market Watch, `false` - count all available symbols                 |
| `deadline`          | `DateTime?`         | ❌       | Absolute per‑call **UTC** deadline → converted to timeout                                           |
| `cancellationToken` | `CancellationToken` | ❌       | Cooperative cancel for the call/retry loop                                                          |

---

## ⬆️ Output — `SymbolsTotalData`

| Field   | Type    | Description                                                                                                      |
| ------- | ------- | ---------------------------------------------------------------------------------------------------------------- |
| `Total` | `int32` | If `selectedOnly=true`, returns the number of symbols selected in Market Watch. Otherwise, total number of all symbols. |

---

## 💬 Just the essentials

* **What it is.** Single RPC returning the count of symbols (selected or all available).
* **Why you need it.** Check how many symbols are available, iterate through symbols, verify Market Watch configuration.
* **Two modes.** `selectedOnly=true` → symbols in Market Watch, `selectedOnly=false` → all available symbols from broker.

---

## 🎯 Purpose

Use this method when you need to:

* Count how many symbols are currently in Market Watch.
* Get the total number of available symbols from your broker.
* Iterate through symbols using `SymbolNameAsync(index)` (you need the total count first).
* Verify that specific symbols are loaded in Market Watch.

---

## 🧩 Notes & Tips

* Use `selectedOnly=true` to get symbols currently visible in Market Watch (actively used symbols).
* Use `selectedOnly=false` to get the total count of all symbols your broker offers.
* Combine with `SymbolNameAsync(index, selected)` to iterate through symbols.
* The count includes all symbol types: currencies, stocks, indices, commodities, etc.
* Use short per‑call timeout (3–5s) as this is a fast operation.

---

## 🔗 Usage Examples

### 1) Count symbols in Market Watch

```csharp
// Get number of symbols currently selected in Market Watch
var totalSelected = await acc.SymbolsTotalAsync(
    selectedOnly: true,
    deadline: DateTime.UtcNow.AddSeconds(3));
Console.WriteLine($"Symbols in Market Watch: {totalSelected.Total}");
```

### 2) Count all available symbols

```csharp
// Get total number of symbols available from broker
var totalAll = await acc.SymbolsTotalAsync(selectedOnly: false);
Console.WriteLine($"Total available symbols: {totalAll.Total}");
```

### 3) Compare selected vs available

```csharp
// Show selected vs available symbols
var selected = await acc.SymbolsTotalAsync(selectedOnly: true);
var all = await acc.SymbolsTotalAsync(selectedOnly: false);

Console.WriteLine($"Market Watch: {selected.Total} symbols");
Console.WriteLine($"Available:    {all.Total} symbols");
Console.WriteLine($"Not selected: {all.Total - selected.Total} symbols");
```

### 4) Iterate through all selected symbols

```csharp
// Get count and iterate through all symbols in Market Watch
var total = await acc.SymbolsTotalAsync(selectedOnly: true);

Console.WriteLine($"Symbols in Market Watch ({total.Total}):");
for (int i = 0; i < total.Total; i++)
{
    var symbolData = await acc.SymbolNameAsync(index: i, selected: true);
    Console.WriteLine($"  [{i}] {symbolData.Name}");
}
```

### 5) Verify Market Watch is populated

```csharp
// Check if Market Watch has symbols before trading
var selected = await acc.SymbolsTotalAsync(selectedOnly: true);

if (selected.Total == 0)
{
    Console.WriteLine("⚠️ Market Watch is empty! Add symbols first.");
    return;
}

Console.WriteLine($"✅ Market Watch has {selected.Total} symbols");
```
