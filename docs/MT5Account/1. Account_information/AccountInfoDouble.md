# ✅ Getting Individual Account Double Properties

> **Request:** single `double` property from **MT5** account. Fetch specific numeric properties like balance, equity, margin, profit, etc.

**API Information:**

* **SDK wrapper:** `MT5Account.AccountInfoDoubleAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.AccountInformation`
* **Proto definition:** `AccountInfoDouble` (defined in `mt5-term-api-account-information.proto`)

### RPC

* **Service:** `mt5_term_api.AccountInformation`
* **Method:** `AccountInfoDouble(AccountInfoDoubleRequest) → AccountInfoDoubleReply`
* **Low‑level client (generated):** `AccountInformation.AccountInfoDouble(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<double> AccountInfoDoubleAsync(
            AccountInfoDoublePropertyType property,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`AccountInfoDoubleRequest { property_id: AccountInfoDoublePropertyType }`


**Reply message:**

`AccountInfoDoubleReply { data: AccountInfoDoubleData }` or `{ error: Error }`

---

## 🔽 Input

| Parameter           | Type                               | Required | Description                                               |
| ------------------- | ---------------------------------- | -------- | --------------------------------------------------------- |
| `property`          | `AccountInfoDoublePropertyType`    | ✅       | Property to retrieve (see enum below)                     |
| `deadline`          | `DateTime?`                        | ❌       | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`                | ❌       | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `AccountInfoDoubleData`

| Field            | Type     | Description                 |
| ---------------- | -------- | --------------------------- |
| `RequestedValue` | `double` | The requested property value |

The method returns `double` directly (unwrapped from the proto message).

---

## 🧱 Related enums (from proto)

### `AccountInfoDoublePropertyType`

| Enum Value                   | Value | Description                                                                                                                       | MQL5 Docs                                                     |
| ---------------------------- | ----- | --------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------- |
| `AccountBalance`            | 0     | Account balance in the deposit currency                                                                                           | [AccountInfoDouble](https://www.mql5.com/en/docs/account/accountinfodouble) |
| `AccountCredit`             | 1     | Account credit in the deposit currency                                                                                            |                                                               |
| `AccountProfit`             | 2     | Current profit of an account in the deposit currency                                                                              |                                                               |
| `AccountEquity`             | 3     | Account equity in the deposit currency (Balance + Profit)                                                                         |                                                               |
| `AccountMargin`             | 4     | Account margin used in the deposit currency                                                                                       |                                                               |
| `AccountMarginFree`        | 5     | Free margin of an account in the deposit currency                                                                                 |                                                               |
| `AccountMarginLevel`       | 6     | Account margin level in percents                                                                                                  |                                                               |
| `AccountMarginSoCall`     | 7     | Margin call level. Depending on the set AccountMarginSoMode is expressed in percents or in the deposit currency               |                                                               |
| `AccountMarginSoSo`       | 8     | Margin stop out level. Depending on the set AccountMarginSoMode is expressed in percents or in the deposit currency           |                                                               |
| `AccountMarginInitial`     | 9     | Initial margin. The amount reserved on an account to cover the margin of all pending orders                                       |                                                               |
| `AccountMarginMaintenance` | 10    | Maintenance margin. The minimum equity reserved on an account to cover the minimum amount of all open positions                   |                                                               |
| `AccountAssets`             | 11    | The current assets of an account                                                                                                  |                                                               |
| `AccountLiabilities`        | 12    | The current liabilities on an account                                                                                             |                                                               |
| `AccountCommissionBlocked` | 13    | The current blocked commission amount on an account                                                                               |                                                               |

---

## 💬 Just the essentials

* **What it is.** Single RPC returning one specific `double` property of the account.
* **Why you need it.** When you only need one property (e.g., margin level before placing an order) instead of fetching the full account summary.
* **Performance.** Lightweight call — ideal for frequent checks of specific properties.
* **Alternative.** Use `AccountSummaryAsync()` if you need multiple properties at once.

---

## 🎯 Purpose

Use this method when you need to:

* Check a single account property (margin, equity, profit, etc.) without fetching all account data.
* Monitor specific properties frequently (e.g., margin level for risk management).
* Verify free margin before placing trades.
* Calculate margin requirements dynamically.

---

## 🧩 Notes & Tips

* Prefer `AccountSummaryAsync()` if you need multiple properties — it's more efficient to fetch all data in one call.
* Use short per‑call timeout (3–5s) with retries for margin checks before trading operations.
* All MT5Account methods have built-in protection against transient gRPC errors with automatic reconnection.
* For UI dashboards displaying multiple properties, use `AccountSummaryAsync()` instead.
* The method returns `double` directly (not wrapped in a proto message) for convenience.

---

## 🔗 Usage Examples

### 1) Check margin level before placing order

```csharp
// Ensure sufficient margin before trading
var marginLevel = await acct.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.AccountMarginLevel,
    deadline: DateTime.UtcNow.AddSeconds(3));

if (marginLevel < 200.0)
{
    Console.WriteLine($"⚠️ Warning: Low margin level {marginLevel:F2}%");
    // Skip trading or reduce position size
}
else
{
    Console.WriteLine($"✅ Margin level OK: {marginLevel:F2}%");
}
```

### 2) Get current profit

```csharp
// Monitor floating profit/loss
var profit = await acct.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.AccountProfit);
Console.WriteLine($"Current P/L: {profit:F2}");
```

### 3) Check free margin

```csharp
// Verify free margin before opening position
var freeMargin = await acct.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.AccountMarginFree);

Console.WriteLine($"Free margin: {freeMargin:F2}");

if (freeMargin < 100.0)
{
    Console.WriteLine("⚠️ Insufficient free margin");
}
```

### 4) Monitor margin usage

```csharp
// Fetch margin-related properties
var margin = await acct.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.AccountMargin);
var marginFree = await acct.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.AccountMarginFree);
var marginLevel = await acct.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.AccountMarginLevel);

Console.WriteLine($"Margin:       {margin:F2}");
Console.WriteLine($"Free:         {marginFree:F2}");
Console.WriteLine($"Level:        {marginLevel:F2}%");
```

### 5) Compare equity vs balance

```csharp
// Calculate floating P/L by comparing equity and balance
var balance = await acct.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.AccountBalance);
var equity = await acct.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.AccountEquity);

var floatingPL = equity - balance;
Console.WriteLine($"Balance:      {balance:F2}");
Console.WriteLine($"Equity:       {equity:F2}");
Console.WriteLine($"Floating P/L: {floatingPL:F2}");
```
