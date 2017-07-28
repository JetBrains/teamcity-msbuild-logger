namespace TeamCity.MSBuild.Logger
{
    internal interface IStringService
    {
        [NotNull] string FormatResourceString([NotNull] string resourceName, [NotNull] params object[] args);

        [CanBeNull] string UnescapeAll([CanBeNull] string escapedString);
    }
}