# Reverse (`reverse`) 🔄

## What it Does 🎯

Reverses the position(s) on a given symbol — closes current exposure and opens a new position in the opposite direction.

Variants:

* **`reverse`** → by symbol (net exposure or all positions).
* **`reverse.ticket`** → by exact ticket ID.

---

## Input Parameters ⬇️

### For `reverse`

| Parameter         | Type   | Required | Description                                |
| ----------------- | ------ | -------- | ------------------------------------------ |
| `--profile`, `-p` | string | ✅        | Profile from `profiles.json`.              |
| `--symbol`, `-s`  | string | ✅        | Symbol to reverse.                         |
| `--mode`          | string | ❌        | Reverse mode: `net` (default) or `all`.    |
| `--sl`            | double | ❌        | Optional Stop Loss for the new position.   |
| `--tp`            | double | ❌        | Optional Take Profit for the new position. |
| `--deviation`     | int    | ❌        | Max slippage in points (default: 10).      |
| `--output`, `-o`  | string | ❌        | Output: `text` (default) or `json`.        |
| `--timeout-ms`    | int    | ❌        | Timeout in ms (default: 30000).            |
| `--dry-run`       | flag   | ❌        | Show action plan without sending orders.   |

### For `reverse.ticket`

| Parameter         | Type   | Required | Description                        |
| ----------------- | ------ | -------- | ---------------------------------- |
| `--profile`, `-p` | string | ✅        | Profile.                           |
| `--ticket`, `-t`  | ulong  | ✅        | Ticket of the position to reverse. |
| `--sl`            | double | ❌        | Optional Stop Loss.                |
| `--tp`            | double | ❌        | Optional Take Profit.              |
| `--deviation`     | int    | ❌        | Slippage tolerance.                |
| `--output`, `-o`  | string | ❌        | Output: `text` or `json`.          |
| `--timeout-ms`    | int    | ❌        | Timeout in ms.                     |
| `--dry-run`       | flag   | ❌        | Plan only, no execution.           |

---

## Output Fields ⬆️

| Field      | Type   | Description                                                       |
| ---------- | ------ | ----------------------------------------------------------------- |
| `Closed[]` | array  | Info on positions closed (ticket, symbol, volume).                |
| `Opened`   | object | Info on the new opposite position (ticket, symbol, volume, side). |
| `Status`   | string | `OK` / `Error` with message.                                      |

---

## How to Use 🛠️

### CLI

```powershell
# Reverse by symbol (net exposure)
dotnet run -- reverse -p demo -s EURUSD --mode net

# Reverse by ticket ID
dotnet run -- reverse.ticket -p demo -t 123456 --sl 1.0950 --tp 1.1050
```

### PowerShell Shortcuts (from `shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
rv -s EURUSD        # reverse net exposure on EURUSD
rvt -t 123456       # reverse specific ticket
```

---

## When to Use ❓

* **Strategy flip** — when algo/opinion changes direction.
* **Stop-and-reverse systems** — common pattern in trend-following bots.
* **Manual kill/flip** — quickly switch side during volatile news.

---

## Notes & Safety 🛡️

* Ensure enough free margin for the new opposite position.
* Closing + opening may not be atomic; there can be small slippage.
* `--mode all` can be heavy: closes all tickets of the symbol before reversing.
* `--dry-run` is recommended to preview what will be closed/opened.

---

## Code Reference (to be filled by you) 🧩

```csharp
var modeOpt = new Option<string>(
    name: "--mode",
    description: "Reverse mode: net (single opposite order 2x) | flat (close all for symbol, then open 1x)",
    getDefaultValue: () => "net");

var reverse = new Command("reverse", "Reverse net position for a symbol");
reverse.AddAlias("rv");

reverse.AddOption(profileOpt);
reverse.AddOption(symbolOpt);
reverse.AddOption(modeOpt);
// reuse SL/TP/deviation so user can set protective exits for the new leg
reverse.AddOption(slOpt);
reverse.AddOption(tpOpt);
reverse.AddOption(devOpt);

reverse.SetHandler(async (string profile, string? symbol, string mode, double? sl, double? tp, int deviation, int timeoutMs, bool dryRun) =>
{
```
