:: Starts a command prompt with correct path setup 
::
:: Copyright 2017- Consilium Marine & Safety AB
::
:: Marcus Sonestedt - 2017-01-16 - 

@echo off
pushd %~dp0
setlocal
title Aras Development Command Prompt - %CD%:: Setup paths so we can run msbuild (Assume VS2017 installed)

echo Setting up paths to Visual Studio 2017 ...

if "%VSWHERE%"=="" set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"

for /f "usebackq tokens=*" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.Component.MSBuild -property installationPath`) do (
  set VCInstallDir=%%i
)

echo %VCInstallDir%

call "%VCInstallDir%\Common7\Tools\VsMSBuildCmd.bat"
echo.

IF ERRORLEVEL 1 (
  echo Failed to set paths to Visual Studio 2015 MSBuild. Not installed?
  pause
  exit /b 1
)

:: Get Nuget.exe

SET NUGET_EXE=%~dp0\Core\.nuget\nuget.exe
IF NOT EXIST "%NUGET_EXE%" (
  echo Downloading NuGet.exe ...
  mkdir "%~dp0\Core\.nuget\" >NUL

  %~dp0\Core\ArasTools\Wget\wget.exe -nd -P %~dp0\.nuget\ --no-check-certificate https://dist.nuget.org/win-x86-commandline/latest/nuget.exe 
  REM powershell Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile "%NUGET_EXE%"

  :: wget changes title, so restore it
  title Aras Development Command Prompt - %CD%
)

attrib +h %~dp0\.nuget

:: Restore nuget packages

cd %~dp0\..\

echo Restoring NuGet packages ...
echo.
"%NUGET_EXE%" restore 
echo.

cd %~dp0

:: Build AML Sync Tool so that it is updated and available

echo Building ArasSyncTool ...
echo.
msbuild "%~dp0\ArasSync\ArasSync.csproj" /p:Configuration=Release /Verbosity:minimal /nologo
echo.

IF ERRORLEVEL 1 (
  echo Failed to build ArasSyncTool! Need to Restore NuGet packages on Solution first?
  pause
  exit /b 1
)

set PATH=%~dp0\ArasSync\bin\Release;%PATH%
set PATH=%~dp0\ArasTools\consoleUpgrade;%PATH%

echo Checking Aras login/password (to run unittests and sync features...)
echo.
ArasSync login --uselogininfo

:: Done

echo.
echo Listing command available in 'arassync' tool (ise in a feature's directory) ...
ArasSync --help

echo Listing Aras databases (* will be used for automated tests) ...
ArasSync listdb --shortformat
echo.

:: DONT_START_CMDPROMPT, or whatever you like
IF "%1x" == "x" ( 
  cmd /k
) ELSE (
  popd
)
