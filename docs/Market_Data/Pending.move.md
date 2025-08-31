# Pending Move (`pending.move`) ↔️

## What it Does

Moves a **pending order** to a **new entry price** (or by an **offset** if supported).
Useful to chase price action without recreating the order.

> This command operates on **pending** orders only (Buy Limit/Stop, Sell Limit/Stop, and their *Limit* variants). It does **not** work on already filled positions.

---

## Input Parameters ⬇️

| Parameter         | Type     | Description                                           |
| ----------------- | -------- | ----------------------------------------------------- |
| `--profile`, `-p` | string   | Which profile to use (from `profiles.json`).          |
| `--ticket`, `-t`  | ulong    | Ticket ID of the pending order to move.               |
| `--price`         | double   |  **Absolute** new entry price.                         |
| `--offset-points` | int      | **Relative** shift in points (e.g., `+50`, `-25`).    |
| `--sl`            | double   | Optional new Stop Loss (kept unchanged if omitted).   |
| `--tp`            | double   |  Optional new Take Profit (kept unchanged if omitted). |
| `--expiration`    | DateTime |  Optional new expiration (UTC).                        |
| `--output`, `-o`  | string   |  `text` (default) or `json`.                           |
| `--timeout-ms`    | int      |  RPC timeout in ms (default: 30000).                   |
| `--dry-run`       | flag     |  Print the intended change without sending request.    |

> Your build may support **either** `--price` **or** `--offset-points`, or both. If both are provided, prefer absolute `--price`.

---

## Output Fields ⬆️

| Field        | Type     | Description                              |
| ------------ | -------- | ---------------------------------------- |
| `Ticket`     | ulong    | Target order ticket.                     |
| `OldPrice`   | double   | Previous entry price.                    |
| `NewPrice`   | double   | New entry price after the move.          |
| `SL`         | double   | Stop Loss after the move (if changed).   |
| `TP`         | double   | Take Profit after the move (if changed). |
| `Expiration` | DateTime | Expiration (if set/changed).             |
| `Status`     | string   | `OK` or error description.               |

---

## How to Use 🛠️

### CLI

```powershell
# Move to an absolute price
dotnet run -- pending.move -p demo -t 123456 --price 1.1000

# Move by an offset of +50 points (if supported)
dotnet run -- pending.move -p demo -t 123456 --offset-points 50

# Also adjust SL/TP
dotnet run -- pending.move -p demo -t 123456 --price 1.1000 --sl 1.0950 --tp 1.1050 -o json
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
pmove -t 123456 --price 1.1000
```

---

## When to Use ❓

* Nudge a pending entry closer/further as market evolves.
* Keep the order alive while trailing the price without cancellation/recreate.
* Adjust SL/TP along with a relocated entry.

---

## Notes & Safety 🛡️

* Broker **stop levels** and min distance apply; the move may be rejected if too close to current price.
* For `offset-points`, the calculation uses the symbol **point size** (e.g., 0.00001 for 5‑digit FX).
* Ensure the order type is compatible with the new side of the book (e.g., Buy Limit must stay below market, Buy Stop above, etc.).
* `--dry-run` is recommended to verify intended values.

---

## Code Reference 🧩

```csharp
var pmByPtsOpt  = new Option<int>(new[] { "--by-points", "-P" }, "Shift by points (signed, e.g. +15 or -20)") { IsRequired = true };

var pendingMove = new Command("pending.move", "Move a pending order price(s) by ±N points");
pendingMove.AddAlias("pmove");

pendingMove.AddOption(profileOpt);
pendingMove.AddOption(pmTicketOpt);
pendingMove.AddOption(pmByPtsOpt);
pendingMove.AddOption(timeoutOpt);
pendingMove.AddOption(dryRunOpt);

pendingMove.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(pmTicketOpt);
    var byPoints  = ctx.ParseResult.GetValueForOption(pmByPtsOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    if (byPoints == 0) { Console.WriteLine("Nothing to do: by-points is 0."); return; }

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:PENDING.MOVE Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} ByPoints:{By}", ticket, byPoints))
    {
        try
        {
            await ConnectAsync();
```
