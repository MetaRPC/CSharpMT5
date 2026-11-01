# ✅ Symbol Names as a List (`SymbolNamesAsync`)

> **Sugar method:** Returns the full list of symbol names as a plain `IReadOnlyList<string>` (unwraps the protobuf container).

**API Information:**

* **Extension method:** `MT5Service.SymbolNamesAsync(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Underlying calls:** `SymbolListAsync()`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public static Task<IReadOnlyList<string>> SymbolNamesAsync(
        this MT5Service svc,
        int timeoutSec = 20, CancellationToken ct = default);
}
```

---

## 🔽 Input

| Parameter    | Type                | Description                             |
| ------------ | ------------------- | --------------------------------------- |
| `svc`        | `MT5Service`        | MT5Service instance (extension method)  |
| `timeoutSec` | `int`               | Timeout in seconds (default: 20)        |
| `ct`         | `CancellationToken` | Cancellation token                      |

---

## ⬆️ Output — `IReadOnlyList<string>`

All available symbol names as a plain list (unwrapped from `SymbolListData.Symbols`).

---

## 💬 Just the essentials

* **What it is.** Every symbol name the broker exposes, as a `List<string>`.
* **Why you need it.** Discover instruments; filter client‑side; no index loop.
* **Difference from low‑level.** Returns a plain list instead of the `SymbolListData` container.

---

## 🎯 Purpose

* Populate a symbol picker / autocomplete.
* Filter by substring (e.g. all `*USD` pairs).
* Validate configured symbols against what the broker provides.

---

## 🧩 Notes & Tips

* **Large lists** — brokers can expose thousands of symbols; filter instead of selecting all.
* **Names only** — use quotes/properties calls for prices.
* **Market Watch** — being listed ≠ selected; call `EnsureSelected` before trading.

---

## 🔧 Under the Hood

```csharp
var data = await svc.SymbolListAsync(Dl(timeoutSec), ct);
return data.Symbols.ToList();
```

---

## 🔗 Usage Examples

### 1) Count and preview

```csharp
// svc — MT5Service instance
var names = await svc.SymbolNamesAsync();
Console.WriteLine($"{names.Count} symbols. First 10: {string.Join(", ", names.Take(10))}");
```

### 2) Filter to USD pairs

```csharp
var usd = (await svc.SymbolNamesAsync()).Where(s => s.Contains("USD")).ToList();
Console.WriteLine($"{usd.Count} USD instruments");
```

---

## 🔗 Related Methods

* **Low-level:** [`SymbolListAsync()`](../../MT5Account/2.%20Symbol_information/SymbolList.md)
* **Sugar:** `PriceBarsLast()` (OHLC bars), `GetSymbolSnapshot()` (one symbol’s tick/precision)
