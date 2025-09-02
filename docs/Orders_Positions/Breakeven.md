# Breakeven (`breakeven`) ⚖️

## What it does

Moves a position’s **Stop Loss** to **breakeven** (near the entry price) with an optional **offset**.
Default safety behavior: only improves SL (moves it toward profit). You can override with `--force`.

---
## Method signatures (used by this command) 📘

```csharp
// Fetch open positions (and pending orders)
public async Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// Guess point size for a symbol
public double PointGuess(string symbol);

// Ensure a symbol is visible (best-effort prep)
public async Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    TimeSpan? pollInterval = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// Modify SL/TP for an existing position
public async Task<bool> ModifyPositionSlTpAsync(
    ulong ticket,
    double? sl,
    double? tp,
    CancellationToken ct);
```

## Input parameters ⬇️

| Parameter               | Type    | Description                                                                    |
| ----------------------- | ------- | ------------------------------------------------------------------------------ |
| `--profile`, `-p`       | string  | Profile from `profiles.json`.                                                  |
| `--ticket`, `-t`        | ulong   | **Required.** Position ticket to adjust.                                       |
| `--offset`              | double? | Offset from entry in **price units** (e.g., `0.0002`).                         |
| `--offset-points`, `-P` | int?    | Offset from entry in **points** (e.g., `20`). Converted via symbol point size. |
| `--force`               | flag    | Allow move even if it **does not improve** the current SL.                     |
| `--dry-run`             | flag    | Calculate and log the target SL **without** sending a modify request.          |
| `--timeout-ms`          | int     | Per-RPC timeout in milliseconds. **Default:** `30000`.                         |

---

## What gets printed ⬆️

Text logs only (no JSON mode). Examples:

```
info: Cmd:BREAKEVEN Profile:demo
info: BREAKEVEN done: ticket=123456 symbol=EURUSD SL=1.09320
```

Dry run:

```
[DRY-RUN] BREAKEVEN #123456 EURUSD: SL -> 1.09320
```

Failure samples:

```
Position with ticket #123456 not found.              (exit code 2)
No improvement: current=1.09320 target=1.09310 ...   (exit code 2)
```

---

## How it works 🧠

* **BUY:** `NewSL = Entry + Offset`
* **SELL:** `NewSL = Entry - Offset`
* Offset source: `--offset` (price) **or** `--offset-points × pointSize`.
* If `--force` is **not** set, SL must **improve** (move toward profit) vs current SL, otherwise the command exits with code `2`.
* Best-effort **EnsureSymbolVisibleAsync(symbol, \~3s)** before modify.
* Modify is sent via **`ModifyPositionSlTpAsync(ticket, SL, TP:null)`**.

---

## How to use 🛠️

### CLI

```powershell
# Move SL to exact breakeven
dotnet run -- breakeven -p demo -t 123456

# Breakeven + 20 points buffer
dotnet run -- breakeven -p demo -t 123456 --offset-points 20

# Force a non-improving move (use with caution)
dotnet run -- breakeven -p demo -t 123456 --offset 0.0002 --force

# Preview without changing anything on the server
dotnet run -- breakeven -p demo -t 123456 --offset 0.0002 --dry-run
```

### PowerShell shortcut (from `shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
be -t 123456 -offsetPts 20   # expands to: mt5 breakeven -p demo -t 123456 --offset-points 20 --timeout-ms 90000
```

---

## Notes & safety 🛡️

* Broker **stops level / freeze level** can reject too-tight SL.
* For typical 5-digit FX, `20` points = `2` pips. Always confirm **point size** (`symbol show`).
* Point size fallback used when unknown: **`JPY` → `0.01`**, otherwise **`0.0001`**.
* On errors, the process sets a non-zero `Environment.ExitCode`.

---

## Code example 🧩

```csharp
// Options
var beTicketOpt       = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket to move SL to breakeven") { IsRequired = true };
var beOffsetPriceOpt  = new Option<double?>(new[] { "--offset" }, "Offset from entry in PRICE units (e.g., 0.0002)");
var beOffsetPointsOpt = new Option<int?>(new[] { "--offset-points", "-P" }, "Offset from entry in POINTS");
var beForceOpt        = new Option<bool>(new[] { "--force" }, "Allow worsening SL (by default only improve)");

