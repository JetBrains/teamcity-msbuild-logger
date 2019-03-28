namespace TeamCity.MSBuild.Logger
{
    internal interface IEnvironment
    {
        bool TargetOutputLogging { get; }

        [NotNull] string DiagnosticsFile { get; }
    }
}
