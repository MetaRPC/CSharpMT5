# Close All (`close-all`) 🧹

## What it Does 🎯

Closes **all open positions** on the current MT5 account in one go.
Useful for emergency flattening, end-of-session cleanup, or switching strategies.

> *If your implementation supports filters (e.g., by symbol/side/magic), use them to limit what gets closed. Otherwise it closes **everything** that’s open.*

---

## Input Parameters ⬇️

| Parameter                     | Type   | Required | Description                                               |
| ----------------------------- | ------ | -------- | --------------------------------------------------------- |
| `--profile`, `-p`             | string | ✅        | Which profile to use (from `profiles.json`).              |
| `--output`, `-o`              | string | ❌        | `text` (default) or `json`.                               |
| `--timeout-ms`                | int    | ❌        | RPC timeout in ms (default: 30000).                       |
| `--dry-run`                   | flag   | ❌        | Print planned actions without sending requests.           |
| *(optional)* `--symbol`, `-s` | string | ❌        | Close only positions for a symbol (if supported by code). |
| *(optional)* `--side`         | string | ❌        | `buy` / `sell` (if supported).                            |
| *(optional)* `--magic`        | int    | ❌        | Filter by EA magic (if supported).                        |

---

## Output Fields ⬆️

| Field     | Type  | Description                                           |
| --------- | ----- | ----------------------------------------------------- |
| `Total`   | int   | How many positions were targeted.                     |
| `Closed`  | int   | How many were successfully closed.                    |
| `Errors`  | int   | How many failed to close.                             |
| `Items[]` | array | Per-ticket results (ticket, volume, status, message). |

---

## How to Use 🛠️

### CLI

```powershell
# Close everything on the account (no filters)
dotnet run -- close-all -p demo

# Preview only (no real requests)
dotnet run -- close-all -p demo --dry-run -o json

# If filters are supported by your build:
dotnet run -- close-all -p demo -s EURUSD
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
close-all    # or alias if you have one in shortcasts
```

---

## When to Use ❓

* **Emergency flatten** — flatten exposure quickly.
* **End-of-day** — exit everything before market close.
* **Strategy switch** — clear previous positions prior to deployment.

---

## Notes & Safety 🛡️

* Consider slippage (`deviation`) defaults used internally by your close calls.
* If server rejects some closes (e.g., trading disabled, market closed), the result should report per-ticket errors.
* In `--dry-run` mode nothing is sent; use it to confirm filters and scope.

---

## Code Reference 🧩

```csharp
var caSymbolOpt = new Option<string?>(new[] { "--filter-symbol", "-s" }, "Close only positions for this symbol (e.g., EURUSD)");
var caYesOpt    = new Option<bool>(new[] { "--yes", "-y" }, "Do not ask for confirmation");
var caDevOpt    = new Option<int>(new[] { "--deviation" }, () => 10, "Max slippage in points");

var closeAll = new Command("close-all", "Close ALL open positions (optionally filtered by symbol)");
closeAll.AddAlias("flatten");
closeAll.AddAlias("close.all"); 

closeAll.AddOption(profileOpt);
closeAll.AddOption(caSymbolOpt);
closeAll.AddOption(caYesOpt);
closeAll.AddOption(caDevOpt);
closeAll.AddOption(timeoutOpt);
closeAll.AddOption(dryRunOpt);

closeAll.SetHandler(async (InvocationContext ctx) =>
{
    var profile      = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var filterSymbol = ctx.ParseResult.GetValueForOption(caSymbolOpt);
    var yes          = ctx.ParseResult.GetValueForOption(caYesOpt);
    var deviation    = ctx.ParseResult.GetValueForOption(caDevOpt);
    var timeoutMs    = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun       = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (!string.IsNullOrWhiteSpace(filterSymbol)) _ = Validators.EnsureSymbol(filterSymbol);

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CLOSE-ALL Profile:{Profile}", profile))
    using (_logger.BeginScope("FilterSymbol:{Symbol} Dev:{Dev}", filterSymbol ?? "<any>", deviation))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            var map = await CallWithRetry(
                ct => _mt5Account.ListPositionVolumesAsync(filterSymbol, ct),
                opCts.Token);

            if (map.Count == 0)
            {
                Console.WriteLine("No positions to close.");
                return;
            }

            if (!yes || dryRun)
            {
                Console.WriteLine($"Will close {map.Count} position(s){(filterSymbol is null ? "" : $" for {filterSymbol}")}. Deviation={deviation}");
                foreach (var (ticket, vol) in map.Take(10))
                    Console.WriteLine($"  #{ticket} vol={vol}");
                if (map.Count > 10) Console.WriteLine($"  ... and {map.Count - 10} more");
                if (dryRun) return;
                Console.WriteLine("Pass --yes to execute.");
                Environment.ExitCode = 2;
                return;
            }

            int ok = 0, fail = 0;
            foreach (var (ticket, vol) in map)
            {
                try
                {
                    await CallWithRetry(ct => _mt5Account.ClosePositionFullAsync(ticket, vol, deviation, ct), opCts.Token);
                    ok++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Close #{Ticket} vol={Vol} failed: {Msg}", ticket, vol, ex.Message);
                    fail++;
                }
            }

            Console.WriteLine($"Closed OK: {ok}; Failed: {fail}");
            Environment.ExitCode = fail == 0 ? 0 : 1;
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

root.AddCommand(closeAll);
```
