# ✅ Get Account Snapshot (`GetAccountSnapshot`)

> **Sugar method:** Gets account summary and opened orders in one convenient call. Returns both as a single record.

**API Information:**

* **Extension method:** `MT5Service.GetAccountSnapshot(...)` (from `MT5ServiceExtensions`)
* **Package:** Part of `mt5_term_api` library
* **Underlying calls:** `AccountSummaryAsync()` + `OpenedOrdersAsync()`

### Method Signature

```csharp
public static class MT5ServiceExtensions
{
    public sealed record AccountSnapshot(
        AccountSummaryData Summary,
        OpenedOrdersData OpenedOrders
    );

    public static async Task<AccountSnapshot> GetAccountSnapshot(
        this MT5Service svc,
        int timeoutSec = 15,
        CancellationToken ct = default);
}
```

---

## 🔽 Input

| Parameter    | Type                | Description                                     |
| ------------ | ------------------- | ----------------------------------------------- |
| `svc`        | `MT5Service`        | MT5Service instance (extension method)          |
| `timeoutSec` | `int`               | Timeout in seconds (default: 15)                |
| `ct`         | `CancellationToken` | Cancellation token                              |

---

## ⬆️ Output — `AccountSnapshot`

| Field         | Type                  | Description                           |
| ------------- | --------------------- | ------------------------------------- |
| `Summary`     | `AccountSummaryData`  | Account summary (balance, equity, etc) |
| `OpenedOrders`| `OpenedOrdersData`    | All opened orders and positions        |

### `AccountSummaryData` — Account information

| Field                             | Type                          | Description                     |
| --------------------------------- | ----------------------------- | ------------------------------- |
| `AccountLogin`                    | `int64`                       | Account login number            |
| `AccountBalance`                  | `double`                      | Account balance                 |
| `AccountEquity`                   | `double`                      | Account equity                  |
| `AccountUserName`                 | `string`                      | Account holder name             |
| `AccountLeverage`                 | `int64`                       | Account leverage                |
| `AccountTradeMode`                | `MrpcEnumAccountTradeMode`    | Trade mode (Demo/Contest/Real)  |
| `AccountCompanyName`              | `string`                      | Broker company name             |
| `AccountCurrency`                 | `string`                      | Account currency                |
| `ServerTime`                      | `Timestamp`                   | Server time                     |
| `UtcTimezoneServerTimeShiftMinutes` | `int64`                     | Server time UTC offset          |
| `AccountCredit`                   | `double`                      | Account credit                  |

### `OpenedOrdersData` — Opened orders and positions

| Field         | Type                        | Description                       |
| ------------- | --------------------------- | --------------------------------- |
| `OpenedOrders`| `List<OpenedOrderInfo>`     | Array of pending orders           |
| `PositionInfos`| `List<PositionInfo>`       | Array of open positions           |

---

## 💬 Just the essentials

* **What it is.** One-liner to get complete account state: balance, equity, and all active positions/orders.
* **Why you need it.** Avoid two separate API calls. Get everything needed for dashboards, risk checks, position monitoring.
* **Sanity check.** Returns record with `Summary` (account data) and `OpenedOrders` (positions/orders). Check counts and balances.

---

## 🎯 Purpose

Use it for account monitoring:

* Display account dashboard.
* Check account balance and equity before trading.
* Get list of active positions and pending orders.
* Calculate risk metrics (margin level, exposure).
* Quick account health check.

---

## 🧩 Notes & Tips

* **Single call:** Combines two API calls internally but feels like one operation.
* **Atomic snapshot:** Both calls use same deadline, so data is relatively synchronized.
* **Sorted orders:** Orders sorted by open time (ascending) by default.
* **Timeout:** 15 seconds default is usually enough for both calls.
* **Record type:** Returns immutable `AccountSnapshot` record with two properties.
* **No filtering:** Returns all opened orders/positions. Filter in your code if needed.

