# Getting an Account Summary

> **Request:** full account summary (`AccountSummaryData`) from MT5
> Fetch all core account metrics in a single call.

### Code Example

```csharp
var summary = await _mt5Account.AccountSummaryAsync();
_logger.LogInformation($"Account Summary: Balance={summary.AccountBalance}");
```

✨ **Method Signature:**

```csharp
Task<AccountSummaryData> AccountSummaryAsync()
```

---

## Input

*None* — no parameters.

---

## Output

**`AccountSummaryData`** — object with the following properties:

* **`AccountBalance`** (`double`) — current account balance in the deposit currency.
* **`AccountCredit`** (`double`) — credit amount provided by the broker.
* **`AccountProfit`** (`double`) — current floating profit or loss in the deposit currency.
* **`AccountEquity`** (`double`) — account equity (balance + profit + credit).
* **`AccountMargin`** (`double`) — margin currently used for open positions.
* **`AccountFreeMargin`** (`double`) — free margin available.
* **`AccountMarginLevel`** (`double`) — margin level percentage (equity / margin × 100).
* **`AccountMarginInitial`** (`double`) — initial margin requirement for open positions.
* **`AccountMarginMaintenance`** (`double`) — maintenance margin requirement.

---

## Purpose

Retrieve all core account metrics in a single call, making your monitoring, logging, and code workflow more efficient. 🚀
