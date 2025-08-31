# Close (`close`) 🔒

## What it Does

Closes an **open position** or **order** in MT5 by ticket ID.
Supports both **real execution** and **dry-run** mode for testing.

---

## Input Parameters ⬇️

| Parameter         | Type   | Description                                             |
| ----------------- | ------ | ------------------------------------------------------- |
| `--profile`, `-p` | string | Which profile to use (from `profiles.json`).            |
| `--ticket`, `-t`  | ulong  | Ticket ID of the order/position to close.               |
| `--volume`, `-v`  | double | Volume (lots) to close. If omitted → close full volume. |
| `--deviation`     | int    | Max slippage in points (default: 10).                   |
| `--output`, `-o`  | string |  `text` (default) or `json`.                             |
| `--timeout-ms`    | int    | RPC timeout in ms (default: 30000).                     |
| `--dry-run`       | flag   | Print what would happen, but don’t send order.          |

---

## Output Fields ⬆️

| Field    | Type   | Description                           |
| -------- | ------ | ------------------------------------- |
| `Ticket` | ulong  | Ticket ID of the closed position.     |
| `Volume` | double | Volume closed.                        |
| `Result` | string | Result of operation (`OK` / `Error`). |

---

## How to Use 🛠️

### CLI

```powershell
# Close position 123456 completely
dotnet run -- close -p demo -t 123456

# Close partially (0.1 lots) with JSON output
dotnet run -- close -p demo -t 123456 -v 0.1 -o json --timeout-ms 60000
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
close -t 123456
```

---

## When to Use ❓

* To exit a trade by ticket ID.
* For partial closes when only part of the volume is specified.
* For testing logic with `--dry-run` before sending real requests.

---

## Code Reference 🧩

```csharp
var close = new Command("close", "Close order by ticket");
close.AddAlias("c");

close.AddOption(profileOpt);
close.AddOption(ticketOpt);
close.AddOption(volumeOpt);
close.AddOption(devOpt);
close.AddOption(outputOpt);
close.AddOption(timeoutOpt);
close.AddOption(dryRunOpt);

close.SetHandler(async (InvocationContext ctx) =>
{
    var profile = ctx.ParseResult.GetValueForOption(profileOpt)!;
    var ticket  = ctx.ParseResult.GetValueForOption(ticketOpt);
    var volume  = ctx.ParseResult.GetValueForOption(volumeOpt);
    var dev     = ctx.ParseResult.GetValueForOption(devOpt);
    var output  = ctx.ParseResult.GetValueForOption(outputOpt) ?? "text";
    var timeout = ctx.ParseResult.GetValueForOption(timeoutOpt);
    var dryRun  = ctx.ParseResult.GetValueForOption(dryRunOpt);

    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    if (volume.HasValue) Validators.EnsureVolume(volume.Value);
    Validators.EnsureDeviation(dev);

    _selectedProfile = profile;

    using (UseOpTimeout(timeout))
    using (_logger.BeginScope("Cmd:CLOSE Profile:{Profile}", profile))
    using (_logger.BeginScope("Ticket:{Ticket}", ticket))
    using (_logger.BeginScope("Volume:{Vol}", volume ?? -1))
    {
        if (dryRun)
        {
            Console.WriteLine($"[DRY-RUN] CLOSE ticket={ticket} vol={volume ?? -1}");
            return;
        }

        try
        {
            await ConnectAsync();
```

---

📌 In short:
— `close` lets you **exit trades by ticket**, fully or partially.
— Works with **profiles**, **timeouts**, **dry-run**, and **output modes**.
