# CloseBy (`closeby`) 🔁

Closes **two opposite positions** on the **same symbol** by emulating MT5 *Close By* with **two separate market closes**.

---

## Preconditions ✅

* Two **open positions** on the **same symbol**.
* Positions must be in **opposite directions** (one BUY, one SELL).
* Account must allow hedging / opposite positions.

---

## Input Parameters

| Parameter         | Type   | Required | Description                                        |
| ----------------- | ------ | -------- | -------------------------------------------------- |
| `--profile`, `-p` | string | yes      | Profile from `profiles.json`.                      |
| `--a`, `-a`       | ulong  | yes      | Ticket of the **first** position.                  |
| `--b`, `-b`       | ulong  | yes      | Ticket of the **opposite** position.               |
| `--volume`, `-v`  | double | yes      | Volume (lots) to close on each leg (see notes).    |
| `--deviation`     | int    | no       | Max slippage (points). **Default:** `10`.          |
| `--timeout-ms`    | int    | no       | RPC timeout in ms. **Default:** `30000`.           |
| `--dry-run`       | flag   | no       | Print the action plan without sending any request. |

> **Note:** This command is **text‑only**; JSON output is **not** supported by the current handler.

---

## Output (text) & Exit Codes

**Actual handler output (from your `Program.cs`):**

```
[DRY-RUN] CLOSEBY a=<A> b=<B> volume=<VOL> deviation=<DEV>
✔ closeby (emulated) done
```

> The current implementation prints a **single success line** and does **not** include symbol/residual details.

Errors (typical):

```
One or both positions not found.                (exit code 2)
Invalid tickets/volume                          (exit code 2)
RPC error: <broker message>                     (exit code 1)
```

**Exit codes**

* `0` — success
* `2` — validation/guard failures
* `1` — fatal error (printed via ErrorPrinter)

---

## How to Use

```powershell
# Emulate CloseBy for two opposite tickets, 0.10 lots on each leg
dotnet run -- closeby -p demo -a 123456 -b 654321 -v 0.10

# Custom deviation
dotnet run -- closeby -p demo -a 111111 -b 222222 -v 0.05 --deviation 20

# Dry‑run plan
dotnet run -- closeby -p demo -a 111111 -b 222222 -v 0.02 --dry-run
```

> No built‑in PowerShell alias in `ps/shortcasts.ps1`. You can add:
>
> ```powershell
> function cb { param([ulong]$a,[ulong]$b,[double]$v,[string]$p=$PF,[int]$to=$TO)
>   mt5 closeby -p $p -a $a -b $b -v $v --timeout-ms $to }
> ```

---

## Notes & Safety 🛡️

* **Non‑atomic:** two independent closes → slippage/partial failures possible.
* **Assumptions not enforced by code:** the current handler does **not** validate same‑symbol or opposite sides; it simply attempts two partial closes via `CloseByEmulatedAsync`. Ensure tickets really are opposite legs on the same symbol.
* **Volume clamping:** the helper is called with the requested volume; if it exceeds available, the broker will reject. Ensure volume ≤ min(position volumes).
* **Lot limits:** requested volume must comply with **min/step/max**. See **[symbol limits](../Market_Data/Limits.md)**.
* **Visibility:** some brokers require the symbol to be visible — best‑effort ensure visibility before closing.

---

## Method Signatures (quick ref)

```csharp
// Emulated CloseBy (actual signature in your MT5Account.cs)
public Task CloseByEmulatedAsync(
    ulong ticketA,
    ulong ticketB,
    double volume,
    int deviation,
    CancellationToken ct);

// Also used around it in other commands (not strictly required here):
public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public Task<(double min, double step, double max)> GetVolumeConstraintsAsync(
    string symbol,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public Task EnsureSymbolVisibleAsync(
    string symbol,
    TimeSpan? maxWait = null,
    TimeSpan? pollInterval = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

---

## Code Reference (actual call site)

```csharp
closeby.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var a         = ctx.ParseResult.GetValueForOption(cbATicketOpt);
    var b         = ctx.ParseResult.GetValueForOption(cbBTicketOpt);
    var volume    = ctx.ParseResult.GetValueForOption(cbVolOpt);
    var deviation = ctx.ParseResult.GetValueForOption(cbDevOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    if (a == 0 || b == 0) throw new ArgumentOutOfRangeException("tickets", "Tickets must be > 0.");
    if (volume <= 0) throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be > 0.");

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:CLOSEBY Profile:{Profile}", profile))
    using (_logger.BeginScope("A:{A} B:{B} Vol:{Vol} Dev:{Dev}", a, b, volume, deviation))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] CLOSEBY a={a} b={b} volume={volume} deviation={deviation}");
            return;
        }

        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            await CallWithRetry(ct => _mt5Account.CloseByEmulatedAsync(a, b, volume, deviation, ct), opCts.Token);

            Console.WriteLine("✔ closeby (emulated) done");
        }
        catch (Exception ex)
        {
            ErrorPrinter.Print(_logger, ex, IsDetailed());
            Environment.ExitCode = 1;
        }
        finally
        {
            try { await _mt5Account.DisconnectAsync(); } catch { /* ignore */ }
        }
    }
});
```

---

## See also

* **[`close.partial`](./Close.partial.md)** — close an exact lot amount per position
* **[`close.percent`](./Close.percent.md)** — close a percentage of current volume
* **[`symbol limits`](../Market_Data/Limits.md)** — min/step/max lot constraints
