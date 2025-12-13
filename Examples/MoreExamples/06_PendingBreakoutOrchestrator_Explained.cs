/*══════════════════════════════════════════════════════════════════════════════
 📚 ORCHESTRATOR BREAKDOWN: PendingBreakoutOrchestrator

 🎯 WHAT THIS FILE DOES:
    This is a COMPLETE LINE-BY-LINE explanation of PendingBreakoutOrchestrator.
    Every single line of code is explained for absolute beginners.

 🔍 WHERE ORIGINAL CODE IS:
    Examples/Orchestrators/PendingBreakoutOrchestrator.cs

 📖 WHAT YOU'LL LEARN:
    • How to place pending orders (BUY STOP, SELL STOP)
    • How to monitor which order triggers first
    • How to cancel unfilled pending orders
    • How to use while loops for monitoring
    • How to work with DateTime and TimeSpan

 ⚠️ NOTE: This file is for LEARNING ONLY - it's not meant to run!
          Use original file in Orchestrators/ folder to actually run the strategy.

══════════════════════════════════════════════════════════════════════════════*/

using System;                      // Import Console, DateTime classes
using System.Threading;            // Import CancellationToken
using System.Threading.Tasks;      // Import Task and async/await
using MetaRPC.CSharpMT5;           // Import MT5Service with all API methods
using mt5_term_api;                // Import protobuf types (OrderSendData, etc.)

namespace MetaRPC.CSharpMT5.Examples.MoreExamples
{
    // ═══════════════════════════════════════════════════════════════════════
    // CLASS DEFINITION: PendingBreakoutOrchestrator
    // ═══════════════════════════════════════════════════════════════════════

    public class PendingBreakoutOrchestrator
    {
        // ═══════════════════════════════════════════════════════════════════════
        // FIELD: _service (WHERE MT5 API METHODS COME FROM)
        // ═══════════════════════════════════════════════════════════════════════

        // 'private readonly' = can only be set in constructor
        // Stores MT5Service object for calling API methods
        private readonly MT5Service _service;


        // ═══════════════════════════════════════════════════════════════════════
        // PROPERTIES: CONFIGURABLE PARAMETERS
        // ═══════════════════════════════════════════════════════════════════════

        // Symbol to trade (default: EURUSD)
        public string Symbol { get; set; } = "EURUSD";

        // Distance from current price to place pending orders (default: 25 points)
        // BUY STOP will be +25 points above Ask
        // SELL STOP will be -25 points below Bid
        public int BreakoutDistancePoints { get; set; } = 25;

        // Stop Loss distance in points (default: 15 points)
        public int StopLossPoints { get; set; } = 15;

        // Take Profit distance in points (default: 30 points)
        public int TakeProfitPoints { get; set; } = 30;

        // Fixed lot size for both orders (default: 0.01)
        // Type: double (decimal number)
        public double Volume { get; set; } = 0.01;

        // Maximum minutes to wait for breakout (default: 30 minutes)
        // If no breakout in 30 minutes, both orders cancel
        public int MaxWaitMinutes { get; set; } = 30;


        // ═══════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR: HOW THIS CLASS IS CREATED
        // ═══════════════════════════════════════════════════════════════════════

        // Constructor receives MT5Service and stores it in _service field
        public PendingBreakoutOrchestrator(MT5Service service)
        {
            // Store service object in private field
            // Now all methods can use: _service.BuyStopPoints(), etc.
            _service = service;
        }


        // ═══════════════════════════════════════════════════════════════════════
        // MAIN METHOD: ExecuteAsync (WHERE STRATEGY RUNS)
        // ═══════════════════════════════════════════════════════════════════════

