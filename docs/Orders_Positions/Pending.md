# Pending (`pending`) 🕒

Utilities for working with **pending orders**. The group currently exposes:

* **`pending list`** — list current **pending orders with details** (type, symbol, price, SL/TP, expiration).
* See also: **[`Pending.modify`](../Market_Data/Pending.modify.md)** and **[`Pending.move`](../Market_Data/Pending.move.md)** for editing/moving pendings.

> ℹ️ If you only need **ticket IDs**, use the top‑level **[`orders`](../Orders_Positions/Orders.md)** command (it prints tickets for both pendings and positions). The built‑in `pending list` here prints **detailed rows**, not tickets‑only.

---

## Subcommand: `pending list` (`ls`)

Lists **all current pending orders** for the selected profile/account.

### Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                |
| ----------------- | ------ | -------- | ------------------------------------------ |
| `--profile`, `-p` | string | yes      | Profile from `profiles.json`.              |
| `--output`, `-o`  | string | no       | Output format: `text` (default) or `json`. |
| `--timeout-ms`    | int    | no       | RPC timeout in ms (default: `30000`).      |

> PowerShell shortcut in `ps/shortcasts.ps1`: **`pdls`** expands to `mt5 pending list -p <profile> --timeout-ms 90000`.

### Output ⬆️

**Text mode** (preview up to 50 rows):

```
Pending orders (N):
#<ticket>  <type>  <symbol>  vol=<lots>  price=<p>  SL=<sl>  TP=<tp>  exp=<iso-or-–>
...
```

**JSON mode**:

* Raw payload: array from `OpenedOrdersData.PendingInfos` (fields depend on your proto/build).

### Notes & Safety

* List may be empty if all orders were filled/expired.
* For cancellation: use **[`Cancel`](../Orders_Positions/Cancel.md)** or **[`Cancel_All`](../Orders_Positions/Cancel_All.md)**.
* For precise price shifts by points: **[`Pending.move`](../Market_Data/Pending.move.md)**.

---

## Method Signature

```csharp
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

### Proto reference

```proto
message OpenedOrdersData {
  repeated OpenedOrderInfo opened_orders = 1;
  repeated PositionInfo    position_infos = 2;
}
```

---

## Code Reference 🧩

```csharp
await ConnectAsync();

var opened   = await _mt5Account.OpenedOrdersAsync();
var pendings = opened.PendingInfos; // your build exposes this collection

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

---

## See also

* **[`Pending_List`](../Misc/Pending_List.md)** — standalone page for this subcommand (user‑facing)
* **[`Pending.modify`](../Market_Data/Pending.modify.md)** — set exact prices/SL/TP/TIF
* **[`Pending.move`](../Market_Data/Pending.move.md)** — shift prices by ±N points
* **[`Cancel`](../Orders_Positions/Cancel.md)**, **[`Cancel_All`](../Orders_Positions/Cancel_All.md)**
* **[`Ticket_Show`](../Misc/Ticket_Show.md)** — inspect a specific ticket
