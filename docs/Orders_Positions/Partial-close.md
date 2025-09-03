# Partial Close (`partial-close`) ✂️

Closes **part** of an open position by ticket. You can specify either:

* an **exact volume** in lots, or
* a **percentage** of the current position volume.

> Convenience command. For dedicated variants see **[`close.partial`](./Close.partial.md)** (exact lots) and **[`close.percent`](./Close.percent.md)** (by %).

---

## Input Parameters

| Parameter         | Type    | Required | Description                                        |
| ----------------- | ------- | -------- | -------------------------------------------------- |
| `--profile`, `-p` | string  | yes      | Profile from `profiles.json`.                      |
| `--ticket`, `-t`  | ulong   | yes      | Position ticket.                                   |
| `--percent`, `-P` | int?    | no       | Percent of current volume to close (1–100).        |
| `--volume`, `-v`  | double? | no       | Exact volume (lots) to close.                      |
| `--timeout-ms`    | int     | no       | RPC timeout in ms (default: `30000`).              |
| `--dry-run`       | flag    | no       | Print intended action without sending the request. |

**Rule:** exactly **one** of `--percent` **or** `--volume` must be provided.

---

## Output (text) & Exit Codes

Examples:

```
[DRY-RUN] PARTIAL-CLOSE #123456 pct=50 → vol=0.12
✔ partial-close done: ticket=123456 closed=0.12
```

Errors:

```
Provide exactly one of --percent or --volume.  (exit code 2)
Position #123456 not found.                    (exit code 2)
Computed close volume below MinLot after step. (exit code 2)
RPC error: <broker message>                    (exit code 1)
```

**Exit codes**

* `0` — success
* `2` — validation/guard failures
* `1` — fatal error (printed via ErrorPrinter)

---

## How to Use

```powershell
# Close 50% of position
dotnet run -- partial-close -p demo -t 123456 -P 50

# Close exactly 0.02 lots
dotnet run -- partial-close -p demo -t 123456 -v 0.02

# Dry-run
dotnet run -- partial-close -p demo -t 123456 -P 25 --dry-run
```

### PowerShell shortcut (optional)

```powershell
function pc { param([ulong]$t,[int]$P,[double]$v,[string]$p=$PF,[int]$to=$TO)
  if ($PSBoundParameters.ContainsKey('P')) { mt5 'partial-close' -p $p -t $t -P $P --timeout-ms $to }
  elseif ($PSBoundParameters.ContainsKey('v')) { mt5 'partial-close' -p $p -t $t -v $v --timeout-ms $to }
}
```

---

## Notes & Safety 🛡️

* If `--volume` exceeds current position size, it is clamped to the maximum available.
* Percent path uses `currentVol * pct/100`. The result is **rounded down to lot step** and clamped to `[MinLot; currentVol]`.
* Some brokers require the symbol to be visible; the implementation best‑effort ensures visibility based on the ticket’s symbol.
* Works on **positions** only. For pendings use **`cancel`**.
* Use **[symbol limits](../Market_Data/Limits.md)** to check `min/step/max` for your symbol.

---

## Method Signatures (quick ref)

```csharp
// Resolve ticket → symbol & current volume
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// Lot constraints
public Task<(double min, double step, double max)> GetVolumeConstraintsAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// Ensure symbol visibility (best‑effort)
public Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    TimeSpan? pollInterval = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// Partial close (deviation is fixed internally for this wrapper)
public Task ClosePositionPartialAsync(
    ulong ticket,
    double volume,
    int deviation,
    CancellationToken cancellationToken);
```

---

## Code Reference (aligned wrapper)

```csharp
var pcTicketOpt  = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket to partially close") { IsRequired = true };
var pcPercentOpt = new Option<int?>(new[] { "--percent", "-P" }, "Percent of current volume to close (1..100)");
var pcVolumeOpt  = new Option<double?>(new[] { "--volume", "-v" }, "Exact volume to close (lots)");

var pclose = new Command("partial-close", "Partially close a position by ticket");
pclose.AddAlias("pc");

pclose.AddOption(profileOpt);
pclose.AddOption(pcTicketOpt);
pclose.AddOption(pcPercentOpt);
pclose.AddOption(pcVolumeOpt);
pclose.AddOption(timeoutOpt);
pclose.AddOption(dryRunOpt);

pclose.SetHandler(async (string profile, ulong ticket, int? percent, double? volume, int timeoutMs, bool dryRun) =>
{
    Validators.EnsureProfile(profile);
    if ((percent is null && volume is null) || (percent is not null && volume is not null))
    {
        Console.WriteLine("Provide exactly one of --percent or --volume.");
        Environment.ExitCode = 2; return;
    }
    if (percent is not null && (percent <= 0 || percent > 100))
        throw new ArgumentOutOfRangeException(nameof(percent), "Percent must be in (0;100].");
    if (volume is not null && volume <= 0)
        throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be > 0.");

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:PARTIAL-CLOSE Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} Pct:{Pct} Vol:{Vol}", ticket, percent, volume))
    {
        if (dryRun)
        {
            Console.WriteLine(percent is not null
                ? $"[DRY-RUN] PARTIAL-CLOSE #{ticket} pct={percent}"
                : $"[DRY-RUN] PARTIAL-CLOSE #{ticket} vol={volume}");
            return;
        }

        try
        {
            await ConnectAsync();

            // 1) resolve symbol & current volume
            var opened = await _mt5Account.OpenedOrdersAsync();
            var pos = opened.PositionInfos.FirstOrDefault(p => (ulong)p.Ticket == ticket || unchecked((ulong)p.Ticket) == ticket);
            if (pos is null) { Console.WriteLine($"Position #{ticket} not found."); Environment.ExitCode = 2; return; }

            var symbol = pos.Symbol;
            var currentVol = pos.Volume;

            // 2) compute requested
            double req = volume ?? currentVol * (percent!.Value / 100.0);

            // 3) normalize by symbol constraints
            var (min, step, max) = await _mt5Account.GetVolumeConstraintsAsync(symbol);
            double eff = Math.Max(min, Math.Min(currentVol, Math.Floor(req / step + 1e-9) * step));
            if (eff < min || eff <= 0) { Console.WriteLine("Computed close volume below MinLot after step."); Environment.ExitCode = 2; return; }

            // 4) best‑effort visibility
            try { await _mt5Account.EnsureSymbolVisibleAsync(symbol, TimeSpan.FromSeconds(3)); } catch { }

            // 5) execute (fixed deviation = 10 for this wrapper)
            await _mt5Account.ClosePositionPartialAsync(ticket, eff, deviation: 10, CancellationToken.None);
            Console.WriteLine($"\u2714 partial-close done: ticket={ticket} closed={eff}");
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { }
        }
    }
}, profileOpt, pcTicketOpt, pcPercentOpt, pcVolumeOpt, timeoutOpt, dryRunOpt);
```

---

## See also

* **[`close.partial`](./Close.partial.md)** — close an exact lot amount
* **[`close.percent`](./Close.percent.md)** — close a percentage of current volume
* **[`symbol limits`](../Market_Data/Limits.md)** — min/step/max lot constraints
