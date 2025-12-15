// ══════════════════════════════════════════════════════════════════════════════
// FILE: 14_MarketDepth_Explained.cs
// PURPOSE: Market Depth (DOM) - Understanding order book and liquidity
//
// Topics covered:
//   1. WHAT is Market Depth (Depth of Market - DOM)
//   2. HOW to subscribe to market book updates
//   3. HOW to read bid and ask levels
//   4. HOW to calculate liquidity at price level
//   5. HOW to get best bid/ask from order book
//   6. WHEN to use DOM vs regular tick data
//
// Key principle: Market Depth shows ORDER BOOK - all pending buy/sell orders
// from other traders. Useful for large orders and understanding liquidity.
//
// ⚠️  Note: Not all brokers provide DOM data!
// Check if your broker supports market depth before using these methods.
//
// ══════════════════════════════════════════════════════════════════════════════

using System;
using System.Linq;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5;
using MetaRPC.CSharpMT5.Examples.Helpers;
using mt5_term_api;

namespace MetaRPC.CSharpMT5.Examples.MoreExamples;

// Declare public static class
public static class MarketDepthExamples
{
    // Define async method
    public static async Task RunAsync()
    {
        // ═══════════════════════════════════════════════════════════════════════
        // STEP 1: DISPLAY HEADER
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   MARKET DEPTH (DOM) - Order Book and Liquidity           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

        // ═══════════════════════════════════════════════════════════════════════
        // STEP 2: ESTABLISH CONNECTION
        // ═══════════════════════════════════════════════════════════════════════

        // Load configuration from appsettings.json
        var config = ConnectionHelper.BuildConfiguration();
        Console.WriteLine("✓ Configuration loaded");

        // Connect to MT5 Terminal
        var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);
        Console.WriteLine("✓ Connected to MT5 Terminal\n");

        // Create MT5Service wrapper
        var service = new MT5Service(account);

        // Create MT5Sugar convenience API
        var sugar = new MT5Sugar(service);

