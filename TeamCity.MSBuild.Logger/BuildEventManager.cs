namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Build.Framework;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class BuildEventManager: IBuildEventManager
    {
        [NotNull] private readonly IStringService _stringService;
        private readonly IDictionary<string, int> _projectTargetKey = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<string, int> _projectKey = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<BuildEventContext, ProjectStartedEventMinimumFields> _projectStartedEvents = new Dictionary<BuildEventContext, ProjectStartedEventMinimumFields>(ComparerContextNodeId.Shared);
        private readonly IDictionary<BuildEventContext, TargetStartedEventMinimumFields> _targetStartedEvents = new Dictionary<BuildEventContext, TargetStartedEventMinimumFields>(ComparerContextNodeIdTargetId.Shared);
        private int _projectIncrementKey;

        public BuildEventManager(
            [NotNull] IStringService stringService)
        {
            _stringService = stringService ?? throw new ArgumentNullException(nameof(stringService));
        }

        public void AddProjectStartedEvent(ProjectStartedEventArgs e, bool requireTimestamp)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            var projectStartedEvent = GetProjectStartedEvent(e.ParentProjectBuildEventContext);
            lock (_projectStartedEvents)
            {
                if (_projectStartedEvents.ContainsKey(e.BuildEventContext))
                {
                    return;
                }

                if (!_projectKey.TryGetValue(e.ProjectFile, out int projectIncrementKey))
                {
                    _projectIncrementKey = _projectIncrementKey + 1;
                    _projectKey.Add(e.ProjectFile, _projectIncrementKey);
                    projectIncrementKey = _projectIncrementKey;
                }

                if (!_projectTargetKey.TryGetValue(e.ProjectFile, out int entryPointKey))
                {
                    _projectTargetKey.Add(e.ProjectFile, 1);
                }
                else
                {
                    _projectTargetKey[e.ProjectFile] = entryPointKey + 1;
                }

                _projectStartedEvents.Add(e.BuildEventContext, new ProjectStartedEventMinimumFields(projectIncrementKey, entryPointKey, e, projectStartedEvent, requireTimestamp));
            }
        }

        public void AddTargetStartedEvent(TargetStartedEventArgs e, bool requireTimeStamp)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (_targetStartedEvents.ContainsKey(e.BuildEventContext))
            {
                return;
            }

            _targetStartedEvents.Add(e.BuildEventContext, new TargetStartedEventMinimumFields(e, requireTimeStamp));
        }

        public void SetErrorWarningFlagOnCallStack(BuildEventContext e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            foreach (var projectCall in GetProjectCallStack(e))
            {
                if (projectCall != null)
                {
                    projectCall.ErrorInProject = true;
                }
            }
        }

        public IEnumerable<string> ProjectCallStackFromProject(BuildEventContext e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            var projectStartedEvent = GetProjectStartedEvent(e);
            if (projectStartedEvent == null)
            {
                return Enumerable.Empty<string>();
            }

            return (
                from projectCall in GetProjectCallStack(e)
                select string.IsNullOrEmpty(projectCall.TargetNames)
                    ? _stringService.FormatResourceString("ProjectStackWithTargetNames", projectCall.ProjectFile, projectCall.TargetNames, projectCall.FullProjectKey)
                    : _stringService.FormatResourceString("ProjectStackWithDefaultTargets", projectCall.ProjectFile, projectCall.FullProjectKey))
                    .Reverse();
        }

        public ProjectStartedEventMinimumFields GetProjectStartedEvent(BuildEventContext e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            return _projectStartedEvents.TryGetValue(e, out ProjectStartedEventMinimumFields result) ? result : null;
        }

        public TargetStartedEventMinimumFields GetTargetStartedEvent(BuildEventContext e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            return _targetStartedEvents.TryGetValue(e, out TargetStartedEventMinimumFields result) ? result : null;
        }

        public void RemoveProjectStartedEvent(BuildEventContext e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            var projectStartedEvent = GetProjectStartedEvent(e);
            if (projectStartedEvent == null || projectStartedEvent.ErrorInProject)
            {
                return;
            }

            _projectStartedEvents.Remove(e);
        }

        public void RemoveTargetStartedEvent(BuildEventContext e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            var targetStartedEvent = GetTargetStartedEvent(e);
            if (targetStartedEvent == null || targetStartedEvent.ErrorInTarget)
            {
                return;
            }

            _targetStartedEvents.Remove(e);
        }

        public void Reset()
        {
            _projectTargetKey.Clear();
            _projectKey.Clear();
            _projectStartedEvents.Clear();
            _targetStartedEvents.Clear();
            _projectIncrementKey = 0;
        }

        [NotNull]
        private IEnumerable<ProjectStartedEventMinimumFields> GetProjectCallStack([NotNull] BuildEventContext e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            var projectStartedEvent = GetProjectStartedEvent(e);
            if (projectStartedEvent == null)
            {
                yield break;
            }

            yield return projectStartedEvent;
            while (projectStartedEvent.ParentProjectStartedEvent != null)
            {
                projectStartedEvent = projectStartedEvent.ParentProjectStartedEvent;
                yield return projectStartedEvent;
            }
        }
    }
}
