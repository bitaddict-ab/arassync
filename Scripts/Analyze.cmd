@echo off
setlocal
cd /D %~dp0..

call %~dp0\Build.cmd
set _BUILD_ERRORLEVEL %ERRORLEVEL%

echo =============== Analyzing code for quality/conformity issues ===============
echo.

echo === Analyzing projects ===
echo.

setlocal disabledelayedexpansion
set INSPECTCODE="%USERPROFILE%\.nuget\packages\jetbrains.resharper.commandlinetools\2018.3.4\tools\InspectCode.exe"

IF NOT EXIST %INSPECTCODE% (
  echo Failed to find %INSPECTCODE%
  exit /b 3
)

set XMLREPORT="%CD%\resharper-inspect-report.xml"
set HTMLREPORT="%CD%\resharper-inspect-report.html"

@echo on
%INSPECTCODE% Cpac.Aras.sln --output=%XMLREPORT% --format=xml --severity=WARNING --verbosity=WARN --no-swea
@echo off
IF ERRORLEVEL 1 exit /b %ERRORLEVEL%

echo. 
echo === Generating HTML report ===
echo. 

%CD%\bin\msxsl.exe %XMLREPORT% "%~dp0\inspectcode-templates\inspectcode.xsl" -o %HTMLREPORT%
IF ERRORLEVEL 1 exit /b %ERRORLEVEL%

echo.
echo === Success ===
echo.

IF "%JENKINS_URL%"=="" (
  echo Launching web browser
  start "Analyze" "%HTMLREPORT%"
)

exit /b %_BUILD_ERRORLEVEL%

