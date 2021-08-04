namespace TeamCity.MSBuild.Logger.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using JetBrains.Annotations;
    using Shouldly;
    using Xunit;

    internal static class IntegrationTests
    {

        public static void ResultShouldBe([NotNull] this CommandLineResult actualResult, [NotNull] CommandLineResult expectedResult, [CanBeNull] bool? producesTeamCityServiceMessages = null)
        {
            if (actualResult == null) throw new ArgumentNullException(nameof(actualResult));
            if (expectedResult == null) throw new ArgumentNullException(nameof(expectedResult));
            actualResult.ExitCode.ShouldBe(expectedResult.ExitCode);
            CheckOutput(actualResult.StdOut);
            CheckOutput(actualResult.StdError);
            if (producesTeamCityServiceMessages.HasValue)
            {
                (ServiceMessages.GetNumberServiceMessage(actualResult.StdOut) > 0).ShouldBe(producesTeamCityServiceMessages.Value);
            }

            ServiceMessages.ResultShouldContainCorrectStructureAndSequence(actualResult.StdOut);
            ServiceMessages.ResultShouldContainCorrectStructureAndSequence(actualResult.StdError);
        }

        private static void CheckOutput([NotNull] this IEnumerable<string> actualLines)
        {
            if (actualLines == null) throw new ArgumentNullException(nameof(actualLines));
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

        private static void CheckLines([CanBeNull] this string actualLine, [CanBeNull] string expectedLine)
        {
            var modifiedActualLine = ReplaceChangeableItems(actualLine);
            var modifiedExpectedLine = ReplaceChangeableItems(expectedLine);
            if (modifiedActualLine != modifiedExpectedLine)
            {
                Assert.Equal(modifiedActualLine, modifiedExpectedLine);
            }
        }

        private static string ReplaceChangeableItems([CanBeNull] string line) => string.IsNullOrWhiteSpace(line) ? line : new string(ExcludeChangeableChars(line).ToArray());

        private static IEnumerable<char> ExcludeChangeableChars([NotNull] IEnumerable<char> chars)
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

        public static string CreateLoggerString(
            [NotNull] this string framework,
            [CanBeNull] string parameters = "")
        {
            if (framework == null) throw new ArgumentNullException(nameof(framework));
#if DEBUG
            const string configuration = "Debug";
#else
            const string configuration = "Release";
#endif

            var loggerPath = Path.GetFullPath(Path.Combine(CommandLine.WorkingDirectory, $@"TeamCity.MSBuild.Logger\bin\{configuration}\{framework}\publish\TeamCity.MSBuild.Logger.dll"));
            if (!string.IsNullOrWhiteSpace(parameters))
            {
                parameters = ";" + parameters;
            }

            return $"TeamCity.MSBuild.Logger.TeamCityMSBuildLogger,{loggerPath}{parameters}";
        }
    }
}
