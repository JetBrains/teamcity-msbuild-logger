namespace TeamCity.MSBuild.Logger
{
    using System.Collections.Generic;
    using Microsoft.Build.Framework;
    using IoC;

    internal interface IBuildEventManager
    {
        void AddProjectStartedEvent([NotNull] ProjectStartedEventArgs e, bool requireTimestamp);

        void AddTargetStartedEvent([NotNull] TargetStartedEventArgs e, bool requireTimeStamp);

        [CanBeNull] ProjectStartedEventMinimumFields GetProjectStartedEvent([NotNull] BuildEventContext e);

        [CanBeNull] TargetStartedEventMinimumFields GetTargetStartedEvent([NotNull] BuildEventContext e);

        [NotNull] IEnumerable<string> ProjectCallStackFromProject([NotNull] BuildEventContext e);

        void RemoveProjectStartedEvent([NotNull] BuildEventContext e);

        void RemoveTargetStartedEvent([NotNull] BuildEventContext e);

        void SetErrorWarningFlagOnCallStack([NotNull] BuildEventContext e);

        void Reset();
    }
}