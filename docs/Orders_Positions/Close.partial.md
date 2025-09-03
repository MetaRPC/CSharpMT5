# Close Partial (`close.partial`) 🪓

Closes an **exact volume** (in lots) of a position by ticket.

Unlike `close.half` or `close.percent`, this command lets you choose the **precise number of lots** to close.

---
## Method Signatures

```csharp
// Read open positions (to validate ticket, get symbol/current volume if needed)
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// Partial close (variant A — explicit deviation)
public Task ClosePositionPartialAsync(
    ulong ticket,
    double volume,
    int deviation,
    CancellationToken cancellationToken);

// Alternative partial close (variant B — by symbol)
public Task CloseOrderByTicketAsync(
    ulong ticket,
    string symbol,
    double volume,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

## Input Parameters

| Parameter         | Type   | Required | Description                                    |
| ----------------- | ------ | -------- | ---------------------------------------------- |
| `--profile`, `-p` | string | yes      | Profile from `profiles.json`.                  |
| `--ticket`, `-t`  | ulong  | yes      | Position ticket to partially close.            |
| `--volume`, `-v`  | double | yes      | Exact volume to close (lots).                  |
| `--deviation`     | int    | no       | Max slippage (points). Default: `10`.          |
| `--timeout-ms`    | int    | no       | RPC timeout in ms (default: `30000`).          |
| `--dry-run`       | flag   | no       | Print intended action without sending request. |

> **Note:** This command is **text-only**; JSON output is not supported by the current handler.

---

## Output (text) & Exit Codes

Examples:

```
[DRY-RUN] CLOSE.PARTIAL ticket=123456 volume=0.03 deviation=10
✔ close.partial done: ticket=123456 closed=0.03
```

Errors:

```
Position #123456 not found.          (exit code 2)
Invalid volume: 0.00                 (exit code 2)
RPC error: <broker message>          (exit code 1)
```

**Exit codes**

* `0` — success
* `2` — validation/not found/guard failures
* `1` — fatal error (printed via ErrorPrinter)

---

## How to Use

```powershell
# Close exactly 0.03 lots
dotnet run -- close.partial -p demo -t 123456 -v 0.03

# With custom slippage
dotnet run -- close.partial -p demo -t 123456 -v 0.01 --deviation 20

# Dry-run (no request sent)
dotnet run -- close.partial -p demo -t 123456 -v 0.05 --dry-run
```

### PowerShell shortcut (from `ps/shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
cp -t 123456 -v 0.03
# → mt5 close.partial -p demo -t 123456 -v 0.03 --deviation 10 --timeout-ms 90000
```

---

## Notes & Safety

* The requested volume must comply with **symbol min/step/max** — check **[symbol limits](../Market_Data/Limits.md)**. No auto-rounding is performed by this command.
* If `volume` > current position size, the broker will reject the request.
* Residual volume must remain ≥ MinLot.
* RPCs honor `--timeout-ms` via the operation cancellation token.

---

## Code Reference (illustrative)

```csharp
var cpVolumeOpt = new Option<double>(new[] { "--volume", "-v" }, "Volume (lots) to close")
{
    IsRequired = true
};

var closePartial = new Command("close.partial", "Partially close a position by ticket");
closePartial.AddAlias("cp");

closePartial.AddOption(profileOpt);
closePartial.AddOption(cpTicketOpt);
closePartial.AddOption(cpVolumeOpt);
closePartial.AddOption(devOpt);      // deviation in points
closePartial.AddOption(timeoutOpt);
closePartial.AddOption(dryRunOpt);

closePartial.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(cpTicketOpt);
    var volume    = ctx.ParseResult.GetValueForOption(cpVolumeOpt);
    var deviation = ctx.ParseResult.GetValueForOption(devOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (ticket == 0) throw new ArgumentOutOfRangeException(nameof(ticket), "Ticket must be > 0.");
    if (volume <= 0) throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be > 0.");
    // Optional: Validators.EnsureDeviation(deviation);

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CLOSE.PARTIAL Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} Vol:{Vol} Dev:{Dev}", ticket, volume, deviation))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] CLOSE.PARTIAL ticket={ticket} volume={volume} deviation={deviation}");
            return;
        }

        await ConnectAsync();

        // (Optional) If you need the symbol for visibility checks or extra guards:
        // var opened = await _mt5Account.OpenedOrdersAsync();
        // var pos = opened.PositionInfos.FirstOrDefault(p => (ulong)p.Ticket == ticket);
        // if (pos is null) { Console.WriteLine($"Position #{ticket} not found."); Environment.ExitCode = 2; return; }
        // try { await _mt5Account.EnsureSymbolVisibleAsync(pos.Symbol, TimeSpan.FromSeconds(3)); } catch { }

        await _mt5Account.ClosePositionPartialAsync(ticket, volume, deviation, CancellationToken.None);
        Console.WriteLine($"\u2714 close.partial done: ticket={ticket} closed={volume}");
    }
});
```

---

## See also

* **[`close.percent`](./Close.percent.md)** — close by percentage of current volume
* **[`symbol limits`](../Market_Data/Limits.md)** — min/step/max lot constraints
