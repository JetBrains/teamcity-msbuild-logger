// ReSharper disable UnusedType.Global
namespace TeamCity.MSBuild.Logger
{
    using Microsoft.Build.Framework;

    // ReSharper disable once UnusedMember.Global
    public class TeamCityMsBuildLogger : INodeLogger
    {
        private readonly Composition _composition = new();

        private INodeLogger Logger => _composition.Logger;
        
        public string Parameters
        {
            get => Logger.Parameters;
            set => Logger.Parameters = value;
        }

        public LoggerVerbosity Verbosity
        {
            get => Logger.Verbosity;
            set => Logger.Verbosity = value;
        }
        
        public void Initialize(IEventSource eventSource, int nodeCount) => Logger.Initialize(eventSource, nodeCount);

        public void Initialize(IEventSource eventSource) => Logger.Initialize(eventSource);

        public void Shutdown()
        {
            Logger.Shutdown();
            _composition.Dispose();
        }
    }
}
