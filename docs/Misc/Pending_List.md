# Pending List (`pending list`) 📝

## What it Does

Shows all **pending orders** for the selected account/profile. Handy to review stop/limit orders before modifying or cancelling.

> Subcommand of **`pending`**. Invoke as `pending list` (alias: `ls`).

---
## Method Signature 🧩

```csharp
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

## Input Parameters ⬇️

| Parameter       | Type   | Required | Description                            |
| --------------- | ------ | -------- | -------------------------------------- |
| `--profile, -p` | string | yes      | Profile to use (from `profiles.json`). |
| `--output, -o`  | string | no       | `text` (default) or `json`.            |
| `--timeout-ms`  | int    | no       | Per-RPC timeout (default: `30000`).    |

---

## Output ⬆️

**Text mode**

```
Pending orders (<N>):
#<ticket>  <type>  <symbol>  vol=<lots>  price=<p>  SL=<sl>  TP=<tp>  exp=<iso-or-–>
...
```

**JSON mode**

* Raw payload: array of pending entries from `OpenedOrdersData.PendingInfos` (exact fields depend on your proto).

Typical fields per item (may vary by broker/proto):

* `Ticket` (ulong), `Symbol` (string), `Type` (enum), `Volume` (double),
* `Price` / `Stop` / `Limit` (depending on order kind), `StopLoss`, `TakeProfit`, `Expiration`.

---

## How to Use 🛠️

```powershell
# Text
dotnet run -- pending list -p demo

# JSON
dotnet run -- pending list -p demo -o json
```

### PowerShell Shortcuts

```powershell
. .\\ps\\shortcasts.ps1
use-pf demo
pdls  # expands to: mt5 pending list -p demo --timeout-ms 90000
```

---

## Notes & Safety 🛡️

* List may be empty if all orders were filled or expired.
* For editing/moving: use `pending.modify` / `pending.move`.
* For cancellation: see `pending cancel` / `cancel` (ticket-based).

---

## Code Reference 🧷 (short)

```csharp
await ConnectAsync();

var opened = await _mt5Account.OpenedOrdersAsync();
var pendings = opened.PendingInfos;

if (IsJson(output))
{
    Console.WriteLine(ToJson(pendings));
}
else
{
    if (pendings.Count == 0) { Console.WriteLine("No pending orders."); return; }
    Console.WriteLine($"Pending orders ({pendings.Count}):");
    foreach (var p in pendings.Take(50))
        Console.WriteLine($"#{p.Ticket}  {p.Type}  {p.Symbol}  vol={p.Volume}  price={p.Price}  SL={p.StopLoss}  TP={p.TakeProfit}  exp={p.Expiration}");
}
```

* `OpenedOrdersData` contains both `PositionInfos` and `PendingInfos`.
* `pending list` uses the `PendingInfos` collection.
* `sortMode` is always left at the default (`ByOpenTimeAsc`) in CLI.
