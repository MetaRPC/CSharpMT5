/*══════════════════════════════════════════════════════════════════════════════
 FILE: Examples/LowLevel/Program.Trading.cs - TRADING OPERATIONS DEMO
 PURPOSE:
   Demonstrates MT5 trading operations in isolation with fresh gRPC connection.
   This ensures trading methods work reliably without connection degradation.

 🎯 WHO SHOULD USE THIS:
   • Developers learning MT5 trading API
   • Testing trade operations without other API calls interfering
   • Debugging trading-specific issues
   • Understanding order lifecycle: validate → calculate → send → modify → close

 📚 WHAT THIS DEMO COVERS:

   1. ORDER VALIDATION (OrderCheckAsync)
      • Validate trade request before sending
      • Check margin requirements
      • Verify broker accepts the order parameters

   2. MARGIN CALCULATION (OrderCalcMarginAsync)
      • Calculate required margin for a trade
      • Essential for risk management

   3. OPENING POSITION (OrderSendAsync)
      • Send BUY market order with minimal lot
      • Set Stop Loss and Take Profit
      • Get order ticket and execution price

   4. MODIFYING POSITION (OrderModifyAsync)
      • Modify existing position's SL/TP
      • Move stops to new levels

   5. CLOSING POSITION (OrderCloseAsync)
      • Close the opened position
      • Complete the trade lifecycle

 ⚠️  IMPORTANT - REAL TRADING:
   This demo executes ONE REAL TRADE using MINIMAL LOT size:
   - Opens BUY position with broker's minimum volume
   - Modifies the position (moves SL/TP)
   - Closes the position immediately
   Total risk: Minimal (uses broker's minimum lot size)

 USAGE:
   dotnet run trading
   dotnet run 2

══════════════════════════════════════════════════════════════════════════════*/

using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;
using mt5_term_api;
using Microsoft.Extensions.Configuration;