var breakeven = new Command("breakeven", "Move SL to entry ± offset (breakeven) for a position");
breakeven.AddAlias("be");

breakeven.AddOption(profileOpt);
breakeven.AddOption(beTicketOpt);
breakeven.AddOption(beOffsetPriceOpt);
breakeven.AddOption(beOffsetPointsOpt);
breakeven.AddOption(beForceOpt);

// Handler (actual signature)
breakeven.SetHandler(async (string profile, ulong ticket, double? offsetPrice, int? offsetPoints, bool force, int timeoutMs, bool dryRun) =>
{
    if (offsetPrice is not null && offsetPoints is not null)
    {
        Console.WriteLine("Use either --offset (price) OR --offset-points, not both.");
        Environment.ExitCode = 2;
        return;
    }
    if (offsetPrice is not null && offsetPrice < 0)   throw new ArgumentOutOfRangeException(nameof(offsetPrice));
    if (offsetPoints is not null && offsetPoints < 0) throw new ArgumentOutOfRangeException(nameof(offsetPoints));

    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:BREAKEVEN Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} OffsetPrice:{OffsetPrice} OffsetPoints:{OffsetPoints} Force:{Force}", ticket, offsetPrice, offsetPoints, force))
    {
        try
        {
            await ConnectAsync();

            // 1) read positions
            var opened = await _mt5Account.OpenedOrdersAsync();
            var pos = opened.PositionInfos.FirstOrDefault(p =>
                p.Ticket == ticket || unchecked((ulong)p.Ticket) == ticket);
            if (pos is null)
            {
                Console.WriteLine($"Position with ticket #{ticket} not found.");
                Environment.ExitCode = 2;
                return;
            }

            var symbol     = pos.Symbol;
            var entryPrice = pos.PriceOpen;

            // 2) resolve offset to PRICE units
            double offPrice;
            if (offsetPrice is not null)
            {
                offPrice = offsetPrice.Value;
            }
            else if (offsetPoints is not null)
            {
                var pointSize = _mt5Account.PointGuess(symbol);
                if (pointSize <= 0)
                    pointSize = symbol.EndsWith("JPY", StringComparison.OrdinalIgnoreCase) ? 0.01 : 0.0001;
                offPrice = offsetPoints.Value * pointSize;
            }
            else
            {
                offPrice = 0.0; // exact BE
            }

            // 3) compute target SL
            bool isLong = IsLongPosition(pos); // project helper
            var targetSl = isLong ? (entryPrice + offPrice) : (entryPrice - offPrice);

            // 4) improvement check unless --force
            var currentSl = TryGetDoubleProperty(pos, "StopLoss", "SL", "Sl"); // project helper
            if (!force && currentSl is not null)
            {
                bool improves = isLong ? targetSl > currentSl.Value
                                       : targetSl < currentSl.Value;
                if (!improves)
                {
                    Console.WriteLine($"No improvement: current={currentSl.Value} target={targetSl}. Use --force to override.");
                    Environment.ExitCode = 2;
                    return;
                }
            }

            // best-effort: make sure symbol is visible (non-fatal if fails)
            try { await _mt5Account.EnsureSymbolVisibleAsync(symbol, maxWait: TimeSpan.FromSeconds(3)); } catch { }

            if (dryRun)
            {
                Console.WriteLine($"[DRY-RUN] BREAKEVEN #{ticket} {symbol}: SL -> {targetSl}");
                return;
            }

            // 5) apply change
            await _mt5Account.ModifyPositionSlTpAsync(ticket, targetSl, null, CancellationToken.None);

            _logger.LogInformation("BREAKEVEN done: ticket={Ticket} symbol={Symbol} SL={SL}", ticket, symbol, targetSl);
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
}, profileOpt, beTicketOpt, beOffsetPriceOpt, beOffsetPointsOpt, beForceOpt, timeoutOpt, dryRunOpt);
```

---



### Command handler signature (as wired)

```csharp
(string profile,
 ulong ticket,
 double? offsetPrice,
 int? offsetPoints,
 bool force,
 int timeoutMs,
 bool dryRun) => Task
```
