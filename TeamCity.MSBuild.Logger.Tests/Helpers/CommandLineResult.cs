// ReSharper disable All
namespace TeamCity.MSBuild.Logger.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class CommandLineResult
    {
        public CommandLineResult(
            [IoC.NotNull] CommandLine commandLine,
            int exitCode,
            [IoC.NotNull] IList<string> stdOut,
            [IoC.NotNull] IList<string> stdError)
        {
            CommandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
            ExitCode = exitCode;
            StdOut = stdOut ?? throw new ArgumentNullException(nameof(stdOut));
            StdError = stdError ?? throw new ArgumentNullException(nameof(stdError));
        }

        public CommandLine CommandLine { [IoC.NotNull] get; }

        public int ExitCode { get; }

        public IEnumerable<string> StdOut { [IoC.NotNull] get; }

        public IEnumerable<string> StdError { [IoC.NotNull] get; }
    }
}