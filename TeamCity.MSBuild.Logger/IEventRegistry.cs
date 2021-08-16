namespace TeamCity.MSBuild.Logger
{
    using System;
    using Microsoft.Build.Framework;

    internal interface IEventRegistry
    {
        IDisposable Register(BuildEventArgs buildEventArgs);
    }
}