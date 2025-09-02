# Ensure Symbol Visible (helper) 👁️

## What it Does 🎯

Best‑effort makes sure a **symbol is visible** in MT5 (Market Watch) **before** quotes/orders. If the symbol is hidden, enables it and waits up to a short timeout.

> This is **not** a CLI command. It’s a helper used inside commands like `quote`, `buy`, `sell`.

---

## Where It’s Used 🔗

Called right before market operations:

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

Typical callers:

* `quote` — to ensure first snapshot tick arrives;
* `buy` / `sell` — to avoid “symbol not selected/unknown”.

---

## Method Signature 🧩

```csharp
public Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    TimeSpan? pollInterval = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

**Params**

* `symbol` — e.g., `EURUSD`.
* `maxWait` — total wait time (default: `null` → internal default).
* `pollInterval` — how often to re-check visibility (optional).
* `deadline` — optional absolute RPC deadline.
* `cancellationToken` — cooperative cancel.

> If already visible, resolves immediately. On errors: throws; typical handlers log **warning** and proceed when possible.

---

## Why It Matters ❗

In MT5 a symbol must be visible to:

* receive quotes (snapshot/stream),
* place/modify orders without “symbol not selected”.

---

## Good Practices ✅

* Keep `maxWait` short (2–5s): it’s a pre‑flight check.
* Wrap in `try/catch` that **excludes** `OperationCanceledException`.
* Call it **before** `SymbolInfoTickAsync` / order placement.

---

## Minimal Usage 🧷

```csharp
await _mt5Account.EnsureSymbolVisibleAsync(symbol, maxWait: TimeSpan.FromSeconds(3));
```

📌 In short: a small guard that makes quotes/orders more reliable by ensuring the symbol is present in Market Watch first.
