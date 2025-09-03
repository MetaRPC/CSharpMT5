# Sell (`sell`) 📉

Sends a **market SELL** order for the selected symbol and volume.

Alias: `s`

---

## Input Parameters ⬇️

| Parameter         | Type    | Required | Description                                                     |
| ----------------- | ------- | -------- | --------------------------------------------------------------- |
| `--profile`, `-p` | string  | yes      | Profile from `profiles.json`.                                   |
| `--symbol`, `-s`  | string  | no       | Symbol (e.g., `EURUSD`). Defaults to profile’s `DefaultSymbol`. |
| `--volume`, `-v`  | double  | yes      | Volume in lots.                                                 |
| `--sl`            | double? | no       | Stop Loss **price** (absolute).                                 |
| `--tp`            | double? | no       | Take Profit **price** (absolute).                               |
| `--deviation`     | int     | no       | Max slippage in **points** (default: `10`).                     |
| `--output`, `-o`  | string  | no       | `text` (default) or `json`.                                     |
| `--timeout-ms`    | int     | no       | Per‑RPC timeout in ms (default: `30000`).                       |
| `--dry-run`       | flag    | no       | Print intended action without sending order.                    |

---

## Output ⬆️

**Text (logger):**

```
SELL done: ticket=12345678
```

**JSON:**

```json
{
  "Side": "SELL",
  "Symbol": "EURUSD",
  "Volume": 0.1,
  "Deviation": 10,
  "SL": 1.1000,
  "TP": 1.0800,
  "Ticket": 12345678
}
```

Exit codes: `0` success; `1` fatal error.

---

## How to Use

```powershell
# Basic sell (text)
dotnet run -- sell -p demo -s EURUSD -v 0.10

# With SL/TP + JSON
dotnet run -- sell -p demo -s EURUSD -v 0.10 --sl 1.1000 --tp 1.0800 -o json

# Dry‑run
dotnet run -- sell -p demo -s EURUSD -v 0.10 --dry-run
```

Shortcast (from `ps/shortcasts.ps1`):

```powershell
s -s EURUSD -v 0.10 -sl 1.1000 -tp 1.0800
# → mt5 sell -p demo -s EURUSD -v 0.10 --sl 1.1000 --tp 1.0800 --timeout-ms 90000
```

---

## Notes & Safety 🛡️

* Best‑effort **EnsureSymbolVisibleAsync** (\~3s) before sending the order.
* `--sl`/`--tp` are **absolute prices** and must respect broker **StopsLevel** / min distance.
* Use `--deviation` to control tolerance in volatile markets.
* Check **lot min/step/max** before trading (see **[Limits](../Market_Data/Limits.md)**).

---

## Method Signatures

```csharp
public Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    TimeSpan? pollInterval = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public Task<ulong> SendMarketOrderAsync(
    string symbol,
    bool isBuy,
    double volume,
    int deviation,
    double? stopLoss = null,
    double? takeProfit = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

---

## Code Reference 🧩

```csharp
var sell = new Command("sell", "Market sell");
sell.AddAlias("s");

sell.AddOption(profileOpt);
sell.AddOption(symbolOpt);
sell.AddOption(volumeOpt);
sell.AddOption(slOpt);
sell.AddOption(tpOpt);
sell.AddOption(devOpt);
sell.AddOption(outputOpt);
sell.AddOption(timeoutOpt);
sell.AddOption(dryRunOpt);

sell.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var symbolV   = ctx.ParseResult.GetValueForOption(symbolOpt);
    var volume    = ctx.ParseResult.GetValueForOption(volumeOpt);
    var sl        = ctx.ParseResult.GetValueForOption(slOpt);
    var tp        = ctx.ParseResult.GetValueForOption(tpOpt);
    var deviation = ctx.ParseResult.GetValueForOption(devOpt);
    var output    = ctx.ParseResult.GetValueForOption(outputOpt) ?? "text";
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureVolume(volume);
    Validators.EnsureDeviation(deviation);

    var s = Validators.EnsureSymbol(symbolV ?? GetOptions().DefaultSymbol);

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:SELL Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol}", s))
    using (_logger.BeginScope("OrderParams Vol:{Vol} Dev:{Dev} SL:{SL} TP:{TP}", volume, deviation, sl, tp))
    {
        if (dryRun)
        {
            if (string.Equals(output, "json", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(JsonSerializer.Serialize(new { DryRun = true, Side = "SELL", Symbol = s, Volume = volume, Deviation = deviation, SL = sl, TP = tp }));
            }
            else
            {
                Console.WriteLine($"[DRY-RUN] SELL {s} vol={volume} dev={deviation} SL={sl} TP={tp}");
            }
            return;
        }

        try
        {
            await ConnectAsync();

            // Best‑effort visibility (non‑fatal if fails)
            try { await _mt5Account.EnsureSymbolVisibleAsync(s, TimeSpan.FromSeconds(3)); } catch { }

            // Send SELL
            var ticket = await _mt5Account.SendMarketOrderAsync(s, isBuy: false, volume: volume, deviation: deviation, stopLoss: sl, takeProfit: tp, cancellationToken: CancellationToken.None);

            if (string.Equals(output, "json", StringComparison.OrdinalIgnoreCase))
                Console.WriteLine(JsonSerializer.Serialize(new { Side = "SELL", Symbol = s, Volume = volume, Deviation = deviation, SL = sl, TP = tp, Ticket = ticket }));
            else
                _logger.LogInformation("SELL done: ticket={Ticket}", ticket);
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
```

---

## See also

* **[Buy](../Orders_Positions/Buy.md)** — market BUY
* **[Limits](../Market_Data/Limits.md)** — lot min/step/max
* **[Quote](../Market_Data/Quote.md)** — quick snapshot price
