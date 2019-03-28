namespace TeamCity.MSBuild.Logger
{
    using IoC;

    internal interface ILogWriter
    {
        void Write([CanBeNull] string message, [CanBeNull] IConsole console = null);

        void SetColor(Color color);

        void ResetColor();
    }
}
