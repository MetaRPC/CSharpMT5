# CSharpMT5 CLI — Shortcasts & Live Examples

> General launch pattern (PowerShell / CMD):
> `dotnet run -- <command> [options]`

---

## Global options (most used)

| Option                  | Meaning                                           | Notes                                       |   |
| ----------------------- | ------------------------------------------------- | ------------------------------------------- | - |
| `-p, --profile <name>`  | Profile from `profiles.json` (default: `default`) |                                             |   |
| `-s, --symbol <SYM>`    | Instrument/ticker, e.g., `EURUSD`                 |                                             |   |
| `-v, --volume <lots>`   | Order volume in lots                              |                                             |   |
| `--deviation <pts>`     | Max slippage in points                            |                                             |   |
| \`-o, --output text     | json\`                                            | Output format (if supported by the command) |   |
| `--timeout-ms <ms>`     | Per-RPC timeout (default `30000`)                 |                                             |   |
| `--dry-run`             | Don’t send anything, just show what would be done |                                             |   |
| `--verbose` / `--trace` | Extended logging                                  |                                             |   |

**Tip:** keep passwords out of files — set an environment variable `MT5_PASSWORD`.

---

## Command reference (shortcasts)

**Format:** command — aliases — what it does

### Profiles

* `profiles list` — `ls` — list profile names from `profiles.json`
* `profiles show` — `view` — show profile parameters (respects `MT5_PASSWORD`)

### General / info

* `info` — `i` — account summary
* `quote` — `q` — one-shot spot quote (Bid/Ask/Time)
* `health` — `ping` — quick connectivity diagnostics
* `stream` — `st` — tick/event stream (with autoreconnect)

### Market

* `buy` — `b` — market buy
* `sell` — `s` — market sell

### Closing / reversing

* `close` — `c` — close by ticket (volume normalized per symbol rules)
* `close.percent` — `cpp` — close a percent of a ticket
* `close.half` — `ch` — close half of a ticket (alias of `close.percent --pct 50`)
* `close.partial` — `cp` — close an **exact** volume on a ticket
* `close-all` — `flatten, close.all` — close all open positions (optional symbol filter)
* `close-symbol` — `cs, flatten-symbol` — close all positions for a symbol
* `reverse` — `rv` — reverse **net** position for a symbol (`--mode net|flat`)
* `reverse.ticket` — `rvt` — reverse a **specific** ticket

### SL/TP & breakeven

* `position.modify` — `posmod` — set SL/TP by **price** (ticket)
* `position.modify.points` — `pmp` — set SL/TP by **distance in points** from entry/market
* `breakeven` — `be` — move SL to BE ± offset (price/points)
* `trail.start` — — start a local trailing stop
* `trail.stop` — — stop the local trailing

### Pending orders

* `place` — `pl` — place a pending (limit / stop / stop-limit)
* `pending.modify` — `pm` — change pending parameters (prices/SL/TP/expiry)
* `pending.move` — `pmove` — shift pending price(s) by ±N points
* `pending list` — (under `pending`) — list pending tickets
* `cancel` — `x` — delete a pending by ticket
* `cancel.all` — `ca` — delete all pendings (with optional filters)

### Tickets / symbols / lists

* `ticket show` — `t sh` — show info for a ticket (opened / recent history)
* `positions` — `pos` — open positions
* `orders` — `ord` — open orders and position tickets
* `symbol ensure` — `en` — make sure symbol is visible in terminal
* `symbol limits` — `lim` — lot min/step/max
* `symbol show` — `sh` — short symbol card (quote + lot limits)

### History / tools

* `history` — `h` — orders/deals history for N days
* `history.export` — `hexport` — export history (CSV/JSON)
* `lot.calc` — `lc` — position sizing from risk (%) and SL distance (points)
* `panic` — — close all positions and cancel all pendings (optional symbol filter)

---

## Quick start

```powershell
# Show available profiles
dotnet run -- profiles list

# Show profile (respects MT5_PASSWORD)
dotnet run -- profiles show -p default
dotnet run -- profiles show -p demo

