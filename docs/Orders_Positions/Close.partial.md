# Close Partial (`close.partial`) 🪓

## What it Does 🎯

Closes a **specific volume** of a position by ticket.
Unlike `close.half` or `close.percent`, this command lets you choose the **exact number of lots** to close.

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                    |
| ----------------- | ------ | -------- | ---------------------------------------------- |
| `--profile`, `-p` | string | ✅        | Profile from `profiles.json`.                  |
| `--ticket`, `-t`  | ulong  | ✅        | Position ticket to partially close.            |
| `--volume`, `-v`  | double | ✅        | Exact volume to close (in lots).               |
| `--deviation`     | int    | ❌        | Max slippage (points). Default: `10`.          |
| `--output`, `-o`  | string | ❌        | `text` (default) or `json`.                    |
| `--timeout-ms`    | int    | ❌        | RPC timeout in ms (default: 30000).            |
| `--dry-run`       | flag   | ❌        | Print intended action without sending request. |

---

## Output Fields ⬆️

| Field       | Type   | Description                   |
| ----------- | ------ | ----------------------------- |
| `Ticket`    | ulong  | Original position ticket.     |
| `Closed`    | double | Volume closed (requested).    |
| `Remaining` | double | Volume still open.            |
| `Price`     | double | Execution price of the close. |
| `Status`    | string | `OK` or error description.    |

---

## How to Use 🛠️

### CLI

```powershell
# Close exactly 0.03 lots of position 123456
dotnet run -- close.partial -p demo -t 123456 -v 0.03

# With custom slippage
dotnet run -- close.partial -p demo -t 123456 -v 0.01 --deviation 20

# JSON + dry-run
dotnet run -- close.partial -p demo -t 123456 -v 0.05 --dry-run -o json
```

### PowerShell Shortcuts (from `shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
cp -t 123456 -v 0.03
# expands to: mt5 close.partial -p demo -t 123456 -v 0.03 --deviation 10 --timeout-ms 90000
```

---

## When to Use ❓

* To close an exact lot size rather than a fraction of the whole.
* Useful for scaling out at predefined levels (e.g., close 0.1 at +10 pips, 0.2 at +20 pips).
* Can combine with scripts or algos that calculate optimal partial close size.

---

## Notes & Safety 🛡️

* Ensure the requested volume respects **symbol min/step/max** — use `symbol limits` to verify.
* If requested volume > position size, broker rejects the request.
* Residual must still be ≥ MinLot, otherwise the whole position may close.

---

## Code Reference 🧩

```csharp
var cpVolumeOpt = new Option<double>(new[] { "--volume", "-v" }, "Volume (lots) to close")
{
    IsRequired = true
};

var closePartial = new Command("close.partial", "Partially close a position by ticket");
closePartial.AddAlias("cp");

closePartial.AddOption(profileOpt);
closePartial.AddOption(cpTicketOpt);
closePartial.AddOption(cpVolumeOpt);
closePartial.AddOption(devOpt);      // reuse: deviation in points
closePartial.AddOption(timeoutOpt);
closePartial.AddOption(dryRunOpt);

closePartial.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(cpTicketOpt);
    var volume    = ctx.ParseResult.GetValueForOption(cpVolumeOpt);
    var deviation = ctx.ParseResult.GetValueForOption(devOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (ticket == 0) throw new ArgumentOutOfRangeException(nameof(ticket), "Ticket must be > 0.");
    if (volume <= 0) throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be > 0.");

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CLOSE.PARTIAL Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} Vol:{Vol} Dev:{Dev}", ticket, volume, deviation))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] CLOSE.PARTIAL ticket={ticket} volume={volume} deviation={deviation}");
            return;
        }

        try
        {
            await ConnectAsync();

            using var opCts = StartOpCts();
            await CallWithRetry(
                ct => _mt5Account.ClosePositionPartialAsync(ticket, volume, deviation, ct),
                opCts.Token);

            Console.WriteLine("✔ close.partial done");
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
});

root.AddCommand(closePartial);
```
