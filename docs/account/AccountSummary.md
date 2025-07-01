# Getting an Account Summary

>**Request:** full account summary (AccountSummaryData) from MT5

✨Fetch full account summary

### Code Example

```csharp
var summary = await _mt5Account.AccountSummaryAsync();
_logger.LogInformation($"Account Summary: Balance={summary.AccountBalance}");
```

**Method Signature** Task<AccountSummaryData> AccountSummaryAsync()
Input: none

**Output:** 
AccountSummaryData — object containing key account metrics:
AccountBalance
Equity
Margin
FreeMargin
etc.

**Purpose** - Retrieve all core account metrics in a single call, making your monitoring, logging, and code workflow more efficient. 🚀
