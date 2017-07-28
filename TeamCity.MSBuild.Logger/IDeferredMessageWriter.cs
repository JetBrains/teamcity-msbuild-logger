using Microsoft.Build.Framework;

namespace TeamCity.MSBuild.Logger
{
    internal interface IDeferredMessageWriter
    {
        void DisplayDeferredProjectStartedEvent(BuildEventContext e);

        void DisplayDeferredStartedEvents(BuildEventContext e);

        void DisplayDeferredTargetStartedEvent(BuildEventContext e);

        void ShownBuildEventContext(BuildEventContext e);
    }
}