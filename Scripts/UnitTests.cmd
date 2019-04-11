@echo off
setlocal

cd /D %~dp0..

call %~dp0\Build.cmd
SET _BUILDERRLEVEL=%ERRORLEVEL%

echo =============== Running unit tests ===============
echo.

echo === Running non-integration unit tests ===
echo.

set XMLREPORT="%CD%\nunit-report.xml"
set HTMLREPORT="%CD%\nunit-report.html"

setlocal disabledelayedexpansion
call %~dp0\_NUnit.cmd Cpac.Aras.sln --skipnontestassemblies --labels=before^
 --where:"cat != IntegrationTest" --result=%XMLREPORT%;format=nunit2 --noheader

set _TESTERRLEVEL=%ERRORLEVEL%

echo. 
echo === Generating HTML report ===

%CD%\bin\msxsl.exe %XMLREPORT% "%~dp0\nunit-templates\html-report-v2.xslt" -o %HTMLREPORT%
set _HTMLERRLEVEL=%ERRORLEVEL%

echo.
IF NOT "%_BUILDERRLEVEL%" == "0" (
  echo === Build failed with return code %_BUILDERRLEVEL% ===
) 

IF NOT "%_TESTERRLEVEL%" == "0" ( 
  echo === Unit tests failed with return code %_TESTERRLEVEL% ===
)

IF NOT "%_HTMLERRLEVEL%" == "0" ( 
  echo === HTML report generation failed with return code %_HTMLERRLEVEL% ===
)

IF NOT "%_BUILDERRLEVEL%%_TESTERRLEVEL%%_HTMLERRLEVEL%" == "000" (  
  exit /b 1
)

echo.
echo === Success! ===
echo.

IF "%JENKINS_URL%"=="" (
  echo Launching web browser
  start "UnitTests" "%HTMLREPORT%"
)

