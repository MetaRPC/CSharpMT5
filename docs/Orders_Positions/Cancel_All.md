# Cancel All (`cancel.all`) 

## What it Does

Cancels **multiple pending orders** in one go.
You can optionally filter by **symbol** and **pending type**.

*Does not close open positions.* For positions use `close`, `close-all`, or `close-symbol`.

---

## Input Parameters ⬇️

| Parameter         | Type   | Description                                                                                            |
| ----------------- | ------ | ------------------------------------------------------------------------------------------------------ |
| `--profile`, `-p` | string | Profile from `profiles.json`.                                                                          |
| `--symbol`        | string | Filter: only cancel pendings for this symbol (e.g., `EURUSD`).                                         |
| `--type`          | string | Filter by pending type: `any` (default), `limit`, `stop`, `stoplimit` (adjust to your implementation). |
| `--output`, `-o`  | string | `text` (default) or `json`.                                                                            |
| `--timeout-ms`    | int    | RPC timeout in ms (default: 30000).                                                                    |

> No `--yes` flag here — this command targets **pending orders** only.

---

## Output Fields ⬆️

| Field      | Type  | Description                                                      |
| ---------- | ----- | ---------------------------------------------------------------- |
| `Total`    | int   | How many pending orders matched the filters.                     |
| `Canceled` | int   | How many were successfully canceled.                             |
| `Errors`   | int   | How many failed to cancel.                                       |
| `Items[]`  | array | Per-ticket results: `{ Ticket, Symbol, Type, Status, Message }`. |

---

## How to Use 🛠️

### CLI

```powershell
# Cancel ALL pendings on the account
dotnet run -- cancel.all -p demo

# Cancel only for EURUSD
dotnet run -- cancel.all -p demo --symbol EURUSD

# Cancel only stop/stop-limit types (if supported)
dotnet run -- cancel.all -p demo --type stop

# JSON summary
dotnet run -- cancel.all -p demo -o json
```

### PowerShell Shortcuts (from `shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
ca                  # → mt5 cancel.all -p demo --timeout-ms 90000
ca -s EURUSD        # → mt5 cancel.all -p demo --symbol EURUSD --timeout-ms 90000
ca -s XAUUSD -type stop   # → mt5 cancel.all -p demo --symbol XAUUSD --type stop --timeout-ms 90000
```

---

## When to Use ❓

* Clean up the board before news/events.
* Reset environment for tests or redeploys.
* Remove stale pendings across many symbols quickly.

---

## Notes & Safety 🛡️

* The command is **idempotent** — re-running after successful cancel should find nothing to cancel.
* Some brokers require the symbol to be visible in Market Watch; if you filter by `--symbol`, it’s safe to ensure visibility first.
* Consider rate limits: bulk operations should handle retries gracefully.

---

## Code Reference (to be filled by you) 🧩

```csharp
 var pendSymbolOpt = new Option<string?>(new[] { "--symbol", "-s" }, "Filter by symbol (optional)");
var pendTypeOpt   = new Option<string?>(new[] { "--type" }, "Filter by type: limit|stop|stoplimit|any (default any)");

var cancelAll = new Command("cancel.all", "Cancel all pending orders (optionally filtered)");
cancelAll.AddAlias("ca");

cancelAll.AddOption(profileOpt);
cancelAll.AddOption(pendSymbolOpt);
cancelAll.AddOption(pendTypeOpt);
cancelAll.AddOption(timeoutOpt);
cancelAll.AddOption(dryRunOpt);

cancelAll.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var symbol    = ctx.ParseResult.GetValueForOption(pendSymbolOpt);
    var typeStr   = (ctx.ParseResult.GetValueForOption(pendTypeOpt) ?? "any").Trim().ToLowerInvariant();
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (!string.IsNullOrWhiteSpace(symbol)) _ = Validators.EnsureSymbol(symbol!);

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CANCEL.ALL Profile:{Profile}", profile))
    using (_logger.BeginScope("Filter Symbol:{Symbol} Type:{Type}", symbol ?? "<any>", typeStr))
    {
        try
        {
            await ConnectAsync();
```
