# Checking Trade Request Validity

> **Request:** simulate and validate a trade order without sending it

Verify that a proposed market or pending order meets all server‐side conditions (margin, volume, price) before execution.

### Code Example

```csharp
var checkRequest = new OrderCheckRequest
{
    MqlTradeRequest = new MrpcMqlTradeRequest
    {
        Symbol     = Constants.DefaultSymbol,
        Volume     = Constants.DefaultVolume,
        Price      = tick.Ask,
        StopLimit  = tick.Ask + 0.0005,  // example values
        StopLoss   = tick.Ask - 0.0010,
        TakeProfit = tick.Ask + 0.0010,
        Deviation  = 10,                 // max slippage
        OrderType  = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
        Expiration = null,
        Comment    = string.Empty,
        Position   = 0,
        PositionBy = 0
    }
};
var check = await _mt5Account.OrderCheckAsync(checkRequest);
_logger.LogInformation(
    "OrderCheckAsync: Margin={Margin} ReturnCode={RetCode} ReturnString={RetString}",
    check.Margin,
    check.ReturnedCode,
    check.ReturnedStringCode);
```

✨**Method Signature:**
```csharp
Task<OrderCheckData> OrderCheckAsync(
    OrderCheckRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```
* **Input:**
    * **request (OrderCheckRequest):**
      * **MqlTradeRequest** (MrpcMqlTradeRequest):
        * `Symbol` (string) – symbol name, e.g. "EURUSD".
        * `Volume` (double) – volume in lots.
        * `Price` (double) – desired execution price.
        * `StopLimit`, `StopLoss`, `TakeProfit` (double?) – optional price levels.
        * `Deviation` (int) – maximum allowed slippage.
        * `OrderType` (ENUM_ORDER_TYPE_TF) – buy/sell direction.
        * `Expiration` (DateTime?), `Comment` (string), `Position`, `PositionBy` – other trade fields.

* **Output:**
    * **OrderCheckData** with properties:
      * `Margin (double)` – required margin for this request.
      * `ReturnedCode (uint)` – server return code.
      * `ReturnedStringCode (string)` – human‐readable return code.

**Purpose:** Perform a dry‐run of your trade logic to catch margin violations, invalid volumes, or price errors before placing real orders. 🚀



