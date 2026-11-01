# ✅ Full Symbol List (`SymbolListAsync`)

> **Request:** Get the complete list of symbol names available on the platform as one call on **MT5**.

**API Information:**

* **SDK wrapper:** `MT5Account.SymbolListAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `SymbolList` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `SymbolList(SymbolListRequest) → SymbolListReply`
* **Low‑level client (generated):** `MarketInfo.MarketInfoClient.SymbolList(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<SymbolListData> SymbolListAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`SymbolListRequest { }` — no parameters.


**Reply message:**

`SymbolListReply { data: SymbolListData }`

---

## 🔽 Input

| Parameter           | Type                | Description                                               |
| ------------------- | ------------------- | --------------------------------------------------------- |
| `deadline`          | `DateTime?`         | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `SymbolListData`

| Field     | Type                       | Description                        |
| --------- | -------------------------- | ---------------------------------- |
| `Symbols` | `repeated string`          | All available symbol names         |

---

## 💬 Just the essentials

* **What it is.** The full inventory of symbol **names** in one response.
* **Why you need it.** Discover what the broker offers without looping `SymbolName` by index.
* **Compare with.** [`SymbolsTotal`](./SymbolsTotal.md) + [`SymbolName`](./SymbolName.md) — count + per‑index; this returns everything at once.

---

## 🎯 Purpose

Use it to enumerate instruments:

* Populate a symbol picker / autocomplete.
* Filter by substring (e.g. all `*USD` pairs) client‑side.
* Validate configured symbols against what the broker actually provides.

---

## 🧩 Notes & Tips

* **Large lists:** Brokers can expose thousands of symbols — filter client‑side instead of selecting all.
* **Names only:** This returns names; use [`SymbolInfoTick`](./SymbolInfoTick.md) / `SymbolInfoDouble` for quotes/properties.
* **Market Watch:** Being listed here doesn’t mean selected — call [`SymbolSelect`](./SymbolSelect.md) before trading.

---

## 🔗 Usage Examples

### 1) List everything, show the first few

```csharp
// acc — connected MT5Account
var list = await acc.SymbolListAsync();
Console.WriteLine($"Symbols available: {list.Symbols.Count}");
Console.WriteLine(string.Join(", ", list.Symbols.Take(10)));
```

---

### 2) Filter to USD pairs

```csharp
var list = await acc.SymbolListAsync();
var usd  = list.Symbols.Where(s => s.Contains("USD")).ToList();
Console.WriteLine($"{usd.Count} USD instruments");
```
