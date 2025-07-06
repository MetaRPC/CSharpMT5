## Positions

### Getting total open positions

> **Requesting** the total number of open positions on the account.

#### Code Example

```csharp
var total = await _mt5Account.PositionsTotalAsync();
_logger.LogInformation("PositionsTotalAsync full: {@positions}", total);
```
✨**Method Signature:**
```csharp
 Task<PositionsTotalResponse> PositionsTotalAsync();
```
 **Input:** None.

 **Output:** 
  * **PositionsTotalResponse** — object with property:
    * **Total** (`int`) — total number of open positions.

**Purpose:**
It allows you to quickly find out how many positions are currently open, without downloading detailed data for each one.🚀




# Getting positions history

> **Requesting** the list of historical positions, sorted by open time.

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
Task<PositionsHistoryResponse> PositionsHistoryAsync( AH_ENUM_POSITIONS_HISTORY_SORT_TYPE sortType, DateTime from, DateTime to );
```

 **Input:**
 * **sortType** (`AH_ENUM_POSITIONS_HISTORY_SORT_TYPE`) — sort order (e.g., `AhPositionOpenTimeAsc`).
 * **from** (`DateTime`) — start of the period (UTC).
 * **to** (`DateTime`) — end of the period (UTC).

 **Output:**
  * **PositionsHistoryResponse** — object with property:
* **HistoryPositions** (`IReadOnlyList<PositionInfo>`) — list of positions within the period.

**Purpose:**
Provides a universal method for loading position history for any period with the specified sorting.🚀

# Example Wrapper: ShowPositions

>An example of a method that immediately blocks the total number of positions and their history for the last month.

```csharp
private async Task ShowPositions()
{
    _logger.LogInformation("=== Positions ===");

    var total = await _mt5Account.PositionsTotalAsync();
    _logger.LogInformation("PositionsTotalAsync full: {@positions}", total);

    var history = await _mt5Account.PositionsHistoryAsync(
        AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeAsc,
        DateTime.UtcNow.AddDays(-30),
        DateTime.UtcNow
    );
    _logger.LogInformation("PositionsHistoryAsync: Count={Count}", history.HistoryPositions.Count);
}
```
