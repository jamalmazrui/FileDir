@echo off
rem installPandoc.cmd -- install or update Pandoc, which FileDir uses to convert documents between formats.
rem
rem Run from the installer's final page, or on its own at any time. The
rem common half -- the log under %LOCALAPPDATA%\FileDir\logs, the quiet
rem flag when the installer is driving -- is the kit's homerInstall.cmd, so a
rem fix there reaches every install script in every Homer app.
rem
rem MACHINE WIDE, to its own default folder, never under FileDir's tree: an
rem upgrade of FileDir replaces that tree and would take the tool with it.
setlocal enabledelayedexpansion
set "sScript=%~n0"
if not exist "%~dp0homerInstall.cmd" (
  echo(
  echo homerInstall.cmd is missing from %~dp0
  echo That file is part of this program. Reinstall, or copy it from the
  echo program's zip into this folder, and run this again.
  echo(
  if not defined noPause pause
  exit /b 1
)
call "%~dp0homerInstall.cmd" setup "%~f0" %*

set "bReinstall="
for %%A in (%*) do if /i "%%A"=="reinstall" set "bReinstall=1"

where pandoc >nul 2>&1
if not errorlevel 1 if not defined bReinstall goto :update

echo Installing Pandoc. Nothing is asked of you while it runs.
winget install --id JohnMacFarlane.Pandoc --exact --silent --accept-source-agreements --accept-package-agreements --disable-interactivity >> "%log%" 2>&1
set "iCode=%ERRORLEVEL%"
call "%~dp0homerInstall.cmd" log "winget install JohnMacFarlane.Pandoc exit code %iCode%"
if not "%iCode%"=="0" goto :failed
echo Pandoc is installed.
goto :done

:update
echo Pandoc is installed. Checking for a newer version.
winget upgrade --id JohnMacFarlane.Pandoc --exact --silent --accept-source-agreements --accept-package-agreements --disable-interactivity >> "%log%" 2>&1
set "iCode=%ERRORLEVEL%"
call "%~dp0homerInstall.cmd" log "winget upgrade JohnMacFarlane.Pandoc exit code %iCode%"
if "%iCode%"=="0" echo Pandoc was updated.
if not "%iCode%"=="0" echo Pandoc is already the newest winget offers.
goto :done

:failed
echo(
echo Pandoc could not be installed automatically. The log says why:
echo   %log%
echo(
if not defined noPause pause
endlocal
exit /b 1

:done
echo(
if not defined noPause pause
endlocal
exit /b 0
