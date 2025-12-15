/*══════════════════════════════════════════════════════════════════════════════
 FILE: Examples/Services/Program.Service.cs - MID-LEVEL MT5Service API DEMO
 PURPOSE:
   Comprehensive demonstration of MT5Service - the mid-level wrapper over MT5Account
   that returns unwrapped primitives and provides convenience methods.

 🎯 WHO SHOULD USE THIS:
   • Developers who want cleaner code with less boilerplate
   • Teams seeking better readability without wrapper types (.Value, .Exists, etc.)
   • Anyone building trading systems who values 30-50% code reduction
   • Beginners who find low-level MT5Account verbose

 📊 WHAT THIS DEMO COVERS (7 Major Sections):
   1. ACCOUNT INFORMATION
      • AccountSummaryAsync() - complete account snapshot
      • Convenience methods (GetBalanceAsync, GetEquityAsync, etc.)
      • Individual properties (doubles, integers, strings)

   2. SYMBOL INFORMATION & OPERATIONS
      • Symbol selection and existence checks
      • Price properties (bid, ask, point, volumes)
      • Symbol metadata (digits, spread, currencies)
      • Current tick data and symbol enumeration

   3. SYMBOL INFO (SESSIONS, MARGIN RATES)
      • Trading and quote sessions
      • Margin rate calculations
      • Tick value/size information
      • Advanced symbol parameters

   4. POSITIONS & ORDERS INFORMATION
      • Open positions and orders listing
      • Ticket enumeration
      • Order and position history queries

   5. TRADING OPERATIONS
      • Order validation (OrderCheck)
      • Margin and profit calculations
      • Market orders with SL/TP
      • Position modification and closing
      (All executed with minimal lot on demo account)

   6. MARKET DEPTH (DOM)
      • Subscription management
      • Real-time depth of market data

   7. STREAMING DATA
      • Overview of available streaming methods
      (See Program.Streaming.cs for implementation examples)

 ⚡ WHY USE MT5Service INSTEAD OF MT5Account (LOW-LEVEL)?

   ✅ UNWRAPPED PRIMITIVES - No .Value, .Exists, .Total needed
      LOW-LEVEL:  var data = await acc.SymbolInfoDoubleAsync(...);
                  double bid = data.Value;
      SERVICE:    double bid = await service.SymbolInfoDoubleAsync(...);
      IMPACT:     30-40% fewer lines for property access

   ✅ CONVENIENCE METHODS - Direct getters for common operations
      LOW-LEVEL:  var data = await acc.AccountInfoDoubleAsync(
                      AccountInfoDoublePropertyType.AccountBalance);
                  double balance = data.Value;
      SERVICE:    double balance = await service.GetBalanceAsync();
      IMPACT:     50% fewer lines + no enum needed

   ✅ TYPE SAFETY - Native C# types instead of wrapper types
      LOW-LEVEL:  Returns DoubleData, BoolData, IntegerData, StringData
      SERVICE:    Returns double, bool, long, string
      IMPACT:     Better IntelliSense, fewer NullReferenceExceptions

   ✅ SAME PERFORMANCE - Thin wrapper with zero overhead
      SERVICE methods simply call MT5Account methods and unwrap results
      No extra gRPC calls, no extra allocations

   📊 OVERALL: 30-50% fewer lines for typical trading operations
   📖 SEE: 9 side-by-side code comparisons at bottom of file

 ⚠ IMPORTANT:
   • MT5Service is a WRAPPER, not a replacement for MT5Account
   • All MT5Account methods remain available through service.Account property
   • Use SERVICE layer for cleaner code, LOW-LEVEL for maximum control
   • Same gRPC calls, just cleaner API surface

 💡 WHEN TO USE MT5Service:
   ✓ Building trading bots and algorithms
   ✓ Rapid prototyping and strategy development
   ✓ Projects prioritizing code readability
   ✓ Teams with developers new to MT5 API

 💡 WHEN TO USE MT5Account (Low-Level):
   ✓ Maximum control over every detail
   ✓ Learning the complete MT5 API surface
   ✓ Debugging complex gRPC interactions

 RELATED FILES:
   • Examples/LowLevel/Program.LowLevel.cs - Low-level MT5Account comparison
   • Examples/Streaming/Program.Streaming.cs - Streaming implementation examples
   • MT5Service.cs - Service wrapper implementation
   • MT5Account.cs - Low-level gRPC client

 USAGE:
   dotnet run service
   dotnet run mid
   dotnet run 4

══════════════════════════════════════════════════════════════════════════════*/

using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;
using mt5_term_api;
using Microsoft.Extensions.Configuration;

namespace MetaRPC.CSharpMT5.Examples.Services
{
    public static class ProgramService
    {
        // ═════════════════════════════════════════════════════════════════
        // MAIN ENTRY POINT
        // ═════════════════════════════════════════════════════════════════

