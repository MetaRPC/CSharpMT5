/*══════════════════════════════════════════════════════════════════════════════
 FILE: Examples/Sugar/Program.Sugar.Monitor.cs — POSITION & HISTORY MONITORING
 PURPOSE:
   Demonstrate MT5Sugar API monitoring and history methods through a realistic
   position monitoring scenario with real-time P&L tracking and statistics.

 🎯 WHO SHOULD USE THIS:
   • Traders who need real-time position monitoring
   • Developers building dashboard and analytics tools
   • Users who want to track profitable vs losing positions
   • Anyone needing historical trade analysis

 📊 WHAT THIS DEMO COVERS (10 Methods):

   1. MARKET ORDERS
      • PlaceMarket() - Open BUY/SELL at market price with SL/TP

   2. SESSION INFORMATION
      • GetQuoteSessionAsync() - Query quote session timing (when quotes available)
      • GetTradeSessionAsync() - Query trade session timing (when trading allowed)

   3. POSITION MONITORING
      • GetPositionCountAsync() - Count open positions (all or by symbol)
      • GetTotalProfitLossAsync() - Calculate total profit/loss across positions
      • GetProfitablePositionsAsync() - Filter positions with positive profit
      • GetLosingPositionsAsync() - Filter positions with negative profit
      • GetPositionStatsBySymbolAsync() - Aggregate statistics grouped by symbol

   4. HISTORY QUERIES
      • OrdersHistoryLast() - Recent order history with pagination/sorting
      • PositionsHistoryPaged() - Closed positions history with date range

 ⚠️  IMPORTANT - TRADING OPERATIONS:
   This demo executes REAL TRADES using MINIMAL LOT sizes:
   - Opens several test positions for monitoring
   - Queries session information
   - Monitors P&L in real-time
   - Analyzes profitable vs losing positions
   - Queries order and position history

   Total risk: Minimal (all operations use broker's minimum lot size)

 💡 WHEN TO USE MONITORING API:
   • Building trading dashboards
   • Real-time P&L tracking
   • Position risk analysis
   • Trade statistics and reporting
   • Session timing validation

 USAGE:
   dotnet run monitor
   dotnet run 6
   dotnet run history
══════════════════════════════════════════════════════════════════════════════*/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;
using mt5_term_api;

namespace MetaRPC.CSharpMT5.Examples.Sugar;

public static class ProgramSugarMonitor
{
    public static async Task RunAsync()
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        PrintHeader();

