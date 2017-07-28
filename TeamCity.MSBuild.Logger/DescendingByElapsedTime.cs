namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;

    internal class DescendingByElapsedTime : IComparer<IPerformanceCounter>
    {
        public static readonly IComparer<IPerformanceCounter> Shared = new DescendingByElapsedTime();

        private DescendingByElapsedTime()
        {
        }

        public int Compare(IPerformanceCounter x, IPerformanceCounter y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            if (!x.ReenteredScope && !y.ReenteredScope)
            {
                return TimeSpan.Compare(x.ElapsedTime, y.ElapsedTime);
            }

            if (x.Equals(y))
            {
                return 0;
            }

            return x.ReenteredScope ? -1 : 1;
        }
    }
}