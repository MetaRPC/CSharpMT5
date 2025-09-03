# Ticket Show (`ticket show`) — Internal Flow (Specific\_Ticket.md) 🎫

## What it Does

Technical walkthrough of how **`ticket show`** resolves and prints **a specific ticket**:

1. checks **open sets** (positions/pendings) and, if present, prints a compact card;
2. if not found, **falls back to history** for the last *N* days.

This page is for engineers (under‑the‑hood). For a user‑facing overview, see **Ticket\_Show\.md**.

---
## Method Signatures 

```csharp
public Task<OpenedOrdersTicketsData> OpenedOrdersTicketsAsync(
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public Task<OpenedOrdersData> OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);

public Task<OrdersHistoryData> OrderHistoryAsync(
    DateTime from,
    DateTime to,
    BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sortMode = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
    int pageNumber = 0,
    int itemsPerPage = 0,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default);
```

## Input Parameters ⬇️

| Parameter       | Type   | Required | Description                                                      |
| --------------- | ------ | -------- | ---------------------------------------------------------------- |
| `--profile, -p` | string | yes      | Profile from `profiles.json`.                                    |
| `--ticket, -t`  | ulong  | yes      | Ticket ID to inspect.                                            |
| `--days, -d`    | int    | no       | Look back *N* days in history if not found open (default: `30`). |
| `--output, -o`  | string | no       | `text` (default) or `json`.                                      |
| `--timeout-ms`  | int    | no       | Per‑RPC timeout in milliseconds (default: `30000`).              |

---

## Output ⬆️

**Open (position/pending)** — prints: **Symbol**, **Volume**, **Price** (open), optional **SL/TP**, optional **Profit**, bucket tag (`POSITION`/`PENDING`).

**History** — one of:

* **ORDER history**: `Symbol`, `State`, `VolumeInitial→VolumeCurrent`, `PriceOpen`, `setup/done` timestamps.
* **DEAL history**: `Symbol`, `Type`, `Volume`, `Price`, `Profit`, `time`.

If not found anywhere → `Ticket #<id> not found in open sets or last <days> days.` (exit code `2`). Fatal errors set exit code `1`.

> Fields like `Side`, `ClosePrice`, `Commission`, `Swap` may exist in proto but are **not printed** by the current handler. Extend printing if you need them.

---

## How to Use 🛠️

```powershell
# Inspect ticket in open sets or recent history
dotnet run -- ticket show -p demo -t 123456

# JSON + 7‑day history fallback
dotnet run -- ticket show -p demo -t 123456 -o json -d 7
```

### PowerShell Shortcut

```powershell
. .\ps\shortcasts.ps1
use-pf demo
tsh -t 123456
```

---

## Under‑the‑hood Flow 

1. **Quick membership**: `_mt5Account.OpenedOrdersTicketsAsync()` → detect if the ticket is currently open (orders/positions).
2. **Fetch object**: `_mt5Account.OpenedOrdersAsync()` → find the element inside the aggregate via `TryFindByTicketInAggregate(...)` and print.
3. **History fallback**: `_mt5Account.OrderHistoryAsync(from, to)` → scan `HistoryOrder`/`HistoryDeal` by ticket.

> Note: The project deliberately **does not** have `GetOpenedAggregateAsync`. Use the two calls above.

---

## Code Reference 🧩

```csharp
await ConnectAsync();

// 1) open sets: quick membership
var tickets = await _mt5Account.OpenedOrdersTicketsAsync();
bool isOpenOrder    = tickets.OpenedOrdersTickets.Contains((long)ticket);
bool isOpenPosition = tickets.OpenedPositionTickets.Contains((long)ticket);

// 2) open aggregate: fetch object & print
var opened = await _mt5Account.OpenedOrdersAsync();
var obj = TryFindByTicketInAggregate(opened, ticket, out var bucket);
if (obj != null)
{
    // print JSON/text depending on `output`
}
else
{
    // 3) history fallback
    var from = DateTime.UtcNow.AddDays(-Math.Abs(days));
    var to   = DateTime.UtcNow;
    var hist = await _mt5Account.OrderHistoryAsync(from, to);
    // locate in HistoryOrder/HistoryDeal and print
}
```

