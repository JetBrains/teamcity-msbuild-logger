namespace TeamCity.MSBuild.Logger
{
    using System;
    using JetBrains.Annotations;
    using JetBrains.Annotations;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class AnsiLogWriter : ILogWriter
    {
        [NotNull] private readonly IColorStorage _colorStorage;
        [NotNull] private readonly IColorTheme _colorTheme;
        [NotNull] private readonly IConsole _defaultConsole;

        public AnsiLogWriter(
            [NotNull] IConsole defaultConsole,
            [NotNull] IColorTheme colorTheme,
            [NotNull] IColorStorage colorStorage)
        {
            _colorStorage = colorStorage ?? throw new ArgumentNullException(nameof(colorStorage));
            _colorTheme = colorTheme ?? throw new ArgumentNullException(nameof(colorTheme));
            _defaultConsole = defaultConsole ?? throw new ArgumentNullException(nameof(defaultConsole));
        }

        public void Write(string message, IConsole console = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            (console ?? _defaultConsole).Write(_colorStorage.Color.HasValue ? $"\x001B[{_colorTheme.GetAnsiColor(_colorStorage.Color.Value)}m{message}" : message);
        }

        public void SetColor(Color color)
        {
            _colorStorage.SetColor(color);
        }

        public void ResetColor()
        {
            _colorStorage.ResetColor();
        }
    }
}
