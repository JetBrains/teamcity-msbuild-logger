namespace TeamCity.MSBuild.Logger
{
    using System;
    using JetBrains.Annotations;

    internal interface ILogFormatter
    {
        [NotNull] string FormatLogTimeStamp(DateTime timeStamp);

        [NotNull] string FormatTimeSpan(TimeSpan timeSpan);
    }
}