        // ════════════════════════════════════════════════════════════════
        // CONNECTION SETUP
        // ════════════════════════════════════════════════════════════════
        var config = ConnectionHelper.BuildConfiguration();
        var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);
        var svc = new MT5Service(account);

        Console.WriteLine("✓ Connected to MT5 terminal\n");

        // ════════════════════════════════════════════════════════════════
        // STEP 1: SETUP TEST POSITIONS
        // ════════════════════════════════════════════════════════════════
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ STEP 1: SETUP TEST POSITIONS FOR MONITORING                 ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

        string symbol = "EURUSD";
        double testVolume = 0.01;

        // Ensure symbol is selected
        await svc.EnsureSelected(symbol);
        Console.WriteLine($"✓ Symbol '{symbol}' selected\n");

        // Get current price
        var symSnapshot = await svc.GetSymbolSnapshot(symbol);
        double currentBid = symSnapshot.Tick.Bid;
        double currentAsk = symSnapshot.Tick.Ask;

        Console.WriteLine($"Current Price: Bid={currentBid:F5}, Ask={currentAsk:F5}\n");

        // ══════════════════════════════════════════════════════════════
        // [1] PlaceMarket
        //     Places market orders (BUY or SELL) at current price.
        //     Supports optional SL/TP, comment, deviation, magic number.
        //     Returns OrderResult with ticket number and execution details.
        // ══════════════════════════════════════════════════════════════
        Console.WriteLine("[1] PlaceMarket - Opening test positions...");

        var buyResult1 = await svc.PlaceMarket(symbol, testVolume, isBuy: true,
            sl: currentAsk - 0.0050,  // Wide SL
            tp: currentAsk + 0.0020,  // Tight TP
            comment: "MONITOR-BUY-1");

        if (buyResult1.Order > 0)
        {
            Console.WriteLine($"    ✓ BUY position opened: Ticket #{buyResult1.Order}");
        }

        await Task.Delay(500);

        var sellResult1 = await svc.PlaceMarket(symbol, testVolume, isBuy: false,
            sl: currentBid + 0.0050,  // Wide SL
            tp: currentBid - 0.0020,  // Tight TP
            comment: "MONITOR-SELL-1");

        if (sellResult1.Order > 0)
        {
            Console.WriteLine($"    ✓ SELL position opened: Ticket #{sellResult1.Order}");
        }

        await Task.Delay(500);

        var buyResult2 = await svc.PlaceMarket(symbol, testVolume, isBuy: true,
            sl: currentAsk - 0.0030,
            tp: currentAsk + 0.0030,
            comment: "MONITOR-BUY-2");

        if (buyResult2.Order > 0)
        {
            Console.WriteLine($"    ✓ BUY position opened: Ticket #{buyResult2.Order}");
        }

        Console.WriteLine($"\n✓ Opened 3 test positions\n");
        await Task.Delay(2000); // Wait for positions to settle

        // ════════════════════════════════════════════════════════════════
        // STEP 2: SESSION INFO QUERIES
        // ════════════════════════════════════════════════════════════════
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ STEP 2: SESSION INFO QUERIES                                 ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

        // ══════════════════════════════════════════════════════════════
        // [2] GetQuoteSessionAsync
        //     Retrieves quote session timing for a symbol on specific day.
        //     Quote sessions define when price quotes are available.
        //     Returns session start/end times as Timestamp objects.
        //     Useful for determining market data availability windows.
        // ══════════════════════════════════════════════════════════════
        try
        {
            var quoteSession = await svc.GetQuoteSessionAsync(
                symbol,
                mt5_term_api.DayOfWeek.Monday,
                sessionIndex: 0
            );
            Console.WriteLine($"[2] GetQuoteSessionAsync (Monday, session 0):");
            Console.WriteLine($"    → Start: {quoteSession.From.ToDateTime():HH:mm:ss}");
            Console.WriteLine($"    → End:   {quoteSession.To.ToDateTime():HH:mm:ss}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[2] GetQuoteSessionAsync: ⚠ {ex.Message.Split('.')[0]}");
        }

        // ══════════════════════════════════════════════════════════════
        // [3] GetTradeSessionAsync
        //     Retrieves trade session timing for a symbol on specific day.
        //     Trade sessions define when trading operations are allowed.
        //     Returns session start/end times as Timestamp objects.
        //     Essential for knowing when you can place/modify/close orders.
        // ══════════════════════════════════════════════════════════════
        try
        {
            var tradeSession = await svc.GetTradeSessionAsync(
                symbol,
                mt5_term_api.DayOfWeek.Monday,
                sessionIndex: 0
            );
            Console.WriteLine($"\n[3] GetTradeSessionAsync (Monday, session 0):");
            Console.WriteLine($"    → Start: {tradeSession.From.ToDateTime():HH:mm:ss}");
            Console.WriteLine($"    → End:   {tradeSession.To.ToDateTime():HH:mm:ss}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[3] GetTradeSessionAsync: ⚠ {ex.Message.Split('.')[0]}\n");
        }

        // ════════════════════════════════════════════════════════════════
        // STEP 3: POSITION MONITORING & STATISTICS
        // ════════════════════════════════════════════════════════════════
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ STEP 3: POSITION MONITORING & STATISTICS                     ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

        // ══════════════════════════════════════════════════════════════
        // [4] GetPositionCountAsync
        //     Counts open positions (all symbols or specific symbol).
        //     Fast way to check portfolio size without fetching full data.
        //     Returns integer count of currently open positions.
        // ══════════════════════════════════════════════════════════════
        int positionCount = await svc.GetPositionCountAsync();
        Console.WriteLine($"[4] GetPositionCountAsync: {positionCount} total positions\n");

        int symbolPositionCount = await svc.GetPositionCountAsync(symbol);
        Console.WriteLine($"    GetPositionCountAsync('{symbol}'): {symbolPositionCount} positions for {symbol}\n");

        // ══════════════════════════════════════════════════════════════
        // [5] GetTotalProfitLossAsync
        //     Calculates total profit/loss across positions.
        //     Can aggregate all symbols or filter by specific symbol.
        //     Returns double value in account currency (positive = profit).
        //     Essential for real-time portfolio P&L monitoring.
        // ══════════════════════════════════════════════════════════════
        double totalPnL = await svc.GetTotalProfitLossAsync();
        Console.WriteLine($"[5] GetTotalProfitLossAsync: {totalPnL:F2} USD (all positions)");
        Console.WriteLine($"    → {(totalPnL >= 0 ? "✓ PROFIT" : "✗ LOSS")}\n");

        double symbolPnL = await svc.GetTotalProfitLossAsync(symbol);
        Console.WriteLine($"    GetTotalProfitLossAsync('{symbol}'): {symbolPnL:F2} USD");
        Console.WriteLine($"    → {(symbolPnL >= 0 ? "✓ PROFIT" : "✗ LOSS")}\n");

        // ══════════════════════════════════════════════════════════════
        // [6] GetProfitablePositionsAsync
        //     Filters and returns only positions with positive profit.
        //     Can filter all symbols or specific symbol.
        //     Returns List<object> with position details (Ticket, Profit, Symbol, etc.).
        //     Perfect for identifying winning trades in real-time.
        // ══════════════════════════════════════════════════════════════
        var profitablePositions = await svc.GetProfitablePositionsAsync();
        Console.WriteLine($"[6] GetProfitablePositionsAsync: {profitablePositions.Count} profitable positions (all symbols)");

        if (profitablePositions.Any())
        {
            foreach (var pos in profitablePositions.Take(3))
            {
                // Position data is stored as dynamic objects, need to access via reflection or dynamic
                var posData = pos as dynamic;
                if (posData != null)
                {
                    try
                    {
                        Console.WriteLine($"    ✓ Ticket #{posData.Ticket}: +{posData.Profit:F2} USD (Symbol: {posData.Symbol})");
                    }
                    catch
                    {
                        Console.WriteLine($"    ✓ Position (details unavailable)");
                    }
                }
            }
        }
        Console.WriteLine();

        // ══════════════════════════════════════════════════════════════
        // [7] GetLosingPositionsAsync
        //     Filters and returns only positions with negative profit.
        //     Can filter all symbols or specific symbol.
        //     Returns List<object> with position details.
        //     Useful for risk management and stop-loss monitoring.
        // ══════════════════════════════════════════════════════════════
        var losingPositions = await svc.GetLosingPositionsAsync();
        Console.WriteLine($"[7] GetLosingPositionsAsync: {losingPositions.Count} losing positions (all symbols)");

        if (losingPositions.Any())
        {
            foreach (var pos in losingPositions.Take(3))
            {
                var posData = pos as dynamic;
                if (posData != null)
                {
                    try
                    {
                        Console.WriteLine($"    ✗ Ticket #{posData.Ticket}: {posData.Profit:F2} USD (Symbol: {posData.Symbol})");
                    }
                    catch
                    {
                        Console.WriteLine($"    ✗ Position (details unavailable)");
                    }
                }
            }
        }
        Console.WriteLine();

        // ══════════════════════════════════════════════════════════════
        // [8] GetPositionStatsBySymbolAsync
        //     Aggregates position statistics grouped by symbol.
        //     Returns Dictionary<string, (count, totalVolume, totalPnL)>.
        //     Shows count, total volume, and P&L per symbol.
        //     Excellent for multi-symbol portfolio analysis.
        // ══════════════════════════════════════════════════════════════
        try
        {
            var symbolStats = await svc.GetPositionStatsBySymbolAsync();
            Console.WriteLine($"[8] GetPositionStatsBySymbolAsync: Statistics by symbol");

            if (symbolStats.Any())
            {
                foreach (var kvp in symbolStats.Take(5))
                {
                    var sym = kvp.Key;
                    var stats = kvp.Value;
                    Console.WriteLine($"    → {sym}:");
                    Console.WriteLine($"       Positions: {stats.count}, Volume: {stats.totalVolume:F2}, P&L: {stats.totalPnL:F2}");
                }
            }
            else
            {
                Console.WriteLine("    → No position statistics available");
            }
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[8] GetPositionStatsBySymbolAsync: ⚠ {ex.Message.Split('.')[0]}\n");
        }

        // ════════════════════════════════════════════════════════════════
        // STEP 4: HISTORY QUERIES
        // ════════════════════════════════════════════════════════════════
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ STEP 4: HISTORY QUERIES                                      ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

        // ══════════════════════════════════════════════════════════════
        // [9] OrdersHistoryLast
        //     Retrieves order history for the last N days with pagination.
        //     Supports sorting (by time, ticket, profit, etc.).
        //     Returns HistoryOrdersResponse with order details and metadata.
        //     Ideal for analyzing recent trading activity and performance.
        // ══════════════════════════════════════════════════════════════
        Console.WriteLine("[9] OrdersHistoryLast (last 7 days):");
        var recentOrders = await svc.OrdersHistoryLast(
            days: 7,
            page: 0,
            size: 10,
            sort: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc
        );

        if (recentOrders.HistoryData.Count > 0)
        {
            int orderIndex = 1;
            foreach (var historyItem in recentOrders.HistoryData.Take(5))
            {
                var order = historyItem.HistoryOrder;
                if (order != null)
                {
                    Console.WriteLine($"    {orderIndex}. Ticket #{order.Ticket}: {order.Symbol} {order.Type}");
                    Console.WriteLine($"       Volume: {order.VolumeCurrent}, State: {order.State}");
                    Console.WriteLine($"       Time: {order.SetupTime.ToDateTime():yyyy-MM-dd HH:mm:ss}");
                    orderIndex++;
                }
            }
            Console.WriteLine($"    ... total {recentOrders.HistoryData.Count} history items\n");
        }
        else
        {
            Console.WriteLine("    → No recent order history found\n");
        }

        // ══════════════════════════════════════════════════════════════
        // [10] PositionsHistoryPaged
        //      Queries closed position history with date range and pagination.
        //      Filters by open/close time ranges for precise historical queries.
        //      Returns PositionsHistoryResponse with position/deal details.
        //      Perfect for backtesting analysis and performance reports.
        // ══════════════════════════════════════════════════════════════
        Console.WriteLine("[10] PositionsHistoryPaged (last 24 hours, page 1):");

        var now = DateTime.UtcNow;
        var yesterday = now.AddDays(-1);

        var positionHistory = await svc.PositionsHistoryPaged(
            openFrom: yesterday,
            openTo: now,
            page: 0,
            size: 10
        );

        if (positionHistory.HistoryPositions.Count > 0)
        {
            int posIndex = 1;
            foreach (var position in positionHistory.HistoryPositions.Take(5))
            {
                Console.WriteLine($"    {posIndex}. Position #{position.PositionTicket}: {position.Symbol}");
                Console.WriteLine($"       Type: {position.OrderType}, Volume: {position.Volume}");
                Console.WriteLine($"       Profit: {position.Profit:F2} USD");
                Console.WriteLine($"       Open: {position.OpenTime.ToDateTime():yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"       Close: {position.CloseTime.ToDateTime():yyyy-MM-dd HH:mm:ss}");
                posIndex++;
            }
            Console.WriteLine($"    ... total {positionHistory.HistoryPositions.Count} positions\n");
        }
        else
        {
            Console.WriteLine("    → No position history in last 24 hours\n");
        }

        // ════════════════════════════════════════════════════════════════
        // STEP 5: CLEANUP TEST POSITIONS
        // ════════════════════════════════════════════════════════════════
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ STEP 5: CLEANUP TEST POSITIONS                               ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

        Console.WriteLine("Closing all test positions...");

        try
        {
            await svc.CloseAllPositions(symbol);
            Console.WriteLine($"✓ All positions on '{symbol}' closed\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Cleanup warning: {ex.Message}\n");
        }

        // Final verification
        int remainingPositions = await svc.GetPositionCountAsync();
        Console.WriteLine($"Final position count: {remainingPositions}");

        // ════════════════════════════════════════════════════════════════
        // SUMMARY
        // ════════════════════════════════════════════════════════════════
        Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ MONITORING & HISTORY DEMO COMPLETED                          ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

        Console.WriteLine("Demonstrated Methods (10):");
        Console.WriteLine("  ✓ Position Filters: GetProfitablePositionsAsync, GetLosingPositionsAsync");
        Console.WriteLine("  ✓ Statistics: GetTotalProfitLossAsync, GetPositionCountAsync");
        Console.WriteLine("  ✓ Grouping: GetPositionStatsBySymbolAsync");
        Console.WriteLine("  ✓ Session Info: GetQuoteSessionAsync, GetTradeSessionAsync");
        Console.WriteLine("  ✓ History: OrdersHistoryLast, PositionsHistoryPaged");
        Console.WriteLine("  ✓ Trading: PlaceMarket\n");

        Console.WriteLine("This demo showcases all monitoring and history capabilities");
        Console.WriteLine("of the MT5Sugar API for position tracking and analysis.\n");
    }

    private static void PrintHeader()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                                              ║");
        Console.WriteLine("║       MT5 SUGAR API - MONITORING & HISTORY DEMO              ║");
        Console.WriteLine("║                                                              ║");
        Console.WriteLine("║  SCENARIO: Real-time Position & History Monitor              ║");
        Console.WriteLine("║                                                              ║");
        Console.WriteLine("║  This demo showcases comprehensive position monitoring,      ║");
        Console.WriteLine("║  statistical analysis, and historical data querying.         ║");
        Console.WriteLine("║  You'll see filtering, aggregation, session info, and        ║");
        Console.WriteLine("║  paginated history access through clean Sugar API methods.   ║");
        Console.WriteLine("║                                                              ║");
        Console.WriteLine("║  WORKFLOW:                                                   ║");
        Console.WriteLine("║  → Open test positions with varying SL/TP settings           ║");
        Console.WriteLine("║  → Real-time monitoring of profitable/losing positions       ║");
        Console.WriteLine("║  → Calculate total P&L and position counts                   ║");
        Console.WriteLine("║  → Group statistics by symbol                                ║");
        Console.WriteLine("║  → Query order & position history with pagination            ║");
        Console.WriteLine("║  → Check session timing info (quote/trade sessions)          ║");
        Console.WriteLine("║  → Cleanup all test positions                                ║");
        Console.WriteLine("║                                                              ║");
        Console.WriteLine("║  DEMONSTRATED: 10 monitoring & history methods               ║");
        Console.WriteLine("║  Categories: Filtering, Statistics, Sessions, History        ║");
        Console.WriteLine("║                                                              ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");
    }
}

