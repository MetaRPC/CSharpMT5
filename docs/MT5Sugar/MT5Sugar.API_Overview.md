# MT5Sugar - Complete API Reference

> **High-level convenience methods** for MetaTrader 5 trading automation in C#

**MT5Sugar** extends `MT5Service` with convenient methods for:

- 🎯 Risk-based position sizing (fixed dollar risk)
- 📊 Automatic volume and price normalization
- 🔄 Bulk operations (close all positions, cancel all orders)
- 📈 DOM (Market Depth) - order book analysis
- ✅ Pre-flight order validation and margin checking
- 📉 P&L monitoring and position statistics

📖 **Important:** [How to Work with Streaming Subscriptions](../All_Guides/GRPC_STREAM_MANAGEMENT.md) - Complete guide to real-time data streams

---

## Navigation by Region

### [01] 🔧 INFRASTRUCTURE

**Basic infrastructure for working with symbols**

| Method | Description | Documentation |
|--------|-------------|---------------|
| `EnsureSelected()` | Ensures symbol is selected and synchronized in MT5 | [→ Docs](1.%20Infrastructure/EnsureSelected.md) |

---

### [02] 📸 SNAPSHOTS

**Instant snapshots of account and symbol state**

| Method | Description | Documentation |
|--------|-------------|---------------|
| `GetAccountSnapshot()` | Complete account snapshot (balance, margin, equity) | [→ Docs](2.%20Snapshots/GetAccountSnapshot.md) |
| `GetSymbolSnapshot()` | Complete symbol snapshot (prices, spread, limits) | [→ Docs](2.%20Snapshots/GetSymbolSnapshot.md) |

**Data classes:**

- `AccountSnapshot` - account data structure
- `SymbolSnapshot` - symbol data structure

---

### [03] 🔢 NORMALIZATION & UTILS

**Utilities for price normalization and unit conversion**

| Method | Description | Documentation |
|--------|-------------|---------------|
| `GetPointAsync()` | Get point size for symbol | [→ Docs](3.%20Normalization_Utils/GetPointAsync.md) |
| `GetDigitsAsync()` | Number of decimal places | [→ Docs](3.%20Normalization_Utils/GetDigitsAsync.md) |
| `NormalizePriceAsync()` | Normalize price by TickSize | [→ Docs](3.%20Normalization_Utils/NormalizePriceAsync.md) |
| `PointsToPipsAsync()` | Convert points to pips | [→ Docs](3.%20Normalization_Utils/PointsToPipsAsync.md) |
| `GetSpreadPointsAsync()` | Current spread in points | [→ Docs](3.%20Normalization_Utils/GetSpreadPointsAsync.md) |

---

### [04] 📜 HISTORY HELPERS

**Retrieve order and position history with pagination**

| Method | Description | Documentation |
|--------|-------------|---------------|
| `OrdersHistoryLast()` | Closed orders history for N days | [→ Docs](4.%20History_Helpers/OrdersHistoryLast.md) |
| `PositionsHistoryPaged()` | Position history with pagination | [→ Docs](4.%20History_Helpers/PositionsHistoryPaged.md) |

---

### [05] 🌊 STREAMS HELPERS

**Bounded streaming - reading ticks and trades with limits**

| Method | Description | Documentation |
|--------|-------------|---------------|
| `ReadTicks()` | Read N ticks or until timeout | [→ Docs](5.%20Streams_Helpers/ReadTicks.md) |
| `ReadTrades()` | Read N trade events or until timeout | [→ Docs](5.%20Streams_Helpers/ReadTrades.md) |

---

### [06] 💹 TRADING — MARKET & PENDING

**Placing and managing market and pending orders**

| Method | Description | Documentation |
|--------|-------------|---------------|
| `PlaceMarket()` | Place market order (BUY/SELL) | [→ Docs](6.%20Trading_Market_Pending/PlaceMarket.md) |
| `PlacePending()` | Place pending order (Limit/Stop) | [→ Docs](6.%20Trading_Market_Pending/PlacePending.md) |
| `ModifySlTpAsync()` | Modify SL/TP of existing order | [→ Docs](6.%20Trading_Market_Pending/ModifySlTpAsync.md) |
| `CloseByTicket()` | Close position by ticket | [→ Docs](6.%20Trading_Market_Pending/CloseByTicket.md) |
| `CloseAll()` | Close all positions (with filters) | [→ Docs](6.%20Trading_Market_Pending/CloseAll.md) |

---

### [07] ⚖️ VOLUME & PRICE UTILITIES

**Critical utilities for volume calculation and price operations**

