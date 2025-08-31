# Ticket (`ticket`) 🎫

## What it Does

Provides a group of commands to **work with a specific ticket**.
Tickets can represent **open positions**, **pending orders**, or **historical trades**.
---

## Input Parameters (generic) ⬇️

Most ticket sub‑commands share these core parameters:

| Parameter         | Type   | Description                      |
| ----------------- | ------ |  -------------------------------- |
| `--profile`, `-p` | string |  Profile from `profiles.json`.    |
| `--ticket`, `-t`  | ulong  |  Ticket ID to operate on.         |
| `--timeout-ms`    | int    |  RPC timeout (default: 30000).    |
| `--output`, `-o`  | string |  Output format: `text` or `json`. |

---

## Typical Uses 🛠️

* **Inspection** — check details of an order/position.
* **Modification** — adjust SL/TP or price (if supported).
* **Closing** — close position by ticket (if supported in your build).

---

## Example Usage

### CLI

```powershell
# Show info about a ticket
dotnet run -- ticket show -p demo -t 123456
```

### PowerShell Shortcut

```powershell
. .\ps\shortcasts.ps1
use-pf demo
tsh -t 123456
```

---

## Notes & Safety 🛡️

* Ticket IDs are **unique per account** — always select correct profile.
* For closed tickets, history lookback may be required (see `ticket show`).

---

## Code Reference 🧩

```csharp
// --- Helper: search ticket across aggregated collections (positions, pendings, etc.)
private static object? TryFindByTicketInAggregate(object openedAggregate, ulong ticket, out string? bucketName)
{
    bucketName = null;
    var t = openedAggregate.GetType();

    foreach (var p in t.GetProperties())
    {
        // only sequences (List/array/etc.); strings will be skipped as IEnumerable<char>
        if (!typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType)) continue;

        var seq = p.GetValue(openedAggregate) as System.Collections.IEnumerable;
        if (seq is null) continue;

        foreach (var item in seq)
        {
            if (item is null) continue;

            var it = item.GetType();
            var tp = it.GetProperty("Ticket") ?? it.GetProperty("ticket");
            if (tp is null) continue;

            var v = tp.GetValue(item);
            bool match =
                v is ulong ut && ut == ticket ||
                v is long  lt && unchecked((ulong)lt) == ticket;

            if (match)
            {
                bucketName = p.Name; // which collection the item came from
                return item;
            }
        }
    }
    return null;
}
```
**An example of usage inside the ticket** `show handler` 🧩

```csharp
// inside 'ticket show' handler, after ConnectAsync()
using var opCts = StartOpCts();

// 1) get current open aggregates (whatever your API returns)
var opened = await CallWithRetry(
    ct => _mt5Account.GetOpenedAggregateAsync(ct),
    opCts.Token);

// 2) try to locate in opened (positions / pendings)
if (TryFindByTicketInAggregate(opened, ticket, out var bucket) is { } found)
{
    if (IsJson(output)) Console.WriteLine(ToJson(found));
    else _logger.LogInformation("Ticket found in {Bucket}: {Item}", bucket, ToJson(found));
}
else
{
    // 3) fallback to recent history window
    var from = DateTime.UtcNow.AddDays(-Math.Abs(days));
    var to   = DateTime.UtcNow;

    var hist = await CallWithRetry(
        ct => _mt5Account.OrderHistoryAsync(from, to, deadline: null, cancellationToken: ct),
        opCts.Token);

    // history items may be union of orders/deals; do a simple scan by Ticket
    var item = hist.HistoryData.FirstOrDefault(h =>
    {
        var o = h.HistoryOrder;
        var d = h.HistoryDeal;
        return (o != null && (ulong)o.Ticket == ticket) ||
               (d != null && (ulong)d.Ticket == ticket);
    });

    if (item != null)
    {
        if (IsJson(output)) Console.WriteLine(ToJson(item));
        else _logger.LogInformation("Ticket found in HISTORY: {Item}", ToJson(item));
    }
    else
    {
        _logger.LogWarning("Ticket {Ticket} not found in opened nor in the last {Days}d history.", ticket, days);
        Environment.ExitCode = 2;
    }
}
```
