namespace TeamCity.MSBuild.Logger
{
    using System;
    using IoC;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class TeamCityColorTheme : IColorTheme
    {
        [NotNull] private readonly IColorTheme _defaultColorTheme;

        public TeamCityColorTheme(
            [NotNull] IColorTheme defaultColorTheme)
        {
            _defaultColorTheme = defaultColorTheme ?? throw new ArgumentNullException(nameof(defaultColorTheme));
        }

        public ConsoleColor GetConsoleColor(Color color)
        {
            return _defaultColorTheme.GetConsoleColor(color);
        }

        public string GetAnsiColor(Color color)
        {
            switch (color)
            {
                case Color.SummaryInfo:
                case Color.PerformanceCounterInfo:
                    return "35";
                case Color.Details:
                    return "34";
                case Color.Task:
                    return "36";
                default:
                    return _defaultColorTheme.GetAnsiColor(color);
            }
        }
    }
}
