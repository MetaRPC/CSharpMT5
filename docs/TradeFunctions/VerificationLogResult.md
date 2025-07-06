# Verifying a Trade and Logging Margin vs Free Margin

> **Request:** simulate a trade and retrieve required margin and remaining free margin

### Code Example

```csharp
var checkResult = await _mt5Account.OrderCheckAsync(checkRequest);
_logger.LogInformation(
    "OrderCheck: Margin={Margin}, FreeMargin={FreeMargin}",
    checkResult.MqlTradeCheckResult.Margin,
    checkResult.MqlTradeCheckResult.FreeMargin);
```

✨**Method Signature:**
```csharp
Task<OrderCheckData> OrderCheckAsync(
    OrderCheckRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```
 **Input:** 
* **request** (`OrderCheckRequest`):
 * encapsulates an `MrpcMqlTradeRequest` with fields like `Symbol`, `Volume`, `Price`, `StopLoss`, `TakeProfit`, etc.

 **Output:**
 * **OrderCheckData** with property:
  * **MqlTradeCheckResult** (`MqlTradeCheckResultData`) — contains:
    * **Margin** (`double`) — margin required for the proposed trade.
    * **FreeMargin** (`double`) — remaining free margin after this trade.

**Purpose:** Ensure a trade is valid and inspect both the required margin and the impact on your free margin before execution. 🚀
