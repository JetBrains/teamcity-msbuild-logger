namespace TeamCity.MSBuild.Logger
{
    using IoC;

    internal interface IConsole
    {
        void Write([CanBeNull] string text);
    }
}