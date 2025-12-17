# ✅ Get Margin Requirements (`SymbolInfoMarginRateAsync`)

> **Request:** Get margin rates (initial and maintenance) for a symbol and order type on **MT5**.

**API Information:**

* **SDK wrapper:** `MT5Account.SymbolInfoMarginRateAsync(...)` (from NuGet package `MetaRPC.MT5`)
* **gRPC service:** `mt5_term_api.MarketInfo`
* **Proto definition:** `SymbolInfoMarginRate` (defined in `mt5-term-api-market-info.proto`)

### RPC

* **Service:** `mt5_term_api.MarketInfo`
* **Method:** `SymbolInfoMarginRate(SymbolInfoMarginRateRequest) → SymbolInfoMarginRateReply`
* **Low‑level client (generated):** `MarketInfo.MarketInfoClient.SymbolInfoMarginRate(request, headers, deadline, cancellationToken)`
* **SDK wrapper (your class):**

```csharp
namespace mt5_term_api
{
    public class MT5Account
    {
        public async Task<SymbolInfoMarginRateData> SymbolInfoMarginRateAsync(
            SymbolInfoMarginRateRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default);
    }
}
```

**Request message:**

`SymbolInfoMarginRateRequest { symbol, order_type }`


**Reply message:**

`SymbolInfoMarginRateReply { data: SymbolInfoMarginRateData }`

---

## 🔽 Input

| Parameter           | Type                           | Description                                               |
| ------------------- | ------------------------------ | --------------------------------------------------------- |
| `request`           | `SymbolInfoMarginRateRequest`  | Protobuf request with symbol and order type              |
| `deadline`          | `DateTime?`                    | Absolute per‑call **UTC** deadline → converted to timeout |
| `cancellationToken` | `CancellationToken`            | Cooperative cancel for the call/retry loop                |

### `SymbolInfoMarginRateRequest`

| Field        | Type              | Description                                              |
| ------------ | ----------------- | -------------------------------------------------------- |
| `Symbol`     | `string`          | Symbol name (e.g., `"EURUSD"`, `"XAUUSD"`) **REQUIRED**  |
| `OrderType`  | `ENUM_ORDER_TYPE` | Order type (Buy, Sell, BuyLimit, etc.) **REQUIRED**       |

---

## ⬆️ Output — `SymbolInfoMarginRateData`

| Field                     | Type     | Description                                                           |
| ------------------------- | -------- | --------------------------------------------------------------------- |
| `MaintenanceMarginRate`   | `double` | Maintenance margin rate (minimum to maintain 1 lot open position)     |
| `InitialMarginRate`       | `double` | Initial margin rate (security deposit for 1 lot deal)                 |

---

## 🧱 Related enums (from proto)

### `ENUM_ORDER_TYPE`

* `OrderTypeBuy` — Market Buy order
* `OrderTypeSell` — Market Sell order
* `OrderTypeBuyLimit` — Buy Limit pending order
* `OrderTypeSellLimit` — Sell Limit pending order
* `OrderTypeBuyStop` — Buy Stop pending order
* `OrderTypeSellStop` — Sell Stop pending order
* `OrderTypeBuyStopLimit` — Buy Stop Limit pending order
* `OrderTypeSellStopLimit` — Sell Stop Limit pending order
* `OrderTypeCloseBy` — Order to close position by opposite one

---

## 💬 Just the essentials

* **What it is.** Returns margin rates for opening and maintaining positions. Rates are multipliers for calculating required margin.
* **Why you need it.** Calculate required margin before trading, implement risk management, validate available margin for new positions.
* **Sanity check.** If `InitialMarginRate > 0` → valid margin data. Multiply by symbol's initial/maintenance margin to get actual margin requirement.

---

## 🎯 Purpose

Use it to calculate margin requirements:

* Determine margin needed to open a position.
* Calculate maintenance margin for risk management.
* Validate if account has sufficient margin.
* Compare margin requirements across symbols/order types.

---

## 🧩 Notes & Tips

* **Margin calculation:** Actual margin = Rate × Symbol's InitialMargin (or MaintenanceMargin).
* **Order type matters:** Margin rates can differ between Buy and Sell for some symbols (especially exotic pairs).
* **Initial vs Maintenance:** Initial margin is locked when opening. Maintenance margin determines when stop-out occurs.
* **Symbol-specific:** Different symbols have different base margin values. This method returns the rate multiplier.
* **Use with SymbolInfo:** Combine with `SymbolInfoDouble` to get symbol's base margin values.

---

## 🔗 Usage Examples

### 1) Basic margin rate retrieval

```csharp
// acc — connected MT5Account

var result = await acc.SymbolInfoMarginRateAsync(new SymbolInfoMarginRateRequest
{
    Symbol = "EURUSD",
    OrderType = ENUM_ORDER_TYPE.OrderTypeBuy
});

Console.WriteLine($"Initial margin rate: {result.InitialMarginRate}");
Console.WriteLine($"Maintenance margin rate: {result.MaintenanceMarginRate}");
```

---

### 2) Calculate required margin for position