| Method | Description | Documentation |
|--------|-------------|---------------|
| `CalcVolumeForRiskAsync()`  | **PRIMARY METHOD** - calculate volume by risk | [→ Docs](8.%20Volume_Price_Utils/CalcVolumeForRiskAsync.md) |
| `GetVolumeLimitsAsync()` | Get Min/Max/Step for symbol volume | [→ Docs](8.%20Volume_Price_Utils/GetVolumeLimitsAsync.md) |
| `NormalizeVolumeAsync()` | Normalize volume to broker limits | [→ Docs](8.%20Volume_Price_Utils/NormalizeVolumeAsync.md) |
| `GetTickValueAndSizeAsync()` | Get TickValue and TickSize | [→ Docs](8.%20Volume_Price_Utils/GetTickValueAndSizeAsync.md) |
| `PriceFromOffsetPointsAsync()` | Calculate price from point offset | [→ Docs](8.%20Volume_Price_Utils/PriceFromOffsetPointsAsync.md) |

---

### [08] 📍 PENDING HELPERS (BY POINTS)

**Pending orders with point-based distance**

| Method | Description | Documentation |
|--------|-------------|---------------|
| `BuyLimitPoints()` | Buy Limit N points below Ask | [→ Docs](9.%20Pending_ByPoints/BuyLimitPoints.md) |
| `SellLimitPoints()` | Sell Limit N points above Bid | [→ Docs](9.%20Pending_ByPoints/SellLimitPoints.md) |
| `BuyStopPoints()` | Buy Stop N points above Ask | [→ Docs](9.%20Pending_ByPoints/BuyStopPoints.md) |
| `SellStopPoints()` | Sell Stop N points below Bid | [→ Docs](9.%20Pending_ByPoints/SellStopPoints.md) |

---

### [09] 💰 MARKET BY RISK

**Market orders with fixed dollar risk**

| Method | Description | Documentation |
|--------|-------------|---------------|
| `BuyMarketByRisk()`  | BUY with automatic volume calculation by risk | [→ Docs](10.%20Market_ByRisk/BuyMarketByRisk.md) |
| `SellMarketByRisk()`  | SELL with automatic volume calculation by risk | [→ Docs](10.%20Market_ByRisk/SellMarketByRisk.md) |

**Example:**
```csharp
// Risk exactly $100 with 50-point SL
await svc.BuyMarketByRisk("EURUSD", stopPoints: 50, riskMoney: 100);
```

---

### [10] 🧹 BULK CONVENIENCE

**Bulk operations with orders and positions**

| Method | Description | Documentation |
|--------|-------------|---------------|
| `CancelAll()` | Cancel all pending orders | [→ Docs](11.%20Bulk_Convenience/CancelAll.md) |
| `CloseAllPositions()` | Close all market positions | [→ Docs](11.%20Bulk_Convenience/CloseAllPositions.md) |
| `CloseAllPending()` | Close/cancel all pending orders | [→ Docs](11.%20Bulk_Convenience/CloseAllPending.md) |

---

### [11] 📊 MARKET DEPTH (DOM)

**Level II market data - order book analysis**

| Method | Description | Documentation |
|--------|-------------|---------------|
| `SubscribeToMarketBookAsync()` | Subscribe to order book (returns IDisposable) | [→ Docs](12.%20Market_Depth_DOM/SubscribeToMarketBookAsync.md) |
| `GetMarketBookSnapshotAsync()` | Get order book snapshot | [→ Docs](12.%20Market_Depth_DOM/GetMarketBookSnapshotAsync.md) |
| `GetBestBidAskFromBookAsync()` | Best Bid/Ask from order book | [→ Docs](12.%20Market_Depth_DOM/GetBestBidAskFromBookAsync.md) |
| `CalculateLiquidityAtLevelAsync()` | Calculate liquidity at price level | [→ Docs](12.%20Market_Depth_DOM/CalculateLiquidityAtLevelAsync.md) |

**Data class:**

- `MarketBookSubscription` - subscription management

---

### [12] ✅ ORDER VALIDATION 

**Pre-flight checks to prevent errors**

| Method | Description | Documentation |
|--------|-------------|---------------|
| `ValidateOrderAsync()` | Full order validation before sending | [→ Docs](13.%20Order_Validation/ValidateOrderAsync.md) |
| `CalculateBuyMarginAsync()` | Calculate margin for BUY order | [→ Docs](13.%20Order_Validation/CalculateBuyMarginAsync.md) |
| `CalculateSellMarginAsync()` | Calculate margin for SELL order | [→ Docs](13.%20Order_Validation/CalculateSellMarginAsync.md) |
| `CheckMarginAvailabilityAsync()`  | Check free margin sufficiency | [→ Docs](13.%20Order_Validation/CheckMarginAvailabilityAsync.md) |

