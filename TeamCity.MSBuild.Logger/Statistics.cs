﻿namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using Pure.DI;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Statistics : IStatistics
    {
        [NotNull] private readonly ILoggerContext _context;
        private readonly Dictionary<StatisticsMode, IStatistics> _statistics;

        public Statistics(
            [NotNull] ILoggerContext context,
            [NotNull][Tag(StatisticsMode.Default)] IStatistics defaultStatistics,
            [NotNull][Tag(StatisticsMode.TeamCity)] IStatistics teamcityStatistics)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _statistics = new Dictionary<StatisticsMode, IStatistics>
            {
                { StatisticsMode.Default, defaultStatistics ?? throw new ArgumentNullException(nameof(defaultStatistics))},
                { StatisticsMode.TeamCity, teamcityStatistics ?? throw new ArgumentNullException(nameof(teamcityStatistics))}
            };
        }

        private IStatistics CurrentStatistics => _statistics[_context.Parameters?.StatisticsMode ?? StatisticsMode.Default];

        public void Publish()
        {
            CurrentStatistics.Publish();
        }
    }
}