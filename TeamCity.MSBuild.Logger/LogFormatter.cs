namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Globalization;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class LogFormatter: ILogFormatter
    {
        public string FormatLogTimeStamp(DateTime timeStamp)
        {
            return timeStamp.ToString("HH:mm:ss.fff", CultureInfo.CurrentCulture);
        }

        public string FormatTimeSpan(TimeSpan timeSpan)
        {
            var length = Math.Min(11, timeSpan.ToString().Length);
            return timeSpan.ToString().Substring(0, length);
        }
    }
}