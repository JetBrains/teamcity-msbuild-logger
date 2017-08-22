namespace TeamCity.MSBuild.Logger
{
    using System;
    using Microsoft.Build.Framework;

    internal class ProjectStartedEventMinimumFields
    {
        private readonly ProjectFullKey _projectFullKey;

        public DateTime TimeStamp { get; }

        public int ProjectKey => _projectFullKey.ProjectKey;

        public int EntryPointKey => _projectFullKey.EntryPointKey;

        public string FullProjectKey => _projectFullKey.ToString();

        public ProjectStartedEventMinimumFields ParentProjectStartedEvent { get; }

        public string TargetNames { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int ProjectId { get; }

        public string ProjectFile { get; }

        public bool ShowProjectFinishedEvent { get; set; }

        public bool ErrorInProject { get; set; }

        public BuildEventContext ProjectBuildEventContext { get; }

        public ProjectStartedEventMinimumFields(
            int projectKey,
            int entryPointKey,
            ProjectStartedEventArgs startedEvent,
            ProjectStartedEventMinimumFields parentProjectStartedEvent,
            bool requireTimeStamp)
        {
            TargetNames = startedEvent.TargetNames;
            ProjectFile = startedEvent.ProjectFile;
            ShowProjectFinishedEvent = false;
            ErrorInProject = false;
            ProjectId = startedEvent.ProjectId;
            ProjectBuildEventContext = startedEvent.BuildEventContext;
            ParentProjectStartedEvent = parentProjectStartedEvent;
            _projectFullKey = new ProjectFullKey(projectKey, entryPointKey);
            if (requireTimeStamp)
            {
                TimeStamp = startedEvent.Timestamp;
            }
        }
    }
}
