@echo off
setlocal EnableExtensions
rem installMpv.cmd -- install the mpv media player for FileDir.
rem
rem The winget package shinchiro.mpv delivers an Inno Setup installer,
rem mpv_installer_x64.exe, built by the mpv2winget project. That installer sets
rem PrivilegesRequired=admin and DefaultDirName={autopf}\MPV Player, so it
rem always installs machine-wide into C:\Program Files\MPV Player.
rem
rem Its winget manifest declares no scope, so passing --scope machine leaves
rem winget with nothing that matches and it refuses the package outright with
rem 0x8a150010, "No applicable installer found". The switch is therefore left
rem off: the package installs machine-wide on its own.
rem
rem CONSOLE VERSUS LOG. The console gets short plain sentences. Paths probed,
rem commands, exit codes and winget's own output go to the log.
rem
rem WHAT IT DID. Anything actually done is written to
rem FileDir_setup_actions.txt, which is what the Results box reports. A player
rem already installed that needed nothing writes no action.
rem
rem REINSTALL. Pass "reinstall" as an argument, or set FILEDIR_REINSTALL=1,
rem which is what the installer's Reinstall checkbox does. An existing copy is
rem then replaced rather than accepted, and winget is given --force so it runs
rem the installer again instead of reporting the package already present.
rem
rem NOTHING PAUSES.
set "logDir=%LOCALAPPDATA%\FileDir\logs"
if not exist "%logDir%" mkdir "%logDir%" >nul 2>&1
set "logFile=%logDir%\FileDir_setup.log"
set "actionFile=%logDir%\FileDir_setup_actions.txt"
set "sMpv="
set "sVersion="
set "didWhat="
set "bReinstall="
set "sArgs=%*"
if not "%sArgs%"=="" echo %sArgs% | findstr /i /c:"reinstall" >nul && set "bReinstall=1"
if /i "%FILEDIR_REINSTALL%"=="1" set "bReinstall=1"

call :log "[installMpv] started %date% %time%"
call :log "[installMpv] arguments: %sArgs%"
call :log "[installMpv] reinstall requested: %bReinstall%"
call :log "[installMpv] user: %USERDOMAIN%\%USERNAME%"

net session >nul 2>&1
if errorlevel 1 goto notElevated
call :log "[installMpv] elevated: yes"

where winget >nul 2>&1
if errorlevel 1 goto noWinget
for /f "delims=" %%v in ('winget --version 2^>nul') do call :log "[installMpv] winget version: %%v"

call :log "[installMpv] looking for an existing copy"
call :findMpv
if defined bReinstall goto reinstall
if defined sMpv goto alreadyThere

call :say "Installing mpv, about 60 MB."
call :log "[installMpv] winget install shinchiro.mpv"
winget install --id shinchiro.mpv --exact --silent --accept-package-agreements --accept-source-agreements --disable-interactivity >> "%logFile%" 2>&1
call :log "[installMpv] winget exit %errorlevel%"
set "didWhat=installed"
goto check

:reinstall
if defined sMpv call :log "[installMpv] the existing copy at %sMpv% will be replaced"
call :say "Reinstalling mpv."
call :log "[installMpv] winget install shinchiro.mpv --force"
winget install --id shinchiro.mpv --exact --force --silent --accept-package-agreements --accept-source-agreements --disable-interactivity >> "%logFile%" 2>&1
call :log "[installMpv] winget exit %errorlevel%"
set "didWhat=reinstalled"

:check
set "sMpv="
call :findMpv
if not defined sMpv goto failed

:verify
call :log "[installMpv] mpv is at %sMpv%"
rem The player's path contains a space, and a quoted command inside for /f is
rem read by another cmd before it runs. Written to a file and read back, which
rem has no quoting to get wrong.
set "verFile=%logDir%\FileDir_mpvVersion.tmp"
"%sMpv%" --version > "%verFile%" 2>&1
for /f "tokens=1,2" %%a in ('type "%verFile%"') do if not defined sVersion set "sVersion=%%b"
type "%verFile%" >> "%logFile%"
del "%verFile%" >nul 2>&1
call :log "[installMpv] version: %sVersion%"
if not defined didWhat goto quietDone
if defined sVersion (call :action "mpv %didWhat%, %sVersion%") else (call :action "mpv %didWhat%.")
call :say "Done."
call :log "[installMpv] done"
exit /b 0

:alreadyThere
call :say "mpv is already installed."
call :log "[installMpv] no reinstall asked for, so the existing copy is kept"
goto verify

:quietDone
call :log "[installMpv] nothing needed doing"
exit /b 0

:notElevated
call :say "mpv needs administrator rights to install. Nothing was changed."
call :action "mpv could not be installed, because administrator rights were missing."
call :log "[installMpv] net session failed, so the process is not elevated"
exit /b 1

:noWinget
call :say "Windows Package Manager is missing, so mpv could not be installed."
call :action "mpv could not be installed, because Windows Package Manager is missing."
call :log "[installMpv] winget is not on the PATH"
exit /b 2

:failed
call :say "mpv could not be installed. The log has the details."
call :action "mpv could not be installed."
call :log "[installMpv] mpv was not found after the install attempt"
exit /b 3

:findMpv
set "sMpv="
call :try "%ProgramFiles%\MPV Player\mpv.exe"
call :try "%ProgramFiles%\mpv\mpv.exe"
call :try "%ProgramFiles%\MPV Media Player\mpv.exe"
call :try "%ProgramFiles%\FileDir\mpv.exe"
call :try "%ProgramFiles%\WinGet\Links\mpv.exe"
call :try "%LOCALAPPDATA%\Programs\MPV Player\mpv.exe"
call :try "%LOCALAPPDATA%\Programs\mpv\mpv.exe"
call :try "%LOCALAPPDATA%\Microsoft\WinGet\Links\mpv.exe"
if defined sMpv goto :eof
for /f "delims=" %%f in ('where mpv 2^>nul') do call :take "%%f"
if not defined sMpv call :log "[installMpv]   mpv was not found in any known location"
goto :eof

:try
if defined sMpv goto :eof
if not exist "%~1" goto tryNo
set "sMpv=%~1"
call :log "[installMpv]   found: %~1"
goto :eof

:tryNo
call :log "[installMpv]   no: %~1"
goto :eof

:take
if defined sMpv goto :eof
set "sMpv=%~1"
call :log "[installMpv]   found on the PATH: %~1"
goto :eof

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
