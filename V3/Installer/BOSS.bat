@echo off
pushd %~dp0

set GIT=F:/Documents/Clients/Parallel 42/The Skypark/Git/
set WORKSPACE=F:/Documents/Clients/Parallel 42/The Skypark/Dist/
set SHA1="C88AD16B642DD3250BF72A248DB7139EA35B798C"

set input=%WORKSPACE%Input/
set tools=%WORKSPACE%Tools/


mkdir "%input%"

ECHO.
ECHO ------------------ Working directories ------------------
ECHO %input%

ECHO.
ECHO ------------------ Prepare ------------------
rmdir /s/q "%input%installer/"

ECHO.
ECHO ------------------ Build / Obfuscate ------------------
set msBuildExe="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
%msBuildExe% "./Release.proj"


ECHO.
ECHO ------------------ Sign ------------------
signtool sign /tr "http://timestamp.digicert.com" /td sha256 /fd sha256 /as /n "Parallel 42 LLC" /v "%GIT%tsp-installer-3/BOSS/Final/The Skypark Installer.exe"


::ECHO.
::ECHO ------------------ Transponder ------------------
::mkdir "%input%transponder/"
::echo f | xcopy /e /f /y /i "./TSP_Launcher/bin/Release/TSP_Launcher.exe" "%input%transponder/TSP_Launcher.exe"
::echo f | xcopy /e /f /y /i "./BOSS/Obfuscator_Output/TSP_Transponder.exe" "%input%transponder/TSP_Transponder.exe"
::echo f | xcopy /e /f /y /i "./TSP_Transponder/bin/x64/Release/airports.csv" "%input%transponder/airports.csv"

pause

exit 0