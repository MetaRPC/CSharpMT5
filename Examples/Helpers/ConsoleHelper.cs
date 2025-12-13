/*══════════════════════════════════════════════════════════════════════════════
 FILE: Examples/Helpers/ConsoleHelper.cs
 PURPOSE:
   Shared console output helpers for all example programs.
   Provides banner printing, section headers, colored messages.
══════════════════════════════════════════════════════════════════════════════*/

using System;

namespace MetaRPC.CSharpMT5.Examples.Helpers
{
    public static class ConsoleHelper
    {
        // ═════════════════════════════════════════════════════════════════
        // SECTION HEADERS
        // ═════════════════════════════════════════════════════════════════

        public static void PrintSection(string title)
        {
            Console.WriteLine($"\n╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║ {title,-64} ║");
            Console.WriteLine($"╚══════════════════════════════════════════════════════════════════╝");
        }

        public static void PrintSectionWithNewline(string title)
        {
            Console.WriteLine($"\n╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║ {title,-64} ║");
            Console.WriteLine($"╚══════════════════════════════════════════════════════════════════╝\n");
        }

        // ═════════════════════════════════════════════════════════════════
        // COLORED MESSAGES
        // ═════════════════════════════════════════════════════════════════

        public static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void PrintInfo(string message)
        {
            Console.WriteLine($"  {message}");
        }

        public static void PrintWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
