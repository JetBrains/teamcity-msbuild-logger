namespace TeamCity.MSBuild.Logger
{
    using IoC;

    internal interface IParametersParser
    {
        bool TryParse([CanBeNull] string parametersString, [NotNull] Parameters parameters, out string error);
    }
}