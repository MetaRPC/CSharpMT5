# Position Modify (`position.modify`) ✏️

## What it Does 🎯

Updates **Stop Loss** and/or **Take Profit** for an **open position** by ticket.

Alias: `posmod`

---

## Input Parameters ⬇️

| Parameter         | Type    | Required      | Description                            |
| ----------------- | ------- | ------------- | -------------------------------------- |
| `--profile`, `-p` | string  | ✅             | Profile from `profiles.json`.          |
| `--ticket`, `-t`  | ulong   | ✅             | Position ticket. Must be `> 0`.        |
| `--sl`            | double? | ⛔ (either/or) | New **Stop Loss price** (absolute).    |
| `--tp`            | double? | ⛔ (either/or) | New **Take Profit price** (absolute).  |
| `--timeout-ms`    | int     | ❌             | RPC timeout in ms (default: `30000`).  |
| `--dry-run`       | flag    | ❌             | Print intended action without sending. |

> At least **one** of `--sl` or `--tp` must be provided.

---

## Output Fields ⬆️

| Field    | Type   | Description                        |
| -------- | ------ | ---------------------------------- |
| `Ticket` | ulong  | Modified ticket.                   |
| `NewSL`  | double | Applied Stop Loss (if provided).   |
| `NewTP`  | double | Applied Take Profit (if provided). |
| `Status` | string | `OK` or error description.         |

---

## How to Use 🛠️

### CLI

```powershell
# Set SL and TP
dotnet run -- position.modify -p demo -t 123456 --sl 1.0950 --tp 1.1050

# Only SL
dotnet run -- position.modify -p demo -t 123456 --sl 1.0900

# Dry‑run (no request)
dotnet run -- position.modify -p demo -t 123456 --tp 1.1100 --dry-run
```

### PowerShell Shortcuts (from `ps/shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
posmod -t 123456 -sl 1.0900 -tp 1.1100
# expands to: mt5 position.modify -p demo -t 123456 --sl 1.0900 --tp 1.1100 --timeout-ms 90000
```

---

## Notes & Safety 🛡️

* `--sl` / `--tp` are **prices** (not offsets). They must respect broker **StopsLevel** (min distance).
* If neither `--sl` nor `--tp` is provided, the command fails fast with a clear error.
* Use `symbol show`/`limits` to verify instrument properties and allowable distances.

---

## Code Reference (exact) 🧩

```csharp
var posModTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket") { IsRequired = true };
var posModSlOpt     = new Option<double?>(new[] { "--sl" }, "New Stop Loss (price)");
var posModTpOpt     = new Option<double?>(new[] { "--tp" }, "New Take Profit (price)");

var posModify = new Command("position.modify", "Modify SL/TP for a position by ticket");
posModify.AddAlias("posmod");

posModify.AddOption(profileOpt);
posModify.AddOption(posModTicketOpt);
posModify.AddOption(posModSlOpt);
posModify.AddOption(posModTpOpt);
posModify.AddOption(timeoutOpt);
posModify.AddOption(dryRunOpt);

posModify.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(posModTicketOpt);
    var sl        = ctx.ParseResult.GetValueForOption(posModSlOpt);
    var tp        = ctx.ParseResult.GetValueForOption(posModTpOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (ticket == 0) throw new ArgumentOutOfRangeException(nameof(ticket), "Ticket must be > 0.");
    if (sl is null && tp is null) throw new ArgumentException("Specify at least one of --sl or --tp.");

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:POSITION.MODIFY Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} SL:{SL} TP:{TP}", ticket, sl, tp))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] POSITION.MODIFY ticket={ticket} SL={(sl?.ToString() ?? "-")} TP={(tp?.ToString() ?? "-")}");
            return;
        }

        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            await CallWithRetry(
                ct => _mt5Account.ModifyPositionSlTpAsync(ticket, sl, tp, ct),
                opCts.Token);

            Console.WriteLine("✔ position.modify done");
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

root.AddCommand(posModify);
```
