namespace TeamCity.MSBuild.Logger.EventHandlers
{
    using System;
    using Microsoft.Build.Framework;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class TaskStartedHandler : IBuildEventHandler<TaskStartedEventArgs>
    {
        [NotNull] private readonly IStringService _stringService;
        [NotNull] private readonly IDeferredMessageWriter _deferredMessageWriter;
        [NotNull] private readonly IMessageWriter _messageWriter;
        [NotNull] private readonly ILoggerContext _context;
        [NotNull] private readonly ILogWriter _logWriter;
        [NotNull] private readonly IPerformanceCounterFactory _performanceCounterFactory;

        public TaskStartedHandler(
            [NotNull] ILoggerContext context,
            [NotNull] ILogWriter logWriter,
            [NotNull] IPerformanceCounterFactory performanceCounterFactory,
            [NotNull] IMessageWriter messageWriter,
            [NotNull] IDeferredMessageWriter deferredMessageWriter,
            [NotNull] IStringService stringService)
        {
            _stringService = stringService ?? throw new ArgumentNullException(nameof(stringService));
            _deferredMessageWriter = deferredMessageWriter ?? throw new ArgumentNullException(nameof(deferredMessageWriter));
            _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
            _performanceCounterFactory = performanceCounterFactory ?? throw new ArgumentNullException(nameof(performanceCounterFactory));
        }

        public void Handle(TaskStartedEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (e.BuildEventContext == null) throw new ArgumentException(nameof(e));
            if (_context.IsVerbosityAtLeast(LoggerVerbosity.Detailed))
            {
                _deferredMessageWriter.DisplayDeferredStartedEvents(e.BuildEventContext);
                if (!_context.Parameters.ShowOnlyErrors && !_context.Parameters.ShowOnlyWarnings)
                {
                    var prefixAlreadyWritten = _messageWriter.WriteTargetMessagePrefix(e, e.BuildEventContext, e.Timestamp);
                    _logWriter.SetColor(Color.Task);
                    if (_context.IsVerbosityAtLeast(LoggerVerbosity.Diagnostic) || (_context.Parameters.ShowEventId ?? false))
                    {
                        _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("TaskMessageWithId", (object)e.Message, (object)e.BuildEventContext.TaskId), prefixAlreadyWritten);
                    }
                    else
                    {
                        _messageWriter.WriteMessageAligned(e.Message, prefixAlreadyWritten);
                    }

                    _logWriter.ResetColor();
                }

                _deferredMessageWriter.ShownBuildEventContext(e.BuildEventContext);
            }

            if (!_context.Parameters.ShowPerfSummary)
            {
                return;
            }

            _performanceCounterFactory.GetOrCreatePerformanceCounter(e.TaskName, _context.TaskPerformanceCounters).AddEventStarted(null, e.BuildEventContext, e.Timestamp, null);
        }
    }
}
