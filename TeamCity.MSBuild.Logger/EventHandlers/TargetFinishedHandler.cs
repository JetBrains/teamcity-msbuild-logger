namespace TeamCity.MSBuild.Logger.EventHandlers
{
    using System;
    using System.Collections;
    using Microsoft.Build.Framework;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class TargetFinishedHandler : IBuildEventHandler<TargetFinishedEventArgs>
    {
        [NotNull] private readonly IHierarchicalMessageWriter _hierarchicalMessageWriter;
        [NotNull] private readonly IBuildEventManager _buildEventManager;
        [NotNull] private readonly IDeferredMessageWriter _deferredMessageWriter;
        [NotNull] private readonly IMessageWriter _messageWriter;
        [NotNull] private readonly ILoggerContext _context;
        [NotNull] private readonly ILogWriter _logWriter;
        [NotNull] private readonly IPerformanceCounterFactory _performanceCounterFactory;

        public TargetFinishedHandler(
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
            _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
            _performanceCounterFactory = performanceCounterFactory ?? throw new ArgumentNullException(nameof(performanceCounterFactory));
        }

        public void Handle(TargetFinishedEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (e.BuildEventContext == null) throw new ArgumentException(nameof(e));
            if (_context.Parameters.ShowPerfSummary)
            {
                _performanceCounterFactory.GetOrCreatePerformanceCounter(e.TargetName, _context.TargetPerformanceCounters).AddEventFinished(null, e.BuildEventContext, e.Timestamp);
            }

            if (_context.IsVerbosityAtLeast(LoggerVerbosity.Detailed))
            {
                _deferredMessageWriter.DisplayDeferredTargetStartedEvent(e.BuildEventContext);
                var targetStartedEvent = _buildEventManager.GetTargetStartedEvent(e.BuildEventContext);
                if (targetStartedEvent == null) throw new ArgumentNullException("Started event should not be null in the finished event handler");
                if (targetStartedEvent.ShowTargetFinishedEvent)
                {
                    if (_context.Parameters.ShowTargetOutputs)
                    {
                        var targetOutputs = e.TargetOutputs;
                        if (targetOutputs != null)
                        {
                            _messageWriter.WriteMessageAligned(ResourceUtilities.FormatResourceString("TargetOutputItemsHeader"), false);
                            foreach (ITaskItem taskItem in targetOutputs)
                            {
                                _messageWriter.WriteMessageAligned(ResourceUtilities.FormatResourceString("TargetOutputItem", taskItem.ItemSpec), false);
                                foreach (DictionaryEntry dictionaryEntry in taskItem.CloneCustomMetadata())
                                {
                                    _messageWriter.WriteMessageAligned(new string(' ', 8) + dictionaryEntry.Key + " = " + taskItem.GetMetadata(dictionaryEntry.Key as string), false);
                                }
                            }
                        }
                    }

                    if (!_context.Parameters.ShowOnlyErrors && !_context.Parameters.ShowOnlyWarnings)
                    {
                        _context.LastProjectFullKey = _context.GetFullProjectKey(e.BuildEventContext);
                        _messageWriter.WriteLinePrefix(e.BuildEventContext, e.Timestamp, false);
                        _logWriter.SetColor(Color.BuildStage);
                        if (_context.IsVerbosityAtLeast(LoggerVerbosity.Diagnostic) || (_context.Parameters.ShowEventId ?? false))
                        {
                            _messageWriter.WriteMessageAligned(ResourceUtilities.FormatResourceString("TargetMessageWithId", (object)e.Message, (object)e.BuildEventContext.TargetId), true);
                        }
                        else
                        {
                            _messageWriter.WriteMessageAligned(e.Message, true);
                        }

                        _logWriter.ResetColor();
                    }

                    _deferredMessageWriter.ShownBuildEventContext(e.BuildEventContext);
                    _hierarchicalMessageWriter.FinishBlock(targetStartedEvent.HierarchicalKey);
                }
            }

            _buildEventManager.RemoveTargetStartedEvent(e.BuildEventContext);
        }
    }
}
