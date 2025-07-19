# Getting a Double Account Property1

> **Request:** account balance (double) from MT5
> Fetch the account balance or other floating-point property as a `double`.

---

### Code Example

```csharp
var balance = await _mt5Account.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.AccountBalance
);
_logger.LogInformation($"AccountInfoDouble: Balance={balance}");
```

---

### Method Signature

```csharp
Task<double> AccountInfoDoubleAsync(AccountInfoDoublePropertyType property)
```

---

## 🔽 Input

| Parameter  | Type                            | Description                                                 |
| ---------- | ------------------------------- | ----------------------------------------------------------- |
| `property` | `AccountInfoDoublePropertyType` | Enum indicating which account double-type field to retrieve |

### `AccountInfoDoublePropertyType` Enum Values

| Value                      | Description                             |
| -------------------------- | --------------------------------------- |
| `AccountBalance`           | Account balance in the deposit currency |
| `AccountCredit`            | Broker credit on the account            |
| `AccountProfit`            | Current profit                          |
| `AccountEquity`            | Account equity                          |
| `AccountAssets`            | Total account assets                    |
| `AccountLiabilities`       | Total account liabilities               |
| `AccountCommissionBlocked` | Currently blocked commission amount     |
| `AccountMargin`            | Used margin                             |
| `AccountMarginFree`        | Free margin                             |
| `AccountMarginLevel`       | Margin level in %                       |
| `AccountMarginInitial`     | Reserved margin for pending orders      |
| `AccountMarginMaintenance` | Maintenance margin requirement          |
| `AccountMarginSoCall`      | Margin call threshold                   |
| `AccountMarginSoSo`        | Stop-out margin level                   |

---

## ⬆️ Output

| Type     | Description                                 |
| -------- | ------------------------------------------- |
| `double` | The numeric value of the requested property |

---

## 🎯 Purpose

Use this method to fetch **any floating-point account parameter** with a single unified API.

This makes your code:

* Clean and reusable
* Easier to extend to other account stats
* Ideal for dashboards, analytics, and account monitoring

Simply pass a different enum value to retrieve other metrics.