---

## 🔧 Under the Hood

This sugar method combines two low-level calls:

```csharp
var deadline = DateTime.UtcNow.AddSeconds(timeoutSec);

// Step 1: Get account summary (balance, equity, leverage, etc)
var summary = await svc.AccountSummaryAsync(deadline, ct);

// Step 2: Get all opened orders and positions
var openedOrders = await svc.OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, deadline, ct);

// Step 3: Return as single record
return new AccountSnapshot(summary, openedOrders);
```

**What it improves:**

* **Combines 2 calls into 1** - simpler API
* **Shared deadline** - both calls use same timeout
* **Single return type** - immutable record with both results
* **Clearer intent** - "get account snapshot" vs "get summary + get orders"

---

## 📊 Low-Level Alternative

**WITHOUT sugar (manual approach):**
```csharp
// You have to do this manually:
var deadline = DateTime.UtcNow.AddSeconds(15);

// Call 1: Get account summary
var summary = await svc.AccountSummaryAsync(deadline, ct);

// Call 2: Get opened orders
var openedOrders = await svc.OpenedOrdersAsync(
    BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, deadline, ct);

// Now you have two separate objects to work with
Console.WriteLine($"Balance: {summary.AccountBalance}");
Console.WriteLine($"Positions: {openedOrders.PositionInfos.Count}");
```

**WITH sugar (one-liner):**
```csharp
// Sugar method does all of the above:
var snapshot = await svc.GetAccountSnapshot();

// Work with single object:
Console.WriteLine($"Balance: {snapshot.Summary.AccountBalance}");
Console.WriteLine($"Positions: {snapshot.OpenedOrders.PositionInfos.Count}");
```

**Benefits:**

* ✅ **2 calls → 1 call**
* ✅ **Automatic deadline management**
* ✅ **Single object to work with**
* ✅ **Guaranteed consistency** (both calls use same deadline)

---

## 🔗 Usage Examples

### 1) Basic account snapshot

```csharp
// svc — MT5Service instance

var snapshot = await svc.GetAccountSnapshot();

Console.WriteLine("Account Summary:");
Console.WriteLine($"  Balance: ${snapshot.Summary.AccountBalance:F2}");
Console.WriteLine($"  Equity:  ${snapshot.Summary.AccountEquity:F2}");
Console.WriteLine($"  Leverage: 1:{snapshot.Summary.AccountLeverage}");
Console.WriteLine($"  Currency: {snapshot.Summary.AccountCurrency}");
Console.WriteLine();

Console.WriteLine($"Active Positions: {snapshot.OpenedOrders.PositionInfos.Count}");
Console.WriteLine($"Pending Orders:   {snapshot.OpenedOrders.OpenedOrders.Count}");
```

---

### 2) Display all positions

```csharp
var snapshot = await svc.GetAccountSnapshot();

Console.WriteLine("Open Positions:");
Console.WriteLine("────────────────────────────────────────────────");

foreach (var pos in snapshot.OpenedOrders.PositionInfos)
{
    var type = pos.Type == 0 ? "BUY" : "SELL";
    var profitSign = pos.Profit >= 0 ? "+" : "";

    Console.WriteLine($"#{pos.Ticket} {type} {pos.Symbol}");
    Console.WriteLine($"  Volume: {pos.Volume} lots");
    Console.WriteLine($"  Entry:  {pos.PriceOpen}");
    Console.WriteLine($"  P/L:    {profitSign}${pos.Profit:F2}");
    Console.WriteLine();
}
```

---

### 3) Calculate total exposure

