# Partial Close (`partial-close`) ✂️

## What it Does 🎯

Closes **part** of an open position by ticket. You can specify either:

* an **exact volume** in lots, or
* a **percentage** of the current position volume.

---

## Input Parameters ⬇️

| Parameter         | Type    | Required      | Description                                 |
| ----------------- | ------- | ------------- | ------------------------------------------- |
| `--profile`, `-p` | string  | ✅             | Profile from `profiles.json`.               |
| `--ticket`, `-t`  | ulong   | ✅             | Position ticket.                            |
| `--percent`, `-P` | int?    | ⛔ (either/or) | Percent of current volume to close (1–100). |
| `--volume`, `-v`  | double? | ⛔ (either/or) | Exact volume (lots) to close.               |
| `--timeout-ms`    | int     | ❌             | RPC timeout in ms (default: `30000`).       |
| `--dry-run`       | flag    | ❌             | Print intended action without sending.      |

⚠️ Exactly one of `--percent` **or** `--volume` must be provided.

---

## Output Fields ⬆️

| Field        | Type   | Description                  |
| ------------ | ------ | ---------------------------- |
| `Ticket`     | ulong  | Position ticket.             |
| `Symbol`     | string | Instrument symbol.           |
| `ClosedVol`  | double | Actual closed volume (lots). |
| `CurrentVol` | double | Volume before partial close. |
| `Status`     | string | `OK` or error message.       |

---

## How to Use 🛠️

### CLI

```powershell
# Close 50% of position
dotnet run -- partial-close -p demo -t 123456 -P 50

# Close exactly 0.02 lots
dotnet run -- partial-close -p demo -t 123456 -v 0.02

# Dry-run
dotnet run -- partial-close -p demo -t 123456 -P 25 --dry-run
```

### PowerShell Shortcut (custom)

You can alias `partial-close` as `pc`:

```powershell
pc -t 123456 -P 50
# expands to: mt5 partial-close -p demo -t 123456 -P 50 --timeout-ms 90000
```

---

## Notes & Safety 🛡️

* If the requested `--volume` exceeds the current position volume, it is clamped to the maximum available.
* If the computed volume (from `--percent`) is ≤ 0, the command aborts safely.
* Symbol visibility is ensured before sending close request.
* Only works on **positions**, not pending orders. Use `cancel` for pending deletion.

---

## Code Reference 🧩

```csharp
var pcTicketOpt  = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket to partially close") { IsRequired = true };
var pcPercentOpt = new Option<int?>(new[] { "--percent", "-P" }, "Percent of current volume to close (1..100)");
var pcVolumeOpt  = new Option<double?>(new[] { "--volume", "-v" }, "Exact volume to close (lots)");

var pclose = new Command("partial-close", "Partially close a position by ticket");
pclose.AddAlias("pc");

pclose.AddOption(profileOpt);
pclose.AddOption(pcTicketOpt);
pclose.AddOption(pcPercentOpt);
pclose.AddOption(pcVolumeOpt);

pclose.SetHandler(async (string profile, ulong ticket, int? percent, double? volume, int timeoutMs, bool dryRun) =>
{
```
