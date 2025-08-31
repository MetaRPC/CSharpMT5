# Ticket Toolbox 🎟️

## Purpose

Work with a **specific ticket**: inspect, reverse, or partially close.
These commands rely on ticket IDs (`ulong`) from MT5 and respect our validation rules (`Validators.EnsureTicket`).

---

## Available Commands 🛠️

### 🔍 Inspect

* **`ticket show`**
  Show info for a specific ticket.
  If ticket is open → full position/order details.
  If ticket is closed → recent history lookup.

### ↔️ Reverse

* **`reverse.ticket`**
  Reverse a single position by ticket (open opposite with same volume).
  Useful for flipping only **one** position instead of whole net exposure.

### ✂️ Partial closes

* **`close` / `close.partial` / `close.percent` / `close.half`**
  All these support working **by ticket**.

  * `close` — full close (or with specified volume).
  * `close.partial` — close exact lot amount.
  * `close.percent` — close percentage.
  * `close.half` — shortcut for 50%.

---

## Implementation Notes ⚙️

* **Validation:**
  All commands call `Validators.EnsureTicket(ticket)` to ensure `>0`.

* **Lookup:**
  Utility `TryFindByTicketInAggregate(...)` is used to locate an order/position inside returned aggregates (`OpenedOrdersAsync`, `OpenedOrdersTicketsAsync`).

* **Execution:**

  * `CloseOrderByTicketAsync` — the low-level gRPC API used by `close*` commands.
  * `ModifyPositionSlTpAsync` — for SL/TP changes.
  * `CloseByEmulatedAsync` — for `closeby` logic.

* **Dry-run:**
  Every ticket command supports `--dry-run` to preview without sending.

---

## Shortcuts ⚡

| Command          | Alias  |
| ---------------- | ------ |
| `ticket show`    | `t sh` |
| `reverse.ticket` | `rvt`  |
| `close.partial`  | `cp`   |
| `close.percent`  | `cpp`  |
| `close.half`     | `ch`   |
