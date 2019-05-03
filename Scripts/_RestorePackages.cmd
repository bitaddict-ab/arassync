@echo off
echo.

if NOT "%NO_PACKAGE_RESTORE%"=="" ( 
  echo Package restore disabled via environment variable
  exit /b 0 
)

echo === Restoring packages ===
echo.
REM powershell -command "Set-ExecutionPolicy Bypass -Scope CurrentUser"
REM powershell -command ".\scripts\_RestoreNuGetCsproj.ps1" %CD%
call %~dp0\_nuget.cmd restore .\BitAddict.Aras.OpenSource.sln
echo.
exit /b %ERRORLEVEL%
