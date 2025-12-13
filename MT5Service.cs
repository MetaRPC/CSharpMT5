/*══════════════════════════════════════════════════════════════════════════════
 FILE: MT5Service.cs — MID-LEVEL API WRAPPER FOR MT5
══════════════════════════════════════════════════════════════════════════════

 PURPOSE:
   Service layer wrapper over MT5Account (low-level gRPC client).
   Provides cleaner, more ergonomic API by:
   • Unwrapping primitive return types (double, long, string, bool, int)
   • Adding convenience methods (GetBalance, BuyMarket, GetBid, etc.)
   • Keeping the same power as low-level, but with less boilerplate

 ARCHITECTURE:
   MT5Account (Low-Level)  →  Direct gRPC calls, returns Data wrappers
        ↓
   MT5Service (Mid-Level)  →  Unwraps primitives + convenience shortcuts
        ↓
   MT5Sugar (High-Level)   →  Complex business logic, smart helpers

 WHAT CHANGED:
   ✓ Unwraps simple values from Data objects (no .Value, .Exists needed)
   ✓ Added ~30 convenience methods for common operations
   ✓ Delegates all calls to underlying MT5Account instance
   ✓ Returns primitives where possible, rich objects where needed

──────────────────────────────────────────────────────────────────────────────
 📋 FULL METHOD LIST
──────────────────────────────────────────────────────────────────────────────

 [01] CONNECTION
   • ConnectByHostPortAsync     — Thin wrapper (no changes)
   • ConnectByServerNameAsync   — Thin wrapper (no changes)

 [02] ACCOUNT INFORMATION
   ✓ AccountSummaryAsync        — Thin wrapper (returns rich object)
   ✓ AccountInfoDoubleAsync     — UNWRAPPED: returns double (was Data.Value)
   ✓ AccountInfoIntegerAsync    — UNWRAPPED: returns long (was Data.Value)
   ✓ AccountInfoStringAsync     — UNWRAPPED: returns string (was Data.Value)

   NEW CONVENIENCE METHODS:
   • GetBalanceAsync            — Shortcut for AccountInfoDouble(Balance)
   • GetEquityAsync             — Shortcut for AccountInfoDouble(Equity)
   • GetMarginAsync             — Shortcut for AccountInfoDouble(Margin)
   • GetFreeMarginAsync         — Shortcut for AccountInfoDouble(MarginFree)
   • GetProfitAsync             — Shortcut for AccountInfoDouble(Profit)
   • GetLoginAsync              — Shortcut for AccountInfoInteger(Login)
   • GetLeverageAsync           — Shortcut for AccountInfoInteger(Leverage)
   • GetAccountNameAsync        — Shortcut for AccountInfoString(Name)
   • GetServerNameAsync         — Shortcut for AccountInfoString(Server)
   • GetCurrencyAsync           — Shortcut for AccountInfoString(Currency)

 [03] SYMBOL OPERATIONS
   ✓ SymbolsTotalAsync          — UNWRAPPED: returns int (was Data.Total)
   ✓ SymbolExistAsync           — UNWRAPPED: returns bool (was Data.Exists)
   ✓ SymbolNameAsync            — UNWRAPPED: returns string (was Data.Name)
   ✓ SymbolSelectAsync          — UNWRAPPED: returns bool (was Data.Success)
   ✓ SymbolIsSynchronizedAsync  — UNWRAPPED: returns bool (was Data.Synchronized)
   ✓ SymbolInfoDoubleAsync      — UNWRAPPED: returns double (was Data.Value)
   ✓ SymbolInfoIntegerAsync     — UNWRAPPED: returns long (was Data.Value)
   ✓ SymbolInfoStringAsync      — UNWRAPPED: returns string (was Data.Value)
   • SymbolInfoMarginRateAsync  — Thin wrapper (returns rich object)

   NEW CONVENIENCE METHODS:
   • GetBidAsync                — Shortcut for SymbolInfoDouble(Bid)
   • GetAskAsync                — Shortcut for SymbolInfoDouble(Ask)
   • GetSpreadAsync             — Shortcut for SymbolInfoInteger(Spread)
   • GetVolumeMinAsync          — Shortcut for SymbolInfoDouble(VolumeMin)
   • GetVolumeMaxAsync          — Shortcut for SymbolInfoDouble(VolumeMax)
   • GetVolumeStepAsync         — Shortcut for SymbolInfoDouble(VolumeStep)
   • IsSymbolAvailableAsync     — Combines Exist + IsSynchronized checks

 [04] SYMBOL INFO
   • SymbolInfoTickAsync        — Thin wrapper (returns MrpcMqlTick)
   • QuoteAsync                 — Alias for SymbolInfoTickAsync
   • SymbolInfoSessionQuoteAsync — Thin wrapper (returns rich object)
   • SymbolInfoSessionTradeAsync — Thin wrapper (returns rich object)
   • TickValueWithSizeAsync     — Thin wrapper (returns rich object)
   • SymbolParamsManyAsync      — Thin wrapper (returns rich object)

 [05] MARKET DEPTH (DOM)
   • MarketBookAddAsync         — Thin wrapper (returns rich object)
   • MarketBookReleaseAsync     — Thin wrapper (returns rich object)
   • MarketBookGetAsync         — Thin wrapper (returns rich object)

 [06] ORDERS / POSITIONS / HISTORY
   • OpenedOrdersAsync          — Thin wrapper (returns rich object)
   • OpenedOrdersTicketsAsync   — Thin wrapper (returns rich object)
   • OrderHistoryAsync          — Thin wrapper (returns rich object)
   • PositionsHistoryAsync      — Thin wrapper (returns rich object)
   • PositionsTotalAsync        — Thin wrapper (returns rich object)

 [07] PRE-TRADE & TRADING
   • OrderCalcMarginAsync       — Thin wrapper (returns rich object)
   • OrderCheckAsync            — Thin wrapper (returns rich object)
   • OrderSendAsync             — Thin wrapper (returns rich object)
   • OrderModifyAsync           — Thin wrapper (returns rich object)
   • OrderCloseAsync            — Thin wrapper (returns rich object)

   NEW CONVENIENCE METHODS:
   • BuyMarketAsync             — Simplified market BUY (builds OrderSendRequest)
   • SellMarketAsync            — Simplified market SELL (builds OrderSendRequest)
   • BuyLimitAsync              — Simplified pending BUY LIMIT
   • SellLimitAsync             — Simplified pending SELL LIMIT
   • BuyStopAsync               — Simplified pending BUY STOP
   • SellStopAsync              — Simplified pending SELL STOP
   • GetRecentOrdersAsync       — Get history for last N days
   • GetTodayOrdersAsync        — Get today's order history
   • IsTradingAllowedAsync      — Check if trading is enabled

 [08] STREAMING
   • OnSymbolTickAsync          — Thin wrapper (IAsyncEnumerable)
   • OnTradeAsync               — Thin wrapper (IAsyncEnumerable)
   • OnPositionProfitAsync      — Thin wrapper (IAsyncEnumerable)
   • OnPositionsAndPendingOrdersTicketsAsync — Thin wrapper (IAsyncEnumerable)
   • OnTradeTransactionAsync    — Thin wrapper (IAsyncEnumerable)

──────────────────────────────────────────────────────────────────────────────
 USAGE EXAMPLES:
──────────────────────────────────────────────────────────────────────────────

   var service = new MT5Service(account);

   // BEFORE (Low-Level):
   var data = await account.AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountBalance);
   double balance = data.Value;  // Need to unwrap!

   // AFTER (Service):
   double balance = await service.GetBalanceAsync();  // Direct value!

   // Trading made simple:
   var result = await service.BuyMarketAsync("EURUSD", 0.01,
       stopLoss: 1.0800, takeProfit: 1.0900);

   // Symbol checks:
   bool canTrade = await service.IsSymbolAvailableAsync("EURUSD");
   if (canTrade && await service.IsTradingAllowedAsync()) {
       // Place order...
   }

══════════════════════════════════════════════════════════════════════════════*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using mt5_term_api;

namespace MetaRPC.CSharpMT5
{

    public class MT5Service
    {
        private readonly MT5Account _acc;
        public MT5Service(MT5Account account) => _acc = account;

        // ─────────────────────────────────────────────────────────────
        #region [01] CONNECTION
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Establishes connection to MT5 terminal via gRPC using direct host and port.
        /// </summary>
        /// <param name="host">gRPC server hostname or IP address (e.g., "mt5.mrpc.pro").</param>
        /// <param name="port">gRPC server port number (default: 443 for HTTPS).</param>
        /// <param name="baseChartSymbol">Symbol to use for initial chart/market watch (default: "EURUSD").</param>
        /// <param name="waitForTerminalIsAlive">If true, waits for terminal to be ready before returning (default: true).</param>
        /// <param name="timeoutSeconds">Connection timeout in seconds (default: 30).</param>
        /// <param name="deadline">Optional RPC deadline for this call.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Task that completes when connection is established.</returns>
        /// <exception cref="ConnectExceptionMT5">Thrown when connection fails or times out.</exception>
        public Task ConnectByHostPortAsync(
            string host,
            int port = 443,
            string baseChartSymbol = "EURUSD",
            bool waitForTerminalIsAlive = true,
            int timeoutSeconds = 30,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.ConnectByHostPortAsync(host, port, baseChartSymbol, waitForTerminalIsAlive, timeoutSeconds, deadline, cancellationToken);

        /// <summary>
        /// Establishes connection to MT5 terminal via gRPC using server name lookup.
        /// </summary>
        /// <param name="serverName">MT5 server name (e.g., "FxPro-MT5 Demo") - will be resolved to host:port.</param>
        /// <param name="baseChartSymbol">Symbol to use for initial chart/market watch (default: "EURUSD").</param>
        /// <param name="waitForTerminalIsAlive">If true, waits for terminal to be ready before returning (default: true).</param>
        /// <param name="timeoutSeconds">Connection timeout in seconds (default: 30).</param>
        /// <param name="deadline">Optional RPC deadline for this call.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Task that completes when connection is established.</returns>
        /// <exception cref="ConnectExceptionMT5">Thrown when connection fails, server name not found, or times out.</exception>
        public Task ConnectByServerNameAsync(
            string serverName,
            string baseChartSymbol = "EURUSD",
            bool waitForTerminalIsAlive = true,
            int timeoutSeconds = 30,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.ConnectByServerNameAsync(serverName, baseChartSymbol, waitForTerminalIsAlive, timeoutSeconds, deadline, cancellationToken);

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region [02] ACCOUNT INFO
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Retrieves comprehensive account summary including balance, equity, margin, and key ratios.
        /// </summary>
        /// <param name="deadline">Optional RPC deadline; if null, uses default timeout.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>
        /// <see cref="AccountSummaryData"/> containing balance, equity, margin, free margin, profit,
        /// margin level, and other account metrics.
        /// </returns>
        /// <remarks>
        /// This is a rich object that provides multiple account fields in one call.
        /// For single field access, use GetBalanceAsync(), GetEquityAsync(), etc.
        /// </remarks>
        public Task<AccountSummaryData> AccountSummaryAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.AccountSummaryAsync(deadline, cancellationToken);

        /// <summary>
        /// Retrieves a double-precision numeric account property (e.g., Balance, Equity, Margin).
        /// </summary>
        /// <param name="property">The account property type to query.</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Unwrapped double value of the requested property (no .Value needed).</returns>
        /// <remarks>
        /// UNWRAPPED: Returns primitive double directly instead of Data wrapper.
        /// Common properties: AccountBalance, AccountEquity, AccountMargin, AccountMarginFree, AccountProfit.
        /// </remarks>
        public Task<double> AccountInfoDoubleAsync(
            AccountInfoDoublePropertyType property,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.AccountInfoDoubleAsync(property, deadline, cancellationToken);

        /// <summary>
        /// Retrieves an integer account property (e.g., Login, Leverage, TradeMode).
        /// </summary>
        /// <param name="property">The account property type to query.</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Unwrapped long value of the requested property (no .Value needed).</returns>
        /// <remarks>
        /// UNWRAPPED: Returns primitive long directly instead of Data wrapper.
        /// Common properties: AccountLogin, AccountLeverage, AccountTradeAllowed, AccountTradeMode.
        /// </remarks>
        public Task<long> AccountInfoIntegerAsync(
            AccountInfoIntegerPropertyType property,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.AccountInfoIntegerAsync(property, deadline, cancellationToken);

        /// <summary>
        /// Retrieves a string account property (e.g., Name, Server, Currency).
        /// </summary>
        /// <param name="property">The account property type to query.</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Unwrapped string value of the requested property (no .Value needed).</returns>
        /// <remarks>
        /// UNWRAPPED: Returns primitive string directly instead of Data wrapper.
        /// Common properties: AccountName, AccountServer, AccountCurrency, AccountCompany.
        /// </remarks>
        public Task<string> AccountInfoStringAsync(
            AccountInfoStringPropertyType property,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.AccountInfoStringAsync(property, deadline, cancellationToken);

        // Convenience methods for common account properties

        /// <summary>
        /// Gets current account balance (total funds deposited minus withdrawals).
        /// </summary>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Account balance in account currency.</returns>
        /// <remarks>Convenience shortcut for AccountInfoDoubleAsync(AccountBalance).</remarks>
        public Task<double> GetBalanceAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountBalance, deadline, cancellationToken);

        /// <summary>
        /// Gets current account equity (balance + floating profit/loss).
        /// </summary>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Account equity in account currency.</returns>
        /// <remarks>
        /// Convenience shortcut for AccountInfoDoubleAsync(AccountEquity).
        /// Equity = Balance + Profit from open positions.
        /// </remarks>
        public Task<double> GetEquityAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountEquity, deadline, cancellationToken);

        /// <summary>
        /// Gets currently used margin for open positions.
        /// </summary>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Used margin in account currency.</returns>
        /// <remarks>Convenience shortcut for AccountInfoDoubleAsync(AccountMargin).</remarks>
        public Task<double> GetMarginAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountMargin, deadline, cancellationToken);

        /// <summary>
        /// Gets current free margin available for opening new positions.
        /// </summary>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Free margin in account currency.</returns>
        /// <remarks>
        /// Convenience shortcut for AccountInfoDoubleAsync(AccountMarginFree).
        /// Free Margin = Equity - Used Margin.
        /// </remarks>
        public Task<double> GetFreeMarginAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountMarginFree, deadline, cancellationToken);

        /// <summary>
        /// Gets current floating profit/loss from all open positions.
        /// </summary>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Total floating profit in account currency (can be negative for loss).</returns>
        /// <remarks>Convenience shortcut for AccountInfoDoubleAsync(AccountProfit).</remarks>
        public Task<double> GetProfitAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => AccountInfoDoubleAsync(AccountInfoDoublePropertyType.AccountProfit, deadline, cancellationToken);

        /// <summary>
        /// Gets the account login/identification number.
        /// </summary>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Account login number as long.</returns>
        /// <remarks>Convenience shortcut for AccountInfoIntegerAsync(AccountLogin).</remarks>
        public Task<long> GetLoginAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => AccountInfoIntegerAsync(AccountInfoIntegerPropertyType.AccountLogin, deadline, cancellationToken);

        /// <summary>
        /// Gets the account leverage ratio (e.g., 100 for 1:100, 500 for 1:500).
        /// </summary>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Leverage as long (e.g., 100, 500, 1000).</returns>
        /// <remarks>Convenience shortcut for AccountInfoIntegerAsync(AccountLeverage).</remarks>
        public Task<long> GetLeverageAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => AccountInfoIntegerAsync(AccountInfoIntegerPropertyType.AccountLeverage, deadline, cancellationToken);

        /// <summary>
        /// Gets the account owner's name.
        /// </summary>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Account holder name as string.</returns>
        /// <remarks>Convenience shortcut for AccountInfoStringAsync(AccountName).</remarks>
        public Task<string> GetAccountNameAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => AccountInfoStringAsync(AccountInfoStringPropertyType.AccountName, deadline, cancellationToken);

        /// <summary>
        /// Gets the MT5 server name this account is connected to.
        /// </summary>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Server name as string (e.g., "FxPro-MT5 Demo").</returns>
        /// <remarks>Convenience shortcut for AccountInfoStringAsync(AccountServer).</remarks>
        public Task<string> GetServerNameAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => AccountInfoStringAsync(AccountInfoStringPropertyType.AccountServer, deadline, cancellationToken);

        /// <summary>
        /// Gets the account's base currency.
        /// </summary>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Currency code as string (e.g., "USD", "EUR", "GBP").</returns>
        /// <remarks>Convenience shortcut for AccountInfoStringAsync(AccountCurrency).</remarks>
        public Task<string> GetCurrencyAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => AccountInfoStringAsync(AccountInfoStringPropertyType.AccountCurrency, deadline, cancellationToken);

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region [03] SYMBOLS
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the total number of symbols available in the terminal.
        /// </summary>
        /// <param name="selectedOnly">If true, counts only symbols selected in Market Watch; if false, counts all symbols.</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Total symbol count as int.</returns>
        /// <remarks>UNWRAPPED: Returns primitive int directly (was Data.Total).</remarks>
        public async Task<int> SymbolsTotalAsync(
            bool selectedOnly,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var data = await _acc.SymbolsTotalAsync(selectedOnly, deadline, cancellationToken);
            return data.Total;
        }

        /// <summary>
        /// Checks whether the specified symbol exists in the terminal's symbol list.
        /// </summary>
        /// <param name="symbol">Symbol name to check (e.g., "EURUSD", "XAUUSD").</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if symbol exists, false otherwise.</returns>
        /// <remarks>
        /// UNWRAPPED: Returns primitive bool directly (was Data.Exists).
        /// Note: Existence doesn't guarantee the symbol is synchronized - use IsSymbolAvailableAsync() for that.
        /// </remarks>
        public async Task<bool> SymbolExistAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var data = await _acc.SymbolExistAsync(symbol, deadline, cancellationToken);
            return data.Exists;
        }

        /// <summary>
        /// Gets the symbol name by its index position in the symbols list.
        /// </summary>
        /// <param name="index">Zero-based symbol index in the list.</param>
        /// <param name="selected">If true, searches only among selected symbols in Market Watch; if false, searches all symbols.</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Symbol name at the specified index, or empty string if index is out of range.</returns>
        /// <remarks>UNWRAPPED: Returns primitive string directly (was Data.Name).</remarks>
        public async Task<string> SymbolNameAsync(
            int index,
            bool selected,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var data = await _acc.SymbolNameAsync(index, selected, deadline, cancellationToken);
            return data.Name;
        }

        /// <summary>
        /// Selects or deselects a symbol in the Market Watch window.
        /// </summary>
        /// <param name="symbol">Symbol name to modify (e.g., "EURUSD").</param>
        /// <param name="selected">True to add symbol to Market Watch, false to remove it (default: true).</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if operation succeeded, false otherwise.</returns>
        /// <remarks>
        /// UNWRAPPED: Returns primitive bool directly (was Data.Success).
        /// Symbols must be selected in Market Watch to receive real-time quotes and trade them.
        /// </remarks>
        public async Task<bool> SymbolSelectAsync(
            string symbol,
            bool selected = true,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var data = await _acc.SymbolSelectAsync(symbol, selected, deadline, cancellationToken);
            return data.Success;
        }

        /// <summary>
        /// Checks whether the specified symbol is synchronized with the server (receiving live quotes).
        /// </summary>
        /// <param name="symbol">Symbol name to verify (e.g., "EURUSD").</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if symbol is synchronized and ready for trading, false otherwise.</returns>
        /// <remarks>
        /// UNWRAPPED: Returns primitive bool directly (was Data.Synchronized).
        /// A symbol must be both Exist and Synchronized to be tradeable.
        /// Use IsSymbolAvailableAsync() for combined check.
        /// </remarks>
        public async Task<bool> SymbolIsSynchronizedAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var data = await _acc.SymbolIsSynchronizedAsync(symbol, deadline, cancellationToken);
            return data.Synchronized;
        }

        /// <summary>
        /// Retrieves a double-precision numeric property of the specified symbol.
        /// </summary>
        /// <param name="symbol">Target symbol name (e.g., "EURUSD", "XAUUSD").</param>
        /// <param name="property">Numeric property to query (e.g., SymbolBid, SymbolAsk, SymbolPoint, SymbolVolumeMin).</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Unwrapped double value of the requested property.</returns>
        /// <remarks>
        /// UNWRAPPED: Returns primitive double directly (was Data.Value).
        /// Common properties: SymbolBid, SymbolAsk, SymbolPoint, SymbolVolumeMin, SymbolVolumeMax, SymbolVolumeStep.
        /// </remarks>
        public async Task<double> SymbolInfoDoubleAsync(
            string symbol,
            SymbolInfoDoubleProperty property,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var data = await _acc.SymbolInfoDoubleAsync(symbol, property, deadline, cancellationToken);
            return data.Value;
        }

        /// <summary>
        /// Retrieves an integer property of the specified symbol.
        /// </summary>
        /// <param name="symbol">Target symbol name (e.g., "EURUSD").</param>
        /// <param name="property">Integer property to query (e.g., SymbolDigits, SymbolSpread, SymbolTradeMode).</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Unwrapped long value of the requested property.</returns>
        /// <remarks>
        /// UNWRAPPED: Returns primitive long directly (was Data.Value).
        /// Common properties: SymbolDigits, SymbolSpread, SymbolTradeMode, SymbolStopsLevel.
        /// </remarks>
        public async Task<long> SymbolInfoIntegerAsync(
            string symbol,
            SymbolInfoIntegerProperty property,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var data = await _acc.SymbolInfoIntegerAsync(symbol, property, deadline, cancellationToken);
            return data.Value;
        }

        /// <summary>
        /// Retrieves a string property of the specified symbol.
        /// </summary>
        /// <param name="symbol">Target symbol name (e.g., "EURUSD").</param>
        /// <param name="property">String property to query (e.g., SymbolDescription, SymbolBaseCurrency, SymbolPath).</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Unwrapped string value of the requested property.</returns>
        /// <remarks>
        /// UNWRAPPED: Returns primitive string directly (was Data.Value).
        /// Common properties: SymbolDescription, SymbolBaseCurrency, SymbolProfitCurrency, SymbolPath.
        /// </remarks>
        public async Task<string> SymbolInfoStringAsync(
            string symbol,
            SymbolInfoStringProperty property,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var data = await _acc.SymbolInfoStringAsync(symbol, property, deadline, cancellationToken);
            return data.Value;
        }

        /// <summary>
        /// Retrieves margin rate information for the specified symbol and order type.
        /// </summary>
        /// <param name="symbol">Target symbol name (e.g., "EURUSD").</param>
        /// <param name="orderType">Order type for margin calculation (BUY or SELL).</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>
        /// <see cref="SymbolInfoMarginRateData"/> containing initial and maintenance margin rates.
        /// </returns>
        /// <remarks>
        /// Thin wrapper - returns rich object. Used for calculating required margin for positions.
        /// </remarks>
        public Task<SymbolInfoMarginRateData> SymbolInfoMarginRateAsync(
            string symbol,
            ENUM_ORDER_TYPE orderType,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.SymbolInfoMarginRateAsync(symbol, orderType, deadline, cancellationToken);

        // Convenience methods for common symbol properties

        /// <summary>
        /// Gets the current bid price for the specified symbol.
        /// </summary>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Current bid price as double.</returns>
        /// <remarks>Convenience shortcut for SymbolInfoDoubleAsync(symbol, SymbolBid).</remarks>
        public Task<double> GetBidAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolBid, deadline, cancellationToken);

        /// <summary>
        /// Gets the current ask price for the specified symbol.
        /// </summary>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Current ask price as double.</returns>
        /// <remarks>Convenience shortcut for SymbolInfoDoubleAsync(symbol, SymbolAsk).</remarks>
        public Task<double> GetAskAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolAsk, deadline, cancellationToken);

        /// <summary>
        /// Gets the current spread in points for the specified symbol.
        /// </summary>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Current spread in points as long.</returns>
        /// <remarks>Convenience shortcut for SymbolInfoIntegerAsync(symbol, SymbolSpread).</remarks>
        public Task<long> GetSpreadAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => SymbolInfoIntegerAsync(symbol, SymbolInfoIntegerProperty.SymbolSpread, deadline, cancellationToken);

        /// <summary>
        /// Gets the minimum allowed lot size for the specified symbol.
        /// </summary>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Minimum volume in lots as double (e.g., 0.01).</returns>
        /// <remarks>
        /// Convenience shortcut for SymbolInfoDoubleAsync(symbol, SymbolVolumeMin).
        /// Use this to validate order volumes before placing trades.
        /// </remarks>
        public Task<double> GetVolumeMinAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeMin, deadline, cancellationToken);

        /// <summary>
        /// Gets the maximum allowed lot size for the specified symbol.
        /// </summary>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Maximum volume in lots as double (e.g., 100.0).</returns>
        /// <remarks>Convenience shortcut for SymbolInfoDoubleAsync(symbol, SymbolVolumeMax).</remarks>
        public Task<double> GetVolumeMaxAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeMax, deadline, cancellationToken);

        /// <summary>
        /// Gets the volume step increment for the specified symbol.
        /// </summary>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Volume step in lots as double (e.g., 0.01 means volumes must be multiples of 0.01).</returns>
        /// <remarks>
        /// Convenience shortcut for SymbolInfoDoubleAsync(symbol, SymbolVolumeStep).
        /// Use this to round volumes to valid increments (e.g., if step is 0.01, volume 0.015 is invalid).
        /// </remarks>
        public Task<double> GetVolumeStepAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolVolumeStep, deadline, cancellationToken);

        /// <summary>
        /// Gets the point size for the specified symbol (minimum price change).
        /// </summary>
        /// <param name="symbol">Symbol name (e.g., "EURUSD").</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Point value as double (e.g., 0.00001 for EURUSD on 5-digit broker).</returns>
        /// <remarks>
        /// Convenience shortcut for SymbolInfoDoubleAsync(symbol, SymbolPoint).
        /// Used for price calculations and converting points to price levels.
        /// </remarks>
        public Task<double> GetPointAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => SymbolInfoDoubleAsync(symbol, SymbolInfoDoubleProperty.SymbolPoint, deadline, cancellationToken);

        /// <summary>
        /// Checks if a symbol exists in the terminal AND is synchronized with server (fully ready for trading).
        /// </summary>
        /// <param name="symbol">Symbol name to check (e.g., "EURUSD").</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if symbol exists and is synchronized (ready to trade), false otherwise.</returns>
        /// <remarks>
        /// NEW CONVENIENCE METHOD: Combines SymbolExistAsync() and SymbolIsSynchronizedAsync() checks.
        /// More efficient than calling both methods separately. Use this before attempting to trade a symbol.
        /// </remarks>
        public async Task<bool> IsSymbolAvailableAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var exists = await SymbolExistAsync(symbol, deadline, cancellationToken);
            if (!exists) return false;
            return await SymbolIsSynchronizedAsync(symbol, deadline, cancellationToken);
        }

        /// <summary>
        /// Checks if trading operations are allowed for the current account.
        /// </summary>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if trading is allowed, false if account is read-only or trading is disabled.</returns>
        /// <remarks>
        /// NEW CONVENIENCE METHOD: Checks AccountInfoIntegerAsync(AccountTradeAllowed).
        /// Returns false for investor/read-only accounts or when trading is globally disabled.
        /// </remarks>
        public async Task<bool> IsTradingAllowedAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var allowed = await AccountInfoIntegerAsync(AccountInfoIntegerPropertyType.AccountTradeAllowed, deadline, cancellationToken);
            return allowed == 1;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region [04] SYMBOL INFO
        // ─────────────────────────────────────────────────────────────

        /// <summary>Gets the latest tick data for the specified symbol.</summary>
        /// <param name="symbol">Target symbol name (e.g., "EURUSD").</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Last known tick containing bid, ask, and related fields.</returns>
        public Task<MrpcMqlTick> SymbolInfoTickAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.SymbolInfoTickAsync(symbol, deadline, cancellationToken);

        /// <summary>Gets current quote for symbol (convenience alias for SymbolInfoTickAsync).</summary>
        /// <param name="symbol">Target symbol name (e.g., "EURUSD").</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Last known tick containing bid, ask, and related fields.</returns>
        public Task<MrpcMqlTick> QuoteAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => SymbolInfoTickAsync(symbol, deadline, cancellationToken);

        /// <summary>Gets trading session quote information for the specified symbol and day.</summary>
        /// <param name="symbol">Target symbol name (e.g., "EURUSD").</param>
        /// <param name="dayOfWeek">Day of the week for which session data is requested.</param>
        /// <param name="sessionIndex">Index of the session on that day.</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Session quote information including open and close times.</returns>
        public Task<SymbolInfoSessionQuoteData> SymbolInfoSessionQuoteAsync(
            string symbol,
            mt5_term_api.DayOfWeek dayOfWeek,
            int sessionIndex,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.SymbolInfoSessionQuoteAsync(symbol, dayOfWeek, sessionIndex, deadline, cancellationToken);

        /// <summary>Gets trading session trade information for the specified symbol and day.</summary>
        /// <param name="symbol">Target symbol name (e.g., "EURUSD").</param>
        /// <param name="dayOfWeek">Day of the week for which trade session data is requested.</param>
        /// <param name="sessionIndex">Index of the session on that day.</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Session trade information including open and close times.</returns>
        public Task<SymbolInfoSessionTradeData> SymbolInfoSessionTradeAsync(
            string symbol,
            mt5_term_api.DayOfWeek dayOfWeek,
            int sessionIndex,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.SymbolInfoSessionTradeAsync(symbol, dayOfWeek, sessionIndex, deadline, cancellationToken);

        /// <summary>Calculates tick value and size for one or multiple symbols.</summary>
        /// <param name="symbols">Collection of symbol names (e.g., "EURUSD", "GBPUSD").</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Tick value and size data for each specified symbol.</returns>
        public Task<TickValueWithSizeData> TickValueWithSizeAsync(
            IEnumerable<string> symbols,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.TickValueWithSizeAsync(symbols, deadline, cancellationToken);

        /// <summary>Retrieves multiple symbol parameters in a single request.</summary>
        /// <param name="request">Request object specifying symbols and parameters to fetch.</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Combined data set containing requested symbol parameters.</returns>
        public Task<SymbolParamsManyData> SymbolParamsManyAsync(
            SymbolParamsManyRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.SymbolParamsManyAsync(request, deadline, cancellationToken);

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region [05] MARKET BOOK
        // ─────────────────────────────────────────────────────────────

        /// <summary>Subscribes to market depth (order book) updates for the specified symbol.</summary>
        /// <param name="symbol">Symbol name to subscribe to (e.g., "EURUSD").</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result indicating whether the subscription was successful.</returns>
        public Task<MarketBookAddData> MarketBookAddAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.MarketBookAddAsync(symbol, deadline, cancellationToken);

        /// <summary>Unsubscribes from market depth (order book) updates for the specified symbol.</summary>
        /// <param name="symbol">Symbol name to unsubscribe from (e.g., "EURUSD").</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result indicating whether the unsubscription was successful.</returns>
        public Task<MarketBookReleaseData> MarketBookReleaseAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.MarketBookReleaseAsync(symbol, deadline, cancellationToken);

        /// <summary>Retrieves the current market depth (order book) for the specified symbol.</summary>
        /// <param name="symbol">Symbol name to query (e.g., "EURUSD").</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Current order book data containing bid and ask levels.</returns>
        public Task<MarketBookGetData> MarketBookGetAsync(
            string symbol,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.MarketBookGetAsync(symbol, deadline, cancellationToken);

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region [06] ORDERS / POSITIONS / HISTORY
        // ─────────────────────────────────────────────────────────────

        /// <summary>Retrieves all currently opened orders with optional sorting.</summary>
        /// <param name="sortMode">Sorting mode for the orders (default: by open time ascending).</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of opened orders with full details.</returns>
        public Task<OpenedOrdersData> OpenedOrdersAsync(
            BMT5_ENUM_OPENED_ORDER_SORT_TYPE sortMode = BMT5_ENUM_OPENED_ORDER_SORT_TYPE.Bmt5OpenedOrderSortByOpenTimeAsc,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.OpenedOrdersAsync(sortMode, deadline, cancellationToken);

        /// <summary>Retrieves ticket numbers of all currently opened orders.</summary>
        /// <returns>List of ticket IDs for opened orders.</returns>
        public Task<OpenedOrdersTicketsData> OpenedOrdersTicketsAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.OpenedOrdersTicketsAsync(deadline, cancellationToken);

        /// <summary>Retrieves historical orders within a specified date range.</summary>
        /// <param name="from">Start date of the history interval.</param>
        /// <param name="to">End date of the history interval.</param>
        /// <param name="sortMode">Sorting mode for historical orders (default: by close time ascending).</param>
        /// <param name="pageNumber">Page number for paginated results (0 for all).</param>
        /// <param name="itemsPerPage">Number of items per page (0 for all).</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Historical orders data within the requested period.</returns>
        public Task<OrdersHistoryData> OrderHistoryAsync(
            DateTime from,
            DateTime to,
            BMT5_ENUM_ORDER_HISTORY_SORT_TYPE sortMode = BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeAsc,
            int pageNumber = 0,
            int itemsPerPage = 0,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.OrderHistoryAsync(from, to, sortMode, pageNumber, itemsPerPage, deadline, cancellationToken);

        /// <summary>Retrieves historical positions with sorting, optional time filters, and paging.</summary>
        /// <param name="sortType">Sorting mode for the positions history.</param>
        /// <param name="openFrom">Filter: include positions opened on/after this time (optional).</param>
        /// <param name="openTo">Filter: include positions opened before/at this time (optional).</param>
        /// <param name="page">Page index for paginated results (0-based).</param>
        /// <param name="size">Items per page (0 to return all).</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Positions history matching the specified criteria.</returns>
        public Task<PositionsHistoryData> PositionsHistoryAsync(
            AH_ENUM_POSITIONS_HISTORY_SORT_TYPE sortType,
            DateTime? openFrom = null,
            DateTime? openTo = null,
            int page = 0,
            int size = 0,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.PositionsHistoryAsync(sortType, openFrom, openTo, page, size, deadline, cancellationToken);

        /// <summary>Returns the total number of currently open positions.</summary>
        /// <returns>Total count of open positions.</returns>
        public Task<PositionsTotalData> PositionsTotalAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.PositionsTotalAsync(deadline, cancellationToken);

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region [07] PRE-TRADE
        // ─────────────────────────────────────────────────────────────


        /// <summary>Estimates required margin for a prospective order.</summary>
        /// <param name="request">Order parameters (e.g., symbol, type, volume, price).</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Calculated margin data for the given order parameters.</returns>
        public Task<OrderCalcMarginData> OrderCalcMarginAsync(
            OrderCalcMarginRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.OrderCalcMarginAsync(request, deadline, cancellationToken);

        /// <summary>Performs a pre-trade check to validate order parameters.</summary>
        /// <param name="request">Order parameters to verify before sending.</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result containing validation status and possible trade errors.</returns>
        public Task<OrderCheckData> OrderCheckAsync(
            OrderCheckRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.OrderCheckAsync(request, deadline, cancellationToken);

        /// <summary>Sends a trade order to the server for execution.</summary>
        /// <param name="request">Order details including symbol, type, volume, and price.</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result containing execution outcome and order ticket.</returns>
        public Task<OrderSendData> OrderSendAsync(
            OrderSendRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.OrderSendAsync(request, deadline, cancellationToken);

        /// <summary>Modifies parameters of an existing pending order.</summary>
        /// <param name="request">New order parameters such as price, SL, or TP levels.</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result indicating success or failure of the modification.</returns>
        public Task<OrderModifyData> OrderModifyAsync(
            OrderModifyRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.OrderModifyAsync(request, deadline, cancellationToken);

        /// <summary>Closes an existing open order or position.</summary>
        /// <param name="request">Order close parameters including ticket, volume, and price.</param>
        /// <param name="deadline">Optional deadline for the gRPC call.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result containing close status and related trade data.</returns>
        public Task<OrderCloseData> OrderCloseAsync(
            OrderCloseRequest request,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => _acc.OrderCloseAsync(request, deadline, cancellationToken);

        // Convenience methods for trading operations

        /// <summary>
        /// Opens a market BUY order at the current ask price.
        /// </summary>
        /// <param name="symbol">Symbol to trade (e.g., "EURUSD").</param>
        /// <param name="volume">Volume in lots (e.g., 0.01, 0.1, 1.0).</param>
        /// <param name="stopLoss">Optional stop loss price level.</param>
        /// <param name="takeProfit">Optional take profit price level.</param>
        /// <param name="comment">Optional comment for the order (max 31 characters).</param>
        /// <param name="magic">Optional magic number for order identification by EA (default: 0).</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>
        /// <see cref="OrderSendData"/> containing order ticket, result code, and execution details.
        /// </returns>
        /// <remarks>
        /// NEW CONVENIENCE METHOD: Simplifies market order placement - no need to manually build OrderSendRequest.
        /// Executes immediately at current market price (ask for buy). Use for instant order execution.
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = await service.BuyMarketAsync("EURUSD", 0.01, stopLoss: 1.0800, takeProfit: 1.0900);
        /// if (result.RetCode == 10009) Console.WriteLine($"Order opened: ticket #{result.Order}");
        /// </code>
        /// </example>
        public Task<OrderSendData> BuyMarketAsync(
            string symbol,
            double volume,
            double? stopLoss = null,
            double? takeProfit = null,
            string comment = "",
            ulong magic = 0,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var request = new OrderSendRequest
            {
                Symbol = symbol,
                Volume = volume,
                Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,
                Comment = comment,
                ExpertId = magic
            };
            if (stopLoss.HasValue)
                request.StopLoss = stopLoss.Value;
            if (takeProfit.HasValue)
                request.TakeProfit = takeProfit.Value;
            return OrderSendAsync(request, deadline, cancellationToken);
        }

        /// <summary>
        /// Opens a market SELL order at the current bid price.
        /// </summary>
        /// <param name="symbol">Symbol to trade (e.g., "EURUSD").</param>
        /// <param name="volume">Volume in lots (e.g., 0.01, 0.1, 1.0).</param>
        /// <param name="stopLoss">Optional stop loss price level.</param>
        /// <param name="takeProfit">Optional take profit price level.</param>
        /// <param name="comment">Optional comment for the order (max 31 characters).</param>
        /// <param name="magic">Optional magic number for order identification by EA (default: 0).</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>
        /// <see cref="OrderSendData"/> containing order ticket, result code, and execution details.
        /// </returns>
        /// <remarks>
        /// NEW CONVENIENCE METHOD: Simplifies market order placement - no need to manually build OrderSendRequest.
        /// Executes immediately at current market price (bid for sell). Use for instant order execution.
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = await service.SellMarketAsync("EURUSD", 0.01, stopLoss: 1.0900, takeProfit: 1.0800);
        /// </code>
        /// </example>
        public Task<OrderSendData> SellMarketAsync(
            string symbol,
            double volume,
            double? stopLoss = null,
            double? takeProfit = null,
            string comment = "",
            ulong magic = 0,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var request = new OrderSendRequest
            {
                Symbol = symbol,
                Volume = volume,
                Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSell,
                Comment = comment,
                ExpertId = magic
            };
            if (stopLoss.HasValue)
                request.StopLoss = stopLoss.Value;
            if (takeProfit.HasValue)
                request.TakeProfit = takeProfit.Value;
            return OrderSendAsync(request, deadline, cancellationToken);
        }

        /// <summary>
        /// Places a pending BUY LIMIT order (buy below current price).
        /// </summary>
        /// <param name="symbol">Symbol to trade (e.g., "EURUSD").</param>
        /// <param name="volume">Volume in lots.</param>
        /// <param name="price">Entry price level - must be below current ask price.</param>
        /// <param name="stopLoss">Optional stop loss price level.</param>
        /// <param name="takeProfit">Optional take profit price level.</param>
        /// <param name="comment">Optional comment for the order.</param>
        /// <param name="magic">Optional magic number for order identification by EA.</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>
        /// <see cref="OrderSendData"/> containing order ticket and result code.
        /// </returns>
        /// <remarks>
        /// NEW CONVENIENCE METHOD: Places pending order that triggers when price drops to specified level.
        /// Use when expecting price to fall before going up. Order activates automatically when price reaches entry level.
        /// </remarks>
        public Task<OrderSendData> BuyLimitAsync(
            string symbol,
            double volume,
            double price,
            double? stopLoss = null,
            double? takeProfit = null,
            string comment = "",
            ulong magic = 0,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var request = new OrderSendRequest
            {
                Symbol = symbol,
                Volume = volume,
                Price = price,
                Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyLimit,
                Comment = comment,
                ExpertId = magic
            };
            if (stopLoss.HasValue)
                request.StopLoss = stopLoss.Value;
            if (takeProfit.HasValue)
                request.TakeProfit = takeProfit.Value;
            return OrderSendAsync(request, deadline, cancellationToken);
        }

        /// <summary>
        /// Places a pending SELL LIMIT order (sell above current price).
        /// </summary>
        /// <param name="symbol">Symbol to trade (e.g., "EURUSD").</param>
        /// <param name="volume">Volume in lots.</param>
        /// <param name="price">Entry price level - must be above current bid price.</param>
        /// <param name="stopLoss">Optional stop loss price level.</param>
        /// <param name="takeProfit">Optional take profit price level.</param>
        /// <param name="comment">Optional comment for the order.</param>
        /// <param name="magic">Optional magic number for order identification by EA.</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>
        /// <see cref="OrderSendData"/> containing order ticket and result code.
        /// </returns>
        /// <remarks>
        /// NEW CONVENIENCE METHOD: Places pending order that triggers when price rises to specified level.
        /// Use when expecting price to rise before going down. Order activates automatically when price reaches entry level.
        /// </remarks>
        public Task<OrderSendData> SellLimitAsync(
            string symbol,
            double volume,
            double price,
            double? stopLoss = null,
            double? takeProfit = null,
            string comment = "",
            ulong magic = 0,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var request = new OrderSendRequest
            {
                Symbol = symbol,
                Volume = volume,
                Price = price,
                Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellLimit,
                Comment = comment,
                ExpertId = magic
            };
            if (stopLoss.HasValue)
                request.StopLoss = stopLoss.Value;
            if (takeProfit.HasValue)
                request.TakeProfit = takeProfit.Value;
            return OrderSendAsync(request, deadline, cancellationToken);
        }

        /// <summary>
        /// Places a pending BUY STOP order (buy above current price on breakout).
        /// </summary>
        /// <param name="symbol">Symbol to trade (e.g., "EURUSD").</param>
        /// <param name="volume">Volume in lots.</param>
        /// <param name="price">Entry price level - must be above current ask price.</param>
        /// <param name="stopLoss">Optional stop loss price level.</param>
        /// <param name="takeProfit">Optional take profit price level.</param>
        /// <param name="comment">Optional comment for the order.</param>
        /// <param name="magic">Optional magic number for order identification by EA.</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>
        /// <see cref="OrderSendData"/> containing order ticket and result code.
        /// </returns>
        /// <remarks>
        /// NEW CONVENIENCE METHOD: Places pending order that triggers when price breaks above specified level.
        /// Use for breakout strategies - buy when price exceeds resistance. Order activates automatically on breakout.
        /// </remarks>
        public Task<OrderSendData> BuyStopAsync(
            string symbol,
            double volume,
            double price,
            double? stopLoss = null,
            double? takeProfit = null,
            string comment = "",
            ulong magic = 0,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var request = new OrderSendRequest
            {
                Symbol = symbol,
                Volume = volume,
                Price = price,
                Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyStop,
                Comment = comment,
                ExpertId = magic
            };
            if (stopLoss.HasValue)
                request.StopLoss = stopLoss.Value;
            if (takeProfit.HasValue)
                request.TakeProfit = takeProfit.Value;
            return OrderSendAsync(request, deadline, cancellationToken);
        }

        /// <summary>
        /// Places a pending SELL STOP order (sell below current price on breakout).
        /// </summary>
        /// <param name="symbol">Symbol to trade (e.g., "EURUSD").</param>
        /// <param name="volume">Volume in lots.</param>
        /// <param name="price">Entry price level - must be below current bid price.</param>
        /// <param name="stopLoss">Optional stop loss price level.</param>
        /// <param name="takeProfit">Optional take profit price level.</param>
        /// <param name="comment">Optional comment for the order.</param>
        /// <param name="magic">Optional magic number for order identification by EA.</param>
        /// <param name="deadline">Optional RPC deadline.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>
        /// <see cref="OrderSendData"/> containing order ticket and result code.
        /// </returns>
        /// <remarks>
        /// NEW CONVENIENCE METHOD: Places pending order that triggers when price breaks below specified level.
        /// Use for breakout strategies - sell when price breaks support. Order activates automatically on breakout.
        /// </remarks>
        public Task<OrderSendData> SellStopAsync(
            string symbol,
            double volume,
            double price,
            double? stopLoss = null,
            double? takeProfit = null,
            string comment = "",
            ulong magic = 0,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            var request = new OrderSendRequest
            {
                Symbol = symbol,
                Volume = volume,
                Price = price,
                Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellStop,
                Comment = comment,
                ExpertId = magic
            };
            if (stopLoss.HasValue)
                request.StopLoss = stopLoss.Value;
            if (takeProfit.HasValue)
                request.TakeProfit = takeProfit.Value;
            return OrderSendAsync(request, deadline, cancellationToken);
        }

        // Convenience methods for history

        /// <summary>Gets recent orders from history (last N days).</summary>
        public Task<OrdersHistoryData> GetRecentOrdersAsync(
            int days = 7,
            int limit = 100,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return OrderHistoryAsync(
                from: DateTime.UtcNow.AddDays(-days),
                to: DateTime.UtcNow,
                sortMode: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc,
                pageNumber: 0,
                itemsPerPage: limit,
                deadline: deadline,
                cancellationToken: cancellationToken
            );
        }

        /// <summary>Gets today's orders from history.</summary>
        public Task<OrdersHistoryData> GetTodayOrdersAsync(
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            => GetRecentOrdersAsync(days: 1, limit: 1000, deadline, cancellationToken);

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region [08] STREAMS
        // ─────────────────────────────────────────────────────────────


        /// <summary>Streams live tick updates for the specified symbols.</summary>
        /// <param name="symbols">Collection of symbol names to subscribe to (e.g., "EURUSD", "GBPUSD").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous stream of tick data for the subscribed symbols.</returns>
        public IAsyncEnumerable<OnSymbolTickData> OnSymbolTickAsync(
            IEnumerable<string> symbols,
            CancellationToken cancellationToken = default)
            => _acc.OnSymbolTickAsync(symbols, cancellationToken);

        /// <summary>Streams real-time trade events such as order executions or closures.</summary>
        /// <returns>Asynchronous stream of trade event data.</returns>
        public IAsyncEnumerable<OnTradeData> OnTradeAsync(
            CancellationToken cancellationToken = default)
            => _acc.OnTradeAsync(cancellationToken);

        /// <summary>Streams periodic snapshots of position profit/loss.</summary>
        /// <param name="intervalMs">Polling interval in milliseconds between snapshots.</param>
        /// <param name="ignoreEmpty">Skip emissions when there are no positions (true by default).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous stream of profit data per position.</returns>
        public IAsyncEnumerable<OnPositionProfitData> OnPositionProfitAsync(
            int intervalMs,
            bool ignoreEmpty = true,
            CancellationToken cancellationToken = default)
            => _acc.OnPositionProfitAsync(intervalMs, ignoreEmpty, cancellationToken);

        /// <summary>Streams periodic snapshots of tickets for open positions and pending orders.</summary>
        /// <param name="intervalMs">Polling interval in milliseconds between snapshots.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous stream containing ticket lists for positions and pending orders.</returns>
        public IAsyncEnumerable<OnPositionsAndPendingOrdersTicketsData> OnPositionsAndPendingOrdersTicketsAsync(
            int intervalMs,
            CancellationToken cancellationToken = default)
            => _acc.OnPositionsAndPendingOrdersTicketsAsync(intervalMs, cancellationToken);

        /// <summary>Streams real-time trade transaction events from the terminal.</summary>
        /// <returns>Asynchronous stream of trade transaction data (order updates, deal executions, etc.).</returns>
        public IAsyncEnumerable<OnTradeTransactionData> OnTradeTransactionAsync(
            CancellationToken cancellationToken = default)
            => _acc.OnTradeTransactionAsync(cancellationToken);

        #endregion

        // ═════════════════════════════════════════════════════════════════════════════
        #region USAGE EXAMPLES: MT5Account (Low-Level) vs MT5Service (Mid-Level)
        // ═════════════════════════════════════════════════════════════════════════════

        /*
        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ EXAMPLE 1: Getting Account Balance                                          │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │                                                                             │
        │ ❌ LOW-LEVEL (MT5Account) - Verbose, requires unwrapping:                   │
        │                                                                             │
        │   var balanceData = await account.AccountInfoDoubleAsync(                   │
        │       AccountInfoDoublePropertyType.AccountBalance);                        │
        │   double balance = balanceData.Value;  // ← Need to unwrap!                 │
        │                                                                             │
        │ ✅ MID-LEVEL (MT5Service) - Clean, direct:                                  │
        │                                                                             │
        │   double balance = await service.GetBalanceAsync();  //  Already unwrapped  │
        │                                                                             │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ EXAMPLE 2: Getting Symbol Bid/Ask Prices                                    │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │                                                                             │
        │ ❌ LOW-LEVEL (MT5Account):                                                  │
        │                                                                             │
        │   var bidData = await account.SymbolInfoDoubleAsync(                        │
        │       "EURUSD",                                                             │
        │       SymbolInfoDoubleProperty.SymbolBid);                                  │
        │   double bid = bidData.Value;                                               │
        │                                                                             │
        │   var askData = await account.SymbolInfoDoubleAsync(                        │
        │       "EURUSD",                                                             │
        │       SymbolInfoDoubleProperty.SymbolAsk);                                  │
        │   double ask = askData.Value;                                               │
        │                                                                             │
        │ ✅ MID-LEVEL (MT5Service):                                                  │
        │                                                                             │
        │   double bid = await service.GetBidAsync("EURUSD");                         │
        │   double ask = await service.GetAskAsync("EURUSD");                         │
        │                                                                             │
        │   // Or calculate spread instantly:                                         │
        │   double spread = ask - bid;                                                │
        │                                                                             │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ EXAMPLE 3: Opening Market BUY Order                                         │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │                                                                             │
        │ ❌ LOW-LEVEL (MT5Account) - Manual request building:                       │
        │                                                                             │
        │   var request = new OrderSendRequest                                        │
        │   {                                                                         │
        │       Symbol = "EURUSD",                                                    │
        │       Volume = 0.01,                                                        │
        │       Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuy,                    │
        │       StopLoss = 1.0800,                                                    │
        │       TakeProfit = 1.0900,                                                  │
        │       Comment = "My trade",                                                 │
        │       ExpertId = 12345                                                      │
        │   };                                                                        │
        │   var result = await account.OrderSendAsync(request);                       │
        │                                                                             │
        │ ✅ MID-LEVEL (MT5Service) - One-liner with named parameters:                │
        │                                                                             │
        │   var result = await service.BuyMarketAsync(                                │
        │       symbol: "EURUSD",                                                     │
        │       volume: 0.01,                                                         │
        │       stopLoss: 1.0800,                                                     │
        │       takeProfit: 1.0900,                                                   │
        │       comment: "My trade",                                                  │
        │       magic: 12345);                                                        │
        │                                                                             │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ EXAMPLE 4: Checking if Symbol is Available for Trading                      │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │                                                                             │
        │ ❌ LOW-LEVEL (MT5Account) - Two separate calls + unwrapping:                │
        │                                                                             │
        │   var existsData = await account.SymbolExistAsync("GBPUSD");                │
        │   if (!existsData.Exists)                                                   │
        │       return false;                                                         │
        │                                                                             │
        │   var syncData = await account.SymbolIsSynchronizedAsync("GBPUSD");         │
        │   bool available = syncData.Synchronized;                                   │
        │                                                                             │
        │ ✅ MID-LEVEL (MT5Service) - Combined check in one call:                     │
        │                                                                             │
        │   bool available = await service.IsSymbolAvailableAsync("GBPUSD");          │
        │                                                                             │
        │   // Handles both Exist check AND Synchronized check automatically!         │
        │                                                                             │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ EXAMPLE 5: Getting Account Equity and Margin Information                    │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │                                                                             │
        │ ❌ LOW-LEVEL (MT5Account):                                                  │
        │                                                                             │
        │   var equityData = await account.AccountInfoDoubleAsync(                    │
        │       AccountInfoDoublePropertyType.AccountEquity);                         │
        │   double equity = equityData.Value;                                         │
        │                                                                             │
        │   var marginData = await account.AccountInfoDoubleAsync(                    │
        │       AccountInfoDoublePropertyType.AccountMargin);                         │
        │   double margin = marginData.Value;                                         │
        │                                                                             │
        │   var freeMarginData = await account.AccountInfoDoubleAsync(                │
        │       AccountInfoDoublePropertyType.AccountMarginFree);                     │
        │   double freeMargin = freeMarginData.Value;                                 │
        │                                                                             │
        │   double marginLevel = (margin > 0) ? (equity / margin * 100) : 0;          │
        │                                                                             │
        │ ✅ MID-LEVEL (MT5Service):                                                  │
        │                                                                             │
        │   double equity = await service.GetEquityAsync();                           │
        │   double margin = await service.GetMarginAsync();                           │
        │   double freeMargin = await service.GetFreeMarginAsync();                   │
        │   double marginLevel = (margin > 0) ? (equity / margin * 100) : 0;          │
        │                                                                             │
        │   // Clean, readable, no ceremony!                                          │
        │                                                                             │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ EXAMPLE 6: Getting Symbol Trading Constraints                               │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │                                                                             │
        │ ❌ LOW-LEVEL (MT5Account):                                                  │
        │                                                                             │
        │   var minVolData = await account.SymbolInfoDoubleAsync(                     │
        │       "XAUUSD", SymbolInfoDoubleProperty.SymbolVolumeMin);                  │
        │   double minVol = minVolData.Value;                                         │
        │                                                                             │
        │   var maxVolData = await account.SymbolInfoDoubleAsync(                     │
        │       "XAUUSD", SymbolInfoDoubleProperty.SymbolVolumeMax);                  │
        │   double maxVol = maxVolData.Value;                                         │
        │                                                                             │
        │   var stepData = await account.SymbolInfoDoubleAsync(                       │
        │       "XAUUSD", SymbolInfoDoubleProperty.SymbolVolumeStep);                 │
        │   double step = stepData.Value;                                             │
        │                                                                             │
        │ ✅ MID-LEVEL (MT5Service):                                                  │
        │                                                                             │
        │   double minVol = await service.GetVolumeMinAsync("XAUUSD");                │
        │   double maxVol = await service.GetVolumeMaxAsync("XAUUSD");                │
        │   double step = await service.GetVolumeStepAsync("XAUUSD");                 │
        │                                                                             │
        │   // Perfect for validation before placing orders!                          │
        │                                                                             │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ EXAMPLE 7: Placing Pending Orders (BUY LIMIT / SELL STOP)                   │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │                                                                             │
        │ ❌ LOW-LEVEL (MT5Account):                                                  │
        │                                                                             │
        │   var buyLimitReq = new OrderSendRequest                                    │
        │   {                                                                         │
        │       Symbol = "EURUSD",                                                    │
        │       Volume = 0.1,                                                         │
        │       Price = 1.0850,                                                       │
        │       Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeBuyLimit,               │
        │       StopLoss = 1.0800,                                                    │
        │       TakeProfit = 1.0900                                                   │
        │   };                                                                        │
        │   var buyResult = await account.OrderSendAsync(buyLimitReq);                │
        │                                                                             │
        │   var sellStopReq = new OrderSendRequest                                    │
        │   {                                                                         │
        │       Symbol = "GBPUSD",                                                    │
        │       Volume = 0.05,                                                        │
        │       Price = 1.2500,                                                       │
        │       Operation = TMT5_ENUM_ORDER_TYPE.Tmt5OrderTypeSellStop,               │
        │       StopLoss = 1.2550,                                                    │
        │       TakeProfit = 1.2450                                                   │
        │   };                                                                        │
        │   var sellResult = await account.OrderSendAsync(sellStopReq);               │
        │                                                                             │
        │ ✅ MID-LEVEL (MT5Service):                                                  │
        │                                                                             │
        │   var buyResult = await service.BuyLimitAsync(                              │
        │       "EURUSD", 0.1, price: 1.0850,                                         │
        │       stopLoss: 1.0800, takeProfit: 1.0900);                                │
        │                                                                             │
        │   var sellResult = await service.SellStopAsync(                             │
        │       "GBPUSD", 0.05, price: 1.2500,                                        │
        │       stopLoss: 1.2550, takeProfit: 1.2450);                                │
        │                                                                             │
        │   // Shorter, clearer intent, less boilerplate!                             │
        │                                                                             │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ EXAMPLE 8: Getting Recent Trading History                                   │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │                                                                             │
        │ ❌ LOW-LEVEL (MT5Account):                                                  │
        │                                                                             │
        │   var history = await account.OrderHistoryAsync(                            │
        │       from: DateTime.UtcNow.AddDays(-7),                                    │
        │       to: DateTime.UtcNow,                                                  │
        │       sortMode: BMT5_ENUM_ORDER_HISTORY_SORT_TYPE.Bmt5SortByCloseTimeDesc,  │
        │       pageNumber: 0,                                                        │
        │       itemsPerPage: 100);                                                   │
        │                                                                             │
        │   // Need to remember all parameter names and enum values                   │
        │                                                                             │
        │ ✅ MID-LEVEL (MT5Service):                                                  │
        │                                                                             │
        │   var lastWeek = await service.GetRecentOrdersAsync(days: 7);               │
        │   var today = await service.GetTodayOrdersAsync();                          │
        │                                                                             │
        │   // Sensible defaults, readable intent!                                    │
        │                                                                             │
        └─────────────────────────────────────────────────────────────────────────────┘

        ╔═════════════════════════════════════════════════════════════════════════════╗
        ║  KEY TAKEAWAYS:                                                             ║
        ╠═════════════════════════════════════════════════════════════════════════════╣
        ║                                                                             ║
        ║  ✓ MT5Service eliminates .Value unwrapping boilerplate                      ║
        ║  ✓ Convenience methods reduce code by 50-70%                                ║
        ║  ✓ Named parameters make trading calls self-documenting                     ║
        ║  ✓ Combined checks (IsSymbolAvailable) reduce round-trips                   ║
        ║  ✓ Trading shortcuts (BuyMarket, SellLimit) hide OrderSendRequest details   ║
        ║  ✓ History helpers (GetTodayOrders) provide sensible defaults               ║
        ║                                                                             ║
        ║  Use MT5Account when you need low-level control.                            ║
        ║  Use MT5Service for 90% of typical trading operations.                      ║
        ║  Use MT5Sugar for complex business logic and validations.                   ║
        ║                                                                             ║
        ╚═════════════════════════════════════════════════════════════════════════════╝
        */

        #endregion
    }
}
