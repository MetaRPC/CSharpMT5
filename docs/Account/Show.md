# Show (`sym show`) 🔍

## What it Does

Shows a short card for a symbol: **last quote (Bid/Ask/Time)** and **volume limits (min/step/max)**.
Best-effort makes the symbol visible in the terminal before requests.

---

## Method Signatures

```csharp
// Ensures that a symbol is visible in terminal UI (best-effort).
public Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    CancellationToken cancellationToken = default);

// Returns last tick (Bid/Ask/Time) for a symbol.
public Task<TickData> SymbolInfoTickAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// Returns (min, step, max) volume constraints for a symbol.
public Task<(double min, double step, double max)> GetVolumeConstraintsAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

## Code Reference 🧩

```csharp
// --- Quick use ---
// Get quote + volume limits for a symbol.
var sym = symbol ?? GetOptions().DefaultSymbol;

// Ensure symbol is visible (best-effort)
await _mt5Account.EnsureSymbolVisibleAsync(sym);

// Quote
var tick = await _mt5Account.SymbolInfoTickAsync(sym);

// Volume limits
var (min, step, max) = await _mt5Account.GetVolumeConstraintsAsync(sym);

// Print
Console.WriteLine($"{sym}: Bid={tick.Bid} Ask={tick.Ask} Time={tick.Time}");
Console.WriteLine($"Volume: min={min} step={step} max={max}");

// --- JSON output example ---
// Console.WriteLine(ToJson(new {
//     symbol = sym,
//     quote = tick,
//     volume = new { min, step, max }
// }));
```

## Input Parameters ⬇️

| Parameter       | Type   | Required | Description                                          |
| --------------- | ------ | -------- | ---------------------------------------------------- |
| `--profile, -p` | string | yes      | Which profile to use (from `profiles.json`).         |
| `--symbol, -s`  | string | no       | Symbol name (defaults to profile’s `DefaultSymbol`). |
| `--output`      | string | no       | Output format: `text` (default) or `json`.           |
| `--timeout-ms`  | int    | no       | Per-RPC timeout in milliseconds (default: `30000`).  |

---

## Output Fields ⬆️

| Field         | Type   | Description              |
| ------------- | ------ | ------------------------ |
| `Quote.Bid`   | double | Best bid price           |
| `Quote.Ask`   | double | Best ask price           |
| `Quote.Time`  | string | Server time of the quote |
| `Volume.min`  | double | Minimal allowed volume   |
| `Volume.step` | double | Volume step              |
| `Volume.max`  | double | Maximum allowed volume   |

---

## How to Use 🛠️

### CLI

```powershell
dotnet run -- sym show -p demo
dotnet run -- sym show -p demo -s EURUSD
dotnet run -- sym show -p demo -s XAUUSD --output json
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
sym show            # uses DefaultSymbol from the profile
sym show -s EURUSD  # explicit symbol
```
---

## When to Use ❓

* **Quick diagnostics** — quickly check the quotation and volume limits.
* **Before placing orders** — verify the available volumes and that the symbol is visible.
* **Environment check** — to make sure that the server returns the correct data on the tool.

