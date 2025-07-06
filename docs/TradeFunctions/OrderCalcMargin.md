# Calculating Required Margin

> **Request:** margin calculation for a planned trade

Compute the margin needed to open a position with given parameters.

### Code Example

```csharp
// Calculate margin for a buy order on EURUSD
var margin = await _mt5Account.OrderCalcMarginAsync(new OrderCalcMarginRequest
{
    Symbol    = Constants.DefaultSymbol,
    OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
    Volume    = Constants.DefaultVolume,
    OpenPrice = tick.Ask
});
_logger.LogInformation(
    "OrderCalcMargin: Margin={Margin}",
    margin.Margin);
```

✨**Method Signature:**
```csharp
Task<OrderCalcMarginData> OrderCalcMarginAsync(
    OrderCalcMarginRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

* **Input:**
    * **request (OrderCalcMarginRequest):**
      * `Symbol (string)`: the name of the symbol, for example "EURUSD".
      * `OrderType (ENUM_ORDER_TYPE_TF)`: the direction of the transaction, for example, OrderTypeTfBuy or OrderTypeTfSell.
      * `Volume (double)`: the volume of the transaction (in lots).
      * `Open Price (double)`: the price of opening a position.

* **Output:**
    * **OrderCalcMarginData** with the field:
      * `Margin (double)` — the required margin in the account currency.

**Purpose:** Allow pre-trade margin checks programmatically, so you can manage risk and position sizing before sending an order. 🚀
