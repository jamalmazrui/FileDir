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
rem THE PACKAGE ID IS shinchiro.mpv, NOT mpv.mpv.
rem
rem "mpv.mpv" does not exist. winget answered -1978335212, which is
rem APPINSTALLER_CLI_ERROR_NO_APPLICATIONS_FOUND -- "no packages found" -- and
rem that was read as a scope problem for three rounds. Nothing was ever
rem installed. shinchiro.mpv is the Windows build, wrapped in an installer that
rem registers file associations and the context menu.
rem
rem MACHINE SCOPE ONLY, no per-user retry. A Homer Tools installer runs as
rem administrator and installs machine wide; anyone wanting a copy that needs no
rem privileges should use the zip archive instead. A retry without the scope
rem would put the program somewhere this policy does not intend and would hide
rem the failure, which is exactly what happened here.
echo Installing mpv, about 60 MB
echo [installMpv] winget install shinchiro.mpv --scope machine >> "%logFile%"
winget install --id shinchiro.mpv -e --scope machine --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
set "code=%errorlevel%"
echo [installMpv] install exit %code% >> "%logFile%"
if not "%code%"=="0" (
  echo [installMpv] machine-wide install failed with %code% >> "%logFile%"
  echo mpv could not be installed machine wide. The reason is in the log.
)

:verify
rem VERIFY BY LOCATION, NOT BY "where".
rem
rem This script asked "where mpv" straight after installing it, and a process
rem cannot see a PATH entry added after it started. So a successful install was
rem reported as a failure, the summary repeated it, and the person reasonably
rem installed it again. mpv was there the whole time.
set "mpvExe="
for %%d in (
  "%ProgramFiles%\MPV Player\mpv.exe"
  "%ProgramFiles(x86)%\MPV Player\mpv.exe"
  "%ProgramFiles%\mpv\mpv.exe"
  "%ProgramFiles%\MPV Media Player\mpv.exe"
  "%ProgramFiles(x86)%\MPV Media Player\mpv.exe"
  "%LOCALAPPDATA%\Programs\mpv\mpv.exe"
  "%LOCALAPPDATA%\Microsoft\WinGet\Links\mpv.exe"
) do if not defined mpvExe if exist %%d set "mpvExe=%%~d"
if not defined mpvExe (
  rem The folder winget unpacks a portable package into, whose name carries the
  rem publisher and version, so it is searched rather than guessed.
  for /f "delims=" %%p in ('dir /s /b "%LOCALAPPDATA%\Microsoft\WinGet\Packages\mpv.exe" 2^>nul') do (
    if not defined mpvExe set "mpvExe=%%p"
  )
)
if not defined mpvExe where mpv >nul 2>&1 && for /f "delims=" %%p in ('where mpv 2^>nul') do (
  if not defined mpvExe set "mpvExe=%%p"
)
if not defined mpvExe goto failed
echo [installMpv] found at %mpvExe% >> "%logFile%"
echo Done.
echo [installMpv] done >> "%logFile%"
exit /b 0

:failed
echo mpv was not found after the install step. The log is:
echo %logFile%
echo [installMpv] FAILED: mpv not found after install >> "%logFile%"
exit /b 3
