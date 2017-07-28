namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Build.Framework;

    internal interface IPerformanceCounter
    {
        TimeSpan ElapsedTime { get; }

        bool ReenteredScope { get; }

        int MessageIdentLevel { set; }

        void AddEventFinished(string projectTargetNames, BuildEventContext buildEventContext, DateTime eventTimeStamp);

        void AddEventStarted([CanBeNull] string projectTargetNames, BuildEventContext buildEventContext, DateTime eventTimeStamp, [CanBeNull] IEqualityComparer<BuildEventContext> comparer);

        void PrintCounterMessage();
    }
}