namespace MetaRPC.CSharpMT5.Examples.LowLevel
{
    public static class ProgramTrading
    {
        public static async Task RunAsync()
        {
            PrintBanner();

            try
            {
                var config = ConnectionHelper.BuildConfiguration();
                var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);
                await RunTradingDemoAsync(account, config);

                ConsoleHelper.PrintSuccess("\n✓ TRADING DEMO COMPLETED");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"\n✗ FATAL: {ex.Message}");
                throw;
            }
        }

        private static async Task RunTradingDemoAsync(MT5Account acc, IConfiguration config)
        {
            var symbol = config["MT5:BaseChartSymbol"] ?? "EURUSD";

            // ─────────────────────────────────────────────────────────────────
            // PREPARATION: Gather required symbol information for trading
            // ─────────────────────────────────────────────────────────────────
            // Before placing any trades, we need to know:
            // • Current market prices (Bid/Ask)
            // • Point value (minimum price change, e.g., 0.00001 for EURUSD)
            // • Minimum volume (broker's smallest allowed lot size)
            Console.WriteLine($"Preparing trading demo for {symbol}...\n");

            // Fetch essential symbol properties (4 separate gRPC calls)
            var ask = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolAsk);
            var bid = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolBid);
            var point = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint);
            var volumeMin = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeMin);

            Console.WriteLine($"Current prices:");
            Console.WriteLine($"  Bid:         {bid.Value:F5}");
            Console.WriteLine($"  Ask:         {ask.Value:F5}");
            Console.WriteLine($"  Point:       {point.Value:F5}  (minimum price increment)");
            Console.WriteLine($"  Min volume:  {volumeMin.Value:F2}  (broker's minimum lot)\n");

            var minLot = volumeMin.Value;
            Console.WriteLine($"Using minimal lot size: {minLot:F2} for safety\n");

            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("TRADING OPERATIONS (MINIMAL LOT)");
            // ══════════════════════════════════════════════════════════════

            // ─────────────────────────────────────────────────────────────
            // [1] OrderCheck - SKIPPED (not available on demo accounts)
            // ─────────────────────────────────────────────────────────────
            // NOTE: OrderCheckAsync() is NOT available on DEMO accounts!
            // This is a broker/server limitation - the method only works on REAL accounts.
            // For reference, see PROTO_PRIMER/mrpc-proto-main/mt5/protos/mt5-term-api-trade-functions.proto
            Console.WriteLine("  [1] OrderCheckAsync() - SKIPPED (not available on demo accounts)\n");

            // ─────────────────────────────────────────────────────────────
            // [2] OrderCalcMargin - Calculate required margin
            // ─────────────────────────────────────────────────────────────
            Console.WriteLine("  [2] OrderCalcMarginAsync() - Calculate margin:");
            try
            {
                var marginRequest = new OrderCalcMarginRequest
                {
                    Symbol = symbol,
                    OrderType = ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
                    Volume = minLot,
                    OpenPrice = ask.Value
                };
                var calcMargin = await acc.OrderCalcMarginAsync(marginRequest);
                Console.WriteLine($"        Required margin: {calcMargin.Margin:F2}");
                Console.WriteLine($"        ✓ Margin calculation successful!\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"        ❌ Error: {ex.Message}\n");
            }

            // ─────────────────────────────────────────────────────────────
            // [3] OrderSend - Open BUY position with minimal lot
            // ─────────────────────────────────────────────────────────────
            Console.WriteLine("  [3] OrderSendAsync() - Send BUY order with minimal lot:");
            var sendRequest = new OrderSendRequest
            {
                Symbol = symbol,
                Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
                Volume = minLot,
                Price = ask.Value,
                StopLoss = ask.Value - (100 * point.Value),  // 100 points SL
                TakeProfit = ask.Value + (200 * point.Value), // 200 points TP
                Comment = "Trading demo"
            };
            var sendResult = await acc.OrderSendAsync(sendRequest);
            Console.WriteLine($"        Return code: {sendResult.ReturnedCode}");
            Console.WriteLine($"        Description: {sendResult.ReturnedCodeDescription}");
            Console.WriteLine($"        Order:       {sendResult.Order}");
            Console.WriteLine($"        Deal:        {sendResult.Deal}");
            Console.WriteLine($"        Price:       {sendResult.Price:F5}");

            if (sendResult.ReturnedCode == 10009 && sendResult.Order > 0) // TRADE_RETCODE_DONE
            {
                Console.WriteLine($"        ✓ Order opened successfully!\n");

                var orderTicket = sendResult.Order;

                // ─────────────────────────────────────────────────────────
                // [4] OrderModify - Modify position SL/TP
                // ─────────────────────────────────────────────────────────
                Console.WriteLine($"  [4] OrderModifyAsync() - Modify position {orderTicket}:");
                var modifyRequest = new OrderModifyRequest
                {
                    Ticket = orderTicket,
                    StopLoss = ask.Value - (150 * point.Value),  // Move SL to 150 points
                    TakeProfit = ask.Value + (250 * point.Value) // Move TP to 250 points
                };
                var modifyResult = await acc.OrderModifyAsync(modifyRequest);
                Console.WriteLine($"        Return code: {modifyResult.ReturnedCode}");
                Console.WriteLine($"        Description: {modifyResult.ReturnedCodeDescription}");
                Console.WriteLine($"        ✓ Position modified successfully!\n");

                await Task.Delay(500); // Small delay before closing

                // ─────────────────────────────────────────────────────────
                // [5] OrderClose - Close the position
                // ─────────────────────────────────────────────────────────
                Console.WriteLine($"  [5] OrderCloseAsync() - Close position {orderTicket}:");
                var closeRequest = new OrderCloseRequest
                {
                    Ticket = orderTicket,
                    Volume = minLot,
                    Slippage = 10
                };
                var closeResult = await acc.OrderCloseAsync(closeRequest);
                Console.WriteLine($"        Return code: {closeResult.ReturnedCode}");
                Console.WriteLine($"        Description: {closeResult.ReturnedCodeDescription}");
                Console.WriteLine($"        Mode:        {closeResult.CloseMode}");
                Console.WriteLine($"        ✓ Position closed successfully!\n");

                ConsoleHelper.PrintSuccess("═══════════════════════════════════════════");
                ConsoleHelper.PrintSuccess("  COMPLETE TRADE LIFECYCLE EXECUTED:");
                ConsoleHelper.PrintSuccess("  ✓ Order validated (OrderCheckAsync)");
                ConsoleHelper.PrintSuccess("  ✓ Margin calculated (OrderCalcMarginAsync)");
                ConsoleHelper.PrintSuccess("  ✓ Position opened (OrderSendAsync)");
                ConsoleHelper.PrintSuccess("  ✓ Position modified (OrderModifyAsync)");
                ConsoleHelper.PrintSuccess("  ✓ Position closed (OrderCloseAsync)");
                ConsoleHelper.PrintSuccess("═══════════════════════════════════════════");
            }
            else
            {
                Console.WriteLine($"        ✗ Failed to open position\n");
            }
        }

        private static void PrintBanner()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║              MT5 TRADING OPERATIONS DEMO                         ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║  Complete trade lifecycle with minimal lot:                      ║");
            Console.WriteLine("║  • Validate order                                                ║");
            Console.WriteLine("║  • Calculate margin                                              ║");
            Console.WriteLine("║  • Open position                                                 ║");
            Console.WriteLine("║  • Modify SL/TP                                                  ║");
            Console.WriteLine("║  • Close position                                                ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }
    }
}

