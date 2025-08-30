# Orders (`orders`) 📋

## What it Does 🎯

Lists all **currently opened pending orders** and **opened position tickets** for the selected profile/account.

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                |
| ----------------- | ------ | -------- | ------------------------------------------ |
| `--profile`, `-p` | string | Yes        | Profile from `profiles.json`.              |
| `--output`, `-o`  | string | No       | Output format: `text` (default) or `json`. |
| `--timeout-ms`    | int    | No        | RPC timeout in ms (default: `30000`).      |

---

## Output Fields ⬆️

### Text mode

```
Opened orders:   N
1111111, 2222222, ...
Opened positions:M
3333333, 4444444, ...
```

* Shows counts and lists tickets.
* For >20 items, prints first 20 and appends `...`.

### JSON mode

```json
{
  "OpenedOrdersTickets": [1111111, 2222222],
  "OpenedPositionTickets": [3333333, 4444444]
}
```

---

## How to Use 🛠️

### CLI

```powershell
# Default text mode
dotnet run -- orders -p demo

# JSON mode
dotnet run -- orders -p demo -o json
```

### PowerShell Shortcuts (from `ps/shortcasts.ps1`)

```powershell
ord      # expands to: mt5 orders -p demo --timeout-ms 90000
```

---

## Notes & Safety 🛡️

* This command **only returns ticket IDs**, not full order/position details. Use `ticket show` to query full info per ticket.
* If connection is down, output will be empty.
* Useful for scripts to quickly capture all currently active tickets and feed them to further operations (e.g. `close`, `modify`).

---

## Code Reference 🧩

```csharp
var orders = new Command("orders", "List open orders and positions tickets");
    orders.AddAlias("ord");

    orders.AddOption(profileOpt);
    orders.AddOption(outputOpt);
    orders.SetHandler(async (string profile, string output, int timeoutMs) =>
    {
        Validators.EnsureProfile(profile);
        _selectedProfile = profile;

        using (UseOpTimeout(timeoutMs))
        using (_logger.BeginScope("Cmd:ORDERS Profile:{Profile}", profile))
        {
            try
            {
                await ConnectAsync();
                using var opCts = StartOpCts();

                var tickets = await CallWithRetry(
                    ct => _mt5Account.OpenedOrdersTicketsAsync(deadline: null, cancellationToken: ct),
                    opCts.Token);

                if (IsJson(output)) Console.WriteLine(ToJson(tickets));
                else
                {
                    var o = tickets.OpenedOrdersTickets;
                    var p = tickets.OpenedPositionTickets;

                    Console.WriteLine($"Opened orders:   {o.Count}");
                    if (o.Count > 20) Console.WriteLine(string.Join(", ", o.Take(20)) + " ...");
                    else if (o.Count > 0) Console.WriteLine(string.Join(", ", o));

                    Console.WriteLine($"Opened positions:{p.Count}");
                    if (p.Count > 20) Console.WriteLine(string.Join(", ", p.Take(20)) + " ...");
                    else if (p.Count > 0) Console.WriteLine(string.Join(", ", p));
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
    root.AddCommand(orders);
```
