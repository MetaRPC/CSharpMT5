// ══════════════════════════════════════════════════════════════════════════════
// FILE: 01_DirectGrpcCalls.cs
// PURPOSE: Direct Low-Level gRPC API calls with EVERY LINE explained
//
// This file contains WORKING examples copied from Examples/LowLevel/Program.LowLevel.cs
// Each example has detailed comments explaining every single line.
//
// Methods demonstrated (5 working examples):
//   1. ACCOUNT: AccountSummaryAsync() - Get all account data
//   2. SYMBOL: SymbolInfoIntegerAsync() - Get symbol digits
//   3. POSITIONS: OpenedOrdersAsync() - Get open positions & orders
//   4. TICK DATA: SymbolInfoTickAsync() - Get current price tick
//   5. HISTORY: PositionsHistoryAsync() - Get closed positions history
//
// IMPORTANT: Without Console.WriteLine, results will NOT appear in terminal!
// ══════════════════════════════════════════════════════════════════════════════

// Import System namespace - provides Console, DateTime, Task
using System;

// Import Task types for async/await
using System.Threading.Tasks;

// Import connection helper
using MetaRPC.CSharpMT5.Examples.Helpers;

// Import MT5 gRPC API types
using mt5_term_api;

// Import MT5Account class
using MetaRPC.CSharpMT5;

// Declare namespace
namespace MetaRPC.CSharpMT5.Examples.MoreExamples;

