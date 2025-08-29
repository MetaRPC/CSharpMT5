using System;

namespace MetaRPC.CSharpMT5
{
    // English-only comments (as requested)
    internal static class Validators
    {
        public static void EnsureProfile(string? profile)
        {
            if (string.IsNullOrWhiteSpace(profile))
                throw new ArgumentException("Profile name must be provided.");
        }

        public static string EnsureSymbol(string? symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("Symbol must be provided.");
            return symbol.Trim();
        }

        public static void EnsureVolume(double volume)
        {
            if (double.IsNaN(volume) || double.IsInfinity(volume) || volume <= 0)
                throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be a positive number.");
        }

        public static void EnsureDeviation(int deviation)
        {
            // Safe wide bounds; tighten later if you want
            if (deviation < 0 || deviation > 2000)
                throw new ArgumentOutOfRangeException(nameof(deviation), "Deviation must be between 0 and 2000.");
        }

        public static void EnsureTicket(ulong ticket)
        {
            if (ticket == 0)
                throw new ArgumentOutOfRangeException(nameof(ticket), "Ticket must be > 0.");
        }
    }
}
