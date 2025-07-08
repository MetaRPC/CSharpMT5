# Getting a Double Account Property

> **Request:** account balance (double) from MT5
> Fetch the account balance as a double

### Code Example

```csharp
var balance = await _mt5Account.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.AccountBalance
);
_logger.LogInformation($"AccountInfoDouble: Balance={balance}");
```

✨ **Method Signature:**

```csharp
Task<double> AccountInfoDoubleAsync(AccountInfoDoublePropertyType property)
```

---

## Input

**property** (`AccountInfoDoublePropertyType`): enumeration value indicating which double-precision account property to fetch. Available values:

* **AccountBalance** — Account balance in the deposit currency.
* **AccountCredit** — The amount of the credit provided by the broker in the deposit currency.
* **AccountProfit** — The amount of current profit on the account in the deposit currency.
* **AccountEquity** — Account equity in the deposit currency.
* **AccountAssets** — Current amount of assets on the account.
* **AccountLiabilities** — Current amount of liabilities on the account.
* **AccountCommissionBlocked** — The current amount of blocked commissions on the account.
* **AccountMargin** — Account margin used in the deposit currency.
* **AccountMarginFree** — Free margin of an account in the deposit currency.
* **AccountMarginLevel** — Account margin level in percents.
* **AccountMarginInitial** — Initial margin: amount reserved on an account to cover the margin of all pending orders.
* **AccountMarginMaintenance** — Maintenance margin: minimum equity reserved on an account to cover the minimum amount of all open positions.
* **AccountMarginSoCall** — Margin call level: expressed in percents or in the deposit currency depending on margin call mode.
* **AccountMarginSoSo** — Margin stop-out level: expressed in percents or in the deposit currency depending on margin stop-out mode.

---

## Output

* `double` — the requested numeric value (e.g., `12345.67`).

---

## Purpose

Keep your code concise and future-ready by using one universal method to retrieve any floating-point account property — simply swap the enum value! 🚀