```csharp
var snapshot = await svc.GetAccountSnapshot();

var totalProfit = snapshot.OpenedOrders.PositionInfos.Sum(p => p.Profit);
var totalVolume = snapshot.OpenedOrders.PositionInfos.Sum(p => p.Volume);

Console.WriteLine($"Account Equity:  ${snapshot.Summary.AccountEquity:F2}");
Console.WriteLine($"Total Positions: {snapshot.OpenedOrders.PositionInfos.Count}");
Console.WriteLine($"Total Volume:    {totalVolume:F2} lots");
Console.WriteLine($"Total P/L:       ${totalProfit:F2}");

var returnPct = (totalProfit / snapshot.Summary.AccountBalance) * 100;
Console.WriteLine($"Return:          {returnPct:F2}%");
```

---

### 4) Check if account can trade

```csharp
var snapshot = await svc.GetAccountSnapshot();

var balance = snapshot.Summary.AccountBalance;
var equity = snapshot.Summary.AccountEquity;
var openPositions = snapshot.OpenedOrders.PositionInfos.Count;

bool canTrade = true;
List<string> warnings = new();

if (equity < balance * 0.5)
{
    warnings.Add($"Low equity: ${equity:F2} (50% of balance)");
    canTrade = false;
}

if (openPositions >= 10)
{
    warnings.Add($"Too many positions: {openPositions}");
    canTrade = false;
}

if (canTrade)
{
    Console.WriteLine("✓ Account OK for trading");
}
else
{
    Console.WriteLine("⚠ Trading not recommended:");
    foreach (var warning in warnings)
    {
        Console.WriteLine($"  • {warning}");
    }
}
```

---

### 5) Group positions by symbol

```csharp
var snapshot = await svc.GetAccountSnapshot();

var positionsBySymbol = snapshot.OpenedOrders.PositionInfos
    .GroupBy(p => p.Symbol)
    .Select(g => new
    {
        Symbol = g.Key,
        Count = g.Count(),
        TotalVolume = g.Sum(p => p.Volume),
        TotalProfit = g.Sum(p => p.Profit)
    })
    .OrderByDescending(x => x.TotalVolume);

Console.WriteLine("Positions by Symbol:");
Console.WriteLine("────────────────────────────────────────────");

foreach (var group in positionsBySymbol)
{
    Console.WriteLine($"{group.Symbol,-10} Count: {group.Count,2} | " +
                     $"Volume: {group.TotalVolume,6:F2} | " +
                     $"P/L: ${group.TotalProfit,8:F2}");
}
```

---

### 6) Risk dashboard

```csharp
var snapshot = await svc.GetAccountSnapshot();

var balance = snapshot.Summary.AccountBalance;
var equity = snapshot.Summary.AccountEquity;
var totalProfit = snapshot.OpenedOrders.PositionInfos.Sum(p => p.Profit);
var totalSwap = snapshot.OpenedOrders.PositionInfos.Sum(p => p.Swap);

Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("          RISK DASHBOARD                   ");
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine($"Balance:       ${balance,12:F2}");
Console.WriteLine($"Equity:        ${equity,12:F2}");
Console.WriteLine($"Floating P/L:  ${totalProfit,12:F2}");
Console.WriteLine($"Swap:          ${totalSwap,12:F2}");
Console.WriteLine();
Console.WriteLine($"Positions:     {snapshot.OpenedOrders.PositionInfos.Count,12}");
Console.WriteLine($"Pending:       {snapshot.OpenedOrders.OpenedOrders.Count,12}");
Console.WriteLine();

var drawdown = balance - equity;
var drawdownPct = (drawdown / balance) * 100;

if (drawdownPct > 0)
{
    Console.WriteLine($"Drawdown:      ${drawdown,12:F2} ({drawdownPct:F2}%)");
}
else
{
    var profit = equity - balance;
    var profitPct = (profit / balance) * 100;
    Console.WriteLine($"Profit:        ${profit,12:F2} ({profitPct:F2}%)");
}
```

---

