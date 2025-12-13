/*══════════════════════════════════════════════════════════════════════════════
 FILE: Examples/Helpers/ProgressBarHelper.cs
 PURPOSE:
   Utility class for displaying console progress bars during time-based waits.
   Used by orchestrators to visualize countdowns and waiting periods.

 USAGE EXAMPLE:
   // Wait 60 seconds with progress bar
   await ProgressBarHelper.WaitWithProgressBar(
       totalSeconds: 60,
       message: "Waiting for news event",
       ct: cancellationToken
   );

   Output:
   Waiting for news event: [█████████░░░░░░░░░░] 45% (27s / 60s)

══════════════════════════════════════════════════════════════════════════════*/

using System;
using System.Threading;
using System.Threading.Tasks;

namespace MetaRPC.CSharpMT5.Examples.Helpers
{
    public static class ProgressBarHelper
    {
        /// <summary>
        /// Displays a progress bar while waiting for a specified duration.
        /// Updates every second with visual progress indicator.
        /// </summary>
        /// <param name="totalSeconds">Total seconds to wait</param>
        /// <param name="message">Message to display before progress bar</param>
        /// <param name="ct">Cancellation token</param>
        public static async Task WaitWithProgressBar(
            int totalSeconds,
            string message = "Waiting",
            CancellationToken ct = default)
        {
            const int barWidth = 20; // Width of progress bar in characters

            for (int elapsed = 0; elapsed <= totalSeconds; elapsed++)
            {
                if (ct.IsCancellationRequested)
                    break;

                // Calculate progress
                double progress = (double)elapsed / totalSeconds;
                int filledWidth = (int)(progress * barWidth);
                int emptyWidth = barWidth - filledWidth;

                // Build progress bar
                string bar = new string('█', filledWidth) + new string('░', emptyWidth);
                int percent = (int)(progress * 100);
                int remaining = totalSeconds - elapsed;

                // Build output string
                string output = $"  {message}: [{bar}] {percent}% ({elapsed}s / {totalSeconds}s) - {remaining}s remaining";

                // Clear current line completely (fill with spaces) then write progress
                int consoleWidth = 120; // Safe default width
                try { consoleWidth = Console.WindowWidth; } catch { }
                string clearLine = new string(' ', consoleWidth);
                Console.Write($"\r{clearLine}\r{output}");

                // Wait 1 second before next update (unless it's the last iteration)
                if (elapsed < totalSeconds)
                {
                    await Task.Delay(1000, ct);
                }
            }

            // Move to next line after completion
            Console.WriteLine();
        }

        /// <summary>
        /// Displays a simple countdown without progress bar.
        /// </summary>
        /// <param name="totalSeconds">Total seconds to count down</param>
        /// <param name="message">Message to display before countdown</param>
        /// <param name="ct">Cancellation token</param>
        public static async Task CountdownWithoutBar(
            int totalSeconds,
            string message = "Countdown",
            CancellationToken ct = default)
        {
            for (int remaining = totalSeconds; remaining >= 0; remaining--)
            {
                if (ct.IsCancellationRequested)
                    break;

                // Build output string
                string output = $"  {message}: {remaining}s remaining...";

                // Clear current line completely then write
                int consoleWidth = 120;
                try { consoleWidth = Console.WindowWidth; } catch { }
                var clearLine = new string(' ', consoleWidth);
                Console.Write($"\r{clearLine}\r{output}");

                if (remaining > 0)
                {
                    await Task.Delay(1000, ct);
                }
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Displays a monitoring progress bar for operations with unknown duration.
        /// Updates periodically and shows elapsed time.
        /// </summary>
        /// <param name="maxSeconds">Maximum seconds to monitor</param>
        /// <param name="message">Message to display</param>
        /// <param name="updateIntervalMs">Update interval in milliseconds</param>
        /// <param name="ct">Cancellation token</param>
        public static async Task MonitorWithProgressBar(
            int maxSeconds,
            string message = "Monitoring",
            int updateIntervalMs = 5000,
            CancellationToken ct = default)
        {
            var startTime = DateTime.UtcNow;

            while (!ct.IsCancellationRequested)
            {
                var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;

                if (elapsed >= maxSeconds)
                    break;

                double progress = elapsed / maxSeconds;
                int percent = (int)(progress * 100);
                int remaining = maxSeconds - (int)elapsed;

                // Build output string
                string output = $"  {message}: {(int)elapsed}s elapsed / {maxSeconds}s max - {remaining}s remaining";

                // Clear current line completely then write
                int consoleWidth = 120;
                try { consoleWidth = Console.WindowWidth; } catch { }
                var clearLine = new string(' ', consoleWidth);
                Console.Write($"\r{clearLine}\r{output}");

                await Task.Delay(updateIntervalMs, ct);
            }

            Console.WriteLine();
        }
    }
}
