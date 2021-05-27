namespace TeamCity.MSBuild.Logger
{
    using JetBrains.Annotations;

    internal interface IConsole
    {
        void Write([CanBeNull] string text);
    }
}