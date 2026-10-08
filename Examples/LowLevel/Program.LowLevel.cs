/*══════════════════════════════════════════════════════════════════════════════
 FILE: Examples/LowLevel/Program.LowLevel.cs - LOW-LEVEL MT5 API INFORMATION DEMO
 PURPOSE:
   Comprehensive demonstration of MT5 information retrieval methods via MT5Account class.
   This is a REFERENCE GUIDE for account, symbol, position, and market data queries
   WITHOUT trading operations (see Program.Trading.cs for trading examples).

 🎯 WHO SHOULD USE THIS:
   • Developers learning MT5 gRPC API information methods
   • Users building monitoring/dashboard applications
   • Debugging data retrieval and understanding API structures
   • Learning read-only operations on MT5Account class

 📚 WHAT THIS DEMO COVERS (5 Sections):

   1. ACCOUNT INFORMATION
      • AccountSummaryAsync() - Get all account data in one call (RECOMMENDED)
      • AccountInfoDoubleAsync() - Individual properties (Balance, Equity, Margin, etc.)
      • AccountInfoIntegerAsync() - Integer properties (Login, Leverage, etc.)
      • AccountInfoStringAsync() - String properties (Currency, Company, etc.)

   2. SYMBOL INFORMATION & OPERATIONS
      • SymbolSelectAsync() - Add/remove symbol from Market Watch
      • SymbolExistAsync() - Check if symbol exists
      • SymbolIsSynchronizedAsync() - Check sync status
      • SymbolInfoDoubleAsync() - Bid, Ask, Point, Volume Min/Max/Step
      • SymbolInfoIntegerAsync() - Digits, Spread, Stops Level
      • SymbolInfoStringAsync() - Description, Base/Profit Currency
      • SymbolInfoTickAsync() - Get last tick data
      • SymbolsTotalAsync() - Count total/selected symbols
      • SymbolNameAsync() - Get symbol name by index
      • SymbolInfoMarginRateAsync() - Get margin requirements
      • SymbolInfoSessionQuoteAsync() - Quote session times
      • SymbolInfoSessionTradeAsync() - Trade session times
      • SymbolParamsManyAsync() - Detailed symbol parameters
      • TickValueWithSizeAsync() - Tick values for multiple symbols

   3. POSITIONS & ORDERS INFORMATION
      • PositionsTotalAsync() - Count open positions
      • OpenedOrdersAsync() - Get all opened orders & positions
      • OpenedOrdersTicketsAsync() - Get only ticket numbers (lightweight)
      • OrderHistoryAsync() - Historical orders with pagination
      • PositionsHistoryAsync() - Historical positions

   4. MARKET DEPTH (DOM - Depth of Market)
      • MarketBookAddAsync() - Subscribe to DOM updates
      • MarketBookGetAsync() - Get current market depth
      • MarketBookReleaseAsync() - Unsubscribe from DOM

   5. STREAMING METHODS (Reference Only)
      • Lists available streaming methods (not executed in this demo)
      • See Program.Streaming.cs for full streaming examples


 🔄 COMPARISON: Low-Level vs High-Level API:

   Low-Level (MT5Account):
   ✓ Direct gRPC calls with raw protobuf messages
   ✓ Maximum control and flexibility
   ✓ See exactly what data MT5 API returns
   ✗ More verbose code
   ✗ Need to handle protobuf structures manually

   High-Level (MT5Service):
   ✓ Simplified wrapper methods (BuyMarket, SellMarket, etc.)
   ✓ Cleaner code, less boilerplate
   ✓ Built-in error handling and helpers
   ✗ Less control over exact API calls
   ✗ Abstracts away some details

 💡 WHEN TO USE LOW-LEVEL API:
   • You need access to methods not wrapped in MT5Service
   • Building your own custom wrappers
   • Debugging issues with high-level wrappers
   • Need exact control over protobuf request/response
   • Performance-critical operations requiring minimal overhead

 📖 RELATED EXAMPLES:
   • Program.Trading.cs - Trading operations (OrderSend, OrderModify, OrderClose)
   • Program.Streaming.cs - Real-time data streams (Ticks, Trades, P/L)
   • MT5Service examples - High-level trading wrappers
   • MT5Sugar examples - Syntactic sugar for common operations

 USAGE:
   dotnet run 1
   dotnet run lowlevel
══════════════════════════════════════════════════════════════════════════════*/

