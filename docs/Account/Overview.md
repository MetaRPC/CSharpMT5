# Account — Overview 🧭

This section groups all commands related to **account identity, connectivity, and basic status**.

---

## 📂 Files in this section

???+ info "Open the docs in this section"
    * **[Info](./Info.md)** — one-shot account snapshot (balance, equity, margin info via summary + extras).
    * **[Profiles](./Profiles.md)** — how profiles work (`profiles.json`), switching, defaults, and tips.
    * **[Show](./Show.md)** — detailed account info dump (raw fields, troubleshooting, and JSON mode).

---

## 🤔 When to use which?

???+ question "Decision table"
    | Task                                                | Use          |
    | --------------------------------------------------- | ------------ |
    | Quick health check before trading                   | **[Info](./Info.md)**     |
    | Manage / switch connection targets (demo / live)    | **[Profiles](./Profiles.md)** |
    | Deep inspection (IDs, server clock, leverage, etc.) | **[Show](./Show.md)**     |

    **Rule of thumb:** start with **Info** → if credentials/connection look off, jump to **Profiles** → for deep dives use **Show**.

---

## ⚡ Quickstarts

???+ success "Account snapshot (text)"
    ```powershell
    dotnet run -- info -p demo
    ```

???+ example "Detailed view (JSON)"
    ```powershell
    dotnet run -- show -p demo -o json
    ```

???+ tip "Switch default profile (PowerShell shortcast)"
    ```powershell
    . .\ps\shortcasts.ps1
    use-pf demo
    info   # expands to: mt5 info -p demo --timeout-ms 90000
    ```

---

## 📝 Notes

???+ warning "Good to know"
    * All commands honor `--timeout-ms` and use a single **connect → call → disconnect** flow.
    * JSON outputs are intended for scripting/CI; text is friendlier for terminals.
    * If a command reports *not connected*, verify your **profile credentials** and network access first.
    * Need more? See **[Troubleshooting & FAQ](../Troubleshooting%28FAQ%29.md)** and **[Timeouts & Retries Policy](../Timeouts_RetriesPolicy.md)**.

---

## 🔗 Related

???+ quote "Jump to other areas"
    * Account: **[Info](./Info.md)** · **[Profiles](./Profiles.md)** · **[Show](./Show.md)**
    * Market data: **[Quote](../Market_Data/Quote.md)** · **[Symbol](../Market_Data/Symbol.md)** · **[Ensure Visible](../Market_Data/Ensure_Symbol_Visible.md)**
    * Orders & Positions: **[Overview](../Orders_Positions/Orders_Positions_Overview.md)** · **[Place](../Orders_Positions/Place.md)** · **[Modify](../Orders_Positions/Modify.md)**