---

## Proto Reference (excerpts) 📜
```proto
message OpenedOrdersTicketsData {
  repeated int64 opened_orders_tickets = 1;
  repeated int64 opened_position_tickets = 2;
}
```

```proto
message OpenedOrdersData {
  repeated OpenedOrderInfo opened_orders = 1;
  repeated PositionInfo   position_infos = 2;
}

message OpenedOrderInfo {
  uint32 index = 1;
  uint64 ticket = 2;
  double price_current = 3;
  double price_open = 4;
  double stop_limit = 5;
  double stop_loss = 6;
  double take_profit = 7;
  double volume_current = 8;
  double volume_initial = 9;
  int64  magic_number = 10;
  int32  reason = 11;
  BMT5_ENUM_ORDER_TYPE  type = 12;
  BMT5_ENUM_ORDER_STATE state = 13;
  google.protobuf.Timestamp time_expiration = 14;
  // NOTE: Some builds include `string symbol` here. If absent, derive symbol using other endpoints or mapping.
}

message PositionInfo {
  uint32 index = 1;
  uint64 ticket = 2;
  google.protobuf.Timestamp open_time = 3;
  double volume = 4;
  double price_open = 5;
  double stop_loss = 6;
  double take_profit = 7;
  double price_current = 8;
  double swap = 9;
  double profit = 10;
  google.protobuf.Timestamp last_update_time = 11;
  BMT5_ENUM_POSITION_TYPE type = 12;
  int64  magic_number = 13;
  int64  identifier = 14;
  BMT5_ENUM_POSITION_REASON reason = 15;
  string symbol = 16;
  string comment = 17;
  string external_id = 18;
  double position_commission = 19;
  int64  account_login = 20;
}
```

```proto
message OrdersHistoryData {
  int32 arrayTotal   = 1;
  int32 pageNumber   = 2;
  int32 itemsPerPage = 3;
  repeated HistoryData history_data = 4;
}

message HistoryData {
  uint32 index = 1;
  OrderHistoryData history_order = 2;
  DealHistoryData  history_deal  = 3;
}

message OrderHistoryData {
  uint64 ticket = 1;
  google.protobuf.Timestamp setup_time = 2;
  google.protobuf.Timestamp done_time  = 3;
  BMT5_ENUM_ORDER_STATE state = 4;
  double price_current = 5;
  double price_open    = 6;
  double stop_limit    = 7;
  double stop_loss     = 8;
  double take_profit   = 9;
  double volume_current = 10;
  double volume_initial = 11;
  int64  magic_number   = 12;
  BMT5_ENUM_ORDER_TYPE type = 13;
  google.protobuf.Timestamp time_expiration = 14;
}
```

```proto
enum BMT5_ENUM_OPENED_ORDER_SORT_TYPE {
  BMT5_OPENED_ORDER_SORT_BY_OPEN_TIME_ASC  = 0;
  BMT5_OPENED_ORDER_SORT_BY_OPEN_TIME_DESC = 1;
  BMT5_OPENED_ORDER_SORT_BY_ORDER_TICKET_ID_ASC  = 2;
  BMT5_OPENED_ORDER_SORT_BY_ORDER_TICKET_ID_DESC = 3;
}

enum BMT5_ENUM_ORDER_HISTORY_SORT_TYPE {
  BMT5_SORT_BY_OPEN_TIME_ASC   = 0;
  BMT5_SORT_BY_OPEN_TIME_DESC  = 1;
  BMT5_SORT_BY_CLOSE_TIME_ASC  = 2;
  BMT5_SORT_BY_CLOSE_TIME_DESC = 3;
  BMT5_SORT_BY_ORDER_TICKET_ID_ASC = 4; // ...
}

enum BMT5_ENUM_POSITION_TYPE {
  BMT5_POSITION_TYPE_BUY  = 0;
  BMT5_POSITION_TYPE_SELL = 1;
}
```
