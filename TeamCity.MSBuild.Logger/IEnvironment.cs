namespace TeamCity.MSBuild.Logger
{
    using JetBrains.Annotations;

    internal interface IEnvironment
    {
        string GetEnvironmentVariable(string name);
        
        bool TargetOutputLogging { get; }

        [NotNull] string DiagnosticsFile { get; }
    }
}
