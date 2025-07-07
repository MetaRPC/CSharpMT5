# Streaming: `OnPositionsAndPendingOrdersTicketsAsync`

> **Request:** real-time snapshots of open positions and pending order ticket IDs from MT5

Subscribe to a live stream that provides, at a regular interval, the current lists of ticket IDs for both open positions and pending orders.

### Code Example

```csharp
await foreach (var snapshot in _mt5Account.OnPositionsAndPendingOrdersTicketsAsync(
    1000,      // polling interval in milliseconds
    cts.Token  // cancellation token
))
{
    _logger.LogInformation(
        "OnPositionsAndPendingOrdersTicketsAsync: Tickets={Tickets}",
        string.Join(", ", snapshot.TicketIds)
    );
}
```

✨ **Method Signature:**

```csharp
IAsyncEnumerable<OnPositionsAndPendingOrdersTicketsData> OnPositionsAndPendingOrdersTicketsAsync(
    int intervalMs,
    CancellationToken cancellationToken = default
)
```

---

## Input

* **`intervalMs`** (`int`) — polling interval in milliseconds between each snapshot.
* **`cancellationToken`** (`CancellationToken`) — token to cancel the streaming when requested.

---

## Output

**`OnPositionsAndPendingOrdersTicketsData`** — structure with:

* **`TicketIds`** (`IReadOnlyList<ulong>`) — combined list of ticket IDs for all currently open positions and pending orders.

> *Note: If your implementation separates position and pending-order tickets into different lists (e.g., `PositionTicketIds` and `PendingOrderTicketIds`), describe each accordingly.*

---

## Purpose

Enables continuous monitoring of your MT5 account’s orders by providing up-to-the-moment ticket lists, so you can automatically react to any changes in positions or pending orders without polling manually. 🚀
