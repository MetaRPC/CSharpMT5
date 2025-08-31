# Position Modify Points (`position.modify.points`) 📐

## What it Does

Sets **Stop Loss** and/or **Take Profit** by a **distance in points** from a chosen base price — either the **entry price** or the current **market** price.

Alias: `pmp`

---

## Input Parameters ⬇️

| Parameter         | Type   | Description                                |
| ----------------- | ------ |------------------------------------------ |
| `--profile`, `-p` | string | Profile from `profiles.json`.              |
| `--ticket`, `-t`  | ulong  | Position ticket (> 0).                     |
| `--sl-points`     | int?   | SL distance in **points** (≥ 0).           |
| `--tp-points`     | int?   | TP distance in **points** (≥ 0).           |
| `--from`          | string | Base price: `entry` (default) or `market`. |
| `--timeout-ms`    | int    | RPC timeout in ms (default: `30000`).      |
| `--dry-run`       | flag   | Print intended action without sending.     |

> At least **one** of `--sl-points` or `--tp-points` must be provided.
> `--from` selects the base price: *entry* uses `PriceOpen`; *market* uses **Bid** for BUY and **Ask** for SELL.

---

## How Prices Are Calculated 🧮

Let `Pbase` be the base price (`entry` or `market`), and `point` be the instrument point size.

* For **BUY** positions:

  * `SL = Pbase − sl_points × point`
  * `TP = Pbase + tp_points × point`
* For **SELL** positions:

  * `SL = Pbase + sl_points × point`
  * `TP = Pbase − tp_points × point`

**Point size guess:** uses `_mt5Account.PointGuess(symbol)`; if it returns ≤ 0, falls back to `0.01` for JPY symbols, otherwise `0.0001`.

---

## Output Fields ⬆️

| Field    | Type   | Description                              |
| -------- | ------ | ---------------------------------------- |
| `Ticket` | ulong  | Modified ticket.                         |
| `NewSL`  | double | Applied SL price (if provided).          |
| `NewTP`  | double | Applied TP price (if provided).          |
| `From`   | string | `entry` or `market` used for base price. |
| `Status` | string | `OK` or error description.               |

---

## How to Use 🛠️

### CLI

```powershell
# Set SL 150 pts below entry, TP 300 pts above entry (BUY logic)
dotnet run -- position.modify.points -p demo -t 123456 --sl-points 150 --tp-points 300

# From MARKET (SELL logic): SL 200 pts, TP 100 pts
dotnet run -- position.modify.points -p demo -t 123456 --from market --sl-points 200 --tp-points 100

# Only TP by points
dotnet run -- position.modify.points -p demo -t 123456 --tp-points 250

# Dry‑run
dotnet run -- position.modify.points -p demo -t 123456 --from market --sl-points 120 --dry-run
```

### PowerShell Shortcuts (from `ps/shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
pmp -t 123456 -slp 150 -tpp 300 -from entry
# expands to: mt5 position.modify.points -p demo -t 123456 --sl-points 150 --tp-points 300 --from entry --timeout-ms 90000
```

---

## Notes & Safety 🛡️

* Distances are in **points**, not pips; confirm the instrument’s point value (`symbol show`).
* Resulting prices must respect broker **StopsLevel** (min distance) and symbol trading settings.
* For *market* base, uses **Bid** for BUY and **Ask** for SELL to avoid instant stops.
* If both point distances are omitted, the command fails fast with a clear error.

---

## Code Reference 🧩

```csharp
var pmpTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket") { IsRequired = true };
var pmpSlPtsOpt  = new Option<int?>(new[] { "--sl-points" }, "SL distance in POINTS (from base price)");
var pmpTpPtsOpt  = new Option<int?>(new[] { "--tp-points" }, "TP distance in POINTS (from base price)");
var pmpFromOpt   = new Option<string>(new[] { "--from" }, () => "entry", "entry|market"); // default: entry

var posModPts = new Command("position.modify.points", "Set SL/TP by distance in points from entry/market");
posModPts.AddAlias("pmp");

posModPts.AddOption(profileOpt);
posModPts.AddOption(pmpTicketOpt);
posModPts.AddOption(pmpSlPtsOpt);
posModPts.AddOption(pmpTpPtsOpt);
posModPts.AddOption(pmpFromOpt);
posModPts.AddOption(timeoutOpt);
posModPts.AddOption(dryRunOpt);

posModPts.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(pmpTicketOpt);
    var slPts     = ctx.ParseResult.GetValueForOption(pmpSlPtsOpt);
    var tpPts     = ctx.ParseResult.GetValueForOption(pmpTpPtsOpt);
    var fromStr   = (ctx.ParseResult.GetValueForOption(pmpFromOpt) ?? "entry").Trim().ToLowerInvariant();
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    if (fromStr != "entry" && fromStr != "market")
        throw new ArgumentException("Invalid --from. Use entry|market.");
    if (slPts is null && tpPts is null)
        throw new ArgumentException("Specify at least one of --sl-points or --tp-points.");
    if (slPts is not null && slPts < 0) throw new ArgumentOutOfRangeException(nameof(slPts));
    if (tpPts is not null && tpPts < 0) throw new ArgumentOutOfRangeException(nameof(tpPts));

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:POSITION.MODIFY.POINTS Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} SLpts:{SL} TPpts:{TP} From:{From}", ticket, slPts, tpPts, fromStr))
    {
        try
        {
            await ConnectAsync();
```
