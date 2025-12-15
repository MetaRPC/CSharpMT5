/*══════════════════════════════════════════════════════════════════════════════
 FILE: Examples/Sugar/Program.Sugar.cs - HIGH-LEVEL SUGAR API COMPREHENSIVE DEMO
 PURPOSE:
   Complete demonstration of MT5Service.Extensions - the "Sugar" layer providing
   user-friendly trading methods with smart defaults and automatic calculations.

 🎯 WHO SHOULD USE THIS:
   • Traders who want simple, high-level API for quick strategy implementation
   • Developers building trading bots and algorithms
   • Anyone who prefers clean, readable code over verbose protobuf calls
   • Teams needing risk-based position sizing and bulk operations

 🍬 WHAT IS "SUGAR" API:
   Sugar API = Extension methods that make trading sweeter (easier)!
   Instead of 5-10 lines of low-level code, you write 1 line with smart defaults.

   Example WITHOUT Sugar (Low-Level - 4 separate calls):
     var tick = await acc.SymbolInfoTickAsync(symbol);
     var point = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint);
     var digits = await acc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolDigits);
     var marginRate = await acc.SymbolInfoMarginRateAsync(symbol, ENUM_ORDER_TYPE.OrderTypeBuy);

   Example WITH Sugar (1 call gets everything):
     var snapshot = await svc.GetSymbolSnapshot(symbol);
     // snapshot contains: Tick + Point + Digits + MarginRate

 📊 WHAT THIS DEMO COVERS (19 Sections):

   1. SYMBOL SELECTION & SNAPSHOT
      • EnsureSelected() - Add symbol to MarketWatch if not present
      • GetSymbolSnapshot() - Get Tick + Point + Digits + Margin in ONE call

   2. ACCOUNT SNAPSHOT
      • GetAccountSnapshot() - Get Summary + OpenedOrders in ONE call

   3. NORMALIZATION HELPERS
      • GetPointAsync() - Get symbol point size
      • GetDigitsAsync() - Get decimal places for symbol
      • GetSpreadPointsAsync() - Get current spread in points
      • NormalizePriceAsync() - Round price to broker precision
      • PointsToPipsAsync() - Convert points to pips (handles JPY pairs)

   4. VOLUME HELPERS
      • GetVolumeLimitsAsync() - Get min/max/step volume constraints
      • NormalizeVolumeAsync() - Round volume to broker step

   5. TICK VALUE & SIZE
      • GetTickValueAndSizeAsync() - Get tick value and size in ONE call

   6. RISK CALCULATION ⭐
      • CalcVolumeForRiskAsync() - Calculate lot size based on dollar risk amount

   7. PRICE OFFSET HELPERS
      • PriceFromOffsetPointsAsync() - Calculate price with point offset

   8. TRADING: BUY MARKET (minimum lot)
      • BuyMarket() - Open BUY position with explicit parameters
      • Demonstrates: symbol, volume, SL/TP in points, comment, magic

   9. TRADING: SELL MARKET (minimum lot)
      • SellMarket() - Open SELL position with explicit parameters
      • Demonstrates: symbol, volume, SL/TP in points, comment, magic

   10. TRADING: BUY BY RISK ⭐
      • BuyMarketByRisk() - Open BUY with AUTOMATIC lot calculation
      • Calculates volume based on risk amount (e.g., risk $5)

   11. TRADING: SELL BY RISK ⭐
      • SellMarketByRisk() - Open SELL with AUTOMATIC lot calculation
      • Calculates volume based on risk amount (e.g., risk $5)

   12. MODIFY POSITION (if opened)
      • ModifySlTpAsync() - Change SL/TP of existing position

   13. CLOSE BY TICKET (if opened)
      • CloseByTicket() - Close specific position by ticket number
      • Includes 5-second delay to prevent rate limiting

   14. BULK OPERATIONS
      • CloseAllPositions() - Close all open positions
      • CancelAll() - Cancel all pending orders

   15. HISTORY HELPERS: ORDERS
      • OrdersHistoryLast() - Get last N days of order history with pagination
      • Demonstrates: days parameter, page number, page size, sorting

   15.2 HISTORY HELPERS: POSITIONS (PAGED)
      • PositionsHistoryPaged() - Get historical positions with pagination
      • Demonstrates: sorting, page number, page size

   16. STREAMING HELPERS: TICKS
      • ReadTicks() - Stream tick data with automatic limits
      • Demonstrates: maxEvents limit, duration timeout

   16.2 STREAMING HELPERS: TRADES
      • ReadTrades() - Stream trade events with automatic limits
      • Demonstrates: maxEvents limit, duration timeout

   17. PENDING ORDERS BY POINTS ⭐
      • BuyLimitPoints() - Place BUY LIMIT using point offset (no price calculation!)
      • SellLimitPoints() - Place SELL LIMIT using point offset
      • BuyStopPoints() - Place BUY STOP using point offset
      • SellStopPoints() - Place SELL STOP using point offset

   18. BULK OPERATIONS: ADVANCED
      • CancelAll() - Cancel all or symbol-filtered pending orders
      • CloseAllPositions() - Close all or symbol-filtered positions
      • CloseAll() - Combined close positions and cancel orders with direction filter

   19. PLACE PENDING WITH EXPLICIT TYPE
      • PlacePending() - Place pending order with explicit ENUM_ORDER_TYPE
      • Demonstrates: full control over order type and price

 ⚠️  IMPORTANT - THIS DEMO EXECUTES REAL TRADES:
   This demo performs MULTIPLE REAL TRADING OPERATIONS using MINIMAL LOT sizes:

   Operations executed:
   • Opens 2 market positions (BUY + SELL)
   • Opens 2 risk-based positions (BUY + SELL with $5 risk each)
   • Places 4 pending orders (BuyLimit, SellLimit, BuyStop, SellStop)
   • Modifies position SL/TP
   • Closes positions by ticket
   • Cancels all pending orders

   Total risk: MINIMAL (all operations use broker's minimum lot size)
   Safe for demo accounts, suitable for live accounts

 🔄 COMPARISON: Sugar vs Service vs Low-Level:

   SUGAR API (MT5Service.Extensions) ⭐ THIS FILE:
   ✓ ONE-LINE calls for complex operations
   ✓ AUTOMATIC risk calculation and position sizing
   ✓ SMART DEFAULTS for all parameters
   ✓ POINTS-BASED pending orders (no manual price calculation)
   ✓ BULK OPERATIONS (CloseAll, CancelAll)
   ✓ STREAMING with automatic limits and timeouts
   ✓ PERFECT for rapid strategy development
   ✗ Less control over exact request details

   SERVICE API (MT5Service):
   ✓ Returns unwrapped primitives (no .Value needed)
   ✓ Convenience methods (GetBalanceAsync, etc.)
   ✓ 30-50% less code than low-level
   ✗ Still requires manual calculations
   ✗ No automatic risk-based sizing

   LOW-LEVEL API (MT5Account):
   ✓ FULL CONTROL over every parameter
   ✓ See exact protobuf requests/responses
   ✓ Access to ALL MT5 API features
   ✗ Most verbose (5-10 lines per operation)
   ✗ Manual calculations required for everything
   ✗ Need to handle normalization yourself

 💡 WHEN TO USE SUGAR API:
   ✓ Building trading strategies and algorithms
   ✓ Rapid prototyping of trading ideas
   ✓ Risk-based position sizing (e.g., "risk $10 per trade")
   ✓ Batch operations (close all positions, cancel all orders)
   ✓ Don't want to deal with price/volume normalization
   ✓ Prefer clean, readable, maintainable code
   ✓ Want point-based pending orders without price calculations

 💡 WHEN NOT TO USE SUGAR API:
   ✗ Need exact control over protobuf requests
   ✗ Debugging complex gRPC interactions
   ✗ Learning low-level MT5 API internals
   → Use MT5Account (Low-Level) instead

 📖 COMMON USAGE PATTERNS:

   Pattern 1: Open position with $10 risk
   ─────────────────────────────────────────────────────────────────
   await svc.BuyMarketByRisk(
       symbol: "EURUSD",
       stopPoints: 100,      // 100-point stop loss
       riskMoney: 10.0,      // Risk $10 if SL hit
       tpPoints: 200,        // 200-point take profit
       comment: "Strategy-1"
   );

   Pattern 2: Place pending grid (no price calculation needed!)
   ─────────────────────────────────────────────────────────────────
   var snapshot = await svc.GetSymbolSnapshot("EURUSD");
   await svc.BuyLimitPoints("EURUSD", 0.01, priceOffsetPoints: 50, slPoints: 100, tpPoints: 200);
   await svc.SellLimitPoints("EURUSD", 0.01, priceOffsetPoints: 50, slPoints: 100, tpPoints: 200);

   Pattern 3: Emergency close all positions
   ─────────────────────────────────────────────────────────────────
   int closed = await svc.CloseAllPositions("EURUSD");
   Console.WriteLine($"Closed {closed} positions");

   Pattern 4: Scalping with automatic sizing
   ─────────────────────────────────────────────────────────────────
   var snapshot = await svc.GetSymbolSnapshot("EURUSD");
   var result = await svc.BuyMarketByRisk("EURUSD", stopPoints: 50, riskMoney: 5.0, tpPoints: 100);
   // Opens position risking $5 with 50-point stop, 100-point target

 RELATED FILES:
   • MT5Service.Extensions.cs - Source code of all sugar methods
   • Examples/LowLevel/Program.LowLevel.cs - Low-level API comparison
   • Examples/Services/Program.Service.cs - Mid-level Service API comparison
   • Examples/Sugar/Program.Sugar.Scalper.cs - Scalping strategy example
   • Examples/Sugar/Program.Sugar.PendingOrders.cs - Grid trading example

 USAGE:
   dotnet run sugar
   dotnet run 5
   dotnet run high

══════════════════════════════════════════════════════════════════════════════*/

