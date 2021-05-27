using System;

namespace TeamCity.MSBuild.Logger
{
    using JetBrains.Annotations;

    internal interface IColorTheme
    {
        ConsoleColor GetConsoleColor(Color color);

        [NotNull] string GetAnsiColor(Color color);
    }
}
