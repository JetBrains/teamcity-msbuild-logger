// ReSharper disable UnusedType.Global
namespace TeamCity.MSBuild.Logger
{
    using Microsoft.Build.Framework;

    // ReSharper disable once UnusedMember.Global
    public class TeamCityMsBuildLogger : INodeLogger
    {
        private readonly INodeLogger _logger = Composer.Resolve<INodeLogger>();

        public string Parameters
        {
            get => _logger.Parameters;
            set => _logger.Parameters = value;
        }

        public LoggerVerbosity Verbosity
        {
            get => _logger.Verbosity;
            set => _logger.Verbosity = value;
        }

        public void Initialize(IEventSource eventSource, int nodeCount)
        {
            _logger.Initialize(eventSource, nodeCount);
        }

        public void Initialize(IEventSource eventSource)
        {
            _logger.Initialize(eventSource);
        }

        public void Shutdown()
        {
            _logger.Shutdown();
        }
    }
}
