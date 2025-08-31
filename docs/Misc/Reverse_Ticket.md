# Reverse Ticket (`reverse.ticket`) 🔄

## What it Does

Reverses a **single position by ticket** — closes the specified position and immediately opens a new one of the opposite side with the same volume (and optional SL/TP).

---

## Input Parameters ⬇️

| Parameter         | Type   |  Description                                         |
| ----------------- | ------ |  --------------------------------------------------- |
| `--profile`, `-p` | string |  Profile to use (from `profiles.json`).              |
| `--ticket`, `-t`  | ulong  |  Position ticket ID to reverse.                      |
| `--sl`            | double |  Optional Stop Loss for the new reversed position.   |
| `--tp`            | double |  Optional Take Profit for the new reversed position. |
| `--deviation`     | int    |  Max. slippage (points), default: 10.                |
| `--timeout-ms`    | int    |  RPC timeout in ms (default: 30000).                 |
| `--output`, `-o`  | string |  Output format: `text` (default) or `json`.          |

---

## Output Fields ⬆️

| Field    | Type   | Description                                   |
| -------- | ------ | --------------------------------------------- |
| `Ticket` | ulong  | Ticket of the **new reversed position**.      |
| `Symbol` | string | Symbol of the trade.                          |
| `Volume` | double | Volume of the new trade.                      |
| `Side`   | string | `BUY` or `SELL` (opposite of the closed one). |
| `SL`     | double | Stop Loss (if set).                           |
| `TP`     | double | Take Profit (if set).                         |

---

## How to Use 🛠️

### CLI

```powershell
# Reverse by ticket
dotnet run -- reverse.ticket -p demo -t 123456 --sl 1.1000 --tp 1.2000
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
rvt -t 123456 --sl 1.1000 --tp 1.2000
# expands to: mt5 reverse.ticket -p demo -t 123456 --sl 1.1000 --tp 1.2000 --timeout-ms 90000
```

---

## When to Use ❓

* To instantly flip exposure for a **specific ticket** without manually closing and re‑sending orders.
* For strategies requiring fast direction switch (scalping, news trading).
* To test opposite scenarios quickly in demo environments.

---

## Notes & Safety 🛡️

* Be careful with SL/TP: if omitted, new order may have none set.
* Market slippage may occur — adjust `--deviation` if needed.
* Always confirm ticket belongs to the intended symbol before reversing.

---

## Code Reference 🧩

```csharp
 var rvTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket") { IsRequired = true };

var reverseTicket = new Command("reverse.ticket", "Reverse a single position by ticket");
reverseTicket.AddAlias("rvt");

reverseTicket.AddOption(profileOpt);
reverseTicket.AddOption(rvTicketOpt);
reverseTicket.AddOption(slOpt);
reverseTicket.AddOption(tpOpt);
reverseTicket.AddOption(devOpt);
reverseTicket.AddOption(timeoutOpt);
reverseTicket.AddOption(dryRunOpt);

reverseTicket.SetHandler(async (InvocationContext ctx) =>
{
    var profile   = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket    = ctx.ParseResult.GetValueForOption(rvTicketOpt);
    var sl        = ctx.ParseResult.GetValueForOption(slOpt);
    var tp        = ctx.ParseResult.GetValueForOption(tpOpt);
    var deviation = ctx.ParseResult.GetValueForOption(devOpt);
    var timeoutMs = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun    = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);

    using (UseOpTimeout(timeoutMs))
    using (_logger.BeginScope("Cmd:REVERSE.TICKET Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket} Dev:{Dev}", ticket, deviation))
    {
        try
        {
            await ConnectAsync();
            using var opCts = StartOpCts();

            var opened = await CallWithRetry(
                ct => _mt5Account.OpenedOrdersAsync(deadline: null, cancellationToken: ct),
                opCts.Token);

            var pos = opened.PositionInfos.FirstOrDefault(p => Convert.ToUInt64(p.Ticket) == ticket);
            if (pos is null)
            {
                Console.WriteLine($"Position #{ticket} not found.");
                Environment.ExitCode = 2;
                return;
            }

            var symbol = pos.Symbol;
            var vol    = pos.Volume;
            var isLong = IsLongPosition(pos); 

            try
            {
                using var visCts = StartOpCts();
                await _mt5Account.EnsureSymbolVisibleAsync(symbol, TimeSpan.FromSeconds(3), null, null, visCts.Token);
            }
            catch { /* ignore */ }

            if (dryRun)
            {
                Console.WriteLine($"[DRY-RUN] REVERSE.TICKET #{ticket} {symbol}: close {vol}, then {(isLong ? "SELL" : "BUY")} {vol} (dev={deviation}) SL={sl} TP={tp}");
                return;
            }

            await CallWithRetry(ct => _mt5Account.ClosePositionPartialAsync(ticket, vol, deviation, ct), opCts.Token);

            await CallWithRetry(ct => _mt5Account.SendMarketOrderAsync(
                    symbol: symbol,
                    isBuy: !isLong,
                    volume: vol,
                    deviation: deviation,
                    stopLoss: sl,
                    takeProfit: tp,
                    deadline: null,
                    cancellationToken: ct),
                opCts.Token);

            Console.WriteLine("✔ reverse.ticket done");
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

root.AddCommand(reverseTicket);
```
