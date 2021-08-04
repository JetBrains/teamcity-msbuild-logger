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

        public bool PlainServiceMessage { get; set; }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public string FlowId => System.Environment.GetEnvironmentVariable("TEAMCITY_PROCESS_FLOW_ID") ?? string.Empty;

        public override string ToString() => $"{nameof(Debug)}: {Debug}, {nameof(ShowOnlyWarnings)}: {ShowOnlyWarnings}, {nameof(ShowEnvironment)}: {ShowEnvironment}, {nameof(Verbosity)}: {Verbosity}, {nameof(ShowPerfSummary)}: {ShowPerfSummary}, {nameof(ShowItemAndPropertyList)}: {ShowItemAndPropertyList}, {nameof(ShowSummary)}: {ShowSummary}, {nameof(ShowOnlyErrors)}: {ShowOnlyErrors}, {nameof(ShowProjectFile)}: {ShowProjectFile}, {nameof(ShowCommandLine)}: {ShowCommandLine}, {nameof(ShowTimeStamp)}: {ShowTimeStamp}, {nameof(ShowEventId)}: {ShowEventId}, {nameof(ForceNoAlign)}: {ForceNoAlign}, {nameof(AlignMessages)}: {AlignMessages}, {nameof(ShowTargetOutputs)}: {ShowTargetOutputs}, {nameof(BufferWidth)}: {BufferWidth}, {nameof(ColorMode)}: {ColorMode}, {nameof(TeamCityMode)}: {TeamCityMode}, {nameof(StatisticsMode)}: {StatisticsMode}, {nameof(ColorThemeMode)}: {ColorThemeMode}, {nameof(PlainServiceMessage)}: {PlainServiceMessage}";
    }
}
