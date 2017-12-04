namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Globalization;
    using JetBrains.TeamCity.ServiceMessages.Write.Special;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class TeamCityStatistics : IStatistics
    {
        [NotNull] private readonly ITeamCityWriter _writer;
        [NotNull] private readonly ILoggerContext _context;

        public TeamCityStatistics(
            [NotNull] ILoggerContext context,
            [NotNull] ITeamCityWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Publish()
        {
            _writer.WriteBuildStatistics("BuildStatsW", _context.WarningCount.ToString(CultureInfo.InvariantCulture));
            _writer.WriteBuildStatistics("BuildStatsE", _context.ErrorCount.ToString(CultureInfo.InvariantCulture));
        }
    }
}