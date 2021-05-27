namespace TeamCity.MSBuild.Logger
{
    using Microsoft.Build.Framework;
    using JetBrains.Annotations;

    internal interface IEventFormatter
    {
        [NotNull] string FormatEventMessage([NotNull] BuildErrorEventArgs e, bool removeCarriageReturn, bool showProjectFile);

        [NotNull] string FormatEventMessage([NotNull] BuildMessageEventArgs e, bool removeCarriageReturn, bool showProjectFile);

        [NotNull] string FormatEventMessage([NotNull] BuildWarningEventArgs e, bool removeCarriageReturn, bool showProjectFile);
    }
}