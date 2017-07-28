namespace TeamCity.MSBuild.Logger.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class CommandLine
    {
        private readonly IDictionary<string, string> _envitonmentVariables;

        public CommandLine([NotNull] string executableFile, [NotNull] IDictionary<string, string> envitonmentVariables, [NotNull] params string[] args)
        {
            ExecutableFile = executableFile ?? throw new ArgumentNullException(nameof(executableFile));
            Args = args ?? throw new ArgumentNullException(nameof(args));
            _envitonmentVariables = envitonmentVariables ?? throw new ArgumentNullException(nameof(envitonmentVariables));
        }

        public static string WorkingDirectory => Path.GetFullPath(Path.Combine(typeof(CommandLine).GetTypeInfo().Assembly.Location, "../../../../../"));

        public string ExecutableFile { [NotNull] get; }

        public string[] Args { [NotNull] get; }

        public bool TryExecute(out CommandLineResult result)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = WorkingDirectory,
                    FileName = ExecutableFile,
                    Arguments = string.Join(" ", Args.Select(i => $"\"{i}\"").ToArray()),
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            foreach (var envVar in _envitonmentVariables)
            {
#if NET45
                if (envVar.Value == null)
                {
                    process.StartInfo.EnvironmentVariables.Remove(envVar.Key);
                }
                else
                {
                    process.StartInfo.EnvironmentVariables[envVar.Key] = envVar.Value;
                }
#else
                if (envVar.Value == null)
                {
                    process.StartInfo.Environment.Remove(envVar.Key);
                }
                else
                {
                    process.StartInfo.Environment[envVar.Key] = envVar.Value;
                }
#endif
            }

            IList<string> stdOut = new List<string>();
            IList<string> stdError = new List<string>();
            process.OutputDataReceived += (sender, args) => { if(args.Data!= null) stdOut.Add(args.Data); };
            process.ErrorDataReceived += (sender, args) => { if(args.Data != null) stdError.Add(args.Data); };

            Console.WriteLine($"Run: {process.StartInfo.FileName} {process.StartInfo.Arguments}");
            if (!process.Start())
            {
                result = default(CommandLineResult);
                return false;
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            result = new CommandLineResult(this, process.ExitCode, stdOut, stdError);
            return true;
        }

        public override string ToString()
        {
            return string.Join(" ", Enumerable.Repeat(ExecutableFile, 1).Concat(Args).Select(i => $"\"{i}\""));
        }
    }
}