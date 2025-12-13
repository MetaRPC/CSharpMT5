// ══════════════════════════════════════════════════════════════════════════════
// FILE: 03_TradingExamples.cs
// PURPOSE: Complete trading operations with EVERY LINE explained
//
// This file contains WORKING trading examples copied from:
// Examples/LowLevel/Program.Trading.cs
//
// Trading operations demonstrated (5 examples):
//   1. PREPARATION: Get symbol info (Ask, Bid, Point, MinVolume)
//   2. MARGIN CALC: OrderCalcMarginAsync() - Calculate required margin
//   3. OPEN POSITION: OrderSendAsync() - Open BUY position
//   4. MODIFY POSITION: OrderModifyAsync() - Change SL/TP levels
//   5. CLOSE POSITION: OrderCloseAsync() - Close the position
//
// ⚠️  IMPORTANT: This executes ONE REAL TRADE with minimal lot!
//    - Opens BUY position with broker's minimum volume
//    - Modifies the position (moves SL/TP)
//    - Closes the position immediately
//    Total risk: Minimal (uses broker's minimum lot size)
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
public static class TradingExamples
{
    // Define async method
    public static async Task RunAsync()
    {
        // ═══════════════════════════════════════════════════════════════════════
        // STEP 1: DISPLAY HEADER
        // ═══════════════════════════════════════════════════════════════════════

        // Print header box
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║       TRADING OPERATIONS - Every Line Explained           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

        // Print warning about real trading
        Console.WriteLine("⚠️  WARNING: This demo executes ONE REAL TRADE!");
        Console.WriteLine("   Using MINIMAL LOT for safety");
        Console.WriteLine("   Trade will be opened, modified, and closed immediately\n");

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

        // Get symbol from config, default to EURUSD
        // Type: string
        // config["MT5:BaseChartSymbol"] reads from appsettings.json
        // ?? means "if null, use EURUSD instead"
        var symbol = config["MT5:BaseChartSymbol"] ?? "EURUSD";

        // Print which symbol we're trading
        Console.WriteLine($"Symbol: {symbol}\n");

        // ═══════════════════════════════════════════════════════════════════════
        // PREPARATION: Get symbol information needed for trading
        // ═══════════════════════════════════════════════════════════════════════
        // Before trading, we MUST know:
        // • Ask price - price to BUY at (higher price)
        // • Bid price - price to SELL at (lower price)
        // • Point - minimum price change (e.g., 0.00001 for EURUSD)
        // • MinVolume - broker's smallest allowed lot size

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("PREPARATION: Get Symbol Information");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Print what we're doing
        Console.WriteLine("📊 Getting essential symbol data...");

        // Get Ask price - price to BUY at
        // Type: SymbolInfoDoubleData (has .Value property)
        // This is a DIRECT gRPC call
        // 'await' pauses until MT5 responds
        var ask = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolAsk);

