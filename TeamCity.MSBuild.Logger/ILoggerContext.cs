namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Build.Framework;

    internal interface ILoggerContext
    {
        DateTime BuildStarted { get; set; }

        int CurrentIndentLevel { get; }

        IDictionary<BuildEventContext, IList<BuildMessageEventArgs>> DeferredMessages { [NotNull] get; }

        int ErrorCount { get; set; }

        IList<BuildErrorEventArgs> ErrorList { [CanBeNull] get; }

        bool HasBuildStarted { get; set; }

        [CanBeNull] BuildEventContext LastDisplayedBuildEventContext { get; set; }

        ProjectFullKey LastProjectFullKey { get; set; }

        int NumberOfProcessors { get; }

        Parameters Parameters { [NotNull] get; }

        int PrefixWidth { get; set; }

        bool SkipProjectStartedText { get; }

        IDictionary<string, IPerformanceCounter> ProjectPerformanceCounters { [NotNull] get; }

        IDictionary<string, IPerformanceCounter> TargetPerformanceCounters { [NotNull] get; }

        IDictionary<string, IPerformanceCounter> TaskPerformanceCounters { [NotNull] get; }

        LoggerVerbosity Verbosity { get; }

        int WarningCount { get; set; }

        IList<BuildWarningEventArgs> WarningList { [CanBeNull] get; }

        [NotNull] ProjectFullKey GetFullProjectKey([CanBeNull] BuildEventContext e);

        void Initialize(
            int numberOfProcessors,
            bool skipProjectStartedText,
            [NotNull] Parameters parameters);

        bool IsVerbosityAtLeast(LoggerVerbosity checkVerbosity);

        void ResetConsoleLoggerState();
    }
}