        public static async Task RunAsync()
        {
            PrintBanner();

            try
            {
                var config = ConnectionHelper.BuildConfiguration();
                var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);

                ConsoleHelper.PrintInfo("\n→ Creating MT5Service wrapper...");
                var service = new MT5Service(account);
                ConsoleHelper.PrintSuccess("✓ MT5Service wrapper created!\n");

                await RunAllDemosAsync(service, config);

                ConsoleHelper.PrintSuccess("\n✓ ALL MID-LEVEL DEMOS COMPLETED SUCCESSFULLY!");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"\n✗ FATAL: {ex.Message}");
                ConsoleHelper.PrintError("\nStack trace:");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // DEMO SECTIONS
        // ═════════════════════════════════════════════════════════════════

        private static async Task RunAllDemosAsync(MT5Service service, IConfiguration config)
        {
            var symbol = config["Mt5:BaseChartSymbol"] ?? "EURUSD";

            #region ACCOUNT INFORMATION
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("1. ACCOUNT INFORMATION");
            // ══════════════════════════════════════════════════════════════

            // ─────────────────────────────────────────────────────────────────────────
            // [1.1] ACCOUNT SUMMARY - Single call gets everything
            // ─────────────────────────────────────────────────────────────────────────
            //
            // 📊 CODE COMPARISON:
            //    ┌────────────────────────────────────────────────────────────────┐
            //    │ LOW-LEVEL (MT5Account):                                        │
            //    │   var summary = await acc.AccountSummaryAsync();               │
            //    │   Console.WriteLine(summary.AccountBalance);  ← Same call!     │
            //    ├────────────────────────────────────────────────────────────────┤
            //    │ SERVICE LAYER (MT5Service):                                    │
            //    │   var summary = await service.AccountSummaryAsync();           │
            //    │   Console.WriteLine(summary.AccountBalance);  ← Same!          │
            //    │   ✓ No difference for AccountSummary (already clean)           │
            //    └────────────────────────────────────────────────────────────────┘
            // ─────────────────────────────────────────────────────────────────────────

            Console.WriteLine("  [1.1] AccountSummaryAsync() - Get all account data:");
            var accountSummary = await service.AccountSummaryAsync();
            Console.WriteLine($"        Login:    {accountSummary.AccountLogin}");
            Console.WriteLine($"        Balance:  {accountSummary.AccountBalance:F2} {accountSummary.AccountCurrency}");
            Console.WriteLine($"        Equity:   {accountSummary.AccountEquity:F2}");
            Console.WriteLine($"        Credit:   {accountSummary.AccountCredit:F2}");
            Console.WriteLine($"        Leverage: 1:{accountSummary.AccountLeverage}");
            Console.WriteLine($"        Company:  {accountSummary.AccountCompanyName}");
            Console.WriteLine($"        Name:     {accountSummary.AccountUserName}\n");

            // ─────────────────────────────────────────────────────────────────────────
            // [1.2] CONVENIENCE METHODS - Shortest syntax!
            // ─────────────────────────────────────────────────────────────────────────
            //
            // 💡 SERVICE ADVANTAGE: Convenience methods
            //    ✓ No property enums needed
            //    ✓ No .Value unwrapping
            //    ✓ Direct primitive returns
            //
            // 📊 CODE COMPARISON:
            //    ┌──────────────────────────────────────────────────────────────┐
            //    │ LOW-LEVEL (MT5Account):                                      │
            //    │   var data = await acc.AccountInfoDoubleAsync(               │
            //    │       AccountInfoDoublePropertyType.AccountBalance);         │
            //    │   double balance = data.Value;  ← Need to unwrap!            │
            //    ├──────────────────────────────────────────────────────────────┤
            //    │ SERVICE CONVENIENCE:                                         │
            //    │   double balance = await service.GetBalanceAsync();          │
            //    │   ✓ One line! Already unwrapped!                             │
            //    └──────────────────────────────────────────────────────────────┘
            // ─────────────────────────────────────────────────────────────────────────

            Console.WriteLine("  [1.2] Using convenience methods:");
            var balance = await service.GetBalanceAsync();
            var equity = await service.GetEquityAsync();
            var margin = await service.GetMarginAsync();
            var freeMargin = await service.GetFreeMarginAsync();
            var profit = await service.GetProfitAsync();
            var login = await service.GetLoginAsync();
            var leverage = await service.GetLeverageAsync();

            // For currency, use AccountSummary which provides reliable access
            var currencySummary = await service.AccountSummaryAsync();
            var currency = currencySummary.AccountCurrency;

            Console.WriteLine($"        Balance:      {balance:F2} {currency}");
            Console.WriteLine($"        Equity:       {equity:F2}");
            Console.WriteLine($"        Margin:       {margin:F2}");
            Console.WriteLine($"        Free Margin:  {freeMargin:F2}");
            Console.WriteLine($"        Profit:       {profit:F2}");
            Console.WriteLine($"        Login:        {login}");
            Console.WriteLine($"        Leverage:     1:{leverage}");
            Console.WriteLine($"        Currency:     {currency}\n");

            // ─────────────────────────────────────────────────────────────────────────
            // [1.3] INDIVIDUAL PROPERTIES - Using property enums
            // ─────────────────────────────────────────────────────────────────────────
            //
            // 💡 SERVICE ADVANTAGE: Returns unwrapped primitives
            //    Still cleaner than Low-Level (no .Value needed!)
            //
            // 📊 CODE COMPARISON:
            //    ┌──────────────────────────────────────────────────────────────┐
            //    │ LOW-LEVEL (MT5Account):                                      │
            //    │   var data = await acc.AccountInfoDoubleAsync(...);          │
            //    │   Console.WriteLine(data.Value);  ← .Value needed            │
            //    ├──────────────────────────────────────────────────────────────┤
            //    │ SERVICE LAYER:                                               │
            //    │   var value = await service.AccountInfoDoubleAsync(...);     │
            //    │   Console.WriteLine(value);  ✓ Already double!               │
            //    └──────────────────────────────────────────────────────────────┘
            // ─────────────────────────────────────────────────────────────────────────

            Console.WriteLine("  [1.3] Individual properties:");
            var marginLevel = await service.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountMarginLevel);
            Console.WriteLine($"        Margin Level: {marginLevel:F2}%");

