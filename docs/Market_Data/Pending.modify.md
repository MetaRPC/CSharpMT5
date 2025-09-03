# Pending Modify (`pending.modify`) ✏️

## What it Does

Modifies an existing **pending order** (Limit / Stop / Stop‑Limit): entry/trigger/limit price, SL/TP, and TIF/expiration.

> A similar command: **`pending.move`** (`pmove') — shifts prices **by ±N points**. The exact setting of the values is shown here.

---
## Method Signature

```csharp
public Task<bool> ModifyPendingOrderAsync(
    ulong ticket,
    string? type,               // "buylimit"|"selllimit"|"buystop"|"sellstop"|"buystoplimit"|"sellstoplimit"|null
    double? price,              // for limit/stop
    double? stop,               // for stop/stop-limit (trigger)
    double? limit,              // for stop-limit (limit price)
    double? sl,
    double? tp,
    string? tif,                // "GTC"|"DAY"|"GTD"|null
    DateTimeOffset? expire,
    CancellationToken ct);
```

## Input Parameters ⬇️

| Parameter       | Type           | Required | Description                                                                 |           |         |          |              |                                              |
| --------------- | -------------- | -------- | --------------------------------------------------------------------------- | --------- | ------- | -------- | ------------ | -------------------------------------------- |
| `--profile, -p` | string         | yes      | Which profile to use (from `profiles.json`).                                |           |         |          |              |                                              |
| `--ticket, -t`  | ulong          | yes      | Pending order ticket.                                                       |           |         |          |              |                                              |
| `--type` | string | no | \`buylimit | selllimit | buystop | sellstop | buystoplimit | sellstoplimit\` (for validating invariants). |
| `--price`       | double | no | New **entry price** (for Limit/Stop).                                     |           |         |          |              |                                              |
| `--stop`        | double | no | New **trigger price** (for Stop/Stop‑Limit).                              |           |         |          |              |                                              |
| `--limit`       | double | no | New **limit price** (for Stop‑Limit).                                     |           |         |          |              |                                              |
| `--sl`          | double         | no       | New Stop Loss (absolute).                                                   |           |         |          |              |                                              |
| `--tp`          | double         | no       | New Take Profit (absolute).                                                 |           |         |          |              |                                              |
| `--tif`         | string         | no       | \`GTC                                                                       | DAY       | GTD\`.  |          |              |                                              |
| `--expire`      | DateTimeOffset | no | ISO‑8601, **is used only** for `--tif=GTD` (Specified/SpecifiedDay). |           |         |          |              |                                              |
| `--symbol, -s` | string | no | For best‑effort `ensure-visible` (optional).                          |           |         |          |              |                                              |
| `--timeout-ms`  | int            | no       | Per‑RPC timeout (default `30000`).                                          |           |         |          |              |                                              |
| `--dry-run`     | flag | no | Show changes, **without** sending a request.                                |           |         |          |              |                                              |

> Параметра `--output` **нет** — команда печатает текст.

---

## Rules & Validation ✅

* **Stop‑Limit**: requires **both** `--stop` and `--limit'. '--price` is not allowed for this type of **.

  * `buystoplimit`: `limit ≤ stop`
  * `sellstoplimit`: `limit ≥ stop`
* **Limit/Stop**: Requires `--price'.
* **TIF**: `GTC` / `DAY' / `GTD' (=`Specified`/`SpecifiedDay'); for `GTD`, you can/should set `--expire` (UTC/ISO‑8601).

---

## How to Use 🛠️

```powershell
# Change SL/TP only
dotnet run -- pending.modify -p demo -t 123456 --sl 1.0950 --tp 1.1050

# Change entry price (Buy Limit)
dotnet run -- pending.modify -p demo -t 123456 --type buylimit --price 1.1000

# Stop‑Limit: set trigger & limit (no --price)
dotnet run -- pending.modify -p demo -t 123456 --type buystoplimit --stop 1.1010 --limit 1.1005

# TIF=GTD with expiry
dotnet run -- pending.modify -p demo -t 123456 --tif GTD --expire "2025-09-01T12:00:00Z"

# Dry‑run preview
dotnet run -- pending.modify -p demo -t 123456 --price 1.1000 --dry-run
```

---

## Code Reference 🧩

```csharp
// (optional) ensure visibility
if (!string.IsNullOrWhiteSpace(symbol))
{
    using var visCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
    try
    {
        await _mt5Account.EnsureSymbolVisibleAsync(
            symbol,
            maxWait: TimeSpan.FromSeconds(3),
            cancellationToken: visCts.Token);
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        Console.WriteLine($"WARN: ensure-visible failed: {ex.Message}");
    }
}

// Build cancellation with command timeout
using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs ?? 30000));
var ct = cts.Token;

// Guard: at least one change must be provided
if (price is null && stop is null && limit is null && sl is null && tp is null && tif is null && expire is null)
    throw new ArgumentException("No changes specified. Provide at least one flag.");

// Apply changes
var ok = await _mt5Account.ModifyPendingOrderAsync(
    ticket: ticket,
    type: typeStr,   // optional; used only to validate invariants client-side
    price: price,
    stop: stop,
    limit: limit,
    sl: sl,
    tp: tp,
    tif: tifStr,     // "GTC"|"DAY"|"GTD" (SPECIFIED/SPECIFIED_DAY also accepted)
    expire: expire,
    ct: ct);

Console.WriteLine(ok ? "✓ pending.modify done" : "⚠ pending.modify returned false");

```
