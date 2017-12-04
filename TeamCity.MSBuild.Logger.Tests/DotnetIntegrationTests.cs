namespace TeamCity.MSBuild.Logger.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Helpers;
    using Shouldly;
    using Xunit;

#if NETCOREAPP1_0
    [Collection("Integration")]
    public class DotnetIntegrationTests
    {
        [Theory]
        [ClassData(typeof(TestDataGenerator))]
        public void ShouldProduceSameMessagesAsConsoleLoggerViaDotnet(
            string framework,
            int processCount,
            [NotNull] string verbosity,
            [CanBeNull] string parameters,
            bool producesTeamCityServiceMessages,
            string dotnetVersion)
        {
            // Given
            var environmentVariables = new Dictionary<string, string>();
            var loggerString = framework.CreateLoggerString(parameters);
            var projectDir = Path.GetFullPath(Path.Combine(CommandLine.WorkingDirectory, @"IntegrationTests\Console"));
            var projectPath = Path.Combine(projectDir, "Console.csproj");
            var globalJsonPath = Path.Combine(projectDir, "global.json");

            using(var json = File.CreateText(globalJsonPath))
            {
                json.WriteLine("{");
                json.WriteLine("\"sdk\": {");
                json.WriteLine($"\"version\": \"{dotnetVersion}\"");
                json.WriteLine("}");
                json.WriteLine("}");
            }

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
            restoreWithLoggerCommandLine.TryExecute(out var restoreWithLoggerResult).ShouldBe(true);
            restoreCommandLine.TryExecute(out var restoreResult).ShouldBe(true);

            buildWithLoggerCommandLine.TryExecute(out var buildWithLoggerResult).ShouldBe(true);
            buildCommandLine.TryExecute(out var buildResult).ShouldBe(true);

            // Then
            restoreWithLoggerResult.ResultShouldBe(restoreResult);
            buildWithLoggerResult.ResultShouldBe(buildResult, producesTeamCityServiceMessages);

            try
            {
                File.Delete(globalJsonPath);
            }
            catch
            {
                // ignored
            }
        }

        private class TestDataGenerator : IEnumerable<object[]>
        {
            private static readonly object[][] Cases =
            {
                new object[] { "netcoreapp1.0", 10, "minimal", null, false },
                new object[] { "netcoreapp1.0", 1, "m", null, false },
                new object[] { "netcoreapp1.0", 1, "quiet", null, false },
                new object[] { "netcoreapp1.0", 10, "quiet", null, false },
                new object[] { "netcoreapp1.0", 1, "q", null, false },
                new object[] { "netcoreapp1.0", 10, "q", null, false },
                new object[] { "netcoreapp1.0", 10, "normal", null, false },
                new object[] { "netcoreapp1.0", 10, "normal", "TEAMcity", true },
                new object[] { "netcoreapp1.0", 10, "n", null, false },
                new object[] { "netcoreapp1.0", 10, "detailed", null, false },
                new object[] { "netcoreapp1.0", 10, "d", null, false },
                new object[] { "netcoreapp1.0", 10, "diagnostic", null, false },
                new object[] { "netcoreapp1.0", 10, "diag", null, false },
                new object[] { "netcoreapp1.0", 10, "deTailed", null, false },
                new object[] { "netcoreapp1.0", 10, "diag", "teamcity", true },
                new object[] { "netcoreapp1.0", 10, "deTailed", "teamcity", true }
            };

            public IEnumerator<object[]> GetEnumerator() => CreateCases().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private static IEnumerable<object[]> CreateCases()
            {
                var cmd = Path.Combine(CommandLine.WorkingDirectory, @"tools\dotnet-sdk.cmd");
                var listCommandLine = new CommandLine(cmd, new Dictionary<string, string>(), "list");
                listCommandLine.TryExecute(out var listCommandLineResult).ShouldBe(true);
                var dotnetVersions = listCommandLineResult.StdOut.Skip(1);

                return
                    from dotnetVersion in dotnetVersions
                    from caseData in Cases
                    select CreateCase(caseData, dotnetVersion);
            }

            private static object[] CreateCase(object[] caseData, string dotNetVersion)
            {
                var data = new object[caseData.Length + 1];
                caseData.CopyTo(data, 0);
                data[data.Length - 1] = dotNetVersion;
                return data;
            }
        }
    }
#endif
}