# Connectivity check
dotnet run -- health -p default
dotnet run -- health -p demo

# One quote
dotnet run -- q -p demo -s EURUSD
```

---

## Practical scenarios

### 1) Open a position and manage it

```powershell
# Buy 0.10 with SL/TP and slippage control
dotnet run -- buy -p demo -s EURUSD -v 0.10 --sl 1.0700 --tp 1.0800 --deviation 10

# Move SL to breakeven +2 pts
dotnet run -- breakeven -p demo -t 123456 --offset-points 2

# Set SL/TP as distances from entry (in points)
dotnet run -- position.modify.points -p demo -t 123456 --sl-points 150 --tp-points 250 --from entry

# Start trailing stop (classic), step 20 pts, distance 150 pts
dotnet run -- trail.start -p demo -t 123456 --distance 150 --step 20 --mode classic

# Stop trailing
dotnet run -- trail.stop -p demo -t 123456
```

> Add `--dry-run` to preview without sending.

---

### 2) Partial closes & reverse

```powershell
# Close 50%
dotnet run -- close.half -p default -t 123456 --deviation 10

# Close 30% (close.percent)
dotnet run -- close.percent -p default -t 123456 --pct 30 --deviation 10

# Close exactly 0.05 lots
dotnet run -- close.partial -p default -t 123456 --volume 0.05 --deviation 10

# Reverse net exposure by symbol
#   net  : place one 2x opposite order vs current net
#   flat : flatten symbol, then open 1x in the opposite direction
dotnet run -- reverse -p default -s EURUSD --mode net  --deviation 10 --sl 1.0700 --tp 1.0850
dotnet run -- reverse -p default -s EURUSD --mode flat --deviation 10 --sl 1.0700 --tp 1.0850

# Reverse a specific ticket
dotnet run -- reverse.ticket -p default -t 123456 --deviation 10 --sl 1.0700 --tp 1.0850
```

---

### 3) Pendings: place, modify, move

```powershell
# Place Buy Limit
dotnet run -- place -p demo --type buylimit  -s EURUSD --price 1.0750 --sl 1.0700 --tp 1.0800

# Place Sell Stop
dotnet run -- place -p demo --type sellstop  -s EURUSD --price 1.0720 --sl 1.0750 --tp 1.0690

# Place Buy Stop Limit (needs both stop and limit)
dotnet run -- place -p demo --type buystoplimit -s EURUSD --stop 1.0760 --limit 1.0758 --sl 1.0740 --tp 1.0790

# Modify pending: new entry and SL
dotnet run -- pending.modify -p demo -t 778899 --price 1.0765 --sl 1.0745

# Move pending prices by +15 points (moves price and trigger if any)
dotnet run -- pending.move -p demo -t 778899 --by-points 15

# List pendings
dotnet run -- pending list -p demo

# Delete a pending
dotnet run -- cancel -p demo -t 778899 -s EURUSD

# Delete all pendings (filters: symbol and type)
dotnet run -- cancel.all -p demo --symbol EURUSD
dotnet run -- cancel.all -p demo --type limit
dotnet run -- cancel.all -p demo --type stop
dotnet run -- cancel.all -p demo --type stoplimit
```

---

### 4) Lists & symbols

```powershell
# All open positions
dotnet run -- positions -p default -o json

# Open orders and position tickets
dotnet run -- orders -p default

# Short symbol card (quote + lot limits)
dotnet run -- symbol show -p default -s EURUSD

# Ensure symbol is visible in terminal
dotnet run -- symbol ensure -p default -s GBPUSD

# Lot limits for a symbol
dotnet run -- symbol limits -p default -s USDJPY
```

---

### 5) By ticket (and history)

```powershell
# Show ticket info (opened; otherwise recent history for N days)
dotnet run -- ticket show -p default -t 123456 --days 30

# History for 7 days (orders/deals)
dotnet run -- history -p default -d 7

