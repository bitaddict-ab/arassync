:: First checks/asks for ARAS login, then starts Visual Studio. 
:: This allows developers to run unit/integration tests against server from within VS.

@echo off

call %~dp0\CommandPrompt DONT_START_CMDPROMPT
if ERRORLEVEL 1 (
    pause
    exit /b 1
)

echo.
echo Starting Visual Studio...
echo.

start "%~dp0\devenv.cmd" "%~dp0\..\BitAddict.Aras.OpenSource.sln"

:: Sleep 5 secs
ping -n 5 localhost >NUL
