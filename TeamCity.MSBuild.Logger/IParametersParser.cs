namespace TeamCity.MSBuild.Logger
{
    internal interface IParametersParser
    {
        bool TryParse([CanBeNull] string parametersString, [NotNull] Parameters parameters, out string error);
    }
}