# Close Percent (`close.percent`) 🎯%

## What it Does

Closes a **percentage of a position’s volume** by ticket.
Great for scaling‑out with rules like “take 50% at +1R, trail the rest”.

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                    |
| ----------------- | ------ | -------- | ---------------------------------------------- |
| `--profile`, `-p` | string | ✅        | Profile from `profiles.json`.                  |
| `--ticket`, `-t`  | ulong  | ✅        | Position ticket to partially close.            |
| `--pct`           | double | ✅        | Percentage to close (e.g., `50` for 50%).      |
| `--deviation`     | int    | ❌        | Max slippage (points). Default: `10`.          |
| `--output`, `-o`  | string | ❌        | `text` (default) or `json`.                    |
| `--timeout-ms`    | int    | ❌        | RPC timeout in ms (default: 30000).            |
| `--dry-run`       | flag   | ❌        | Print intended action without sending request. |

---

## Output Fields ⬆️

| Field       | Type   | Description                                                  |
| ----------- | ------ | ------------------------------------------------------------ |
| `Ticket`    | ulong  | Original position ticket.                                    |
| `Closed`    | double | Volume closed (computed from percent & rounded to lot step). |
| `Remaining` | double | Volume still open.                                           |
| `Price`     | double | Execution price of the close.                                |
| `Percent`   | double | Requested percent closed.                                    |
| `Status`    | string | `OK` or error description.                                   |

---

## How to Use 🛠️

### CLI

```powershell
# Close 50% of position 123456
dotnet run -- close.percent -p demo -t 123456 --pct 50

# Close 25% with wider slippage and JSON output
dotnet run -- close.percent -p demo -t 123456 --pct 25 --deviation 20 -o json

# Dry-run (no request sent)
dotnet run -- close.percent -p demo -t 123456 --pct 33.3 --dry-run
```

### PowerShell Shortcuts (from `shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
cpp -t 123456 -pct 50
# expands to: mt5 close.percent -p demo -t 123456 --pct 50 --deviation 10 --timeout-ms 90000
```

---

## When to Use ❓

* Rule‑based scaling out at fixed checkpoints (25%, 50%, 75%).
* Reduce risk exposure while keeping a runner.
* Portfolio management: normalize exposure across positions.

---

## Notes & Safety 🛡️

* The computed close volume is **rounded to lot step** and clamped to `[min lot; current volume]`.
* If the result is below **MinLot**, broker may reject — consider `close.partial` instead.
* `--deviation` matters on fast markets; widen if you see rejections.

---

## Code Reference 🧩

```csharp
var cpTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket") { IsRequired = true };
var cpPctOpt    = new Option<double>(new[] { "--pct" }, () => 50.0, "Percent to close (0 < pct ≤ 100)");
var cpDevOpt    = devOpt;

var closePercent = new Command("close.percent", "Close a percentage of a position by ticket");
closePercent.AddAlias("cpp");

closePercent.AddOption(profileOpt);
closePercent.AddOption(cpTicketOpt);
closePercent.AddOption(cpPctOpt);
closePercent.AddOption(cpDevOpt);
closePercent.AddOption(timeoutOpt);
closePercent.AddOption(dryRunOpt);

            closePercent.SetHandler(async (InvocationContext ctx) =>
            {
                var profile = ctx.ParseResult.GetValueForOption(profileOpt)!;
                var ticket = ctx.ParseResult.GetValueForOption(cpTicketOpt);
                var pct = ctx.ParseResult.GetValueForOption(cpPctOpt);
                var deviation = ctx.ParseResult.GetValueForOption(cpDevOpt);
                var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
                var dryRun = ctx.ParseResult.GetValueForOption(dryRunOpt);

                Validators.EnsureProfile(profile);
                Validators.EnsureTicket(ticket);
                if (pct <= 0 || pct > 100) throw new ArgumentOutOfRangeException(nameof(pct), "Percent must be in (0;100].");

                using (UseOpTimeout(timeoutMs))
                using (_logger.BeginScope("Cmd:CLOSE.PERCENT Profile:{Profile}", profile))
                using (_logger.BeginScope("Ticket:{Ticket} Pct:{Pct} Dev:{Dev}", ticket, pct, deviation))
                {
                    try
                    {
                        await ConnectAsync();
                        using var opCts = StartOpCts();

                        var opened = await CallWithRetry(
                            ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                            opCts.Token);

                        var pos = opened.PositionInfos.FirstOrDefault(p => Convert.ToUInt64(p.Ticket) == ticket);
                        if (pos is null)
                        {
                            Console.WriteLine($"Position #{ticket} not found.");
                            Environment.ExitCode = 2;
                            return;
                        }

                        var symbol = pos.Symbol;
                        var posVol = pos.Volume;                  // double
                        var toClose = posVol * (pct / 100.0);

                        toClose = Math.Round(toClose, 2, MidpointRounding.AwayFromZero);
                        if (toClose <= 0) { Console.WriteLine("Nothing to close after rounding."); Environment.ExitCode = 2; return; }
                        if (toClose > posVol) toClose = posVol;

                        if (dryRun)
                        {
                            Console.WriteLine($"[DRY-RUN] CLOSE.PERCENT #{ticket} {symbol}: close {toClose} of {posVol} (dev={deviation})");
                            return;
                        }

                        await CallWithRetry(
                            ct => _mt5Account.ClosePositionPartialAsync(ticket, toClose, deviation, ct),
                            opCts.Token);

                        Console.WriteLine($"✔ close.percent done: #{ticket} vol={toClose}");
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
```
