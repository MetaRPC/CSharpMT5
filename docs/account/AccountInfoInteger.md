# Getting an integer account property

> **Requesting** leverage (integer) for MT5.

### Code Example

```csharp
var leverage = await _mt5Account.AccountInfoIntegerAsync(
    AccountInfoIntegerPropertyType.AccountLeverage
);
_logger.LogInformation($"AccountInfoInteger: Leverage={leverage}");
```

✨**Method Signature:** 
```csharp
AccountInfoIntegerAsync(AccountInfoIntegerPropertyType property)
```

* **Input: property (AccountInfoIntegerPropertyType) - an enumeration indicating which integer property to get.**

* **Examples:** 
   * _AccountLeverage_
   * _AccountLogin_(account ID)
   * _TradeMode_

* **Output: Int - the value of the requested property (for example, leverage = 100).**

**Purpose** - Allows you to select and get any integer property of an account through a single universal method, while keeping your code clean, consistent, and easy to extend.🚀