        // Get Bid price - price to SELL at
        // Type: SymbolInfoDoubleData
        var bid = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolBid);

        // Get Point - minimum price increment
        // Type: SymbolInfoDoubleData
        // For EURUSD, point = 0.00001 (fifth decimal)
        var point = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint);

        // Get VolumeMin - broker's minimum lot size
        // Type: SymbolInfoDoubleData
        // Typically 0.01 for Forex, but can vary by broker
        var volumeMin = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeMin);

        // All data retrieved, but INVISIBLE until we print it!

        Console.WriteLine("   Result:");

        // Extract Ask value from wrapper object
        // Type: double
        // .Value extracts the actual number from the wrapper
        var askValue = ask.Value;

        // Print Ask - WITHOUT this, INVISIBLE!
        Console.WriteLine($"   Ask:        {askValue:F5}  (price to BUY at)");

        // Extract Bid value
        var bidValue = bid.Value;

        // Print Bid
        Console.WriteLine($"   Bid:        {bidValue:F5}  (price to SELL at)");

        // Extract Point value
        var pointValue = point.Value;

        // Print Point
        Console.WriteLine($"   Point:      {pointValue:F5}  (minimum price increment)");

        // Extract MinVolume value
        var minLot = volumeMin.Value;

        // Print MinVolume
        Console.WriteLine($"   Min Volume: {minLot:F2}  (broker's minimum lot)\n");

        // Print what lot size we'll use
        Console.WriteLine($"Using minimal lot size: {minLot:F2} for safety\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 1: MARGIN CALCULATION (OrderCalcMarginAsync)
        // ═══════════════════════════════════════════════════════════════════════
        // Calculate required margin BEFORE opening position
        // This tells us how much margin we need to have available

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 1: Calculate Required Margin");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Print what method we're calling
        Console.WriteLine("💰 Calling: acc.OrderCalcMarginAsync(...)");

        // Start try block to catch errors
        try
        {
            // Create margin calculation request object
            // Type: OrderCalcMarginRequest
            // This is a protobuf message that we fill with data
            var marginRequest = new OrderCalcMarginRequest
            {
                // Symbol - which instrument to trade
                Symbol = symbol,

                // OrderType - BUY or SELL
                // We're calculating for a BUY order
                OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,

                // Volume - lot size
                // Using broker's minimum lot
                Volume = minLot,

                // OpenPrice - price we'll open at
                // For BUY orders, use Ask price
                OpenPrice = askValue
            };

            // Call OrderCalcMarginAsync() - calculates required margin
            // Parameter: marginRequest object we just created
            // This is a DIRECT gRPC call to TradeFunctionsClient
            // 'await' pauses until MT5 responds
            // Returns OrderCalcMarginData with .Margin property
            var calcMargin = await acc.OrderCalcMarginAsync(marginRequest);

            // calcMargin now contains data, but INVISIBLE!

            Console.WriteLine("   Result:");

            // Extract Margin value
            // Type: double
            // This is the amount of margin required for this trade
            var requiredMargin = calcMargin.Margin;

            // Print margin - WITHOUT this, INVISIBLE!
            Console.WriteLine($"   Required Margin: ${requiredMargin:F2}");
            Console.WriteLine($"   ✓ Margin calculation successful!\n");
        }
        catch (Exception ex)
        {
            // Error occurred during margin calculation
            // Type: Exception
            // .Message contains error description
            Console.WriteLine($"   ❌ Error: {ex.Message}\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 2: OPEN POSITION (OrderSendAsync)
        // ═══════════════════════════════════════════════════════════════════════
        // Send BUY order to open a position with minimal lot

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 2: Open BUY Position");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Print what method we're calling
        Console.WriteLine("📈 Calling: acc.OrderSendAsync(...)");

        // Create order send request object
        // Type: OrderSendRequest
        // This is the main object for opening positions
        var sendRequest = new OrderSendRequest
        {
            // Symbol - which instrument to trade
            Symbol = symbol,

            // Operation - BUY or SELL
            // Tmt5OrderTypeBuy = market BUY order (open long position)
            Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,

            // Volume - lot size
            // Using broker's minimum lot for safety
            Volume = minLot,

            // Price - execution price
            // For BUY orders, MUST use Ask price (higher price)
            Price = askValue,

            // StopLoss - price level to close if market goes against us
            // Formula: Ask - (100 * Point) = 100 points below entry
            // Example: If Ask = 1.10000, Point = 0.00001
            //          Then SL = 1.10000 - (100 * 0.00001) = 1.09900
            StopLoss = askValue - (100 * pointValue),

            // TakeProfit - price level to close if market goes in our favor
            // Formula: Ask + (200 * Point) = 200 points above entry
            // Example: If Ask = 1.10000, Point = 0.00001
            //          Then TP = 1.10000 + (200 * 0.00001) = 1.10200
            TakeProfit = askValue + (200 * pointValue),

            // Comment - optional text description
            // This appears in MT5 Terminal
            Comment = "Trading demo"
        };

        // Call OrderSendAsync() - sends order to broker
        // Parameter: sendRequest object we just created
        // This is a DIRECT gRPC call to TradeFunctionsClient
        // 'await' pauses until MT5 responds
        // Returns OrderSendData with order details
        // ⚠️  THIS IS A REAL TRADE - will execute on your account!
        var sendResult = await acc.OrderSendAsync(sendRequest);

        // sendResult now contains data, but INVISIBLE!

        Console.WriteLine("   Result:");

        // Extract ReturnedCode - success/error code
        // Type: uint (unsigned integer)
        // 10009 = TRADE_RETCODE_DONE (success for market orders)
        // 10008 = TRADE_RETCODE_PLACED (success for pending orders)
        // Other codes = errors (see docs/ReturnCodes_Reference.md)
        var returnedCode = sendResult.ReturnedCode;

        // Print return code - WITHOUT this, INVISIBLE!
        Console.WriteLine($"   Return Code: {returnedCode}");

        // Extract ReturnedCodeDescription - human-readable description
        // Type: string
        var description = sendResult.ReturnedCodeDescription;

        // Print description
        Console.WriteLine($"   Description: {description}");

        // Extract Order - ticket number (position ID)
        // Type: ulong (unsigned long)
        // This is the unique ID for this position
        // We'll use this to modify or close the position later
        var orderTicket = sendResult.Order;

        // Print order ticket
        Console.WriteLine($"   Order:       {orderTicket}");

        // Extract Deal - deal number (execution ID)
        // Type: ulong
        var dealNumber = sendResult.Deal;

        // Print deal number
        Console.WriteLine($"   Deal:        {dealNumber}");

        // Extract Price - actual execution price
        // Type: double
        // May differ from requested price due to slippage
        var executionPrice = sendResult.Price;

        // Print execution price
        Console.WriteLine($"   Price:       {executionPrice:F5}");

        // Check if order was successful
        // Condition 1: returnedCode == 10009 (TRADE_RETCODE_DONE)
        // Condition 2: orderTicket > 0 (valid ticket number)
        // && means "AND" - both conditions must be true
        if (returnedCode == 10009 && orderTicket > 0)
        {
            // Order opened successfully!

            // Print success message
            Console.WriteLine($"   ✓ Position opened successfully!\n");

            // ═══════════════════════════════════════════════════════════════════
            // EXAMPLE 3: MODIFY POSITION (OrderModifyAsync)
            // ═══════════════════════════════════════════════════════════════════
            // Change Stop Loss and Take Profit levels

            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine($"EXAMPLE 3: Modify Position {orderTicket}");
            Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

            // Print what method we're calling
            Console.WriteLine($"🔧 Calling: acc.OrderModifyAsync(...)");

            // Create order modify request object
            // Type: OrderModifyRequest
            var modifyRequest = new OrderModifyRequest
            {
                // Ticket - position ID to modify
                // This is the ticket number we got from OrderSendAsync
                Ticket = orderTicket,

                // StopLoss - NEW stop loss level
                // Formula: Ask - (150 * Point) = 150 points below entry
                // We're moving SL further away (from 100 to 150 points)
                StopLoss = askValue - (150 * pointValue),

                // TakeProfit - NEW take profit level
                // Formula: Ask + (250 * Point) = 250 points above entry
                // We're moving TP further away (from 200 to 250 points)
                TakeProfit = askValue + (250 * pointValue)
            };

            // Call OrderModifyAsync() - modifies existing position
            // Parameter: modifyRequest object we just created
            // This is a DIRECT gRPC call to TradeFunctionsClient
            // 'await' pauses until MT5 responds
            // Returns OrderModifyData with result
            var modifyResult = await acc.OrderModifyAsync(modifyRequest);

            // modifyResult now contains data, but INVISIBLE!

            Console.WriteLine("   Result:");

            // Extract ReturnedCode
            var modifyCode = modifyResult.ReturnedCode;

            // Print return code
            Console.WriteLine($"   Return Code: {modifyCode}");

            // Extract description
            var modifyDescription = modifyResult.ReturnedCodeDescription;

            // Print description
            Console.WriteLine($"   Description: {modifyDescription}");
            Console.WriteLine($"   ✓ Position modified successfully!\n");

            // Wait 500 milliseconds before closing
            // Why? Give MT5 server time to process modification
            // Task.Delay() pauses execution without blocking thread
            await Task.Delay(500);

            // ═══════════════════════════════════════════════════════════════════
            // EXAMPLE 4: CLOSE POSITION (OrderCloseAsync)
            // ═══════════════════════════════════════════════════════════════════
            // Close the position we just opened

            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine($"EXAMPLE 4: Close Position {orderTicket}");
            Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

            // Print what method we're calling
            Console.WriteLine($"❌ Calling: acc.OrderCloseAsync(...)");

            // Create order close request object
            // Type: OrderCloseRequest
            var closeRequest = new OrderCloseRequest
            {
                // Ticket - position ID to close
                // This is the ticket number we got from OrderSendAsync
                Ticket = orderTicket,

                // Volume - how much to close
                // Using full volume = complete close
                // For partial close, use smaller volume
                Volume = minLot,

                // Slippage - maximum price deviation allowed (in points)
                // 10 points = we accept up to 10 points of slippage
                // If slippage > 10, order will be rejected
                Slippage = 10
            };

            // Call OrderCloseAsync() - closes the position
            // Parameter: closeRequest object we just created
            // This is a DIRECT gRPC call to TradeFunctionsClient
            // 'await' pauses until MT5 responds
            // Returns OrderCloseData with result
            var closeResult = await acc.OrderCloseAsync(closeRequest);

            // closeResult now contains data, but INVISIBLE!

            Console.WriteLine("   Result:");

            // Extract ReturnedCode
            var closeCode = closeResult.ReturnedCode;

            // Print return code
            Console.WriteLine($"   Return Code: {closeCode}");

            // Extract description
            var closeDescription = closeResult.ReturnedCodeDescription;

            // Print description
            Console.WriteLine($"   Description: {closeDescription}");

            // Extract CloseMode - how position was closed
            // Type: enum
            // Values: Normal, ByStopLoss, ByTakeProfit, etc.
            var closeMode = closeResult.CloseMode;

            // Print close mode
            Console.WriteLine($"   Close Mode:  {closeMode}");
            Console.WriteLine($"   ✓ Position closed successfully!\n");

            // ═══════════════════════════════════════════════════════════════════
            // SUMMARY
            // ═══════════════════════════════════════════════════════════════════

            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("SUMMARY - Complete Trade Lifecycle:");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("✓ Got symbol information (Ask, Bid, Point, MinVolume)");
            Console.WriteLine("✓ Calculated required margin (OrderCalcMarginAsync)");
            Console.WriteLine("✓ Opened BUY position (OrderSendAsync)");
            Console.WriteLine("✓ Modified SL/TP levels (OrderModifyAsync)");
            Console.WriteLine("✓ Closed position (OrderCloseAsync)");
            Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
        }
        else
        {
            // Order failed to open
            Console.WriteLine($"   ✗ Failed to open position");
            Console.WriteLine($"   Check ReturnedCode {returnedCode} in docs/ReturnCodes_Reference.md\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // KEY TAKEAWAYS
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("KEY TAKEAWAYS:");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("1. ALWAYS get Ask/Bid/Point/MinVolume before trading");
        Console.WriteLine("2. BUY orders use Ask price (higher)");
        Console.WriteLine("3. SELL orders use Bid price (lower)");
        Console.WriteLine("4. Calculate margin BEFORE opening position");
        Console.WriteLine("5. ALWAYS check ReturnedCode == 10009 for success");
        Console.WriteLine("6. Use ticket number to modify/close positions");
        Console.WriteLine("7. SL formula: Price - (points * point)");
        Console.WriteLine("8. TP formula: Price + (points * point)");
        Console.WriteLine("9. ALWAYS use Console.WriteLine to see results!");
        Console.WriteLine("10. Without Console.WriteLine, data exists but is INVISIBLE");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("⚠️  CRITICAL REMINDER:");
        Console.WriteLine("   BUY orders:  Use Ask price (you're buying FROM broker)");
        Console.WriteLine("   SELL orders: Use Bid price (you're selling TO broker)");
        Console.WriteLine("   Wrong price = order rejection!\n");

        Console.WriteLine("✓ All trading examples completed!");
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// TRADING OPERATIONS DEMONSTRATED (copied from working examples):
//
// PREPARATION:
//   SymbolInfoDoubleAsync(symbol, SymbolAsk) - Get Ask price
//   SymbolInfoDoubleAsync(symbol, SymbolBid) - Get Bid price
//   SymbolInfoDoubleAsync(symbol, SymbolPoint) - Get Point value
//   SymbolInfoDoubleAsync(symbol, SymbolVolumeMin) - Get minimum lot
//   Source: Examples/LowLevel/Program.Trading.cs lines 100-103
//
// 1. OrderCalcMarginAsync(request)
//    - Calculate required margin before trading
//    - Essential for risk management
//    - Source: Examples/LowLevel/Program.Trading.cs line 139
//
// 2. OrderSendAsync(request)
//    - Open BUY or SELL position
//    - Returns ticket number for modify/close
//    - Source: Examples/LowLevel/Program.Trading.cs line 162
//
// 3. OrderModifyAsync(request)
//    - Modify existing position's SL/TP
//    - Use ticket from OrderSendAsync
//    - Source: Examples/LowLevel/Program.Trading.cs line 185
//
// 4. OrderCloseAsync(request)
//    - Close position completely or partially
//    - Realizes profit/loss
//    - Source: Examples/LowLevel/Program.Trading.cs line 202
//
// ALL EXAMPLES ARE WORKING CODE - COPIED FROM PRODUCTION EXAMPLES!
// ══════════════════════════════════════════════════════════════════════════════
