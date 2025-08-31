хорошо
идем дальше
# Profiles (`profiles.json`)

## What it Does 🎯

Profiles act as **connection presets** for MT5 accounts.
Each entry in `profiles.json` stores credentials and server details, so you don’t have to type them every time.

---

## Structure 📂

Typical `profiles.json`:

```json
{
  "default": {
    "ServerName": "MetaQuotes-Demo",
    "Host": "mt5.mrpc.pro",
    "Port": 443,
    "Login": 21455,
    "Password": "demo-pass"
  },
  "live": {
    "ServerName": "MyBroker-Live",
    "Host": "live.broker.com",
    "Port": 443,
    "Login": 123456,
    "Password": "super-secret"
  }
}
```

* `default` — fallback profile (used if you don’t specify `--profile`).
* `live` — second profile for a real account.

---

## Input Parameters ⬇️

When you run commands (`info`, `quote`, `buy`, `sell`, etc.):

| Parameter   | Description                                 | Example             |
| ----------- | ------------------------------------------- | ------------------- |
| `--profile` | Name of the profile from `profiles.json`.   | `--profile default` |
| `use-pf`    | PowerShell shortcut for switching profiles. | `use-pf live`       |

---

## Why Profiles ❓

* **Convenience**: No need to retype login/password/server each time.
* **Safety**: Store creds in one file (never hardcode them in scripts).
* **Flexibility**: Switch instantly between demo/live environments.
* **Automation**: CI/CD pipelines can swap profiles without touching code.

---

## How to Use 🛠️

### CLI

```powershell
dotnet run -- info --profile default
dotnet run -- quote --profile live --symbol EURUSD
```

### PowerShell Shortcuts

```powershell
. .\ps\shortcasts.ps1
use-pf default   # selects default profile
info             # runs with that profile
use-pf live      # instantly switch
info             # now runs on live account
```

---

## Code Reference 🧩

```csharp
// Validate and select profile
Validators.EnsureProfile(profile);
_selectedProfile = profile;

// later used in ConnectAsync()
await ConnectAsync();
```

---

📌 In short:
— `profiles.json` = your connection catalog.
— `--profile` or `use-pf` = the switch.
— In code always via `Validators.EnsureProfile` → `_selectedProfile`.