/*══════════════════════════════════════════════════════════════════════════════
 USAGE:
   dotnet run monitor

 QUICK CODE EXAMPLES:

   // [1] Place market order with SL/TP
   var result = await svc.PlaceMarket("EURUSD", 0.01, isBuy: true,
       sl: 1.0850, tp: 1.0900, comment: "Test");

   // [2-3] Session information
   var quoteSession = await svc.GetQuoteSessionAsync("EURUSD", DayOfWeek.Monday, 0);
   var tradeSession = await svc.GetTradeSessionAsync("EURUSD", DayOfWeek.Monday, 0);

   // [4] Count positions
   int totalCount = await svc.GetPositionCountAsync();
   int symbolCount = await svc.GetPositionCountAsync("EURUSD");

   // [5] Calculate profit/loss
   double totalPnL = await svc.GetTotalProfitLossAsync();
   double symbolPnL = await svc.GetTotalProfitLossAsync("EURUSD");

   // [6-7] Filter positions by profit
   var winners = await svc.GetProfitablePositionsAsync();
   var losers = await svc.GetLosingPositionsAsync("EURUSD");

   // [8] Statistics grouped by symbol
   var stats = await svc.GetPositionStatsBySymbolAsync();
   foreach (var kvp in stats) {
       Console.WriteLine($"{kvp.Key}: {kvp.Value.count} positions");
   }

   // [9] Recent order history
   var orders = await svc.OrdersHistoryLast(
       days: 7, page: 0, size: 10,
       sort: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc
   );

   // [10] Closed positions history with date range
   var history = await svc.PositionsHistoryPaged(
       openFrom: DateTime.UtcNow.AddDays(-7),
       openTo: DateTime.UtcNow,
       page: 0, size: 20
   );

══════════════════════════════════════════════════════════════════════════════*/
