namespace TeamCity.MSBuild.Logger
{
    using System;
    using IoC;

    internal interface ILogFormatter
    {
        [NotNull] string FormatLogTimeStamp(DateTime timeStamp);

        [NotNull] string FormatTimeSpan(TimeSpan timeSpan);
    }
}