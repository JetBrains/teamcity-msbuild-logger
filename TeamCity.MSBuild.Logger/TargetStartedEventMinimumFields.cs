namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Build.Framework;

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal class TargetStartedEventMinimumFields
    {
        public DateTime TimeStamp { get; }

        public string TargetName { get; }

        public string TargetFile { get; }

        public string ProjectFile { get; }

        public string Message { get; }

        public bool ShowTargetFinishedEvent { get; set; }

        public bool ErrorInTarget { get; set; }

        public BuildEventContext TargetBuildEventContext { get; }

        public string ParentTarget { get; }

        public string FullTargetKey { get; }

        public HierarchicalKey HierarchicalKey { [NotNull] get; }

        public TargetStartedEventMinimumFields(
            TargetStartedEventArgs startedEvent,
            bool requireTimeStamp)
        {
            TargetName = startedEvent.TargetName;
            TargetFile = startedEvent.TargetFile;
            ProjectFile = startedEvent.ProjectFile;
            ShowTargetFinishedEvent = false;
            ErrorInTarget = false;
            Message = startedEvent.Message;
            TargetBuildEventContext = startedEvent.BuildEventContext;
            if (requireTimeStamp)
            {
                TimeStamp = startedEvent.Timestamp;
            }

            ParentTarget = startedEvent.ParentTarget;
            FullTargetKey = $"{TargetFile}.{TargetName}";
            HierarchicalKey = new HierarchicalKey($"{ProjectFile}**{TargetFile}**{TargetName}**{TargetBuildEventContext.ProjectContextId}**{TargetBuildEventContext.TargetId}");
        }
    }
}
