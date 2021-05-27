namespace TeamCity.MSBuild.Logger
{
    using System;
    using Microsoft.Build.Framework;
    using EventHandlers;
    using JetBrains.TeamCity.ServiceMessages.Read;
    using JetBrains.TeamCity.ServiceMessages.Write;
    using JetBrains.TeamCity.ServiceMessages.Write.Special;
    using JetBrains.TeamCity.ServiceMessages.Write.Special.Impl.Updater;
    using Pure.DI;
    using static Pure.DI.Lifetime;

    internal static partial class Composer
    {
        static Composer()
        {
            DI.Setup()
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
                .Bind<IPerformanceCounter>().To<PerformanceCounter>()
                .Bind<IColorStorage>().To<ColorStorage>()

                // Colors
                .Bind<IColorTheme>().As(Singleton).To<ColorTheme>()
                .Bind<IColorTheme>().Tag(ColorThemeMode.Default).As(Singleton).To<DefaultColorTheme>()
                .Bind<IColorTheme>().Tag(ColorThemeMode.TeamCity).As(Singleton).To<TeamCityColorTheme>()
                
                // IStatistics
                .Bind<IStatistics>().As(Singleton).To<Statistics>()
                .Bind<IStatistics>().Tag(StatisticsMode.Default).As(Singleton).To<DefaultStatistics>()
                .Bind<IStatistics>().Tag(StatisticsMode.TeamCity).As(Singleton).To<TeamCityStatistics>()
                
                // ILogWriter
                .Bind<ILogWriter>().As(Singleton).To<LogWriter>()
                .Bind<ILogWriter>().Tag(ColorMode.Default).As(Singleton).To<DefaultLogWriter>()
                .Bind<ILogWriter>().Bind<IHierarchicalMessageWriter>().As(Singleton).Tag(ColorMode.TeamCity).Tag(TeamCityMode.SupportHierarchy).To<TeamCityHierarchicalMessageWriter>()
                .Bind<ILogWriter>().Tag(ColorMode.NoColor).As(Singleton).To<NoColorLogWriter>()
                .Bind<ILogWriter>().Tag(ColorMode.AnsiColor).As(Singleton).To<AnsiLogWriter>()
                
                // IHierarchicalMessageWriter
                .Bind<IHierarchicalMessageWriter>().As(Singleton).To<HierarchicalMessageWriter>()
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
                
                // Service messages
                .Bind<ITeamCityServiceMessages>().As(Singleton).To<TeamCityServiceMessages>()
                .Bind<IServiceMessageFormatter>().As(Singleton).To<ServiceMessageFormatter>()
                .Bind<IFlowIdGenerator>().As(Singleton).To<FlowIdGenerator>()
                .Bind<DateTime>().As(Singleton).To(_ => DateTime.Now)
                .Bind<IServiceMessageUpdater>().As(Singleton).To<TimestampUpdater>()
                .Bind<ITeamCityWriter>().As(Singleton).To(
                    ctx => ctx.Resolve<ITeamCityServiceMessages>().CreateWriter(
                        str => ctx.Resolve<ILogWriter>(ColorMode.NoColor).Write(str + "\n")))
                .Bind<IServiceMessageParser>().As(Singleton).To<ServiceMessageParser>();
        }
    }
}
