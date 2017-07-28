using DevTeam.IoC.Contracts;
using System;
using System.Collections.Generic;

namespace TeamCity.MSBuild.Logger
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class HierarchicalMessageWriter : IHierarchicalMessageWriter
    {
        [NotNull] private readonly Dictionary<TeamCityMode, IHierarchicalMessageWriter> _hierarchicalMessageWriter;
        [NotNull] private readonly ILoggerContext _context;

        public HierarchicalMessageWriter(
            [NotNull] ILoggerContext context,
            [NotNull] [Tag(TeamCityMode.Off)] IHierarchicalMessageWriter defaultHierarchicalMessageWriter,
            [NotNull] [Tag(TeamCityMode.SupportHierarchy)] IHierarchicalMessageWriter teamcityHierarchicalMessageWriter)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _hierarchicalMessageWriter = new Dictionary<TeamCityMode, IHierarchicalMessageWriter>
            {
                { TeamCityMode.Off, defaultHierarchicalMessageWriter ?? throw new ArgumentNullException(nameof(defaultHierarchicalMessageWriter))},
                { TeamCityMode.SupportHierarchy, teamcityHierarchicalMessageWriter ?? throw new ArgumentNullException(nameof(teamcityHierarchicalMessageWriter))}
            };
        }

        private IHierarchicalMessageWriter CurrentHierarchicalMessageWriter => _hierarchicalMessageWriter[_context.Parameters?.TeamCityMode ?? TeamCityMode.Off];

        public void StartBlock(HierarchicalKey key, string name, string message = "")
        {
            CurrentHierarchicalMessageWriter.StartBlock(key, name, message);
        }

        public void FinishBlock(HierarchicalKey key, string message = "")
        {
            CurrentHierarchicalMessageWriter.FinishBlock(key, message);
        }
    }
}
