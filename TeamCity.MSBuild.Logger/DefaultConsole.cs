namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Threading;
    using IoC;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DefaultConsole : IConsole
    {
        [NotNull] private readonly IDiagnostics _diagnostics;
        // ReSharper disable once IdentifierTypo
        private int _reentrancy;

        public DefaultConsole([NotNull] IDiagnostics diagnostics)
        {
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
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
                    Console.Write(text);
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
