rem nuget restore
dotnet restore TeamCity.MSBuild.Logger.sln
dotnet msbuild build.proj /t:Build;Publish /r /p:Configuration=Release