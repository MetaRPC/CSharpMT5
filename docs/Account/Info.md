# Getting an Account Summary (`info`) 📟

Fetches **real‑time account snapshot** from MT5 and prints it either in **text** (console) or **JSON** (machine‑readable).
Used for checking account state, verifying connectivity, and quick diagnostics.

---

## Input Parameters ⬇️

| Parameter      | Type   | Required | Description                                                                  |
| -------------- | ------ | :------: | ---------------------------------------------------------------------------- |
| `--profile`    | string |     ✅    | Which profile to use (from `profiles.json` — holds login, server, password). |
| `--output`     | string |     ❌    | Output format: `text` (default) or `json`.                                   |
| `--timeout-ms` | int    |     ❌    | Per‑RPC timeout in milliseconds (default: 30000).                            |

---

## Output Fields ⬆️

Printed from `AccountSummaryData` + extra info via `AccountInformation`:

| Field        | Type     | Description                          |
| ------------ | -------- | ------------------------------------ |
| `Login`      | int64    | Account ID (login).                  |
| `UserName`   | string   | Account holder’s name.               |
| `Currency`   | string   | Deposit currency (e.g. USD, EUR).    |
| `Balance`    | double   | Current balance excluding open P/L.  |
| `Equity`     | double   | Balance including floating P/L.      |
| `Leverage`   | int      | Account leverage (e.g. 500).         |
| `TradeMode`  | enum     | Account trade mode (e.g. Demo/Real). |
| `Company`    | string   | Broker name.                         |
| `Margin`     | double   | Currently used margin.               |
| `FreeMargin` | double   | Margin still available for trading.  |
| `ServerTime` | DateTime | Server time in UTC.                  |
| `UTC Shift`  | int      | Timezone offset in minutes.          |

---

## How to Use 🛠️

### Full CLI

```powershell
dotnet run -- info -p demo --output json --timeout-ms 90000
```

### PowerShell Shortcuts (from `ps/shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
use-to 90000
info
```

* `use-pf demo` → choose profile `demo` once.
* `use-to 90000` → set default timeout (ms).
* `info` → expands to `mt5 info -p demo --timeout-ms 90000`.

---

## When to Use ❓

* **Before sending orders** — check equity, free margin, leverage.
* **Monitoring** — feed JSON into dashboards, CI/CD or alerts.
* **Diagnostics** — confirm MT5 terminal is connected and profile credentials work.
* **Risk control** — margin usage visible before high‑risk trades.

---

## Code Reference 🧩

```csharp
var summary = await _mt5Account.AccountSummaryAsync();

_logger.LogInformation("=== Account Info ===");
_logger.LogInformation("Login: {0}", summary.AccountLogin);
_logger.LogInformation("Balance: {0}", summary.AccountBalance);
_logger.LogInformation("Equity: {0}", summary.AccountEquity);
// ... prints leverage, trade mode, margin, free margin, etc.
```
