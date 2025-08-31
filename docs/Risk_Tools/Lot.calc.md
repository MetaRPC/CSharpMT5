# Lot Calc (`lot.calc`) 📐

## What it Does 🎯

Calculates a **position volume (lots)** from **risk %** and **Stop-Loss distance (points)**.
Returns a size that fits the risk budget and respects the symbol’s min/step/max limits.

---

## Input Parameters ⬇️

| Parameter        | Type   | Required | Description                                                              |
| ---------------- | ------ | -------- | ------------------------------------------------------------------------ |
| `--symbol`, `-s` | string | ✅        | Symbol to trade (e.g. `EURUSD`).                                         |
| `--risk-pct`     | double | ✅        | Risk percent of **balance** to put at stake (e.g. `1.0`).                |
| `--sl-points`    | int    | ✅        | Stop-Loss distance in **points** (e.g. `200`).                           |
| `--balance`      | double | ✅        | Account balance used for the calc (e.g. `1000`).                         |
| `--min-lot`      | double | ✅        | Minimum lot allowed for the symbol.                                      |
| `--lot-step`     | double | ✅        | Lot increment step.                                                      |
| `--max-lot`      | double | ❌        | Maximum lot (optional; if omitted, no upper clamp beyond server limits). |
| `--output`, `-o` | string | ❌        | Output: `text` (default) or `json`.                                      |
| `--timeout-ms`   | int    | ❌        | Timeout in ms (default: 30000).                                          |

> Tip: You can fetch `min-lot/lot-step/max-lot` first via `symbol limits`.

---

## Output Fields ⬆️

| Field                 | Type    | Description                                         |
| --------------------- | ------- | --------------------------------------------------- |
| `symbol`              | string  | Target symbol used for calculation.                 |
| `balance`             | double  | Balance value passed in the request.                |
| `risk_pct`            | double  | Risk percentage applied to balance.                 |
| `sl_points`           | int     | Stop-loss distance in points.                       |
| `point_value_per_lot` | double  | Money value per point per 1 lot (from server).      |
| `volume_raw`          | double  | Calculated raw volume before rounding.              |
| `volume`              | double  | Final recommended volume (after rounding/clamping). |
| `lot_step`            | double  | Lot step used for rounding.                         |
| `min_lot`             | double  | Minimum lot allowed.                                |
| `max_lot`             | double? | Maximum lot cap (if specified).                     |

|-------------------------|---------|-------------|
| `symbol`                | string  | Target symbol used for calculation. |
| `balance`               | double  | Balance value passed in the request. |
| `risk_pct`              | double  | Risk percentage applied to balance. |
| `sl_points`             | int     | Stop‑loss distance in points. |
| `point_value_per_lot`   | double  | Money value per point per 1 lot (from server). |
| `volume_raw`            | double  | Calculated raw volume before rounding. |
| `volume`                | double  | Final recommended volume (after rounding and clamping). |
| `lot_step`              | double  | Lot step used for rounding. |
| `min_lot`               | double  | Minimum lot allowed. |
| `max_lot`               | double? | Maximum lot cap (if specified). |

-----------------------|--------|-------------|
| `Symbol`              | string | Target symbol. |
| `RiskPct`             | double | Risk percent used. |
| `Balance`             | double | Balance used in the calc. |
| `StopDistancePoints`  | int    | SL distance in points. |
| `ValuePerPoint`       | double | Estimated money value per point per 1.0 lot (symbol-specific). |
| `RawLots`             | double | Unclamped/unrounded lots from the risk formula. |
| `RoundedLots`         | double | Rounded to `lot-step`. |
| `ClampedLots`         | double | Enforced by `min-lot` and `max-lot` (if provided). |
| `RiskAmount`          | double | Monetary risk for `ClampedLots`. |

---

## How to Use 🛠️

### CLI

```powershell
# 1% risk, 200 pts SL, 1000 balance, typical FX limits
dotnet run -- lot.calc --symbol EURUSD --risk-pct 1 --sl-points 200 --balance 1000 --min-lot 0.01 --lot-step 0.01 --max-lot 5 -o json
```

### PowerShell Shortcuts (from `shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-sym EURUSD
lc -s EURUSD -riskPct 1 -slPts 200 -balance 1000 -minLot 0.01 -lotStep 0.01 -maxLot 5 -o json
# expands to: mt5 lot.calc --symbol EURUSD --risk-pct 1 --sl-points 200 --balance 1000 --min-lot 0.01 --lot-step 0.01 --max-lot 5 -o json --timeout-ms 90000
```

---

## Formula (Concept) 🧮

1. Risk amount = `Balance * RiskPct / 100`
2. Points value for 1 lot = `ValuePerPoint` (depends on symbol, contract size, tick value)
3. Lots (raw) = `Risk amount / (SL points * ValuePerPoint)`
4. Round to `lot-step`, then clamp to `[min-lot, max-lot]`.

> Your implementation may compute `ValuePerPoint` on the server side. If you compute locally, make sure to use correct **contract size** and **pip/point** definitions for the symbol.

---

## When to Use ❓

* Pre‑trade sizing for **buy/sell** commands.
* Risk controls in bots and scripts.
* UI validations (disable order button if calc → below min-lot).

---

## Notes & Safety 🛡️

* If `sl-points` is too small, the required volume may exceed `max-lot` — the result will be clamped and **actual risk may be lower** than requested.
* Always confirm the **point size** and **contract size** for non‑FX symbols (CFDs, metals, crypto).
* For netting vs hedging modes, risk logic might differ; adjust accordingly.

---

## Code Reference (exact) 🧩

```csharp
var lcSymbolOpt   = new Option<string>(new[] { "--symbol", "-s" }, () => GetOptions().DefaultSymbol, "Symbol");
var lcRiskPctOpt  = new Option<double>(new[] { "--risk-pct" }, "Risk percent of balance (e.g., 1 for 1%)") { IsRequired = true };
var lcSlPtsOpt    = new Option<int>(new[] { "--sl-points" }, "Stop-loss distance in POINTS") { IsRequired = true };
var lcBalanceOpt  = new Option<double>(new[] { "--balance" }, "Account balance (same currency as risk)") { IsRequired = true };
var lcMinLotOpt   = new Option<double>(new[] { "--min-lot" }, () => 0.01, "Min lot size");
var lcStepLotOpt  = new Option<double>(new[] { "--lot-step" }, () => 0.01, "Lot step");
var lcMaxLotOpt   = new Option<double?>(new[] { "--max-lot" }, "Max lot size cap (optional)");

var lotCalc = new Command("lot.calc", "Calculate position volume by risk % and SL distance (points)");
lotCalc.AddAlias("lc");

lotCalc.AddOption(lcSymbolOpt);
lotCalc.AddOption(lcRiskPctOpt);
lotCalc.AddOption(lcSlPtsOpt);
lotCalc.AddOption(lcBalanceOpt);
lotCalc.AddOption(lcMinLotOpt);
lotCalc.AddOption(lcStepLotOpt);
lotCalc.AddOption(lcMaxLotOpt);
lotCalc.AddOption(timeoutOpt);
lotCalc.AddOption(outputOpt);

lotCalc.SetHandler(async (InvocationContext ctx) =>
```
