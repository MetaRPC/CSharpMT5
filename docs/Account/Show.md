# Show (`show`) 🔍

## What it Does 

Lists available symbols from the MT5 terminal.
By default prints **visible (active)** symbols, but can also fetch the **entire symbol catalog**.

---

## Input Parameters ⬇️

| Parameter      | Type   | Required | Description                                            |
| -------------- | ------ | :------: | ------------------------------------------------------ |
| `--profile`    | string |     ✅    | Which profile to use (from `profiles.json`).           |
| `--output`     | string |     ❌    | Output format: `text` (default) or `json`.             |
| `--timeout-ms` | int    |     ❌    | Per-RPC timeout in milliseconds (default: 30000).      |
| `--all`        | flag   |     ❌    | If set, lists **all symbols** (not only visible ones). |

---

## Output Fields ⬆️

Each symbol entry typically includes:

| Field     | Type   | Description                             |
| --------- | ------ | --------------------------------------- |
| `Symbol`  | string | Symbol name (e.g., `EURUSD`, `XAUUSD`). |
| `Visible` | bool   | Whether symbol is currently visible.    |
| `Digits`  | int    | Number of decimal digits (precision).   |
| `Trade`   | bool   | Whether symbol is allowed for trading.  |

---

## How to Use 🛠️

???+ example "CLI"
\`\`\`powershell
\# Visible symbols only (default)
dotnet run -- show -p demo

````
# Entire catalog in JSON (good for scripting)
dotnet run -- show -p demo --all -o json
```
````

???+ tip "PowerShell Shortcuts"
\`\`\`powershell
. .\ps\shortcasts.ps1
use-pf demo

````
show         # lists visible symbols
show --all   # lists entire catalog
```
````

---

## When to Use ❓

* **Before placing orders** — check if a symbol is visible and tradable.
* **Diagnostics** — verify the server exposes expected instruments.
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

## Notes & Troubleshooting 🧯

!!! note "Symbol visibility"
Many brokers hide most instruments by default. To trade a symbol reliably, make it visible first — see **[Ensure Symbol Visible](../Market_Data/Ensure_Symbol_Visible.md)**.

!!! warning "Broker suffixes"
If a symbol is missing, try broker suffixes like `EURUSD.m` or `XAUUSD.a`. Check your broker's naming.

!!! tip "Deeper symbol fields"
For tick size/point, contract size, filters for SL/TP, and margin settings, see **[Symbol](../Market_Data/Symbol.md)**.

---

## 🔗 Related

* Market data → **[Quote](../Market_Data/Quote.md)** · **[Symbol](../Market_Data/Symbol.md)** · **[Ensure Symbol Visible](../Market_Data/Ensure_Symbol_Visible.md)**
* Orders → **[Place](../Orders_Positions/Place.md)** · **[Modify](../Orders_Positions/Modify.md)**

---

📌 **In short**

* `show` = list of instruments.
* `--all` toggles between visible vs. full catalog.
* Profiles and timeouts behave the same as other commands.
