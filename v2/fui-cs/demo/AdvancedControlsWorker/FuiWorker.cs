using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

using FuiCs.Workers;

namespace FuiCs.Demo.AdvancedControlsWorker;

/// <summary>The advanced controls route's worker jobs.</summary>
public static class FuiWorker
{
    public static void Register(WorkerEntries entries) => entries
        .Add("largestPrimeCalculatorWorker", input => new LargestPrimeCalculatorJob().Start(input));

    /// <summary>
    /// Searches for primes for five seconds, reporting the elapsed percentage and yielding once a second; a cancel takes
    /// effect at the next yield and fails the job with <c>cancelled:</c> and the percentage reached.
    /// </summary>
    private sealed class LargestPrimeCalculatorJob : WorkerJob
    {
        private static readonly TimeSpan Total = TimeSpan.FromSeconds(5.0);
        private static readonly TimeSpan YieldInterval = TimeSpan.FromSeconds(1.0);
        private const int TimeCheckMask = 127;

        protected override async Task Run()
        {
            var clock = Stopwatch.StartNew();
            var nextYieldAt = YieldInterval;
            var candidate = 2;
            var largestPrime = 2;
            while (true)
            {
                if (IsCancelled)
                {
                    Fail("cancelled:" + Percent(clock.Elapsed).ToString(CultureInfo.InvariantCulture));
                    return;
                }
                var sliceEnd = nextYieldAt < Total ? nextYieldAt : Total;
                var now = clock.Elapsed;
                while (now < sliceEnd)
                {
                    if (IsPrime(candidate))
                    {
                        largestPrime = candidate;
                    }
                    candidate += 1;
                    if ((candidate & TimeCheckMask) == 0)
                    {
                        now = clock.Elapsed;
                    }
                }
                now = clock.Elapsed;
                ReportProgress(Percent(now).ToString(CultureInfo.InvariantCulture));
                if (now >= Total)
                {
                    Complete(largestPrime.ToString(CultureInfo.InvariantCulture));
                    return;
                }
                nextYieldAt += YieldInterval;
                if (nextYieldAt > Total)
                {
                    nextYieldAt = Total;
                }
                await YieldAsync();
            }
        }

        private static int Percent(TimeSpan elapsed) =>
            elapsed <= TimeSpan.Zero ? 0 : elapsed >= Total ? 100 : (int)(elapsed.TotalMilliseconds * 100.0 / Total.TotalMilliseconds);

        private static bool IsPrime(int value)
        {
            if (value < 2)
            {
                return false;
            }
            if (value == 2)
            {
                return true;
            }
            if ((value & 1) == 0)
            {
                return false;
            }
            for (var divisor = 3; divisor <= value / divisor; divisor += 2)
            {
                if (value % divisor == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
