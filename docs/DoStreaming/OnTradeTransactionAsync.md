# Streaming: OnTradeTransactionAsync

> **Request:** real-time trade transaction events (order creation, modification, execution) from MT5
> Subscribe to a live stream of detailed trade transaction data.

---

### Code Example

```csharp
await foreach (var txn in _mt5Account.OnTradeTransactionAsync(cts.Token))
{
    _logger.LogInformation(
        "OnTradeTransactionAsync: Transaction={@Transaction}",
        txn);
}
```

---

### Method Signature

```csharp
IAsyncEnumerable<OnTradeTransactionData> OnTradeTransactionAsync(
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

Returns a stream of **OnTradeTransactionData** items:

| Field          | Type                            | Description                          |
| -------------- | ------------------------------- | ------------------------------------ |
| `Time`         | `DateTime`                      | UTC time of transaction              |
| `Type`         | `TradeTransactionType`          | Type of transaction (see enum below) |
| `Symbol`       | `string`                        | Symbol associated with transaction   |
| `Order`        | `TradeTransactionOrderData?`    | Related order details (nullable)     |
| `Deal`         | `TradeTransactionDealData?`     | Related deal details (nullable)      |
| `Position`     | `TradeTransactionPositionData?` | Related position details (nullable)  |
| `RequestId`    | `ulong`                         | ID of original trade request         |
| `Reason`       | `TradeTransactionReason`        | Reason for the transaction (enum)    |
| `ErrorCode`    | `int`                           | Server error code (if any)           |
| `ErrorMessage` | `string`                        | Server error description (if any)    |

### `TradeTransactionType` Enum Values

| Value                            | Description          |
| -------------------------------- | -------------------- |
| `TradeTransactionOrderAdd`       | Order was added      |
| `TradeTransactionOrderUpdate`    | Order was updated    |
| `TradeTransactionOrderRemove`    | Order was removed    |
| `TradeTransactionDealAdd`        | Deal was added       |
| `TradeTransactionDealUpdate`     | Deal was updated     |
| `TradeTransactionDealRemove`     | Deal was removed     |
| `TradeTransactionPositionOpen`   | Position was opened  |
| `TradeTransactionPositionUpdate` | Position was updated |
| `TradeTransactionPositionClose`  | Position was closed  |

---

## 🎯 Purpose

This stream captures **every trade transaction in real time** including order actions, deal results, and position changes. Ideal for:

* Building detailed audit logs
* Triggering custom alerts
* Tracking execution feedback immediately
* Supporting trade lifecycle automation
