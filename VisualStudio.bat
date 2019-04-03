:: First checks/asks for ARAS login, then starts Visual Studio 2015. 
:: This allows developers to run unit/integration tests against server from within VS.

@echo off

cd %~dp0

call CommandPrompt DONT_START_CMDPROMPT

echo Starting Visual Studio 2015 ...
echo.

start "%VS140COMNTOOLS%\..\IDE\devenv.exe" "%~dp0Consilium.Aras.sln"

:: Sleep 5 secs
ping -n 5 localhost >NUL