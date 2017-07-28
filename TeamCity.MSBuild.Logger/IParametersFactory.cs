namespace TeamCity.MSBuild.Logger
{
    internal interface IParametersFactory
    {
        [NotNull] Parameters Create();
    }
}
