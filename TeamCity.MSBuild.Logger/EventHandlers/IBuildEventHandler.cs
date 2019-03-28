namespace TeamCity.MSBuild.Logger.EventHandlers
{
    using IoC;
    using Microsoft.Build.Framework;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal interface IBuildEventHandler<in TBuildEventArgs> where TBuildEventArgs : BuildEventArgs
    {
        void Handle([NotNull] TBuildEventArgs e);
    }
}
