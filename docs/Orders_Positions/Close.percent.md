# Close Percent (`close.percent`) 🎯%

Closes a **percentage of a position’s volume** by ticket. Great for scaling‑out with rules like “take 50% at +1R, trail the rest”.

---
## Method Signatures

```csharp
// Read open positions (resolve ticket → symbol & current volume)
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// Query lot constraints
public Task<(double min, double step, double max)> GetVolumeConstraintsAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// Best‑effort: ensure the symbol is visible before trading
public Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    TimeSpan? pollInterval = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// Partial close (variant A — explicit deviation)
public Task ClosePositionPartialAsync(
    ulong ticket,
    double volume,
    int deviation,
    CancellationToken cancellationToken);

// Partial close (variant B — by symbol; deviation via defaults)
public Task CloseOrderByTicketAsync(
    ulong ticket,
    string symbol,
    double volume,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                               |
| ----------------- | ------ | -------- | --------------------------------------------------------- |
| `--profile`, `-p` | string | yes      | Profile from `profiles.json`.                             |
| `--ticket`, `-t`  | ulong  | yes      | Position ticket to partially close.                       |
| `--pct`           | double | no       | Percentage to close (0 < `pct` ≤ 100). **Default:** `50`. |
| `--deviation`     | int    | no       | Max slippage (points). **Default:** `10`.                 |
| `--timeout-ms`    | int    | no       | RPC timeout in ms (**default:** `30000`).                 |
| `--dry-run`       | flag   | no       | Print intended action without sending a request.          |

> **Note:** This command is **text-only**; JSON output is not supported by the current handler.

---

## Output ⬆️

Examples:

```
[DRY-RUN] CLOSE.PERCENT ticket=123456 pct=50 volume=0.12 deviation=10
✔ close.percent done: ticket=123456 closed=0.12 (pct=50)
```

Errors:

```
Position #123456 not found.                      (exit code 2)
Percent must be in (0;100].                     (exit code 2)
Computed close volume below MinLot after step.  (exit code 2)
RPC error: <broker message>                      (exit code 1)
```

**Exit codes**

* `0` — success
* `2` — validation/not found/guard failures (including too‑small rounded volume)
* `1` — fatal error (printed via ErrorPrinter)

---

## How to Use

```powershell
# Close 50% (default)
dotnet run -- close.percent -p demo -t 123456

# Close 25% with wider slippage
dotnet run -- close.percent -p demo -t 123456 --pct 25 --deviation 20

# Dry‑run (no request sent)
dotnet run -- close.percent -p demo -t 123456 --pct 33.3 --dry-run
```

### PowerShell shortcut (from `ps/shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
cpp -t 123456 -pct 50
# → mt5 close.percent -p demo -t 123456 --pct 50 --deviation 10 --timeout-ms 90000
```

---

## Notes & Safety

* The computed close volume is **rounded to lot step** and clamped to `[MinLot; CurrentVolume]`.
* If the rounded result is **below MinLot**, the broker may reject the request — consider [`close.partial`](./Close.partial.md) to choose a valid lot explicitly.
* `pct = 100` closes the entire position; behavior is equivalent to full close.
* `--deviation` matters on fast markets; widen if you see rejections.
* Use **[symbol limits](../Market_Data/Limits.md)** to verify `min/step/max` for your symbol.

---

## Code Reference 🧩

```csharp
var cpTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket") { IsRequired = true };
var cpPctOpt    = new Option<double>(new[] { "--pct" }, () => 50.0, "Percent to close (0 < pct ≤ 100)");
var cpDevOpt    = devOpt;

var closePercent = new Command("close.percent", "Close a percentage of a position by ticket");
closePercent.AddAlias("cpp");

closePercent.AddOption(profileOpt);
closePercent.AddOption(cpTicketOpt);
closePercent.AddOption(cpPctOpt);
closePercent.AddOption(cpDevOpt);
closePercent.AddOption(timeoutOpt);
closePercent.AddOption(dryRunOpt);

closePercent.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(cpTicketOpt);
    var pct       = ctx.ParseResult.GetValueForOption(cpPctOpt);
    var deviation = ctx.ParseResult.GetValueForOption(cpDevOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    if (pct <= 0 || pct > 100) throw new ArgumentOutOfRangeException(nameof(pct), "Percent must be in (0;100].");

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CLOSE.PERCENT Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} Pct:{Pct} Dev:{Dev}", ticket, pct, deviation))
    {
        await ConnectAsync();

        // 1) Resolve symbol & current volume
        var opened = await _mt5Account.OpenedOrdersAsync();
        var pos = opened.PositionInfos.FirstOrDefault(p => (ulong)p.Ticket == ticket || unchecked((ulong)p.Ticket) == ticket);
        if (pos is null) { Console.WriteLine($"Position #{ticket} not found."); Environment.ExitCode = 2; return; }
        var symbol = pos.Symbol;
        var currentVol = pos.Volume;

        // 2) Compute requested close volume
        var req = currentVol * (pct / 100.0);

        // 3) Normalize to lot constraints
        var (min, step, max) = await _mt5Account.GetVolumeConstraintsAsync(symbol);
        double norm = Math.Max(min, Math.Min(currentVol, Math.Floor(req / step + 1e-9) * step));
        if (norm < min || norm <= 0) { Console.WriteLine("Computed close volume below MinLot after step."); Environment.ExitCode = 2; return; }

        // 4) Best‑effort ensure visibility
        try { await _mt5Account.EnsureSymbolVisibleAsync(symbol, TimeSpan.FromSeconds(3)); } catch { }

        // 5) Dry‑run or execute
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] CLOSE.PERCENT ticket={ticket} pct={pct} volume={norm} deviation={deviation}");
            return;
        }

        await _mt5Account.ClosePositionPartialAsync(ticket, norm, deviation, CancellationToken.None);
        Console.WriteLine($"\u2714 close.percent done: ticket={ticket} closed={norm} (pct={pct})");
    }
});
```

---

## See also

* **[`close.half`](./Close.half.md)** — close 50% via alias
* **[`close.partial`](./Close.partial.md)** — close an exact lot amount
* **[`symbol limits`](../Market_Data/Limits.md)** — min/step/max lot constraints
