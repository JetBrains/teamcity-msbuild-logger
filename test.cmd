dotnet restore TeamCity.MSBuild.Logger.sln
dotnet msbuild build.proj /t:Build;Publish;Test /r /p:Configuration=Release