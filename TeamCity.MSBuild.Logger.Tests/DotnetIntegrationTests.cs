namespace TeamCity.MSBuild.Logger.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using Helpers;
    using Shouldly;
    using Xunit;

#if NETCOREAPP1_0
    [Collection("Integration")]
    public class DotnetIntegrationTests
    {
        [Theory]
        [InlineData("netcoreapp1.0", 10, "minimal", null, false)]
        [InlineData("netcoreapp1.0", 1, "m", null, false)]
        [InlineData("netcoreapp1.0", 1, "quiet", null, false)]
        [InlineData("netcoreapp1.0", 10, "quiet", null, false)]
        [InlineData("netcoreapp1.0", 1, "q", null, false)]
        [InlineData("netcoreapp1.0", 10, "q", null, false)]
        [InlineData("netcoreapp1.0", 10, "normal", null, false)]
        [InlineData("netcoreapp1.0", 10, "normal", "TEAMcity", true)]
        [InlineData("netcoreapp1.0", 10, "n", null, false)]
        [InlineData("netcoreapp1.0", 10, "detailed", null, false)]
        [InlineData("netcoreapp1.0", 10, "d", null, false)]
        [InlineData("netcoreapp1.0", 10, "diagnostic", null, false)]
        [InlineData("netcoreapp1.0", 10, "diag", null, false)]
        [InlineData("netcoreapp1.0", 10, "deTailed", null, false)]
        [InlineData("netcoreapp1.0", 10, "diag", "teamcity", true)]
        [InlineData("netcoreapp1.0", 10, "deTailed", "teamcity", true)]
        public void ShouldProduceSameMessagesAsConsoleLoggerViaDotnet(
            string framework,
            int processCount,
            [NotNull] string verbosity,
            [CanBeNull] string parameters,
            bool producesTeamCityServiceMessages)
        {
            // Given
            var environmentVariables = new Dictionary<string, string>();
            var loggerString = framework.CreateLoggerString(parameters);
            var projectPath = Path.GetFullPath(Path.Combine(CommandLine.WorkingDirectory, @"IntegrationTests\Console\Console.csproj"));
            var restoreWithLoggerCommandLine = new CommandLine(
                @"dotnet",
                environmentVariables,
                "restore",
                projectPath,
                "--verbosity",
                verbosity,
                "/noconsolelogger",
                $"/m:{processCount}",
                $@"/l:{loggerString}");

            var buildWithLoggerCommandLine = new CommandLine(
                @"dotnet",
                environmentVariables,
                "build",
                projectPath,
                "--verbosity",
                verbosity,
                "/noconsolelogger",
                $"/m:{processCount}",
                $@"/l:{loggerString}");

            var restoreCommandLine = new CommandLine(
                @"dotnet",
                environmentVariables,
                "restore",
                projectPath,
                "--verbosity",
                verbosity,
                $"/m:{processCount}");

            var buildCommandLine = new CommandLine(
                @"dotnet",
                environmentVariables,
                "build",
                projectPath,
                "--verbosity",
                verbosity,
                $"/m:{processCount}");

            // When
            restoreWithLoggerCommandLine.TryExecute(out CommandLineResult restoreWithLoggerResult).ShouldBe(true);
            restoreCommandLine.TryExecute(out CommandLineResult restoreResult).ShouldBe(true);

            buildWithLoggerCommandLine.TryExecute(out CommandLineResult buildWithLoggerResult).ShouldBe(true);
            buildCommandLine.TryExecute(out CommandLineResult buildResult).ShouldBe(true);

            // Then
            restoreWithLoggerResult.ResultShouldBe(restoreResult);
            buildWithLoggerResult.ResultShouldBe(buildResult, producesTeamCityServiceMessages);
        }
    }
#endif
}