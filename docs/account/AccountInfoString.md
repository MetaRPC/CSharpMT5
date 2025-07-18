# Getting a String Account Property

> **Request:** account string property from MT5 (e.g., currency)
> Fetch any string-based account property as a text value.

---

### Code Example

```csharp
var currency = await _mt5Account.AccountInfoStringAsync(
    AccountInfoStringPropertyType.AccountCurrency
);
_logger.LogInformation($"AccountInfoString: Currency={currency}");
```

---

### Method Signature

```csharp
Task<string> AccountInfoStringAsync(AccountInfoStringPropertyType property)
```

---

## 🔽 Input

| Parameter  | Type                            | Description                                                |
| ---------- | ------------------------------- | ---------------------------------------------------------- |
| `property` | `AccountInfoStringPropertyType` | Enum that specifies which string account property to fetch |

### `AccountInfoStringPropertyType` Enum Values

| Value             | MQL5 Const         | Description                                       |
| ----------------- | ------------------ | ------------------------------------------------- |
| `AccountName`     | `ACCOUNT_NAME`     | Full name of the account owner                    |
| `AccountServer`   | `ACCOUNT_SERVER`   | Trading server name (e.g., "MetaQuotes-Demo")     |
| `AccountCurrency` | `ACCOUNT_CURRENCY` | Deposit currency of the account (e.g., "USD")     |
| `AccountCompany`  | `ACCOUNT_COMPANY`  | Name of the brokerage company serving the account |

---

## ⬆️ Output

| Type     | Description                  |
| -------- | ---------------------------- |
| `string` | Requested account text value |

---

## 🎯 Purpose

Use this method to retrieve any **string-type account attribute** via a consistent enum interface.

Perfect for:

* Displaying user or broker identity
* Logging deposit currency or server location
* Building dynamic UIs based on account metadata

Change only the enum to retrieve other string fields.
