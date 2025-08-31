# History (`history`) 🕰️

## What it Does

Fetches **account trading history** for the **last N days** from MT5 and prints it in **text** or **JSON**.
Under the hood it calls `_mt5Account.OrderHistoryAsync(from, to)` where `from = UtcNow - days` and `to = UtcNow`.

---

## Input Parameters ⬇️

| Parameter      | Type   | Required | Description                                       |
| -------------- | ------ | -------- | ------------------------------------------------- |
| `--profile`    | string | yes        | Profile to use (from `profiles.json`).            |
| `--output`     | string | no        | `text` (default) or `json`.                       |
| `--days`       | int    | yes        | Number of days to look back. **Must be > 0**.     |
| `--timeout-ms` | int    | no        | Per-RPC timeout in milliseconds (default: 30000). |

> Note: This command uses a **fixed time window** (`now - days` → `now`). There are no `--from/--to` or `--mode` switches in the current implementation.

---

## Output Fields ⬆️

Each history **item** is either an **Order** or a **Deal**. The CLI prints up to the first 10 items in text mode (with a "+N more" line if applicable). JSON returns the full payload from the server.

### Order fields (when `HistoryOrder` is present)

| Field           | Type      | Description                   |
| --------------- | --------- | ----------------------------- |
| `Ticket`        | int64     | Order/position ticket.        |
| `Symbol`        | string    | Instrument (e.g., `EURUSD`).  |
| `VolumeInitial` | double    | Initial lots.                 |
| `VolumeCurrent` | double    | Current lots at close.        |
| `PriceOpen`     | double    | Entry price.                  |
| `SetupTime`     | DateTime? | When the order was set up.    |
| `DoneTime`      | DateTime? | When the order was completed. |

### Deal fields (when `HistoryDeal` is present)

| Field    | Type      | Description       |
| -------- | --------- | ----------------- |
| `Ticket` | int64     | Deal ticket.      |
| `Symbol` | string    | Instrument.       |
| `Volume` | double    | Executed lots.    |
| `Price`  | double    | Execution price.  |
| `Profit` | double    | P/L of this deal. |
| `Time`   | DateTime? | Deal time.        |

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
. .\ps\shortcasts.ps1
use-pf demo
h --days 14          # alias for `history`
```

---

## Code Reference (exact) 🧩

```csharp
var history = new Command("history", "Orders/deals history for the last N days");
history.AddAlias("h");

history.AddOption(profileOpt);
history.AddOption(outputOpt);
history.AddOption(daysOpt);
history.SetHandler(async (string profile, string output, int days, int timeoutMs) =>
{
    Validators.EnsureProfile(profile);
    if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "Days must be > 0.");
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:HISTORY Profile:{Profile}", profile))
    using (_logger.BeginScope("Days:{Days}", days))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            var from = DateTime.UtcNow.AddDays(-Math.Abs(days));
            var to   = DateTime.UtcNow;

            var res = await CallWithRetry(
                ct => _mt5Account.OrderHistoryAsync(from, to, deadline: null, cancellationToken: ct),
                opCts.Token);

            if (IsJson(output)) Console.WriteLine(ToJson(res));
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
                        Console.WriteLine(
                            $"ORDER  #{o.Ticket}  {o.Symbol}...vol={o.VolumeInitial}->{o.VolumeCurrent}  open={o.PriceOpen} " +
                            $"setup={setup:O} done={done:O}");
                    }
                    else if (h.HistoryDeal is not null)
                    {
                        var d = h.HistoryDeal;
                        var t = d.Time?.ToDateTime();
                        Console.WriteLine(
                            $"DEAL   #{d.Ticket}  {d.Symbol}...  vol={d.Volume}  price={d.Price}  pnl={d.Profit}  time={t:O}");
                    }
                }
                if (items.Count > 10) Console.WriteLine($"... and {items.Count - 10} more");
            }
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
}, profileOpt, outputOpt, daysOpt, timeoutOpt);
root.AddCommand(history);
```
---

📌 In short:
— `history` = last-N-days orders & deals, one-shot snapshot.
— Text prints a concise preview (first 10), JSON returns full payload.
— Same profile/timeout pattern as the other commands.
