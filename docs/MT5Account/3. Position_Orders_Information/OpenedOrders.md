# ✅ Getting All Opened Orders & Positions

> **Request:** detailed information about all opened orders and positions from **MT5**. Get complete data for active orders and positions with sorting options.

**API Information:**

* **SDK wrapper:** `MT5Account.OpenedOrdersAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.AccountHelper`
* **Proto definition:** `OpenedOrders` (defined in `mt5-term-api-account-helper.proto`)

### RPC

* **Service:** `mt5_term_api.AccountHelper`
* **Method:** `OpenedOrders(OpenedOrdersRequest) → OpenedOrdersReply`
* **Low‑level client (generated):** `AccountHelper.OpenedOrders(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<OpenedOrdersData> OpenedOrdersAsync(
            BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`OpenedOrdersRequest { inputSortMode: BMT5_ENUM_OPENED_ORDER_SORT_TYPE }`


**Reply message:**

`OpenedOrdersReply { data: OpenedOrdersData }` or `{ error: Error }`

---

## 🔽 Input

| Parameter           | Type                                  | Required | Description                                                     |
| ------------------- | ------------------------------------- | -------- | --------------------------------------------------------------- |
| `sortMode`          | `BMT5_ENUM_OPENED_ORDER_SORT_TYPE`    | ❌       | Sort order for results (default: by open time ascending)       |
| `deadline`          | `DateTime?`                           | ❌       | Absolute per‑call **UTC** deadline → converted to timeout      |
| `cancellationToken` | `CancellationToken`                   | ❌       | Cooperative cancel for the call/retry loop                     |

---

## ⬆️ Output — `OpenedOrdersData`

| Field            | Type                        | Description                                        |
| ---------------- | --------------------------- | -------------------------------------------------- |
| `OpenedOrders`   | `List<OpenedOrderInfo>`     | List of all active pending orders                  |
| `PositionInfos`  | `List<PositionInfo>`        | List of all currently open positions               |

---

## 🧱 Related types and enums (from proto)

### `BMT5_ENUM_OPENED_ORDER_SORT_TYPE` (Sort Options)

| Enum Value                                   | Value | Description                           |
| -------------------------------------------- | ----- | ------------------------------------- |
| `Bmt5OpenedOrderSortByOpenTimeAsc`    | 0     | Sort by open time (oldest first)      |
| `Bmt5OpenedOrderSortByOpenTimeDesc`   | 1     | Sort by open time (newest first)      |
| `Bmt5OpenedOrderSortByOrderTicketIdAsc` | 2  | Sort by ticket ID (ascending)         |
| `Bmt5OpenedOrderSortByOrderTicketIdDesc` | 3 | Sort by ticket ID (descending)        |

### `OpenedOrderInfo` (Pending Orders)

| Field            | Type                          | Description                                                   |
| ---------------- | ----------------------------- | ------------------------------------------------------------- |
| `Index`          | `uint32`                      | Zero-based index in the list                                  |
| `Ticket`         | `ulong`                       | Order ticket ID (unique identifier)                           |
| `PriceCurrent`   | `double`                      | Current market price for the symbol                           |
| `PriceOpen`      | `double`                      | Order price (trigger price for pending orders)                |
| `StopLimit`      | `double`                      | StopLimit price (for STOP_LIMIT orders)                       |
| `StopLoss`       | `double`                      | Stop Loss price                                               |
| `TakeProfit`     | `double`                      | Take Profit price                                             |
| `VolumeCurrent`  | `double`                      | Current unfilled volume (lots)                                |
| `VolumeInitial`  | `double`                      | Initial order volume (lots)                                   |
| `MagicNumber`    | `long`                        | Magic number (EA identifier)                                  |
| `Reason`         | `int32`                       | Order placement reason (enum value)                           |
| `Type`           | `BMT5_ENUM_ORDER_TYPE`        | Order type (Buy, Sell, BuyLimit, etc.)                        |
| `State`          | `BMT5_ENUM_ORDER_STATE`       | Order state (Placed, Partial, etc.)                           |
| `TimeExpiration` | `Timestamp`                   | Order expiration time                                         |
| `TimeSetup`      | `Timestamp`                   | Order setup time (when placed)                                |
| `TimeDone`       | `Timestamp`                   | Order completion time (if filled/cancelled)                   |
| `TypeFilling`    | `BMT5_ENUM_ORDER_TYPE_FILLING`| Filling mode (FOK, IOC, Return, BOC)                          |
| `TypeTime`       | `BMT5_ENUM_ORDER_TYPE_TIME`   | Order lifetime (GTC, DAY, SPECIFIED, SPECIFIED_DAY)           |
| `PositionId`     | `long`                        | Position ID this order belongs to                             |
| `PositionById`   | `long`                        | Position ID for CloseBy operations                            |
| `Symbol`         | `string`                      | Trading symbol                                                |
| `ExternalId`     | `string`                      | External order identifier                                     |
| `Comment`        | `string`                      | Order comment                                                 |
| `AccountLogin`   | `long`                        | Account login number                                          |

### `PositionInfo` (Open Positions)

| Field                | Type                        | Description                                        |
| -------------------- | --------------------------- | -------------------------------------------------- |
| `Index`              | `uint32`                    | Zero-based index in the list                       |
| `Ticket`             | `ulong`                     | Position ticket ID                                 |
| `OpenTime`           | `Timestamp`                 | Position open time                                 |
| `Volume`             | `double`                    | Position volume (lots)                             |
| `PriceOpen`          | `double`                    | Position open price                                |
| `StopLoss`           | `double`                    | Stop Loss price                                    |
| `TakeProfit`         | `double`                    | Take Profit price                                  |
| `PriceCurrent`       | `double`                    | Current market price                               |
| `Swap`               | `double`                    | Accumulated swap                                   |
| `Profit`             | `double`                    | Current profit/loss                                |
| `LastUpdateTime`     | `Timestamp`                 | Last position update time                          |
| `Type`               | `BMT5_ENUM_POSITION_TYPE`   | Position type (Buy or Sell)                        |
| `MagicNumber`        | `long`                      | Magic number (EA identifier)                       |
| `Identifier`         | `long`                      | Position identifier                                |
| `Reason`             | `BMT5_ENUM_POSITION_REASON` | Position open reason (Client, Mobile, Web, Expert) |
| `Symbol`             | `string`                    | Trading symbol                                     |
| `Comment`            | `string`                    | Position comment                                   |
| `ExternalId`         | `string`                    | External position identifier                       |
| `PositionCommission` | `double`                    | Commission charged for position                    |
| `AccountLogin`       | `long`                      | Account login number                               |

### `BMT5_ENUM_ORDER_TYPE`

| Enum Value                   | Value | Description                                                        |
| ---------------------------- | ----- | ------------------------------------------------------------------ |
| `Bmt5OrderTypeBuy`        | 0     | Market Buy order                                                   |
| `Bmt5OrderTypeSell`       | 1     | Market Sell order                                                  |
| `Bmt5OrderTypeBuyLimit`  | 2     | Buy Limit pending order                                            |
| `Bmt5OrderTypeSellLimit` | 3     | Sell Limit pending order                                           |
| `Bmt5OrderTypeBuyStop`   | 4     | Buy Stop pending order                                             |
| `Bmt5OrderTypeSellStop`  | 5     | Sell Stop pending order                                            |
| `Bmt5OrderTypeBuyStopLimit` | 6 | Upon reaching order price, Buy Limit placed at StopLimit price     |
| `Bmt5OrderTypeSellStopLimit` | 7 | Upon reaching order price, Sell Limit placed at StopLimit price   |
| `Bmt5OrderTypeCloseBy`   | 8     | Order to close position by opposite one                            |

### `BMT5_ENUM_ORDER_STATE`

| Enum Value                       | Value | Description                                  |
| -------------------------------- | ----- | -------------------------------------------- |
| `Bmt5OrderStateStarted`       | 0     | Order checked, not yet accepted by broker    |
| `Bmt5OrderStatePlaced`        | 1     | Order accepted                               |
| `Bmt5OrderStateCanceled`      | 2     | Order canceled by client                     |
| `Bmt5OrderStatePartial`       | 3     | Order partially executed                     |
| `Bmt5OrderStateFilled`        | 4     | Order fully executed                         |
| `Bmt5OrderStateRejected`      | 5     | Order rejected                               |
| `Bmt5OrderStateExpired`       | 6     | Order expired                                |
| `Bmt5OrderStateRequestAdd`   | 7     | Order being registered (placing)             |
| `Bmt5OrderStateRequestModify`| 8     | Order being modified                         |
| `Bmt5OrderStateRequestCancel`| 9     | Order being deleted                          |

### `BMT5_ENUM_POSITION_TYPE`

| Enum Value                | Value | Description    |
| ------------------------- | ----- | -------------- |
| `Bmt5PositionTypeBuy`  | 0     | Buy position   |
| `Bmt5PositionTypeSell` | 1     | Sell position  |

### `BMT5_ENUM_POSITION_REASON`

| Enum Value                     | Value | Description                        |
| ------------------------------ | ----- | ---------------------------------- |
| `Bmt5PositionReasonClient`  | 0     | Opened from desktop terminal       |
| `Bmt5PositionReasonMobile`  | 1     | Opened from mobile app             |
| `Bmt5PositionReasonWeb`     | 2     | Opened from web platform           |
| `Bmt5PositionReasonExpert`  | 3     | Opened by EA/script                |
| `OrderReasonSl`              | 4     | Position closed/opened by Stop Loss |
| `OrderReasonTp`              | 5     | Position closed/opened by Take Profit |
| `OrderReasonSo`              | 6     | Position closed/opened by Stop Out (margin call) |

### `BMT5_ENUM_ORDER_TYPE_FILLING`

| Enum Value                    | Value | Description                                                                              |
| ----------------------------- | ----- | ---------------------------------------------------------------------------------------- |
| `Bmt5OrderFillingFok`      | 0     | Fill or Kill - execute full volume or cancel                                             |
| `Bmt5OrderFillingIoc`      | 1     | Immediate or Cancel - execute max available volume, cancel remainder                     |
| `Bmt5OrderFillingReturn`   | 2     | Return - partial fills allowed, remaining volume stays as order                          |
| `Bmt5OrderFillingBoc`      | 3     | Book or Cancel - execute only if full volume available as single offer                   |

### `BMT5_ENUM_ORDER_TYPE_TIME`

| Enum Value                         | Value | Description                                                        |
| ---------------------------------- | ----- | ------------------------------------------------------------------ |
| `Bmt5OrderTimeGtc`              | 0     | Good Till Cancel - valid until explicitly cancelled                |
| `Bmt5OrderTimeDay`              | 1     | Good Till Day - valid until end of trading day                     |
| `Bmt5OrderTimeSpecified`        | 2     | Good Till Specified - valid until expiration time                  |
| `Bmt5OrderTimeSpecifiedDay`    | 3     | Valid till 23:59:59 of specified day                               |

---

## 💬 Just the essentials

* **What it is.** Single RPC returning detailed information about all active orders and positions.
* **Why you need it.** Monitor open positions, check pending orders, calculate total exposure, update UI, risk management.
* **Two result lists.** `OpenedOrders` (pending orders) and `PositionInfos` (open positions).
* **Rich data.** Each object contains 19-24 fields with complete order/position details.
* **Sorting options.** 4 sort modes - by time or ticket ID, ascending or descending.

---

## 🎯 Purpose

Use this method when you need to:

* Get complete snapshot of all trading activity on the account.
* Display current positions and pending orders in UI.
* Calculate total exposure across all positions.
* Monitor profit/loss for all open positions.
* Check for specific pending orders before placing new ones.
* Implement risk management based on current positions.
* Update trading dashboard with real-time data.

---

## 🧩 Notes & Tips

* Returns **two separate lists** - pending orders and open positions.
* `OpenedOrders` contains only **pending orders** (not yet executed).
* `PositionInfos` contains only **open positions** (already executed).
* Use `PositionsTotalAsync()` for quick count check before calling this method.
* Timestamp fields are `google.protobuf.Timestamp` - convert using `DateTimeOffset.FromUnixTimeSeconds()`.
* `MagicNumber` = 0 means manually opened (not by EA).
* For **netting accounts**: one position per symbol (buy or sell).
* For **hedging accounts**: multiple positions per symbol allowed (buy + sell).
* Use short timeout (5-10s) depending on number of positions.
* Empty lists mean no pending orders or open positions.
* `PriceCurrent` shows current market price for comparison with entry/target prices.
* `Profit` in PositionInfo is **unrealized P&L** (not yet closed).

---

## 🔗 Usage Examples

### 1) Get all positions and orders

```csharp
// Retrieve complete trading state
var data = await acc.OpenedOrdersAsync(
    sortMode: BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
    deadline: DateTime.UtcNow.AddSeconds(10));

