# MT5Service - Account Convenience Methods

> 10 convenient methods for quick access to account properties without `.Value` unwrapping

---

## 🎯 Why These Methods Exist

**Problem**: In MT5Account, every call returns a `Data` object that needs unwrapping:
```csharp
var data = await account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountBalance);
double balance = data.Value;  // ← unwrapping required!
```

**Solution**: MT5Service provides direct methods that return values immediately:
```csharp
double balance = await service.GetBalanceAsync();  // ✅ ready!
```

**Benefits:**

- ✅ Less code (1 line instead of 2)
- ✅ No `.Value` ceremony
- ✅ Readable method names (`GetBalance` vs `AccountInfoDouble(BALANCE)`)
- ✅ Fewer errors (can't forget `.Value`)

---

## 📋 All 10 Methods

| Method | Returns | Low-Level Equivalent |
|--------|---------|---------------------|
| `GetBalanceAsync()` | Account balance | `AccountInfoDoubleAsync(AccountBalance)` |
| `GetEquityAsync()` | Equity (balance + floating P/L) | `AccountInfoDoubleAsync(AccountEquity)` |
| `GetMarginAsync()` | Used margin | `AccountInfoDoubleAsync(AccountMargin)` |
| `GetFreeMarginAsync()` | Free margin | `AccountInfoDoubleAsync(AccountMarginFree)` |
| `GetProfitAsync()` | Current floating profit/loss | `AccountInfoDoubleAsync(AccountProfit)` |
| `GetLoginAsync()` | Account number (login) | `AccountInfoIntegerAsync(AccountLogin)` |
| `GetLeverageAsync()` | Leverage ratio | `AccountInfoIntegerAsync(AccountLeverage)` |
| `GetAccountNameAsync()` | Account owner name | `AccountInfoStringAsync(AccountName)` |
| `GetServerNameAsync()` | MT5 server name | `AccountInfoStringAsync(AccountServer)` |
| `GetCurrencyAsync()` | Account currency | `AccountInfoStringAsync(AccountCurrency)` |

---

## 💡 Usage Examples

### Example 1: Checking Balance and Equity

```csharp
// ❌ BEFORE (MT5Account) - 4 lines:
var balanceData = await account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountBalance);
double balance = balanceData.Value;
var equityData = await account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountEquity);
double equity = equityData.Value;

// ✅ AFTER (MT5Service) - 2 lines:
double balance = await service.GetBalanceAsync();
double equity = await service.GetEquityAsync();

Console.WriteLine($"Balance: ${balance:F2}, Equity: ${equity:F2}");
```

---

### Example 2: Calculating Margin Level

```csharp
// ❌ BEFORE (MT5Account) - 6 lines with unwrapping:
var equityData = await account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountEquity);
double equity = equityData.Value;
var marginData = await account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountMargin);
double margin = marginData.Value;
double marginLevel = (margin > 0) ? (equity / margin * 100) : 0;
Console.WriteLine($"Margin Level: {marginLevel:F2}%");

// ✅ AFTER (MT5Service) - 3 lines, no ceremony:
double equity = await service.GetEquityAsync();
double margin = await service.GetMarginAsync();
double marginLevel = (margin > 0) ? (equity / margin * 100) : 0;
Console.WriteLine($"Margin Level: {marginLevel:F2}%");
```

**Code reduction: 50%** (6 lines → 3 lines)

---

### Example 3: Checking Free Margin Before Opening Position

```csharp
// ❌ BEFORE (MT5Account):
var freeMarginData = await account.AccountInfoDoubleAsync(
    AccountInfoDoublePropertyType.AccountMarginFree);
double freeMargin = freeMarginData.Value;

if (freeMargin >= 100.0)
{
    // Can open position
}

// ✅ AFTER (MT5Service):
double freeMargin = await service.GetFreeMarginAsync();

if (freeMargin >= 100.0)
{
    // Can open position
}
```

**Readability:** Intent is clear from the first line!

---

### Example 4: Displaying Account Information

```csharp
// ❌ BEFORE (MT5Account) - 12 lines:
var loginData = await account.AccountInfoIntegerAsync(AccountInfoIntegerPropertyType.AccountLogin);
long login = loginData.Value;
var nameData = await account.AccountInfoStringAsync(AccountInfoStringPropertyType.AccountName);
string name = nameData.Value;
var serverData = await account.AccountInfoStringAsync(AccountInfoStringPropertyType.AccountServer);
string server = serverData.Value;
var currencyData = await account.AccountInfoStringAsync(AccountInfoStringPropertyType.AccountCurrency);
string currency = currencyData.Value;
var leverageData = await account.AccountInfoIntegerAsync(AccountInfoIntegerPropertyType.AccountLeverage);
long leverage = leverageData.Value;

Console.WriteLine($"Account: {login} ({name})");
Console.WriteLine($"Server: {server}, Currency: {currency}, Leverage: 1:{leverage}");

// ✅ AFTER (MT5Service) - 6 lines:
long login = await service.GetLoginAsync();
string name = await service.GetAccountNameAsync();
string server = await service.GetServerNameAsync();
string currency = await service.GetCurrencyAsync();
long leverage = await service.GetLeverageAsync();

Console.WriteLine($"Account: {login} ({name})");
Console.WriteLine($"Server: {server}, Currency: {currency}, Leverage: 1:{leverage}");
```

**Code reduction: 50%** (12 lines → 6 lines)

---

### Example 5: Monitoring Current Profit

```csharp
// ❌ BEFORE (MT5Account):
var profitData = await account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountProfit);
double profit = profitData.Value;

if (profit > 0)
    Console.WriteLine($"✅ Profit: +${profit:F2}");
else
    Console.WriteLine($"❌ Loss: ${profit:F2}");

// ✅ AFTER (MT5Service):
double profit = await service.GetProfitAsync();

if (profit > 0)
    Console.WriteLine($"✅ Profit: +${profit:F2}");
else
    Console.WriteLine($"❌ Loss: ${profit:F2}");
```

**Clean:** One line for data retrieval, rest is business logic.

---

### Example 6: Parallel Retrieval of All Account Metrics

```csharp
// ✅ All methods are async - can run in parallel!
var balanceTask = service.GetBalanceAsync();
var equityTask = service.GetEquityAsync();
var marginTask = service.GetMarginAsync();
var freeMarginTask = service.GetFreeMarginAsync();
var profitTask = service.GetProfitAsync();

await Task.WhenAll(balanceTask, equityTask, marginTask, freeMarginTask, profitTask);

Console.WriteLine($"Balance:      ${balanceTask.Result:F2}");
Console.WriteLine($"Equity:       ${equityTask.Result:F2}");
Console.WriteLine($"Margin:       ${marginTask.Result:F2}");
Console.WriteLine($"Free Margin:  ${freeMarginTask.Result:F2}");
Console.WriteLine($"Profit:       ${profitTask.Result:F2}");
```

**Performance:** All requests execute concurrently!

---

### Example 7: Validating Account Before Trading

```csharp
// Check essential account parameters
var leverage = await service.GetLeverageAsync();
var currency = await service.GetCurrencyAsync();
var freeMargin = await service.GetFreeMarginAsync();

if (leverage < 100)
{
    Console.WriteLine("⚠️ Warning: Low leverage, large margin required!");
}

if (currency != "USD")
{
    Console.WriteLine($"ℹ️ Account currency: {currency} (not USD)");
}

if (freeMargin < 500.0)
{
    Console.WriteLine("❌ Not enough free margin to trade!");
    return;
}

Console.WriteLine("✅ Account ready for trading!");
```

---

## 🔑 Key Benefits

| Aspect | MT5Account (Low-Level) | MT5Service (Convenience) |
|--------|----------------------|-------------------------|
| **Code** | 2 lines per value | 1 line per value |
| **Readability** | `AccountInfoDoubleAsync(AccountBalance)` | `GetBalanceAsync()` |
| **Unwrapping** | Always need `.Value` | ❌ Not needed |
| **Errors** | Can forget `.Value` | ✅ Impossible |
| **Write Speed** | Slower | ✅ Faster |
| **Maintenance** | Harder | ✅ Easier |

---

## 📊 Code Reduction Statistics

| Task | Lines (MT5Account) | Lines (MT5Service) | Reduction |
|------|-------------------|--------------------|-----------|
| Get 1 value | 2 | 1 | **50%** |
| Get 5 values | 10 | 5 | **50%** |
| Display account info | 12 | 6 | **50%** |
| Calculate margin level | 6 | 3 | **50%** |

**Average reduction: 50%** for account operations!

---

## 🎓 When to Use

### ✅ Use MT5Service when:
- Need to quickly get account property value
- Writing trading bot or strategy
- Development speed matters
- Want clean, readable code

### ⚠️ Use MT5Account when:
- Need exotic properties (no convenience method available)
- Require full control over low-level API
- Building custom wrapper on top of MT5Account

---

## 🔗 See Also

* **[MT5Service Overview](./MT5Service.Overview.md)** - Complete description of MT5Service improvements
* **[Symbol Convenience Methods](./Symbol_Convenience_Methods.md)** - Convenient methods for symbols
* **[Trading Convenience Methods](./Trading_Convenience_Methods.md)** - Simplified trading methods
* **[MT5Account](../MT5Account/MT5Account.Master.Overview.md)** - Low-level API reference

---

## 💡 Summary

**10 methods** that make account operations **2x faster** and **2x cleaner**. No ceremony, no `.Value` - just what you need!

```csharp
// It's simple:
var balance = await service.GetBalanceAsync();
var equity = await service.GetEquityAsync();
var profit = await service.GetProfitAsync();

Console.WriteLine($"Balance: ${balance:F2}, Equity: ${equity:F2}, P/L: ${profit:F2}");
```

**Write less, do more!** 🚀
