# Health (`health`) 🩺

## What it Does 🎯

Performs a quick **health check** of the MT5 connection.
Confirms that the profile is valid, the terminal/server is reachable, and basic RPCs respond.

---

## Input Parameters ⬇️

| Parameter      | Type   | Required | Description                                       |
| -------------- | ------ | -------- | ------------------------------------------------- |
| `--profile`    | string | ✅        | Which profile to use (from `profiles.json`).      |
| `--output`     | string | ❌        | Output format: `text` (default) or `json`.        |
| `--timeout-ms` | int    | ❌        | Per-RPC timeout in milliseconds (default: 30000). |

---

## Output Fields ⬆️

Health response usually includes:

| Field        | Type     | Description                                |
| ------------ | -------- | ------------------------------------------ |
| `Status`     | string   | Overall result (`OK`, `Error`, `Timeout`). |
| `AccountId`  | int64    | Account login if connection succeeded.     |
| `ServerTime` | DateTime | Current server time (if available).        |
| `LatencyMs`  | int      | Measured round-trip latency (approximate). |

---

## How to Use 🛠️

### CLI

```powershell
dotnet run -- health -p demo
dotnet run -- health -p demo --output json --timeout-ms 10000
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
health
```

---

## When to Use ❓

* **Connection test** — before running other commands (info, buy/sell).
* **Monitoring** — periodic checks for automation or dashboards.
* **Diagnostics** — distinguish between network issues vs. terminal issues.

---

## Code Reference 🧩

```csharp
using var opCts = StartOpCts();
var status = await CallWithRetry(
    ct => _mt5Account.HealthAsync(deadline: null, cancellationToken: ct),
    opCts.Token);

if (IsJson(output)) Console.WriteLine(ToJson(status));
else
    _logger.LogInformation("Health check: {Status}", status.Result);
```

---

📌 In short:
— `health` = ping-like command for MT5.
— Helps to quickly verify connectivity and latency.
— Safe to run anytime, does not affect trading state.
