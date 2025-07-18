# Getting an Account Summary

> **Request:** full account summary (`AccountSummaryData`) from MT5
> Fetch all core account metrics in a single call.

---

### Code Example

```csharp
var summary = await _mt5Account.AccountSummaryAsync();
_logger.LogInformation($"Account Summary: Balance={summary.AccountBalance}");
```

---

### Method Signature

```csharp
Task<AccountSummaryData> AccountSummaryAsync()
```

---

## 🔽 Input

No input parameters.

---

## ⬆️ Output

Returns an **AccountSummaryData** object with:

| Field                      | Type     | Description                                |
| -------------------------- | -------- | ------------------------------------------ |
| `AccountBalance`           | `double` | Account balance (excluding floating P/L)   |
| `AccountCredit`            | `double` | Credit provided by broker                  |
| `AccountProfit`            | `double` | Current floating profit/loss               |
| `AccountEquity`            | `double` | Equity = balance + profit + credit         |
| `AccountMargin`            | `double` | Margin currently used for open positions   |
| `AccountFreeMargin`        | `double` | Free margin available for new trades       |
| `AccountMarginLevel`       | `double` | Margin level (%) = equity / margin × 100   |
| `AccountMarginInitial`     | `double` | Initial margin reserved for open positions |
| `AccountMarginMaintenance` | `double` | Minimum required maintenance margin        |

---

## 🎯 Purpose

Use this method to retrieve a **full snapshot of account metrics** in a single call.

Perfect for:

* Displaying account stats on dashboards
* Validating available margin before placing trades
* Monitoring account health, risk, and leverage
