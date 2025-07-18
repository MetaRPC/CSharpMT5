# Streaming: OnTradeAsync

> **Request:** real-time trade events from MT5
> Subscribe to a live stream of trade events (orders, deals, positions).

---

### Code Example

```csharp
await foreach (var tradeEvent in _mt5Account.OnTradeAsync(cts.Token))
{
    _logger.LogInformation(
        "OnTradeAsync: TradeEvent={@TradeEvent}",
        tradeEvent);
}
```

---

### Method Signature

```csharp
IAsyncEnumerable<OnTradeData> OnTradeAsync(
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

| Parameter           | Type                | Description                |
| ------------------- | ------------------- | -------------------------- |
| `cancellationToken` | `CancellationToken` | Token to cancel the stream |

---

## ⬆️ Output

Returns a stream of **OnTradeData** items:

| Field       | Type             | Description                                         |
| ----------- | ---------------- | --------------------------------------------------- |
| `Symbol`    | `string`         | Trading symbol involved in the event                |
| `Type`      | `TradeEventType` | Event type (order, deal, position add/update/close) |
| `Order`     | `OrderInfo?`     | Details of the related order (nullable)             |
| `Deal`      | `DealInfo?`      | Details of the related deal (nullable)              |
| `Position`  | `PositionInfo?`  | Details of the related position (nullable)          |
| `Timestamp` | `DateTime`       | UTC time when the event occurred                    |

### `TradeEventType` Enum Values

| Value            | Description                        |
| ---------------- | ---------------------------------- |
| `OrderAdd`       | A new order was placed             |
| `OrderUpdate`    | An existing order was modified     |
| `OrderClose`     | An order was closed                |
| `DealAdd`        | A new deal was executed            |
| `DealUpdate`     | A deal update (e.g., partial fill) |
| `PositionAdd`    | A new position was opened          |
| `PositionUpdate` | An existing position was modified  |
| `PositionClose`  | A position was closed              |

---

## 🎯 Purpose

This method enables your app to receive **live trade activity from the server**, so it can:

* Update UI or logs instantly
* React to position changes with automation
* Notify users of real-time order/deal execution
* Enforce compliance or risk logic in real time
