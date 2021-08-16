namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Build.Framework;

    internal class EventContext : IEventRegistry, IEventContext
    {
        private readonly LinkedList<BuildEventArgs> _events = new LinkedList<BuildEventArgs>();

        public IDisposable Register(BuildEventArgs buildEventArgs)
        {
            _events.AddLast(buildEventArgs);
            return Disposable.Create(() => { _events.Remove(buildEventArgs); });
        }

        public bool TryGetEvent(out BuildEventArgs buildEventArgs)
        {
            if (_events.Count > 0)
            {
                buildEventArgs = _events.Last.Value;
                return true;
            }

            buildEventArgs = default;
            return false;
        }
    }
}