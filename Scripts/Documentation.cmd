@echo off

echo =============== Building documentation ===============
echo.

setlocal 
cd /D %~dp0..

call %~dp0\_RestorePackages.cmd
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

call %~dp0\_nuget.cmd restore .\Docs\Docs.csproj -SolutionDir %CD%
if ERRORLEVEL 1 exit /b %ERRORLEVEL%


echo.
echo === Generating HTML documentation into Documentation\_site ===
echo.

call %~dp0\_msbuild.cmd .\Docs\Docs.csproj /t:Build /verbosity:minimal /filelogger
echo.

IF ERRORLEVEL 1 (
  echo === HTML Build Failed ===
  EXIT /b %ERRORLEVEL%
)

echo === Success ===

IF "%JENKINS_URL%"=="" (
  echo.
  echo Launching web browser
  start "Docs" "%CD%\Docs\_site\index.html"
)
