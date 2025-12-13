/*══════════════════════════════════════════════════════════════════════════════
 FILE: Examples/Helpers/ProtobufInspector.cs — INTERACTIVE PROTOBUF TYPES INSPECTOR
 PURPOSE:
   Interactive developer utility to explore MetaRPC.MT5 protobuf types, fields,
   enums, and data structures from the MT5 gRPC API.

 🎯 WHAT THIS TOOL DOES:
   • Interactive search for types, fields, and enums
   • Real-time inspection of protobuf message structures
   • Field-level discovery (find which types contain specific fields)
   • Enum value exploration (see all possible values)
   • Type browsing (list all available types)

 📖 HOW TO USE:

   1. START THE INSPECTOR:
      dotnet run inspect

   2. AVAILABLE COMMANDS:

      ┌─────────────────────────────────────────────────────────────────────┐
      │ COMMAND          │ DESCRIPTION                                      │
      ├─────────────────────────────────────────────────────────────────────┤
      │ list             │ List all available protobuf types                │
      │ ls               │ (alias for list)                                 │
      ├─────────────────────────────────────────────────────────────────────┤
      │ <TypeName>       │ Inspect specific type (e.g., "OpenedOrdersData") │
      │                  │ Shows all properties and their types             │
      ├─────────────────────────────────────────────────────────────────────┤
      │ search <text>    │ Search for types containing text                 │
      │ find <text>      │ (e.g., "search Order" finds all Order* types)    │
      ├─────────────────────────────────────────────────────────────────────┤
      │ field <name>     │ Find all types containing a specific field       │
      │                  │ (e.g., "field Balance" → AccountSummaryData)     │
      ├─────────────────────────────────────────────────────────────────────┤
      │ enum <name>      │ Show all values of an enum                       │
      │                  │ (e.g., "enum BMT5_ENUM_ORDER_TYPE")              │
      ├─────────────────────────────────────────────────────────────────────┤
      │ help             │ Show this help message                           │
      │ ?                │ (alias for help)                                 │
      ├─────────────────────────────────────────────────────────────────────┤
      │ exit             │ Exit the inspector                               │
      │ quit             │ (alias for exit)                                 │
      └─────────────────────────────────────────────────────────────────────┘

 💡 PRACTICAL EXAMPLES:

   Example 1: Find out what fields AccountSummaryData has
   > AccountSummaryData
   Output:
     AccountBalance: double
     AccountEquity: double
     AccountProfit: double
     ...

   Example 2: Find which type has the "Ticket" field
   > field Ticket
   Output:
     Found in: OpenedOrderInfo, PositionInfo, OrderCloseRequest, ...

   Example 3: See all ORDER_TYPE values
   > enum ENUM_ORDER_TYPE
   Output:
     ORDER_TYPE_BUY = 0
     ORDER_TYPE_SELL = 1
     ORDER_TYPE_BUY_LIMIT = 2
     ...

   Example 4: Find all types related to "Position"
   > search Position
   Output:
     PositionInfo
     PositionsTotalData
     PositionsHistoryData
     ...

   Example 5: List all available types
   > list
   Output:
     [Shows all protobuf Data/Request types]

 🔍 COMMON USE CASES:

   USE CASE 1: "I'm getting 'field not found' error"
   → Use: field <fieldname>
   → Example: field FreeMargin
   → Result: Shows you the correct field name and which type has it

   USE CASE 2: "What properties does X have?"
   → Use: <TypeName>
   → Example: OpenedOrdersData
   → Result: Lists all properties (OpenedOrders, PositionInfos, etc.)

   USE CASE 3: "What are valid enum values?"
   → Use: enum <EnumName>
   → Example: enum BMT5_ENUM_ORDER_TYPE
   → Result: Shows all values with their numeric codes

   USE CASE 4: "I need to find types related to orders"
   → Use: search Order
   → Result: Lists all types with "Order" in the name

   USE CASE 5: "I want to browse what's available"
   → Use: list
   → Result: Shows all available types to explore

 USAGE:
   dotnet run inspect
   dotnet run 7

══════════════════════════════════════════════════════════════════════════════*/

