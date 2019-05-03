:: Build and configures ArasSync.exe, then starts a command prompt with correct PATH
::
:: Copyright 2017-2019 Consilium Marine & Safety AB, CPAC Systems AB, Bit Addict AB
:: MIT License - see COPYING.TXT

@echo off
pushd %~dp0\..
setlocal
title Aras Development Command Prompt - %CD%

:: Restore nuget packages
call %~dp0\_RestorePackages.cmd

IF ERRORLEVEL 1 (
  echo Failed to build ArasSync! Something is wrong.
  pause
  exit /b 1
)

:: Build ArasSync so that the exe is up-to-date and available in PATH

echo === Building ArasSync in Release ===
call %~dp0\_MSBuild.cmd "%CD%\ArasSync\ArasSync.csproj" /p:Configuration=Release /Verbosity:minimal /nologo

IF ERRORLEVEL 1 (
  echo Failed to build ArasSync! Something is wrong.
  pause
  exit /b 1
)

set PATH=%CD%\ArasSync\bin\Release;%PATH%

@echo on
ArasSync about
@echo off
if ERRORLEVEL 1 (
  echo Failed to run ArasSync! Something is wrong.
  pause
  exit /b 1
)

echo.
echo Listing command available in 'arassync' tool (main use in a feature's directory) ...
@echo on
ArasSync --help
@echo off

echo Listing configured Aras databases ...
@echo on
ArasSync listdb
@echo off

echo Checking Aras login/password (encrypted and stored locally, used to run unittests and sync features...)
@echo on
ArasSync login --uselogininfo
@echo off

:: DONT_START_CMDPROMPT, or whatever you like
IF "%1x" == "x" ( 
  echo.
  echo === Ready! ===
  echo.
  cmd /k
) ELSE (
  popd
)
