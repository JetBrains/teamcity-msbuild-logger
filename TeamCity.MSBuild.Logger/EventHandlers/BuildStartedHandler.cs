namespace TeamCity.MSBuild.Logger.EventHandlers
{
    using Microsoft.Build.Framework;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class BuildStartedHandler : IBuildEventHandler<BuildStartedEventArgs>
    {
        [NotNull] private readonly IStringService _stringService;
        [NotNull] private readonly IHierarchicalMessageWriter _hierarchicalMessageWriter;
        [NotNull] private readonly IMessageWriter _messageWriter;
        [NotNull] private readonly ILoggerContext _context;
        [NotNull] private readonly ILogWriter _logWriter;

        public BuildStartedHandler(
            [NotNull] ILoggerContext context,
            [NotNull] ILogWriter logWriter,
            [NotNull] IMessageWriter messageWriter,
            [NotNull] IHierarchicalMessageWriter hierarchicalMessageWriter,
            [NotNull] IStringService stringService)
        {
            _stringService = stringService ?? throw new ArgumentNullException(nameof(stringService));
            _hierarchicalMessageWriter = hierarchicalMessageWriter ?? throw new ArgumentNullException(nameof(hierarchicalMessageWriter));
            _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
        }

        public void Handle(BuildStartedEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            _context.BuildStarted = e.Timestamp;
            _context.HasBuildStarted = true;
            if (_context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings)
            {
                return;
            }

            if (_context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
            {
                _messageWriter.WriteLinePrettyFromResource("BuildStartedWithTime", e.Timestamp);
            }

            WriteEnvironment(e.BuildEnvironment);
        }

        private void WriteEnvironment([CanBeNull] IDictionary<string, string> environment)
        {
            if (environment == null || environment.Count == 0 || _context.Verbosity != LoggerVerbosity.Diagnostic && !_context.Parameters.ShowEnvironment)
            {
                return;
            }

            OutputEnvironment(environment);
            _messageWriter.WriteNewLine();
        }

        private void OutputEnvironment(IDictionary<string, string> environment)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            _logWriter.SetColor(Color.SummaryHeader);
            _hierarchicalMessageWriter.StartBlock(WellknownHierarchicalKeys.EnvironmentHeader, "Environment", _stringService.FormatResourceString("EnvironmentHeader"));
            foreach (var keyValuePair in environment)
            {
                _logWriter.SetColor(Color.SummaryInfo);
                _messageWriter.WriteMessageAligned(string.Format(CultureInfo.CurrentCulture, "{0} = {1}", keyValuePair.Key, keyValuePair.Value), false);
            }

            _hierarchicalMessageWriter.FinishBlock(WellknownHierarchicalKeys.EnvironmentHeader);
            _logWriter.ResetColor();
        }
    }
}