        // Define symbol for examples
        string symbol = "EURUSD";

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 1: WHAT IS MARKET DEPTH?
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 1: Understanding Market Depth (Order Book)");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 WHAT IS MARKET DEPTH (DOM)?");
        Console.WriteLine("   The ORDER BOOK showing all pending buy and sell orders");
        Console.WriteLine("   from other market participants at different price levels.\n");

        Console.WriteLine("📊 REGULAR TICK DATA vs MARKET DEPTH:\n");

        Console.WriteLine("   REGULAR TICK (what you normally use):");
        Console.WriteLine("   - Shows: Current Bid and Ask prices");
        Console.WriteLine("   - Example: Bid 1.10000, Ask 1.10002");
        Console.WriteLine("   - Tells you: Where you can buy/sell RIGHT NOW");
        Console.WriteLine("   - Good for: Normal trading\n");

        Console.WriteLine("   MARKET DEPTH (order book):");
        Console.WriteLine("   - Shows: ALL price levels with pending orders");
        Console.WriteLine("   - Example: 10 levels below and above current price");
        Console.WriteLine("   - Tells you: How much volume available at each level");
        Console.WriteLine("   - Good for: Large orders, liquidity analysis\n");

        Console.WriteLine("📈 EXAMPLE ORDER BOOK:");
        Console.WriteLine("   ┌─────────────┬──────────┬─────────────┬──────────┐");
        Console.WriteLine("   │  SELL Orders (Ask side) │ BUY Orders (Bid side)  │");
        Console.WriteLine("   ├─────────────┼──────────┼─────────────┼──────────┤");
        Console.WriteLine("   │  Price      │  Volume  │  Price      │  Volume  │");
        Console.WriteLine("   ├─────────────┼──────────┼─────────────┼──────────┤");
        Console.WriteLine("   │  1.10010    │   2.5    │             │          │");
        Console.WriteLine("   │  1.10008    │   5.0    │             │          │");
        Console.WriteLine("   │  1.10005    │  10.0    │             │          │");
        Console.WriteLine("   │  1.10002 ←──┼───  (ASK - where you BUY)         │");
        Console.WriteLine("   │  1.10000 ←──┼───  (BID - where you SELL)        │");
        Console.WriteLine("   │             │          │  1.09998    │   8.0    │");
        Console.WriteLine("   │             │          │  1.09995    │  12.0    │");
        Console.WriteLine("   │             │          │  1.09990    │   3.5    │");
        Console.WriteLine("   └─────────────┴──────────┴─────────────┴──────────┘\n");

        Console.WriteLine("💡 INTERPRETATION:");
        Console.WriteLine("   - At 1.10002: 2.5 lots available to BUY");
        Console.WriteLine("   - At 1.09998: 8.0 lots available to SELL");
        Console.WriteLine("   - Large volume at 1.10005 = resistance level");
        Console.WriteLine("   - Large volume at 1.09995 = support level\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 2: SUBSCRIBING TO MARKET DEPTH
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 2: How to Subscribe to Market Depth");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("⚠️  IMPORTANT: Check if broker supports DOM");
        Console.WriteLine("   Not all brokers provide market depth data!\n");

        Console.WriteLine("📞 SUBSCRIPTION PROCESS:\n");

        Console.WriteLine("1. ADD SYMBOL TO MARKET WATCH:");
        Console.WriteLine("   await account.MarketBookAddAsync(symbol);\n");

        Console.WriteLine("2. GET MARKET BOOK DATA:");
        Console.WriteLine("   var book = await account.MarketBookGetAsync(symbol);\n");

        Console.WriteLine("3. WHEN DONE, RELEASE:");
        Console.WriteLine("   await account.MarketBookReleaseAsync(symbol);\n");

        Console.WriteLine("✅ ATTEMPTING TO SUBSCRIBE:");

        try
        {
            // Step 1: Add symbol to market book
            Console.WriteLine($"\n📞 Calling: account.MarketBookAddAsync(\"{symbol}\")");
            var addResult = await account.MarketBookAddAsync(new SymbolRequest { Symbol = symbol });

            if (addResult.ReturnedCode == 1)  // 1 = success for MarketBook
            {
                Console.WriteLine($"   ✓ Subscribed to {symbol} market depth\n");

                // Step 2: Get market book snapshot
                Console.WriteLine($"📞 Calling: account.MarketBookGetAsync(\"{symbol}\")");
                var bookData = await account.MarketBookGetAsync(new SymbolRequest { Symbol = symbol });

                if (bookData.MarketBook != null && bookData.MarketBook.Count > 0)
                {
                    Console.WriteLine($"   ✓ Received {bookData.MarketBook.Count} price levels\n");

                    Console.WriteLine("📊 MARKET DEPTH SNAPSHOT:\n");
                    Console.WriteLine("   ┌─────────────┬──────────┬──────────┬─────────────┐");
                    Console.WriteLine("   │  Price      │  Volume  │  Type    │  Direction  │");
                    Console.WriteLine("   ├─────────────┼──────────┼──────────┼─────────────┤");

                    foreach (var level in bookData.MarketBook.Take(10))  // Show first 10 levels
                    {
                        string type = level.Type == 1 ? "SELL" : "BUY ";
                        string direction = level.Type == 1 ? "Ask side" : "Bid side";
                        Console.WriteLine($"   │  {level.Price:F5}  │  {level.VolumeReal,6:F2}  │  {type}    │  {direction}  │");
                    }

                    Console.WriteLine("   └─────────────┴──────────┴──────────┴─────────────┘\n");

                    // Calculate total liquidity
                    double totalBuyVolume = bookData.MarketBook.Where(b => b.Type == 2).Sum(b => b.VolumeReal);
                    double totalSellVolume = bookData.MarketBook.Where(b => b.Type == 1).Sum(b => b.VolumeReal);

                    Console.WriteLine($"📊 LIQUIDITY SUMMARY:");
                    Console.WriteLine($"   Total BUY orders (Bid side): {totalBuyVolume:F2} lots");
                    Console.WriteLine($"   Total SELL orders (Ask side): {totalSellVolume:F2} lots");
                    Console.WriteLine($"   Bid/Ask volume ratio: {(totalBuyVolume / totalSellVolume):F2}\n");

                    // Step 3: Release subscription
                    Console.WriteLine($"📞 Calling: account.MarketBookReleaseAsync(\"{symbol}\")");
                    await account.MarketBookReleaseAsync(new SymbolRequest { Symbol = symbol });
                    Console.WriteLine($"   ✓ Released {symbol} market depth subscription\n");
                }
                else
                {
                    Console.WriteLine($"   ⚠️  No market depth data available");
                    Console.WriteLine($"   Your broker may not support DOM for {symbol}\n");
                }
            }
            else
            {
                Console.WriteLine($"   ✗ Failed to subscribe: ReturnCode {addResult.ReturnedCode}");
                Console.WriteLine($"   Your broker may not support market depth\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n⚠️  MARKET DEPTH NOT AVAILABLE:");
            Console.WriteLine($"   Error: {ex.Message}");
            Console.WriteLine($"   Your broker does not support DOM data\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 3: USING MT5SUGAR DOM HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 3: MT5Sugar Market Depth Helpers");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 MT5SUGAR PROVIDES CONVENIENT METHODS:\n");

        Console.WriteLine("📚 AVAILABLE METHODS:");
        Console.WriteLine("   - GetMarketBookSnapshotAsync(symbol) - Get order book");
        Console.WriteLine("   - GetBestBidAskFromBookAsync(symbol) - Best prices from DOM");
        Console.WriteLine("   - CalculateLiquidityAtLevelAsync(symbol, price) - Volume at level");
        Console.WriteLine("   - SubscribeToMarketBookAsync(symbol, callback) - Real-time updates\n");

        Console.WriteLine("✅ METHOD 1: Get Order Book Snapshot\n");

        try
        {
            Console.WriteLine($"📞 Calling: sugar.GetMarketBookSnapshotAsync(\"{symbol}\")");
            var snapshot = await sugar.GetMarketBookSnapshotAsync(symbol);

            if (snapshot != null && snapshot.Count > 0)
            {
                Console.WriteLine($"   ✓ Retrieved {snapshot.Count} levels\n");

                var bidLevels = snapshot.Where(b => b.Type == 2).OrderByDescending(b => b.Price).Take(5);
                var askLevels = snapshot.Where(b => b.Type == 1).OrderBy(b => b.Price).Take(5);

                Console.WriteLine("   Top 5 BID levels (buyers):");
                foreach (var bid in bidLevels)
                {
                    Console.WriteLine($"      {bid.Price:F5} - {bid.VolumeReal:F2} lots");
                }

                Console.WriteLine("\n   Top 5 ASK levels (sellers):");
                foreach (var ask in askLevels)
                {
                    Console.WriteLine($"      {ask.Price:F5} - {ask.VolumeReal:F2} lots");
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine($"   No data available (broker doesn't support DOM)\n");
            }
        }
        catch
        {
            Console.WriteLine($"   Broker doesn't support market depth\n");
        }

        Console.WriteLine("✅ METHOD 2: Get Best Bid/Ask from Order Book\n");

        try
        {
            Console.WriteLine($"📞 Calling: sugar.GetBestBidAskFromBookAsync(\"{symbol}\")");
            var (bestBid, bestAsk) = await sugar.GetBestBidAskFromBookAsync(symbol);

            Console.WriteLine($"   Best BID: {bestBid:F5} (highest buy order)");
            Console.WriteLine($"   Best ASK: {bestAsk:F5} (lowest sell order)");
            Console.WriteLine($"   Spread: {(bestAsk - bestBid) / await sugar.GetPointAsync(symbol):F1} points\n");
        }
        catch
        {
            Console.WriteLine($"   Broker doesn't support market depth\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 4: CALCULATING LIQUIDITY AT PRICE LEVEL
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 4: Analyzing Liquidity at Specific Price");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 WHY CHECK LIQUIDITY?");
        Console.WriteLine("   Large orders need sufficient liquidity to execute");
        Console.WriteLine("   without significant slippage.\n");

        Console.WriteLine("📊 SCENARIO:");
        Console.WriteLine("   You want to buy 10 lots EURUSD");
        Console.WriteLine("   Need to check if enough sell orders available\n");

        try
        {
            var tick = await service.SymbolInfoTickAsync(symbol);
            double targetPrice = tick.Ask;

            Console.WriteLine($"📞 Calling: sugar.CalculateLiquidityAtLevelAsync()");
            Console.WriteLine($"   Target price: {targetPrice:F5}");
            Console.WriteLine($"   Looking for: SELL orders (you want to BUY)\n");

            double liquidity = await sugar.CalculateLiquidityAtLevelAsync(symbol, targetPrice);

            Console.WriteLine($"📊 RESULT:");
            Console.WriteLine($"   Available liquidity: {liquidity:F2} lots");

            double desiredVolume = 10.0;
            Console.WriteLine($"   Desired volume: {desiredVolume} lots\n");

            if (liquidity >= desiredVolume)
            {
                Console.WriteLine($"✅ SUFFICIENT LIQUIDITY:");
                Console.WriteLine($"   {liquidity:F2} lots available ≥ {desiredVolume} lots needed");
                Console.WriteLine($"   Your order can execute with minimal slippage\n");
            }
            else if (liquidity > 0)
            {
                Console.WriteLine($"⚠️  INSUFFICIENT LIQUIDITY:");
                Console.WriteLine($"   {liquidity:F2} lots available < {desiredVolume} lots needed");
                Console.WriteLine($"   Shortage: {desiredVolume - liquidity:F2} lots");
                Console.WriteLine($"   Risk: Price slippage when order executes\n");

                Console.WriteLine($"🔧 SOLUTIONS:");
                Console.WriteLine($"   1. Split into smaller orders");
                Console.WriteLine($"   2. Use iceberg order (hide size)");
                Console.WriteLine($"   3. Wait for more liquidity");
                Console.WriteLine($"   4. Accept slippage risk\n");
            }
            else
            {
                Console.WriteLine($"⚠️  NO LIQUIDITY DATA:");
                Console.WriteLine($"   Broker doesn't provide DOM for this symbol\n");
            }
        }
        catch
        {
            Console.WriteLine($"   Broker doesn't support liquidity analysis\n");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 5: REAL-TIME MARKET BOOK MONITORING
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 5: Real-Time Market Depth Monitoring");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("💡 REAL-TIME SUBSCRIPTION:");
        Console.WriteLine("   Monitor order book changes as they happen\n");

        Console.WriteLine("✅ IMPLEMENTATION PATTERN:");
        Console.WriteLine(@"
   using System.Threading;

   var cts = new CancellationTokenSource();

   // Callback for market book updates
   void OnMarketBookUpdate(List<BookInfo> book)
   {
       Console.WriteLine($""Order book updated: {book.Count} levels"");

       // Find best bid and ask
       var bestBid = book.Where(b => b.Type == 2)
                        .OrderByDescending(b => b.Price)
                        .FirstOrDefault();
       var bestAsk = book.Where(b => b.Type == 1)
                        .OrderBy(b => b.Price)
                        .FirstOrDefault();

       Console.WriteLine($""Best BID: {bestBid?.Price:F5} ({bestBid?.VolumeReal:F2} lots)"");
       Console.WriteLine($""Best ASK: {bestAsk?.Price:F5} ({bestAsk?.VolumeReal:F2} lots)"");
       Console.WriteLine();
   }

   // Subscribe
   await sugar.SubscribeToMarketBookAsync(symbol, OnMarketBookUpdate);

   // Monitor for 30 seconds
   await Task.Delay(30000, cts.Token);

   // Unsubscribe
   await account.MarketBookReleaseAsync(new SymbolRequest { Symbol = symbol });
");

        Console.WriteLine("⚠️  NOT EXECUTING (demonstration only)");
        Console.WriteLine("   Use this pattern in your own monitoring loops\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 6: WHEN TO USE MARKET DEPTH
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 6: When to Use Market Depth vs Regular Ticks");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("✅ USE MARKET DEPTH (DOM) WHEN:\n");

        Console.WriteLine("   1. LARGE ORDERS (> 1 lot):");
        Console.WriteLine("      - Check if enough liquidity available");
        Console.WriteLine("      - Avoid price slippage");
        Console.WriteLine("      - Example: Buying 50 lots EURUSD\n");

        Console.WriteLine("   2. SUPPORT/RESISTANCE ANALYSIS:");
        Console.WriteLine("      - Large volume clusters = strong levels");
        Console.WriteLine("      - Example: 100 lots at 1.10000 = strong support\n");

        Console.WriteLine("   3. ORDER FLOW TRADING:");
        Console.WriteLine("      - Watch buyer/seller balance");
        Console.WriteLine("      - Imbalance indicates potential move");
        Console.WriteLine("      - Example: More buyers than sellers = bullish\n");

        Console.WriteLine("   4. ICEBERG ORDER DETECTION:");
        Console.WriteLine("      - Large hidden orders show in book gradually");
        Console.WriteLine("      - Repeated fills at same level = iceberg\n");

        Console.WriteLine("   5. SCALPING TIGHT SPREADS:");
        Console.WriteLine("      - Place orders between bid/ask");
        Console.WriteLine("      - Act as market maker");
        Console.WriteLine("      - Requires DOM data\n");

        Console.WriteLine("❌ USE REGULAR TICKS WHEN:\n");

        Console.WriteLine("   1. SMALL ORDERS (< 1 lot):");
        Console.WriteLine("      - Liquidity not an issue");
        Console.WriteLine("      - Regular tick data sufficient\n");

        Console.WriteLine("   2. TREND FOLLOWING:");
        Console.WriteLine("      - Don't care about exact entry");
        Console.WriteLine("      - Direction matters more than price\n");

        Console.WriteLine("   3. BROKER DOESN'T SUPPORT DOM:");
        Console.WriteLine("      - Many retail brokers don't provide it");
        Console.WriteLine("      - Use what's available\n");

        Console.WriteLine("   4. LONG TIMEFRAMES:");
        Console.WriteLine("      - Daily/Weekly trading");
        Console.WriteLine("      - DOM changes too fast to matter\n");

        // ═══════════════════════════════════════════════════════════════════════
        // EXAMPLE 7: DOM DATA STRUCTURE
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("EXAMPLE 7: Understanding DOM Data Structure");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("📊 BOOKINFO STRUCTURE (each price level):\n");

        Console.WriteLine("   Properties:");
        Console.WriteLine("   - Type: 1 = SELL order (Ask), 2 = BUY order (Bid)");
        Console.WriteLine("   - Price: Price level (e.g., 1.10000)");
        Console.WriteLine("   - Volume: Volume in lots (e.g., 5.0)");
        Console.WriteLine("   - VolumeReal: Same as Volume (double precision)\n");

        Console.WriteLine("💡 SORTING:");
        Console.WriteLine("   BID (buy orders): Sorted from highest to lowest price");
        Console.WriteLine("   ASK (sell orders): Sorted from lowest to highest price\n");

        Console.WriteLine("✅ EXAMPLE CODE:");
        Console.WriteLine(@"
   var book = await sugar.GetMarketBookSnapshotAsync(symbol);

   // Get top 5 bids (highest buy prices)
   var topBids = book.Where(b => b.Type == 2)
                    .OrderByDescending(b => b.Price)
                    .Take(5);

   // Get top 5 asks (lowest sell prices)
   var topAsks = book.Where(b => b.Type == 1)
                    .OrderBy(b => b.Price)
                    .Take(5);

   // Calculate spread from DOM
   double bestBid = topBids.First().Price;
   double bestAsk = topAsks.First().Price;
   double spread = bestAsk - bestBid;

   // Find support/resistance
   var strongSupport = topBids.OrderByDescending(b => b.VolumeReal).First();
   Console.WriteLine($""Strong support at {strongSupport.Price:F5}"");
   Console.WriteLine($""Volume: {strongSupport.VolumeReal:F2} lots"");
");

        // ═══════════════════════════════════════════════════════════════════════
        // SUMMARY
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("SUMMARY - Market Depth Essentials");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Console.WriteLine("📋 WHAT IS DOM:");
        Console.WriteLine("   - Order book showing all pending buy/sell orders");
        Console.WriteLine("   - Multiple price levels above and below current price");
        Console.WriteLine("   - Shows volume available at each level");
        Console.WriteLine("   - NOT available from all brokers\n");

        Console.WriteLine("📚 KEY METHODS:\n");

        Console.WriteLine("LOW-LEVEL (MT5Account):");
        Console.WriteLine("   - MarketBookAddAsync(symbol) - Subscribe");
        Console.WriteLine("   - MarketBookGetAsync(symbol) - Get snapshot");
        Console.WriteLine("   - MarketBookReleaseAsync(symbol) - Unsubscribe\n");

        Console.WriteLine("HIGH-LEVEL (MT5Sugar):");
        Console.WriteLine("   - GetMarketBookSnapshotAsync(symbol)");
        Console.WriteLine("   - GetBestBidAskFromBookAsync(symbol)");
        Console.WriteLine("   - CalculateLiquidityAtLevelAsync(symbol, price)");
        Console.WriteLine("   - SubscribeToMarketBookAsync(symbol, callback)\n");

        Console.WriteLine("💡 USE CASES:");
        Console.WriteLine("   ✅ Large orders (check liquidity)");
        Console.WriteLine("   ✅ Support/resistance analysis");
        Console.WriteLine("   ✅ Order flow trading");
        Console.WriteLine("   ✅ Market making / scalping");
        Console.WriteLine("   ❌ Small retail orders (overkill)");
        Console.WriteLine("   ❌ Long-term trading (too granular)\n");

        Console.WriteLine("⚠️  LIMITATIONS:");
        Console.WriteLine("   - Not all brokers provide DOM data");
        Console.WriteLine("   - ECN brokers more likely to have it");
        Console.WriteLine("   - Retail brokers often don't support it");
        Console.WriteLine("   - Data may be delayed or aggregated");
        Console.WriteLine("   - Check broker specs before relying on DOM\n");

        Console.WriteLine("🔧 BEST PRACTICES:");
        Console.WriteLine("   1. Always check if MarketBookAddAsync succeeds");
        Console.WriteLine("   2. Release subscription when done (save resources)");
        Console.WriteLine("   3. Handle missing DOM gracefully (fallback to ticks)");
        Console.WriteLine("   4. Use for large orders only (> 1 lot)");
        Console.WriteLine("   5. Monitor liquidity imbalance for trade signals\n");

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Market Depth = See the full picture of liquidity!       ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
    }
}
