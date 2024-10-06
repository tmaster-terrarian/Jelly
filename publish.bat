@echo off
set /p version=<version.txt

dotnet pack ./LDtk/LDtk.csproj -c Release -o ./Nuget/ /p:version=%version%

dotnet nuget push "Nuget/JellyEngine.%version%.nupkg" --api-key %1 --source "github"
