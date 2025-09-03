# Trail Stop (`trail.stop`) 🛑

Stops a **local trailing stop** previously started with [`trail.start`](./Trail.start.md).

> This cancels the **client‑side** trailing loop for a given position. No server RPC is required.

Alias: `trstop`

---

## Purpose

* Cancel the active trailing logic for a **specific position ticket**.
* Useful when you no longer want SL to auto‑adjust.

---

## Usage 💻

```powershell
dotnet run -- trail.stop -p demo -t 123456
```

---

## Options ⚙️

| Option      | Alias | Required | Description                          |
| ----------- | ----- | -------- | ------------------------------------ |
| `--profile` | `-p`  | yes      | Profile name (from `profiles.json`). |
| `--ticket`  | `-t`  | yes      | Position ticket (numeric ID).        |

> Tip: No `--timeout-ms` is needed; stopping is **local**.

---

## Output ✅

* Success: `✔ trailing stopped for #<ticket>`
* If no active trailer is found: `No active trailing for #<ticket>` (exit code `2`).

---

## Notes & Safety 🛡️

* Trailing is **client‑side** — it runs only while your app/CLI is running.
* Stopping a trailer **does not modify** SL/TP on the server; it only stops further automatic updates.
* You can manually adjust SL/TP afterwards using **[Modify](./Modify.md)** or **[Position.modify.points](./Position.modify.points.md)**.

---

## Code Reference (concise) 💻

> Implementation is local to your CLI (Program). If you keep a registry of active trailers (e.g., `ConcurrentDictionary<ulong, CancellationTokenSource>`), stopping is simply a `Cancel()` + removal.

```csharp
var trTicketOpt = new Option<ulong>(new[] { "--ticket", "-t" }, "Position ticket") { IsRequired = true };

var trailStop = new Command("trail.stop", "Stop local trailing stop for a position");
trailStop.AddOption(profileOpt);
trailStop.AddOption(trTicketOpt);

trailStop.SetHandler((string profile, ulong ticket) =>
{
    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);

    if (StopTrailingLocal(ticket))
        Console.WriteLine($"\u2714 trailing stopped for #{ticket}");
    else
    {
        Console.WriteLine($"No active trailing for #{ticket}");
        Environment.ExitCode = 2;
    }
}, profileOpt, trTicketOpt);

// Helper in Program:
static readonly ConcurrentDictionary<ulong, CancellationTokenSource> _trailSessions = new();
static bool StopTrailingLocal(ulong ticket)
{
    return _trailSessions.TryRemove(ticket, out var cts) && TryCancel(cts);
}
static bool TryCancel(CancellationTokenSource cts)
{
    try { cts.Cancel(); return true; } catch { return false; }
}
```

---

## See also

* **[Trail.start](./Trail.start.md)** — start a local trailing session
* **[Modify](./Modify.md)** — set SL/TP by absolute price
* **[Position.modify.points](./Position.modify.points.md)** — set SL/TP by distance in points
* **[Subscribe](../Streaming/Subscribe.md)** — streaming quotes used by trailing
