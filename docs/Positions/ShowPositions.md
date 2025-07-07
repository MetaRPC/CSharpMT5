# Positions

## Getting Total Open Positions

> **Request:** total number of open positions on the account.

### Code Example

```csharp
var total = await _mt5Account.PositionsTotalAsync();
_logger.LogInformation("PositionsTotalAsync: Total={Total}", total.Total);
```

✨ **Method Signature:**

```csharp
Task<PositionsTotalResponse> PositionsTotalAsync()
```

---

### Input

*None* — this method takes no parameters.

### Output

**`PositionsTotalResponse`** — structure with:

* **`Total`** (`int`) — total number of currently open positions.

**Purpose:**
Quickly determine how many positions are currently open without fetching full position details. 🚀

---

## Getting Positions History

> **Request:** list of historical open/closed positions for a given period, sorted by open time (or other criteria).

### Code Example

```csharp
var history = await _mt5Account.PositionsHistoryAsync(
    AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeAsc,
    DateTime.UtcNow.AddDays(-30),
    DateTime.UtcNow
);
_logger.LogInformation("PositionsHistoryAsync: Count={Count}", history.HistoryPositions.Count);
```

✨ **Method Signature:**

```csharp
Task<PositionsHistoryResponse> PositionsHistoryAsync(
    AH_ENUM_POSITIONS_HISTORY_SORT_TYPE sortType,
    DateTime from,
    DateTime to
)
```

---

### Input

* **`sortType`** (`AH_ENUM_POSITIONS_HISTORY_SORT_TYPE`) — sort order for returned positions. Possible values:

  * **AhPositionOpenTimeAsc** — sort by open time ascending.
  * **AhPositionOpenTimeDesc** — sort by open time descending.
  * **AhPositionCloseTimeAsc** — sort by close time ascending.
  * **AhPositionCloseTimeDesc** — sort by close time descending.

* **`from`** (`DateTime`) — start of the period (UTC).

* **`to`** (`DateTime`) — end of the period (UTC).

### Output

**`PositionsHistoryResponse`** — structure with:

* **`HistoryPositions`** (`IReadOnlyList<PositionInfo>`) — list of positions within the specified period.

### `PositionInfo` Structure

Each item in `HistoryPositions` contains:

* **`Ticket`** (`ulong`) — unique identifier of the position.
* **`Symbol`** (`string`) — trading symbol (e.g., "EURUSD").
* **`Type`** (`AH_ENUM_POSITION_TYPE`) — position type. Possible values:

  * **PositionTypeBuy** — long position.
  * **PositionTypeSell** — short position.
* **`Volume`** (`double`) — volume in lots.
* **`PriceOpen`** (`double`) — price at which the position was opened.
* **`PriceCurrent`** (`double`) — current market price for the symbol.
* **`Swap`** (`double`) — accumulated swap/rollover charges.
* **`Profit`** (`double`) — floating profit or loss of the position in deposit currency.
* **`Commission`** (`double`) — commissions charged for the position.
* **`OpenTime`** (`DateTime`) — UTC timestamp when the position was opened.
* **`CloseTime`** (`DateTime`) — UTC timestamp when the position was closed (for closed positions history).

**Purpose:**
Provides full position details over a given period with sorting, enabling in-depth analysis, reporting, and auditing of trading activity. 🚀
