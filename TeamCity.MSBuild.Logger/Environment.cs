namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Environment : IEnvironment
    {
        private static readonly Dictionary<string, string> Envs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        static Environment()
        {
            foreach (var entry in System.Environment.GetEnvironmentVariables().OfType<DictionaryEntry>())
            {
                var key = entry.Key?.ToString()?.Trim() ?? string.Empty;
                var value = entry.Value?.ToString()?.Trim() ?? string.Empty; 
                Envs[key] = value;
            }
        }
        
        public string GetEnvironmentVariable(string name) => Envs.TryGetValue(name, out var val) ? val : string.Empty;

        public string DiagnosticsFile => System.Environment.GetEnvironmentVariable("MSBUILDDIAGNOSTICSFILE") ?? string.Empty;

        public bool TargetOutputLogging => !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("MSBUILDTARGETOUTPUTLOGGING"));        
    }
}