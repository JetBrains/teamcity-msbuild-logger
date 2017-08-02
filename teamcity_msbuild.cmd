rem msbuild IntegrationTests\Console\Console.csproj /t:build /v:diag /noconsolelogger /logger:TeamCity.MSBuild.Logger.TeamCityMSBuildLogger,C:\Projects\TeamCity\TeamCity.MSBuild.Logger\TeamCity.MSBuild.Logger\bin\Debug\net452\TeamCity.MSBuild.Logger.dll

msbuild IntegrationTests\Console\Console.csproj /t:build /noconsolelogger /logger:TeamCity.MSBuild.Logger.TeamCityMSBuildLogger,C:\Projects\TeamCity\TeamCity.MSBuild.Logger\TeamCity.MSBuild.Logger\bin\Debug\net45\TeamCity.MSBuild.Logger.dll;teamcity
