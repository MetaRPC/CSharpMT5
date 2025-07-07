## Streaming: OnPositionProfitAsync

> **Request:** real-time position profit updates from MT5

Subscribe to a live stream of profit changes for open positions.

### Code Example

```csharp
await foreach (var profitUpdate in _mt5Account.OnPositionProfitAsync(
    1000,         // polling interval in milliseconds
    true,         // ignore empty updates
    cts.Token))
{
    _logger.LogInformation(
        "OnPositionProfitAsync: Profit={Profit}",
        profitUpdate.Data.Profit); // replace .Data.Profit with the actual profit property
}
```

✨ **Method Signature:**

```csharp
IAsyncEnumerable<OnPositionProfitData> OnPositionProfitAsync(
    int intervalMs,
    bool ignoreEmpty = true,
    CancellationToken cancellationToken = default
)
```

---

## Input

* **`intervalMs`** (`int`) — polling interval in milliseconds.
* **`ignoreEmpty`** (`bool`, optional, default=`true`) — skip updates with no profit change if `true`.
* **`cancellationToken`** (`CancellationToken`, optional) — token to cancel the streaming operation.

---

## Output

**`IAsyncEnumerable<OnPositionProfitData>`** — asynchronous stream of profit update items:

**`OnPositionProfitData`** — structure with:

* **`Data`** (`PositionProfitData`) — payload object containing:

  * **`Ticket`** (`ulong`) — unique identifier for the position.
  * **`Profit`** (`double`) — current profit or loss for the position (in account currency).
  * *(other fields if provided by the API)*

---

## Purpose

Enable continuous, real-time tracking of position profitability, allowing automated alerts or risk-management actions when profit thresholds are crossed. 🚀
