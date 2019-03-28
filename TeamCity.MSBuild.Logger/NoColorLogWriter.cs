namespace TeamCity.MSBuild.Logger
{
    using System;
    using IoC;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class NoColorLogWriter : ILogWriter
    {
        [NotNull] private readonly IConsole _defaultConsole;

        public NoColorLogWriter(
            [NotNull] IConsole defaultConsole)
        {
            _defaultConsole = defaultConsole ?? throw new ArgumentNullException(nameof(defaultConsole));
        }

        public void Write(string message, IConsole console = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            (console ?? _defaultConsole).Write(message);
        }

        public void SetColor(Color color)
        {
        }

        public void ResetColor()
        {
        }
    }
}
