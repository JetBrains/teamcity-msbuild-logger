// ReSharper disable PartialTypeWithSinglePart
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

    internal partial class Composition
    {
        // ReSharper disable once UnusedMember.Local
        private static void Setup() =>
            DI.Setup(nameof(Composition))
                .DefaultLifetime(Singleton)
                .Bind<INodeLogger>().To<NodeLogger>().Root<INodeLogger>("Logger")
                .Bind<IEnvironment>().To<Environment>()
                .Bind<IDiagnostics>().To<Diagnostics>()
                .Bind<ILoggerContext>().To<LoggerContext>()
                .Bind<IConsole>().Bind<IInitializable>().To<DefaultConsole>()
                .Bind<IStringService>().To<StringService>()
                .Bind<IPathService>().To<PathService>()
                .Bind<Parameters>().To<Parameters>()
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
                .Bind<IColorTheme>(ColorThemeMode.Default).To<DefaultColorTheme>()
                .Bind<IColorTheme>(ColorThemeMode.TeamCity).To<TeamCityColorTheme>()
                
                // IStatistics
                .Bind<IStatistics>().To<Statistics>()
                .Bind<IStatistics>(StatisticsMode.Default).To<DefaultStatistics>()
                .Bind<IStatistics>(StatisticsMode.TeamCity).To<TeamCityStatistics>()
                
                // ILogWriter
                .Bind<ILogWriter>().To<LogWriter>()
                .Bind<ILogWriter>(ColorMode.Default).To<DefaultLogWriter>()
                .Bind<ILogWriter>().Bind<IHierarchicalMessageWriter>().Tags(ColorMode.TeamCity, TeamCityMode.SupportHierarchy).To<TeamCityHierarchicalMessageWriter>()
                .Bind<ILogWriter>(ColorMode.NoColor).To<NoColorLogWriter>()
                .Bind<ILogWriter>(ColorMode.AnsiColor).To<AnsiLogWriter>()
                
                // IHierarchicalMessageWriter
                .Bind<IHierarchicalMessageWriter>().To<HierarchicalMessageWriter>()
                .Bind<IHierarchicalMessageWriter>(TeamCityMode.Off).To<DefaultHierarchicalMessageWriter>()
                
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
                .Bind<IServiceMessageUpdater>(typeof(TimestampUpdater)).To<TimestampUpdater>()
                .Bind<IServiceMessageUpdater>(typeof(BuildErrorMessageUpdater)).To<BuildErrorMessageUpdater>()
                .Bind<IServiceMessageUpdater>(typeof(BuildWarningMessageUpdater)).To<BuildWarningMessageUpdater>()
                .Bind<IServiceMessageUpdater>(typeof(BuildMessageMessageUpdater)).To<BuildMessageMessageUpdater>()
                .Bind<ITeamCityWriter>().To(
                    ctx =>
                    {
                        ctx.Inject<ITeamCityServiceMessages>(out var teamCityServiceMessages);
                        ctx.Inject<ILogWriter>(ColorMode.NoColor, out var logWriter);
                        return teamCityServiceMessages.CreateWriter(
                            str => { logWriter.Write(str + "\n"); });
                    })
                .Bind<IServiceMessageParser>().To<ServiceMessageParser>();
    }
}
