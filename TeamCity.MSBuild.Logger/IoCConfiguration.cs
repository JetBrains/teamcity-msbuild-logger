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
            yield return container.Feature(Wellknown.Feature.Default);
        }

        public IEnumerable<IDisposable> Apply([NotNull] IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<INodeLogger, NodeLogger>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IColorTheme, ColorTheme>();
            yield return container.Register().Tag(ColorThemeMode.Default).Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IColorTheme, DefaultColorTheme>();
            yield return container.Register().Tag(ColorThemeMode.TeamCity).Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IColorTheme, TeamCityColorTheme>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IConsole, DefaultConsole>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IStringService, StringService>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IPathService, PathService>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IEnvironmentService, EnvironmentService>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<ILogWriter, LogWriter>();
            yield return container.Register().Tag(ColorMode.Default).Lifetime(Wellknown.Lifetime.Singleton).Autowiring<ILogWriter, DefaultLogWriter>();
            yield return container.Register().Tag(ColorMode.Ansi).Lifetime(Wellknown.Lifetime.Singleton).Autowiring<ILogWriter, AnsiLogWriter>();
            yield return container.Register().Tag(ColorMode.NoColor).Lifetime(Wellknown.Lifetime.Singleton).Autowiring<ILogWriter, NoColorLogWriter>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IParametersFactory, ParametersFactory>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IParametersParser, ParametersParser>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IPerformanceCounterFactory, PerformanceCounterFactory>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<ILogFormatter, LogFormatter>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IEventFormatter, EventFormatter>();
            yield return container.Register().State<string>(0).Autowiring<IPerformanceCounter, PerformanceCounter>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IBuildEventManager, BuildEventManager>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IDeferredMessageWriter, DeferredMessageWriter>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IMessageWriter, MessageWriter>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<ILoggerContext, LoggerContext>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IServiceMessageFormatter, ServiceMessageFormatter>();
            yield return container.Register().State<string>(0).Autowiring<ITeamCityBlock, TeamCityBlock>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IHierarchicalMessageWriter, HierarchicalMessageWriter>();
            yield return container.Register().Tag(TeamCityMode.Off).Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IHierarchicalMessageWriter, DefaultHierarchicalMessageWriter>();
            yield return container.Register().Tag(TeamCityMode.SupportHierarchy).Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IHierarchicalMessageWriter, TeamCityHierarchicalMessageWriter>();

            // Build event handlers
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IBuildEventHandler<BuildFinishedEventArgs>, BuildFinishedHandler>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IBuildEventHandler<BuildStartedEventArgs>, BuildStartedHandler>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IBuildEventHandler<CustomBuildEventArgs>, CustomEventHandler>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IBuildEventHandler<BuildErrorEventArgs>, ErrorHandler>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IBuildEventHandler<BuildMessageEventArgs>, MessageHandler>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IBuildEventHandler<ProjectFinishedEventArgs>, ProjectFinishedHandler>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IBuildEventHandler<ProjectStartedEventArgs>, ProjectStartedHandler>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IBuildEventHandler<TargetFinishedEventArgs>, TargetFinishedHandler>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IBuildEventHandler<TargetStartedEventArgs>, TargetStartedHandler>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IBuildEventHandler<TaskFinishedEventArgs>, TaskFinishedHandler>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IBuildEventHandler<TaskStartedEventArgs>, TaskStartedHandler>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IBuildEventHandler<BuildWarningEventArgs>, WarningHandler>();
        }
    }
}
