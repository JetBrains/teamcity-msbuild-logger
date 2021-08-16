namespace TeamCity.MSBuild.Logger
{
    using Microsoft.Build.Framework;

    internal interface IEventContext
    {
        bool TryGetEvent(out BuildEventArgs buildEventArgs);
    }
}