using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;
using mt5_term_api;

namespace MetaRPC.CSharpMT5.Examples.Sugar
{
    public static class ProgramSugar
    {
        public static async Task RunAsync()
        {
            PrintBanner();

            try
            {
                // ─── [01] SETUP ─────────────────────────────────────────────
                var config = ConnectionHelper.BuildConfiguration();
                var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);
                var service = new MT5Service(account);

                ConsoleHelper.PrintSuccess("✓ MT5Service ready with Extensions (Sugar layer)!\n");

                var symbol = config["MT5:BaseChartSymbol"] ?? "EURUSD";

                // ─── [02] RUN ALL SUGAR DEMOS ───────────────────────────────
                await RunAllSugarDemosAsync(service, symbol);

                ConsoleHelper.PrintSuccess("\n✓ ALL SUGAR DEMOS COMPLETED");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"\n✗ FATAL: {ex.Message}");
                throw;
            }
        }

        private static async Task RunAllSugarDemosAsync(MT5Service svc, string symbol)
        {
            // ══════════════════════════════════════════════════════════════
            // 1. SYMBOL SELECTION & SNAPSHOT
            //    Ensure symbol is in MarketWatch and get complete snapshot.
            //    Snapshot includes: tick data, point size, digits, margin rate.
            //    One call gets all essential symbol properties for trading.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("1. SYMBOL SELECTION & SNAPSHOT");

            Console.WriteLine($"  Symbol: {symbol}\n");

            // Ensure symbol is selected
            Console.WriteLine("  → EnsureSelected()...");
            await svc.EnsureSelected(symbol);
            Console.WriteLine("    ✓ Symbol ensured in MarketWatch\n");

            // Get complete symbol snapshot (tick + point + digits + margin)
            Console.WriteLine("  → GetSymbolSnapshot()...");
            var symbolSnapshot = await svc.GetSymbolSnapshot(symbol);
            Console.WriteLine($"    ✓ Snapshot: {symbolSnapshot.GetType().Name}");
            Console.WriteLine("    → Contains: Tick + Point + Digits + MarginRate\n");

            // ══════════════════════════════════════════════════════════════
            // 2. ACCOUNT SNAPSHOT
            //    Get complete account state in ONE call: account summary
            //    + all opened orders and positions combined.
            //    Perfect for monitoring overall account status.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("2. ACCOUNT SNAPSHOT");

            Console.WriteLine("  → GetAccountSnapshot()...");
            var accountSnapshot = await svc.GetAccountSnapshot();
            Console.WriteLine($"    ✓ Snapshot: {accountSnapshot.GetType().Name}");
            Console.WriteLine("    → Contains: Summary + OpenedOrders in ONE call\n");

            // ══════════════════════════════════════════════════════════════
            // 3. NORMALIZATION HELPERS
            //    Normalize prices and calculate pip values according to
            //    symbol specifications (point size, digits).
            //    Essential for accurate price manipulation and display.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("3. NORMALIZATION HELPERS");

            var point = await svc.GetPointAsync(symbol);
            var digits = await svc.GetDigitsAsync(symbol);
            var spread = await svc.GetSpreadPointsAsync(symbol);

            Console.WriteLine($"  Point:         {point}");
            Console.WriteLine($"  Digits:        {digits}");
            Console.WriteLine($"  Spread:        {spread:F1} points\n");

            // Price normalization
            double rawPrice = 1.123456789;
            var normalized = await svc.NormalizePriceAsync(symbol, rawPrice);
            Console.WriteLine($"  Raw price:     {rawPrice}");
            Console.WriteLine($"  Normalized:    {normalized}\n");

            // Points to pips conversion
            double points = 150;
            var pips = await svc.PointsToPipsAsync(symbol, points);
            Console.WriteLine($"  {points} points = {pips:F1} pips\n");

            // ══════════════════════════════════════════════════════════════
            // 4. VOLUME HELPERS
            //    Get volume limits (min/max/step) and normalize lot sizes
            //    to comply with broker requirements.
            //    Prevents "invalid volume" errors.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("4. VOLUME HELPERS");

            var (minVol, maxVol, stepVol) = await svc.GetVolumeLimitsAsync(symbol);
            Console.WriteLine($"  Min Volume:    {minVol}");
            Console.WriteLine($"  Max Volume:    {maxVol}");
            Console.WriteLine($"  Volume Step:   {stepVol}\n");

            // Volume normalization
            double rawVolume = 0.0123;
            var normalizedVol = await svc.NormalizeVolumeAsync(symbol, rawVolume);
            Console.WriteLine($"  Raw volume:    {rawVolume}");
            Console.WriteLine($"  Normalized:    {normalizedVol}\n");

            // ══════════════════════════════════════════════════════════════
            // 5. TICK VALUE & SIZE
            //    Get tick value and size for P/L calculations.
            //    Tick value = how much 1 point movement is worth in account currency.
            //    Essential for risk/reward calculations.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("5. TICK VALUE & SIZE");

            var (tickValue, tickSize) = await svc.GetTickValueAndSizeAsync(symbol);
            Console.WriteLine($"  Tick Value:    {tickValue}");
            Console.WriteLine($"  Tick Size:     {tickSize}\n");

            // ══════════════════════════════════════════════════════════════
            // 6. RISK CALCULATION
            //    Calculate position size based on risk amount and stop loss.
            //    CalcVolumeForRiskAsync determines lot size to risk specific
            //    dollar amount given stop distance in points.
            //    Essential for position sizing and risk management.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("6. RISK CALCULATION");

            double stopPoints = 100;
            double riskMoney = 10.0; // Risk $10

            Console.WriteLine($"  Stop:          {stopPoints} points");
            Console.WriteLine($"  Risk:          ${riskMoney}\n");

            var calcVolume = await svc.CalcVolumeForRiskAsync(symbol, stopPoints, riskMoney);
            Console.WriteLine($"  → CalcVolumeForRiskAsync()");
            Console.WriteLine($"    Calculated volume: {calcVolume:F2} lots\n");

            // ══════════════════════════════════════════════════════════════
            // 7. PRICE OFFSET HELPERS
            //    Calculate prices with offset in points from current market.
            //    PriceFromOffsetPointsAsync adds/subtracts points from bid/ask
            //    based on order direction. Useful for pending orders.
            //    Automatically handles normalization and direction logic.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("7. PRICE OFFSET HELPERS");

            double offsetPoints = 50;
            var buyPrice = await svc.PriceFromOffsetPointsAsync(symbol, ENUM_ORDER_TYPE.OrderTypeBuy, offsetPoints);
            var sellPrice = await svc.PriceFromOffsetPointsAsync(symbol, ENUM_ORDER_TYPE.OrderTypeSell, offsetPoints);

            Console.WriteLine($"  Offset:        {offsetPoints} points");
            Console.WriteLine($"  Buy price:     {buyPrice}");
            Console.WriteLine($"  Sell price:    {sellPrice}\n");

            // ══════════════════════════════════════════════════════════════
            // 8. TRADING: BUY MARKET (minimum lot)
            //    Open BUY position at current market price (Ask).
            //    Simplest way to enter long position - one line of code.
            //    Optional SL/TP can be set as price levels.
            //    Returns trade result with ticket number and return code.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("8. TRADING: BUY MARKET (minimum lot)");

            Console.WriteLine($"  Testing on {symbol} ({minVol} lot)...\n");
            Console.WriteLine("  → BuyMarket()...");

            ulong? buyTicket = null;
            try
            {
                var buyResult = await svc.BuyMarketAsync(
                    symbol: symbol,
                    volume: minVol,
                    stopLoss: 0,
                    takeProfit: 0,
                    comment: "SUGAR-BUY"
                );

                Console.WriteLine($"    ✓ Return code: {buyResult.ReturnedCode}");
                if (buyResult.Order > 0)
                {
                    buyTicket = buyResult.Order;
                    Console.WriteLine($"    ✓ Ticket: {buyTicket.Value}\n");
                }
                else
                {
                    Console.WriteLine($"    ✗ No ticket returned\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            // ══════════════════════════════════════════════════════════════
            // 9. TRADING: SELL MARKET (minimum lot)
            //    Open SELL position at current market price (Bid).
            //    Simplest way to enter short position - one line of code.
            //    Optional SL/TP can be set as price levels.
            //    Returns trade result with ticket number and return code.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("9. TRADING: SELL MARKET (minimum lot)");

            Console.WriteLine("  → SellMarket()...");

            try
            {
                var sellResult = await svc.SellMarketAsync(
                    symbol: symbol,
                    volume: minVol,
                    stopLoss: 0,
                    takeProfit: 0,
                    comment: "SUGAR-SELL"
                );

                Console.WriteLine($"    ✓ Return code: {sellResult.ReturnedCode}");
                if (sellResult.Order > 0)
                {
                    Console.WriteLine($"    ✓ Ticket: {sellResult.Order}\n");
                }
                else
                {
                    Console.WriteLine($"    ✗ No ticket returned\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            // ══════════════════════════════════════════════════════════════
            // 10. TRADING: BUY BY RISK
            //    Open BUY position with automatic lot calculation based on risk.
            //    Specify risk amount ($) and stop distance (points),
            //    volume is calculated automatically to match your risk.
            //    Perfect for consistent risk management across trades.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("10. TRADING: BUY BY RISK");

            Console.WriteLine($"  Risk: ${riskMoney}, Stop: {stopPoints} points\n");
            Console.WriteLine("  → BuyMarketByRisk()...");

            try
            {
                var buyRiskResult = await svc.BuyMarketByRisk(
                    symbol: symbol,
                    stopPoints: stopPoints,
                    riskMoney: riskMoney,
                    tpPoints: 200,
                    comment: "SUGAR-BUY-RISK"
                );

                Console.WriteLine($"    ✓ Return code: {buyRiskResult.ReturnedCode}");
                if (buyRiskResult.Order > 0)
                {
                    Console.WriteLine($"    ✓ Ticket: {buyRiskResult.Order}\n");
                }
                else
                {
                    Console.WriteLine($"    ✗ No ticket returned\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            // ══════════════════════════════════════════════════════════════
            // 11. TRADING: SELL BY RISK
            //    Open SELL position with automatic lot calculation based on risk.
            //    Specify risk amount ($) and stop distance (points),
            //    volume is calculated automatically to match your risk.
            //    Perfect for consistent risk management across trades.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("11. TRADING: SELL BY RISK");

            Console.WriteLine("  → SellMarketByRisk()...");

            try
            {
                var sellRiskResult = await svc.SellMarketByRisk(
                    symbol: symbol,
                    stopPoints: stopPoints,
                    riskMoney: riskMoney,
                    tpPoints: 200,
                    comment: "SUGAR-SELL-RISK"
                );

                Console.WriteLine($"    ✓ Return code: {sellRiskResult.ReturnedCode}");
                if (sellRiskResult.Order > 0)
                {
                    Console.WriteLine($"    ✓ Ticket: {sellRiskResult.Order}\n");
                }
                else
                {
                    Console.WriteLine($"    ✗ No ticket returned\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            // ══════════════════════════════════════════════════════════════
            // 12. MODIFY POSITION (if opened)
            //    Change Stop Loss and Take Profit of existing position.
            //    ModifySlTpAsync updates SL/TP levels by ticket number.
            //    Use 0 to remove SL or TP. Essential for trailing stops
            //    and adjusting risk/reward as market moves.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("12. MODIFY POSITION (if opened)");

            if (buyTicket.HasValue)
            {
                Console.WriteLine($"  Ticket: {buyTicket.Value}\n");
                Console.WriteLine("  → ModifySlTpAsync()...");

                try
                {
                    var currentSnapshot = await svc.GetSymbolSnapshot(symbol);
                    var newSl = currentSnapshot.Tick.Bid - (150 * point);
                    var newTp = currentSnapshot.Tick.Bid + (250 * point);

                    var modifyResult = await svc.ModifySlTpAsync(
                        ticket: buyTicket.Value,
                        slPrice: newSl,
                        tpPrice: newTp
                    );

                    Console.WriteLine($"    ✓ Modified: Return code {modifyResult.ReturnedCode}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
                }
            }
            else
            {
                Console.WriteLine("  → SKIPPED (no ticket from BUY operation)\n");
            }

            // ══════════════════════════════════════════════════════════════
            // 13. CLOSE BY TICKET (if opened)
            //    Close specific position by ticket number.
            //    CloseByTicket closes full or partial volume at market price.
            //    Specify volume to close (use position volume for full close).
            //    Returns trade result with return code.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("13. CLOSE BY TICKET (if opened)");

            if (buyTicket.HasValue)
            {
                Console.WriteLine($"  Ticket: {buyTicket.Value}\n");
                Console.WriteLine("  ⏸ Waiting 5 seconds to prevent rate limiting...");
                await Task.Delay(5000); // 5-second delay to prevent TRADE_RETCODE_CONNECTION

                Console.WriteLine("  → CloseByTicket()...");

                try
                {
                    var closeResult = await svc.CloseByTicket(
                        ticket: buyTicket.Value,
                        volume: minVol
                    );

                    Console.WriteLine($"    ✓ Closed: Return code {closeResult.ReturnedCode}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
                }
            }
            else
            {
                Console.WriteLine("  → SKIPPED (no ticket from BUY operation)\n");
            }

            // ══════════════════════════════════════════════════════════════
            // 14. BULK OPERATIONS
            //    Close or cancel multiple orders/positions at once.
            //    CloseAllPending - cancels all pending orders for symbol.
            //    CloseAllPositions - closes all open positions.
            //    CancelAll, CloseAll - filter by symbol and direction.
            //    Perfect for emergency exits or end-of-day cleanup.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("14. BULK OPERATIONS");

            Console.WriteLine("  → CloseAllPending()...");
            try
            {
                var closedCount = await svc.CloseAllPending(symbol);
                Console.WriteLine($"    ✓ Closed {closedCount} pending orders\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            // Note: CloseAll, CloseAllPositions, CancelAll available
            Console.WriteLine("  Other bulk methods available:");
            Console.WriteLine("    • CloseAll(symbol, isBuy)");
            Console.WriteLine("    • CloseAllPositions(symbol, isBuy)");
            Console.WriteLine("    • CancelAll(symbol, isBuy)\n");

            // ══════════════════════════════════════════════════════════════
            // 15. HISTORY HELPERS: ORDERS
            //    Retrieve historical orders with pagination and sorting.
            //    OrdersHistoryLast gets last N days of closed/cancelled orders.
            //    Supports sorting by close time, open time, ticket, etc.
            //    Essential for trade analytics and performance tracking.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("15. HISTORY HELPERS: ORDERS");

            Console.WriteLine("  → OrdersHistoryLast()...");
            try
            {
                var history = await svc.OrdersHistoryLast(
                    days: 7,
                    page: 0,
                    size: 10,
                    sort: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc
                );

                Console.WriteLine($"    ✓ History: {history.GetType().Name}");
                Console.WriteLine("    → Last 7 days, page 0, size 10\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            // ══════════════════════════════════════════════════════════════
            // 15.2 HISTORY HELPERS: POSITIONS (PAGED)
            //    Retrieve historical positions with pagination and sorting.
            //    PositionsHistoryPaged gets closed positions from account history.
            //    Supports sorting by open/close time, profit, volume, etc.
            //    Essential for analyzing past performance and P/L.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("15.2 HISTORY HELPERS: POSITIONS (PAGED)");

            Console.WriteLine("  → PositionsHistoryPaged()...");
            try
            {
                var posHistory = await svc.PositionsHistoryPaged(
                    sort: AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeDesc,
                    page: 0,
                    size: 10
                );

                Console.WriteLine($"    ✓ History: {posHistory.GetType().Name}");
                if (posHistory.HistoryPositions != null)
                {
                    Console.WriteLine($"    → Found {posHistory.HistoryPositions.Count} positions\n");
                }
                else
                {
                    Console.WriteLine("    → No positions found\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            // ══════════════════════════════════════════════════════════════
            // 16. STREAMING HELPERS: TICKS
            //    Stream real-time tick data with automatic limits.
            //    ReadTicks subscribes to price updates for specified symbols.
            //    Automatically stops after max events or timeout reached.
            //    Perfect for testing, sampling, and quick data collection.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("16. STREAMING HELPERS: TICKS");

            Console.WriteLine("  → ReadTicks() with limits...");
            Console.WriteLine($"    Max 3 ticks OR 2 seconds\n");

            int tickCount = 0;
            try
            {
                await foreach (var tickData in svc.ReadTicks(
                    symbols: new[] { symbol },
                    maxEvents: 3,
                    durationSec: 2))
                {
                    tickCount++;
                    Console.WriteLine($"    Tick #{tickCount}: {tickData.GetType().Name}");
                }

                Console.WriteLine($"\n    ✓ Received {tickCount} tick(s)\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            // ══════════════════════════════════════════════════════════════
            // 16.2 STREAMING HELPERS: TRADES
            //    Stream real-time trade events from YOUR account.
            //    ReadTrades shows your order executions, modifications, closes.
            //    Automatically stops after max events or timeout.
            //    Note: Shows only YOUR trades, not other market participants.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("16.2 STREAMING HELPERS: TRADES");

            Console.WriteLine("  → ReadTrades() with limits...");
            Console.WriteLine($"    Max 5 trade events OR 3 seconds\n");

            int tradeCount = 0;
            try
            {
                await foreach (var tradeData in svc.ReadTrades(
                    maxEvents: 5,
                    durationSec: 3))
                {
                    tradeCount++;
                    Console.WriteLine($"    Trade #{tradeCount}: {tradeData.GetType().Name}");
                }

                Console.WriteLine($"\n    ✓ Received {tradeCount} trade event(s)\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            // ══════════════════════════════════════════════════════════════
            // 17. PENDING ORDERS BY POINTS
            //    Place pending orders using point offsets from current price.
            //    BuyLimitPoints/SellLimitPoints - limit orders (below/above market).
            //    BuyStopPoints/SellStopPoints - stop orders (above/below market).
            //    Automatically calculates entry price from offset in points.
            //    SL/TP also specified in points - no manual price calculations!
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("17. PENDING ORDERS BY POINTS");

            Console.WriteLine($"  Testing pending orders on {symbol} ({minVol} lot)...\n");

            Console.WriteLine("  → BuyLimitPoints() - 50 points below ask...");
            try
            {
                var buyLimitResult = await svc.BuyLimitPoints(
                    symbol: symbol,
                    volume: minVol,
                    priceOffsetPoints: 50,
                    slPoints: 100,
                    tpPoints: 200,
                    comment: "SUGAR-BUY-LIMIT"
                );

                Console.WriteLine($"    ✓ Order placed: {buyLimitResult.GetType().Name}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            Console.WriteLine("  → SellLimitPoints() - 50 points above bid...");
            try
            {
                var sellLimitResult = await svc.SellLimitPoints(
                    symbol: symbol,
                    volume: minVol,
                    priceOffsetPoints: 50,
                    slPoints: 100,
                    tpPoints: 200,
                    comment: "SUGAR-SELL-LIMIT"
                );

                Console.WriteLine($"    ✓ Order placed: {sellLimitResult.GetType().Name}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            Console.WriteLine("  → BuyStopPoints() - 50 points above ask...");
            try
            {
                var buyStopResult = await svc.BuyStopPoints(
                    symbol: symbol,
                    volume: minVol,
                    priceOffsetPoints: 50,
                    slPoints: 100,
                    tpPoints: 200,
                    comment: "SUGAR-BUY-STOP"
                );

                Console.WriteLine($"    ✓ Order placed: {buyStopResult.GetType().Name}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            Console.WriteLine("  → SellStopPoints() - 50 points below bid...");
            try
            {
                var sellStopResult = await svc.SellStopPoints(
                    symbol: symbol,
                    volume: minVol,
                    priceOffsetPoints: 50,
                    slPoints: 100,
                    tpPoints: 200,
                    comment: "SUGAR-SELL-STOP"
                );

                Console.WriteLine($"    ✓ Order placed: {sellStopResult.GetType().Name}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            // ══════════════════════════════════════════════════════════════
            // 18. BULK OPERATIONS: ADVANCED
            //    Advanced batch operations for closing and cancelling.
            //    CancelAll - cancel all pending orders (with optional filters).
            //    CloseAllPositions - close all open positions.
            //    CloseAll - combined: close positions AND cancel orders by direction.
            //    Essential for risk management and account cleanup.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("18. BULK OPERATIONS: ADVANCED");

            Console.WriteLine("  → CancelAll() - Cancel all pending orders...");
            try
            {
                var cancelledCount = await svc.CancelAll(symbol);
                Console.WriteLine($"    ✓ Cancelled {cancelledCount} pending order(s)\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            Console.WriteLine("  → CloseAllPositions() - Close all open positions...");
            try
            {
                var closedPosCount = await svc.CloseAllPositions(symbol);
                Console.WriteLine($"    ✓ Closed {closedPosCount} position(s)\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            Console.WriteLine("  → CloseAll() - Close all BUY positions and orders...");
            try
            {
                var closedAllCount = await svc.CloseAll(symbol, isBuy: true);
                Console.WriteLine($"    ✓ Closed {closedAllCount} BUY position(s)/order(s)\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            // ══════════════════════════════════════════════════════════════
            // 19. PLACE PENDING WITH EXPLICIT TYPE
            //    Place pending order with explicit order type specification.
            //    PlacePending allows full control - specify exact order type,
            //    entry price, SL, TP as absolute price levels.
            //    More flexible than *Points helpers - use when you need
            //    precise control over order parameters.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("19. PLACE PENDING WITH EXPLICIT TYPE");

            Console.WriteLine("  → PlacePending() with explicit order type...");
            try
            {
                var currentAsk = await svc.GetSymbolSnapshot(symbol);
                var pendingPrice = currentAsk.Tick.Ask - (30 * point);

                var pendingResult = await svc.PlacePending(
                    symbol: symbol,
                    volume: minVol,
                    type: ENUM_ORDER_TYPE.OrderTypeBuyLimit,
                    price: pendingPrice,
                    sl: pendingPrice - (100 * point),
                    tp: pendingPrice + (200 * point),
                    comment: "SUGAR-PENDING-EXPLICIT"
                );

                Console.WriteLine($"    ✓ Pending order: {pendingResult.GetType().Name}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // BANNER
        // ═════════════════════════════════════════════════════════════════

        private static void PrintBanner()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║         HIGH-LEVEL SUGAR API - Extensions Demo                   ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║                       MT5Service.Extensions                      ║");
            Console.WriteLine("║  User-friendly trading with smart defaults & risk management     ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝\n");
        }
    }
}
