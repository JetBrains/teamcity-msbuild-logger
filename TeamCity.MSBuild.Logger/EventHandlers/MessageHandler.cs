namespace TeamCity.MSBuild.Logger.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using IoC;
    using Microsoft.Build.Framework;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class MessageHandler : IBuildEventHandler<BuildMessageEventArgs>
    {
        [NotNull] private readonly IBuildEventManager _buildEventManager;
        [NotNull] private readonly IDeferredMessageWriter _deferredMessageWriter;
        [NotNull] private readonly IMessageWriter _messageWriter;
        [NotNull] private readonly ILoggerContext _context;

        public MessageHandler(
            [NotNull] ILoggerContext context,
            [NotNull] IMessageWriter messageWriter,
            [NotNull] IDeferredMessageWriter deferredMessageWriter,
            [NotNull] IBuildEventManager buildEventManager)
        {
            _buildEventManager = buildEventManager ?? throw new ArgumentNullException(nameof(buildEventManager));
            _deferredMessageWriter = deferredMessageWriter ?? throw new ArgumentNullException(nameof(deferredMessageWriter));
            _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Handle(BuildMessageEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (_context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings)
            {
                return;
            }

            if (e == null) throw new ArgumentNullException(nameof(e));
            if (e.BuildEventContext == null) throw new ArgumentException(nameof(e));
            bool flag;
            var lightenText = false;
            if (e is TaskCommandLineEventArgs)
            {
                if (_context.Parameters.ShowCommandLine.HasValue && !_context.Parameters.ShowCommandLine.Value || !_context.Parameters.ShowCommandLine.HasValue && !_context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
                {
                    return;
                }

                flag = true;
            }
            else
            {
                switch (e.Importance)
                {
                    case MessageImportance.High:
                        flag = _context.IsVerbosityAtLeast(LoggerVerbosity.Minimal);
                        break;
                    case MessageImportance.Normal:
                        flag = _context.IsVerbosityAtLeast(LoggerVerbosity.Normal);
                        lightenText = true;
                        break;
                    case MessageImportance.Low:
                        flag = _context.IsVerbosityAtLeast(LoggerVerbosity.Detailed);
                        lightenText = true;
                        break;
                    default:
                        throw new InvalidOperationException("Impossible");
                }
            }

            if (!flag)
            {
                return;
            }

            if (_context.HasBuildStarted && e.BuildEventContext.ProjectContextId != -2 && _buildEventManager.GetProjectStartedEvent(e.BuildEventContext) == null && _context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
            {
                if (!_context.DeferredMessages.TryGetValue(e.BuildEventContext, out IList<BuildMessageEventArgs> messageEventArgsList))
                {
                    messageEventArgsList = new List<BuildMessageEventArgs>();
                    _context.DeferredMessages.Add(e.BuildEventContext, messageEventArgsList);
                }
                
                messageEventArgsList.Add(e);
            }
            else
            {
                _deferredMessageWriter.DisplayDeferredStartedEvents(e.BuildEventContext);
                _messageWriter.PrintMessage(e, lightenText);
                _deferredMessageWriter.ShownBuildEventContext(e.BuildEventContext);
            }
        }
    }
}