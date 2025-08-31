# CloseBy (`closeby`) 🔁

## What it Does

Closes **two opposite positions** on the **same symbol** by emulating MT5 *Close By* with **two separate closes**.
This is implemented via `_mt5Account.CloseByEmulatedAsync(...)`.

**Note:** Unlike native MT5 CloseBy, this emulated version executes two market closes, so commissions/swaps apply for both legs.

---

## Preconditions ✅

* Two **open positions** on the **same symbol**.
* Positions must be in **opposite directions** (one BUY, one SELL).
* Account must allow hedging / opposite positions.

---

## Input Parameters ⬇️

| Parameter         | Type   | Description                               |
| ----------------- | ------ | ----------------------------------------- |
| `--profile`, `-p` | string |  Profile from `profiles.json`.             |
| `--a`, `-a`       | ulong  |  Ticket of the **first position**.         |
| `--b`, `-b`       | ulong  |  Ticket of the **opposite position**.      |
| `--volume`, `-v`  | double |  Volume (lots) to close on each leg.       |
| `--deviation`     | int    |  Max slippage in points. Default: `10`.    |
| `--timeout-ms`    | int    |  Per‑RPC timeout in ms (default: `30000`). |
| `--dry-run`       | flag   |  Print action without sending request.     |

---

## Output Fields ⬆️

| Field       | Type   | Description                                   |
| ----------- | ------ | --------------------------------------------- |
| `TicketA`   | ulong  | Primary ticket (`--a`).                       |
| `TicketB`   | ulong  | Counter ticket (`--b`).                       |
| `Symbol`    | string | Symbol of both positions.                     |
| `ClosedVol` | double | Volume closed on each leg.                    |
| `Residual`  | object | If volumes differ: remaining ticket & volume. |
| `Status`    | string | `OK` or error description.                    |

---

## How to Use 🛠️

### CLI

```powershell
# Close two opposite positions against each other (emulated)
dotnet run -- closeby -p demo -a 123456 -b 654321 -v 0.10

# JSON + custom deviation
dotnet run -- closeby -p demo -a 111111 -b 222222 -v 0.05 --deviation 20 -o json

# Dry-run
dotnet run -- closeby -p demo -a 111111 -b 222222 -v 0.02 --dry-run
```

### PowerShell Shortcut (optional)

Not present in your `shortcasts.ps1`. You may add one like:

```powershell
function cb { param([ulong]$a,[ulong]$b,[double]$v,[string]$p=$PF,[int]$to=$TO)
  mt5 closeby -p $p -a $a -b $b -v $v --timeout-ms $to }
```

---

## When to Use ❓

* You have BUY and SELL on the same symbol and want to flatten both by equal volume.
* For scripts/algos that require a CloseBy‑like effect but broker/account does not support native CloseBy.

---

## Notes & Safety 🛡️

* This is an **emulation**: broker processes 2 closes, not a single atomic one.
* Symbols must match exactly.
* If volumes are unequal, the larger position will remain open with reduced size.

---

## Code Reference 🧩

```csharp
     var cbATicketOpt = new Option<ulong>(new[] { "--a", "-a" }, "Ticket of the first position") { IsRequired = true };
var cbBTicketOpt = new Option<ulong>(new[] { "--b", "-b" }, "Ticket of the opposite position") { IsRequired = true };
var cbVolOpt     = new Option<double>(new[] { "--volume", "-v" }, "Volume (lots) to close on each leg") { IsRequired = true };
var cbDevOpt     = new Option<int>(new[] { "--deviation" }, () => 10, "Max slippage in points");

var closeby = new Command("closeby", "Close a position by the opposite position (emulated with two closes)");
closeby.AddOption(profileOpt);
closeby.AddOption(cbATicketOpt);
closeby.AddOption(cbBTicketOpt);
closeby.AddOption(cbVolOpt);
closeby.AddOption(cbDevOpt);
closeby.AddOption(timeoutOpt);
closeby.AddOption(dryRunOpt);

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
```
