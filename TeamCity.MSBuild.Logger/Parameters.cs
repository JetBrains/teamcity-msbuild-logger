namespace TeamCity.MSBuild.Logger
{
    using Microsoft.Build.Framework;

    internal class Parameters
    {
        public bool Debug { get; set; }

        public bool ShowOnlyWarnings { get; set; }

        public bool ShowEnvironment { get; set; }

        public LoggerVerbosity Verbosity { get; set; }

        public bool ShowPerfSummary { get; set; }

        public bool ShowItemAndPropertyList { get; set; }

        public bool? ShowSummary { get; set; }

        public bool ShowOnlyErrors { get; set; }

        public bool ShowProjectFile { get; set; }

        public bool? ShowCommandLine { get; set; }

        public bool ShowTimeStamp { get; set; }

        public bool? ShowEventId { get; set; }

        public bool ForceNoAlign { get; set; }

        public bool AlignMessages { get; set; }

        public bool ShowTargetOutputs { get; set; }

        public int BufferWidth { get; set; }

        public ColorMode ColorMode { get; set; } = ColorMode.Default;

        public TeamCityMode TeamCityMode { get; set; } = TeamCityMode.Off;

        public StatisticsMode StatisticsMode { get; set; } = StatisticsMode.Default;

        public ColorThemeMode ColorThemeMode { get; set; } = ColorThemeMode.Default;

        public bool PlaneServiceMessage { get; set; } = false;
    }
}
