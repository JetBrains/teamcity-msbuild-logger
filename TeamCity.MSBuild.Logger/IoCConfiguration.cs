namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Build.Framework;
    using EventHandlers;
    using IoC;
    using JetBrains.TeamCity.ServiceMessages.Write;
    using JetBrains.TeamCity.ServiceMessages.Write.Special;
    using JetBrains.TeamCity.ServiceMessages.Write.Special.Impl.Updater;

    internal class IoCConfiguration : IConfiguration
    {
        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            yield return container.Bind<INodeLogger>().Lifetime(Lifetime.Singletone).To<NodeLogger>();
            yield return container.Bind<ILoggerContext>().Lifetime(Lifetime.Singletone).To<LoggerContext>();
            yield return container.Bind<IConsole>().Lifetime(Lifetime.Singletone).To<DefaultConsole>();
            yield return container.Bind<IStringService>().Lifetime(Lifetime.Singletone).To<StringService>();
            yield return container.Bind<IPathService>().Lifetime(Lifetime.Singletone).To<PathService>();
            yield return container.Bind<IParametersParser>().Lifetime(Lifetime.Singletone).To<ParametersParser>();
            yield return container.Bind<IPerformanceCounterFactory>().Lifetime(Lifetime.Singletone).To<PerformanceCounterFactory>();
            yield return container.Bind<ILogFormatter>().Lifetime(Lifetime.Singletone).To<LogFormatter>();
            yield return container.Bind<IEventFormatter>().Lifetime(Lifetime.Singletone).To<EventFormatter>();
            yield return container.Bind<IBuildEventManager>().Lifetime(Lifetime.Singletone).To<BuildEventManager>();
            yield return container.Bind<IDeferredMessageWriter>().Lifetime(Lifetime.Singletone).To<DeferredMessageWriter>();
            yield return container.Bind<IMessageWriter>().Lifetime(Lifetime.Singletone).To<MessageWriter>();

            // IColorTheme
            yield return container.Bind<IColorTheme>().Lifetime(Lifetime.Singletone).To<ColorTheme>(Has.Ref("defaultColorTheme", ColorThemeMode.Default), Has.Ref("teamCityColorTheme", ColorThemeMode.TeamCity));
            yield return container.Bind<IColorTheme>().Tag(ColorThemeMode.Default).Lifetime(Lifetime.Singletone).To<DefaultColorTheme>();
            yield return container.Bind<IColorTheme>().Tag(ColorThemeMode.TeamCity).Lifetime(Lifetime.Singletone).To<TeamCityColorTheme>(Has.Ref("defaultColorTheme", ColorThemeMode.Default));

            // IColorTheme
            yield return container.Bind<IStatistics>().Lifetime(Lifetime.Singletone).To<Statistics>(Has.Ref("defaultStatistics", StatisticsMode.Default), Has.Ref("teamcityStatistics", StatisticsMode.TeamCity));
            yield return container.Bind<IStatistics>().Tag(StatisticsMode.Default).Lifetime(Lifetime.Singletone).To<DefaultStatistics>();
            yield return container.Bind<IStatistics>().Tag(StatisticsMode.TeamCity).Lifetime(Lifetime.Singletone).To<TeamCityStatistics>();

            // ILogWriter
            yield return container.Bind<ILogWriter>().Lifetime(Lifetime.Singletone).To<LogWriter>(Has.Ref("defaultLogWriter", ColorMode.Default), Has.Ref("ansiLogWriter", ColorMode.TeamCity), Has.Ref("noColorLogWriter", ColorMode.NoColor), Has.Ref("ansiColorLogWriter", ColorMode.AnsiColor));
            yield return container.Bind<ILogWriter>().Tag(ColorMode.Default).Lifetime(Lifetime.Singletone).To<DefaultLogWriter>();
            yield return container.Bind<TeamCityHierarchicalMessageWriter, ILogWriter, IHierarchicalMessageWriter>().Tag(ColorMode.TeamCity).Tag(TeamCityMode.SupportHierarchy).To();
            yield return container.Bind<ILogWriter>().Tag(ColorMode.NoColor).Lifetime(Lifetime.Singletone).To<NoColorLogWriter>();
            yield return container.Bind<ILogWriter>().Tag(ColorMode.AnsiColor).Lifetime(Lifetime.Singletone).To<AnsiLogWriter>();

            // IHierarchicalMessageWriter
            yield return container.Bind<IHierarchicalMessageWriter>().Lifetime(Lifetime.Singletone).To<HierarchicalMessageWriter>(Has.Ref("defaultHierarchicalMessageWriter", TeamCityMode.Off), Has.Ref("teamcityHierarchicalMessageWriter", TeamCityMode.SupportHierarchy));
            yield return container.Bind<IHierarchicalMessageWriter>().Tag(TeamCityMode.Off).Lifetime(Lifetime.Singletone).To<DefaultHierarchicalMessageWriter>();

            // Build event handlers
            yield return container.Bind<IBuildEventHandler<BuildFinishedEventArgs>>().Lifetime(Lifetime.Singletone).To<BuildFinishedHandler>();
            yield return container.Bind<IBuildEventHandler<BuildStartedEventArgs>>().Lifetime(Lifetime.Singletone).To<BuildStartedHandler>();
            yield return container.Bind<IBuildEventHandler<CustomBuildEventArgs>>().Lifetime(Lifetime.Singletone).To<CustomEventHandler>();
            yield return container.Bind<IBuildEventHandler<BuildErrorEventArgs>>().Lifetime(Lifetime.Singletone).To<ErrorHandler>();
            yield return container.Bind<IBuildEventHandler<BuildMessageEventArgs>>().Lifetime(Lifetime.Singletone).To<MessageHandler>();
            yield return container.Bind<IBuildEventHandler<ProjectFinishedEventArgs>>().Lifetime(Lifetime.Singletone).To<ProjectFinishedHandler>();
            yield return container.Bind<IBuildEventHandler<ProjectStartedEventArgs>>().Lifetime(Lifetime.Singletone).To<ProjectStartedHandler>();
            yield return container.Bind<IBuildEventHandler<TargetFinishedEventArgs>>().Lifetime(Lifetime.Singletone).To<TargetFinishedHandler>();
            yield return container.Bind<IBuildEventHandler<TargetStartedEventArgs>>().Lifetime(Lifetime.Singletone).To<TargetStartedHandler>();
            yield return container.Bind<IBuildEventHandler<TaskFinishedEventArgs>>().Lifetime(Lifetime.Singletone).To<TaskFinishedHandler>();
            yield return container.Bind<IBuildEventHandler<TaskStartedEventArgs>>().Lifetime(Lifetime.Singletone).To<TaskStartedHandler>();
            yield return container.Bind<IBuildEventHandler<BuildWarningEventArgs>>().Lifetime(Lifetime.Singletone).To<WarningHandler>();

            yield return container.Bind<ITeamCityWriter>().To(ctx =>
                {
                    var logWriter = ctx.ResolvingContainer.Tag(ColorMode.NoColor).Get<ILogWriter>();
                    return new TeamCityServiceMessages(
                        new ServiceMessageFormatter(),
                        new FlowIdGenerator(),
                        new IServiceMessageUpdater[] { new TimestampUpdater(() => DateTime.Now) }).CreateWriter(str => logWriter.Write(str + "\n"));
                });

            yield return container.Bind<IPerformanceCounter>().To<PerformanceCounter>(Has.Arg("scopeName", 0));
            yield return container.Bind<IColorStorage>().To<ColorStorage>();
        }
    }
}
