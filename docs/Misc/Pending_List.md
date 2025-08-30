# Pending List (`pending list`) 📝

## What it Does 🎯

Shows all **pending orders** for the selected account/profile.
Useful to review stop/limit orders before modifying or cancelling them.

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                |
| ----------------- | ------ | -------- | ------------------------------------------ |
| `--profile`, `-p` | string | ✅        | Profile to use (from `profiles.json`).     |
| `--output`, `-o`  | string | ❌        | Output format: `text` (default) or `json`. |
| `--timeout-ms`    | int    | ❌        | RPC timeout (default: 30000).              |

---

## Output Fields ⬆️

Printed per pending order (based on proto definition):

| Field        | Type   | Description                                    |
| ------------ | ------ | ---------------------------------------------- |
| `Ticket`     | ulong  | Unique order ticket ID.                        |
| `Symbol`     | string | Target symbol (e.g., `EURUSD`).                |
| `Type`       | enum   | Pending order type (BuyLimit, SellStop, etc.). |
| `Volume`     | double | Order volume (lots).                           |
| `Price`      | double | Entry price.                                   |
| `StopLoss`   | double | Stop Loss (if set).                            |
| `TakeProfit` | double | Take Profit (if set).                          |
| `Expiration` | Date   | Expiration time if applicable.                 |

---

## How to Use 🛠️

### CLI

```powershell
# Show pending orders for default profile
dotnet run -- pending list -p demo

# JSON output
dotnet run -- pending list -p demo -o json
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
pdls     # expands to: mt5 pending list -p demo --timeout-ms 90000
```

---

## When to Use ❓

* To review all open pending orders before trading decisions.
* To fetch order tickets for later use in `pending.modify` or `pending.cancel`.
* To audit whether expiry times and prices are correct.

---

## Notes & Safety 🛡️

* Pending orders can expire automatically — list may be empty if none are active.
* Combine with `history` to check execution of expired or triggered orders.

---

## Code Reference (to be filled by you) 🧩

```csharp
var pending = new Command("pending", "Pending orders utilities");
pending.AddAlias("pd");

// pending list
var pendingList = new Command("list", "List pending order tickets");
pendingList.AddAlias("ls");

pendingList.AddOption(profileOpt);
pendingList.AddOption(outputOpt);

// we reuse the global timeout option added earlier
pendingList.SetHandler(async (string profile, string output, int timeoutMs) =>
{
    Validators.EnsureProfile(profile);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:PENDING/LIST Profile:{Profile}", profile))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            // This call returns both pending-order tickets and open-position tickets
            var tickets = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersTicketsAsync(deadline: null, cancellationToken: ct),
                opCts.Token);

            var pendingTickets  = tickets.OpenedOrdersTickets;     // pending orders
            // var positionTickets = tickets.OpenedPositionTickets; // not used here

            if (IsJson(output))
            {
                var dto = new
                {
                    count   = pendingTickets.Count,
                    tickets = pendingTickets.ToArray()
                };
                Console.WriteLine(ToJson(dto));
            }
            else
            {
                Console.WriteLine($"Pending orders: {pendingTickets.Count}");
                if (pendingTickets.Count > 20)
                    Console.WriteLine(string.Join(", ", pendingTickets.Take(20)) + " ...");
                else if (pendingTickets.Count > 0)
                    Console.WriteLine(string.Join(", ", pendingTickets));
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
}, profileOpt, outputOpt, timeoutOpt);

pending.AddCommand(pendingList);
root.AddCommand(pending);
```
