# Getting a Double Account Property

> **Request:** account balance (double) from MT5

 Fetch the account balance as a double

### Code Example

```csharp
var balance = await _mt5Account.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.AccountBalance
);
_logger.LogInformation($"AccountInfoDouble: Balance={balance}");
```

✨**Method Signature:** AccountInfoDoubleAsync(AccountInfoDoublePropertyType property)

* **Input: property (AccountInfoDoublePropertyType): enumeration value indicating which double‐precision property to fetch.**

* **Examples:** 
    * _AccountBalance_
    * _Equity_(current equity)
    *  _FreeMargin_
    *   _Credit_

* **Output: double — the requested numeric value (e.g., balance = 12345.67).**

**Purpose** - Keep your code concise and future-ready by using one universal method to retrieve any floating-point account property. No more copy-paste overload — simply swap the enum and you’re good to go! 🚀
