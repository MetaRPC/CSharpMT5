# Market Data Overview 📊

This section contains commands related to **symbols, quotes, and trading utilities**. These commands allow you to:

* Fetch quotes and reverse trades.
* Manage open and pending orders.
* Ensure symbols are available for trading.
* Inspect trading limits and panic-close positions.

---

| Command                                  | Alias(es)              | Description                                              |
| ---------------------------------------- | ---------------------- | -------------------------------------------------------- |
| **[close](./Close.md)**                  | `c`                    | Close a specific position or order by ticket.            |
| **[close-all](./Close-all.md)**          | `flatten`, `close.all` | Close **all** positions (optionally filtered by symbol). |
| **[close-symbol](./Close-symbol.md)**    | `cs`, `flatten-symbol` | Close all positions for a given symbol.                  |
| **[sym ensure-visible](./Ensure_Symbol_Visible.md)** | –            | Ensure the given symbol is visible in Market Watch.      |
| **[sym limits](./Limits.md)**            | `lim`                  | Show min/step/max volume for a symbol.                   |
| **[panic](./Panic.md)**                  | –                      | Emergency: close all positions and cancel all pendings.  |
| **[pending.modify](./Pending.modify.md)**| `pm`                   | Modify parameters of an existing pending order.          |
| **[pending.move](./Pending.move.md)**    | `pmove`                | Move a pending order by a given number of points.        |
| **[quote](./Quote.md)**                  | `q`                    | Get a snapshot price (Bid/Ask/Time).                     |
| **[reverse](./Reverse.md)**              | `rv`                   | Reverse position(s) for a given symbol.                  |
| **[sym show](../Account/Show.md)**       | –                      | Show a compact symbol card: quote + volume limits.       |

---

## Typical Use Cases 🛠️

* **Pre-trade checks:** ensure symbol visibility (**[ensure-visible](./Ensure_Symbol_Visible.md)**), inspect trading limits (**[limits](./Limits.md)**).
* **Market monitoring:** fetch a quick snapshot price (**[quote](./Quote.md)**).
* **Order management:** close (**[close](./Close.md)**), close all (**[close-all](./Close-all.md)**), reverse (**[reverse](./Reverse.md)**), panic (**[panic](./Panic.md)**).
* **Pending orders:** edit (**[pending.modify](./Pending.modify.md)**) or move (**[pending.move](./Pending.move.md)**) pending orders dynamically.

---
