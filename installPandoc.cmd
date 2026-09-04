@echo off
rem installPandoc.cmd -- smart install of Pandoc, machine-wide.
rem
rem Pandoc goes to C:\Program Files\Pandoc, which is where its own installer
rem puts it and where every other program looks for it. One copy on the machine
rem serves FileDir, EdSharp and HomerScribe alike. It is about 100 MB, and a
rem copy of that inside each program's folder is not something to ask anyone to
rem download three times.
rem
rem CONSOLE VERSUS LOG. The console gets short plain sentences. Commands, exit
rem codes and tool output go to the log.
rem
rem WHAT IT DID. Anything actually done -- installed, updated, reinstalled or
rem failed -- is written to FileDir_setup_actions.txt, which is what the Results
rem box reports. A component that was already present and needed nothing writes
rem no action, so the box stays quiet about it.
rem
rem REINSTALL. Pass "reinstall" as an argument, or set FILEDIR_REINSTALL=1. The
rem installer's Reinstall checkbox passes that word. Without it, an existing
rem Pandoc is updated in place; with it, the same version is installed again,
rem which needs --force because winget otherwise declines to repeat itself.
rem
rem NOTHING PAUSES: a console waiting for a keypress interrupts the installation.
setlocal
set "logDir=%LOCALAPPDATA%\FileDir\logs"
if not exist "%logDir%" mkdir "%logDir%" >nul 2>&1
set "logFile=%logDir%\FileDir_setup.log"
set "actionFile=%logDir%\FileDir_setup_actions.txt"
set "sVersion="
set "bReinstall="
set "sArgs=%*"
if not "%sArgs%"=="" echo %sArgs% | findstr /i /c:"reinstall" >nul && set "bReinstall=1"
if /i "%FILEDIR_REINSTALL%"=="1" set "bReinstall=1"

call :log "[installPandoc] started %date% %time%"
call :log "[installPandoc] arguments: %sArgs%"
call :log "[installPandoc] reinstall requested: %bReinstall%"

rem A just-installed Pandoc is not on this console's PATH yet, so the install
rem location is probed as well as the name.
if exist "%ProgramFiles%\Pandoc\pandoc.exe" set "PATH=%ProgramFiles%\Pandoc;%PATH%"
where pandoc >nul 2>&1
if errorlevel 1 goto install_pandoc
if defined bReinstall goto reinstall_pandoc

call :say "Updating Pandoc."
call :log "[installPandoc] winget upgrade JohnMacFarlane.Pandoc"
winget upgrade --id JohnMacFarlane.Pandoc -e --scope machine --architecture x64 --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
set "code=%errorlevel%"
call :log "[installPandoc] winget upgrade exit %code%"
rem A non-zero code here nearly always means there was nothing to upgrade, which
rem is not an action and not a failure.
if "%code%"=="0" (set "didWhat=updated") else (set "didWhat=")
if not "%code%"=="0" call :say "Pandoc is already current."
goto verify

:reinstall_pandoc
call :say "Reinstalling Pandoc."
call :log "[installPandoc] winget install JohnMacFarlane.Pandoc --force"
winget install --id JohnMacFarlane.Pandoc -e --force --scope machine --architecture x64 --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
call :log "[installPandoc] winget install exit %errorlevel%"
set "didWhat=reinstalled"
goto verify

:install_pandoc
call :say "Installing Pandoc, about 100 MB."
call :log "[installPandoc] winget install JohnMacFarlane.Pandoc"
winget install --id JohnMacFarlane.Pandoc -e --scope machine --architecture x64 --silent --disable-interactivity --accept-package-agreements --accept-source-agreements >> "%logFile%" 2>&1
call :log "[installPandoc] winget install exit %errorlevel%"
set "didWhat=installed"

:verify
if exist "%ProgramFiles%\Pandoc\pandoc.exe" set "PATH=%ProgramFiles%\Pandoc;%PATH%"
where pandoc >nul 2>&1
if errorlevel 1 goto failed
for /f "tokens=1,2" %%a in ('pandoc --version 2^>nul') do if not defined sVersion set "sVersion=%%b"
call :log "[installPandoc] version: %sVersion%"
if not defined didWhat goto quietDone
if defined sVersion (call :action "Pandoc %didWhat%, %sVersion%") else (call :action "Pandoc %didWhat%.")
call :say "Done."
call :log "[installPandoc] done"
exit /b 0

:quietDone
call :log "[installPandoc] nothing needed doing"
exit /b 0

:failed
call :say "Pandoc could not be installed. The log has the details."
call :action "Pandoc could not be installed."
call :log "[installPandoc] FAILED: pandoc not found after install"
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
