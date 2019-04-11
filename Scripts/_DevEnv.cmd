@echo off
setlocal enabledelayedexpansion

REM https://github.com/Microsoft/vswhere
set VSWHERE=%~dp0\..\bin\vswhere.exe
set VSVERSION=[15.5,16.0)
set DEVENV=

echo.
echo Locating latest Visual Studio of version %VSVERSION% using %VSWHERE%

set VSWHERECMD=%VSWHERE% -latest -property installationPath -format value -version "%VSVERSION%" 

for /f "usebackq tokens=*" %%i in (`%VSWHERECMD%`) do (
  if "%DEVENV%x"=="x" (
      set DEVENV="%%i\Common7\IDE\devenv.com"
  )
)

setlocal disabledelayedexpansion

if %DEVENV%x==x ( 
    echo Failed to find Visual Studio installation
    exit /b 3
)

pushd %~dp0..
echo.
echo Running %DEVENV% %* in %CD%
echo.
%DEVENV% %*

exit /b %ERRORLEVEL%
