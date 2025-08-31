# Getting an Account Summary (`info`) 📟

Fetch a **real‑time account snapshot** from MT5 and print it in **text** (human) or **JSON** (machine‑readable). Use it to verify connectivity, monitor balances/equity, or script health checks.

---

## 🔎 Synopsis

```powershell
dotnet run -- info -p <profile> [-o text|json] [--timeout-ms <ms>]
```

> Tip: Prefer `-o json` when piping into tools, dashboards, or CI.

---

## ⬇️ Input parameters

| Option                 | Type   | Required | Description                                             |
| ---------------------- | ------ | :------: | ------------------------------------------------------- |
| `-p, --profile <name>` | string |     ✅    | Profile from `profiles.json` (login, server, password). |
| `-o, --output <fmt>`   | string |     ❌    | `text` (default) or `json`.                             |
| `--timeout-ms <ms>`    | int    |     ❌    | Per‑RPC timeout (default **30000**).                    |

---

## ⬆️ Output fields

Values are composed from `AccountSummaryData` + additional `AccountInformation` reads.

| Field        | Type     | Meaning                         |
| ------------ | -------- | ------------------------------- |
| `Login`      | int64    | Account ID (login).             |
| `UserName`   | string   | Account holder name.            |
| `Currency`   | string   | Deposit currency (USD/EUR/…).   |
| `Balance`    | double   | Balance excluding floating P/L. |
| `Equity`     | double   | Balance incl. floating P/L.     |
| `Leverage`   | int      | e.g. 500.                       |
| `TradeMode`  | enum     | Demo/Real/etc.                  |
| `Company`    | string   | Broker name.                    |
| `Margin`     | double   | Used margin.                    |
| `FreeMargin` | double   | Available margin.               |
| `ServerTime` | DateTime | Server time (UTC).              |
| `UTC Shift`  | int      | Timezone offset in minutes.     |

---

## 🧪 Examples

\=== "CLI (JSON)"

````
???+ example "Code example — CLI with JSON output"

    ```powershell
    # Full JSON snapshot (good for scripting)
    dotnet run -- info -p demo -o json --timeout-ms 90000
    ```
````

\=== "CLI (text)"

````
???+ example "Code example — human‑readable text"

    ```powershell
    # Compact, readable dump
    dotnet run -- info -p demo -o text
    ```
````

\=== "PowerShell Shortcasts"

````
???+ example "Code example — using aliases from ps/shortcasts.ps1"

    ```powershell
    . .\ps\shortcasts.ps1
    use-pf demo
    use-to 90000
    info   # expands to: mt5 info -p demo --timeout-ms 90000
    ```
````

\=== "C# API"

````
???+ example "Code example — call from your C# app"

    ```csharp
    var summary = await _mt5Account.AccountSummaryAsync();

    _logger.LogInformation("=== Account Info ===");
    _logger.LogInformation("Login: {0}", summary.AccountLogin);
    _logger.LogInformation("Balance: {0}", summary.AccountBalance);
    _logger.LogInformation("Equity: {0}", summary.AccountEquity);
    // ... leverage, trade mode, margin, free margin, company, etc.
    ```
````

---

## ❓ When to use

* **Before sending orders** — confirm equity, free margin, leverage.
* **Monitoring** — feed JSON to dashboards/alerts.
* **Diagnostics** — verify terminal connection + credentials.
* **Risk control** — margin usage before high‑risk trades.
---

## 🔗 Related

* Profiles → **[Account/Profiles](../Account/Profiles.md)**
* Health checks → **[Diagnostics/Health](../Diagnostics/Health.md)**
* Timeouts & retries → **[Timeouts\_RetriesPolicy](../Timeouts_RetriesPolicy.md)**
* Troubleshooting → **[Troubleshooting & FAQ](../Troubleshooting%28FAQ%29.md)**
