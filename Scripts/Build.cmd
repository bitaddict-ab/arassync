@echo off

echo =============== Building all projects ===============
echo.

setlocal
cd /D  %~dp0..

call %~dp0\_RestorePackages.cmd
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

REM no need to do twice in same session
set NO_PACKAGE_RESTORE=1

echo.
echo === Building projects in debug ===
echo.

call scripts\_msbuild.cmd BitAddict.Aras.OpenSource.sln /p:Configuration=Debug /nologo /verbosity:minimal /filelogger
set _BUILDERRLEVEL=%ERRORLEVEL%
echo.

IF NOT "%_BUILDERRLEVEL%" == "0" (
  echo === Build failed with return code %_BUILDERRLEVEL% ===
  exit /b 1
) 

echo === Success! ===
