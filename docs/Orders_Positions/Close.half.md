# Close Half (`close.half`) ✂️

## What it Does 🎯

Closes **half of a position’s volume** by ticket.
Convenient for partial profit‑taking while keeping the other half running.

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                    |
| ----------------- | ------ | -------- | ---------------------------------------------- |
| `--profile`, `-p` | string | ✅        | Profile from `profiles.json`.                  |
| `--ticket`, `-t`  | ulong  | ✅        | Position ticket to partially close.            |
| `--deviation`     | int    | ❌        | Max slippage (points). Default: `10`.          |
| `--output`, `-o`  | string | ❌        | `text` (default) or `json`.                    |
| `--timeout-ms`    | int    | ❌        | RPC timeout in ms (default: 30000).            |
| `--dry-run`       | flag   | ❌        | Print intended action without sending request. |

---

## Output Fields ⬆️

| Field       | Type   | Description                       |
| ----------- | ------ | --------------------------------- |
| `Ticket`    | ulong  | Original position ticket.         |
| `Closed`    | double | Volume closed (half of original). |
| `Remaining` | double | Volume still open.                |
| `Price`     | double | Execution price of the close.     |
| `Status`    | string | `OK` or error description.        |

---

## How to Use 🛠️

### CLI

```powershell
# Close half of position 123456
dotnet run -- close.half -p demo -t 123456

# With custom slippage and JSON output
dotnet run -- close.half -p demo -t 123456 --deviation 20 -o json

# Dry-run (no request sent)
dotnet run -- close.half -p demo -t 123456 --dry-run
```

### PowerShell Shortcuts (from `shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
ch -t 123456
# expands to: mt5 close.half -p demo -t 123456 --deviation 10 --timeout-ms 90000
```

---

## When to Use ❓

* To lock in profit on half the trade while keeping exposure.
* Common in scaling‑out strategies (e.g., close 50% at +1R, let rest run).
* Useful for testing margin/risk effect of partial closes.

---

## Notes & Safety 🛡️

* If the position volume is not evenly divisible by 2, the code rounds to nearest lot step (check `symbol limits`).
* Broker may reject very small residual lots — always confirm `MinLot` and `LotStep`.
* `--deviation` is critical for fast markets; widen if you get rejections.

---

## Code Reference 🧩

```csharp
var chTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket") { IsRequired = true };

var closeHalf = new Command("close.half", "Close half of a position by ticket");
closeHalf.AddAlias("ch");

closeHalf.AddOption(profileOpt);
closeHalf.AddOption(chTicketOpt);
closeHalf.AddOption(cpDevOpt);
closeHalf.AddOption(timeoutOpt);
closeHalf.AddOption(dryRunOpt);

closeHalf.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(chTicketOpt);
    var deviation = ctx.ParseResult.GetValueForOption(cpDevOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    ctx.Console.WriteLine("(alias) close.half -> close.percent --pct 50");

    await closePercent.InvokeAsync(new[]
    {
        "--profile",    profile,
        "--ticket",     ticket.ToString(System.Globalization.CultureInfo.InvariantCulture),
        "--pct",        "50",
        "--deviation",  deviation.ToString(System.Globalization.CultureInfo.InvariantCulture),
        "--timeout-ms", timeoutMs.ToString(System.Globalization.CultureInfo.InvariantCulture),
        // if desired, we throw dry-run:
        // dryRun ? "--dry-run" : null
    }
        // if you add conditional elements, finish .Where(s => s != null)!.ToArray()
    );
});

root.AddCommand(closePercent);
root.AddCommand(closeHalf);
```
