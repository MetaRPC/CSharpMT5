# Profiles (`profiles.json`) 👤

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

!!! note "Where the file is read from"
The CLI looks for `profiles.json` in the **working directory**.
If you keep it in `Config/`, run from the repo root so it’s discovered, or copy it next to the executable.

!!! warning "Secrets"
Don’t commit real passwords. Prefer an environment variable **`MT5_PASSWORD`** to override `Password` at runtime.

!!! tip "Precedence"
If **`MT5_PASSWORD`** is set, it **wins over** the value in `profiles.json`. You can keep `Password` empty in the file to force env-only usage.

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
# Use a specific profile
dotnet run -- info --profile default

# Quote with an explicit profile & symbol
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
// Validate and select the profile
Validators.EnsureProfile(profile);
_selectedProfile = profile;

// Later used in ConnectAsync()
await ConnectAsync();
```

---

📌 **In short**:

* `profiles.json` = your connection catalog.
* `--profile` or `use-pf` = the switch.
* In code: `Validators.EnsureProfile` → `_selectedProfile` → `ConnectAsync()`.
