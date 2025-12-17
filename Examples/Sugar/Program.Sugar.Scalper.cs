/*══════════════════════════════════════════════════════════════════════════════
 FILE: Examples/Sugar/Program.Sugar.Scalper.cs - SCALPING STRATEGY WITH RISK MANAGEMENT
 PURPOSE:
   Demonstrates a complete scalping trading strategy using Sugar API with automatic
   risk-based position sizing, margin validation, and real-time P&L monitoring.

 🎯 WHO SHOULD USE THIS:
   • Scalpers who need fast entry/exit with precise risk control
   • Traders implementing risk-based position sizing ($-based stops)
   • Developers building automated scalping bots
   • Anyone who wants to validate margin before trading

 🔄 WHAT THIS STRATEGY DOES:
   This is a REALISTIC SCALPING STRATEGY that executes:
   1. Validates available margin before trading
   2. Opens BUY position risking $5 with 50-point stop
   3. Opens SELL position risking $5 with 50-point stop
   4. Modifies stops to breakeven when in profit
   5. Closes positions when target reached
   6. Monitors total P&L in real-time

 📋 WHAT THIS DEMO COVERS (15 Methods):

   1. INFRASTRUCTURE
      • EnsureSelected() - Add symbol to Market Watch

   2. SNAPSHOTS
      • GetAccountSnapshot() - Account info in one call
      • GetSymbolSnapshot() - Symbol info in one call

   3. NORMALIZATION HELPERS
      • GetPointAsync() - Get point size
      • GetDigitsAsync() - Get decimal digits
      • GetSpreadPointsAsync() - Get spread in points

   4. VOLUME & RISK CALCULATION ⭐
      • GetVolumeLimitsAsync() - Get min/max/step volume
      • CalcVolumeForRiskAsync() - Calculate lot by risk amount

   5. MARKET ORDERS (Risk-Based) ⭐
      • BuyMarketByRisk() - Open BUY with auto lot calculation
      • SellMarketByRisk() - Open SELL with auto lot calculation

   6. POSITION MANAGEMENT
      • ModifySlTpAsync() - Modify SL/TP
      • CloseByTicket() - Close position

   7. MARGIN VALIDATION ⭐
      • CheckMarginAvailabilityAsync() - Check margin availability
      • CalculateBuyMarginAsync() - Calculate buy margin

   8. POSITION MONITORING
      • GetTotalProfitLossAsync() - Get total P&L

 ⚠️  IMPORTANT - TRADING OPERATIONS:
   This demo executes REAL TRADES using RISK-BASED SIZING:
   - Opens 2 market positions (BUY + SELL)
   - Risk: $5 per position = $10 total
   - Uses auto-calculated lot sizes based on risk
   - All trades close automatically

   Total risk: $10 (controlled by risk parameters)

 💡 WHEN TO USE RISK-BASED TRADING:
   • Scalping strategies with tight stops
   • Dollar-based risk management
   • Automatic position sizing
   • Margin validation before trading
   • Real-time P&L monitoring

 USAGE:
   dotnet run scalper
   dotnet run 8
══════════════════════════════════════════════════════════════════════════════*/

using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;
using Microsoft.Extensions.Configuration;

