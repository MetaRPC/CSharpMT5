# Place (`place`) 🧱

Places a **pending order**. Supports **Limit**, **Stop**, and **Stop‑Limit** types, optional **SL/TP**, and **TIF/expiry**.

> Market orders are **not** handled here — use **[Buy](../Orders_Positions/Buy.md)** / **[Sell](../Orders_Positions/Sell.md)**.

---

## Supported Types

`buylimit`, `selllimit`, `buystop`, `sellstop`, `buystoplimit`, `sellstoplimit`

---

## Input Parameters ⬇️

| Parameter         | Type            | Required | Description                                                          |       |                                                        |
| ----------------- | --------------- | -------- | -------------------------------------------------------------------- | ----- | ------------------------------------------------------ |
| `--profile`, `-p` | string          | yes      | Profile from `profiles.json`.                                        |       |                                                        |
| `--symbol`, `-s`  | string          | no       | Symbol (e.g., `EURUSD`). Defaults to **app** option `DefaultSymbol`. |       |                                                        |
| `--volume`, `-v`  | double          | yes      | Volume in lots.                                                      |       |                                                        |
| `--type`          | string          | yes      | One of supported types above.                                        |       |                                                        |
| `--price`         | double?         | no       | **Entry** price for **Limit/Stop** (not for Stop‑Limit).             |       |                                                        |
| `--stop`          | double?         | no       | **Trigger** price for **Stop‑Limit**.                                |       |                                                        |
| `--limit`         | double?         | no       | **Limit** price for **Stop‑Limit**.                                  |       |                                                        |
| `--tif`           | string?         | no       | Time‑in‑force: `GTC`                                                 | `DAY` | `GTD` *(aliases: `SPECIFIED`, `SPECIFIED_DAY` → GTD).* |
| `--expire`        | DateTimeOffset? | no       | Expiry (ISO‑8601) when `--tif=GTD`.                                  |       |                                                        |
| `--sl`            | double?         | no       | Stop Loss price.                                                     |       |                                                        |
| `--tp`            | double?         | no       | Take Profit price.                                                   |       |                                                        |
| `--timeout-ms`    | int             | no       | Per‑RPC timeout in ms (default: `30000`).                            |       |                                                        |
| `--dry-run`       | flag            | no       | Print intended action without sending a request.                     |       |                                                        |

---

## Validation Rules

* **Stop‑Limit** (`buystoplimit`/`sellstoplimit`):

  * `--stop` **and** `--limit` required; **do not** pass `--price`.
  * Buy Stop‑Limit: `limit ≤ stop`; Sell Stop‑Limit: `limit ≥ stop`.
* **Limit/Stop** (`buylimit`/`selllimit`/`buystop`/`sellstop`):

  * `--price` is **required** and must be `> 0`.
* **TIF**: if `--tif=GTD`, then `--expire` is **required**.

---

## Output ⬆️

* Text log only, e.g.: `PLACE done: ticket=<id>`
* Exit codes: `0` success; `2` not supported/validation; `1` fatal error (printed via ErrorPrinter).

---

## How to Use

```powershell
# Buy Limit @ 1.0950 (0.10 lots)
dotnet run -- place -p demo -s EURUSD -v 0.10 --type buylimit --price 1.0950 --sl 1.0900 --tp 1.1000

# Sell Stop @ 1.0900 for today (DAY)
dotnet run -- place -p demo -s EURUSD -v 0.20 --type sellstop --price 1.0900 --tif DAY

# Stop‑Limit: trigger 1.1000, limit 1.0995 (GTD)
dotnet run -- place -p demo -s EURUSD -v 0.10 --type buystoplimit --stop 1.1000 --limit 1.0995 --tif GTD --expire 2025-09-30T15:00:00Z

# Dry‑run (no request)
dotnet run -- place -p demo -s EURUSD -v 0.10 --type buystop --price 1.1000 --dry-run
```

---

## Notes & Safety

* Best‑effort **EnsureSymbolVisibleAsync** (≈3s) before placing to avoid "symbol not selected".
* `--sl`/`--tp` are **prices**; must respect broker **StopsLevel** / min distance.
* `volume` must respect **min/step/max** — see **[symbol limits](../Market_Data/Limits.md)**.
* If server doesn’t support a type/TIF, a descriptive error is printed.

---

## Method Signatures

