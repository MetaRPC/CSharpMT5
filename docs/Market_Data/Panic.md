# Panic (`panic`) 🚨

## What it Does

Runs an **emergency flatten**: closes **all open positions** and cancels **all pending orders** (optionally filtered by symbol). Designed for "oh‑no" moments.

---
## Method Signatures

```csharp
public Task<Dictionary<ulong, double>> ListPositionVolumesAsync(
    string? symbol,
    CancellationToken ct);

public Task<(int ok, int fail)> ClosePositionsAsync(
    IEnumerable<(ulong Ticket, string Symbol, double Volume)> batch,
    CancellationToken ct); // helper in Program (not part of MT5Account)

public Task ClosePositionFullAsync(
    ulong ticket,
    double volume,
    int deviation,
    CancellationToken ct);

public Task<IReadOnlyList<ulong>> ListPendingTicketsAsync(
    string? symbol,
    CancellationToken ct);

public Task CancelPendingOrderAsync(
    ulong ticket,
    CancellationToken ct);
```
## Input Parameters ⬇️

| Parameter       | Type   | Required | Description                                          |
| --------------- | ------ | -------- | ---------------------------------------------------- |
| `--profile, -p` | string | yes      | Which profile to use (from `profiles.json`).         |
| `--symbol, -s`  | string | no       | Limit scope to one symbol (e.g., `EURUSD`).          |
| `--deviation`   | int    | no       | Max slippage (points) for closes (default: `10`).    |
| `--timeout-ms`  | int    | no       | Per‑RPC timeout in milliseconds (default: `30000`).  |
| `--dry-run`     | flag   | no       | Print intended actions but **do not** send requests. |

> Note: **Нет** параметра `--output` — команда печатает текст.

---

## Output ⬆️

**Text only.**

* Target summary: `PANIC targets: positions=<N>, pendings=<M>`
* `--dry-run` preview:

  * `[DRY-RUN] CLOSE ticket=<id> vol=<lots>`
  * `[DRY-RUN] CANCEL ticket=<id>`
* Execution result: `✔ panic done`

Exit codes:

* `0` — выполнено (отдельные ошибки логируются предупреждениями);
* `1` — критическая ошибка (распечатана через ErrorPrinter).

---

## How to Use 🛠️

```powershell
# Emergency flatten
dotnet run -- panic -p demo

# Limit to EURUSD with tighter slippage
dotnet run -- panic -p demo -s EURUSD --deviation 5

# Dry-run preview
dotnet run -- panic -p demo --dry-run
```

---

## Code Reference 🧩

```csharp
await ConnectAsync();

// Gather scope
var pos  = await _mt5Account.ListPositionVolumesAsync(symbol: symbol, CancellationToken.None);
var pend = await _mt5Account.ListPendingTicketsAsync(symbol: symbol, CancellationToken.None);
Console.WriteLine($"PANIC targets: positions={pos.Count}, pendings={pend.Count}");

if (dryRun)
{
    foreach (var kv in pos)  Console.WriteLine($"[DRY-RUN] CLOSE ticket={kv.Key} vol={kv.Value}");
    foreach (var t in pend)  Console.WriteLine($"[DRY-RUN] CANCEL ticket={t}");
    return;
}

// Close positions first (free margin), then cancel pendings
foreach (var (ticket, vol) in pos)
    await _mt5Account.ClosePositionFullAsync(ticket, vol, deviation: deviation, CancellationToken.None);

foreach (var t in pend)
    await _mt5Account.CancelPendingOrderAsync(t, CancellationToken.None);

Console.WriteLine("✔ panic done");
```


