namespace TeamCity.MSBuild.Logger
{
    using System;

    internal interface ILogFormatter
    {
        [NotNull] string FormatLogTimeStamp(DateTime timeStamp);

        [NotNull] string FormatTimeSpan(TimeSpan timeSpan);
    }
}