namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class TeamCityHierarchicalMessageWriter : IHierarchicalMessageWriter
    {
        [NotNull] private readonly Func<string, ITeamCityBlock> _teamCityBlockFactory;
        [NotNull] private readonly Dictionary<HierarchicalKey, ITeamCityBlock> _blocks = new Dictionary<HierarchicalKey, ITeamCityBlock>();
        [NotNull] private readonly IMessageWriter _messageWriter;

        public TeamCityHierarchicalMessageWriter(
            [NotNull] IMessageWriter messageWriter,
            [NotNull] Func<string, ITeamCityBlock> teamCityBlockFactory)
        {
            _teamCityBlockFactory = teamCityBlockFactory ?? throw new ArgumentNullException(nameof(teamCityBlockFactory));
            _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
        }

        public void StartBlock(HierarchicalKey key, string name, string message = "")
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            _blocks.Add(key, _teamCityBlockFactory(name));
            if (!string.IsNullOrEmpty(message))
            {
                _messageWriter.WriteMessageAligned(message, true);
            }
        }

        public void FinishBlock(HierarchicalKey key, string message = "")
        {
            if (!string.IsNullOrEmpty(message))
            {
                _messageWriter.WriteMessageAligned(message, true);
            }

            if (_blocks.TryGetValue(key, out ITeamCityBlock block))
            {
                _blocks.Remove(key);
                block.Dispose();
            }
        }
    }
}
