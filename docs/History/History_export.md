# History Export (`history.export`) 📤

Exports **trading history** for the last *N* days to a **file** in either **CSV** or **JSON** format.
Under the hood calls `_mt5Account.ExportHistoryAsync(days, symbol, format, ct)` and writes the returned payload to disk.

Alias: `hexport`

---

## Method Signature

```csharp
public Task<string> ExportHistoryAsync(
    int days,
    string? symbol,
    string format,
    CancellationToken ct);
```

---

## Input Parameters ⬇️

| Parameter       | Type   | Required | Description                                                            |
| --------------- | ------ | -------- | ---------------------------------------------------------------------- |
| `--profile, -p` | string | yes      | Profile to use (from `profiles.json`).                                 |
| `--days, -d`    | int    | yes      | How many days back to fetch. **Must be > 0**.                          |
| `--symbol, -s`  | string | no       | Filter by symbol (e.g. `EURUSD`). If omitted, exports **all** symbols. |
| `--to`          | string | no       | Output format: `csv` (default) or `json`.                              |
| `--file, -f`    | string | yes      | **Output path** to write to (e.g. `C:\\temp\\hist.csv`).               |
| `--timeout-ms`  | int    | no       | Per‑RPC timeout in milliseconds (default: `30000`).                    |

> The command validates `--to` and accepts only `csv` or `json`.

---

## Output ⬆️

* Writes the exported content to the path given by `--file`.
* Prints a one‑line success message: `✔ history.export written to: <file>`
* **Exit codes**:

  * `0` — success;
  * `2` — unsupported format/operation (e.g., server replied `NotSupportedException`);
  * `1` — other errors (printed via `ErrorPrinter`).

---

## How to Use

```powershell
# Last 7 days, all symbols, CSV to file
dotnet run -- history.export -p demo -d 7 --to csv -f C:\\temp\\hist.csv --timeout-ms 60000

# Last 30 days, only EURUSD, JSON
dotnet run -- history.export -p demo -d 30 -s EURUSD --to json -f C:\\temp\\eurusd-history.json
```

Shortcast (optional):

```powershell
hexport -d 7 -s EURUSD -to json -f C:\\temp\\eurusd.json
# → mt5 history.export -p demo -d 7 -s EURUSD --to json -f C:\\temp\\eurusd.json --timeout-ms 90000
```

---

## Notes & Safety 🛡️

* `--days` must be strictly **> 0**; the tool rejects `0` or negatives.
* Ensure the target folder for `--file` **exists** and is writable.
* CSV/JSON **schema** is defined by your server’s implementation (the tool writes raw payload as‑is).
* If you pass `--symbol`, only trades/orders for that instrument are exported.
* Network hiccups/timeouts are reported via `ErrorPrinter` and set a non‑zero exit code.

---

## Code Reference 🧩

```csharp
// Parameters already validated & connection established by caller flow
var data = await _mt5Account.ExportHistoryAsync(days, symbol, to, CancellationToken.None);
System.IO.File.WriteAllText(file, data);
Console.WriteLine($"\u2714 history.export written to: {file}");
```

---

## See also

* **[History.md](./History.md)** — interactive history preview
* **[History\_Overview.md](./History_Overview.md)** — concepts & fields
* **[Profiles](../Account/Profiles.md)** — selecting connection settings
* **[Timeouts & Retries](../Timeouts_RetriesPolicy.md)** — guidance on `--timeout-ms`
