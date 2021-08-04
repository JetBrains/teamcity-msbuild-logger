﻿namespace TeamCity.MSBuild.Logger
{
    using System;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DefaultColorTheme : IColorTheme
    {
        public ConsoleColor GetConsoleColor(Color color)
        {
            switch (color)
            {
                case Color.BuildStage:
                    return ConsoleColor.Cyan;
                case Color.SummaryHeader:
                case Color.PerformanceHeader:
                case Color.Items:
                    return ConsoleColor.Blue;
                case Color.Success:
                    return ConsoleColor.Green;
                case Color.Warning:
                case Color.WarningSummary:
                    return ConsoleColor.Yellow;
                case Color.Error:
                case Color.ErrorSummary:
                    return ConsoleColor.Red;
                case Color.SummaryInfo:
                    return ConsoleColor.Gray;
                case Color.Details:
                    return ConsoleColor.DarkGray;
                case Color.Task:
                    return ConsoleColor.DarkCyan;
                case Color.PerformanceCounterInfo:
                    return ConsoleColor.White;
                default:
                    throw new ArgumentException($"Unknown color \"{color}\"");
            }


        }

        public string GetAnsiColor(Color color)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (color)
            {
                case Color.Task:
                    return "36";
                case Color.SummaryInfo:
                    return "37";
                case Color.Details:
                    return "30;1";
                case Color.Success:
                    return "32;1";
                case Color.SummaryHeader:
                case Color.PerformanceHeader:
                case Color.Items:
                    return "34;1";
                case Color.BuildStage:
                    return "36;1";
                case Color.Error:
                    return "31;1";
                case Color.Warning:
                    return "33;1";
                case Color.PerformanceCounterInfo:
                    return "37;1";
                default:
                    throw new ArgumentException($"Unknown color \"{color}\"");
            }
        }
    }
}
