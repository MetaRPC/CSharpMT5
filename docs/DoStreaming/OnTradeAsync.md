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

---

## Input

* **`cancellationToken`** (`CancellationToken`) — token to cancel the streaming.

## Output

* **Async stream of `OnTradeData` objects** — each representing a trade event. The `OnTradeData` structure includes:

  * **`Symbol`** (`string`) — trading symbol associated with the event.
  * **`Type`** (`TradeEventType`) — kind of event (e.g., `OrderAdd`, `OrderUpdate`, `DealAdd`, `DealUpdate`, `PositionAdd`, `PositionUpdate`, `PositionClose`).
  * **`Order`** (`OrderInfo?`) — details of the order involved, if applicable.
  * **`Deal`** (`DealInfo?`) — details of the deal involved, if applicable.
  * **`Position`** (`PositionInfo?`) — details of the position involved, if applicable.
  * **`Timestamp`** (`DateTime`) — UTC time of the event.

### `TradeEventType` Enum Values

* `OrderAdd` — a new order was placed.
* `OrderUpdate` — an existing order was modified.
* `OrderClose` — an order was closed.
* `DealAdd` — a new deal was executed.
* `DealUpdate` — a deal update (e.g., partial fill).
* `PositionAdd` — a new position was opened.
* `PositionUpdate` — an existing position’s parameters changed.
* `PositionClose` — a position was closed.

## Purpose

Allows your application to react immediately to any server‐side trade event in real time, enabling instant logging, risk management, or user notifications when orders, deals, or positions change. 🚀