Console.WriteLine($"Open Positions:  {data.PositionInfos.Count}");
Console.WriteLine($"Pending Orders:  {data.OpenedOrders.Count}");
```

### 2) Display all open positions

```csharp
// Show position details
var data = await acc.OpenedOrdersAsync();

Console.WriteLine("=== OPEN POSITIONS ===");
Console.WriteLine("Ticket      Symbol      Type  Volume   Entry      Current    Profit");
Console.WriteLine("───────────────────────────────────────────────────────────────────");

foreach (var pos in data.PositionInfos)
{
    var type = pos.Type == BMT5_ENUM_POSITION_TYPE.Bmt5PositionTypeBuy ? "BUY " : "SELL";
    var openTime = DateTimeOffset.FromUnixTimeSeconds(pos.OpenTime.Seconds).DateTime;

    Console.WriteLine($"{pos.Ticket,-10}  {pos.Symbol,-10}  {type}  {pos.Volume,6:F2}  {pos.PriceOpen,8:F5}  {pos.PriceCurrent,8:F5}  {pos.Profit,8:F2}");
}
```

### 3) Calculate total profit/loss

```csharp
// Sum unrealized P&L across all positions
var data = await acc.OpenedOrdersAsync();

double totalProfit = data.PositionInfos.Sum(p => p.Profit);
double totalSwap = data.PositionInfos.Sum(p => p.Swap);
double totalCommission = data.PositionInfos.Sum(p => p.PositionCommission);

