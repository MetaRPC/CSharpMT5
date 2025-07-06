# Streaming: OnPositionProfitAsync

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

✨**Method Signature:**
```csharp
IAsyncEnumerable<OnPositionProfitData> OnPositionProfitAsync(
    int intervalMs,
    bool ignoreEmpty = true,
    CancellationToken cancellationToken = default
)
```
* **Input:**
    * **intervalMs** (`int`) — polling interval in milliseconds.
    * **ignoreEmpty** (`bool`) — skip updates with no change if `true`.
    * **cancellationToken** (`CancellationToken`) — token to cancel the stream.

* **Output:**
    * async stream of **OnPositionProfitData** objects, each containing profit update details (e.g., `Profit`).

**Purpose:** Monitor position profitability in real time, allowing automated alerts or risk controls when thresholds are crossed. 🚀

