@echo off
rem installMediaTools.cmd -- smart install of the three media tools FileDir uses:
rem ExifTool, ffmpeg with ffprobe, and yt-dlp.
rem
rem Machine wide, by winget, for the same reason Pandoc is: together they are
rem well over 100 MB, and EdSharp, HomerScribe and FileDir all want them. Three
rem copies of the same executables under Program Files is not something to ask
rem anyone to download.
rem
rem One checkbox for three tools because they are used together: Type Extended
rem reads metadata with ExifTool, Output As converts audio and video with
rem ffmpeg, and Web Download fetches media with yt-dlp, which itself calls
rem ffmpeg. Installing one without the others leaves a command half working.
rem
rem Probe first: an existing copy is updated in place, never duplicated. A tool
rem already sitting in the FileDir folder wins over any of these at run time, so
rem a developer copy is never disturbed.
rem NOTHING PAUSES. Failures are logged, and the summary at the very end reports
rem the outcome of every checkbox.
setlocal
set "logFile=%LOCALAPPDATA%\FileDir\logs\FileDir_setup.log"
if not exist "%LOCALAPPDATA%\FileDir\logs" mkdir "%LOCALAPPDATA%\FileDir\logs" >nul 2>&1
echo [installMediaTools] started %date% %time% >> "%logFile%"
echo.
set "failed="

call :oneTool "ExifTool" "OliverBetz.ExifTool" "exiftool"
call :oneTool "ffmpeg" "Gyan.FFmpeg" "ffmpeg"
call :oneTool "yt-dlp" "yt-dlp.yt-dlp" "yt-dlp"

if defined failed goto failed
echo Done.
echo [installMediaTools] done >> "%logFile%"
exit /b 0

:failed
echo One or more media tools did not install. The log is:
echo %logFile%
echo [installMediaTools] FAILED: %failed% >> "%logFile%"
exit /b 3

:oneTool
rem %1 friendly name, %2 winget id, %3 executable name
where %~3 >nul 2>&1
if errorlevel 1 goto :installTool
echo Updating %~1
echo [installMediaTools] winget upgrade %~2 >> "%logFile%"
winget upgrade --id %~2 -e --scope machine --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
echo [installMediaTools] %~1 upgrade exit %errorlevel% >> "%logFile%"
exit /b 0

:installTool
echo Installing %~1
echo [installMediaTools] winget install %~2 >> "%logFile%"
winget install --id %~2 -e --scope machine --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
set "code=%errorlevel%"
echo [installMediaTools] %~1 install exit %code% >> "%logFile%"
rem winget refuses machine scope for a package published only per user; that is
rem not a failure, so the same install is tried again without the scope.
if not "%code%"=="0" (
  echo [installMediaTools] retrying %~1 without a scope >> "%logFile%"
  winget install --id %~2 -e --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
  echo [installMediaTools] %~1 retry exit %errorlevel% >> "%logFile%"
  if not "%errorlevel%"=="0" set "failed=%failed% %~1"
)
exit /b 0
