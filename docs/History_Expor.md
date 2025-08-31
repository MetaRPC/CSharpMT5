# 📜 History & Export

## Purpose

Work with account trading history — list past orders/deals and export them into machine-readable formats (CSV/JSON) for further analysis.

---

## 🔹 `history` — show history for N days

*Lists all trades (orders & deals) for the last N days.*

**Options:**

| Option             | Meaning                            | Required |
| ------------------ | ---------------------------------- | -------- |
| `-p, --profile`    | Profile from `profiles.json`       | Yes      |
| `-d, --days <int>` | How many days back to query        | Yes      |
| `-o, --output`     | Format: `text` (default) or `json` | No       |
| `--timeout-ms`     | Per-RPC timeout                    | No       |

**Examples:**

```powershell
# Show last 7 days in text
dotnet run -- history -p demo -d 7

# Show last 30 days in JSON
dotnet run -- history -p demo -d 30 -o json
```

---

## 🔹 `history.export` — export to CSV/JSON

*Export history into a file for external tools.*

**Options:**

| Option              | Meaning                                  | Required |
| ------------------- | ---------------------------------------- | -------- |
| `-p, --profile`     | Profile from `profiles.json`             | Yes      |
| `--days <int>`      | How many days back to include            | Yes      |
| `--to <fmt>`        | Export format: `csv` or `json`           | Yes      |
| `--file <path>`     | Output file path                         | Yes      |
| `--symbol <SYM>`    | Optional filter by symbol (e.g., EURUSD) | No       |
| `--timeout-ms <ms>` | Per-RPC timeout                          | No       |

**Examples:**

```powershell
# Export 30 days to CSV
dotnet run -- history.export -p demo --days 30 --to csv  --file C:\temp\hist.csv

# Export 30 days to JSON, filter by symbol
dotnet run -- history.export -p demo --days 30 --to json --file C:\temp\eurusd.json --symbol EURUSD
```

## Related Docs 📚

* [History.md](./History.md) — Orders/deals history for N days  
* [History_export.md](./History_export.md) — Export history (CSV/JSON)  

---

## 🔹 Shortcasts

From `ps/shortcasts.ps1`:

```powershell
# Show history
hist   # => mt5 history -p $PF -d 7 -o text

# Export
hexport # => mt5 history.export -p $PF --days 30 --to csv --file <...>
```

---

## 📌 Notes specific to our code

* Both commands call `_mt5Account.HistoryOrdersAsync/DealsAsync` internally.
* `history` prints directly to console (text or JSON).
* `history.export` serializes to a file, using safe retry & timeout logic.
* Output aligns with MT5’s *orders* and *deals* model (tickets, prices, volumes, times).
