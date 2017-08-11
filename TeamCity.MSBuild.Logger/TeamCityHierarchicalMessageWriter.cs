namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class TeamCityHierarchicalMessageWriter : IHierarchicalMessageWriter
    {
        [NotNull] private readonly Func<string, ITeamCityBlock> _teamCityBlockFactory;
        [NotNull] private readonly Dictionary<HierarchicalKey, ITeamCityBlock> _blocks = new Dictionary<HierarchicalKey, ITeamCityBlock>();

        public TeamCityHierarchicalMessageWriter(
            [NotNull] Func<string, ITeamCityBlock> teamCityBlockFactory)
        {
            _teamCityBlockFactory = teamCityBlockFactory ?? throw new ArgumentNullException(nameof(teamCityBlockFactory));
        }

        public void StartBlock(HierarchicalKey key, string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            _blocks.Add(key, _teamCityBlockFactory(name));
        }

        public void FinishBlock(HierarchicalKey key)
        {
            if (_blocks.TryGetValue(key, out ITeamCityBlock block))
            {
                _blocks.Remove(key);
                block.Dispose();
            }
        }
    }
}
