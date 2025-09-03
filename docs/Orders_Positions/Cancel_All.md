# Cancel All (`cancel.all`) 🧹

Cancels **multiple pending orders** in one go. Optional filters by **symbol** and by **pending type**.

*Does not close open positions.* For positions use **[close](../Market_Data/Close.md)**, **[close-all](../Market_Data/Close-all.md)**, or **[close-symbol](../Market_Data/Close-symbol.md)**.

---

## Method Signatures 

```csharp
// Snapshot with detailed items (needed for type filtering)
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// Fast ticket list (when you don't need Type info)
public Task<IReadOnlyList<ulong>> ListPendingTicketsAsync(
    string? symbol,
    CancellationToken cancellationToken);

// Cancel a single pending by ticket
public Task CancelPendingOrderAsync(
    ulong ticket,
    CancellationToken cancellationToken);

// Best‑effort: ensure symbol visible (optional)
public Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    TimeSpan? pollInterval = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                                            |
| ----------------- | ------ | -------- | ---------------------------------------------------------------------- |
| `--profile`, `-p` | string | yes      | Profile from `profiles.json`.                                          |
| `--symbol`, `-s`  | string | no       | Filter: cancel only pendings for this symbol (e.g., `EURUSD`).         |
| `--type`          | string | no       | Filter by pending type: `any` (default), `limit`, `stop`, `stoplimit`. |
| `--timeout-ms`    | int    | no       | RPC timeout in ms (default: `30000`).                                  |
| `--dry-run`       | flag   | no       | Print intended actions without sending requests.                       |

> **Note:** This command is **text‑only**; `--output` is not supported.

---

## Output ⬆️ (text)

```
CANCEL.ALL targets: <N>  (symbol=<SYM|any>, type=<any|limit|stop|stoplimit>)
[DRY-RUN] CANCEL #<ticket1>
[DRY-RUN] CANCEL #<ticket2>
...
✔ cancel.all done  |  canceled: <ok>/<N>, errors: <fail>
```

**Exit codes**

* `0` — success (some items may still fail; see counters)
* `2` — nothing matched the filters (N=0)
* `1` — fatal error (printed via ErrorPrinter)

---

## How to Use 🛠️

```powershell
# Cancel ALL pendings on the account
dotnet run -- cancel.all -p demo

# Only for EURUSD
dotnet run -- cancel.all -p demo -s EURUSD

# Only stop/stop‑limit types (if supported in your broker mapping)
dotnet run -- cancel.all -p demo --type stop

# Dry‑run (plan only)
dotnet run -- cancel.all -p demo -s XAUUSD --type limit --dry-run
```

### PowerShell Shortcuts (from `ps/shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
ca                    # → mt5 cancel.all -p demo --timeout-ms 90000
ca -s EURUSD          # → mt5 cancel.all -p demo --symbol EURUSD --timeout-ms 90000
ca -s XAUUSD -type stop   # → mt5 cancel.all -p demo --symbol XAUUSD --type stop --timeout-ms 90000
```

---

## Notes & Safety 🛡️

* **Idempotent:** re‑running after successful cancel should find nothing to cancel.
* If `--symbol` is set, it’s safe to best‑effort **ensure visibility** first.
* Consider rate limits: bulk operations should handle retries gracefully.
* `--type` relies on your pending type mapping. If unsure, prefer `type=any`.

---

## Code Reference 🧩

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
        await ConnectAsync();

        // 1) build candidate set
        IReadOnlyList<ulong> tickets;
        if (string.Equals(typeStr, "any", StringComparison.Ordinal))
        {
            // Fast path: only symbol filter → use ticket list
            tickets = await _mt5Account.ListPendingTicketsAsync(symbol, CancellationToken.None);
        }
        else
        {
            // Need Type info → use aggregate, then filter by symbol+type
            var opened = await _mt5Account.OpenedOrdersAsync(cancellationToken: CancellationToken.None);
            var query = opened.PendingInfos.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(symbol))
                query = query.Where(p => string.Equals(p.Symbol, symbol, StringComparison.OrdinalIgnoreCase));

            query = typeStr switch
            {
                "limit"     => query.Where(p => IsLimit(p.Type)),
                "stop"      => query.Where(p => IsStop(p.Type)),
                "stoplimit" => query.Where(p => IsStopLimit(p.Type)),
                _            => query
            };
            tickets = query.Select(p => p.Ticket).ToArray();
        }

        var total = tickets.Count;
        Console.WriteLine($"CANCEL.ALL targets: {total}  (symbol={symbol ?? "any"}, type={typeStr})");
        if (total == 0)
        {
            // Exit code 2 recommended by the docs policy
            // Environment.ExitCode = 2; return;
            return; // let the outer runner set codes if needed
        }

        if (dryRun)
        {
            foreach (var t in tickets.Take(100))
                Console.WriteLine($"[DRY-RUN] CANCEL #{t}");
            if (tickets.Count > 100)
                Console.WriteLine($"... and {tickets.Count - 100} more");
            return;
        }

        // 2) execute
        int ok = 0, fail = 0;
        foreach (var t in tickets)
        {
            try { await _mt5Account.CancelPendingOrderAsync(t, CancellationToken.None); ok++; }
            catch (Exception ex) { Console.WriteLine($"WARN: cancel #{t} failed: {ex.Message}"); fail++; }
        }

        Console.WriteLine($"\u2714 cancel.all done  |  canceled: {ok}/{total}, errors: {fail}");
    }
});
```

---

## See also 🔗

* **[pending list](../Misc/Pending_List.md)** — enumerate current pendings
* **[pending.modify](../Market_Data/Pending.modify.md)** — edit pending parameters
* **[pending.move](../Market_Data/Pending.move.md)** — shift pending by ±points
* **[panic](../Market_Data/Panic.md)** — cancel pendings + flatten positions (emergency)
