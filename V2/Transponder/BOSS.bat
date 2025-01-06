@echo off
pushd %~dp0

set GIT=F:\Documents\Clients\Parallel 42\The Skypark\Git\
set WORKSPACE=F:\Documents\Clients\Parallel 42\The Skypark\Git\Dist\

set input=%WORKSPACE%Input/

mkdir "%input%"

ECHO.
ECHO ------------------ Working directories ------------------
ECHO %input%

ECHO.
ECHO ------------------ Prepare ------------------
rmdir /s/q "%input%transponder/"

ECHO.
ECHO ------------------ Build / Obfuscate ------------------
set msBuildExe="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
%msBuildExe% "./Release.proj"
pause

ECHO.
ECHO ------------------ Transponder ------------------
mkdir "%input%transponder/"
echo f | xcopy /e /f /y /i "./TSP_Transponder/bin/x64/Release/VersionNumber.txt" "%input%v.txt"
echo f | xcopy /e /f /y /i "./TSP_Launcher/bin/Release/TSP_Launcher.exe" "%input%transponder/TSP_Launcher.exe"
echo f | xcopy /e /f /y /i "./BOSS/Obfuscator_Output/TSP_Transponder.exe" "%input%transponder/TSP_Transponder.exe"

pause
::%SystemRoot%\explorer.exe "%WORKSPACE%"
exit 0