# Export history to CSV/JSON
dotnet run -- history.export -p default -d 30 --to csv  --file C:\temp\hist.csv
dotnet run -- history.export -p default -d 30 --to json --file C:\temp\hist.json
```

---

### 6) Risk & “panic”

```powershell
# Lot sizing from risk 1% and SL = 150 pts
dotnet run -- lot.calc -s EURUSD --risk-pct 1 --sl-points 150 --balance 1000 --min-lot 0.01 --lot-step 0.01

# Close everything and cancel all pendings (optional symbol)
dotnet run -- panic -p default
dotnet run -- panic -p default --symbol EURUSD
```

---

## SL/TP: exact prices vs points

* **Exact prices:** `position.modify`

```powershell
dotnet run -- position.modify -p default -t 123456 --sl 1.0700 --tp 1.0830
```

* **Distances in points:** `position.modify.points`
  Base: `--from entry|market`. With `market`: long uses `Bid`, short uses `Ask`.

```powershell
dotnet run -- position.modify.points -p default -t 123456 --sl-points 150 --tp-points 250 --from market
```

**Breakeven:** `breakeven`

* `--offset 0.0002` (price) **or**
* `--offset-points 20` (points),
* `--force` — allow worsening SL (by default only improves).

```powershell
dotnet run -- breakeven -p default -t 123456 --offset-points 20
dotnet run -- breakeven -p default -t 123456 --offset 0.0002 --force
```

---

## Trailing

```powershell
# Start: classic or chandelier
dotnet run -- trail.start -p default -t 123456 --distance 150 --step 20 --mode classic

# Stop
dotnet run -- trail.stop -p default -t 123456
```

---

## JSON output, dry-run & logging

```powershell
# JSON output (when supported)
dotnet run -- positions -p default -o json

# Dry-run: show what would be sent
dotnet run -- reverse -p default -s EURUSD --mode net --sl 1.0700 --tp 1.0850 --dry-run

# Verbose logs
dotnet run -- q -p demo -s EURUSD --verbose
dotnet run -- q -p demo -s EURUSD --trace
```

---

## Useful checks & common issues

* **“Set Host or MtClusterName”** — profile isn’t picked up. Make sure `profiles.json` is:

  * in the current working directory **or**
  * in `Config/` and copied to `bin/...` as `profiles.json` (project already configured).

  Verify with:

  ```powershell
  dotnet run -- profiles show -p default
  dotnet run -- health -p default
  ```

* **No permission / symbol hidden** — run `symbol ensure -s <SYM>`.

* **Timeouts** — increase `--timeout-ms` or use `--trace` to see where it hangs.

---

## Minimal `profiles.json` example

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

**Notes:**

* For `demo` you can set `ServerName` only → uses `ConnectByServerNameAsync`.
* For `default` use `Host/Port` → `ConnectByHostPortAsync`.
* Prefer `MT5_PASSWORD` env var (it overrides the profile password).

---

## Command sampler (copy & run)

```powershell
# Info / checks
dotnet run -- profiles list
dotnet run -- profiles show -p demo
dotnet run -- health -p demo

# Open, manage, close
dotnet run -- buy -p demo -s EURUSD -v 0.10 --sl 1.0700 --tp 1.0800 --deviation 10
dotnet run -- breakeven -p demo -t 123456 --offset-points 10
dotnet run -- position.modify.points -p demo -t 123456 --sl-points 150 --tp-points 250 --from entry
dotnet run -- close -p demo -t 123456 -s EURUSD -v 0.10

# Pendings
dotnet run -- place -p demo --type buylimit -s EURUSD --price 1.0750 --sl 1.0700 --tp 1.0800
dotnet run -- pending.modify -p demo -t 778899 --price 1.0760
dotnet run -- pending.move   -p demo -t 778899 --by-points 15
dotnet run -- cancel -p demo -t 778899 -s EURUSD

# Bulk actions
dotnet run -- close-symbol -p demo -s EURUSD --yes
dotnet run -- cancel.all   -p demo --symbol EURUSD
dotnet run -- panic        -p demo

# Lists / history
dotnet run -- positions -p demo
dotnet run -- orders    -p demo
dotnet run -- history   -p demo -d 7
dotnet run -- history.export -p demo -d 30 --to csv  --file C:\temp\hist.csv
```
