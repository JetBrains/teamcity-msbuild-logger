namespace TeamCity.MSBuild.Logger
{
    using IoC;

    internal interface IPathService
    {
        [NotNull] string GetFileName([NotNull] string path);
    }
}
