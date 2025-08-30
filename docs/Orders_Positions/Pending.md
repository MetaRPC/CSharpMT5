# Pending (`pending`) 🕒

## What it Does 🎯

Provides utilities for working with **pending orders**. Currently supports listing tickets of all open pending orders.

---

## Subcommands 📂

### `pending list` (`pdls`)

Lists tickets of all current pending orders for the selected profile/account.

#### Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                |
| ----------------- | ------ | -------- | ------------------------------------------ |
| `--profile`, `-p` | string | ✅        | Profile from `profiles.json`.              |
| `--output`, `-o`  | string | ❌        | Output format: `text` (default) or `json`. |
| `--timeout-ms`    | int    | ❌        | RPC timeout in ms (default: `30000`).      |

#### Output Fields ⬆️

**Text mode**:

```
Pending orders: N
1111111, 2222222, ...
```

* Shows ticket IDs. If >20, prints first 20 + `...`.

**JSON mode**:

```json
{
  "count": 2,
  "tickets": [1111111, 2222222]
}
```

---

## How to Use 🛠️

### CLI

```powershell
# Default text output
dotnet run -- pending list -p demo

# JSON output
dotnet run -- pending list -p demo -o json
```

### PowerShell Shortcuts (from `ps/shortcasts.ps1`)

```powershell
pdls   # expands to: mt5 pending list -p demo --timeout-ms 90000
```

---

## Notes & Safety 🛡️

* Returns **only pending orders**, not active positions.
* For full order details, use `ticket show -t <id>`.
* Useful for automation: quickly collect all open pending tickets and feed them to `cancel` or `pending.modify`.

---

## Code Reference 🧩

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
