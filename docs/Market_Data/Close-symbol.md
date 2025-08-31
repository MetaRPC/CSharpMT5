# Close by Symbol (`close-symbol`)

## What it Does

Closes **all open positions** for a specific **symbol** on the current MT5 account.
Great for flattening exposure on one instrument without touching others.

---

## Input Parameters ⬇️

| Parameter              | Type   | Description                                             |
| ---------------------- | ------ | ------------------------------------------------------- |
| `--profile`, `-p`      | string | Which profile to use (from `profiles.json`).            |
| `--symbol`, `-s`       | string | Symbol to close (e.g. `EURUSD`).                        |
| `--deviation`          | int    | Max slippage in points (default: 10).                   |
| `--output`, `-o`       | string | `text` (default) or `json`.                             |
| `--timeout-ms`         | int    | Per-RPC timeout in ms (default: 30000).                 |
| `--dry-run`            | flag   | Print planned actions without sending requests.         |
| *(optional)* `--side`  | string | Limit to `buy` or `sell` positions only (if supported). |
| *(optional)* `--magic` | int    | Filter by EA magic number (if supported).               |

---

## Output Fields ⬆️

| Field     | Type   | Description                                       |
| --------- | ------ | ------------------------------------------------- |
| `Symbol`  | string | Target symbol.                                    |
| `Total`   | int    | How many positions were targeted.                 |
| `Closed`  | int    | How many were successfully closed.                |
| `Errors`  | int    | How many failed to close.                         |
| `Items[]` | array  | Per-ticket results (ticket, volume, status, msg). |

---

## How to Use 🛠️

### CLI

```powershell
# Close all EURUSD positions
dotnet run -- close-symbol -p demo -s EURUSD

# Preview only, JSON
dotnet run -- close-symbol -p demo -s EURUSD --dry-run -o json --timeout-ms 60000
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
close-symbol -s EURUSD
```

---

## When to Use ❓

* Flatten risk on a single instrument quickly.
* End-of-session cleanup per symbol.
* Strategy rollovers where only one market needs to be cleared.

---

## Notes & Safety 🛡️

* Consider the default `deviation` used for close requests.
* If the market is closed or trading is disabled for the symbol, items will be reported under `Errors`.
* `--dry-run` is safe: it logs the intended actions without sending orders.

---

## Code Reference 🧩

```csharp
var closeSymbol = new Command("close-symbol", "Close ALL open positions for a given symbol");
closeSymbol.AddAlias("cs");
closeSymbol.AddAlias("flatten-symbol");

closeSymbol.AddOption(profileOpt);
closeSymbol.AddOption(symbolOpt);
// reuse the global confirmation flag from close-all (caYesOpt)
closeSymbol.AddOption(caYesOpt);

closeSymbol.SetHandler(async (string profile, string? symbol, bool yes, int timeoutMs, bool dryRun) =>
{
    Validators.EnsureProfile(profile);

    // Default to profile's default symbol if not provided
    var s = Validators.EnsureSymbol(symbol ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CLOSE-SYMBOL Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol}", s))
    {
        try
        {
            if (dryRun)
            {
                Console.WriteLine($"[DRY-RUN] Would close all positions for {s}.");
                return;
            }

            await ConnectAsync();
```
