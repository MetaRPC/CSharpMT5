# Streaming: OnTradeAsync

> **Request:** real-time trade events from MT5

Subscribe to a live stream of trade events (orders, deals, positions).

### Code Example

```csharp
await foreach (var tradeEvent in _mt5Account.OnTradeAsync(cts.Token))
{
    _logger.LogInformation(
        "OnTradeAsync: TradeEvent={@TradeEvent}",
        tradeEvent);
}
```

✨**Method Signature:**
```csharp
IAsyncEnumerable<OnTradeData> OnTradeAsync(
    CancellationToken cancellationToken = default
)
```

* **Input:**
    * **cancellationToken** (`CancellationToken`) — token to cancel the streaming.

* **Output:**
    * async stream of **OnTradeData** objects, each representing a trade event (order/deal/position update).

**Purpose:** React to all server-side trade events in real time, enabling immediate handling of executed orders, position changes, and deal confirmations. 🚀
