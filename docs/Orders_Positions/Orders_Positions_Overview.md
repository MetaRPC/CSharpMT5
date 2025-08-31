# Orders & Positions Overview ⚖️

This section documents all commands for **trading operations** — placing, modifying, closing, reversing, and managing orders/positions.

---

## 📌 Covered Commands

| Command                  | Alias     | Purpose                                                |
| ------------------------ | --------- | ------------------------------------------------------ |
| `buy`                    | `b`       | Place market buy order.                                |
| `sell`                   | `s`       | Place market sell order.                               |
| `close`                  | `c`       | Close a specific position/order by ticket.             |
| `close-all`              | `flatten` | Close all positions/orders (with optional filters).    |
| `close-symbol`           | `cs`      | Close all positions/orders for a specific symbol.      |
| `close.partial`          | `cp`      | Close part of a position by exact volume.              |
| `close.percent`          | `cpp`     | Close part of a position by percentage.                |
| `close.half`             | `ch`      | Special shortcut: close half of a position.            |
| `closeby`                | —         | Close position A using opposite position B (emulated). |
| `modify`                 | `m`       | Modify SL/TP for a position by ticket.                 |
| `position.modify`        | `posmod`  | Modify SL/TP by exact price.                           |
| `position.modify.points` | `pmp`     | Modify SL/TP using point distance (from entry/market). |
| `orders`                 | `ord`     | List open order & position tickets.                    |
| `positions`              | `pos`     | List active positions with details.                    |
| `pending`                | `pd`      | Utilities for pending orders (list, manage).           |
| `place`                  | `pl`      | Place pending order (limit/stop/stop-limit).           |
| `cancel`                 | `x`       | Cancel (delete) a pending order by ticket.             |
| `cancel.all`             | `ca`      | Cancel all pending orders (with optional filters).     |
| `reverse`                | `rv`      | Reverse all positions for a symbol.                    |
| `reverse.ticket`         | `rvt`     | Reverse a specific position by ticket.                 |
| `breakeven`              | `be`      | Move SL to entry (with optional offset).               |
| `trail.start`            | —         | Start local trailing stop for a position.              |
| `trail.stop`             | —         | Stop local trailing stop for a position.               |

---

## 🎯 Use Cases

* **Scalping / day-trading** → Fast market `buy` / `sell` with optional SL/TP.
* **Risk management** → `modify`, `position.modify`, `breakeven`, `trail.start`.
* **Portfolio actions** → `close-all`, `flatten`, `reverse`.
* **Pending strategies** → `place`, `pending`, `cancel.all`.
* **Position tuning** → `close.partial`, `close.percent`, `close.half`.

---

## 🔗 Related Sections

* [Account](../Account/Account_Overview.md) → for profiles, info, account state.
* [History](../History/History_Overview.md) → for historical orders/trades.
* [Market Data](../Market_Data/Market_Data_Overview.md) → for quotes, symbol settings.
* [Misc](../Misc/Misc_Overview.md) → for ticket utilities and extra tools.

---

This overview serves as a **map of all trading actions**. Each command has its own dedicated doc with usage, parameters, and examples.
