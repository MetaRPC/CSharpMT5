# Close All Pending Orders (`CloseAllPending`)

> **Alias method:** Direct alias for `CancelAll()`. Cancels all pending orders with optional filtering.

**API Information:**

* **Extension method:** `MT5Service.CloseAllPending(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Region:** [11] BULK CONVENIENCE
* **Implementation:** `=> svc.CancelAll(symbol, isBuy, timeoutSec, ct);`

---

## Method Signature

```csharp
public static Task<int> CloseAllPending(
    this MT5Service svc,
    string? symbol = null,
    bool? isBuy = null,
    int timeoutSec = 30,
    CancellationToken ct = default)
    => svc.CancelAll(symbol, isBuy, timeoutSec, ct);
```

---

## 💬 Just the essentials

* **What it is:** Alias for `CancelAll()` - exact same functionality, just a different method name.
* **Why it exists:** Some developers prefer "close" terminology for pending orders instead of "cancel".
* **Which to use:** Personal preference! Both do exactly the same thing.

---

## 🎯 Purpose

Use it for:

* Same purposes as `CancelAll()` - see [CancelAll.md](CancelAll.md) for full documentation
* When you prefer "close" terminology over "cancel"

---

## 🔗 Full Documentation

This method is a **direct alias** for `CancelAll()`. For complete documentation, including:

* Detailed examples
* Common pitfalls
* Error handling
* Usage scenarios

**See:** [CancelAll.md](CancelAll.md)

---

## 📋 Quick Reference

```csharp
// These are IDENTICAL:
int result1 = await svc.CancelAll(symbol: "EURUSD");
int result2 = await svc.CloseAllPending(symbol: "EURUSD");
// Both cancel all pending orders for EURUSD

// Internally, CloseAllPending just calls CancelAll:
public static Task<int> CloseAllPending(...)
    => svc.CancelAll(symbol, isBuy, timeoutSec, ct);
```

---

## 🔗 Related Methods

**🔄 Aliases:**

* `CancelAll()` - Primary method (this is an alias for it)

**🍬 Related Sugar methods:**

* `CloseAllPositions()` - Closes MARKET positions (different!)
* `CloseAll()` - From Region 06, closes market positions

---

## ⚠️ Common Pitfalls

1. **Thinking it's different from CancelAll:**
   ```csharp
   // ❌ WRONG: Thinking these are different
   await svc.CancelAll();       // Cancels pending orders
   await svc.CloseAllPending(); // ← Does EXACTLY the same thing!

   // ✅ CORRECT: They are aliases
   // Use whichever name you prefer
   ```

2. **Confusing with CloseAllPositions:**
   ```csharp
   // ❌ CONFUSION: Different targets!
   await svc.CloseAllPending();    // Cancels PENDING orders
   await svc.CloseAllPositions();  // Closes MARKET positions

   // These affect different order types!
   ```

---

## 💡 Summary

**CloseAllPending** is a simple alias for `CancelAll()`:

* ✅ Exact same functionality as `CancelAll()`
* ✅ Just a naming preference
* ✅ Use whichever name makes more sense to you

```csharp
// Both are identical:
await svc.CancelAll(symbol: "EURUSD");      // Option 1
await svc.CloseAllPending(symbol: "EURUSD"); // Option 2
```

**For full documentation, see [CancelAll.md](CancelAll.md)** 📖
