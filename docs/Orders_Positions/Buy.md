# Buy (`buy`) 🟢

## What it Does 🎯

Places a **market BUY** order on MT5 for the selected symbol and volume.
Supports optional **SL/TP**, **deviation**, **timeout**, **output mode**, and **dry‑run**.

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                                |
| ----------------- | ------ | -------- | ---------------------------------------------------------- |
| `--profile`, `-p` | string | ✅        | Profile from `profiles.json` to connect with.              |
| `--symbol`, `-s`  | string | ✅        | Symbol to trade (e.g., `EURUSD`). Defaults to app setting. |
| `--volume`, `-v`  | double | ✅        | Trade volume in lots (e.g., `0.10`).                       |
| `--sl`            | double | ❌        | Stop Loss **price** (absolute). Optional.                  |
| `--tp`            | double | ❌        | Take Profit **price** (absolute). Optional.                |
| `--deviation`     | int    | ❌        | Max slippage in **points** (default: `10`).                |
| `--output`, `-o`  | string | ❌        | `text` (default) or `json`.                                |
| `--timeout-ms`    | int    | ❌        | Per‑RPC timeout in milliseconds (default: `30000`).        |
| `--dry-run`       | flag   | ❌        | Print action plan without sending an order.                |

---

## Output Fields ⬆️

| Field    | Type   | Description                          |
| -------- | ------ | ------------------------------------ |
| `Ticket` | ulong  | Ticket of the created position.      |
| `Symbol` | string | Traded instrument.                   |
| `Volume` | double | Executed volume.                     |
| `SL`     | double | Stop Loss (if set).                  |
| `TP`     | double | Take Profit (if set).                |
| `Status` | string | `OK` or error text (in error cases). |

---

## How to Use 🛠️

### CLI

```powershell
# Minimal
dotnet run -- buy -p demo -s EURUSD -v 0.10

# With SL/TP and custom deviation
dotnet run -- buy -p demo -s EURUSD -v 0.10 --sl 1.0950 --tp 1.1050 --deviation 20

# JSON output
dotnet run -- buy -p demo -s EURUSD -v 0.10 -o json --timeout-ms 60000

# Dry‑run (no order will be sent)
dotnet run -- buy -p demo -s EURUSD -v 0.10 --dry-run
```

### PowerShell Shortcuts (from `ps/shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
b -s EURUSD -v 0.10 --sl 1.0950 --tp 1.1050 -dev 10
# expands to: mt5 buy -p demo -s EURUSD -v 0.10 --sl 1.0950 --tp 1.1050 --deviation 10 --timeout-ms 90000
```

---

## Notes & Safety 🛡️

* Before sending the order, the app makes a **best‑effort** call to ensure the symbol is visible in Market Watch (`EnsureSymbolVisibleAsync`).
* `--sl`/`--tp` are **prices**, not distances. They must respect broker **stops level** and min distance.
* Use `symbol limits` to check **min/step/max** lot sizes before placing orders.
* If the market is closed or trading is disabled, the broker will reject the request.

---

## Code Reference (exact) 🧩

```csharp
var buy = new Command("buy", "Market buy");
buy.AddAlias("b");

// options
buy.AddOption(profileOpt);
buy.AddOption(symbolOpt);
buy.AddOption(volumeOpt);
buy.AddOption(slOpt);
buy.AddOption(tpOpt);
buy.AddOption(devOpt);
buy.AddOption(outputOpt);
buy.AddOption(timeoutOpt);
buy.AddOption(dryRunOpt);

// Context-based handler: read all options from ctx.ParseResult
buy.SetHandler(async (InvocationContext ctx) =>
{
    // Read options
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var symbolOptV= ctx.ParseResult.GetValueForOption(symbolOpt);
    var volume    = ctx.ParseResult.GetValueForOption(volumeOpt);
    var sl        = ctx.ParseResult.GetValueForOption(slOpt);
    var tp        = ctx.ParseResult.GetValueForOption(tpOpt);
    var deviation = ctx.ParseResult.GetValueForOption(devOpt);
    var output    = ctx.ParseResult.GetValueForOption(outputOpt) ?? "text";
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    // Validate & select profile/symbol/params
    Validators.EnsureProfile(profile);
    Validators.EnsureVolume(volume);
    Validators.EnsureDeviation(deviation);

    var s = Validators.EnsureSymbol(symbolOptV ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:BUY Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol}", s))
    using (_logger.BeginScope("OrderParams Vol:{Vol} Dev:{Dev} SL:{SL} TP:{TP}", volume, deviation, sl, tp))
    {
        if (dryRun)
        {
            if (string.Equals(output, "json", StringComparison.OrdinalIgnoreCase))
            {
                var payload = new {
                    DryRun = true, Side = "BUY", Symbol = s, Volume = volume,
                    Deviation = deviation, SL = sl, TP = tp
                };
                Console.WriteLine(JsonSerializer.Serialize(payload));
            }
            else
            {
                Console.WriteLine($"[DRY-RUN] BUY {s} vol={volume} dev={deviation} SL={sl} TP={tp}");
            }
            return;
        }

        try
        {
            await ConnectAsync();

            // Ensure symbol visible (best-effort)
            try
            {
                using var visCts = StartOpCts();
                await _mt5Account.EnsureSymbolVisibleAsync(
                    s, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
            }

            // Send order with retry
            using var opCts = StartOpCts();
            var ticket = await CallWithRetry(
                ct => _mt5Account.SendMarketOrderAsync(
                    symbol: s, isBuy: true, volume: volume, deviation: deviation,
                    stopLoss: sl, takeProfit: tp, deadline: null, cancellationToken: ct),
                opCts.Token);

            // Single print point
            if (string.Equals(output, "json", StringComparison.OrdinalIgnoreCase))
            {
                var payload = new {
                    Side = "BUY", Symbol = s, Volume = volume, Deviation = deviation,
                    SL = sl, TP = tp, Ticket = ticket
                };
                Console.WriteLine(JsonSerializer.Serialize(payload));
            }
            else
            {
                _logger.LogInformation("BUY done: ticket={Ticket}", ticket);
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
});

// Add command once
root.AddCommand(buy);
```

---

📌 In short:
**`buy`** = quick market entry with guardrails (visibility check, retries, single output), and plays nicely with profiles, timeouts, and shortcasts.
