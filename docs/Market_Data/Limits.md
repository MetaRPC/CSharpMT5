# Limits (`sym limits`) 📏

## What it Does

Shows **volume trading limits** for a symbol and the latest **quote**:

* **Minimum lot** (`min`)
* **Lot step** (`step`)
* **Maximum lot** (`max`)
* **Quote**: Bid / Ask / Time

Best‑effort ensures the symbol is visible before requests.

---

## Method Signatures

```csharp
public Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    TimeSpan? pollInterval = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public Task<TickData> SymbolInfoTickAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public Task<(double min, double step, double max)> GetVolumeConstraintsAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

## Input Parameters ⬇️

| Parameter       | Type   | Required | Description                                            |
| --------------- | ------ | -------- | ------------------------------------------------------ |
| `--profile, -p` | string | yes      | Which profile to use (from `profiles.json`).           |
| `--symbol, -s`  | string | no       | Target symbol (defaults to profile’s `DefaultSymbol`). |
| `--output, -o`  | string | no       | `text` (default) or `json`.                            |
| `--timeout-ms`  | int    | no       | Per‑RPC timeout in milliseconds (default: `30000`).    |

Aliases: `lim` (subcommand of `symbol`/`sym`).

---

## Output Fields ⬆️

**Text mode**

```
<SYMBOL>:
  Quote: Bid=<bid> Ask=<ask> Time=<iso>
  Volume: min=<min> step=<step> max=<max>
```

**JSON mode**

```json
{
  "symbol": "EURUSD",
  "quote": { "Bid": 1.23456, "Ask": 1.23470, "Time": "2025-09-02T14:22:33Z" },
  "volume": { "min": 0.01, "step": 0.01, "max": 100 }
}
```

---

## How to Use 🛠️

```powershell
# Text
dotnet run -- sym limits -p demo -s EURUSD

# JSON
dotnet run -- sym limits -p demo -s EURUSD -o json
```

---

## Code Reference 🧩

```csharp
var sym = symbol ?? GetOptions().DefaultSymbol;

// Best‑effort visibility
try { await _mt5Account.EnsureSymbolVisibleAsync(sym, TimeSpan.FromSeconds(3)); } catch (Exception ex) when (ex is not OperationCanceledException) { }

// Quote + limits
var tick = await _mt5Account.SymbolInfoTickAsync(sym);
var (min, step, max) = await _mt5Account.GetVolumeConstraintsAsync(sym);

Console.WriteLine($"{sym}:\n  Quote: Bid={tick.Bid} Ask={tick.Ask} Time={tick.Time}\n  Volume: min={min} step={step} max={max}");
```

