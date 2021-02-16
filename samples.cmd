rem msbuild IntegrationTests\Console\Console.csproj /t:build /v:diag /noconsolelogger /logger:TeamCity.MSBuild.Logger.TeamCityMSBuildLogger,C:\Projects\TeamCity\TeamCity.MSBuild.Logger\TeamCity.MSBuild.Logger\bin\Debug\net452\TeamCity.MSBuild.Logger.dll

rem msbuild IntegrationTests\Console\Console.csproj /t:build /noconsolelogger /logger:TeamCity.MSBuild.Logger.TeamCityMSBuildLogger,C:\Projects\TeamCity\TeamCity.MSBuild.Logger\TeamCity.MSBuild.Logger\bin\Debug\net45\TeamCity.MSBuild.Logger.dll;teamcity /verbosity:diag
rem dotnet "restore" "C:\Projects\TeamCity\TeamCity.MSBuild.Logger\IntegrationTests\Console\Console.csproj" "--verbosity" "detailed" "/noconsolelogger" "/m:10" "/l:TeamCity.MSBuild.Logger.TeamCityMSBuildLogger,C:\Projects\TeamCity\TeamCity.MSBuild.Logger\TeamCity.MSBuild.Logger\bin\Debug\netcoreapp1.0\publish\TeamCity.MSBuild.Logger.dll"
rem dotnet "restore" "C:\Projects\TeamCity\TeamCity.MSBuild.Logger\IntegrationTests\Console\Console.csproj" "--verbosity" "detailed"

SET MSBUILDDIAGNOSTICSFILE=C:\Projects\TeamCity\TeamCity.MSBuild.Logger\aaa.txt
rem dotnet build C:\Projects\TeamCity\TeamCity.MSBuild.Logger\IntegrationTests\Console\Console.csproj --verbosity d /noconsolelogger /l:TeamCity.MSBuild.Logger.TeamCityMSBuildLogger,bin\publish\msbuild15\TeamCity.MSBuild.Logger.dll;teamcity
dotnet build C:\Projects\TeamCity\TeamCity.MSBuild.Logger\IntegrationTests\Console\Console.csproj /noconsolelogger /l:TeamCity.MSBuild.Logger.TeamCityMSBuildLogger,bin\publish\msbuild15\TeamCity.MSBuild.Logger.dll;teamcity
