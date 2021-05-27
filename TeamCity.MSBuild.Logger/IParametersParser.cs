namespace TeamCity.MSBuild.Logger
{
    using JetBrains.Annotations;

    internal interface IParametersParser
    {
        bool TryParse([CanBeNull] string parametersString, [NotNull] Parameters parameters, out string error);
    }
}