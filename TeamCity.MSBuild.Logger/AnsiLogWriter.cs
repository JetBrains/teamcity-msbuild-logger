namespace TeamCity.MSBuild.Logger
{
    using System;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class AnsiLogWriter : ILogWriter
    {
        [NotNull] private readonly IColorTheme _colorTheme;
        [NotNull] private readonly IConsole _defaultConsole;
        private Color? _currentColor;

        public AnsiLogWriter(
            [NotNull] IConsole defaultConsole,
            [NotNull] IColorTheme colorTheme)
        {
            _colorTheme = colorTheme ?? throw new ArgumentNullException(nameof(colorTheme));
            _defaultConsole = defaultConsole ?? throw new ArgumentNullException(nameof(defaultConsole));
        }

        public void Write(string message, IConsole console = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            (console ?? _defaultConsole).Write(_currentColor.HasValue ? $"\x001B[{_colorTheme.GetAnsiColor(_currentColor.Value)}m{message}" : message);
        }

        public void SetColor(Color color)
        {
            _currentColor = color;
        }

        public void ResetColor()
        {
            _currentColor = null;
        }
    }
}