---

### [13] ⏰ SESSION TIME

**Trading and quote session information**

| Method | Description | Documentation |
|--------|-------------|---------------|
| `GetQuoteSessionAsync()` | Quote session time (when prices are visible) | [→ Docs](14.%20Session_Time/GetQuoteSessionAsync.md) |
| `GetTradeSessionAsync()` | Trading session time (when trading is allowed) | [→ Docs](14.%20Session_Time/GetTradeSessionAsync.md) |

---

### [14] 📈 POSITION MONITORING

**Monitoring and analyzing open positions**

| Method | Description | Documentation |
|--------|-------------|---------------|
| `GetProfitablePositionsAsync()` | Get all profitable positions | [→ Docs](15.%20Position_Monitoring/GetProfitablePositionsAsync.md) |
| `GetLosingPositionsAsync()` | Get all losing positions | [→ Docs](15.%20Position_Monitoring/GetLosingPositionsAsync.md) |
| `GetTotalProfitLossAsync()`  | Total P&L across all positions | [→ Docs](15.%20Position_Monitoring/GetTotalProfitLossAsync.md) |
| `GetPositionCountAsync()` | Number of open positions | [→ Docs](15.%20Position_Monitoring/GetPositionCountAsync.md) |
| `GetPositionStatsBySymbolAsync()` | Position statistics by symbol | [→ Docs](15.%20Position_Monitoring/GetPositionStatsBySymbolAsync.md) |

---

## 🎯 Common Use Cases

### Simple market order
```csharp
// Buy 0.10 lots EURUSD
var result = await svc.PlaceMarket("EURUSD", 0.10, isBuy: true);
Console.WriteLine($"Order #{result.Order} at {result.Price:F5}");
```

### Order with fixed risk ($100)
```csharp
// Risk $100, stop 50 points, take 150 points
await svc.BuyMarketByRisk("EURUSD",
    stopPoints: 50,
    riskMoney: 100,
    tpPoints: 150);
```

### Margin check before trading
```csharp
var (hasEnough, free, required) = await svc.CheckMarginAvailabilityAsync(
    "EURUSD", volume: 1.0, isBuy: true);

if (hasEnough)
{
    await svc.PlaceMarket("EURUSD", 1.0, isBuy: true);
}
else
{
    Console.WriteLine($"Insufficient margin: need ${required}, have ${free}");
}
```

### Emergency close on drawdown
```csharp
double totalPL = await svc.GetTotalProfitLossAsync();

if (totalPL < -500)
{
    Console.WriteLine("🚨 Drawdown $500 - closing all positions!");
    await svc.CloseAllPositions();
}
```

### Order book (DOM) analysis
```csharp
using (await svc.SubscribeToMarketBookAsync("EURUSD"))
{
    var book = await svc.GetMarketBookSnapshotAsync("EURUSD");
    var (bestBid, bestAsk) = await svc.GetBestBidAskFromBookAsync("EURUSD");

    var liquidity = await svc.CalculateLiquidityAtLevelAsync(
        "EURUSD", bestBid, isBuy: true);

    Console.WriteLine($"Liquidity at {bestBid}: {liquidity} lots");
}
```

---

## 🔗 Related Documentation

- **[MT5Account Documentation](../API_Reference/MT5Account.md)** - Low-level RPC methods
- **[MT5Service Documentation](../API_Reference/MT5Service.md)** - Mid-level service layer
- **MT5Sugar** (this document) - High-level convenience methods

---

## 📦 Architecture

```
MT5Sugar (Extension Methods) ← You are here
    ↓ Risk-based trading, DOM, bulk operations
    ↓
MT5Service (Mid-level)
    ↓ Typed wrappers, snapshots, validation
    ↓
MT5Account (Low-level)
    ↓ Direct gRPC/Protobuf calls
    ↓
MetaTrader 5 Terminal
```

## 📝 Conventions

- All methods are asynchronous (`async Task<T>`)
- Extend `MT5Service` class (extension methods)
- Parameter `timeoutSec` - RPC operation timeout (default 10-20 sec)
- Parameter `ct` - `CancellationToken` for operation cancellation
- Prices are always **absolute**, not relative
- Volumes are always in **lots**, not currency units
- **Points** - minimum price increment
- **Pips** - standard unit for traders (1 pip = 10 points for 5-digit)


🎉 **Ready to use!**
