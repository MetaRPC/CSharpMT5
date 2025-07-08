# Calculating Required Margin

> **Request:** margin calculation for a planned trade

Compute the margin needed to open a position with given parameters.

### Code Example

```csharp
// Calculate margin for a buy order on EURUSD
tick = await _mt5Account.SymbolInfoTickAsync(Constants.DefaultSymbol);
var margin = await _mt5Account.OrderCalcMarginAsync(new OrderCalcMarginRequest
{
    Symbol    = Constants.DefaultSymbol,
    OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
    Volume    = Constants.DefaultVolume,
    OpenPrice = tick.Ask
});
_logger.LogInformation(
    "OrderCalcMargin: Margin={Margin}",
    margin.Margin
);
```

✨ **Method Signature:**

```csharp
Task<OrderCalcMarginData> OrderCalcMarginAsync(
    OrderCalcMarginRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## Input

**`OrderCalcMarginRequest`** — structure with the following fields:

* **`Symbol`** (`string`) — trading symbol to calculate margin for (e.g., `"EURUSD"`).
* **`OrderType`** (`ENUM_ORDER_TYPE_TF`) — type of order. Possible values:

  * **`OrderTypeTfBuy`** — market Buy order
  * **`OrderTypeTfSell`** — market Sell order
  * **`OrderTypeTfBuyLimit`** — pending Buy Limit order
  * **`OrderTypeTfSellLimit`** — pending Sell Limit order
  * **`OrderTypeTfBuyStop`** — pending Buy Stop order
  * **`OrderTypeTfSellStop`** — pending Sell Stop order
  * **`OrderTypeTfBuyStopLimit`** — pending Buy Stop Limit order
  * **`OrderTypeTfSellStopLimit`** — pending Sell Stop Limit order
* **`Volume`** (`double`) — volume in lots for which to calculate margin (e.g., `0.1`).
* **`OpenPrice`** (`double`) — proposed opening price of the position.

Optional parameters (passed directly to the async call):

* **`deadline`** (`DateTime?`) — optional UTC deadline for the operation.
* **`cancellationToken`** (`CancellationToken`) — optional token to cancel the request.

---

## Output

**`OrderCalcMarginData`** — structure with the following field:

* **`Margin`** (`double`) — required margin amount in the account currency.

---

## Purpose

Allows pre-trade margin checks programmatically, so you can manage risk and position sizing before sending an order. 🚀
