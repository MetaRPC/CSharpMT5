# History (`history`) 🕰️

## What it Does 🎯

Fetches **account trading history** from MT5 and prints it in **text** or **JSON**.
Typical use: audit past trades, generate P/L reports, or feed analytics.

---

## Input Parameters ⬇️

| Parameter      | Type   | Required | Description                                              |
| -------------- | ------ | -------- | -------------------------------------------------------- |
| `--profile`    | string | ✅        | Which profile to use (from `profiles.json`).             |
| `--output`     | string | ❌        | `text` (default) or `json`.                              |
| `--timeout-ms` | int    | ❌        | Per-RPC timeout in milliseconds (default: 30000).        |
| `--from`       | string | ❌        | Start time (UTC). ISO-8601, e.g. `2025-08-01T00:00:00Z`. |
| `--to`         | string | ❌        | End time (UTC). ISO-8601, e.g. `2025-08-31T23:59:59Z`.   |
| `--symbol`     | string | ❌        | Filter by symbol (e.g. `EURUSD`).                        |
| `--mode`       | string | ❌        | `orders` or `deals` (default: `orders`).                 |
| `--limit`      | int    | ❌        | Max items to return (server permitting).                 |

> If `--from/--to` are omitted, the command typically uses a **recent window** (e.g. last 7–30 days). Adjust to your needs.

---

## Output Fields ⬆️

### Orders (closed positions)

| Field        | Type     | Description                                  |
| ------------ | -------- | -------------------------------------------- |
| `Ticket`     | int64    | Order/position ticket.                       |
| `Symbol`     | string   | Instrument (e.g., `EURUSD`).                 |
| `Type`       | string   | BUY/SELL (or position side).                 |
| `Volume`     | double   | Lots.                                        |
| `OpenTime`   | DateTime | Time the position opened (UTC).              |
| `OpenPrice`  | double   | Entry price.                                 |
| `CloseTime`  | DateTime | Time the position closed (UTC).              |
| `ClosePrice` | double   | Exit price.                                  |
| `SL` / `TP`  | double   | StopLoss / TakeProfit at close (if present). |
| `Profit`     | double   | Net P/L of the position.                     |
| `Commission` | double   | Total commission.                            |
| `Swap`       | double   | Accrued swaps.                               |
| `Comment`    | string   | Order comment (if any).                      |
| `Magic`      | int      | EA magic number (if used).                   |

### Deals (executions)

| Field        | Type     | Description                      |
| ------------ | -------- | -------------------------------- |
| `DealId`     | int64    | Unique deal id.                  |
| `OrderId`    | int64    | Parent order id.                 |
| `Symbol`     | string   | Instrument.                      |
| `Type`       | string   | Buy/Sell/Balance/Commission/etc. |
| `Volume`     | double   | Lots executed.                   |
| `Price`      | double   | Execution price.                 |
| `Time`       | DateTime | Deal time (UTC).                 |
| `Profit`     | double   | Profit of this deal only.        |
| `Commission` | double   | Commission for this deal.        |
| `Swap`       | double   | Swap on this deal.               |
| `Comment`    | string   | Comment (if any).                |

> Exact field names depend on your protobuf models; the table reflects the common set used in MT5 APIs.

---

## How to Use 🛠️

### CLI

```powershell
# Last month, closed orders
dotnet run -- history -p demo --from 2025-08-01T00:00:00Z --to 2025-08-31T23:59:59Z --mode orders --output json

# Recent deals for EURUSD, max 100 rows
dotnet run -- history -p demo --symbol EURUSD --mode deals --limit 100 --output text
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
# last 7 days orders
history --from (Get-Date).AddDays(-7).ToUniversalTime().ToString("o") --mode orders --output json
```

---

## When to Use ❓

* **Reporting** — export P/L for a period or by symbol.
* **Reconciliation** — match orders vs deals, audit commission & swaps.
* **Analytics** — feed downstream systems (pandas, BI, dashboards).

---

## Code Reference 🧩

```csharp
// Parse window & filters
var fromUtc = DateTime.Parse(fromStr, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
var toUtc   = DateTime.Parse(toStr,   null, System.Globalization.DateTimeStyles.AdjustToUniversal);

using var opCts = StartOpCts();
if (string.Equals(mode, "deals", StringComparison.OrdinalIgnoreCase))
{
    var deals = await CallWithRetry(
        ct => _mt5Account.HistoryDealsAsync(fromUtc, toUtc, symbol, limit, ct),
        opCts.Token);
    if (IsJson(output)) Console.WriteLine(ToJson(deals));
    else foreach (var d in deals.Items)
        _logger.LogInformation("{Time} {Symbol} {Type} vol={Vol} price={Price} profit={Profit}", d.Time, d.Symbol, d.Type, d.Volume, d.Price, d.Profit);
}
else
{
    var orders = await CallWithRetry(
        ct => _mt5Account.HistoryOrdersAsync(fromUtc, toUtc, symbol, limit, ct),
        opCts.Token);
    if (IsJson(output)) Console.WriteLine(ToJson(orders));
    else foreach (var o in orders.Items)
        _logger.LogInformation("{Open}→{Close} {Symbol} {Type} vol={Vol} PnL={Profit}", o.OpenTime, o.CloseTime, o.Symbol, o.Type, o.Volume, o.Profit);
}
```

---

📌 In short:
— `history` = flexible export of past **orders** or **deals** with time window & filters.
— Outputs clean **text** or **JSON** for downstream processing.
— Same profile/timeout patterns as the other commands.

