namespace TeamCity.MSBuild.Logger
{
    using JetBrains.Annotations;

    internal interface IEnvironment
    {
        bool TargetOutputLogging { get; }

        [NotNull] string DiagnosticsFile { get; }
    }
}
