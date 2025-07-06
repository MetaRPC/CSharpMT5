# Streaming: OnPositionsAndPendingOrdersTicketsAsync

> **Request:** real-time snapshots of open positions and pending order ticket IDs

Subscribe to a live stream of ticket lists for both positions and pending orders at a given interval.

### Code Example

```csharp
await foreach (var snapshot in _mt5Account.OnPositionsAndPendingOrdersTicketsAsync(
    1000,      // polling interval in milliseconds
    cts.Token  // cancellation token
))
{
    _logger.LogInformation(
        "OnPositionsAndPendingOrdersTicketsAsync: Tickets={Tickets}",
        string.Join(", ", snapshot.TicketIds) // replace `TicketIds` with the actual property name
    );
}
```

✨**Method Signature:**
```csharp
IAsyncEnumerable<OnPositionsAndPendingOrdersTicketsData> OnPositionsAndPendingOrdersTicketsAsync(
    int intervalMs,
    CancellationToken cancellationToken = default
)
```

 **Input:**
 * **intervalMs** (`int`) — interval in milliseconds between each poll.
 * **cancellationToken** (`CancellationToken`) — token to signal cancellation.

 **Output:**
 * async stream of **OnPositionsAndPendingOrdersTicketsData**, containing a collection of ticket IDs (e.g. `TicketIds` or similar).

**Purpose:** Keep track of your current open positions and pending orders IDs in real time, enabling you to react instantly to changes in your order book. 🚀
