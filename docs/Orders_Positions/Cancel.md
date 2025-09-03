# Cancel (`cancel`) 🗑️

Cancels a **pending order by ticket** on the selected MT5 account/profile. Use when you want to remove a single Buy/Sell Limit/Stop (including *Stop‑Limit* variants) without touching other orders.

> This command targets **pending** orders only. For closing open positions, use **[close](../Market_Data/Close.md)**.

---

## Method Signatures 

> Full details live in the MT5Account service. These are the calls the command uses.

```csharp
// (optional) aggregate snapshot to validate the ticket & symbol
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// cancel a pending order by its ticket
public Task CancelPendingOrderAsync(
    ulong ticket,
    CancellationToken cancellationToken);
```

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                 |
| ----------------- | ------ | -------- | ------------------------------------------- |
| `--profile`, `-p` | string | yes      | Profile from `profiles.json`.               |
| `--ticket`, `-t`  | ulong  | yes      | **Pending** order ticket to cancel.         |
| `--symbol`, `-s`  | string | yes      | Safety filter: symbol **must** match order. |
| `--timeout-ms`    | int    | no       | RPC timeout in ms (default: `30000`).       |
| `--dry-run`       | flag   | no       | Print intended action; no request sent.     |

> **Note:** This command is **text‑only**; `--output` is not supported.

---

## Output ⬆️

**Text only.**

* Success: `✔ cancel done: #<ticket> <SYMBOL>`
* Dry‑run: `[DRY-RUN] CANCEL pending #<ticket> <SYMBOL>`
* Not found / not pending / symbol mismatch:
  `Pending order #<ticket> not found.` **or** `Ticket #<ticket> does not belong to <SYMBOL>.`

**Exit codes**

* `0` — success
* `2` — not found / not pending / symbol mismatch / validation guard
* `1` — fatal error (printed via ErrorPrinter)

---

## How to Use 🛠️

### CLI

```powershell
# Cancel a pending order by ticket (with symbol safety)
dotnet run -- cancel -p demo -t 123456 -s EURUSD

# Dry‑run (no request will be sent)
dotnet run -- cancel -p demo -t 123456 -s EURUSD --dry-run
```

### PowerShell Shortcuts (from `ps/shortcasts.ps1`)

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
* Before moving/re‑placing a pending at a new price (instead of `pending.move`).

---

## Notes & Safety 🛡️

* Verify the **ticket really refers to a pending order**; brokers reject cancel for already‑filled/expired tickets.
* `--symbol` is an intentional safety guard: the command validates the ticket’s symbol via the open aggregate before cancel.
* Combine with **[pending list](../Misc/Pending_List.md)** to find tickets, or with **[ticket show](../Misc/Ticket_Show.md)** to inspect details first.

---

## Code Reference 🧩

> This is illustrative. Your actual wiring uses System.CommandLine and common helpers.

```csharp
var cancel = new Command("cancel", "Cancel (delete) pending order by ticket");
cancel.AddAlias("x");

var cancelTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Pending order ticket") { IsRequired = true };
var cancelSymbolOpt = new Option<string>(new[] { "--symbol", "-s" }, "Symbol (e.g., EURUSD)") { IsRequired = true };

cancel.AddOption(profileOpt);
cancel.AddOption(cancelTicketOpt);
cancel.AddOption(cancelSymbolOpt);
cancel.AddOption(timeoutOpt);
cancel.AddOption(dryRunOpt);

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

        await ConnectAsync();
        // optional pre-check via aggregate (OpenedOrdersAsync) to assert ticket+symbol
        await _mt5Account.CancelPendingOrderAsync(ticket, CancellationToken.None);
        Console.WriteLine($"✔ cancel done: #{ticket} {symbol}");
    }
}, profileOpt, cancelTicketOpt, cancelSymbolOpt, timeoutOpt, dryRunOpt);
```

---

## See also 🔗

* **[pending list](../Misc/Pending_List.md)** — enumerate current pendings
* **[pending.modify](../Market_Data/Pending.modify.md)** — edit pending parameters
* **[pending.move](../Market_Data/Pending.move.md)** — shift pending by ±points
* **[ticket show](../Misc/Ticket_Show.md)** — inspect a ticket (open/history)
