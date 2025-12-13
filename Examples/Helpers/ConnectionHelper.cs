/*══════════════════════════════════════════════════════════════════════════════
 FILE: Examples/Helpers/ConnectionHelper.cs
 PURPOSE:
   Centralized configuration loader and connection manager for all example programs.
   Provides reusable methods for loading appsettings.json and establishing MT5
   terminal connections via gRPC.

 🎯 WHO USES THIS:
   • ALL example programs (LowLevel, Service, Sugar, Streaming, Orchestrators)
   • User code that needs standardized connection handling
   • Any code that needs to load Config/appsettings.json

 📋 WHAT THIS HELPER PROVIDES:

   1. BuildConfiguration()
      • Loads Config/appsettings.json from current directory
      • Returns IConfiguration object for accessing settings
      • Enables hot-reload when config file changes
      • Throws if config file is missing

   2. CreateAndConnectAccountAsync(config)
      • Creates MT5Account instance from configuration
      • Handles GUID generation for new sessions
      • Supports two connection methods:
        a) ConnectByServerNameAsync (recommended, uses broker server name)
        b) ConnectByHostPortAsync (fallback, uses host:port)
      • Validates connection with configurable timeout
      • Returns ready-to-use MT5Account instance

 ⚙️ CONFIGURATION REQUIREMENTS (appsettings.json):

   Required fields:
   • MT5:User                 - Account login number
   • MT5:Password             - Account password
   • MT5:ServerName           - Broker server name (e.g., "Broker-Demo")
      OR
   • MT5:Host + MT5:Port      - Broker host and port (only if ServerName not set)

   Optional fields:
   • MT5:GrpcServer           - gRPC gateway address (default: grpc.mt5.mrpc.pro)
   • MT5:InstanceId           - Session GUID (auto-generated if not provided)
   • MT5:BaseChartSymbol      - Symbol for connection (default: "EURUSD")
   • MT5:ConnectTimeoutSeconds - Connection timeout (default: 30)

 🔄 CONNECTION LOGIC:

   1. Load credentials from appsettings.json
   2. Generate or load session GUID (InstanceId)
   3. Create MT5Account with credentials
   4. Try connection methods in order:
      a) ConnectByServerNameAsync (if ServerName is provided)
      b) ConnectByHostPortAsync (if Host is MT5 broker, not gRPC gateway)
   5. Wait for terminal to respond within timeout
   6. Return connected account instance

 ⚠️ IMPORTANT NOTES:

   • GUID is ALWAYS required for grpc.mt5.mrpc.pro infrastructure
   • If InstanceId is not in config, a new GUID is generated per session
   • ServerName connection is preferred over Host:Port
   • Host:Port is skipped if host contains "mrpc.pro" (gRPC gateway address)
   • Connection throws exception if terminal doesn't respond within timeout

 💡 USAGE EXAMPLES:

   // Basic usage in example programs:
   var config = ConnectionHelper.BuildConfiguration();
   var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);
   var service = new MT5Service(account);

   // Access config values:
   var symbol = config["Mt5:BaseChartSymbol"] ?? "EURUSD";
   var user = config["Mt5:User"];

 RELATED FILES:
   • Config/appsettings.json - Configuration file loaded by this helper
   • ConsoleHelper.cs - Console output formatting (used by this helper)
   • MT5Account.cs - Low-level gRPC client created by this helper

══════════════════════════════════════════════════════════════════════════════*/

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MetaRPC.CSharpMT5.Examples.Helpers
{
    public static class ConnectionHelper
    {
        // ═════════════════════════════════════════════════════════════════
        // CONFIGURATION
        // ═════════════════════════════════════════════════════════════════

        public static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Config/appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        // ═════════════════════════════════════════════════════════════════
        // CONNECTION
        // ═════════════════════════════════════════════════════════════════

        public static async Task<MT5Account> CreateAndConnectAccountAsync(IConfiguration config)
        {
            ConsoleHelper.PrintSection("CONNECTION");

            var user = ulong.Parse(config["MT5:User"] ?? throw new Exception("MT5:User required"));
            var password = config["MT5:Password"] ?? "";
            var grpcServer = config["MT5:GrpcServer"];
            var instanceId = config["MT5:InstanceId"];
            var serverName = config["MT5:ServerName"];
            var host = config["MT5:Host"];
            var port = int.Parse(config["MT5:Port"] ?? "443");
            var baseSymbol = config["MT5:BaseChartSymbol"] ?? "EURUSD";
            var timeout = int.Parse(config["MT5:ConnectTimeoutSeconds"] ?? "30");

            ConsoleHelper.PrintInfo($"User:          {user}");
            ConsoleHelper.PrintInfo($"gRPC Server:   {grpcServer ?? "default"}");
            ConsoleHelper.PrintInfo($"Base Symbol:   {baseSymbol}");

            // CRITICALLY IMPORTANT: for the new grpc.mt5.mrpc.pro infrastructure
            // A GUID is ALWAYS required, even for the first connection!
            var accountId = Guid.Empty;
            if (!string.IsNullOrEmpty(instanceId))
            {
                accountId = Guid.Parse(instanceId);
            }
            else
            {
                // Generating a new GUID for the session
                accountId = Guid.NewGuid();
                ConsoleHelper.PrintInfo($"Generated Session ID: {accountId}");
            }

            var account = new MT5Account(
                user: user,
                password: password,
                grpcServer: grpcServer,
                id: accountId
            );

            ConsoleHelper.PrintInfo("\n→ Connecting to MT5 terminal...");

            // Try ConnectByServerNameAsync first (if ServerName is provided)
            if (!string.IsNullOrEmpty(serverName))
            {
                ConsoleHelper.PrintInfo($"  Method: ConnectByServerNameAsync");
                ConsoleHelper.PrintInfo($"  Server: {serverName}");

                await account.ConnectByServerNameAsync(
                    serverName: serverName,
                    baseChartSymbol: baseSymbol,
                    waitForTerminalIsAlive: true,
                    timeoutSeconds: timeout
                );

                ConsoleHelper.PrintSuccess("  ✓ Connected via ServerName!\n");
                return account;
            }

            // Fallback to ConnectByHostPortAsync (only if Host is MT5 broker address, not gRPC gateway)
            if (!string.IsNullOrEmpty(host) && !host.Contains("mrpc.pro"))
            {
                ConsoleHelper.PrintInfo($"  Method: ConnectByHostPortAsync");
                ConsoleHelper.PrintInfo($"  Host:   {host}:{port}");

                await account.ConnectByHostPortAsync(
                    host: host,
                    port: port,
                    baseChartSymbol: baseSymbol,
                    waitForTerminalIsAlive: true,
                    timeoutSeconds: timeout
                );

                ConsoleHelper.PrintSuccess("  ✓ Connected via Host:Port!\n");
                return account;
            }

            throw new Exception("Neither ServerName nor valid Host is configured in appsettings.json");
        }
    }
}
