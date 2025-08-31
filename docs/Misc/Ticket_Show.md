# Ticket Show (`ticket show`) 🎫

## What it Does

Displays full **info for a specific ticket** — either an **open trade** or a **recently closed order** (within history range).

---

## Input Parameters ⬇️

| Parameter         | Type   | Description                                    |
| ----------------- | ------ | ---------------------------------------------- |
| `--profile`, `-p` | string | Profile from `profiles.json`.                  |
| `--ticket`, `-t`  | ulong  |  Ticket ID to inspect.                          |
| `--days`, `-d`    | int    |  History lookback window in days (default: 30). |
| `--timeout-ms`    | int    |  RPC timeout (default: 30000).                  |
| `--output`, `-o`  | string |  Output format: `text` (default) or `json`.     |

---

## Output Fields ⬆️

Depending on ticket type (open vs closed):

| Field        | Type   | Description                  |
| ------------ | ------ | ---------------------------- |
| `Ticket`     | ulong  | Ticket number.               |
| `Symbol`     | string | Symbol name.                 |
| `Side`       | string | `BUY` or `SELL`.             |
| `Volume`     | double | Trade volume (lots).         |
| `OpenPrice`  | double | Price of entry.              |
| `ClosePrice` | double | Price of exit (if closed).   |
| `SL`         | double | Stop Loss (if set).          |
| `TP`         | double | Take Profit (if set).        |
| `Commission` | double | Commission charged.          |
| `Swap`       | double | Swap charged.                |
| `Profit`     | double | Profit or loss.              |
| `OpenTime`   | Date   | Time of opening.             |
| `CloseTime`  | Date   | Time of closing (if closed). |

---

## How to Use 🛠️

### CLI

```powershell
# Inspect an open or recent ticket
dotnet run -- ticket show -p demo -t 123456

# With history lookback of 7 days
dotnet run -- ticket show -p demo -t 123456 -d 7

# JSON output
dotnet run -- ticket show -p demo -t 123456 -o json
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
tsh -t 123456 -d 7
# expands to: mt5 ticket show -p demo -t 123456 -d 7 --timeout-ms 90000
```

---

## When to Use ❓

* To check **details of an open position** directly by ticket.
* To retrieve **recently closed order info** without parsing full history.
* To confirm P/L, SL/TP, and execution prices for audits.

---

## Notes & Safety 🛡️

* Lookback `--days` matters: if ticket closed long ago, it may not appear.
* Ensure `profiles.json` points to correct account — ticket IDs are per account.

---

## Code Reference 🧩

```csharp
var ticketCmd = new Command("ticket", "Work with a specific ticket");
ticketCmd.AddAlias("t");

// ticket show
var tShow = new Command("show", "Show info for the ticket (open or from recent history)");
tShow.AddAlias("sh");

var tOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Ticket id") { IsRequired = true };
var tDaysOpt = new Option<int>(new[] { "--days", "-d" }, () => 30, "If not open, search in last N days history");

tShow.AddOption(profileOpt);
tShow.AddOption(outputOpt);
tShow.AddOption(tOpt);
tShow.AddOption(tDaysOpt);

tShow.SetHandler(async (string profile, string output, ulong ticket, int days, int timeoutMs) =>
{
    Validators.EnsureProfile(profile);
    if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "Days must be > 0.");
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:TICKET-SHOW Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket}", ticket))
    {
        try
        {
            await ConnectAsync();
```
