namespace TeamCity.MSBuild.Logger
{
    using System;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ParametersFactory: IParametersFactory
    {
        internal const string TeamcityVersionEnvVarName = "TEAMCITY_VERSION";
        private readonly IEnvironmentService _environmentService;

        public ParametersFactory(
            [NotNull] IEnvironmentService environmentService)
        {
            _environmentService = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
        }

        public Parameters Create()
        {
            var parameters = new Parameters();
            if (_environmentService.GetEnvironmentVariable(TeamcityVersionEnvVarName) != string.Empty)
            {
                parameters.ColorMode = ColorMode.Ansi;
                parameters.TeamCityMode = TeamCityMode.SupportHierarchy;
                parameters.ColorThemeMode = ColorThemeMode.TeamCity;
            }

            return parameters;
        }
    }
}
