# History — Overview 📜

The **History** section provides access to closed deals and account history over a given period. It is primarily used for reporting, analytics, and exporting trade data for further processing (spreadsheets, dashboards, or audits).

---

## Commands Included

| Command                                    | Alias     | Description                                              |
| ------------------------------------------ | --------- | -------------------------------------------------------- |
| [`History.md`](./History.md)               | `hist`    | Show account history for the last N days (text or JSON). |
| [`History_export.md`](./History_export.md) | `hexport` | Export account history into a file (CSV, JSON, etc.).    |

---

## When to Use ❓

* **History** → quick look at recent trades (CLI or script output).
* **History Export** → structured data for Excel/BI tools.

---

## Typical Workflows 🔄

1. **Diagnostics:** Check trades over the last 7 days:

   ```powershell
   hist -d 7 -o text
   ```

2. **Export to CSV for analysis:**

   ```powershell
   hexport -d 30 --to csv --file my_trades.csv
   ```

3. **Automation:** Schedule daily exports in JSON for dashboards:

   ```powershell
   hexport -d 1 --to json --file daily.json
   ```

---

## Notes 📝

* All commands require a **profile** (`-p demo` or `-p live`) to select the account.
* Default symbol filter is **all symbols**, unless explicitly set.
* Timeouts (`--timeout-ms`) protect against long server responses.

---
