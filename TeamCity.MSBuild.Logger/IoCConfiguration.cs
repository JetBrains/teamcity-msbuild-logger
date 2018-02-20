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

    internal class IoCConfiguration : IConfiguration
    {
        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            yield return container.Bind<INodeLogger>().As(Lifetime.Singleton).To<NodeLogger>();
            yield return container.Bind<ILoggerContext>().As(Lifetime.Singleton).To<LoggerContext>();
            yield return container.Bind<IConsole>().As(Lifetime.Singleton).To<DefaultConsole>();
            yield return container.Bind<IStringService>().As(Lifetime.Singleton).To<StringService>();
            yield return container.Bind<IPathService>().As(Lifetime.Singleton).To<PathService>();
            yield return container.Bind<IParametersParser>().As(Lifetime.Singleton).To<ParametersParser>();
            yield return container.Bind<IPerformanceCounterFactory>().As(Lifetime.Singleton).To<PerformanceCounterFactory>();
            yield return container.Bind<ILogFormatter>().As(Lifetime.Singleton).To<LogFormatter>();
            yield return container.Bind<IEventFormatter>().As(Lifetime.Singleton).To<EventFormatter>();
            yield return container.Bind<IBuildEventManager>().As(Lifetime.Singleton).To<BuildEventManager>();
            yield return container.Bind<IDeferredMessageWriter>().As(Lifetime.Singleton).To<DeferredMessageWriter>();
            yield return container.Bind<IMessageWriter>().As(Lifetime.Singleton).To<MessageWriter>();

            // IColorTheme
            yield return container.Bind<IColorTheme>().As(Lifetime.Singleton).To<ColorTheme>(
                ctx => new ColorTheme(ctx.Container.Inject<ILoggerContext>(),
                    ctx.Container.Inject<IColorTheme>(ColorThemeMode.Default),
                    ctx.Container.Inject<IColorTheme>(ColorThemeMode.TeamCity)));
            yield return container.Bind<IColorTheme>().Tag(ColorThemeMode.Default).As(Lifetime.Singleton).To<DefaultColorTheme>();
            yield return container.Bind<IColorTheme>().Tag(ColorThemeMode.TeamCity).As(Lifetime.Singleton).To<TeamCityColorTheme>(
                    ctx => new TeamCityColorTheme(ctx.Container.Inject<IColorTheme>(ColorThemeMode.Default)));

            // IColorTheme
            yield return container.Bind<IStatistics>().As(Lifetime.Singleton).To<Statistics>(
                ctx => new Statistics(ctx.Container.Inject<ILoggerContext>(),
                    ctx.Container.Inject<IStatistics>(StatisticsMode.Default),
                    ctx.Container.Inject<IStatistics>(StatisticsMode.TeamCity)));
            yield return container.Bind<IStatistics>().Tag(StatisticsMode.Default).As(Lifetime.Singleton).To<DefaultStatistics>();
            yield return container.Bind<IStatistics>().Tag(StatisticsMode.TeamCity).As(Lifetime.Singleton).To<TeamCityStatistics>();

            // ILogWriter
            yield return container.Bind<ILogWriter>().As(Lifetime.Singleton).To<LogWriter>(
                ctx => new LogWriter(ctx.Container.Inject<ILoggerContext>(),
                    ctx.Container.Inject<ILogWriter>(ColorMode.Default),
                    ctx.Container.Inject<ILogWriter>(ColorMode.TeamCity),
                    ctx.Container.Inject<ILogWriter>(ColorMode.NoColor),
                    ctx.Container.Inject<ILogWriter>(ColorMode.AnsiColor)));
            yield return container.Bind<ILogWriter>().Tag(ColorMode.Default).As(Lifetime.Singleton).To<DefaultLogWriter>();
            yield return container.Bind<TeamCityHierarchicalMessageWriter, ILogWriter, IHierarchicalMessageWriter>().Tag(ColorMode.TeamCity).Tag(TeamCityMode.SupportHierarchy).To();
            yield return container.Bind<ILogWriter>().Tag(ColorMode.NoColor).As(Lifetime.Singleton).To<NoColorLogWriter>();
            yield return container.Bind<ILogWriter>().Tag(ColorMode.AnsiColor).As(Lifetime.Singleton).To<AnsiLogWriter>();

            // IHierarchicalMessageWriter
            yield return container.Bind<IHierarchicalMessageWriter>().As(Lifetime.Singleton).To<HierarchicalMessageWriter>(
                ctx => new HierarchicalMessageWriter(
                    ctx.Container.Inject<ILoggerContext>(),
                    ctx.Container.Inject<IHierarchicalMessageWriter>(TeamCityMode.Off),
                    ctx.Container.Inject<IHierarchicalMessageWriter>(TeamCityMode.SupportHierarchy)));
            yield return container.Bind<IHierarchicalMessageWriter>().Tag(TeamCityMode.Off).As(Lifetime.Singleton).To<DefaultHierarchicalMessageWriter>();

            // Build event handlers
            yield return container.Bind<IBuildEventHandler<BuildFinishedEventArgs>>().As(Lifetime.Singleton).To<BuildFinishedHandler>();
            yield return container.Bind<IBuildEventHandler<BuildStartedEventArgs>>().As(Lifetime.Singleton).To<BuildStartedHandler>();
            yield return container.Bind<IBuildEventHandler<CustomBuildEventArgs>>().As(Lifetime.Singleton).To<CustomEventHandler>();
            yield return container.Bind<IBuildEventHandler<BuildErrorEventArgs>>().As(Lifetime.Singleton).To<ErrorHandler>();
            yield return container.Bind<IBuildEventHandler<BuildMessageEventArgs>>().As(Lifetime.Singleton).To<MessageHandler>();
            yield return container.Bind<IBuildEventHandler<ProjectFinishedEventArgs>>().As(Lifetime.Singleton).To<ProjectFinishedHandler>();
            yield return container.Bind<IBuildEventHandler<ProjectStartedEventArgs>>().As(Lifetime.Singleton).To<ProjectStartedHandler>();
            yield return container.Bind<IBuildEventHandler<TargetFinishedEventArgs>>().As(Lifetime.Singleton).To<TargetFinishedHandler>();
            yield return container.Bind<IBuildEventHandler<TargetStartedEventArgs>>().As(Lifetime.Singleton).To<TargetStartedHandler>();
            yield return container.Bind<IBuildEventHandler<TaskFinishedEventArgs>>().As(Lifetime.Singleton).To<TaskFinishedHandler>();
            yield return container.Bind<IBuildEventHandler<TaskStartedEventArgs>>().As(Lifetime.Singleton).To<TaskStartedHandler>();
            yield return container.Bind<IBuildEventHandler<BuildWarningEventArgs>>().As(Lifetime.Singleton).To<WarningHandler>();

            yield return container.Bind<ITeamCityWriter>().To(
                ctx => CreateWriter(ctx.Container.Inject<ILogWriter>(ColorMode.NoColor)));
            
            yield return container.Bind<IServiceMessageParser>().To<ServiceMessageParser>();
            yield return container.Bind<IPerformanceCounter>().To<PerformanceCounter>(
                ctx => new PerformanceCounter((string)ctx.Args[0], ctx.Container.Inject<ILogWriter>(), ctx.Container.Inject<IPerformanceCounterFactory>(), ctx.Container.Inject<IMessageWriter>()));
            yield return container.Bind<IColorStorage>().To<ColorStorage>();
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
