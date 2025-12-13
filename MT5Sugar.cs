/*══════════════════════════════════════════════════════════════════════════════
 FILE: MT5Sugar.cs (formerly MT5Service.Extensions.cs)

 PURPOSE:
   High-level convenience extensions ("sugar") over the thin MT5Service facade.
   Keeps MT5Service itself 1:1 with MT5Account; all UX helpers live here.

   This file provides a rich API layer with 45+ extension methods organized into
   15 functional regions, covering everything from basic symbol operations to
   advanced risk management and position monitoring.

 ARCHITECTURE:
   Three-tier design:
   ┌─────────────────────────────────────────────────────────────────────────┐
   │  MT5Sugar (THIS FILE)     - High-level convenience methods              │
   │                             - Risk management, bulk operations          │
   │                             - Point-based helpers, snapshots            │
   ├─────────────────────────────────────────────────────────────────────────┤
   │  MT5Service               - Mid-level facade (unwrapped primitives)     │
   │                             - Direct gRPC call wrappers                 │
   ├─────────────────────────────────────────────────────────────────────────┤
   │  MT5Account               - Low-level gRPC client                       │
   │                             - Connection management, streaming          │
   └─────────────────────────────────────────────────────────────────────────┘

 CONTENTS BY REGION:

 [01] INFRASTRUCTURE (2 methods)
      • Dl()                    - Deadline helper (private)
      • EnsureSelected()        - Ensure symbol is in Market Watch

 [02] SNAPSHOTS (4 methods + 2 classes)
      • GetAccountSnapshot()    - Account info snapshot
      • AccountSnapshot         - Immutable account state
      • GetSymbolSnapshot()     - Symbol info snapshot
      • SymbolSnapshot          - Immutable symbol state

 [03] NORMALIZATION & UTILS (5 methods)
      • GetPointAsync()         - Get symbol point size
      • GetDigitsAsync()        - Get symbol digits
      • NormalizePriceAsync()   - Normalize price to tick size
      • PointsToPipsAsync()     - Convert points to pips
      • GetSpreadPointsAsync()  - Get current spread in points

 [04] HISTORY HELPERS (2 methods)
      • OrdersHistoryLast()     - Get recent closed orders
      • PositionsHistoryPaged() - Get paginated position history

 [05] STREAMS HELPERS (2 methods)
      • ReadTicks()             - Async enumerable tick stream (bounded)
      • ReadTrades()            - Async enumerable trade event stream (bounded)

 [06] TRADING — MARKET & PENDING (5 methods)
      • PlaceMarket()           - Open market order (BUY/SELL)
      • PlacePending()          - Place pending order (Limit/Stop)
      • ModifySlTpAsync()       - Modify SL/TP for existing order
      • CloseByTicket()         - Close order by ticket
      • CloseAll()              - Close all orders (with filters)

 [07] VOLUME & PRICE UTILITIES (5 methods)
      • GetVolumeLimitsAsync()  - Get min/max/step volume
      • NormalizeVolumeAsync()  - Normalize volume to broker constraints
      • GetTickValueAndSizeAsync() - Get tick value & tick size
      • PriceFromOffsetPointsAsync() - Calculate price from point offset
      • CalcVolumeForRiskAsync() - Calculate lot size by risk amount ⭐

 [08] PENDING HELPERS (BY POINTS) (6 methods)
      • SetOperationByCode()    - Set order type via reflection (private)
      • BuildPendingRequest()   - Build pending order request (private)
      • BuyLimitPoints()        - Place Buy Limit by points offset
      • SellLimitPoints()       - Place Sell Limit by points offset
      • BuyStopPoints()         - Place Buy Stop by points offset
      • SellStopPoints()        - Place Sell Stop by points offset

 [09] MARKET BY RISK (5 methods)
      • BuyMarketByRisk()       - Buy with auto-calculated volume by risk ⭐
      • SellMarketByRisk()      - Sell with auto-calculated volume by risk ⭐
      • GetOrderTypeInt()       - Get order type as int (private)
      • IsBuyType()             - Check if order type is buy (private)
      • IsMarketType()          - Check if order type is market (private)

 [10] BULK CONVENIENCE (3 methods)
      • CancelAll()             - Cancel all pending orders (with filters)
      • CloseAllPositions()     - Close all market positions (with filters)
      • CloseAllPending()       - Alias for CancelAll

 [11] MARKET DEPTH (DOM) (5 methods + 1 class)
      • SubscribeToMarketBookAsync() - Subscribe to order book
      • MarketBookSubscription  - Disposable subscription wrapper
      • GetMarketBookSnapshotAsync() - Get current order book snapshot
      • GetBestBidAskFromBookAsync() - Extract best bid/ask from book
      • CalculateLiquidityAtLevelAsync() - Calculate liquidity at price level

 [12] ORDER VALIDATION (4 methods)
      • ValidateOrderAsync()    - Pre-flight order validation
      • CalculateBuyMarginAsync() - Calculate margin for buy order
      • CalculateSellMarginAsync() - Calculate margin for sell order
      • CheckMarginAvailabilityAsync() - Check if enough margin available ⭐

 [13] SESSION TIME (2 methods)
      • GetQuoteSessionAsync()  - Get quote session info
      • GetTradeSessionAsync()  - Get trade session info

 [14] POSITION MONITORING (5 methods)
      • GetProfitablePositionsAsync() - Get all profitable positions
      • GetLosingPositionsAsync()     - Get all losing positions
      • GetTotalProfitLossAsync()     - Calculate total P&L
      • GetPositionCountAsync()       - Count open positions
      • GetPositionStatsBySymbolAsync() - Aggregate stats by symbol

 TOTAL: 45+ public methods + 5 private helpers + 3 data classes

 KEY FEATURES:
   ✓ Risk-based position sizing (CalcVolumeForRiskAsync)
   ✓ Point-based pending orders (no manual price calculation)
   ✓ Bulk operations (close/cancel all with filters)
   ✓ Market depth (order book) helpers
   ✓ Position monitoring & P&L tracking
   ✓ Pre-flight order validation
   ✓ Bounded streaming helpers (safe tick/trade streams)
   ✓ Immutable snapshots (account & symbol state)

 NOTES:
   • All methods use async/await pattern
   • Timeout defaults: 10-30 seconds depending on operation
   • Reflection used to avoid hard enum dependencies on gRPC types
   • Private Dl() helper converts relative timeouts to absolute deadlines

══════════════════════════════════════════════════════════════════════════════*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using mt5_term_api;
using System.Linq;

namespace MetaRPC.CSharpMT5
{
    /// <summary>Convenience extensions ("sugar") grouped by domains for <see cref="MT5Service"/>.</summary>
    public static class MT5ServiceExtensions

    {
        // ─────────────────────────────────────────────────────────────────────
           #region [01] INFRASTRUCTURE
        // ─────────────────────────────────────────────────────────────────────


        /// <summary>
        /// Creates a per-call gRPC deadline from current UTC time plus specified seconds.
        /// Internal helper used throughout MT5Sugar for consistent timeout handling.
        /// </summary>
        /// <param name="seconds">Number of seconds from now for the deadline.</param>
        /// <returns>DateTime representing the absolute deadline in UTC.</returns>
        /// <remarks>
        /// This is a private helper method that converts relative timeouts (seconds) to absolute deadlines (DateTime).
        /// All public methods in MT5Sugar accept timeoutSec parameters and internally convert them via this helper.
        /// </remarks>
        private static DateTime Dl(int seconds) => DateTime.UtcNow.AddSeconds(seconds);

        /// <summary>
        /// Ensures the specified symbol is selected in the MT5 terminal and fully synchronized.
        /// Required before performing operations on symbols that may not be in Market Watch.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name to select and synchronize (e.g., "EURUSD", "XAUUSD").</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 10).</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>Task that completes when symbol is selected and synchronized.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the symbol cannot be synchronized in the terminal.
        /// This typically occurs if the symbol doesn't exist or broker doesn't provide it.
        /// </exception>
        /// <remarks>
        /// <para>This is a critical helper for symbol operations. Many MT5 functions require symbols to be:</para>
        /// <list type="bullet">
        ///   <item><description>1. Selected (added to Market Watch)</description></item>
        ///   <item><description>2. Synchronized (receiving live quotes from broker)</description></item>
        /// </list>
        /// <para>Without this, operations like getting symbol info, placing orders, or retrieving ticks may fail.</para>
        /// <para>PERFORMANCE NOTE: This method makes 2 sequential RPC calls. Cache symbols when possible.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Ensure EURUSD is ready before getting tick data
        /// await service.EnsureSelected("EURUSD");
        /// var tick = await service.SymbolInfoTickAsync("EURUSD");
        ///
        /// // Safe usage in a helper method
        /// public async Task GetPrice(string symbol)
        /// {
        ///     await service.EnsureSelected(symbol);
        ///     return await service.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolBid);
        /// }
        /// </code>
        /// </example>
        public static async Task EnsureSelected(this MT5Service svc, string symbol, int timeoutSec = 10, CancellationToken ct = default)
        {
            await svc.SymbolSelectAsync(symbol, selected: true, Dl(timeoutSec), ct);
            var sync = await svc.SymbolIsSynchronizedAsync(symbol, Dl(timeoutSec), ct);
            if (!sync)
                throw new InvalidOperationException($"Symbol '{symbol}' is not synchronized in terminal.");
        }

        #endregion
        // ─────────────────────────────────────────────────────────────────────
           #region [02] SNAPSHOTS
        // ─────────────────────────────────────────────────────────────────────


        /// <summary>
        /// Composite snapshot containing account summary and all currently opened orders/positions.
        /// Provides a complete view of account state at a single point in time.
        /// </summary>
        /// <param name="Summary">Account summary with balance, equity, margin, and other account-level metrics.</param>
        /// <param name="OpenedOrders">All opened orders and positions sorted by open time (ascending).</param>
        /// <remarks>
        /// This record combines two frequently-needed pieces of information into one atomic snapshot.
        /// Useful for dashboards, risk monitoring, and account state logging.
        /// </remarks>
        public sealed record AccountSnapshot(AccountSummaryData Summary, OpenedOrdersData OpenedOrders);

        /// <summary>
        /// Retrieves a complete account snapshot including summary data and all opened orders/positions.
        /// Optimizes performance by fetching both datasets in parallel where possible.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 15).</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>
        /// <see cref="AccountSnapshot"/> containing account summary and opened orders.
        /// </returns>
        /// <remarks>
        /// <para>This method combines two RPC calls into a single operation:</para>
        /// <list type="number">
        ///   <item><description>AccountSummaryAsync - gets balance, equity, margin, profit, etc.</description></item>
        ///   <item><description>OpenedOrdersAsync - gets all active orders and positions</description></item>
        /// </list>
        /// <para>PERFORMANCE: Both calls use the same deadline for consistency.</para>
        /// <para>USE CASES:</para>
        /// <list type="bullet">
        ///   <item><description>Dashboard updates showing account state + active trades</description></item>
        ///   <item><description>Risk monitoring (check margin level + open positions)</description></item>
        ///   <item><description>Periodic state logging/auditing</description></item>
        ///   <item><description>Pre-trade validation (check available margin before opening)</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Get complete account state
        /// var snapshot = await service.GetAccountSnapshot();
        ///
        /// Console.WriteLine($"Balance: {snapshot.Summary.Balance}");
        /// Console.WriteLine($"Equity: {snapshot.Summary.Equity}");
        /// Console.WriteLine($"Free Margin: {snapshot.Summary.FreeMargin}");
        /// Console.WriteLine($"Open Positions: {snapshot.OpenedOrders.Orders.Count}");
        ///
        /// // Risk check before trading
        /// if (snapshot.Summary.MarginLevel &lt; 200)
        ///     Console.WriteLine("WARNING: Low margin level!");
        /// </code>
        /// </example>
        public static async Task<AccountSnapshot> GetAccountSnapshot(this MT5Service svc, int timeoutSec = 15, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            var summary = await svc.AccountSummaryAsync(dl, ct);
            var opened = await svc.OpenedOrdersAsync(BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, dl, ct);
            return new AccountSnapshot(summary, opened);
        }

        /// <summary>
        /// Composite snapshot containing essential symbol trading parameters at a single point in time.
        /// Includes current tick, price precision, and margin requirements.
        /// </summary>
        /// <param name="Tick">Current tick data (bid, ask, last, volume, time).</param>
        /// <param name="Point">Minimal price change step (e.g., 0.00001 for EURUSD).</param>
        /// <param name="Digits">Number of decimal places in price quotes (e.g., 5 for EURUSD).</param>
        /// <param name="MarginRate">Margin rate data for BUY orders (initial margin, maintenance margin).</param>
        /// <remarks>
        /// This record bundles the most commonly-needed symbol properties for trading operations.
        /// Particularly useful for order validation, price normalization, and risk calculations.
        /// </remarks>
        public sealed record SymbolSnapshot(
            MrpcMqlTick Tick,
            double Point,
            int Digits,
            SymbolInfoMarginRateData MarginRate
        );

        /// <summary>
        /// Retrieves a complete trading snapshot for the specified symbol.
        /// Fetches current tick, price precision, and margin requirements in one operation.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name to snapshot (e.g., "EURUSD", "XAUUSD", "BTCUSD").</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 10).</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>
        /// <see cref="SymbolSnapshot"/> containing tick data, point size, digits, and margin rates.
        /// </returns>
        /// <remarks>
        /// <para>This method performs the following operations:</para>
        /// <list type="number">
        ///   <item><description>Ensures symbol is selected and synchronized</description></item>
        ///   <item><description>Fetches current tick (bid, ask, last price)</description></item>
        ///   <item><description>Retrieves point size (minimal price step)</description></item>
        ///   <item><description>Gets price precision (number of decimal digits)</description></item>
        ///   <item><description>Queries margin rate for BUY orders</description></item>
        /// </list>
        /// <para>PERFORMANCE: Makes 5 sequential RPC calls. Consider caching for frequently-used symbols.</para>
        /// <para>USE CASES:</para>
        /// <list type="bullet">
        ///   <item><description>Pre-trade validation (check current prices + margin)</description></item>
        ///   <item><description>Price normalization (round to correct digits)</description></item>
        ///   <item><description>Stop-loss/take-profit calculation (use point for pip distance)</description></item>
        ///   <item><description>Risk management (calculate margin requirements)</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Get complete symbol trading parameters
        /// var snapshot = await service.GetSymbolSnapshot("EURUSD");
        ///
        /// Console.WriteLine($"Bid: {snapshot.Tick.Bid}");
        /// Console.WriteLine($"Ask: {snapshot.Tick.Ask}");
        /// Console.WriteLine($"Point: {snapshot.Point}");
        /// Console.WriteLine($"Digits: {snapshot.Digits}");
        ///
        /// // Calculate stop-loss 50 pips below current bid
        /// double stopLoss = snapshot.Tick.Bid - (50 * snapshot.Point);
        ///
        /// // Round to correct precision
        /// stopLoss = Math.Round(stopLoss, snapshot.Digits);
        ///
        /// Console.WriteLine($"Stop Loss: {stopLoss}");
        /// </code>
        /// </example>
        public static async Task<SymbolSnapshot> GetSymbolSnapshot(this MT5Service svc, string symbol, int timeoutSec = 10, CancellationToken ct = default)
        {
            await svc.EnsureSelected(symbol, timeoutSec, ct);
            var dl = Dl(timeoutSec);

            var tick = await svc.SymbolInfoTickAsync(symbol, dl, ct);
            var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint, dl, ct);

            // Int64 → int conversion for digits
            var digits = (int)await svc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolDigits, dl, ct);

            // Safe variant: use enum value 0 as BUY (do not depend on member name)
            var margin = await svc.SymbolInfoMarginRateAsync(symbol, (mt5_term_api.ENUM_ORDER_TYPE)0, dl, ct);

            return new SymbolSnapshot(tick, point, digits, margin);
        }

        #endregion
        // ─────────────────────────────────────────────────────────────────────
           #region [03] NORMALIZATION & UTILS
        // ─────────────────────────────────────────────────────────────────────


        /// <summary>
        /// Retrieves the symbol's point value - the minimal price change step.
        /// Convenience wrapper around SymbolInfoDouble(SymbolPoint).
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD", "XAUUSD").</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 10).</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>Point value as a double (e.g., 0.00001 for 5-digit EURUSD, 0.01 for XAUUSD).</returns>
        /// <remarks>
        /// <para>POINT is the fundamental unit for price calculations in MT5:</para>
        /// <list type="bullet">
        ///   <item><description>5-digit EURUSD: point = 0.00001 (1 pip = 10 points)</description></item>
        ///   <item><description>3-digit USDJPY: point = 0.001 (1 pip = 10 points)</description></item>
        ///   <item><description>2-digit XAUUSD: point = 0.01 (1 pip = 1 point)</description></item>
        /// </list>
        /// <para>Used for calculating stop-loss/take-profit distances, spreads, and price movements.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Calculate 50-pip stop-loss
        /// double point = await service.GetPointAsync("EURUSD");
        /// double stopDistance = 50 * 10 * point;  // 50 pips * 10 points/pip * point size
        /// </code>
        /// </example>
        public static async Task<double> GetPointAsync(this MT5Service svc, string symbol, int timeoutSec = 10, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            return await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint, dl, ct);
        }

        /// <summary>
        /// Gets the number of decimal places used in price quotes for the specified symbol.
        /// Convenience wrapper around SymbolInfoInteger(SymbolDigits).
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD", "XAUUSD").</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 10).</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>Number of digits as integer (e.g., 5 for EURUSD, 2 for XAUUSD, 3 for USDJPY).</returns>
        /// <remarks>
        /// <para>Digits determine price precision and formatting:</para>
        /// <list type="bullet">
        ///   <item><description>5 digits: 1.08550 (typical for major FX pairs)</description></item>
        ///   <item><description>3 digits: 150.123 (typical for JPY pairs)</description></item>
        ///   <item><description>2 digits: 1925.50 (typical for gold/silver)</description></item>
        /// </list>
        /// <para>Use with Math.Round() to format prices correctly before displaying or sending to MT5.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Round price to symbol's precision
        /// int digits = await service.GetDigitsAsync("EURUSD");
        /// double price = 1.085551234;
        /// double rounded = Math.Round(price, digits);  // 1.08555
        /// </code>
        /// </example>
        public static async Task<int> GetDigitsAsync(this MT5Service svc, string symbol, int timeoutSec = 10, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            long v = await svc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolDigits, dl, ct);
            return (int)v;
        }

        /// <summary>
        /// Normalizes a price to the symbol's tick size (not just digits).
        /// Uses SYMBOL_TRADE_TICK_SIZE for strict broker-compliant rounding.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD", "XAUUSD").</param>
        /// <param name="price">Raw price value to normalize.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 10).</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>Normalized price that aligns with the symbol's tick size.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if SYMBOL_TRADE_TICK_SIZE is zero or invalid.
        /// </exception>
        /// <remarks>
        /// <para>IMPORTANT: This uses tick size, NOT digits. Tick size is broker-specific and more accurate.</para>
        /// <para>Example: Some brokers use tick size of 0.00005 for EURUSD (5 points), not 0.00001.</para>
        /// <para>Always use this method to normalize stop-loss, take-profit, and limit prices before sending orders.</para>
        /// <para>PERFORMANCE: Makes 1 RPC call. Consider caching tick size for repeated operations.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Normalize stop-loss price before order submission
        /// double rawStopLoss = 1.08547;
        /// double normalizedSL = await service.NormalizePriceAsync("EURUSD", rawStopLoss);
        ///
        /// // Use normalized price in order request
        /// var request = new OrderSendRequest
        /// {
        ///     Symbol = "EURUSD",
        ///     Volume = 0.01,
        ///     StopLoss = normalizedSL  // Guaranteed valid by broker
        /// };
        /// </code>
        /// </example>
        public static async Task<double> NormalizePriceAsync(this MT5Service svc, string symbol, double price, int timeoutSec = 10, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            // Strict normalization by SYMBOL_TRADE_TICK_SIZE (not by digits)
            var tickSize = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolTradeTickSize, dl, ct);
            if (tickSize <= 0) throw new InvalidOperationException("SymbolTradeTickSize is 0");
            var steps = Math.Round(price / tickSize);
            return steps * tickSize;
        }

        /// <summary>
        /// Converts points to pips based on symbol's digit precision.
        /// For 5-digit pairs: 10 points = 1 pip. For 3-digit pairs: 10 points = 1 pip.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD", "USDJPY").</param>
        /// <param name="points">Number of points to convert.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 10).</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>Equivalent value in pips (fractional).</returns>
        /// <remarks>
        /// <para>PIP (Percentage In Point) is the standard unit traders use for price movements:</para>
        /// <list type="bullet">
        ///   <item><description>5-digit EURUSD (1.08550): 1 pip = 10 points = 0.0010 move</description></item>
        ///   <item><description>3-digit USDJPY (150.123): 1 pip = 10 points = 0.10 move</description></item>
        ///   <item><description>2-digit XAUUSD (1925.50): 1 pip = 1 point = 1.00 move</description></item>
        /// </list>
        /// <para>Use this for human-readable distance reporting (e.g., "Stop loss is 20 pips away").</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Calculate spread in pips
        /// double spreadPoints = await service.GetSpreadPointsAsync("EURUSD");
        /// double spreadPips = await service.PointsToPipsAsync("EURUSD", spreadPoints);
        /// Console.WriteLine($"Spread: {spreadPips:F1} pips");  // e.g., "Spread: 1.5 pips"
        /// </code>
        /// </example>
        public static async Task<double> PointsToPipsAsync(this MT5Service svc, string symbol, double points, int timeoutSec = 10, CancellationToken ct = default)
        {
            var digits = await svc.GetDigitsAsync(symbol, timeoutSec, ct);
            var factor = Math.Pow(10, Math.Max(0, digits - 4));
            return points / factor;
        }

        /// <summary>
        /// Calculates the current spread (Ask - Bid) in points.
        /// Fetches latest tick and computes spread using symbol's point value.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD", "XAUUSD").</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 10).</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>Spread in points (e.g., 15.0 for a 1.5-pip spread on 5-digit EURUSD).</returns>
        /// <remarks>
        /// <para>Spread represents the broker's markup and liquidity cost.</para>
        /// <para>Typical spreads:</para>
        /// <list type="bullet">
        ///   <item><description>EURUSD: 10-20 points (1-2 pips) on ECN accounts</description></item>
        ///   <item><description>XAUUSD: 20-50 points (0.20-0.50 USD) depending on volatility</description></item>
        ///   <item><description>Exotic pairs: 50-200+ points</description></item>
        /// </list>
        /// <para>PERFORMANCE: Makes 2 RPC calls (tick + point). Monitor spread before trading.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Check if spread is acceptable before trading
        /// double spreadPoints = await service.GetSpreadPointsAsync("EURUSD");
        /// double spreadPips = await service.PointsToPipsAsync("EURUSD", spreadPoints);
        ///
        /// if (spreadPips > 3.0)
        /// {
        ///     Console.WriteLine($"WARNING: High spread ({spreadPips:F1} pips). Consider waiting.");
        ///     return;
        /// }
        ///
        /// // Spread is acceptable, proceed with order
        /// await service.BuyMarketAsync("EURUSD", 0.01);
        /// </code>
        /// </example>
        public static async Task<double> GetSpreadPointsAsync(this MT5Service svc, string symbol, int timeoutSec = 10, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            var tick = await svc.SymbolInfoTickAsync(symbol, dl, ct);
            var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint, dl, ct);
            return (tick.Ask - tick.Bid) / point;
        }

        #endregion
        // ─────────────────────────────────────────────────────────────────────
           #region [04] HISTORY HELPERS
        // ─────────────────────────────────────────────────────────────────────


        /// <summary>
        /// Retrieves closed orders history for the last N days with pagination support.
        /// Convenience wrapper that automatically calculates the date range from current time.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="days">Number of days to look back from today (default: 7).</param>
        /// <param name="page">Page number for pagination, zero-based (default: 0).</param>
        /// <param name="size">Number of records per page (default: 100).</param>
        /// <param name="sort">Sort order for results (default: by close time ascending).</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 20).</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>
        /// <see cref="OrdersHistoryData"/> containing closed orders within the specified date range.
        /// </returns>
        /// <remarks>
        /// <para>Fetches orders that were closed/cancelled within the last N days.</para>
        /// <para>Date range: from (now - N days) to (now), both in UTC.</para>
        /// <para>PAGINATION: Use page/size parameters to retrieve large histories incrementally.</para>
        /// <para>USE CASES:</para>
        /// <list type="bullet">
        ///   <item><description>Trading journal/audit (retrieve last week's trades)</description></item>
        ///   <item><description>Performance analysis (calculate win rate, avg profit)</description></item>
        ///   <item><description>Trade replication (replay recent orders)</description></item>
        ///   <item><description>Debugging (check what orders were executed)</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Get last 7 days of closed orders
        /// var history = await service.OrdersHistoryLast(days: 7);
        /// Console.WriteLine($"Found {history.Orders.Count} closed orders");
        ///
        /// // Get last 30 days, second page (orders 100-199)
        /// var page2 = await service.OrdersHistoryLast(days: 30, page: 1, size: 100);
        ///
        /// // Get today's orders sorted by close time descending
        /// var today = await service.OrdersHistoryLast(
        ///     days: 1,
        ///     sort: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc
        /// );
        /// </code>
        /// </example>
        public static Task<OrdersHistoryData> OrdersHistoryLast(this MT5Service svc,
            int days = 7, int page = 0, int size = 100,
            BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sort = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
            int timeoutSec = 20, CancellationToken ct = default)
        {
            var to = DateTime.UtcNow;
            var from = to.AddDays(-days);
            return svc.OrderHistoryAsync(from, to, sort, page, size, Dl(timeoutSec), ct);
        }

        /// <summary>
        /// Retrieves positions history with pagination and optional date filtering.
        /// Returns closed positions (deals) with full profit/loss information.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="sort">Sort order for results (default: by position open time ascending).</param>
        /// <param name="openFrom">Optional start date for position open time filter (UTC). Null = no lower bound.</param>
        /// <param name="openTo">Optional end date for position open time filter (UTC). Null = no upper bound.</param>
        /// <param name="page">Page number for pagination, zero-based (default: 0).</param>
        /// <param name="size">Number of records per page (default: 100).</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 20).</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>
        /// <see cref="PositionsHistoryData"/> containing closed positions with P&amp;L details.
        /// </returns>
        /// <remarks>
        /// <para>IMPORTANT: This retrieves CLOSED positions (deals), not current open positions.</para>
        /// <para>Each position includes:</para>
        /// <list type="bullet">
        ///   <item><description>Entry/exit prices and times</description></item>
        ///   <item><description>Realized profit/loss</description></item>
        ///   <item><description>Commission and swap charges</description></item>
        ///   <item><description>Position volume and symbol</description></item>
        /// </list>
        /// <para>PAGINATION: Essential for accounts with large trading history.</para>
        /// <para>USE CASES:</para>
        /// <list type="bullet">
        ///   <item><description>Performance analytics (calculate total profit, drawdown)</description></item>
        ///   <item><description>Strategy backtesting validation (compare algo vs actual)</description></item>
        ///   <item><description>Tax reporting (export closed positions for accounting)</description></item>
        ///   <item><description>Trade history review (analyze winning/losing patterns)</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Get all closed positions, first page
        /// var positions = await service.PositionsHistoryPaged();
        ///
        /// // Get positions closed in January 2024
        /// var jan2024 = await service.PositionsHistoryPaged(
        ///     openFrom: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        ///     openTo: new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)
        /// );
        ///
        /// // Calculate total profit from history
        /// double totalProfit = 0;
        /// foreach (var pos in positions.Positions)
        /// {
        ///     // Access profit using reflection (structure varies)
        ///     var profitProp = pos.GetType().GetProperty("Profit");
        ///     if (profitProp != null)
        ///         totalProfit += Convert.ToDouble(profitProp.GetValue(pos));
        /// }
        /// Console.WriteLine($"Total P&amp;L: ${totalProfit:F2}");
        /// </code>
        /// </example>
        public static Task<PositionsHistoryData> PositionsHistoryPaged(
            this MT5Service svc,
            AH_ENUM_POSITIONS_HISTORY_SORT_TYPE sort = AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeAsc,
            DateTime? openFrom = null, DateTime? openTo = null,
            int page = 0, int size = 100,
            int timeoutSec = 20, CancellationToken ct = default)
            => svc.PositionsHistoryAsync(sort, openFrom, openTo, page, size, Dl(timeoutSec), ct);

        #endregion
        // ─────────────────────────────────────────────────────────────────────
           #region [05] STREAMS HELPERS
        // ─────────────────────────────────────────────────────────────────────


        /// <summary>
        /// Reads a limited number of tick events from specified symbols or until duration timeout expires.
        /// Provides bounded streaming with automatic termination safeguards.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbols">Collection of symbol names to subscribe to (e.g., ["EURUSD", "XAUUSD"]).</param>
        /// <param name="maxEvents">Maximum number of tick events to read before stopping (default: 50).</param>
        /// <param name="durationSec">Maximum duration in seconds to read ticks before timeout (default: 5).</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>
        /// Async enumerable stream of <see cref="OnSymbolTickData"/> tick events.
        /// Stream automatically terminates when either maxEvents or durationSec is reached.
        /// </returns>
        /// <remarks>
        /// <para>BOUNDED STREAMING: Unlike raw OnSymbolTickAsync, this helper automatically limits the stream.</para>
        /// <para>TERMINATION CONDITIONS (whichever comes first):</para>
        /// <list type="number">
        ///   <item><description>Received maxEvents tick updates</description></item>
        ///   <item><description>Duration timeout (durationSec) expired</description></item>
        ///   <item><description>Cancellation token triggered</description></item>
        /// </list>
        /// <para>USE CASES:</para>
        /// <list type="bullet">
        ///   <item><description>Sample recent tick data for analysis</description></item>
        ///   <item><description>Monitor prices briefly before placing orders</description></item>
        ///   <item><description>Test streaming connectivity without infinite loops</description></item>
        ///   <item><description>Collect tick samples for spread/volatility calculation</description></item>
        /// </list>
        /// <para>PERFORMANCE: Creates a linked CancellationTokenSource for timeout management.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Read up to 20 tick updates from EURUSD, max 10 seconds
        /// await foreach (var tick in service.ReadTicks(["EURUSD"], maxEvents: 20, durationSec: 10))
        /// {
        ///     Console.WriteLine($"{tick.Symbol}: Bid={tick.Bid}, Ask={tick.Ask}");
        /// }
        ///
        /// // Monitor multiple symbols briefly
        /// var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY" };
        /// int tickCount = 0;
        /// await foreach (var tick in service.ReadTicks(symbols, maxEvents: 30, durationSec: 5))
        /// {
        ///     tickCount++;
        ///     Console.WriteLine($"[{tickCount}] {tick.Symbol}: {tick.Bid}");
        /// }
        /// </code>
        /// </example>
        public static async IAsyncEnumerable<OnSymbolTickData> ReadTicks(this MT5Service svc,
            IEnumerable<string> symbols, int maxEvents = 50, int durationSec = 5,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(durationSec));
            int count = 0;
            await foreach (var e in svc.OnSymbolTickAsync(symbols, cts.Token))
            {
                yield return e;
                if (++count >= maxEvents) yield break;
            }
        }

        /// <summary>
        /// Reads a limited number of trade transaction events or until duration timeout expires.
        /// Monitors order fills, modifications, and cancellations with automatic termination.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="maxEvents">Maximum number of trade events to read before stopping (default: 20).</param>
        /// <param name="durationSec">Maximum duration in seconds to read events before timeout (default: 5).</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>
        /// Async enumerable stream of <see cref="OnTradeData"/> trade transaction events.
        /// Stream automatically terminates when either maxEvents or durationSec is reached.
        /// </returns>
        /// <remarks>
        /// <para>TRADE EVENTS include:</para>
        /// <list type="bullet">
        ///   <item><description>Order placements (market, limit, stop)</description></item>
        ///   <item><description>Order fills and partial fills</description></item>
        ///   <item><description>Order modifications (SL/TP changes)</description></item>
        ///   <item><description>Order cancellations</description></item>
        ///   <item><description>Position opens/closes</description></item>
        /// </list>
        /// <para>BOUNDED STREAMING: Automatically limits the stream to prevent infinite loops.</para>
        /// <para>TERMINATION CONDITIONS (whichever comes first):</para>
        /// <list type="number">
        ///   <item><description>Received maxEvents trade updates</description></item>
        ///   <item><description>Duration timeout (durationSec) expired</description></item>
        ///   <item><description>Cancellation token triggered</description></item>
        /// </list>
        /// <para>USE CASES:</para>
        /// <list type="bullet">
        ///   <item><description>Monitor order execution confirmations</description></item>
        ///   <item><description>Audit recent trading activity</description></item>
        ///   <item><description>Test trade notification system</description></item>
        ///   <item><description>Verify stop-loss/take-profit triggers</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Monitor next 10 trade events
        /// await foreach (var trade in service.ReadTrades(maxEvents: 10, durationSec: 30))
        /// {
        ///     Console.WriteLine($"Trade: {trade.Type} - Order #{trade.Order}");
        ///
        ///     // Check if order was filled
        ///     if (trade.Type == TradeTransactionType.DealAdd)
        ///         Console.WriteLine($"  ✓ Filled at {trade.Price}");
        /// }
        ///
        /// // Wait for order confirmation with timeout
        /// bool orderConfirmed = false;
        /// await foreach (var trade in service.ReadTrades(maxEvents: 5, durationSec: 10))
        /// {
        ///     if (trade.Order == myOrderTicket)
        ///     {
        ///         orderConfirmed = true;
        ///         Console.WriteLine("Order confirmed!");
        ///         break;
        ///     }
        /// }
        /// </code>
        /// </example>
        public static async IAsyncEnumerable<OnTradeData> ReadTrades(this MT5Service svc,
            int maxEvents = 20, int durationSec = 5,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(durationSec));
            int count = 0;
            await foreach (var e in svc.OnTradeAsync(cts.Token))
            {
                yield return e;
                if (++count >= maxEvents) yield break;
            }
        }

        #endregion
        // ─────────────────────────────────────────────────────────────────────
           #region [06] TRADING — MARKET & PENDING
        // ─────────────────────────────────────────────────────────────────────


        /// <summary>
        /// Places a market order (BUY or SELL) with optional stop-loss and take-profit.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol to trade (e.g., "EURUSD").</param>
        /// <param name="volume">Volume in lots.</param>
        /// <param name="isBuy">True for BUY, false for SELL.</param>
        /// <param name="sl">Optional stop-loss price.</param>
        /// <param name="tp">Optional take-profit price.</param>
        /// <param name="comment">Optional order comment.</param>
        /// <param name="deviationPoints">Maximum price deviation in points.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 15).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Order send result with ticket number and execution details.</returns>
        /// <remarks>
        /// Uses reflection to set order type (BUY=0, SELL=1) to avoid hard enum dependency.
        /// Automatically ensures symbol is selected before placing order.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Buy 0.01 lot EURUSD with SL/TP
        /// var result = await service.PlaceMarket("EURUSD", 0.01, isBuy: true,
        ///     sl: 1.0800, tp: 1.0900);
        /// Console.WriteLine($"Order #{result.Order}, RetCode: {result.ReturnedCode}");
        /// </code>
        /// </example>
        public static async Task<mt5_term_api.OrderSendData> PlaceMarket(
            this MT5Service svc,
            string symbol, double volume, bool isBuy,
            double? sl = null, double? tp = null,
            string? comment = null, int deviationPoints = 0,
            int timeoutSec = 15, CancellationToken ct = default)
        {
            await svc.EnsureSelected(symbol, timeoutSec, ct);

            var req = new mt5_term_api.OrderSendRequest
            {
                Symbol = symbol,
                Volume = volume,
                // For market order, Price is not set
                StopLoss = sl ?? 0,
                TakeProfit = tp ?? 0,
                Comment = comment ?? string.Empty,
                Slippage = (ulong)Math.Max(0, deviationPoints),
            };

            // Set Operation via reflection to avoid hard dependency on enum symbol names (BUY=0, SELL=1)
            var opProp = typeof(mt5_term_api.OrderSendRequest).GetProperty("Operation");
            if (opProp is null)
                throw new InvalidOperationException("OrderSendRequest.Operation property not found.");

            var opEnumType = opProp.PropertyType;
            var opValue = Enum.ToObject(opEnumType, isBuy ? 0 : 1);
            opProp.SetValue(req, opValue);

            return await svc.OrderSendAsync(req, Dl(timeoutSec), ct);
        }

        /// <summary>
        /// Places a pending order (Buy/Sell Limit or Stop) at specified price.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol to trade.</param>
        /// <param name="volume">Volume in lots.</param>
        /// <param name="type">Order type (BuyLimit, SellLimit, BuyStop, SellStop).</param>
        /// <param name="price">Entry price for the pending order.</param>
        /// <param name="sl">Optional stop-loss price.</param>
        /// <param name="tp">Optional take-profit price.</param>
        /// <param name="comment">Optional order comment.</param>
        /// <param name="deviationPoints">Maximum price deviation in points.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 15).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Order send result with ticket number.</returns>
        /// <example>
        /// <code>
        /// // Place Buy Limit at 1.0850
        /// var result = await service.PlacePending("EURUSD", 0.01,
        ///     ENUM_ORDER_TYPE.OrderTypeBuyLimit, 1.0850,
        ///     sl: 1.0800, tp: 1.0900);
        /// </code>
        /// </example>
        public static async Task<mt5_term_api.OrderSendData> PlacePending(
            this MT5Service svc,
            string symbol, double volume,
            ENUM_ORDER_TYPE type,
            double price,
            double? sl = null, double? tp = null,
            string? comment = null, int deviationPoints = 0,
            int timeoutSec = 15, CancellationToken ct = default)
        {
            await svc.EnsureSelected(symbol, timeoutSec, ct);

            var req = new mt5_term_api.OrderSendRequest
            {
                Symbol = symbol,
                Volume = volume,
                Price = price,
                StopLoss = sl ?? 0,
                TakeProfit = tp ?? 0,
                Comment = comment ?? string.Empty,
                Slippage = (ulong)Math.Max(0, deviationPoints),
            };

            // Set Operation via reflection (value comes from external ENUM_ORDER_TYPE)
            var opProp = typeof(mt5_term_api.OrderSendRequest).GetProperty("Operation");
            if (opProp is null)
                throw new InvalidOperationException("OrderSendRequest.Operation property not found.");
            var opEnumType = opProp.PropertyType;
            var opValue = Enum.ToObject(opEnumType, (int)(object)type);
            opProp.SetValue(req, opValue);

            return await svc.OrderSendAsync(req, Dl(timeoutSec), ct);
        }

        /// <summary>
        /// Modifies stop-loss and/or take-profit for an existing order or position.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="ticket">Order or position ticket number.</param>
        /// <param name="slPrice">New stop-loss price (absolute value). Pass null to keep current SL unchanged.</param>
        /// <param name="tpPrice">New take-profit price (absolute value). Pass null to keep current TP unchanged.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 10).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Order modify result with execution details.</returns>
        /// <exception cref="ArgumentException">Thrown if both slPrice and tpPrice are null.</exception>
        /// <remarks>
        /// At least one parameter (slPrice or tpPrice) must be provided.
        /// Pass absolute price values, not relative points or offsets.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Modify only stop-loss
        /// await service.ModifySlTpAsync(ticket: 12345, slPrice: 1.0850);
        ///
        /// // Modify only take-profit
        /// await service.ModifySlTpAsync(ticket: 12345, tpPrice: 1.0950);
        ///
        /// // Modify both SL and TP
        /// await service.ModifySlTpAsync(ticket: 12345, slPrice: 1.0850, tpPrice: 1.0950);
        /// </code>
        /// </example>
        public static Task<OrderModifyData> ModifySlTpAsync(
            this MT5Service svc,
            ulong ticket,
            double? slPrice = null,
            double? tpPrice = null,
            int timeoutSec = 10,
            CancellationToken ct = default)
        {
            if (slPrice is null && tpPrice is null)
                throw new ArgumentException(
                    "Nothing to modify. Provide at least one parameter: slPrice or tpPrice. " +
                    "Example: ModifySlTpAsync(ticket, slPrice: 1.2345) or ModifySlTpAsync(ticket, tpPrice: 1.2400).");

            var req = new OrderModifyRequest { Ticket = ticket };

            // Set only explicitly provided fields (avoid accidental resets)
            if (slPrice is double sl) req.StopLoss = sl;
            if (tpPrice is double tp) req.TakeProfit = tp;

            return svc.OrderModifyAsync(req, Dl(timeoutSec), ct);
        }

        /// <summary>
        /// Closes an order or position by ticket number with optional partial volume.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="ticket">Order or position ticket number to close.</param>
        /// <param name="volume">Volume to close in lots. Pass null to close entire position.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 15).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Order close result with execution details.</returns>
        /// <remarks>
        /// For full position closure, pass volume as null.
        /// For partial closure, specify exact volume to close (must not exceed position volume).
        /// </remarks>
        /// <example>
        /// <code>
        /// // Close entire position
        /// await service.CloseByTicket(ticket: 12345);
        ///
        /// // Close 0.5 lots from position
        /// await service.CloseByTicket(ticket: 12345, volume: 0.5);
        /// </code>
        /// </example>
        public static Task<OrderCloseData> CloseByTicket(this MT5Service svc,
            ulong ticket, double? volume = null,
            int timeoutSec = 15, CancellationToken ct = default)
        {
            var req = new OrderCloseRequest { Ticket = ticket, Volume = volume ?? 0 };
            return svc.OrderCloseAsync(req, Dl(timeoutSec), ct);
        }

        /// <summary>
        /// Closes all open orders and positions with optional filtering by symbol and direction.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Optional symbol filter (e.g., "EURUSD"). Pass null to close all symbols.</param>
        /// <param name="isBuy">Optional direction filter. True = close only BUY orders, False = close only SELL orders, null = close both.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 30).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Number of orders/positions that were closed.</returns>
        /// <remarks>
        /// This method retrieves all open orders, applies filters, and closes matching positions sequentially.
        /// WARNING: Use with caution in live trading. Consider filters to avoid closing unintended positions.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Close all positions
        /// int closed = await service.CloseAll();
        ///
        /// // Close only EURUSD positions
        /// int closed = await service.CloseAll(symbol: "EURUSD");
        ///
        /// // Close only BUY positions on GBPUSD
        /// int closed = await service.CloseAll(symbol: "GBPUSD", isBuy: true);
        /// </code>
        /// </example>
        public static async Task<int> CloseAll(
            this MT5Service svc,
            string? symbol = null, bool? isBuy = null,
            int timeoutSec = 30, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            var opened = await svc.OpenedOrdersAsync(
                BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, dl, ct);

            var orders = EnumerateOrders(opened);
            int closed = 0;

            foreach (var o in orders)
            {
                var t = o.GetType();

                var symbolVal = (string?)t.GetProperty("Symbol")?.GetValue(o) ?? "";
                if (symbol is not null && !string.Equals(symbolVal, symbol, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (isBuy.HasValue)
                {
                    // enum -> int (Buy=0, Sell=1, BuyLimit=2, SellLimit=3, BuyStop=4, SellStop=5, BuyStopLimit=6, SellStopLimit=7)
                    var typeObj = t.GetProperty("Type")?.GetValue(o);
                    var typeInt = typeObj is null ? -1 : Convert.ToInt32(typeObj);

                    bool orderIsBuy =
                           typeInt == 0  // Buy
                        || typeInt == 2  // BuyLimit
                        || typeInt == 4  // BuyStop
                        || typeInt == 6; // BuyStopLimit

                    if (orderIsBuy != isBuy.Value) continue;
                }

                var ticket = Convert.ToUInt64(t.GetProperty("Ticket")?.GetValue(o) ?? 0UL);
                if (ticket != 0UL)
                {
                    await svc.CloseByTicket(ticket, /*volume*/ null, timeoutSec, ct);
                    closed++;
                }
            }

            return closed;
        }

        #endregion
        // ─────────────────────────────────────────────────────────────────────
           #region PRIVATE HELPER: ORDER ENUMERATION (Internal - Not Public API)
        // ─────────────────────────────────────────────────────────────────────
        

        private static IEnumerable<object> EnumerateOrders(object openedOrdersData)
        {
            var dataType = openedOrdersData.GetType();

            // 1) First priority: Try collection with "PositionInfo" (open positions)
            foreach (var p in dataType.GetProperties())
            {
                var pt = p.PropertyType;
                if (pt != typeof(string) &&
                    typeof(System.Collections.IEnumerable).IsAssignableFrom(pt))
                {
                    Type? elem = null;
                    if (pt.IsArray)
                        elem = pt.GetElementType();
                    else if (pt.IsGenericType)
                        elem = pt.GetGenericArguments().FirstOrDefault();

                    if (elem != null && elem.Name.Contains("PositionInfo", StringComparison.OrdinalIgnoreCase))
                    {
                        if (p.GetValue(openedOrdersData) is System.Collections.IEnumerable seq)
                        {
                            foreach (var item in seq) yield return item!;
                            yield break;
                        }
                    }
                }
            }

            // 2) Second priority: Try collection with "OpenedOrderInfo" (pending orders)
            foreach (var p in dataType.GetProperties())
            {
                var pt = p.PropertyType;
                if (pt != typeof(string) &&
                    typeof(System.Collections.IEnumerable).IsAssignableFrom(pt))
                {
                    Type? elem = null;
                    if (pt.IsArray)
                        elem = pt.GetElementType();
                    else if (pt.IsGenericType)
                        elem = pt.GetGenericArguments().FirstOrDefault();

                    if (elem != null && elem.Name.Contains("OpenedOrderInfo", StringComparison.OrdinalIgnoreCase))
                    {
                        if (p.GetValue(openedOrdersData) is System.Collections.IEnumerable seq)
                        {
                            foreach (var item in seq) yield return item!;
                            yield break;
                        }
                    }
                }
            }

            // 3) Fallback: property named "Orders", if present
            var ordersProp = dataType.GetProperty("Orders");
            if (ordersProp?.GetValue(openedOrdersData) is System.Collections.IEnumerable fallbackSeq)
            {
                foreach (var item in fallbackSeq) yield return item!;
            }
        }

        #endregion
        // ─────────────────────────────────────────────────────────────────────
           #region [07] VOLUME & PRICE UTILITIES
        // ─────────────────────────────────────────────────────────────────────
        

        /// <summary>
        /// Retrieves volume constraints for a symbol (minimum, maximum, and step size).
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD", "XAUUSD").</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 10).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Tuple containing minimum volume, maximum volume, and volume step.</returns>
        /// <remarks>
        /// Volume step defines the smallest increment allowed (e.g., 0.01 lots).
        /// Use this data to validate and normalize volumes before placing orders.
        /// </remarks>
        /// <example>
        /// <code>
        /// var (min, max, step) = await service.GetVolumeLimitsAsync("EURUSD");
        /// Console.WriteLine($"Min: {min}, Max: {max}, Step: {step}");
        /// </code>
        /// </example>
        public static async Task<(double min, double max, double step)> GetVolumeLimitsAsync(this MT5Service svc, string symbol, int timeoutSec = 10, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            var min = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeMin, dl, ct);
            var max = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeMax, dl, ct);
            var step = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeStep, dl, ct);
            return (min, max, step);
        }

        /// <summary>
        /// Normalizes volume to comply with symbol's step size and min/max limits.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="volume">Desired volume in lots.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 10).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Normalized volume that broker will accept.</returns>
        /// <remarks>
        /// Rounds volume to nearest valid step and clamps to min/max range.
        /// Always use before placing orders to avoid broker rejections.
        /// </remarks>
        /// <example>
        /// <code>
        /// double rawVolume = 0.037;
        /// double normalized = await service.NormalizeVolumeAsync("EURUSD", rawVolume);
        /// // Returns 0.04 if step is 0.01
        /// </code>
        /// </example>
        public static async Task<double> NormalizeVolumeAsync(this MT5Service svc, string symbol, double volume, int timeoutSec = 10, CancellationToken ct = default)
        {
            var (min, max, step) = await svc.GetVolumeLimitsAsync(symbol, timeoutSec, ct);
            if (step <= 0) step = 0.01; // safety net
            var n = Math.Round((volume - min) / step);
            var v = min + n * step;
            v = Math.Max(min, Math.Min(max, v));
            return v;
        }

        /// <summary>
        /// Retrieves tick value and tick size for risk and P&amp;L calculations.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 10).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Tuple containing tick value (monetary value per tick) and tick size (price increment).</returns>
        /// <remarks>
        /// Tick value represents how much money changes per one tick of price movement for 1 lot.
        /// Essential for accurate risk calculations and position sizing.
        /// </remarks>
        /// <example>
        /// <code>
        /// var (tickValue, tickSize) = await service.GetTickValueAndSizeAsync("EURUSD");
        /// Console.WriteLine($"Tick value: ${tickValue}, Tick size: {tickSize}");
        /// </code>
        /// </example>
        public static async Task<(double tickValue, double tickSize)> GetTickValueAndSizeAsync(this MT5Service svc, string symbol, int timeoutSec = 10, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            var tv = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolTradeTickValue, dl, ct);
            var ts = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolTradeTickSize, dl, ct);
            return (tv, ts);
        }

        /// <summary>
        /// Calculates pending order price by offset in points from current market price.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="type">Order type (BuyLimit, SellStop, etc.).</param>
        /// <param name="offsetPoints">Distance in points from current bid/ask.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 10).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Normalized price for pending order placement.</returns>
        /// <remarks>
        /// Automatically uses Ask for buy orders and Bid for sell orders.
        /// Result is normalized to symbol's tick size.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Buy Stop 50 points above current Ask
        /// double price = await service.PriceFromOffsetPointsAsync("EURUSD",
        ///     ENUM_ORDER_TYPE.OrderTypeBuyStop, offsetPoints: 50);
        /// </code>
        /// </example>
        public static async Task<double> PriceFromOffsetPointsAsync(this MT5Service svc, string symbol, ENUM_ORDER_TYPE type, double offsetPoints, int timeoutSec = 10, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            var tick = await svc.SymbolInfoTickAsync(symbol, dl, ct);
            var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint, dl, ct);

            // enum → int; avoid relying on specific enum member names
            int t = Convert.ToInt32((object)type);
            bool isBuy = t == 0 || t == 2 || t == 4 || t == 6; // Buy / BuyLimit / BuyStop / BuyStopLimit

            double basis = isBuy ? tick.Ask : tick.Bid;
            double raw = basis + (isBuy ? +1.0 : -1.0) * (offsetPoints * point);

            return await svc.NormalizePriceAsync(symbol, raw, timeoutSec, ct);
        }

        /// <summary>
        /// Calculates position size (volume in lots) based on risk amount and stop-loss distance.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="stopPoints">Stop-loss distance in points.</param>
        /// <param name="riskMoney">Maximum amount of money to risk (in account currency).</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 10).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Normalized volume in lots that matches the specified risk.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if stopPoints or riskMoney is zero or negative.</exception>
        /// <exception cref="InvalidOperationException">Thrown if tick size is invalid or calculation fails.</exception>
        /// <remarks>
        /// Core risk management method. Calculates exact lot size so that if stop-loss is hit,
        /// the loss equals the specified risk amount.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Risk $100 with 50-point stop-loss
        /// double volume = await service.CalcVolumeForRiskAsync("EURUSD",
        ///     stopPoints: 50, riskMoney: 100);
        /// // Returns volume like 0.20 lots (broker-normalized)
        /// </code>
        /// </example>
        public static async Task<double> CalcVolumeForRiskAsync(this MT5Service svc, string symbol, double stopPoints, double riskMoney, int timeoutSec = 10, CancellationToken ct = default)
        {
            if (stopPoints <= 0) throw new ArgumentOutOfRangeException(nameof(stopPoints));
            if (riskMoney <= 0) throw new ArgumentOutOfRangeException(nameof(riskMoney));

            var dl = Dl(timeoutSec);
            var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint, dl, ct);
            var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync(symbol, timeoutSec, ct);
            if (tickSize <= 0) throw new InvalidOperationException("TickSize is 0");

            var lossPerLot = (stopPoints * point / tickSize) * tickValue; // money for 1 lot
            if (lossPerLot <= 0) throw new InvalidOperationException("Computed lossPerLot <= 0");
            var vol = riskMoney / lossPerLot;
            return await svc.NormalizeVolumeAsync(symbol, vol, timeoutSec, ct);
        }
        #endregion


        // ─────────────────────────────────────────────────────────────────────
           #region [08] PENDING HELPERS (BY POINTS)
        // ─────────────────────────────────────────────────────────────────────
        

        private static void SetOperationByCode(mt5_term_api.OrderSendRequest req, int code)
        {
            var opProp = typeof(mt5_term_api.OrderSendRequest).GetProperty("Operation");
            if (opProp is null) throw new InvalidOperationException("OrderSendRequest.Operation not found.");
            var enumType = opProp.PropertyType;
            var enumVal = Enum.ToObject(enumType, code);
            opProp.SetValue(req, enumVal);
        }

        private static mt5_term_api.OrderSendRequest BuildPendingRequest(
            string symbol, double volume, double price,
            double sl, double tp, string? comment, int deviationPoints, int operationCode)
        {
            var req = new mt5_term_api.OrderSendRequest
            {
                Symbol = symbol,
                Volume = volume,
                Price = price,
                StopLoss = sl,
                TakeProfit = tp,
                Comment = comment ?? string.Empty,
                Slippage = (ulong)Math.Max(0, deviationPoints),
            };
            SetOperationByCode(req, operationCode);
            return req;
        }

        /// <summary>
        /// Places Buy Limit pending order using point-based offset from current Ask price.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="volume">Volume in lots.</param>
        /// <param name="priceOffsetPoints">Distance in points below current Ask (always positive).</param>
        /// <param name="slPoints">Optional stop-loss distance in points below entry price.</param>
        /// <param name="tpPoints">Optional take-profit distance in points above entry price.</param>
        /// <param name="comment">Optional order comment.</param>
        /// <param name="deviationPoints">Maximum price deviation in points.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 15).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Order send result with ticket number.</returns>
        /// <example>
        /// <code>
        /// // Buy Limit 10 points below Ask with 20-point SL and 30-point TP
        /// await service.BuyLimitPoints("EURUSD", 0.01, priceOffsetPoints: 10,
        ///     slPoints: 20, tpPoints: 30);
        /// </code>
        /// </example>
        public static async Task<mt5_term_api.OrderSendData> BuyLimitPoints(
            this MT5Service svc, string symbol, double volume, double priceOffsetPoints,
            double? slPoints = null, double? tpPoints = null, string? comment = null, int deviationPoints = 0,
            int timeoutSec = 15, CancellationToken ct = default)
        {
            await svc.EnsureSelected(symbol, timeoutSec, ct);

            var dl = Dl(timeoutSec);
            var tick = await svc.SymbolInfoTickAsync(symbol, dl, ct);
            var point = await svc.SymbolInfoDoubleAsync(symbol, mt5_term_api.SymbolInfoDoubleProperty.SymbolPoint, dl, ct);

            // BuyLimit: from ASK downward
            var rawPrice = tick.Ask - Math.Abs(priceOffsetPoints) * point;
            var price = await svc.NormalizePriceAsync(symbol, rawPrice, timeoutSec, ct);

            double sl = slPoints.HasValue ? price - slPoints.Value * point : 0;
            double tp = tpPoints.HasValue ? price + tpPoints.Value * point : 0;

            // BuyLimit = 2
            var req = BuildPendingRequest(symbol, volume, price, sl, tp, comment, deviationPoints, operationCode: 2);
            return await svc.OrderSendAsync(req, dl, ct);
        }

        /// <summary>
        /// Places Sell Limit pending order using point-based offset from current Bid price.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="volume">Volume in lots.</param>
        /// <param name="priceOffsetPoints">Distance in points above current Bid (always positive).</param>
        /// <param name="slPoints">Optional stop-loss distance in points above entry price.</param>
        /// <param name="tpPoints">Optional take-profit distance in points below entry price.</param>
        /// <param name="comment">Optional order comment.</param>
        /// <param name="deviationPoints">Maximum price deviation in points.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 15).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Order send result with ticket number.</returns>
        /// <example>
        /// <code>
        /// // Sell Limit 10 points above Bid
        /// await service.SellLimitPoints("EURUSD", 0.01, priceOffsetPoints: 10,
        ///     slPoints: 20, tpPoints: 30);
        /// </code>
        /// </example>
        public static async Task<mt5_term_api.OrderSendData> SellLimitPoints(
            this MT5Service svc, string symbol, double volume, double priceOffsetPoints,
            double? slPoints = null, double? tpPoints = null, string? comment = null, int deviationPoints = 0,
            int timeoutSec = 15, CancellationToken ct = default)
        {
            await svc.EnsureSelected(symbol, timeoutSec, ct);

            var dl = Dl(timeoutSec);
            var tick = await svc.SymbolInfoTickAsync(symbol, dl, ct);
            var point = await svc.SymbolInfoDoubleAsync(symbol, mt5_term_api.SymbolInfoDoubleProperty.SymbolPoint, dl, ct);

            // SellLimit: from BID upward
            var rawPrice = tick.Bid + Math.Abs(priceOffsetPoints) * point;
            var price = await svc.NormalizePriceAsync(symbol, rawPrice, timeoutSec, ct);

            double sl = slPoints.HasValue ? price + slPoints.Value * point : 0;
            double tp = tpPoints.HasValue ? price - tpPoints.Value * point : 0;

            // SellLimit = 3
            var req = BuildPendingRequest(symbol, volume, price, sl, tp, comment, deviationPoints, operationCode: 3);
            return await svc.OrderSendAsync(req, dl, ct);
        }

        /// <summary>
        /// Places Buy Stop pending order using point-based offset from current Ask price.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="volume">Volume in lots.</param>
        /// <param name="priceOffsetPoints">Distance in points above current Ask (always positive).</param>
        /// <param name="slPoints">Optional stop-loss distance in points below entry price.</param>
        /// <param name="tpPoints">Optional take-profit distance in points above entry price.</param>
        /// <param name="comment">Optional order comment.</param>
        /// <param name="deviationPoints">Maximum price deviation in points.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 15).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Order send result with ticket number.</returns>
        /// <example>
        /// <code>
        /// // Buy Stop 20 points above Ask
        /// await service.BuyStopPoints("EURUSD", 0.01, priceOffsetPoints: 20,
        ///     slPoints: 30, tpPoints: 50);
        /// </code>
        /// </example>
        public static async Task<mt5_term_api.OrderSendData> BuyStopPoints(
            this MT5Service svc, string symbol, double volume, double priceOffsetPoints,
            double? slPoints = null, double? tpPoints = null, string? comment = null, int deviationPoints = 0,
            int timeoutSec = 15, CancellationToken ct = default)
        {
            await svc.EnsureSelected(symbol, timeoutSec, ct);

            var dl = Dl(timeoutSec);
            var tick = await svc.SymbolInfoTickAsync(symbol, dl, ct);
            var point = await svc.SymbolInfoDoubleAsync(symbol, mt5_term_api.SymbolInfoDoubleProperty.SymbolPoint, dl, ct);

            // BuyStop: from ASK upward
            var rawPrice = tick.Ask + Math.Abs(priceOffsetPoints) * point;
            var price = await svc.NormalizePriceAsync(symbol, rawPrice, timeoutSec, ct);

            double sl = slPoints.HasValue ? price - slPoints.Value * point : 0;
            double tp = tpPoints.HasValue ? price + tpPoints.Value * point : 0;

            // BuyStop = 4
            var req = BuildPendingRequest(symbol, volume, price, sl, tp, comment, deviationPoints, operationCode: 4);
            return await svc.OrderSendAsync(req, dl, ct);
        }

        /// <summary>
        /// Places Sell Stop pending order using point-based offset from current Bid price.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="volume">Volume in lots.</param>
        /// <param name="priceOffsetPoints">Distance in points below current Bid (always positive).</param>
        /// <param name="slPoints">Optional stop-loss distance in points above entry price.</param>
        /// <param name="tpPoints">Optional take-profit distance in points below entry price.</param>
        /// <param name="comment">Optional order comment.</param>
        /// <param name="deviationPoints">Maximum price deviation in points.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 15).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Order send result with ticket number.</returns>
        /// <example>
        /// <code>
        /// // Sell Stop 20 points below Bid
        /// await service.SellStopPoints("EURUSD", 0.01, priceOffsetPoints: 20,
        ///     slPoints: 30, tpPoints: 50);
        /// </code>
        /// </example>
        public static async Task<mt5_term_api.OrderSendData> SellStopPoints(
            this MT5Service svc, string symbol, double volume, double priceOffsetPoints,
            double? slPoints = null, double? tpPoints = null, string? comment = null, int deviationPoints = 0,
            int timeoutSec = 15, CancellationToken ct = default)
        {
            await svc.EnsureSelected(symbol, timeoutSec, ct);

            var dl = Dl(timeoutSec);
            var tick = await svc.SymbolInfoTickAsync(symbol, dl, ct);
            var point = await svc.SymbolInfoDoubleAsync(symbol, mt5_term_api.SymbolInfoDoubleProperty.SymbolPoint, dl, ct);

            // SellStop: from BID downward
            var rawPrice = tick.Bid - Math.Abs(priceOffsetPoints) * point;
            var price = await svc.NormalizePriceAsync(symbol, rawPrice, timeoutSec, ct);

            double sl = slPoints.HasValue ? price + slPoints.Value * point : 0;
            double tp = tpPoints.HasValue ? price - tpPoints.Value * point : 0;

            // SellStop = 5
            var req = BuildPendingRequest(symbol, volume, price, sl, tp, comment, deviationPoints, operationCode: 5);
            return await svc.OrderSendAsync(req, dl, ct);
        }

        #endregion
        // ─────────────────────────────────────────────────────────────────────
           #region [09] MARKET BY RISK
        // ─────────────────────────────────────────────────────────────────────
        

        /// <summary>
        /// Opens Buy market position with volume calculated based on risk amount and stop-loss distance.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="stopPoints">Stop-loss distance in points.</param>
        /// <param name="riskMoney">Maximum amount to risk (in account currency).</param>
        /// <param name="tpPoints">Optional take-profit distance in points.</param>
        /// <param name="comment">Optional order comment.</param>
        /// <param name="deviationPoints">Maximum price deviation in points.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 20).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Order send result with ticket number.</returns>
        /// <remarks>
        /// Combines CalcVolumeForRiskAsync and market order execution in one method.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Buy with $100 risk and 50-point stop-loss
        /// await service.BuyMarketByRisk("EURUSD", stopPoints: 50,
        ///     riskMoney: 100, tpPoints: 100);
        /// </code>
        /// </example>
        public static async Task<OrderSendData> BuyMarketByRisk(this MT5Service svc, string symbol, double stopPoints, double riskMoney,
            double? tpPoints = null, string? comment = null, int deviationPoints = 0,
            int timeoutSec = 20, CancellationToken ct = default)
        {
            await svc.EnsureSelected(symbol, timeoutSec, ct);
            var vol = await svc.CalcVolumeForRiskAsync(symbol, stopPoints, riskMoney, timeoutSec, ct);
            var dl = Dl(timeoutSec);
            var tick = await svc.SymbolInfoTickAsync(symbol, dl, ct);
            var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint, dl, ct);
            double sl = tick.Bid - stopPoints * point;
            double tp = tpPoints.HasValue ? tick.Ask + tpPoints.Value * point : 0;
            // Normalize critical prices to tick size
            sl = sl > 0 ? await svc.NormalizePriceAsync(symbol, sl, timeoutSec, ct) : 0;
            tp = tp > 0 ? await svc.NormalizePriceAsync(symbol, tp, timeoutSec, ct) : 0;
            return await svc.PlaceMarket(symbol, vol, isBuy: true, sl, tp, comment, deviationPoints, timeoutSec, ct);
        }

        /// <summary>
        /// Opens Sell market position with volume calculated based on risk amount and stop-loss distance.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="stopPoints">Stop-loss distance in points.</param>
        /// <param name="riskMoney">Maximum amount to risk (in account currency).</param>
        /// <param name="tpPoints">Optional take-profit distance in points.</param>
        /// <param name="comment">Optional order comment.</param>
        /// <param name="deviationPoints">Maximum price deviation in points.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 20).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Order send result with ticket number.</returns>
        /// <remarks>
        /// Combines CalcVolumeForRiskAsync and market order execution in one method.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Sell with $100 risk and 50-point stop-loss
        /// await service.SellMarketByRisk("EURUSD", stopPoints: 50,
        ///     riskMoney: 100, tpPoints: 100);
        /// </code>
        /// </example>
        public static async Task<OrderSendData> SellMarketByRisk(this MT5Service svc, string symbol, double stopPoints, double riskMoney,
            double? tpPoints = null, string? comment = null, int deviationPoints = 0,
            int timeoutSec = 20, CancellationToken ct = default)
        {
            await svc.EnsureSelected(symbol, timeoutSec, ct);
            var vol = await svc.CalcVolumeForRiskAsync(symbol, stopPoints, riskMoney, timeoutSec, ct);
            var dl = Dl(timeoutSec);
            var tick = await svc.SymbolInfoTickAsync(symbol, dl, ct);
            var point = await svc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint, dl, ct);
            double sl = tick.Ask + stopPoints * point;
            double tp = tpPoints.HasValue ? tick.Bid - tpPoints.Value * point : 0;
            sl = sl > 0 ? await svc.NormalizePriceAsync(symbol, sl, timeoutSec, ct) : 0;
            tp = tp > 0 ? await svc.NormalizePriceAsync(symbol, tp, timeoutSec, ct) : 0;
            return await svc.PlaceMarket(symbol, vol, isBuy: false, sl, tp, comment, deviationPoints, timeoutSec, ct);
        }

        // Helpers for order-type decoding (enum → int)
        private static int GetOrderTypeInt(object order)
        {
            var typeObj = order.GetType().GetProperty("Type")?.GetValue(order);
            return typeObj is null ? -1 : Convert.ToInt32(typeObj);
        }
        private static bool IsBuyType(int t) => t == 0 || t == 2 || t == 4 || t == 6; // Buy / BuyLimit / BuyStop / BuyStopLimit
        private static bool IsMarketType(int t) => t == 0 || t == 1;                    // Buy / Sell
        private static bool IsPendingType(int t) => t >= 2 && t <= 7;                  // *Limit / *Stop / *StopLimit


        #endregion
        // ─────────────────────────────────────────────────────────────────────
           #region [10] BULK CONVENIENCE
        // ─────────────────────────────────────────────────────────────────────
        

        /// <summary>
        /// Cancels all pending orders with optional filtering by symbol and direction.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Optional symbol filter. Pass null to cancel all symbols.</param>
        /// <param name="isBuy">Optional direction filter. True = cancel only buy orders, False = cancel only sell orders, null = cancel both.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 30).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Number of pending orders cancelled.</returns>
        /// <remarks>
        /// Only affects pending orders (Limit, Stop, Stop Limit). Market positions are not cancelled.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Cancel all pending orders
        /// int cancelled = await service.CancelAll();
        ///
        /// // Cancel only EURUSD buy pending orders
        /// int cancelled = await service.CancelAll(symbol: "EURUSD", isBuy: true);
        /// </code>
        /// </example>
        public static async Task<int> CancelAll(
            this MT5Service svc, string? symbol = null, bool? isBuy = null,
            int timeoutSec = 30, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            var opened = await svc.OpenedOrdersAsync(
                BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, dl, ct);

            int cancelled = 0;

            foreach (var o in EnumerateOrders(opened))
            {
                var t = o.GetType();
                var typeInt = GetOrderTypeInt(o);
                if (!IsPendingType(typeInt)) continue;

                var sym = (string?)t.GetProperty("Symbol")?.GetValue(o) ?? "";
                if (symbol is not null && !string.Equals(sym, symbol, StringComparison.OrdinalIgnoreCase)) continue;

                if (isBuy.HasValue && (IsBuyType(typeInt) != isBuy.Value)) continue;

                var ticket = Convert.ToUInt64(t.GetProperty("Ticket")?.GetValue(o) ?? 0UL);
                if (ticket != 0UL)
                {
                    await svc.CloseByTicket(ticket, null, timeoutSec, ct);
                    cancelled++;
                }
            }

            return cancelled;
        }

        /// <summary>
        /// Closes all open market positions with optional filtering by symbol and direction.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Optional symbol filter. Pass null to close all symbols.</param>
        /// <param name="isBuy">Optional direction filter. True = close only buy positions, False = close only sell positions, null = close both.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 30).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Number of market positions closed.</returns>
        /// <remarks>
        /// Only affects market positions (BUY/SELL). Pending orders are not affected.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Close all market positions
        /// int closed = await service.CloseAllPositions();
        ///
        /// // Close only GBPUSD sell positions
        /// int closed = await service.CloseAllPositions(symbol: "GBPUSD", isBuy: false);
        /// </code>
        /// </example>
        public static async Task<int> CloseAllPositions(
            this MT5Service svc, string? symbol = null, bool? isBuy = null,
            int timeoutSec = 30, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            var opened = await svc.OpenedOrdersAsync(
                BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, dl, ct);

            int closed = 0;

            foreach (var o in EnumerateOrders(opened))
            {
                var t = o.GetType();
                var typeInt = GetOrderTypeInt(o);
                if (!IsMarketType(typeInt)) continue;

                var sym = (string?)t.GetProperty("Symbol")?.GetValue(o) ?? "";
                if (symbol is not null && !string.Equals(sym, symbol, StringComparison.OrdinalIgnoreCase)) continue;

                if (isBuy.HasValue && (IsBuyType(typeInt) != isBuy.Value)) continue;

                var ticket = Convert.ToUInt64(t.GetProperty("Ticket")?.GetValue(o) ?? 0UL);
                if (ticket != 0UL)
                {
                    await svc.CloseByTicket(ticket, null, timeoutSec, ct);
                    closed++;
                }
            }

            return closed;
        }

        /// <summary>
        /// Alias for CancelAll. Cancels all pending orders with optional filtering.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Optional symbol filter.</param>
        /// <param name="isBuy">Optional direction filter.</param>
        /// <param name="timeoutSec">RPC timeout in seconds (default: 30).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Number of pending orders cancelled.</returns>
        public static Task<int> CloseAllPending(this MT5Service svc, string? symbol = null, bool? isBuy = null,
            int timeoutSec = 30, CancellationToken ct = default)
            => svc.CancelAll(symbol, isBuy, timeoutSec, ct);

        #endregion

        // ═════════════════════════════════════════════════════════════════════════════
        #region [11] MARKET DEPTH (DOM) — Order Book Helpers
        // ═════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Subscribe to Market Depth (order book) for a symbol and return a disposable subscription.
        /// Automatically releases the subscription when disposed.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol to subscribe (e.g., "EURUSD").</param>
        /// <param name="timeoutSec">RPC timeout in seconds.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>IDisposable that releases the subscription when disposed.</returns>
        /// <example>
        /// using (await svc.SubscribeToMarketBookAsync("EURUSD"))
        /// {
        ///     var book = await svc.MarketBookGetAsync("EURUSD");
        ///     // Use book data...
        /// } // Auto-releases subscription
        /// </example>
        public static async Task<IDisposable> SubscribeToMarketBookAsync(this MT5Service svc, string symbol,
            int timeoutSec = 15, CancellationToken ct = default)
        {
            await svc.MarketBookAddAsync(symbol, Dl(timeoutSec), ct);
            return new MarketBookSubscription(svc, symbol, timeoutSec);
        }

        private class MarketBookSubscription : IDisposable
        {
            private readonly MT5Service _svc;
            private readonly string _symbol;
            private readonly int _timeoutSec;
            private bool _disposed;

            public MarketBookSubscription(MT5Service svc, string symbol, int timeoutSec)
            {
                _svc = svc;
                _symbol = symbol;
                _timeoutSec = timeoutSec;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    // Fire-and-forget release (cannot await in Dispose)
                    _ = _svc.MarketBookReleaseAsync(_symbol, Dl(_timeoutSec), default);
                }
            }
        }

        /// <summary>
        /// Gets the current Market Depth (order book) snapshot for a symbol.
        /// You must subscribe first using SubscribeToMarketBookAsync or MarketBookAddAsync.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol to query (e.g., "EURUSD").</param>
        /// <param name="timeoutSec">RPC timeout in seconds.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>MarketBookGetData with buy/sell book entries.</returns>
        public static Task<MarketBookGetData> GetMarketBookSnapshotAsync(this MT5Service svc, string symbol,
            int timeoutSec = 15, CancellationToken ct = default)
            => svc.MarketBookGetAsync(symbol, Dl(timeoutSec), ct);

        /// <summary>
        /// Extracts best bid and ask prices from the current order book.
        /// Returns (bestBid, bestAsk) or (0, 0) if book is empty.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol to query (e.g., "EURUSD").</param>
        /// <param name="timeoutSec">RPC timeout in seconds.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Tuple (bestBid, bestAsk).</returns>
        public static async Task<(double bestBid, double bestAsk)> GetBestBidAskFromBookAsync(this MT5Service svc,
            string symbol, int timeoutSec = 15, CancellationToken ct = default)
        {
            var book = await svc.MarketBookGetAsync(symbol, Dl(timeoutSec), ct);

            double bestBid = 0;
            double bestAsk = 0;

            // Find highest bid (BookType.Buy)
            var bids = book.MqlBookInfos.Where(b => b.Type == BookType.Buy);
            if (bids.Any())
                bestBid = bids.Max(b => b.Price);

            // Find lowest ask (BookType.Sell)
            var asks = book.MqlBookInfos.Where(b => b.Type == BookType.Sell);
            if (asks.Any())
                bestAsk = asks.Min(s => s.Price);

            return (bestBid, bestAsk);
        }

        /// <summary>
        /// Calculates total liquidity (volume) available at a specific price level in the order book.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol to query (e.g., "EURUSD").</param>
        /// <param name="price">Price level to check.</param>
        /// <param name="isBuy">True for buy side (bids), false for sell side (asks).</param>
        /// <param name="timeoutSec">RPC timeout in seconds.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Total volume at the price level.</returns>
        public static async Task<long> CalculateLiquidityAtLevelAsync(this MT5Service svc, string symbol,
            double price, bool isBuy, int timeoutSec = 15, CancellationToken ct = default)
        {
            var book = await svc.MarketBookGetAsync(symbol, Dl(timeoutSec), ct);
            var targetType = isBuy ? BookType.Buy : BookType.Sell;

            return book.MqlBookInfos
                      .Where(e => e.Type == targetType && Math.Abs(e.Price - price) < 0.00001)
                      .Sum(e => e.Volume);
        }

        #endregion

        // ═════════════════════════════════════════════════════════════════════════════
        #region [12] ORDER VALIDATION — Pre-flight Checks
        // ═════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Validates an order before sending (pre-flight check).
        /// Checks symbol availability, lot size constraints, margin requirements, etc.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="request">OrderCheckRequest with MqlTradeRequest populated.</param>
        /// <param name="timeoutSec">RPC timeout in seconds.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>OrderCheckData with validation result and potential error codes.</returns>
        /// <example>
        /// var tradeReq = new MqlTradeRequest { Symbol = "EURUSD", Volume = 0.01, ... };
        /// var checkReq = new OrderCheckRequest { MqlTradeRequest = tradeReq };
        /// var result = await svc.ValidateOrderAsync(checkReq);
        /// if (result.MqlTradeCheckResult.ReturnedCode == 0) Console.WriteLine("Order is valid!");
        /// </example>
        public static Task<OrderCheckData> ValidateOrderAsync(this MT5Service svc, OrderCheckRequest request,
            int timeoutSec = 15, CancellationToken ct = default)
            => svc.OrderCheckAsync(request, Dl(timeoutSec), ct);

        /// <summary>
        /// Calculates margin required for a BUY order before placing it.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol to trade.</param>
        /// <param name="volume">Volume in lots.</param>
        /// <param name="price">Entry price (0 for market price).</param>
        /// <param name="timeoutSec">RPC timeout in seconds.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Required margin amount.</returns>
        public static async Task<double> CalculateBuyMarginAsync(this MT5Service svc, string symbol, double volume,
            double price = 0, int timeoutSec = 15, CancellationToken ct = default)
        {
            var request = new OrderCalcMarginRequest
            {
                Symbol = symbol,
                OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
                Volume = volume,
                OpenPrice = price
            };

            var result = await svc.OrderCalcMarginAsync(request, Dl(timeoutSec), ct);
            return result.Margin;
        }

        /// <summary>
        /// Calculates margin required for a SELL order before placing it.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol to trade.</param>
        /// <param name="volume">Volume in lots.</param>
        /// <param name="price">Entry price (0 for market price).</param>
        /// <param name="timeoutSec">RPC timeout in seconds.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Required margin amount.</returns>
        public static async Task<double> CalculateSellMarginAsync(this MT5Service svc, string symbol, double volume,
            double price = 0, int timeoutSec = 15, CancellationToken ct = default)
        {
            var request = new OrderCalcMarginRequest
            {
                Symbol = symbol,
                OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfSell,
                Volume = volume,
                OpenPrice = price
            };

            var result = await svc.OrderCalcMarginAsync(request, Dl(timeoutSec), ct);
            return result.Margin;
        }

        /// <summary>
        /// Checks if account has enough free margin for a trade.
        /// Returns (hasEnoughMargin, freeMargin, requiredMargin).
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol to trade.</param>
        /// <param name="volume">Volume in lots.</param>
        /// <param name="isBuy">True for BUY, false for SELL.</param>
        /// <param name="price">Entry price (0 for market price).</param>
        /// <param name="timeoutSec">RPC timeout in seconds.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Tuple (hasEnoughMargin, freeMargin, requiredMargin).</returns>
        public static async Task<(bool hasEnough, double freeMargin, double required)> CheckMarginAvailabilityAsync(
            this MT5Service svc, string symbol, double volume, bool isBuy, double price = 0,
            int timeoutSec = 20, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);

            // Get required margin
            var marginReq = new OrderCalcMarginRequest
            {
                Symbol = symbol,
                OrderType = isBuy ? ENUM_ORDER_TYPE_TF.OrderTypeTfBuy : ENUM_ORDER_TYPE_TF.OrderTypeTfSell,
                Volume = volume,
                OpenPrice = price
            };

            var marginTask = svc.OrderCalcMarginAsync(marginReq, dl, ct);
            var freeMarginTask = svc.GetFreeMarginAsync(dl, ct);

            await Task.WhenAll(marginTask, freeMarginTask);

            var requiredMargin = (await marginTask).Margin;
            var freeMargin = await freeMarginTask;

            bool hasEnough = freeMargin >= requiredMargin;

            return (hasEnough, freeMargin, requiredMargin);
        }

        #endregion

        // ═════════════════════════════════════════════════════════════════════════════
        #region [13] SESSION TIME — Trading Session Helpers
        // ═════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Gets quote session information for a symbol.
        /// Convenience wrapper with default timeout.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol to query.</param>
        /// <param name="dayOfWeek">Day of week.</param>
        /// <param name="sessionIndex">Session index (0 = first session, 1 = second, etc.).</param>
        /// <param name="timeoutSec">RPC timeout in seconds.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>SymbolInfoSessionQuoteData.</returns>
        public static Task<SymbolInfoSessionQuoteData> GetQuoteSessionAsync(this MT5Service svc, string symbol,
            mt5_term_api.DayOfWeek dayOfWeek, int sessionIndex = 0,
            int timeoutSec = 15, CancellationToken ct = default)
            => svc.SymbolInfoSessionQuoteAsync(symbol, dayOfWeek, sessionIndex, Dl(timeoutSec), ct);

        /// <summary>
        /// Gets trade session information for a symbol.
        /// Convenience wrapper with default timeout.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Symbol to query.</param>
        /// <param name="dayOfWeek">Day of week.</param>
        /// <param name="sessionIndex">Session index (0 = first session, 1 = second, etc.).</param>
        /// <param name="timeoutSec">RPC timeout in seconds.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>SymbolInfoSessionTradeData.</returns>
        public static Task<SymbolInfoSessionTradeData> GetTradeSessionAsync(this MT5Service svc, string symbol,
            mt5_term_api.DayOfWeek dayOfWeek, int sessionIndex = 0,
            int timeoutSec = 15, CancellationToken ct = default)
            => svc.SymbolInfoSessionTradeAsync(symbol, dayOfWeek, sessionIndex, Dl(timeoutSec), ct);

        #endregion

        // ═════════════════════════════════════════════════════════════════════════════
        #region [14] POSITION MONITORING — Position Filtering & Aggregation
        // ═════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Gets all currently profitable positions (P&amp;L &gt; 0).
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Optional symbol filter (null = all symbols).</param>
        /// <param name="timeoutSec">RPC timeout in seconds.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of profitable positions.</returns>
        public static async Task<List<object>> GetProfitablePositionsAsync(this MT5Service svc, string? symbol = null,
            int timeoutSec = 20, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            var opened = await svc.OpenedOrdersAsync(BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, dl, ct);
            var profitable = new List<object>();

            foreach (var o in EnumerateOrders(opened))
            {
                var t = o.GetType();
                var typeInt = GetOrderTypeInt(o);
                if (!IsMarketType(typeInt)) continue;

                var sym = (string?)t.GetProperty("Symbol")?.GetValue(o) ?? "";
                if (symbol != null && !string.Equals(sym, symbol, StringComparison.OrdinalIgnoreCase)) continue;

                var profitProp = t.GetProperty("Profit");
                if (profitProp != null)
                {
                    var profit = Convert.ToDouble(profitProp.GetValue(o));
                    if (profit > 0) profitable.Add(o);
                }
            }

            return profitable;
        }

        /// <summary>
        /// Gets all currently losing positions (P&amp;L &lt; 0).
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Optional symbol filter (null = all symbols).</param>
        /// <param name="timeoutSec">RPC timeout in seconds.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of losing positions.</returns>
        public static async Task<List<object>> GetLosingPositionsAsync(this MT5Service svc, string? symbol = null,
            int timeoutSec = 20, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            var opened = await svc.OpenedOrdersAsync(BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, dl, ct);
            var losing = new List<object>();

            foreach (var o in EnumerateOrders(opened))
            {
                var t = o.GetType();
                var typeInt = GetOrderTypeInt(o);
                if (!IsMarketType(typeInt)) continue;

                var sym = (string?)t.GetProperty("Symbol")?.GetValue(o) ?? "";
                if (symbol != null && !string.Equals(sym, symbol, StringComparison.OrdinalIgnoreCase)) continue;

                var profitProp = t.GetProperty("Profit");
                if (profitProp != null)
                {
                    var profit = Convert.ToDouble(profitProp.GetValue(o));
                    if (profit < 0) losing.Add(o);
                }
            }

            return losing;
        }

        /// <summary>
        /// Calculates total profit/loss across all open positions.
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Optional symbol filter (null = all symbols).</param>
        /// <param name="timeoutSec">RPC timeout in seconds.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Total P&amp;L.</returns>
        public static async Task<double> GetTotalProfitLossAsync(this MT5Service svc, string? symbol = null,
            int timeoutSec = 20, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            var opened = await svc.OpenedOrdersAsync(BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, dl, ct);
            double totalPnL = 0;

            foreach (var o in EnumerateOrders(opened))
            {
                var t = o.GetType();
                var typeInt = GetOrderTypeInt(o);
                if (!IsMarketType(typeInt)) continue;

                var sym = (string?)t.GetProperty("Symbol")?.GetValue(o) ?? "";
                if (symbol != null && !string.Equals(sym, symbol, StringComparison.OrdinalIgnoreCase)) continue;

                var profitProp = t.GetProperty("Profit");
                if (profitProp != null)
                    totalPnL += Convert.ToDouble(profitProp.GetValue(o));
            }

            return totalPnL;
        }

        /// <summary>
        /// Gets count of open positions (optionally filtered by symbol).
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="symbol">Optional symbol filter (null = all symbols).</param>
        /// <param name="timeoutSec">RPC timeout in seconds.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Number of open positions.</returns>
        public static async Task<int> GetPositionCountAsync(this MT5Service svc, string? symbol = null,
            int timeoutSec = 20, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            var opened = await svc.OpenedOrdersAsync(BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, dl, ct);
            int count = 0;

            foreach (var o in EnumerateOrders(opened))
            {
                var t = o.GetType();
                var typeInt = GetOrderTypeInt(o);
                if (!IsMarketType(typeInt)) continue;

                var sym = (string?)t.GetProperty("Symbol")?.GetValue(o) ?? "";
                if (symbol != null && !string.Equals(sym, symbol, StringComparison.OrdinalIgnoreCase)) continue;

                count++;
            }

            return count;
        }

        /// <summary>
        /// Aggregates position statistics by symbol.
        /// Returns dictionary: symbol -> (count, totalVolume, totalPnL).
        /// </summary>
        /// <param name="svc">MT5Service instance.</param>
        /// <param name="timeoutSec">RPC timeout in seconds.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Dictionary of position statistics per symbol.</returns>
        public static async Task<Dictionary<string, (int count, double totalVolume, double totalPnL)>>
            GetPositionStatsBySymbolAsync(this MT5Service svc, int timeoutSec = 20, CancellationToken ct = default)
        {
            var dl = Dl(timeoutSec);
            var opened = await svc.OpenedOrdersAsync(BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc, dl, ct);
            var stats = new Dictionary<string, (int count, double totalVolume, double totalPnL)>();

            foreach (var o in EnumerateOrders(opened))
            {
                var t = o.GetType();
                var typeInt = GetOrderTypeInt(o);
                if (!IsMarketType(typeInt)) continue;

                var symbolProp = t.GetProperty("Symbol");
                var volumeProp = t.GetProperty("Volume");
                var profitProp = t.GetProperty("Profit");

                if (symbolProp == null || volumeProp == null || profitProp == null)
                    continue;

                string sym = symbolProp.GetValue(o)?.ToString() ?? "";
                double volume = Convert.ToDouble(volumeProp.GetValue(o));
                double profit = Convert.ToDouble(profitProp.GetValue(o));

                if (!stats.ContainsKey(sym))
                    stats[sym] = (0, 0, 0);

                var (count, totalVol, totalPnL) = stats[sym];
                stats[sym] = (count + 1, totalVol + volume, totalPnL + profit);
            }

            return stats;
        }

        #endregion

    }
}

