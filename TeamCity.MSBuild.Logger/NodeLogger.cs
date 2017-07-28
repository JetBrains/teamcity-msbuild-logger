﻿namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using EventHandlers;
    using Microsoft.Build.Framework;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class NodeLogger : INodeLogger
    {
        [NotNull] private readonly IParametersFactory _parametersFactory;
        [NotNull] private readonly ILoggerContext _context;
        [NotNull] private readonly IBuildEventHandler<BuildMessageEventArgs> _messageHandler;
        [NotNull] private readonly IBuildEventHandler<BuildFinishedEventArgs> _buildFinishedHandler;
        [NotNull] private readonly IBuildEventHandler<ProjectStartedEventArgs> _projectStartedHandler;
        [NotNull] private readonly IBuildEventHandler<ProjectFinishedEventArgs> _projectFinishedHandler;
        [NotNull] private readonly IBuildEventHandler<TargetStartedEventArgs> _targetStartedHandler;
        [NotNull] private readonly IBuildEventHandler<TargetFinishedEventArgs> _targetFinishedHandler;
        [NotNull] private readonly IBuildEventHandler<TaskStartedEventArgs> _taskStartedHandler;
        [NotNull] private readonly IBuildEventHandler<TaskFinishedEventArgs> _taskFinishedHandler;
        [NotNull] private readonly IBuildEventHandler<BuildErrorEventArgs> _errorHandler;
        [NotNull] private readonly IBuildEventHandler<BuildWarningEventArgs> _warningHandler;
        [NotNull] private readonly IBuildEventHandler<CustomBuildEventArgs> _customEventHandler;
        [NotNull] private readonly IBuildEventHandler<BuildStartedEventArgs> _buildStartedEventHandler;
        [NotNull] private readonly IParametersParser _parametersParser;
        [NotNull] private readonly ILogWriter _logWriter;

        [CanBeNull] private Parameters _parameters;

        public NodeLogger(
            [NotNull] IParametersFactory parametersFactory,
            [NotNull] IParametersParser parametersParser,
            [NotNull] ILogWriter logWriter,
            [NotNull] ILoggerContext context,
            [NotNull] IBuildEventHandler<BuildStartedEventArgs> buildStartedHandler,
            [NotNull] IBuildEventHandler<BuildMessageEventArgs> messageHandler,
            [NotNull] IBuildEventHandler<BuildFinishedEventArgs> buildFinishedHandler,
            [NotNull] IBuildEventHandler<ProjectStartedEventArgs> projectStartedHandler,
            [NotNull] IBuildEventHandler<ProjectFinishedEventArgs> projectFinishedHandler,
            [NotNull] IBuildEventHandler<TargetStartedEventArgs> targetStartedHandler,
            [NotNull] IBuildEventHandler<TargetFinishedEventArgs> targetFinishedHandler,
            [NotNull] IBuildEventHandler<TaskStartedEventArgs> taskStartedHandler,
            [NotNull] IBuildEventHandler<TaskFinishedEventArgs> taskFinishedHandler,
            [NotNull] IBuildEventHandler<BuildErrorEventArgs> errorHandler,
            [NotNull] IBuildEventHandler<BuildWarningEventArgs> warningHandler,
            [NotNull] IBuildEventHandler<CustomBuildEventArgs> customEventHandler)
        {
            _parametersFactory = parametersFactory ?? throw new ArgumentNullException(nameof(parametersFactory));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _parametersParser = parametersParser ?? throw new ArgumentNullException(nameof(parametersParser));
            _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
            _buildStartedEventHandler = buildStartedHandler ?? throw new ArgumentNullException(nameof(buildStartedHandler));
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            _buildFinishedHandler = buildFinishedHandler ?? throw new ArgumentNullException(nameof(buildFinishedHandler));
            _projectStartedHandler = projectStartedHandler ?? throw new ArgumentNullException(nameof(projectStartedHandler));
            _projectFinishedHandler = projectFinishedHandler ?? throw new ArgumentNullException(nameof(projectFinishedHandler));
            _targetStartedHandler = targetStartedHandler ?? throw new ArgumentNullException(nameof(targetStartedHandler));
            _targetFinishedHandler = targetFinishedHandler ?? throw new ArgumentNullException(nameof(targetFinishedHandler));
            _taskStartedHandler = taskStartedHandler ?? throw new ArgumentNullException(nameof(taskStartedHandler));
            _taskFinishedHandler = taskFinishedHandler ?? throw new ArgumentNullException(nameof(taskFinishedHandler));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _warningHandler = warningHandler ?? throw new ArgumentNullException(nameof(warningHandler));
            _customEventHandler = customEventHandler ?? throw new ArgumentNullException(nameof(customEventHandler));
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool SkipProjectStartedText { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedMember.Global
        public bool ShowSummary
        {
            get => _parameters?.ShowSummary ?? false;
            // ReSharper disable once UnusedMember.Global
            set
            {
                if (_parameters != null)
                {
                    _parameters.ShowSummary = value;
                }
            }
        }

        public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Normal;

        public string Parameters { get; set; }

        public void Initialize(IEventSource eventSource, int nodeCount)
        {
            _parameters = _parametersFactory.Create();
            _parameters.Verbosity = Verbosity;
            if (Parameters != null)
            {
                if (!_parametersParser.TryParse(Parameters, _parameters, out string error))
                {
                    throw new LoggerException(error);
                }
            }

            _context.Initialize(
                nodeCount,
                SkipProjectStartedText,
                _parameters);

            if (nodeCount == 1 && _parameters.ShowEventId.HasValue)
            {
                _parameters.ShowEventId = false;
            }

            if (_parameters.Debug)
            {
                _logWriter.SetColor(Color.Warning);
                _logWriter.Write($"\nWaiting for debugger in process: [{Process.GetCurrentProcess().Id}] \"{Process.GetCurrentProcess().ProcessName}\"\n");
                _logWriter.ResetColor();
                while (!Debugger.IsAttached)
                {
                    Thread.Sleep(100);
                }
            }

            if (_context.IsVerbosityAtLeast(LoggerVerbosity.Diagnostic))
            {
                _parameters.ShowPerfSummary = true;
            }

            _parameters.ShowTargetOutputs = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MSBUILDTARGETOUTPUTLOGGING"));
            if (!_parameters.ShowSummary.HasValue && _context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
            {
                _parameters.ShowSummary = true;
            }

            if (_parameters.ShowOnlyWarnings || _parameters.ShowOnlyErrors)
            {
                if (!_parameters.ShowSummary.HasValue)
                {
                    _parameters.ShowSummary = false;
                }

                _parameters.ShowPerfSummary = false;
            }

            if (eventSource == null)
            {
                return;
            }

            eventSource.BuildStarted += (sender, e) => _buildStartedEventHandler.Handle(e);
            eventSource.BuildFinished += (sender, e) => _buildFinishedHandler.Handle(e);
            eventSource.ProjectStarted += (sender, e) => _projectStartedHandler.Handle(e);
            eventSource.ProjectFinished += (sender, e) => _projectFinishedHandler.Handle(e);
            eventSource.TargetStarted += (sender, e) => _targetStartedHandler.Handle(e);
            eventSource.TargetFinished += (sender, e) => _targetFinishedHandler.Handle(e);
            eventSource.TaskStarted += (sender, e) => _taskStartedHandler.Handle(e);
            eventSource.TaskFinished += (sender, e) => _taskFinishedHandler.Handle(e);
            eventSource.ErrorRaised += (sender, e) => _errorHandler.Handle(e);
            eventSource.WarningRaised += (sender, e) => _warningHandler.Handle(e);
            eventSource.MessageRaised += (sender, e) => _messageHandler.Handle(e);
            eventSource.CustomEventRaised += (sender, e) => _customEventHandler.Handle(e);
        }

        public void Initialize(IEventSource eventSource)
        {
            Initialize(eventSource, 1);
        }

        public virtual void Shutdown()
        {
        }
    }
}
