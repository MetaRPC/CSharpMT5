# Market Data Overview 📊

This section contains commands related to **symbols, quotes, and trading utilities**. These commands allow you to:

* Fetch quotes and reverse trades.
* Manage open and pending orders.
* Ensure symbols are available for trading.
* Inspect trading limits and panic‑close positions.

---

| Command                         | Alias(es)              | Description                                              |
| ------------------------------- | ---------------------- | -------------------------------------------------------- |
| **close**                       | `c`                    | Close a specific position or order by ticket.            |
| **close-all**                   | `flatten`, `close.all` | Close **all** positions (optionally filtered by symbol). |
| **close-symbol**                | `cs`, `flatten-symbol` | Close all positions for a given symbol.                  |
| **sym ensure-visible** (helper) | –                      | Ensure the given symbol is visible in Market Watch.      |
| **sym limits**                  | `lim`                  | Show min/step/max volume for a symbol.                   |
| **panic**                       | –                      | Emergency: close all positions and cancel all pendings.  |
| **pending.modify**              | `pm`                   | Modify parameters of an existing pending order.          |
| **pending.move**                | `pmove`                | Move a pending order by a given number of points.        |
| **quote**                       | `q`                    | Get a snapshot price (Bid/Ask/Time).                     |
| **reverse**                     | `rv`                   | Reverse position(s) for a given symbol.                  |
| **sym show**                    | –                      | Show a compact symbol card: quote + volume limits.       |

---

## Typical Use Cases 🛠️

* **Pre‑trade checks:** ensure symbol visibility (`ensure`), inspect trading limits (`limits`).
* **Market monitoring:** fetch a quick snapshot price (`quote`).
* **Order management:** close, reverse, or panic‑exit positions.
* **Pending orders:** adjust or move pending orders dynamically.

---
