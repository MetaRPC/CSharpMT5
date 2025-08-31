# Cancel (`cancel`) 🗑️

## What it Does

Cancels a **pending order by ticket** on the selected MT5 account/profile.
Use when you want to remove a single Buy/Sell Limit/Stop (including *Limit* variants) without touching other orders.

> This command targets **pending** orders only. For closing open positions, use `close`.

---

## Input Parameters ⬇️

| Parameter         | Type   | Description                                    |
| ----------------- | ------ |---------------------------------------------- |
| `--profile`, `-p` | string | Profile from `profiles.json`.                  |
| `--ticket`, `-t`  | ulong  | **Pending order** ticket to cancel.            |
| `--symbol`, `-s`  | string | Optional symbol filter (safety guard).         |
| `--output`, `-o`  | string | `text` (default) or `json`.                    |
| `--timeout-ms`    | int    | RPC timeout in ms (default: 30000).            |
| `--dry-run`       | flag   | Print intended action without sending request. |

---

## Output Fields ⬆️

| Field    | Type   | Description                             |
| -------- | ------ | --------------------------------------- |
| `Ticket` | ulong  | Canceled ticket.                        |
| `Symbol` | string | Symbol of the order.                    |
| `Type`   | string | Pending type (BuyLimit, SellStop, ...). |
| `Status` | string | `OK` or error description.              |

---

## How to Use 🛠️

### CLI

```powershell
# Cancel a pending order by ticket
dotnet run -- cancel -p demo -t 123456

# JSON output
dotnet run -- cancel -p demo -t 123456 -o json

# With symbol safety filter
dotnet run -- cancel -p demo -t 123456 -s EURUSD

# Dry-run (no request will be sent)
dotnet run -- cancel -p demo -t 123456 --dry-run
```

### PowerShell Shortcuts (from `shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
x -t 123456 -s EURUSD
# expands to: mt5 cancel -p demo -t 123456 -s EURUSD --timeout-ms 90000
```

---

## When to Use ❓

* To remove a **single** pending order quickly.
* As part of a cleanup routine without affecting other tickets.
* Before moving/re-placing a pending at a new price (instead of `pending.move`).

---

## Notes & Safety 🛡️

* Verify the **ticket really refers to a pending order**; brokers reject cancel for already-filled/expired tickets.
* `--symbol` is optional but helps avoid canceling a ticket on the wrong instrument.
* Combine with `pending list` to find tickets, or with `ticket show` to inspect details first.

---

## Code Reference 🧩

```csharp
var cancel = new Command("cancel", "Cancel (delete) pending order by ticket");
cancel.AddAlias("x");

var cancelTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Pending order ticket") { IsRequired = true };
var cancelSymbolOpt = new Option<string>(new[] { "--symbol", "-s" }, "Symbol (e.g., EURUSD)") { IsRequired = true };

cancel.AddOption(profileOpt);
cancel.AddOption(cancelTicketOpt);
cancel.AddOption(cancelSymbolOpt);

cancel.SetHandler(async (string profile, ulong ticket, string symbol, int timeoutMs, bool dryRun) =>
{
    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    symbol = Validators.EnsureSymbol(symbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CANCEL Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} Symbol:{Symbol}", ticket, symbol))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] CANCEL pending #{ticket} {symbol}");
            return;
        }

        try
        {
            await ConnectAsync();
```
