namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.IO;
    using System.Threading;
    using JetBrains.Annotations;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DefaultConsole : IConsole
    {
        [NotNull] private readonly IDiagnostics _diagnostics;
        [NotNull] private readonly TextWriter _out;
        // ReSharper disable once IdentifierTypo
        private int _reentrancy;

        public DefaultConsole([NotNull] IDiagnostics diagnostics)
        {
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            // https://youtrack.jetbrains.com/issue/TW-72330
            _out = Console.Out;
        }

        public void Write(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                // ReSharper disable once IdentifierTypo
                var reentrancy = Interlocked.Increment(ref _reentrancy) - 1;
                // ReSharper disable once AccessToModifiedClosure
                _diagnostics.Send(() => $"[{reentrancy} +] Write({text.Trim()})");
                try
                {
                    _out.Write(text);
                }
                finally
                {
                    reentrancy = Interlocked.Decrement(ref _reentrancy);
                    _diagnostics.Send(() => $"[{reentrancy} -] Write({text.Trim()})");
                }
            }
        }
    }
}
