# Sell (`sell`) 📉

## What it Does

Sends a **market SELL order** (opens/extends a short position) on the selected symbol via `_mt5Account.SendMarketOrderAsync(..., isBuy: false, ...)`.

---

## Input Parameters ⬇️

| Parameter         | Type    | Description                                                     |
| ----------------- | ------- | --------------------------------------------------------------- |
| `--profile`, `-p` | string  | Profile from `profiles.json`.                                   |
| `--symbol`, `-s`  | string  | Symbol (e.g., `EURUSD`). Defaults to profile’s `DefaultSymbol`. |
| `--volume`, `-v`  | double  | Volume in lots.                                                 |
| `--sl`            | double? | Stop Loss price (absolute).                                     |
| `--tp`            | double? | Take Profit price (absolute).                                   |
| `--deviation`     | int     | Max slippage (points). Default: `10`.                           |
| `--output`, `-o`  | string  | `text` (default) or `json`.                                     |
| `--timeout-ms`    | int     | Per‑RPC timeout in ms (default: `30000`).                       |
| `--dry-run`       | flag    | Print intended action without sending order.                    |

---

## Output ⬆️

**Text (logger)**

```
SELL done: ticket=12345678
```

**JSON**

```json
{
  "Side": "SELL",
  "Symbol": "EURUSD",
  "Volume": 0.1,
  "Deviation": 10,
  "SL": 1.0000,
  "TP": 1.2000,
  "Ticket": 12345678
}
```

---

## How to Use 🛠️

### CLI

```powershell
# Basic sell (text output)
dotnet run -- sell -p demo -s EURUSD -v 0.10

# With SL/TP + JSON
dotnet run -- sell -p demo -s EURUSD -v 0.10 --sl 1.1000 --tp 1.0800 -o json

# Dry‑run (no order sent)
dotnet run -- sell -p demo -s EURUSD -v 0.10 --dry-run
```

### PowerShell Shortcuts (from `ps/shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
s -s EURUSD -v 0.10 -sl 1.1000 -tp 1.0800
# expands to: mt5 sell -p demo -s EURUSD -v 0.10 --sl 1.1000 --tp 1.0800 --timeout-ms 90000
```

---

## Notes & Safety 🛡️

* Ensures symbol is **visible** in Market Watch before sending (best‑effort).
* **Preflight** pulls a fresh tick to validate/adjust SL/TP via `PreflightStops` for SELL logic.
* Use `--deviation` to control execution tolerance in volatile markets.

---

## Code Reference (exact) 💻

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
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:SELL Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol}", s))
    using (_logger.BeginScope("OrderParams Vol:{Vol} Dev:{Dev} SL:{SL} TP:{TP}", volume, deviation, sl, tp))
    {
        if (dryRun)
        {
            if (string.Equals(output, "json", StringComparison.OrdinalIgnoreCase))
            {
                var payload = new { DryRun = true, Side = "SELL", Symbol = s, Volume = volume, Deviation = deviation, SL = sl, TP = tp };
                Console.WriteLine(JsonSerializer.Serialize(payload));
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
```
