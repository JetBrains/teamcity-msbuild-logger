namespace TeamCity.MSBuild.Logger
{
    using JetBrains.Annotations;

    internal interface IStringService
    {
        [NotNull] string FormatResourceString([NotNull] string resourceName, [NotNull] params object[] args);

        // ReSharper disable once IdentifierTypo
        [CanBeNull] string UnescapeAll([CanBeNull] string escapedString);
    }
}