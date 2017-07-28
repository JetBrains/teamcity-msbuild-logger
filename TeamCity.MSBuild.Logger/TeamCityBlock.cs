namespace TeamCity.MSBuild.Logger
{
    using System;
    using DevTeam.IoC.Contracts;
    using JetBrains.TeamCity.ServiceMessages.Write;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class TeamCityBlock : ITeamCityBlock
    {
        [NotNull] private readonly ILogWriter _noColorLogWriter;
        private readonly string _blockClosedServiceMessage;

        public TeamCityBlock(
            [State] string name,
            [NotNull] IServiceMessageFormatter serviceMessageFormatter,
            [NotNull] [Tag(ColorMode.NoColor)] ILogWriter noColorLogWriter)
        {
            if (serviceMessageFormatter == null) throw new ArgumentNullException(nameof(serviceMessageFormatter));
            _noColorLogWriter = noColorLogWriter ?? throw new ArgumentNullException(nameof(noColorLogWriter));

            var blockOpenedServiceMessage = serviceMessageFormatter.FormatMessage(new ServiceMessage("blockOpened") {{"name", name } });
            _blockClosedServiceMessage = serviceMessageFormatter.FormatMessage(new ServiceMessage("blockClosed") { { "name", name } });
            noColorLogWriter.Write(blockOpenedServiceMessage + "\n");
        }

        public void Dispose()
        {
            _noColorLogWriter.Write(_blockClosedServiceMessage + "\n");
        }
    }
}
