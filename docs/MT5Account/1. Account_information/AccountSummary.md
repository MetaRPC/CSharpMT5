# ✅ Getting an Account Summary

> **Request:** full account summary (`AccountSummaryData`) from **MT5**. Fetch all core account metrics in a single call.

**API Information:**

* **SDK wrapper:** `MT5Account.AccountSummaryAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.AccountHelper`
* **Proto definition:** `AccountSummary` (defined in `mt5-term-api-account-helper.proto`)

### RPC

* **Service:** `mt5_term_api.AccountHelper`
* **Method:** `AccountSummary(AccountSummaryRequest) → AccountSummaryReply`
* **Low‑level client (generated):** `AccountHelper.AccountSummaryAsync(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace mt5_term_api
{
    public class MT5Account
    {
        public async Task<AccountSummaryData> AccountSummaryAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`AccountSummaryRequest {}`


**Reply message:**

`AccountSummaryReply { data: AccountSummaryData }`

---

## 🔽 Input

No required parameters.

| Parameter           | Type                | Description                                               |
| ------------------- | ------------------- | --------------------------------------------------------- |
| `deadline`          | `DateTime?`         | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken` | Cooperative cancel for the call/retry loop                |

---

## ⬆️ Output — `AccountSummaryData`

| Field                                | Type         | Description                              |
| ------------------------------------ | ------------ | ---------------------------------------- |
| `AccountLogin`                       | `long`       | Trading account login (ID).              |
| `AccountBalance`                     | `double`     | Balance excluding floating P/L.          |
| `AccountEquity`                      | `double`     | Equity = balance + floating P/L.         |
| `AccountLeverage`                    | `long`       | Leverage (e.g., `100` for 1:100).        |
| `AccountTradeMode`                   | `int (enum)` | See `MrpcEnumAccountTradeMode` below.    |
| `AccountCurrency`                    | `string`     | Deposit currency (e.g., `USD`).          |
| `AccountCompanyName`                 | `string`     | Broker/company display name.             |
| `AccountUserName`                    | `string`     | Account holder name.                     |
| `ServerTime`                         | `Timestamp`  | Server time (UTC) at response.           |
| `UtcTimezoneServerTimeShiftMinutes`  | `long`       | Server offset relative to UTC (minutes). |
| `AccountCredit`                      | `double`     | Credit amount.                           |

---

## 🧱 Related enums (from proto)

### `MrpcEnumAccountTradeMode`

* `MRPC_ACCOUNT_TRADE_MODE_DEMO = 0` — demo/practice
* `MRPC_ACCOUNT_TRADE_MODE_CONTEST = 1` — contest
* `MRPC_ACCOUNT_TRADE_MODE_REAL = 2` — real trading

> Map enum → label in UI via your helper, e.g. a `switch` or generated `Name(value)`.

---

## 💬 Just the essentials

* **What it is.** Single RPC returning account state: balance, equity, currency, leverage, trade mode, server time.
* **Why you need it.** Fast dashboard/CLI status; double‑check login/currency/leverage; heartbeat via `ServerTimeUtc`.
* **Sanity check.** If you see `AccountLogin`, `AccountCurrency`, `AccountLeverage`, `AccountEquity` → connection is alive.

---

## 🎯 Purpose

Use it to display real‑time account state and sanity‑check connectivity:

* Dashboard/CLI status in one call.
* Verify free‑margin & equity before trading.
* Terminal heartbeat via `ServerTimeUtc` and `UtcServerTimeShiftMinutes`.

---

## 🧩 Notes & Tips

* Prefer a short per‑call timeout (3–5s) with retries when the terminal is warming up/syncing.
* All MT5Account methods have built-in protection against transient gRPC errors with automatic reconnection.
* Convert enum codes to labels as close to UI as possible.

---

## 🔗 Usage Examples

### 1) Per‑call deadline

```csharp
// Enforce a short absolute UTC deadline to avoid hanging calls
var summary = await acct.AccountSummaryAsync(
    deadline: DateTime.UtcNow.AddSeconds(3));
Console.WriteLine($"[deadline] Equity={summary.AccountEquity:F2}");
```

### 2) Cooperative cancellation (with CancellationToken)

```csharp
// Allow a graceful stop from another task
using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(3));
var s2 = await acct.AccountSummaryAsync(
    deadline: DateTime.UtcNow.AddSeconds(3),
    cancellationToken: cts.Token);
Console.WriteLine($"[cancel] Currency={s2.AccountCurrency}");
```

### 3) Compact status line for UI/CLI

```csharp
var s = await acct.AccountSummaryAsync();
var status = $"Acc {s.AccountLogin} | {s.AccountCurrency} | " +
             $"Bal {s.AccountBalance:F2} | Eq {s.AccountEquity:F2} | " +
             $"Lev {s.AccountLeverage} | Mode {s.AccountTradeMode}";
Console.WriteLine(status);
```

### 4) Human‑readable server time with timezone shift

```csharp
var x = await acct.AccountSummaryAsync();
var serverUtc = x.ServerTime.ToDateTime(); // generated Timestamp → DateTime
var shift = TimeSpan.FromMinutes(x.UtcTimezoneServerTimeShiftMinutes);
var serverLocal = serverUtc + shift;
Console.WriteLine($"Server time: {serverLocal:O} (shift {shift})");
```

### 5) Map proto → thin view‑model

```csharp
// Keep only what UI needs; fast and test‑friendly
public record AccountSummaryView(
    long Login,
    string Currency,
    double Balance,
    double Equity,
    long Leverage,
    int Mode)
{
    public static AccountSummaryView FromProto(AccountSummaryData p) =>
        new(
            Login: p.AccountLogin,
            Currency: p.AccountCurrency,
            Balance: p.AccountBalance,
            Equity: p.AccountEquity,
            Leverage: p.AccountLeverage,
            Mode: p.AccountTradeMode
        );
}

var proto = await acct.AccountSummaryAsync();
var view = AccountSummaryView.FromProto(proto);
Console.WriteLine(view);
```
