@echo off
rem installMediaTools.cmd -- smart install of the three media tools FileDir uses:
rem ExifTool, ffmpeg with ffprobe, and yt-dlp.
rem
rem Machine wide, by winget, for the same reason Pandoc is: together they are
rem well over 100 MB, and EdSharp, HomerScribe and FileDir all want them.
rem
rem One checkbox for three tools because they are used together: Type Extended
rem reads metadata with ExifTool, Output As converts audio and video with
rem ffmpeg, and Web Download fetches media with yt-dlp, which itself calls
rem ffmpeg. Installing one without the others leaves a command half working.
rem
rem CONSOLE VERSUS LOG. The console gets short plain sentences. Commands, exit
rem codes and tool output go to the log.
rem
rem WHAT IT DID. Each tool that is installed, updated, reinstalled or fails
rem writes one line to FileDir_setup_actions.txt, which is what the Results box
rem reports. A tool that already sat there and needed nothing writes nothing.
rem
rem REINSTALL. Pass "reinstall" as an argument, or set FILEDIR_REINSTALL=1. The
rem installer's Reinstall checkbox passes that word, and each tool is then
rem installed again with --force rather than merely upgraded.
rem
rem NO PARENTHESISED BLOCK SETS AND READS THE SAME VARIABLE. Inside a block,
rem %errorlevel% is replaced when the block is READ rather than when it runs, so
rem a code captured that way is the code from before the command. Every branch
rem here is a label instead.
rem
rem NOTHING PAUSES.
setlocal
set "logDir=%LOCALAPPDATA%\FileDir\logs"
if not exist "%logDir%" mkdir "%logDir%" >nul 2>&1
set "logFile=%logDir%\FileDir_setup.log"
set "actionFile=%logDir%\FileDir_setup_actions.txt"
set "failed="
set "bReinstall="
set "sArgs=%*"
if not "%sArgs%"=="" echo %sArgs% | findstr /i /c:"reinstall" >nul && set "bReinstall=1"
if /i "%FILEDIR_REINSTALL%"=="1" set "bReinstall=1"

call :log "[installMediaTools] started %date% %time%"
call :log "[installMediaTools] arguments: %sArgs%"
call :log "[installMediaTools] reinstall requested: %bReinstall%"

call :oneTool "ExifTool" "OliverBetz.ExifTool" "exiftool"
call :oneTool "ffmpeg" "Gyan.FFmpeg" "ffmpeg"
call :oneTool "yt-dlp" "yt-dlp.yt-dlp" "yt-dlp"

if defined failed goto failed
call :log "[installMediaTools] done"
exit /b 0

:failed
call :say "Some media tools could not be installed. The log has the details."
call :log "[installMediaTools] FAILED:%failed%"
exit /b 3

:oneTool
rem %1 friendly name, %2 winget id, %3 executable name
where %~3 >nul 2>&1
if errorlevel 1 goto installTool
if defined bReinstall goto reinstallTool

call :say "Updating %~1."
call :log "[installMediaTools] winget upgrade %~2"
winget upgrade --id %~2 -e --scope machine --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
set "code=%errorlevel%"
call :log "[installMediaTools] %~1 upgrade exit %code%"
rem A non-zero code here nearly always means there was nothing to upgrade, which
rem is not an action and not a failure.
if not "%code%"=="0" goto upgradeNothing
call :action "%~1 updated."
exit /b 0

:upgradeNothing
call :log "[installMediaTools] %~1 was already current"
exit /b 0

:reinstallTool
call :say "Reinstalling %~1."
call :log "[installMediaTools] winget install %~2 --force"
winget install --id %~2 -e --force --scope machine --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
set "code=%errorlevel%"
call :log "[installMediaTools] %~1 reinstall exit %code%"
if "%code%"=="0" goto reinstallDone
call :log "[installMediaTools] retrying %~1 without a scope"
winget install --id %~2 -e --force --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
set "code=%errorlevel%"
call :log "[installMediaTools] %~1 retry exit %code%"
if "%code%"=="0" goto reinstallDone
call :action "%~1 could not be reinstalled."
set "failed=%failed% %~1"
exit /b 0

:reinstallDone
call :action "%~1 reinstalled."
exit /b 0

:installTool
call :say "Installing %~1."
call :log "[installMediaTools] winget install %~2"
winget install --id %~2 -e --scope machine --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
set "code=%errorlevel%"
call :log "[installMediaTools] %~1 install exit %code%"
rem winget refuses machine scope for a package published only per user; that is
rem not a failure, so the same install is tried again without the scope.
if "%code%"=="0" goto installDone
call :log "[installMediaTools] retrying %~1 without a scope"
winget install --id %~2 -e --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
set "code=%errorlevel%"
call :log "[installMediaTools] %~1 retry exit %code%"
if "%code%"=="0" goto installDone
call :action "%~1 could not be installed."
set "failed=%failed% %~1"
exit /b 0

:installDone
call :action "%~1 installed."
exit /b 0

:log
>>"%logFile%" echo %~1
goto :eof

:say
echo %~1
>>"%logFile%" echo [console] %~1
goto :eof

:action
>>"%actionFile%" echo %~1
>>"%logFile%" echo [action] %~1
goto :eof
