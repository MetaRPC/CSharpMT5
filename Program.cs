/*══════════════════════════════════════════════════════════════════════════════
 FILE: Program.cs — MAIN ENTRY POINT FOR ALL EXAMPLES

 ╔═══════════════════════════════════════════════════════════════════════════╗
 ║                           WHAT IS THIS FILE?                              ║
 ╚═══════════════════════════════════════════════════════════════════════════╝

 This is the ENTRY POINT for all demonstration examples from the Examples/ folder.
 It provides an interactive menu to showcase how the MT5 gRPC API works at different
 abstraction levels.

 ╔═══════════════════════════════════════════════════════════════════════════╗
 ║                        LEARNING PROGRESSION                               ║
 ╚═══════════════════════════════════════════════════════════════════════════╝

 Follow this order to understand the architecture from ground up:

 1️⃣ LOW-LEVEL (MT5Account)
    → Examples/LowLevel/
    → Direct gRPC/protobuf calls
    → Raw data structures
    → Understand the foundation

 2️⃣ MID-LEVEL (MT5Service)
    → Examples/Services/
    → Wrapper facade over MT5Account
    → Cleaner API, same functionality
    → Organized method groups

 3️⃣ HIGH-LEVEL (MT5Sugar - Extension Methods)
    → Examples/Sugar/
    → One-liner operations
    → Risk-based calculations
    → Smart defaults and normalization

 4️⃣ ORCHESTRATORS (Complete Strategies)
    → Examples/Orchestrators/
    → Automated trade lifecycle
    → Multiple API calls combined
    → Entry → Monitor → Exit logic

 5️⃣ PRESETS (Ready-to-Use Systems)
    → Examples/Presets/
    → Full trading systems
    → Adaptive market analysis
    → Multiple orchestrators combined

 ╔═══════════════════════════════════════════════════════════════════════════╗
 ║                    LEARNING RESOURCES FOR BEGINNERS                       ║
 ╚═══════════════════════════════════════════════════════════════════════════╝

 📚 If you need LINE-BY-LINE explanations:
    → Examples/MoreExamples/
    → Every single line commented
    → Explains basic C# concepts
    → Shows why Console.WriteLine is needed
    → Detailed breakdown of orchestrators

 Files in MoreExamples/:
    01_DirectGrpcCalls.cs                       - Low-level API calls explained
    02_StreamingExamples.cs                     - Real-time data streams explained
    03_TradingExamples.cs                       - Trading operations explained
    04_SugarExamples.cs                         - Sugar API with calculations
    05_SimpleScalpingOrchestrator_Explained.cs  - Full orchestrator breakdown
    06_PendingBreakoutOrchestrator_Explained.cs - Full orchestrator breakdown

 ╔═══════════════════════════════════════════════════════════════════════════╗
 ║                      WHERE TO WRITE YOUR OWN CODE                         ║
 ╚═══════════════════════════════════════════════════════════════════════════╝

 🛠️ User Code Sandbox (Your Testing Area):
    → Examples/UserCode/ProgramUserCode.cs
    → Write your experiments here
    → Quick test via: dotnet run usercode
    → Uncomment case "16" in this file (Program.cs) to enable

 ╔═══════════════════════════════════════════════════════════════════════════╗
 ║                    WHERE PROTOBUF DEFINITIONS COME FROM                   ║
 ╚═══════════════════════════════════════════════════════════════════════════╝

 📦 Protobuf Source:
    → NuGet package: mt5-term-api
    → Contains all .proto definitions compiled to C# classes
    → Installed via: dotnet add package mt5-term-api
    → Namespace: mt5_term_api

 🔍 How to explore protobuf types:
    → Run: dotnet run inspect
    → Shows all available protobuf message fields
    → Useful when API structure changes
    → Located in: Examples/Helpers/ProtobufInspector.cs

 ╔═══════════════════════════════════════════════════════════════════════════╗
 ║                              USAGE                                        ║
 ╚═══════════════════════════════════════════════════════════════════════════╝

 Interactive Menu:
   dotnet run                    → Shows menu with 16 options

 Direct Launch (skip menu):
   dotnet run lowlevel          → Low-level examples
   dotnet run sugar             → High-level Sugar API
   dotnet run scalping          → Simple Scalping orchestrator
   dotnet run adaptive          → Adaptive Market Preset
   dotnet run inspect           → Protobuf Inspector
   dotnet run usercode          → Your custom code (if uncommented)

 Available Commands:
   lowlevel, trading, streaming, service, sugar, monitor, pending, scalper,
   grid, news, breakout, hedge, scalping, adaptive, inspect, usercode

 ╔═══════════════════════════════════════════════════════════════════════════╗
 ║                         PROJECT STRUCTURE                                 ║
 ╚═══════════════════════════════════════════════════════════════════════════╝

 CSharpMT5/
 ├── MT5Account.cs              ← Low-level gRPC client (BASE)
 ├── MT5Service.cs              ← Mid-level wrapper (FACADE)
 ├── MT5Sugar.cs                ← High-level extensions (SUGAR)
 ├── Program.cs                 ← THIS FILE (entry point)
 ├── Config/appsettings.json    ← Connection settings
 └── Examples/
     ├── LowLevel/              ← Direct gRPC examples [1-3]
     ├── Services/              ← MT5Service examples [4]
     ├── Sugar/                 ← Sugar API examples [5-8]
     ├── Orchestrators/         ← Trading strategies [9-13]
     ├── Presets/               ← Complete systems [14]
     ├── MoreExamples/          ← Line-by-line tutorials
     ├── UserCode/              ← Your sandbox [16]
     └── Helpers/               ← ProtobufInspector [15]

══════════════════════════════════════════════════════════════════════════════*/

