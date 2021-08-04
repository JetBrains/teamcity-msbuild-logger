﻿namespace TeamCity.MSBuild.Logger
{
    using System.Collections.Generic;
    using System;
    using JetBrains.Annotations;
    using Pure.DI;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class LogWriter : ILogWriter
    {
        [NotNull] private readonly Dictionary<ColorMode, ILogWriter> _logWriters;
        [NotNull] private readonly ILoggerContext _context;

        public LogWriter(
            [NotNull] ILoggerContext context,
            [NotNull][Tag(ColorMode.Default)] ILogWriter defaultLogWriter,
            [NotNull][Tag(ColorMode.TeamCity)] ILogWriter ansiLogWriter,
            [NotNull][Tag(ColorMode.NoColor)] ILogWriter noColorLogWriter,
            [NotNull][Tag(ColorMode.AnsiColor)] ILogWriter ansiColorLogWriter)
        {
            _logWriters = new Dictionary<ColorMode, ILogWriter>
            {
                { ColorMode.Default, defaultLogWriter ?? throw new ArgumentNullException(nameof(defaultLogWriter))},
                { ColorMode.TeamCity, ansiLogWriter ?? throw new ArgumentNullException(nameof(ansiLogWriter))},
                { ColorMode.NoColor, noColorLogWriter ?? throw new ArgumentNullException(nameof(noColorLogWriter))},
                { ColorMode.AnsiColor, ansiColorLogWriter ?? throw new ArgumentNullException(nameof(ansiColorLogWriter))}
            };

            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private ILogWriter CurrentLogWriter => _logWriters[_context.Parameters?.ColorMode ?? ColorMode.Default];

        public void Write(string message, IConsole console = null)
        {
            CurrentLogWriter.Write(message, console);
        }

        public void SetColor(Color color)
        {
            CurrentLogWriter.SetColor(color);
        }

        public void ResetColor()
        {
            CurrentLogWriter.ResetColor();
        }
    }
}
