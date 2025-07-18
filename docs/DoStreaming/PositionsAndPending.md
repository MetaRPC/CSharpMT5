# Streaming: `OnPositionsAndPendingOrdersTicketsAsync`

> **Request:** real-time snapshots of open positions and pending order ticket IDs from MT5
> Subscribe to a live stream that provides, at a regular interval, the current lists of ticket IDs for both open positions and pending orders.

---

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

---

### Method Signature

```csharp
IAsyncEnumerable<OnPositionsAndPendingOrdersTicketsData> OnPositionsAndPendingOrdersTicketsAsync(
    int intervalMs,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

| Parameter           | Type                | Description                      |
| ------------------- | ------------------- | -------------------------------- |
| `intervalMs`        | `int`               | Polling interval in milliseconds |
| `cancellationToken` | `CancellationToken` | Token to cancel the stream       |

---

## ⬆️ Output

Returns a stream of **OnPositionsAndPendingOrdersTicketsData** items:

| Field       | Type                   | Description                                                       |
| ----------- | ---------------------- | ----------------------------------------------------------------- |
| `TicketIds` | `IReadOnlyList<ulong>` | Combined list of ticket IDs for open positions and pending orders |

> ℹ️ If your implementation separates ticket types (e.g. `PositionTicketIds`, `PendingOrderTicketIds`), list each explicitly.

---

## 🎯 Purpose

This stream enables **continuous monitoring** of your MT5 account’s positions and orders by providing updated ticket lists on a set interval. Ideal for:

* Tracking lifecycle of orders in real time
* Keeping client-side caches in sync
* Reducing manual polling and improving efficiency
* Automatically reacting to changes in positions or pending orders 🚀
