# Timeouts & Retries Policy ⏱️🔁

This page explains how CSharpMT5 CLI bounds operations with **timeouts** and stabilizes calls with **retries**. Use it to tune reliability for local use and CI.

---

## TL;DR

* `--timeout-ms` sets a **per‑RPC** time budget (default **30000 ms**).
* Every high‑level command is wrapped by **operation scopes** to prevent hangs.
* `CallWithRetry(...)` re‑invokes transiently failing RPCs with a short **exponential backoff + jitter**.
* Prefer **shorter timeouts** in CI (fast fail) and **longer** for slow terminals.

---

## Control knobs

| Knob               | Scope                           | Default                  | Where                         |
| ------------------ | ------------------------------- | ------------------------ | ----------------------------- |
| `--timeout-ms`     | **Per RPC** (single gRPC call)  | `30000`                  | CLI option on most commands   |
| `UseOpTimeout(ms)` | **Whole command** (outer scope) | inherits                 | `Program.cs` wrapper          |
| `StartOpCts()`     | **Sub‑step** time budget        | derived from outer scope | used before each awaited call |

**Rule of thumb:** outer scope ≥ inner per‑RPC timeouts. Avoid nested timeouts with conflicting budgets.

---

## Execution model (typical command)

1. Parse CLI → read `--timeout-ms`.
2. `using (UseOpTimeout(timeoutMs))` — creates an **outer CancellationTokenSource** for the command.
3. `await ConnectAsync()` — connection itself is bounded by the outer CTS.
4. Before *each* RPC: `using var opCts = StartOpCts();`
5. Call via `await CallWithRetry(fn, opCts.Token)` — transient errors are retried.
6. Single **print point** (either JSON or text) → exit.

---

## What is retried

`CallWithRetry` targets **transient** conditions, for example:

* `StatusCode.Unavailable` (server temporarily down, reconnect in progress)
* `TERMINAL_INSTANCE_NOT_FOUND` (terminal restarted; wrapper reconnects)
* Network hiccups (channel reset)

Backoff policy (conceptually):

```
base = 200–500 ms
next = base * 2^attempt + small jitter (±30%)
max attempts = small fixed number (e.g., 3–5)
cap single sleep ≈ 2–3 s
```

**Not** retried:

* Validation / argument errors (e.g., invalid SL/TP constraints)
* Server‑rejected business rules (e.g., market closed, no money)
* Deterministic protobuf/serialization errors

---

## Choosing a timeout

| Scenario                    | Suggested `--timeout-ms` | Why                               |
| --------------------------- | ------------------------ | --------------------------------- |
| Local development           | `30000–60000`            | Human‑paced, network can wobble   |
| CI fast checks (quote/info) | `3000–10000`             | Fail fast to keep pipeline crisp  |
| Heavy accounts / history    | `60000–120000`           | Larger payloads, slower terminals |
| Streaming (`stream`)        | `30000` (handshake)      | Then long‑lived read loop         |

**Tip:** If you see frequent `DeadlineExceeded`, first verify connectivity and server load; throwing more timeout sometimes only hides real issues.

---

## Patterns & anti‑patterns

✅ **Do**

* Use a **single** print point to avoid duplicated output on retries.
* Keep **idempotency** in mind: a retried read is safe; a retried write should be guarded (we rely on server‑side ticketing semantics).
* Log scopes: `BeginScope("Cmd:... Symbol:{Symbol}", ...)` for traceability.

🚫 **Don’t**

* Don’t do *nested unrelated CTS* with smaller deadlines than the outer one.
* Don’t swallow `OperationCanceledException` silently—print a clear timeout line.

---

## Sample skeleton

```csharp
using (UseOpTimeout(timeoutMs))
using (_logger.BeginScope("Cmd:BUY Profile:{Profile}", profile))
{
    await ConnectAsync();

    using var opCts = StartOpCts();
    var ticket = await CallWithRetry(
        ct => _mt5Account.SendMarketOrderAsync(
            symbol: s, isBuy: true, volume: v, deviation: dev,
            stopLoss: sl, takeProfit: tp, deadline: null, cancellationToken: ct),
        opCts.Token);

    _logger.LogInformation("BUY done: ticket={Ticket}", ticket);
}
```

---

## Debugging timeouts

1. Re‑run with `--trace` to see where it stops (connect vs RPC).
2. Test basic connectivity:

   ```powershell
   dotnet run -- health -p <profile>
   dotnet run -- quote -p <profile> -s EURUSD
   ```
3. Try a larger `--timeout-ms` and watch logs for `Unavailable`.
4. If only **writes** fail (buy/sell/modify) → check symbol visibility (`symbol ensure`) and trading session status.

---

## FAQ

**Q: Why does my `info` print twice sometimes?**
A: Ensure printing happens **outside** `CallWithRetry`; log/return values *inside* the retry, but write to console once.

**Q: Should I retry on `DeadlineExceeded`?**
A: Usually no—first increase `--timeout-ms` or reduce payload. Retrying a strictly time‑bounded call rarely helps.

**Q: How many retries are used?**
A: A small, fixed number to avoid long stalls (keep user experience snappy). Check code for exact constant if you need to tweak.

---

## CI recommendations

* Use `--output json` for machine parsing.
* Keep `--timeout-ms` small (`5–10 s`) for smoke tests.
* Separate **connectivity** step (`health`) from functional steps.
* Alert on repeated `Unavailable` spikes—indicates terminal or network instability.

---

## Related

* Getting Started → `Getting_Started.md`
* Logging & Output → `CLI_Logging_Output.md` (optional page)
* Shortcasts → `cli_shortcasts_examples.md`
