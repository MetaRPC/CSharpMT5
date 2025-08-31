# CSharpMT5 — Getting Started 🚀

A fast, script‑friendly **CLI** for MT5 via gRPC. Run one‑shot actions (quotes, orders, history) or wire it into scripts/CI.

---

## ✅ What you need

* **.NET 8+** (SDK)
* **MT5 account** (demo or live) and network access to your MT5 gateway
* Your **profiles.json** (connection + defaults)
* (Optional) **PowerShell** for shortcasts (helper aliases)

---

## 🧩 Install & Run

Clone and run from the repo root:

```powershell
# First run
dotnet restore

# General pattern
dotnet run -- <command> [options]
```

Examples:

```powershell
# Show profiles and connectivity
dotnet run -- profiles list
dotnet run -- profiles show -p default
dotnet run -- health -p default

# One quote
dotnet run -- quote -p default -s EURUSD
```

> Tip: use `-o json` for machine‑readable output where supported.

---

## 🔐 Profiles & credentials

Create **profiles.json** in the working directory (or in `Config/`, copied to build output). Minimal examples:

```json
{
  "default": {
    "AccountId": 111111,
    "Password": "YOUR_PASSWORD",
    "Host": "95.217.147.61",
    "Port": 443,
    "DefaultSymbol": "EURUSD",
    "DefaultVolume": 0.1
  },
  "demo": {
    "AccountId": 95591860,
    "Password": "YOUR_DEMO_PASSWORD",
    "ServerName": "MetaQuotes-Demo",
    "DefaultSymbol": "GBPUSD",
    "DefaultVolume": 0.2
  }
}
```

**Password priority:** the environment variable `MT5_PASSWORD` overrides the password in the selected profile.

Check your setup:

```powershell
dotnet run -- profiles show -p default
```

---

## ⚙️ Global options (most used)

| Option                  | Meaning                                   | Notes                                 |                 |
| ----------------------- | ----------------------------------------- | ------------------------------------- | --------------- |
| `-p, --profile <name>`  | Which profile from `profiles.json` to use | default is `default`                  |                 |
| `-s, --symbol <SYM>`    | Instrument, e.g., `EURUSD`                | defaults to profile’s `DefaultSymbol` |                 |
| `-v, --volume <lots>`   | Order volume in lots                      | defaults to profile’s `DefaultVolume` |                 |
| `--deviation <pts>`     | Max slippage in points                    | useful for market orders              |                 |
| \`-o, --output text     | json\`                                    | Output format                         | where supported |
| `--timeout-ms <ms>`     | Per‑RPC timeout                           | default `30000`                       |                 |
| `--dry-run`             | Print the action without sending          | safe preview                          |                 |
| `--verbose` / `--trace` | Increase logging                          | `trace` is very detailed              |                 |

---

## ▶️ First five commands

```powershell
# 1) See account summary
dotnet run -- info -p default

# 2) Check price
dotnet run -- q -p default -s EURUSD

# 3) Place a small demo buy (if your profile points to demo)
dotnet run -- buy -p demo -s EURUSD -v 0.10 --sl 1.0700 --tp 1.0800 --deviation 10

# 4) Move SL to breakeven by points
dotnet run -- breakeven -p demo -t 123456 --offset-points 20

# 5) Export recent history to CSV
dotnet run -- history.export -p demo -d 7 --to csv --file C:\\temp\\hist.csv
```

> Use `--dry-run` to preview what would be sent without touching the account.

---

## ⚡ Shortcasts (optional)

Load aliases from `ps/shortcasts.ps1` once per session:

```powershell
. .\ps\shortcasts.ps1
use-pf demo      # default profile
use-sym EURUSD   # default symbol
use-to 90000     # default timeout

info             # expands to: mt5 info -p demo --timeout-ms 90000
q                # expands to: mt5 quote -p demo -s EURUSD --timeout-ms 90000
b -v 0.10        # market buy with defaults
```

See also: **CLI Shortcasts & Live Examples**.

---

## 🧠 SL/TP rules (quick)

* **BUY**: enter at **Ask** → `SL < Ask`, `TP > Ask`
* **SELL**: enter at **Bid** → `SL > Bid`, `TP < Bid`
* Use `position.modify.points` with `--from entry|market` to set distances in **points**.

---

## ⏱ Timeouts & retries

* `--timeout-ms` bounds each RPC. Internally we wrap operations in `UseOpTimeout` and per‑call CTS via `StartOpCts`.
* Calls go through `CallWithRetry(...)` to automatically retry selected transient errors.
* For CI, reduce timeout (fast fail). For slow terminals, increase to 60–120s.

---

## 🛟 Troubleshooting

* **“Set Host or MtClusterName”** → profile not picked up. Run `profiles show` and verify `profiles.json` path.
* **Hidden symbol** → `symbol ensure -s <SYM>` before trading or pending changes.
* **Timeouts** → raise `--timeout-ms`, test with `--trace` to see where it stuck.
* **Zero Margin/FreeMargin** on empty accounts is normal; equity ≈ balance when flat.

---

## 🔗 What next

* **Profiles** → details & tips: `Profiles_Reference.md`
* **Account / Info** → `Info.md`
* **Market data** → `Quote.md`, `Symbol/Show.md`, `Symbol/Limits.md`, `Symbol/Ensure.md`
* **Orders & Positions** → `Orders_Positions_Overview.md`
* **History** → `History.md`, `History_Export.md`
* **Misc tools** → `Ticket.md`, `Lot_Calc.md`, `Panic.md`

If something is unclear, open an issue or ping in the repo discussions — happy to help! 🎯
