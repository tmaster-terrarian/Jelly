@echo off
set /p version=<version.txt

dotnet pack ./LDtk/LDtk.csproj -c Release -o ./Nuget/ /p:version=%version%

dotnet nuget push "Nuget/Jelly.%version%.nupkg" --api-key %1 --source "github"
