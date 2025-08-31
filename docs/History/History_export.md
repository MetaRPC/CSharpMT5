# History Export (`history.export`) 📤

## What it Does

Exports **trading history** for the last *N* days to a **file** in either **CSV** or **JSON** format.
The command calls `_mt5Account.ExportHistoryAsync(days, symbol, to, ct)` and writes the returned payload to disk.

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                                            |
| ----------------- | ------ | -------- | ---------------------------------------------------------------------- |
| `--profile`, `-p` | string | yes        | Profile to use (from `profiles.json`).                                 |
| `--days`, `-d`    | int    | yes        | How many days back to fetch. Must be `> 0`.                            |
| `--symbol`, `-s`  | string | no        | Filter by symbol (e.g. `EURUSD`). If omitted, exports **all** symbols. |
| `--to`            | string | no        | Output format: `csv` (default) or `json`.                              |
| `--file`, `-f`    | string | yes        | **Output path** to write to (e.g. `C:\temp\hist.csv`).                 |
| `--timeout-ms`    | int    | no        | Per-RPC timeout in milliseconds (default: 30000).                      |

> Format validation: the command enforces `--to csv|json` and fails otherwise.

---

## Output ⬆️

* Writes the exported content to the path given by `--file`.
* Prints a one-line success message on completion:

  * `✔ history.export written to: <file>`
* **Exit codes**:

  * `0` — success;
  * `2` — unsupported format/operation (e.g., server replied `NotSupportedException`);
  * `1` — other errors (printed via `ErrorPrinter`).

---

## How to Use 🛠️

### CLI Examples

```powershell
# Last 7 days, all symbols, CSV to file
dotnet run -- history.export -p demo -d 7 --to csv -f C:\\temp\\hist.csv --timeout-ms 60000

# Last 30 days, only EURUSD, JSON
dotnet run -- history.export -p demo -d 30 -s EURUSD --to json -f C:\\temp\\eurusd-history.json
```

### Notes

* If `--symbol` is omitted, the server exports all symbols.
* CSV/JSON schema is produced by the server side of `_mt5Account.ExportHistoryAsync(...)`.

---

## Code Reference (exact) 🧩

```csharp
var heDaysOpt  = daysOpt;
var heSymbolOpt= new Option<string?>(new[] { "--symbol", "-s" }, "Filter by symbol (optional)");
var heToOpt    = new Option<string>(new[] { "--to" }, () => "csv", "csv|json");
var heFileOpt  = new Option<string>(new[] { "--file", "-f" }, "Output path") { IsRequired = true };

var histExport = new Command("history.export", "Export trading history (deals/orders) to CSV/JSON");
histExport.AddAlias("hexport");

histExport.AddOption(profileOpt);
histExport.AddOption(heDaysOpt);
histExport.AddOption(heSymbolOpt);
histExport.AddOption(heToOpt);
histExport.AddOption(heFileOpt);
histExport.AddOption(timeoutOpt);

histExport.SetHandler(async (InvocationContext ctx) =>
{
    var profile = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var days    = ctx.ParseResult.GetValueForOption(heDaysOpt);
    var symbol  = ctx.ParseResult.GetValueForOption(heSymbolOpt);
    var to      = (ctx.ParseResult.GetValueForOption(heToOpt) ?? "csv").Trim().ToLowerInvariant();
    var file    = ctx.ParseResult.GetValueForOption(heFileOpt)!;
    var timeout = ctx.ParseResult.GetValueForOption(timeoutOpt);

    if (to != "csv" && to != "json") throw new ArgumentException("Use --to csv|json");

    Validators.EnsureProfile(profile);
    if (!string.IsNullOrWhiteSpace(symbol)) _ = Validators.EnsureSymbol(symbol);

    using (UseOpTimeout(timeout))
    using (_logger.BeginScope("Cmd:HISTORY.EXPORT Profile:{Profile}", profile))
    using (_logger.BeginScope("Days:{Days} Symbol:{Symbol} To:{To} File:{File}", days, symbol ?? "<any>", to, file))
    {
        try
        {
            await ConnectAsync();
```

---

📌 In short:
— `history.export` writes **CSV/JSON** history for the last *N* days to disk.
— Supports optional `--symbol` filter.
— Strictly validates `--to` and returns clear exit codes.
