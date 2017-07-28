namespace TeamCity.MSBuild.Logger.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Helpers;
    using Shouldly;
    using Xunit;

#if NETCOREAPP1_0
    public class IntegrationTests
    {
        [Theory]
        [ClassData(typeof(TestDataGenerator))]
        public void ShouldProduceSameMessagesAsConsoleLoggerViaDotnet(
            int processCount,
            [NotNull] string verbosity,
            [CanBeNull] string parameters,
            [CanBeNull] string envVars,
            bool producesTeamCityServiceMessages)
        {
            // Given
            var environmentVariables = ExtractDictionary(envVars);
            if (environmentVariables.Count > 0)
            {
                return;
            }

            var loggerString = CreateLoggerString("netcoreapp1.0", parameters);
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
            CheckResult(restoreWithLoggerResult, restoreResult);
            CheckResult(buildWithLoggerResult, buildResult, producesTeamCityServiceMessages);
        }

        [Theory]
        [ClassData(typeof(TestDataGenerator))]
        // ReSharper disable once InconsistentNaming
        public void ShouldProduceSameMessagesAsConsoleLoggerWhenMSBuild(
            int processCount,
            [NotNull] string verbosity,
            [CanBeNull] string parameters,
            [CanBeNull] string envVars,
            bool producesTeamCityServiceMessages)
        {
            // Given
            var environmentVariables = ExtractDictionary(envVars);
            var loggerString = CreateLoggerString("net452", parameters);
            var projectPath = Path.GetFullPath(Path.Combine(CommandLine.WorkingDirectory, @"IntegrationTests\Console\Console.csproj"));
            var restoreWithLoggerCommandLine = new CommandLine(
                @"msbuild",
                environmentVariables,
                "/t:restore",
                projectPath,
                "/noconsolelogger",
                $"/m:{processCount}",
                $@"/logger:{loggerString}");

            var buildWithLoggerCommandLine = new CommandLine(
                @"msbuild",
                environmentVariables,
                "/t:build",
                projectPath,
                "/noconsolelogger",
                $"/verbosity:{verbosity}",
                $"/m:{processCount}",
                $@"/l:{loggerString}");

            var restoreCommandLine = new CommandLine(
                @"msbuild",
                environmentVariables,
                "/t:restore",
                projectPath,
                $"/m:{processCount}");

            var buildCommandLine = new CommandLine(
                @"msbuild",
                environmentVariables,
                "/t:build",
                projectPath,
                $"/verbosity:{verbosity}",
                $"/m:{processCount}");

            // When
            restoreWithLoggerCommandLine.TryExecute(out CommandLineResult restoreWithLoggerResult).ShouldBe(true);
            restoreCommandLine.TryExecute(out CommandLineResult restoreResult).ShouldBe(true);

            buildWithLoggerCommandLine.TryExecute(out CommandLineResult buildWithLoggerResult).ShouldBe(true);
            buildCommandLine.TryExecute(out CommandLineResult buildResult).ShouldBe(true);

            // Then
            CheckResult(restoreWithLoggerResult, restoreResult, producesTeamCityServiceMessages);
            CheckResult(buildWithLoggerResult, buildResult);
        }

        private static void CheckResult([NotNull] CommandLineResult actualResult, [NotNull] CommandLineResult expectedResult, [CanBeNull] bool? producesTeamCityServiceMessages = null)
        {
            if (actualResult == null) throw new ArgumentNullException(nameof(actualResult));
            if (expectedResult == null) throw new ArgumentNullException(nameof(expectedResult));
            actualResult.ExitCode.ShouldBe(expectedResult.ExitCode);
            CheckOutput(actualResult.StdOut, expectedResult.StdOut);
            CheckOutput(actualResult.StdError, expectedResult.StdError);
            if (producesTeamCityServiceMessages.HasValue)
            {
                (ServiceMessages.GetNumberServiceMessage(actualResult.StdOut) > 0).ShouldBe(producesTeamCityServiceMessages.Value);
            }

            ServiceMessages.ResultShouldContainCorrectStructureAndSequence(actualResult.StdOut);
            ServiceMessages.ResultShouldContainCorrectStructureAndSequence(actualResult.StdError);
        }

        private static void CheckOutput([NotNull] IEnumerable<string> actualLines, [NotNull] IEnumerable<string> expectedLines)
        {
            if (actualLines == null) throw new ArgumentNullException(nameof(actualLines));
            if (expectedLines == null) throw new ArgumentNullException(nameof(expectedLines));
            // ReSharper disable once PossibleMultipleEnumeration
            var filteredActualLines = ServiceMessages.FilterTeamCityServiceMessages(actualLines).ToList();
            // ReSharper disable once PossibleMultipleEnumeration
            var curExpectedLines = ServiceMessages.FilterTeamCityServiceMessages(actualLines).ToList();
            filteredActualLines.Count.ShouldBe(curExpectedLines.Count);
            foreach (var pair in filteredActualLines.Zip(curExpectedLines, (actualLine, expectedLine) => new { actualLine, expectedLine }))
            {
                CheckLines(pair.actualLine, pair.expectedLine);
            }
        }

        private static void CheckLines([CanBeNull] string actualLine, [CanBeNull] string expectedLine)
        {
            var modifiedActualLine = ReplaceChangableItems(actualLine);
            var modifiedExpectedLine = ReplaceChangableItems(expectedLine);
            if (modifiedActualLine != modifiedExpectedLine)
            {
                Assert.Equal(modifiedActualLine,modifiedExpectedLine);
            }
        }

        private static string ReplaceChangableItems([CanBeNull] string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return line;
            }

            return new string(ExcludeChangableChars(line).ToArray());
        }

        private static IEnumerable<char> ExcludeChangableChars([NotNull] IEnumerable<char> chars)
        {
            if (chars == null) throw new ArgumentNullException(nameof(chars));
            foreach (var c in chars)
            {
                if (char.IsDigit(c))
                {
                    continue;
                }

                yield return c;
            }
        }

        private static string CreateLoggerString(
            [NotNull] string framework,
            [CanBeNull] string parameters = "")
        {
            if (framework == null) throw new ArgumentNullException(nameof(framework));
#if DEBUG
            const string configuration = "Debug";
#else
            const string configuration = "Release";
#endif

            var loggerPath = Path.GetFullPath(Path.Combine(CommandLine.WorkingDirectory, $@"TeamCity.MSBuild.Logger\bin\{configuration}\{framework}\TeamCity.MSBuild.Logger.dll"));
            if (!string.IsNullOrWhiteSpace(parameters))
            {
                parameters = ";" + parameters;
            }

            return $"TeamCity.MSBuild.Logger.TeamCityMSBuildLogger,{loggerPath}{parameters}";
        }

        private static IDictionary<string, string> ExtractDictionary([CanBeNull] string dict)
        {
            if (string.IsNullOrWhiteSpace(dict))
            {
                return new Dictionary<string, string>();
            }

            return (
                from item in dict.Split(';')
                let parts = item.Split('=')
                where parts.Length == 2
                select new {name = parts[0].Trim(), value = parts[1].Trim()}).ToDictionary(i => i.name, i => i.value);
        }

        private class TestDataGenerator : IEnumerable<object[]>
        {
            private readonly List<object[]> _data = new List<object[]>
            {
                new object[] {10, "minimal", null, null, false},
                new object[] {1, "m", null, null, false},
                new object[] {1, "quiet", null, null, false},
                new object[] {10, "quiet", null, null, false},
                new object[] {1, "q", null, null, false},
                new object[] {10, "q", null, null, false},
                new object[] {10, "normal", null, null, false},
                new object[] {10, "normal", "TEAMcity", null, true},
                new object[] {10, "n", null, null, false},
                new object[] {10, "detailed", null, null, false},
                new object[] {10, "d", null, null, false},
                new object[] {10, "diagnostic", null, null, false},
                new object[] {10, "diag", null, null, false},
                new object[] {10, "deTailed", null, null, false},
                new object[] {10, "diag", "teamcity", null, true},
                new object[] {10, "diag", "teamcity", null, true},
                new object[] {10, "deTailed", "teamcity", null, true},
                new object[] {10, "deTailed", null, $"{ParametersFactory.TeamcityVersionEnvVarName}=10.0", true},
            };

            public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
#endif
}