namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using Pure.DI;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class HierarchicalMessageWriter : IHierarchicalMessageWriter
    {
        [NotNull] private readonly Dictionary<TeamCityMode, IHierarchicalMessageWriter> _hierarchicalMessageWriter;
        [NotNull] private readonly ILoggerContext _context;

        public HierarchicalMessageWriter(
            [NotNull] ILoggerContext context,
            [NotNull][Tag(TeamCityMode.Off)] IHierarchicalMessageWriter defaultHierarchicalMessageWriter,
            [NotNull][Tag(TeamCityMode.SupportHierarchy)] IHierarchicalMessageWriter teamcityHierarchicalMessageWriter)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _hierarchicalMessageWriter = new Dictionary<TeamCityMode, IHierarchicalMessageWriter>
            {
                { TeamCityMode.Off, defaultHierarchicalMessageWriter ?? throw new ArgumentNullException(nameof(defaultHierarchicalMessageWriter))},
                { TeamCityMode.SupportHierarchy, teamcityHierarchicalMessageWriter ?? throw new ArgumentNullException(nameof(teamcityHierarchicalMessageWriter))}
            };
        }

        private IHierarchicalMessageWriter CurrentHierarchicalMessageWriter => _hierarchicalMessageWriter[_context.Parameters?.TeamCityMode ?? TeamCityMode.Off];

        public void StartBlock(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            CurrentHierarchicalMessageWriter.StartBlock(name);
        }

        public void FinishBlock()
        {
            CurrentHierarchicalMessageWriter.FinishBlock();
        }
    }
}
