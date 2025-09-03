# Modify (`modify`) ✏️

Updates **Stop Loss** and/or **Take Profit** for a **position by ticket**.

> This command is **text-only** (no JSON). At least one of `--sl` or `--tp` must be provided.

---
## Method Signatures

```csharp
// Ensure a symbol is visible (best‑effort prep)
public Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    TimeSpan? pollInterval = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// Modify SL/TP for an existing position
public Task<bool> ModifyPositionSlTpAsync(
    ulong ticket,
    double? sl,
    double? tp,
    CancellationToken ct);

// (Optional) Read open positions if you want to log current SL/TP before changes
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

## Input Parameters

| Parameter         | Type    | Required | Description                                                  |
| ----------------- | ------- | -------- | ------------------------------------------------------------ |
| `--profile`, `-p` | string  | yes      | Profile from `profiles.json`.                                |
| `--ticket`, `-t`  | ulong   | yes      | Position ticket to modify.                                   |
| `--sl`            | double? | no       | New **Stop Loss price** (absolute).                          |
| `--tp`            | double? | no       | New **Take Profit price** (absolute).                        |
| `--symbol`, `-s`  | string? | no       | Optional symbol (used to ensure visibility on some servers). |
| `--timeout-ms`    | int     | no       | RPC timeout in ms (default: `30000`).                        |
| `--dry-run`       | flag    | no       | Print intended action without sending the request.           |

> **Validation rule:** `--sl` **or** `--tp` must be specified (one or both). If both are omitted, the command fails fast.

---

## Output (text) & Exit Codes

Examples:

```
[DRY-RUN] MODIFY #123456 SL=1.0900 TP=1.1100
✔ modify done: ticket=123456 SL=1.0900 TP=1.1100
```

Errors (examples):

```
At least one of --sl/--tp must be provided.  (exit code 2)
Position #123456 not found.                  (exit code 2)
RPC error: <broker message>                  (exit code 1)
```

**Exit codes**

* `0` — success
* `2` — validation / guard failures
* `1` — fatal error (printed via ErrorPrinter)

---

## How to Use

```powershell
# Set both SL and TP
dotnet run -- modify -p demo -t 123456 --sl 1.0950 --tp 1.1050

# Only SL
dotnet run -- modify -p demo -t 123456 --sl 1.0900

# With symbol visibility (some servers require it)
dotnet run -- modify -p demo -t 123456 --sl 1.0900 -s EURUSD

# Dry-run
dotnet run -- modify -p demo -t 123456 --tp 1.1100 --dry-run
```

### PowerShell Shortcut (from `ps/shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
posmod -t 123456 -sl 1.0900 -tp 1.1100
# expands to: mt5 position.modify -p demo -t 123456 --sl 1.0900 --tp 1.1100 --timeout-ms 90000
```

---

## Notes & Safety 🛡️

* `--sl` / `--tp` are **prices** (not distances). They must respect broker **StopsLevel** (min distance) and freeze levels.
* Some servers require the **symbol to be visible** in Market Watch; pass `-s EURUSD` to let the app perform a best‑effort ensure‑visible.
* The command does not auto‑normalize SL/TP to tick size — provide valid prices for your symbol.
* On errors, the process sets a non‑zero `Environment.ExitCode`.

---

## Code Reference 🧩

```csharp
var modify = new Command("modify", "Modify StopLoss / TakeProfit by ticket");
modify.AddAlias("m");

var modTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Ticket to modify") { IsRequired = true };
var modSlOpt     = new Option<double?>("--sl", "New Stop Loss (price)");
var modTpOpt     = new Option<double?>("--tp", "New Take Profit (price)");
var modSymbolOpt = new Option<string?>(new[] { "--symbol", "-s" }, "Symbol (optional; used to ensure visibility if needed)");

modify.AddOption(profileOpt);
modify.AddOption(modTicketOpt);
modify.AddOption(modSlOpt);
modify.AddOption(modTpOpt);
modify.AddOption(modSymbolOpt);
modify.AddOption(timeoutOpt);
modify.AddOption(dryRunOpt);

modify.SetHandler(async (string profile, ulong ticket, double? sl, double? tp, string? symbol, int timeoutMs, bool dryRun) =>
{
    Validators.EnsureProfile(profile);
    if (sl is null && tp is null)
    {
        Console.WriteLine("At least one of --sl/--tp must be provided.");
        Environment.ExitCode = 2;
        return;
    }

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:MODIFY Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} SL:{SL} TP:{TP}", ticket, sl, tp))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] MODIFY #{ticket} SL={sl} TP={tp}");
            return;
        }

        try
        {
            await ConnectAsync();

            // Best‑effort ensure visibility
            if (!string.IsNullOrWhiteSpace(symbol))
            {
                try { await _mt5Account.EnsureSymbolVisibleAsync(symbol!, TimeSpan.FromSeconds(3)); } catch { }
            }

            await _mt5Account.ModifyPositionSlTpAsync(ticket, sl, tp, CancellationToken.None);
            Console.WriteLine($"\u2714 modify done: ticket={ticket} SL={sl} TP={tp}");
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
}, profileOpt, modTicketOpt, modSlOpt, modTpOpt, modSymbolOpt, timeoutOpt, dryRunOpt);
```

---

## See also

* **[`Position.modify.points`](./Position.modify.points.md)** — move SL/TP by **±N points**
* **[`symbol limits`](../Market_Data/Limits.md)** — min/step/max lot constraints
