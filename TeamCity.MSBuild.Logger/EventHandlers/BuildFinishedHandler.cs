namespace TeamCity.MSBuild.Logger.EventHandlers
{
    using System;
    using Microsoft.Build.Framework;
    using System.Linq;
    using System.Collections.Generic;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class BuildFinishedHandler : IBuildEventHandler<BuildFinishedEventArgs>
    {
        [NotNull] private readonly IStringService _stringService;
        [NotNull] private readonly IHierarchicalMessageWriter _hierarchicalMessageWriter;
        [NotNull] private readonly IEventFormatter _eventFormatter;
        [NotNull] private readonly ILogFormatter _logFormatter;
        [NotNull] private readonly IBuildEventManager _buildEventManager;
        [NotNull] private readonly IMessageWriter _messageWriter;
        [NotNull] private readonly ILoggerContext _context;
        [NotNull] private readonly ILogWriter _logWriter;

        public BuildFinishedHandler(
            [NotNull] ILoggerContext context,
            [NotNull] ILogWriter logWriter,
            [NotNull] IMessageWriter messageWriter,
            [NotNull] IBuildEventManager buildEventManager,
            [NotNull] ILogFormatter logFormatter,
            [NotNull] IEventFormatter eventFormatter,
            [NotNull] IHierarchicalMessageWriter hierarchicalMessageWriter,
            [NotNull] IStringService stringService)
        {
            _stringService = stringService ?? throw new ArgumentNullException(nameof(stringService));
            _hierarchicalMessageWriter = hierarchicalMessageWriter ?? throw new ArgumentNullException(nameof(hierarchicalMessageWriter));
            _eventFormatter = eventFormatter ?? throw new ArgumentNullException(nameof(eventFormatter));
            _logFormatter = logFormatter ?? throw new ArgumentNullException(nameof(logFormatter));
            _buildEventManager = buildEventManager ?? throw new ArgumentNullException(nameof(buildEventManager));
            _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
        }

        public void Handle(BuildFinishedEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (!_context.Parameters.ShowOnlyErrors && !_context.Parameters.ShowOnlyWarnings && _context.DeferredMessages.Count > 0 && _context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
            {
                _messageWriter.WriteLinePrettyFromResource("DeferredMessages");
                foreach (var message in _context.DeferredMessages.Values.SelectMany(i => i))
                {
                    _messageWriter.PrintMessage(message, false);
                }
            }

            if (_context.Parameters.ShowPerfSummary)
            {
                ShowPerfSummary();
            }

            if (_context.IsVerbosityAtLeast(LoggerVerbosity.Normal) || (_context.Parameters.ShowSummary ?? false))
            {
                if (e.Succeeded)
                {
                    _logWriter.SetColor(Color.Success);
                }

                _messageWriter.WriteNewLine();
                _messageWriter.WriteLinePretty(e.Message);
                _logWriter.ResetColor();
            }

            if (_context.Parameters.ShowSummary ?? false)
            {
                if (_context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
                {
                    ShowNestedErrorWarningSummary();
                }
                else
                {
                    ShowFlatErrorWarningSummary();
                }

                if (_context.WarningCount > 0)
                {
                    _logWriter.SetColor(Color.Warning);
                }

                _messageWriter.WriteLinePrettyFromResource(2, "WarningCount", _context.WarningCount);
                _logWriter.ResetColor();
                if (_context.ErrorCount > 0)
                {
                    _logWriter.SetColor(Color.Error);
                }

                _messageWriter.WriteLinePrettyFromResource(2, "ErrorCount", _context.ErrorCount);
                _logWriter.ResetColor();
            }

            if (_context.IsVerbosityAtLeast(LoggerVerbosity.Normal) || (_context.Parameters.ShowSummary ?? false))
            {
                var str = _logFormatter.FormatTimeSpan(e.Timestamp - _context.BuildStarted);
                _messageWriter.WriteNewLine();
                _messageWriter.WriteLinePrettyFromResource("TimeElapsed", str);
            }

            _context.ResetConsoleLoggerState();
        }

        private void ShowFlatErrorWarningSummary()
        {
            if (_context.WarningList?.Count == 0 && _context.ErrorList?.Count == 0 || _context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings)
            {
                return;
            }

            _messageWriter.WriteNewLine();

            if (_context.WarningList != null && _context.WarningList.Count > 0)
            {
                _logWriter.SetColor(Color.Warning);
                foreach (var warning in _context.WarningList)
                {
                    _messageWriter.WriteMessageAligned(_eventFormatter.FormatEventMessage(warning, false, _context.Parameters.ShowProjectFile), true);
                }
            }

            if (_context.ErrorList != null && _context.ErrorList.Count > 0)
            {
                _logWriter.SetColor(Color.Error);
                foreach (var error in _context.ErrorList)
                {
                    _messageWriter.WriteMessageAligned(_eventFormatter.FormatEventMessage(error, false, _context.Parameters.ShowProjectFile), true);
                }
            }

            _logWriter.ResetColor();
        }

        private void ShowPerfSummary()
        {
            _hierarchicalMessageWriter.StartBlock(WellknownHierarchicalKeys.PerformanceSummary, "Performance Summary");

            if (_context.ProjectPerformanceCounters != null)
            {
                _logWriter.SetColor(Color.PerformanceHeader);
                _messageWriter.WriteNewLine();
                _messageWriter.WriteLinePrettyFromResource("ProjectPerformanceSummary");
                _logWriter.SetColor(Color.SummaryInfo);
                _messageWriter.DisplayCounters(_context.ProjectPerformanceCounters);
            }

            if (_context.TargetPerformanceCounters != null)
            {
                _logWriter.SetColor(Color.PerformanceHeader);
                _messageWriter.WriteNewLine();
                _messageWriter.WriteLinePrettyFromResource("TargetPerformanceSummary");
                _logWriter.SetColor(Color.SummaryInfo);
                _messageWriter.DisplayCounters(_context.TargetPerformanceCounters);
            }

            if (_context.TaskPerformanceCounters != null)
            {
                _logWriter.SetColor(Color.PerformanceHeader);
                _messageWriter.WriteNewLine();
                _messageWriter.WriteLinePrettyFromResource("TaskPerformanceSummary");
                _logWriter.SetColor(Color.SummaryInfo);
                _messageWriter.DisplayCounters(_context.TaskPerformanceCounters);
            }

            _hierarchicalMessageWriter.FinishBlock(WellknownHierarchicalKeys.PerformanceSummary);
            _logWriter.ResetColor();
        }


        private void ShowNestedErrorWarningSummary()
        {
            if (_context.WarningList?.Count == 0 && _context.ErrorList?.Count == 0 || _context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings)
            {
                return;
            }

            if (_context.WarningCount > 0)
            {
                _logWriter.SetColor(Color.Warning);
                ShowErrorWarningSummary(_context.WarningList);
            }

            if (_context.ErrorCount > 0)
            {
                _logWriter.SetColor(Color.Error);
                ShowErrorWarningSummary(_context.ErrorList);
            }

            _logWriter.ResetColor();
        }

        private void ShowErrorWarningSummary<T>(IEnumerable<T> events) where T : BuildEventArgs
        {
            var dictionary = new Dictionary<ErrorWarningSummaryDictionaryKey, List<T>>();
            foreach (var warningEventArgs in events)
            {
                string targetName = null;
                var targetStartedEvent = _buildEventManager.GetTargetStartedEvent(warningEventArgs.BuildEventContext);
                if (targetStartedEvent != null)
                {
                    targetName = targetStartedEvent.TargetName;
                }

                var key = new ErrorWarningSummaryDictionaryKey(warningEventArgs.BuildEventContext, targetName);
                if (!dictionary.TryGetValue(key, out List<T> list))
                {
                    list = new List<T>();
                    dictionary.Add(key, list);
                }

                list.Add(warningEventArgs);
            }

            BuildEventContext buildEventContext = null;
            string curTargetName = null;
            foreach (var keyValuePair in dictionary)
            {
                if (buildEventContext != keyValuePair.Key.EntryPointContext)
                {
                    _messageWriter.WriteNewLine();
                    foreach (var message in _buildEventManager.ProjectCallStackFromProject(keyValuePair.Key.EntryPointContext))
                    {
                        _messageWriter.WriteMessageAligned(message, false);
                    }

                    buildEventContext = keyValuePair.Key.EntryPointContext;
                }

                if (string.Compare(curTargetName, keyValuePair.Key.TargetName, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    if (!string.IsNullOrEmpty(keyValuePair.Key.TargetName))
                    {
                        _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("ErrorWarningInTarget", (object)keyValuePair.Key.TargetName), false);
                    }

                    curTargetName = keyValuePair.Key.TargetName;
                }

                foreach (var obj in keyValuePair.Value)
                {
                    var errorEventArgs = obj as BuildErrorEventArgs;
                    if (errorEventArgs != null)
                    {
                        _messageWriter.WriteMessageAligned("  " + _eventFormatter.FormatEventMessage(errorEventArgs, false, _context.Parameters.ShowProjectFile), false);
                        continue;
                    }

                    var warningEventArgs = obj as BuildWarningEventArgs;
                    if (warningEventArgs != null)
                    {
                        _messageWriter.WriteMessageAligned("  " + _eventFormatter.FormatEventMessage(warningEventArgs, false, _context.Parameters.ShowProjectFile), false);
                    }
                }

                _messageWriter.WriteNewLine();
            }
        }
    }
}
