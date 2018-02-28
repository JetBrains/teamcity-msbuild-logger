## MSBuild logger for [<img src="https://cdn.worldvectorlogo.com/logos/teamcity.svg" height="20" align="center"/>](https://www.jetbrains.com/teamcity/)

[<img src="http://jb.gg/badges/official.svg" height="20"/>](https://confluence.jetbrains.com/display/ALL/JetBrains+on+GitHub) [![NuGet TeamCity.Dotnet.Integration](https://buildstats.info/nuget/TeamCity.Dotnet.Integration?includePreReleases=false)](https://www.nuget.org/packages/TeamCity.Dotnet.Integration) [![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0) [<img src="http://teamcity.jetbrains.com/app/rest/builds/buildType:(id:TeamCityPluginsByJetBrains_TeamCityDotnetIntegration_TeamCityMSBuildLogger)/statusIcon.svg"/>](http://teamcity.jetbrains.com/viewType.html?buildTypeId=TeamCityPluginsByJetBrains_TeamCityDotnetIntegration_TeamCityMSBuildLogger&guest=1)

Provides the TeamCity integration with [__*.NET CLI*__](https://www.microsoft.com/net/core)/[__*MSBuild*__](https://msdn.microsoft.com/en-US/library/0k6kkbsd.aspx) tools.

<img src="https://github.com/JetBrains/TeamCity.MSBuild.Logger/blob/master/Docs/TeamCityBuildLog.png"/>

## Supported platforms:

* [.NET CLI](https://www.microsoft.com/net/core)

```
dotnet build my.csproj /noconsolelogger /l:TeamCity.MSBuild.Logger.TeamCityMSBuildLogger,path_to_logger\TeamCity.MSBuild.Logger.dll;teamcity
```

* [MSBuild 12+](https://msdn.microsoft.com/en-US/library/0k6kkbsd.aspx)

```
msbuild.exe my.csproj /t:build /noconsolelogger /l:TeamCity.MSBuild.Logger.TeamCityMSBuildLogger,path_to_logger\TeamCity.MSBuild.Logger.dll;teamcity
```

## Download

  * [Stable version](http://teamcity.jetbrains.com/guestAuth/app/rest/builds/buildType:TeamCityPluginsByJetBrains_TeamCityDotnetIntegration_TeamCityMSBuildLogger,pinned:true,status:SUCCESS,tags:release/artifacts/content/TeamCity.MSBuild.Logger.zip )
  * [Nightly build](http://teamcity.jetbrains.com/guestAuth/app/rest/builds/buildType:TeamCityPluginsByJetBrains_TeamCityDotnetIntegration_TeamCityMSBuildLogger,status:SUCCESS/artifacts/content/TeamCity.MSBuild.Logger.zip)

## TeamCity integration

TeamCity Integration is working from-the-box while you are using [TeamCity dotnet plugin](https://github.com/JetBrains/teamcity-dotnet-plugin). Also it is possible to use TeamCity logger manually, see more details in the [Wiki](https://github.com/JetBrains/TeamCity.MSBuild.Logger/wiki/How-to-use).

## License

It is under the [Apache License](LICENSE).
