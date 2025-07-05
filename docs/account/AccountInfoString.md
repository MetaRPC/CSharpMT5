# Getting a String Account Property

> **Requesting** account currency (string) from MT5.

### Code Example

```csharp
var currency = await _mt5Account.AccountInfoStringAsync(
    AccountInfoStringPropertyType.AccountCurrency
);
_logger.LogInformation($"AccountInfoString: Currency={currency}");
```

✨**Method Signature:**
 ```csharp
AccountInfoStringAsync(AccountInfoStringPropertyType property)
 ```
* **Input: property (AccountInfoStringPropertyType) — enumeration value indicating which text property to fetch.**

* **Examples:**
   * _AccountCurrency_(e.g. “USD”)
   * _ServerName_ — name of the trading server
   * _AccountOwner_ — owner’s name

* **Output: string — the requested value (for example, "EUR").**

**Purpose:** Use this single, universal method to retrieve any string-based account property, keeping your code clean, consistent, and easy to extend. 🚀

