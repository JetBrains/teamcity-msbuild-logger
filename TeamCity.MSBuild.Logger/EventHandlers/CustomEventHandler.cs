namespace TeamCity.MSBuild.Logger.EventHandlers
{
    using System;
    using Microsoft.Build.Framework;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class CustomEventHandler : IBuildEventHandler<CustomBuildEventArgs>
    {
        [NotNull] private readonly IDeferredMessageWriter _deferredMessageWriter;
        [NotNull] private readonly IMessageWriter _messageWriter;
        [NotNull] private readonly ILoggerContext _context;

        public CustomEventHandler(
            [NotNull] ILoggerContext context,
            [NotNull] IMessageWriter messageWriter,
            [NotNull] IDeferredMessageWriter deferredMessageWriter)
        {
            _deferredMessageWriter = deferredMessageWriter ?? throw new ArgumentNullException(nameof(deferredMessageWriter));
            _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Handle(CustomBuildEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (_context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings)
            {
                return;
            }

            if (e == null) throw new ArgumentNullException(nameof(e));
            if (e.BuildEventContext == null) throw new ArgumentException(nameof(e));

            if (!_context.IsVerbosityAtLeast(LoggerVerbosity.Detailed) || e.Message == null)
            {
                return;
            }

            _deferredMessageWriter.DisplayDeferredStartedEvents(e.BuildEventContext);
            _messageWriter.WriteLinePrefix(e.BuildEventContext, e.Timestamp, false);
            _messageWriter.WriteMessageAligned(e.Message, true);
            _deferredMessageWriter.ShownBuildEventContext(e.BuildEventContext);
        }
    }
}
