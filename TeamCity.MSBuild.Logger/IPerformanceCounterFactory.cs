namespace TeamCity.MSBuild.Logger
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    internal interface IPerformanceCounterFactory
    {
        [NotNull]
        IPerformanceCounter GetOrCreatePerformanceCounter([NotNull] string scopeName, IDictionary<string, IPerformanceCounter> performanceCounters);
    }
}
