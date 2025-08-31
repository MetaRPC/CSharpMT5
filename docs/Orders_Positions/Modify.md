# Modify (`modify`) ✏️

## What it Does

Updates **Stop Loss** and/or **Take Profit** for a position **by ticket**.

> Your repo also has a `position.modify` flow used by the shortcast `posmod`. This page documents the generic **`modify`** command you pasted; if your build exposes it as `position.modify`, adjust the command name in examples accordingly.

---

## Input Parameters ⬇️

| Parameter         | Type    | Description                                                  |
| ----------------- | ------- | ------------------------------------------------------------ |
| `--profile`, `-p` | string  | Profile from `profiles.json`.                                |
| `--ticket`, `-t`  | ulong   | Position ticket to modify.                                   |
| `--sl`            | double? | New **Stop Loss price** (absolute).                          |
| `--tp`            | double? | New **Take Profit price** (absolute).                        |
| `--symbol`, `-s`  | string? | Optional symbol (used to ensure visibility on some servers). |
| `--timeout-ms`    | int     | RPC timeout in ms (default: `30000`).                        |
| `--dry-run`       | flag    | Print intended action without sending the request.           |

> At least **one** of `--sl` or `--tp` **must** be provided.

---

## Output Fields ⬆️

| Field    | Type   | Description                          |
| -------- | ------ | ------------------------------------ |
| `Ticket` | ulong  | Modified ticket.                     |
| `OldSL`  | double | Previous Stop Loss (if available).   |
| `OldTP`  | double | Previous Take Profit (if available). |
| `NewSL`  | double | Applied Stop Loss.                   |
| `NewTP`  | double | Applied Take Profit.                 |
| `Status` | string | `OK` or error description.           |

---

## How to Use 🛠️

### CLI (this command)

```powershell
# Set both SL and TP
dotnet run -- modify -p demo -t 123456 --sl 1.0950 --tp 1.1050

# Only SL
dotnet run -- modify -p demo -t 123456 --sl 1.0900

# With symbol visibility (some servers require it)
dotnet run -- modify -p demo -t 123456 --sl 1.0900 -s EURUSD

# Dry-run
dotnet run -- modify -p demo -t 123456 --tp 1.1100 --dry-run
```

### PowerShell Shortcuts (from `ps/shortcasts.ps1`)

If you use the shortcast **`posmod`**:

```powershell
. .\ps\shortcasts.ps1
use-pf demo
posmod -t 123456 -sl 1.0900 -tp 1.1100
# expands to: mt5 position.modify -p demo -t 123456 --sl 1.0900 --tp 1.1100 --timeout-ms 90000
```

---

## Notes & Safety 🛡️

* `--sl` / `--tp` are **prices** (not offsets). They must respect broker **StopsLevel** (min distance).
* Some servers require the **symbol to be visible** in Market Watch; pass `-s EURUSD` to let the tool ensure visibility.
* If both `--sl` and `--tp` are omitted, the command will fail fast with a clear error.

---

## Code Reference 🧩

```csharp
>var modify = new Command("modify", "Modify StopLoss / TakeProfit by ticket");
modify.AddAlias("m");

var modTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Ticket to modify") { IsRequired = true };
var modSlOpt     = new Option<double?>("--sl", "New Stop Loss (price)");
var modTpOpt     = new Option<double?>("--tp", "New Take Profit (price)");
// optional symbol: some servers are picky and require symbol visibility in terminal
var modSymbolOpt = new Option<string?>(
    new[] { "--symbol", "-s" },
    description: "Symbol (optional; used to ensure visibility if needed)");

modify.AddOption(profileOpt);
modify.AddOption(modTicketOpt); 
modify.AddOption(modSlOpt);
modify.AddOption(modTpOpt);
modify.AddOption(modSymbolOpt);

modify.SetHandler(async (string profile, ulong ticket, double? sl, double? tp, string? symbol, int timeoutMs, bool dryRun) =>
{
```
