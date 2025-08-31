# Quote (`quote`) 💬

## What it Does

Fetches a **snapshot price** for a symbol (Bid/Ask/Time) from MT5.
Used for quick checks, scripts, and preflight before placing orders.

---

## Input Parameters ⬇️

| Parameter         | Type   | Description                                  |
| ----------------- | ------ |  -------------------------------------------- |
| `--profile`, `-p` | string |  Which profile to use (from `profiles.json`). |
| `--symbol`, `-s`  | string |  Symbol to query (e.g., `EURUSD`).            |
| `--output`, `-o`  | string |  `text` (default) or `json`.                  |
| `--timeout-ms`    | int    |  RPC timeout in ms (default: 30000).          |

---

## Output Fields ⬆️

| Field     | Type     | Description                                             |
| --------- | -------- | ------------------------------------------------------- |
| `Symbol`  | string   | Instrument name.                                        |
| `Bid`     | double   | Best bid price.                                         |
| `Ask`     | double   | Best ask price.                                         |
| `TimeUtc` | DateTime | Snapshot server time (UTC).                             |
| `Mid`     | double   | (Derived) `(Bid + Ask) / 2`.                            |
| `Spread`  | double   | (Derived) `Ask - Bid`.                                  |
| `AgeMs`   | int      | (Derived) age of the quote in milliseconds.             |
| `Stale`   | bool     | (Derived) true if `AgeMs > 5000` (5s) — treat as stale. |

> JSON output contains raw fields of the snapshot; in text mode the CLI prints a concise line with bid/ask/time (+ derived metrics if implemented).

---

## How to Use 🛠️

### CLI

```powershell
# Text
dotnet run -- quote -p demo -s EURUSD --timeout-ms 30000

# JSON
dotnet run -- quote -p demo -s EURUSD -o json --timeout-ms 30000
```

### PowerShell Shortcuts (from `ps/shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
use-sym EURUSD
q              # expands to: mt5 quote -p demo -s EURUSD --timeout-ms 90000
q -s XAUUSD    # overrides default symbol
```

---

## Notes 🧩

* Before fetching a quote the code performs a **best-effort visibility check** for the symbol using `EnsureSymbolVisibleAsync(symbol, 3s, ct)`.
* If the quote is **stale** (e.g., `AgeMs > 5000`), tooling may mark it as `[STALE >5s]`.

---

## Code Reference 🧷

```csharp
var quoteCmd = new Command("quote", "Get a snapshot price (Bid/Ask/Time)");
quoteCmd.AddAlias("q");

quoteCmd.AddOption(profileOpt);
quoteCmd.AddOption(symbolOpt);
quoteCmd.AddOption(outputOpt);

quoteCmd.SetHandler(async (string profile, string? symbol, string output, int timeoutMs) =>
{
    Validators.EnsureProfile(profile);
    var s = Validators.EnsureSymbol(symbol ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:QUOTE Profile:{Profile}", profile))
    using (_logger.BeginScope("Symbol:{Symbol}", s))
    {
        try
        {
            await ConnectAsync();
```

---

📌 In short:
— `quote` = one-shot snapshot with Bid/Ask/Time (plus derived metrics when needed).
— Works with profiles, timeouts, and integrates the symbol-visibility helper.
— Use the **`q`** shortcast for fast terminal work.
