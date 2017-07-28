using System;

namespace TeamCity.MSBuild.Logger
{
    internal interface IColorTheme
    {
        ConsoleColor GetConsoleColor(Color color);

        [NotNull] string GetAnsiColor(Color color);
    }
}