```csharp
var symbol = "XAUUSD";
var lots = 0.10;

// Get margin rates
var marginRates = await acc.SymbolInfoMarginRateAsync(new SymbolInfoMarginRateRequest
{
    Symbol = symbol,
    OrderType = ENUM_ORDER_TYPE.OrderTypeBuy
});

// Get symbol margin info (using SymbolInfoDouble)
// Assuming you have method to get SYMBOL_MARGIN_INITIAL
var baseMargin = 100.0; // Example: $100 base margin for 1 lot

var requiredMargin = lots * baseMargin * marginRates.InitialMarginRate;

Console.WriteLine($"Required margin for {lots} lots: ${requiredMargin:F2}");
```

---

### 3) Compare Buy vs Sell margin requirements

```csharp
var symbol = "USDJPY";

var buyMargin = await acc.SymbolInfoMarginRateAsync(new SymbolInfoMarginRateRequest
{
    Symbol = symbol,
    OrderType = ENUM_ORDER_TYPE.OrderTypeBuy
});

var sellMargin = await acc.SymbolInfoMarginRateAsync(new SymbolInfoMarginRateRequest
{
    Symbol = symbol,
    OrderType = ENUM_ORDER_TYPE.OrderTypeSell
});

Console.WriteLine($"Buy - Initial: {buyMargin.InitialMarginRate}, Maintenance: {buyMargin.MaintenanceMarginRate}");
Console.WriteLine($"Sell - Initial: {sellMargin.InitialMarginRate}, Maintenance: {sellMargin.MaintenanceMarginRate}");

if (buyMargin.InitialMarginRate != sellMargin.InitialMarginRate)
{
    Console.WriteLine("⚠ Asymmetric margin requirements detected");
}
```

---

### 4) Check if account can open position

```csharp
var symbol = "BTCUSD";
var lots = 0.05;

// Get account equity
var account = await acc.AccountSummaryAsync();
var availableMargin = account.AccountEquity;

// Get margin rates
var marginRates = await acc.SymbolInfoMarginRateAsync(new SymbolInfoMarginRateRequest
{
    Symbol = symbol,
    OrderType = ENUM_ORDER_TYPE.OrderTypeBuy
});

// Calculate required margin (simplified example)
var baseMargin = 1000.0; // Example base margin
var requiredMargin = lots * baseMargin * marginRates.InitialMarginRate;

if (availableMargin >= requiredMargin)
{
    Console.WriteLine($"✓ Can open {lots} lots (need ${requiredMargin:F2}, have ${availableMargin:F2})");
}
else
{
    Console.WriteLine($"✗ Insufficient margin (need ${requiredMargin:F2}, have ${availableMargin:F2})");
}
```

---

### 5) Margin rates for all order types

```csharp
var symbol = "EURUSD";

var orderTypes = new[]
{
    ENUM_ORDER_TYPE.OrderTypeBuy,
    ENUM_ORDER_TYPE.OrderTypeSell,
    ENUM_ORDER_TYPE.OrderTypeBuyLimit,
    ENUM_ORDER_TYPE.OrderTypeSellLimit,
    ENUM_ORDER_TYPE.OrderTypeBuyStop,
    ENUM_ORDER_TYPE.OrderTypeSellStop
};

Console.WriteLine($"Margin rates for {symbol}:\n");

foreach (var orderType in orderTypes)
{
    try
    {
        var result = await acc.SymbolInfoMarginRateAsync(new SymbolInfoMarginRateRequest
        {
            Symbol = symbol,
            OrderType = orderType
        });

        Console.WriteLine($"{orderType,-25} Initial: {result.InitialMarginRate:F4}  Maintenance: {result.MaintenanceMarginRate:F4}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{orderType,-25} Error: {ex.Message}");
    }
}
```

---

### 6) Risk management: calculate stop-out level

```csharp
var symbol = "XAUUSD";
var lots = 0.10;

// Get current position info
var positions = await acc.OpenedOrdersAsync();
var position = positions.Positions.FirstOrDefault(p => p.Symbol == symbol);

if (position != null)
{
    // Get maintenance margin rate
    var marginRates = await acc.SymbolInfoMarginRateAsync(new SymbolInfoMarginRateRequest
    {
        Symbol = symbol,
        OrderType = position.Type == 0 ? ENUM_ORDER_TYPE.OrderTypeBuy : ENUM_ORDER_TYPE.OrderTypeSell
    });

    // Calculate maintenance margin requirement
    var baseMargin = 1000.0; // Example
    var maintenanceMargin = position.Volume * baseMargin * marginRates.MaintenanceMarginRate;

    // Get account info
    var account = await acc.AccountSummaryAsync();

    // Calculate margin level
    var marginLevel = (account.AccountEquity / maintenanceMargin) * 100;

    Console.WriteLine($"Position maintenance margin: ${maintenanceMargin:F2}");
    Console.WriteLine($"Current equity: ${account.AccountEquity:F2}");
    Console.WriteLine($"Margin level: {marginLevel:F2}%");

    if (marginLevel < 50)
    {
        Console.WriteLine("⚠ WARNING: Close to stop-out level!");
    }
}
```