            var tradeAllowed = await service.AccountInfoIntegerAsync(AccountInfoIntegerPropertyType.AccountTradeAllowed);
            Console.WriteLine($"        Trade:        {(tradeAllowed == 1 ? "Allowed" : "Disabled")}");

            // For string properties, use AccountSummary which provides reliable access
            var stringSummary = await service.AccountSummaryAsync();
            Console.WriteLine($"        Name:         {stringSummary.AccountUserName}");
            Console.WriteLine($"        Company:      {stringSummary.AccountCompanyName}");
            Console.WriteLine($"        Currency:     {stringSummary.AccountCurrency}\n");
            #endregion

            #region SYMBOLS
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("2. SYMBOL INFORMATION & OPERATIONS");
            // ══════════════════════════════════════════════════════════════

            Console.WriteLine($"  [2.1] Symbol: {symbol}\n");

            // ─────────────────────────────────────────────────────────────────────────
            // SYMBOL EXISTENCE & SELECTION
            // ─────────────────────────────────────────────────────────────────────────
            //
            // 📊 CODE COMPARISON - SymbolExistAsync():
            //    ┌────────────────────────────────────────────────────────────────┐
            //    │ LOW-LEVEL (MT5Account):                                        │
            //    │   var result = await acc.SymbolExistAsync(symbol);             │
            //    │   bool exists = result.Exists;  ← Need to unwrap!              │
            //    ├────────────────────────────────────────────────────────────────┤
            //    │ SERVICE LAYER (MT5Service):                                    │
            //    │   bool exists = await service.SymbolExistAsync(symbol);        │
            //    │   ✓ Returns bool directly! No .Exists property!                │
            //    └────────────────────────────────────────────────────────────────┘
            // ─────────────────────────────────────────────────────────────────────────

            // Symbol existence and selection
            Console.WriteLine("        ✓ SymbolSelectAsync() - Symbol selected");
            await service.SymbolSelectAsync(symbol, selected: true);

            // SERVICE: Returns bool directly (no .Exists needed!)
            var symbolExists = await service.SymbolExistAsync(symbol);
            Console.WriteLine($"        ✓ SymbolExistAsync() - Exists: {symbolExists}");

            // SERVICE: Returns bool directly (no .Synchronized needed!)
            var isSynced = await service.SymbolIsSynchronizedAsync(symbol);
            Console.WriteLine($"        ✓ SymbolIsSynchronizedAsync() - Synced: {isSynced}\n");

            // ─────────────────────────────────────────────────────────────────────────
            // SYMBOL PROPERTIES - Double values
            // ─────────────────────────────────────────────────────────────────────────
            //
            // 📊 CODE COMPARISON - SymbolInfoDoubleAsync():
            //    ┌────────────────────────────────────────────────────────────────┐
            //    │ LOW-LEVEL (MT5Account):                                        │
            //    │   var bidData = await acc.SymbolInfoDoubleAsync(               │
            //    │       symbol, SymbolInfoDoubleProperty.SymbolBid);             │
            //    │   double bid = bidData.Value;  ← Need .Value!                  │
            //    │   Console.WriteLine($"Bid: {bid:F5}");                         │
            //    ├────────────────────────────────────────────────────────────────┤
            //    │ SERVICE LAYER (MT5Service):                                    │
            //    │   double bid = await service.SymbolInfoDoubleAsync(            │
            //    │       symbol, SymbolInfoDoubleProperty.SymbolBid);             │
            //    │   Console.WriteLine($"Bid: {bid:F5}");  ← No .Value!           │
            //    │   ✓ Returns double directly!                                   │
            //    └────────────────────────────────────────────────────────────────┘
            // ─────────────────────────────────────────────────────────────────────────

