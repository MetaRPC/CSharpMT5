# Position Modify Points (`position.modify.points`) 📐

Sets **Stop Loss** and/or **Take Profit** by a **distance in points** from a chosen base price — either the **entry price** or the current **market** price.

Alias: `pmp`

---

## Input Parameters ⬇️

| Parameter         | Type   | Required | Description                                        |
| ----------------- | ------ | -------- | -------------------------------------------------- |
| `--profile`, `-p` | string | yes      | Profile from `profiles.json`.                      |
| `--ticket`, `-t`  | ulong  | yes      | Position ticket (> 0).                             |
| `--sl-points`     | int?   | no       | SL distance in **points** (≥ 0).                   |
| `--tp-points`     | int?   | no       | TP distance in **points** (≥ 0).                   |
| `--from`          | string | no       | Base price: `entry` (default) or `market`.         |
| `--timeout-ms`    | int    | no       | Per‑RPC timeout in ms (default: `30000`).          |
| `--dry-run`       | flag   | no       | Print intended action without sending the request. |

> At least **one** of `--sl-points` or `--tp-points` must be provided.
> `--from` selects the base price: *entry* uses `PriceOpen`; *market* uses **Bid** for BUY and **Ask** for SELL.

---

## How Prices Are Calculated 🧮

Let `Pbase` be the base price (`entry` or `market`), and `point` be the instrument point size.

* For **BUY** positions:

  * `SL = Pbase − sl_points × point`
  * `TP = Pbase + tp_points × point`
* For **SELL** positions:

  * `SL = Pbase + sl_points × point`
  * `TP = Pbase − tp_points × point`

**Point size guess:** uses `_mt5Account.PointGuess(symbol)`; if it returns ≤ 0, falls back to `0.01` for JPY symbols, otherwise `0.0001`.

---

## Output ⬆️

| Field    | Type   | Description                              |
| -------- | ------ | ---------------------------------------- |
| `Ticket` | ulong  | Modified ticket.                         |
| `NewSL`  | double | Applied SL price (if provided).          |
| `NewTP`  | double | Applied TP price (if provided).          |
| `From`   | string | `entry` or `market` used for base price. |
| `Status` | string | `OK` or error description.               |

Text only. Exit codes: `0` success; `2` validation/not found; `1` fatal error.

---

## How to Use

```powershell
# SL 150 pts below entry, TP 300 pts above entry (BUY logic)
dotnet run -- position.modify.points -p demo -t 123456 --sl-points 150 --tp-points 300

# From MARKET (SELL logic): SL 200 pts, TP 100 pts
dotnet run -- position.modify.points -p demo -t 123456 --from market --sl-points 200 --tp-points 100

# Only TP by points
dotnet run -- position.modify.points -p demo -t 123456 --tp-points 250

# Dry‑run
dotnet run -- position.modify.points -p demo -t 123456 --from market --sl-points 120 --dry-run
```

Shortcast (from `ps/shortcasts.ps1`):

```powershell
pmp -t 123456 -slp 150 -tpp 300 -from entry
# → mt5 position.modify.points -p demo -t 123456 --sl-points 150 --tp-points 300 --from entry --timeout-ms 90000
```

---

## Notes & Safety 🛡️

* Distances are in **points**, not pips; confirm the instrument’s point value (see **[Quote](../Market_Data/Quote.md)** or **[Symbol](../Market_Data/Symbol.md)**).
* Resulting prices must respect broker **StopsLevel** / min distance.
* For *market* base, uses **Bid** for BUY and **Ask** for SELL to avoid instant stops.
* If both point distances are omitted, the command fails fast with a clear error.

---

## Method Signatures

```csharp
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public Task<TickData> SymbolInfoTickAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public double PointGuess(string symbol);

public Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    TimeSpan? pollInterval = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public Task<bool> ModifyPositionSlTpAsync(
    ulong ticket,
    double? sl,
    double? tp,
    CancellationToken ct);
```

---

## Code Reference 🧩

```csharp
posModPts.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(pmpTicketOpt);
    var slPts     = ctx.ParseResult.GetValueForOption(pmpSlPtsOpt);
    var tpPts     = ctx.ParseResult.GetValueForOption(pmpTpPtsOpt);
    var fromStr   = (ctx.ParseResult.GetValueForOption(pmpFromOpt) ?? "entry").Trim().ToLowerInvariant();
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    if (fromStr is not ("entry" or "market")) throw new ArgumentException("--from must be entry|market");
    if (slPts is null && tpPts is null) throw new ArgumentException("Provide --sl-points and/or --tp-points");
    if (slPts is < 0 || tpPts is < 0) throw new ArgumentOutOfRangeException("Point distances must be >= 0");

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:POSITION.MODIFY.POINTS Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} SLpts:{SL} TPpts:{TP} From:{From}", ticket, slPts, tpPts, fromStr))
    {
        await ConnectAsync();

        var opened = await _mt5Account.OpenedOrdersAsync();
        var pos = opened.PositionInfos.FirstOrDefault(p => (ulong)p.Ticket == ticket || unchecked((ulong)p.Ticket) == ticket);
        if (pos is null) { Console.WriteLine($"Position #{ticket} not found."); Environment.ExitCode = 2; return; }

        var symbol  = pos.Symbol;
        var isLong  = IsLongPosition(pos); // helper in project
        var point   = _mt5Account.PointGuess(symbol);
        if (point <= 0) point = symbol.EndsWith("JPY", StringComparison.OrdinalIgnoreCase) ? 0.01 : 0.0001;

        // Base price
        double pbase;
        if (fromStr == "entry") pbase = pos.PriceOpen;
        else
        {
            var tick = await _mt5Account.SymbolInfoTickAsync(symbol);
            pbase = isLong ? tick.Bid : tick.Ask; // BUY→Bid, SELL→Ask
        }

        // Compute targets
        double? newSl = null, newTp = null;
        if (slPts is not null)
            newSl = isLong ? pbase - slPts.Value * point : pbase + slPts.Value * point;
        if (tpPts is not null)
            newTp = isLong ? pbase + tpPts.Value * point : pbase - tpPts.Value * point;

        // Round to point grid
        static double R(double v, double pt) => Math.Round(v / pt) * pt;
        if (newSl is not null) newSl = R(newSl.Value, point);
        if (newTp is not null) newTp = R(newTp.Value, point);

        // Best‑effort ensure visibility (non‑fatal)
        try { await _mt5Account.EnsureSymbolVisibleAsync(symbol, TimeSpan.FromSeconds(3)); } catch { }

        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] POSITION.MODIFY.POINTS #{ticket} {symbol} from={fromStr} SL->{newSl} TP->{newTp}");
            return;
        }

        await _mt5Account.ModifyPositionSlTpAsync(ticket, newSl, newTp, CancellationToken.None);
        Console.WriteLine($"✔ position.modify.points done: ticket={ticket} from={fromStr} SL={newSl} TP={newTp}");
    }
});
```

---

## See also

* **[Modify (by price)](../Orders_Positions/Modify.md)** — set absolute SL/TP
* **[Limits](../Market_Data/Limits.md)** — min/step/max
* **[Quote](../Market_Data/Quote.md)** — check current Bid/Ask
