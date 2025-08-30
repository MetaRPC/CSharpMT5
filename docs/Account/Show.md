# Show (`show`) 🔍

## What it Does 🎯

Lists available symbols from MT5 terminal.
By default prints visible (active) symbols, but can also fetch the entire symbol catalog.

---

## Input Parameters ⬇️

| Parameter      | Type   | Required | Description                                            |
| -------------- | ------ | -------- | ------------------------------------------------------ |
| `--profile`    | string | ✅        | Which profile to use (from `profiles.json`).           |
| `--output`     | string | ❌        | Output format: `text` (default) or `json`.             |
| `--timeout-ms` | int    | ❌        | Per-RPC timeout in milliseconds (default: 30000).      |
| `--all`        | flag   | ❌        | If set, lists **all symbols** (not only visible ones). |

---

## Output Fields ⬆️

Each symbol entry typically includes:

| Field     | Type   | Description                            |
| --------- | ------ | -------------------------------------- |
| `Symbol`  | string | Symbol name (e.g. EURUSD, XAUUSD).     |
| `Visible` | bool   | Whether symbol is currently visible.   |
| `Digits`  | int    | Number of decimal digits (precision).  |
| `Trade`   | bool   | Whether symbol is allowed for trading. |

---

## How to Use 🛠️

### CLI

```powershell
dotnet run -- show -p demo
dotnet run -- show -p demo --all --output json
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
show         # lists visible symbols
show --all   # lists entire catalog
```

---

## When to Use ❓

* **Before placing orders** — check if a symbol is visible and tradable.
* **Diagnostics** — verify server provides expected instruments.
* **Setup** — ensure required instruments are subscribed before trading.

---

## Code Reference 🧩

```csharp
using var opCts = StartOpCts();
var symbols = await CallWithRetry(
    ct => _mt5Account.SymbolsAsync(all: listAll, cancellationToken: ct),
    opCts.Token);

if (IsJson(output)) Console.WriteLine(ToJson(symbols));
else
{
    foreach (var s in symbols.Items)
        _logger.LogInformation("{Symbol}  Visible={Vis}  Digits={Digits}  Trade={Trade}",
            s.Symbol, s.IsVisible, s.Digits, s.TradeMode);
}
```

---

📌 In short:
— `show` = list of instruments.
— Supports filtering (visible vs all).
— Works with profiles + timeouts the same as other commands.
