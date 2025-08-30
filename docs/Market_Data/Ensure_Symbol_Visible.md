# Ensure Symbol Visible (helper) 👁️

## What it Does 🎯

Guarantees that a **symbol is visible (selected)** in the MT5 terminal (Market Watch) **before** you request quotes or send orders.
If the symbol is hidden, the helper tries to enable it and waits up to a given timeout.

> This is **not** a CLI command. It is a helper used inside commands like `quote`, `buy`, and `sell`.

---

## Where It’s Used 🔗

You’ll see it in handlers before any market operation:

```csharp
try
{
    using var visCts = StartOpCts();
    await _mt5Account.EnsureSymbolVisibleAsync(
        s, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
}
```

Commands using it:

* `quote` — to ensure the first snapshot tick can arrive.
* `buy` / `sell` — to ensure the trading request will not fail with “symbol not selected/unknown”.

---

## Signature 🧩 (from project usage)

```csharp
Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan maxWait,
    CancellationToken cancellationToken)
```

* **`symbol`** — instrument name, e.g., `EURUSD`.
* **`maxWait`** — how long to wait for the symbol to become visible.
* **`cancellationToken`** — operation cancellation control.

> The helper completes silently if the symbol is already visible. Otherwise it enables the symbol and waits up to `maxWait`. On errors it throws, which we log as a warning and continue when possible.

---

## Why It Matters ❗

In MT5 the symbol must be **visible** to:

* receive quotes (`FirstTickAsync`, streaming),
* place/modify orders without “symbol not selected” errors.

By calling this helper, we proactively prevent these pitfalls.

---

## Typical Flow 🧭

1. Validate profile & params.
2. Connect to MT5.
3. **EnsureSymbolVisibleAsync(symbol, maxWait, ct)**.
4. Proceed: request quote / send order.
5. Handle errors or print output once.

---

## Failure Modes & Handling 🛡️

* **Timeout** (symbol didn’t appear in time): throws → we log a **warning** and may still proceed if optional.
* **Terminal not ready / disconnected**: upstream exceptions (caught by `CallWithRetry` / outer `try/catch`).
* **Cancellation**: `OperationCanceledException` passes through; do not log as error.

---

## Good Practices ✅

* Keep `maxWait` **short** (2–5s). It’s a pre-flight check, not a long wait.
* Always call it **before** `FirstTickAsync` and **before** order placement.
* Wrap in a `try/catch` that **excludes** `OperationCanceledException` and logs a **warning**, not an error (as in the snippet above).

---

## Code References (from your handlers) 🧷

### In `quote`

```csharp
// best-effort: ensure visibility
try
{
    using var visCts = StartOpCts();
    await _mt5Account.EnsureSymbolVisibleAsync(
        s, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
}
```

### In `buy` / `sell`

```csharp
try
{
    using var visCts = StartOpCts();
    await _mt5Account.EnsureSymbolVisibleAsync(
        s, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
}
```

---

📌 In short:
**Ensure Symbol Visible** is a small but essential guard that makes quote retrieval and order placement more reliable by ensuring the symbol is present in Market Watch before any action.
