# Market Data Overview 📊

This section contains commands related to **symbols, quotes, and trading utilities**. These commands allow you to:

* Fetch quotes and reverse trades.
* Manage open and pending orders.
* Ensure symbols are available for trading.
* Inspect trading limits and panic‑close positions.

---

## Available Commands ⚙️

| Command                                               | Alias   | Description                                                   |
| ----------------------------------------------------- | ------- | ------------------------------------------------------------- |
| [Close](./Close.md)                                   | –       | Close a specific position or order by ticket.                 |
| [Close-all](./Close-all.md)                           | flatten | Close **all** positions (optionally filtered by symbol).      |
| [Close-symbol](./Close-symbol.md)                     | cs      | Close all positions for a given symbol.                       |
| [Ensure\_Symbol\_Visible](./Ensure_Symbol_Visible.md) | en      | Ensure the given symbol is visible in Market Watch.           |
| [Limits](./Limits.md)                                 | lim     | Show min/step/max volume for a symbol.                        |
| [Panic](./Panic.md)                                   | –       | Emergency: close all positions and cancel all pending orders. |
| [Pending.modify](./Pending.modify.md)                 | pm      | Modify parameters of an existing pending order.               |
| [Pending.move](./Pending.move.md)                     | pmove   | Move a pending order by a given number of points.             |
| [Quote](./Quote.md)                                   | q       | Get a snapshot price (Bid/Ask/Time).                          |
| [Reverse](./Reverse.md)                               | rv      | Reverse position(s) for a given symbol.                       |
| [Symbol](./Symbol.md)                                 | sh      | Show detailed information about a trading symbol.             |

---

## Typical Use Cases 🛠️

* **Pre‑trade checks:** ensure symbol visibility (`ensure`), inspect trading limits (`limits`).
* **Market monitoring:** fetch a quick snapshot price (`quote`).
* **Order management:** close, reverse, or panic‑exit positions.
* **Pending orders:** adjust or move pending orders dynamically.

---

## Shortcuts ⌨️

These commands are also available via PowerShell shortcuts (see [shortcuts.ps1](../shortcuts.md)):

* `q` → `quote`
* `en` → `symbol ensure`
* `lim` → `symbol limits`
* `sh` → `symbol show`
* `rv` → `reverse`
* `cs` → `close-symbol`
* `flatten` → `close-all`
