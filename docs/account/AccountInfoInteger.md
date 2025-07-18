# Getting an Integer Account Property

> **Request:** integer account property from MT5 (e.g., leverage)
> Fetch any integer-precision account property as a `long` value.

---

### Code Example

```csharp
var leverage = await _mt5Account.AccountInfoIntegerAsync(
    AccountInfoIntegerPropertyType.AccountLeverage
);
_logger.LogInformation($"AccountInfoInteger: Leverage={leverage}");
```

---

### Method Signature

```csharp
Task<long> AccountInfoIntegerAsync(AccountInfoIntegerPropertyType property)
```

---

## 🔽 Input

| Parameter  | Type                             | Description                                            |
| ---------- | -------------------------------- | ------------------------------------------------------ |
| `property` | `AccountInfoIntegerPropertyType` | Enum that specifies which integer account value to get |

### `AccountInfoIntegerPropertyType` Enum Values

| Value                   | MQL5 Const                | Description                                            |
| ----------------------- | ------------------------- | ------------------------------------------------------ |
| `AccountLogin`          | `ACCOUNT_LOGIN`           | Account number (login ID)                              |
| `AccountTradeMode`      | `ACCOUNT_TRADE_MODE`      | Trade mode (see `AccountTradeMode` enum)               |
| `AccountLeverage`       | `ACCOUNT_LEVERAGE`        | Account leverage                                       |
| `AccountLimitOrders`    | `ACCOUNT_LIMIT_ORDERS`    | Max number of allowed active pending orders            |
| `AccountMarginSoMode`   | `ACCOUNT_MARGIN_SO_MODE`  | Stop-out margin mode (see `StopoutMode` enum)          |
| `AccountTradeAllowed`   | `ACCOUNT_TRADE_ALLOWED`   | Whether manual trading is allowed (boolean)            |
| `AccountTradeExpert`    | `ACCOUNT_TRADE_EXPERT`    | Whether Expert Advisors can trade (boolean)            |
| `AccountMarginMode`     | `ACCOUNT_MARGIN_MODE`     | Margin calculation mode (see `AccountMarginMode` enum) |
| `AccountCurrencyDigits` | `ACCOUNT_CURRENCY_DIGITS` | Decimal digits for account currency display            |
| `AccountFifoClose`      | `ACCOUNT_FIFO_CLOSE`      | Whether FIFO close rule is enforced                    |
| `AccountHedgeAllowed`   | `ACCOUNT_HEDGE_ALLOWED`   | Whether hedging (opposite positions) is allowed        |

---

## ⬆️ Output

| Type   | Description                                  |
| ------ | -------------------------------------------- |
| `long` | Numeric value of the requested account field |

---

## 🎯 Purpose

Use this method to access **integer-type account properties** in a flexible way by passing an enum key.

Perfect for:

* Reading account leverage or login ID
* Checking trading permissions and platform modes
* Building adaptive trading logic without hardcoding constant names
