namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;
    using DevTeam.IoC.Contracts;
    using Microsoft.Build.Framework;
    using EventHandlers;
    using JetBrains.TeamCity.ServiceMessages.Write;
    using JetBrains.TeamCity.ServiceMessages.Write.Special;
    using JetBrains.TeamCity.ServiceMessages.Write.Special.Impl.Updater;

    internal class IoCConfiguration : IConfiguration
    {
        public IEnumerable<IConfiguration> GetDependencies([NotNull] IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return container.Feature(Wellknown.Feature.Lifetimes);
            yield return container.Feature(Wellknown.Feature.Resolvers);
        }

        public IEnumerable<IDisposable> Apply([NotNull] IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return container.Register()
                .Lifetime(Wellknown.Lifetime.Singleton).Lifetime(Wellknown.Lifetime.AutoDisposing).With()
                    .Autowiring<INodeLogger, NodeLogger>()
                    .And().Autowiring<ILoggerContext, LoggerContext>()
                    .And().Autowiring<IConsole, DefaultConsole>()
                    .And().Autowiring<IStringService, StringService>()
                    .And().Autowiring<IPathService, PathService>()
                    .And().Autowiring<IParametersParser, ParametersParser>()
                    .And().Autowiring<IPerformanceCounterFactory, PerformanceCounterFactory>()
                    .And().Autowiring<ILogFormatter, LogFormatter>()
                    .And().Autowiring<IEventFormatter, EventFormatter>()
                    .And().Autowiring<IBuildEventManager, BuildEventManager>()
                    .And().Autowiring<IDeferredMessageWriter, DeferredMessageWriter>()
                    .And().Autowiring<IMessageWriter, MessageWriter>()
                    // IColorTheme
                    .And().Autowiring<IColorTheme, ColorTheme>()
                    .And().Tag(ColorThemeMode.Default).Autowiring<IColorTheme, DefaultColorTheme>()
                    .And().Tag(ColorThemeMode.TeamCity).Autowiring<IColorTheme, TeamCityColorTheme>()
                    // IColorTheme
                    .And().Autowiring<IStatistics, Statistics>()
                    .And().Tag(StatisticsMode.Default).Autowiring<IStatistics, DefaultStatistics>()
                    .And().Tag(StatisticsMode.TeamCity).Autowiring<IStatistics, TeamCityStatistics>()
                    // ILogWriter
                    .And().Autowiring<ILogWriter, LogWriter>()
                    .And().Tag(ColorMode.Default).Autowiring<ILogWriter, DefaultLogWriter>()
                    .And().Tag(ColorMode.TeamCity, TeamCityMode.SupportHierarchy).Contract<IHierarchicalMessageWriter>().Autowiring<ILogWriter, TeamCityHierarchicalMessageWriter>()
                    .And().Tag(ColorMode.NoColor).Autowiring<ILogWriter, NoColorLogWriter>()
                    .And().Tag(ColorMode.AnsiColor).Autowiring<ILogWriter, AnsiLogWriter>()
                    // IHierarchicalMessageWriter
                    .And().Autowiring<IHierarchicalMessageWriter, HierarchicalMessageWriter>()
                    .And().Tag(TeamCityMode.Off).Autowiring<IHierarchicalMessageWriter, DefaultHierarchicalMessageWriter>()
                    // Build event handlers
                    .And().Autowiring<IBuildEventHandler<BuildFinishedEventArgs>, BuildFinishedHandler>()
                    .And().Autowiring<IBuildEventHandler<BuildStartedEventArgs>, BuildStartedHandler>()
                    .And().Autowiring<IBuildEventHandler<CustomBuildEventArgs>, CustomEventHandler>()
                    .And().Autowiring<IBuildEventHandler<BuildErrorEventArgs>, ErrorHandler>()
                    .And().Autowiring<IBuildEventHandler<BuildMessageEventArgs>, MessageHandler>()
                    .And().Autowiring<IBuildEventHandler<ProjectFinishedEventArgs>, ProjectFinishedHandler>()
                    .And().Autowiring<IBuildEventHandler<ProjectStartedEventArgs>, ProjectStartedHandler>()
                    .And().Autowiring<IBuildEventHandler<TargetFinishedEventArgs>, TargetFinishedHandler>()
                    .And().Autowiring<IBuildEventHandler<TargetStartedEventArgs>, TargetStartedHandler>()
                    .And().Autowiring<IBuildEventHandler<TaskFinishedEventArgs>, TaskFinishedHandler>()
                    .And().Autowiring<IBuildEventHandler<TaskStartedEventArgs>, TaskStartedHandler>()
                    .And().Autowiring<IBuildEventHandler<BuildWarningEventArgs>, WarningHandler>();

            yield return container.Register()
                .Contract<ITeamCityWriter>().FactoryMethod(ctx =>
                {
                    var logWriter = ctx.ResolverContext.Container.Resolve().Tag(ColorMode.NoColor).Instance<ILogWriter>();
                    return new TeamCityServiceMessages(
                        new ServiceMessageFormatter(),
                        new FlowIdGenerator(),
                        new IServiceMessageUpdater[] { new TimestampUpdater(() => DateTime.Now) }).CreateWriter(str => logWriter.Write(str + "\n"));
                });

            yield return container.Register()
                .State<string>(0).Autowiring<IPerformanceCounter, PerformanceCounter>();

            yield return container.Register()
                .Autowiring<IColorStorage, ColorStorage>();
        }
    }
}
