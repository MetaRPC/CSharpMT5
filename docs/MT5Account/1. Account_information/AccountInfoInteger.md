# ✅ Getting Individual Account Integer Properties

> **Request:** single `long` property from **MT5** account. Fetch specific integer properties like login, leverage, trade mode, limits, etc.

**API Information:**

* **SDK wrapper:** `MT5Account.AccountInfoIntegerAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.AccountInformation`
* **Proto definition:** `AccountInfoInteger` (defined in `mt5-term-api-account-information.proto`)

### RPC

* **Service:** `mt5_term_api.AccountInformation`
* **Method:** `AccountInfoInteger(AccountInfoIntegerRequest) → AccountInfoIntegerReply`
* **Low‑level client (generated):** `AccountInformation.AccountInfoInteger(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace MetaRPC.CSharpMT5
{
    public class MT5Account
    {
        public async Task<long> AccountInfoIntegerAsync(
            AccountInfoIntegerPropertyType property,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`AccountInfoIntegerRequest { property_id: AccountInfoIntegerPropertyType }`


**Reply message:**

`AccountInfoIntegerReply { data: AccountInfoIntegerData }` or `{ error: Error }`

---

## 🔽 Input

| Parameter           | Type                                | Required | Description                                               |
| ------------------- | ----------------------------------- | -------- | --------------------------------------------------------- |
| `property`          | `AccountInfoIntegerPropertyType`    | ✅       | Property to retrieve (see enum below)                     |
| `deadline`          | `DateTime?`                         | ❌       | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`                 | ❌       | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `AccountInfoIntegerData`

| Field            | Type   | Description                 |
| ---------------- | ------ | --------------------------- |
| `RequestedValue` | `long` | The requested property value |

The method returns `long` directly (unwrapped from the proto message).

---

## 🧱 Related enums (from proto)

### `AccountInfoIntegerPropertyType`