using System;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5.Examples;
using MetaRPC.CSharpMT5.Examples.LowLevel;
using MetaRPC.CSharpMT5.Examples.Services;
using MetaRPC.CSharpMT5.Examples.Sugar;
using MetaRPC.CSharpMT5.Examples.Helpers;
using MetaRPC.CSharpMT5.Examples.Orchestrators;

namespace MetaRPC.CSharpMT5;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // ═════════════════════════════════════════════════════════════════
        // MAIN LOOP
        // ═════════════════════════════════════════════════════════════════
        while (true)
        {
            string? command = null;

            // Get command from args or show menu
            if (args.Length > 0)
            {
                // Direct command mode: dotnet run <command>
                command = args[0].ToLower();
            }
            else
            {
                // Interactive menu mode
                PrintBanner();
                command = ShowMenu();
            }

            // ═════════════════════════════════════════════════════════════
            // EXECUTE COMMAND
            // ═════════════════════════════════════════════════════════════
            try
            {
                bool exitRequested = await ExecuteCommand(command);

                if (exitRequested)
                {
                    Console.WriteLine("\nExiting...");
                    return 0;
                }

                // Command-line mode: exit after one run
                if (args.Length > 0)
                {
                    Console.WriteLine("\n\nPress any key to exit...");
                    Console.ReadKey();
                    return 0;
                }

                // Interactive mode: ask to continue
                Console.WriteLine("\n");
                Console.WriteLine("┌──────────────────────────────────────────────────────────────┐");
                Console.WriteLine("│  [M] Return to Main Menu  |  [0] Exit                        │");
                Console.WriteLine("└──────────────────────────────────────────────────────────────┘");
                Console.Write("\nYour choice: ");

                string? nextAction = Console.ReadLine()?.Trim().ToLower();
                Console.WriteLine();

                if (nextAction == "0" || nextAction == "exit" || nextAction == "quit")
                {
                    Console.WriteLine("Exiting...");
                    return 0;
                }

                // Continue to menu
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                    ERROR OCCURRED                          ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");

                if (args.Length > 0)
                {
                    return 1;
                }

                Console.WriteLine("\nPress any key to return to menu...");
                Console.ReadKey();
                Console.WriteLine("\n");
            }
        }
    }

    // ═════════════════════════════════════════════════════════════════
    // BANNER
    // ═════════════════════════════════════════════════════════════════
    private static void PrintBanner()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                                                  ║");
        Console.WriteLine("║              CSharpMT5 - MetaTrader 5 gRPC Client                ║");
        Console.WriteLine("║                                                                  ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
    }

    // ═════════════════════════════════════════════════════════════════
    // MENU
    // ═════════════════════════════════════════════════════════════════
    private static string ShowMenu()
    {
        Console.WriteLine("┌──────────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│  LOW-LEVEL EXAMPLES (Direct gRPC)                                │");
        Console.WriteLine("├──────────────────────────────────────────────────────────────────┤");
        Console.WriteLine("│  [1]  Low-Level Basic      → dotnet run lowlevel                 │");
        Console.WriteLine("│  [2]  Trading Operations   → dotnet run trading                  │");
        Console.WriteLine("│  [3]  Streaming Examples   → dotnet run streaming                │");
        Console.WriteLine("├──────────────────────────────────────────────────────────────────┤");
        Console.WriteLine("│  MID-LEVEL (MT5Service Wrapper)                                  │");
        Console.WriteLine("├──────────────────────────────────────────────────────────────────┤");
        Console.WriteLine("│  [4]  Service Examples     → dotnet run service                  │");
        Console.WriteLine("├──────────────────────────────────────────────────────────────────┤");
        Console.WriteLine("│  HIGH-LEVEL (Sugar API - Recommended)                            │");
        Console.WriteLine("├──────────────────────────────────────────────────────────────────┤");
        Console.WriteLine("│  [5]  Sugar Examples       → dotnet run sugar                    │");
        Console.WriteLine("│  [6]  Monitor & History    → dotnet run monitor                  │");
        Console.WriteLine("│  [7]  Pending Orders       → dotnet run pending                  │");
        Console.WriteLine("│  [8]  Scalper Demo         → dotnet run scalper                  │");
        Console.WriteLine("├──────────────────────────────────────────────────────────────────┤");
        Console.WriteLine("│  ORCHESTRATORS (Automated Strategies)                            │");
        Console.WriteLine("├──────────────────────────────────────────────────────────────────┤");
        Console.WriteLine("│  [9]  Grid Trading         → dotnet run grid                     │");
        Console.WriteLine("│  [10] News Straddle        → dotnet run news                     │");
        Console.WriteLine("│  [11] Pending Breakout     → dotnet run breakout                 │");
        Console.WriteLine("│  [12] Quick Hedge          → dotnet run hedge                    │");
        Console.WriteLine("│  [13] Simple Scalping      → dotnet run scalping                 │");
        Console.WriteLine("├──────────────────────────────────────────────────────────────────┤");
        Console.WriteLine("│  PRESETS & TOOLS                                                 │");
        Console.WriteLine("├──────────────────────────────────────────────────────────────────┤");
        Console.WriteLine("│  [14] Adaptive Preset      → dotnet run adaptive                 │");
        Console.WriteLine("│  [15] Protobuf Inspector   → dotnet run inspect                  │");
        Console.WriteLine("├──────────────────────────────────────────────────────────────────┤");
        Console.WriteLine("│  USER CODE (Sandbox for your experiments)                        │");
        Console.WriteLine("├──────────────────────────────────────────────────────────────────┤");
        Console.WriteLine("│  [16] User Code Sandbox    → dotnet run usercode (COMMENTED)     │");
        Console.WriteLine("├──────────────────────────────────────────────────────────────────┤");
        Console.WriteLine("│  [0]  EXIT                                                       │");
        Console.WriteLine("└──────────────────────────────────────────────────────────────────┘");
        Console.WriteLine();
        Console.Write("Enter your choice: ");

        string? input = Console.ReadLine()?.Trim();
        Console.WriteLine();

        return input ?? "0";
    }

    // ═════════════════════════════════════════════════════════════════
    // COMMAND EXECUTOR
    // ═════════════════════════════════════════════════════════════════
    private static async Task<bool> ExecuteCommand(string? command)
    {
        if (string.IsNullOrEmpty(command))
        {
            return false;
        }

        command = command.ToLower();

        switch (command)
        {
            // ═════════════════════════════════════════════════════════════
            // LOW-LEVEL EXAMPLES
            // ═════════════════════════════════════════════════════════════
            case "1":
            case "lowlevel":
            case "low":
                await ProgramLowLevel.RunAsync();
                return false;

            case "2":
            case "trading":
            case "trade":
                await ProgramTrading.RunAsync();
                return false;

            case "3":
            case "streaming":
            case "streams":
                await MetaRPC.CSharpMT5.Examples.LowLevel.Program.RunStreamingAsync();
                return false;

            // ═════════════════════════════════════════════════════════════
            // MID-LEVEL
            // ═════════════════════════════════════════════════════════════
            case "4":
            case "service":
            case "mid":
                await ProgramService.RunAsync();
                return false;

            // ═════════════════════════════════════════════════════════════
            // HIGH-LEVEL (SUGAR)
            // ═════════════════════════════════════════════════════════════
            case "5":
            case "sugar":
            case "high":
                await ProgramSugar.RunAsync();
                return false;

            case "6":
            case "monitor":
            case "history":
                await ProgramSugarMonitor.RunAsync();
                return false;

            case "7":
            case "pending":
            case "pendingorders":
                await ProgramSugarPendingOrders.RunAsync();
                return false;

            case "8":
            case "scalper":
                await ProgramSugarScalper.RunAsync();
                return false;

            // ═════════════════════════════════════════════════════════════
            // ORCHESTRATORS
            // ═════════════════════════════════════════════════════════════
            case "9":
            case "grid":
            case "gridtrading":
                await RunOrchestrator_Grid();
                return false;

            case "10":
            case "news":
            case "newsstraddle":
                await RunOrchestrator_News();
                return false;

            case "11":
            case "breakout":
                await RunOrchestrator_Breakout();
                return false;

            case "12":
            case "hedge":
            case "quickhedge":
                await RunOrchestrator_Hedge();
                return false;

            case "13":
            case "scalping":
            case "simplescalping":
                await RunOrchestrator_Scalping();
                return false;

            // ═════════════════════════════════════════════════════════════
            // PRESETS & TOOLS
            // ═════════════════════════════════════════════════════════════
            case "14":
            case "adaptive":
            case "preset":
                await RunPreset_Adaptive();
                return false;

            case "15":
            case "inspect":
            case "types":
                ProtobufInspector.Run();
                return false;

            // ═════════════════════════════════════════════════════════════
            // USER CODE SANDBOX (COMMENTED OUT - UNCOMMENT TO USE)
            // ═════════════════════════════════════════════════════════════
            // case "16":
            // case "usercode":
            // case "user":
            // case "sandbox":
            //     await MetaRPC.CSharpMT5.Examples.UserCode.ProgramUserCode.RunAsync();
            //     return false;

            // ═════════════════════════════════════════════════════════════
            // EXIT
            // ═════════════════════════════════════════════════════════════
            case "0":
            case "exit":
            case "quit":
                return true;

            // ═════════════════════════════════════════════════════════════
            // INVALID COMMAND
            // ═════════════════════════════════════════════════════════════
            default:
                Console.WriteLine($"\n❌ Invalid command: {command}");
                Console.WriteLine("Valid commands: 1-16, 0 (exit)");
                Console.WriteLine("Or use keywords: lowlevel, trading, streaming, service, sugar, etc.");
                Console.WriteLine("\nNote: Command [16] usercode is commented out by default.");
                Console.WriteLine("      Uncomment in Program.cs to enable your custom sandbox code.");
                return false;
        }
    }

    // ═════════════════════════════════════════════════════════════════
    // ORCHESTRATOR RUNNERS
    // ═════════════════════════════════════════════════════════════════

    private static async Task RunOrchestrator_Grid()
    {
        var config = ConnectionHelper.BuildConfiguration();
        var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);

        try
        {
            var service = new MT5Service(account);

            var orchestrator = new GridTradingOrchestrator(service)
            {
                Symbol = "EURUSD",
                GridLevels = 3,
                GridSpacingPoints = 20,
                VolumePerLevel = 0.01,
                StopLossPoints = 50,
                TakeProfitPoints = 30,
                MaxRunMinutes = 15
            };

            await orchestrator.ExecuteAsync();
        }
        finally
        {
            // Cleanup: properly shutdown gRPC channel to avoid resource leaks
            await account.GrpcChannel.ShutdownAsync();
        }
    }

    private static async Task RunOrchestrator_News()
    {
        var config = ConnectionHelper.BuildConfiguration();
        var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);

        try
        {
            var service = new MT5Service(account);

            var orchestrator = new NewsStraddleOrchestrator(service)
            {
                Symbol = "EURUSD",
                StraddleDistancePoints = 15,
                Volume = 0.02,
                StopLossPoints = 20,
                TakeProfitPoints = 40,
                SecondsBeforeNews = 60,
                MaxWaitAfterNewsSeconds = 180
            };

            await orchestrator.ExecuteAsync();
        }
        finally
        {
            // Cleanup: properly shutdown gRPC channel to avoid resource leaks
            await account.GrpcChannel.ShutdownAsync();
        }
    }

    private static async Task RunOrchestrator_Breakout()
    {
        var config = ConnectionHelper.BuildConfiguration();
        var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);

        try
        {
            var service = new MT5Service(account);

            var orchestrator = new PendingBreakoutOrchestrator(service)
            {
                Symbol = "EURUSD",
                BreakoutDistancePoints = 25,
                Volume = 0.01,
                StopLossPoints = 15,
                TakeProfitPoints = 30,
                MaxWaitMinutes = 30
            };

            await orchestrator.ExecuteAsync();
        }
        finally
        {
            // Cleanup: properly shutdown gRPC channel to avoid resource leaks
            await account.GrpcChannel.ShutdownAsync();
        }
    }

    private static async Task RunOrchestrator_Hedge()
    {
        var config = ConnectionHelper.BuildConfiguration();
        var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);

        try
        {
            var service = new MT5Service(account);

            var orchestrator = new QuickHedgeOrchestrator(service)
            {
                Symbol = "EURUSD",
                RiskAmount = 30.0,
                StopLossPoints = 25,
                TakeProfitPoints = 40,
                OpenBuyFirst = true,
                HedgeTriggerPoints = 15
            };

            await orchestrator.ExecuteAsync();
        }
        finally
        {
            // Cleanup: properly shutdown gRPC channel to avoid resource leaks
            await account.GrpcChannel.ShutdownAsync();
        }
    }

    private static async Task RunOrchestrator_Scalping()
    {
        var config = ConnectionHelper.BuildConfiguration();
        var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);

        try
        {
            var service = new MT5Service(account);

            var orchestrator = new SimpleScalpingOrchestrator(service)
            {
                Symbol = "EURUSD",
                RiskAmount = 20.0,
                StopLossPoints = 10,
                TakeProfitPoints = 20,
                IsBuy = true,
                MaxHoldSeconds = 60
            };

            await orchestrator.ExecuteAsync();
        }
        finally
        {
            // Cleanup: properly shutdown gRPC channel to avoid resource leaks
            await account.GrpcChannel.ShutdownAsync();
        }
    }

    private static async Task RunPreset_Adaptive()
    {
        var config = ConnectionHelper.BuildConfiguration();
        var account = await ConnectionHelper.CreateAndConnectAccountAsync(config);

        try
        {
            var service = new MT5Service(account);

            var preset = new MetaRPC.CSharpMT5.Examples.Presets.AdaptiveMarketModePreset(service)
            {
                Symbol = "EURUSD",
                BaseRiskAmount = 20.0,
                LowVolatilityThreshold = 15.0,
                HighVolatilityThreshold = 40.0,
                EnableNewsMode = true
            };

            await preset.ExecuteAsync();
        }
        finally
        {
            // Cleanup: properly shutdown gRPC channel to avoid resource leaks
            await account.GrpcChannel.ShutdownAsync();
        }
    }
}
