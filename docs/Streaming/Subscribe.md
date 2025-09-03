# Stream (`stream`) 📡

## What it does

Subscribes to **real‑time events** and writes concise text logs:

* **Ticks for a symbol** (prints **only** `Symbol` and `Ask`).
* Trade events (short service line).
* Position P\&L updates (short service line).
* Position & pending‑order tickets (short service line).

Keeps **auto‑reconnecting** until the requested duration elapses.

---

## Input parameters ⬇️

| Parameter         | Type   | Description                                                                                                                                                      |
| ----------------- | ------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `--profile`, `-p` | string | Profile name from `profiles.json`.                                                                                                                               |
| `--seconds`, `-S` | int    | How many seconds to run. **Default:** `10`.                                                                                                                      |
| `--symbol`, `-s`  | string | Desired symbol. **Current behavior:** the subscription internally uses the **profile’s `DefaultSymbol`**; this flag **does not change** the actual subscription. |
| `--timeout-ms`    | int    | Global per‑RPC timeout (applies to RPC calls). **Default:** `30000`.                                                                                             |

> ⚠️ **Not supported:** `--output` and JSON mode. The command prints **text logs only**.

---

## What gets printed ⬆️

### Ticks

One line per tick:

```
OnSymbolTickAsync: Symbol=EURUSD Ask=1.23456
```

Fields `Bid`, `Time`, `Mid`, `Spread` are **not** printed.

### Other event lines

Periodic service lines may appear:

```
OnTradeAsync: Trade event received
OnPositionProfitAsync: Update received
OnPositionsAndPendingOrdersTicketsAsync: Update received
```

---

## How to use 🛠️

### CLI

```powershell
# 10 seconds of streaming (text logs)
dotnet run -- stream -p demo -s EURUSD --seconds 10

# Command alias
dotnet run -- st -p demo -S 15 -s XAUUSD
```

### PowerShell shortcuts (from `ps/shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
use-sym EURUSD
st 15 EURUSD   # expands to: mt5 stream -p demo --seconds 15 -s EURUSD --timeout-ms 90000
```

---

## Notes 🧩

* Before subscribing, the code calls **`EnsureSymbolVisibleAsync(symbol, ~3s)`** to avoid “symbol not selected”. Prep errors are non‑fatal.
* The actual subscription uses **`DefaultSymbol` from the profile**, not the `--symbol` value.
* **Auto‑reconnect with backoff** is implemented and runs until `--seconds` is reached.
* On completion/cancellation the command logs a summary and attempts a clean disconnect.
* On connection/stream setup failures the process sets `Environment.ExitCode = 1`.

---

## Method signatures

```csharp
// Ensure a symbol is visible in the terminal (best‑effort wait)
Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    TimeSpan? pollInterval = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// Subscribe to real‑time ticks (code uses profile DefaultSymbol)
IAsyncEnumerable<OnSymbolTickData> OnSymbolTickAsync(
    IEnumerable<string> symbols,
    CancellationToken cancellationToken = default);

// Trade‑related events
IAsyncEnumerable<OnTradeData> OnTradeAsync(
    CancellationToken cancellationToken = default);

// Periodic/snapshot P&L updates
IAsyncEnumerable<OnPositionProfitData> OnPositionProfitAsync(
    int intervalMs,
    bool ignoreEmpty = true,
    CancellationToken cancellationToken = default);

// Tickets for positions & pending orders
IAsyncEnumerable<OnPositionsAndPendingOrdersTicketsData> OnPositionsAndPendingOrdersTicketsAsync(
    int intervalMs,
    CancellationToken cancellationToken = default);
```

---

## Sample logs 🧾

```
info: Cmd:STREAM Profile:demo
info: Symbol:EURUSD Seconds:10
info: Streaming started (auto-reconnect enabled).
info: OnSymbolTickAsync: Symbol=EURUSD Ask=1.09321
info: OnPositionProfitAsync: Update received
info: OnTradeAsync: Trade event received
info: OnPositionsAndPendingOrdersTicketsAsync: Update received
info: Streaming stopped. Elapsed=10.0s
```
