@echo off
echo.

if NOT "%NO_PACKAGE_RESTORE%"=="" ( 
  echo Package restore disabled via environment variable
  exit /b 0 
)

REM call scripts\_msbuild.cmd /t:Restore %MSBUILD_ARGS%
REM call scripts\_nuget.cmd restore

echo === Restoring packages ===
echo.
REM powershell -command "Set-ExecutionPolicy Bypass -Scope CurrentUser"
REM powershell -command ".\scripts\_RestoreNuGetCsproj.ps1" %CD%
call scripts\_nuget.cmd restore Cpac.Aras.sln
echo.
exit /b %ERRORLEVEL%
