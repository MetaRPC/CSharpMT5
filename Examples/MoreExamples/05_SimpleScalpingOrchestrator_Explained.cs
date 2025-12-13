/*══════════════════════════════════════════════════════════════════════════════
 📚 ORCHESTRATOR BREAKDOWN: SimpleScalpingOrchestrator

 🎯 WHAT THIS FILE DOES:
    This is a COMPLETE LINE-BY-LINE explanation of SimpleScalpingOrchestrator.
    Every single line of code is explained for absolute beginners.

 🔍 WHERE ORIGINAL CODE IS:
    Examples/Orchestrators/SimpleScalpingOrchestrator.cs

 📖 WHAT YOU'LL LEARN:
    • How orchestrators are structured (class → properties → method)
    • How to open market positions with risk-based sizing
    • How to check if position still exists
    • How to close positions manually
    • How to calculate profit/loss

 ⚠️ NOTE: This file is for LEARNING ONLY - it's not meant to run!
          Use original file in Orchestrators/ folder to actually run the strategy.

══════════════════════════════════════════════════════════════════════════════*/

using System;                      // Import Console class for printing output
using System.Threading;            // Import CancellationToken for stopping operations
using System.Threading.Tasks;      // Import Task and async/await functionality
using MetaRPC.CSharpMT5;           // Import MT5Service class with all API methods
using mt5_term_api;                // Import protobuf types (OrderSendData, etc.)

namespace MetaRPC.CSharpMT5.Examples.MoreExamples
{
    // ═══════════════════════════════════════════════════════════════════════
    // CLASS DEFINITION: SimpleScalpingOrchestrator
    // ═══════════════════════════════════════════════════════════════════════

    // 'public' = this class can be used from anywhere
    // 'class' = defines a new type called SimpleScalpingOrchestrator
    public class SimpleScalpingOrchestrator
    {
        // ═══════════════════════════════════════════════════════════════════════
        // FIELD: _service (WHERE MT5 API METHODS COME FROM)
        // ═══════════════════════════════════════════════════════════════════════

        // 'private' = only this class can see this field
        // 'readonly' = can only be set in constructor, cannot change after
        // 'MT5Service' = type that contains all API methods (BuyMarketByRisk, etc.)
        // '_service' = variable name (underscore means private field)
        private readonly MT5Service _service;

        // This field stores the MT5Service object passed in constructor
        // Every method in this class uses _service to call MT5 API
        // Example: _service.GetBalanceAsync() calls the balance API


        // ═══════════════════════════════════════════════════════════════════════
        // PROPERTIES: CONFIGURABLE PARAMETERS
        // ═══════════════════════════════════════════════════════════════════════

        // These are PUBLIC PROPERTIES = can be set from outside the class
        // { get; set; } = means they can be read and written
        // '= "EURUSD"' = default value if user doesn't set it

        // Symbol to trade (default: EURUSD)
        // Type: string (text)
        public string Symbol { get; set; } = "EURUSD";

        // Dollar amount to risk per trade (default: $20)
        // Type: double (decimal number)
        // This is used to calculate lot size automatically
        public double RiskAmount { get; set; } = 20.0;

        // Stop Loss distance in points (default: 10 points)
        // Type: int (whole number)
        // Example: if price is 1.10000, SL at 1.09990 (10 points away)
        public int StopLossPoints { get; set; } = 10;

        // Take Profit distance in points (default: 20 points)
        // Type: int (whole number)
        // Example: if price is 1.10000, TP at 1.10020 (20 points away)
        public int TakeProfitPoints { get; set; } = 20;

        // Trade direction: true = BUY, false = SELL (default: BUY)
        // Type: bool (boolean - true or false)
        public bool IsBuy { get; set; } = true;

        // Maximum seconds to hold position before force close (default: 60 seconds)
        // Type: int (whole number)
        // If SL/TP not hit in 60 seconds, position closes manually
        public int MaxHoldSeconds { get; set; } = 60;


        // ═══════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR: HOW THIS CLASS IS CREATED
        // ═══════════════════════════════════════════════════════════════════════

        // Constructor is called when you do: new SimpleScalpingOrchestrator(service)
        // It receives MT5Service object and stores it in _service field

