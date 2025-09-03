# Close Half (`close.half`) ✂️

Closes **half of a position’s volume** by ticket.

> This command is a **thin alias** for **[`close.percent`](./Close.percent.md)** with `--pct 50`. All validation, printing, and exit codes are inherited from `close.percent`.

---

## Input Parameters

| Parameter         | Type   | Required | Description                           |
| ----------------- | ------ | -------- | ------------------------------------- |
| `--profile`, `-p` | string | yes      | Profile from `profiles.json`.         |
| `--ticket`, `-t`  | ulong  | yes      | Position ticket to partially close.   |
| `--deviation`     | int    | no       | Max slippage (points). Default: `10`. |
| `--timeout-ms`    | int    | no       | RPC timeout in ms (default: `30000`). |

> **Note:** `close.half` does **not** accept `--output` or `--dry-run` directly. The handler delegates to `close.percent --pct 50` and only forwards the options listed above.

---

## Output & Exit Codes

Output format and exit codes are the **same as** in [`close.percent`](./Close.percent.md).

During execution you may see an info line:

```
(alias) close.half -> close.percent --pct 50
```

---

## How to Use

```powershell
# Close half of position 123456
dotnet run -- close.half -p demo -t 123456

# With custom slippage
dotnet run -- close.half -p demo -t 123456 --deviation 20

# With custom timeout
dotnet run -- close.half -p demo -t 123456 --timeout-ms 60000
```

### PowerShell shortcut (from `ps/shortcasts.ps1`)

```powershell
. .\ps\shortcasts.ps1
use-pf demo
ch -t 123456
# expands to: mt5 close.half -p demo -t 123456 --deviation 10 --timeout-ms 90000
```

---

## Notes & Safety

* Real execution is performed by [`close.percent`](./Close.percent.md). If half-volume doesn’t align with the lot step, rounding is handled by the base command.
* Brokers may reject too-small residual lots — check **[symbol limits](../Market_Data/Limits.md)**.
* `--deviation` matters in fast markets; widen if you see slippage rejections.

---

## Method Signatures (quick ref)

> `close.half` delegates to `close.percent`, which in turn uses the underlying MT5Account RPCs below. Depending on build, either of the close helpers may be used.

```csharp
// Read open positions to resolve symbol & current volume
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// Best‑effort: ensure the symbol is visible before trading
public Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    TimeSpan? pollInterval = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

// Partial close (variant A — explicit deviation)
public Task ClosePositionPartialAsync(
    ulong ticket,
    double volume,
    int deviation,
    CancellationToken cancellationToken);

// Partial close (variant B — by symbol; deviation handled internally/defaults)
public Task CloseOrderByTicketAsync(
    ulong ticket,
    string symbol,
    double volume,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

## Alias wiring (illustrative)

```csharp
ctx.Console.WriteLine("(alias) close.half -> close.percent --pct 50");

await closePercent.InvokeAsync(new[]
{
    "--profile",    profile,
    "--ticket",     ticket.ToString(CultureInfo.InvariantCulture),
    "--pct",        "50",
    "--deviation",  deviation.ToString(CultureInfo.InvariantCulture),
    "--timeout-ms", timeoutMs.ToString(CultureInfo.InvariantCulture)
    // Note: --dry-run and --output are NOT forwarded in this alias
});
```

---

## See also

* **[`close.percent`](./Close.percent.md)** — base command for partial closes
* **[`symbol limits`](../Market_Data/Limits.md)** — min/step/max lot constraints
