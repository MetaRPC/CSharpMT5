# Place (`place`) 🧱

## What it Does 🎯

Places a **pending order** on MT5. Supports **Limit**, **Stop**, and **Stop‑Limit** types, optional **SL/TP**, and **TIF/expiry**.

> Market orders are **not** handled here. Use `buy` / `sell` for market execution.

---

## Supported Types ✅

`buylimit`, `selllimit`, `buystop`, `sellstop`, `buystoplimit`, `sellstoplimit`

---

## Input Parameters ⬇️

| Parameter         | Type            | Required | Description                                                                         |       |        |
| ----------------- | --------------- | -------- | ----------------------------------------------------------------------------------- | ----- | ------ |
| `--profile`, `-p` | string          | ✅        | Profile from `profiles.json`.                                                       |       |        |
| `--symbol`, `-s`  | string          | ✅        | Symbol (e.g., `EURUSD`).                                                            |       |        |
| `--volume`, `-v`  | double          | ✅        | Volume in lots.                                                                     |       |        |
| `--type`          | string          | ✅        | One of: `buylimit` `selllimit` `buystop` `sellstop` `buystoplimit` `sellstoplimit`. |       |        |
| `--price`         | double?         | ⚠️       | **Entry price** for **Limit/Stop** (not used for Stop‑Limit).                       |       |        |
| `--stop`          | double?         | ⚠️       | **Trigger** price for **Stop‑Limit**.                                               |       |        |
| `--limit`         | double?         | ⚠️       | **Limit** price for **Stop‑Limit**.                                                 |       |        |
| `--tif`           | string?         | ❌        | Time‑in‑force: `GTC`                                                                | `DAY` | `GTD`. |
| `--expire`        | DateTimeOffset? | ❌        | Expiry (ISO‑8601) when `--tif=GTD`.                                                 |       |        |
| `--sl`            | double?         | ❌        | Stop Loss price.                                                                    |       |        |
| `--tp`            | double?         | ❌        | Take Profit price.                                                                  |       |        |
| `--timeout-ms`    | int             | ❌        | Per‑RPC timeout in ms (default: `30000`).                                           |       |        |
| `--dry-run`       | flag            | ❌        | Print the plan without sending order.                                               |       |        |

---

## Validation Rules 🔍

* **Stop‑Limit** (`buystoplimit`/`sellstoplimit`):

  * `--stop` **and** `--limit` are **required**.
  * Do **not** pass `--price` (ignored/forbidden).
  * For **Buy Stop‑Limit**: `limit <= stop`.
  * For **Sell Stop‑Limit**: `limit >= stop`.
* **Limit/Stop** (`buylimit`/`selllimit`/`buystop`/`sellstop`):

  * `--price` is **required** and must be `> 0`.
* **TIF**: if `--tif=GTD`, then `--expire` is **required**.
* Market‑type strings are **not supported** in this command (use `buy`/`sell`).

---

## Output Fields ⬆️

| Field    | Type  | Description                                              |
| -------- | ----- | -------------------------------------------------------- |
| `Ticket` | ulong | Ticket of the created pending order (logged on success). |

---

## How to Use 🛠️

### CLI — Limit & Stop

```powershell
# Place Buy Limit @ 1.0950 (0.10 lots)
dotnet run -- place -p demo -s EURUSD -v 0.10 --type buylimit --price 1.0950 --sl 1.0900 --tp 1.1000

# Place Sell Stop @ 1.0900
dotnet run -- place -p demo -s EURUSD -v 0.20 --type sellstop --price 1.0900 --tif DAY
```

### CLI — Stop‑Limit

```powershell
# Buy Stop‑Limit: trigger 1.1000, limit 1.0995
dotnet run -- place -p demo -s EURUSD -v 0.10 --type buystoplimit --stop 1.1000 --limit 1.0995 --sl 1.0950 --tp 1.1050

# Sell Stop‑Limit: trigger 1.0900, limit 1.0905
dotnet run -- place -p demo -s EURUSD -v 0.10 --type sellstoplimit --stop 1.0900 --limit 1.0905 --tif GTD --expire 2025-09-30T15:00:00Z
```

### Dry‑run

```powershell
# Preview without sending
dotnet run -- place -p demo -s EURUSD -v 0.10 --type buystop --price 1.1000 --dry-run
```

---

## Notes & Safety 🛡️

* Before placing, the app best‑effort calls **EnsureSymbolVisibleAsync** (up to \~3s) to avoid server rejections.
* `--sl`/`--tp` are absolute **prices**; must respect broker **StopsLevel** and min distances.
* `volume` must respect **symbol min/step/max** (check `symbol limits`).
* If server doesn’t support the requested type/TIF, you’ll get a descriptive error.

---

## Code Reference (to be filled by you) 🧩

```csharp
var placeTypeOpt  = new Option<string>(new[] { "--type" }, "buylimit|selllimit|buystop|sellstop|buystoplimit|sellstoplimit")
{
    IsRequired = true
};

var placePriceOpt = new Option<double?>(new[] { "--price" }, "Entry price for limit/stop");

var placeStopOpt   = new Option<double?>(new[] { "--stop"  }, "Trigger price for stop/stop-limit");
var placeLimitOpt  = new Option<double?>(new[] { "--limit" }, "Limit price for stop-limit");
var placeTifOpt    = new Option<string?>(new[] { "--tif"   }, "Time-in-force: GTC|DAY|GTD");
var placeExpireOpt = new Option<DateTimeOffset?>(new[] { "--expire" }, "Expiry (ISO-8601) when --tif=GTD");

var place = new Command("place", "Place a pending order");
place.AddAlias("pl");

place.AddOption(profileOpt);
place.AddOption(symbolOpt);
place.AddOption(volumeOpt);
place.AddOption(placeTypeOpt);
place.AddOption(placePriceOpt);
place.AddOption(placeStopOpt);
place.AddOption(placeLimitOpt);
place.AddOption(placeTifOpt);
place.AddOption(placeExpireOpt);
place.AddOption(slOpt);
place.AddOption(tpOpt);

place.SetHandler(async (InvocationContext ctx) =>
{
    var profile    = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var symbolArg  = ctx.ParseResult.GetValueForOption(symbolOpt);
    var volume     = ctx.ParseResult.GetValueForOption(volumeOpt);
    var typeStr    = ctx.ParseResult.GetValueForOption(placeTypeOpt)!;   // buylimit|...|sellstoplimit
    var priceOptV  = ctx.ParseResult.GetValueForOption(placePriceOpt);
    var stopV      = ctx.ParseResult.GetValueForOption(placeStopOpt);
    var limitV     = ctx.ParseResult.GetValueForOption(placeLimitOpt);
    var tifStr     = ctx.ParseResult.GetValueForOption(placeTifOpt);
    var expireV    = ctx.ParseResult.GetValueForOption(placeExpireOpt);
    var sl         = ctx.ParseResult.GetValueForOption(slOpt);
    var tp         = ctx.ParseResult.GetValueForOption(tpOpt);
    var timeoutMs  = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun     = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureVolume(volume);

    var s = Validators.EnsureSymbol(symbolArg ?? GetOptions().DefaultSymbol);
    _selectedProfile = profile;

    var orderKind = ParseOrderType(typeStr);           // TMT5_ENUM_ORDER_TYPE...
    var tifKind   = ParseTif(tifStr);                  // TMT5_ENUM_ORDER_TYPE_TIME...

    bool isStopLimit = orderKind is
        TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyStopLimit or
        TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellStopLimit;

    bool isLimitOrStop = orderKind is
        TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyLimit or
        TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellLimit or
        TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyStop  or
        TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellStop;
```
