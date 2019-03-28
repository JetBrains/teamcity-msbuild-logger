namespace TeamCity.MSBuild.Logger
{
    using IoC;

    internal interface IEnvironment
    {
        bool TargetOutputLogging { get; }

        [NotNull] string DiagnosticsFile { get; }
    }
}
