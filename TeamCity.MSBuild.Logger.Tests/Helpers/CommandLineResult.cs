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
            [NotNull] CommandLine commandLine,
            int exitCode,
            [NotNull] IList<string> stdOut,
            [NotNull] IList<string> stdError)
        {
            CommandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
            ExitCode = exitCode;
            StdOut = stdOut ?? throw new ArgumentNullException(nameof(stdOut));
            StdError = stdError ?? throw new ArgumentNullException(nameof(stdError));
        }

        public CommandLine CommandLine { [NotNull] get; }

        public int ExitCode { get; }

        public IEnumerable<string> StdOut { [NotNull] get; }

        public IEnumerable<string> StdError { [NotNull] get; }
    }
}