# Trail Start (`trail.start`) 🚦

Starts a **local trailing stop** for an existing position (by ticket). The app monitors ticks and **moves SL** according to chosen mode and distances.

> Client‑side feature: trailing works **while the CLI/app is running and connected**.

Alias: `trstart`

---

## Modes ⚙️

* `classic` — SL trails price by a fixed **distance** (points), updating only when price moves by at least **step** points.
* `chandelier` — SL follows an internal high/low buffer (ATR‑style), honoring `distance` and `step` (requires enum `MT5Account.TrailMode`).

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Default | Description                                |
| ----------------- | ------ | -------- | ------- | ------------------------------------------ |
| `--profile`, `-p` | string | yes      | —       | Profile from `profiles.json`.              |
| `--ticket`, `-t`  | ulong  | yes      | —       | Position ticket to trail.                  |
| `--distance`      | int    | no       | 150     | Distance in **points** from price to SL.   |
| `--step`          | int    | no       | 20      | Minimal **price move (points)** to update. |
| `--mode`          | string | no       | classic | `classic` or `chandelier`.                 |
| `--timeout-ms`    | int    | no       | 30000   | RPC timeout for initial queries.           |
| `--dry-run`       | flag   | no       | —       | Print intent without starting trailing.    |

**Validation**: `distance > 0`, `step > 0`, `mode ∈ {classic, chandelier}` (case‑insensitive).

---

## Output ⬆️

* Success: `✔ trail.start scheduled`
* Dry‑run: plan with parameters
* Errors: detailed log + non‑zero exit code

---

## How to Use

```powershell
# Classic trailing: 150 pts distance, 20 pts step
dotnet run -- trail.start -p demo -t 123456 --distance 150 --step 20 --mode classic

# Chandelier: wider distances
dotnet run -- trail.start -p demo -t 123456 --distance 300 --step 50 --mode chandelier

# Dry‑run
dotnet run -- trail.start -p demo -t 123456 --distance 200 --step 30 --dry-run
```

Optional shortcast (`ps/shortcasts.ps1`):

```powershell
function trstart { param([ulong]$t,[int]$dist=150,[int]$step=20,[string]$mode='classic',[string]$p=$PF,[int]$to=$TO)
  mt5 trail.start -p $p -t $t --distance $dist --step $step --mode $mode --timeout-ms $to }
```

---

## Method Signatures

```csharp
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public double PointGuess(string symbol);

public Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    TimeSpan? pollInterval = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public Task<bool> ModifyPositionSlTpAsync(
    ulong ticket,
    double? sl,
    double? tp,
    CancellationToken ct);
```

> Trailing tick feed/loop is implemented in the **command handler** (Program) using quotes; initial snapshot may use `SymbolInfoTickAsync`. No extra MT5Account RPCs are required beyond SL updates.

---

## Code Reference 🧩

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
    var modeStr  = ctx.ParseResult.GetValueForOption(trModeOpt) ?? "classic";
    var timeoutMs= ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun   = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    if (distance <= 0) throw new ArgumentOutOfRangeException(nameof(distance));
    if (step <= 0)     throw new ArgumentOutOfRangeException(nameof(step));
    if (!Enum.TryParse<MT5Account.TrailMode>(modeStr, ignoreCase: true, out var mode))
        throw new ArgumentException("Invalid --mode. Use classic|chandelier.");

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:TRAIL.START Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} Dist:{Dist} Step:{Step} Mode:{Mode}", ticket, distance, step, mode))
    {
        try
        {
            await ConnectAsync();

            // Ensure the symbol is visible (best‑effort, non‑fatal if fails)
            var opened = await _mt5Account.OpenedOrdersAsync();
            var pos = opened.PositionInfos.FirstOrDefault(p => (ulong)p.Ticket == ticket || p.Ticket == (long)ticket)
                      ?? throw new InvalidOperationException($"Position #{ticket} not found.");
            try { await _mt5Account.EnsureSymbolVisibleAsync(pos.Symbol, TimeSpan.FromSeconds(3)); } catch { }

            if (dryRun)
            {
                Console.WriteLine($"[DRY-RUN] TRAIL.START ticket={ticket} mode={mode} dist={distance} step={step}");
                return;
            }

            // Launch trailing (helper in Program; loops ticks & calls ModifyPositionSlTpAsync)
            _ = RunTrailingAsync(ticket, pos.Symbol, distance, step, mode, CancellationToken.None);
            Console.WriteLine("✔ trail.start scheduled");
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
});

// Helper (Program): advances SL only toward profit depending on mode
static async Task RunTrailingAsync(ulong ticket, string symbol, int distance, int step, MT5Account.TrailMode mode, CancellationToken ct)
{
    // Pseudocode: subscribe or poll quotes, compute target SL, call ModifyPositionSlTpAsync when step threshold reached.
}
```

---

## Notes & Safety 🛡️

* **Local process**: trailing stops when CLI/app **exits** or **disconnects**.
* SL only moves **toward profit** (never widens risk in `classic`).
* Distances are in **points** (not pips). Verify point size via **[Symbol → Limits](../Market_Data/Limits.md)** or **[Quote](../Market_Data/Quote.md)**.
* Broker **StopsLevel / FreezeLevel** rules still apply.
* Works only with **positions** (not pendings). For pending orders see **[Pending.md](../Orders_Positions/Pending.md)**.

---

## See also

* **[Position.modify.points](./Position.modify.points.md)** — set SL/TP by point distance
* **[Modify](./Modify.md)** — set SL/TP by absolute price
* **[Trail.stop](./Trail.stop.md)** — stop a running trailing session
* **[Subscribe](../Streaming/Subscribe.md)** — price stream fundamentals
