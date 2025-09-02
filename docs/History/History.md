# History (`history`) 🕰️

## What it Does

Fetches **account trading history** for the **last N days** and prints it in **text** or **JSON**.
Under the hood calls `_mt5Account.OrderHistoryAsync(from, to)` with `from = UtcNow - days`, `to = UtcNow`.

Default RPC args used by the wrapper:

* `sortMode = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc`
* `pageNumber = 0`, `itemsPerPage = 0` (no paging)

---

## Input Parameters ⬇️

| Parameter       | Type   | Required | Description                                         |
| --------------- | ------ | -------- | --------------------------------------------------- |
| `--profile, -p` | string | yes      | Profile to use (from `profiles.json`).              |
| `--output`      | string | no       | `text` (default) or `json`.                         |
| `--days, -d`    | int    | yes      | Number of days to look back. **Must be > 0**.       |
| `--timeout-ms`  | int    | no       | Per-RPC timeout in milliseconds (default: `30000`). |

---

## Output Fields ⬆️

Each history **item** is either an **Order** (has `HistoryOrder`) or a **Deal** (has `HistoryDeal`).

### Text mode (preview)

* Prints the **first 10 items** and a total count.
* `ORDER` line: `Ticket`, `Symbol`, `State`, `VolumeInitial`, `VolumeCurrent`, `PriceOpen`, `SetupTime`, `DoneTime`.
* `DEAL`  line: `Ticket`, `Symbol`, `Type`, `Volume`, `Price`, `Profit`, `Time`.

### JSON mode (full)

* Raw server payload (`OrdersHistoryData`) with a collection of items, each containing `HistoryOrder` **or** `HistoryDeal` objects from `mt5_term_api`.
* In addition to the above, the actual protocol models may contain additional fields (for example, comments, reasons, SL/TP, etc.) — they will be returned to JSON "as is" according to `mt5_term_api`.

---

## How to Use 🛠️

### CLI

```powershell
# Last 7 days (text)
dotnet run -- history -p demo --days 7

# Last 30 days (JSON)
dotnet run -- history -p demo --days 30 --output json --timeout-ms 60000
```

### PowerShell Shortcuts

```powershell
. .\\ps\\shortcasts.ps1
use-pf demo
h --days 14          # alias for `history`
```

---

## Code Reference 🧩 (без CallWithRetry)

```csharp
// Compute time window
var from = DateTime.UtcNow.AddDays(-Math.Abs(days));
var to   = DateTime.UtcNow;

// Fetch history snapshot
var res = await _mt5Account.OrderHistoryAsync(from, to);

if (IsJson(output))
{
    Console.WriteLine(ToJson(res)); // full server payload
}
else
{
    var items = res.HistoryData;
    Console.WriteLine($"History items: {items.Count}");
    foreach (var h in items.Take(10))
    {
        if (h.HistoryOrder is not null)
        {
            var o = h.HistoryOrder;
            var setup = o.SetupTime?.ToDateTime();
            var done  = o.DoneTime?.ToDateTime();
            Console.WriteLine($"ORDER  #{o.Ticket}  {o.Symbol}  state={o.State}  " +
                              $"vol={o.VolumeInitial}->{o.VolumeCurrent}  open={o.PriceOpen}  " +
                              $"setup={setup:O} done={done:O}");
        }
        else if (h.HistoryDeal is not null)
        {
            var d = h.HistoryDeal;
            var t = d.Time?.ToDateTime();
            Console.WriteLine($"DEAL   #{d.Ticket}  {d.Symbol}  type={d.Type}  " +
                              $"vol={d.Volume}  price={d.Price}  pnl={d.Profit}  time={t:O}");
        }
    }
}
```

### Method Signature

```csharp
public Task<OrdersHistoryData> OrderHistoryAsync(
    DateTime from,
    DateTime to,
    BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sortMode = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
    int pageNumber = 0,
    int itemsPerPage = 0,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```