namespace MetaRPC.CSharpMT5.Examples.Sugar
{
    public static class ProgramSugarScalper
    {
        public static async Task RunAsync()
        {
            PrintBanner();

            try
            {
                // ─── SETUP ───────────────────────────────────────────────
                var config = ConnectionHelper.BuildConfiguration();
                var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);
                var svc = new MT5Service(account);

                ConsoleHelper.PrintSuccess("✓ Connected! Sugar API ready for scalping.\n");

                var symbol = config["Mt5:BaseChartSymbol"] ?? "EURUSD";

                // ─── RUN SCALPER STRATEGY ────────────────────────────────
                await RunScalperStrategyAsync(svc, symbol);

                ConsoleHelper.PrintSuccess("\n✓ SCALPER STRATEGY COMPLETED!");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"\n✗ ERROR: {ex.Message}");
                throw;
            }
        }

        private static async Task RunScalperStrategyAsync(MT5Service svc, string symbol)
        {
            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("STEP 1: MARKET PREPARATION");
            // ══════════════════════════════════════════════════════════════

            // ══════════════════════════════════════════════════════════════
            // [1] EnsureSelected
            //     Ensures symbol is added to Market Watch if not already present.
            //     Required before any symbol operations to guarantee data availability.
            //     Idempotent: safe to call multiple times without side effects.
            // ══════════════════════════════════════════════════════════════
            Console.WriteLine($"  → EnsureSelected({symbol})...");
            await svc.EnsureSelected(symbol);
            Console.WriteLine("    ✓ Symbol added to Market Watch\n");

            // ══════════════════════════════════════════════════════════════
            // [2] GetAccountSnapshot
            //     Retrieves account summary with balance, equity, margin in ONE call.
            //     Combines AccountSummaryAsync + OpenedOrdersAsync for efficiency.
            //     Returns: AccountBalance, AccountEquity, AccountMargin, AccountLeverage,
            //              AccountCurrency, and list of currently opened positions/orders.
            //     Use this instead of multiple individual AccountInfo* calls.
            // ══════════════════════════════════════════════════════════════
            Console.WriteLine("  → GetAccountSnapshot()...");
            var accSnapshot = await svc.GetAccountSnapshot();
            Console.WriteLine($"    ✓ Balance: {accSnapshot.Summary.AccountBalance:F2} {accSnapshot.Summary.AccountCurrency}");
            Console.WriteLine($"    ✓ Equity:  {accSnapshot.Summary.AccountEquity:F2}");
            Console.WriteLine($"    ✓ Leverage: 1:{accSnapshot.Summary.AccountLeverage}\n");

            // ══════════════════════════════════════════════════════════════
            // [3] GetSymbolSnapshot
            //     Retrieves complete symbol info in ONE call: Tick + Point + Digits + MarginRate.
            //     Combines 4 separate calls: SymbolInfoTickAsync, SymbolInfoDoubleAsync (Point),
            //     SymbolInfoIntegerAsync (Digits), SymbolInfoMarginRateAsync.
            //     Essential for any trading operation requiring price and precision data.
            // ══════════════════════════════════════════════════════════════
            Console.WriteLine("  → GetSymbolSnapshot()...");
            var symSnapshot = await svc.GetSymbolSnapshot(symbol);
            Console.WriteLine($"    ✓ Bid:   {symSnapshot.Tick.Bid:F5}");
            Console.WriteLine($"    ✓ Ask:   {symSnapshot.Tick.Ask:F5}");
            Console.WriteLine($"    ✓ Point: {symSnapshot.Point}");
            Console.WriteLine($"    ✓ Digits: {symSnapshot.Digits}\n");

            // ══════════════════════════════════════════════════════════════
            // [4] GetSpreadPointsAsync
            //     Returns current spread in points (not pips).
            //     Spread = (Ask - Bid) / Point.
            //     Important for scalping to filter out high-spread conditions.
            // ══════════════════════════════════════════════════════════════
            Console.WriteLine("  → GetSpreadPointsAsync()...");
            var spread = await svc.GetSpreadPointsAsync(symbol);
            Console.WriteLine($"    ✓ Spread: {spread:F1} points\n");

            // ══════════════════════════════════════════════════════════════
            // [5] GetVolumeLimitsAsync
            //     Returns min/max/step volume constraints from broker.
            //     Essential for validating and normalizing trade volumes.
            //     Returns tuple: (MinVolume, MaxVolume, VolumeStep).
            //     Always use these limits before placing orders.
            // ══════════════════════════════════════════════════════════════
            Console.WriteLine("  → GetVolumeLimitsAsync()...");
            var (minVol, maxVol, stepVol) = await svc.GetVolumeLimitsAsync(symbol);
            Console.WriteLine($"    ✓ Min:  {minVol:F2}");
            Console.WriteLine($"    ✓ Max:  {maxVol:F2}");
            Console.WriteLine($"    ✓ Step: {stepVol:F2}\n");

            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("STEP 2: RISK CALCULATION");
            // ══════════════════════════════════════════════════════════════

            double riskMoney = 5.0; // Risk $5 per trade
            double stopPoints = 50; // 50-point stop loss
            double tpPoints = 100;  // 100-point take profit

            Console.WriteLine($"  Risk per trade: ${riskMoney}");
            Console.WriteLine($"  Stop Loss:      {stopPoints} points");
            Console.WriteLine($"  Take Profit:    {tpPoints} points\n");

            // ══════════════════════════════════════════════════════════════
            // [6] CalcVolumeForRiskAsync ⭐ KEY METHOD
            //     Calculates lot size based on dollar risk amount and stop loss distance.
            //     Formula: Volume = RiskMoney / (StopPoints × Point × TickValue)
            //     Automatically accounts for: tick value, point size, account currency.
            //     This is THE method for risk-based position sizing in scalping.
            //     Returns: Calculated volume (may need adjustment to broker min/step).
            // ══════════════════════════════════════════════════════════════
            Console.WriteLine("  → CalcVolumeForRiskAsync()...");
            var calcVolume = await svc.CalcVolumeForRiskAsync(symbol, stopPoints, riskMoney);
            Console.WriteLine($"    ✓ Calculated volume: {calcVolume:F2} lots");

            // Use minimum of calculated or broker minimum
            var tradeVolume = Math.Max(calcVolume, minVol);
            Console.WriteLine($"    ✓ Trade volume:      {tradeVolume:F2} lots (adjusted to minimum)\n");

            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("STEP 3: MARGIN VALIDATION");
            // ══════════════════════════════════════════════════════════════

            // ══════════════════════════════════════════════════════════════
            // [7] CalculateBuyMarginAsync
            //     Calculates required margin for opening a BUY position.
            //     Uses OrderCalcMarginAsync under the hood with proper order type.
            //     Parameters: symbol, volume, price (use current Ask, 0 may not work).
            //     Returns: Required margin in account currency.
            //     Note: Some brokers don't support this - will throw exception.
            //     Essential for pre-trade validation to avoid margin call errors.
            // ══════════════════════════════════════════════════════════════
            Console.WriteLine("  → CalculateBuyMarginAsync()...");
            try
            {
                // Pass current Ask price (0 = market price may not work on some brokers)
                var currentAsk = symSnapshot.Tick.Ask;
                var buyMargin = await svc.CalculateBuyMarginAsync(symbol, tradeVolume, price: currentAsk);
                Console.WriteLine($"    ✓ Required margin for BUY: {buyMargin:F2}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ⚠ Not supported by broker: {ex.Message.Split('.')[0]}");
                Console.WriteLine($"    → Skipping margin calculation (broker limitation)\n");
            }

            // ══════════════════════════════════════════════════════════════
            // [8] CheckMarginAvailabilityAsync ⭐ IMPORTANT
            //     Verifies sufficient margin before opening position - prevents rejections.
            //     Compares free margin with required margin for the trade.
            //     Parameters: symbol, volume, isBuy flag.
            //     Returns tuple: (hasEnoughMargin, freeMargin, requiredMargin).
            //     Best practice: ALWAYS check before opening position to avoid errors.
            //     Note: Some brokers don't support this - will throw exception.
            // ══════════════════════════════════════════════════════════════
            Console.WriteLine("  → CheckMarginAvailabilityAsync()...");
            try
            {
                var (hasEnough, freeMargin, required) = await svc.CheckMarginAvailabilityAsync(
                    symbol,
                    tradeVolume,
                    isBuy: true
                );

                Console.WriteLine($"    ✓ Has enough margin: {hasEnough}");
                Console.WriteLine($"    ✓ Free margin:       {freeMargin:F2}");
                Console.WriteLine($"    ✓ Required:          {required:F2}\n");

                if (!hasEnough)
                {
                    ConsoleHelper.PrintError("    ✗ Insufficient margin! Cannot trade.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ⚠ Not supported by broker: {ex.Message.Split('.')[0]}");
                Console.WriteLine($"    → Continuing without margin check (broker limitation)\n");
            }

            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("STEP 4: OPEN POSITIONS (RISK-BASED)");
            // ══════════════════════════════════════════════════════════════

            ulong? buyTicket = null;
            ulong? sellTicket = null;

            // ══════════════════════════════════════════════════════════════
            // [9] BuyMarketByRisk ⭐⭐⭐ SIGNATURE METHOD
            //     Opens BUY position with AUTOMATIC lot size calculation based on risk.
            //     This is the CORE of risk-based trading - set risk in dollars, not lots!
            //     Parameters:
            //       - symbol: Trading symbol
            //       - stopPoints: Stop loss distance in points
            //       - riskMoney: Dollar amount to risk if stop hit
            //       - tpPoints: Take profit distance in points (optional)
            //       - comment: Order comment (optional)
            //     Process:
            //       1. Calculates volume via CalcVolumeForRiskAsync
            //       2. Normalizes volume to broker constraints
            //       3. Gets current Ask price
            //       4. Calculates SL/TP prices from point offsets
            //       5. Places market order via OrderSendAsync
            //     Returns: OrderResult with ticket, price, return code.
            //     Perfect for scalping: "Risk $5 with 50-point stop" - done!
            // ══════════════════════════════════════════════════════════════
            Console.WriteLine($"  → BuyMarketByRisk(risk=${riskMoney}, stop={stopPoints}pts)...");
            try
            {
                var buyTask = svc.BuyMarketByRisk(
                    symbol: symbol,
                    stopPoints: stopPoints,
                    riskMoney: riskMoney,
                    tpPoints: tpPoints,
                    comment: "SCALPER-BUY"
                );

                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
                var completedTask = await Task.WhenAny(buyTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    Console.WriteLine($"    ✗ Timeout: Operation took longer than 30 seconds\n");
                }
                else
                {
                    var buyResult = await buyTask;
                    Console.WriteLine($"    ✓ Return code: {buyResult.ReturnedCode}");
                    Console.WriteLine($"    ✓ Description: {buyResult.ReturnedCodeDescription}");

                    if (buyResult.Order > 0)
                    {
                        buyTicket = buyResult.Order;
                        Console.WriteLine($"    ✓ BUY Ticket: {buyTicket.Value}");
                        Console.WriteLine($"    ✓ Price:      {buyResult.Price:F5}\n");
                    }
                    else
                    {
                        Console.WriteLine("    ✗ No ticket returned\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            await Task.Delay(500); // Small delay between trades

            // ══════════════════════════════════════════════════════════════
            // [10] SellMarketByRisk ⭐⭐⭐ SIGNATURE METHOD
            //     Opens SELL position with AUTOMATIC lot size calculation based on risk.
            //     Mirror of BuyMarketByRisk for SELL direction.
            //     Parameters: same as BuyMarketByRisk
            //     Process:
            //       1. Calculates volume via CalcVolumeForRiskAsync
            //       2. Normalizes volume to broker constraints
            //       3. Gets current Bid price (SELL uses Bid!)
            //       4. Calculates SL/TP prices from point offsets
            //          (SL = Bid + stopPoints × Point, TP = Bid - tpPoints × Point)
            //       5. Places market order via OrderSendAsync
            //     Returns: OrderResult with ticket, price, return code.
            //     Combined with BuyMarketByRisk for hedged scalping strategies.
            // ══════════════════════════════════════════════════════════════
            Console.WriteLine($"  → SellMarketByRisk(risk=${riskMoney}, stop={stopPoints}pts)...");
            try
            {
                var sellTask = svc.SellMarketByRisk(
                    symbol: symbol,
                    stopPoints: stopPoints,
                    riskMoney: riskMoney,
                    tpPoints: tpPoints,
                    comment: "SCALPER-SELL"
                );

                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
                var completedTask = await Task.WhenAny(sellTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    Console.WriteLine($"    ✗ Timeout: Operation took longer than 30 seconds\n");
                }
                else
                {
                    var sellResult = await sellTask;
                    Console.WriteLine($"    ✓ Return code: {sellResult.ReturnedCode}");
                    Console.WriteLine($"    ✓ Description: {sellResult.ReturnedCodeDescription}");

                    if (sellResult.Order > 0)
                    {
                        sellTicket = sellResult.Order;
                        Console.WriteLine($"    ✓ SELL Ticket: {sellTicket.Value}");
                        Console.WriteLine($"    ✓ Price:       {sellResult.Price:F5}\n");
                    }
                    else
                    {
                        Console.WriteLine("    ✗ No ticket returned\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Failed: {ex.Message}\n");
            }

            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("STEP 5: MONITOR & MODIFY POSITIONS");
            // ══════════════════════════════════════════════════════════════

            // ══════════════════════════════════════════════════════════════
            // [11] GetTotalProfitLossAsync
            //     Returns sum of unrealized P&L for all open positions on symbol.
            //     Queries all open positions and sums their Profit field.
            //     Useful for: portfolio monitoring, drawdown control, profit targets.
            //     Returns: Total profit/loss in account currency (negative = loss).
            //     Use this for real-time P&L tracking in scalping strategies.
            // ══════════════════════════════════════════════════════════════
            Console.WriteLine("  → GetTotalProfitLossAsync()...");
            var totalPnL = await svc.GetTotalProfitLossAsync(symbol);
            Console.WriteLine($"    ✓ Total P&L: {totalPnL:F2}\n");

            // ══════════════════════════════════════════════════════════════
            // [12] ModifySlTpAsync - Breakeven Strategy
            //     Modifies Stop Loss and Take Profit of existing position by ticket.
            //     Common scalping technique: move SL to entry (breakeven) when in profit.
            //     Parameters:
            //       - ticket: Position ticket number
            //       - slPrice: New stop loss price (use entry price for breakeven)
            //       - tpPrice: New take profit price
            //     Returns: OrderResult with return code (10009 = success).
            //     Important: Broker may reject if new SL/TP too close to market.
            //     Use GetPointAsync + GetDigitsAsync for proper price precision.
            // ══════════════════════════════════════════════════════════════
            if (buyTicket.HasValue)
            {
                Console.WriteLine($"  → ModifySlTpAsync() - Moving BUY to breakeven...");
                try
                {
                    var point = await svc.GetPointAsync(symbol);
                    var currentSnapshot = await svc.GetSymbolSnapshot(symbol);

                    // Set SL 30 points below entry for safety (not exactly at breakeven)
                    var newSl = currentSnapshot.Tick.Ask - (30 * point); // 30 points below entry
                    var newTp = currentSnapshot.Tick.Ask + (tpPoints * point);

                    var modifyResult = await svc.ModifySlTpAsync(
                        ticket: buyTicket.Value,
                        slPrice: newSl,
                        tpPrice: newTp
                    );

                    Console.WriteLine($"    ✓ Modified BUY: {modifyResult.ReturnedCode}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"    ✗ Modify failed: {ex.Message}\n");
                }
            }

            if (sellTicket.HasValue)
            {
                Console.WriteLine($"  → ModifySlTpAsync() - Moving SELL to breakeven...");
                try
                {
                    var point = await svc.GetPointAsync(symbol);
                    var currentSnapshot = await svc.GetSymbolSnapshot(symbol);

                    // Set SL 30 points above entry for safety (not exactly at breakeven)
                    var newSl = currentSnapshot.Tick.Bid + (30 * point); // 30 points above entry
                    var newTp = currentSnapshot.Tick.Bid - (tpPoints * point);

                    var modifyResult = await svc.ModifySlTpAsync(
                        ticket: sellTicket.Value,
                        slPrice: newSl,
                        tpPrice: newTp
                    );

                    Console.WriteLine($"    ✓ Modified SELL: {modifyResult.ReturnedCode}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"    ✗ Modify failed: {ex.Message}\n");
                }
            }

            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("STEP 6: CLOSE POSITIONS");
            // ══════════════════════════════════════════════════════════════

            await Task.Delay(1000); // Wait 1 second

            // ══════════════════════════════════════════════════════════════
            // [13] CloseByTicket
            //     Closes specific position by ticket number with specified volume.
            //     Parameters:
            //       - ticket: Position ticket to close
            //       - volume: Volume to close (use full position volume for complete close)
            //     Process:
            //       1. Queries position by ticket to get current state
            //       2. Determines opposite operation (BUY→SELL, SELL→BUY)
            //       3. Gets current market price (Bid for BUY, Ask for SELL)
            //       4. Sends closing order via OrderSendAsync
            //     Returns: OrderCloseResult with ReturnedCode and CloseMode.
            //     CloseMode indicates: normal close, close-by, or partial close.
            //     Use for: manual exit, stop-out prevention, profit taking.
            // ══════════════════════════════════════════════════════════════
            if (buyTicket.HasValue)
            {
                Console.WriteLine($"  → CloseByTicket({buyTicket.Value})...");
                try
                {
                    var closeResult = await svc.CloseByTicket(
                        ticket: buyTicket.Value,
                        volume: tradeVolume
                    );

                    Console.WriteLine($"    ✓ Closed BUY: {closeResult.ReturnedCode}");
                    Console.WriteLine($"    ✓ Mode:       {closeResult.CloseMode}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"    ✗ Close failed: {ex.Message}\n");
                }
            }

            // ══════════════════════════════════════════════════════════════
            // [14] CloseByTicket (SELL position)
            //     Same as [13] but closing SELL position.
            //     SELL positions close at Ask price (opposite of entry at Bid).
            //     Important: Slippage may occur between close request and execution.
            //     For bulk closing, use CloseAllPositions() instead of individual closes.
            // ══════════════════════════════════════════════════════════════
            if (sellTicket.HasValue)
            {
                Console.WriteLine($"  → CloseByTicket({sellTicket.Value})...");
                try
                {
                    var closeResult = await svc.CloseByTicket(
                        ticket: sellTicket.Value,
                        volume: tradeVolume
                    );

                    Console.WriteLine($"    ✓ Closed SELL: {closeResult.ReturnedCode}");
                    Console.WriteLine($"    ✓ Mode:        {closeResult.CloseMode}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"    ✗ Close failed: {ex.Message}\n");
                }
            }

            // ══════════════════════════════════════════════════════════════
            ConsoleHelper.PrintSection("STEP 7: FINAL SUMMARY");
            // ══════════════════════════════════════════════════════════════

            Console.WriteLine("  Scalping strategy execution completed:");
            Console.WriteLine($"    • Opened 2 positions (BUY + SELL)");
            Console.WriteLine($"    • Risk per trade: ${riskMoney}");
            Console.WriteLine($"    • Stop loss: {stopPoints} points");
            Console.WriteLine($"    • Modified to breakeven");
            Console.WriteLine($"    • Closed both positions\n");

            Console.WriteLine("  Sugar methods used:");
            Console.WriteLine("    ✓ Risk-based sizing (CalcVolumeForRiskAsync)");
            Console.WriteLine("    ✓ Margin validation (CheckMarginAvailabilityAsync)");
            Console.WriteLine("    ✓ Market by risk (BuyMarketByRisk, SellMarketByRisk)");
            Console.WriteLine("    ✓ Position management (ModifySlTpAsync, CloseByTicket)");
            Console.WriteLine("    ✓ P&L monitoring (GetTotalProfitLossAsync)\n");
        }

        private static void PrintBanner()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║          🍬 SUGAR API: SCALPING STRATEGY DEMO                    ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║  Complete scalping scenario with risk management:                ║");
            Console.WriteLine("║   • Risk-based position sizing                                   ║");
            Console.WriteLine("║   • Margin validation before trading                             ║");
            Console.WriteLine("║   • Market orders with auto lot calculation                      ║");
            Console.WriteLine("║   • Position modification (breakeven)                            ║");
            Console.WriteLine("║   • Real-time P&L monitoring                                     ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("║  Risk: $5 per trade | Stop: 50 points | TP: 100 points           ║");
            Console.WriteLine("║  Positions: 2 (BUY + SELL) | Total risk: $10                     ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }
    }
}

