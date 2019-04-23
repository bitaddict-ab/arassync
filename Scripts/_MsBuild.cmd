@echo off
setlocal enabledelayedexpansion

REM https://github.com/Microsoft/vswhere
set VSWHERE=%~dp0\..\bin\vswhere.exe
set VSVERSION=[15.5,17.0)
set MSBUILD=

echo.
echo Locating latest Visual Studio of version %VSVERSION% using %VSWHERE%

set VSWHERECMD=%VSWHERE% -latest -property installationPath -format value -requires Microsoft.Component.MSBuild -version "%VSVERSION%" 

for /f "usebackq tokens=*" %%i in (`%VSWHERECMD%`) do (
  if "%MSBUILD%x"=="x" (
      set MSBUILD="%%i\MSBuild\15.0\bin\msbuild.exe"
  )
)

setlocal disabledelayedexpansion

if %MSBUILD%x==x ( 
    echo Failed to find Visual Studio installation with MSBuild
    exit /b 3
)

pushd %~dp0..
echo.
echo Running %MSBUILD% %* in %CD%
echo.
%MSBUILD% %*

exit /b %ERRORLEVEL%
