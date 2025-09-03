# Positions (`positions`) 📈

Lists all **active (open) positions** for the selected profile/account.

Alias: `pos`

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                |
| ----------------- | ------ | -------- | ------------------------------------------ |
| `--profile`, `-p` | string | yes      | Profile from `profiles.json`.              |
| `--output`, `-o`  | string | no       | Output format: `text` (default) or `json`. |
| `--timeout-ms`    | int    | no       | RPC timeout in ms (default: `30000`).      |

---

## Output ⬆️

**Text**

```
Positions: N
SYMBOL  #TICKET  vol=V  open=PRICE  pnl=PROFIT
...
```

\*Prints up to **10** positions; if more exist, shows `... and K more`.\*

**JSON**

```json
{
  "PositionInfos": [
    { "Ticket": 123456, "Symbol": "EURUSD", "Volume": 0.10, "PriceOpen": 1.0950, "Profit": 12.34 }
  ]
}
```

---

## Method Signature

```csharp
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

---

## How to Use

```powershell
# Text
dotnet run -- positions -p demo

# JSON
dotnet run -- positions -p demo -o json
```

Shortcasts (from `ps/shortcasts.ps1`):

```powershell
positions   # → mt5 positions -p demo --timeout-ms 90000
pos         # alias to the same
```

---

## Code Reference 🧩

```csharp
var positions = new Command("positions", "List active positions");
positions.AddAlias("pos");
positions.AddOption(profileOpt);
positions.AddOption(outputOpt);
positions.AddOption(timeoutOpt);

positions.SetHandler(async (string profile, string output, int timeoutMs) =>
{
    Validators.EnsureProfile(profile);

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:POSITIONS Profile:{Profile}", profile))
    {
        try
        {
            await ConnectAsync();
            var opened = await _mt5Account.OpenedOrdersAsync();
            var list = opened.PositionInfos;

            if (string.Equals(output, "json", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(ToJson(new { PositionInfos = list }));
                return;
            }

            if (list.Count == 0)
            {
                Console.WriteLine("No positions.");
                return;
            }

            Console.WriteLine($"Positions: {list.Count}");
            foreach (var p in list.Take(10))
                Console.WriteLine($"{p.Symbol}  #{p.Ticket}  vol={p.Volume}  open={p.PriceOpen}  pnl={p.Profit}");
            if (list.Count > 10) Console.WriteLine($"... and {list.Count - 10} more");
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

## Notes & Safety 🛡️

* This is a **quick overview**; use **Ticket_Show.md** for detailed info on a specific ticket.
* PnL shown is a snapshot and may change between calls.

**See also:** **[Orders.md](../Orders_Positions/Orders.md)**, **[Ticket\_Show.md](../Misc/Ticket_Show.md)**
