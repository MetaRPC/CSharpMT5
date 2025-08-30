# Getting an Account Summary (`info`) 📟

**Goal:** Fetch `AccountSummaryData` from MT5 and print a one‑shot account snapshot (text or JSON).

**Architecture (Under the hood):**

```
                ┌─────────────────────────────────────┐
                │        💻 MT5 Terminal / Server     │
                │ (broker connection, quotes, orders) │
                └───────────────────┬─────────────────┘
                                    │ gRPC
                                    ▼
                 ┌──────────────────────────────────┐
                 │   🛰️ MT5 gRPC Services (stubs)   │
                 │  AccountHelper / TradingHelper   │
                 └───────────────┬──────────────────┘
                                 │
                                 ▼
          ┌────────────────────────────────────────────┐
          │  ⚙️ C# CLI App (Program.cs)                │
          │  • info handler (System.CommandLine)       │
          │  • Validators.EnsureProfile / UseOpTimeout │
          │  • calls _mt5Account.AccountSummaryAsync() │
          └───────────────┬────────────────────────────┘
                          │
                          ▼
          ┌────────────────────────────────────────────┐
          │  📦 _mt5Account (service wrapper)          │
          │  • builds protobuf request                  │
          │  • invokes AccountHelper.AccountSummary()   │
          └───────────────┬────────────────────────────┘
                          │
          ┌───────────────┴───────────────┐
          │                               │
          ▼                               ▼
┌──────────────────────┐       ┌────────────────────────┐
│ 🖥️ Text output        │       │ 🧾 JSON output          │
│ (logger/console)     │       │ (for scripts/CI)       │
└──────────────────────┘       └────────────────────────┘

(Optionally via shortcuts)
┌───────────────────────────────────────────────────────────┐
│  📜 PowerShell shortcuts (ps/shortcasts.ps1)               │
│  • mt5 info ...   • use-pf demo   • info -p demo -t 90000 │
└───────────────────────────────────────────────────────────┘
```

---

## Why / When to Use ❓

* **Monitoring**: check account health before trading (balance, equity, margin).
* **Automation**: feed JSON summary into CI/CD pipelines or monitoring dashboards.
* **Diagnostics**: verify connection to MT5 and correct profile configuration.
* **Risk control**: ensure free margin and leverage are correct before sending orders.

---

## Quick Code Example 🧩

```csharp
Validators.EnsureProfile(profile);
using (UseOpTimeout(timeoutMs))
{
    await ConnectAsync();
    var summary = await _mt5Account.AccountSummaryAsync();
    Console.WriteLine(output == "json"
        ? JsonSerializer.Serialize(summary)
        : $"Balance: {summary.AccountBalance}");
    try { await _mt5Account.DisconnectAsync(); } catch { }
}
```

---

## Quick Access Commands ⚙️

### Plain .NET

```powershell
dotnet run -- info -p demo --output json --timeout-ms 90000
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
use-to 90000
info
```
