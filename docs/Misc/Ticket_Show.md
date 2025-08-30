# Ticket Show (`ticket show`) 🎫

## What it Does 🎯

Displays full **info for a specific ticket** — either an **open trade** or a **recently closed order** (within history range).

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                    |
| ----------------- | ------ | -------- | ---------------------------------------------- |
| `--profile`, `-p` | string | ✅        | Profile from `profiles.json`.                  |
| `--ticket`, `-t`  | ulong  | ✅        | Ticket ID to inspect.                          |
| `--days`, `-d`    | int    | ❌        | History lookback window in days (default: 30). |
| `--timeout-ms`    | int    | ❌        | RPC timeout (default: 30000).                  |
| `--output`, `-o`  | string | ❌        | Output format: `text` (default) or `json`.     |

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

## Code Reference (to be filled by you) 🧩

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

            // 1) check open sets (orders/positions)
            using var opCts = StartOpCts();
            var openTickets = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersTicketsAsync(deadline: null, cancellationToken: ct),
                opCts.Token);

            bool isOpenOrder    = openTickets.OpenedOrdersTickets.Contains((long)ticket);
bool isOpenPosition = openTickets.OpenedPositionTickets.Contains((long)ticket);


            // 2) try to get full object from opened aggregate
            using var aggCts = StartOpCts();
            var openedAgg = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                aggCts.Token);

            var obj = TryFindByTicketInAggregate(openedAgg, ticket, out var bucket);

            if (obj is not null)
            {
                var symbol = Get<string>(obj, "Symbol");
                var volume = Get<double?>(obj, "Volume") ?? 0.0;
                var price  = Get<double?>(obj, "PriceOpen", "OpenPrice", "Price") ?? 0.0;
                var sl     = Get<double?>(obj, "StopLoss", "SL");
                var tp     = Get<double?>(obj, "TakeProfit", "TP");
                var profit = Get<double?>(obj, "Profit");

                if (IsJson(output))
                {
                    Console.WriteLine(ToJson(new {
                        ticket,
                        state = isOpenPosition ? "position-open" : isOpenOrder ? "pending-open" : "open-unknown",
                        bucket,
                        symbol,
                        volume,
                        priceOpen = price,
                        sl,
                        tp,
                        profit
                    }));
                }
                else
                {
                    Console.WriteLine($"Ticket #{ticket}  [{(isOpenPosition ? "POSITION" : isOpenOrder ? "PENDING" : bucket)}]");
                    Console.WriteLine($"  Symbol: {symbol}");
                    Console.WriteLine($"  Volume: {volume}");
                    Console.WriteLine($"  Price:  {price}");
                    if (sl.HasValue) Console.WriteLine($"  SL:     {sl}");
                    if (tp.HasValue) Console.WriteLine($"  TP:     {tp}");
                    if (profit.HasValue) Console.WriteLine($"  PnL:    {profit}");
                }

                try { await _mt5Account.DisconnectAsync(); } catch { }
                return;
            }

            // 3) not found in open -> search in history
            using var hCts = StartOpCts();
            var from = DateTime.UtcNow.AddDays(-Math.Abs(days));
            var to   = DateTime.UtcNow;
            var hist = await CallWithRetry(
                ct => _mt5Account.OrderHistoryAsync(from, to, deadline: null, cancellationToken: ct),
                hCts.Token);

            var item = hist.HistoryData.FirstOrDefault(h =>
                (h.HistoryOrder?.Ticket ?? 0UL) == ticket ||
                (h.HistoryDeal?.Ticket  ?? 0UL) == ticket);

            if (item is null)
            {
                if (IsJson(output)) Console.WriteLine(ToJson(new { ticket, found = false }));
                else Console.WriteLine($"Ticket #{ticket} not found in open sets or last {days} days.");
                Environment.ExitCode = 2;
            }
            else
            {
                if (item.HistoryOrder is not null)
                {
                    var o = item.HistoryOrder;
                    var setup = o.SetupTime?.ToDateTime();
                    var done  = o.DoneTime?.ToDateTime();

                    if (IsJson(output))
                    {
                        Console.WriteLine(ToJson(new {
                            ticket,
                            type = "order-history",
                            o.Symbol, o.State, o.VolumeInitial, o.VolumeCurrent, o.PriceOpen, setup, done
                        }));
                    }
                    else
                    {
                        Console.WriteLine($"Ticket #{ticket} [ORDER history]");
                        Console.WriteLine($"  {o.Symbol} state={o.State} vol={o.VolumeInitial}->{o.VolumeCurrent} open={o.PriceOpen}");
                        Console.WriteLine($"  setup={setup:O}  done={done:O}");
                    }
                }
                else if (item.HistoryDeal is not null)
                {
                    var d = item.HistoryDeal;
                    var t = d.Time?.ToDateTime();

                    if (IsJson(output))
                    {
                        Console.WriteLine(ToJson(new {
                            ticket,
                            type = "deal-history",
                            d.Symbol, d.Type, d.Volume, d.Price, d.Profit, t
                        }));
                    }
                    else
                    {
                        Console.WriteLine($"Ticket #{ticket} [DEAL history]");
                        Console.WriteLine($"  {d.Symbol} type={d.Type} vol={d.Volume} price={d.Price} pnl={d.Profit} time={t:O}");
                    }
                }
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
}, profileOpt, outputOpt, tOpt, tDaysOpt, timeoutOpt);

ticketCmd.AddCommand(tShow);
root.AddCommand(ticketCmd);
```