// Declare public static class
public static class DirectGrpcCalls
{
    // Define async method
    public static async Task RunAsync()
    {
        // ═══════════════════════════════════════════════════════════════════════
        // STEP 1: DISPLAY HEADER
        // ═══════════════════════════════════════════════════════════════════════

        // Print header box
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║       DIRECT GRPC CALLS - Every Line Explained             ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

        // ═══════════════════════════════════════════════════════════════════════
        // STEP 2: ESTABLISH CONNECTION
        // ═══════════════════════════════════════════════════════════════════════

        // Call BuildConfiguration() - reads appsettings.json
        var config = ConnectionHelper.BuildConfiguration();
        Console.WriteLine("✓ Configuration loaded");

        // Call CreateAndConnectAccountAsync() - connects to MT5
        // Returns MT5Account object - our main gRPC client
        var acc = await ConnectionHelper.CreateAndConnectAccountAsync(config);
        Console.WriteLine("✓ Connected to MT5 Terminal\n");

        // Define symbol to use in examples
        string symbol = "EURUSD";

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 1: ACCOUNT INFORMATION (AccountSummaryAsync)
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 1: Get Account Summary");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Print what method we're calling
        Console.WriteLine("📊 Calling: acc.AccountSummaryAsync()");

        // Call AccountSummaryAsync() - gets ALL account data in one call
        // This is a DIRECT gRPC call to AccountInformationClient
        // 'await' pauses here until MT5 responds
        // Returns AccountSummaryData object with all account properties
        var summary = await acc.AccountSummaryAsync();

        // summary now contains data, but it's INVISIBLE until we print it!

        Console.WriteLine("   Result:");

        // Extract Login (account number) from summary
        // Type: ulong (unsigned 64-bit integer)
        var login = summary.AccountLogin;

        // Print login - WITHOUT this line, it's INVISIBLE in console!
        Console.WriteLine($"   Login:    {login}");

        // Extract Balance - deposited funds (without current P&L)
        // Type: double
        var balance = summary.AccountBalance;

        // Print balance with 2 decimal places
        Console.WriteLine($"   Balance:  {balance:F2} {summary.AccountCurrency}");

        // Extract Equity - current account value (balance + floating profit)
        // Type: double
        var equity = summary.AccountEquity;

        // Print equity
        Console.WriteLine($"   Equity:   {equity:F2}");

        // Extract Credit - additional funds from broker
        // Type: double
        var credit = summary.AccountCredit;

        // Print credit
        Console.WriteLine($"   Credit:   {credit:F2}");

        // Extract Leverage - borrowing multiplier (e.g., 100 = can trade 100x balance)
        // Type: ulong
        var leverage = summary.AccountLeverage;

        // Print leverage in "1:XXX" format
        Console.WriteLine($"   Leverage: 1:{leverage}");

        // Extract Company - broker company name
        // Type: string
        var company = summary.AccountCompanyName;

        // Print company
        Console.WriteLine($"   Company:  {company}");

        // Extract account holder name
        // Type: string
        var userName = summary.AccountUserName;

        // Print user name
        Console.WriteLine($"   Name:     {userName}\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 2: SYMBOL INFORMATION (SymbolInfoIntegerAsync)
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 2: Get Symbol Digits");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Print what method we're calling
        Console.WriteLine($"📏 Calling: acc.SymbolInfoIntegerAsync(\"{symbol}\", SymbolInfoIntegerProperty.SymbolDigits)");

        // Call SymbolInfoIntegerAsync() - gets ONE specific integer property
        // Parameter 1: symbol name (string) - which instrument
        // Parameter 2: property enum - which property to get
        // This is a DIRECT gRPC call to MarketInfoClient
        // 'await' pauses until MT5 responds
        // Returns SymbolInfoIntegerData object containing the value
        var digits = await acc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolDigits);

        // digits object now contains data, but INVISIBLE until printed!

        Console.WriteLine("   Result:");

        // Extract the actual integer value from the result object
        // SymbolInfoIntegerData has a 'Value' property with the number
        // Type: long
        var digitsValue = digits.Value;

        // Print the digits value - WITHOUT this, it's INVISIBLE!
        Console.WriteLine($"   Digits: {digitsValue}");

        // Print explanation of what digits means
        Console.WriteLine($"   Meaning: {symbol} prices have {digitsValue} decimal places");
        Console.WriteLine($"   Example: 1.12345 ({digitsValue} digits after decimal)\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 3: OPEN POSITIONS (OpenedOrdersAsync)
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 3: Get Open Positions & Orders");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Print what method we're calling
        Console.WriteLine("📋 Calling: acc.OpenedOrdersAsync(BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeDesc)");

        // Call OpenedOrdersAsync() - gets all open positions and pending orders
        // Parameter: sort type enum - how to sort results (by open time, descending)
        // This is a DIRECT gRPC call to TradeFunctionsClient
        // 'await' pauses until MT5 responds
        // Returns OpenedOrdersData object with positions and orders
        var openedOrders = await acc.OpenedOrdersAsync(BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeDesc);

        // openedOrders now contains data, but INVISIBLE!

        Console.WriteLine("   Result:");

        // Extract opened orders collection
        // OpenedOrders = collection of pending orders (not yet filled)
        // Type: RepeatedField<OpenedOrder> (like List)
        var ordersCollection = openedOrders.OpenedOrders;

        // Get count of pending orders
        // .Count property returns number of elements
        var ordersCount = ordersCollection.Count;

        // Print count - WITHOUT this, it's INVISIBLE!
        Console.WriteLine($"   Opened orders:   {ordersCount}");

        // Extract position infos collection
        // PositionInfos = collection of open positions (already filled orders)
        // Type: RepeatedField<PositionInfo>
        var positionsCollection = openedOrders.PositionInfos;

        // Get count of open positions
        var positionsCount = positionsCollection.Count;

        // Print count
        Console.WriteLine($"   Open positions:  {positionsCount}");

        // Check if there are any open positions
        if (positionsCount > 0)
        {
            // There are positions - show details of first one

            // Get first position from collection
            // [0] = first element (index starts at 0)
            var pos = positionsCollection[0];

            // Print section header
            Console.WriteLine("\n   First Position:");

            // Extract Ticket - unique position ID
            // Type: ulong
            var ticket = pos.Ticket;

            // Print ticket
            Console.WriteLine($"     Ticket:  {ticket}");

            // Extract Symbol - which instrument
            // Type: string
            var posSymbol = pos.Symbol;

            // Print symbol
            Console.WriteLine($"     Symbol:  {posSymbol}");

            // Extract Type - BUY or SELL
            // Type: ENUM_POSITION_TYPE enum
            var posType = pos.Type;

            // Print type
            Console.WriteLine($"     Type:    {posType}");

            // Extract Volume - lot size
            // Type: double
            var volume = pos.Volume;

            // Print volume with 2 decimal places
            Console.WriteLine($"     Volume:  {volume:F2}");

            // Extract Profit - current unrealized P&L
            // Type: double
            var profit = pos.Profit;

            // Print profit
            Console.WriteLine($"     Profit:  {profit:F2}");
        }
        else
        {
            // No positions open
            Console.WriteLine("   (No open positions)");
        }

        // Print blank line
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 4: TICK DATA (SymbolInfoTickAsync)
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 4: Get Current Tick (Price)");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Print what method we're calling
        Console.WriteLine($"💱 Calling: acc.SymbolInfoTickAsync(\"{symbol}\")");

        // Call SymbolInfoTickAsync() - gets current price tick
        // Parameter: symbol name (string)
        // This is a DIRECT gRPC call to MarketInfoClient
        // 'await' pauses until MT5 responds
        // Returns MqlTick object with Bid, Ask, Last, Volume, Time
        var tick = await acc.SymbolInfoTickAsync(symbol);

        // tick now contains price data, but INVISIBLE!

        Console.WriteLine("   Result:");

        // Extract Time - when this tick occurred
        // Type: long (Unix timestamp in seconds since 1970-01-01)
        var tickTime = tick.Time;

        // Convert Unix timestamp to DateTime for human readability
        // DateTimeOffset.FromUnixTimeSeconds() converts Unix time
        // .DateTime gets the DateTime portion
        var tickDateTime = DateTimeOffset.FromUnixTimeSeconds(tickTime).DateTime;

        // Print time - WITHOUT this, it's INVISIBLE!
        Console.WriteLine($"   Time:   {tickDateTime}");

        // Extract Bid - price at which you can SELL
        // Type: double
        var bid = tick.Bid;

        // Print bid with 5 decimal places
        Console.WriteLine($"   Bid:    {bid:F5}");

        // Extract Ask - price at which you can BUY
        // Type: double
        var ask = tick.Ask;

        // Print ask
        Console.WriteLine($"   Ask:    {ask:F5}");

        // Extract Last - price of last trade
        // Type: double
        var last = tick.Last;

        // Print last price
        Console.WriteLine($"   Last:   {last:F5}");

        // Extract Volume - number of price changes
        // Type: ulong
        var tickVolume = tick.Volume;

        // Print volume
        Console.WriteLine($"   Volume: {tickVolume}\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 5: POSITIONS HISTORY (PositionsHistoryAsync)
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 5: Get Positions History");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Print what method we're calling
        Console.WriteLine("📜 Calling: acc.PositionsHistoryAsync(...)");

        // Define time range for history query
        // Get positions from last 7 days

        // Calculate start time - 7 days ago
        // DateTime.UtcNow = current UTC time
        // AddDays(-7) = subtract 7 days
        var fromTime = DateTime.UtcNow.AddDays(-7);

        // Set end time to now
        var toTime = DateTime.UtcNow;

        // Call PositionsHistoryAsync() - gets closed positions history
        // Parameter 1: sort type - how to sort results
        // Parameter 2: openFrom - start date (nullable DateTime)
        // Parameter 3: openTo - end date (nullable DateTime)
        // Parameter 4: page - page number for pagination (0 = first page)
        // Parameter 5: size - items per page (10 = return max 10 positions)
        // This is a DIRECT gRPC call to TradeFunctionsClient
        // 'await' pauses until MT5 responds
        // Returns PositionsHistoryData with list of closed positions
        var posHistory = await acc.PositionsHistoryAsync(
            AH_ENUM_POSITIONS_HISTORY_SORT_TYPE.AhPositionOpenTimeDesc,
            openFrom: fromTime,
            openTo: toTime,
            page: 0,
            size: 10
        );

        // posHistory now contains data, but INVISIBLE!

        Console.WriteLine("   Result:");

        // Extract the list of history positions
        // HistoryPositions = collection of closed positions
        // Type: RepeatedField<PositionHistory>
        var historyPositions = posHistory.HistoryPositions;

        // Get count of positions returned
        var historyCount = historyPositions.Count;

        // Print count - WITHOUT this, INVISIBLE!
        Console.WriteLine($"   Positions returned: {historyCount}");

        // Check if there are any history positions
        if (historyCount > 0)
        {
            // There are positions - show summary only
            // PositionHistoryInfo structure is complex, so we just show count
            Console.WriteLine($"   (Found {historyCount} closed positions)");
            Console.WriteLine("   Note: Full position details available in original structure");
        }
        else
        {
            // No history positions
            Console.WriteLine("   (No closed positions in last 7 days)");
        }

        // Print blank line
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════════════════
        // SUMMARY
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("SUMMARY - What We Did:");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("✓ Retrieved account summary (AccountSummaryAsync)");
        Console.WriteLine("✓ Retrieved symbol digits (SymbolInfoIntegerAsync)");
        Console.WriteLine("✓ Retrieved open positions (OpenedOrdersAsync)");
        Console.WriteLine("✓ Retrieved current tick/price (SymbolInfoTickAsync)");
        Console.WriteLine("✓ Retrieved positions history (PositionsHistoryAsync)");
        Console.WriteLine();

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("KEY TAKEAWAYS:");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("1. ALWAYS use 'await' when calling async methods");
        Console.WriteLine("2. ALWAYS use Console.WriteLine to see results!");
        Console.WriteLine("3. Without Console.WriteLine, data exists but is INVISIBLE");
        Console.WriteLine("4. All results have a '.Value' property with the actual data");
        Console.WriteLine("5. Collections use .Count property and [index] to access elements");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("⚠️  CRITICAL REMINDER:");
        Console.WriteLine("   var data = await acc.SomeMethod();  // ❌ Data invisible!");
        Console.WriteLine("   Console.WriteLine(data.Value);      // ✅ Now visible!");
        Console.WriteLine();

        Console.WriteLine("✓ Example completed successfully!");
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// METHODS DEMONSTRATED (copied from working examples):
//
// 1. AccountSummaryAsync()
//    - Gets all account data in one call
//    - Source: Examples/LowLevel/Program.LowLevel.cs line 138
//
// 2. SymbolInfoIntegerAsync(symbol, property)
//    - Gets specific integer property for a symbol
//    - Source: Examples/LowLevel/Program.LowLevel.cs line 233
//
// 3. OpenedOrdersAsync(sortType)
//    - Gets all open positions and pending orders
//    - Source: Examples/LowLevel/Program.LowLevel.cs line 386
//
// 4. SymbolInfoTickAsync(symbol)
//    - Gets current tick with Bid, Ask, Last, Volume, Time
//    - Source: Examples/LowLevel/Program.LowLevel.cs line 255
//
// 5. PositionsHistoryAsync(sortType, from, to, page, size)
//    - Gets closed positions history with pagination
//    - Source: Examples/LowLevel/Program.LowLevel.cs line 430
//
// ALL EXAMPLES ARE WORKING CODE - COPIED FROM PRODUCTION EXAMPLES!
// ══════════════════════════════════════════════════════════════════════════════
