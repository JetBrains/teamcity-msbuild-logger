namespace TeamCity.MSBuild.Logger
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Environment : IEnvironment
    {
        public string DiagnosticsFile => System.Environment.GetEnvironmentVariable("MSBUILDDIAGNOSTICSFILE") ?? string.Empty;

        public bool TargetOutputLogging => !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("MSBUILDTARGETOUTPUTLOGGING"));        
    }
}