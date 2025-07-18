# Checking Trade Request Validity (OrderCheckAsync)

> **Request:** simulate and validate a trade order without sending it
> Verify that a proposed market or pending order meets all server-side conditions (margin, volume, price) before execution.

---

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

---

### Method Signature

```csharp
Task<OrderCheckData> OrderCheckAsync(
    OrderCheckRequest request,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default
)
```

---

## 🔽 Input

**OrderCheckRequest** — object with:

| Field             | Type                  | Description               |
| ----------------- | --------------------- | ------------------------- |
| `MqlTradeRequest` | `MrpcMqlTradeRequest` | Trade request to validate |

### `MrpcMqlTradeRequest` Fields

| Field        | Type                 | Description                         |
| ------------ | -------------------- | ----------------------------------- |
| `Symbol`     | `string`             | Trading symbol (e.g., "EURUSD")     |
| `Volume`     | `double`             | Order volume in lots                |
| `Price`      | `double`             | Execution price                     |
| `StopLimit`  | `double?`            | Stop limit price (optional)         |
| `StopLoss`   | `double?`            | Stop Loss level (optional)          |
| `TakeProfit` | `double?`            | Take Profit level (optional)        |
| `Deviation`  | `int`                | Max price slippage                  |
| `OrderType`  | `ENUM_ORDER_TYPE_TF` | Order type                          |
| `Expiration` | `DateTime?`          | Pending order expiration (optional) |
| `Comment`    | `string`             | Order comment (optional)            |
| `Position`   | `ulong`              | Position ID (optional)              |
| `PositionBy` | `ulong`              | PositionBy ID (optional)            |

### `ENUM_ORDER_TYPE_TF` Values

| Value                      | Description             |
| -------------------------- | ----------------------- |
| `OrderTypeTfBuy`           | Market Buy              |
| `OrderTypeTfSell`          | Market Sell             |
| `OrderTypeTfBuyLimit`      | Pending Buy Limit       |
| `OrderTypeTfSellLimit`     | Pending Sell Limit      |
| `OrderTypeTfBuyStop`       | Pending Buy Stop        |
| `OrderTypeTfSellStop`      | Pending Sell Stop       |
| `OrderTypeTfBuyStopLimit`  | Pending Buy Stop Limit  |
| `OrderTypeTfSellStopLimit` | Pending Sell Stop Limit |

---

## ⬆️ Output

Returns an **OrderCheckData** object:

| Field                | Type     | Description                                  |
| -------------------- | -------- | -------------------------------------------- |
| `Margin`             | `double` | Required margin for this order               |
| `FreeMargin`         | `double` | Remaining margin after applying this request |
| `ReturnedCode`       | `uint`   | Numeric validation code returned by server   |
| `ReturnedStringCode` | `string` | Human-readable description of the validation |

---

## 🎯 Purpose

Use this method to perform a "dry run" of an order request and receive validation results **without sending the actual order**.

Useful for:

* Pre-checking margin sufficiency
* Validating price/volume/risk before execution
* Building robust trading workflows and UIs with clear error messages
