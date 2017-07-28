namespace TeamCity.MSBuild.Logger.EventHandlers
{
    using System;
    using Microsoft.Build.Framework;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ProjectFinishedHandler : IBuildEventHandler<ProjectFinishedEventArgs>
    {
        [NotNull] private readonly IHierarchicalMessageWriter _hierarchicalMessageWriter;
        [NotNull] private readonly IBuildEventManager _buildEventManager;
        [NotNull] private readonly IDeferredMessageWriter _deferredMessageWriter;
        [NotNull] private readonly IMessageWriter _messageWriter;
        [NotNull] private readonly ILoggerContext _context;
        [NotNull] private readonly IPerformanceCounterFactory _performanceCounterFactory;
        [NotNull] private readonly ILogWriter _logWriter;

        public ProjectFinishedHandler(
            [NotNull] ILoggerContext context,
            [NotNull] ILogWriter logWriter,
            [NotNull] IPerformanceCounterFactory performanceCounterFactory,
            [NotNull] IMessageWriter messageWriter,
            [NotNull] IHierarchicalMessageWriter hierarchicalMessageWriter,
            [NotNull] IDeferredMessageWriter deferredMessageWriter,
            [NotNull] IBuildEventManager buildEventManager)
        {
            _hierarchicalMessageWriter = hierarchicalMessageWriter ?? throw new ArgumentNullException(nameof(hierarchicalMessageWriter));
            _buildEventManager = buildEventManager ?? throw new ArgumentNullException(nameof(buildEventManager));
            _deferredMessageWriter = deferredMessageWriter ?? throw new ArgumentNullException(nameof(deferredMessageWriter));
            _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _performanceCounterFactory = performanceCounterFactory ?? throw new ArgumentNullException(nameof(performanceCounterFactory));
            _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
        }

        public void Handle(ProjectFinishedEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            var projectStartedEvent = _buildEventManager.GetProjectStartedEvent(e.BuildEventContext);
            if (projectStartedEvent == null) throw new ArgumentException($"Project finished event for {e.ProjectFile} received without matching start event", nameof(e));
            if (_context.Parameters.ShowPerfSummary)
            {
                _performanceCounterFactory.GetOrCreatePerformanceCounter(e.ProjectFile, _context.ProjectPerformanceCounters).AddEventFinished(projectStartedEvent.TargetNames, e.BuildEventContext, e.Timestamp);
            }

            if (_context.IsVerbosityAtLeast(LoggerVerbosity.Normal) && projectStartedEvent.ShowProjectFinishedEvent)
            {
                _context.LastProjectFullKey = _context.GetFullProjectKey(e.BuildEventContext);
                if (!_context.Parameters.ShowOnlyErrors && !_context.Parameters.ShowOnlyWarnings)
                {
                    _messageWriter.WriteLinePrefix(e.BuildEventContext, e.Timestamp, false);
                    _logWriter.SetColor(Color.BuildStage);
                    var targetNames = projectStartedEvent.TargetNames;
                    var projectFile = projectStartedEvent.ProjectFile ?? string.Empty;
                    if (string.IsNullOrEmpty(targetNames))
                    {
                        if (e.Succeeded)
                        {
                            _hierarchicalMessageWriter.FinishBlock(projectStartedEvent.HierarchicalKey, ResourceUtilities.FormatResourceString("ProjectFinishedPrefixWithDefaultTargetsMultiProc", projectFile));
                        }
                        else
                        {
                            _hierarchicalMessageWriter.FinishBlock(projectStartedEvent.HierarchicalKey, ResourceUtilities.FormatResourceString("ProjectFinishedPrefixWithDefaultTargetsMultiProcFailed", projectFile));
                        }
                    }
                    else
                    {
                        if (e.Succeeded)
                        {
                            _hierarchicalMessageWriter.FinishBlock(projectStartedEvent.HierarchicalKey, ResourceUtilities.FormatResourceString("ProjectFinishedPrefixWithTargetNamesMultiProc", projectFile, targetNames));
                        }
                        else
                        {
                            _hierarchicalMessageWriter.FinishBlock(projectStartedEvent.HierarchicalKey, ResourceUtilities.FormatResourceString("ProjectFinishedPrefixWithTargetNamesMultiProcFailed", projectFile, targetNames));
                        }
                    }
                }

                _deferredMessageWriter.ShownBuildEventContext(projectStartedEvent.ProjectBuildEventContext);
                _logWriter.ResetColor();
            }

            _buildEventManager.RemoveProjectStartedEvent(e.BuildEventContext);
        }
    }
}