| Enum Value               | Value | Description                                                                                                                                                                                                                                                                                                                                                                                                                                            | MQL5 Docs                                                        |
| ------------------------ | ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------- |
| `AccountLogin`          | 0     | Account number                                                                                                                                                                                                                                                                                                                                                                                                                                         | [AccountInfoInteger](https://www.mql5.com/en/docs/account/accountinfointeger) |
| `AccountTradeMode`     | 1     | Account trade mode (Demo/Contest/Real) — see `MrpcEnumAccountTradeMode`                                                                                                                                                                                                                                                                                                                                                                               |                                                                  |
| `AccountLeverage`       | 2     | Account leverage (e.g., `100` for 1:100)                                                                                                                                                                                                                                                                                                                                                                                                              |                                                                  |
| `AccountLimitOrders`   | 3     | Maximum allowed number of open positions and active pending orders (in total). `0` = unlimited                                                                                                                                                                                                                                                                                                                                                        |                                                                  |
| `AccountMarginSoMode` | 4     | Mode for setting the minimal allowed margin — see `EnumAccountStopOutMode` (not yet documented)                                                                                                                                                                                                                                                                                                                                                       |                                                                  |
| `AccountTradeAllowed`  | 5     | Allowed trade for the current account (returns 1 or 0)                                                                                                                                                                                                                                                                                                                                                                                                 |                                                                  |
| `AccountTradeExpert`   | 6     | Allowed trade for an Expert Advisor (returns 1 or 0)                                                                                                                                                                                                                                                                                                                                                                                                   |                                                                  |
| `AccountMarginMode`    | 7     | Margin calculation mode — see `EnumAccountMarginMode` (not yet documented)                                                                                                                                                                                                                                                                                                                                                                             |                                                                  |
| `AccountCurrencyDigits`| 8     | The number of decimal places in the account currency, which are required for an accurate display of trading results                                                                                                                                                                                                                                                                                                                                    |                                                                  |
| `AccountFifoClose`     | 9     | An indication showing that positions can only be closed by FIFO rule. If the property value is set to true, then each symbol positions will be closed in the same order, in which they are opened, starting with the oldest one. In case of an attempt to close positions in a different order, the trader will receive an appropriate error (returns 1 or 0)                                                                                          |                                                                  |
| `AccountHedgeAllowed`  | 10    | Allowed opposite positions on a single symbol (hedging) - returns 1 or 0                                                                                                                                                                                                                                                                                                                                                                               |                                                                  |

### Related Enums (referenced above)

#### `MrpcEnumAccountTradeMode`

* `MrpcAccountTradeModeDemo = 0` — demo/practice
* `MrpcAccountTradeModeContest = 1` — contest
* `MrpcAccountTradeModeReal = 2` — real trading

> See [AccountSummary.md](AccountSummary.md) for full details.

---

## 💬 Just the essentials

* **What it is.** Single RPC returning one specific `long` property of the account.
* **Why you need it.** When you only need one integer property (e.g., leverage, login, trade mode) instead of fetching the full account summary.
* **Performance.** Lightweight call — ideal for quick checks of specific properties.
* **Type note.** All properties return `long` (int64), even boolean flags (1 = true, 0 = false).
* **Alternative.** Use `AccountSummaryAsync()` if you need multiple properties at once.

---

## 🎯 Purpose

Use this method when you need to:

* Check a single integer account property without fetching all account data.
* Verify account leverage before calculating position sizes.
* Check if trading is allowed (`AccountTradeAllowed`, `AccountTradeExpert`).
* Determine account trade mode (demo/contest/real).
* Check position limits (`AccountLimitOrders`).
* Verify FIFO close rules or hedging permissions.

---

## 🧩 Notes & Tips

* Prefer `AccountSummaryAsync()` if you need multiple properties — it's more efficient to fetch all data in one call.
* Boolean properties return `1` (true) or `0` (false) as `long` values.
* Use short per‑call timeout (3–5s) with retries when checking permissions before trading operations.
* All MT5Account methods have built-in protection against transient gRPC errors with automatic reconnection.
* The method returns `long` directly (not wrapped in a proto message) for convenience.

---

## 🔗 Usage Examples

### 1) Get account login

```csharp
// Retrieve account login number
var login = await acct.AccountInfoIntegerAsync(
    AccountInfoIntegerPropertyType.AccountLogin,
    deadline: DateTime.UtcNow.AddSeconds(3));
Console.WriteLine($"Account login: {login}");
```

### 2) Check account leverage

```csharp
// Get current leverage setting
var leverage = await acct.AccountInfoIntegerAsync(
    AccountInfoIntegerPropertyType.AccountLeverage);
Console.WriteLine($"Account leverage: 1:{leverage}");
```

### 3) Verify trading is allowed

```csharp
// Check if trading is enabled for this account
var tradeAllowed = await acct.AccountInfoIntegerAsync(
    AccountInfoIntegerPropertyType.AccountTradeAllowed);

if (tradeAllowed == 0)
{
    Console.WriteLine("⚠️ Trading is disabled for this account");
    return;
}

// Check if EA trading is allowed
var expertAllowed = await acct.AccountInfoIntegerAsync(
    AccountInfoIntegerPropertyType.AccountTradeExpert);

if (expertAllowed == 0)
{
    Console.WriteLine("⚠️ Expert Advisor trading is disabled");
}
```

### 4) Get account trade mode

```csharp
// Determine if account is demo, contest, or real
var tradeMode = await acct.AccountInfoIntegerAsync(
    AccountInfoIntegerPropertyType.AccountTradeMode);

var modeLabel = tradeMode switch
{
    0 => "Demo",
    1 => "Contest",
    2 => "Real",
    _ => "Unknown"
};

Console.WriteLine($"Account mode: {modeLabel}");
```

### 5) Check position limits

```csharp
// Get maximum allowed positions + pending orders
var limitOrders = await acct.AccountInfoIntegerAsync(
    AccountInfoIntegerPropertyType.AccountLimitOrders);

if (limitOrders == 0)
{
    Console.WriteLine("✅ No position limits");
}
else
{
    Console.WriteLine($"⚠️ Max positions + pending orders: {limitOrders}");
}
```
