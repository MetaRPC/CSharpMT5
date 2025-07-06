# Streaming: OnTradeTransactionAsync

> **Request:** real-time trade transaction events (order creation, modification, execution)

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
* **Input:**
    * **cancellationToken** (`CancellationToken`) — token to cancel the streaming.     

* **Output:**
    * async stream of **OnTradeTransactionData**, each containing full details of a trade transaction (order, deal, position change).

**Purpose:** Capture every granular trade transaction in real time, enabling audit logs, custom notifications, or automated trade-management logic as soon as activity occurs. 🚀
