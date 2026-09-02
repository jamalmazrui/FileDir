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
rem for years, is described upstream as partially fixed, and depends on how the
rem binary was built -- the build FileDir uses does not have it. Meanwhile every
rem photograph an iPhone takes is HEIC.
rem
rem Machine wide, by winget, like Pandoc and the media tools: about 50 MB, and
rem EdSharp and HomerScribe may want it too.
rem
rem Probe first, never pause, log milestones. The summary at the very end
rem reports the outcome of every checkbox.
setlocal
set "logFile=%LOCALAPPDATA%\FileDir\logs\FileDir_setup.log"
if not exist "%LOCALAPPDATA%\FileDir\logs" mkdir "%LOCALAPPDATA%\FileDir\logs" >nul 2>&1
echo [installImageTools] started %date% %time% >> "%logFile%"
echo.

where magick >nul 2>&1
if errorlevel 1 goto install_magick
echo Updating ImageMagick
echo [installImageTools] winget upgrade ImageMagick.ImageMagick >> "%logFile%"
winget upgrade --id ImageMagick.ImageMagick -e --scope machine --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
echo [installImageTools] upgrade exit %errorlevel% >> "%logFile%"
goto verify

:install_magick
echo Installing ImageMagick, about 50 MB
echo [installImageTools] winget install ImageMagick.ImageMagick >> "%logFile%"
winget install --id ImageMagick.ImageMagick -e --scope machine --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
set "code=%errorlevel%"
echo [installImageTools] install exit %code% >> "%logFile%"
rem winget refuses machine scope for a package published only per user; that is
rem not a failure, so the same install is tried again without the scope.
if not "%code%"=="0" (
  echo [installImageTools] retrying without a scope >> "%logFile%"
  winget install --id ImageMagick.ImageMagick -e --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
  echo [installImageTools] retry exit %errorlevel% >> "%logFile%"
)

:verify
where magick >nul 2>&1
if errorlevel 1 goto failed
for /f "delims=" %%v in ('magick -version 2^>nul') do (
  echo [installImageTools] %%v >> "%logFile%"
  goto reported
)
:reported
echo Done.
echo [installImageTools] done >> "%logFile%"
exit /b 0

:failed
echo ImageMagick was not found after the install step. The log is:
echo %logFile%
echo [installImageTools] FAILED: magick not found after install >> "%logFile%"
exit /b 3
