namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;
    using DevTeam.IoC.Contracts;
    using Microsoft.Build.Framework;
    using EventHandlers;
    using JetBrains.TeamCity.ServiceMessages.Write;

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
                .Lifetime(Wellknown.Lifetime.Singleton).With()
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
                    .And().Autowiring<IServiceMessageFormatter, ServiceMessageFormatter>()
                    // IColorTheme
                    .And().Autowiring<IColorTheme, ColorTheme>()
                    .And().Tag(ColorThemeMode.Default).Autowiring<IColorTheme, DefaultColorTheme>()
                    .And().Tag(ColorThemeMode.TeamCity).Autowiring<IColorTheme, TeamCityColorTheme>()
                    // ILogWriter
                    .And().Autowiring<ILogWriter, LogWriter>()
                    .And().Tag(ColorMode.Default).Autowiring<ILogWriter, DefaultLogWriter>()
                    .And().Tag(ColorMode.Ansi).Autowiring<ILogWriter, AnsiLogWriter>()
                    .And().Tag(ColorMode.NoColor).Autowiring<ILogWriter, NoColorLogWriter>()
                    // IHierarchicalMessageWriter
                    .And().Autowiring<IHierarchicalMessageWriter, HierarchicalMessageWriter>()
                    .And().Tag(TeamCityMode.Off).Autowiring<IHierarchicalMessageWriter, DefaultHierarchicalMessageWriter>()
                    .And().Tag(TeamCityMode.SupportHierarchy).Autowiring<IHierarchicalMessageWriter, TeamCityHierarchicalMessageWriter>()
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
                .State<string>(0).With()
                    .Autowiring<ITeamCityBlock, TeamCityBlock>()
                    .And().Autowiring<IPerformanceCounter, PerformanceCounter>();
        }
    }
}
