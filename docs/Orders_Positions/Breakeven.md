# Breakeven (`breakeven`) ⚖️

## What it Does 🎯

Moves **Stop Loss** on a position to **breakeven** (near entry price), with optional **offset**.
Used to remove downside risk after the trade goes in your favor.

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                                              |
| ----------------- | ------ | -------- | ------------------------------------------------------------------------ |
| `--profile`, `-p` | string | ✅        | Profile from `profiles.json`.                                            |
| `--ticket`, `-t`  | ulong  | ✅        | Position ticket to adjust.                                               |
| `--offset`        | double | ❌        | Offset from entry price in **price units** (e.g., `0.0002`).             |
| `--offset-points` | int    | ❌        | Offset from entry price in **points** (e.g., `20`).                      |
| `--force`         | flag   | ❌        | Force move even if it would **worsen** SL (by default, we only improve). |
| `--output`, `-o`  | string | ❌        | `text` (default) or `json`.                                              |
| `--timeout-ms`    | int    | ❌        | RPC timeout in ms (default: 30000).                                      |

> If both `--offset` and `--offset-points` are provided, the price `--offset` usually takes precedence.

---

## Output Fields ⬆️

| Field    | Type   | Description                                |
| -------- | ------ | ------------------------------------------ |
| `Ticket` | ulong  | Position ticket.                           |
| `Symbol` | string | Symbol of the position.                    |
| `Entry`  | double | Entry price.                               |
| `OldSL`  | double | Stop Loss before the move.                 |
| `NewSL`  | double | Stop Loss after the move (entry ± offset). |
| `Offset` | string | Offset applied (in price and/or points).   |
| `Status` | string | `OK` or error description.                 |

---

## How it Works 🧠

* For **BUY** positions: `NewSL = Entry + Offset` (offset can be 0 → exact BE).
* For **SELL** positions: `NewSL = Entry - Offset`.
* If `--force` **not** set, implementation should only move SL **toward profit** (never further away than current SL).
* If `--offset-points` is used, the offset is computed with the symbol **point size**.

---

## How to Use 🛠️

### CLI

```powershell
# Move SL to exact breakeven
dotnet run -- breakeven -p demo -t 123456

# Breakeven + 20 points buffer
dotnet run -- breakeven -p demo -t 123456 --offset-points 20

# Force move even if it degrades SL (use with caution!)
dotnet run -- breakeven -p demo -t 123456 --offset 0.0002 --force -o json
```

### PowerShell Shortcuts (from `shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
be -t 123456 -offsetPts 20   # expands to: mt5 breakeven -p demo -t 123456 --offset-points 20 --timeout-ms 90000
```

---

## Notes & Safety 🛡️

* Broker **stops level** may prevent SL from being too close to market — expect rejections if too tight.
* For 5‑digit FX, `20` points = `2` pips; verify your point size via `symbol show`.
* Consider combining with `position.modify.points` (`pmp`) for more granular control.

---

## Code Reference (to be filled by you) 🧩

```csharp
var beTicketOpt       = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket to move SL to breakeven") { IsRequired = true };
var beOffsetPriceOpt  = new Option<double?>(new[] { "--offset" }, "Offset from entry in PRICE units (e.g., 0.0002)");
var beOffsetPointsOpt = new Option<int?>(new[] { "--offset-points", "-P" }, "Offset from entry in POINTS");
var beForceOpt        = new Option<bool>(new[] { "--force" }, "Allow worsening SL (by default only improve)");

var breakeven = new Command("breakeven", "Move SL to entry ± offset (breakeven) for a position");
breakeven.AddAlias("be");

breakeven.AddOption(profileOpt);
breakeven.AddOption(beTicketOpt);
breakeven.AddOption(beOffsetPriceOpt);
breakeven.AddOption(beOffsetPointsOpt);
breakeven.AddOption(beForceOpt);

breakeven.SetHandler(async (string profile, ulong ticket, double? offsetPrice, int? offsetPoints, bool force, int timeoutMs, bool dryRun) =>
{
```
