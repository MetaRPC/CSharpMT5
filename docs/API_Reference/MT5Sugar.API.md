Parsing MT5Sugar.cs...
Found 50 methods
Generated docs/API_Reference/MT5Sugar.API.md
---

## Table of Contents

1. [EnsureSelected](#ensureselected)
2. [AccountSnapshot](#accountsnapshot)
3. [GetAccountSnapshot](#getaccountsnapshot)
4. [SymbolSnapshot](#symbolsnapshot)
5. [GetSymbolSnapshot](#getsymbolsnapshot)
6. [GetPointAsync](#getpointasync)
7. [GetDigitsAsync](#getdigitsasync)
8. [NormalizePriceAsync](#normalizepriceasync)
9. [PointsToPipsAsync](#pointstopipsasync)
10. [GetSpreadPointsAsync](#getspreadpointsasync)
11. [OrdersHistoryLast](#ordershistorylast)
12. [PositionsHistoryPaged](#positionshistorypaged)
13. [ReadTicks](#readticks)
14. [ReadTrades](#readtrades)
15. [PlaceMarket](#placemarket)
16. [PlacePending](#placepending)
17. [ModifySlTpAsync](#modifysltpasync)
18. [CloseByTicket](#closebyticket)
19. [CloseAll](#closeall)
20. [GetVolumeLimitsAsync](#getvolumelimitsasync)
21. [NormalizeVolumeAsync](#normalizevolumeasync)
22. [GetTickValueAndSizeAsync](#gettickvalueandsizeasync)
23. [PriceFromOffsetPointsAsync](#pricefromoffsetpointsasync)
24. [CalcVolumeForRiskAsync](#calcvolumeforriskasync)
25. [BuyLimitPoints](#buylimitpoints)
26. [SellLimitPoints](#selllimitpoints)
27. [BuyStopPoints](#buystoppoints)
28. [SellStopPoints](#sellstoppoints)
29. [BuyMarketByRisk](#buymarketbyrisk)
30. [SellMarketByRisk](#sellmarketbyrisk)
31. [CancelAll](#cancelall)
32. [CloseAllPositions](#closeallpositions)
33. [CloseAllPending](#closeallpending)
34. [SubscribeToMarketBookAsync](#subscribetomarketbookasync)
35. [GetMarketBookSnapshotAsync](#getmarketbooksnapshotasync)
36. [GetBestBidAskFromBookAsync](#getbestbidaskfrombookasync)
37. [CalculateLiquidityAtLevelAsync](#calculateliquidityatlevelasync)
38. [ValidateOrderAsync](#validateorderasync)
39. [CalculateBuyMarginAsync](#calculatebuymarginasync)
40. [CalculateSellMarginAsync](#calculatesellmarginasync)
41. [CheckMarginAvailabilityAsync](#checkmarginavailabilityasync)
42. [GetQuoteSessionAsync](#getquotesessionasync)
43. [GetTradeSessionAsync](#gettradesessionasync)
44. [GetProfitablePositionsAsync](#getprofitablepositionsasync)
45. [GetLosingPositionsAsync](#getlosingpositionsasync)
46. [GetTotalProfitLossAsync](#gettotalprofitlossasync)
47. [GetPositionCountAsync](#getpositioncountasync)
48. [GetPositionStatsBySymbolAsync](#getpositionstatsbysymbolasync)
49. [AccountSnapshot](#accountsnapshot)
50. [SymbolSnapshot](#symbolsnapshot)

---

## EnsureSelected

Ensures the specified symbol is selected in the MT5 terminal and fully synchronized. Required before performing operations on symbols that may not be in Market Watch.

### Signature

```csharp
public Task EnsureSelected(
    this MT5Service svc,
    string symbol,
    int timeoutSec = 10,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name to select and synchronize (e.g., "EURUSD", "XAUUSD"). |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10). |
| `ct` | `CancellationToken` | Cancellation token to cancel the operation. |

### Returns

`Task` - Task that completes when symbol is selected and synchronized.

### Remarks

<para>This is a critical helper for symbol operations. Many MT5 functions require symbols to be:</para> <list type="bullet"> <item><description>1. Selected (added to Market Watch)</description></item> <item><description>2. Synchronized (receiving live quotes from broker)</description></item> </list> <para>Without this, operations like getting symbol info, placing orders, or retrieving ticks may fail.</para> <para>PERFORMANCE NOTE: This method makes 2 sequential RPC calls. Cache symbols when possible.</para>

---

## AccountSnapshot

Composite snapshot containing account summary and all currently opened orders/positions. Provides a complete view of account state at a single point in time.

### Signature

```csharp
public sealed record AccountSnapshot(
    AccountSummaryData Summary,
    OpenedOrdersData OpenedOrders
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `Summary` | `AccountSummaryData` | Account summary with balance, equity, margin, and other account-level metrics. |
| `OpenedOrders` | `OpenedOrdersData` | All opened orders and positions sorted by open time (ascending). |

### Returns

`sealed record`

### Remarks

This record combines two frequently-needed pieces of information into one atomic snapshot. Useful for dashboards, risk monitoring, and account state logging.

---

## GetAccountSnapshot

Retrieves a complete account snapshot including summary data and all opened orders/positions. Optimizes performance by fetching both datasets in parallel where possible.

### Signature

```csharp
public Task<AccountSnapshot> GetAccountSnapshot(
    this MT5Service svc,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15). |
| `ct` | `CancellationToken` | Cancellation token to cancel the operation. |

### Returns

`Task<AccountSnapshot>` - <see cref="AccountSnapshot"/> containing account summary and opened orders.

### Remarks

<para>This method combines two RPC calls into a single operation:</para> <list type="number"> <item><description>AccountSummaryAsync - gets balance, equity, margin, profit, etc.</description></item> <item><description>OpenedOrdersAsync - gets all active orders and positions</description></item> </list> <para>PERFORMANCE: Both calls use the same deadline for consistency.</para> <para>USE CASES:</para> <list type="bullet"> <item><description>Dashboard updates showing account state + active trades</description></item> <item><description>Risk monitoring (check margin level + open positions)</description></item> <item><description>Periodic state logging/auditing</description></item> <item><description>Pre-trade validation (check available margin before opening)</description></item> </list>

---

## SymbolSnapshot

Composite snapshot containing essential symbol trading parameters at a single point in time. Includes current tick, price precision, and margin requirements.

### Signature

```csharp
public sealed record SymbolSnapshot(
    MrpcMqlTick Tick,
    double Point,
    int Digits,
    SymbolInfoMarginRateData MarginRate
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `Tick` | `MrpcMqlTick` | Current tick data (bid, ask, last, volume, time). |
| `Point` | `double` | Minimal price change step (e.g., 0.00001 for EURUSD). |
| `Digits` | `int` | Number of decimal places in price quotes (e.g., 5 for EURUSD). |
| `MarginRate` | `SymbolInfoMarginRateData` | Margin rate data for BUY orders (initial margin, maintenance margin). |

### Returns

`sealed record`

### Remarks

This record bundles the most commonly-needed symbol properties for trading operations. Particularly useful for order validation, price normalization, and risk calculations.

---

## GetSymbolSnapshot

Retrieves a complete trading snapshot for the specified symbol. Fetches current tick, price precision, and margin requirements in one operation.

### Signature

```csharp
public Task<SymbolSnapshot> GetSymbolSnapshot(
    this MT5Service svc,
    string symbol,
    int timeoutSec = 10,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name to snapshot (e.g., "EURUSD", "XAUUSD", "BTCUSD"). |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10). |
| `ct` | `CancellationToken` | Cancellation token to cancel the operation. |

### Returns

`Task<SymbolSnapshot>` - <see cref="SymbolSnapshot"/> containing tick data, point size, digits, and margin rates.

### Remarks

<para>This method performs the following operations:</para> <list type="number"> <item><description>Ensures symbol is selected and synchronized</description></item> <item><description>Fetches current tick (bid, ask, last price)</description></item> <item><description>Retrieves point size (minimal price step)</description></item> <item><description>Gets price precision (number of decimal digits)</description></item> <item><description>Queries margin rate for BUY orders</description></item> </list> <para>PERFORMANCE: Makes 5 sequential RPC calls. Consider caching for frequently-used symbols.</para> <para>USE CASES:</para> <list type="bullet"> <item><description>Pre-trade validation (check current prices + margin)</description></item> <item><description>Price normalization (round to correct digits)</description></item> <item><description>Stop-loss/take-profit calculation (use point for pip distance)</description></item> <item><description>Risk management (calculate margin requirements)</description></item> </list>

---

## GetPointAsync

Retrieves the symbol's point value - the minimal price change step. Convenience wrapper around SymbolInfoDouble(SymbolPoint).

### Signature

```csharp
public Task<double> GetPointAsync(
    this MT5Service svc,
    string symbol,
    int timeoutSec = 10,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD", "XAUUSD"). |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10). |
| `ct` | `CancellationToken` | Cancellation token to cancel the operation. |

### Returns

`Task<double>` - Point value as a double (e.g., 0.00001 for 5-digit EURUSD, 0.01 for XAUUSD).

### Remarks

<para>POINT is the fundamental unit for price calculations in MT5:</para> <list type="bullet"> <item><description>5-digit EURUSD: point = 0.00001 (1 pip = 10 points)</description></item> <item><description>3-digit USDJPY: point = 0.001 (1 pip = 10 points)</description></item> <item><description>2-digit XAUUSD: point = 0.01 (1 pip = 1 point)</description></item> </list> <para>Used for calculating stop-loss/take-profit distances, spreads, and price movements.</para>

---

## GetDigitsAsync

Gets the number of decimal places used in price quotes for the specified symbol. Convenience wrapper around SymbolInfoInteger(SymbolDigits).

### Signature

```csharp
public Task<int> GetDigitsAsync(
    this MT5Service svc,
    string symbol,
    int timeoutSec = 10,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD", "XAUUSD"). |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10). |
| `ct` | `CancellationToken` | Cancellation token to cancel the operation. |

### Returns

`Task<int>` - Number of digits as integer (e.g., 5 for EURUSD, 2 for XAUUSD, 3 for USDJPY).

### Remarks

<para>Digits determine price precision and formatting:</para> <list type="bullet"> <item><description>5 digits: 1.08550 (typical for major FX pairs)</description></item> <item><description>3 digits: 150.123 (typical for JPY pairs)</description></item> <item><description>2 digits: 1925.50 (typical for gold/silver)</description></item> </list> <para>Use with Math.Round() to format prices correctly before displaying or sending to MT5.</para>

---

## NormalizePriceAsync

Normalizes a price to the symbol's tick size (not just digits). Uses SYMBOL_TRADE_TICK_SIZE for strict broker-compliant rounding.

### Signature

```csharp
public Task<double> NormalizePriceAsync(
    this MT5Service svc,
    string symbol,
    double price,
    int timeoutSec = 10,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD", "XAUUSD"). |
| `price` | `double` | Raw price value to normalize. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10). |
| `ct` | `CancellationToken` | Cancellation token to cancel the operation. |

### Returns

`Task<double>` - Normalized price that aligns with the symbol's tick size.

### Remarks

<para>IMPORTANT: This uses tick size, NOT digits. Tick size is broker-specific and more accurate.</para> <para>Example: Some brokers use tick size of 0.00005 for EURUSD (5 points), not 0.00001.</para> <para>Always use this method to normalize stop-loss, take-profit, and limit prices before sending orders.</para> <para>PERFORMANCE: Makes 1 RPC call. Consider caching tick size for repeated operations.</para>

---

## PointsToPipsAsync

Converts points to pips based on symbol's digit precision. For 5-digit pairs: 10 points = 1 pip. For 3-digit pairs: 10 points = 1 pip.

### Signature

```csharp
public Task<double> PointsToPipsAsync(
    this MT5Service svc,
    string symbol,
    double points,
    int timeoutSec = 10,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD", "USDJPY"). |
| `points` | `double` | Number of points to convert. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10). |
| `ct` | `CancellationToken` | Cancellation token to cancel the operation. |

### Returns

`Task<double>` - Equivalent value in pips (fractional).

### Remarks

<para>PIP (Percentage In Point) is the standard unit traders use for price movements:</para> <list type="bullet"> <item><description>5-digit EURUSD (1.08550): 1 pip = 10 points = 0.0010 move</description></item> <item><description>3-digit USDJPY (150.123): 1 pip = 10 points = 0.10 move</description></item> <item><description>2-digit XAUUSD (1925.50): 1 pip = 1 point = 1.00 move</description></item> </list> <para>Use this for human-readable distance reporting (e.g., "Stop loss is 20 pips away").</para>

---

## GetSpreadPointsAsync

Calculates the current spread (Ask - Bid) in points. Fetches latest tick and computes spread using symbol's point value.

### Signature

```csharp
public Task<double> GetSpreadPointsAsync(
    this MT5Service svc,
    string symbol,
    int timeoutSec = 10,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD", "XAUUSD"). |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10). |
| `ct` | `CancellationToken` | Cancellation token to cancel the operation. |

### Returns

`Task<double>` - Spread in points (e.g., 15.0 for a 1.5-pip spread on 5-digit EURUSD).

### Remarks

<para>Spread represents the broker's markup and liquidity cost.</para> <para>Typical spreads:</para> <list type="bullet"> <item><description>EURUSD: 10-20 points (1-2 pips) on ECN accounts</description></item> <item><description>XAUUSD: 20-50 points (0.20-0.50 USD) depending on volatility</description></item> <item><description>Exotic pairs: 50-200+ points</description></item> </list> <para>PERFORMANCE: Makes 2 RPC calls (tick + point). Monitor spread before trading.</para>

---

## OrdersHistoryLast

Retrieves closed orders history for the last N days with pagination support. Convenience wrapper that automatically calculates the date range from current time.

### Signature

```csharp
public Task<OrdersHistoryData> OrdersHistoryLast(
    this MT5Service svc,
    int days = 7,
    int page = 0,
    int size = 100,
    BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sort = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
    int timeoutSec = 20,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `days` | `int` | Number of days to look back from today (default: 7). |
| `page` | `int` | Page number for pagination, zero-based (default: 0). |
| `size` | `int` | Number of records per page (default: 100). |
| `sort` | `BMT5_ENUM_ORDER_HISTORY_SORT_TYPE` | Sort order for results (default: by close time ascending). |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 20). |
| `ct` | `CancellationToken` | Cancellation token to cancel the operation. |

### Returns

`Task<OrdersHistoryData>` - <see cref="OrdersHistoryData"/> containing closed orders within the specified date range.

### Remarks

<para>Fetches orders that were closed/cancelled within the last N days.</para> <para>Date range: from (now - N days) to (now), both in UTC.</para> <para>PAGINATION: Use page/size parameters to retrieve large histories incrementally.</para> <para>USE CASES:</para> <list type="bullet"> <item><description>Trading journal/audit (retrieve last week's trades)</description></item> <item><description>Performance analysis (calculate win rate, avg profit)</description></item> <item><description>Trade replication (replay recent orders)</description></item> <item><description>Debugging (check what orders were executed)</description></item> </list>

---

## PositionsHistoryPaged

Retrieves positions history with pagination and optional date filtering. Returns closed positions (deals) with full profit/loss information.

### Signature

```csharp
public Task<PositionsHistoryData> PositionsHistoryPaged(
    this MT5Service svc,
    AH_ENUM_POSITIONS_HISTORY_SORT_TYPE sort = AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeAsc,
    DateTime? openFrom = null,
    DateTime? openTo = null,
    int page = 0,
    int size = 100,
    int timeoutSec = 20,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `sort` | `AH_ENUM_POSITIONS_HISTORY_SORT_TYPE` | Sort order for results (default: by position open time ascending). |
| `openFrom` | `DateTime?` | Optional start date for position open time filter (UTC). Null = no lower bound. |
| `openTo` | `DateTime?` | Optional end date for position open time filter (UTC). Null = no upper bound. |
| `page` | `int` | Page number for pagination, zero-based (default: 0). |
| `size` | `int` | Number of records per page (default: 100). |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 20). |
| `ct` | `CancellationToken` | Cancellation token to cancel the operation. |

### Returns

`Task<PositionsHistoryData>` - <see cref="PositionsHistoryData"/> containing closed positions with P&amp;L details.

### Remarks

<para>IMPORTANT: This retrieves CLOSED positions (deals), not current open positions.</para> <para>Each position includes:</para> <list type="bullet"> <item><description>Entry/exit prices and times</description></item> <item><description>Realized profit/loss</description></item> <item><description>Commission and swap charges</description></item> <item><description>Position volume and symbol</description></item> </list> <para>PAGINATION: Essential for accounts with large trading history.</para> <para>USE CASES:</para> <list type="bullet"> <item><description>Performance analytics (calculate total profit, drawdown)</description></item> <item><description>Strategy backtesting validation (compare algo vs actual)</description></item> <item><description>Tax reporting (export closed positions for accounting)</description></item> <item><description>Trade history review (analyze winning/losing patterns)</description></item> </list>

---

## ReadTicks

Reads a limited number of tick events from specified symbols or until duration timeout expires. Provides bounded streaming with automatic termination safeguards.

### Signature

```csharp
public IAsyncEnumerable<OnSymbolTickData> ReadTicks(
    this MT5Service svc,
    IEnumerable<string> symbols,
    int maxEvents = 50,
    int durationSec = 5,
    [EnumeratorCancellation] CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbols` | `IEnumerable<string>` | Collection of symbol names to subscribe to (e.g., ["EURUSD", "XAUUSD"]). |
| `maxEvents` | `int` | Maximum number of tick events to read before stopping (default: 50). |
| `durationSec` | `int` | Maximum duration in seconds to read ticks before timeout (default: 5). |
| `ct` | `CancellationToken` | Cancellation token to cancel the operation. |

### Returns

`IAsyncEnumerable<OnSymbolTickData>` - Async enumerable stream of <see cref="OnSymbolTickData"/> tick events. Stream automatically terminates when either maxEvents or durationSec is reached.

### Remarks

<para>BOUNDED STREAMING: Unlike raw OnSymbolTickAsync, this helper automatically limits the stream.</para> <para>TERMINATION CONDITIONS (whichever comes first):</para> <list type="number"> <item><description>Received maxEvents tick updates</description></item> <item><description>Duration timeout (durationSec) expired</description></item> <item><description>Cancellation token triggered</description></item> </list> <para>USE CASES:</para> <list type="bullet"> <item><description>Sample recent tick data for analysis</description></item> <item><description>Monitor prices briefly before placing orders</description></item> <item><description>Test streaming connectivity without infinite loops</description></item> <item><description>Collect tick samples for spread/volatility calculation</description></item> </list> <para>PERFORMANCE: Creates a linked CancellationTokenSource for timeout management.</para>

---

## ReadTrades

Reads a limited number of trade transaction events or until duration timeout expires. Monitors order fills, modifications, and cancellations with automatic termination.

### Signature

```csharp
public IAsyncEnumerable<OnTradeData> ReadTrades(
    this MT5Service svc,
    int maxEvents = 20,
    int durationSec = 5,
    [EnumeratorCancellation] CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `maxEvents` | `int` | Maximum number of trade events to read before stopping (default: 20). |
| `durationSec` | `int` | Maximum duration in seconds to read events before timeout (default: 5). |
| `ct` | `CancellationToken` | Cancellation token to cancel the operation. |

### Returns

`IAsyncEnumerable<OnTradeData>` - Async enumerable stream of <see cref="OnTradeData"/> trade transaction events. Stream automatically terminates when either maxEvents or durationSec is reached.

### Remarks

<para>TRADE EVENTS include:</para> <list type="bullet"> <item><description>Order placements (market, limit, stop)</description></item> <item><description>Order fills and partial fills</description></item> <item><description>Order modifications (SL/TP changes)</description></item> <item><description>Order cancellations</description></item> <item><description>Position opens/closes</description></item> </list> <para>BOUNDED STREAMING: Automatically limits the stream to prevent infinite loops.</para> <para>TERMINATION CONDITIONS (whichever comes first):</para> <list type="number"> <item><description>Received maxEvents trade updates</description></item> <item><description>Duration timeout (durationSec) expired</description></item> <item><description>Cancellation token triggered</description></item> </list> <para>USE CASES:</para> <list type="bullet"> <item><description>Monitor order execution confirmations</description></item> <item><description>Audit recent trading activity</description></item> <item><description>Test trade notification system</description></item> <item><description>Verify stop-loss/take-profit triggers</description></item> </list>

---

## PlaceMarket

Places a market order (BUY or SELL) with optional stop-loss and take-profit.

### Signature

```csharp
public Task<mt5_term_api.OrderSendData> PlaceMarket(
    this MT5Service svc,
    string symbol,
    double volume,
    bool isBuy,
    double? sl = null,
    double? tp = null,
    string? comment = null,
    int deviationPoints = 0,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol to trade (e.g., "EURUSD"). |
| `volume` | `double` | Volume in lots. |
| `isBuy` | `bool` | True for BUY, false for SELL. |
| `sl` | `double?` | Optional stop-loss price. |
| `tp` | `double?` | Optional take-profit price. |
| `comment` | `string?` | Optional order comment. |
| `deviationPoints` | `int` | Maximum price deviation in points. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<mt5_term_api.OrderSendData>` - Order send result with ticket number and execution details.

### Remarks

Uses reflection to set order type (BUY=0, SELL=1) to avoid hard enum dependency. Automatically ensures symbol is selected before placing order.

---

## PlacePending

Places a pending order (Buy/Sell Limit or Stop) at specified price.

### Signature

```csharp
public Task<mt5_term_api.OrderSendData> PlacePending(
    this MT5Service svc,
    string symbol,
    double volume,
    ENUM_ORDER_TYPE type,
    double price,
    double? sl = null,
    double? tp = null,
    string? comment = null,
    int deviationPoints = 0,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol to trade. |
| `volume` | `double` | Volume in lots. |
| `type` | `ENUM_ORDER_TYPE` | Order type (BuyLimit, SellLimit, BuyStop, SellStop). |
| `price` | `double` | Entry price for the pending order. |
| `sl` | `double?` | Optional stop-loss price. |
| `tp` | `double?` | Optional take-profit price. |
| `comment` | `string?` | Optional order comment. |
| `deviationPoints` | `int` | Maximum price deviation in points. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<mt5_term_api.OrderSendData>` - Order send result with ticket number.

---

## ModifySlTpAsync

Modifies stop-loss and/or take-profit for an existing order or position.

### Signature

```csharp
public Task<OrderModifyData> ModifySlTpAsync(
    this MT5Service svc,
    ulong ticket,
    double? slPrice = null,
    double? tpPrice = null,
    int timeoutSec = 10,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `ticket` | `ulong` | Order or position ticket number. |
| `slPrice` | `double?` | New stop-loss price (absolute value). Pass null to keep current SL unchanged. |
| `tpPrice` | `double?` | New take-profit price (absolute value). Pass null to keep current TP unchanged. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<OrderModifyData>` - Order modify result with execution details.

### Remarks

At least one parameter (slPrice or tpPrice) must be provided. Pass absolute price values, not relative points or offsets.

---

## CloseByTicket

Closes an order or position by ticket number with optional partial volume.

### Signature

```csharp
public Task<OrderCloseData> CloseByTicket(
    this MT5Service svc,
    ulong ticket,
    double? volume = null,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `ticket` | `ulong` | Order or position ticket number to close. |
| `volume` | `double?` | Volume to close in lots. Pass null to close entire position. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<OrderCloseData>` - Order close result with execution details.

### Remarks

For full position closure, pass volume as null. For partial closure, specify exact volume to close (must not exceed position volume).

---

## CloseAll

Closes all open orders and positions with optional filtering by symbol and direction.

### Signature

```csharp
public Task<int> CloseAll(
    this MT5Service svc,
    string? symbol = null,
    bool? isBuy = null,
    int timeoutSec = 30,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string?` | Optional symbol filter (e.g., "EURUSD"). Pass null to close all symbols. |
| `isBuy` | `bool?` | Optional direction filter. True = close only BUY orders, False = close only SELL orders, null = close both. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 30). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<int>` - Number of orders/positions that were closed.

### Remarks

This method retrieves all open orders, applies filters, and closes matching positions sequentially. WARNING: Use with caution in live trading. Consider filters to avoid closing unintended positions.

---

## GetVolumeLimitsAsync

Retrieves volume constraints for a symbol (minimum, maximum, and step size).

### Signature

```csharp
public Task<(double min, double max, double step)> GetVolumeLimitsAsync(
    this MT5Service svc,
    string symbol,
    int timeoutSec = 10,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD", "XAUUSD"). |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<(double min, double max, double step)>` - Tuple containing minimum volume, maximum volume, and volume step.

### Remarks

Volume step defines the smallest increment allowed (e.g., 0.01 lots). Use this data to validate and normalize volumes before placing orders.

---

## NormalizeVolumeAsync

Normalizes volume to comply with symbol's step size and min/max limits.

### Signature

```csharp
public Task<double> NormalizeVolumeAsync(
    this MT5Service svc,
    string symbol,
    double volume,
    int timeoutSec = 10,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `volume` | `double` | Desired volume in lots. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<double>` - Normalized volume that broker will accept.

### Remarks

Rounds volume to nearest valid step and clamps to min/max range. Always use before placing orders to avoid broker rejections.

---

## GetTickValueAndSizeAsync

Retrieves tick value and tick size for risk and P&amp;L calculations.

### Signature

```csharp
public Task<(double tickValue, double tickSize)> GetTickValueAndSizeAsync(
    this MT5Service svc,
    string symbol,
    int timeoutSec = 10,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<(double tickValue, double tickSize)>` - Tuple containing tick value (monetary value per tick) and tick size (price increment).

### Remarks

Tick value represents how much money changes per one tick of price movement for 1 lot. Essential for accurate risk calculations and position sizing.

---

## PriceFromOffsetPointsAsync

Calculates pending order price by offset in points from current market price.

### Signature

```csharp
public Task<double> PriceFromOffsetPointsAsync(
    this MT5Service svc,
    string symbol,
    ENUM_ORDER_TYPE type,
    double offsetPoints,
    int timeoutSec = 10,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `type` | `ENUM_ORDER_TYPE` | Order type (BuyLimit, SellStop, etc.). |
| `offsetPoints` | `double` | Distance in points from current bid/ask. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<double>` - Normalized price for pending order placement.

### Remarks

Automatically uses Ask for buy orders and Bid for sell orders. Result is normalized to symbol's tick size.

---

## CalcVolumeForRiskAsync

Calculates position size (volume in lots) based on risk amount and stop-loss distance.

### Signature

```csharp
public Task<double> CalcVolumeForRiskAsync(
    this MT5Service svc,
    string symbol,
    double stopPoints,
    double riskMoney,
    int timeoutSec = 10,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `stopPoints` | `double` | Stop-loss distance in points. |
| `riskMoney` | `double` | Maximum amount of money to risk (in account currency). |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 10). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<double>` - Normalized volume in lots that matches the specified risk.

### Remarks

Core risk management method. Calculates exact lot size so that if stop-loss is hit, the loss equals the specified risk amount.

---

## BuyLimitPoints

Places Buy Limit pending order using point-based offset from current Ask price.

### Signature

```csharp
public Task<mt5_term_api.OrderSendData> BuyLimitPoints(
    this MT5Service svc,
    string symbol,
    double volume,
    double priceOffsetPoints,
    double? slPoints = null,
    double? tpPoints = null,
    string? comment = null,
    int deviationPoints = 0,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `volume` | `double` | Volume in lots. |
| `priceOffsetPoints` | `double` | Distance in points below current Ask (always positive). |
| `slPoints` | `double?` | Optional stop-loss distance in points below entry price. |
| `tpPoints` | `double?` | Optional take-profit distance in points above entry price. |
| `comment` | `string?` | Optional order comment. |
| `deviationPoints` | `int` | Maximum price deviation in points. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<mt5_term_api.OrderSendData>` - Order send result with ticket number.

---

## SellLimitPoints

Places Sell Limit pending order using point-based offset from current Bid price.

### Signature

```csharp
public Task<mt5_term_api.OrderSendData> SellLimitPoints(
    this MT5Service svc,
    string symbol,
    double volume,
    double priceOffsetPoints,
    double? slPoints = null,
    double? tpPoints = null,
    string? comment = null,
    int deviationPoints = 0,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `volume` | `double` | Volume in lots. |
| `priceOffsetPoints` | `double` | Distance in points above current Bid (always positive). |
| `slPoints` | `double?` | Optional stop-loss distance in points above entry price. |
| `tpPoints` | `double?` | Optional take-profit distance in points below entry price. |
| `comment` | `string?` | Optional order comment. |
| `deviationPoints` | `int` | Maximum price deviation in points. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<mt5_term_api.OrderSendData>` - Order send result with ticket number.

---

## BuyStopPoints

Places Buy Stop pending order using point-based offset from current Ask price.

### Signature

```csharp
public Task<mt5_term_api.OrderSendData> BuyStopPoints(
    this MT5Service svc,
    string symbol,
    double volume,
    double priceOffsetPoints,
    double? slPoints = null,
    double? tpPoints = null,
    string? comment = null,
    int deviationPoints = 0,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `volume` | `double` | Volume in lots. |
| `priceOffsetPoints` | `double` | Distance in points above current Ask (always positive). |
| `slPoints` | `double?` | Optional stop-loss distance in points below entry price. |
| `tpPoints` | `double?` | Optional take-profit distance in points above entry price. |
| `comment` | `string?` | Optional order comment. |
| `deviationPoints` | `int` | Maximum price deviation in points. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<mt5_term_api.OrderSendData>` - Order send result with ticket number.

---

## SellStopPoints

Places Sell Stop pending order using point-based offset from current Bid price.

### Signature

```csharp
public Task<mt5_term_api.OrderSendData> SellStopPoints(
    this MT5Service svc,
    string symbol,
    double volume,
    double priceOffsetPoints,
    double? slPoints = null,
    double? tpPoints = null,
    string? comment = null,
    int deviationPoints = 0,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `volume` | `double` | Volume in lots. |
| `priceOffsetPoints` | `double` | Distance in points below current Bid (always positive). |
| `slPoints` | `double?` | Optional stop-loss distance in points above entry price. |
| `tpPoints` | `double?` | Optional take-profit distance in points below entry price. |
| `comment` | `string?` | Optional order comment. |
| `deviationPoints` | `int` | Maximum price deviation in points. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 15). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<mt5_term_api.OrderSendData>` - Order send result with ticket number.

---

## BuyMarketByRisk

Opens Buy market position with volume calculated based on risk amount and stop-loss distance.

### Signature

```csharp
public Task<OrderSendData> BuyMarketByRisk(
    this MT5Service svc,
    string symbol,
    double stopPoints,
    double riskMoney,
    double? tpPoints = null,
    string? comment = null,
    int deviationPoints = 0,
    int timeoutSec = 20,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `stopPoints` | `double` | Stop-loss distance in points. |
| `riskMoney` | `double` | Maximum amount to risk (in account currency). |
| `tpPoints` | `double?` | Optional take-profit distance in points. |
| `comment` | `string?` | Optional order comment. |
| `deviationPoints` | `int` | Maximum price deviation in points. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 20). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<OrderSendData>` - Order send result with ticket number.

### Remarks

Combines CalcVolumeForRiskAsync and market order execution in one method.

---

## SellMarketByRisk

Opens Sell market position with volume calculated based on risk amount and stop-loss distance.

### Signature

```csharp
public Task<OrderSendData> SellMarketByRisk(
    this MT5Service svc,
    string symbol,
    double stopPoints,
    double riskMoney,
    double? tpPoints = null,
    string? comment = null,
    int deviationPoints = 0,
    int timeoutSec = 20,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol name (e.g., "EURUSD"). |
| `stopPoints` | `double` | Stop-loss distance in points. |
| `riskMoney` | `double` | Maximum amount to risk (in account currency). |
| `tpPoints` | `double?` | Optional take-profit distance in points. |
| `comment` | `string?` | Optional order comment. |
| `deviationPoints` | `int` | Maximum price deviation in points. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 20). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<OrderSendData>` - Order send result with ticket number.

### Remarks

Combines CalcVolumeForRiskAsync and market order execution in one method.

---

## CancelAll

Cancels all pending orders with optional filtering by symbol and direction.

### Signature

```csharp
public Task<int> CancelAll(
    this MT5Service svc,
    string? symbol = null,
    bool? isBuy = null,
    int timeoutSec = 30,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string?` | Optional symbol filter. Pass null to cancel all symbols. |
| `isBuy` | `bool?` | Optional direction filter. True = cancel only buy orders, False = cancel only sell orders, null = cancel both. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 30). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<int>` - Number of pending orders cancelled.

### Remarks

Only affects pending orders (Limit, Stop, Stop Limit). Market positions are not cancelled.

---

## CloseAllPositions

Closes all open market positions with optional filtering by symbol and direction.

### Signature

```csharp
public Task<int> CloseAllPositions(
    this MT5Service svc,
    string? symbol = null,
    bool? isBuy = null,
    int timeoutSec = 30,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string?` | Optional symbol filter. Pass null to close all symbols. |
| `isBuy` | `bool?` | Optional direction filter. True = close only buy positions, False = close only sell positions, null = close both. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 30). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<int>` - Number of market positions closed.

### Remarks

Only affects market positions (BUY/SELL). Pending orders are not affected.

---

## CloseAllPending

Alias for CancelAll. Cancels all pending orders with optional filtering.

### Signature

```csharp
public Task<int> CloseAllPending(
    this MT5Service svc,
    string? symbol = null,
    bool? isBuy = null,
    int timeoutSec = 30,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string?` | Optional symbol filter. |
| `isBuy` | `bool?` | Optional direction filter. |
| `timeoutSec` | `int` | RPC timeout in seconds (default: 30). |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<int>` - Number of pending orders cancelled.

---

## SubscribeToMarketBookAsync

Subscribe to Market Depth (order book) for a symbol and return a disposable subscription. Automatically releases the subscription when disposed.

### Signature

```csharp
public Task<IDisposable> SubscribeToMarketBookAsync(
    this MT5Service svc,
    string symbol,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol to subscribe (e.g., "EURUSD"). |
| `timeoutSec` | `int` | RPC timeout in seconds. |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<IDisposable>` - IDisposable that releases the subscription when disposed.

---

## GetMarketBookSnapshotAsync

Gets the current Market Depth (order book) snapshot for a symbol. You must subscribe first using SubscribeToMarketBookAsync or MarketBookAddAsync.

### Signature

```csharp
public Task<MarketBookGetData> GetMarketBookSnapshotAsync(
    this MT5Service svc,
    string symbol,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol to query (e.g., "EURUSD"). |
| `timeoutSec` | `int` | RPC timeout in seconds. |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<MarketBookGetData>` - MarketBookGetData with buy/sell book entries.

---

## GetBestBidAskFromBookAsync

Extracts best bid and ask prices from the current order book. Returns (bestBid, bestAsk) or (0, 0) if book is empty.

### Signature

```csharp
public Task<(double bestBid, double bestAsk)> GetBestBidAskFromBookAsync(
    this MT5Service svc,
    string symbol,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol to query (e.g., "EURUSD"). |
| `timeoutSec` | `int` | RPC timeout in seconds. |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<(double bestBid, double bestAsk)>` - Tuple (bestBid, bestAsk).

---

## CalculateLiquidityAtLevelAsync

Calculates total liquidity (volume) available at a specific price level in the order book.

### Signature

```csharp
public Task<long> CalculateLiquidityAtLevelAsync(
    this MT5Service svc,
    string symbol,
    double price,
    bool isBuy,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol to query (e.g., "EURUSD"). |
| `price` | `double` | Price level to check. |
| `isBuy` | `bool` | True for buy side (bids), false for sell side (asks). |
| `timeoutSec` | `int` | RPC timeout in seconds. |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<long>` - Total volume at the price level.

---

## ValidateOrderAsync

Validates an order before sending (pre-flight check). Checks symbol availability, lot size constraints, margin requirements, etc.

### Signature

```csharp
public Task<OrderCheckData> ValidateOrderAsync(
    this MT5Service svc,
    OrderCheckRequest request,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `request` | `OrderCheckRequest` | OrderCheckRequest with MqlTradeRequest populated. |
| `timeoutSec` | `int` | RPC timeout in seconds. |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<OrderCheckData>` - OrderCheckData with validation result and potential error codes.

---

## CalculateBuyMarginAsync

Calculates margin required for a BUY order before placing it.

### Signature

```csharp
public Task<double> CalculateBuyMarginAsync(
    this MT5Service svc,
    string symbol,
    double volume,
    double price = 0,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol to trade. |
| `volume` | `double` | Volume in lots. |
| `price` | `double` | Entry price (0 for market price). |
| `timeoutSec` | `int` | RPC timeout in seconds. |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<double>` - Required margin amount.

---

## CalculateSellMarginAsync

Calculates margin required for a SELL order before placing it.

### Signature

```csharp
public Task<double> CalculateSellMarginAsync(
    this MT5Service svc,
    string symbol,
    double volume,
    double price = 0,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol to trade. |
| `volume` | `double` | Volume in lots. |
| `price` | `double` | Entry price (0 for market price). |
| `timeoutSec` | `int` | RPC timeout in seconds. |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<double>` - Required margin amount.

---

## CheckMarginAvailabilityAsync

Checks if account has enough free margin for a trade. Returns (hasEnoughMargin, freeMargin, requiredMargin).

### Signature

```csharp
public Task<(bool hasEnough, double freeMargin, double required)> CheckMarginAvailabilityAsync(
    this MT5Service svc,
    string symbol,
    double volume,
    bool isBuy,
    double price = 0,
    int timeoutSec = 20,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol to trade. |
| `volume` | `double` | Volume in lots. |
| `isBuy` | `bool` | True for BUY, false for SELL. |
| `price` | `double` | Entry price (0 for market price). |
| `timeoutSec` | `int` | RPC timeout in seconds. |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<(bool hasEnough, double freeMargin, double required)>` - Tuple (hasEnoughMargin, freeMargin, requiredMargin).

---

## GetQuoteSessionAsync

Gets quote session information for a symbol. Convenience wrapper with default timeout.

### Signature

```csharp
public Task<SymbolInfoSessionQuoteData> GetQuoteSessionAsync(
    this MT5Service svc,
    string symbol,
    mt5_term_api.DayOfWeek dayOfWeek,
    int sessionIndex = 0,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol to query. |
| `dayOfWeek` | `mt5_term_api.DayOfWeek` | Day of week. |
| `sessionIndex` | `int` | Session index (0 = first session, 1 = second, etc.). |
| `timeoutSec` | `int` | RPC timeout in seconds. |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<SymbolInfoSessionQuoteData>` - SymbolInfoSessionQuoteData.

---

## GetTradeSessionAsync

Gets trade session information for a symbol. Convenience wrapper with default timeout.

### Signature

```csharp
public Task<SymbolInfoSessionTradeData> GetTradeSessionAsync(
    this MT5Service svc,
    string symbol,
    mt5_term_api.DayOfWeek dayOfWeek,
    int sessionIndex = 0,
    int timeoutSec = 15,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string` | Symbol to query. |
| `dayOfWeek` | `mt5_term_api.DayOfWeek` | Day of week. |
| `sessionIndex` | `int` | Session index (0 = first session, 1 = second, etc.). |
| `timeoutSec` | `int` | RPC timeout in seconds. |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<SymbolInfoSessionTradeData>` - SymbolInfoSessionTradeData.

---

## GetProfitablePositionsAsync

Gets all currently profitable positions (P&amp;L &gt; 0).

### Signature

```csharp
public Task<List<object>> GetProfitablePositionsAsync(
    this MT5Service svc,
    string? symbol = null,
    int timeoutSec = 20,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string?` | Optional symbol filter (null = all symbols). |
| `timeoutSec` | `int` | RPC timeout in seconds. |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<List<object>>` - List of profitable positions.

---

## GetLosingPositionsAsync

Gets all currently losing positions (P&amp;L &lt; 0).

### Signature

```csharp
public Task<List<object>> GetLosingPositionsAsync(
    this MT5Service svc,
    string? symbol = null,
    int timeoutSec = 20,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string?` | Optional symbol filter (null = all symbols). |
| `timeoutSec` | `int` | RPC timeout in seconds. |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<List<object>>` - List of losing positions.

---

## GetTotalProfitLossAsync

Calculates total profit/loss across all open positions.

### Signature

```csharp
public Task<double> GetTotalProfitLossAsync(
    this MT5Service svc,
    string? symbol = null,
    int timeoutSec = 20,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string?` | Optional symbol filter (null = all symbols). |
| `timeoutSec` | `int` | RPC timeout in seconds. |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<double>` - Total P&amp;L.

---

## GetPositionCountAsync

Gets count of open positions (optionally filtered by symbol).

### Signature

```csharp
public Task<int> GetPositionCountAsync(
    this MT5Service svc,
    string? symbol = null,
    int timeoutSec = 20,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `symbol` | `string?` | Optional symbol filter (null = all symbols). |
| `timeoutSec` | `int` | RPC timeout in seconds. |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<int>` - Number of open positions.

---

## GetPositionStatsBySymbolAsync

Aggregates position statistics by symbol. Returns dictionary: symbol -> (count, totalVolume, totalPnL).

### Signature

```csharp
public Task<Dictionary<string, (int count, double totalVolume, double totalPnL)>>
            GetPositionStatsBySymbolAsync(
    this MT5Service svc,
    int timeoutSec = 20,
    CancellationToken ct = default
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `svc` | `MT5Service` | MT5Service instance. |
| `timeoutSec` | `int` | RPC timeout in seconds. |
| `ct` | `CancellationToken` | Cancellation token. |

### Returns

`Task<Dictionary<string, (int count, double totalVolume, double totalPnL)>>
           ` - Dictionary of position statistics per symbol.

---

## AccountSnapshot

Composite snapshot containing account summary and all currently opened orders/positions. Provides a complete view of account state at a single point in time.

### Signature

```csharp
public record AccountSnapshot(
    AccountSummaryData Summary,
    OpenedOrdersData OpenedOrders
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `Summary` | `AccountSummaryData` | Account summary with balance, equity, margin, and other account-level metrics. |
| `OpenedOrders` | `OpenedOrdersData` | All opened orders and positions sorted by open time (ascending). |

### Remarks

This record combines two frequently-needed pieces of information into one atomic snapshot. Useful for dashboards, risk monitoring, and account state logging.

---

## SymbolSnapshot

Composite snapshot containing essential symbol trading parameters at a single point in time. Includes current tick, price precision, and margin requirements.

### Signature

```csharp
public record SymbolSnapshot(
    MrpcMqlTick Tick,
    double Point,
    int Digits,
    SymbolInfoMarginRateData MarginRate
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `Tick` | `MrpcMqlTick` | Current tick data (bid, ask, last, volume, time). |
| `Point` | `double` | Minimal price change step (e.g., 0.00001 for EURUSD). |
| `Digits` | `int` | Number of decimal places in price quotes (e.g., 5 for EURUSD). |
| `MarginRate` | `SymbolInfoMarginRateData` | Margin rate data for BUY orders (initial margin, maintenance margin). |

### Remarks

This record bundles the most commonly-needed symbol properties for trading operations. Particularly useful for order validation, price normalization, and risk calculations.

---