        // 'public' = anyone can create this class
        // 'SimpleScalpingOrchestrator' = constructor name (same as class name)
        // 'MT5Service service' = parameter - must pass MT5Service when creating
        public SimpleScalpingOrchestrator(MT5Service service)
        {
            // Store the passed service object in our private field
            // 'this._service' = the field defined above at line 38
            // 'service' = the parameter passed to constructor
            _service = service;

            // Now _service is available to all methods in this class!
            // Every method can use: _service.GetBalanceAsync(), etc.
        }


        // ═══════════════════════════════════════════════════════════════════════
        // MAIN METHOD: ExecuteAsync (WHERE STRATEGY RUNS)
        // ═══════════════════════════════════════════════════════════════════════

        // This method contains ALL the strategy logic
        // It's called when user runs: orchestrator.ExecuteAsync()

        // 'public' = can be called from outside
        // 'async' = this method uses 'await' for asynchronous operations
        // 'Task<double>' = returns a double value wrapped in Task (profit/loss)
        // 'ExecuteAsync' = method name
        // 'CancellationToken ct = default' = optional parameter to cancel operation
        public async Task<double> ExecuteAsync(CancellationToken ct = default)
        {
            // ═══════════════════════════════════════════════════════════════════
            // STEP 1: PRINT HEADER AND INITIAL INFO
            // ═══════════════════════════════════════════════════════════════════

            // Print strategy name banner
            // '\n' = newline character (blank line)
            Console.WriteLine("\n+============================================================+");
            Console.WriteLine("|  SIMPLE SCALPING ORCHESTRATOR                             |");
            Console.WriteLine("+============================================================+\n");

            // Get current account balance
            // 'await' = pause here until MT5 responds with balance
            // '_service.GetBalanceAsync()' = calls MT5 API to get balance
            // Returns: double (example: 10000.50)
            var initialBalance = await _service.GetBalanceAsync();

            // Print starting balance - WITHOUT this line, balance is INVISIBLE!
            // '$' before string = interpolation, allows {initialBalance} to insert value
            // ':F2' = format as decimal with 2 digits after dot (10000.50)
            Console.WriteLine($"  Starting balance: ${initialBalance:F2}");

            // Print all strategy parameters
            // These come from the properties set above (lines 49-70)
            Console.WriteLine($"  Symbol: {Symbol}");

            // Ternary operator: (condition ? if_true : if_false)
            // If IsBuy is true, prints "BUY", otherwise prints "SELL"
            Console.WriteLine($"  Direction: {(IsBuy ? "BUY" : "SELL")}");

            // Print risk amount
            Console.WriteLine($"  Risk: ${RiskAmount:F2}");

            // Print SL and TP points on same line
            Console.WriteLine($"  SL: {StopLossPoints} pts | TP: {TakeProfitPoints} pts");

            // Print max hold time
            Console.WriteLine($"  Max hold: {MaxHoldSeconds}s\n");


            // ═══════════════════════════════════════════════════════════════════
            // STEP 2: TRY TO EXECUTE STRATEGY (ERROR HANDLING)
            // ═══════════════════════════════════════════════════════════════════

            // 'try' block = if any error happens inside, jump to 'catch' block
            // This prevents program from crashing
            try
            {
                // ═══════════════════════════════════════════════════════════════
                // STEP 3: OPEN MARKET POSITION
                // ═══════════════════════════════════════════════════════════════

                // Print status message
                Console.WriteLine("  Opening position...");

                // Declare variable to store order result
                // 'OrderSendData' = type from mt5_term_api (protobuf)
                // Contains: Order (ticket number), Volume (lot size), ReturnedCode, etc.
                // 'result' = variable name
                OrderSendData result;

                // Check direction: BUY or SELL?
                // 'if (IsBuy)' = if IsBuy property is true
                if (IsBuy)
                {
                    // ═══════════════════════════════════════════════════════════
                    // OPEN BUY POSITION
                    // ═══════════════════════════════════════════════════════════

                    // Call BuyMarketByRisk method from MT5Service (Sugar API)
                    // This method is defined in MT5Sugar.cs
                    // 'await' = pause here until order is sent and MT5 responds

                    // WHAT BuyMarketByRisk DOES INTERNALLY:
                    // 1. Gets current Ask price for symbol
                    // 2. Gets Point value for symbol
                    // 3. Calls CalcVolumeForRiskAsync to calculate lot size
                    //    Formula: RiskMoney / (StopLossPoints * PointValue * ContractSize)
                    // 4. Calculates SL price: Ask - (stopPoints * Point)
                    // 5. Calculates TP price: Ask + (tpPoints * Point)
                    // 6. Sends OrderSendAsync with all parameters
                    // 7. Returns OrderSendData result

                    result = await _service.BuyMarketByRisk(
                        symbol: Symbol,              // "EURUSD" (from property)
                        stopPoints: StopLossPoints,  // 10 (from property)
                        riskMoney: RiskAmount,       // 20.0 (from property)
                        tpPoints: TakeProfitPoints,  // 20 (from property)
                        comment: "Scalper"           // Comment shown in MT5 terminal
                    );

                    // After this line, 'result' contains:
                    // - result.Order = ticket number (example: 128402483)
                    // - result.Volume = calculated lot size (example: 2.33)
                    // - result.ReturnedCode = success code (10009 = success)
                    // - result.Comment = broker's response message
                }
                else
                {
                    // ═══════════════════════════════════════════════════════════
                    // OPEN SELL POSITION
                    // ═══════════════════════════════════════════════════════════

                    // Same as BuyMarketByRisk but for SELL direction
                    // WHAT SellMarketByRisk DOES INTERNALLY:
                    // 1. Gets current Bid price for symbol
                    // 2. Calculates lot size based on risk
                    // 3. Calculates SL price: Bid + (stopPoints * Point)
                    // 4. Calculates TP price: Bid - (tpPoints * Point)
                    // 5. Sends SELL order

                    result = await _service.SellMarketByRisk(
                        symbol: Symbol,              // "EURUSD"
                        stopPoints: StopLossPoints,  // 10
                        riskMoney: RiskAmount,       // 20.0
                        tpPoints: TakeProfitPoints,  // 20
                        comment: "Scalper"           // Comment
                    );
                }


                // ═══════════════════════════════════════════════════════════════
                // STEP 4: CHECK IF ORDER WAS SUCCESSFUL
                // ═══════════════════════════════════════════════════════════════

                // ReturnedCode 10009 = TRADE_RETCODE_DONE (success)
                // If code is NOT 10009, order failed
                // '!=' means "not equal to"
                if (result.ReturnedCode != 10009)
                {
                    // Order failed! Print error message
                    // result.Comment contains broker's error message
                    Console.WriteLine($"  ✗ Order failed: {result.Comment}");

                    // Return 0 (no profit) and exit method
                    // This stops execution and goes back to caller
                    return 0;
                }

                // If we reach here, order succeeded!

                // Print success message with ticket number
                // result.Order = ticket number (example: 128402483)
                Console.WriteLine($"  ✓ Position opened: #{result.Order}");

                // Print calculated volume (lot size)
                // result.Volume = lot size calculated by BuyMarketByRisk
                // ':F2' = format with 2 decimals (example: 2.33)
                Console.WriteLine($"  Volume: {result.Volume:F2} lots\n");


                // ═══════════════════════════════════════════════════════════════
                // STEP 5: HOLD POSITION FOR MAX DURATION
                // ═══════════════════════════════════════════════════════════════

                // Print waiting message
                Console.WriteLine($"  ⏳ Holding for {MaxHoldSeconds}s...\n");

                // Wait for MaxHoldSeconds seconds
                // 'Task.Delay' = asynchronous wait (doesn't block thread)
                // 'MaxHoldSeconds * 1000' = convert seconds to milliseconds
                // Example: 60 seconds * 1000 = 60000 milliseconds
                // 'ct' = CancellationToken to allow early cancellation
                await Task.Delay(MaxHoldSeconds * 1000, ct);

                // After delay completes, continue to next line


                // ═══════════════════════════════════════════════════════════════
                // STEP 6: CHECK IF POSITION STILL EXISTS
                // ═══════════════════════════════════════════════════════════════

                // Get all currently open position tickets from MT5
                // 'await' = pause until MT5 responds with list of tickets
                // Returns: OpenedPositionTickets object with array of ticket numbers
                var tickets = await _service.OpenedOrdersTicketsAsync();

                // Create boolean flag to track if our position is still open
                // 'bool' = boolean type (true or false)
                // 'stillOpen' = variable name
                // '= false' = initial value (assume closed)
                bool stillOpen = false;

                // Loop through all open position tickets
                // 'foreach' = iterate over each item in collection
                // 'var ticket' = current ticket number in loop
                // 'tickets.OpenedPositionTickets' = array of ticket numbers
                foreach (var ticket in tickets.OpenedPositionTickets)
                {
                    // Check if this ticket matches our order ticket
                    // 'ticket' = current ticket from loop (type: long)
                    // 'result.Order' = our order ticket (type: ulong)
                    // '(long)result.Order' = cast ulong to long for comparison
                    if (ticket == (long)result.Order)
                    {
                        // Found it! Our position is still open
                        stillOpen = true;

                        // Exit loop early (no need to check remaining tickets)
                        break;
                    }
                }

                // After loop: stillOpen = true if found, false if not found


                // ═══════════════════════════════════════════════════════════════
                // STEP 7: CLOSE POSITION IF STILL OPEN
                // ═══════════════════════════════════════════════════════════════

                // Check if position is still open
                if (stillOpen)
                {
                    // Position didn't hit SL or TP in time limit
                    // Need to close it manually

                    // Print status message
                    Console.WriteLine($"  Position still open after {MaxHoldSeconds}s - closing manually...");

                    // Close position by ticket number
                    // 'await' = pause until close operation completes
                    // 'CloseByTicket' = method from MT5Service
                    // 'result.Order' = ticket number of our position
                    await _service.CloseByTicket(result.Order);

                    // Print success message
                    Console.WriteLine("  ✓ Position closed");
                }
                else
                {
                    // Position already closed (SL or TP was hit)
                    // No action needed, just inform user
                    Console.WriteLine("  ✓ Position closed automatically (SL/TP hit)");
                }


                // ═══════════════════════════════════════════════════════════════
                // STEP 8: CALCULATE PROFIT/LOSS
                // ═══════════════════════════════════════════════════════════════

                // Get final account balance after trade
                // 'await' = pause until MT5 responds
                var finalBalance = await _service.GetBalanceAsync();

                // Calculate profit/loss
                // Formula: Final Balance - Initial Balance
                // Example: $10050.00 - $10000.00 = $50.00 profit
                // Type: double (can be positive or negative)
                var profit = finalBalance - initialBalance;

                // Print final balance - WITHOUT this, it's INVISIBLE!
                Console.WriteLine($"\n  Final balance: ${finalBalance:F2}");

                // Print profit/loss
                // If positive: profit, if negative: loss
                Console.WriteLine($"  Profit/Loss: ${profit:F2}");

                // Print footer
                Console.WriteLine("\n+============================================================+\n");

                // Return profit value to caller
                // Caller can use this: var result = await orchestrator.ExecuteAsync();
                return profit;
            }
            catch (Exception ex)
            {
                // ═══════════════════════════════════════════════════════════════
                // ERROR HANDLER: IF ANYTHING GOES WRONG
                // ═══════════════════════════════════════════════════════════════

                // This block runs if ANY error happens in try block
                // 'Exception ex' = error object containing error details
                // 'ex.Message' = error message text

                // Print error message
                Console.WriteLine($"\n  ✗ Error: {ex.Message}");
                Console.WriteLine("+============================================================+\n");

                // Return 0 (no profit/loss calculated due to error)
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
    - Field: _service (holds MT5Service object with all API methods)
    - Properties: Symbol, RiskAmount, StopLossPoints, etc. (configurable)
    - Constructor: Receives MT5Service and stores it in _service
    - Method: ExecuteAsync() runs the entire strategy

 2. EXECUTION FLOW:
    Step 1: Print header and get initial balance
    Step 2: Open BUY or SELL position with risk-based sizing
    Step 3: Check if order succeeded (ReturnedCode 10009)
    Step 4: Hold position for MaxHoldSeconds
    Step 5: Check if position still open
    Step 6: Close manually if still open, or note SL/TP hit
    Step 7: Calculate profit/loss (final - initial balance)
    Step 8: Return profit value

 3. KEY CONCEPTS:
    - _service field = WHERE all API methods come from
    - await = PAUSES until API responds (non-blocking)
    - Console.WriteLine = MAKES data visible (without it, data is invisible!)
    - foreach loop = CHECK each ticket to find our position
    - try/catch = HANDLES errors gracefully

 4. SUGAR API USED:
    - BuyMarketByRisk = Calculates lot size + opens BUY (defined in MT5Sugar.cs)
    - SellMarketByRisk = Calculates lot size + opens SELL (defined in MT5Sugar.cs)
    - GetBalanceAsync = Gets account balance (defined in MT5Service.cs)
    - OpenedOrdersTicketsAsync = Gets all open tickets (defined in MT5Service.cs)
    - CloseByTicket = Closes position by ticket (defined in MT5Service.cs)

══════════════════════════════════════════════════════════════════════════════*/