using System;
using System.Linq;
using System.Reflection;
using mt5_term_api;

namespace MetaRPC.CSharpMT5.Examples.Helpers
{
    /// <summary>
    /// Interactive Protobuf Types Inspector
    /// Explore MT5 gRPC API types, fields, and enums interactively
    /// </summary>
    public static class ProtobufInspector
    {
        private static readonly Assembly Assembly = typeof(AccountSummaryData).Assembly;

        // ═════════════════════════════════════════════════════════════════
        // MAIN ENTRY POINT
        // ═════════════════════════════════════════════════════════════════

        public static void Run()
        {
            PrintHeader();
            PrintQuickStart();

            // Interactive loop
            while (true)
            {
                Console.Write("\n> ");
                var input = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(input))
                    continue;

                var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var command = parts[0].ToLower();
                var arg = parts.Length > 1 ? parts[1] : "";

                switch (command)
                {
                    case "exit":
                    case "quit":
                    case "q":
                        Console.WriteLine("\n👋 Goodbye!");
                        return;

                    case "help":
                    case "?":
                        PrintHelp();
                        break;

                    case "list":
                    case "ls":
                        ListAllTypes();
                        break;

                    case "search":
                    case "find":
                        if (string.IsNullOrEmpty(arg))
                            Console.WriteLine("❌ Usage: search <text>");
                        else
                            SearchTypes(arg);
                        break;

                    case "field":
                        if (string.IsNullOrEmpty(arg))
                            Console.WriteLine("❌ Usage: field <fieldname>");
                        else
                            FindField(arg);
                        break;

                    case "enum":
                        if (string.IsNullOrEmpty(arg))
                            Console.WriteLine("❌ Usage: enum <enumname>");
                        else
                            InspectEnum(arg);
                        break;

                    default:
                        // Assume it's a type name
                        InspectType(input);
                        break;
                }
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // COMMAND IMPLEMENTATIONS
        // ═════════════════════════════════════════════════════════════════

        private static void ListAllTypes()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              ALL PROTOBUF TYPES                            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");

            var types = Assembly.GetTypes()
                .Where(t => t.Namespace == "mt5_term_api" &&
                           t.IsPublic &&
                           (t.Name.EndsWith("Data") ||
                            t.Name.EndsWith("Request") ||
                            t.Name.EndsWith("Reply")))
                .OrderBy(t => t.Name)
                .ToList();

            Console.WriteLine($"\n📦 Found {types.Count} types:\n");

            foreach (var type in types)
            {
                var category = type.Name.EndsWith("Data") ? "[Data]   " :
                              type.Name.EndsWith("Request") ? "[Request]" :
                              "[Reply]  ";
                Console.WriteLine($"  {category} {type.Name}");
            }

            Console.WriteLine($"\n💡 Type a name to inspect it (e.g., 'OpenedOrdersData')");
        }

