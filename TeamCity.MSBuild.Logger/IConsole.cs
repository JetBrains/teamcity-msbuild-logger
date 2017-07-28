namespace TeamCity.MSBuild.Logger
{
    internal interface IConsole
    {
        void Write([CanBeNull] string text);
    }
}