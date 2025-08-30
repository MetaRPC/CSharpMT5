# Positions (`positions`) 📈

## What it Does 🎯

Lists all **active (open) positions** for the selected profile/account.

Alias: `pos`

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                |
| ----------------- | ------ | -------- | ------------------------------------------ |
| `--profile`, `-p` | string | ✅        | Profile from `profiles.json`.              |
| `--output`, `-o`  | string | ❌        | Output format: `text` (default) or `json`. |
| `--timeout-ms`    | int    | ❌        | RPC timeout in ms (default: `30000`).      |

---

## Output ⬆️

### Text mode

```
Positions: N
SYMBOL  #TICKET  vol=V  open=PRICE  pnl=PROFIT
...
```

* Prints up to **10** positions; if more exist, shows `... and K more`.

### JSON mode

The raw structure from `_mt5Account.OpenedOrdersAsync()` (field `PositionInfos[]`). Example shape:

```json
{
  "PositionInfos": [
    {
      "Ticket": 123456,
      "Symbol": "EURUSD",
      "Volume": 0.10,
      "PriceOpen": 1.0950,
      "Profit": 12.34
    }
  ]
}
```

---

## How to Use 🛠️

### CLI

```powershell
# Default text
dotnet run -- positions -p demo

# JSON output
dotnet run -- positions -p demo -o json
```

### PowerShell Shortcuts (from `ps/shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
positions   # expands to: mt5 positions -p demo --timeout-ms 90000
pos         # alias to the same
```

---

## Notes & Safety 🛡️

* Designed for **quick overview**; use `ticket show` for full details of a specific position.
* PnL shown is a snapshot; values may change between calls.
* If the connection is down or there are no positions, the list may be empty.

---

## Code Reference (to be filled by you) 🧩

```csharp
var positions = new Command("positions", "List active positions");
    positions.AddAlias("pos");

    positions.AddOption(profileOpt);
    positions.AddOption(outputOpt);
    positions.SetHandler(async (string profile, string output, int timeoutMs) =>
    {
        Validators.EnsureProfile(profile);
        _selectedProfile = profile;

        using (UseOpTimeout(timeoutMs))
        using (_logger.BeginScope("Cmd:POSITIONS Profile:{Profile}", profile))
        {
            try
            {
                await ConnectAsync();
                using var opCts = StartOpCts();

                var opened = await CallWithRetry(
                    ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                    opCts.Token);

                if (IsJson(output)) Console.WriteLine(ToJson(opened));
                else
                {
                    var list = opened.PositionInfos;
                    Console.WriteLine($"Positions: {list.Count}");
                    foreach (var p in list.Take(10))
                        Console.WriteLine($"{p.Symbol}  #{p.Ticket}  vol={p.Volume}  open={p.PriceOpen}  pnl={p.Profit}");
                    if (list.Count > 10) Console.WriteLine($"... and {list.Count - 10} more");
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
    root.AddCommand(positions);
```
