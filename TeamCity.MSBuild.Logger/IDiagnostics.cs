namespace TeamCity.MSBuild.Logger
{
    using System;

    internal interface IDiagnostics
    {
        void Send(Func<string> diagnosticsBuilder);
    }
}