using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;
using mt5_term_api;
using Microsoft.Extensions.Configuration;

namespace MetaRPC.CSharpMT5.Examples.LowLevel
{
    public static class ProgramLowLevel
    {
        public static async Task RunAsync()
        {
            PrintBanner();

            MT5Account? account = null;
            try
            {
                var config = ConnectionHelper.BuildConfiguration();
                account = await ConnectionHelper.CreateAndConnectAccountAsync(config);
                await RunAllDemosAsync(account, config);

                ConsoleHelper.PrintSuccess("\n✓ ALL LOW-LEVEL DEMOS COMPLETED");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"\n✗ FATAL: {ex.Message}");
                throw;
            }
            finally
            {
                await ConnectionHelper.DisconnectAsync(account);
            }
        }

        private static async Task RunAllDemosAsync(MT5Account acc, IConfiguration config)
        {
            var symbol = config["MT5:BaseChartSymbol"] ?? "EURUSD";

            #region ACCOUNT INFORMATION
            // ══════════════════════════════════════════════════════════════
            // 1. ACCOUNT INFORMATION DEMO
            //    Retrieve account details: balance, equity, margin, leverage,
            //    currency, company info, and trading permissions.
            //    Essential for monitoring account state and risk management.
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("1. ACCOUNT INFORMATION");

            // ─────────────────────────────────────────────────────────────
            // METHOD 1: AccountSummaryAsync() - ONE CALL GETS EVERYTHING
            // ─────────────────────────────────────────────────────────────
            // ✅ RECOMMENDED: This is the most efficient way to get account data.
            //    Single gRPC call returns all account properties at once.
            //    Use this unless you need only 1-2 specific properties.
            Console.WriteLine("  [1.1] AccountSummaryAsync() - Get all account data:");
            var summary = await acc.AccountSummaryAsync();

            // Display core account properties
            Console.WriteLine($"        Login:    {summary.AccountLogin}");
            Console.WriteLine($"        Balance:  {summary.AccountBalance:F2} {summary.AccountCurrency}");
            Console.WriteLine($"        Equity:   {summary.AccountEquity:F2}");
            Console.WriteLine($"        Credit:   {summary.AccountCredit:F2}");
            Console.WriteLine($"        Leverage: 1:{summary.AccountLeverage}");
            Console.WriteLine($"        Company:  {summary.AccountCompanyName}");
            Console.WriteLine($"        Name:     {summary.AccountUserName}\n");

            // Get margin information (still using summary object)
            // Note: Margin properties require separate calls even with AccountSummaryAsync
            var margin = await acc.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountMargin);
            var marginFree = await acc.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountMarginFree);
            var marginLevel = await acc.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountMarginLevel);
            Console.WriteLine($"        Margin:   {margin:F2}");
            Console.WriteLine($"        Free:     {marginFree:F2}");
            Console.WriteLine($"        Level:    {marginLevel:F2}%\n");

            // ─────────────────────────────────────────────────────────────
            // METHOD 2: AccountInfoXxxAsync() - INDIVIDUAL PROPERTY CALLS
            // ─────────────────────────────────────────────────────────────
            // Alternative approach: Get each property separately
            // Use when you need only specific properties (fewer gRPC calls)
            // Properties are categorized by type: Double, Integer, String
            Console.WriteLine("  [1.2] Individual AccountInfo calls:");

            // Double properties (numeric values with decimals)
            var balance = await acc.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountBalance);
            var equity = await acc.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountEquity);

            // Integer properties (whole numbers like login, leverage)
            var login = await acc.AccountInfoIntegerAsync(AccountInfoIntegerPropertyType.AccountLogin);
            var leverage = await acc.AccountInfoIntegerAsync(AccountInfoIntegerPropertyType.AccountLeverage);

            // String properties (text values like currency, company)
            // For string properties, use AccountSummary which provides reliable access
            var summaryData = await acc.AccountSummaryAsync();
            var currency = summaryData.AccountCurrency;
            var company = summaryData.AccountCompanyName;

            Console.WriteLine($"        Balance:  {balance:F2} {currency}");
            Console.WriteLine($"        Equity:   {equity:F2}");
            Console.WriteLine($"        Login:    {login}");
            Console.WriteLine($"        Leverage: 1:{leverage}");
            Console.WriteLine($"        Company:  {company}\n");
            #endregion

            #region SYMBOL INFORMATION
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("2. SYMBOL INFORMATION & OPERATIONS");
            // ══════════════════════════════════════════════════════════════

            // ─────────────────────────────────────────────────────────────
            // BASIC SYMBOL OPERATIONS - Select, Check Existence, Check Sync
            // ─────────────────────────────────────────────────────────────
            Console.WriteLine($"  [2.1] Symbol: {symbol}\n");

            // Add symbol to Market Watch (required before trading)
            await acc.SymbolSelectAsync(symbol, select: true);
            Console.WriteLine($"        ✓ SymbolSelectAsync() - Symbol selected");

            // Verify symbol exists on this broker
            var symbolExists = await acc.SymbolExistAsync(symbol);
            Console.WriteLine($"        ✓ SymbolExistAsync() - Exists: {symbolExists.Exists}, Custom: {symbolExists.IsCustom}");

            // Check if symbol data is synchronized with server
            var isSynced = await acc.SymbolIsSynchronizedAsync(symbol);
            Console.WriteLine($"        ✓ SymbolIsSynchronizedAsync() - Synced: {isSynced.Synchronized}\n");

            // ─────────────────────────────────────────────────────────────
            // SYMBOL PROPERTIES - Get individual Double/Integer/String values
            // ─────────────────────────────────────────────────────────────
            // Properties are categorized by data type (similar to AccountInfo pattern)
            // Each call is a separate gRPC request to MT5

            // Double properties: prices, volumes, point size
            Console.WriteLine("  [2.2] SymbolInfoDoubleAsync() - Double properties:");
            var bid = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolBid);
            var ask = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolAsk);
            var point = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint);
            var volumeMin = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeMin);
            var volumeMax = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeMax);
            var volumeStep = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeStep);

            Console.WriteLine($"        Bid:         {bid.Value:F5}");
            Console.WriteLine($"        Ask:         {ask.Value:F5}");
            Console.WriteLine($"        Point:       {point.Value:F5}");
            Console.WriteLine($"        Volume Min:  {volumeMin.Value:F2}");
            Console.WriteLine($"        Volume Max:  {volumeMax.Value:F2}");
            Console.WriteLine($"        Volume Step: {volumeStep.Value:F2}\n");

            // Integer properties: digits, spread, stops level
            Console.WriteLine("  [2.3] SymbolInfoIntegerAsync() - Integer properties:");
            var digits = await acc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolDigits);
            var spread = await acc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolSpread);
            var stopsLevel = await acc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolTradeStopsLevel);
            Console.WriteLine($"        Digits:       {digits.Value}");
            Console.WriteLine($"        Spread:       {spread.Value}");
            Console.WriteLine($"        Stops Level:  {stopsLevel.Value}\n");

            // String properties: description, currencies
            Console.WriteLine("  [2.4] SymbolInfoStringAsync() - String properties:");
            var description = await acc.SymbolInfoStringAsync(symbol, SymbolInfoStringProperty.SymbolDescription);
            var baseCurrency = await acc.SymbolInfoStringAsync(symbol, SymbolInfoStringProperty.SymbolCurrencyBase);
            var profitCurrency = await acc.SymbolInfoStringAsync(symbol, SymbolInfoStringProperty.SymbolCurrencyProfit);
            Console.WriteLine($"        Description:  {description.Value}");
            Console.WriteLine($"        Base:         {baseCurrency.Value}");
            Console.WriteLine($"        Profit:       {profitCurrency.Value}\n");

            // ─────────────────────────────────────────────────────────────
            // TICK DATA & SYMBOL LIST
            // ─────────────────────────────────────────────────────────────

            // Get current tick with timestamp
            Console.WriteLine("  [2.5] SymbolInfoTickAsync() - Get last tick:");
            var tick = await acc.SymbolInfoTickAsync(symbol);
            Console.WriteLine($"        Time:   {DateTimeOffset.FromUnixTimeSeconds(tick.Time).DateTime}");
            Console.WriteLine($"        Bid:    {tick.Bid:F5}");
            Console.WriteLine($"        Ask:    {tick.Ask:F5}");
            Console.WriteLine($"        Last:   {tick.Last:F5}");
            Console.WriteLine($"        Volume: {tick.Volume}\n");

            // Count total symbols (in Market Watch vs all available)
            Console.WriteLine("  [2.6] SymbolsTotalAsync() - Count symbols:");
            var totalSelected = await acc.SymbolsTotalAsync(selectedOnly: true);
            var totalAll = await acc.SymbolsTotalAsync(selectedOnly: false);
            Console.WriteLine($"        Selected in MarketWatch: {totalSelected.Total}");
            Console.WriteLine($"        Total available:         {totalAll.Total}\n");

            // Get symbol name by position in Market Watch
            Console.WriteLine("  [2.7] SymbolNameAsync() - Get symbol by index:");
            var symbolName = await acc.SymbolNameAsync(index: 0, selected: true);
            Console.WriteLine($"        Symbol[0]: {symbolName.Name}\n");

            // ─────────────────────────────────────────────────────────────
            // ADVANCED SYMBOL INFO - Margin, Sessions, Detailed Parameters
            // ─────────────────────────────────────────────────────────────
            // ⚠️ Note: Not all brokers support these advanced methods

            Console.WriteLine("  [2.8] SymbolInfoMarginRateAsync() - Get margin rates:");
            try
            {
                var marginRate = await acc.SymbolInfoMarginRateAsync(symbol, ENUM_ORDER_TYPE.OrderTypeBuy);
                Console.WriteLine($"        Initial margin rate:      {marginRate.InitialMarginRate:F4}");
                Console.WriteLine($"        Maintenance margin rate:  {marginRate.MaintenanceMarginRate:F4}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"        ⚠️  Margin rate not available (broker may not support this method)");
                Console.WriteLine($"        Error: {ex.Message.Split('\n')[0]}");
            }
            Console.WriteLine();

            // Get quote session times (when quotes are available)
            Console.WriteLine("  [2.9] SymbolInfoSessionQuoteAsync() - Get quote session:");
            try
            {
                var deadline5sec = DateTime.UtcNow.AddSeconds(5);
                var sessionQuote = await acc.SymbolInfoSessionQuoteAsync(symbol, mt5_term_api.DayOfWeek.Monday, sessionIndex: 0, deadline: deadline5sec);
                Console.WriteLine($"        From: {sessionQuote.From}");
                Console.WriteLine($"        To:   {sessionQuote.To}");
            }
            catch
            {
                Console.WriteLine($"        No quote session for {symbol}");
            }
            Console.WriteLine();

            // Get trade session times (when trading is allowed)
            Console.WriteLine("  [2.10] SymbolInfoSessionTradeAsync() - Get trade session:");
            try
            {
                var deadline5sec = DateTime.UtcNow.AddSeconds(5);
                var sessionTrade = await acc.SymbolInfoSessionTradeAsync(symbol, mt5_term_api.DayOfWeek.Monday, sessionIndex: 0, deadline: deadline5sec);
                Console.WriteLine($"        From: {sessionTrade.From}");
                Console.WriteLine($"        To:   {sessionTrade.To}");
            }
            catch
            {
                Console.WriteLine($"        No trade session for {symbol}");
            }
            Console.WriteLine();

            // Get detailed parameters for multiple symbols at once
            Console.WriteLine("  [2.11] SymbolParamsManyAsync() - Get detailed symbol parameters:");
            try
            {
                var deadline10sec = DateTime.UtcNow.AddSeconds(10);
                var symbolParamsRequest = new SymbolParamsManyRequest();
                var symbolParams = await acc.SymbolParamsManyAsync(symbolParamsRequest, deadline10sec);
                Console.WriteLine($"        Total symbols: {symbolParams.SymbolsTotal}");
                if (symbolParams.SymbolInfos.Count > 0)
                {
                    var info = symbolParams.SymbolInfos[0];
                    Console.WriteLine($"        First symbol:    {info.Name}");
                    Console.WriteLine($"        Bid:             {info.Bid:F5}");
                    Console.WriteLine($"        Ask:             {info.Ask:F5}");
                    Console.WriteLine($"        Contract size:   {info.TradeContractSize}");
                    Console.WriteLine($"        Tick value:      {info.TradeTickValue:F5}");
                    Console.WriteLine($"        Tick size:       {info.TradeTickSize:F5}");
                    Console.WriteLine($"        Swap long:       {info.SwapLong:F2}");
                    Console.WriteLine($"        Swap short:      {info.SwapShort:F2}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"        ⚠️  Symbol params not available (broker may not support this method)");
                Console.WriteLine($"        Error: {ex.Message.Split('\n')[0]}");
            }
            Console.WriteLine();

            // Get tick values for multiple symbols (batch operation)
            Console.WriteLine("  [2.12] TickValueWithSizeAsync() - Get tick values:");
            try
            {
                var deadline10sec = DateTime.UtcNow.AddSeconds(10);
                var tickValues = await acc.TickValueWithSizeAsync(new[] { symbol }, deadline10sec);
                foreach (var tickVal in tickValues.SymbolTickSizeInfos)
                {
                    Console.WriteLine($"        {tickVal.Name,-10} | Tick value: {tickVal.TradeTickValue:F5} | Tick size: {tickVal.TradeTickSize:F5}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"        ⚠️  Tick values not available (broker may not support this method)");
                Console.WriteLine($"        Error: {ex.Message.Split('\n')[0]}");
            }
            Console.WriteLine();
            #endregion

            #region ORDERS INFORMATION
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("3. POSITIONS & ORDERS INFORMATION");
            // ══════════════════════════════════════════════════════════════

            // ─────────────────────────────────────────────────────────────
            // CURRENT POSITIONS & ORDERS - Real-time Open Trades
            // ─────────────────────────────────────────────────────────────

            // Count total open positions (fast, lightweight)
            Console.WriteLine("  [3.1] PositionsTotalAsync() - Count open positions:");
            var positionsTotal = await acc.PositionsTotalAsync();
            Console.WriteLine($"        Total positions: {positionsTotal.TotalPositions}\n");

            // Get full data for all opened orders and positions
            Console.WriteLine("  [3.2] OpenedOrdersAsync() - Get all opened orders & positions:");
            var openedOrders = await acc.OpenedOrdersAsync(BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeDesc);
            Console.WriteLine($"        Opened orders:   {openedOrders.OpenedOrders.Count}");
            Console.WriteLine($"        Open positions:  {openedOrders.PositionInfos.Count}");

            if (openedOrders.PositionInfos.Count > 0)
            {
                var pos = openedOrders.PositionInfos[0];
                Console.WriteLine($"\n        First Position:");
                Console.WriteLine($"          Ticket:  {pos.Ticket}");
                Console.WriteLine($"          Symbol:  {pos.Symbol}");
                Console.WriteLine($"          Type:    {pos.Type}");
                Console.WriteLine($"          Volume:  {pos.Volume:F2}");
                Console.WriteLine($"          Profit:  {pos.Profit:F2}");
            }
            Console.WriteLine();

            // Get only ticket numbers (very fast, minimal data transfer)
            Console.WriteLine("  [3.3] OpenedOrdersTicketsAsync() - Get ticket list:");
            var tickets = await acc.OpenedOrdersTicketsAsync();
            Console.WriteLine($"        Position tickets: {tickets.OpenedPositionTickets.Count}");
            Console.WriteLine($"        Order tickets:    {tickets.OpenedOrdersTickets.Count}\n");

            // ─────────────────────────────────────────────────────────────
            // HISTORICAL DATA - Past Orders & Closed Positions
            // ─────────────────────────────────────────────────────────────

            // Get order history with pagination
            Console.WriteLine("  [3.4] OrderHistoryAsync() - Get history (last 7 days):");
            var fromTime = DateTime.UtcNow.AddDays(-7);
            var toTime = DateTime.UtcNow;
            var history = await acc.OrderHistoryAsync(
                fromTime,
                toTime,
                BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc,
                pageNumber: 0,
                itemsPerPage: 10
            );
            Console.WriteLine($"        Total orders: {history.ArrayTotal}");
            Console.WriteLine($"        Page:         {history.PageNumber}");
            Console.WriteLine($"        Per page:     {history.ItemsPerPage}");
            Console.WriteLine($"        Returned:     {history.HistoryData.Count}\n");

            // Get positions history (closed positions with P&L)
            Console.WriteLine("  [3.5] PositionsHistoryAsync() - Get positions history:");
            var posHistory = await acc.PositionsHistoryAsync(
                AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeDesc,
                openFrom: fromTime,
                openTo: toTime,
                page: 0,
                size: 10
            );
            Console.WriteLine($"        Positions returned: {posHistory.HistoryPositions.Count}\n");
            #endregion

            #region MARKET DEPTH
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("4. MARKET DEPTH (DOM)");
            // ══════════════════════════════════════════════════════════════
            // ⚠️ Note: Market Depth (Order Book) is NOT available for all brokers/symbols
            //    Many Forex brokers don't provide Level 2 data

            Console.WriteLine($"  Market Book (Depth of Market) - Testing {symbol}:\n");
            try
            {
                // Subscribe to market depth updates
                Console.WriteLine($"  [4.1] MarketBookAddAsync() - Subscribe to DOM:");
                var deadline5sec = DateTime.UtcNow.AddSeconds(5);
                var domAdd = await acc.MarketBookAddAsync(symbol, deadline5sec);
                Console.WriteLine($"        Subscription opened: {domAdd.OpenedSuccessfully}");

                if (domAdd.OpenedSuccessfully)
                {
                    // Get current market depth snapshot
                    Console.WriteLine($"  [4.2] MarketBookGetAsync() - Get market depth:");
                    var deadline15sec = DateTime.UtcNow.AddSeconds(15);
                    var domData = await acc.MarketBookGetAsync(symbol, deadline15sec);
                    Console.WriteLine($"        DOM entries: {domData.MqlBookInfos.Count}");

                    if (domData.MqlBookInfos.Count > 0)
                    {
                        Console.WriteLine("\n        First 5 entries:");
                        foreach (var entry in domData.MqlBookInfos.Take(5))
                        {
                            Console.WriteLine($"          {entry.Type,20} | Price: {entry.Price:F5} | Volume: {entry.Volume}");
                        }
                    }

                    // Unsubscribe to clean up resources
                    Console.WriteLine($"\n  [4.3] MarketBookReleaseAsync() - Unsubscribe:");
                    var deadline15sec2 = DateTime.UtcNow.AddSeconds(15);
                    var domRelease = await acc.MarketBookReleaseAsync(symbol, deadline15sec2);
                    Console.WriteLine($"        Subscription closed: {domRelease.ClosedSuccessfully}\n");
                }
                else
                {
                    Console.WriteLine($"        DOM subscription failed for {symbol}\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"        ⚠ DOM not available: {ex.Message}");
                Console.WriteLine($"        (Broker may not provide market depth for {symbol})\n");
            }
            #endregion

            #region STREAMING
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("5. STREAMING METHODS (REFERENCE)");
            // ══════════════════════════════════════════════════════════════
            // These methods are NOT executed in this demo to avoid keeping streams open.
            // Full streaming examples are available separately.

            Console.WriteLine("  Available streaming methods from MT5Account class:\n");
            Console.WriteLine("    • SubscribeToTicksAsync()");
            Console.WriteLine("      → Real-time tick data stream (Bid/Ask/Volume updates)");
            Console.WriteLine("      → Use for: Price monitoring, technical indicators\n");

            Console.WriteLine("    • SubscribeToTradeTransactionAsync()");
            Console.WriteLine("      → Trade transaction events (order fills, modifications)");
            Console.WriteLine("      → Use for: Order execution tracking, trade confirmations\n");

            Console.WriteLine("    • SubscribeToPositionProfitAsync()");
            Console.WriteLine("      → Position P&L updates (real-time profit/loss changes)");
            Console.WriteLine("      → Use for: Risk management, dynamic stop-loss adjustment\n");

            Console.WriteLine("  💡 For detailed streaming examples:");
            Console.WriteLine("     Run: dotnet run streaming\n");

            Console.WriteLine("  💡 For trading operations:");
            Console.WriteLine("     Run: dotnet run trading\n");
            #endregion
        }

        private static void PrintBanner()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║              LOW-LEVEL MT5 API DEMO                              ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║  Direct gRPC calls via MT5Account                                ║");
            Console.WriteLine("║  • No abstractions                                               ║");
            Console.WriteLine("║  • Raw protobuf messages                                         ║");
            Console.WriteLine("║  • Maximum control                                               ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }
    }
}

/*
 ═══════════════════════════════════════════════════════════════════════════════
  REFERENCE GUIDE - Quick lookup for developers
 ═══════════════════════════════════════════════════════════════════════════════

 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  COMMON RETURN CODES (from OrderSend, OrderModify, OrderClose)              │
 └─────────────────────────────────────────────────────────────────────────────┘

 Success codes:
   10009  TRADE_RETCODE_DONE       - Request completed successfully
   10008  TRADE_RETCODE_PLACED     - Order placed (pending order)

 Error codes (most common):
   10004  TRADE_RETCODE_REJECT     - Request rejected by server
   10006  TRADE_RETCODE_REQUOTE    - Requote (price changed)
   10013  TRADE_RETCODE_INVALID    - Invalid request
   10014  TRADE_RETCODE_INVALID_VOLUME - Invalid volume
   10015  TRADE_RETCODE_INVALID_PRICE  - Invalid price
   10016  TRADE_RETCODE_INVALID_STOPS  - Invalid stops (SL/TP)
   10018  TRADE_RETCODE_MARKET_CLOSED  - Market is closed
   10019  TRADE_RETCODE_NO_MONEY       - Not enough money
   10025  TRADE_RETCODE_TOO_MANY_REQUESTS - Too many requests


 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  PROPERTY TYPES CHEAT SHEET                                                 │
 └─────────────────────────────────────────────────────────────────────────────┘

 AccountInfo:
   DOUBLE   → Balance, Equity, Credit, Margin, MarginFree, MarginLevel, Profit
   INTEGER  → Login, Leverage, LimitOrders, MarginMode, TradeAllowed
   STRING   → Currency, Company, Name, Server

 SymbolInfo:
   DOUBLE   → Bid, Ask, Last, Point, VolumeMin, VolumeMax, VolumeStep,
              ContractSize, TickValue, TickSize, SwapLong, SwapShort
   INTEGER  → Digits, Spread, StopsLevel, FreezeLevel, TradeMode, TradeExecution
   STRING   → Description, Path, CurrencyBase, CurrencyProfit, CurrencyMargin


 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  ORDER TYPE FILLING MODES                                                   │
 └─────────────────────────────────────────────────────────────────────────────┘

   IOC (Immediate or Cancel):
     - Execute immediately at market price
     - Cancel remaining volume if not filled
     - ✅ MOST COMMON for market orders

   FOK (Fill or Kill):
     - Execute entire order or reject completely
     - No partial fills allowed
     - Use when exact volume is critical

   RETURN (Return):
     - Execute available volume
     - Place rest as limit order
     - Rarely used, broker-dependent

 ⚠️ Not all brokers support all modes! Check broker documentation.


 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  COMMON GOTCHAS & TROUBLESHOOTING                                           │
 └─────────────────────────────────────────────────────────────────────────────┘

 1. OrderCheck fails with zero price
    ❌ Price = 0
    ✅ Price = ask.Value (for Buy) or bid.Value (for Sell)
    → OrderCheck REQUIRES actual market price, not zero

 2. OrderSend rejected: "Invalid stops"
    Problem: SL/TP too close to market price
    Solution: Check SymbolInfoInteger(SYMBOL_TRADE_STOPS_LEVEL)
    → Stops must be at least StopsLevel points away from entry

 3. OrderSend rejected: "Invalid volume"
    Problem: Volume doesn't match broker constraints
    Solution: Use MT5Sugar.NormalizeVolumeAsync() or check:
    - VolumeMin: Minimum lot size (e.g., 0.01)
    - VolumeMax: Maximum lot size (e.g., 100.0)
    - VolumeStep: Volume increment (e.g., 0.01)
    → Volume must be: VolumeMin ≤ volume ≤ VolumeMax and divisible by VolumeStep

 4. MarketBookAdd fails
    Problem: Broker doesn't provide market depth for this symbol
    Solution: Check broker documentation or test with stocks/futures
    → Most Forex brokers DON'T provide Level 2 data

 5. Deadline parameter confusion
    ⚠️ Some methods have optional 'deadline' parameter (DateTime)
    - If NOT provided: Uses default timeout (usually 30 seconds)
    - If provided: Must be DateTime.UtcNow.AddSeconds(N)
    → Always use UTC time, not local time!

 6. Position vs Order confusion
    Position: Net exposure in a symbol (netting mode)
    Order: Individual trade ticket
    → MT5 accounts can be in NETTING or HEDGING mode
       - Netting: One position per symbol (combines all trades)
       - Hedging: Multiple positions allowed per symbol


 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  PERFORMANCE TIPS                                                           │
 └─────────────────────────────────────────────────────────────────────────────┘

 Use batch methods when possible:
   ❌ for (symbol in symbols) { await SymbolInfoTickAsync(symbol); }
   ✅ await SymbolParamsManyAsync(symbols);
   → Single gRPC call vs N calls = faster & less overhead

 Prefer summary methods over individual property calls:
   ❌ await AccountInfoDoubleAsync(Balance);
      await AccountInfoDoubleAsync(Equity);
      await AccountInfoIntegerAsync(Login);
   ✅ var summary = await AccountSummaryAsync();
   → 1 call vs 3 calls

 Use OpenedOrdersTicketsAsync() when you only need ticket numbers:
   ❌ var orders = await OpenedOrdersAsync(); // Returns full data
   ✅ var tickets = await OpenedOrdersTicketsAsync(); // Returns only tickets
   → Minimal data transfer = faster response


 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  WHEN TO USE LOW-LEVEL API vs HIGH-LEVEL (MT5Service/MT5Sugar)              │
 └─────────────────────────────────────────────────────────────────────────────┘

 Use LOW-LEVEL API (MT5Account) when:
   ✓ You need methods not wrapped in MT5Service
   ✓ Building custom trading frameworks
   ✓ Debugging issues with high-level wrappers
   ✓ Need exact control over protobuf messages
   ✓ Performance-critical operations (skip abstraction layer)
   ✓ Information retrieval and market data queries

 Use HIGH-LEVEL API (MT5Service/MT5Sugar) when:
   ✓ Standard trading operations (open/close/modify positions)
   ✓ Risk management with automatic lot size calculation
   ✓ Cleaner, more readable code
   ✓ Built-in error handling and validation
   ✓ Rapid prototyping and development

 ┌─────────────────────────────────────────────────────────────────────────────┐
 │  ADDITIONAL EXAMPLES                                                         │
 └─────────────────────────────────────────────────────────────────────────────┘

 For trading operations, see:
   • Program.Trading.cs - Complete trading lifecycle (OrderSend/Modify/Close)
   • Run: dotnet run trading

 For streaming data, see:
   • Program.Streaming.cs - Real-time ticks, trades, P/L updates
   • Run: dotnet run streaming

*/