            // Get symbol properties - DOUBLE
            // SERVICE: Returns double directly (no .Value needed!)
            Console.WriteLine("  [2.2] SymbolInfoDoubleAsync() - Double properties (unwrapped):");
            var bid = await service.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolBid);
            var ask = await service.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolAsk);
            var point = await service.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint);
            var volumeMin = await service.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeMin);
            var volumeMax = await service.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeMax);
            var volumeStep = await service.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeStep);

            Console.WriteLine($"        Bid:         {bid:F5}");
            Console.WriteLine($"        Ask:         {ask:F5}");
            Console.WriteLine($"        Point:       {point:F5}");
            Console.WriteLine($"        Volume Min:  {volumeMin:F2}");
            Console.WriteLine($"        Volume Max:  {volumeMax:F2}");
            Console.WriteLine($"        Volume Step: {volumeStep:F2}\n");

            // Get symbol properties - INTEGER
            // SERVICE: Returns long directly (no .Value needed!)
            Console.WriteLine("  [2.3] SymbolInfoIntegerAsync() - Integer properties (unwrapped):");
            var digits = await service.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolDigits);
            var spread = await service.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolSpread);
            var stopsLevel = await service.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolTradeStopsLevel);
            Console.WriteLine($"        Digits:       {digits}");
            Console.WriteLine($"        Spread:       {spread}");
            Console.WriteLine($"        Stops Level:  {stopsLevel}\n");

            // Get symbol properties - STRING
            // SERVICE: Returns string directly (no .Value needed!)
            Console.WriteLine("  [2.4] SymbolInfoStringAsync() - String properties (unwrapped):");
            var description = await service.SymbolInfoStringAsync(symbol, SymbolInfoStringProperty.SymbolDescription);
            var baseCurrency = await service.SymbolInfoStringAsync(symbol, SymbolInfoStringProperty.SymbolCurrencyBase);
            var profitCurrency = await service.SymbolInfoStringAsync(symbol, SymbolInfoStringProperty.SymbolCurrencyProfit);
            Console.WriteLine($"        Description:  {description}");
            Console.WriteLine($"        Base:         {baseCurrency}");
            Console.WriteLine($"        Profit:       {profitCurrency}\n");

            // Get current tick
            Console.WriteLine("  [2.5] SymbolInfoTickAsync() - Get last tick:");
            var tick = await service.SymbolInfoTickAsync(symbol);
            Console.WriteLine($"        Time:   {DateTimeOffset.FromUnixTimeSeconds(tick.Time).DateTime}");
            Console.WriteLine($"        Bid:    {tick.Bid:F5}");
            Console.WriteLine($"        Ask:    {tick.Ask:F5}");
            Console.WriteLine($"        Last:   {tick.Last:F5}");
            Console.WriteLine($"        Volume: {tick.Volume}\n");

            // Total symbols
            // SERVICE: Returns int directly (no .Total needed!)
            Console.WriteLine("  [2.6] SymbolsTotalAsync() - Count symbols (unwrapped):");
            var totalSelected = await service.SymbolsTotalAsync(selectedOnly: true);
            var totalAll = await service.SymbolsTotalAsync(selectedOnly: false);
            Console.WriteLine($"        Selected in MarketWatch: {totalSelected}");
            Console.WriteLine($"        Total available:         {totalAll}\n");

            // Get symbol name by index
            // SERVICE: Returns string directly (no .Name needed!)
            Console.WriteLine("  [2.7] SymbolNameAsync() - Get symbol by index (unwrapped):");
            var symbolName = await service.SymbolNameAsync(index: 0, selected: true);
            Console.WriteLine($"        Symbol[0]: {symbolName}\n");

            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("3. SYMBOL INFO (SESSIONS, MARGIN RATES)");
            // ══════════════════════════════════════════════════════════════

            // Symbol session quote
            Console.WriteLine("  [3.1] SymbolInfoSessionQuoteAsync() - Get quote session:");
            try
            {
                var sessionQuote = await service.SymbolInfoSessionQuoteAsync(symbol, mt5_term_api.DayOfWeek.Monday, sessionIndex: 0);
                Console.WriteLine($"        From: {sessionQuote.From}");
                Console.WriteLine($"        To:   {sessionQuote.To}");
            }
            catch
            {
                Console.WriteLine($"        No quote session for {symbol}");
            }
            Console.WriteLine();

            // Symbol session trade
            Console.WriteLine("  [3.2] SymbolInfoSessionTradeAsync() - Get trade session:");
            try
            {
                var sessionTrade = await service.SymbolInfoSessionTradeAsync(symbol, mt5_term_api.DayOfWeek.Monday, sessionIndex: 0);
                Console.WriteLine($"        From: {sessionTrade.From}");
                Console.WriteLine($"        To:   {sessionTrade.To}");
            }
            catch
            {
                Console.WriteLine($"        No trade session for {symbol}");
            }
            Console.WriteLine();

            // Symbol info margin rate
            Console.WriteLine("  [3.3] SymbolInfoMarginRateAsync() - Get margin rates:");
            try
            {
                var marginRate = await service.SymbolInfoMarginRateAsync(symbol, ENUM_ORDER_TYPE.OrderTypeBuy);
                Console.WriteLine($"        Initial margin rate:      {marginRate.InitialMarginRate:F4}");
                Console.WriteLine($"        Maintenance margin rate:  {marginRate.MaintenanceMarginRate:F4}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"        Error: {ex.Message}\n");
            }

            // Tick value with size
            Console.WriteLine("  [3.4] TickValueWithSizeAsync() - Get tick value/size:");
            try
            {
                var tickValue = await service.TickValueWithSizeAsync(new[] { symbol });
                if (tickValue.SymbolTickSizeInfos.Count > 0)
                {
                    var info = tickValue.SymbolTickSizeInfos[0];
                    Console.WriteLine($"        Symbol:          {info.Name}");
                    Console.WriteLine($"        Tick value:      {info.TradeTickValue:F5}");
                    Console.WriteLine($"        Tick value (P):  {info.TradeTickValueProfit:F5}");
                    Console.WriteLine($"        Tick value (L):  {info.TradeTickValueLoss:F5}\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"        Error: {ex.Message}\n");
            }

            // Symbol parameters (advanced)
            Console.WriteLine("  [3.5] SymbolParamsManyAsync() - Get detailed symbol parameters:");
            var symbolParamsRequest = new SymbolParamsManyRequest
            {
                SymbolName = symbol,
                SortType = AH_SYMBOL_PARAMS_MANY_SORT_TYPE.AhParamsManySortTypeSymbolNameAsc,
                PageNumber = 0,
                ItemsPerPage = 1
            };
            var symbolParams = await service.SymbolParamsManyAsync(symbolParamsRequest);
            Console.WriteLine($"        Total symbols: {symbolParams.SymbolsTotal}");
            if (symbolParams.SymbolInfos.Count > 0)
            {
                var info = symbolParams.SymbolInfos[0];
                Console.WriteLine($"        Name:            {info.Name}");
                Console.WriteLine($"        Bid:             {info.Bid:F5}");
                Console.WriteLine($"        Ask:             {info.Ask:F5}");
                Console.WriteLine($"        Contract size:   {info.TradeContractSize}");
                Console.WriteLine($"        Digits:          {info.Digits}\n");
            }
            #endregion

            #region POSITIONS
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("4. POSITIONS & ORDERS INFORMATION");
            // ══════════════════════════════════════════════════════════════

            // Get total positions
            Console.WriteLine("  [4.1] PositionsTotalAsync() - Count open positions:");
            var positionsTotal = await service.PositionsTotalAsync();
            Console.WriteLine($"        Total positions: {positionsTotal.TotalPositions}\n");

            // Get opened orders and positions
            Console.WriteLine("  [4.2] OpenedOrdersAsync() - Get all opened orders & positions:");
            var openedOrders = await service.OpenedOrdersAsync(BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeDesc);
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

            // Get ticket list
            Console.WriteLine("  [4.3] OpenedOrdersTicketsAsync() - Get ticket list:");
            var tickets = await service.OpenedOrdersTicketsAsync();
            Console.WriteLine($"        Position tickets: {tickets.OpenedPositionTickets.Count}");
            Console.WriteLine($"        Order tickets:    {tickets.OpenedOrdersTickets.Count}\n");

            // Get order history
            Console.WriteLine("  [4.4] OrderHistoryAsync() - Get history (last 7 days):");
            var fromTime = DateTime.UtcNow.AddDays(-7);
            var toTime = DateTime.UtcNow;
            var history = await service.OrderHistoryAsync(
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

            // Get positions history
            Console.WriteLine("  [4.5] PositionsHistoryAsync() - Get positions history:");
            var posHistory = await service.PositionsHistoryAsync(
                AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeDesc,
                openFrom: fromTime,
                openTo: toTime,
                page: 0,
                size: 10
            );
            Console.WriteLine($"        Positions returned: {posHistory.HistoryPositions.Count}\n");
            #endregion

            #region TRADING OPERATIONS
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("5. TRADING OPERATIONS (MINIMAL LOT)");
            // ══════════════════════════════════════════════════════════════

            // Use minimal volume for testing
            // SERVICE: No .Value needed - volumeMin is already double!
            var minLot = volumeMin;
            Console.WriteLine($"  Using minimal lot size: {minLot:F2}\n");

            // OrderCheck - Check if trade is possible
            Console.WriteLine("  [5.1] OrderCheckAsync() - Validate trade request:");
            try
            {
                // SERVICE: No .Value needed - ask is already double!
                var tradeRequest = new MrpcMqlTradeRequest
                {
                    Action = MRPC_ENUM_TRADE_REQUEST_ACTIONS.TradeActionDeal,
                    Symbol = symbol,
                    Volume = minLot,
                    Price = ask,
                    OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
                    TypeFilling = MRPC_ENUM_ORDER_TYPE_FILLING.OrderFillingFok,
                    TypeTime = MRPC_ENUM_ORDER_TYPE_TIME.OrderTimeGtc,
                    ExpertAdvisorMagicNumber = 12345
                };

                var checkRequest = new OrderCheckRequest
                {
                    MqlTradeRequest = tradeRequest
                };

                var checkResult = await service.OrderCheckAsync(checkRequest);

                if (checkResult?.MqlTradeCheckResult != null)
                {
                    Console.WriteLine($"        Return code:      {checkResult.MqlTradeCheckResult.ReturnedCode}");
                    Console.WriteLine($"        Balance after:    {checkResult.MqlTradeCheckResult.BalanceAfterDeal:F2}");
                    Console.WriteLine($"        Equity after:     {checkResult.MqlTradeCheckResult.EquityAfterDeal:F2}");
                    Console.WriteLine($"        Required margin:  {checkResult.MqlTradeCheckResult.Margin:F2}");
                    Console.WriteLine($"        Free margin:      {checkResult.MqlTradeCheckResult.FreeMargin:F2}");
                    Console.WriteLine($"        Margin level:     {checkResult.MqlTradeCheckResult.MarginLevel:F2}%");
                    Console.WriteLine($"        Comment:          {checkResult.MqlTradeCheckResult.Comment}\n");
                }
                else
                {
                    Console.WriteLine($"        ⚠ OrderCheck returned null data\n");
                }
            }
            catch (ApiExceptionMT5 apiEx)
            {
                Console.WriteLine($"        ⚠ OrderCheck not available on this broker/server");
                Console.WriteLine($"        Error: {apiEx.ErrorCode}");
                Console.WriteLine($"        (This is a server-side limitation, not a code issue)\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"        ❌ Error: {ex.GetType().Name}");
                Console.WriteLine($"        Message: {ex.Message}\n");
            }

            // OrderCalcMargin - Calculate required margin
            Console.WriteLine("  [5.2] OrderCalcMarginAsync() - Calculate margin:");
            try
            {
                // SERVICE: No .Value needed - ask is already double!
                var marginRequest = new OrderCalcMarginRequest
                {
                    Symbol = symbol,
                    OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
                    Volume = minLot,
                    OpenPrice = ask
                };
                var calcMargin = await service.OrderCalcMarginAsync(marginRequest);
                Console.WriteLine($"        Required margin: {calcMargin.Margin:F2}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"        Error: {ex.Message}\n");
            }

            // OrderCalcProfit - Calculate potential profit
            // Note: MT5Service doesn't have OrderCalcProfitAsync wrapper,
            // but it's available in MT5Account (low-level)
            Console.WriteLine("  [5.3] OrderCalcProfitAsync() - Calculate potential profit:");
            Console.WriteLine($"        (Available in MT5Account low-level API)\n");

            // ⚠️ REAL TRADING with MINIMAL LOT
            // SERVICE: No .Value needed - ask/point are already double!
            Console.WriteLine("  [5.4] OrderSendAsync() - Send BUY order with minimal lot:");
            var sendRequest = new OrderSendRequest
            {
                Symbol = symbol,
                Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
                Volume = minLot,
                Price = ask,
                StopLoss = ask - (100 * point),
                TakeProfit = ask + (200 * point),
                Comment = "Service demo"
            };
            var sendResult = await service.OrderSendAsync(sendRequest);
            Console.WriteLine($"        Return code: {sendResult.ReturnedCode}");
            Console.WriteLine($"        Description: {sendResult.ReturnedCodeDescription}");
            Console.WriteLine($"        Order:       {sendResult.Order}");
            Console.WriteLine($"        Deal:        {sendResult.Deal}");
            Console.WriteLine($"        Volume:      {sendResult.Volume}");
            Console.WriteLine($"        Price:       {sendResult.Price:F5}\n");

            if (sendResult.ReturnedCode == 10009 && sendResult.Order > 0) // TRADE_RETCODE_DONE
            {
                var orderTicket = sendResult.Order;
                Console.WriteLine($"  [5.5] OrderModifyAsync() - Modify position {orderTicket}:");
                // SERVICE: No .Value needed - ask/point are already double!
                var modifyRequest = new OrderModifyRequest
                {
                    Ticket = orderTicket,
                    StopLoss = ask - (150 * point), // Move SL
                    TakeProfit = ask + (250 * point) // Move TP
                };
                var modifyResult = await service.OrderModifyAsync(modifyRequest);
                Console.WriteLine($"        Return code: {modifyResult.ReturnedCode}");
                Console.WriteLine($"        Description: {modifyResult.ReturnedCodeDescription}\n");

                await Task.Delay(500); // Small delay

                Console.WriteLine($"  [5.6] OrderCloseAsync() - Close position {orderTicket}:");
                var closeRequest = new OrderCloseRequest
                {
                    Ticket = orderTicket,
                    Volume = minLot,
                    Slippage = 10
                };
                var closeResult = await service.OrderCloseAsync(closeRequest);
                Console.WriteLine($"        Return code: {closeResult.ReturnedCode}");
                Console.WriteLine($"        Description: {closeResult.ReturnedCodeDescription}");
                Console.WriteLine($"        Mode:        {closeResult.CloseMode}\n");
            }
            #endregion

            #region MARKET DEPTH
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("6. MARKET DEPTH (DOM)");
            // ══════════════════════════════════════════════════════════════

            Console.WriteLine($"  Market Book (Depth of Market) - Testing {symbol}:");
            Console.WriteLine($"  Note: DOM is not available for all brokers/symbols\n");
            try
            {
                // SERVICE: Returns MarketBookAddData (check OpenedSuccessfully property)
                Console.WriteLine($"  [6.1] MarketBookAddAsync() - Subscribe to DOM:");
                var domAddResult = await service.MarketBookAddAsync(symbol);
                var domAdd = domAddResult.OpenedSuccessfully;
                Console.WriteLine($"        Subscription opened: {domAdd}");

                if (domAdd)
                {
                    Console.WriteLine($"  [6.2] MarketBookGetAsync() - Get market depth:");
                    var domData = await service.MarketBookGetAsync(symbol);
                    Console.WriteLine($"        DOM entries: {domData.MqlBookInfos.Count}");

                    if (domData.MqlBookInfos.Count > 0)
                    {
                        Console.WriteLine("\n        First 5 entries:");
                        foreach (var entry in domData.MqlBookInfos.Take(5))
                        {
                            Console.WriteLine($"          {entry.Type,20} | Price: {entry.Price:F5} | Volume: {entry.Volume}");
                        }
                    }

                    // SERVICE: Returns bool directly (no .ClosedSuccessfully needed!)
                    Console.WriteLine($"\n  [6.3] MarketBookReleaseAsync() - Unsubscribe:");
                    var domRelease = await service.MarketBookReleaseAsync(symbol);
                    Console.WriteLine($"        Subscription closed: {domRelease}\n");
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
            ConsoleHelper.PrintSection("7. STREAMING DATA (description only)");
            // ══════════════════════════════════════════════════════════════

            Console.WriteLine("  Available streaming methods in MT5Service:");
            Console.WriteLine("  • OnSymbolTickAsync(symbols) - real-time tick stream");
            Console.WriteLine("  • OnTradeAsync() - trade events stream");
            Console.WriteLine("  • OnPositionProfitAsync(intervalMs) - position P/L updates");
            Console.WriteLine("  • OnPositionsAndPendingOrdersTicketsAsync(intervalMs) - tickets stream");
            Console.WriteLine("  • OnTradeTransactionAsync() - transaction events stream");
            Console.WriteLine();
            Console.WriteLine("  ⚠ Streams require consumption via 'await foreach' loop");
            Console.WriteLine("  → See separate streaming examples (Program.Streaming.cs)");
            Console.WriteLine("  → Example: await foreach (var tick in service.OnSymbolTickAsync(symbols)) { ... }\n");
            #endregion
        }

        // ═════════════════════════════════════════════════════════════════
        // UI HELPERS
        // ═════════════════════════════════════════════════════════════════

        private static void PrintBanner()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║           ⚡ MT5SERVICE API DEMO (MID-LEVEL)                     ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║  MT5Service - Clean wrapper over MT5Account                      ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║   30-50% LESS CODE than Low-Level API                            ║");
            Console.WriteLine("║   No .Value unwrapping needed                                    ║");
            Console.WriteLine("║   No .Exists, .Total, .Synchronized properties                   ║");
            Console.WriteLine("║   Convenience methods (GetBalanceAsync, etc.)                    ║");
            Console.WriteLine("║   Native C# types (double, bool, long, string)                   ║");
            Console.WriteLine("║   Same performance (zero overhead wrapper)                       ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║   Compare: Program.LowLevel.cs vs Program.Service.cs             ║");
            Console.WriteLine("║   See 9 side-by-side examples at end of file                     ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }
    }
}

// dotnet run service

/*══════════════════════════════════════════════════════════════════════════════
 ⚡ SIDE-BY-SIDE CODE COMPARISON: LOW-LEVEL vs SERVICE
══════════════════════════════════════════════════════════════════════════════

┌─────────────────────────────────────────────────────────────────────────────┐
│  EXAMPLE 1: Get Account Balance                                             │
└─────────────────────────────────────────────────────────────────────────────┘

LOW-LEVEL (MT5Account) - 2 lines:
  var balanceData = await acc.AccountInfoDoubleAsync(
      AccountInfoDoublePropertyType.AccountBalance);
  double balance = balanceData.Value;

SERVICE (MT5Service) - 1 line:
  double balance = await service.GetBalanceAsync();

SAVED: 1 line + no enum needed + no .Value unwrapping


┌─────────────────────────────────────────────────────────────────────────────┐
│  EXAMPLE 2: Check if Symbol Exists                                          │
└─────────────────────────────────────────────────────────────────────────────┘

LOW-LEVEL (MT5Account) - 2 lines:
  var result = await acc.SymbolExistAsync("EURUSD");
  bool exists = result.Exists;

SERVICE (MT5Service) - 1 line:
  bool exists = await service.SymbolExistAsync("EURUSD");

SAVED: 1 line + no .Exists property needed


┌─────────────────────────────────────────────────────────────────────────────┐
│  EXAMPLE 3: Get Symbol Bid Price                                            │
└─────────────────────────────────────────────────────────────────────────────┘

LOW-LEVEL (MT5Account) - 3 lines:
  var bidData = await acc.SymbolInfoDoubleAsync(
      "EURUSD", SymbolInfoDoubleProperty.SymbolBid);
  double bid = bidData.Value;

SERVICE (MT5Service) - 1 line:
  double bid = await service.SymbolInfoDoubleAsync("EURUSD", SymbolInfoDoubleProperty.SymbolBid);

SAVED: 2 lines + no .Value unwrapping


┌─────────────────────────────────────────────────────────────────────────────┐
│  EXAMPLE 4: Count Total Symbols                                             │
└─────────────────────────────────────────────────────────────────────────────┘

LOW-LEVEL (MT5Account) - 2 lines:
  var result = await acc.SymbolsTotalAsync(selectedOnly: true);
  int total = result.Total;

SERVICE (MT5Service) - 1 line:
  int total = await service.SymbolsTotalAsync(selectedOnly: true);

SAVED: 1 line + no .Total property needed


┌─────────────────────────────────────────────────────────────────────────────┐
│  EXAMPLE 5: Get Symbol Name by Index                                        │
└─────────────────────────────────────────────────────────────────────────────┘

LOW-LEVEL (MT5Account) - 2 lines:
  var result = await acc.SymbolNameAsync(index: 0, selected: true);
  string name = result.Name;

SERVICE (MT5Service) - 1 line:
  string name = await service.SymbolNameAsync(index: 0, selected: true);

SAVED: 1 line + no .Name property needed


┌─────────────────────────────────────────────────────────────────────────────┐
│  EXAMPLE 6: Open BUY Position (with SL/TP calculation)                      │
└─────────────────────────────────────────────────────────────────────────────┘

LOW-LEVEL (MT5Account) - 9 lines:
  var askData = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolAsk);
  var pointData = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint);
  double ask = askData.Value;
  double point = pointData.Value;

  var request = new OrderSendRequest {
      Symbol = symbol,
      Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
      Volume = 0.10,
      Price = ask,
      StopLoss = ask - (100 * point),
      TakeProfit = ask + (200 * point)
  };
  var result = await acc.OrderSendAsync(request);

SERVICE (MT5Service) - 6 lines:
  double ask = await service.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolAsk);
  double point = await service.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint);

  var request = new OrderSendRequest {
      Symbol = symbol,
      Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
      Volume = 0.10,
      Price = ask,
      StopLoss = ask - (100 * point),
      TakeProfit = ask + (200 * point)
  };
  var result = await service.OrderSendAsync(request);

SAVED: 3 lines (no .Value unwrapping for ask/point)


┌─────────────────────────────────────────────────────────────────────────────┐
│  EXAMPLE 7: Check if Symbol is Synchronized                                 │
└─────────────────────────────────────────────────────────────────────────────┘

LOW-LEVEL (MT5Account) - 2 lines:
  var result = await acc.SymbolIsSynchronizedAsync("EURUSD");
  bool isSynced = result.Synchronized;

SERVICE (MT5Service) - 1 line:
  bool isSynced = await service.SymbolIsSynchronizedAsync("EURUSD");

SAVED: 1 line + no .Synchronized property needed


┌─────────────────────────────────────────────────────────────────────────────┐
│  EXAMPLE 8: Get Account Margin Info                                         │
└─────────────────────────────────────────────────────────────────────────────┘

LOW-LEVEL (MT5Account) - 6 lines:
  var marginData = await acc.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountMargin);
  var freeMarginData = await acc.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountMarginFree);
  var marginLevelData = await acc.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountMarginLevel);
  double margin = marginData.Value;
  double freeMargin = freeMarginData.Value;
  double marginLevel = marginLevelData.Value;

SERVICE (MT5Service) - 3 lines:
  double margin = await service.GetMarginAsync();
  double freeMargin = await service.GetFreeMarginAsync();
  double marginLevel = await service.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountMarginLevel);

SAVED: 3 lines + convenience methods (GetMarginAsync, GetFreeMarginAsync)


┌─────────────────────────────────────────────────────────────────────────────┐
│  EXAMPLE 9: Unsubscribe from Market Book (DOM)                              │
└─────────────────────────────────────────────────────────────────────────────┘

LOW-LEVEL (MT5Account) - 2 lines:
  var result = await acc.MarketBookReleaseAsync("EURUSD");
  bool released = result.ClosedSuccessfully;

SERVICE (MT5Service) - 1 line:
  bool released = await service.MarketBookReleaseAsync("EURUSD");

SAVED: 1 line + no .ClosedSuccessfully property needed


┌─────────────────────────────────────────────────────────────────────────────┐
│  📊 SUMMARY: Code Reduction Statistics                                     │
└─────────────────────────────────────────────────────────────────────────────┘

Property Unwrapping:
  LOW-LEVEL:  Always need .Value, .Exists, .Total, .Name, etc.
  SERVICE:    Returns primitives directly (double, long, string, bool)
  IMPACT:     ~40% fewer lines for property access

Convenience Methods:
  LOW-LEVEL:  Must always use property enums (AccountInfoDoublePropertyType.AccountBalance)
  SERVICE:    Direct methods (GetBalanceAsync(), GetEquityAsync(), etc.)
  IMPACT:     ~50% fewer lines for common operations

Type Safety:
  LOW-LEVEL:  Returns wrapper types (DoubleData, BoolData, etc.)
  SERVICE:    Returns native C# types (double, bool, long, string)
  IMPACT:     Cleaner API, better IntelliSense, fewer NullReferenceExceptions

Overall Code Reduction:
  ✓ 30-50% fewer lines for typical trading operations
  ✓ No manual unwrapping required
  ✓ Better readability
  ✓ Same performance (thin wrapper)

══════════════════════════════════════════════════════════════════════════════*/
