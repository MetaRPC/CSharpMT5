# Getting an integer account property

> **Requesting** leverage (integer) for MT5.

### Code Example

```csharp
var leverage = await _mt5Account.AccountInfoIntegerAsync(
    AccountInfoIntegerPropertyType.AccountLeverage
);
_logger.LogInformation($"AccountInfoInteger: Leverage={leverage}");
```

**Method Signature:** AccountInfoIntegerAsync(AccountInfoIntegerPropertyType property)

**Input:** property (AccountInfoIntegerPropertyType) - an enumeration indicating which integer property to get.

**Examples:** AccountLeverage, AccountLogin(account ID), TradeMode.

**Output:** Int - the value of the requested property (for example, leverage = 100).

**Purpose** - Allows you to select and get any integer property of an account through a single universal method, while ma