double netPL = totalProfit + totalSwap - totalCommission;

Console.WriteLine($"Total Profit:     {totalProfit:F2}");
Console.WriteLine($"Total Swap:       {totalSwap:F2}");
Console.WriteLine($"Total Commission: {totalCommission:F2}");
Console.WriteLine($"Net P&L:          {netPL:F2}");
```

### 4) List pending orders

```csharp
// Show all pending orders
var data = await acc.OpenedOrdersAsync(
    sortMode: BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOrderTicketIdDesc);

Console.WriteLine("=== PENDING ORDERS ===");
Console.WriteLine("Ticket      Symbol      Type           Price      Volume   State");
Console.WriteLine("───────────────────────────────────────────────────────────────────");

foreach (var order in data.OpenedOrders)
{
    var typeName = order.Type.ToString().Replace("BMT5_ORDER_TYPE_", "");
    var stateName = order.State.ToString().Replace("BMT5_ORDER_STATE_", "");

    Console.WriteLine($"{order.Ticket,-10}  {order.Symbol,-10}  {typeName,-12}  {order.PriceOpen,8:F5}  {order.VolumeInitial,6:F2}  {stateName}");
}
```

### 5) Check positions for specific EA

```csharp
// Filter positions by magic number
var data = await acc.OpenedOrdersAsync();
var eaMagic = 123456;

