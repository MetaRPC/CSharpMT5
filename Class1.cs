

namespace csharp_nuget
{
    using System;
    using Microsoft.Extensions.Logging;
    using MetaRPC.CSharpMT5;
    using mt5_term_api;
    using global::MetaRPC.CSharpMT5;

    namespace MetaRPC.CSharpMT5
    {
        public class TempDebug
        {
            private readonly ILogger<TempDebug> _logger;
            private readonly MT5Account _mt5Account;

            public TempDebug(ILogger<TempDebug> logger, MT5Account mt5Account)
            {
                _logger = logger;
                _mt5Account = mt5Account;
            }

            public async Task DebugPositionsHistoryAsync()
            {
                var history = await _mt5Account.PositionsHistoryAsync(
                    (AH_ENUM_POSITIONS_HISTORY_SORT_TYPE)0,
                    DateTime.UtcNow.AddDays(-30),
                    DateTime.UtcNow);

                _logger.LogInformation("PositionsHistoryAsync: {History}", history.ToString());
                // Попробуй вывести ToString или сериализуй через JSON
            }

            public async Task DebugSymbolNameAsync()
            {
                var name = await _mt5Account.SymbolNameAsync(0, false);
                _logger.LogInformation("SymbolNameAsync RAW: {Name}", name.ToString());
            }