        private static void SearchTypes(string searchTerm)
        {
            Console.WriteLine($"\n🔍 Searching for types containing '{searchTerm}'...\n");

            var types = Assembly.GetTypes()
                .Where(t => t.Namespace == "mt5_term_api" &&
                           t.IsPublic &&
                           t.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderBy(t => t.Name)
                .ToList();

            if (types.Count == 0)
            {
                Console.WriteLine($"❌ No types found containing '{searchTerm}'");
                return;
            }

            Console.WriteLine($"✅ Found {types.Count} type(s):\n");

            foreach (var type in types)
            {
                var kind = type.IsEnum ? "[Enum]" :
                          type.IsClass ? "[Class]" :
                          "[Struct]";
                Console.WriteLine($"  {kind,-8} {type.Name}");
            }

            Console.WriteLine($"\n💡 Type a name to inspect it");
        }

        private static void FindField(string fieldName)
        {
            Console.WriteLine($"\n🔍 Searching for field '{fieldName}'...\n");

            var typesWithField = Assembly.GetTypes()
                .Where(t => t.Namespace == "mt5_term_api" && t.IsPublic && t.IsClass)
                .Where(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Any(p => p.Name.Contains(fieldName, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(t => t.Name)
                .ToList();

            if (typesWithField.Count == 0)
            {
                Console.WriteLine($"❌ No types found with field containing '{fieldName}'");
                return;
            }

            Console.WriteLine($"✅ Found in {typesWithField.Count} type(s):\n");

            foreach (var type in typesWithField)
            {
                var matchingProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.Name.Contains(fieldName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                Console.WriteLine($"📦 {type.Name}:");
                foreach (var prop in matchingProps)
                {
                    Console.WriteLine($"   └─ {prop.Name}: {GetTypeName(prop.PropertyType)}");
                }
                Console.WriteLine();
            }
        }

        private static void InspectEnum(string enumName)
        {
            var type = Assembly.GetTypes()
                .FirstOrDefault(t => t.Namespace == "mt5_term_api" &&
                                    t.IsPublic &&
                                    t.IsEnum &&
                                    t.Name.Equals(enumName, StringComparison.OrdinalIgnoreCase));

            if (type == null)
            {
                Console.WriteLine($"\n❌ Enum '{enumName}' not found");
                Console.WriteLine($"💡 Try: enum BMT5_ENUM_ORDER_TYPE");
                return;
            }

            Console.WriteLine($"\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║ ENUM: {type.Name,-51}                                        ║");
            Console.WriteLine($"╚════════════════════════════════════════════════════════════╝\n");

            var values = Enum.GetValues(type);
            var names = Enum.GetNames(type);

            for (int i = 0; i < values.Length; i++)
            {
                var value = Convert.ToInt32(values.GetValue(i));
                Console.WriteLine($"  {names[i],-50} = {value}");
            }
        }

        private static void InspectType(string typeName)
        {
            // Try exact match first
            var type = Assembly.GetType($"mt5_term_api.{typeName}");

            // Try case-insensitive search
            if (type == null)
            {
                type = Assembly.GetTypes()
                    .FirstOrDefault(t => t.Namespace == "mt5_term_api" &&
                                        t.IsPublic &&
                                        t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
            }

            if (type == null)
            {
                Console.WriteLine($"\n❌ Type '{typeName}' not found");
                Console.WriteLine($"💡 Try: 'search {typeName}' to find similar types");
                Console.WriteLine($"💡 Or:  'list' to see all available types");
                return;
            }

            if (type.IsEnum)
            {
                InspectEnum(type.Name);
                return;
            }

            Console.WriteLine($"\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║ TYPE: {type.Name,-51} ║");
            Console.WriteLine($"╚════════════════════════════════════════════════════════════╝\n");

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(p => p.Name)
                .ToList();

            if (props.Count == 0)
            {
                Console.WriteLine("  (no public properties)");
                return;
            }

            Console.WriteLine($"📋 Properties ({props.Count}):\n");

            foreach (var prop in props)
            {
                var typeName2 = GetTypeName(prop.PropertyType);
                var isCollection = prop.PropertyType.IsGenericType &&
                                  prop.PropertyType.GetGenericTypeDefinition().Name.Contains("RepeatedField");

                var prefix = isCollection ? "  📚" : "  •";
                Console.WriteLine($"{prefix} {prop.Name,-40} : {typeName2}");
            }

            Console.WriteLine($"\n💡 To see field values of an enum, use: enum <EnumName>");
        }

        // ═════════════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═════════════════════════════════════════════════════════════════

        private static string GetTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();
                var genericArgs = type.GetGenericArguments();

                if (genericDef.Name.Contains("RepeatedField"))
                {
                    return $"List<{GetTypeName(genericArgs[0])}>";
                }

                var baseName = genericDef.Name.Split('`')[0];
                var argNames = string.Join(", ", genericArgs.Select(GetTypeName));
                return $"{baseName}<{argNames}>";
            }

            if (type.IsArray)
            {
                return $"{GetTypeName(type.GetElementType()!)}[]";
            }

            // Simplify full names
            return type.Name switch
            {
                "Int32" => "int",
                "Int64" => "long",
                "Double" => "double",
                "Single" => "float",
                "String" => "string",
                "Boolean" => "bool",
                _ => type.Name
            };
        }

        // ═════════════════════════════════════════════════════════════════
        // UI HELPERS
        // ═════════════════════════════════════════════════════════════════

        private static void PrintHeader()
        {
            try { Console.Clear(); } catch { /* Ignore if console not available */ }
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                                    ║");
            Console.WriteLine("║          🔍 INTERACTIVE PROTOBUF TYPES INSPECTOR 🔍                ║");
            Console.WriteLine("║                                                                    ║");
            Console.WriteLine("║         Explore MT5 gRPC API Types, Fields & Enums                ║");
            Console.WriteLine("║                                                                    ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine($"\nAssembly: {Assembly.GetName().Name} v{Assembly.GetName().Version}");
        }

        private static void PrintQuickStart()
        {
            Console.WriteLine("\n┌────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ QUICK START GUIDE                                              │");
            Console.WriteLine("├────────────────────────────────────────────────────────────────┤");
            Console.WriteLine("│ • Type 'help' for full command list                            │");
            Console.WriteLine("│ • Type 'list' to see all available types                       │");
            Console.WriteLine("│ • Type a type name to inspect it (e.g., 'OpenedOrdersData')    │");
            Console.WriteLine("│ • Type 'exit' to quit                                          │");
            Console.WriteLine("└────────────────────────────────────────────────────────────────┘");
        }

        private static void PrintHelp()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        COMMAND REFERENCE                           ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("📋 BROWSING COMMANDS:");
            Console.WriteLine("  list, ls              - List all available protobuf types");
            Console.WriteLine("  <TypeName>            - Inspect specific type (e.g., OpenedOrdersData)");
            Console.WriteLine();
            Console.WriteLine("🔍 SEARCH COMMANDS:");
            Console.WriteLine("  search <text>         - Find types containing text");
            Console.WriteLine("  find <text>           - (alias for search)");
            Console.WriteLine("  field <name>          - Find types with specific field");
            Console.WriteLine("  enum <name>           - Show enum values");
            Console.WriteLine();
            Console.WriteLine("ℹ️  UTILITY COMMANDS:");
            Console.WriteLine("  help, ?               - Show this help");
            Console.WriteLine("  exit, quit, q         - Exit inspector");
            Console.WriteLine();
            Console.WriteLine("💡 EXAMPLES:");
            Console.WriteLine("  > list                     # See all types");
            Console.WriteLine("  > OpenedOrdersData         # Inspect OpenedOrdersData");
            Console.WriteLine("  > search Order             # Find types with 'Order'");
            Console.WriteLine("  > field Ticket             # Find types with Ticket field");
            Console.WriteLine("  > enum BMT5_ENUM_ORDER_TYPE  # Show order type values");
            Console.WriteLine();
            Console.WriteLine("🎯 COMMON USE CASES:");
            Console.WriteLine("  • 'field not found' error  → Use: field <fieldname>");
            Console.WriteLine("  • Need enum values         → Use: enum <EnumName>");
            Console.WriteLine("  • Explore available types  → Use: list");
            Console.WriteLine("  • Find related types       → Use: search <keyword>");
        }
    }
}

/*══════════════════════════════════════════════════════════════════════════════
 USAGE EXAMPLES:

 Start the inspector:
   dotnet run inspect

 Interactive session:
   > list
   [Shows all types]

   > OpenedOrdersData
   [Shows: OpenedOrders, PositionInfos]

   > field Balance
   [Shows: AccountSummaryData.AccountBalance]

   > enum BMT5_ENUM_ORDER_TYPE
   [Shows: Bmt5OrderTypeBuy = 0, Bmt5OrderTypeSell = 1, ...]

   > search Position
   [Shows: PositionInfo, PositionsTotalData, ...]

   > exit
   [Quits inspector]

══════════════════════════════════════════════════════════════════════════════*/
