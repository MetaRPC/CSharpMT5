# CSharpMT5 CLI — Shortcasts & Live Examples 🚀

> ⚙️ **General launch pattern (PowerShell / CMD):**
>
> ```powershell
> dotnet run -- <command> [options]
> ```

---

## 🌍 Global Options

| Option                  | Meaning                                           | Notes                      |                                     |
| ----------------------- | ------------------------------------------------- | -------------------------- | ----------------------------------- |
| `-p, --profile <name>`  | Profile from `profiles.json` (default: `default`) | Switch accounts quickly    |                                     |
| `-s, --symbol <SYM>`    | Instrument/ticker, e.g., `EURUSD`                 | Default taken from profile |                                     |
| `-v, --volume <lots>`   | Order volume in lots                              |                            |                                     |
| `--deviation <pts>`     | Max slippage in points                            |                            |                                     |
| \`-o, --output text     | json\`                                            | Output format              | `text` or `json` (machine-friendly) |
| `--timeout-ms <ms>`     | Per-RPC timeout (default: `30000`)                | Avoids hangs               |                                     |
| `--dry-run`             | Don’t send anything, just show what would be done | Great for testing          |                                     |
| `--verbose` / `--trace` | Extended logging                                  | Debug / trace everything   |                                     |

💡 **Tip:** set password via environment variable `MT5_PASSWORD` instead of hardcoding in files.

---

## 📑 Command Reference (Shortcasts)

**Format:** command — aliases — what it does

### 👤 Profiles

* `profiles list` — `ls` → list profile names
* `profiles show` — `view` → show profile parameters (uses `MT5_PASSWORD`)

### 📊 General / Info

* `info` — `i` → account summary
* `quote` — `q` → one-shot spot quote (Bid/Ask/Time)
* `health` — `ping` → connectivity diagnostics
* `stream` — `st` → tick/event stream (auto-reconnect)

### 💹 Market Orders

* `buy` — `b` → market buy
* `sell` — `s` → market sell

### 🔄 Closing / Reversing

* `close` — `c` → close by ticket (exact/normalized volume)
* `close.percent` — `cpp` → close by percent
* `close.half` — `ch` → close half (50%)
* `close.partial` — `cp` → close exact volume
* `close-all` — `flatten` → close all open positions
* `close-symbol` — `cs` → close all positions for a symbol
* `reverse` — `rv` → reverse **net** position (`--mode net|flat`)
* `reverse.ticket` — `rvt` → reverse a **specific** ticket

### 🎯 SL/TP & Safety

* `position.modify` — `posmod` → set SL/TP by **price**
* `position.modify.points` — `pmp` → set SL/TP by **points**
* `breakeven` — `be` → move SL to breakeven ± offset
* `trail.start` → start a local trailing stop
* `trail.stop` → stop trailing

### 📌 Pending Orders

* `place` — `pl` → place pending (limit/stop/stop-limit)
* `pending.modify` — `pm` → change pending params
* `pending.move` — `pmove` → shift price(s) by ±N pts
* `pending list` — `ls` → list pending tickets
* `cancel` — `x` → delete pending by ticket
* `cancel.all` — `ca` → delete all pendings (with filters)

### 🎟 Tickets / Symbols / Lists

* `ticket show` — `tsh` → show info for a ticket (open/recent)
* `positions` — `pos` → active positions
* `orders` — `ord` → open orders + positions tickets
* `symbol ensure` — `en` → make symbol visible
* `symbol limits` — `lim` → lot min/step/max
* `symbol show` — `sh` → symbol card (quote + limits)

### 📜 History / Tools

* `history` — `h` → orders/deals history for N days
* `history.export` — `hexport` → export history (CSV/JSON)
* `lot.calc` — `lc` → lot size from risk% & SL pts
* `panic` → close all + cancel all pendings

---

## 🚀 Quick Start

```powershell
# Profiles
dotnet run -- profiles list
dotnet run -- profiles show -p demo

# Connectivity
dotnet run -- health -p demo

# One quote
dotnet run -- q -p demo -s EURUSD
```

---

## 🛠 Practical Scenarios

### 1️⃣ Open & Manage Position

```powershell
dotnet run -- buy -p demo -s EURUSD -v 0.10 --sl 1.0700 --tp 1.0800 --deviation 10
dotnet run -- breakeven -p demo -t 123456 --offset-points 2
dotnet run -- position.modify.points -p demo -t 123456 --sl-points 150 --tp-points 250
dotnet run -- trail.start -p demo -t 123456 --distance 150 --step 20
dotnet run -- trail.stop -p demo -t 123456
```

### 2️⃣ Partial Close & Reverse

```powershell
dotnet run -- close.half -p demo -t 123456
dotnet run -- close.percent -p demo -t 123456 --pct 30
dotnet run -- close.partial -p demo -t 123456 --volume 0.05
dotnet run -- reverse -p demo -s EURUSD --mode net  --sl 1.0700 --tp 1.0850
dotnet run -- reverse.ticket -p demo -t 123456 --sl 1.0700 --tp 1.0850
```

### 3️⃣ Pendings

```powershell
dotnet run -- place -p demo --type buylimit  -s EURUSD --price 1.0750 --sl 1.0700 --tp 1.0800
dotnet run -- pending.modify -p demo -t 778899 --price 1.0765 --sl 1.0745
dotnet run -- pending.move -p demo -t 778899 --by-points 15
dotnet run -- pending list -p demo
dotnet run -- cancel -p demo -t 778899 -s EURUSD
dotnet run -- cancel.all -p demo --symbol EURUSD
```

### 4️⃣ Lists & Symbols

```powershell
dotnet run -- positions -p demo -o json
dotnet run -- orders -p demo
dotnet run -- symbol show -p demo -s EURUSD
dotnet run -- symbol ensure -p demo -s GBPUSD
dotnet run -- symbol limits -p demo -s USDJPY
```

### 5️⃣ Tickets & History

```powershell
dotnet run -- ticket show -p demo -t 123456 --days 30
dotnet run -- history -p demo -d 7
dotnet run -- history.export -p demo -d 30 --to csv  --file C:\temp\hist.csv
```

### 6️⃣ Risk & Panic

```powershell
dotnet run -- lot.calc -s EURUSD --risk-pct 1 --sl-points 150 --balance 1000
dotnet run -- panic -p demo
```

---

## 🔎 Debugging & Tips

* **Profile not found:** check `profiles.json` placement
* **Hidden symbol:** run `symbol ensure`
* **Timeouts:** bump `--timeout-ms` or add `--trace`

---
