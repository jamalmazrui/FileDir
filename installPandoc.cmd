@echo off
rem installPandoc.cmd -- smart install of Pandoc, machine-wide.
rem
rem Pandoc goes to C:\Program Files\Pandoc, which is where its own installer
rem puts it and where every other program looks for it. One copy on the machine
rem serves FileDir, EdSharp and HomerScribe alike. It is about 100 MB, and a
rem copy of that inside each program's folder is not something to ask anyone to
rem download three times.
rem
rem Probe first: an existing Pandoc is updated in place, never duplicated.
rem NOTHING PAUSES: a console waiting for a keypress interrupts the
rem installation. Failures are logged, and the summary shown at the very end
rem reports the outcome of every checkbox.
setlocal
set "logFile=%LOCALAPPDATA%\FileDir\logs\FileDir_setup.log"
if not exist "%LOCALAPPDATA%\FileDir\logs" mkdir "%LOCALAPPDATA%\FileDir\logs" >nul 2>&1
echo [installPandoc] started %date% %time% >> "%logFile%"
echo.

rem A just-installed Pandoc is not on this console's PATH yet, so the install
rem location is probed as well as the name. This is the gap that made the Ollama
rem script reinstall a program that was already there.
if exist "%ProgramFiles%\Pandoc\pandoc.exe" set "PATH=%ProgramFiles%\Pandoc;%PATH%"
where pandoc >nul 2>&1
if errorlevel 1 goto install_pandoc
echo Updating Pandoc
echo [installPandoc] winget upgrade JohnMacFarlane.Pandoc >> "%logFile%"
winget upgrade --id JohnMacFarlane.Pandoc -e --scope machine --architecture x64 --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
echo [installPandoc] winget upgrade exit %errorlevel% >> "%logFile%"
if errorlevel 1 (echo Already current.) else (echo Updated.)
goto verify

:install_pandoc
echo Installing Pandoc, about 100 MB
echo [installPandoc] winget install JohnMacFarlane.Pandoc >> "%logFile%"
winget install --id JohnMacFarlane.Pandoc -e --scope machine --architecture x64 --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
echo [installPandoc] winget install exit %errorlevel% >> "%logFile%"

:verify
if exist "%ProgramFiles%\Pandoc\pandoc.exe" set "PATH=%ProgramFiles%\Pandoc;%PATH%"
where pandoc >nul 2>&1
if errorlevel 1 goto failed
for /f "delims=" %%v in ('pandoc --version 2^>nul') do (
  echo [installPandoc] %%v >> "%logFile%"
  goto reported
)
:reported
echo Done.
echo [installPandoc] done >> "%logFile%"
exit /b 0

:failed
echo Pandoc was not found after the install step. The log is:
echo %logFile%
echo [installPandoc] FAILED: pandoc not found after install >> "%logFile%"
exit /b 3
