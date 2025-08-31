# Misc Overview 🛠️

This section collects **utility commands** that don’t fall strictly into Account, History, or Market Data categories. They are mostly focused on **ticket-level operations** and quick lists.

---

## Included Commands 📑

| Command                               | Alias  | Description                                                    |
| ------------------------------------- | ------ | -------------------------------------------------------------- |
| [List](List.md)                       | —      | Show profile names or other generic lists.                     |
| [Pending List](Pending_List.md)       | `pdls` | List pending orders in the account.                            |
| [Reverse Ticket](Reverse_Ticket.md)   | `rvt`  | Reverse a specific position by its ticket.                     |
| [Specific Ticket](Specific_Ticket.md) | —      | Utility to work with a specific ticket (lookups, validation).  |
| [Ticket Show](Ticket_Show.md)         | `tsh`  | Show detailed info for a ticket (open or from recent history). |

---

## Purpose 🎯

* Provide **ticket-level control** for positions and orders.
* Help with **diagnostics** when checking individual orders.
* Supply **lists** of pending orders or profiles to quickly orient in the environment.

---

## Typical Use Cases ⚙️

* Quickly check what pending orders exist: `pdls`.
* Reverse an open position without guessing the symbol: `rvt -t 123456`.
* Show a ticket's history context for audit or debugging: `tsh -t 123456 -d 30`.
* Validate ticket presence or extract info programmatically with `Specific_Ticket`.

---

## Shortcuts 🔑 (from `ps/shortcasts.ps1`)

* `pdls` → `mt5 pending list`
* `rvt` → `mt5 reverse.ticket`
* `tsh` → `mt5 ticket show`

---

## Notes 📝

These commands are auxiliary but very handy when building scripts or diagnosing issues. They complement the main trading and account-management commands by focusing on **individual orders/tickets**.
