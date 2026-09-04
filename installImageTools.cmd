@echo off
rem installImageTools.cmd -- install ImageMagick, which reads the picture
rem formats ffmpeg cannot.
rem
rem WHAT IT BUYS. iPhone photos (HEIC and HEIF), camera raw files (CR2, NEF,
rem ARW, DNG and the rest), SVG drawings turned into pictures, and Windows icon
rem files. ffmpeg handles PNG, JPEG, BMP, GIF, TIFF and WebP perfectly well and
rem keeps doing so; this is only for what it cannot reach.
rem
rem HEIC is the reason to bother. FFmpeg's HEIF support has been an open ticket
rem for years and depends on how the binary was built -- the build FileDir uses
rem does not have it. Meanwhile every photograph an iPhone takes is HEIC.
rem
rem Machine wide, by winget, like Pandoc and the media tools: about 50 MB, and
rem EdSharp and HomerScribe may want it too.
rem
rem CONSOLE VERSUS LOG. The console gets short plain sentences. Commands, exit
rem codes and tool output go to the log.
rem
rem WHAT IT DID. Anything actually done is written to
rem FileDir_setup_actions.txt, which is what the Results box reports. A
rem component already present that needed nothing writes no action.
rem
rem REINSTALL. Pass "reinstall" as an argument, or set FILEDIR_REINSTALL=1,
rem which is what the installer's Reinstall checkbox does. --force is required
rem or winget declines to install a version it has already installed.
rem
rem NOTHING PAUSES.
setlocal
set "logDir=%LOCALAPPDATA%\FileDir\logs"
if not exist "%logDir%" mkdir "%logDir%" >nul 2>&1
set "logFile=%logDir%\FileDir_setup.log"
set "actionFile=%logDir%\FileDir_setup_actions.txt"
set "sVersion="
set "didWhat="
set "bReinstall="
set "sArgs=%*"
if not "%sArgs%"=="" echo %sArgs% | findstr /i /c:"reinstall" >nul && set "bReinstall=1"
if /i "%FILEDIR_REINSTALL%"=="1" set "bReinstall=1"

call :log "[installImageTools] started %date% %time%"
call :log "[installImageTools] arguments: %sArgs%"
call :log "[installImageTools] reinstall requested: %bReinstall%"

where magick >nul 2>&1
if errorlevel 1 goto install_magick
if defined bReinstall goto reinstall_magick

call :say "Updating ImageMagick."
call :log "[installImageTools] winget upgrade ImageMagick.ImageMagick"
winget upgrade --id ImageMagick.ImageMagick -e --scope machine --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
set "code=%errorlevel%"
call :log "[installImageTools] upgrade exit %code%"
if "%code%"=="0" set "didWhat=updated"
if not "%code%"=="0" call :say "ImageMagick is already current."
goto verify

:reinstall_magick
call :say "Reinstalling ImageMagick."
call :log "[installImageTools] winget install ImageMagick.ImageMagick --force"
winget install --id ImageMagick.ImageMagick -e --force --scope machine --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
call :log "[installImageTools] reinstall exit %errorlevel%"
set "didWhat=reinstalled"
goto verify

:install_magick
call :say "Installing ImageMagick, about 50 MB."
call :log "[installImageTools] winget install ImageMagick.ImageMagick"
winget install --id ImageMagick.ImageMagick -e --scope machine --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
set "code=%errorlevel%"
call :log "[installImageTools] install exit %code%"
set "didWhat=installed"
rem MACHINE SCOPE ONLY, no per-user retry. A Homer Tools installer runs as
rem administrator and installs machine wide. A retry without the scope would
rem put the program where this policy does not intend, and would turn a visible
rem failure into a silent one.
if not "%code%"=="0" call :log "[installImageTools] machine-wide install failed with %code%"

:verify
where magick >nul 2>&1
if errorlevel 1 goto failed
for /f "tokens=1,2,3" %%a in ('magick -version 2^>nul') do if not defined sVersion set "sVersion=%%c"
call :log "[installImageTools] version: %sVersion%"
if not defined didWhat goto quietDone
if defined sVersion (call :action "ImageMagick %didWhat%, %sVersion%") else (call :action "ImageMagick %didWhat%.")
call :say "Done."
call :log "[installImageTools] done"
exit /b 0

:quietDone
call :log "[installImageTools] nothing needed doing"
exit /b 0

:failed
call :say "ImageMagick could not be installed. The log has the details."
call :action "ImageMagick could not be installed."
call :log "[installImageTools] FAILED: magick not found after install"
exit /b 3

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
