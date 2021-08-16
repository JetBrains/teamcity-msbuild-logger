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
                .Default(Singleton)
                .Bind<INodeLogger>().To<NodeLogger>()
                .Bind<IEnvironment>().To<Environment>()
                .Bind<IDiagnostics>().To<Diagnostics>()
                .Bind<ILoggerContext>().To<LoggerContext>()
                .Bind<IConsole>().Bind<IInitializable>().To<DefaultConsole>()
                .Bind<IStringService>().To<StringService>()
                .Bind<IPathService>().To<PathService>()
                .Bind<IParametersParser>().To<ParametersParser>()
                .Bind<IPerformanceCounterFactory>().To<PerformanceCounterFactory>()
                .Bind<ILogFormatter>().To<LogFormatter>()
                .Bind<IEventFormatter>().To<EventFormatter>()
                .Bind<IBuildEventManager>().To<BuildEventManager>()
                .Bind<IDeferredMessageWriter>().To<DeferredMessageWriter>()
                .Bind<IMessageWriter>().To<MessageWriter>()
                .Bind<IPerformanceCounter>().As(Transient).To<PerformanceCounter>()
                .Bind<IColorStorage>().As(Transient).To<ColorStorage>()
                .Bind<IEventContext>().Bind<IEventRegistry>().To<EventContext>()

                // Colors
                .Bind<IColorTheme>().To<ColorTheme>()
                .Bind<IColorTheme>().Tag(ColorThemeMode.Default).To<DefaultColorTheme>()
                .Bind<IColorTheme>().Tag(ColorThemeMode.TeamCity).To<TeamCityColorTheme>()
                
                // IStatistics
                .Bind<IStatistics>().To<Statistics>()
                .Bind<IStatistics>().Tag(StatisticsMode.Default).To<DefaultStatistics>()
                .Bind<IStatistics>().Tag(StatisticsMode.TeamCity).To<TeamCityStatistics>()
                
                // ILogWriter
                .Bind<ILogWriter>().To<LogWriter>()
                .Bind<ILogWriter>().Tag(ColorMode.Default).To<DefaultLogWriter>()
                .Bind<ILogWriter>().Bind<IHierarchicalMessageWriter>().Tag(ColorMode.TeamCity).Tag(TeamCityMode.SupportHierarchy).To<TeamCityHierarchicalMessageWriter>()
                .Bind<ILogWriter>().Tag(ColorMode.NoColor).To<NoColorLogWriter>()
                .Bind<ILogWriter>().Tag(ColorMode.AnsiColor).To<AnsiLogWriter>()
                
                // IHierarchicalMessageWriter
                .Bind<IHierarchicalMessageWriter>().To<HierarchicalMessageWriter>()
                .Bind<IHierarchicalMessageWriter>().Tag(TeamCityMode.Off).To<DefaultHierarchicalMessageWriter>()
                
                // Build event handlers
                .Bind<IBuildEventHandler<BuildFinishedEventArgs>>().To<BuildFinishedHandler>()
                .Bind<IBuildEventHandler<BuildStartedEventArgs>>().To<BuildStartedHandler>()
                .Bind<IBuildEventHandler<CustomBuildEventArgs>>().To<CustomEventHandler>()
                .Bind<IBuildEventHandler<BuildErrorEventArgs>>().To<ErrorHandler>()
                .Bind<IBuildEventHandler<BuildMessageEventArgs>>().To<MessageHandler>()
                .Bind<IBuildEventHandler<ProjectFinishedEventArgs>>().To<ProjectFinishedHandler>()
                .Bind<IBuildEventHandler<ProjectStartedEventArgs>>().To<ProjectStartedHandler>()
                .Bind<IBuildEventHandler<TargetFinishedEventArgs>>().To<TargetFinishedHandler>()
                .Bind<IBuildEventHandler<TargetStartedEventArgs>>().To<TargetStartedHandler>()
                .Bind<IBuildEventHandler<TaskFinishedEventArgs>>().To<TaskFinishedHandler>()
                .Bind<IBuildEventHandler<TaskStartedEventArgs>>().To<TaskStartedHandler>()
                .Bind<IBuildEventHandler<BuildWarningEventArgs>>().To<WarningHandler>()
                
                // Service messages
                .Bind<ITeamCityServiceMessages>().To<TeamCityServiceMessages>()
                .Bind<IServiceMessageFormatter>().To<ServiceMessageFormatter>()
                .Bind<IFlowIdGenerator>().To<FlowIdGenerator>()
                .Bind<DateTime>().As(Transient).To(_ => DateTime.Now)
                .Bind<IServiceMessageUpdater>().To<TimestampUpdater>()
                .Bind<ITeamCityWriter>().To(
                    ctx => ctx.Resolve<ITeamCityServiceMessages>().CreateWriter(
                        str => ctx.Resolve<ILogWriter>(ColorMode.NoColor).Write(str + "\n")))
                .Bind<IServiceMessageParser>().To<ServiceMessageParser>();
        }
    }
}
