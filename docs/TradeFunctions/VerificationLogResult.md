# Verifying a Trade and Logging Margin vs Free Margin

> **Request:** simulate a trade and retrieve required margin and remaining free margin.

### Code Example

```csharp
var checkResult = await _mt5Account.OrderCheckAsync(checkRequest);
_logger.LogInformation(
    "OrderCheck: Margin={Margin}, FreeMargin={FreeMargin}",
    checkResult.MqlTradeCheckResult.Margin,
    checkResult.MqlTradeCheckResult.FreeMargin
);
```

✨ **Method Signature:**

```csharp
Task<OrderCheckData> OrderCheckAsync(
    OrderCheckRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## Input

**`OrderCheckRequest`** — structure with the following field:

* **`MqlTradeRequest`** (`MrpcMqlTradeRequest`) — trade parameters to validate:

  * **`Symbol`** (`string`) — trading symbol (e.g., `"EURUSD"`).
  * **`Volume`** (`double`) — requested volume in lots.
  * **`Price`** (`double`) — desired execution price.
  * **`StopLimit`** (`double?`) — stop-limit price (optional).
  * **`StopLoss`** (`double?`) — stop-loss level (optional).
  * **`TakeProfit`** (`double?`) — take-profit level (optional).
  * **`Deviation`** (`int`) — maximum allowed slippage in points.
  * **`OrderType`** (`ENUM_ORDER_TYPE_TF`) — order type. Possible values:

    * `OrderTypeTfBuy`, `OrderTypeTfSell`, `OrderTypeTfBuyLimit`, `OrderTypeTfSellLimit`, `OrderTypeTfBuyStop`, `OrderTypeTfSellStop`, `OrderTypeTfBuyStopLimit`, `OrderTypeTfSellStopLimit`.
  * **`Expiration`** (`DateTime?`) — expiration time for pending orders (optional).
  * **`Comment`** (`string`) — optional comment attached to the order.
  * **`Position`** (`ulong`) — ticket of order to be modified (optional).
  * **`PositionBy`** (`ulong`) — secondary ticket reference (optional).

---

## Output

**`OrderCheckData`** — structure with the following field:

* **`MqlTradeCheckResult`** (`MqlTradeCheckResultData`) — result of the simulated trade check:

  * **`Margin`** (`double`) — required margin for this request.
  * **`FreeMargin`** (`double`) — remaining free margin after the simulated trade.

---

## Purpose

Ensures a trade is valid and shows both the required margin and the impact on free margin before execution, improving pre-trade risk management and decision-making. 🚀