var eaPositions = data.PositionInfos
    .Where(p => p.MagicNumber == eaMagic)
    .ToList();

Console.WriteLine($"Positions for EA {eaMagic}: {eaPositions.Count}");

foreach (var pos in eaPositions)
{
    Console.WriteLine($"  {pos.Symbol} - Ticket: {pos.Ticket}, Profit: {pos.Profit:F2}");
}
```

### 6) Monitor positions by symbol

```csharp
// Group positions by symbol
var data = await acc.OpenedOrdersAsync();

var bySymbol = data.PositionInfos
    .GroupBy(p => p.Symbol)
    .OrderByDescending(g => g.Sum(p => Math.Abs(p.Volume)));

Console.WriteLine("=== EXPOSURE BY SYMBOL ===");
Console.WriteLine("Symbol      Positions  Total Volume   Net P&L");
Console.WriteLine("────────────────────────────────────────────────");

foreach (var group in bySymbol)
{
    var count = group.Count();
    var totalVolume = group.Sum(p => p.Volume);
    var totalPL = group.Sum(p => p.Profit);

    Console.WriteLine($"{group.Key,-10}  {count,8}  {totalVolume,12:F2}  {totalPL,10:F2}");
}
```

### 7) Check order expiration

```csharp
// Find orders expiring soon
var data = await acc.OpenedOrdersAsync();
var warningMinutes = 30;

Console.WriteLine($"Orders expiring within {warningMinutes} minutes:");

foreach (var order in data.OpenedOrders)
{
    if (order.TimeExpiration != null && order.TimeExpiration.Seconds > 0)
    {
        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(order.TimeExpiration.Seconds).DateTime;
        var timeLeft = expirationTime - DateTime.UtcNow;

        if (timeLeft.TotalMinutes > 0 && timeLeft.TotalMinutes <= warningMinutes)
        {
            Console.WriteLine($"  Ticket {order.Ticket}: {order.Symbol} expires in {timeLeft.TotalMinutes:F0} min");
        }
    }
}
```

### 8) Sort by newest positions first

```csharp
// Get most recent positions
var data = await acc.OpenedOrdersAsync(
    sortMode: BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeDesc);

Console.WriteLine("=== LATEST POSITIONS (newest first) ===");

foreach (var pos in data.PositionInfos.Take(5))
{
    var openTime = DateTimeOffset.FromUnixTimeSeconds(pos.OpenTime.Seconds).DateTime;
    var type = pos.Type == BMT5_ENUM_POSITION_TYPE.Bmt5PositionTypeBuy ? "BUY" : "SELL";

    Console.WriteLine($"{openTime:yyyy-MM-dd HH:mm:ss} - {pos.Symbol} {type} {pos.Volume:F2} @ {pos.PriceOpen:F5}");
}
```
