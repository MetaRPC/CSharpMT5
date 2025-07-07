# Getting an Integer Account Property

> **Request:** integer account property from MT5 (e.g., leverage)

Fetch any integer-precision account property as a long value.

### Code Example

```csharp
var leverage = await _mt5Account.AccountInfoIntegerAsync(
    AccountInfoIntegerPropertyType.AccountLeverage
);
_logger.LogInformation($"AccountInfoInteger: Leverage={leverage}");
```

✨ **Method Signature:**

```csharp
Task<long> AccountInfoIntegerAsync(AccountInfoIntegerPropertyType property)
```

---

## Input

**property** (`AccountInfoIntegerPropertyType`): enumeration value indicating which integer account property to fetch. Available values: ([mql5.com](https://www.mql5.com/en/docs/constants/environment_state/accountinformation))

* **AccountLogin** (`ACCOUNT_LOGIN`) — account number (long)
* **AccountTradeMode** (`ACCOUNT_TRADE_MODE`) — account trade mode (`AccountTradeMode` enum)
* **AccountLeverage** (`ACCOUNT_LEVERAGE`) — account leverage (long)
* **AccountLimitOrders** (`ACCOUNT_LIMIT_ORDERS`) — maximum allowed number of active pending orders (int)
* **AccountMarginSoMode** (`ACCOUNT_MARGIN_SO_MODE`) — mode for setting the minimal allowed margin (`StopoutMode` enum)
* **AccountTradeAllowed** (`ACCOUNT_TRADE_ALLOWED`) — trade allowed for the current account (bool)
* **AccountTradeExpert** (`ACCOUNT_TRADE_EXPERT`) — trade allowed for an Expert Advisor (bool)
* **AccountMarginMode** (`ACCOUNT_MARGIN_MODE`) — margin calculation mode (`AccountMarginMode` enum)
* **AccountCurrencyDigits** (`ACCOUNT_CURRENCY_DIGITS`) — number of decimal places in account currency (int)
* **AccountFifoClose** (`ACCOUNT_FIFO_CLOSE`) — positions can only be closed by FIFO rule (bool)
* **AccountHedgeAllowed** (`ACCOUNT_HEDGE_ALLOWED`) — allowed opposite positions on a single symbol (bool)

---

## Output

* `long` — the requested numeric value (e.g., `100`).

---

## Purpose

Keep your code concise and future-ready by using one universal method to retrieve any integer account property — simply swap the enum value! 🚀