        public async Task<double> ExecuteAsync(CancellationToken ct = default)
        {
            // ═══════════════════════════════════════════════════════════════════
            // STEP 1: PRINT HEADER AND INITIAL INFO
            // ═══════════════════════════════════════════════════════════════════

            Console.WriteLine("\n+============================================================+");
            Console.WriteLine("|  PENDING BREAKOUT ORCHESTRATOR                            |");
            Console.WriteLine("+============================================================+\n");

            // Get starting balance
            var initialBalance = await _service.GetBalanceAsync();

            // Print strategy parameters
            Console.WriteLine($"  Starting balance: ${initialBalance:F2}");
            Console.WriteLine($"  Symbol: {Symbol}");
            Console.WriteLine($"  Breakout distance: {BreakoutDistancePoints} pts");
            Console.WriteLine($"  Volume: {Volume:F2} lots");
            Console.WriteLine($"  SL: {StopLossPoints} pts | TP: {TakeProfitPoints} pts\n");


            try
            {
                // ═══════════════════════════════════════════════════════════════
                // STEP 2: GET CURRENT PRICE
                // ═══════════════════════════════════════════════════════════════

                // Get current tick (Bid and Ask prices)
                // 'SymbolInfoTickAsync' = method from MT5Service (defined in MT5Service.cs)
                // Returns: SymbolTickInfo object with Bid, Ask, Last, Volume, etc.
                var tick = await _service.SymbolInfoTickAsync(Symbol);

                // tick now contains:
                // - tick.Bid = current Bid price (example: 1.09995)
                // - tick.Ask = current Ask price (example: 1.10000)
                // - tick.Last = last deal price
                // - tick.Volume = tick volume

                // Print current prices - WITHOUT this, tick data is INVISIBLE!
                // ':F5' = format with 5 decimals (example: 1.10000)
                Console.WriteLine($"  Current: Bid={tick.Bid:F5}, Ask={tick.Ask:F5}\n");


                // ═══════════════════════════════════════════════════════════════
                // STEP 3: PLACE BUY STOP ORDER (ABOVE CURRENT PRICE)
                // ═══════════════════════════════════════════════════════════════

                Console.WriteLine("  Placing BUY STOP order...");

                // Call BuyStopPoints method from MT5Service (Sugar API)
                // This method is defined in MT5Sugar.cs
                // 'await' = pause here until order is placed and MT5 responds

                // WHAT BuyStopPoints DOES INTERNALLY:
                // 1. Gets current Ask price (example: 1.10000)
                // 2. Gets Point value (example: 0.00001 for EURUSD)
                // 3. Calculates order price: Ask + (priceOffsetPoints * Point)
                //    Example: 1.10000 + (25 * 0.00001) = 1.10025
                // 4. Calculates SL price: orderPrice - (slPoints * Point)
                //    Example: 1.10025 - (15 * 0.00001) = 1.10010
                // 5. Calculates TP price: orderPrice + (tpPoints * Point)
                //    Example: 1.10025 + (30 * 0.00001) = 1.10055
                // 6. Sends OrderSendAsync with ORDER_TYPE_BUY_STOP
                // 7. Returns OrderSendData result

                var buyStopResult = await _service.BuyStopPoints(
                    symbol: Symbol,                           // "EURUSD"
                    volume: Volume,                           // 0.01 lots
                    priceOffsetPoints: BreakoutDistancePoints, // +25 points above Ask
                    slPoints: StopLossPoints,                 // 15 points SL
                    tpPoints: TakeProfitPoints,               // 30 points TP
                    comment: "Breakout-Buy"                   // Comment in MT5
                );

                // buyStopResult now contains:
                // - buyStopResult.Order = pending order ticket number
                // - buyStopResult.ReturnedCode = success code (10009 = success)
                // - buyStopResult.Comment = broker's response message

                // Check if BUY STOP placement succeeded
                if (buyStopResult.ReturnedCode != 10009)
                {
                    // Order failed! Print error and exit
                    Console.WriteLine($"  ✗ BUY STOP failed: {buyStopResult.Comment}\n");
                    return 0;  // Return 0 profit and stop execution
                }

                // Order succeeded! Print ticket number
                Console.WriteLine($"  ✓ BUY STOP placed: #{buyStopResult.Order}\n");


                // ═══════════════════════════════════════════════════════════════
                // STEP 4: PLACE SELL STOP ORDER (BELOW CURRENT PRICE)
                // ═══════════════════════════════════════════════════════════════

                Console.WriteLine("  Placing SELL STOP order...");

                // Call SellStopPoints method from MT5Service (Sugar API)
                // This method is defined in MT5Sugar.cs

                // WHAT SellStopPoints DOES INTERNALLY:
                // 1. Gets current Bid price (example: 1.09995)
                // 2. Gets Point value (example: 0.00001)
                // 3. Calculates order price: Bid + (priceOffsetPoints * Point)
                //    IMPORTANT: priceOffsetPoints is NEGATIVE (-25)
                //    Example: 1.09995 + (-25 * 0.00001) = 1.09970
                // 4. Calculates SL price: orderPrice + (slPoints * Point)
                //    Example: 1.09970 + (15 * 0.00001) = 1.09985
                // 5. Calculates TP price: orderPrice - (tpPoints * Point)
                //    Example: 1.09970 - (30 * 0.00001) = 1.09940
                // 6. Sends OrderSendAsync with ORDER_TYPE_SELL_STOP
                // 7. Returns OrderSendData result

                var sellStopResult = await _service.SellStopPoints(
                    symbol: Symbol,                            // "EURUSD"
                    volume: Volume,                            // 0.01 lots
                    priceOffsetPoints: -BreakoutDistancePoints, // -25 points below Bid
                    slPoints: StopLossPoints,                  // 15 points SL
                    tpPoints: TakeProfitPoints,                // 30 points TP
                    comment: "Breakout-Sell"                   // Comment in MT5
                );

                // Check if SELL STOP placement succeeded
                if (sellStopResult.ReturnedCode != 10009)
                {
                    // SELL STOP failed! Need to clean up BUY STOP before exiting
                    Console.WriteLine($"  ✗ SELL STOP failed: {sellStopResult.Comment}");
                    Console.WriteLine("  Canceling BUY STOP...");

                    // Cancel the BUY STOP order we placed earlier
                    // CloseByTicket works for both positions AND pending orders
                    await _service.CloseByTicket(buyStopResult.Order);

                    return 0;  // Return 0 profit and stop execution
                }

                // SELL STOP succeeded! Print ticket number
                Console.WriteLine($"  ✓ SELL STOP placed: #{sellStopResult.Order}\n");

                // Now we have TWO pending orders:
                // - BUY STOP at +25 points above current Ask
                // - SELL STOP at -25 points below current Bid


                // ═══════════════════════════════════════════════════════════════
                // STEP 5: MONITOR ORDERS UNTIL ONE TRIGGERS OR TIMEOUT
                // ═══════════════════════════════════════════════════════════════

                Console.WriteLine($"  ⏳ Waiting up to {MaxWaitMinutes} minutes for breakout...\n");

                // Get current UTC time (timezone-independent)
                // DateTime.UtcNow = current time in UTC
                var startTime = DateTime.UtcNow;

                // Create timeout duration
                // TimeSpan.FromMinutes(30) = 30-minute duration
                var timeout = TimeSpan.FromMinutes(MaxWaitMinutes);

                // Variables to track which order executed
                // 'ulong?' = nullable ulong (can be null or a number)
                // '= null' = initially null (no order executed yet)
                ulong? executedOrder = null;  // Which order triggered
                ulong? cancelOrder = null;    // Which order needs to be canceled

                // ═══════════════════════════════════════════════════════════════
                // MONITORING LOOP: CHECK EVERY 3 SECONDS
                // ═══════════════════════════════════════════════════════════════

                // 'while' loop = repeat until condition becomes false
                // Loop continues while:
                // 1. NOT timed out: DateTime.UtcNow - startTime < timeout
                // 2. NOT canceled: !ct.IsCancellationRequested
                while (DateTime.UtcNow - startTime < timeout && !ct.IsCancellationRequested)
                {
                    // Wait 3 seconds before checking again
                    // '3000' = 3000 milliseconds = 3 seconds
                    // This prevents hammering MT5 with constant requests
                    await Task.Delay(3000, ct);

                    // ═══════════════════════════════════════════════════════════
                    // CHECK PENDING ORDER STATUS
                    // ═══════════════════════════════════════════════════════════

                    // Get all currently pending order tickets from MT5
                    // Returns: object with OpenedOrdersTickets array
                    var tickets = await _service.OpenedOrdersTicketsAsync();

                    // Create boolean flags to track each order's status
                    // 'bool' = boolean type (true or false)
                    // '= false' = assume not pending (will be set to true if found)
                    bool buyStillPending = false;
                    bool sellStillPending = false;

                    // Loop through all pending order tickets
                    // 'foreach' = iterate over each ticket in collection
                    foreach (var ticket in tickets.OpenedOrdersTickets)
                    {
                        // Check if this ticket is our BUY STOP order
                        // 'ticket' = current ticket from loop (type: long)
                        // 'buyStopResult.Order' = our BUY STOP ticket (type: ulong)
                        // '(long)buyStopResult.Order' = cast ulong to long
                        if (ticket == (long)buyStopResult.Order)
                            buyStillPending = true;  // Found it! Still pending

                        // Check if this ticket is our SELL STOP order
                        if (ticket == (long)sellStopResult.Order)
                            sellStillPending = true;  // Found it! Still pending
                    }

                    // After loop:
                    // - buyStillPending = true if BUY STOP found, false if executed/canceled
                    // - sellStillPending = true if SELL STOP found, false if executed/canceled


                    // ═══════════════════════════════════════════════════════════
                    // DETERMINE WHAT HAPPENED
                    // ═══════════════════════════════════════════════════════════

                    // CASE 1: BUY STOP EXECUTED (upward breakout)
                    // How we know: BUY not pending anymore, but SELL still pending
                    if (!buyStillPending && sellStillPending)
                    {
                        // BUY STOP triggered! Price moved up!
                        Console.WriteLine("  🚀 BUY STOP EXECUTED! Upward breakout!");

                        // Store which orders to track
                        executedOrder = buyStopResult.Order;   // BUY executed
                        cancelOrder = sellStopResult.Order;    // SELL needs canceling

                        // Exit loop - no need to keep checking
                        break;
                    }
                    // CASE 2: SELL STOP EXECUTED (downward breakout)
                    // How we know: SELL not pending anymore, but BUY still pending
                    else if (buyStillPending && !sellStillPending)
                    {
                        // SELL STOP triggered! Price moved down!
                        Console.WriteLine("  🚀 SELL STOP EXECUTED! Downward breakout!");

                        // Store which orders to track
                        executedOrder = sellStopResult.Order;  // SELL executed
                        cancelOrder = buyStopResult.Order;     // BUY needs canceling

                        // Exit loop
                        break;
                    }
                    // CASE 3: BOTH EXECUTED OR CANCELED (rare)
                    // How we know: Neither order is pending anymore
                    else if (!buyStillPending && !sellStillPending)
                    {
                        // Both orders gone (both triggered, or both canceled, or one of each)
                        Console.WriteLine("  ✓ Both orders executed or canceled");

                        // Exit loop
                        break;
                    }
                    // CASE 4: BOTH STILL PENDING
                    // How we know: Both orders still in pending list
                    // Action: Continue loop, check again in 3 seconds

                } // End of while loop


                // ═══════════════════════════════════════════════════════════════
                // STEP 6: CANCEL OPPOSITE ORDER (IF ONE TRIGGERED)
                // ═══════════════════════════════════════════════════════════════

                // Check if we have an order to cancel
                // 'cancelOrder.HasValue' = checks if nullable ulong has a value (not null)
                if (cancelOrder.HasValue)
                {
                    // One order triggered, need to cancel the other
                    // 'cancelOrder.Value' = gets the actual ulong value
                    Console.WriteLine($"  Canceling opposite order #{cancelOrder.Value}...");

                    // Cancel the pending order
                    // CloseByTicket works for both positions AND pending orders
                    await _service.CloseByTicket(cancelOrder.Value);

                    Console.WriteLine("  ✓ Canceled\n");
                }
                else
                {
                    // ═══════════════════════════════════════════════════════════
                    // TIMEOUT: CANCEL BOTH ORDERS
                    // ═══════════════════════════════════════════════════════════

                    // No breakout happened in MaxWaitMinutes
                    // Need to cancel BOTH pending orders
                    Console.WriteLine($"  ⏱ Timeout after {MaxWaitMinutes} minutes - canceling both orders...");

                    // Cancel BUY STOP order
                    await _service.CloseByTicket(buyStopResult.Order);

                    // Cancel SELL STOP order
                    await _service.CloseByTicket(sellStopResult.Order);

                    Console.WriteLine("  ✓ Both canceled\n");
                }


                // ═══════════════════════════════════════════════════════════════
                // STEP 7: CALCULATE PROFIT/LOSS
                // ═══════════════════════════════════════════════════════════════

                // Get final account balance
                var finalBalance = await _service.GetBalanceAsync();

                // Calculate profit/loss
                // Formula: Final Balance - Initial Balance
                var profit = finalBalance - initialBalance;

                // Print results - WITHOUT this, results are INVISIBLE!
                Console.WriteLine($"  Final balance: ${finalBalance:F2}");
                Console.WriteLine($"  Profit/Loss: ${profit:F2}");
                Console.WriteLine("\n+============================================================+\n");

                // Return profit value to caller
                return profit;
            }
            catch (Exception ex)
            {
                // ═══════════════════════════════════════════════════════════════
                // ERROR HANDLER
                // ═══════════════════════════════════════════════════════════════

                // Print error message if anything goes wrong
                Console.WriteLine($"\n  ✗ Error: {ex.Message}");
                Console.WriteLine("+============================================================+\n");

                // Return 0 (no profit/loss)
                return 0;
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        // END OF CLASS
        // ═══════════════════════════════════════════════════════════════════════
    }
}


/*══════════════════════════════════════════════════════════════════════════════
 📊 SUMMARY: HOW THIS ORCHESTRATOR WORKS

 1. CLASS STRUCTURE:
    - Field: _service (holds MT5Service object)
    - Properties: Symbol, BreakoutDistancePoints, Volume, etc.
    - Constructor: Receives and stores MT5Service
    - Method: ExecuteAsync() runs the breakout strategy

 2. EXECUTION FLOW:
    Step 1: Print header and get initial balance
    Step 2: Get current Bid/Ask prices
    Step 3: Place BUY STOP order +25 points above Ask
    Step 4: Place SELL STOP order -25 points below Bid
    Step 5: Monitor every 3 seconds to see which triggers
    Step 6: Cancel opposite order when one executes
    Step 7: Calculate profit/loss

 3. KEY CONCEPTS:
    - PENDING ORDERS = orders that wait for price to reach specific level
    - BUY STOP = triggers when price goes UP to trigger level
    - SELL STOP = triggers when price goes DOWN to trigger level
    - while loop = REPEATEDLY check status until breakout or timeout
    - DateTime.UtcNow = CURRENT time in UTC timezone
    - TimeSpan = DURATION (example: 30 minutes)
    - nullable ulong? = can be null OR a number

 4. MONITORING LOGIC:
    - Loop every 3 seconds (avoid hammering MT5)
    - Get all pending order tickets
    - Check if OUR orders are still in the list
    - If BUY missing but SELL present → BUY triggered (upward breakout)
    - If SELL missing but BUY present → SELL triggered (downward breakout)
    - If both missing → both triggered/canceled
    - If both present → no breakout yet, keep waiting

 5. SUGAR API USED:
    - SymbolInfoTickAsync = Get current Bid/Ask (defined in MT5Service.cs)
    - BuyStopPoints = Place BUY STOP pending order (defined in MT5Sugar.cs)
    - SellStopPoints = Place SELL STOP pending order (defined in MT5Sugar.cs)
    - OpenedOrdersTicketsAsync = Get pending order tickets (defined in MT5Service.cs)
    - CloseByTicket = Cancel pending order (defined in MT5Service.cs)
    - GetBalanceAsync = Get account balance (defined in MT5Service.cs)

 6. WHY THIS STRATEGY?
    - Best for CONSOLIDATION periods before news events
    - Captures breakout in EITHER direction (no need to predict)
    - Automatically cancels opposite order (prevents double exposure)
    - Risk-managed with SL/TP on both orders

══════════════════════════════════════════════════════════════════════════════*/
