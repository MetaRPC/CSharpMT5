# Ticket Show (`ticket show`) 🎫

## What it Does

Displays full **info for a specific ticket** — first tries **open sets** (positions/pendings); if not found, searches **recent history** (last *N* days).

> Group: `ticket` (alias `t`). Subcommand: `show` (alias `sh`).

> Need the internal flow and proto details? See **Specific\_Ticket.md**.

---
## Method Signatures (quick ref) 🧩

> Full details live in [Specific_Ticket.md](./Specific_Ticket.md). This page is a user-facing overview.

```csharp
public Task<OpenedOrdersTicketsData> OpenedOrdersTicketsAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public Task<OrdersHistoryData> OrderHistoryAsync(
    DateTime from,
    DateTime to,
    BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sortMode = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
    int pageNumber = 0,
    int itemsPerPage = 0,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

## Input Parameters ⬇️

| Parameter       | Type   | Required | Description                                         |
| --------------- | ------ | -------- | --------------------------------------------------- |
| `--profile, -p` | string | yes      | Profile from `profiles.json`.                       |
| `--ticket, -t`  | ulong  | yes      | Ticket ID to inspect.                               |
| `--days, -d`    | int    | no       | History lookback if not found open (default: `30`). |
| `--output, -o`  | string | no       | `text` (default) or `json`.                         |
| `--timeout-ms`  | int    | no       | Per‑RPC timeout (default: `30000`).                 |

---

## Output ⬆️ (what the current handler prints)

**Open (position/pending)**

* `Symbol`, `Volume`, `Price` (open), optional `SL` / `TP`, optional `Profit`, and bucket tag: `POSITION` or `PENDING`.

**History (order)**

* `Symbol`, `State`, `VolumeInitial→VolumeCurrent`, `PriceOpen`, timestamps `setup` / `done`.

**History (deal)**

* `Symbol`, `Type`, `Volume`, `Price`, `Profit`, `time`.

> Fields like `Side`, `ClosePrice`, `Commission`, `Swap` exist in proto but are **not printed** by the current handler. Extend printing if needed.

---

## How to Use 🛠️

### CLI

```powershell
# Inspect an open or recent ticket
dotnet run -- ticket show -p demo -t 123456

# With 7‑day history fallback
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

## Notes & Safety 🛡️

* `--days` matters: closed long ago → may not show up.
* Ticket IDs are **per account** — ensure the correct profile.
* Full proto mapping and internal flow are documented in **Specific\_Ticket.md**.

---

## Proto Field Mapping (summary) 🧬

**Open — Position** (`PositionInfo`)

* `Symbol` ← `PositionInfo.symbol`
* `Volume` ← `PositionInfo.volume`
* `Price` (open) ← `PositionInfo.price_open`
* `SL` ← `PositionInfo.stop_loss`
* `TP` ← `PositionInfo.take_profit`
* `Profit` ← `PositionInfo.profit`

**Open — Pending** (`OpenedOrderInfo`)

* `Symbol` ← `OpenedOrderInfo.symbol` *(if present in your build)*
* `Volume` ← `OpenedOrderInfo.volume_current` *(or `volume_initial`)*
* `Price` (entry) ← `OpenedOrderInfo.price_open`
* `SL` ← `OpenedOrderInfo.stop_loss`
* `TP` ← `OpenedOrderInfo.take_profit`
* `Expiration` ← `OpenedOrderInfo.time_expiration`

**History — Order** (`OrderHistoryData`)

* `Symbol` ← `OrderHistoryData.symbol`
* `State` ← `OrderHistoryData.state`
* `VolumeInitial→VolumeCurrent` ← `volume_initial` → `volume_current`
* `PriceOpen` ← `OrderHistoryData.price_open`
* `setup/done` ← `setup_time` / `done_time`

**History — Deal** (`DealHistoryData`)

* `Symbol` ← `DealHistoryData.symbol`
* `Type` ← `DealHistoryData.type`
* `Volume` ← `DealHistoryData.volume`
* `Price` ← `DealHistoryData.price`
* `Profit` ← `DealHistoryData.profit`
* `time` ← `DealHistoryData.time`

> See **Specific\_Ticket.md** → *Proto Reference* for full message/enums.

---

## Code Reference 🧩

```csharp
var ticketCmd = new Command("ticket", "Work with a specific ticket");
ticketCmd.AddAlias("t");

var tShow = new Command("show", "Show info for the ticket (open or from recent history)");
tShow.AddAlias("sh");

var tOpt    = new Option<ulong>(new[] { "--ticket", "-t" }, "Ticket id") { IsRequired = true };
var tDaysOpt= new Option<int>(new[] { "--days", "-d" }, () => 30, "If not open, search in last N days history");

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
        await ConnectAsync();
        // Lookup flow is described in Specific_Ticket.md (tickets → aggregate → history)
    }
}, profileOpt, outputOpt, tOpt, tDaysOpt, timeoutOpt);

ticketCmd.AddCommand(tShow);
root.AddCommand(ticketCmd);
```
