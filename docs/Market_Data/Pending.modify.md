# Pending Modify (`pending.modify`) ✏️

## What it Does

Modifies an existing **pending order** in MT5 (e.g., Buy Limit, Sell Stop, Buy Stop Limit).
Allows changing entry price, stop loss, take profit, and expiration.

---

## Input Parameters ⬇️

| Parameter         | Type     | Description                                          |
| ----------------- | -------- |---------------------------------------------------- |
| `--profile`, `-p` | string   | Which profile to use (from `profiles.json`).         |
| `--ticket`, `-t`  | ulong    | Ticket ID of the pending order.                      |
| `--price`         | double   | New entry price.                                     |
| `--sl`            | double   |  New Stop Loss price.                                 |
| `--tp`            | double   |  New Take Profit price.                               |
| `--expiration`    | DateTime |  Expiration time (UTC).                               |
| `--output`, `-o`  | string   | `text` (default) or `json`.                          |
| `--timeout-ms`    | int      |  RPC timeout in ms (default: 30000).                  |
| `--dry-run`       | flag     |  Print intended modification without sending request. |

---

## Output Fields ⬆️

| Field        | Type     | Description                              |
| ------------ | -------- | ---------------------------------------- |
| `Ticket`     | ulong    | Ticket ID of the modified pending order. |
| `Symbol`     | string   | Target instrument.                       |
| `Price`      | double   | Modified entry price.                    |
| `SL`         | double   | Stop Loss after modification.            |
| `TP`         | double   | Take Profit after modification.          |
| `Expiration` | DateTime | Expiration (if set).                     |
| `Status`     | string   | Result of operation (`OK` / `Error`).    |

---

## How to Use 🛠️

### CLI

```powershell
# Change SL/TP only
dotnet run -- pending.modify -p demo -t 123456 --sl 1.0950 --tp 1.1050

# Change entry price and expiration
dotnet run -- pending.modify -p demo -t 123456 --price 1.1000 --expiration "2025-09-01T12:00:00Z"
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
pm -t 123456 --sl 1.0950 --tp 1.1050
```

---

## When to Use ❓

* To adjust entry prices closer/further as market evolves.
* To tighten or loosen stop-loss / take-profit.
* To add or extend expiration.

---

## Notes & Safety 🛡️

* Order must be **pending** — you cannot use this command on already executed positions.
* Check broker restrictions on minimum distance (stops level).
* Expiration must be in the future and in UTC.
* `--dry-run` helps validate values before sending.

---

## Code Reference 🧩

```csharp
var pmTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Pending order ticket") { IsRequired = true };
var pmTypeOpt   = new Option<string?>(new[] { "--type" }, "buylimit|selllimit|buystop|sellstop|buystoplimit|sellstoplimit (optional, for validation)");
var pmPriceOpt  = new Option<double?>(new[] { "--price" }, "New entry price for limit/stop");
var pmStopOpt   = new Option<double?>(new[] { "--stop" }, "New trigger price for stop/stop-limit");
var pmLimitOpt  = new Option<double?>(new[] { "--limit" }, "New limit price for stop-limit");
var pmSlOpt     = new Option<double?>(new[] { "--sl" }, "New Stop Loss (absolute)");
var pmTpOpt     = new Option<double?>(new[] { "--tp" }, "New Take Profit (absolute)");
var pmTifOpt    = new Option<string?>(new[] { "--tif" }, "GTC|DAY|GTD");
var pmExpireOpt = new Option<DateTimeOffset?>(new[] { "--expire" }, "Expiry (ISO-8601) when --tif=GTD");

var pendingModify = new Command("pending.modify", "Modify a pending order (price/stop-limit/SL/TP/expiry)");
pendingModify.AddAlias("pm");

pendingModify.AddOption(profileOpt);
pendingModify.AddOption(symbolOpt);
pendingModify.AddOption(pmTicketOpt);
pendingModify.AddOption(pmTypeOpt);
pendingModify.AddOption(pmPriceOpt);
pendingModify.AddOption(pmStopOpt);
pendingModify.AddOption(pmLimitOpt);
pendingModify.AddOption(pmSlOpt);
pendingModify.AddOption(pmTpOpt);
pendingModify.AddOption(pmTifOpt);
pendingModify.AddOption(pmExpireOpt);

pendingModify.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var symbolArg = ctx.ParseResult.GetValueForOption(symbolOpt);
    var ticket    = ctx.ParseResult.GetValueForOption(pmTicketOpt);
    var typeStr   = ctx.ParseResult.GetValueForOption(pmTypeOpt);
    var price     = ctx.ParseResult.GetValueForOption(pmPriceOpt);
    var stop      = ctx.ParseResult.GetValueForOption(pmStopOpt);
    var limit     = ctx.ParseResult.GetValueForOption(pmLimitOpt);
    var sl        = ctx.ParseResult.GetValueForOption(pmSlOpt);
    var tp        = ctx.ParseResult.GetValueForOption(pmTpOpt);
    var tifStr    = ctx.ParseResult.GetValueForOption(pmTifOpt);
    var expire    = ctx.ParseResult.GetValueForOption(pmExpireOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    _ = ticket;

    var s = symbolArg ?? GetOptions().DefaultSymbol;
    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:PENDING.MODIFY Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket}", ticket))
    using (_logger.BeginScope("Params Type:{Type} Price:{Price} Stop:{Stop} Limit:{Limit} SL:{SL} TP:{TP} TIF:{TIF} Exp:{Exp}",
                              typeStr, price, stop, limit, sl, tp, tifStr, expire))
    {
        if (!string.IsNullOrWhiteSpace(typeStr))
        {
            var k = typeStr.Replace("-", "").Replace(".", "").Trim().ToLowerInvariant();
            var isStopLimit = k is "buystoplimit" or "sellstoplimit";
            var isLimitOrStop = k is "buylimit" or "selllimit" or "buystop" or "sellstop";

            if (isStopLimit)
            {
                if (!stop.HasValue || !limit.HasValue)
                    throw new ArgumentException("Stop-limit modify requires both --stop and --limit.");
                if (k == "buystoplimit" && !(limit!.Value <= stop!.Value))
                    throw new ArgumentException("Buy Stop Limit modify requires --limit <= --stop.");
                if (k == "sellstoplimit" && !(limit!.Value >= stop!.Value))
                    throw new ArgumentException("Sell Stop Limit modify requires --limit >= --stop.");
                if (price.HasValue)
                    throw new ArgumentException("Do not pass --price for stop-limit modify. Use --stop and --limit.");
            }
            else if (isLimitOrStop)
            {
                if (!price.HasValue || price.Value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(pmPriceOpt), "For limit/stop modify, --price must be > 0.");
            }
        }

        if (!string.IsNullOrWhiteSpace(tifStr) && tifStr!.ToUpperInvariant() == "GTD" && !expire.HasValue)
            throw new ArgumentException("When --tif=GTD is used, --expire must be provided.");

        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] PM ticket={ticket} type={typeStr} price={price} stop={stop} limit={limit} SL={sl} TP={tp} TIF={tifStr} expire={expire}");
            return;
        }

        try
        {
            await ConnectAsync();
```
