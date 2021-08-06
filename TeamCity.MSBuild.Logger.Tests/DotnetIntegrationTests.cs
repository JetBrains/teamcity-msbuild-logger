// ReSharper disable StringLiteralTypo
namespace TeamCity.MSBuild.Logger.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Helpers;
    using JetBrains.Annotations;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    [Collection("Integration")]
    public class DotnetIntegrationTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DotnetIntegrationTests(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

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
            WriteLine();
            WriteLine($@"Run: framework={framework}, sdk={dotnetVersion}, processCount={processCount}, verbosity={verbosity}");

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
            WriteLine();
            WriteLine(@"Without TeamCity logger");

            restoreCommandLine.TryExecute(out var restoreResult).ShouldBe(true);
            buildCommandLine.TryExecute(out var buildResult).ShouldBe(true);

            WriteLine();
            WriteLine(@"With TeamCity logger");

            restoreWithLoggerCommandLine.TryExecute(out var restoreWithLoggerResult).ShouldBe(true);
            buildWithLoggerCommandLine.TryExecute(out var buildWithLoggerResult).ShouldBe(true);

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

        private void WriteLine(string message = "") => _testOutputHelper.WriteLine(message);

        private class TestDataGenerator : IEnumerable<object[]>
        {
            private static readonly object[][] Cases =
            {
                new object[] { "netstandard1.6", 10, "minimal", null, false },
                new object[] { "netstandard1.6", 1, "m", null, false },
                new object[] { "netstandard1.6", 1, "quiet", null, false },
                new object[] { "netstandard1.6", 10, "quiet", null, false },
                new object[] { "netstandard1.6", 1, "q", null, false },
                new object[] { "netstandard1.6", 10, "q", null, false },
                new object[] { "netstandard1.6", 10, "normal", null, false },
                new object[] { "netstandard1.6", 10, "normal", "TEAMcity", true },
                new object[] { "netstandard1.6", 10, "n", null, false },
                new object[] { "netstandard1.6", 10, "detailed", null, false },
                new object[] { "netstandard1.6", 10, "d", null, false },
                new object[] { "netstandard1.6", 10, "diagnostic", null, false },
                new object[] { "netstandard1.6", 10, "diag", null, false },
                new object[] { "netstandard1.6", 10, "deTailed", null, false },
                new object[] { "netstandard1.6", 10, "diag", "teamcity", true },
                new object[] { "netstandard1.6", 10, "deTailed", "teamcity", true }
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
                data[^1] = dotNetVersion;
                return data;
            }
        }
    }
}