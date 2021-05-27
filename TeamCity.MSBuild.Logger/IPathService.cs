namespace TeamCity.MSBuild.Logger
{
    using JetBrains.Annotations;

    internal interface IPathService
    {
        [NotNull] string GetFileName([NotNull] string path);
    }
}
