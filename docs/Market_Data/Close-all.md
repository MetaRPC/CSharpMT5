# Close All (`close-all`) 🧹

## What it Does

Closes **all open positions** on the current MT5 account in one go. Optional filter by symbol. Safety confirmation required unless `--dry-run`.

---

## Input Parameters ⬇️

| Parameter             | Type   | Required | Description                                                            |
| --------------------- | ------ | -------- | ---------------------------------------------------------------------- |
| `--profile, -p`       | string | yes      | Which profile to use (from `profiles.json`).                           |
| `--filter-symbol, -s` | string | no       | Close only positions for this symbol (e.g., `EURUSD`).                 |
| `--deviation`         | int    | no       | Max slippage in points (default: `10`).                                |
| `--yes, -y`           | flag   | no       | Execute without interactive confirmation (otherwise preview & exit=2). |
| `--dry-run`           | flag   | no       | Print planned actions without sending requests.                        |
| `--timeout-ms`        | int    | no       | Per-RPC timeout in milliseconds (default: `30000`).                    |

---

## Output ⬆️

**Text only.**

* If there are no positions: `No positions to close.`
* Without `--yes` or with `--dry-run`: prints the plan (up to 10 lines like `#<ticket> vol=<lots>`) and

  * either `Pass --yes to execute.` (exit code = `2`),
* or simply terminates (in `--dry-run` mode).
* When executed: the final line is `✔ Closed: <ok>, ✖ Failed: <fail>`.

Exit codes:

* `0` — everything is closed successfully;
* `1` — part of the closures failed (warning log);
* `2` — the preview plan is shown without confirmation of `--yes'.

---

## How to Use 🛠️

```powershell
# Close everything
dotnet run -- close-all -p demo --yes

# Preview only (no requests)
dotnet run -- close-all -p demo --dry-run

# Only for EURUSD (with 15 points deviation)
dotnet run -- close-all -p demo -s EURUSD --deviation 15 --yes
```

---

## Code Reference 🧩 (без CallWithRetry)

```csharp
// Preconditions: connection already established; profile selected
var map = await _mt5Account.ListPositionVolumesAsync(filterSymbol: symbol, CancellationToken.None);

if (map.Count == 0)
{
    Console.WriteLine("No positions to close.");
}
else if (!yes || dryRun)
{
    Console.WriteLine($"Will close {map.Count}{(string.IsNullOrEmpty(symbol) ? "" : $" for {symbol}")} Deviation={deviation}");
    foreach (var (ticket, vol) in map.Take(10))
        Console.WriteLine($"  #{ticket} vol={vol}");
    if (map.Count > 10) Console.WriteLine($"  ... and {map.Count - 10} more");
    // In dry-run: stop here; otherwise require --yes
}
else
{
    int ok = 0, fail = 0;
    foreach (var (ticket, vol) in map)
    {
        try
        {
            await _mt5Account.ClosePositionFullAsync(ticket, vol, deviation, CancellationToken.None);
            ok++;
        }
        catch (Exception ex)
        {
            // Log warning; continue with next
            Console.WriteLine($"WARN: Close #{ticket} vol={vol} failed: {ex.Message}");
            fail++;
        }
    }
    Console.WriteLine($"\u2714 Closed: {ok}, \u2716 Failed: {fail}");
}
```

### Method Signatures

```csharp
public Task<Dictionary<ulong, double>> ListPositionVolumesAsync(
    string? symbol,
    CancellationToken ct);

public Task ClosePositionFullAsync(
    ulong ticket,
    double volume,
    int deviation,
    CancellationToken ct);
```
