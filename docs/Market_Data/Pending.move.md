# Pending Move (`pending.move`) ↔️

## What it Does

Moves a **pending order** price by **±N points** (relative shift). Works for pending orders (Limit/Stop/Stop‑Limit). Not applicable to already filled positions.

Alias: `pmove`.

---
## Method Signatures

```csharp
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public Task MovePendingByPointsAsync(
    ulong ticket,
    string symbol,
    int byPoints,
    CancellationToken ct);
```

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                          |
| ----------------- | ------ | -------- | ---------------------------------------------------- |
| `--profile, -p`   | string | yes      | Which profile to use (from `profiles.json`).         |
| `--ticket, -t`    | ulong  | yes      | Pending order ticket to move.                        |
| `--by-points, -P` | int    | yes      | **Signed** shift in points (e.g., `+15`, `-20`).     |
| `--timeout-ms`    | int    | no       | Per‑RPC timeout in milliseconds (default: `30000`).  |
| `--dry-run`       | flag   | no       | Print intended change **without** sending a request. |

> There are no parameters `--price`, `--sl`, `--tp`, `--expiration`, `--output` — ***. To accurately edit prices/SL/TP, use `pending.modify'.

---

## Output ⬆️ (text)

* If the ticket is not found: `Pending order #<ticket> not found.` (exit code `2`).
* Dry‑run: Pre-calculation of old/new prices (Price/PriceTriggerStopLimit, if available).
* Execution: `✔ pending.move done' (exit code `0`; errors are printed as warnings, critical errors are `1`).

---

## How to Use 🛠️

```powershell
# Move by +15 points
dotnet run -- pending.move -p demo -t 123456 -P +15

# Dry‑run (no RPC)
dotnet run -- pending.move -p demo -t 123456 -P -25 --dry-run
```

---

## Code Reference 🧩

```csharp
await ConnectAsync();

// Build cancellation with timeout (default 30s)
using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs ?? 30000));
var ct = cts.Token;

// 1) Find the pending order by ticket
var opened = await _mt5Account.OpenedOrdersAsync(cancellationToken: ct);

// Your helper should ensure it's a PENDING order and return its symbol.
// Example signature: TryFindPending(opened, ticket, out var pending)
var pending = TryFindPending(opened, ticket, out var sym);
if (pending is null)
{
    Console.WriteLine($"Pending order #{ticket} not found.");
    // return 2;
}

// 2) Compute point & delta for preview
// Prefer reliable point from SymbolInfo rather than a local guess
var symInfo = await _mt5Account.SymbolInfoAsync(sym, cancellationToken: ct);
var point = symInfo.Point;
var delta = byPoints * point;

if (dryRun)
{
    // If your model exposes current prices, print old→new
    // For Limit/Stop orders:
    //   var oldPrice = pending.Price;
    //   var newPrice = MT5Account.NormalizePrice(oldPrice + delta, symInfo.Digits);
    // For Stop-Limit orders (if fields exist):
    //   var oldStop  = pending.StopLimit;     // or Trigger
    //   var oldLimit = pending.Price;         // typical layout
    //   var newStop  = MT5Account.NormalizePrice(oldStop  + delta, symInfo.Digits);
    //   var newLimit = MT5Account.NormalizePrice(oldLimit + delta, symInfo.Digits);

    Console.WriteLine($"[DRY-RUN] MOVE #{ticket} {sym} by {byPoints} pt (Δ={delta})");
    // Console.WriteLine($"    price: {oldPrice} -> {newPrice}");
    // Console.WriteLine($"    stop/limit: {oldStop}->{newStop} / {oldLimit}->{newLimit}");
    // return 0;
}
else
{
    // 3) Execute
    await _mt5Account.MovePendingByPointsAsync(ticket, sym, byPoints, ct);
    Console.WriteLine("✔ pending.move done");
    // return 0;
}

```
