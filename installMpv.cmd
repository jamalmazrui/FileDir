@echo off
rem installMpv.cmd -- install mpv, the keyboard-driven media player FileDir
rem hands a play list to.
rem
rem Machine wide, by winget, for the same reason Pandoc and the media tools are:
rem about 60 to 90 MB installed, and EdSharp and HomerScribe may want it too.
rem
rem WHY IT IS NOT TICKED. mpv statically links its own copy of ffmpeg, which
rem FileDir already carries, so a good part of that download is a second copy of
rem something already on the machine. It buys playback and nothing else:
rem conversion is ffmpeg's job and stays ffmpeg's job. Worth having if you play
rem media from the file list, not worth downloading if you do not.
rem
rem Probe first: an existing mpv is updated in place, never duplicated.
rem NOTHING PAUSES. Failures are logged, and the summary at the very end reports
rem the outcome of every checkbox.
setlocal
set "logFile=%LOCALAPPDATA%\FileDir\logs\FileDir_setup.log"
if not exist "%LOCALAPPDATA%\FileDir\logs" mkdir "%LOCALAPPDATA%\FileDir\logs" >nul 2>&1
echo [installMpv] started %date% %time% >> "%logFile%"
echo.

where mpv >nul 2>&1
if errorlevel 1 goto install_mpv
echo Updating mpv
echo [installMpv] winget upgrade mpv >> "%logFile%"
winget upgrade --id mpv.mpv -e --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
echo [installMpv] upgrade exit %errorlevel% >> "%logFile%"
goto verify

:install_mpv
echo Installing mpv, about 60 MB
echo [installMpv] winget install mpv >> "%logFile%"
winget install --id mpv.mpv -e --scope machine --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
set "code=%errorlevel%"
echo [installMpv] install exit %code% >> "%logFile%"
rem winget refuses machine scope for a package published only per user; that is
rem not a failure, so the same install is tried again without the scope.
if not "%code%"=="0" (
  echo [installMpv] retrying without a scope >> "%logFile%"
  winget install --id mpv.mpv -e --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
  echo [installMpv] retry exit %errorlevel% >> "%logFile%"
)

:verify
where mpv >nul 2>&1
if errorlevel 1 goto failed
echo Done.
echo [installMpv] done >> "%logFile%"
exit /b 0

:failed
echo mpv was not found after the install step. The log is:
echo %logFile%
echo [installMpv] FAILED: mpv not found after install >> "%logFile%"
exit /b 3
