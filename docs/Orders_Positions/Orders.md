# Orders (`orders`) 📋

Lists all **currently opened pending order tickets** and **opened position tickets** for the selected profile/account.

---
## Method Signature

```csharp
public Task<OpenedOrdersTicketsData> OpenedOrdersTicketsAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

## Input Parameters

| Parameter         | Type   | Required | Description                                |
| ----------------- | ------ | -------- | ------------------------------------------ |
| `--profile`, `-p` | string | yes      | Profile from `profiles.json`.              |
| `--output`, `-o`  | string | no       | Output format: `text` (default) or `json`. |
| `--timeout-ms`    | int    | no       | RPC timeout in ms (default: `30000`).      |

---

## Output

### Text mode

```
Opened orders:   N
1111111, 2222222, ...
Opened positions: M
3333333, 4444444, ...
```

* Shows counts and lists tickets.
* For >20 items per bucket, prints first 20 then appends `...`.

### JSON mode

```json
{
  "OpenedOrdersTickets": [1111111, 2222222],
  "OpenedPositionTickets": [3333333, 4444444]
}
```

**Exit codes**

* `0` — success
* `1` — fatal error (printed via ErrorPrinter)

---

## How to Use

```powershell
# Default text mode
dotnet run -- orders -p demo

# JSON mode
dotnet run -- orders -p demo -o json
```

### PowerShell Shortcut (from `ps/shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
ord   # → mt5 orders -p demo --timeout-ms 90000
```

---

## Notes & Safety

* This command **returns ticket IDs only**. Use **[ticket show](../Misc/Ticket_Show.md)** to inspect details for a specific ticket.
* If there are no open items, both lists are empty and counts are `0`.
* Handy for scripts to quickly enumerate active tickets and feed them to further operations (`close`, `modify`, etc.).

---

## Proto

```proto
message OpenedOrdersTicketsData {
  repeated int64 opened_orders_tickets = 1;
  repeated int64 opened_position_tickets = 2;
}
```

---

## Code Reference (aligned with your handler)

```csharp
var orders = new Command("orders", "List open orders and positions tickets");
orders.AddAlias("ord");

orders.AddOption(profileOpt);
orders.AddOption(outputOpt);
orders.AddOption(timeoutOpt);

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

            var res = await _mt5Account.OpenedOrdersTicketsAsync();
            var orders = res.OpenedOrdersTickets?.Select(x => unchecked((ulong)x)).ToArray() ?? Array.Empty<ulong>();
            var positions = res.OpenedPositionTickets?.Select(x => unchecked((ulong)x)).ToArray() ?? Array.Empty<ulong>();

            if (IsJson(output))
            {
                Console.WriteLine(ToJson(new {
                    OpenedOrdersTickets = orders,
                    OpenedPositionTickets = positions
                }));
                return;
            }

            // text mode
            Console.WriteLine($"Opened orders:   {orders.Length}");
            if (orders.Length > 0)
            {
                var head = string.Join(", ", orders.Take(20));
                Console.WriteLine(orders.Length > 20 ? head + ", ..." : head);
            }

            Console.WriteLine($"Opened positions: {positions.Length}");
            if (positions.Length > 0)
            {
                var head = string.Join(", ", positions.Take(20));
                Console.WriteLine(positions.Length > 20 ? head + ", ..." : head);
            }
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { }
        }
    }
}, profileOpt, outputOpt, timeoutOpt);
```

---

## See also

* **[`ticket show`](../Misc/Ticket_Show.md)** — inspect a specific ticket (open or from recent history)
* **[`pending list`](../Misc/Pending_List.md)** — list pending orders with details
* **[`positions`](./Positions.md)** — list open positions with details
