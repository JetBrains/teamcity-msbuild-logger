// ReSharper disable StringLiteralTypo
namespace TeamCity.MSBuild.Logger.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using Helpers;
    using IoC;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    [Collection("Integration")]
    // ReSharper disable once InconsistentNaming
    public class MSBuildIntegrationTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public MSBuildIntegrationTests(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

        [Theory]
        [InlineData("net452", 10, "minimal", null, false)]
        [InlineData("net452", 1, "m", null, false)]
        [InlineData("net452", 1, "quiet", null, false)]
        [InlineData("net452", 10, "quiet", null, false)]
        [InlineData("net452", 1, "q", null, false)]
        [InlineData("net452", 10, "q", null, false)]
        [InlineData("net452", 10, "normal", null, false)]
        [InlineData("net452", 10, "normal", "TEAMcity", true)]
        [InlineData("net452", 10, "n", null, false)]
        [InlineData("net452", 10, "detailed", null, false)]
        [InlineData("net452", 10, "d", null, false)]
        [InlineData("net452", 10, "diagnostic", null, false)]
        [InlineData("net452", 10, "diag", null, false)]
        [InlineData("net452", 10, "deTailed", null, false)]
        [InlineData("net452", 10, "diag", "teamcity", true)]
        [InlineData("net452", 10, "deTailed", "teamcity", true)]
        // ReSharper disable once InconsistentNaming
        public void ShouldProduceSameMessagesAsConsoleLoggerWhenMSBuild(
            string framework,
            int processCount,
            [NotNull] string verbosity,
            [CanBeNull] string parameters,
            bool producesTeamCityServiceMessages)
        {
            // Given
            WriteLine();
            WriteLine($@"Run: framework={framework}, processCount={processCount}, verbosity={verbosity}");

            var environmentVariables = new Dictionary<string, string>();
            var loggerString = framework.CreateLoggerString(parameters);
            var projectPath = Path.GetFullPath(Path.Combine(CommandLine.WorkingDirectory, @"IntegrationTests\Console\Console.csproj"));
            var restoreWithLoggerCommandLine = new CommandLine(
                "msbuild.exe",
                environmentVariables,
                "/t:restore",
                projectPath,
                "/noconsolelogger",
                $"/m:{processCount}",
                $@"/logger:{loggerString}");

            var buildWithLoggerCommandLine = new CommandLine(
                "msbuild.exe",
                environmentVariables,
                "/t:build",
                projectPath,
                "/noconsolelogger",
                $"/verbosity:{verbosity}",
                $"/m:{processCount}",
                $@"/l:{loggerString}");

            var restoreCommandLine = new CommandLine(
                "msbuild.exe",
                environmentVariables,
                "/t:restore",
                projectPath,
                $"/m:{processCount}");

            var buildCommandLine = new CommandLine(
                "msbuild.exe",
                environmentVariables,
                "/t:build",
                projectPath,
                $"/verbosity:{verbosity}",
                $"/m:{processCount}");

            // When
            WriteLine();
            WriteLine(@"Without TeamCity logger");

            restoreWithLoggerCommandLine.TryExecute(out var restoreWithLoggerResult).ShouldBe(true);
            buildCommandLine.TryExecute(out var buildResult).ShouldBe(true);

            WriteLine();
            WriteLine(@"With TeamCity logger");

            restoreCommandLine.TryExecute(out var restoreResult).ShouldBe(true);
            buildWithLoggerCommandLine.TryExecute(out var buildWithLoggerResult).ShouldBe(true);

            // Then
            restoreWithLoggerResult.ResultShouldBe(restoreResult, producesTeamCityServiceMessages);
            buildWithLoggerResult.ResultShouldBe(buildResult);
        }

        private void WriteLine(string message = "") => _testOutputHelper.WriteLine(message);
    }
}