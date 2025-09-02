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

// find order & symbol
var opened = await _mt5Account.OpenedOrdersAsync();
var ordObj = TryFindByTicketInAggregate(opened, ticket, out var bucket);
// compute delta = byPoints × point
var point = _mt5Account.PointGuess(symbol);
var delta = byPoints * point;

if (!dryRun)
    await _mt5Account.MovePendingByPointsAsync(ticket, symbol, byPoints, CancellationToken.None);

Console.WriteLine("✔ pending.move done");
```
