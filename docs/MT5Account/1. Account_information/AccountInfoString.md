# ✅ Getting Individual Account String Properties

> **Request:** single `string` property from **MT5** account. Fetch specific text properties like currency, company name, account name, server name.

**API Information:**

* **SDK wrapper:** `MT5Account.AccountInfoStringAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.AccountInformation`
* **Proto definition:** `AccountInfoString` (defined in `mt5-term-api-account-information.proto`)

### RPC

* **Service:** `mt5_term_api.AccountInformation`
* **Method:** `AccountInfoString(AccountInfoStringRequest) → AccountInfoStringReply`
* **Low‑level client (generated):** `AccountInformation.AccountInfoString(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<string> AccountInfoStringAsync(
            AccountInfoStringPropertyType property,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`AccountInfoStringRequest { property_id: AccountInfoStringPropertyType }`


**Reply message:**

`AccountInfoStringReply { data: AccountInfoStringData }` or `{ error: Error }`

---

## 🔽 Input

| Parameter           | Type                             | Required | Description                                               |
| ------------------- | -------------------------------- | -------- | --------------------------------------------------------- |
| `property`          | `AccountInfoStringPropertyType`  | ✅       | Property to retrieve (see enum below)                     |
| `deadline`          | `DateTime?`                      | ❌       | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`              | ❌       | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `AccountInfoStringData`

| Field            | Type     | Description                 |
| ---------------- | -------- | --------------------------- |
| `RequestedValue` | `string` | The requested property value |

The method returns `string` directly (unwrapped from the proto message).

---

## 🧱 Related enums (from proto)

### `AccountInfoStringPropertyType`

| Enum Value        | Value | Description                                     | MQL5 Docs                                                       |
| ----------------- | ----- | ----------------------------------------------- | --------------------------------------------------------------- |
| `ACCOUNT_NAME`    | 0     | Client name                                     | [AccountInfoString](https://www.mql5.com/en/docs/account/accountinfostring) |
| `ACCOUNT_SERVER`  | 1     | Trade server name                               |                                                                 |
| `ACCOUNT_CURRENCY`| 2     | Account currency (e.g., `USD`, `EUR`)           |                                                                 |
| `ACCOUNT_COMPANY` | 3     | Name of a company that serves the account       |                                                                 |

---

## 💬 Just the essentials

* **What it is.** Single RPC returning one specific `string` property of the account.
* **Why you need it.** When you only need one text property (e.g., currency for display, company name for logging) instead of fetching the full account summary.
* **Performance.** Lightweight call — ideal for quick checks of specific properties.
* **Alternative.** Use `AccountSummaryAsync()` if you need multiple properties at once (more efficient).

---

## 🎯 Purpose

Use this method when you need to:

* Get the account currency for display or calculations.
* Retrieve the broker/company name for logging or verification.
* Fetch the client name for personalized UI.
* Get the server name for diagnostics or multi-account management.

---

## 🧩 Notes & Tips

* Prefer `AccountSummaryAsync()` if you need multiple properties — it's more efficient to fetch all data in one call.
* The currency property (`ACCOUNT_CURRENCY`) is commonly used for displaying balances with proper formatting.
* Use short per‑call timeout (3–5s) with retries when fetching properties for UI display.
* All MT5Account methods have built-in protection against transient gRPC errors with automatic reconnection.
* The method returns `string` directly (not wrapped in a proto message) for convenience.

---

## 🔗 Usage Examples

### 1) Get account currency

```csharp
// Retrieve deposit currency
var currency = await acct.AccountInfoStringAsync(
    AccountInfoStringPropertyType.ACCOUNT_CURRENCY,
    deadline: DateTime.UtcNow.AddSeconds(3));
Console.WriteLine($"Account currency: {currency}");
```

### 2) Get broker company name

```csharp
// Fetch broker/company name
var company = await acct.AccountInfoStringAsync(
    AccountInfoStringPropertyType.ACCOUNT_COMPANY);
Console.WriteLine($"Broker: {company}");
```

### 3) Get client name

```csharp
// Retrieve account holder name
var name = await acct.AccountInfoStringAsync(
    AccountInfoStringPropertyType.ACCOUNT_NAME);
Console.WriteLine($"Account holder: {name}");
```

### 4) Get server name

```csharp
// Retrieve trade server name for diagnostics
var server = await acct.AccountInfoStringAsync(
    AccountInfoStringPropertyType.ACCOUNT_SERVER);
Console.WriteLine($"Connected to server: {server}");
```

### 5) Display balance with currency

```csharp
// Combine with AccountInfoDouble for formatted output
var balance = await acct.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.ACCOUNT_BALANCE);
var currency = await acct.AccountInfoStringAsync(
    AccountInfoStringPropertyType.ACCOUNT_CURRENCY);

Console.WriteLine($"Balance: {balance:F2} {currency}");
```
