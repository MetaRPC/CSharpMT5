# MT5Account · Account Information - Overview

> Account balance, equity, margin, leverage, currency, and other account properties. Use this page to choose the right API for accessing account state.

## 📁 What lives here

* **[AccountSummary](./AccountSummary.md)** - **all account info** at once (balance, equity, margin, leverage, profit, etc.).
* **[AccountInfoDouble](./AccountInfoDouble.md)** - **single double value** from account (balance, equity, margin, profit, credit, etc.).
* **[AccountInfoInteger](./AccountInfoInteger.md)** - **single integer value** from account (login, leverage, limit orders, etc.).
* **[AccountInfoString](./AccountInfoString.md)** - **single string value** from account (name, server, currency, company).

---

## 🧭 Plain English

* **AccountSummary** → the **one-stop shop** for complete account snapshot (balance, equity, margin level, etc.).
* **AccountInfoDouble** → grab **one numeric property** when you need just balance or margin.
* **AccountInfoInteger** → grab **one integer property** like login number or leverage.
* **AccountInfoString** → grab **one text property** like account name or currency.

> Rule of thumb: need **full snapshot** → `AccountSummaryAsync`; need **one specific value** → `AccountInfo*Async` (Double/Integer/String).

---

## Quick choose

| If you need…                                     | Use                       | Returns                    | Key inputs                          |
| ------------------------------------------------ | ------------------------- | -------------------------- | ----------------------------------- |
| Complete account snapshot (all values)           | `AccountSummaryAsync`     | Full account data object   | *(none)*                            |
| One numeric value (balance, equity, margin, etc.)| `AccountInfoDoubleAsync`  | Single `double`            | Property enum (BALANCE, EQUITY, etc.) |
| One integer value (login, leverage, etc.)        | `AccountInfoIntegerAsync` | Single `long`              | Property enum (LOGIN, LEVERAGE, etc.) |
| One text value (name, currency, server, etc.)    | `AccountInfoStringAsync`  | Single `string`            | Property enum (NAME, CURRENCY, etc.) |

---

## ❌ Cross‑refs & gotchas

* **Margin Level** = (Equity / Margin) × 100 - watch for stop-out level.
* **Free Margin** = Equity - Margin - available for new positions.
* **AccountSummary** includes everything; use it for dashboards.
* **AccountInfo*** methods are lighter if you only need one property.
* **Currency** affects how profits are calculated - always check account currency.
* **Leverage** determines margin requirements - higher leverage = less margin needed.

---

## 🟢 Minimal snippets

```csharp
// Get complete account snapshot
var summary = await account.AccountSummaryAsync();
Console.WriteLine($"Balance: ${summary.AccountBalance:F2}, Equity: ${summary.AccountEquity:F2}, Leverage: 1:{summary.AccountLeverage}");
```

```csharp
// Get single property - account balance
var balance = await account.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.AccountBalance
);
Console.WriteLine($"Balance: ${balance:F2}");
```

```csharp
// Get account leverage
var leverage = await account.AccountInfoIntegerAsync(
    AccountInfoIntegerPropertyType.AccountLeverage
);
Console.WriteLine($"Leverage: 1:{leverage}");
```

```csharp
// Get account currency
var currency = await account.AccountInfoStringAsync(
    AccountInfoStringPropertyType.AccountCurrency
);
Console.WriteLine($"Currency: {currency}");
```

```csharp
// Check account balance and equity
var summary = await account.AccountSummaryAsync();
var balance = summary.AccountBalance;
var equity = summary.AccountEquity;
if (equity < balance * 0.8)
{
    Console.WriteLine("⚠️ Warning: Equity below 80% of balance!");
}
```

---

## See also

* **Streaming:** [SubscribeToPositionProfit](../7.%20Streaming_Methods/SubscribeToPositionProfit.md) - real-time equity/profit updates
* **Trading calculations:** [OrderCalcMargin](../4.%20Trading_Operattons/OrderCalcMargin.md) - calculate required margin before trading
* **Positions:** [PositionsTotal](../3.%20Position_Orders_Information/PositionsTotal.md) - count open positions
