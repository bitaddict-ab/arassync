@echo off
setlocal
pushd %~dp0..

::call %~dp0\Documentation.cmd
if ERRORLEVEL 1 goto done

:: Get current branch
git rev-parse --abbrev-ref HEAD > .git\current_branch.txt

echo *** Switching to and updating gh-pages branch *** 

git checkout --orphan gh-pages
if ERRORLEVEL 1 goto done
git pull 
if ERRORLEVEL 1 goto done

echo *** Removing old files from disk *** 

git ls-files > files_to_delete.txt
if ERRORLEVEL 1 goto done
for /f "delims=" %%f in (files_to_delete.txt) do del "%%f"
if ERRORLEVEL 1 goto done
del files_to_delete.txt

echo *** Moving site files to %CD% ***
move .\Docs\_site\* .
if ERRORLEVEL 1 goto done

echo *** Committing all changes *** 
git ci -a -m"Updating gh-pages"
if ERRORLEVEL 1 goto done

echo Success!

done:
echo Cleaning up ...
set ERR=%ERRLVL%
for /f "delims=" %%f in (.git\current_branch.txt) do git checkout "%%f"
del .git\current_branch.txt
echo Done ...
pause
popd
EXIT /b %ERRLVL%