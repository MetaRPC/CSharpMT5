# Positions

## Getting Total Open Positions

> **Request:** total number of open positions on the account.

---

### Code Example

```csharp
var total = await _mt5Account.PositionsTotalAsync();
_logger.LogInformation("PositionsTotalAsync: Total={Total}", total.Total);
```

---

### Method Signature

```csharp
Task<PositionsTotalResponse> PositionsTotalAsync()
```

---

## 🔽 Input

*None* — this method takes no parameters.

---

## ⬆️ Output

Returns a **PositionsTotalResponse** object:

| Field   | Type  | Description                               |
| ------- | ----- | ----------------------------------------- |
| `Total` | `int` | Total number of open positions on account |

---

## 🎯 Purpose

Quickly determine how many positions are currently open without fetching full position details.

Ideal for:

* Basic position status display
* Conditional logic (e.g., skip placing order if no positions are open)

---

## Getting Positions History

> **Request:** list of historical open/closed positions for a given period, sorted by open time (or other criteria).

---

### Code Example

```csharp
var history = await _mt5Account.PositionsHistoryAsync(
    AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeAsc,
    DateTime.UtcNow.AddDays(-30),
    DateTime.UtcNow
);
_logger.LogInformation("PositionsHistoryAsync: Count={Count}", history.HistoryPositions.Count);
```

---

### Method Signature

```csharp
Task<PositionsHistoryResponse> PositionsHistoryAsync(
    AH_ENUM_POSITIONS_HISTORY_SORT_TYPE sortType,
    DateTime from,
    DateTime to
)
```

---

## 🔽 Input

| Parameter  | Type                                  | Description                            |
| ---------- | ------------------------------------- | -------------------------------------- |
| `sortType` | `AH_ENUM_POSITIONS_HISTORY_SORT_TYPE` | Sorting strategy for results           |
| `from`     | `DateTime`                            | UTC start time of the history interval |
| `to`       | `DateTime`                            | UTC end time of the history interval   |

### Sort Type Enum Values

| Value                     | Description                   |
| ------------------------- | ----------------------------- |
| `AhPositionOpenTimeAsc`   | Sort by open time ascending   |
| `AhPositionOpenTimeDesc`  | Sort by open time descending  |
| `AhPositionCloseTimeAsc`  | Sort by close time ascending  |
| `AhPositionCloseTimeDesc` | Sort by close time descending |

---

## ⬆️ Output

Returns a **PositionsHistoryResponse** object:

| Field              | Type                          | Description                      |
| ------------------ | ----------------------------- | -------------------------------- |
| `HistoryPositions` | `IReadOnlyList<PositionInfo>` | List of historical position data |

### `PositionInfo` Structure

| Field          | Type                    | Description                               |
| -------------- | ----------------------- | ----------------------------------------- |
| `Ticket`       | `ulong`                 | Unique ID of the position                 |
| `Symbol`       | `string`                | Trading symbol                            |
| `Type`         | `AH_ENUM_POSITION_TYPE` | Position type (Buy/Sell)                  |
| `Volume`       | `double`                | Position volume in lots                   |
| `PriceOpen`    | `double`                | Entry price                               |
| `PriceCurrent` | `double`                | Current price                             |
| `Swap`         | `double`                | Accrued swap charges                      |
| `Profit`       | `double`                | Floating or realized profit/loss          |
| `Commission`   | `double`                | Commissions charged                       |
| `OpenTime`     | `DateTime`              | UTC timestamp when opened                 |
| `CloseTime`    | `DateTime`              | UTC timestamp when closed (if applicable) |

---

## 🎯 Purpose

Use this method to retrieve full **position activity history** within a selected date range.

Perfect for:

* Auditing and reporting
* Generating trade logs
* Analyzing performance over time
