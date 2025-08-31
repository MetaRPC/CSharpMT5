# Limits (`limits`) 📏

## What it Does

Shows **volume trading limits** for a given symbol in MT5:

* **Minimum lot**
* **Lot step**
* **Maximum lot**

This helps validate user input before sending orders.

---

## Input Parameters ⬇️

| Parameter         | Type   |Description                                  |
| ----------------- | ------ |-------------------------------------------- |
| `--profile`, `-p` | string | Which profile to use (from `profiles.json`). |
| `--symbol`, `-s`  | string | Target symbol (e.g. `EURUSD`).               |
| `--output`, `-o`  | string | `text` (default) or `json`.                  |
| `--timeout-ms`    | int    |  RPC timeout in ms (default: 30000).          |

---

## Output Fields ⬆️

| Field        | Type   | Description                          |
| ------------ | ------ | ------------------------------------ |
| `VolumeMin`  | double | Minimum volume (lots).               |
| `VolumeStep` | double | Allowed step (increment) for volume. |
| `VolumeMax`  | double | Maximum volume (lots).               |

---

## How to Use 🛠️

### CLI

```powershell
# Show limits for EURUSD
dotnet run -- limits -p demo -s EURUSD

# JSON output
dotnet run -- limits -p demo -s EURUSD -o json
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf demo
limits -s EURUSD
```

---

## When to Use ❓

* Before sending `buy` / `sell` to ensure requested `--volume` is valid.
* In user-facing UIs to disable invalid inputs.
* In risk-checks or bots to prevent broker rejections.

---

## Notes & Safety 🛡️

* MT5 enforces these values strictly — invalid `volume` will cause `Invalid Volume` errors on order send.
* Always round requested volumes to the nearest `VolumeStep`.

---

## Code Reference 🧩

```csharp
var symLimits = new Command("limits", "Show min/step/max volume for the symbol");
symLimits.AddAlias("lim");
            symLimits.SetHandler(async (string profile, string output, string? s, int timeoutMs) =>
            {
                Validators.EnsureProfile(profile);
                var symbolName = Validators.EnsureSymbol(s ?? GetOptions().DefaultSymbol);
                _selectedProfile = profile;

                using (UseOpTimeout(timeoutMs))
                using (_logger.BeginScope("Cmd:SYMBOL-LIMITS Profile:{Profile}", profile))
                using (_logger.BeginScope("Symbol:{Symbol}", symbolName))
                {
                    try
                    {
                        await ConnectAsync();

```
