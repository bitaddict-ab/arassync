@echo off
setlocal disabledelayedexpansion

echo.
echo Locating Nunit consolerunner 3.10.0

set NUNIT=%USERPROFILE%\.nuget\packages\nunit.consolerunner\3.10.0\tools\nunit3-console.exe

IF NOT EXIST %NUNIT% (
    echo Failed to find nunit3-console.exe
    exit /b 3
)

pushd %~dp0..
echo.
echo Running %NUNIT% %* in %CD%
echo.
%NUNIT% %*
popd 

exit /b !ERRORLEVEL!