namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using EventHandlers;
    using JetBrains.Annotations;
    using Microsoft.Build.Framework;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class NodeLogger : INodeLogger
    {
        [NotNull] private readonly ILoggerContext _context;
        [NotNull] private readonly IEnvironment _environment;
        [NotNull] private readonly IDiagnostics _diagnostics;
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
        [NotNull] private readonly Parameters _parameters = new Parameters();
        [NotNull] private readonly object _lockObject = new object();
        // ReSharper disable once IdentifierTypo
        private int _reentrancy;

        public NodeLogger(
            [NotNull] IParametersParser parametersParser,
            [NotNull] ILogWriter logWriter,
            [NotNull] ILoggerContext context,
            [NotNull] IEnvironment environment,
            [NotNull] IDiagnostics diagnostics,
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
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
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
            get => _parameters.ShowSummary ?? false;
            // ReSharper disable once UnusedMember.Global
            set => _parameters.ShowSummary = value;
        }

        public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Normal;

        public string Parameters { get; set; }

        public void Initialize(IEventSource eventSource, int nodeCount)
        {
            _diagnostics.Send(() => $"Initialize({eventSource}, {nodeCount})");
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
                try
                {
                    _logWriter.SetColor(Color.Warning);
                    _logWriter.Write($"\nWaiting for debugger in process: [{Process.GetCurrentProcess().Id}] \"{Process.GetCurrentProcess().ProcessName}\"\n");
                }
                finally
                {
                    _logWriter.ResetColor();
                }

                while (!Debugger.IsAttached)
                {
                    Thread.Sleep(100);
                }

                Debugger.Break();
            }

            if (_context.IsVerbosityAtLeast(LoggerVerbosity.Diagnostic))
            {
                _parameters.ShowPerfSummary = true;
            }

            _parameters.ShowTargetOutputs = _environment.TargetOutputLogging;
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

            if (_context.IsVerbosityAtLeast(LoggerVerbosity.Diagnostic))
            {
                try
                {
                    _logWriter.SetColor(Color.Details);
                    _logWriter.Write($"Logger parameters: {_parameters}\n");
                }
                finally
                {
                    _logWriter.ResetColor();
                }
            }

            eventSource.BuildStarted += (sender, e) => Handle(_buildStartedEventHandler, e);
            eventSource.BuildFinished += (sender, e) => Handle(_buildFinishedHandler, e);
            eventSource.ProjectStarted += (sender, e) => Handle(_projectStartedHandler, e);
            eventSource.ProjectFinished += (sender, e) => Handle(_projectFinishedHandler, e);
            eventSource.TargetStarted += (sender, e) => Handle(_targetStartedHandler, e);
            eventSource.TargetFinished += (sender, e) => Handle(_targetFinishedHandler, e);
            eventSource.TaskStarted += (sender, e) => Handle(_taskStartedHandler, e);
            eventSource.TaskFinished += (sender, e) => Handle(_taskFinishedHandler, e);
            eventSource.ErrorRaised += (sender, e) => Handle(_errorHandler, e);
            eventSource.WarningRaised += (sender, e) => Handle(_warningHandler, e);
            eventSource.MessageRaised += (sender, e) => Handle(_messageHandler, e);
            eventSource.CustomEventRaised += (sender, e) => Handle(_customEventHandler, e);

            _diagnostics.Send(() => _parameters.ToString());
        }

        public void Initialize(IEventSource eventSource)
        {
            Initialize(eventSource, 1);
        }

        public virtual void Shutdown()
        {
            _diagnostics.Send(() => "Shutdown()");
        }

        private void Handle<TBuildEventArgs>(IBuildEventHandler<TBuildEventArgs> handler, TBuildEventArgs e)
            where TBuildEventArgs : BuildEventArgs
        {
            if (e == null)
            {
                return;
            }

            // ReSharper disable once IdentifierTypo
            var reentrancy = Interlocked.Increment(ref _reentrancy) - 1;
            // ReSharper disable once AccessToModifiedClosure
            _diagnostics.Send(() => $"[{reentrancy} +] Handle<{typeof(TBuildEventArgs).Name}>()");
            try
            {
                lock (_lockObject)
                {
                    using (new HierarchicalContext(e.BuildEventContext?.NodeId ?? 0))
                    {
                        handler.Handle(e);
                    }
                }
            }
            catch (Exception ex)
            {
                var error = $"Exception was occurred while processing a message of type \"{e.GetType()}\":\n{ex}";
                _logWriter.Write(error);
                _diagnostics.Send(() => error);                
            }
            finally
            {
                reentrancy = Interlocked.Decrement(ref _reentrancy);
                _diagnostics.Send(() => $"[{reentrancy} -] Handle<{typeof(TBuildEventArgs).Name}>()");
            }
        }
    }
}
