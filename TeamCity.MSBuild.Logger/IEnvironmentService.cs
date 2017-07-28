namespace TeamCity.MSBuild.Logger
{
    internal interface IEnvironmentService
    {
        [NotNull] string GetEnvironmentVariable([NotNull] string name);
    }
}
