namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Build.Framework;
    using EventHandlers;
    using IoC;
    using JetBrains.TeamCity.ServiceMessages.Read;
    using JetBrains.TeamCity.ServiceMessages.Write;
    using JetBrains.TeamCity.ServiceMessages.Write.Special;
    using JetBrains.TeamCity.ServiceMessages.Write.Special.Impl.Updater;
    using static IoC.Lifetime;

    internal class IoCConfiguration : IConfiguration
    {
        public IEnumerable<IToken> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            yield return container
                .Bind<INodeLogger>().As(Singleton).To<NodeLogger>()
                .Bind<IEnvironment>().As(Singleton).To<Environment>()
                .Bind<IDiagnostics>().As(Singleton).To<Diagnostics>()
                .Bind<ILoggerContext>().As(Singleton).To<LoggerContext>()
                .Bind<IConsole>().As(Singleton).To<DefaultConsole>()
                .Bind<IStringService>().As(Singleton).To<StringService>()
                .Bind<IPathService>().As(Singleton).To<PathService>()
                .Bind<IParametersParser>().As(Singleton).To<ParametersParser>()
                .Bind<IPerformanceCounterFactory>().As(Singleton).To<PerformanceCounterFactory>()
                .Bind<ILogFormatter>().As(Singleton).To<LogFormatter>()
                .Bind<IEventFormatter>().As(Singleton).To<EventFormatter>()
                .Bind<IBuildEventManager>().As(Singleton).To<BuildEventManager>()
                .Bind<IDeferredMessageWriter>().As(Singleton).To<DeferredMessageWriter>()
                .Bind<IMessageWriter>().As(Singleton).To<MessageWriter>()
                // Colors
                .Bind<IColorTheme>().As(Singleton).To<ColorTheme>(
                ctx => new ColorTheme(ctx.Container.Inject<ILoggerContext>(),
                    ctx.Container.Inject<IColorTheme>(ColorThemeMode.Default),
                    ctx.Container.Inject<IColorTheme>(ColorThemeMode.TeamCity)))
                .Bind<IColorTheme>().Tag(ColorThemeMode.Default).As(Singleton).To<DefaultColorTheme>()
                .Bind<IColorTheme>().Tag(ColorThemeMode.TeamCity).As(Singleton).To<TeamCityColorTheme>(
                    ctx => new TeamCityColorTheme(ctx.Container.Inject<IColorTheme>(ColorThemeMode.Default)))
                // IStatistics
                .Bind<IStatistics>().As(Singleton).To<Statistics>(
                ctx => new Statistics(ctx.Container.Inject<ILoggerContext>(),
                    ctx.Container.Inject<IStatistics>(StatisticsMode.Default),
                    ctx.Container.Inject<IStatistics>(StatisticsMode.TeamCity)))
                .Bind<IStatistics>().Tag(StatisticsMode.Default).As(Singleton).To<DefaultStatistics>()
                .Bind<IStatistics>().Tag(StatisticsMode.TeamCity).As(Singleton).To<TeamCityStatistics>()
                // ILogWriter
                .Bind<ILogWriter>().As(Singleton).To<LogWriter>(
                ctx => new LogWriter(ctx.Container.Inject<ILoggerContext>(),
                    ctx.Container.Inject<ILogWriter>(ColorMode.Default),
                    ctx.Container.Inject<ILogWriter>(ColorMode.TeamCity),
                    ctx.Container.Inject<ILogWriter>(ColorMode.NoColor),
                    ctx.Container.Inject<ILogWriter>(ColorMode.AnsiColor)))
                .Bind<ILogWriter>().Tag(ColorMode.Default).As(Singleton).To<DefaultLogWriter>()
                .Bind<TeamCityHierarchicalMessageWriter, ILogWriter, IHierarchicalMessageWriter>().As(Singleton).Tag(ColorMode.TeamCity).Tag(TeamCityMode.SupportHierarchy).To()
                .Bind<ILogWriter>().Tag(ColorMode.NoColor).As(Singleton).To<NoColorLogWriter>()
                .Bind<ILogWriter>().Tag(ColorMode.AnsiColor).As(Singleton).To<AnsiLogWriter>()
                // IHierarchicalMessageWriter
                .Bind<IHierarchicalMessageWriter>().As(Singleton).To<HierarchicalMessageWriter>(
                ctx => new HierarchicalMessageWriter(
                    ctx.Container.Inject<ILoggerContext>(),
                    ctx.Container.Inject<IHierarchicalMessageWriter>(TeamCityMode.Off),
                    ctx.Container.Inject<IHierarchicalMessageWriter>(TeamCityMode.SupportHierarchy)))
                .Bind<IHierarchicalMessageWriter>().Tag(TeamCityMode.Off).As(Singleton).To<DefaultHierarchicalMessageWriter>()
                // Build event handlers
                .Bind<IBuildEventHandler<BuildFinishedEventArgs>>().As(Singleton).To<BuildFinishedHandler>()
                .Bind<IBuildEventHandler<BuildStartedEventArgs>>().As(Singleton).To<BuildStartedHandler>()
                .Bind<IBuildEventHandler<CustomBuildEventArgs>>().As(Singleton).To<CustomEventHandler>()
                .Bind<IBuildEventHandler<BuildErrorEventArgs>>().As(Singleton).To<ErrorHandler>()
                .Bind<IBuildEventHandler<BuildMessageEventArgs>>().As(Singleton).To<MessageHandler>()
                .Bind<IBuildEventHandler<ProjectFinishedEventArgs>>().As(Singleton).To<ProjectFinishedHandler>()
                .Bind<IBuildEventHandler<ProjectStartedEventArgs>>().As(Singleton).To<ProjectStartedHandler>()
                .Bind<IBuildEventHandler<TargetFinishedEventArgs>>().As(Singleton).To<TargetFinishedHandler>()
                .Bind<IBuildEventHandler<TargetStartedEventArgs>>().As(Singleton).To<TargetStartedHandler>()
                .Bind<IBuildEventHandler<TaskFinishedEventArgs>>().As(Singleton).To<TaskFinishedHandler>()
                .Bind<IBuildEventHandler<TaskStartedEventArgs>>().As(Singleton).To<TaskStartedHandler>()
                .Bind<IBuildEventHandler<BuildWarningEventArgs>>().As(Singleton).To<WarningHandler>()
                // TeamCity messages
                .Bind<ITeamCityWriter>().To(
                ctx => CreateWriter(ctx.Container.Inject<ILogWriter>(ColorMode.NoColor)))
                .Bind<IServiceMessageParser>().As(Singleton).To<ServiceMessageParser>()
                .Bind<IPerformanceCounter>().To<PerformanceCounter>(
                ctx => new PerformanceCounter((string)ctx.Args[0], ctx.Container.Inject<ILogWriter>(), ctx.Container.Inject<IPerformanceCounterFactory>(), ctx.Container.Inject<IMessageWriter>()))
                .Bind<IColorStorage>().To<ColorStorage>();
        }

        private static ITeamCityWriter CreateWriter(ILogWriter writer)
        {
            return new TeamCityServiceMessages(
                new ServiceMessageFormatter(),
                new FlowIdGenerator(),
                new IServiceMessageUpdater[] { new TimestampUpdater(() => DateTime.Now) }).CreateWriter(str => writer.Write(str + "\n"));
        }
    }
}
