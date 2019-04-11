@echo off 

echo Running NuGet %* in %CD% ...
echo.

%~dp0\..\bin\nuget.exe %* -NonInteractive
exit /B %ERRORLEVEL%