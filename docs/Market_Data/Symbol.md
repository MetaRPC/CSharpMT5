# Symbol Show (`symbol show`) 🔍

## What it Does

Displays **full metadata** about a given symbol from MT5.
Useful for diagnostics, research, or verifying broker settings (digits, contract size, trade mode, etc.).

⚠️ Related commands:

* [`symbol ensure`](Ensure_Symbol_Visible.md) → make symbol visible in Market Watch.
* [`symbol limits`](Limits.md) → show min/step/max volume for trading.

---

## Input Parameters ⬇️

| Parameter         | Type   | Description                                |
| ----------------- | ------ | ------------------------------------------ |
| `--profile`, `-p` | string |  Profile to use (from `profiles.json`).     |
| `--symbol`, `-s`  | string |  Symbol to query (e.g., `EURUSD`).          |
| `--output`, `-o`  | string |  Output format: `text` (default) or `json`. |
| `--timeout-ms`    | int    |  RPC timeout (default: 30000).              |

---

## Output Fields ⬆️

Typical metadata (depends on your proto):

| Field            | Type   | Description                                      |
| ---------------- | ------ | ------------------------------------------------ |
| `Symbol`         | string | Name of the instrument.                          |
| `Path`           | string | Folder/category in MT5 Market Watch.             |
| `Digits`         | int    | Number of decimal digits.                        |
| `Point`          | double | Smallest price step.                             |
| `ContractSize`   | double | Contract size (e.g., 100000 for FX).             |
| `TradeMode`      | enum   | Symbol trade mode (enabled/disabled/close-only). |
| `CurrencyBase`   | string | Base currency.                                   |
| `CurrencyProfit` | string | Profit currency.                                 |
| `CurrencyMargin` | string | Margin currency.                                 |
| `Spread`         | int    | Spread (points).                                 |
| `StopsLevel`     | int    | Min distance for SL/TP (points).                 |
| `VolumeMin`      | double | Minimum volume allowed.                          |
| `VolumeStep`     | double | Volume step.                                     |
| `VolumeMax`      | double | Maximum volume.                                  |

---

## How to Use 🛠️

### CLI

```powershell
# Show all metadata in text format
dotnet run -- symbol show -p demo -s EURUSD

# JSON output
dotnet run -- symbol show -p demo -s EURUSD -o json
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
use-sym EURUSD
sh      # expands to: mt5 symbol show -p demo -s EURUSD --timeout-ms 90000
```

---

## When to Use ❓

* Debugging unexpected broker restrictions (min lot, stops level, digits).
* Validating contract size and currencies for P/L or margin calculations.
* Building tools or bots that adapt to symbol properties dynamically.

---

## Notes & Safety 🛡️

* Not all brokers expose the same fields; check what your proto and server provide.
* Combine with [`symbol limits`](Limits.md) to validate trade sizes.
* Use `--output json` if you want to parse in external scripts.

---

## Code Reference 🧩

```csharp
var symShow = new Command("show", "Short card: Quote + Limits");
symShow.AddAlias("sh");
            symShow.SetHandler(async (string profile, string output, string? s, int timeoutMs) =>
            {
                Validators.EnsureProfile(profile);
                var symbolName = Validators.EnsureSymbol(s ?? GetOptions().DefaultSymbol);
                _selectedProfile = profile;

                using (UseOpTimeout(timeoutMs))
                using (_logger.BeginScope("Cmd:SYMBOL-SHOW Profile:{Profile}", profile))
                using (_logger.BeginScope("Symbol:{Symbol}", symbolName))
                {
                    try
                    {
                        await ConnectAsync();

                        // Ensure visible
                        try
                        {
                            using var visCts = StartOpCts();
                            await _mt5Account.EnsureSymbolVisibleAsync(
                                symbolName, maxWait: TimeSpan.FromSeconds(3), cancellationToken: visCts.Token);
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            _logger.LogWarning("EnsureSymbolVisibleAsync failed: {Msg}", ex.Message);
                        }

                        // Quote
                        using var qCts = StartOpCts();
                        var tick = await CallWithRetry(
                            ct => _mt5Account.SymbolInfoTickAsync(symbolName, deadline: null, cancellationToken: ct),
                            qCts.Token);

                        // Limits
                        using var lCts = StartOpCts();
                        var (min, step, max) = await CallWithRetry(
                            ct => _mt5Account.GetVolumeConstraintsAsync(symbolName, deadline: null, cancellationToken: ct),
                            lCts.Token);

                        if (IsJson(output))
                        {
                            var json = new
                            {
                                symbol = symbolName,
                                quote = new { tick.Bid, tick.Ask, tick.Time },
                                volume = new { min, step, max }
                            };
                            Console.WriteLine(ToJson(json));
                        }
                        else
                        {
                            Console.WriteLine($"{symbolName}:");
                            Console.WriteLine($"  Quote: Bid={tick.Bid} Ask={tick.Ask} Time={tick.Time}");
                            Console.WriteLine($"  Volume: min={min} step={step} max={max}");
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
            }, profileOpt, outputOpt, symbolOpt, timeoutOpt);
            symShow.AddOption(profileOpt);
            symShow.AddOption(symbolOpt);
            symShow.AddOption(outputOpt);
            symShow.AddOption(timeoutOpt);
            symbol.AddCommand(symShow);

            // register the group
            root.AddCommand(symbol);
```
