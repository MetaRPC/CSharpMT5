
# Health (`health`) 🩺

## What it Does

Performs a quick **health check** of the MT5 connection: validates profile, checks TCP reachability to the server, opens a session, and queries basic RPCs.

---

## Input Parameters ⬇️

| Parameter       | Type   | Required | Description                                         |
| --------------- | ------ | -------- | --------------------------------------------------- |
| `--profile, -p` | string | yes      | Which profile to use (from `profiles.json`).        |
| `--output`      | string | no       | Output format: `text` (default) or `json`.          |
| `--timeout-ms`  | int    | no       | Per-RPC timeout in milliseconds (default: `30000`). |

Aliases: `ping`.

---

## Output Fields ⬆️

Actual report keys:

| Field        | Type   | Description                                            |
| ------------ | ------ | ------------------------------------------------------ |
| `profile`    | string | Selected profile name.                                 |
| `accountId`  | int64  | Account login from the profile.                        |
| `serverName` | string | MT5 server name from the profile.                      |
| `host`       | string | Host used for TCP check and gRPC endpoint.             |
| `port`       | int    | Port used for TCP check.                               |
| `tcp`        | string | TCP reachability status: `ok` or `fail: <message>`.    |
| `terminal`   | string | Terminal/gRPC status: `ok` or `fail: <message>`.       |
| `balance`    | double | Account balance (present if terminal check succeeded). |

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
health  # alias: ping
```

---

## When to Use ❓

* **Connection test** — before running other commands (info, buy/sell).
* **Monitoring** — periodic checks for automation/dashboards.
* **Diagnostics** — distinguish between network problems and terminal failures.

---

## Code Reference 🧩

```csharp
// Quick connectivity probe via AccountSummary
try
{
    // Profile must be selected and connection established by your app’s flow
    var summary = await _mt5Account.AccountSummaryAsync();
    Console.WriteLine($"Health: ok. Balance={summary.AccountBalance}");
}
catch (Exception ex)
{
    Console.WriteLine($"Health: fail: {ex.Message}");
}
```

### Method Signature

```csharp
public Task<AccountSummaryData> AccountSummaryAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```
