@echo off
setlocal enabledelayedexpansion
rem restoreFileDir.cmd -- undo the stale files a FileDir.zip of 5 October 2026
rem laid over C:\FileDir, and retire the walks the twelve-walk pattern replaced.
rem
rem WHAT HAPPENED. That zip was built on an older GitHub master and carried whole
rem files -- FileDir_setup.iss, build.cmd, Developer.md and more -- that
rem overwrote newer ones. The build then failed on an installer function the
rem older .iss did not define. Nothing is lost: every one of those files is
rem committed in your repository, and git puts the committed version back.
rem
rem WHAT THIS DOES, each step logged to logs\FileDir-restore-<stamp>.log:
rem   1. git checkout -- .    restores every tracked file to its committed state
rem      (the ten new walks are untracked and are not touched)
rem   2. deletes build.cmd, which the zip re-added beside your build.cmd
rem   3. deletes the three retired walks and any audio under the old names
rem   4. leaves the ten walks of the pattern in help\ for the next build
set "sHere=%~dp0"
pushd "%sHere%.."
if not exist logs mkdir logs
for /f %%I in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd-HHmmss"') do set "stamp=%%I"
set "log=logs\FileDir-restore-!stamp!.log"
echo restoreFileDir start %date% %time% > "!log!"
echo Script: %~f0 >> "!log!"
echo Folder: %cd% >> "!log!"
echo Command line: %~nx0 %* >> "!log!"
echo Restoring committed files with git...
git status --short >> "!log!" 2>&1
git checkout -- . >> "!log!" 2>&1
echo git checkout exit %errorlevel% >> "!log!"
if exist build.cmd (
  del /q build.cmd
  echo Removed build.cmd, re-added by the zip beside build.cmd >> "!log!"
)
for %%f in (help\Tutorial_00_Overview.inix help\Tutorial_01_Tagging.inix help\Tutorial_Tagging.inix) do (
  if exist "%%f" (
    git rm -q --cached "%%f" >> "!log!" 2>&1
    del /q "%%f"
    echo Retired %%f >> "!log!"
  )
)
if exist "help\tutorials\Tutorial_*.mp3" (
  del /q "help\tutorials\Tutorial_*.mp3"
  echo Retired audio under the old names >> "!log!"
)
set /a n=0
for %%f in (help\Tutorial_*.inix) do set /a n+=1
echo !n! walk scripts in help >> "!log!"
echo Done. !n! walk scripts in help. The log is !log!
echo restoreFileDir end %date% %time% >> "!log!"
popd
endlocal
