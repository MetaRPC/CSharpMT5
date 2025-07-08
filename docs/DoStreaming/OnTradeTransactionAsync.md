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

---

## Output

**`OnTradeTransactionData`** — structure representing a single trade transaction event.

### `OnTradeTransactionData` Fields

* **`Time`** (`DateTime`) — UTC timestamp when the transaction was recorded.
* **`Type`** (`TradeTransactionType`) — kind of transaction. Possible enum values:

  * `TradeTransactionOrderAdd`
  * `TradeTransactionOrderUpdate`
  * `TradeTransactionOrderRemove`
  * `TradeTransactionDealAdd`
  * `TradeTransactionDealUpdate`
  * `TradeTransactionDealRemove`
  * `TradeTransactionPositionOpen`
  * `TradeTransactionPositionUpdate`
  * `TradeTransactionPositionClose`
* **`Symbol`** (`string`) — trading symbol associated with the transaction.
* **`Order`** (`TradeTransactionOrderData?`) — details of the order involved (if applicable):

  * **`OrderId`** (`ulong`)
  * **`Action`** (`OrderActionType` enum)
  * **`Volume`** (`double`)
  * **`Price`** (`double`)
  * **`StopLimit`**, **`StopLoss`**, **`TakeProfit`** (`double?`)
  * **`Deviation`** (`int`)
  * **`MagicNumber`** (`ulong`)
  * **`Comment`** (`string`)
* **`Deal`** (`TradeTransactionDealData?`) — details of the deal involved (if applicable):

  * **`DealId`** (`ulong`)
  * **`OrderId`** (`ulong`)
  * **`Volume`** (`double`)
  * **`Price`** (`double`)
  * **`Profit`** (`double`)
* **`Position`** (`TradeTransactionPositionData?`) — details of the position involved (if applicable):

  * **`PositionId`** (`ulong`)
  * **`OrderId`** (`ulong`)
  * **`Volume`** (`double`)
  * **`PriceOpen`** (`double`)
  * **`Swap`** (`double`)
  * **`Profit`** (`double`)
* **`RequestId`** (`ulong`) — identifier of the original trade request (if produced).
* **`Reason`** (`TradeTransactionReason`) — why the transaction occurred (enum).
* **`ErrorCode`** (`int`) — server error code, if any.
* **`ErrorMessage`** (`string`) — human-readable error description, if any.

---

## Purpose

Capture every granular trade transaction in real time, enabling audit logs, custom notifications, or automated trade-management logic as soon as activity occurs. 🚀