### 7) Monitor positions in loop

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromMinutes(1)); // Monitor for 1 minute

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        var snapshot = await svc.GetAccountSnapshot(ct: cts.Token);

        Console.Clear();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Account Monitor");
        Console.WriteLine("─────────────────────────────────────────");
        Console.WriteLine($"Equity: ${snapshot.Summary.AccountEquity:F2}");
        Console.WriteLine($"Positions: {snapshot.OpenedOrders.PositionInfos.Count}");

        foreach (var pos in snapshot.OpenedOrders.PositionInfos.Take(5))
        {
            var sign = pos.Profit >= 0 ? "+" : "";
            Console.WriteLine($"  {pos.Symbol}: {sign}${pos.Profit:F2}");
        }

        await Task.Delay(2000, cts.Token); // Update every 2 seconds
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nMonitoring stopped");
}
```

---

### 8) Check account mode (Demo/Real)

```csharp
var snapshot = await svc.GetAccountSnapshot();

var tradeMode = snapshot.Summary.AccountTradeMode;

string modeText = tradeMode switch
{
    MrpcEnumAccountTradeMode.MrpcAccountTradeModeDemo => "🔵 DEMO",
    MrpcEnumAccountTradeModeReal => "🔴 REAL",
    MrpcEnumAccountTradeModeContest => "🏆 CONTEST",
    _ => "❓ UNKNOWN"
};

Console.WriteLine($"Account Mode: {modeText}");
Console.WriteLine($"Login: {snapshot.Summary.AccountLogin}");
Console.WriteLine($"Broker: {snapshot.Summary.AccountCompanyName}");
Console.WriteLine($"Name: {snapshot.Summary.AccountUserName}");

if (tradeMode == MrpcEnumAccountTradeMode.MrpcAccountTradeModeReal)
{
    Console.WriteLine("\n⚠️  WARNING: Trading on REAL account!");
}
```

---

### 9) Export to JSON

```csharp
using System.Text.Json;

var snapshot = await svc.GetAccountSnapshot();

var report = new
{
    Timestamp = DateTime.UtcNow,
    Account = new
    {
        snapshot.Summary.AccountLogin,
        snapshot.Summary.AccountBalance,
        snapshot.Summary.AccountEquity,
        snapshot.Summary.AccountCurrency
    },
    Positions = snapshot.OpenedOrders.PositionInfos.Select(p => new
    {
        p.Ticket,
        p.Symbol,
        Type = p.Type == 0 ? "BUY" : "SELL",
        p.Volume,
        p.PriceOpen,
        p.Profit
    })
};

var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
{
    WriteIndented = true
});

File.WriteAllText("account_snapshot.json", json);
Console.WriteLine("Snapshot exported to account_snapshot.json");
```

---

### 10) Compare with previous snapshot

```csharp
AccountSnapshot? previous = null;

while (true)
{
    var current = await svc.GetAccountSnapshot();

    if (previous != null)
    {
        var balanceChange = current.Summary.AccountBalance - previous.Summary.AccountBalance;
        var equityChange = current.Summary.AccountEquity - previous.Summary.AccountEquity;
        var positionChange = current.OpenedOrders.PositionInfos.Count - previous.OpenedOrders.PositionInfos.Count;

        if (balanceChange != 0)
            Console.WriteLine($"Balance: {balanceChange:+0.00;-0.00}");

        if (equityChange != 0)
            Console.WriteLine($"Equity: {equityChange:+0.00;-0.00}");

        if (positionChange != 0)
            Console.WriteLine($"Positions: {positionChange:+0;-0}");
    }

    previous = current;
    await Task.Delay(5000); // Check every 5 seconds
}
```

---

## 🔗 Related Methods

**📦 Low-level methods used internally:**

* `AccountSummaryAsync()` - Get account summary (balance, equity, leverage, etc) - step 1
* `OpenedOrdersAsync()` - Get all opened orders and positions - step 2

**🍬 Other sugar methods:**

* `GetSymbolSnapshot()` - Get symbol data snapshot (tick, point, digits, margin)
* All trading methods need account data for risk checks - can use this snapshot
