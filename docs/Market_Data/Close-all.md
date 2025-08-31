# Close All (`close-all`)

## What it Does

Closes **all open positions** on the current MT5 account in one go.
Useful for emergency flattening, end-of-session cleanup, or switching strategies.

---

## Input Parameters ⬇️

| Parameter                     | Type   | Description                                               |
| ----------------------------- | ------ | --------------------------------------------------------- |
| `--profile`, `-p`             | string | Which profile to use (from `profiles.json`).              |
| `--output`, `-o`              | string | `text` (default) or `json`.                               |
| `--timeout-ms`                | int    | RPC timeout in ms (default: 30000).                       |
| `--dry-run`                   | flag   |  Print planned actions without sending requests.           |
| *(optional)* `--symbol`, `-s` | string |  Close only positions for a symbol (if supported by code). |
| *(optional)* `--side`         | string | `buy` / `sell` (if supported).                            |
| *(optional)* `--magic`        | int    |  Filter by EA magic (if supported).                        |

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
```
