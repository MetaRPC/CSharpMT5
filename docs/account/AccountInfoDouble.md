## Getting a Double Account Property

Requesting leverage (integer) for MT5.

Request account balance (double) from MT5.

var balance = await _mt5Account.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.AccountBalance
);
_logger.LogInformation($"AccountInfoDouble: Balance={balance}");
Method: AccountInfoDoubleAsync(AccountInfoDoublePropertyType property)

Input: property (AccountInfoDoublePropertyType): enumeration value indicating which double‐precision property to fetch.
Examples: AccountBalance, Equity(current equity), FreeMargin, Credit.

Output: double — the requested numeric value (e.g., balance = 12345.67).
Purpose - Use this single, universal method to retrieve any floating‐point account property, keeping your code concise and flexible.
