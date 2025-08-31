# Account — Overview 🧭

This section groups all commands related to **account identity, connectivity, and basic status**.

## Files in this section

* **[Info](./Info.md)** — one‑shot account snapshot (balance, equity, margin info via summary + extras).
* **[Profiles](./Profiles.md)** — how profiles work (`profiles.json`), switching, defaults, and tips.
* **[Show](./Show.md)** — detailed account info dump (raw fields, troubleshooting, and JSON mode).

---

## When to use which?

| Task                                                | Use          |
| --------------------------------------------------- | ------------ |
| Quick health check before trading                   | **Info**     |
| Manage / switch connection targets (demo/live)      | **Profiles** |
| Deep inspection (IDs, server clock, leverage, etc.) | **Show**     |

---

## Quickstarts ⚡

```powershell
# Account snapshot (text)
dotnet run -- info -p demo

# Detailed view (JSON)
dotnet run -- show -p demo -o json

# Switch default profile (PowerShell shortcast)
. .\ps\shortcasts.ps1
use-pf demo
info   # expands to: mt5 info -p demo --timeout-ms 90000
```

---

## Notes 📝

* All commands honor `--timeout-ms` and use a single **connect → call → disconnect** flow.
* JSON outputs are intended for scripting/CI; text is friendly for terminals.
* If a command reports *not connected*, verify your **profile credentials** and network access first.