```csharp
public Task EnsureSymbolVisibleAsync(string symbol, TimeSpan? maxWait = null, TimeSpan? pollInterval = null, DateTime? deadline = null, CancellationToken cancellationToken = default);

public Task<ulong> PlacePendingOrderAsync(
    string symbol,
    string type,    // buylimit|selllimit|buystop|sellstop
    double volume,
    double price,   // entry price for limit/stop
    double? sl,
    double? tp,
    string? tif,    // GTC|DAY|GTD (aliases SPECIFIED, SPECIFIED_DAY)
    DateTimeOffset? expire,
    CancellationToken ct);

public Task<ulong> PlaceStopLimitOrderAsync(
    string symbol,
    string type,    // buystoplimit|sellstoplimit
    double volume,
    double stop,    // trigger
    double limit,   // limit
    double? sl,
    double? tp,
    string? tif,    // GTC|DAY|GTD (aliases SPECIFIED, SPECIFIED_DAY)
    DateTimeOffset? expire,
    CancellationToken ct);
```

---

## Code Reference 🧩

```csharp
place.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var s         = Validators.EnsureSymbol(ctx.ParseResult.GetValueForOption(symbolOpt) ?? GetOptions().DefaultSymbol);
    var vol       = ctx.ParseResult.GetValueForOption(volumeOpt);
    var typeStr   = ctx.ParseResult.GetValueForOption(placeTypeOpt)!;
    var price     = ctx.ParseResult.GetValueForOption(placePriceOpt);
    var stop      = ctx.ParseResult.GetValueForOption(placeStopOpt);
    var limit     = ctx.ParseResult.GetValueForOption(placeLimitOpt);
    var tif       = ctx.ParseResult.GetValueForOption(placeTifOpt);
    var expire    = ctx.ParseResult.GetValueForOption(placeExpireOpt);
    var sl        = ctx.ParseResult.GetValueForOption(slOpt);
    var tp        = ctx.ParseResult.GetValueForOption(tpOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureVolume(vol);

    var isStopLimit   = typeStr is "buystoplimit" or "sellstoplimit";
    var isLimitOrStop = typeStr is "buylimit" or "selllimit" or "buystop" or "sellstop";

    if (isStopLimit)
    {
        if (!stop.HasValue || !limit.HasValue) throw new ArgumentException("Stop-limit requires both --stop and --limit.");
        if (typeStr == "buystoplimit"  && !(limit <= stop)) throw new ArgumentException("Buy Stop-Limit: limit <= stop.");
        if (typeStr == "sellstoplimit" && !(limit >= stop)) throw new ArgumentException("Sell Stop-Limit: limit >= stop.");
        if (price.HasValue) throw new ArgumentException("Do not pass --price for stop-limit.");
    }
    else if (isLimitOrStop)
    {
        if (!price.HasValue || price.Value <= 0) throw new ArgumentOutOfRangeException(nameof(price), "--price must be > 0.");
    }
    else throw new NotSupportedException("Use buy/sell for market orders.");

    if (string.Equals(tif, "GTD", StringComparison.OrdinalIgnoreCase) && !expire.HasValue)
        throw new ArgumentException("When --tif=GTD, --expire is required.");

    using (UseOpTimeout(timeoutMs))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] PLACE {typeStr} {s} price={price} stop={stop} limit={limit} vol={vol} SL={sl} TP={tp} TIF={tif} exp={expire}");
            return;
        }

        await ConnectAsync();
        try { await _mt5Account.EnsureSymbolVisibleAsync(s, TimeSpan.FromSeconds(3)); } catch { }

        ulong ticket = isStopLimit
            ? await _mt5Account.PlaceStopLimitOrderAsync(s, typeStr, vol, stop!.Value, limit!.Value, sl, tp, tif, expire, CancellationToken.None)
            : await _mt5Account.PlacePendingOrderAsync   (s, typeStr, vol, price!.Value,              sl, tp, tif, expire, CancellationToken.None);

        Console.WriteLine($"PLACE done: ticket={ticket}");
    }
});
```

---

## See also

* **[Pending.modify](../Market_Data/Pending.modify.md)** — set exact prices/SL/TP/TIF
* **[Pending.move](../Market_Data/Pending.move.md)** — shift by ±N points
* **[symbol limits](../Market_Data/Limits.md)** — min/step/max lots
* **[Pending\_List](../Misc/Pending_List.md)** — review current pendings
