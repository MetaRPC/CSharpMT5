## Market Info

### Getting the Total Number of Symbols

> **Request:** fetch the count of symbols (either all available or only those selected in Market Watch) from MT5.

### Code Example

```csharp
// Total number of all symbols
var total = await _mt5Account.SymbolsTotalAsync(false);

// Total number of symbols selected in Market Watch
var selectedTotal = await _mt5Account.SymbolsTotalAsync(true);
```

✨ **Method Signature:**

```csharp
Task<SymbolsTotalData> SymbolsTotalAsync(
    bool selectedOnly,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
);
```

---

## Input

* **`selectedOnly`** (`bool`) — filter flag:

  * `false` — return the total count of **all** available symbols.
  * `true` — return the total count of symbols **selected** in Market Watch.
* **`deadline`** (`DateTime?`, optional) — UTC timestamp by which the request must complete; if `null`, default timeout applies.
* **`cancellationToken`** (`CancellationToken`, optional) — token to cancel the request.

---

## Output

**`SymbolsTotalData`** — structure with:

* **`Total`** (`int`) — number of symbols matching the `selectedOnly` filter.

---

## Purpose

Use one method to get either “all-symbol” or “selected-symbol” counts simply by toggling a boolean—keeping your code DRY and future-ready. 🚀