/*
 ═══════════════════════════════════════════════════════════════════════════
  USAGE EXAMPLES — MT5Sugar Convenience Methods
 ═══════════════════════════════════════════════════════════════════════════

 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  EXAMPLE 1: Simple Market Order with Risk Management                        │
 └─────────────────────────────────────────────────────────────────────────────┘

 This is the most common pattern: open a position with automatic volume
 calculation based on your risk tolerance.

 using var account = await MT5Account.ConnectByServerNameAsync("Broker-Server", "Login:Password");
 var service = account.Service;

 // Buy EURUSD risking $100 with 50-point stop-loss
 var result = await service.BuyMarketByRisk(
     symbol: "EURUSD",
     stopPoints: 50,        // SL distance from entry
     riskMoney: 100,        // Max loss if SL hits
     tpPoints: 100          // Optional take-profit 100 points away
 );

 Console.WriteLine($"Order #{result.Order} opened at {result.Price}");
 // Output: Order #123456789 opened at 1.08523

 ─────────────────────────────────────────────────────────────────────────────

 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  EXAMPLE 2: Pending Orders by Points Offset                                 │
 └─────────────────────────────────────────────────────────────────────────────┘

 Place pending orders without manual price calculation. Just specify the offset
 in points from current market price.

 var service = account.Service;

 // Place Buy Limit 20 points below current Ask
 var buyLimit = await service.BuyLimitPoints(
     symbol: "GBPUSD",
     volume: 0.10,
     priceOffsetPoints: 20,  // 20 points below Ask
     slPoints: 30,           // SL 30 points below entry
     tpPoints: 60            // TP 60 points above entry
 );

 // Place Sell Stop 30 points below current Bid (breakout strategy)
 var sellStop = await service.SellStopPoints(
     symbol: "GBPUSD",
     volume: 0.10,
     priceOffsetPoints: 30,  // 30 points below Bid
     slPoints: 40,
     tpPoints: 80
 );

 Console.WriteLine($"Buy Limit: #{buyLimit.Order}, Sell Stop: #{sellStop.Order}");

 ─────────────────────────────────────────────────────────────────────────────

 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  EXAMPLE 3: Bulk Position Management                                        │
 └─────────────────────────────────────────────────────────────────────────────┘

 Close multiple positions at once with filtering options.

 var service = account.Service;

 // Close ALL open positions (market orders)
 int closedAll = await service.CloseAllPositions();
 Console.WriteLine($"Closed {closedAll} positions");

 // Close only EURUSD positions
 int closedEUR = await service.CloseAllPositions(symbol: "EURUSD");

 // Close only BUY positions on GBPUSD
 int closedBuys = await service.CloseAllPositions(symbol: "GBPUSD", isBuy: true);

 // Cancel all pending orders
 int cancelled = await service.CancelAll();

 // Cancel only SELL LIMIT/STOP orders on XAUUSD
 int cancelledSells = await service.CancelAll(symbol: "XAUUSD", isBuy: false);

 ─────────────────────────────────────────────────────────────────────────────

 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  EXAMPLE 4: Position Monitoring & P&L Tracking                              │
 └─────────────────────────────────────────────────────────────────────────────┘

 Monitor your positions and track profit/loss in real-time.

 var service = account.Service;

 // Get total P&L across all positions
 double totalPnL = await service.GetTotalProfitLossAsync();
 Console.WriteLine($"Total P&L: ${totalPnL:F2}");

 // Get only profitable positions
 var profitable = await service.GetProfitablePositionsAsync();
 Console.WriteLine($"Winning trades: {profitable.Count}");

 // Get only losing positions
 var losing = await service.GetLosingPositionsAsync();
 Console.WriteLine($"Losing trades: {losing.Count}");

 // Get position statistics grouped by symbol
 var stats = await service.GetPositionStatsBySymbolAsync();
 foreach (var (symbol, (count, totalVolume, totalPnL)) in stats)
 {
     Console.WriteLine($"{symbol}: {count} positions, {totalVolume:F2} lots, P&L: ${totalPnL:F2}");
 }
 // Output:
 // EURUSD: 3 positions, 0.30 lots, P&L: $45.20
 // GBPUSD: 1 positions, 0.10 lots, P&L: -$12.50

 ─────────────────────────────────────────────────────────────────────────────

 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  EXAMPLE 5: Pre-flight Validation & Margin Check                            │
 └─────────────────────────────────────────────────────────────────────────────┘

 Check if you have enough margin BEFORE placing an order.

 var service = account.Service;

 // Check margin availability for a 1.0 lot EURUSD buy
 var (hasEnough, freeMargin, required) = await service.CheckMarginAvailabilityAsync(
     symbol: "EURUSD",
     volume: 1.0,
     isBuy: true
 );

 if (hasEnough)
 {
     Console.WriteLine($"✓ Sufficient margin: ${freeMargin:F2} free, ${required:F2} required");

     // Place order
     var result = await service.PlaceMarket("EURUSD", 1.0, isBuy: true,
         sl: 1.0800, tp: 1.0900);
 }
 else
 {
     Console.WriteLine($"✗ Insufficient margin: ${freeMargin:F2} free, ${required:F2} required");
     Console.WriteLine($"   Need ${required - freeMargin:F2} more");
 }

 ─────────────────────────────────────────────────────────────────────────────

 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  EXAMPLE 6: Immutable Snapshots for Data Analysis                           │
 └─────────────────────────────────────────────────────────────────────────────┘

 Capture immutable state snapshots for logging, analysis, or comparison.

 var service = account.Service;

 // Account snapshot
 var accSnapshot = await service.GetAccountSnapshot();
 Console.WriteLine($"Balance: ${accSnapshot.Balance:F2}");
 Console.WriteLine($"Equity: ${accSnapshot.Equity:F2}");
 Console.WriteLine($"Free Margin: ${accSnapshot.FreeMargin:F2}");
 Console.WriteLine($"Margin Level: {accSnapshot.MarginLevel:F2}%");
 Console.WriteLine($"Profit: ${accSnapshot.Profit:F2}");

 // Symbol snapshot
 var eurSnapshot = await service.GetSymbolSnapshot("EURUSD");
 Console.WriteLine($"EURUSD Bid: {eurSnapshot.Bid}");
 Console.WriteLine($"EURUSD Ask: {eurSnapshot.Ask}");
 Console.WriteLine($"Spread: {eurSnapshot.SpreadPoints} points");
 Console.WriteLine($"Point: {eurSnapshot.Point}");
 Console.WriteLine($"Digits: {eurSnapshot.Digits}");

 ─────────────────────────────────────────────────────────────────────────────

 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  EXAMPLE 7: Bounded Streaming (Safe Tick/Trade Monitoring)                  │
 └─────────────────────────────────────────────────────────────────────────────┘

 Stream ticks or trades with built-in timeout safety to prevent infinite loops.

 var service = account.Service;
 var cts = new CancellationTokenSource();

 // Stream ticks for 10 seconds max, 100 events max
 await foreach (var tick in service.ReadTicks(
     symbols: new[] { "EURUSD", "GBPUSD" },
     maxSeconds: 10,
     maxEvents: 100,
     ct: cts.Token))
 {
     Console.WriteLine($"[{tick.SymbolInfo.Symbol}] Bid: {tick.Bid}, Ask: {tick.Ask}");

     // Exit early if needed
     if (tick.Bid > 1.09000)
         break;
 }

 // Stream trade events (order executions, modifications)
 await foreach (var trade in service.ReadTrades(
     maxSeconds: 30,
     maxEvents: 50,
     ct: cts.Token))
 {
     Console.WriteLine($"Trade: {trade.OrderType} {trade.Symbol} {trade.Volume} @ {trade.Price}");
 }

 ─────────────────────────────────────────────────────────────────────────────

 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  EXAMPLE 8: Market Depth (Order Book) Analysis                              │
 └─────────────────────────────────────────────────────────────────────────────┘

 Subscribe to order book and analyze liquidity at different price levels.

 var service = account.Service;

 // Subscribe to order book (disposable pattern)
 using (await service.SubscribeToMarketBookAsync("EURUSD"))
 {
     // Get current order book snapshot
     var book = await service.GetMarketBookSnapshotAsync("EURUSD");

     // Extract best bid/ask
     var (bestBid, bestAsk) = await service.GetBestBidAskFromBookAsync("EURUSD");
     Console.WriteLine($"Best Bid: {bestBid}, Best Ask: {bestAsk}");

     // Calculate liquidity at specific price level
     long liquidityAtBid = await service.CalculateLiquidityAtLevelAsync(
         symbol: "EURUSD",
         price: bestBid,
         isBuy: true  // Check bid side
     );
     Console.WriteLine($"Liquidity at {bestBid}: {liquidityAtBid} volume");

     // Analyze order book depth
     foreach (var entry in book.MqlBookInfos.Take(5))
     {
         string side = entry.Type == BookType.Buy ? "BID" : "ASK";
         Console.WriteLine($"{side} {entry.Price} - Vol: {entry.Volume}");
     }
 } // Auto-unsubscribes when disposed

 ─────────────────────────────────────────────────────────────────────────────

 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  EXAMPLE 9: Advanced Risk Management Strategy                               │
 └─────────────────────────────────────────────────────────────────────────────┘

 Combine multiple methods for sophisticated risk management.

 var service = account.Service;
 const double RISK_PER_TRADE = 100.0;  // $100 risk per trade
 const double MAX_DAILY_LOSS = 500.0;  // $500 max daily loss

 // Step 1: Check current daily P&L
 double currentPnL = await service.GetTotalProfitLossAsync();

 if (currentPnL <= -MAX_DAILY_LOSS)
 {
     Console.WriteLine($"Daily loss limit reached: ${currentPnL:F2}");

     // Close all positions and stop trading
     await service.CloseAllPositions();
     return;
 }

 // Step 2: Calculate position size based on risk
 string symbol = "EURUSD";
 double stopPoints = 50;

 double volume = await service.CalcVolumeForRiskAsync(
     symbol: symbol,
     stopPoints: stopPoints,
     riskMoney: RISK_PER_TRADE
 );

 // Step 3: Normalize volume to broker constraints
 volume = await service.NormalizeVolumeAsync(symbol, volume);

 // Step 4: Check margin availability
 var (hasEnough, freeMargin, required) = await service.CheckMarginAvailabilityAsync(
     symbol: symbol,
     volume: volume,
     isBuy: true
 );

 if (!hasEnough)
 {
     Console.WriteLine($"Insufficient margin: need ${required:F2}, have ${freeMargin:F2}");
     return;
 }

 // Step 5: Get normalized SL/TP prices
 var tick = await service.SymbolInfoTickAsync(symbol);
 var point = await service.GetPointAsync(symbol);

 double entryPrice = tick.Ask;
 double slPrice = entryPrice - (stopPoints * point);
 double tpPrice = entryPrice + (stopPoints * 2 * point);  // 1:2 risk/reward

 // Normalize prices to tick size
 slPrice = await service.NormalizePriceAsync(symbol, slPrice);
 tpPrice = await service.NormalizePriceAsync(symbol, tpPrice);

 // Step 6: Place the order
 var result = await service.PlaceMarket(
     symbol: symbol,
     volume: volume,
     isBuy: true,
     sl: slPrice,
     tp: tpPrice,
     comment: $"Risk: ${RISK_PER_TRADE}"
 );

 Console.WriteLine($"✓ Order placed: #{result.Order}");
 Console.WriteLine($"  Volume: {volume:F2} lots");
 Console.WriteLine($"  Entry: {entryPrice}");
 Console.WriteLine($"  SL: {slPrice} ({stopPoints} points)");
 Console.WriteLine($"  TP: {tpPrice} ({stopPoints * 2} points)");
 Console.WriteLine($"  Risk: ${RISK_PER_TRADE}");

 ─────────────────────────────────────────────────────────────────────────────

 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  EXAMPLE 10: Price Normalization & Symbol Utilities                         │
 └─────────────────────────────────────────────────────────────────────────────┘

 Work with symbol-specific properties and normalize prices correctly.

 var service = account.Service;
 string symbol = "EURUSD";

 // Get symbol properties
 double point = await service.GetPointAsync(symbol);
 int digits = await service.GetDigitsAsync(symbol);
 double spread = await service.GetSpreadPointsAsync(symbol);

 Console.WriteLine($"{symbol} properties:");
 Console.WriteLine($"  Point: {point}");
 Console.WriteLine($"  Digits: {digits}");
 Console.WriteLine($"  Spread: {spread} points");

 // Convert points to pips (for display)
 double pointsValue = 50;
 double pips = await service.PointsToPipsAsync(symbol, pointsValue);
 Console.WriteLine($"{pointsValue} points = {pips} pips");

 // Normalize a raw price to broker's tick size
 double rawPrice = 1.085473829;
 double normalized = await service.NormalizePriceAsync(symbol, rawPrice);
 Console.WriteLine($"Raw price: {rawPrice}");
 Console.WriteLine($"Normalized: {normalized}");

 // Get volume constraints
 var (minVol, maxVol, stepVol) = await service.GetVolumeLimitsAsync(symbol);
 Console.WriteLine($"Volume limits: Min={minVol}, Max={maxVol}, Step={stepVol}");

 // Normalize volume
 double rawVolume = 0.037;
 double normVolume = await service.NormalizeVolumeAsync(symbol, rawVolume);
 Console.WriteLine($"Raw volume: {rawVolume} -> Normalized: {normVolume}");

 // Calculate price from point offset
 double targetPrice = await service.PriceFromOffsetPointsAsync(
     symbol: symbol,
     type: ENUM_ORDER_TYPE.OrderTypeBuyLimit,
     offsetPoints: 20  // 20 points below current Ask
 );
 Console.WriteLine($"Buy Limit price (20 points below Ask): {targetPrice}");

 ═══════════════════════════════════════════════════════════════════════════════
*/