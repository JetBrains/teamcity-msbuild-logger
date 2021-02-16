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

## Contribution

We would be a grateful contribution.

- clone this repo
- open the solution _TeamCity.MSBuild.Logger.sln_ in IDE
- configure debugging for _TeamCity.MSBuild.Logger_
  - specify the executable to _dotnet.exe_ for instance like _C:\Program Files\dotnet\dotnet.exe_
  - specify command-line arguments for your case, for instance _build <my_project_path>\MyProject.csproj --verbosity normal /noconsolelogger /l:TeamCity.MSBuild.Logger.TeamCityMSBuildLogger,<this_repo_path>\TeamCity.MSBuild.Logger\bin\Debug\net452\TeamCity.MSBuild.Logger.dll;teamcity;DEBUG_ - the last option _DEBUG_ is needed to debug this logger.
- set breakpoints
- run debugging for _TeamCity.MSBuild.Logger_
- attach to the process _dotnet.exe_ with a number from stdOut
- _test.cmd_ to run integration tests

## License

It is under the [Apache License](LICENSE).
