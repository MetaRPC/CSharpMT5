# Trail Start (`trail.start`) 🏃‍♂️💨

## What it Does 🎯

Starts a **local trailing stop** for an existing position (by ticket). The app monitors price ticks and **moves SL** according to selected mode and distances.

> This is **client‑side** trailing: it works while your CLI/app is running and connected.

---

## Modes ⚙️

* `classic` — SL trails the price by a fixed **distance** in points; updated only when price moves by at least **step** points.
* `chandelier` — SL follows an internal high/low buffer (ATR‑like style), respecting `distance` and `step`. *(Depends on your `_mt5Account` implementation of `TrailMode`.)*

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Default | Description                                |               |
| ----------------- | ------ | -------- | ------- | ------------------------------------------ | ------------- |
| `--profile`, `-p` | string | ✅        | —       | Profile from `profiles.json`.              |               |
| `--ticket`, `-t`  | ulong  | ✅        | —       | Position ticket to trail.                  |               |
| `--distance`      | int    | ✅        | 150     | Distance in **points** from price to SL.   |               |
| `--step`          | int    | ✅        | 20      | Minimal move in **points** to update SL.   |               |
| `--mode`          | string | ✅        | classic | Trailing mode: `classic`                   | `chandelier`. |
| `--timeout-ms`    | int    | ❌        | 30000   | RPC timeout in ms for initial queries.     |               |
| `--dry-run`       | flag   | ❌        | —       | Print intent without starting the trailer. |               |

Validation:

* `--distance > 0`, `--step > 0`.
* `--mode` must be one of `classic|chandelier` (case‑insensitive).

---

## Output ⬆️

* On success: prints `✔ trail.start scheduled`.
* On dry‑run: prints the plan with parameters.
* Errors are logged with details and non‑zero exit code.

---

## How to Use 🛠️

### CLI

```powershell
# Start Classic trailing: 150 pts distance, 20 pts step
dotnet run -- trail.start -p demo -t 123456 --distance 150 --step 20 --mode classic

# Chandelier trailing: wider distance
dotnet run -- trail.start -p demo -t 123456 --distance 300 --step 50 --mode chandelier

# Dry‑run (no start)
dotnet run -- trail.start -p demo -t 123456 --distance 200 --step 30 --dry-run
```

### Optional PowerShell Shortcast

If you want a short command, add to `ps/shortcasts.ps1`:

```powershell
function trstart { param([ulong]$t,[int]$dist=150,[int]$step=20,[string]$mode='classic',[string]$p=$PF,[int]$to=$TO)
  mt5 trail.start -p $p -t $t --distance $dist --step $step --mode $mode --timeout-ms $to }
```

---

## Notes & Safety 🛡️

* **Local process**: trailing stops when the CLI/app **exits** or **disconnects**.
* SL moves only **toward profit** (implementation‑specific; classic trailing should never widen risk).
* Distances are in **points** (not pips). Check `symbol show` for point size.
* Broker **StopsLevel** and min distance rules still apply.
* Works only for **positions** (not pendings).

---

## Code Reference (exact) 💻

```csharp
var trTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket") { IsRequired = true };
var trDistOpt   = new Option<int>(new[] { "--distance" }, () => 150, "Distance in POINTS from price to SL");
var trStepOpt   = new Option<int>(new[] { "--step" }, () => 20, "Minimal move in POINTS to update SL");
var trModeOpt   = new Option<string>(new[] { "--mode" }, () => "classic", "classic|chandelier");

var trailStart = new Command("trail.start", "Start local trailing stop for a position");
trailStart.AddOption(profileOpt);
trailStart.AddOption(trTicketOpt);
trailStart.AddOption(trDistOpt);
trailStart.AddOption(trStepOpt);
trailStart.AddOption(trModeOpt);
trailStart.AddOption(timeoutOpt);
trailStart.AddOption(dryRunOpt);

trailStart.SetHandler(async (InvocationContext ctx) =>
{
    var profile  = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket   = ctx.ParseResult.GetValueForOption(trTicketOpt);
    var distance = ctx.ParseResult.GetValueForOption(trDistOpt);
    var step     = ctx.ParseResult.GetValueForOption(trStepOpt);
    var modestr  = ctx.ParseResult.GetValueForOption(trModeOpt);
    var timeoutMs= ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun   = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    if (distance <= 0) throw new ArgumentOutOfRangeException(nameof(distance));
    if (step <= 0)     throw new ArgumentOutOfRangeException(nameof(step));

    var modeText = (modestr ?? "classic").Trim();
    if (!System.Enum.TryParse<MT5Account.TrailMode>(modeText, ignoreCase: true, out var mode))
        throw new ArgumentException("Invalid --mode. Use classic|chandelier.");

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:TRAIL.START Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} Dist:{Dist} Step:{Step} Mode:{Mode}", ticket, distance, step, mode))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            var opened = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                opCts.Token);

            var pos = opened.PositionInfos.FirstOrDefault(p => Convert.ToUInt64(p.Ticket) == ticket);
            if (pos is null)
            {
                Console.WriteLine($"Position #{ticket} not found.");
                Environment.ExitCode = 2;
                return;
            }

            var symbol = pos.Symbol;
            var isLong = IsLongPosition(pos);

            if (dryRun)
            {
                Console.WriteLine($"[DRY-RUN] TRAIL.START #{ticket} {symbol} mode={mode} dist={distance} step={step}");
                return;
            }

            await _mt5Account.StartTrailingAsync(ticket, symbol, isLong, distance, step, mode, opCts.Token);
            Console.WriteLine("✔ trail.start scheduled");
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
});

root.AddCommand(trailStart);
```
