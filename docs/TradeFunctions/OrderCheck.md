## Checking Trade Request Validity (OrderCheckAsync)

> **Request:** simulate and validate a trade order without sending it

Verify that a proposed market or pending order meets all server-side conditions (margin, volume, price) before execution.

### Code Example

```csharp
var tick = await _mt5Account.SymbolInfoTickAsync(Constants.DefaultSymbol);
var checkRequest = new OrderCheckRequest
{
    MqlTradeRequest = new MrpcMqlTradeRequest
    {
        Symbol     = Constants.DefaultSymbol,
        Volume     = Constants.DefaultVolume,
        Price      = tick.Ask,
        StopLimit  = tick.Ask + 0.0005,
        StopLoss   = tick.Ask - 0.0010,
        TakeProfit = tick.Ask + 0.0010,
        Deviation  = 10,
        OrderType  = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
        Expiration = null,
        Comment    = string.Empty,
        Position   = 0,
        PositionBy = 0
    }
};
var check = await _mt5Account.OrderCheckAsync(checkRequest);
_logger.LogInformation(
    "OrderCheckAsync: Margin={Margin} FreeMargin={FreeMargin} ReturnCode={RetCode} ReturnString={RetString}",
    check.Margin,
    check.FreeMargin,
    check.ReturnedCode,
    check.ReturnedStringCode
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
  * **`Volume`** (`double`) — volume in lots (e.g., `0.1`).
  * **`Price`** (`double`) — desired execution price for market or pending order.
  * **`StopLimit`** (`double?`) — price for a stop-limit order (optional).
  * **`StopLoss`** (`double?`) — stop-loss level (optional).
  * **`TakeProfit`** (`double?`) — take-profit level (optional).
  * **`Deviation`** (`int`) — maximum allowed slippage in price.
  * **`OrderType`** (`ENUM_ORDER_TYPE_TF`) — type of order. Possible values:

    * `OrderTypeTfBuy`, `OrderTypeTfSell`, `OrderTypeTfBuyLimit`, `OrderTypeTfSellLimit`, `OrderTypeTfBuyStop`, `OrderTypeTfSellStop`, `OrderTypeTfBuyStopLimit`, `OrderTypeTfSellStopLimit`.
  * **`Expiration`** (`DateTime?`) — expiration time for pending orders (optional).
  * **`Comment`** (`string`) — arbitrary comment attached to the order.
  * **`Position`** (`ulong`) — reference ticket for modification (optional).
  * **`PositionBy`** (`ulong`) — ticket to modify if specified (optional).

---

## Output

**`OrderCheckData`** — structure with the following fields:

* **`Margin`** (`double`) — required margin for this request.
* **`FreeMargin`** (`double`) — remaining free margin after this request.
* **`ReturnedCode`** (`uint`) — server-side validation code.
* **`ReturnedStringCode`** (`string`) — human-readable description of the validation result.

---

## Purpose

Performs a dry-run of your trade logic to catch margin violations, invalid volumes, or price errors before placing real orders, improving reliability and user feedback. 🚀
