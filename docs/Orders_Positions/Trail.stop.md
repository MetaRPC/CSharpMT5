# `trail.stop` 🛑

Stops a local trailing stop that was previously started with [`trail.start`](../trail_start_doc).

---

## Purpose 

* Cancel an active trailing stop logic for a specific position.
* Useful when you no longer want the system to automatically move Stop Loss.

---

## Usage 💻

```powershell
dotnet run -- trail.stop -p demo -t 123456
```

### Options ⚙️

| Option      | Alias | Description                       |
| ----------- | ----- | --------------------------------- |
| `--profile` | `-p`  | Profile name (from profiles.json) |
| `--ticket`  | `-t`  | Position ticket (numeric ID)      |

---

## Example ✅

```powershell
dotnet run -- trail.stop -p demo -t 123456
```

Output:

```
✔ trailing stopped for #123456
```

---

## Shortcuts ⌨️

In PowerShell (`ps/shortcasts.ps1`):

```powershell
# no direct alias, but can be scripted if needed
mt5 trail.stop -p $PF -t <ticket>
```

---

## Code Reference 🧩

```csharp
var trailStop = new Command("trail.stop", "Stop local trailing stop");
trailStop.AddOption(profileOpt);
trailStop.AddOption(trTicketOpt);

trailStop.SetHandler((string profile, ulong ticket) =>
{
    Validators.EnsureProfile(profile);
    Validators.EnsureTicket(ticket);
    _mt5Account.StopTrailing(ticket);
    Console.WriteLine($"✔ trailing stopped for #{ticket}");
}, profileOpt, trTicketOpt);

root.AddCommand(trailStop);
```
