namespace TeamCity.MSBuild.Logger.EventHandlers
{
    using System;
    using Microsoft.Build.Framework;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class TargetStartedHandler : IBuildEventHandler<TargetStartedEventArgs>
    {
        [NotNull] private readonly IBuildEventManager _buildEventManager;
        [NotNull] private readonly ILoggerContext _context;
        [NotNull] private readonly IPerformanceCounterFactory _performanceCounterFactory;

        public TargetStartedHandler(
            [NotNull] ILoggerContext context,
            [NotNull] IPerformanceCounterFactory performanceCounterFactory,
            [NotNull] IBuildEventManager buildEventManager)
        {
            _buildEventManager = buildEventManager ?? throw new ArgumentNullException(nameof(buildEventManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _performanceCounterFactory = performanceCounterFactory ?? throw new ArgumentNullException(nameof(performanceCounterFactory));
        }

        public void Handle(TargetStartedEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (e.BuildEventContext == null) throw new ArgumentException(nameof(e));
            _buildEventManager.AddTargetStartedEvent(e, _context.Parameters.ShowTimeStamp || _context.IsVerbosityAtLeast(LoggerVerbosity.Detailed));
            if (_context.Parameters.ShowPerfSummary)
            {
                _performanceCounterFactory.GetOrCreatePerformanceCounter(e.TargetName, _context.TargetPerformanceCounters).AddEventStarted(null, e.BuildEventContext, e.Timestamp, ComparerContextNodeIdTargetId.Shared);
            }
        }
    }
}