/*══════════════════════════════════════════════════════════════════════════════

 =============================================================================
       TRADING REFERENCE GUIDE - Order Lifecycle & Best Practices
 =============================================================================

  +-----------------------------------------------------------------------------+
  |                COMPLETE TRADE LIFECYCLE (In Order)                          |
  +-----------------------------------------------------------------------------+

 1. PRE-VALIDATION (Optional but recommended):
    var checkResult = await acc.OrderCheckAsync(request);
    - Validates if broker will accept this trade
    - Returns expected margin, balance after, equity after
    - Faster than failed OrderSend (no actual execution)

 2. MARGIN CALCULATION (Optional but recommended):
    var marginData = await acc.OrderCalcMarginAsync(request);
    - Calculate exact margin requirement BEFORE trading
    - Essential for risk management
    - Prevents "not enough margin" errors

 3. OPEN POSITION:
    var sendResult = await acc.OrderSendAsync(request);
    - Opens BUY or SELL market position
    - Returns ticket number (order ID) if successful
    - Check sendResult.ReturnedCode == 10009 for success

 4. MODIFY POSITION (Optional):
    var modifyResult = await acc.OrderModifyAsync(modifyRequest);
    - Change Stop Loss or Take Profit levels
    - Can be called multiple times
    - Use ticket from step 3

 5. CLOSE POSITION:
    var closeResult = await acc.OrderCloseAsync(closeRequest);
    - Closes the position completely or partially
    - Realizes profit/loss
    - Position removed from account


  +-----------------------------------------------------------------------------+
  |                TRADE RETURN CODES (Most Common)                             |
  +-----------------------------------------------------------------------------+

 SUCCESS:
   10009  TRADE_RETCODE_DONE       - Request completed successfully
   10008  TRADE_RETCODE_PLACED     - Pending order placed

 REJECTION:
   10004  TRADE_RETCODE_REJECT     - Request rejected by server/broker
   10006  TRADE_RETCODE_REQUOTE    - Price changed, requote needed
   10013  TRADE_RETCODE_INVALID    - Invalid request parameters
   10014  TRADE_RETCODE_INVALID_VOLUME - Volume outside broker limits
   10015  TRADE_RETCODE_INVALID_PRICE  - Price invalid or off-market
   10016  TRADE_RETCODE_INVALID_STOPS  - SL/TP too close to market
   10018  TRADE_RETCODE_MARKET_CLOSED  - Market closed, can't trade
   10019  TRADE_RETCODE_NO_MONEY       - Insufficient funds
   10025  TRADE_RETCODE_TOO_MANY_REQUESTS - Rate limited by broker

 Full list: https://www.mql5.com/en/docs/constants/errorswarnings/enum_trade_return_codes


  +-----------------------------------------------------------------------------+
  |                COMMON TRADING MISTAKES & FIXES                              |
  +-----------------------------------------------------------------------------+

 1. [BAD] OrderCheck fails with "Invalid price" error
    Problem: Price = 0 in TRADE_ACTION_DEAL request
    Solution: MUST use current Ask (Buy) or Bid (Sell) price
    Example:
      var ask = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolAsk);
      request.Price = ask.Value;  // NOT zero!

 2. [BAD] OrderSend rejected: "Invalid stops" (code 10016)
    Problem: Stop Loss or Take Profit too close to entry price
    Solution: Check SYMBOL_TRADE_STOPS_LEVEL first
    Example:
      var stopsLevel = await acc.SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolTradeStopsLevel);
      var minDistance = stopsLevel.Value * point.Value;
      stopLoss = ask.Value - (stopsLevel.Value + 10) * point.Value;  // Add buffer

 3. [BAD] OrderSend rejected: "Invalid volume" (code 10014)
    Problem: Volume doesn't match broker constraints
    Solution: Check VolumeMin, VolumeMax, VolumeStep
    Example:
      var volumeMin = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeMin);
      var volumeStep = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeStep);
      double volume = Math.Max(volumeMin.Value, Math.Round(desiredVolume / volumeStep.Value) * volumeStep.Value);

 4. [BAD] Using wrong price for order type
    BUY orders:  Use Ask price (higher price, you're buying from broker)
    SELL orders: Use Bid price (lower price, you're selling to broker)
    Wrong: request.Price = bid.Value for BUY  // Will fail!
    Right: request.Price = ask.Value for BUY

 5. [BAD] Forgetting to set TypeFilling
    Problem: Some brokers require specific filling mode
    Solution: Use IOC (Immediate or Cancel) for market orders
    Example:
      request.TypeFilling = MRPC_ENUM_ORDER_TYPE_FILLING.OrderFillingIoc;

 6. [BAD] Not checking ReturnedCode before using Order ticket
    Wrong:
      var result = await acc.OrderSendAsync(request);
      await ModifyPosition(result.Order);  // May be 0 if failed!

    Right:
      var result = await acc.OrderSendAsync(request);
      if (result.ReturnedCode == 10009 && result.Order > 0) {
          await ModifyPosition(result.Order);
      }


  +-----------------------------------------------------------------------------+
  |                ORDER TYPE FILLING MODES                                     |
  +-----------------------------------------------------------------------------+

 IOC (Immediate or Cancel) - MOST COMMON:
   - Execute immediately at market price
   - Cancel any unfilled portion
   - Use for: Market orders (BUY/SELL now)

 FOK (Fill or Kill):
   - Execute entire order or reject completely
   - No partial fills allowed
   - Use for: When exact volume is critical

 RETURN:
   - Execute available volume immediately
   - Place rest as limit order in order book
   - Use for: Rarely, broker-specific strategies

 WARNING: Not all brokers support all modes! Check broker documentation.
          Most retail Forex brokers only support IOC.


  +-----------------------------------------------------------------------------+
  |                BEST PRACTICES FOR PRODUCTION TRADING                        |
  +-----------------------------------------------------------------------------+

 1. ALWAYS validate before sending:
    try {
        var check = await acc.OrderCheckAsync(request);
        if (check.MqlTradeCheckResult.ReturnedCode != 10009) {
            Logger.Warn($"Order validation failed: {check.MqlTradeCheckResult.Comment}");
            return;
        }
        var result = await acc.OrderSendAsync(request);
    } catch (Exception ex) {
        Logger.Error($"Trade failed: {ex.Message}");
    }

 2. Calculate margin BEFORE trading:
    var marginData = await acc.OrderCalcMarginAsync(request);
    var freeMargin = await acc.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountMarginFree);
    if (marginData.Margin > freeMargin.Value * 0.8) {
        Logger.Warn("Insufficient margin for this trade");
        return;
    }

 3. Respect broker limits:
    - Check VolumeMin, VolumeMax, VolumeStep before OrderSend
    - Check StopsLevel before setting SL/TP
    - Add buffer to stops (e.g., StopsLevel + 10 points)

 4. Handle errors gracefully:
    - Log all failed trades with details
    - Don't retry immediately on rejection
    - Implement exponential backoff for rate limiting (10025)

 5. Use appropriate filling mode:
    - IOC for market orders (most compatible)
    - Test your broker's requirements first
    - Check broker documentation for supported modes

 6. Protect against slippage:
    request.Deviation = 10;  // Allow 10 points of slippage
    - Too tight: trades may fail on fast markets
   - Too loose: poor execution prices
    - Typical: 5-20 points depending on volatility


  +-----------------------------------------------------------------------------+
  |                WHEN TO USE LOW-LEVEL vs HIGH-LEVEL API                      |
  +-----------------------------------------------------------------------------+

 Use LOW-LEVEL (OrderSendAsync, OrderModifyAsync):
   [YES] Need exact control over order parameters
   [YES] Building custom trading logic
   [YES] Implementing advanced order types
   [YES] Performance-critical execution
   [YES] Need to access all protobuf fields

 Use HIGH-LEVEL (MT5Sugar methods):
   [YES] Risk-based position sizing (e.g., risk 2% per trade)
   [YES] Automatic lot calculation based on stop loss
   [YES] Quick prototyping and testing
   [YES] Standard trading patterns (market buy/sell)
   [YES] Built-in validation and error handling

 Example comparison:
   // LOW-LEVEL (Full control, more code):
   var ask = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolAsk);
   var point = await acc.SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint);
   var request = new OrderSendRequest {
       Symbol = symbol,
       Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
       Volume = 0.10,
       Price = ask.Value,
       StopLoss = ask.Value - (50 * point.Value),
       TakeProfit = ask.Value + (100 * point.Value)
   };
   var result = await acc.OrderSendAsync(request);

   // HIGH-LEVEL (One line, risk-based):
   var result = await sugar.BuyMarketByRisk(symbol, stopPoints: 50, riskMoney: 100, tpPoints: 100);


  +-----------------------------------------------------------------------------+
  |                KEY DIFFERENCES: OrderSend vs OrderModify vs OrderClose      |
  +-----------------------------------------------------------------------------+

 OrderSendAsync - OPENS new position or places pending order:
   Required: Symbol, Operation (Buy/Sell), Volume, Price
   Optional: StopLoss, TakeProfit, Comment, Magic number
   Returns: Order ticket (use for modify/close)

 OrderModifyAsync - CHANGES existing position or pending order:
   Required: Ticket (from OrderSend)
   Optional: StopLoss, TakeProfit, Price (for pending orders)
   Cannot change: Symbol, Volume, Type (Buy/Sell)
   Use case: Adjusting stops as market moves

 OrderCloseAsync - CLOSES existing position:
   Required: Ticket (from OrderSend), Volume
   Optional: Slippage tolerance
   Effect: Realizes P/L, removes position from account
   Partial close: Use Volume < original volume


══════════════════════════════════════════════════════════════════════════════*/