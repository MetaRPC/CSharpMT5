# Streaming: OnTradeTransactionAsync

> **Request:** real-time trade transaction events (order creation, modification, execution) from MT5

Subscribe to a live stream of detailed trade transaction data.

### Code Example

```csharp
await foreach (var txn in _mt5Account.OnTradeTransactionAsync(cts.Token))
{
    _logger.LogInformation(
        "OnTradeTransactionAsync: Transaction={@Transaction}",
        txn);
}
```

✨**Method Signature:**

```csharp
IAsyncEnumerable<OnTradeTransactionData> OnTradeTransactionAsync(
    CancellationToken cancellationToken = default
)
```

---

## Input

* **`cancellationToken`** (`CancellationToken`) — token to cancel the streaming.

## Output

* **`OnTradeTransactionData`** — structure containing detailed transaction data. Fields include:

  * **`Transaction`** (`Mt5TradeTransaction`) — the raw trade transaction info (e.g., order/deal IDs, types).
  * **`ErrorCode`** (`int`) — any error code from the server.
  * **`ErrorString`** (`string`) — human-readable error message, if any.

> **Note:** please verify and list all actual fields of `OnTradeTransactionData`, including any nested structures and enums.

---

## Purpose

Capture every granular trade transaction in real time, enabling audit logs, custom notifications, or automated trade-management logic as soon as activity occurs. 🚀
