namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using Microsoft.Build.Framework;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class LoggerContext: ILoggerContext
    {
        [NotNull] private readonly IBuildEventManager _buildEventManager;

        public LoggerContext(
            [NotNull] IBuildEventManager buildEventManager)
        {
            _buildEventManager = buildEventManager ?? throw new ArgumentNullException(nameof(buildEventManager));
            ResetConsoleLoggerState();
        }

        public LoggerVerbosity Verbosity => Parameters.Verbosity;

        public Parameters Parameters { get; private set; }

        public int CurrentIndentLevel { get; } = 0;

        public int NumberOfProcessors { get; private set; }

        public bool SkipProjectStartedText { get; private set; }

        [CanBeNull] public IList<BuildErrorEventArgs> ErrorList { get; private set; }

        [CanBeNull] public IList<BuildWarningEventArgs> WarningList { get; private set; }

        public IDictionary<BuildEventContext, IList<BuildMessageEventArgs>> DeferredMessages { get; }  = new Dictionary<BuildEventContext, IList<BuildMessageEventArgs>>(ComparerContextNodeId.Shared);

        public IDictionary<string, IPerformanceCounter> ProjectPerformanceCounters { get; } = new Dictionary<string, IPerformanceCounter>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, IPerformanceCounter> TargetPerformanceCounters { get; } = new Dictionary<string, IPerformanceCounter>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, IPerformanceCounter> TaskPerformanceCounters { get; } = new Dictionary<string, IPerformanceCounter>(StringComparer.OrdinalIgnoreCase);

        public bool HasBuildStarted { get; set; }

        public DateTime BuildStarted { get; set; }

        public int ErrorCount { get; set; }

        public int WarningCount { get; set; }

        public BuildEventContext LastDisplayedBuildEventContext { get; set; }

        public int PrefixWidth { get; set; }

        public ProjectFullKey LastProjectFullKey { get; set; }

        public void Initialize(
            int numberOfProcessors,
            bool skipProjectStartedText,
            Parameters parameters)
        {
            NumberOfProcessors = numberOfProcessors;
            SkipProjectStartedText = skipProjectStartedText;
            Parameters = parameters;
            if (parameters.ShowSummary ?? false)
            {
                ErrorList = new List<BuildErrorEventArgs>();
                WarningList = new List<BuildWarningEventArgs>();
            }
            else
            {
                ErrorList = null;
                WarningList = null;
            }
        }

        public bool IsVerbosityAtLeast(LoggerVerbosity checkVerbosity)
        {
            return Verbosity >= checkVerbosity;
        }

        public void ResetConsoleLoggerState()
        {
            ErrorCount = 0;
            WarningCount = 0;
            _buildEventManager.Reset();
            PrefixWidth = 0;
            LastDisplayedBuildEventContext = null;
            LastProjectFullKey = new ProjectFullKey(-1, -1);
            HasBuildStarted = false;
            BuildStarted = default(DateTime);
            PrefixWidth = 0;
            ProjectPerformanceCounters.Clear();
            TargetPerformanceCounters.Clear();
            TaskPerformanceCounters.Clear();
        }

        public ProjectFullKey GetFullProjectKey(BuildEventContext e)
        {
            ProjectStartedEventMinimumFields eventMinimumFields = null;
            if (e != null)
            {
                eventMinimumFields = _buildEventManager.GetProjectStartedEvent(e);
            }

            if (eventMinimumFields == null)
            {
                return new ProjectFullKey(0, 0);
            }

            return new ProjectFullKey(eventMinimumFields.ProjectKey, eventMinimumFields.EntryPointKey);
        }
    }
}
