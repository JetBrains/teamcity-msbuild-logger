namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;
    using IoC;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class PerformanceCounterFactory: IPerformanceCounterFactory
    {
        private readonly Func<string, IPerformanceCounter> _performanceCounterFactory;

        public PerformanceCounterFactory(
            [NotNull] Func<string, IPerformanceCounter> performanceCounterFactory)
        {
            _performanceCounterFactory = performanceCounterFactory ?? throw new ArgumentNullException(nameof(performanceCounterFactory));
        }

        public IPerformanceCounter GetOrCreatePerformanceCounter(string scopeName, IDictionary<string, IPerformanceCounter> performanceCounters)
        {
            if (scopeName == null) throw new ArgumentNullException(nameof(scopeName));
            if (!performanceCounters.TryGetValue(scopeName, out IPerformanceCounter performanceCounter))
            {
                performanceCounter = _performanceCounterFactory(scopeName);
                performanceCounters.Add(scopeName, performanceCounter);
            }

            return performanceCounter;
        }
    }
}
