using System;

namespace TeamCity.MSBuild.Logger
{
    using IoC;

    internal interface IColorTheme
    {
        ConsoleColor GetConsoleColor(Color color);

        [NotNull] string GetAnsiColor(Color color);
    }
}
