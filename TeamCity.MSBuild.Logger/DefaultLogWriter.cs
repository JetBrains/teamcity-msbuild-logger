namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.IO;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DefaultLogWriter : ILogWriter
    {
        [NotNull] private readonly IColorTheme _colorTheme;
        [NotNull] private readonly IConsole _defaultConsole;
        private static bool _supportReadingBackgroundColor = true;
        private readonly bool _hasBackgroundColor;

        public DefaultLogWriter(
            [NotNull] IConsole defaultConsole,
            [NotNull] IColorTheme colorTheme)
        {
            _colorTheme = colorTheme ?? throw new ArgumentNullException(nameof(colorTheme));
            _defaultConsole = defaultConsole ?? throw new ArgumentNullException(nameof(defaultConsole));
            _hasBackgroundColor = true;
            try
            {
                // ReSharper disable once UnusedVariable
                var backgroundColor = BackgroundColor;
            }
            catch (IOException)
            {
                _hasBackgroundColor = false;
            }
        }

        private static ConsoleColor BackgroundColor
        {
            get
            {
                if (_supportReadingBackgroundColor)
                {
                    try
                    {
                        return Console.BackgroundColor;
                    }
                    catch (PlatformNotSupportedException)
                    {
                        _supportReadingBackgroundColor = false;
                    }
                }

                return ConsoleColor.Black;
            }
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
            if (!_hasBackgroundColor)
            {
                return;
            }

            try
            {
                Console.ForegroundColor = TransformColor(_colorTheme.GetConsoleColor(color), BackgroundColor);
            }
            catch (IOException)
            {
            }
        }

        public void ResetColor()
        {
            if (!_hasBackgroundColor)
            {
                return;
            }

            try
            {
                Console.ResetColor();
            }
            catch (IOException)
            {
            }
        }

        private static ConsoleColor TransformColor(ConsoleColor foreground, ConsoleColor background)
        {
            var consoleColor = foreground;
            if (foreground == background)
            {
                consoleColor = background == ConsoleColor.Black ? ConsoleColor.Gray : ConsoleColor.Black;
            }

            return consoleColor;
        }
    }
}