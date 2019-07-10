@echo off
setlocal enabledelayedexpansion

REM https://github.com/Microsoft/vswhere
set VSWHERE=%~dp0\..\bin\vswhere.exe

REM VS2017-VS2019
set VSVERSION=[15.0,17.0)

if "%MSBUILD%x"=="x" goto locate

if EXIST "%MSBUILD%" (
    goto done
)

ECHO MsBuild.exe not found at %MSBUILD%
SET MSBUILD=

:locate

echo.
echo Locating latest Visual Studio of version %VSVERSION% using %VSWHERE%

set VSWHERECMD=%VSWHERE% -latest -property installationPath -format value -requires Microsoft.Component.MSBuild -version "%VSVERSION%"

for /f "usebackq tokens=*" %%i in (`%VSWHERECMD%`) do (
    :: VS2017
    if %MSBUILD%x==x (
        if EXIST "%%i\MSBuild\15.0\bin\msbuild.exe" (
            set MSBUILD="%%i\MSBuild\15.0\bin\msbuild.exe"
        )
        :: VS2019+
        if EXIST "%%i\MSBuild\Current\bin\msbuild.exe" (
            set MSBUILD="%%i\MSBuild\Current\bin\msbuild.exe"
        )
    )
)

setlocal disabledelayedexpansion

if %MSBUILD%x==x ( 
    echo Failed to find Visual Studio installation with MSBuild
    exit /b 3
)

if NOT EXIST %MSBUILD% (
    echo Found Visual Studio but expected MSBuild at %MSBUILD%
    exit /b 3
)

:done
pushd %~dp0..
echo.
echo Running %MSBUILD% %* in %CD%
echo.
%MSBUILD% %*

exit /b %ERRORLEVEL%
