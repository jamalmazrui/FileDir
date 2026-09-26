@echo off
rem installImageMagick.cmd -- install or update ImageMagick, which FileDir uses to convert and resize images.
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

where magick >nul 2>&1
if not errorlevel 1 if not defined bReinstall goto :update

echo Installing ImageMagick. Nothing is asked of you while it runs.
winget install --id ImageMagick.ImageMagick --exact --silent --accept-source-agreements --accept-package-agreements --disable-interactivity >> "%log%" 2>&1
set "iCode=%ERRORLEVEL%"
call "%~dp0homerInstall.cmd" log "winget install ImageMagick.ImageMagick exit code %iCode%"
if not "%iCode%"=="0" goto :failed
echo ImageMagick is installed.
goto :done

:update
echo ImageMagick is installed. Checking for a newer version.
winget upgrade --id ImageMagick.ImageMagick --exact --silent --accept-source-agreements --accept-package-agreements --disable-interactivity >> "%log%" 2>&1
set "iCode=%ERRORLEVEL%"
call "%~dp0homerInstall.cmd" log "winget upgrade ImageMagick.ImageMagick exit code %iCode%"
if "%iCode%"=="0" echo ImageMagick was updated.
if not "%iCode%"=="0" echo ImageMagick is already the newest winget offers.
goto :done

:failed
echo(
echo ImageMagick could not be installed automatically. The log says why:
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
