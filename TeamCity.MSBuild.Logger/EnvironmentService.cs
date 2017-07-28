namespace TeamCity.MSBuild.Logger
{
    using System;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class EnvironmentService : IEnvironmentService
    {
        public string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name) ?? string.Empty;
        }
    }
}
