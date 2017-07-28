namespace TeamCity.MSBuild.Logger
{
    internal interface IPathService
    {
        [NotNull] string GetFileName([NotNull] string path);
    }
}
