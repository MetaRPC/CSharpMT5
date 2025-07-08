# Getting a String Account Property

> **Request:** account string property from MT5 (e.g., currency)

Fetch any string-based account property as a text value.

### Code Example

```csharp
var currency = await _mt5Account.AccountInfoStringAsync(
    AccountInfoStringPropertyType.AccountCurrency
);
_logger.LogInformation($"AccountInfoString: Currency={currency}");
```

✨ **Method Signature:**

```csharp
Task<string> AccountInfoStringAsync(AccountInfoStringPropertyType property)
```

---

## Input

**property** (`AccountInfoStringPropertyType`): enumeration value indicating which string account property to fetch. Available values:

* **AccountName** (`ACCOUNT_NAME`) — the name of the account owner (e.g., client’s full name).
* **AccountServer** (`ACCOUNT_SERVER`) — the trading server name (e.g., "MetaQuotes-Demo").
* **AccountCurrency** (`ACCOUNT_CURRENCY`) — the deposit currency of the account (e.g., "USD").
* **AccountCompany** (`ACCOUNT_COMPANY`) — the name of the company (broker) that serves the account.

---

## Output

* `string` — the requested text value (e.g., "EUR", "broker\_name").

---

## Purpose

Use this single, universal method to retrieve any string-based account property, keeping your code clean, consistent, and easy to extend. Simply swap the enum value to get the desired text property! 🚀
