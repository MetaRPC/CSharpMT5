# Quote (`quote`) 💬

## What it Does

Gets a **snapshot price** for one symbol: **Bid / Ask / Time**, plus derived metrics (**Mid**, **Spread**, **AgeMs**) in text mode. Best‑effort ensures the symbol is visible before requesting.

Alias: `q`.

---
## Method Signatures

```csharp
public Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    TimeSpan? pollInterval = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

private Task<QuoteDto> FirstTickAsync(string symbol, CancellationToken ct);
```

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                  |
| ----------------- | ------ | -------- | -------------------------------------------- |
| `--profile`, `-p` | string | yes      | Which profile to use (from `profiles.json`). |
| `--symbol`, `-s`  | string | no       | Symbol to query (defaults to profile’s).     |
| `--output`, `-o`  | string | no       | `text` (default) or `json`.                  |
| `--timeout-ms`    | int    | no       | RPC timeout in ms (default: 30000).          |

---

## Output Fields ⬆️

**Text mode:**

| Field     | Type      | Description                                        |
| --------- | --------- | -------------------------------------------------- |
| `Symbol`  | string    | Symbol name.                                       |
| `Bid`     | double    | Best bid price.                                    |
| `Ask`     | double    | Best ask price.                                    |
| `TimeUtc` | DateTime? | Server time of the tick.                           |
| `Mid`     | double    | `(Bid + Ask) / 2`.                                 |
| `Spread`  | double    | `Ask - Bid`.                                       |
| `AgeMs`   | double    | Age in ms since `TimeUtc` (NaN if time is absent). |

**JSON mode:**

```json
{
  "Symbol": "EURUSD",
  "Bid": 1.23456,
  "Ask": 1.23470,
  "TimeUtc": "2025-09-02T14:22:33Z"
}
```

---

## How to Use 🛠️

```powershell
# Text
dotnet run -- quote -p demo -s EURUSD

# JSON
dotnet run -- quote -p demo -s EURUSD -o json
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
use-sym EURUSD
q
```

---

## Notes 🧩

* Always tries `EnsureSymbolVisibleAsync(symbol, 3s, ct)` before requesting a tick.
* JSON output = raw server payload (no Mid/Spread/AgeMs).
* Text output enriches with derived metrics.

---

## Code Reference 🧷

```csharp
var s = symbol ?? GetOptions().DefaultSymbol;
await _mt5Account.EnsureSymbolVisibleAsync(s, TimeSpan.FromSeconds(3));

using var cts = StartOpCts();
var snap = await FirstTickAsync(s, cts.Token);

var mid    = (snap.Bid + snap.Ask) / 2.0;
var spread = snap.Ask - snap.Bid;
var ageMs  = snap.TimeUtc.HasValue ? Math.Abs((DateTime.UtcNow - snap.TimeUtc.Value).TotalMilliseconds) : double.NaN;
```

