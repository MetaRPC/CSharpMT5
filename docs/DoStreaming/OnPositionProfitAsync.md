## Streaming: OnPositionProfitAsync1

> **Request:** real-time position profit updates from MT5
> Subscribe to a live stream of profit changes for open positions.

---

### Code Example

```csharp
await foreach (var profitUpdate in _mt5Account.OnPositionProfitAsync(
    1000,         // polling interval in milliseconds
    true,         // ignore empty updates
    cts.Token))
{
    _logger.LogInformation(
        "OnPositionProfitAsync: Profit={Profit}",
        profitUpdate.Data.Profit);
}
```

---

### Method Signature

```csharp
IAsyncEnumerable<OnPositionProfitData> OnPositionProfitAsync(
    int intervalMs,
    bool ignoreEmpty = true,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

| Parameter           | Type                | Description                                        |
| ------------------- | ------------------- | -------------------------------------------------- |
| `intervalMs`        | `int`               | Polling interval in milliseconds                   |
| `ignoreEmpty`       | `bool`              | If `true`, skips updates without meaningful change |
| `cancellationToken` | `CancellationToken` | Optional token to cancel the stream                |

---

## ⬆️ Output

Returns a stream of **OnPositionProfitData** items:

| Field  | Type                 | Description                                |
| ------ | -------------------- | ------------------------------------------ |
| `Data` | `PositionProfitData` | Structure with position profit information |

### `PositionProfitData` Structure

| Field    | Type     | Description                                  |
| -------- | -------- | -------------------------------------------- |
| `Ticket` | `ulong`  | Unique identifier of the open position       |
| `Profit` | `double` | Current profit or loss (in deposit currency) |

---

## 🎯 Purpose

This method allows your app to **track position profitability in real time**, which is ideal for:

* Monitoring profit/loss dynamics continuously
* Triggering auto-close or notifications based on thresholds
* Providing users with live PnL updates in dashboards
